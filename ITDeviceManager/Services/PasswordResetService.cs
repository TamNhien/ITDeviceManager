using System.Security.Cryptography;
using System.Text;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Services;

public enum PasswordResetRequestResult
{
    Accepted,
    SmtpNotConfigured
}

public sealed class PasswordResetService
{
    private readonly EmailService _emailService = new();

    public async Task<PasswordResetRequestResult> RequestResetAsync(string email)
    {
        if (!AppSettings.IsSmtpConfigured)
            return PasswordResetRequestResult.SmtpNotConfigured;

        var normalizedEmail = email.Trim();
        await using var db = new AppDbContext();
        var user = await db.Users
            .SingleOrDefaultAsync(x => x.IsActive && x.Email == normalizedEmail);

        // Always return the same outward result to avoid disclosing whether an email exists.
        if (user is null)
            return PasswordResetRequestResult.Accepted;

        var now = DateTimeOffset.UtcNow;
        var oldTokens = await db.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAtUtc == null)
            .ToListAsync();
        foreach (var old in oldTokens)
            old.UsedAtUtc = now;

        var rawToken = CreateToken();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(AppSettings.PasswordResetExpiryMinutes)
        });

        await db.SaveChangesAsync();

        var encodedToken = Uri.EscapeDataString(rawToken);
        var directAppLink = $"{AppSettings.PasswordResetScheme}://reset-password?token={encodedToken}";

        // Use an HTTPS bridge because Gmail/webmail can strip or refuse custom-scheme href values.
        // The sensitive token lives in the fragment. URL fragments never travel in the HTTP request.
        var bridgeBase = AppSettings.PasswordResetWebUrl.Trim().TrimEnd('#');
        var browserResetLink = string.IsNullOrWhiteSpace(bridgeBase)
            ? directAppLink
            : $"{bridgeBase}#token={encodedToken}";

        await _emailService.SendPasswordResetAsync(
            normalizedEmail,
            user.FullName,
            browserResetLink,
            directAppLink);

        return PasswordResetRequestResult.Accepted;
    }

    public async Task<bool> IsTokenValidAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return false;

        var hash = HashToken(rawToken);
        var now = DateTimeOffset.UtcNow;
        await using var db = new AppDbContext();
        return await db.PasswordResetTokens.AnyAsync(x =>
            x.TokenHash == hash && x.UsedAtUtc == null && x.ExpiresAtUtc > now);
    }

    public async Task<bool> ResetPasswordAsync(string rawToken, string newPassword)
    {
        var policyError = PasswordPolicy.Validate(newPassword);
        if (policyError is not null || string.IsNullOrWhiteSpace(rawToken))
            return false;

        var hash = HashToken(rawToken);
        var now = DateTimeOffset.UtcNow;

        await using var db = new AppDbContext();
        var token = await db.PasswordResetTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == hash);

        if (token is null || token.UsedAtUtc is not null || token.ExpiresAtUtc <= now || !token.User.IsActive)
            return false;

        var passwordHash = PasswordHasher.HashPassword(newPassword);
        token.User.PasswordHash = passwordHash;
        token.UsedAtUtc = now;

        var otherTokens = await db.PasswordResetTokens
            .Where(x => x.UserId == token.UserId && x.Id != token.Id && x.UsedAtUtc == null)
            .ToListAsync();
        foreach (var other in otherTokens)
            other.UsedAtUtc = now;

        await db.SaveChangesAsync();
        await AuditService.TryWriteAsync(
            "Đặt lại mật khẩu",
            "Tài khoản",
            $"Đặt lại mật khẩu thành công cho tài khoản {token.User.Username}.",
            token.User.Username,
            token.User.Username,
            token.User.Id);
        LoginAttemptLimiter.RegisterSuccess(token.User.Username);
        return true;
    }

    private static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        try
        {
            return Convert.ToHexString(SHA256.HashData(bytes));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }
}
