using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DashboardForm : AppForm
{
    private readonly Label _total = Card();
    private readonly Label _inUse = Card();
    private readonly Label _available = Card();
    private readonly Label _repair = Card();
    private readonly DataGridView _grid = new();

    public DashboardForm()
    {
        Text = "Tổng quan";
        Font = new Font("Segoe UI", 10);

        var title = new Label
        {
            Text = "TỔNG QUAN HỆ THỐNG",
            Dock = DockStyle.Top,
            Height = 55,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var cards = new TableLayoutPanel { Dock = DockStyle.Top, Height = 130, ColumnCount = 4, Padding = new Padding(0, 8, 0, 8) };
        for (var i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.Controls.Add(_total, 0, 0);
        cards.Controls.Add(_inUse, 1, 0);
        cards.Controls.Add(_available, 2, 0);
        cards.Controls.Add(_repair, 3, 0);

        var recent = new Label { Text = "Cấp phát gần đây", Dock = DockStyle.Top, Height = 38, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        Common.Ui.ConfigureGrid(_grid);

        Controls.Add(_grid);
        Controls.Add(recent);
        Controls.Add(cards);
        Controls.Add(title);
        Load += LoadDataAsync;
    }

    private static Label Card() => new()
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(8),
        BorderStyle = BorderStyle.FixedSingle,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 14, FontStyle.Bold)
    };

    private async void LoadDataAsync(object? sender, EventArgs e)
    {
        await using var db = new AppDbContext();
        _total.Text = $"Tổng thiết bị\n{await db.Devices.CountAsync()}";
        _inUse.Text = $"Đang sử dụng\n{await db.Devices.CountAsync(x => x.Status == DeviceStatus.InUse)}";
        _available.Text = $"Chưa sử dụng\n{await db.Devices.CountAsync(x => x.Status == DeviceStatus.Available)}";
        _repair.Text = $"Đang sửa chữa\n{await db.Devices.CountAsync(x => x.Status == DeviceStatus.Repair)}";

        _grid.DataSource = await db.DeviceAssignments
            .AsNoTracking()
            .OrderByDescending(x => x.AssignedDate)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                Thiết_bị = x.Device.Code + " - " + x.Device.Name,
                Nhân_viên = x.Employee.Code + " - " + x.Employee.FullName,
                Ngày_cấp = x.AssignedDate,
                Ngày_trả = x.ReturnedDate,
                Trạng_thái = x.ReturnedDate == null ? "Đang sử dụng" : "Đã thu hồi"
            })
            .ToListAsync();

        var idColumn = _grid.Columns["Id"];
        if (idColumn is not null) idColumn.Visible = false;
    }
}
