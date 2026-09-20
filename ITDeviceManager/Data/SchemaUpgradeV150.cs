using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

/// <summary>
/// V1.5.0 adds an immutable audit trail for business and authentication activity.
/// The upgrade is idempotent and safe to execute at every application start.
/// </summary>
public static class SchemaUpgradeV150
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        const string sql = """
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id bigint IDENTITY(1,1) NOT NULL,
        OccurredAtUtc datetimeoffset NOT NULL,
        UserId int NULL,
        Username nvarchar(100) NOT NULL,
        Action nvarchar(80) NOT NULL,
        EntityName nvarchar(100) NOT NULL,
        EntityKey nvarchar(200) NULL,
        Description nvarchar(1000) NOT NULL,
        OldValuesJson nvarchar(max) NULL,
        NewValuesJson nvarchar(max) NULL,
        ComputerName nvarchar(100) NULL,
        AppVersion nvarchar(32) NULL,
        CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
    );

    CREATE INDEX IX_AuditLogs_OccurredAtUtc
        ON dbo.AuditLogs(OccurredAtUtc DESC);

    CREATE INDEX IX_AuditLogs_UserId_OccurredAtUtc
        ON dbo.AuditLogs(UserId, OccurredAtUtc DESC);

    CREATE INDEX IX_AuditLogs_Username
        ON dbo.AuditLogs(Username);

    CREATE INDEX IX_AuditLogs_Action
        ON dbo.AuditLogs(Action);

    CREATE INDEX IX_AuditLogs_EntityName
        ON dbo.AuditLogs(EntityName);
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
