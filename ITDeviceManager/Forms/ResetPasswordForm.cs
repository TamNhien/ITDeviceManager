using ITDeviceManager.Common;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class ResetPasswordForm : AppForm
{
    private readonly string _token;
    private readonly PasswordInput _password = new() { Width = 290 };
    private readonly PasswordInput _confirm = new() { Width = 290 };
    private readonly Label _message = new() { AutoSize = true, MaximumSize = new Size(300, 0) };
    private readonly Button _save = new() { Text = "Đặt lại mật khẩu", Width = 160, Height = 38 };

    public ResetPasswordForm(string token)
    {
        _token = token;
        Text = "Đặt lại mật khẩu - IT Device Manager";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(470, 340);
        Font = new Font("Segoe UI", 10);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(82, 28, 20, 20)
        };

        panel.Controls.Add(new Label
        {
            Text = "ĐẶT LẠI MẬT KHẨU",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 16)
        });
        panel.Controls.Add(new Label { Text = "Mật khẩu mới", AutoSize = true });
        panel.Controls.Add(_password);
        panel.Controls.Add(new Label { Text = "Nhập lại mật khẩu", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
        panel.Controls.Add(_confirm);
        panel.Controls.Add(new Label
        {
            Text = $"Mật khẩu từ {PasswordPolicy.MinimumLength}-{PasswordPolicy.MaximumLength} ký tự.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 8, 0, 8)
        });
        panel.Controls.Add(_save);
        panel.Controls.Add(_message);

        Controls.Add(panel);
        AcceptButton = _save;
        Shown += ValidateTokenAsync;
        _save.Click += SaveAsync;
    }

    private async void ValidateTokenAsync(object? sender, EventArgs e)
    {
        var service = new PasswordResetService();
        if (!await service.IsTokenValidAsync(_token))
        {
            _save.Enabled = false;
            _message.ForeColor = Color.Firebrick;
            _message.Text = "Liên kết đặt lại mật khẩu không hợp lệ, đã sử dụng hoặc đã hết hạn.";
        }
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _message.Text = string.Empty;
        var error = PasswordPolicy.Validate(_password.Password);
        if (error is not null)
        {
            _message.ForeColor = Color.Firebrick;
            _message.Text = error;
            return;
        }

        if (!string.Equals(_password.Password, _confirm.Password, StringComparison.Ordinal))
        {
            _message.ForeColor = Color.Firebrick;
            _message.Text = "Mật khẩu nhập lại không khớp.";
            return;
        }

        _save.Enabled = false;
        try
        {
            var service = new PasswordResetService();
            if (!await service.ResetPasswordAsync(_token, _password.Password))
            {
                _message.ForeColor = Color.Firebrick;
                _message.Text = "Liên kết không còn hiệu lực. Hãy yêu cầu một email đặt lại mật khẩu mới.";
                return;
            }

            MessageBox.Show("Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            _save.Enabled = true;
        }
    }
}
