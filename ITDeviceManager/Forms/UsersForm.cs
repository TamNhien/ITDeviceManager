using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class UsersForm : AppForm
{
    private readonly DataGridView _grid = new();

    public UsersForm()
    {
        Text = "Tài khoản";
        Ui.ConfigureGrid(_grid);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55 };
        var add = Ui.Button("Thêm");
        var edit = Ui.Button("Sửa");
        var delete = Ui.Button("Xóa");
        var refresh = Ui.Button("Làm mới");
        buttons.Controls.AddRange([add, edit, delete, refresh]);

        add.Click += async (_, _) =>
        {
            using var form = new UserEditForm();
            if (form.ShowDialog() == DialogResult.OK)
                await LoadDataAsync();
        };
        edit.Click += async (_, _) => await EditAsync();
        delete.Click += async (_, _) => await DeleteAsync();
        refresh.Click += async (_, _) => await LoadDataAsync();

        Controls.Add(_grid);
        Controls.Add(buttons);
        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await using var db = new AppDbContext();
        var raw = await db.Users.AsNoTracking()
            .OrderBy(x => x.Username)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.FullName,
                x.Email,
                x.PhoneNumber,
                RoleName = x.Role.Name,
                x.IsActive
            })
            .ToListAsync();

        _grid.DataSource = raw.Select(x => new
        {
            x.Id,
            Tên_đăng_nhập = x.Username,
            Họ_tên = x.FullName,
            Email = x.Email ?? string.Empty,
            Số_điện_thoại = PhoneNumberValidator.Normalize(x.PhoneNumber),
            Quyền = x.RoleName,
            Hoạt_động = x.IsActive ? "Có" : "Không"
        }).ToList();

        var idColumn = _grid.Columns["Id"];
        if (idColumn is not null)
            idColumn.Visible = false;
    }

    private int? Id() => _grid.CurrentRow?.Cells["Id"].Value as int?;

    private async Task EditAsync()
    {
        var id = Id();
        if (id is null)
            return;

        using var form = new UserEditForm(id);
        if (form.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteAsync()
    {
        var id = Id();
        if (id is null)
            return;

        if (Services.AppSession.CurrentUser?.Id == id)
        {
            MessageBox.Show("Không thể xóa tài khoản đang đăng nhập.");
            return;
        }

        if (!Ui.ConfirmDelete("tài khoản đã chọn"))
            return;

        await using var db = new AppDbContext();
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return;

        db.Remove(user);
        await db.SaveChangesAsync();
        await LoadDataAsync();
    }
}
