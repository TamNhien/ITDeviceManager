-- ITDeviceManager V2.3.0 - Excel bulk import permission.
-- No business-data schema change is required in this release.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
    THROW 51000, N'Permission schema is missing. Upgrade through V1.9.0 first.', 1;

MERGE dbo.Permissions AS target
USING (SELECT N'Import.Excel' AS Code) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET GroupName=N'Nhập Excel', Name=N'Nhập Excel hàng loạt',
               Description=N'Xem trước, kiểm tra và nhập hàng loạt thiết bị/nhân viên từ file Excel.', SortOrder=940
WHEN NOT MATCHED THEN
    INSERT(Code, GroupName, Name, Description, SortOrder)
    VALUES(N'Import.Excel', N'Nhập Excel', N'Nhập Excel hàng loạt',
           N'Xem trước, kiểm tra và nhập hàng loạt thiết bị/nhân viên từ file Excel.', 940);

INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin' AND p.Code=N'Import.Excel'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV230')
BEGIN
    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    INNER JOIN dbo.Permissions p ON p.Code=N'Import.Excel'
    WHERE r.Name=N'Quản lý CNTT'
      AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV230',N'1');
END;

COMMIT TRANSACTION;
