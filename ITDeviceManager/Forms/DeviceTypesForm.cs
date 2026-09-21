using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DeviceTypesForm : AppForm
{
    private readonly DataGridView _grid = new();
    public DeviceTypesForm()
    {
        Text = "Loại thiết bị"; Ui.ConfigureGrid(_grid);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55 };
        var add = Ui.Button("Thêm"); var edit = Ui.Button("Sửa"); var delete = Ui.Button("Xóa"); var refresh = Ui.Button("Làm mới"); var export = Ui.ExportButton(_grid, "Danh sách loại thiết bị");
        buttons.Controls.AddRange([add, edit, delete, refresh, export]); add.Enabled = edit.Enabled = delete.Enabled = AppSession.IsAdmin;
        add.Click += async (_, _) => await AddAsync(); edit.Click += async (_, _) => await EditAsync(); delete.Click += async (_, _) => await DeleteAsync(); refresh.Click += async (_, _) => await LoadDataAsync();
        Controls.Add(_grid); Controls.Add(buttons); Load += async (_, _) => await LoadDataAsync();
    }
    private async Task LoadDataAsync(){ await using var db = new AppDbContext(); _grid.DataSource = await db.DeviceTypes.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new{x.Id,Tên_loại=x.Name,Số_thiết_bị=x.Devices.Count}).ToListAsync(); var idColumn=_grid.Columns["Id"];if(idColumn is not null)idColumn.Visible=false; }
    private int? Id()=>_grid.CurrentRow?.Cells["Id"].Value as int?;
    private async Task AddAsync(){ using var f=new LookupEditForm("Thêm loại thiết bị",false); if(f.ShowDialog()!=DialogResult.OK)return; await using var db=new AppDbContext(); if(await db.DeviceTypes.AnyAsync(x=>x.Name==f.ItemName)){MessageBox.Show("Tên loại thiết bị đã tồn tại.");return;} db.DeviceTypes.Add(new Models.DeviceType{Name=f.ItemName}); await db.SaveChangesAsync(); await LoadDataAsync(); }
    private async Task EditAsync(){var id=Id();if(id is null)return;await using var db=new AppDbContext();var e=await db.DeviceTypes.FindAsync(id);if(e is null)return;using var f=new LookupEditForm("Sửa loại thiết bị",false,name:e.Name);if(f.ShowDialog()!=DialogResult.OK)return;if(await db.DeviceTypes.AnyAsync(x=>x.Name==f.ItemName&&x.Id!=id)){MessageBox.Show("Tên loại thiết bị đã tồn tại.");return;}e.Name=f.ItemName;await db.SaveChangesAsync();await LoadDataAsync();}
    private async Task DeleteAsync(){var id=Id();if(id is null||!Ui.ConfirmDelete("loại thiết bị đã chọn"))return;await using var db=new AppDbContext();if(await db.Devices.AnyAsync(x=>x.DeviceTypeId==id)){MessageBox.Show("Loại thiết bị đang được sử dụng nên không thể xóa.");return;}var e=await db.DeviceTypes.FindAsync(id);if(e is null)return;db.Remove(e);await db.SaveChangesAsync();await LoadDataAsync();}
}
