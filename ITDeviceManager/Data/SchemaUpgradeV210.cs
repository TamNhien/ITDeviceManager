using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV210
{
    private static readonly string[] SoftDeleteTables =
    [
        "Users",
        "Departments",
        "Employees",
        "DeviceTypes",
        "Devices",
        "DeviceMaintenances"
    ];

    public static async Task EnsureColumnsAsync(AppDbContext db)
    {
        foreach (var table in SoftDeleteTables)
        {
            var sql = $"""
IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.{table}', N'IsDeleted') IS NULL
        ALTER TABLE dbo.[{table}] ADD IsDeleted bit NOT NULL CONSTRAINT [DF_{table}_IsDeleted] DEFAULT(0) WITH VALUES;

    IF COL_LENGTH(N'dbo.{table}', N'DeletedAtUtc') IS NULL
        ALTER TABLE dbo.[{table}] ADD DeletedAtUtc datetimeoffset NULL;

    IF COL_LENGTH(N'dbo.{table}', N'DeletedByUserId') IS NULL
        ALTER TABLE dbo.[{table}] ADD DeletedByUserId int NULL;

    IF COL_LENGTH(N'dbo.{table}', N'DeletedByUsername') IS NULL
        ALTER TABLE dbo.[{table}] ADD DeletedByUsername nvarchar(100) NULL;
END;
""";
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }

    public static async Task UpgradeAsync(AppDbContext db)
    {
        await EnsureColumnsAsync(db);
        await EnsureIndexesAsync(db);

        await UpsertPermissionAsync(
            db,
            PermissionCodes.RecycleBinView,
            "Thùng rác",
            "Xem Thùng rác",
            "Xem dữ liệu đã xóa mềm và thông tin người xóa.",
            950);

        await UpsertPermissionAsync(
            db,
            PermissionCodes.RecycleBinRestore,
            "Thùng rác",
            "Khôi phục dữ liệu",
            "Khôi phục dữ liệu đã xóa mềm về màn hình nghiệp vụ.",
            951);

        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin'
  AND p.Code IN (N'RecycleBin.View', N'RecycleBin.Restore')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);
""");

        const string seedDefaults = """
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV210')
BEGIN
    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'RecycleBin.View'
    WHERE r.Name IN (N'Quản lý CNTT', N'Quản lý tài sản', N'Kiểm toán')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'RecycleBin.Restore'
    WHERE r.Name IN (N'Quản lý CNTT', N'Quản lý tài sản')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value])
    VALUES(N'DefaultsSeededV210',N'1');
END;
""";
        await db.Database.ExecuteSqlRawAsync(seedDefaults);
    }

    private static async Task EnsureIndexesAsync(AppDbContext db)
    {
        foreach (var table in SoftDeleteTables)
        {
            var indexName = $"IX_{table}_IsDeleted_DeletedAtUtc";
            var sql = $"""
IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.{table}', N'IsDeleted') IS NOT NULL
   AND COL_LENGTH(N'dbo.{table}', N'DeletedAtUtc') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE object_id=OBJECT_ID(N'dbo.{table}') AND name=N'{indexName}')
BEGIN
    CREATE INDEX [{indexName}] ON dbo.[{table}](IsDeleted, DeletedAtUtc);
END;
""";
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private static Task<int> UpsertPermissionAsync(
        AppDbContext db,
        string code,
        string group,
        string name,
        string description,
        int sortOrder)
        => db.Database.ExecuteSqlInterpolatedAsync($"""
MERGE dbo.Permissions AS target
USING (SELECT {code} AS Code) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET GroupName={group}, Name={name}, Description={description}, SortOrder={sortOrder}
WHEN NOT MATCHED THEN
    INSERT(Code, GroupName, Name, Description, SortOrder)
    VALUES({code}, {group}, {name}, {description}, {sortOrder});
""");
}
