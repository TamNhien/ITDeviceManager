SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @tables TABLE(Name sysname);
INSERT INTO @tables(Name)
VALUES (N'Users'),(N'Departments'),(N'Employees'),(N'DeviceTypes'),(N'Devices'),(N'DeviceMaintenances');

DECLARE @table sysname;
DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT Name FROM @tables;
OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @sql nvarchar(max)=N'';

    IF OBJECT_ID(N'dbo.' + @table, N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH(N'dbo.' + @table, N'IsDeleted') IS NULL
            SET @sql += N'ALTER TABLE dbo.' + QUOTENAME(@table) +
                        N' ADD IsDeleted bit NOT NULL CONSTRAINT ' + QUOTENAME(N'DF_' + @table + N'_IsDeleted') +
                        N' DEFAULT(0) WITH VALUES;';

        IF COL_LENGTH(N'dbo.' + @table, N'DeletedAtUtc') IS NULL
            SET @sql += N'ALTER TABLE dbo.' + QUOTENAME(@table) + N' ADD DeletedAtUtc datetimeoffset NULL;';

        IF COL_LENGTH(N'dbo.' + @table, N'DeletedByUserId') IS NULL
            SET @sql += N'ALTER TABLE dbo.' + QUOTENAME(@table) + N' ADD DeletedByUserId int NULL;';

        IF COL_LENGTH(N'dbo.' + @table, N'DeletedByUsername') IS NULL
            SET @sql += N'ALTER TABLE dbo.' + QUOTENAME(@table) + N' ADD DeletedByUsername nvarchar(100) NULL;';

        IF LEN(@sql) > 0 EXEC sys.sp_executesql @sql;

        DECLARE @indexName sysname=N'IX_' + @table + N'_IsDeleted_DeletedAtUtc';
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE object_id=OBJECT_ID(N'dbo.' + @table) AND name=@indexName)
        BEGIN
            SET @sql=N'CREATE INDEX ' + QUOTENAME(@indexName) + N' ON dbo.' + QUOTENAME(@table) + N'(IsDeleted, DeletedAtUtc);';
            EXEC sys.sp_executesql @sql;
        END;
    END;

    FETCH NEXT FROM table_cursor INTO @table;
END;
CLOSE table_cursor;
DEALLOCATE table_cursor;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NOT NULL
BEGIN
    MERGE dbo.Permissions AS target
    USING (VALUES
        (N'RecycleBin.View', N'Thùng rác', N'Xem Thùng rác', N'Xem dữ liệu đã xóa mềm và thông tin người xóa.', 950),
        (N'RecycleBin.Restore', N'Thùng rác', N'Khôi phục dữ liệu', N'Khôi phục dữ liệu đã xóa mềm về màn hình nghiệp vụ.', 951)
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
      AND p.Code IN (N'RecycleBin.View', N'RecycleBin.Restore')
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RolePermissions rp
          WHERE rp.RoleId=r.Id AND rp.PermissionId=p.Id);

    IF OBJECT_ID(N'dbo.PermissionSchemaMeta', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.PermissionSchemaMeta WHERE [Key]=N'DefaultsSeededV210')
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

        INSERT INTO dbo.PermissionSchemaMeta([Key],[Value]) VALUES(N'DefaultsSeededV210',N'1');
    END;
END;

COMMIT TRANSACTION;
