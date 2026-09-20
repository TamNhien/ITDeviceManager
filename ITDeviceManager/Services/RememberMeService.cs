using System.Text.Json;

namespace ITDeviceManager.Services;

public sealed record LoginPreferences(bool RememberUsername, string Username);

public static class RememberMeService
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ITDeviceManager");

    private static readonly string FilePath = Path.Combine(DirectoryPath, "login-preferences.json");

    public static LoginPreferences Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new LoginPreferences(false, string.Empty);

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<LoginPreferences>(json)
                   ?? new LoginPreferences(false, string.Empty);
        }
        catch
        {
            return new LoginPreferences(false, string.Empty);
        }
    }

    public static void Save(bool rememberUsername, string username)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);

            if (!rememberUsername)
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                return;
            }

            var preferences = new LoginPreferences(true, username.Trim());
            File.WriteAllText(FilePath, JsonSerializer.Serialize(preferences));
        }
        catch
        {
            // Remember-account is optional and must never block sign-in.
        }
    }
}
