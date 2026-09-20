using System.Text.RegularExpressions;
using ITDeviceManager.Data;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class EmployeeEditForm : AppForm
{
    private readonly int? _id;
    private readonly TextBox _code = new() { Width = 260 };
    private readonly TextBox _name = new() { Width = 260 };
    private readonly TextBox _email = new() { Width = 260 };
    private readonly TextBox _phone = new() { Width = 260 };
    private readonly ComboBox _department = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ErrorProvider _errors = new();

    public EmployeeEditForm(int? id = null)
    {
        _id = id;
        Text = id is null ? "Thêm nhân viên" : "Sửa nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(500, 370);
        Font = new Font("Segoe UI", 10);
        _errors.ContainerControl = this;

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(24) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Add(table, "Mã nhân viên *", _code);
        Add(table, "Họ tên *", _name);
        Add(table, "Email", _email);
        Add(table, "Điện thoại", _phone);
        Add(table, "Phòng ban *", _department);

        var buttons = new FlowLayoutPanel { AutoSize = true };
        var save = new Button { Text = "Lưu", Width = 100 };
        var cancel = new Button { Text = "Hủy", Width = 100, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange([save, cancel]);
        var buttonRow = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        table.Controls.Add(new Label(), 0, buttonRow);
        table.Controls.Add(buttons, 1, buttonRow);

        Controls.Add(table);
        CancelButton = cancel;
        Load += LoadAsync;
        save.Click += SaveAsync;
    }

    private static void Add(TableLayoutPanel table, string label, Control control)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 10, 3, 3) }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private async void LoadAsync(object? sender, EventArgs e)
    {
        await using var db = new AppDbContext();
        _department.DataSource = await db.Departments.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        _department.DisplayMember = "Name";
        _department.ValueMember = "Id";

        if (_id is null)
            return;

        var employee = await db.Employees.FindAsync(_id);
        if (employee is null)
            return;

        _code.Text = employee.Code;
        _name.Text = employee.FullName;
        _email.Text = employee.Email ?? string.Empty;
        _phone.Text = employee.Phone ?? string.Empty;
        _department.SelectedValue = employee.DepartmentId;
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;

        if (string.IsNullOrWhiteSpace(_code.Text))
        {
            _errors.SetError(_code, "Vui lòng nhập mã nhân viên.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            _errors.SetError(_name, "Vui lòng nhập họ tên.");
            valid = false;
        }

        if (!string.IsNullOrWhiteSpace(_email.Text) && !Regex.IsMatch(_email.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            _errors.SetError(_email, "Email không hợp lệ.");
            valid = false;
        }

        if (!string.IsNullOrWhiteSpace(_phone.Text) && !Regex.IsMatch(_phone.Text.Trim(), @"^[0-9+ .-]{8,20}$"))
        {
            _errors.SetError(_phone, "Số điện thoại không hợp lệ.");
            valid = false;
        }

        var departmentId = _department.SelectedValue is int selectedDepartmentId ? selectedDepartmentId : 0;
        if (departmentId <= 0)
        {
            _errors.SetError(_department, "Vui lòng chọn phòng ban hợp lệ.");
            valid = false;
        }

        if (!valid)
            return;

        await using var db = new AppDbContext();
        var code = _code.Text.Trim();
        if (await db.Employees.AnyAsync(x => x.Code == code && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_code, "Mã nhân viên đã tồn tại.");
            return;
        }

        Models.Employee employee;
        if (_id is null)
        {
            employee = new Models.Employee();
            db.Employees.Add(employee);
        }
        else
        {
            employee = await db.Employees.FindAsync(_id) ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");
        }

        employee.Code = code;
        employee.FullName = _name.Text.Trim();
        employee.Email = string.IsNullOrWhiteSpace(_email.Text) ? null : _email.Text.Trim();
        employee.Phone = string.IsNullOrWhiteSpace(_phone.Text) ? null : _phone.Text.Trim();
        employee.DepartmentId = departmentId;

        await db.SaveChangesAsync();
        DialogResult = DialogResult.OK;
        Close();
    }
}
