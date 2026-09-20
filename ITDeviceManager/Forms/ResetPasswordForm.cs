using ITDeviceManager.Common;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class ResetPasswordForm : AppForm
{
    private readonly string _token;
    private readonly PasswordInput _password = new() { Width = 300 };
    private readonly PasswordInput _confirm = new() { Width = 300 };
    private readonly Label _strengthLabel = new() { AutoSize = true };
    private readonly Label _requirementsLabel = new() { AutoSize = true, MaximumSize = new Size(320, 0) };
    private readonly Label _confirmLabel = new() { AutoSize = true, MaximumSize = new Size(320, 0) };
    private readonly Label _message = new() { AutoSize = true, MaximumSize = new Size(320, 0) };
    private readonly Button _save = new()
    {
        Text = "Đặt lại mật khẩu",
        Width = 170,
        Height = 38,
        Enabled = false
    };

    private bool _tokenIsValid;

    public ResetPasswordForm(string token)
    {
        _token = token;

        Text = "Đặt lại mật khẩu - IT Device Manager";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 510);
        Font = new Font("Segoe UI", 10);

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(90, 28, 30, 24),
            AutoScroll = true
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "ĐẶT LẠI MẬT KHẨU",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Anchor = AnchorStyles.Left
        };
        table.Controls.Add(title, 0, 0);

        table.Controls.Add(CreatePasswordField("Mật khẩu mới", _password), 0, 1);
        table.Controls.Add(CreatePasswordField("Nhập lại mật khẩu", _confirm), 0, 2);

        _strengthLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _strengthLabel.Anchor = AnchorStyles.Left;
        table.Controls.Add(_strengthLabel, 0, 3);

        _requirementsLabel.ForeColor = Color.DimGray;
        _requirementsLabel.Anchor = AnchorStyles.Left;
        table.Controls.Add(_requirementsLabel, 0, 4);

        _confirmLabel.ForeColor = Color.DimGray;
        _confirmLabel.Anchor = AnchorStyles.Left;
        table.Controls.Add(_confirmLabel, 0, 5);

        var buttonHost = new Panel { Dock = DockStyle.Fill };
        _save.Location = new Point(0, 6);
        buttonHost.Controls.Add(_save);
        table.Controls.Add(buttonHost, 0, 6);

        _message.Margin = new Padding(0, 6, 0, 0);
        table.Controls.Add(_message, 0, 7);

        Controls.Add(table);
        AcceptButton = _save;

        _password.PasswordChanged += (_, _) => RefreshPasswordFeedback();
        _confirm.PasswordChanged += (_, _) => RefreshPasswordFeedback();
        Shown += ValidateTokenAsync;
        _save.Click += SaveAsync;

        RefreshPasswordFeedback();
    }

    private static Control CreatePasswordField(string caption, PasswordInput input)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        panel.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);

        input.Anchor = AnchorStyles.Left;
        panel.Controls.Add(input, 0, 1);
        return panel;
    }

    private void RefreshPasswordFeedback()
    {
        var assessment = PasswordPolicy.Assess(_password.Password);

        _strengthLabel.Text = $"Độ mạnh mật khẩu: {assessment.StrengthText}";
        _strengthLabel.ForeColor = assessment.Strength switch
        {
            PasswordStrength.Strong or PasswordStrength.VeryStrong => Color.DarkGreen,
            PasswordStrength.Medium => Color.DarkOrange,
            PasswordStrength.Weak => Color.Firebrick,
            _ => Color.DimGray
        };

        static string Mark(bool ok) => ok ? "✓" : "✗";
        _requirementsLabel.Text =
            $"{Mark(assessment.HasMinimumLength)} Từ {PasswordPolicy.MinimumLength} ký tự    " +
            $"{Mark(assessment.HasUppercase)} Chữ hoa\n" +
            $"{Mark(assessment.HasLowercase)} Chữ thường    " +
            $"{Mark(assessment.HasDigit)} Số    " +
            $"{Mark(assessment.HasSpecialCharacter)} Ký tự đặc biệt";

        var passwordsMatch =
            _confirm.Password.Length > 0 &&
            string.Equals(_password.Password, _confirm.Password, StringComparison.Ordinal);

        if (_confirm.Password.Length == 0)
        {
            _confirmLabel.ForeColor = Color.DimGray;
            _confirmLabel.Text = "Nhập lại mật khẩu để kiểm tra trùng khớp.";
        }
        else if (passwordsMatch)
        {
            _confirmLabel.ForeColor = Color.DarkGreen;
            _confirmLabel.Text = "✓ Mật khẩu nhập lại trùng khớp.";
        }
        else
        {
            _confirmLabel.ForeColor = Color.Firebrick;
            _confirmLabel.Text = "✗ Mật khẩu nhập lại chưa trùng khớp.";
        }

        _save.Enabled = _tokenIsValid && assessment.MeetsPolicy && passwordsMatch;
    }

    private async void ValidateTokenAsync(object? sender, EventArgs e)
    {
        _save.Enabled = false;

        try
        {
            var service = new PasswordResetService();
            _tokenIsValid = await service.IsTokenValidAsync(_token);

            if (!_tokenIsValid)
            {
                _message.ForeColor = Color.Firebrick;
                _message.Text = "Liên kết đặt lại mật khẩu không hợp lệ, đã sử dụng hoặc đã hết hạn.";
            }
            else
            {
                _message.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _tokenIsValid = false;
            _message.ForeColor = Color.Firebrick;
            _message.Text = "Không thể kiểm tra liên kết đặt lại mật khẩu.\n" + ex.Message;
        }

        RefreshPasswordFeedback();
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _message.Text = string.Empty;

        var error = PasswordPolicy.Validate(_password.Password);
        if (error is not null)
        {
            _message.ForeColor = Color.Firebrick;
            _message.Text = error;
            RefreshPasswordFeedback();
            return;
        }

        if (!string.Equals(_password.Password, _confirm.Password, StringComparison.Ordinal))
        {
            _message.ForeColor = Color.Firebrick;
            _message.Text = "Mật khẩu nhập lại không khớp.";
            RefreshPasswordFeedback();
            return;
        }

        _save.Enabled = false;
        try
        {
            var service = new PasswordResetService();
            if (!await service.ResetPasswordAsync(_token, _password.Password))
            {
                _tokenIsValid = false;
                _message.ForeColor = Color.Firebrick;
                _message.Text = "Liên kết không còn hiệu lực. Hãy yêu cầu một email đặt lại mật khẩu mới.";
                RefreshPasswordFeedback();
                return;
            }

            MessageBox.Show(
                "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            if (!IsDisposed)
                RefreshPasswordFeedback();
        }
    }
}
