using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class AssignmentsForm : AppForm
{
    private readonly DataGridView _grid = new();

    public AssignmentsForm()
    {
        Text = "Cấp phát / Thu hồi";
        Ui.ConfigureGrid(_grid);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55 };
        var assign = Ui.Button("Cấp phát", 120);
        var ret = Ui.Button("Thu hồi", 120);
        var refresh = Ui.Button("Làm mới");
        var export = Ui.ExportButton(_grid, "Lịch sử cấp phát và thu hồi");
        buttons.Controls.AddRange([assign, ret, refresh, export]);
        assign.Enabled = ret.Enabled = AppSession.IsAdmin;
        assign.Click += async (_, _) =>
        {
            using var form = new AssignmentEditForm();
            if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        };
        ret.Click += async (_, _) => await ReturnAsync();
        refresh.Click += async (_, _) => await LoadDataAsync();

        Controls.Add(_grid);
        Controls.Add(buttons);
        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await using var db = new AppDbContext();
        var raw = await db.DeviceAssignments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderByDescending(x => x.AssignedDate)
            .Select(x => new
            {
                x.Id,
                Device = x.Device.Code + " - " + x.Device.Name + (x.Device.IsDeleted ? " [Đã xóa]" : ""),
                Employee = x.Employee.Code + " - " + x.Employee.FullName + (x.Employee.IsDeleted ? " [Đã xóa]" : ""),
                Department = x.Employee.Department.Name + (x.Employee.Department.IsDeleted ? " [Đã xóa]" : ""),
                x.AssignedDate,
                x.ReturnedDate,
                x.Note,
                DeviceStatus = x.Device.Status
            })
            .ToListAsync();

        _grid.DataSource = raw.Select(x => new
        {
            x.Id,
            Thiết_bị = x.Device,
            Nhân_viên = x.Employee,
            Phòng_ban = x.Department,
            Ngày_cấp = x.AssignedDate.ToString("dd/MM/yyyy"),
            Ngày_trả = x.ReturnedDate?.ToString("dd/MM/yyyy") ?? string.Empty,
            Tình_trạng_cấp_phát = x.ReturnedDate is null ? "Đang cấp phát" : "Đã thu hồi",
            Trạng_thái_thiết_bị = x.DeviceStatus.ToDisplayName(),
            Ghi_chú = x.Note
        }).ToList();

        if (_grid.Columns["Id"] is { } idColumn) idColumn.Visible = false;

        AppTheme.NormalizeGridHeaders(_grid);
        AppTheme.SetFillColumn(_grid, "Thiết_bị", 125F, 210);
        AppTheme.SetFillColumn(_grid, "Nhân_viên", 110F, 190);
        AppTheme.SetFillColumn(_grid, "Phòng_ban", 95F, 165);
        AppTheme.SetFixedColumn(_grid, "Ngày_cấp", 105, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Ngày_trả", 105, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Tình_trạng_cấp_phát", 130, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFixedColumn(_grid, "Trạng_thái_thiết_bị", 130, DataGridViewContentAlignment.MiddleCenter);
        AppTheme.SetFillColumn(_grid, "Ghi_chú", 190F, 300, wrap: true);
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        AppTheme.NormalizeGridRows(_grid);
    }

    private int? SelectedId()
    {
        var value = _grid.CurrentRow?.Cells["Id"].Value;
        return value is null ? null : Convert.ToInt32(value);
    }

    private async Task ReturnAsync()
    {
        var id = SelectedId();
        if (id is null) return;

        await using var db = new AppDbContext();
        var assignment = await db.DeviceAssignments.Include(x => x.Device).SingleOrDefaultAsync(x => x.Id == id.Value);
        if (assignment is null) return;
        if (assignment.ReturnedDate is not null)
        {
            MessageBox.Show("Phiếu này đã thu hồi trước đó.");
            return;
        }
        if (MessageBox.Show("Xác nhận thu hồi thiết bị?", "Thu hồi", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        assignment.ReturnedDate = DateTime.Today;

        // Do not erase a real repair/broken/retired state just because an assignment is returned.
        // Only the normal InUse state becomes Available on return.
        if (assignment.Device.Status == DeviceStatus.InUse)
            assignment.Device.Status = DeviceStatus.Available;

        assignment.Device.DepartmentId = null;
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }
}
