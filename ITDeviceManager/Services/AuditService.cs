using ITDeviceManager.Data;
using ITDeviceManager.Models;

namespace ITDeviceManager.Services;

public static class AuditService
{
    public static async Task<bool> TryWriteAsync(
        string action,
        string entityName,
        string description,
        string? entityKey = null,
        string? actorUsername = null,
        int? actorUserId = null,
        string? oldValuesJson = null,
        string? newValuesJson = null)
    {
        try
        {
            var currentUser = AppSession.CurrentUser;
            await using var db = new AppDbContext();
            db.AuditLogs.Add(new AuditLog
            {
                OccurredAtUtc = DateTimeOffset.UtcNow,
                UserId = actorUserId ?? currentUser?.Id,
                Username = NormalizeActor(actorUsername ?? currentUser?.Username),
                Action = action.Trim(),
                EntityName = entityName.Trim(),
                EntityKey = string.IsNullOrWhiteSpace(entityKey) ? null : entityKey.Trim(),
                Description = description.Trim(),
                OldValuesJson = oldValuesJson,
                NewValuesJson = newValuesJson,
                ComputerName = Environment.MachineName,
                AppVersion = GetAppVersion()
            });
            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            // Authentication/logout should never be blocked only because the audit sink is unavailable.
            // Business CRUD is audited transactionally by AppDbContext itself.
            return false;
        }
    }

    public static string GetAppVersion()
        => typeof(AuditService).Assembly.GetName().Version?.ToString(3) ?? "unknown";

    private static string NormalizeActor(string? username)
        => string.IsNullOrWhiteSpace(username) ? "Hệ thống" : username.Trim();
}
