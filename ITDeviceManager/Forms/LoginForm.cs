using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class LoginForm : AppForm
{
    private readonly TextBox _txtUsername = new() { Width = 300 };
    private readonly PasswordInput _txtPassword = new() { Width = 300 };
    private readonly Button _btnLogin = new() { Text = "Đăng nhập", Width = 300, Height = 42 };
    private readonly CheckBox _chkRemember = new() { Text = "Ghi nhớ tài khoản", AutoSize = true };
    private readonly LinkLabel _lnkForgot = new() { Text = "Quên mật khẩu?", AutoSize = true };
    private readonly LinkLabel _lnkRegister = new() { Text = "Đăng ký tài khoản", AutoSize = true };
    private readonly Label _lblMessage = new() { AutoSize = true, MaximumSize = new Size(300, 0) };

    public LoginForm()
    {
        Text = "Đăng nhập - Quản lý thiết bị CNTT";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(560, 650);
        Font = new Font("Segoe UI", 10F);
        BackColor = AppTheme.Background;

        AppTheme.SetButtonRole(_btnLogin, ButtonRole.Primary);

        var card = new ModernCard
        {
            Width = 400,
            Height = 560,
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(48, 28, 48, 28),
            Anchor = AnchorStyles.None
        };
        card.Location = new Point((ClientSize.Width - card.Width) / 2, (ClientSize.Height - card.Height) / 2);

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            BackColor = AppTheme.Surface,
            Padding = Padding.Empty
        };

        var logo = new PictureBox
        {
            Width = 58,
            Height = 58,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(121, 0, 0, 8)
        };
        try { logo.Image = Icon?.ToBitmap(); } catch { }

        content.Controls.Add(logo);
        content.Controls.Add(new Label
        {
            Text = "IT DEVICE MANAGER",
            AutoSize = false,
            Width = 300,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 18F),
            ForeColor = AppTheme.TextPrimary,
            Margin = Padding.Empty
        });
        content.Controls.Add(new Label
        {
            Text = "Đăng nhập để quản lý thiết bị CNTT",
            AutoSize = false,
            Width = 300,
            Height = 32,
            TextAlign = ContentAlignment.TopCenter,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = AppTheme.TextSecondary,
            Margin = new Padding(0, 0, 0, 14)
        });

        content.Controls.Add(FieldLabel("Tên đăng nhập"));
        content.Controls.Add(_txtUsername);
        content.Controls.Add(FieldLabel("Mật khẩu", 12));
        content.Controls.Add(_txtPassword);

        var options = new TableLayoutPanel
        {
            Width = 300,
            Height = 38,
            ColumnCount = 2,
            Margin = new Padding(0, 8, 0, 8)
        };
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        options.Controls.Add(_chkRemember, 0, 0);
        options.Controls.Add(_lnkForgot, 1, 0);
        _chkRemember.Anchor = AnchorStyles.Left;
        _lnkForgot.Anchor = AnchorStyles.Right;
        content.Controls.Add(options);

        content.Controls.Add(_btnLogin);

        var messageHost = new Panel { Width = 300, Height = 48, Margin = Padding.Empty };
        _lblMessage.Location = new Point(0, 8);
        _lblMessage.ForeColor = AppTheme.Danger;
        messageHost.Controls.Add(_lblMessage);
        content.Controls.Add(messageHost);

        var registerRow = new TableLayoutPanel
        {
            Width = 300,
            Height = 34,
            ColumnCount = 2,
            Margin = Padding.Empty
        };
        registerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        registerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        registerRow.Controls.Add(new Label
        {
            Text = "Chưa có tài khoản?",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = AppTheme.TextSecondary
        }, 0, 0);
        _lnkRegister.Dock = DockStyle.Fill;
        _lnkRegister.AutoSize = false;
        _lnkRegister.TextAlign = ContentAlignment.MiddleLeft;
        _lnkRegister.Margin = new Padding(8, 0, 0, 0);
        registerRow.Controls.Add(_lnkRegister, 1, 0);
        content.Controls.Add(registerRow);

        content.Controls.Add(new Label
        {
            Text = $"Tài khoản quản trị mặc định: {DbInitializer.DefaultAdminUsername}",
            AutoSize = false,
            Width = 300,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = AppTheme.TextSecondary,
            Font = new Font("Segoe UI", 8.8F),
            Margin = new Padding(0, 8, 0, 0)
        });

        card.Controls.Add(content);
        Controls.Add(card);
        AcceptButton = _btnLogin;

        Load += (_, _) => LoadRememberedAccount();
        _btnLogin.Click += LoginAsync;
        _lnkRegister.LinkClicked += (_, _) => OpenRegisterForm();
        _lnkForgot.LinkClicked += (_, _) => OpenForgotPasswordForm();
    }

    private static Label FieldLabel(string text, int top = 0) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI Semibold", 9.5F),
        Margin = new Padding(0, top, 0, 5)
    };

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
            _lblMessage.ForeColor = AppTheme.Success;
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
        _lblMessage.ForeColor = AppTheme.Danger;
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
        _btnLogin.Text = "Đang đăng nhập...";
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
            _btnLogin.Text = "Đăng nhập";
        }
    }
}
