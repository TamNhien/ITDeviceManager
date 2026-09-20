using System.Diagnostics;
using System.Text;

namespace ITDeviceManager.Services;

/// <summary>
/// Loads configuration values from a local .env file without adding a third-party dependency.
/// Existing process/user/machine environment variables always take precedence over .env values.
/// </summary>
public static class EnvFileLoader
{
    private const string ExplicitEnvFileVariable = "ITDM_ENV_FILE";

    public static string? LoadedFilePath { get; private set; }

    public static void Load()
    {
        var envFile = FindEnvFile();
        if (envFile is null)
            return;

        foreach (var rawLine in File.ReadLines(envFile, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                line = line[7..].TrimStart();

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            if (!IsValidKey(key))
                continue;

            // Windows/process environment variables are authoritative. The .env file only fills gaps.
            if (Environment.GetEnvironmentVariable(key) is not null)
                continue;

            var value = ParseValue(line[(separatorIndex + 1)..]);
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
        }

        LoadedFilePath = envFile;
        Debug.WriteLine($"[ITDeviceManager] Loaded environment configuration from: {envFile}");
    }

    private static string? FindEnvFile()
    {
        var explicitPath = Environment.GetEnvironmentVariable(ExplicitEnvFileVariable);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            var expanded = Environment.ExpandEnvironmentVariables(explicitPath.Trim().Trim('"'));
            var fullPath = Path.GetFullPath(expanded);
            if (File.Exists(fullPath))
                return fullPath;
        }

        var checkedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var startDirectory in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
                continue;

            var directory = new DirectoryInfo(Path.GetFullPath(startDirectory));
            for (var depth = 0; directory is not null && depth < 10; depth++, directory = directory.Parent)
            {
                if (!checkedDirectories.Add(directory.FullName))
                    continue;

                var candidate = Path.Combine(directory.FullName, ".env");
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
    }

    private static bool IsValidKey(string key)
    {
        if (key.Length == 0 || !(char.IsLetter(key[0]) || key[0] == '_'))
            return false;

        for (var i = 1; i < key.Length; i++)
        {
            var c = key[i];
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                return false;
        }

        return true;
    }

    private static string ParseValue(string rawValue)
    {
        var value = rawValue.Trim();
        if (value.Length == 0)
            return string.Empty;

        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return UnescapeDoubleQuoted(value[1..^1]);

        if (value.Length >= 2 && value[0] == '\'' && value[^1] == '\'')
            return value[1..^1];

        // Allow inline comments in unquoted values only when # is preceded by whitespace.
        for (var i = 1; i < value.Length; i++)
        {
            if (value[i] == '#' && char.IsWhiteSpace(value[i - 1]))
                return value[..i].TrimEnd();
        }

        return value;
    }

    private static string UnescapeDoubleQuoted(string value) => value
        .Replace("\\n", "\n", StringComparison.Ordinal)
        .Replace("\\r", "\r", StringComparison.Ordinal)
        .Replace("\\t", "\t", StringComparison.Ordinal)
        .Replace("\\\"", "\"", StringComparison.Ordinal)
        .Replace("\\\\", "\\", StringComparison.Ordinal);
}
