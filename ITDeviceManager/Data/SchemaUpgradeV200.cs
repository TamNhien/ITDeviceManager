using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV200
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        // SchemaUpgradeV190 runs first and merges PermissionCodes.All, therefore the
        // new DeviceCode.Use permission already exists here. V2.0.0 only needs to
        // seed sensible defaults once without overwriting later administrator edits.
        var roles = new[]
        {
            "Staff",
            "Quản lý CNTT",
            "Kỹ thuật viên",
            "Quản lý tài sản",
            "Helpdesk",
            "Trưởng phòng"
        };

        foreach (var roleName in roles)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
INNER JOIN dbo.Permissions p ON p.Code={PermissionCodes.DeviceCodeUse}
WHERE r.Name={roleName}
  AND NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV200')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);
""");
        }

        await db.Database.ExecuteSqlRawAsync("""
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV200')
    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV200',N'1');
""");
    }
}
