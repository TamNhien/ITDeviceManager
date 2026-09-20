using System.Collections.Concurrent;

namespace ITDeviceManager.Services;

/// <summary>
/// Lightweight in-process throttling for a desktop application.
/// It slows repeated online guesses without changing the database schema.
/// </summary>
public static class LoginAttemptLimiter
{
    private const int MaximumFailures = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(30);
    private static readonly ConcurrentDictionary<string, AttemptState> States = new(StringComparer.OrdinalIgnoreCase);

    public static bool TryBegin(string username, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;
        var key = Normalize(username);

        if (!States.TryGetValue(key, out var state) || state.LockedUntilUtc is null)
            return true;

        var remaining = state.LockedUntilUtc.Value - DateTimeOffset.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            States.TryRemove(key, out _);
            return true;
        }

        retryAfter = remaining;
        return false;
    }

    public static void RegisterFailure(string username)
    {
        var key = Normalize(username);

        States.AddOrUpdate(
            key,
            _ => new AttemptState(1, null),
            (_, current) =>
            {
                if (current.LockedUntilUtc is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
                    return current;

                var failures = current.Failures + 1;
                return failures >= MaximumFailures
                    ? new AttemptState(0, DateTimeOffset.UtcNow.Add(LockoutDuration))
                    : new AttemptState(failures, null);
            });
    }

    public static void RegisterSuccess(string username) =>
        States.TryRemove(Normalize(username), out _);

    private static string Normalize(string username) => username.Trim().ToUpperInvariant();

    private sealed record AttemptState(int Failures, DateTimeOffset? LockedUntilUtc);
}
