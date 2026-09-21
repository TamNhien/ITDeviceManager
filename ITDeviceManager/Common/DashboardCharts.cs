using System.Drawing.Drawing2D;

namespace ITDeviceManager.Common;

internal readonly record struct DashboardChartItem(string Label, int Value, Color Color);

internal abstract class AnimatedChartControl : Control
{
    private readonly System.Windows.Forms.Timer _animationTimer = new() { Interval = 16 };
    protected float AnimationProgress { get; private set; } = 1F;

    protected AnimatedChartControl()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Surface;
        ForeColor = AppTheme.TextPrimary;
        Font = new Font("Segoe UI", 9F);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        _animationTimer.Tick += (_, _) =>
        {
            AnimationProgress = Math.Min(1F, AnimationProgress + 0.065F);
            Invalidate();
            if (AnimationProgress >= 1F)
                _animationTimer.Stop();
        };
    }

    protected void RestartAnimation()
    {
        AnimationProgress = 0F;
        _animationTimer.Stop();
        _animationTimer.Start();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _animationTimer.Dispose();
        base.Dispose(disposing);
    }

    protected static void PrepareGraphics(Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    }
}

/// <summary>
/// Pie chart whose slices start at the center of the circle instead of using a donut hole.
/// Small angular gaps keep each status segment visually distinct on the dark dashboard.
/// </summary>
internal sealed class PieChart : AnimatedChartControl
{
    private readonly List<DashboardChartItem> _items = [];

