using ITDeviceManager.Common;
using ITDeviceManager.Models;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class AlertsForm : AppForm
{
    private readonly DataGridView _grid = new();
    private readonly TextInput _search = new()
    {
        Width = 245,
        PlaceholderText = "Mã, tên, serial...",
        TextAlign = HorizontalAlignment.Center
    };
    private readonly DarkComboBox _typeFilter = new() { Width = 145, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DarkComboBox _stateFilter = new() { Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DarkComboBox _windowFilter = new() { Width = 125, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _totalLabel = SummaryLabel();
    private readonly Label _warrantyLabel = SummaryLabel();
    private readonly Label _maintenanceLabel = SummaryLabel();
    private IReadOnlyList<DeviceAlertItem> _alerts = Array.Empty<DeviceAlertItem>();

    public AlertsForm()
    {
        Text = "Cảnh báo hạn";
        Font = new Font("Segoe UI", 10F);
        BackColor = AppTheme.Background;
        Ui.ConfigureGrid(_grid);

        var summary = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(0, 5, 0, 3),
            WrapContents = false,
            BackColor = AppTheme.Background
        };
        summary.Controls.AddRange([_totalLabel, _warrantyLabel, _maintenanceLabel]);

        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(0, 6, 0, 4),
            WrapContents = false
        };
        filters.Controls.Add(Ui.Label("Tìm kiếm:"));
        filters.Controls.Add(_search);
        filters.Controls.Add(Ui.Label("Loại:"));
        filters.Controls.Add(_typeFilter);
        filters.Controls.Add(Ui.Label("Trạng thái:"));
        filters.Controls.Add(_stateFilter);
        filters.Controls.Add(Ui.Label("Phạm vi:"));
        filters.Controls.Add(_windowFilter);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        var editDevice = Ui.Button("Sửa thiết bị", 120);
        var createTicket = Ui.Button("Tạo phiếu xử lý", 145);
        var refresh = Ui.Button("Làm mới", 100);
        var export = Ui.ExportButton(_grid, "Cảnh báo bảo hành - bảo trì", 108);
        AppTheme.SetButtonRole(createTicket, ButtonRole.Warning);
        AppTheme.SetButtonRole(editDevice, ButtonRole.Secondary);
        buttons.Controls.AddRange([editDevice, createTicket, refresh, export]);

        editDevice.Click += async (_, _) => await EditSelectedDeviceAsync();
        createTicket.Click += async (_, _) => await CreateTicketForSelectedAsync();
        refresh.Click += async (_, _) => await ReloadAsync();
        _grid.CellDoubleClick += async (_, _) =>
        {
            if (PermissionService.Has(PermissionCodes.DeviceUpdate))
                await EditSelectedDeviceAsync();
        };

        _search.TextChanged += (_, _) => ApplyFilters();
        _typeFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        _stateFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        _windowFilter.SelectedIndexChanged += async (_, _) => await ReloadAsync();

        Controls.Add(_grid);
        Controls.Add(buttons);
        Controls.Add(filters);
        Controls.Add(summary);

        Load += async (_, _) =>
        {
            LoadFilterSources();
            await ReloadAsync();
        };
    }

    private static Label SummaryLabel() => new()
    {
        AutoSize = false,
        Width = 210,
        Height = 34,
        Margin = new Padding(0, 0, 10, 0),
        TextAlign = ContentAlignment.MiddleCenter,
        BackColor = AppTheme.SurfaceAlt,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI Semibold", 9.5F)
    };

    private void LoadFilterSources()
    {
        _typeFilter.DataSource = new[]
        {
            new KeyValuePair<int, string>(0, "Tất cả"),
            new KeyValuePair<int, string>((int)DeviceAlertType.Warranty, "Bảo hành"),
            new KeyValuePair<int, string>((int)DeviceAlertType.Maintenance, "Bảo trì")
        };
        _typeFilter.DisplayMember = "Value";
        _typeFilter.ValueMember = "Key";

        _stateFilter.DataSource = new[]
        {
            new KeyValuePair<int, string>(0, "Tất cả"),
            new KeyValuePair<int, string>(1, "Quá hạn"),
            new KeyValuePair<int, string>(2, "Hôm nay"),
            new KeyValuePair<int, string>(3, "1 - 7 ngày"),
            new KeyValuePair<int, string>(4, "Trên 7 ngày")
        };
        _stateFilter.DisplayMember = "Value";
        _stateFilter.ValueMember = "Key";

        _windowFilter.DataSource = new[]
        {
            new KeyValuePair<int, string>(7, "7 ngày"),
            new KeyValuePair<int, string>(30, "30 ngày"),
            new KeyValuePair<int, string>(60, "60 ngày"),
            new KeyValuePair<int, string>(90, "90 ngày")
        };
        _windowFilter.DisplayMember = "Value";
        _windowFilter.ValueMember = "Key";
        _windowFilter.SelectedValue = DeviceAlertService.DefaultLeadDays;
    }

    private async Task ReloadAsync()
    {
        if (!IsHandleCreated)
            return;

        var leadDays = _windowFilter.SelectedValue is int days ? days : DeviceAlertService.DefaultLeadDays;
        try
        {
            _alerts = await DeviceAlertService.GetAlertsAsync(leadDays);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể tải cảnh báo bảo hành / bảo trì.\n\n" + ex.Message,
                "Cảnh báo hạn",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<DeviceAlertItem> query = _alerts;
        var keyword = _search.Text.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.DeviceCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.DeviceName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (x.SerialNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (_typeFilter.SelectedValue is int typeValue && typeValue > 0)
            query = query.Where(x => (int)x.Type == typeValue);

        if (_stateFilter.SelectedValue is int stateValue && stateValue > 0)
        {
            query = stateValue switch
            {
                1 => query.Where(x => x.DaysRemaining < 0),
                2 => query.Where(x => x.DaysRemaining == 0),
                3 => query.Where(x => x.DaysRemaining is >= 1 and <= 7),
                4 => query.Where(x => x.DaysRemaining > 7),
                _ => query
            };
        }

        var rows = query
            .OrderBy(x => x.DaysRemaining)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.DeviceCode)
            .ToList();

        _grid.DataSource = rows.Select(x => new
        {
            DeviceId = x.DeviceId,
            AlertTypeValue = (int)x.Type,
            Mức_độ = x.SeverityName,
            Loại_cảnh_báo = x.TypeName,
            Thiết_bị = x.DeviceCode + " - " + x.DeviceName,
            Serial = x.SerialNumber ?? string.Empty,
            Hạn = x.DueDate.ToString("dd/MM/yyyy"),
            Còn_lại = x.RemainingText,
            Chu_kỳ_bảo_trì = x.MaintenanceIntervalMonths is int months ? $"{months} tháng" : string.Empty
        }).ToList();

        if (_grid.Columns["DeviceId"] is { } deviceIdColumn)
            deviceIdColumn.Visible = false;
        if (_grid.Columns["AlertTypeValue"] is { } alertTypeColumn)
            alertTypeColumn.Visible = false;

        AppTheme.NormalizeGridHeaders(_grid);
        AppTheme.SetFixedColumn(_grid, "Mức_độ", 110, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Loại_cảnh_báo", 120, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFillColumn(_grid, "Thiết_bị", 150F, 250);
        AppTheme.SetFillColumn(_grid, "Serial", 90F, 150);
        AppTheme.SetFixedColumn(_grid, "Hạn", 105, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Còn_lại", 115, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Chu_kỳ_bảo_trì", 125, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.NormalizeGridRows(_grid);

        foreach (DataGridViewRow row in _grid.Rows)
        {
            var text = row.Cells["Mức_độ"].Value?.ToString();
            row.Cells["Mức_độ"].Style.ForeColor = text switch
            {
                "Quá hạn" => AppTheme.Danger,
                "Hôm nay" => AppTheme.Warning,
                "Sắp đến hạn" => AppTheme.Warning,
                _ => AppTheme.Info
            };
        }

        _totalLabel.Text = $"Tổng cảnh báo: {rows.Count:N0}";
        _warrantyLabel.Text = $"Bảo hành: {rows.Count(x => x.Type == DeviceAlertType.Warranty):N0}";
        _maintenanceLabel.Text = $"Bảo trì: {rows.Count(x => x.Type == DeviceAlertType.Maintenance):N0}";
    }

    private int? SelectedDeviceId()
    {
        var value = _grid.CurrentRow?.Cells["DeviceId"].Value;
        return value is int id ? id : null;
    }

    private DeviceAlertType? SelectedAlertType()
    {
        var value = _grid.CurrentRow?.Cells["AlertTypeValue"].Value;
        return value is int raw && Enum.IsDefined(typeof(DeviceAlertType), raw)
            ? (DeviceAlertType)raw
            : null;
    }

    private async Task EditSelectedDeviceAsync()
    {
        PermissionService.Demand(PermissionCodes.DeviceUpdate, "sửa thông tin thiết bị từ cảnh báo");
        var id = SelectedDeviceId();
        if (id is null)
        {
            MessageBox.Show("Hãy chọn một cảnh báo trước.", "Cảnh báo hạn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new DeviceEditForm(id.Value);
        if (form.ShowDialog(this) == DialogResult.OK)
            await ReloadAsync();
    }

    private async Task CreateTicketForSelectedAsync()
    {
        PermissionService.Demand(PermissionCodes.MaintenanceCreate, "tạo phiếu xử lý từ cảnh báo");
        var id = SelectedDeviceId();
        var alertType = SelectedAlertType();
        if (id is null || alertType is null)
        {
            MessageBox.Show("Hãy chọn một cảnh báo trước.", "Cảnh báo hạn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var initialType = alertType == DeviceAlertType.Warranty
            ? MaintenanceType.Warranty
            : MaintenanceType.Preventive;

        using var form = new MaintenanceEditForm(null, id.Value, initialType);
        if (form.ShowDialog(this) == DialogResult.OK)
            await ReloadAsync();
    }
}
