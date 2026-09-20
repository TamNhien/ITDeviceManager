USE [ITDeviceManagerDb];
GO

-- Argon2id PHC strings already include salt + parameters + derived hash.
-- Drop the legacy PasswordSalt column only when every account is Argon2id.
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'PasswordSalt') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.Users
        WHERE PasswordHash IS NULL
           OR PasswordHash NOT LIKE N'$argon2id$%')
    BEGIN
        THROW 51030, N'Khong the xoa PasswordSalt vi van con tai khoan chua dung Argon2id.', 1;
    END;

    EXEC(N'ALTER TABLE dbo.Users DROP COLUMN PasswordSalt;');
END;
GO

SELECT
    CASE WHEN COL_LENGTH(N'dbo.Users', N'PasswordSalt') IS NULL THEN N'OK - PasswordSalt da duoc xoa'
         ELSE N'WARNING - PasswordSalt van con ton tai'
    END AS PasswordSaltStatus;
GO
