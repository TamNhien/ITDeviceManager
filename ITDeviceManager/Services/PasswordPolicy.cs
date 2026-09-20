namespace ITDeviceManager.Services;

public static class PasswordPolicy
{
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;

    public static string? Validate(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "Vui lòng nhập mật khẩu.";

        if (password.Length < MinimumLength)
            return $"Mật khẩu phải có ít nhất {MinimumLength} ký tự.";

        if (password.Length > MaximumLength)
            return $"Mật khẩu không được vượt quá {MaximumLength} ký tự.";

        if (password.All(char.IsWhiteSpace))
            return "Mật khẩu không được chỉ chứa khoảng trắng.";

        return null;
    }
}
