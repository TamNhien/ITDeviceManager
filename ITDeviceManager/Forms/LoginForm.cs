using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class LoginForm : AppForm
{
    private readonly TextBox _txtUsername = new() { Width = 290 };
    private readonly PasswordInput _txtPassword = new() { Width = 290 };
    private readonly Button _btnLogin = new() { Text = "Đăng nhập", Width = 125, Height = 38 };
    private readonly CheckBox _chkRemember = new() { Text = "Ghi nhớ tài khoản", AutoSize = true };
    private readonly LinkLabel _lnkForgot = new() { Text = "Quên mật khẩu?", AutoSize = true };
    private readonly LinkLabel _lnkRegister = new() { Text = "Đăng ký tài khoản", AutoSize = true };
    private readonly Label _lblMessage = new() { AutoSize = true, ForeColor = Color.Firebrick, MaximumSize = new Size(300, 0) };

    public LoginForm()
    {
        Text = "Đăng nhập - Quản lý thiết bị CNTT";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(470, 455);
        Font = new Font("Segoe UI", 10);

        var title = new Label
        {
            Text = "QUẢN LÝ THIẾT BỊ CNTT",
            AutoSize = false,
            Width = 290,
            Height = 48,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(82, 28, 20, 20)
        };

        panel.Controls.Add(title);
        panel.Controls.Add(new Label { Text = "Tên đăng nhập", AutoSize = true });
        panel.Controls.Add(_txtUsername);
        panel.Controls.Add(new Label { Text = "Mật khẩu", AutoSize = true, Margin = new Padding(3, 12, 3, 0) });
        panel.Controls.Add(_txtPassword);

        var options = new TableLayoutPanel
        {
            Width = 290,
            Height = 34,
            ColumnCount = 2,
            Margin = new Padding(0, 6, 0, 6)
        };
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        options.Controls.Add(_chkRemember, 0, 0);
        options.Controls.Add(_lnkForgot, 1, 0);
        _chkRemember.Anchor = AnchorStyles.Left;
        _lnkForgot.Anchor = AnchorStyles.Right;
        panel.Controls.Add(options);

        var loginRow = new TableLayoutPanel
        {
            Width = 290,
            Height = 50,
            ColumnCount = 1,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        loginRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        loginRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _btnLogin.Anchor = AnchorStyles.None;
        loginRow.Controls.Add(_btnLogin, 0, 0);
        panel.Controls.Add(loginRow);

        var messageRow = new Panel
        {
            Width = 290,
            Height = 36,
            Margin = Padding.Empty
        };
        _lblMessage.Location = new Point(0, 2);
        _lblMessage.MaximumSize = new Size(290, 34);
        messageRow.Controls.Add(_lblMessage);
        panel.Controls.Add(messageRow);

        var registerRow = new TableLayoutPanel
        {
            Width = 290,
            Height = 36,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 0),
            Padding = Padding.Empty
        };
        registerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        registerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        registerRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var registerPrompt = new Label
        {
            Text = "Chưa có tài khoản?",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Margin = Padding.Empty
        };
        _lnkRegister.AutoSize = false;
        _lnkRegister.Dock = DockStyle.Fill;
        _lnkRegister.TextAlign = ContentAlignment.MiddleLeft;
        _lnkRegister.Margin = new Padding(8, 0, 0, 0);

        registerRow.Controls.Add(registerPrompt, 0, 0);
        registerRow.Controls.Add(_lnkRegister, 1, 0);
        panel.Controls.Add(registerRow);

        panel.Controls.Add(new Label
        {
            Text = $"Tài khoản mới mặc định: {DbInitializer.DefaultAdminUsername}",
            AutoSize = false,
            Width = 290,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 8, 0, 0)
        });

        Controls.Add(panel);
        AcceptButton = _btnLogin;

        Load += (_, _) => LoadRememberedAccount();
        _btnLogin.Click += LoginAsync;
        _lnkRegister.LinkClicked += (_, _) => OpenRegisterForm();
        _lnkForgot.LinkClicked += (_, _) => OpenForgotPasswordForm();
    }

    private void LoadRememberedAccount()
    {
        var preferences = RememberMeService.Load();
        _chkRemember.Checked = preferences.RememberUsername;
        if (preferences.RememberUsername)
        {
            _txtUsername.Text = preferences.Username;
            _txtPassword.FocusInput();
        }
    }

    private void OpenRegisterForm()
    {
        using var form = new RegisterForm();
        if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(form.RegisteredUsername))
        {
            _txtUsername.Text = form.RegisteredUsername;
            _txtPassword.Password = string.Empty;
            _txtPassword.FocusInput();
            _lblMessage.ForeColor = Color.DarkGreen;
            _lblMessage.Text = "Đăng ký thành công. Bạn có thể đăng nhập ngay.";
        }
    }

    private void OpenForgotPasswordForm()
    {
        using var form = new ForgotPasswordForm();
        form.ShowDialog(this);
    }

    private async void LoginAsync(object? sender, EventArgs e)
    {
        _lblMessage.ForeColor = Color.Firebrick;
        _lblMessage.Text = string.Empty;
        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _lblMessage.Text = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
            return;
        }

        if (!LoginAttemptLimiter.TryBegin(username, out var retryAfter))
        {
            _lblMessage.Text = $"Đăng nhập tạm khóa do thử sai nhiều lần. Vui lòng thử lại sau {Math.Ceiling(retryAfter.TotalSeconds)} giây.";
            return;
        }

        _btnLogin.Enabled = false;
        try
        {
            var auth = new AuthService();
            if (await auth.LoginAsync(username, password))
            {
                LoginAttemptLimiter.RegisterSuccess(username);
                RememberMeService.Save(_chkRemember.Checked, username);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                LoginAttemptLimiter.RegisterFailure(username);
                _lblMessage.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }
        }
        catch
        {
            _lblMessage.Text = "Không thể đăng nhập lúc này. Vui lòng kiểm tra kết nối cơ sở dữ liệu.";
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}
