using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV125
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        // EnsureCreated creates the V1.2.5 schema for a brand-new database.
        // The SQL below upgrades older databases in-place and is safe to run repeatedly.
        await db.Database.EnsureCreatedAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();

        const string addPhoneNumberSql = """
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'PhoneNumber') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PhoneNumber nvarchar(32) NULL;
END;
""";

        const string createPhoneIndexSql = """
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'PhoneNumber') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = N'IX_Users_PhoneNumber'
         AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_PhoneNumber
        ON dbo.Users(PhoneNumber)
        WHERE PhoneNumber IS NOT NULL AND PhoneNumber <> N'';
END;
""";

        // Upgrade user-facing text fields to NVARCHAR when an older/manual schema used
        // VARCHAR/CHAR. This preserves Vietnamese accents such as ă, â, ê, ô, ơ, ư, đ.
        const string ensureUnicodeSql = """
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'FullName' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.Users ALTER COLUMN FullName nvarchar(max) NOT NULL;
END;

IF OBJECT_ID(N'dbo.Departments', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Departments') AND name = N'Name' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.Departments ALTER COLUMN Name nvarchar(max) NOT NULL;
END;

IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Employees') AND name = N'FullName' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.Employees ALTER COLUMN FullName nvarchar(max) NOT NULL;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Employees') AND name = N'Email' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.Employees ALTER COLUMN Email nvarchar(max) NULL;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Employees') AND name = N'Phone' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.Employees ALTER COLUMN Phone nvarchar(max) NULL;
END;

IF OBJECT_ID(N'dbo.DeviceTypes', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DeviceTypes') AND name = N'Name' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.DeviceTypes ALTER COLUMN Name nvarchar(max) NOT NULL;
END;

IF OBJECT_ID(N'dbo.Devices', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Devices') AND name = N'Name' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.Devices ALTER COLUMN Name nvarchar(max) NOT NULL;
END;

IF OBJECT_ID(N'dbo.DeviceAssignments', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DeviceAssignments') AND name = N'Note' AND TYPE_NAME(user_type_id) IN (N'varchar', N'char', N'text'))
        ALTER TABLE dbo.DeviceAssignments ALTER COLUMN Note nvarchar(max) NULL;
END;
""";

        await db.Database.ExecuteSqlRawAsync(addPhoneNumberSql);
        await db.Database.ExecuteSqlRawAsync(createPhoneIndexSql);
        await db.Database.ExecuteSqlRawAsync(ensureUnicodeSql);

        await transaction.CommitAsync();
    }
}
