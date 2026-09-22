using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV230
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
MERGE dbo.Permissions AS target
USING (SELECT {PermissionCodes.ExcelImport} AS Code) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET GroupName=N'Nhập Excel', Name=N'Nhập Excel hàng loạt',
               Description=N'Xem trước, kiểm tra và nhập hàng loạt thiết bị/nhân viên từ file Excel.', SortOrder=940
WHEN NOT MATCHED THEN
    INSERT(Code, GroupName, Name, Description, SortOrder)
    VALUES({PermissionCodes.ExcelImport}, N'Nhập Excel', N'Nhập Excel hàng loạt',
           N'Xem trước, kiểm tra và nhập hàng loạt thiết bị/nhân viên từ file Excel.', 940);
""");

        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin'
  AND p.Code=N'Import.Excel'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV230')
BEGIN
    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'Import.Excel'
    WHERE r.Name=N'Quản lý CNTT'
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV230',N'1');
END;
""");
    }
}
