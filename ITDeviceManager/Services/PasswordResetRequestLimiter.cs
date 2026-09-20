using System.Collections.Concurrent;

namespace ITDeviceManager.Services;

public static class PasswordResetRequestLimiter
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(60);
    private static readonly ConcurrentDictionary<string, DateTimeOffset> LastRequestByEmail = new(StringComparer.OrdinalIgnoreCase);

    public static bool TryBegin(string email, out TimeSpan retryAfter)
    {
        var key = email.Trim();
        var now = DateTimeOffset.UtcNow;

        if (LastRequestByEmail.TryGetValue(key, out var lastRequest))
        {
            var remaining = MinimumInterval - (now - lastRequest);
            if (remaining > TimeSpan.Zero)
            {
                retryAfter = remaining;
                return false;
            }
        }

        LastRequestByEmail[key] = now;
        retryAfter = TimeSpan.Zero;
        return true;
    }
}
