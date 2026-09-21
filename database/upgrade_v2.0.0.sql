/* ITDeviceManager V2.0.0 - QR / Barcode permission seed
   Idempotent. Run only if you want to apply the permission manually; the app
   executes the equivalent upgrade automatically at startup. */

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.PermissionSchemaMeta', N'U') IS NULL
BEGIN
    THROW 50000, 'V1.9.0 permission schema is required before V2.0.0.', 1;
END;

MERGE dbo.Permissions AS target
USING (SELECT N'DeviceCode.Use' AS Code) AS source
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET GroupName=N'Thiết bị',
               Name=N'QR / Barcode thiết bị',
               Description=N'Tạo, lưu, in nhãn QR/Barcode và nhận diện thiết bị bằng máy quét.'
WHEN NOT MATCHED THEN
    INSERT(Code, GroupName, Name, Description, SortOrder)
    VALUES(N'DeviceCode.Use', N'Thiết bị', N'QR / Barcode thiết bị',
           N'Tạo, lưu, in nhãn QR/Barcode và nhận diện thiết bị bằng máy quét.', 5);

INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
SELECT r.Id, p.Id
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE p.Code=N'DeviceCode.Use'
  AND r.Name=N'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

IF NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV200')
BEGIN
    INSERT INTO dbo.RolePermissions(RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    CROSS JOIN dbo.Permissions p
    WHERE p.Code=N'DeviceCode.Use'
      AND r.Name IN (N'Staff', N'Quản lý CNTT', N'Kỹ thuật viên', N'Quản lý tài sản', N'Helpdesk', N'Trưởng phòng')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV200',N'1');
END;

COMMIT TRANSACTION;
