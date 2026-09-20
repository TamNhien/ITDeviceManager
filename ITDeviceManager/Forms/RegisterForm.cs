using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class RegisterForm : AppForm
{
    private readonly TextBox _username = new() { Width = 290 };
    private readonly TextBox _fullName = new() { Width = 290 };
    private readonly TextBox _email = new() { Width = 290 };
    private readonly PasswordInput _password = new() { Width = 290 };
    private readonly PasswordInput _confirmPassword = new() { Width = 290 };
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
        ClientSize = new Size(540, 500);
        Font = new Font("Segoe UI", 10);
        _errors.ContainerControl = this;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(24),
            AutoScroll = true
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, "Tên đăng nhập *", _username);
        AddRow(table, "Họ tên *", _fullName);
        AddRow(table, "Email *", _email);
        AddRow(table, "Mật khẩu *", _password);
        AddRow(table, "Nhập lại mật khẩu *", _confirmPassword);
        AddRow(table, "", new Label
        {
            Text = $"Mật khẩu từ {PasswordPolicy.MinimumLength}-{PasswordPolicy.MaximumLength} ký tự. Tài khoản tự đăng ký có quyền Staff.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            MaximumSize = new Size(300, 0)
        }, 60);

        var buttons = new FlowLayoutPanel { AutoSize = true };
        var cancel = new Button { Text = "Hủy", Width = 100, Height = 36, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange([_save, cancel]);
        AddRow(table, "", buttons, 52);

        Controls.Add(table);
        AcceptButton = _save;
        CancelButton = cancel;
        _save.Click += SaveAsync;
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

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;
        var username = _username.Text.Trim();
        var fullName = _fullName.Text.Trim();
        var email = _email.Text.Trim();

        if (username.Length < 3 || username.Length > 100 || username.Any(char.IsWhiteSpace))
        {
            _errors.SetError(_username, "Tên đăng nhập phải từ 3-100 ký tự và không chứa khoảng trắng.");
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

            var (hash, salt) = PasswordHasher.HashPassword(_password.Password);
            db.Users.Add(new User
            {
                Username = username,
                FullName = fullName,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
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
            MessageBox.Show("Không thể tạo tài khoản. Tên đăng nhập hoặc email có thể đã tồn tại.", "Đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _save.Enabled = true;
        }
    }
}
