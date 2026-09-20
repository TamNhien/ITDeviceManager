using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class MaintenanceEditForm : AppForm
{
    private readonly int? _id;
    private readonly TextBox _code = new() { Width = 320, ReadOnly = true };
    private readonly ComboBox _device = new() { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _type = new() { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _receivedDate = new()
    {
        Width = 180,
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        Value = DateTime.Today
    };
    private readonly TextBox _provider = new() { Width = 320 };
    private readonly NumericUpDown _cost = new()
    {
        Width = 200,
        DecimalPlaces = 0,
        Maximum = 1_000_000_000,
        ThousandsSeparator = true
    };
    private readonly TextBox _issue = new() { Width = 430, Height = 78, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _note = new() { Width = 430, Height = 70, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly ErrorProvider _errors = new();

    public MaintenanceEditForm(int? id = null)
    {
        _id = id;
        Text = id is null ? "Tạo phiếu bảo trì / sửa chữa" : "Cập nhật phiếu bảo trì / sửa chữa";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(720, 590);

        _errors.ContainerControl = this;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(26),
            BackColor = AppTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "Mã phiếu", _code, 46);
        AddRow(layout, "Thiết bị *", _device, 52);
        AddRow(layout, "Loại xử lý *", _type, 52);
        AddRow(layout, "Ngày tiếp nhận *", _receivedDate, 52);
        AddRow(layout, "Đơn vị xử lý", _provider, 52);
        AddRow(layout, "Chi phí", _cost, 52);
        AddRow(layout, "Mô tả lỗi / yêu cầu *", _issue, 92);
        AddRow(layout, "Ghi chú", _note, 84);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = Padding.Empty
        };
        var save = Ui.Button("Lưu", 120);
        var cancel = Ui.Button("Hủy", 110);
        cancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange([save, cancel]);

        var buttonRow = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.Controls.Add(new Label(), 0, buttonRow);
        layout.Controls.Add(buttons, 1, buttonRow);

        Controls.Add(layout);
        CancelButton = cancel;
        Load += LoadAsync;
        save.Click += SaveAsync;
    }

    private static void AddRow(TableLayoutPanel layout, string label, Control control, int height)
    {
        var row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(3, 9, 3, 3)
        }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private async void LoadAsync(object? sender, EventArgs e)
    {
        _type.DataSource = Enum.GetValues<MaintenanceType>()
            .Select(x => new KeyValuePair<MaintenanceType, string>(x, x.ToDisplayName()))
            .ToList();
        _type.DisplayMember = "Value";
        _type.ValueMember = "Key";

        await using var db = new AppDbContext();

        DeviceMaintenance? maintenance = null;
        if (_id is not null)
        {
            maintenance = await db.DeviceMaintenances.AsNoTracking().SingleOrDefaultAsync(x => x.Id == _id.Value);
            if (maintenance is null)
            {
                MessageBox.Show("Không tìm thấy phiếu bảo trì.", "Thông báo");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
        }

        var selectedDeviceId = maintenance?.DeviceId;
        var devices = await db.Devices
            .AsNoTracking()
            .Where(x => x.Status != DeviceStatus.Retired || x.Id == selectedDeviceId)
            .OrderBy(x => x.Code)
            .Select(x => new { x.Id, Display = x.Code + " - " + x.Name + " (" + x.Status.ToDisplayName() + ")" })
            .ToListAsync();
        _device.DataSource = devices;
        _device.DisplayMember = "Display";
        _device.ValueMember = "Id";

        if (maintenance is null)
        {
            _code.Text = await NextCodeAsync(db);
            return;
        }

        _code.Text = maintenance.Code;
        _device.SelectedValue = maintenance.DeviceId;
        _device.Enabled = false;
        _type.SelectedValue = maintenance.Type;
        _receivedDate.Value = maintenance.ReceivedDate.Date;
        _provider.Text = maintenance.Provider ?? string.Empty;
        _cost.Value = Math.Clamp(maintenance.Cost ?? 0m, _cost.Minimum, _cost.Maximum);
        _issue.Text = maintenance.IssueDescription;
        _note.Text = maintenance.Note ?? string.Empty;
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var valid = true;

        var deviceId = _device.SelectedValue is int selectedDeviceId ? selectedDeviceId : 0;
        if (deviceId <= 0)
        {
            _errors.SetError(_device, "Vui lòng chọn thiết bị.");
            valid = false;
        }

        var maintenanceType = _type.SelectedValue is MaintenanceType selectedType ? selectedType : 0;
        if (!Enum.IsDefined(typeof(MaintenanceType), maintenanceType) || (int)maintenanceType == 0)
        {
            _errors.SetError(_type, "Vui lòng chọn loại xử lý.");
            valid = false;
        }
        if (_receivedDate.Value.Date > DateTime.Today)
        {
            _errors.SetError(_receivedDate, "Ngày tiếp nhận không được lớn hơn ngày hiện tại.");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(_issue.Text))
        {
            _errors.SetError(_issue, "Vui lòng nhập mô tả lỗi hoặc yêu cầu xử lý.");
            valid = false;
        }
        if (!valid) return;

        await using var db = new AppDbContext();

        if (_id is null)
        {
            var device = await db.Devices.SingleOrDefaultAsync(x => x.Id == deviceId);
            if (device is null) return;
            if (device.Status == DeviceStatus.Retired)
            {
                MessageBox.Show("Thiết bị đã thanh lý nên không thể tạo phiếu bảo trì mới.", "Không thể tạo phiếu");
                return;
            }

            var hasActiveMaintenance = await db.DeviceMaintenances.AnyAsync(x =>
                x.DeviceId == deviceId &&
                (x.Status == MaintenanceStatus.Pending || x.Status == MaintenanceStatus.InProgress));
            if (hasActiveMaintenance)
            {
                MessageBox.Show("Thiết bị đang có một phiếu bảo trì chưa hoàn tất.", "Không thể tạo phiếu");
                return;
            }

            var code = await NextCodeAsync(db);
            db.DeviceMaintenances.Add(new DeviceMaintenance
            {
                Code = code,
                DeviceId = deviceId,
                Type = maintenanceType,
                ReceivedDate = _receivedDate.Value.Date,
                Provider = Clean(_provider.Text),
                Cost = _cost.Value > 0 ? _cost.Value : null,
                IssueDescription = _issue.Text.Trim(),
                Status = MaintenanceStatus.Pending,
                PreviousDeviceStatus = device.Status,
                Note = Clean(_note.Text),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            device.Status = DeviceStatus.Repair;
        }
        else
        {
            var entity = await db.DeviceMaintenances.SingleOrDefaultAsync(x => x.Id == _id.Value);
            if (entity is null) return;
            if (entity.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
            {
                MessageBox.Show("Phiếu đã hoàn tất hoặc đã hủy. Không thể sửa nội dung nghiệp vụ.", "Không thể sửa");
                return;
            }

            entity.Type = maintenanceType;
            entity.ReceivedDate = _receivedDate.Value.Date;
            entity.Provider = Clean(_provider.Text);
            entity.Cost = _cost.Value > 0 ? _cost.Value : null;
            entity.IssueDescription = _issue.Text.Trim();
            entity.Note = Clean(_note.Text);
            entity.UpdatedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static string? Clean(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async Task<string> NextCodeAsync(AppDbContext db)
    {
        var existing = await db.DeviceMaintenances.AsNoTracking().Select(x => x.Code).ToListAsync();
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i <= 9999; i++)
        {
            var code = $"BT{i:D3}";
            if (!set.Contains(code)) return code;
        }
        throw new InvalidOperationException("Không thể sinh mã phiếu bảo trì mới.");
    }
}
