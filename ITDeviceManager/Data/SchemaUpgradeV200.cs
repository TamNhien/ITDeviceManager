using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV200
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        // V1.9.0 already owns the permission schema. V2.0.0 only adds the
        // QR/Barcode permissions and seeds sensible defaults once, without
        // overwriting any permission choices the administrator made earlier.
        await UpsertPermissionAsync(
            db,
            PermissionCodes.QrBarcodeView,
            "QR / Barcode",
            "Xem QR / Barcode",
            "Xem danh sách, preview và thư mục mã của thiết bị.",
            900);

        await UpsertPermissionAsync(
            db,
            PermissionCodes.QrBarcodeGenerate,
            "QR / Barcode",
            "Tạo QR / Barcode",
            "Tạo hoặc tạo lại file QR và Code 128 cho thiết bị.",
            901);

        // Admin remains fail-open for all defined application permissions.
        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin'
  AND p.Code IN (N'QrBarcode.View', N'QrBarcode.Generate')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);
""");

        const string seedDefaults = """
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV200')
BEGIN
    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'QrBarcode.View'
    WHERE r.Name IN (
        N'Staff', N'Quản lý CNTT', N'Kỹ thuật viên', N'Quản lý tài sản',
        N'Helpdesk', N'Kiểm toán', N'Trưởng phòng', N'Chỉ đọc')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'QrBarcode.Generate'
    WHERE r.Name IN (N'Quản lý CNTT', N'Kỹ thuật viên', N'Quản lý tài sản')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value])
    VALUES(N'DefaultsSeededV200',N'1');
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
