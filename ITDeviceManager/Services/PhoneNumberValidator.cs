namespace ITDeviceManager.Services;

public static class PhoneNumberValidator
{
    public static bool IsValid(string? phoneNumber)
    {
        var normalized = Normalize(phoneNumber);
        if (normalized.Length == 0)
            return false;

        var digits = normalized[0] == '+' ? normalized[1..] : normalized;
        return digits.Length is >= 8 and <= 15 && digits.All(char.IsDigit);
    }

    public static string Normalize(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var value = phoneNumber.Trim();
        var chars = new List<char>(value.Length);

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsDigit(c))
            {
                chars.Add(c);
                continue;
            }

            if (c == '+' && chars.Count == 0)
            {
                chars.Add(c);
                continue;
            }

            if (char.IsWhiteSpace(c) || c is '-' or '.' or '(' or ')')
                continue;

            return string.Empty;
        }

        return new string(chars.ToArray());
    }
}
