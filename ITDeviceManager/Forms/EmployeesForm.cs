using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class EmployeesForm : AppForm
{
    private readonly DataGridView _grid = new();
    private readonly TextInput _search = new(){Width=260,PlaceholderText="Mã, họ tên, email...",TextAlign=HorizontalAlignment.Center};
    public EmployeesForm()
    {
        Text="Nhân viên"; Ui.ConfigureGrid(_grid);
        var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=55,Padding=new Padding(0,7,0,4)}; top.Controls.Add(Ui.Label("Tìm kiếm:")); top.Controls.Add(_search);
        var buttons=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=55}; var add=Ui.Button("Thêm");var edit=Ui.Button("Sửa");var delete=Ui.Button("Xóa");var import=Ui.Button("Nhập Excel");var refresh=Ui.Button("Làm mới");var export=Ui.ExportButton(_grid,"Danh sách nhân viên");AppTheme.SetButtonRole(import,ButtonRole.Secondary);buttons.Controls.AddRange([add,edit,delete,import,refresh,export]); add.Enabled=edit.Enabled=delete.Enabled=AppSession.IsAdmin;
        add.Click+=async(_,_)=>{using var f=new EmployeeEditForm();if(f.ShowDialog()==DialogResult.OK)await LoadDataAsync();}; edit.Click+=async(_,_)=>await EditAsync(); delete.Click+=async(_,_)=>await DeleteAsync(); import.Click+=async(_,_)=>{using var f=new ExcelImportForm();f.ShowDialog(this);if(f.ImportCompleted)await LoadDataAsync();}; refresh.Click+=async(_,_)=>await LoadDataAsync(); _search.TextChanged+=async(_,_)=>await LoadDataAsync();
        Controls.Add(_grid);Controls.Add(buttons);Controls.Add(top);Load+=async(_,_)=>await LoadDataAsync();
    }
    private async Task LoadDataAsync()
    {
        await using var db = new AppDbContext();
        var q = db.Employees.AsNoTracking().AsQueryable();
        var k = _search.Text.Trim();
        if (k.Length > 0)
            q = q.Where(x => x.Code.Contains(k) || x.FullName.Contains(k) || (x.Email != null && x.Email.Contains(k)));

        var raw = await q
            .OrderBy(x => x.Code)
            .Select(x => new { x.Id, x.Code, x.FullName, x.Email, x.Phone, DepartmentName = x.Department.Name })
            .ToListAsync();

        _grid.DataSource = raw.Select(x => new
        {
            x.Id,
            Mã = x.Code,
            Họ_tên = x.FullName,
            Email = x.Email,
            Điện_thoại = PhoneNumberValidator.Normalize(x.Phone),
            Phòng_ban = x.DepartmentName
        }).ToList();

        var idColumn = _grid.Columns["Id"];
        if (idColumn is not null)
            idColumn.Visible = false;
    }
    private int? Id()=>_grid.CurrentRow?.Cells["Id"].Value as int?;
    private async Task EditAsync(){var id=Id();if(id is null)return;using var f=new EmployeeEditForm(id);if(f.ShowDialog()==DialogResult.OK)await LoadDataAsync();}
    private async Task DeleteAsync()
    {
        var id = Id();
        if (id is null || !Ui.ConfirmSoftDelete("nhân viên đã chọn")) return;
        await using var db = new AppDbContext();
        if (await db.DeviceAssignments.AnyAsync(x => x.EmployeeId == id && x.ReturnedDate == null))
        {
            MessageBox.Show("Nhân viên vẫn đang được cấp thiết bị. Hãy thu hồi toàn bộ thiết bị trước khi xóa.", "Không thể xóa");
            return;
        }
        var entity = await db.Employees.FindAsync(id);
        if (entity is null) return;
        SoftDeleteService.MarkDeleted(entity);
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }
}
