using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DevicesForm : AppForm
{
    private readonly DataGridView _grid = new();
    private readonly TextBox _search = new() { Width = 240, PlaceholderText = "Mã, tên, serial..." };
    private readonly ComboBox _typeFilter = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _statusFilter = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

    public DevicesForm()
    {
        Text = "Quản lý thiết bị";
        Font = new Font("Segoe UI", 10);
        Ui.ConfigureGrid(_grid);

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 6, 0, 4) };
        top.Controls.Add(Ui.Label("Tìm kiếm:"));
        top.Controls.Add(_search);
        top.Controls.Add(Ui.Label("Loại:"));
        top.Controls.Add(_typeFilter);
        top.Controls.Add(Ui.Label("Trạng thái:"));
        top.Controls.Add(_statusFilter);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, FlowDirection = FlowDirection.LeftToRight };
        var add = Ui.Button("Thêm");
        var edit = Ui.Button("Sửa");
        var delete = Ui.Button("Xóa");
        var refresh = Ui.Button("Làm mới");
        var export = Ui.ExportButton(_grid, "Danh sách thiết bị");
        buttons.Controls.AddRange([add, edit, delete, refresh, export]);

        add.Enabled = edit.Enabled = delete.Enabled = AppSession.IsAdmin;
        add.Click += async (_, _) => { using var f = new DeviceEditForm(); if (f.ShowDialog() == DialogResult.OK) await LoadDataAsync(); };
        edit.Click += async (_, _) => await EditSelectedAsync();
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
        await using var db = new AppDbContext();
        var types = await db.DeviceTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        _typeFilter.DataSource = new[] { new DeviceType { Id = 0, Name = "Tất cả" } }.Concat(types).ToList();
        _typeFilter.DisplayMember = "Name";
        _typeFilter.ValueMember = "Id";

        var statuses = new List<KeyValuePair<int, string>> { new(0, "Tất cả") };
        statuses.AddRange(Enum.GetValues<DeviceStatus>().Select(x => new KeyValuePair<int, string>((int)x, x.ToDisplayName())));
        _statusFilter.DataSource = statuses;
        _statusFilter.DisplayMember = "Value";
        _statusFilter.ValueMember = "Key";
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (!IsHandleCreated) return;
        await using var db = new AppDbContext();
        var q = db.Devices.AsNoTracking().AsQueryable();
        var keyword = _search.Text.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(x => x.Code.Contains(keyword) || x.Name.Contains(keyword) || (x.SerialNumber != null && x.SerialNumber.Contains(keyword)));

        if (_typeFilter.SelectedValue is int typeId && typeId > 0)
            q = q.Where(x => x.DeviceTypeId == typeId);
        if (_statusFilter.SelectedValue is int statusId && statusId > 0)
            q = q.Where(x => x.Status == (DeviceStatus)statusId);

        var raw = await q.OrderBy(x => x.Code).Select(x => new
        {
            x.Id,
            x.Code,
            x.Name,
            TypeName = x.DeviceType.Name,
            x.SerialNumber,
            x.PurchaseDate,
            x.PurchasePrice,
            x.Status,
            DepartmentName = x.Department != null ? x.Department.Name : null
        }).ToListAsync();

        _grid.DataSource = raw.Select(x => new
        {
            x.Id,
            Mã = x.Code,
            Tên_thiết_bị = x.Name,
            Loại = x.TypeName,
            Serial = x.SerialNumber,
            Ngày_mua = x.PurchaseDate?.ToString("dd/MM/yyyy"),
            Giá_mua = x.PurchasePrice?.ToString("N0"),
            Trạng_thái = x.Status.ToDisplayName(),
            Phòng_ban = x.DepartmentName
        }).ToList();
        var idColumn = _grid.Columns["Id"];
        if (idColumn is not null) idColumn.Visible = false;
        ApplyGridLayout();
    }

    private void ApplyGridLayout()
    {
        // Keep the three descriptive columns compact so the remaining values
        // receive a balanced share of the available width on wide screens.
        AppTheme.SetFixedColumn(_grid, "Mã", 78, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFillColumn(_grid, "Tên_thiết_bị", 100F, 190);
        AppTheme.SetFillColumn(_grid, "Loại", 65F, 120);
        AppTheme.SetFillColumn(_grid, "Serial", 105F, 155);
        AppTheme.SetFillColumn(_grid, "Ngày_mua", 80F, 110);
        AppTheme.SetFillColumn(_grid, "Giá_mua", 80F, 115);
        AppTheme.SetFillColumn(_grid, "Trạng_thái", 90F, 125);
        AppTheme.SetFillColumn(_grid, "Phòng_ban", 70F, 135);

        if (_grid.Columns["Ngày_mua"] is { } purchaseDate)
            purchaseDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        if (_grid.Columns["Giá_mua"] is { } purchasePrice)
            purchasePrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (_grid.Columns["Trạng_thái"] is { } status)
            status.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private int? SelectedId() => _grid.CurrentRow?.Cells["Id"].Value as int?;

    private async Task EditSelectedAsync()
    {
        var id = SelectedId();
        if (id is null) return;
        using var f = new DeviceEditForm(id.Value);
        if (f.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var id = SelectedId();
        if (id is null || !Ui.ConfirmDelete("thiết bị đã chọn")) return;

        await using var db = new AppDbContext();
        var entity = await db.Devices.FindAsync(id.Value);
        if (entity is null) return;
        if (await db.DeviceAssignments.AnyAsync(x => x.DeviceId == id.Value))
        {
            MessageBox.Show("Thiết bị đã có lịch sử cấp phát nên không thể xóa. Bạn có thể chuyển trạng thái sang Thanh lý.", "Không thể xóa");
            return;
        }
        db.Devices.Remove(entity);
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }
}
