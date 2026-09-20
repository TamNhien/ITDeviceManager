using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class AuditLogsForm : AppForm
{
    private const int MaxRows = 1000;

    private readonly DataGridView _grid = new();
    private readonly TextBox _search = new() { Width = 220, PlaceholderText = "Người dùng, nội dung, mã..." };
    private readonly ComboBox _actionFilter = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _entityFilter = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _fromDate = new()
    {
        Width = 138,
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        ShowCheckBox = true,
        Checked = false
    };
    private readonly DateTimePicker _toDate = new()
    {
        Width = 138,
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        ShowCheckBox = true,
        Checked = false
    };
    private readonly Label _countLabel = new()
    {
        AutoSize = true,
        ForeColor = AppTheme.TextSecondary,
        Margin = new Padding(14, 13, 3, 3)
    };

    private bool _loadingFilters;

    public AuditLogsForm()
    {
        Text = "Nhật ký hoạt động";
        Ui.ConfigureGrid(_grid);

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(0, 6, 0, 4),
            WrapContents = true,
            AutoScroll = false
        };
        top.Controls.Add(Ui.Label("Tìm kiếm:"));
        top.Controls.Add(_search);
        top.Controls.Add(Ui.Label("Hành động:"));
        top.Controls.Add(_actionFilter);
        top.Controls.Add(Ui.Label("Đối tượng:"));
        top.Controls.Add(_entityFilter);
        top.Controls.Add(Ui.Label("Từ ngày:"));
        top.Controls.Add(_fromDate);
        top.Controls.Add(Ui.Label("Đến ngày:"));
        top.Controls.Add(_toDate);
        top.Controls.Add(_countLabel);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        var details = Ui.Button("Xem chi tiết", 120);
        var refresh = Ui.Button("Làm mới", 100);
        bottom.Controls.AddRange([details, refresh]);

        details.Click += async (_, _) => await OpenDetailsAsync();
        refresh.Click += async (_, _) => await LoadFiltersAsync(preserveSelection: true);
        _grid.CellDoubleClick += async (_, _) => await OpenDetailsAsync();
        _search.TextChanged += async (_, _) => await LoadDataAsync();
        _actionFilter.SelectedIndexChanged += async (_, _) => { if (!_loadingFilters) await LoadDataAsync(); };
        _entityFilter.SelectedIndexChanged += async (_, _) => { if (!_loadingFilters) await LoadDataAsync(); };
        _fromDate.ValueChanged += async (_, _) => { if (!_loadingFilters) await LoadDataAsync(); };
        _toDate.ValueChanged += async (_, _) => { if (!_loadingFilters) await LoadDataAsync(); };

        Controls.Add(_grid);
        Controls.Add(bottom);
        Controls.Add(top);
        Load += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (!AppSession.IsAdmin)
        {
            MessageBox.Show(
                "Chỉ tài khoản Admin được xem nhật ký hoạt động.",
                "Không có quyền truy cập",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            Close();
            return;
        }

        await LoadFiltersAsync(preserveSelection: false);
    }

    private async Task LoadFiltersAsync(bool preserveSelection)
    {
        _loadingFilters = true;
        try
        {
            var previousAction = preserveSelection ? _actionFilter.SelectedItem?.ToString() : null;
            var previousEntity = preserveSelection ? _entityFilter.SelectedItem?.ToString() : null;

            await using var db = new AppDbContext();
            var actions = await db.AuditLogs.AsNoTracking()
                .Select(x => x.Action)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
            var entities = await db.AuditLogs.AsNoTracking()
                .Select(x => x.EntityName)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            _actionFilter.Items.Clear();
            _actionFilter.Items.Add("Tất cả");
            foreach (var action in actions)
                _actionFilter.Items.Add(action);

            _entityFilter.Items.Clear();
            _entityFilter.Items.Add("Tất cả");
            foreach (var entity in entities)
                _entityFilter.Items.Add(entity);

            _actionFilter.SelectedItem = previousAction is not null && _actionFilter.Items.Contains(previousAction)
                ? previousAction
                : "Tất cả";
            _entityFilter.SelectedItem = previousEntity is not null && _entityFilter.Items.Contains(previousEntity)
                ? previousEntity
                : "Tất cả";
        }
        finally
        {
            _loadingFilters = false;
        }

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (!IsHandleCreated || _loadingFilters)
            return;

        await using var db = new AppDbContext();
        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        var keyword = _search.Text.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Username.Contains(keyword) ||
                x.Action.Contains(keyword) ||
                x.EntityName.Contains(keyword) ||
                x.Description.Contains(keyword) ||
                (x.EntityKey != null && x.EntityKey.Contains(keyword)) ||
                (x.ComputerName != null && x.ComputerName.Contains(keyword)));
        }

        if (_actionFilter.SelectedItem is string action && action != "Tất cả")
            query = query.Where(x => x.Action == action);
        if (_entityFilter.SelectedItem is string entity && entity != "Tất cả")
            query = query.Where(x => x.EntityName == entity);

        if (_fromDate.Checked)
        {
            var local = DateTime.SpecifyKind(_fromDate.Value.Date, DateTimeKind.Local);
            var fromUtc = new DateTimeOffset(local).ToUniversalTime();
            query = query.Where(x => x.OccurredAtUtc >= fromUtc);
        }

        if (_toDate.Checked)
        {
            var localExclusive = DateTime.SpecifyKind(_toDate.Value.Date.AddDays(1), DateTimeKind.Local);
            var toUtcExclusive = new DateTimeOffset(localExclusive).ToUniversalTime();
            query = query.Where(x => x.OccurredAtUtc < toUtcExclusive);
        }

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(MaxRows)
            .Select(x => new
            {
                x.Id,
                x.OccurredAtUtc,
                x.Username,
                x.Action,
                x.EntityName,
                x.EntityKey,
                x.Description,
                x.ComputerName,
                x.AppVersion
            })
            .ToListAsync();

        _grid.DataSource = rows.Select(x => new
        {
            x.Id,
            Thời_gian = x.OccurredAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
            Người_thực_hiện = x.Username,
            Hành_động = x.Action,
            Đối_tượng = x.EntityName,
            Mã_đối_tượng = x.EntityKey ?? string.Empty,
            Nội_dung = x.Description,
            Máy_tính = x.ComputerName ?? string.Empty,
            Phiên_bản = x.AppVersion ?? string.Empty
        }).ToList();

        if (_grid.Columns["Id"] is { } idColumn)
            idColumn.Visible = false;
        if (_grid.Columns["Nội_dung"] is { } descriptionColumn)
            descriptionColumn.FillWeight = 180;
        if (_grid.Columns["Thời_gian"] is { } timeColumn)
            timeColumn.FillWeight = 105;
        if (_grid.Columns["Người_thực_hiện"] is { } userColumn)
            userColumn.FillWeight = 90;

        _countLabel.Text = total > MaxRows
            ? $"Hiển thị {MaxRows:N0}/{total:N0} nhật ký"
            : $"Tổng: {total:N0} nhật ký";
    }

    private long? SelectedId()
    {
        var value = _grid.CurrentRow?.Cells["Id"].Value;
        return value is null ? null : Convert.ToInt64(value);
    }

    private async Task OpenDetailsAsync()
    {
        var id = SelectedId();
        if (id is null)
            return;

        await using var db = new AppDbContext();
        var exists = await db.AuditLogs.AsNoTracking().AnyAsync(x => x.Id == id.Value);
        if (!exists)
        {
            MessageBox.Show("Nhật ký này không còn tồn tại.", "Thông báo");
            await LoadDataAsync();
            return;
        }

        using var form = new AuditLogDetailForm(id.Value);
        form.ShowDialog(this);
    }
}
