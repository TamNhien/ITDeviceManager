namespace ITDeviceManager;

public static class AppSettings
{
    // SQL Server instance currently used by the project.
    // Override with .env / Windows environment variable ITDM_CONNECTION_STRING when needed.
    private const string DefaultConnectionString =
        @"Server=CANHTHIEN;Database=ITDeviceManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("ITDM_CONNECTION_STRING") is { Length: > 0 } value
            ? value
            : DefaultConnectionString;

    // SMTP secrets are intentionally not committed to source control.
    public static string SmtpHost => Environment.GetEnvironmentVariable("ITDM_SMTP_HOST")?.Trim() ?? string.Empty;
    public static int SmtpPort => int.TryParse(Environment.GetEnvironmentVariable("ITDM_SMTP_PORT"), out var port) ? port : 587;
    public static string SmtpUsername => Environment.GetEnvironmentVariable("ITDM_SMTP_USERNAME")?.Trim() ?? string.Empty;
    public static string SmtpPassword => Environment.GetEnvironmentVariable("ITDM_SMTP_PASSWORD") ?? string.Empty;
    public static string SmtpFromEmail => Environment.GetEnvironmentVariable("ITDM_SMTP_FROM_EMAIL")?.Trim() ?? SmtpUsername;
    public static string SmtpFromName => Environment.GetEnvironmentVariable("ITDM_SMTP_FROM_NAME")?.Trim() ?? "IT Device Manager";
    public static bool SmtpUseSslOnConnect => bool.TryParse(Environment.GetEnvironmentVariable("ITDM_SMTP_SSL_ON_CONNECT"), out var useSsl) && useSsl;

    public static bool IsSmtpConfigured =>
        !string.IsNullOrWhiteSpace(SmtpHost) &&
        SmtpPort > 0 &&
        !string.IsNullOrWhiteSpace(SmtpUsername) &&
        !string.IsNullOrWhiteSpace(SmtpPassword) &&
        !string.IsNullOrWhiteSpace(SmtpFromEmail);

    public const string PasswordResetScheme = "itdevicemanager";
    public const int PasswordResetExpiryMinutes = 15;

    // Gmail and several webmail clients intentionally block custom URI schemes in email HTML.
    // The email therefore points to this HTTPS bridge page. The reset token is placed in the
    // URL fragment (#token=...), so the token is NOT sent to GitHub Pages in the HTTP request.
    private const string DefaultPasswordResetWebUrl =
        "https://tamnhien.github.io/ITDeviceManager/reset-password.html";

    public static string PasswordResetWebUrl =>
        Environment.GetEnvironmentVariable("ITDM_PASSWORD_RESET_WEB_URL")?.Trim() is { Length: > 0 } value
            ? value
            : DefaultPasswordResetWebUrl;
}
