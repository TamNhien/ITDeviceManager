using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ITDeviceManager.Services;

public sealed class EmailService
{
    public async Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink)
    {
        if (!AppSettings.IsSmtpConfigured)
            throw new InvalidOperationException("SMTP chưa được cấu hình.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(AppSettings.SmtpFromName, AppSettings.SmtpFromEmail));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = "Đặt lại mật khẩu - IT Device Manager";

        var encodedName = System.Net.WebUtility.HtmlEncode(recipientName);
        var encodedLink = System.Net.WebUtility.HtmlEncode(resetLink);
        var expiry = AppSettings.PasswordResetExpiryMinutes;

        message.Body = new BodyBuilder
        {
            TextBody = $"""
Xin chào {recipientName},

Bạn vừa yêu cầu đặt lại mật khẩu cho IT Device Manager.
Liên kết đặt lại mật khẩu có hiệu lực trong {expiry} phút và chỉ dùng được một lần:

{resetLink}

Nếu bạn không yêu cầu thao tác này, hãy bỏ qua email.
""",
            HtmlBody = $"""
<!doctype html>
<html lang="vi">
<body style="font-family:Segoe UI,Arial,sans-serif;color:#202124;line-height:1.6">
  <h2>Đặt lại mật khẩu IT Device Manager</h2>
  <p>Xin chào <strong>{encodedName}</strong>,</p>
  <p>Bạn vừa yêu cầu đặt lại mật khẩu. Liên kết dưới đây có hiệu lực trong <strong>{expiry} phút</strong> và chỉ sử dụng được một lần.</p>
  <p><a href="{encodedLink}" style="display:inline-block;padding:12px 18px;background:#6f42c1;color:white;text-decoration:none;border-radius:6px">Đặt lại mật khẩu</a></p>
  <p>Nếu nút không mở được, hãy sao chép liên kết sau và mở trên máy đã cài ứng dụng:</p>
  <p style="word-break:break-all">{encodedLink}</p>
  <p>Nếu bạn không yêu cầu thao tác này, hãy bỏ qua email.</p>
</body>
</html>
"""
        }.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = AppSettings.SmtpUseSslOnConnect
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(AppSettings.SmtpHost, AppSettings.SmtpPort, socketOptions);
        await client.AuthenticateAsync(AppSettings.SmtpUsername, AppSettings.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
