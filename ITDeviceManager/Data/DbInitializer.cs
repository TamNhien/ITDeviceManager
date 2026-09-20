using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class DbInitializer
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "Admin@123!2026";

    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await UpgradeSchemaToV120Async(db);

        if (!await db.Users.AnyAsync())
        {
            var (hash, salt) = PasswordHasher.HashPassword(DefaultAdminPassword);
            db.Users.Add(new User
            {
                Username = DefaultAdminUsername,
                Email = null,
                FullName = "Quản trị viên",
                PasswordHash = hash,
                PasswordSalt = salt,
                RoleId = 1,
                IsActive = true
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task UpgradeSchemaToV120Async(AppDbContext db)
    {
        // EnsureCreated does not alter an existing database. This bounded, idempotent
        // upgrade keeps V1.0/V1.1 databases usable when V1.2 adds email + reset tokens.
        const string sql = """
IF COL_LENGTH(N'dbo.Users', N'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Email nvarchar(320) NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Users_Email' AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email
        ON dbo.Users(Email)
        WHERE Email IS NOT NULL AND Email <> N'';
END;

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
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
