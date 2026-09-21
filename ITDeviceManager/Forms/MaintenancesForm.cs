using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class MaintenancesForm : AppForm
{
    private readonly DataGridView _grid = new();
    private readonly TextInput _search = new() { Width = 230, PlaceholderText = "Mã phiếu, thiết bị, lỗi...", TextAlign = HorizontalAlignment.Center };
    private readonly ComboBox _typeFilter = new() { Width = 175, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _statusFilter = new() { Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };

    public MaintenancesForm()
    {
        Text = "Bảo trì / Sửa chữa";
        Ui.ConfigureGrid(_grid);

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(0, 6, 0, 4),
            WrapContents = false
        };
        top.Controls.Add(Ui.Label("Tìm kiếm:"));
        top.Controls.Add(_search);
        top.Controls.Add(Ui.Label("Loại:"));
        top.Controls.Add(_typeFilter);
        top.Controls.Add(Ui.Label("Trạng thái:"));
        top.Controls.Add(_statusFilter);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        var add = Ui.Button("Thêm phiếu", 120);
        var edit = Ui.Button("Sửa", 90);
        var start = Ui.Button("Bắt đầu xử lý", 135);
        var complete = Ui.Button("Hoàn thành", 120);
        var cancel = Ui.Button("Hủy phiếu", 105);
        var delete = Ui.Button("Xóa", 90);
        var refresh = Ui.Button("Làm mới", 100);
        var export = Ui.ExportButton(_grid, "Bảo trì - Sửa chữa - Bảo hành", 108);
        AppTheme.SetButtonRole(start, ButtonRole.Warning);
        AppTheme.SetButtonRole(cancel, ButtonRole.Secondary);
        AppTheme.SetButtonRole(delete, ButtonRole.Danger);
        buttons.Controls.AddRange([add, edit, start, complete, cancel, delete, refresh, export]);

        add.Enabled = edit.Enabled = start.Enabled = complete.Enabled = cancel.Enabled = delete.Enabled = AppSession.IsAdmin;

        add.Click += async (_, _) =>
        {
            using var form = new MaintenanceEditForm();
            if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        };
        edit.Click += async (_, _) => await EditSelectedAsync();
        start.Click += async (_, _) => await StartSelectedAsync();
        complete.Click += async (_, _) => await CompleteSelectedAsync();
        cancel.Click += async (_, _) => await CancelSelectedAsync();
        delete.Click += async (_, _) => await DeleteSelectedAsync();
        refresh.Click += async (_, _) => await LoadFiltersAsync();
        _grid.CellDoubleClick += async (_, _) => { if (AppSession.IsAdmin) await EditSelectedAsync(); };
        _search.TextChanged += async (_, _) => await LoadDataAsync();
        _typeFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
        _statusFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

        Controls.Add(_grid);
        Controls.Add(buttons);
        Controls.Add(top);
        Load += async (_, _) => await LoadFiltersAsync();
    }

    private async Task LoadFiltersAsync()
    {
        var types = new List<KeyValuePair<int, string>> { new(0, "Tất cả") };
        types.AddRange(Enum.GetValues<MaintenanceType>()
            .Select(x => new KeyValuePair<int, string>((int)x, x.ToDisplayName())));
        _typeFilter.DataSource = types;
        _typeFilter.DisplayMember = "Value";
        _typeFilter.ValueMember = "Key";

        var statuses = new List<KeyValuePair<int, string>> { new(0, "Tất cả") };
        statuses.AddRange(Enum.GetValues<MaintenanceStatus>()
            .Select(x => new KeyValuePair<int, string>((int)x, x.ToDisplayName())));
        _statusFilter.DataSource = statuses;
        _statusFilter.DisplayMember = "Value";
        _statusFilter.ValueMember = "Key";

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (!IsHandleCreated) return;
        await using var db = new AppDbContext();
        var query = db.DeviceMaintenances
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        var keyword = _search.Text.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Code.Contains(keyword) ||
                x.Device.Code.Contains(keyword) ||
                x.Device.Name.Contains(keyword) ||
                x.IssueDescription.Contains(keyword) ||
                (x.Provider != null && x.Provider.Contains(keyword)));
        }

        if (_typeFilter.SelectedValue is int typeId && typeId > 0)
            query = query.Where(x => x.Type == (MaintenanceType)typeId);
        if (_statusFilter.SelectedValue is int statusId && statusId > 0)
            query = query.Where(x => x.Status == (MaintenanceStatus)statusId);

        var raw = await query
            .OrderByDescending(x => x.ReceivedDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Code,
                Device = x.Device.Code + " - " + x.Device.Name + (x.Device.IsDeleted ? " [Đã xóa]" : ""),
                x.Type,
                x.ReceivedDate,
                x.CompletedDate,
                x.Provider,
                x.Cost,
                x.IssueDescription,
                x.Resolution,
                x.Status,
                DeviceStatus = x.Device.Status
            })
            .ToListAsync();

        _grid.DataSource = raw.Select(x => new
        {
            x.Id,
            Mã_phiếu = x.Code,
            Thiết_bị = x.Device,
            Loại_xử_lý = x.Type.ToDisplayName(),
            Ngày_tiếp_nhận = x.ReceivedDate.ToString("dd/MM/yyyy"),
            Ngày_hoàn_thành = x.CompletedDate?.ToString("dd/MM/yyyy") ?? string.Empty,
            Đơn_vị_xử_lý = x.Provider,
            Chi_phí = x.Cost?.ToString("N0"),
            Mô_tả = x.IssueDescription,
            Kết_quả = x.Resolution,
            Trạng_thái_phiếu = x.Status.ToDisplayName(),
            Trạng_thái_thiết_bị = x.DeviceStatus.ToDisplayName()
        }).ToList();

        if (_grid.Columns["Id"] is { } idColumn) idColumn.Visible = false;

        AppTheme.NormalizeGridHeaders(_grid);
        AppTheme.SetFixedColumn(_grid, "Mã_phiếu", 82, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFillColumn(_grid, "Thiết_bị", 115F, 210);
        AppTheme.SetFixedColumn(_grid, "Loại_xử_lý", 120, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Ngày_tiếp_nhận", 108, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Ngày_hoàn_thành", 108, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFillColumn(_grid, "Đơn_vị_xử_lý", 90F, 170);
        AppTheme.SetFixedColumn(_grid, "Chi_phí", 105, DataGridViewContentAlignment.MiddleRight);
        AppTheme.SetFillColumn(_grid, "Mô_tả", 165F, 260, wrap: true);
        AppTheme.SetFillColumn(_grid, "Kết_quả", 165F, 260, wrap: true);
        AppTheme.SetFixedColumn(_grid, "Trạng_thái_phiếu", 118, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Trạng_thái_thiết_bị", 128, DataGridViewContentAlignment.MiddleCenter);
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        AppTheme.NormalizeGridRows(_grid);
    }

    private int? SelectedId()
    {
        var value = _grid.CurrentRow?.Cells["Id"].Value;
        return value is null ? null : Convert.ToInt32(value);
    }

    private async Task EditSelectedAsync()
    {
        var id = SelectedId();
        if (id is null) return;
        await using var db = new AppDbContext();
        var status = await db.DeviceMaintenances.AsNoTracking().Where(x => x.Id == id.Value).Select(x => x.Status).SingleOrDefaultAsync();
        if (status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            MessageBox.Show("Phiếu đã hoàn tất hoặc đã hủy nên chỉ được xem trong lịch sử.", "Thông báo");
            return;
        }
        using var form = new MaintenanceEditForm(id.Value);
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async Task StartSelectedAsync()
    {
        var id = SelectedId();
        if (id is null) return;
        await using var db = new AppDbContext();
        var entity = await db.DeviceMaintenances.Include(x => x.Device).SingleOrDefaultAsync(x => x.Id == id.Value);
        if (entity is null) return;
        if (entity.Status != MaintenanceStatus.Pending)
        {
            MessageBox.Show("Chỉ phiếu Chờ xử lý mới có thể bắt đầu.", "Thông báo");
            return;
        }
        entity.Status = MaintenanceStatus.InProgress;
        entity.Device.Status = DeviceStatus.Repair;
        entity.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }

    private async Task CompleteSelectedAsync()
    {
        var id = SelectedId();
        if (id is null) return;
        await using (var db = new AppDbContext())
        {
            var status = await db.DeviceMaintenances.AsNoTracking().Where(x => x.Id == id.Value).Select(x => x.Status).SingleOrDefaultAsync();
            if (status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
            {
                MessageBox.Show("Phiếu không còn ở trạng thái có thể hoàn thành.", "Thông báo");
                return;
            }
        }

        using var form = new MaintenanceCompleteForm(id.Value);
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async Task CancelSelectedAsync()
    {
        var id = SelectedId();
        if (id is null) return;
        if (MessageBox.Show("Xác nhận hủy phiếu bảo trì đang chọn?", "Hủy phiếu", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        await using var db = new AppDbContext();
        var entity = await db.DeviceMaintenances.Include(x => x.Device).SingleOrDefaultAsync(x => x.Id == id.Value);
        if (entity is null) return;
        if (entity.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            MessageBox.Show("Phiếu đã kết thúc nên không thể hủy lại.", "Thông báo");
            return;
        }

        entity.Status = MaintenanceStatus.Cancelled;
        entity.CompletedDate = DateTime.Today;
        entity.ResultDeviceStatus = entity.PreviousDeviceStatus;
        entity.Device.Status = entity.PreviousDeviceStatus;
        entity.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var id = SelectedId();
        if (id is null || !Ui.ConfirmSoftDelete("phiếu bảo trì đã chọn")) return;

        await using var db = new AppDbContext();
        var entity = await db.DeviceMaintenances
            .IgnoreQueryFilters()
            .Include(x => x.Device)
            .SingleOrDefaultAsync(x => x.Id == id.Value && !x.IsDeleted);
        if (entity is null) return;
        if (entity.Status == MaintenanceStatus.InProgress)
        {
            MessageBox.Show("Phiếu đang xử lý. Hãy hoàn thành hoặc hủy phiếu trước khi xóa.", "Không thể xóa");
            return;
        }

        if (entity.Status == MaintenanceStatus.Pending)
            entity.Device.Status = entity.PreviousDeviceStatus;

        SoftDeleteService.MarkDeleted(entity);
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }
}
