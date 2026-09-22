using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DeviceEditForm : AppForm
{
    private readonly int? _id;
    private readonly TextInput _code = new() { Width = 260, TextAlign = HorizontalAlignment.Left };
    private readonly TextInput _name = new() { Width = 260, TextAlign = HorizontalAlignment.Left };
    private readonly TextInput _serial = new() { Width = 260, TextAlign = HorizontalAlignment.Left };
    private readonly DarkComboBox _type = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DarkComboBox _status = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DarkComboBox _department = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NullableDateInput _purchaseDate = new() { Width = 260 };
    private readonly TextInput _price = new() { Width = 260, TextAlign = HorizontalAlignment.Left, PlaceholderText = "Ví dụ: 15000000" };
    private readonly NullableDateInput _warrantyEndDate = new() { Width = 260 };
    private readonly TextInput _maintenanceInterval = new() { Width = 260, TextAlign = HorizontalAlignment.Left, PlaceholderText = "1 - 120 tháng" };
    private readonly NullableDateInput _nextMaintenanceDate = new() { Width = 260 };
    private readonly ErrorProvider _errors = new();

    public DeviceEditForm(int? id = null)
    {
        _id = id;
        Text = id is null ? "Thêm thiết bị" : "Sửa thiết bị";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(540, 680);
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
        AddRow(table, "Hạn bảo hành", _warrantyEndDate);
        AddRow(table, "Chu kỳ BT (tháng)", _maintenanceInterval);
        AddRow(table, "Bảo trì kế tiếp", _nextMaintenanceDate);
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
        _purchaseDate.Value = entity.PurchaseDate;
        _price.Text = entity.PurchasePrice.HasValue ? decimal.Truncate(entity.PurchasePrice.Value).ToString("0", System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        _warrantyEndDate.Value = entity.WarrantyEndDate;
        _maintenanceInterval.Text = entity.MaintenanceIntervalMonths?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        _nextMaintenanceDate.Value = entity.NextMaintenanceDate;
        _type.SelectedValue = entity.DeviceTypeId;
        _status.SelectedValue = entity.Status;
        _department.SelectedValue = entity.DepartmentId ?? 0;
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;
        var normalizedCode = _code.Text.Trim().ToUpperInvariant();
        _code.Text = normalizedCode;
        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            _errors.SetError(_code, "Vui lòng nhập mã thiết bị.");
            valid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedCode, @"^TB\d+$"))
        {
            _errors.SetError(_code, "Mã thiết bị phải có dạng TB + chữ số, ví dụ TB001.");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(_name.Text)) { _errors.SetError(_name, "Vui lòng nhập tên thiết bị."); valid = false; }

        DateTime? purchaseDate = null;
        DateTime? warrantyEndDate = null;
        DateTime? nextMaintenanceDate = null;

        if (!_purchaseDate.TryGetValue(out purchaseDate))
        {
            _errors.SetError(_purchaseDate, "Ngày mua phải có dạng dd/MM/yyyy.");
            valid = false;
        }
        if (!_warrantyEndDate.TryGetValue(out warrantyEndDate))
        {
            _errors.SetError(_warrantyEndDate, "Hạn bảo hành phải có dạng dd/MM/yyyy.");
            valid = false;
        }
        if (!_nextMaintenanceDate.TryGetValue(out nextMaintenanceDate))
        {
            _errors.SetError(_nextMaintenanceDate, "Ngày bảo trì kế tiếp phải có dạng dd/MM/yyyy.");
            valid = false;
        }

        if (purchaseDate.HasValue && purchaseDate.Value.Date > DateTime.Today)
        {
            _errors.SetError(_purchaseDate, "Ngày mua không được lớn hơn ngày hiện tại.");
            valid = false;
        }
        if (warrantyEndDate.HasValue && purchaseDate.HasValue && warrantyEndDate.Value.Date < purchaseDate.Value.Date)
        {
            _errors.SetError(_warrantyEndDate, "Hạn bảo hành không được trước ngày mua.");
            valid = false;
        }
        if (nextMaintenanceDate.HasValue && purchaseDate.HasValue && nextMaintenanceDate.Value.Date < purchaseDate.Value.Date)
        {
            _errors.SetError(_nextMaintenanceDate, "Ngày bảo trì kế tiếp không được trước ngày mua.");
            valid = false;
        }

        decimal? purchasePrice = null;
        var priceText = _price.Text.Trim().Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty);
        if (!string.IsNullOrWhiteSpace(priceText))
        {
            if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice) ||
                parsedPrice <= 0 || parsedPrice > 1_000_000_000_000m)
            {
                _errors.SetError(_price, "Giá mua phải là số dương, tối đa 1.000.000.000.000.");
                valid = false;
            }
            else
            {
                purchasePrice = parsedPrice;
            }
        }

        int? maintenanceIntervalMonths = null;
        var intervalText = _maintenanceInterval.Text.Trim();
        if (!string.IsNullOrWhiteSpace(intervalText))
        {
            if (!int.TryParse(intervalText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedInterval) ||
                parsedInterval < 1 || parsedInterval > 120)
            {
                _errors.SetError(_maintenanceInterval, "Chu kỳ bảo trì phải từ 1 đến 120 tháng.");
                valid = false;
            }
            else
            {
                maintenanceIntervalMonths = parsedInterval;
            }
        }
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
        var code = normalizedCode;
        var serial = string.IsNullOrWhiteSpace(_serial.Text) ? null : _serial.Text.Trim();
        if (await db.Devices.IgnoreQueryFilters().AnyAsync(x => x.Code == code && x.Id != (_id ?? 0)))
        {
            _errors.SetError(_code, "Mã thiết bị đã tồn tại."); return;
        }
        if (serial is not null && await db.Devices.IgnoreQueryFilters().AnyAsync(x => x.SerialNumber == serial && x.Id != (_id ?? 0)))
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

            var hasActiveMaintenance = await db.DeviceMaintenances.AnyAsync(x =>
                x.DeviceId == entity.Id &&
                (x.Status == MaintenanceStatus.Pending || x.Status == MaintenanceStatus.InProgress));
            if (hasActiveMaintenance && deviceStatus != DeviceStatus.Repair)
            {
                _errors.SetError(_status, "Thiết bị đang có phiếu bảo trì chưa hoàn tất nên phải giữ trạng thái Đang sửa chữa.");
                return;
            }
        }

        entity.Code = code;
        entity.Name = _name.Text.Trim();
        entity.SerialNumber = serial;
        entity.DeviceTypeId = deviceTypeId;
        entity.PurchaseDate = purchaseDate?.Date;
        entity.PurchasePrice = purchasePrice;
        entity.WarrantyEndDate = warrantyEndDate?.Date;
        entity.MaintenanceIntervalMonths = maintenanceIntervalMonths;
        if (nextMaintenanceDate.HasValue)
        {
            entity.NextMaintenanceDate = nextMaintenanceDate.Value.Date;
        }
        else if (entity.MaintenanceIntervalMonths is int intervalMonths && entity.PurchaseDate is DateTime savedPurchaseDate)
        {
            entity.NextMaintenanceDate = savedPurchaseDate.AddMonths(intervalMonths);
        }
        else
        {
            entity.NextMaintenanceDate = null;
        }
        entity.Status = deviceStatus;
        entity.DepartmentId = departmentId == 0 ? null : departmentId;
        await db.SaveChangesAsync();
        DialogResult = DialogResult.OK;
        Close();
    }
}
