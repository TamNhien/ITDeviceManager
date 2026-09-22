SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Devices', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Devices', N'WarrantyEndDate') IS NULL
        ALTER TABLE dbo.Devices ADD WarrantyEndDate date NULL;

    IF COL_LENGTH(N'dbo.Devices', N'MaintenanceIntervalMonths') IS NULL
        ALTER TABLE dbo.Devices ADD MaintenanceIntervalMonths int NULL;

    IF COL_LENGTH(N'dbo.Devices', N'NextMaintenanceDate') IS NULL
        ALTER TABLE dbo.Devices ADD NextMaintenanceDate date NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id=OBJECT_ID(N'dbo.Devices') AND name=N'IX_Devices_WarrantyEndDate')
        CREATE INDEX IX_Devices_WarrantyEndDate
        ON dbo.Devices(WarrantyEndDate)
        WHERE WarrantyEndDate IS NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id=OBJECT_ID(N'dbo.Devices') AND name=N'IX_Devices_NextMaintenanceDate')
        CREATE INDEX IX_Devices_NextMaintenanceDate
        ON dbo.Devices(NextMaintenanceDate)
        WHERE NextMaintenanceDate IS NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id=OBJECT_ID(N'dbo.Devices') AND name=N'CK_Devices_MaintenanceIntervalMonths')
        ALTER TABLE dbo.Devices ADD CONSTRAINT CK_Devices_MaintenanceIntervalMonths
        CHECK (MaintenanceIntervalMonths IS NULL OR (MaintenanceIntervalMonths >= 1 AND MaintenanceIntervalMonths <= 120));
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NOT NULL
BEGIN
    MERGE dbo.Permissions AS target
    USING (VALUES
        (N'Alert.View', N'Cảnh báo', N'Xem cảnh báo hạn', N'Xem thiết bị sắp hết bảo hành hoặc đến hạn bảo trì.', 920)
    ) AS source(Code, GroupName, Name, Description, SortOrder)
    ON target.Code=source.Code
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
      AND p.Code=N'Alert.View'
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    IF OBJECT_ID(N'dbo.PermissionSchemaMeta', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV220')
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

        INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV220',N'1');
    END;
END;

COMMIT TRANSACTION;
