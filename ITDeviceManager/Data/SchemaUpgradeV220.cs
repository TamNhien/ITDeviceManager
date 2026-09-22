using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV220
{
    public static async Task EnsureColumnsAsync(AppDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.Devices', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Devices', N'WarrantyEndDate') IS NULL
        ALTER TABLE dbo.Devices ADD WarrantyEndDate date NULL;

    IF COL_LENGTH(N'dbo.Devices', N'MaintenanceIntervalMonths') IS NULL
        ALTER TABLE dbo.Devices ADD MaintenanceIntervalMonths int NULL;

    IF COL_LENGTH(N'dbo.Devices', N'NextMaintenanceDate') IS NULL
        ALTER TABLE dbo.Devices ADD NextMaintenanceDate date NULL;
END;
""";
        await db.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task UpgradeAsync(AppDbContext db)
    {
        await EnsureColumnsAsync(db);

        const string indexesAndConstraint = """
IF OBJECT_ID(N'dbo.Devices', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id=OBJECT_ID(N'dbo.Devices') AND name=N'IX_Devices_WarrantyEndDate')
        CREATE INDEX IX_Devices_WarrantyEndDate ON dbo.Devices(WarrantyEndDate) WHERE WarrantyEndDate IS NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id=OBJECT_ID(N'dbo.Devices') AND name=N'IX_Devices_NextMaintenanceDate')
        CREATE INDEX IX_Devices_NextMaintenanceDate ON dbo.Devices(NextMaintenanceDate) WHERE NextMaintenanceDate IS NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id=OBJECT_ID(N'dbo.Devices') AND name=N'CK_Devices_MaintenanceIntervalMonths')
        ALTER TABLE dbo.Devices ADD CONSTRAINT CK_Devices_MaintenanceIntervalMonths
        CHECK (MaintenanceIntervalMonths IS NULL OR (MaintenanceIntervalMonths >= 1 AND MaintenanceIntervalMonths <= 120));
END;
""";
        await db.Database.ExecuteSqlRawAsync(indexesAndConstraint);

        await UpsertPermissionAsync(
            db,
            PermissionCodes.AlertView,
            "Cảnh báo",
            "Xem cảnh báo hạn",
            "Xem thiết bị sắp hết bảo hành hoặc đến hạn bảo trì.",
            920);

        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin'
  AND p.Code=N'Alert.View'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);
""");

        const string seedDefaults = """
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV220')
BEGIN
    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'Alert.View'
    WHERE r.Name IN (
        N'Staff', N'Quản lý CNTT', N'Kỹ thuật viên', N'Quản lý tài sản',
        N'Helpdesk', N'Kiểm toán', N'Trưởng phòng', N'Chỉ đọc')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value])
    VALUES(N'DefaultsSeededV220',N'1');
END;
""";
        await db.Database.ExecuteSqlRawAsync(seedDefaults);
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