    public void SetData(IEnumerable<DashboardChartItem> items)
    {
        _items.Clear();
        _items.AddRange(items.Where(x => x.Value >= 0));
        RestartAnimation();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        PrepareGraphics(e.Graphics);
        e.Graphics.Clear(BackColor);

        var total = _items.Sum(x => x.Value);
        if (Width < 150 || Height < 110)
            return;

        var legendWidth = Math.Clamp((int)(Width * 0.42F), 170, 245);
        var chartArea = new Rectangle(8, 6, Math.Max(100, Width - legendWidth - 18), Math.Max(100, Height - 12));
        var diameter = Math.Max(86, Math.Min(chartArea.Width - 18, chartArea.Height - 12));
        var pieBounds = new Rectangle(
            chartArea.Left + Math.Max(0, (chartArea.Width - diameter) / 2),
            chartArea.Top + Math.Max(0, (chartArea.Height - diameter) / 2),
            diameter,
            diameter);

        if (total <= 0)
        {
            TextRenderer.DrawText(e.Graphics, "Chưa có dữ liệu", Font, chartArea, AppTheme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            return;
        }

        var visibleItems = _items.Where(x => x.Value > 0).ToList();
        var startAngle = -90F;
        const float gapDegrees = 1.1F;

        foreach (var item in visibleItems)
        {
            var rawSweep = 360F * item.Value / total;
            var animatedSweep = rawSweep * AnimationProgress;
            var drawSweep = Math.Max(0.5F, animatedSweep - gapDegrees);

            using var brush = new SolidBrush(item.Color);
            e.Graphics.FillPie(
                brush,
                pieBounds,
                startAngle + gapDegrees / 2F,
                drawSweep);

            startAngle += rawSweep;
        }

        // Crisp outer edge and center-to-edge separators make the chart read as slices.
        using (var outlinePen = new Pen(AppTheme.BorderStrong, 1.15F))
            e.Graphics.DrawEllipse(outlinePen, pieBounds);

        if (AnimationProgress >= 0.98F && visibleItems.Count > 1)
        {
            var angle = -90F;
            using var separatorPen = new Pen(AppTheme.Surface, 2.0F);
            var center = new PointF(pieBounds.Left + pieBounds.Width / 2F, pieBounds.Top + pieBounds.Height / 2F);
            foreach (var item in visibleItems.Skip(1))
            {
                angle += 360F * visibleItems[visibleItems.IndexOf(item) - 1].Value / total;
                var radians = Math.PI * angle / 180D;
                var edge = new PointF(
                    center.X + (float)Math.Cos(radians) * pieBounds.Width / 2F,
                    center.Y + (float)Math.Sin(radians) * pieBounds.Height / 2F);
                e.Graphics.DrawLine(separatorPen, center, edge);
            }
        }

        var centerLabel = new Rectangle(
            pieBounds.Left + pieBounds.Width / 4,
            pieBounds.Top + pieBounds.Height / 2 - 18,
            pieBounds.Width / 2,
            36);
        using var totalFont = new Font("Segoe UI Semibold", Math.Clamp(diameter / 11F, 13F, 22F));
        TextRenderer.DrawText(e.Graphics, total.ToString("N0"), totalFont, centerLabel, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var legendX = Width - legendWidth + 10;
        var legendTop = Math.Max(10, (Height - _items.Count * 27) / 2);
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var y = legendTop + i * 27;
            var dot = new Rectangle(legendX, y + 6, 10, 10);
            using (var dotBrush = new SolidBrush(item.Color))
                e.Graphics.FillEllipse(dotBrush, dot);

            var labelBounds = new Rectangle(legendX + 18, y, Math.Max(70, legendWidth - 76), 22);
            TextRenderer.DrawText(e.Graphics, item.Label, Font, labelBounds, AppTheme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            var valueBounds = new Rectangle(Width - 50, y, 38, 22);
            TextRenderer.DrawText(e.Graphics, item.Value.ToString("N0"), Font, valueBounds, AppTheme.TextSecondary,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }
}

/// <summary>
/// Vertical column chart inspired by the reference: dark plot area with vivid
/// pink/red -> orange/yellow gradients and values above each rounded column.
/// </summary>
internal sealed class VerticalColumnChart : AnimatedChartControl
{
    private readonly List<DashboardChartItem> _items = [];

    public void SetData(IEnumerable<DashboardChartItem> items)
    {
        _items.Clear();
        _items.AddRange(items.Where(x => x.Value >= 0).Take(6));
        RestartAnimation();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        PrepareGraphics(e.Graphics);
        e.Graphics.Clear(BackColor);

        if (_items.Count == 0)
        {
            TextRenderer.DrawText(e.Graphics, "Chưa có dữ liệu để hiển thị", Font, ClientRectangle, AppTheme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            return;
        }

        var max = Math.Max(1, _items.Max(x => x.Value));
        var left = 18;
        var right = 18;
        var top = 30;
        var bottom = 52;
        var plotWidth = Math.Max(120, Width - left - right);
        var plotHeight = Math.Max(70, Height - top - bottom);
        var baselineY = top + plotHeight;

        using (var gridPen = new Pen(AppTheme.ChartGrid, 1F))
        {
            gridPen.DashStyle = DashStyle.Dot;
            for (var i = 1; i <= 3; i++)
            {
                var y = top + plotHeight * i / 4;
                e.Graphics.DrawLine(gridPen, left, y, left + plotWidth, y);
            }
        }

        var slotWidth = plotWidth / (float)_items.Count;
        var barWidth = Math.Clamp((int)(slotWidth * 0.56F), 28, 86);
        using var valueFont = new Font("Segoe UI Semibold", 9F);
        using var labelFont = new Font("Segoe UI", 8.5F);

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var centerX = left + slotWidth * i + slotWidth / 2F;
            var targetHeight = (float)(plotHeight * 0.82D * (item.Value / (double)max));
            var animatedHeight = Math.Max(item.Value > 0 ? 3F : 0F, targetHeight * AnimationProgress);
            var barRect = new RectangleF(
                centerX - barWidth / 2F,
                baselineY - animatedHeight,
                barWidth,
                animatedHeight);

            if (item.Value > 0 && barRect.Height > 1F)
            {
                using var path = RoundedTopRect(barRect, Math.Min(12F, barWidth / 3F));
                using var gradient = new LinearGradientBrush(
                    barRect,
                    AppTheme.ChartYellow,
                    AppTheme.ChartPink,
                    LinearGradientMode.Vertical);
                var blend = new ColorBlend
                {
                    Colors = [AppTheme.ChartYellow, AppTheme.ChartOrange, AppTheme.ChartRed, AppTheme.ChartPink],
                    Positions = [0F, 0.28F, 0.55F, 1F]
                };
                gradient.InterpolationColors = blend;
                e.Graphics.FillPath(gradient, path);

                using var glowPen = new Pen(Color.FromArgb(90, AppTheme.ChartYellow), 1F);
                e.Graphics.DrawPath(glowPen, path);
            }

            var valueRect = new Rectangle(
                (int)(centerX - slotWidth / 2F),
                Math.Max(1, (int)(barRect.Top - 25)),
                Math.Max(1, (int)slotWidth),
                22);
            TextRenderer.DrawText(e.Graphics, item.Value.ToString("N0"), valueFont, valueRect, AppTheme.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            var labelRect = new Rectangle(
                (int)(centerX - slotWidth / 2F + 3),
                baselineY + 9,
                Math.Max(1, (int)slotWidth - 6),
                bottom - 10);
            TextRenderer.DrawText(e.Graphics, item.Label, labelFont, labelRect, AppTheme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }

    private static GraphicsPath RoundedTopRect(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 1F || rect.Height <= 1F)
            return path;

        radius = Math.Clamp(radius, 1F, Math.Min(rect.Width / 2F, rect.Height / 2F));
        var d = radius * 2F;
        path.StartFigure();
        path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top + radius);
        path.AddArc(rect.Left, rect.Top, d, d, 180F, 90F);
        path.AddLine(rect.Left + radius, rect.Top, rect.Right - radius, rect.Top);
        path.AddArc(rect.Right - d, rect.Top, d, d, 270F, 90F);
        path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom);
        path.CloseFigure();
        return path;
    }
}
