using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class DashboardForm : AppForm
{
    private readonly Label _totalValue = ValueLabel();
    private readonly Label _inUseValue = ValueLabel();
    private readonly Label _availableValue = ValueLabel();
    private readonly Label _repairValue = ValueLabel();
    private readonly Label _brokenValue = ValueLabel();
    private readonly Label _retiredValue = ValueLabel();
    private readonly DataGridView _grid = new();

    public DashboardForm()
    {
        Text = "Tổng quan";
        Font = new Font("Segoe UI", 10F);
        BackColor = AppTheme.Background;

        var intro = new Panel
        {
            Dock = DockStyle.Top,
            Height = 62,
            BackColor = AppTheme.Background
        };
        intro.Controls.Add(new Label
        {
            Text = "Tổng quan hệ thống",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 17F),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(0, 3)
        });
        intro.Controls.Add(new Label
        {
            Text = "Theo dõi nhanh trạng thái thiết bị và hoạt động cấp phát gần đây.",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(2, 36)
        });

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 232,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(0, 0, 0, 12),
            BackColor = AppTheme.Background
        };
        for (var i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        cards.Controls.Add(CreateMetricCard("Tổng thiết bị", "Tất cả tài sản đang quản lý", _totalValue, AppTheme.Primary), 0, 0);
        cards.Controls.Add(CreateMetricCard("Đang sử dụng", "Thiết bị đang được cấp phát", _inUseValue, AppTheme.Success), 1, 0);
        cards.Controls.Add(CreateMetricCard("Chưa sử dụng", "Sẵn sàng để cấp phát", _availableValue, AppTheme.Info), 2, 0);
        cards.Controls.Add(CreateMetricCard("Đang sửa chữa", "Đang bảo trì hoặc khắc phục lỗi", _repairValue, AppTheme.Warning), 0, 1);
        cards.Controls.Add(CreateMetricCard("Hỏng", "Thiết bị chưa thể tiếp tục sử dụng", _brokenValue, AppTheme.Danger), 1, 1);
        cards.Controls.Add(CreateMetricCard("Thanh lý", "Đã ngừng khai thác", _retiredValue, AppTheme.Purple), 2, 1);

        var recentCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(16)
        };

        var recentTitle = new Label
        {
            Text = "Cấp phát gần đây",
            Dock = DockStyle.Top,
            Height = 38,
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var recentSubtitle = new Label
        {
            Text = "10 hoạt động gần nhất; trạng thái thiết bị luôn lấy theo dữ liệu hiện tại.",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 9F),
            ForeColor = AppTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        Ui.ConfigureGrid(_grid);
        _grid.Margin = Padding.Empty;

        recentCard.Controls.Add(_grid);
        recentCard.Controls.Add(recentSubtitle);
        recentCard.Controls.Add(recentTitle);

        Controls.Add(recentCard);
        Controls.Add(cards);
        Controls.Add(intro);
        Load += LoadDataAsync;
    }

    private static Label ValueLabel() => new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 22F),
        ForeColor = AppTheme.TextPrimary,
        Location = new Point(18, 36)
    };

    private static Control CreateMetricCard(string title, string subtitle, Label valueLabel, Color accent)
    {
        var card = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(0)
        };

        var accentBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 5,
            BackColor = accent
        };
        card.Controls.Add(accentBar);

        card.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10.5F),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(18, 14)
        });
        card.Controls.Add(valueLabel);
        card.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(20, 74)
        });
        return card;
    }

    private async void LoadDataAsync(object? sender, EventArgs e)
    {
        await using var db = new AppDbContext();
        _totalValue.Text = (await db.Devices.CountAsync()).ToString("N0");
        _inUseValue.Text = (await db.Devices.CountAsync(x => x.Status == DeviceStatus.InUse)).ToString("N0");
        _availableValue.Text = (await db.Devices.CountAsync(x => x.Status == DeviceStatus.Available)).ToString("N0");
        _repairValue.Text = (await db.Devices.CountAsync(x => x.Status == DeviceStatus.Repair)).ToString("N0");
        _brokenValue.Text = (await db.Devices.CountAsync(x => x.Status == DeviceStatus.Broken)).ToString("N0");
        _retiredValue.Text = (await db.Devices.CountAsync(x => x.Status == DeviceStatus.Retired)).ToString("N0");

        var recentAssignments = await db.DeviceAssignments
            .AsNoTracking()
            .OrderByDescending(x => x.AssignedDate)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                Device = x.Device.Code + " - " + x.Device.Name,
                Employee = x.Employee.Code + " - " + x.Employee.FullName,
                x.AssignedDate,
                x.ReturnedDate,
                DeviceStatus = x.Device.Status
            })
            .ToListAsync();

        _grid.DataSource = recentAssignments.Select(x => new
        {
            x.Id,
            Thiết_bị = x.Device,
            Nhân_viên = x.Employee,
            Ngày_cấp = x.AssignedDate.ToString("dd/MM/yyyy"),
            Ngày_trả = x.ReturnedDate?.ToString("dd/MM/yyyy") ?? string.Empty,
            Tình_trạng_cấp_phát = x.ReturnedDate == null ? "Đang cấp phát" : "Đã thu hồi",
            Trạng_thái_thiết_bị = x.DeviceStatus.ToDisplayName()
        }).ToList();

        var idColumn = _grid.Columns["Id"];
        if (idColumn is not null) idColumn.Visible = false;
    }
}
