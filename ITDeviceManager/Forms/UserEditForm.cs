using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class UserEditForm : AppForm
{
    private readonly int? _id;
    private readonly TextBox _username = new() { Width = 280, MaxLength = 100 };
    private readonly TextBox _fullName = new() { Width = 280, MaxLength = 200 };
    private readonly TextBox _email = new() { Width = 280, MaxLength = 320 };
    private readonly TextBox _phone = new() { Width = 280, MaxLength = 32 };
    private readonly PasswordInput _password = new() { Width = 280 };
    private readonly PasswordInput _passwordConfirm = new() { Width = 280 };
    private readonly ComboBox _role = new() { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _active = new() { Text = "Đang hoạt động", Checked = true };
    private readonly ErrorProvider _errors = new();

    public UserEditForm(int? id = null)
    {
        _id = id;
        Text = id is null ? "Thêm tài khoản" : "Sửa tài khoản";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(590, 610);
        Font = new Font("Segoe UI", 10);
        _errors.ContainerControl = this;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(24),
            AutoScroll = true
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, "Tên đăng nhập *", _username);
        AddRow(table, "Họ tên *", _fullName);
        AddRow(table, "Email", _email);
        AddRow(table, "Số điện thoại", _phone);
        AddRow(table, id is null ? "Mật khẩu *" : "Mật khẩu mới", _password);
        AddRow(table, id is null ? "Nhập lại mật khẩu *" : "Nhập lại mật khẩu", _passwordConfirm);

        var hint = new Label
        {
            Text = $"Mật khẩu {PasswordPolicy.MinimumLength}-{PasswordPolicy.MaximumLength} ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt. Khi sửa tài khoản, để trống nếu không đổi mật khẩu.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            MaximumSize = new Size(330, 0)
        };
        AddRow(table, "", hint, 82);

        AddRow(table, "Quyền *", _role);
        AddRow(table, "Trạng thái", _active);

        var buttons = new FlowLayoutPanel { AutoSize = true };
        var save = new Button { Text = "Lưu", Width = 100, Height = 34 };
        var cancel = new Button { Text = "Hủy", Width = 100, Height = 34, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange([save, cancel]);
        AddRow(table, "", buttons, 50);

        Controls.Add(table);
        CancelButton = cancel;

        Load += LoadAsync;
        save.Click += SaveAsync;
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control, int height = 52)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        table.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Margin = new Padding(3, 10, 3, 3)
        }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private async void LoadAsync(object? sender, EventArgs e)
    {
        await using var db = new AppDbContext();
        _role.DataSource = await db.Roles.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        _role.DisplayMember = "Name";
        _role.ValueMember = "Id";

        if (_id is null)
            return;

        var user = await db.Users.FindAsync(_id);
        if (user is null)
            return;

        _username.Text = user.Username;
        _fullName.Text = user.FullName;
        _email.Text = user.Email ?? string.Empty;
        _phone.Text = PhoneNumberValidator.Normalize(user.PhoneNumber);
        _role.SelectedValue = user.RoleId;
        _active.Checked = user.IsActive;
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;
        var email = _email.Text.Trim();
        var phone = PhoneNumberValidator.Normalize(_phone.Text);

        if (string.IsNullOrWhiteSpace(_username.Text))
        {
            _errors.SetError(_username, "Vui lòng nhập tên đăng nhập.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(_fullName.Text))
        {
            _errors.SetError(_fullName, "Vui lòng nhập họ tên.");
            valid = false;
        }

        if (email.Length > 0 && !EmailAddressValidator.IsValid(email))
        {
            _errors.SetError(_email, "Email không hợp lệ.");
            valid = false;
        }

        if (_phone.Text.Trim().Length > 0 && !PhoneNumberValidator.IsValid(_phone.Text))
        {
            _errors.SetError(_phone, "Số điện thoại phải có từ 8-15 chữ số.");
            valid = false;
        }

        var isChangingPassword = _id is null || _password.Password.Length > 0 || _passwordConfirm.Password.Length > 0;
        if (isChangingPassword)
        {
            var passwordError = PasswordPolicy.Validate(_password.Password);
            if (passwordError is not null)
            {
                _errors.SetError(_password, passwordError);
                valid = false;
            }

            if (!string.Equals(_password.Password, _passwordConfirm.Password, StringComparison.Ordinal))
            {
                _errors.SetError(_passwordConfirm, "Mật khẩu nhập lại không khớp.");
                valid = false;
            }
        }

        var roleId = _role.SelectedValue is int selectedRoleId ? selectedRoleId : 0;
        if (roleId <= 0)
        {
            _errors.SetError(_role, "Vui lòng chọn quyền hợp lệ.");
            valid = false;
        }

        if (!valid)
            return;

        await using var db = new AppDbContext();
        var username = _username.Text.Trim();

        if (await db.Users.AnyAsync(x => x.Username == username && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_username, "Tên đăng nhập đã tồn tại.");
            return;
        }

        if (email.Length > 0 && await db.Users.AnyAsync(x => x.Email == email && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_email, "Email đã được sử dụng bởi tài khoản khác.");
            return;
        }

        if (phone.Length > 0 && await db.Users.AnyAsync(x => x.PhoneNumber == phone && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_phone, "Số điện thoại đã được sử dụng bởi tài khoản khác.");
            return;
        }

        Models.User user;
        if (_id is null)
        {
            user = new Models.User();
            db.Users.Add(user);
        }
        else
        {
            user = await db.Users.FindAsync(_id) ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");
        }

        user.Username = username;
        user.FullName = _fullName.Text.Trim();
        user.Email = email.Length == 0 ? null : email;
        user.PhoneNumber = phone.Length == 0 ? null : phone;
        user.RoleId = roleId;
        user.IsActive = _active.Checked;

        if (isChangingPassword)
        {
            var hash = PasswordHasher.HashPassword(_password.Password);
            user.PasswordHash = hash;
        }

        await db.SaveChangesAsync();
        DialogResult = DialogResult.OK;
        Close();
    }
}
