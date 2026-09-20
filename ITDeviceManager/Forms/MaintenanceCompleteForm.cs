using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class MaintenanceCompleteForm : AppForm
{
    private readonly int _maintenanceId;
    private readonly DateTimePicker _completedDate = new()
    {
        Width = 180,
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        Value = DateTime.Today
    };
    private readonly ComboBox _resultStatus = new() { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _cost = new()
    {
        Width = 200,
        DecimalPlaces = 0,
        Maximum = 1_000_000_000,
        ThousandsSeparator = true
    };
    private readonly TextBox _resolution = new() { Width = 410, Height = 90, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly ErrorProvider _errors = new();
    private DateTime _receivedDate;

    public MaintenanceCompleteForm(int maintenanceId)
    {
        _maintenanceId = maintenanceId;
        Text = "Hoàn thành xử lý thiết bị";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(650, 360);
        _errors.ContainerControl = this;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(26),
            BackColor = AppTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(layout, "Ngày hoàn thành *", _completedDate, 52);
        AddRow(layout, "Trạng thái thiết bị *", _resultStatus, 52);
        AddRow(layout, "Tổng chi phí", _cost, 52);
        AddRow(layout, "Nội dung xử lý *", _resolution, 105);

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        var complete = Ui.Button("Hoàn thành", 130);
        var cancel = Ui.Button("Hủy", 100);
        cancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange([complete, cancel]);
        var row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.Controls.Add(new Label(), 0, row);
        layout.Controls.Add(buttons, 1, row);

        Controls.Add(layout);
        CancelButton = cancel;
        Load += LoadAsync;
        complete.Click += CompleteAsync;
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
            Margin = new Padding(3, 9, 3, 3)
        }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private async void LoadAsync(object? sender, EventArgs e)
    {
        await using var db = new AppDbContext();
        var maintenance = await db.DeviceMaintenances.AsNoTracking().SingleOrDefaultAsync(x => x.Id == _maintenanceId);
        if (maintenance is null)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        _receivedDate = maintenance.ReceivedDate.Date;
        _cost.Value = Math.Clamp(maintenance.Cost ?? 0m, _cost.Minimum, _cost.Maximum);

        var hasActiveAssignment = await db.DeviceAssignments.AsNoTracking()
            .AnyAsync(x => x.DeviceId == maintenance.DeviceId && x.ReturnedDate == null);
        var defaultStatus = hasActiveAssignment ? DeviceStatus.InUse : DeviceStatus.Available;

        var statuses = new[]
        {
            DeviceStatus.Available,
            DeviceStatus.InUse,
            DeviceStatus.Broken,
            DeviceStatus.Retired
        }.Select(x => new KeyValuePair<DeviceStatus, string>(x, x.ToDisplayName())).ToList();
        _resultStatus.DataSource = statuses;
        _resultStatus.DisplayMember = "Value";
        _resultStatus.ValueMember = "Key";
        _resultStatus.SelectedValue = defaultStatus;
    }

    private async void CompleteAsync(object? sender, EventArgs e)
    {
        _errors.Clear();
        var date = _completedDate.Value.Date;
        if (date < _receivedDate || date > DateTime.Today)
        {
            _errors.SetError(_completedDate, "Ngày hoàn thành phải từ ngày tiếp nhận đến ngày hiện tại.");
            return;
        }
        if (_resultStatus.SelectedValue is not DeviceStatus resultStatus || resultStatus == DeviceStatus.Repair)
        {
            _errors.SetError(_resultStatus, "Vui lòng chọn trạng thái thiết bị sau xử lý.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_resolution.Text))
        {
            _errors.SetError(_resolution, "Vui lòng nhập nội dung xử lý.");
            return;
        }

        await using var db = new AppDbContext();
        var maintenance = await db.DeviceMaintenances.Include(x => x.Device)
            .SingleOrDefaultAsync(x => x.Id == _maintenanceId);
        if (maintenance is null) return;
        if (maintenance.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            MessageBox.Show("Phiếu không còn ở trạng thái có thể hoàn thành.", "Thông báo");
            return;
        }

        maintenance.CompletedDate = date;
        maintenance.Resolution = _resolution.Text.Trim();
        maintenance.Cost = _cost.Value > 0 ? _cost.Value : null;
        maintenance.Status = MaintenanceStatus.Completed;
        maintenance.ResultDeviceStatus = resultStatus;
        maintenance.UpdatedAt = DateTime.Now;
        maintenance.Device.Status = resultStatus;

        await db.SaveChangesAsync();
        DialogResult = DialogResult.OK;
        Close();
    }
}
