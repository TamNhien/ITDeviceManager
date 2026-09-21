using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class RecycleBinForm : AppForm
{
    private readonly DataGridView _grid = new();
    private readonly TextInput _search = new()
    {
        Width = 260,
        PlaceholderText = "Mã, tên, người xóa...",
        TextAlign = HorizontalAlignment.Center
    };
    private readonly ComboBox _typeFilter = new()
    {
        Width = 200,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private static readonly KeyValuePair<string, string>[] Categories =
    [
        new("", "Tất cả"),
        new("Device", "Thiết bị"),
        new("Employee", "Nhân viên"),
        new("DeviceType", "Loại thiết bị"),
        new("Department", "Phòng ban"),
        new("User", "Tài khoản"),
        new("Maintenance", "Bảo trì / Sửa chữa")
    ];

    public RecycleBinForm()
    {
        Text = "Thùng rác";
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
        top.Controls.Add(Ui.Label("Loại dữ liệu:"));
        top.Controls.Add(_typeFilter);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        var restore = Ui.Button("Khôi phục", 120);
        var refresh = Ui.Button("Làm mới", 105);
        var export = Ui.ExportButton(_grid, "Dữ liệu trong Thùng rác", 108);
        AppTheme.SetButtonRole(restore, ButtonRole.Primary);
        buttons.Controls.AddRange([restore, refresh, export]);

        restore.Click += async (_, _) => await RestoreSelectedAsync();
        refresh.Click += async (_, _) => await LoadDataAsync();
        _search.TextChanged += async (_, _) => await LoadDataAsync();
        _typeFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
        _grid.CellDoubleClick += async (_, _) =>
        {
            if (PermissionService.Has(PermissionCodes.RecycleBinRestore))
                await RestoreSelectedAsync();
        };

        _typeFilter.DataSource = Categories;
        _typeFilter.DisplayMember = "Value";
        _typeFilter.ValueMember = "Key";

        Controls.Add(_grid);
        Controls.Add(buttons);
        Controls.Add(top);
        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (!IsHandleCreated) return;

        await using var db = new AppDbContext();
        var rows = new List<RecycleBinRow>();

        var devices = await db.Devices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.SerialNumber,
                TypeName = x.DeviceType.Name,
                DepartmentName = x.Department != null ? x.Department.Name : null,
                x.DeletedAtUtc,
                x.DeletedByUsername
            })
            .ToListAsync();
        rows.AddRange(devices.Select(x => new RecycleBinRow(
            "Device", x.Id, "Thiết bị", x.Code, x.Name,
            $"{x.TypeName} · Serial: {x.SerialNumber ?? "-"} · Phòng: {x.DepartmentName ?? "-"}",
            x.DeletedAtUtc, x.DeletedByUsername)));

        var employees = await db.Employees
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.FullName,
                DepartmentName = x.Department.Name,
                x.Email,
                x.DeletedAtUtc,
                x.DeletedByUsername
            })
            .ToListAsync();
        rows.AddRange(employees.Select(x => new RecycleBinRow(
            "Employee", x.Id, "Nhân viên", x.Code, x.FullName,
            $"{x.DepartmentName} · {x.Email ?? "-"}",
            x.DeletedAtUtc, x.DeletedByUsername)));

        var deviceTypes = await db.DeviceTypes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAtUtc)
            .Select(x => new { x.Id, x.Name, x.DeletedAtUtc, x.DeletedByUsername })
            .ToListAsync();
        rows.AddRange(deviceTypes.Select(x => new RecycleBinRow(
            "DeviceType", x.Id, "Loại thiết bị", x.Id.ToString(), x.Name,
            "Danh mục loại thiết bị", x.DeletedAtUtc, x.DeletedByUsername)));

        var departments = await db.Departments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAtUtc)
            .Select(x => new { x.Id, x.Code, x.Name, x.DeletedAtUtc, x.DeletedByUsername })
            .ToListAsync();
        rows.AddRange(departments.Select(x => new RecycleBinRow(
            "Department", x.Id, "Phòng ban", x.Code, x.Name,
            "Danh mục phòng ban", x.DeletedAtUtc, x.DeletedByUsername)));

        var users = await db.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.FullName,
                RoleName = x.Role.Name,
                x.IsActive,
                x.DeletedAtUtc,
                x.DeletedByUsername
            })
            .ToListAsync();
        rows.AddRange(users.Select(x => new RecycleBinRow(
            "User", x.Id, "Tài khoản", x.Username, x.FullName,
            $"Vai trò: {x.RoleName} · {(x.IsActive ? "Đang hoạt động" : "Đã khóa")}",
            x.DeletedAtUtc, x.DeletedByUsername)));

        var maintenances = await db.DeviceMaintenances
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Code,
                DeviceCode = x.Device.Code,
                DeviceName = x.Device.Name,
                x.Type,
                x.Status,
                x.DeletedAtUtc,
                x.DeletedByUsername
            })
            .ToListAsync();
        rows.AddRange(maintenances.Select(x => new RecycleBinRow(
            "Maintenance", x.Id, "Bảo trì / Sửa chữa", x.Code,
            $"{x.DeviceCode} - {x.DeviceName}",
            $"{x.Type.ToDisplayName()} · {x.Status.ToDisplayName()}",
            x.DeletedAtUtc, x.DeletedByUsername)));

        var selectedType = _typeFilter.SelectedValue as string ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(selectedType))
            rows = rows.Where(x => x.EntityType == selectedType).ToList();

        var keyword = _search.Text.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            rows = rows.Where(x =>
                    x.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Details.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (x.DeletedByUsername?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        _grid.DataSource = rows
            .OrderByDescending(x => x.DeletedAtUtc)
            .ToList();

        ConfigureColumns();
    }

    private void ConfigureColumns()
    {
        if (_grid.Columns[nameof(RecycleBinRow.EntityType)] is { } entityType) entityType.Visible = false;
        if (_grid.Columns[nameof(RecycleBinRow.Id)] is { } id) id.Visible = false;

        SetHeader(nameof(RecycleBinRow.Category), "Loại dữ liệu");
        SetHeader(nameof(RecycleBinRow.Key), "Mã / Tài khoản");
        SetHeader(nameof(RecycleBinRow.Name), "Tên / Nội dung");
        SetHeader(nameof(RecycleBinRow.Details), "Chi tiết");
        SetHeader(nameof(RecycleBinRow.DeletedAtDisplay), "Thời gian xóa");
        SetHeader(nameof(RecycleBinRow.DeletedByDisplay), "Người xóa");

        if (_grid.Columns[nameof(RecycleBinRow.DeletedAtUtc)] is { } deletedAtUtc)
            deletedAtUtc.Visible = false;
        if (_grid.Columns[nameof(RecycleBinRow.DeletedByUsername)] is { } deletedByUsername)
            deletedByUsername.Visible = false;

        AppTheme.SetFixedColumn(_grid, nameof(RecycleBinRow.Category), 145, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, nameof(RecycleBinRow.Key), 130, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFillColumn(_grid, nameof(RecycleBinRow.Name), 100F, 190);
        AppTheme.SetFillColumn(_grid, nameof(RecycleBinRow.Details), 155F, 280, wrap: true);
        AppTheme.SetFixedColumn(_grid, nameof(RecycleBinRow.DeletedAtDisplay), 145, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, nameof(RecycleBinRow.DeletedByDisplay), 120, DataGridViewContentAlignment.MiddleCenter);
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        AppTheme.NormalizeGridRows(_grid);
    }

    private void SetHeader(string columnName, string text)
    {
        if (_grid.Columns[columnName] is { } column)
            column.HeaderText = text;
    }

    private RecycleBinRow? SelectedRow() => _grid.CurrentRow?.DataBoundItem as RecycleBinRow;

    private async Task RestoreSelectedAsync()
    {
        PermissionService.Demand(PermissionCodes.RecycleBinRestore, "khôi phục dữ liệu từ Thùng rác");

        var row = SelectedRow();
        if (row is null)
        {
            MessageBox.Show("Hãy chọn một dòng trong Thùng rác.", "Khôi phục", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(
                $"Khôi phục {row.Category.ToLowerInvariant()} '{row.Key} - {row.Name}'?",
                "Xác nhận khôi phục",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        await using var db = new AppDbContext();
        var restored = row.EntityType switch
        {
            "Device" => await RestoreDeviceAsync(db, row.Id),
            "Employee" => await RestoreEmployeeAsync(db, row.Id),
            "DeviceType" => await RestoreDeviceTypeAsync(db, row.Id),
            "Department" => await RestoreDepartmentAsync(db, row.Id),
            "User" => await RestoreUserAsync(db, row.Id),
            "Maintenance" => await RestoreMaintenanceAsync(db, row.Id),
            _ => false
        };

        if (!restored)
            return;

        await db.SaveChangesAsync();
        MessageBox.Show("Khôi phục dữ liệu thành công.", "Thùng rác", MessageBoxButtons.OK, MessageBoxIcon.Information);
        await LoadDataAsync();
    }

    private static async Task<bool> RestoreDeviceAsync(AppDbContext db, int id)
    {
        var entity = await db.Devices.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted);
        if (entity is null) return false;

        var typeActive = await db.DeviceTypes.IgnoreQueryFilters().AnyAsync(x => x.Id == entity.DeviceTypeId && !x.IsDeleted);
        if (!typeActive)
        {
            MessageBox.Show("Loại thiết bị của bản ghi này đang nằm trong Thùng rác. Hãy khôi phục loại thiết bị trước.", "Chưa thể khôi phục");
            return false;
        }

        if (entity.DepartmentId is int departmentId)
        {
            var departmentActive = await db.Departments.IgnoreQueryFilters().AnyAsync(x => x.Id == departmentId && !x.IsDeleted);
            if (!departmentActive)
            {
                MessageBox.Show("Phòng ban của thiết bị đang nằm trong Thùng rác. Hãy khôi phục phòng ban trước.", "Chưa thể khôi phục");
                return false;
            }
        }

        SoftDeleteService.Restore(entity);
        return true;
    }

    private static async Task<bool> RestoreEmployeeAsync(AppDbContext db, int id)
    {
        var entity = await db.Employees.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted);
        if (entity is null) return false;

        var departmentActive = await db.Departments.IgnoreQueryFilters().AnyAsync(x => x.Id == entity.DepartmentId && !x.IsDeleted);
        if (!departmentActive)
        {
            MessageBox.Show("Phòng ban của nhân viên đang nằm trong Thùng rác. Hãy khôi phục phòng ban trước.", "Chưa thể khôi phục");
            return false;
        }

        SoftDeleteService.Restore(entity);
        return true;
    }

    private static async Task<bool> RestoreDeviceTypeAsync(AppDbContext db, int id)
    {
        var entity = await db.DeviceTypes.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted);
        if (entity is null) return false;
        SoftDeleteService.Restore(entity);
        return true;
    }

    private static async Task<bool> RestoreDepartmentAsync(AppDbContext db, int id)
    {
        var entity = await db.Departments.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted);
        if (entity is null) return false;
        SoftDeleteService.Restore(entity);
        return true;
    }

    private static async Task<bool> RestoreUserAsync(AppDbContext db, int id)
    {
        var entity = await db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted);
        if (entity is null) return false;
        SoftDeleteService.Restore(entity);
        return true;
    }

    private static async Task<bool> RestoreMaintenanceAsync(AppDbContext db, int id)
    {
        var entity = await db.DeviceMaintenances
            .IgnoreQueryFilters()
            .Include(x => x.Device)
            .SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted);
        if (entity is null) return false;

        if (entity.Device.IsDeleted)
        {
            MessageBox.Show("Thiết bị của phiếu này đang nằm trong Thùng rác. Hãy khôi phục thiết bị trước.", "Chưa thể khôi phục");
            return false;
        }

        SoftDeleteService.Restore(entity);
        if (entity.Status is MaintenanceStatus.Pending or MaintenanceStatus.InProgress)
            entity.Device.Status = DeviceStatus.Repair;
        return true;
    }

    private sealed record RecycleBinRow(
        string EntityType,
        int Id,
        string Category,
        string Key,
        string Name,
        string Details,
        DateTimeOffset? DeletedAtUtc,
        string? DeletedByUsername)
    {
        public string DeletedAtDisplay => DeletedAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "-";
        public string DeletedByDisplay => string.IsNullOrWhiteSpace(DeletedByUsername) ? "Hệ thống" : DeletedByUsername;
    }
}
