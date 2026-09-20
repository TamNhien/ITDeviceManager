using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class ForgotPasswordForm : AppForm
{
    private readonly TextBox _email = new() { Width = 310 };
    private readonly Button _send = new() { Text = "Gửi email đặt lại mật khẩu", Width = 210, Height = 38 };
    private readonly Label _message = new() { AutoSize = true, MaximumSize = new Size(330, 0) };

    public ForgotPasswordForm()
    {
        Text = "Quên mật khẩu";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(470, 300);
        Font = new Font("Segoe UI", 10);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(70, 28, 20, 20)
        };

        panel.Controls.Add(new Label
        {
            Text = "ĐẶT LẠI MẬT KHẨU",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 14)
        });
        panel.Controls.Add(new Label { Text = "Email đã đăng ký", AutoSize = true });
        panel.Controls.Add(_email);
        panel.Controls.Add(new Label
        {
            Text = "Hệ thống sẽ gửi liên kết dùng một lần, hết hạn sau 15 phút.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            MaximumSize = new Size(320, 0),
            Margin = new Padding(0, 8, 0, 8)
        });
        panel.Controls.Add(_send);
        panel.Controls.Add(_message);

        Controls.Add(panel);
        AcceptButton = _send;
        _send.Click += SendAsync;
    }

    private async void SendAsync(object? sender, EventArgs e)
    {
        _message.Text = string.Empty;
        var email = _email.Text.Trim();
        if (!EmailAddressValidator.IsValid(email))
        {
            _message.ForeColor = Color.Firebrick;
            _message.Text = "Vui lòng nhập email hợp lệ.";
            return;
        }

        if (!PasswordResetRequestLimiter.TryBegin(email, out var retryAfter))
        {
            _message.ForeColor = Color.DarkOrange;
            _message.Text = $"Vui lòng chờ {Math.Ceiling(retryAfter.TotalSeconds)} giây trước khi yêu cầu email mới.";
            return;
        }

        _send.Enabled = false;
        try
        {
            var service = new PasswordResetService();
            var result = await service.RequestResetAsync(email);

            if (result == PasswordResetRequestResult.SmtpNotConfigured)
            {
                _message.ForeColor = Color.DarkOrange;
                _message.Text = "Chưa cấu hình SMTP. Kiểm tra file .env ở thư mục gốc và xem README.md.";
                return;
            }

            _message.ForeColor = Color.DarkGreen;
            _message.Text = "Nếu email đã được đăng ký, bạn sẽ nhận được liên kết đặt lại mật khẩu. Hãy kiểm tra cả thư mục Spam/Junk.";
        }
        catch
        {
            _message.ForeColor = Color.Firebrick;
            _message.Text = "Không thể gửi email lúc này. Vui lòng kiểm tra cấu hình SMTP và kết nối mạng.";
        }
        finally
        {
            _send.Enabled = true;
        }
    }
}
