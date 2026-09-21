USE [ITDeviceManagerDb];
GO

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
GO


IF OBJECT_ID(N'dbo.PermissionSchemaMeta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PermissionSchemaMeta
    (
        [Key] nvarchar(100) NOT NULL CONSTRAINT PK_PermissionSchemaMeta PRIMARY KEY,
        [Value] nvarchar(200) NULL
    );
END;
GO

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
GO

-- Ứng dụng V1.9.0 tự upsert danh mục quyền và seed RolePermissions khi khởi động.
