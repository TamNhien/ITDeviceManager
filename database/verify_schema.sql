USE [ITDeviceManagerDb];
GO

DECLARE @ok bit = 1;

IF COL_LENGTH(N'dbo.Users', N'Email') IS NULL
BEGIN
    PRINT 'FAIL: dbo.Users.Email is missing.';
    SET @ok = 0;
END;

IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    PRINT 'FAIL: dbo.PasswordResetTokens is missing.';
    SET @ok = 0;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Users_Email'
      AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    PRINT 'FAIL: IX_Users_Email is missing.';
    SET @ok = 0;
END;

IF @ok = 1
    PRINT 'OK: ITDeviceManager V1.2.x schema is ready.';
ELSE
    THROW 51000, 'ITDeviceManager schema verification failed.', 1;
GO
