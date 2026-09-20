using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DeviceEditForm : AppForm
{
    private readonly int? _id;
    private readonly TextBox _code = new() { Width = 260 };
    private readonly TextBox _name = new() { Width = 260 };
    private readonly TextBox _serial = new() { Width = 260 };
    private readonly ComboBox _type = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _department = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _purchaseDate = new() { Width = 260, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
    private readonly NumericUpDown _price = new() { Width = 260, Maximum = 1_000_000_000_000m, ThousandsSeparator = true };
    private readonly ErrorProvider _errors = new();

    public DeviceEditForm(int? id = null)
    {
        _id = id;
        Text = id is null ? "Thêm thiết bị" : "Sửa thiết bị";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 525);
        Font = new Font("Segoe UI", 10);
        _errors.ContainerControl = this;

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(24), AutoScroll = true };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(table, "Mã thiết bị *", _code);
        AddRow(table, "Tên thiết bị *", _name);
        AddRow(table, "Loại thiết bị *", _type);
        AddRow(table, "Serial", _serial);
        AddRow(table, "Ngày mua", _purchaseDate);
        AddRow(table, "Giá mua", _price);
        AddRow(table, "Trạng thái", _status);
        AddRow(table, "Phòng ban", _department);

        var save = new Button { Text = "Lưu", Width = 100, Height = 36, DialogResult = DialogResult.None };
        var cancel = new Button { Text = "Hủy", Width = 100, Height = 36, DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { AutoSize = true };
        buttons.Controls.AddRange([save, cancel]);
        var buttonRow = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        table.Controls.Add(new Label(), 0, buttonRow);
        table.Controls.Add(buttons, 1, buttonRow);
        Controls.Add(table);
        CancelButton = cancel;
        save.Click += SaveAsync;
        Load += LoadAsync;
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 10, 3, 3) }, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private async void LoadAsync(object? sender, EventArgs e)
    {
        await using var db = new AppDbContext();
        _type.DataSource = await db.DeviceTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        _type.DisplayMember = "Name"; _type.ValueMember = "Id";

        var deps = await db.Departments.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        _department.DataSource = new[] { new Department { Id = 0, Name = "-- Không gán --" } }.Concat(deps).ToList();
        _department.DisplayMember = "Name"; _department.ValueMember = "Id";

        _status.DataSource = Enum.GetValues<DeviceStatus>().Select(x => new KeyValuePair<DeviceStatus, string>(x, x.ToDisplayName())).ToList();
        _status.DisplayMember = "Value"; _status.ValueMember = "Key";

        if (_id is null) return;
        var entity = await db.Devices.FindAsync(_id.Value);
        if (entity is null) return;
        _code.Text = entity.Code;
        _name.Text = entity.Name;
        _serial.Text = entity.SerialNumber;
        _purchaseDate.Checked = entity.PurchaseDate.HasValue;
        if (entity.PurchaseDate.HasValue) _purchaseDate.Value = entity.PurchaseDate.Value;
        _price.Value = entity.PurchasePrice ?? 0;
        _type.SelectedValue = entity.DeviceTypeId;
        _status.SelectedValue = entity.Status;
        _department.SelectedValue = entity.DepartmentId ?? 0;
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;
        if (string.IsNullOrWhiteSpace(_code.Text)) { _errors.SetError(_code, "Vui lòng nhập mã thiết bị."); valid = false; }
        if (string.IsNullOrWhiteSpace(_name.Text)) { _errors.SetError(_name, "Vui lòng nhập tên thiết bị."); valid = false; }
        if (_purchaseDate.Checked && _purchaseDate.Value.Date > DateTime.Today) { _errors.SetError(_purchaseDate, "Ngày mua không được lớn hơn ngày hiện tại."); valid = false; }
        var deviceTypeId = _type.SelectedValue is int selectedDeviceTypeId ? selectedDeviceTypeId : 0;
        if (deviceTypeId <= 0)
        {
            _errors.SetError(_type, "Vui lòng chọn loại thiết bị.");
            valid = false;
        }

        var deviceStatus = _status.SelectedValue is DeviceStatus selectedStatus ? selectedStatus : 0;
        if (!Enum.IsDefined(typeof(DeviceStatus), deviceStatus) || (int)deviceStatus == 0)
        {
            _errors.SetError(_status, "Vui lòng chọn trạng thái.");
            valid = false;
        }

        var departmentId = _department.SelectedValue is int selectedDepartmentId ? selectedDepartmentId : -1;
        if (departmentId < 0)
        {
            _errors.SetError(_department, "Phòng ban không hợp lệ.");
            valid = false;
        }
        if (!valid) return;

        await using var db = new AppDbContext();
        var code = _code.Text.Trim();
        var serial = string.IsNullOrWhiteSpace(_serial.Text) ? null : _serial.Text.Trim();
        if (await db.Devices.AnyAsync(x => x.Code == code && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_code, "Mã thiết bị đã tồn tại."); return;
        }
        if (serial is not null && await db.Devices.AnyAsync(x => x.SerialNumber == serial && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_serial, "Serial đã tồn tại."); return;
        }

        Device entity;
        if (_id is null)
        {
            entity = new Device();
            db.Devices.Add(entity);
        }
        else
        {
            entity = await db.Devices.FindAsync(_id.Value) ?? throw new InvalidOperationException("Không tìm thấy thiết bị.");
        }

        entity.Code = code;
        entity.Name = _name.Text.Trim();
        entity.SerialNumber = serial;
        entity.DeviceTypeId = deviceTypeId;
        entity.PurchaseDate = _purchaseDate.Checked ? _purchaseDate.Value.Date : null;
        entity.PurchasePrice = _price.Value > 0 ? _price.Value : null;
        entity.Status = deviceStatus;
        entity.DepartmentId = departmentId == 0 ? null : departmentId;
        await db.SaveChangesAsync();
        DialogResult = DialogResult.OK;
        Close();
    }
}
