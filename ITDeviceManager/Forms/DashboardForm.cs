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
    private readonly Label _lastUpdatedLabel = new();
    private readonly PieChart _statusChart = new();
    private readonly VerticalColumnChart _typeChart = new();
    private readonly DataGridView _grid = new();

    public DashboardForm()
    {
        Text = "Tổng quan";
        Font = new Font("Segoe UI", 10F);
        BackColor = AppTheme.Background;

        var intro = BuildIntro();
        var cards = BuildMetricCards();
        var charts = BuildCharts();
        var recentCard = BuildRecentAssignmentsCard();

        Controls.Add(recentCard);
        Controls.Add(charts);
        Controls.Add(cards);
        Controls.Add(intro);
        Load += LoadDataAsync;
    }

    private Control BuildIntro()
    {
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
            Text = "Theo dõi nhanh tài sản, trạng thái sử dụng và cơ cấu thiết bị trong doanh nghiệp.",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(2, 36)
        });

        _lastUpdatedLabel.AutoSize = false;
        _lastUpdatedLabel.Width = 280;
        _lastUpdatedLabel.Height = 28;
        _lastUpdatedLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lastUpdatedLabel.TextAlign = ContentAlignment.MiddleRight;
        _lastUpdatedLabel.Font = new Font("Segoe UI", 8.8F);
        _lastUpdatedLabel.ForeColor = AppTheme.TextSecondary;
        _lastUpdatedLabel.Text = "Đang tải dữ liệu...";
        intro.Controls.Add(_lastUpdatedLabel);
        intro.Resize += (_, _) =>
        {
            _lastUpdatedLabel.Left = Math.Max(0, intro.ClientSize.Width - _lastUpdatedLabel.Width);
            _lastUpdatedLabel.Top = 19;
        };

        return intro;
    }

    private Control BuildMetricCards()
    {
        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 194,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(0, 0, 0, 8),
            BackColor = AppTheme.Background
        };
        for (var i = 0; i < 3; i++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        cards.Controls.Add(CreateMetricCard("Tổng thiết bị", "Tất cả tài sản đang quản lý", _totalValue, AppTheme.Primary), 0, 0);
        cards.Controls.Add(CreateMetricCard("Đang sử dụng", "Đang được cấp phát", _inUseValue, AppTheme.Success), 1, 0);
        cards.Controls.Add(CreateMetricCard("Chưa sử dụng", "Sẵn sàng để cấp phát", _availableValue, AppTheme.Info), 2, 0);
        cards.Controls.Add(CreateMetricCard("Đang sửa chữa", "Bảo trì hoặc khắc phục lỗi", _repairValue, AppTheme.Warning), 0, 1);
        cards.Controls.Add(CreateMetricCard("Hỏng", "Chưa thể tiếp tục sử dụng", _brokenValue, AppTheme.Danger), 1, 1);
        cards.Controls.Add(CreateMetricCard("Thanh lý", "Đã ngừng khai thác", _retiredValue, AppTheme.Purple), 2, 1);
        return cards;
    }

    private Control BuildCharts()
    {
        var charts = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 315,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 2, 0, 10),
            BackColor = AppTheme.Background
        };
        charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        charts.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        charts.Controls.Add(CreateChartCard(
            "Phân bố trạng thái thiết bị",
            string.Empty,
            _statusChart), 0, 0);

        charts.Controls.Add(CreateChartCard(
            "Thiết bị theo loại",
            "Top 6 loại thiết bị theo biểu đồ cột",
            _typeChart), 1, 0);

        return charts;
    }

    private Control BuildRecentAssignmentsCard()
    {
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
        return recentCard;
    }

    private static Label ValueLabel() => new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 19F),
        ForeColor = AppTheme.TextPrimary,
        Location = new Point(20, 31)
    };

    private static Control CreateMetricCard(string title, string subtitle, Label valueLabel, Color accent)
    {
        var card = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = Padding.Empty
        };

        var accentBar = new Panel
        {
            Width = 5,
            Height = 60,
            Location = new Point(8, 13),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = accent
        };
        card.Resize += (_, _) => accentBar.Height = Math.Max(20, card.ClientSize.Height - 26);
        card.Controls.Add(accentBar);

        card.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10.2F),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 12)
        });
        card.Controls.Add(valueLabel);
        card.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(21, 65)
        });
        return card;
    }

    private static Control CreateChartCard(string title, string subtitle, Control chart)
    {
        var card = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(14, 12, 14, 12)
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI Semibold", 11F),
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var subtitleLabel = new Label
        {
            Text = subtitle,
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI", 8.7F),
            ForeColor = AppTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        chart.Dock = DockStyle.Fill;
        chart.Margin = Padding.Empty;
        card.Controls.Add(chart);
        if (!string.IsNullOrWhiteSpace(subtitle))
            card.Controls.Add(subtitleLabel);
        card.Controls.Add(titleLabel);
        return card;
    }

    private async void LoadDataAsync(object? sender, EventArgs e)
    {
        try
        {
            await using var db = new AppDbContext();

            var statusCounts = await db.Devices
                .AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            static int GetCount(IReadOnlyDictionary<DeviceStatus, int> values, DeviceStatus status)
                => values.TryGetValue(status, out var count) ? count : 0;

            var total = statusCounts.Values.Sum();
            var inUse = GetCount(statusCounts, DeviceStatus.InUse);
            var available = GetCount(statusCounts, DeviceStatus.Available);
            var repair = GetCount(statusCounts, DeviceStatus.Repair);
            var broken = GetCount(statusCounts, DeviceStatus.Broken);
            var retired = GetCount(statusCounts, DeviceStatus.Retired);

            _totalValue.Text = total.ToString("N0");
            _inUseValue.Text = inUse.ToString("N0");
            _availableValue.Text = available.ToString("N0");
            _repairValue.Text = repair.ToString("N0");
            _brokenValue.Text = broken.ToString("N0");
            _retiredValue.Text = retired.ToString("N0");

            _statusChart.SetData([
                new DashboardChartItem("Đang sử dụng", inUse, AppTheme.Success),
                new DashboardChartItem("Chưa sử dụng", available, AppTheme.Info),
                new DashboardChartItem("Đang sửa chữa", repair, AppTheme.ChartOrange),
                new DashboardChartItem("Hỏng", broken, AppTheme.ChartRed),
                new DashboardChartItem("Thanh lý", retired, AppTheme.Purple)
            ]);

            var typeCounts = await db.Devices
                .AsNoTracking()
                .GroupBy(x => x.DeviceType.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(6)
                .ToListAsync();

            var barColors = new[]
            {
                AppTheme.Primary,
                AppTheme.Info,
                AppTheme.Success,
                AppTheme.Purple,
                AppTheme.Warning,
                Color.FromArgb(79, 70, 229)
            };
            _typeChart.SetData(typeCounts.Select((x, index) =>
                new DashboardChartItem(x.Name, x.Count, barColors[index % barColors.Length])));

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

            AppTheme.NormalizeGridHeaders(_grid);
            AppTheme.SetFillColumn(_grid, "Thiết_bị", 150F, 220);
            AppTheme.SetFillColumn(_grid, "Nhân_viên", 135F, 200);
            AppTheme.SetFixedColumn(_grid, "Ngày_cấp", 105, DataGridViewContentAlignment.MiddleCenter);
            AppTheme.SetFixedColumn(_grid, "Ngày_trả", 105, DataGridViewContentAlignment.MiddleCenter);
            AppTheme.SetFixedColumn(_grid, "Tình_trạng_cấp_phát", 130, DataGridViewContentAlignment.MiddleCenter);
            AppTheme.SetFixedColumn(_grid, "Trạng_thái_thiết_bị", 130, DataGridViewContentAlignment.MiddleCenter);
            AppTheme.NormalizeGridRows(_grid);

            _lastUpdatedLabel.Text = $"Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm}";
        }
        catch (Exception)
        {
            _lastUpdatedLabel.Text = "Không thể tải dữ liệu tổng quan";
            MessageBox.Show(
                "Không thể tải dữ liệu tổng quan. Vui lòng kiểm tra kết nối cơ sở dữ liệu và thử lại.",
                "Tổng quan",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
