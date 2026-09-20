namespace ITDeviceManager.Services;

public static class PasswordPolicy
{
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;

    public static PasswordAssessment Assess(string password)
    {
        password ??= string.Empty;

        var hasUppercase = password.Any(char.IsUpper);
        var hasLowercase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        var hasMinimumLength = password.Length >= MinimumLength;

        var categoryScore =
            (hasUppercase ? 1 : 0) +
            (hasLowercase ? 1 : 0) +
            (hasDigit ? 1 : 0) +
            (hasSpecial ? 1 : 0);

        var strength = password.Length == 0
            ? PasswordStrength.Empty
            : categoryScore <= 2 || password.Length < 8
                ? PasswordStrength.Weak
                : categoryScore == 3 || !hasMinimumLength
                    ? PasswordStrength.Medium
                    : password.Length >= 16
                        ? PasswordStrength.VeryStrong
                        : PasswordStrength.Strong;

        return new PasswordAssessment(
            hasMinimumLength,
            password.Length <= MaximumLength,
            hasUppercase,
            hasLowercase,
            hasDigit,
            hasSpecial,
            strength);
    }

    public static string? Validate(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "Vui lòng nhập mật khẩu.";

        if (password.Length < MinimumLength)
            return $"Mật khẩu phải có ít nhất {MinimumLength} ký tự.";

        if (password.Length > MaximumLength)
            return $"Mật khẩu không được vượt quá {MaximumLength} ký tự.";

        var assessment = Assess(password);
        if (!assessment.HasUppercase)
            return "Mật khẩu phải có ít nhất 1 chữ hoa.";

        if (!assessment.HasLowercase)
            return "Mật khẩu phải có ít nhất 1 chữ thường.";

        if (!assessment.HasDigit)
            return "Mật khẩu phải có ít nhất 1 chữ số.";

        if (!assessment.HasSpecialCharacter)
            return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.";

        return null;
    }
}

public enum PasswordStrength
{
    Empty = 0,
    Weak = 1,
    Medium = 2,
    Strong = 3,
    VeryStrong = 4
}

public sealed record PasswordAssessment(
    bool HasMinimumLength,
    bool IsWithinMaximumLength,
    bool HasUppercase,
    bool HasLowercase,
    bool HasDigit,
    bool HasSpecialCharacter,
    PasswordStrength Strength)
{
    public bool MeetsPolicy =>
        HasMinimumLength &&
        IsWithinMaximumLength &&
        HasUppercase &&
        HasLowercase &&
        HasDigit &&
        HasSpecialCharacter;

    public string StrengthText => Strength switch
    {
        PasswordStrength.Weak => "Yếu",
        PasswordStrength.Medium => "Trung bình",
        PasswordStrength.Strong => "Mạnh",
        PasswordStrength.VeryStrong => "Rất mạnh",
        _ => "Chưa nhập"
    };
}
