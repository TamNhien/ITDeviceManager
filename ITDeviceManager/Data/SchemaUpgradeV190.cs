using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV190
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        const string createSchema = """
IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Permissions
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
        Code nvarchar(100) NOT NULL,
        GroupName nvarchar(100) NOT NULL,
        Name nvarchar(160) NOT NULL,
        Description nvarchar(500) NULL,
        SortOrder int NOT NULL CONSTRAINT DF_Permissions_SortOrder DEFAULT(0)
    );
    CREATE UNIQUE INDEX IX_Permissions_Code ON dbo.Permissions(Code);
END;

IF OBJECT_ID(N'dbo.PermissionSchemaMeta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PermissionSchemaMeta
    (
        [Key] nvarchar(100) NOT NULL CONSTRAINT PK_PermissionSchemaMeta PRIMARY KEY,
        [Value] nvarchar(200) NULL
    );
END;

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions
    (
        RoleId int NOT NULL,
        PermissionId int NOT NULL,
        CONSTRAINT PK_RolePermissions PRIMARY KEY(RoleId, PermissionId),
        CONSTRAINT FK_RolePermissions_Roles_RoleId FOREIGN KEY(RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions_PermissionId FOREIGN KEY(PermissionId) REFERENCES dbo.Permissions(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_RolePermissions_PermissionId ON dbo.RolePermissions(PermissionId);
END;
""";
        await db.Database.ExecuteSqlRawAsync(createSchema);

        var sortOrder = 0;
        foreach (var permission in PermissionCodes.All)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
MERGE dbo.Permissions AS target
USING (SELECT {permission.Code} AS Code) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET GroupName={permission.Group}, Name={permission.Name}, Description={permission.Description}, SortOrder={sortOrder}
WHEN NOT MATCHED THEN
    INSERT(Code, GroupName, Name, Description, SortOrder)
    VALUES({permission.Code}, {permission.Group}, {permission.Name}, {permission.Description}, {sortOrder});
""");
            sortOrder++;
        }

        // Admin always has every permission.
        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);
""");

        await SeedRoleAsync(db, "Staff",
            PermissionCodes.DashboardView,
            PermissionCodes.DeviceView,
            PermissionCodes.DeviceTypeView,
            PermissionCodes.EmployeeView,
            PermissionCodes.DepartmentView,
            PermissionCodes.AssignmentView,
            PermissionCodes.AssignmentCreate,
            PermissionCodes.AssignmentReturn,
            PermissionCodes.MaintenanceView,
            PermissionCodes.MaintenanceCreate,
            PermissionCodes.MaintenanceUpdate,
            PermissionCodes.ReportExport);

        await SeedRoleAsync(db, "Quản lý CNTT", PermissionCodes.All.Where(x => x.Code != PermissionCodes.PermissionManage).Select(x => x.Code).ToArray());
        await SeedRoleAsync(db, "Kỹ thuật viên",
            PermissionCodes.DashboardView, PermissionCodes.DeviceView, PermissionCodes.DeviceUpdate,
            PermissionCodes.DeviceTypeView, PermissionCodes.EmployeeView, PermissionCodes.DepartmentView,
            PermissionCodes.AssignmentView, PermissionCodes.MaintenanceView, PermissionCodes.MaintenanceCreate,
            PermissionCodes.MaintenanceUpdate, PermissionCodes.ReportExport);
        await SeedRoleAsync(db, "Quản lý tài sản",
            PermissionCodes.DashboardView, PermissionCodes.DeviceView, PermissionCodes.DeviceCreate,
            PermissionCodes.DeviceUpdate, PermissionCodes.DeviceDelete, PermissionCodes.DeviceTypeView,
            PermissionCodes.DeviceTypeCreate, PermissionCodes.DeviceTypeUpdate, PermissionCodes.EmployeeView,
            PermissionCodes.DepartmentView, PermissionCodes.AssignmentView, PermissionCodes.AssignmentCreate,
            PermissionCodes.AssignmentUpdate, PermissionCodes.AssignmentReturn, PermissionCodes.MaintenanceView,
            PermissionCodes.MaintenanceCreate, PermissionCodes.MaintenanceUpdate, PermissionCodes.MaintenanceDelete,
            PermissionCodes.ReportExport, PermissionCodes.AuditView);
        await SeedRoleAsync(db, "Helpdesk",
            PermissionCodes.DashboardView, PermissionCodes.DeviceView, PermissionCodes.EmployeeView,
            PermissionCodes.DepartmentView, PermissionCodes.AssignmentView, PermissionCodes.MaintenanceView,
            PermissionCodes.MaintenanceCreate, PermissionCodes.MaintenanceUpdate);
        await SeedRoleAsync(db, "Kiểm toán",
            PermissionCodes.DashboardView, PermissionCodes.DeviceView, PermissionCodes.DeviceTypeView,
            PermissionCodes.EmployeeView, PermissionCodes.DepartmentView, PermissionCodes.AssignmentView,
            PermissionCodes.MaintenanceView, PermissionCodes.AuditView, PermissionCodes.ReportExport);
        await SeedRoleAsync(db, "Trưởng phòng",
            PermissionCodes.DashboardView, PermissionCodes.DeviceView, PermissionCodes.EmployeeView,
            PermissionCodes.DepartmentView, PermissionCodes.AssignmentView, PermissionCodes.MaintenanceView,
            PermissionCodes.ReportExport);
        await SeedRoleAsync(db, "Nhân sự xem",
            PermissionCodes.DashboardView, PermissionCodes.EmployeeView, PermissionCodes.DepartmentView);
        await SeedRoleAsync(db, "Chỉ đọc",
            PermissionCodes.DashboardView, PermissionCodes.DeviceView, PermissionCodes.DeviceTypeView,
            PermissionCodes.EmployeeView, PermissionCodes.DepartmentView, PermissionCodes.AssignmentView,
            PermissionCodes.MaintenanceView);

        await db.Database.ExecuteSqlRawAsync("""
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV190')
    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV190',N'1');
""");
    }

    private static async Task SeedRoleAsync(AppDbContext db, string roleName, params string[] codes)
    {
        foreach (var code in codes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
INNER JOIN dbo.Permissions p ON p.Code={code}
WHERE r.Name={roleName}
  AND NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV190')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);
""");
        }
    }
}
