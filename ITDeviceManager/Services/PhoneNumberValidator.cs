namespace ITDeviceManager.Services;

public static class PhoneNumberValidator
{
    public static bool IsValid(string? phoneNumber)
    {
        var normalized = Normalize(phoneNumber);
        if (normalized.Length == 0)
            return false;

        return normalized.Length is >= 8 and <= 15 && normalized.All(char.IsDigit);
    }

    /// <summary>
    /// Chuẩn hóa số điện thoại để hiển thị/lưu theo dạng nội địa.
    /// Ví dụ: +84 776 905 500 hoặc 84776905500 -> 0776905500.
    /// Các số đã ở dạng 0xxxxxxxxx được giữ nguyên.
    /// </summary>
    public static string Normalize(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var value = phoneNumber.Trim();
        var digits = new List<char>(value.Length);
        var hasLeadingPlus = false;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsDigit(c))
            {
                digits.Add(c);
                continue;
            }

            if (c == '+' && i == 0)
            {
                hasLeadingPlus = true;
                continue;
            }

            if (char.IsWhiteSpace(c) || c is '-' or '.' or '(' or ')')
                continue;

            return string.Empty;
        }

        if (digits.Count == 0)
            return string.Empty;

        var normalized = new string(digits.ToArray());

        // Việt Nam: +84xxxxxxxxx / 84xxxxxxxxx -> 0xxxxxxxxx.
        // Chỉ chuyển khi phần sau mã quốc gia có ít nhất 8 chữ số để tránh đổi nhầm dữ liệu ngắn.
        if ((hasLeadingPlus || normalized.StartsWith("84", StringComparison.Ordinal)) &&
            normalized.StartsWith("84", StringComparison.Ordinal) &&
            normalized.Length >= 10)
        {
            normalized = "0" + normalized[2..];
        }

        return normalized;
    }
}
