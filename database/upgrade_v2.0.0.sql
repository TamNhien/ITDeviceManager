/*
ITDeviceManager V2.0.0 - QR / Barcode permission upgrade.
The QR PNG files themselves are stored on disk in the project-root QR folder;
no QR image/blob column is added to SQL Server.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
    THROW 51000, N'V1.9.0 permission schema is required before V2.0.0.', 1;

MERGE dbo.Permissions AS target
USING (VALUES
    (N'QrBarcode.View', N'QR / Barcode', N'Xem QR / Barcode', N'Xem danh sách, preview và thư mục mã của thiết bị.', 900),
    (N'QrBarcode.Generate', N'QR / Barcode', N'Tạo QR / Barcode', N'Tạo hoặc tạo lại file QR và Code 128 cho thiết bị.', 901)
) AS source(Code, GroupName, Name, Description, SortOrder)
ON target.Code = source.Code
WHEN MATCHED THEN UPDATE SET
    GroupName=source.GroupName,
    Name=source.Name,
    Description=source.Description,
    SortOrder=source.SortOrder
WHEN NOT MATCHED THEN
    INSERT(Code, GroupName, Name, Description, SortOrder)
    VALUES(source.Code, source.GroupName, source.Name, source.Description, source.SortOrder);

INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.Name=N'Admin'
  AND p.Code IN (N'QrBarcode.View', N'QrBarcode.Generate')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

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

COMMIT TRANSACTION;
