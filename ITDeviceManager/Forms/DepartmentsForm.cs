using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DepartmentsForm : AppForm
{
    private readonly DataGridView _grid = new();
    public DepartmentsForm()
    {
        Text = "Phòng ban";
        Ui.ConfigureGrid(_grid);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55 };
        var add = Ui.Button("Thêm"); var edit = Ui.Button("Sửa"); var delete = Ui.Button("Xóa"); var refresh = Ui.Button("Làm mới");
        buttons.Controls.AddRange([add, edit, delete, refresh]);
        add.Enabled = edit.Enabled = delete.Enabled = AppSession.IsAdmin;
        add.Click += async (_, _) => await AddAsync(); edit.Click += async (_, _) => await EditAsync(); delete.Click += async (_, _) => await DeleteAsync(); refresh.Click += async (_, _) => await LoadDataAsync();
        Controls.Add(_grid); Controls.Add(buttons);
        Load += async (_, _) => await LoadDataAsync();
    }
    private async Task LoadDataAsync()
    {
        await using var db = new AppDbContext();
        _grid.DataSource = await db.Departments.AsNoTracking().OrderBy(x => x.Code).Select(x => new { x.Id, Mã = x.Code, Tên_phòng_ban = x.Name, Số_nhân_viên = x.Employees.Count }).ToListAsync();
        var idColumn = _grid.Columns["Id"];
        if (idColumn is not null) idColumn.Visible = false;
    }
    private int? Id() => _grid.CurrentRow?.Cells["Id"].Value as int?;
    private async Task AddAsync()
    {
        using var f = new LookupEditForm("Thêm phòng ban", true); if (f.ShowDialog() != DialogResult.OK) return;
        await using var db = new AppDbContext();
        if (await db.Departments.AnyAsync(x => x.Code == f.ItemCode)) { MessageBox.Show("Mã phòng ban đã tồn tại."); return; }
        db.Departments.Add(new Models.Department { Code = f.ItemCode, Name = f.ItemName }); await db.SaveChangesAsync(); await LoadDataAsync();
    }
    private async Task EditAsync()
    {
        var id = Id(); if (id is null) return; await using var db = new AppDbContext(); var e = await db.Departments.FindAsync(id); if (e is null) return;
        using var f = new LookupEditForm("Sửa phòng ban", true, e.Code, e.Name); if (f.ShowDialog() != DialogResult.OK) return;
        if (await db.Departments.AnyAsync(x => x.Code == f.ItemCode && x.Id != id)) { MessageBox.Show("Mã phòng ban đã tồn tại."); return; }
        e.Code = f.ItemCode; e.Name = f.ItemName; await db.SaveChangesAsync(); await LoadDataAsync();
    }
    private async Task DeleteAsync()
    {
        var id = Id(); if (id is null || !Ui.ConfirmDelete("phòng ban đã chọn")) return; await using var db = new AppDbContext();
        if (await db.Employees.AnyAsync(x => x.DepartmentId == id) || await db.Devices.AnyAsync(x => x.DepartmentId == id)) { MessageBox.Show("Phòng ban đang được sử dụng nên không thể xóa."); return; }
        var e = await db.Departments.FindAsync(id); if (e is null) return; db.Remove(e); await db.SaveChangesAsync(); await LoadDataAsync();
    }
}
