using System.Net.Mail;

namespace ITDeviceManager.Services;

public static class EmailAddressValidator
{
    public static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320)
            return false;

        try
        {
            var address = new MailAddress(email.Trim());
            return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
