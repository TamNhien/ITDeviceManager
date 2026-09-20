using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

/// <summary>
/// V1.3.0 removes the legacy PasswordSalt column. Argon2id PHC strings already
/// embed the random salt and all cost parameters inside Users.PasswordHash.
/// </summary>
public static class SchemaUpgradeV130
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'PasswordSalt') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.Users
        WHERE PasswordHash IS NULL
           OR PasswordHash NOT LIKE N'$argon2id$%')
    BEGIN
        THROW 51030, N'Không thể xóa PasswordSalt vì vẫn còn tài khoản chưa dùng Argon2id.', 1;
    END;

    EXEC(N'ALTER TABLE dbo.Users DROP COLUMN PasswordSalt;');
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
