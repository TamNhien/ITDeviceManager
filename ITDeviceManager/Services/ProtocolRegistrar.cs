using Microsoft.Win32;

namespace ITDeviceManager.Services;

public static class ProtocolRegistrar
{
    public static void RegisterForCurrentUser()
    {
        try
        {
            var executable = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
                return;

            var scheme = AppSettings.PasswordResetScheme;
            using var root = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{scheme}");
            root?.SetValue(string.Empty, "URL:IT Device Manager Password Reset");
            root?.SetValue("URL Protocol", string.Empty);

            using var icon = root?.CreateSubKey("DefaultIcon");
            icon?.SetValue(string.Empty, $"\"{executable}\",0");

            using var command = root?.CreateSubKey(@"shell\open\command");
            command?.SetValue(string.Empty, $"\"{executable}\" \"%1\"");
        }
        catch
        {
            // Protocol registration improves UX but is not required for normal login.
        }
    }

    public static bool TryGetResetToken(string[] args, out string token)
    {
        token = string.Empty;
        var argument = args.FirstOrDefault(x =>
            x.StartsWith($"{AppSettings.PasswordResetScheme}://", StringComparison.OrdinalIgnoreCase));

        if (argument is null || !Uri.TryCreate(argument, UriKind.Absolute, out var uri))
            return false;

        if (!string.Equals(uri.Scheme, AppSettings.PasswordResetScheme, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, "reset-password", StringComparison.OrdinalIgnoreCase))
            return false;

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in query)
        {
            var pair = part.Split('=', 2);
            if (pair.Length == 2 && string.Equals(pair[0], "token", StringComparison.OrdinalIgnoreCase))
            {
                token = Uri.UnescapeDataString(pair[1]);
                return !string.IsNullOrWhiteSpace(token);
            }
        }

        return false;
    }
}
