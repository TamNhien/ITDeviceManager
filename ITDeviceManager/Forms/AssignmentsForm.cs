using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class AssignmentsForm : AppForm
{
    private readonly DataGridView _grid=new();
    public AssignmentsForm(){Text="Cấp phát / Thu hồi";Ui.ConfigureGrid(_grid);var b=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=55};var assign=Ui.Button("Cấp phát",120);var ret=Ui.Button("Thu hồi",120);var refresh=Ui.Button("Làm mới");b.Controls.AddRange([assign,ret,refresh]);assign.Enabled=ret.Enabled=AppSession.IsAdmin;assign.Click+=async(_,_)=>{using var f=new AssignmentEditForm();if(f.ShowDialog()==DialogResult.OK)await LoadDataAsync();};ret.Click+=async(_,_)=>await ReturnAsync();refresh.Click+=async(_,_)=>await LoadDataAsync();Controls.Add(_grid);Controls.Add(b);Load+=async(_,_)=>await LoadDataAsync();}
    private async Task LoadDataAsync(){await using var db=new AppDbContext();var raw=await db.DeviceAssignments.AsNoTracking().OrderByDescending(x=>x.AssignedDate).Select(x=>new{x.Id,Device=x.Device.Code+" - "+x.Device.Name,Employee=x.Employee.Code+" - "+x.Employee.FullName,Department=x.Employee.Department.Name,x.AssignedDate,x.ReturnedDate,x.Note}).ToListAsync();_grid.DataSource=raw.Select(x=>new{x.Id,Thiết_bị=x.Device,Nhân_viên=x.Employee,Phòng_ban=x.Department,Ngày_cấp=x.AssignedDate.ToString("dd/MM/yyyy"),Ngày_trả=x.ReturnedDate?.ToString("dd/MM/yyyy"),Trạng_thái=x.ReturnedDate is null?"Đang sử dụng":"Đã thu hồi",Ghi_chú=x.Note}).ToList();var idColumn=_grid.Columns["Id"];if(idColumn is not null)idColumn.Visible=false;}
    private int? Id()=>_grid.CurrentRow?.Cells["Id"].Value as int?;
    private async Task ReturnAsync(){var id=Id();if(id is null)return;await using var db=new AppDbContext();var a=await db.DeviceAssignments.Include(x=>x.Device).SingleOrDefaultAsync(x=>x.Id==id);if(a is null)return;if(a.ReturnedDate is not null){MessageBox.Show("Phiếu này đã thu hồi trước đó.");return;}if(MessageBox.Show("Xác nhận thu hồi thiết bị?","Thu hồi",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;a.ReturnedDate=DateTime.Today;a.Device.Status=DeviceStatus.Available;a.Device.DepartmentId=null;await db.SaveChangesAsync();await LoadDataAsync();}
}
