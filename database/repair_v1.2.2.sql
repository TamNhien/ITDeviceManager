USE [ITDeviceManagerDb];
GO

-- V1.2.2 repair migration.
-- Safe to run multiple times.

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Email nvarchar(320) NULL;
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'Email') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = N'IX_Users_Email'
         AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email
        ON dbo.Users(Email)
        WHERE Email IS NOT NULL AND Email <> N'';
END;
GO

IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        Id bigint IDENTITY(1,1) NOT NULL,
        UserId int NOT NULL,
        TokenHash nvarchar(64) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        ExpiresAtUtc datetimeoffset NOT NULL,
        UsedAtUtc datetimeoffset NULL,
        CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (Id),
        CONSTRAINT FK_PasswordResetTokens_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_PasswordResetTokens_TokenHash
        ON dbo.PasswordResetTokens(TokenHash);

    CREATE INDEX IX_PasswordResetTokens_UserId_ExpiresAtUtc
        ON dbo.PasswordResetTokens(UserId, ExpiresAtUtc);
END;
GO

SELECT
    COL_LENGTH(N'dbo.Users', N'Email') AS UsersEmailColumnLength,
    OBJECT_ID(N'dbo.PasswordResetTokens', N'U') AS PasswordResetTokensObjectId;
GO
