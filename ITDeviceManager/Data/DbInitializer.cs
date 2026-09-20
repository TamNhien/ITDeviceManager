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
            db.Users.Add(new User
            {
                Username = DefaultAdminUsername,
                Email = null,
                FullName = "Quản trị viên",
                PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword),
                RoleId = 1,
                IsActive = true
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task UpgradeSchemaToV120Async(AppDbContext db)
    {
        // Keep V1.2.0 upgrades idempotent for databases created by older versions.
        await using var transaction = await db.Database.BeginTransactionAsync();

        const string addEmailColumnSql = """
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Email nvarchar(320) NULL;
END;
""";

        const string createEmailIndexSql = """
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
""";

        const string createResetTokensTableSql = """
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

        await db.Database.ExecuteSqlRawAsync(addEmailColumnSql);
        await db.Database.ExecuteSqlRawAsync(createEmailIndexSql);
        await db.Database.ExecuteSqlRawAsync(createResetTokensTableSql);

        await transaction.CommitAsync();
    }
}
