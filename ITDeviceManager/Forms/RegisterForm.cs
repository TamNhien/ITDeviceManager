using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class RegisterForm : AppForm
{
    private readonly TextBox _username = new() { Width = 300, MaxLength = 100 };
    private readonly TextBox _fullName = new() { Width = 300, MaxLength = 200 };
    private readonly TextBox _email = new() { Width = 300, MaxLength = 320 };
    private readonly TextBox _phone = new() { Width = 300, MaxLength = 32 };
    private readonly PasswordInput _password = new() { Width = 300 };
    private readonly PasswordInput _confirmPassword = new() { Width = 300 };
    private readonly Label _strengthLabel = new() { AutoSize = true };
    private readonly Label _requirementsLabel = new() { AutoSize = true, MaximumSize = new Size(320, 0) };
    private readonly Label _confirmLabel = new() { AutoSize = true, MaximumSize = new Size(320, 0) };
    private readonly ErrorProvider _errors = new();
    private readonly Button _save = new() { Text = "Đăng ký", Width = 120, Height = 36 };

    public string RegisteredUsername { get; private set; } = string.Empty;

    public RegisterForm()
    {
        Text = "Đăng ký tài khoản";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(590, 590);
        Font = new Font("Segoe UI", 10);
        _errors.ContainerControl = this;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(24),
            AutoScroll = true
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, "Tên đăng nhập *", _username);
        AddRow(table, "Họ tên *", _fullName);
        AddRow(table, "Email *", _email);
        AddRow(table, "Số điện thoại *", _phone);
        AddRow(table, "Mật khẩu *", _password);
        AddRow(table, "Nhập lại mật khẩu *", _confirmPassword);

        var passwordFeedback = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = Padding.Empty,
            MaximumSize = new Size(330, 0)
        };
        _strengthLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _requirementsLabel.ForeColor = Color.DimGray;
        _confirmLabel.ForeColor = Color.DimGray;
        passwordFeedback.Controls.AddRange([_strengthLabel, _requirementsLabel, _confirmLabel]);
        AddRow(table, "", passwordFeedback, 118);

        var buttons = new FlowLayoutPanel { AutoSize = true };
        var cancel = new Button { Text = "Hủy", Width = 100, Height = 36, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange([_save, cancel]);
        AddRow(table, "", buttons, 52);

        Controls.Add(table);
        AcceptButton = _save;
        CancelButton = cancel;

        _username.TextChanged += (_, _) =>
        {
            if (IsValidUsername(_username.Text.Trim()))
                _errors.SetError(_username, string.Empty);
        };
        _password.PasswordChanged += (_, _) => RefreshPasswordFeedback();
        _confirmPassword.PasswordChanged += (_, _) => RefreshPasswordFeedback();
        _save.Click += SaveAsync;
        RefreshPasswordFeedback();
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control, int height = 54)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        table.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Margin = new Padding(3, 8, 3, 3)
        }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private static bool IsValidUsername(string username)
    {
        if (username.Length < 3 || username.Length > 100)
            return false;

        // Cho phép tên đăng nhập có chữ Unicode/tiếng Việt, chữ số, khoảng trắng,
        // dấu chấm, gạch dưới và gạch ngang. Ví dụ: "Đạt Br".
        return username.All(c =>
            char.IsLetterOrDigit(c) ||
            char.IsWhiteSpace(c) ||
            c is '.' or '_' or '-');
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
            $"{Mark(assessment.HasMinimumLength)} Từ {PasswordPolicy.MinimumLength} ký tự   " +
            $"{Mark(assessment.HasUppercase)} Chữ hoa\n" +
            $"{Mark(assessment.HasLowercase)} Chữ thường   " +
            $"{Mark(assessment.HasDigit)} Số   " +
            $"{Mark(assessment.HasSpecialCharacter)} Ký tự đặc biệt";

        if (_confirmPassword.Password.Length == 0)
        {
            _confirmLabel.ForeColor = Color.DimGray;
            _confirmLabel.Text = "Nhập lại mật khẩu để kiểm tra trùng khớp.";
        }
        else if (string.Equals(_password.Password, _confirmPassword.Password, StringComparison.Ordinal))
        {
            _confirmLabel.ForeColor = Color.DarkGreen;
            _confirmLabel.Text = "✓ Mật khẩu nhập lại trùng khớp.";
        }
        else
        {
            _confirmLabel.ForeColor = Color.Firebrick;
            _confirmLabel.Text = "✗ Mật khẩu nhập lại chưa trùng khớp.";
        }
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;
        var username = _username.Text.Trim();
        var fullName = _fullName.Text.Trim();
        var email = _email.Text.Trim();
        var phone = PhoneNumberValidator.Normalize(_phone.Text);

        if (!IsValidUsername(username))
        {
            _errors.SetError(_username, "Tên đăng nhập phải từ 3-100 ký tự; chỉ dùng chữ, số, khoảng trắng, dấu chấm, gạch dưới hoặc gạch ngang.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            _errors.SetError(_fullName, "Vui lòng nhập họ tên.");
            valid = false;
        }

        if (!EmailAddressValidator.IsValid(email))
        {
            _errors.SetError(_email, "Email không hợp lệ.");
            valid = false;
        }

        if (!PhoneNumberValidator.IsValid(_phone.Text))
        {
            _errors.SetError(_phone, "Số điện thoại phải có từ 8-15 chữ số. Có thể dùng +84, khoảng trắng, dấu chấm hoặc gạch ngang.");
            valid = false;
        }

        var passwordError = PasswordPolicy.Validate(_password.Password);
        if (passwordError is not null)
        {
            _errors.SetError(_password, passwordError);
            valid = false;
        }

        if (!string.Equals(_password.Password, _confirmPassword.Password, StringComparison.Ordinal))
        {
            _errors.SetError(_confirmPassword, "Mật khẩu nhập lại không khớp.");
            valid = false;
        }

        if (!valid)
            return;

        _save.Enabled = false;
        try
        {
            await using var db = new AppDbContext();
            if (await db.Users.AnyAsync(x => x.Username == username))
            {
                _errors.SetError(_username, "Tên đăng nhập đã tồn tại.");
                return;
            }

            if (await db.Users.AnyAsync(x => x.Email == email))
            {
                _errors.SetError(_email, "Email đã được đăng ký.");
                return;
            }

            if (await db.Users.AnyAsync(x => x.PhoneNumber == phone))
            {
                _errors.SetError(_phone, "Số điện thoại đã được đăng ký.");
                return;
            }

            var hash = PasswordHasher.HashPassword(_password.Password);
            db.Users.Add(new User
            {
                Username = username,
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = hash,
                RoleId = 2,
                IsActive = true
            });

            await db.SaveChangesAsync();
            RegisteredUsername = username;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DbUpdateException)
        {
            MessageBox.Show("Không thể tạo tài khoản. Tên đăng nhập, email hoặc số điện thoại có thể đã tồn tại.", "Đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _save.Enabled = true;
        }
    }
}
