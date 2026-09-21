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
            AnimationProgress = Math.Min(1F, AnimationProgress + 0.075F);
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

internal sealed class DonutChart : AnimatedChartControl
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
        if (Width < 120 || Height < 100)
            return;

        var legendWidth = Math.Clamp((int)(Width * 0.43F), 150, 230);
        var chartArea = new Rectangle(4, 4, Math.Max(90, Width - legendWidth - 12), Math.Max(90, Height - 8));
        var diameter = Math.Max(72, Math.Min(chartArea.Width - 16, chartArea.Height - 18));
        var ringBounds = new Rectangle(
            chartArea.Left + Math.Max(0, (chartArea.Width - diameter) / 2),
            chartArea.Top + Math.Max(0, (chartArea.Height - diameter) / 2),
            diameter,
            diameter);

        var ringWidth = Math.Clamp(diameter / 9, 14, 24);
        using (var trackPen = new Pen(Color.FromArgb(238, 242, 247), ringWidth))
        {
            trackPen.StartCap = LineCap.Round;
            trackPen.EndCap = LineCap.Round;
            e.Graphics.DrawArc(trackPen, ringBounds, -90, 359.8F);
        }

        if (total > 0)
        {
            var startAngle = -90F;
            var visibleItems = _items.Where(x => x.Value > 0).ToList();
            var gap = visibleItems.Count > 1 ? 2.2F : 0F;
            foreach (var item in visibleItems)
            {
                var rawSweep = 360F * item.Value / total;
                var sweep = Math.Max(0.4F, rawSweep - gap);
                using var pen = new Pen(item.Color, ringWidth)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                e.Graphics.DrawArc(pen, ringBounds, startAngle + gap / 2F, sweep * AnimationProgress);
                startAngle += rawSweep;
            }
        }

        var centerBox = new Rectangle(
            ringBounds.Left + ringWidth,
            ringBounds.Top + ringWidth,
            Math.Max(1, ringBounds.Width - ringWidth * 2),
            Math.Max(1, ringBounds.Height - ringWidth * 2));

        using var totalFont = new Font("Segoe UI Semibold", Math.Clamp(diameter / 8F, 16F, 25F), FontStyle.Regular, GraphicsUnit.Point);
        using var captionFont = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        var numberBounds = new Rectangle(centerBox.Left, centerBox.Top + centerBox.Height / 2 - 26, centerBox.Width, 34);
        TextRenderer.DrawText(e.Graphics, total.ToString("N0"), totalFont, numberBounds, AppTheme.TextPrimary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        var captionBounds = new Rectangle(centerBox.Left, numberBounds.Bottom - 2, centerBox.Width, 24);
        TextRenderer.DrawText(e.Graphics, "thiết bị", captionFont, captionBounds, AppTheme.TextSecondary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var legendX = Width - legendWidth + 8;
        var legendTop = Math.Max(8, (Height - _items.Count * 27) / 2);
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var y = legendTop + i * 27;
            var dot = new Rectangle(legendX, y + 6, 10, 10);
            using (var dotBrush = new SolidBrush(item.Color))
            using (var dotPath = AppTheme.RoundedPath(dot, 4))
                e.Graphics.FillPath(dotBrush, dotPath);

            var labelBounds = new Rectangle(legendX + 18, y, Math.Max(70, legendWidth - 70), 22);
            TextRenderer.DrawText(e.Graphics, item.Label, Font, labelBounds, AppTheme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            var valueBounds = new Rectangle(Width - 48, y, 38, 22);
            TextRenderer.DrawText(e.Graphics, item.Value.ToString("N0"), Font, valueBounds, AppTheme.TextSecondary,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }
}

internal sealed class HorizontalBarChart : AnimatedChartControl
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
        var top = 10;
        var bottom = 8;
        var rowHeight = Math.Max(28, (Height - top - bottom) / _items.Count);
        var labelWidth = Math.Clamp((int)(Width * 0.27F), 110, 175);
        var valueWidth = 42;
        var trackLeft = labelWidth + 10;
        var trackWidth = Math.Max(80, Width - trackLeft - valueWidth - 12);
        var barHeight = Math.Clamp(rowHeight - 16, 10, 17);

        using var valueFont = new Font("Segoe UI Semibold", 9F);
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var rowTop = top + i * rowHeight;
            var labelRect = new Rectangle(4, rowTop, labelWidth - 4, rowHeight);
            TextRenderer.DrawText(e.Graphics, item.Label, Font, labelRect, AppTheme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            var trackTop = rowTop + (rowHeight - barHeight) / 2;
            var trackRect = new Rectangle(trackLeft, trackTop, trackWidth, barHeight);
            using (var trackBrush = new SolidBrush(Color.FromArgb(239, 243, 248)))
            using (var trackPath = AppTheme.RoundedPath(trackRect, Math.Max(2, barHeight / 2)))
                e.Graphics.FillPath(trackBrush, trackPath);

            var targetWidth = (int)Math.Round(trackWidth * (item.Value / (double)max) * AnimationProgress);
            if (item.Value > 0 && targetWidth > 0)
            {
                var fillRect = new Rectangle(trackLeft, trackTop, Math.Max(2, targetWidth), barHeight);
                var radius = Math.Max(1, Math.Min(barHeight / 2, fillRect.Width / 2));
                using var fillPath = AppTheme.RoundedPath(fillRect, radius);
                using var fillBrush = new LinearGradientBrush(fillRect, item.Color, AppTheme.Blend(item.Color, Color.White, 0.28F), LinearGradientMode.Horizontal);
                e.Graphics.FillPath(fillBrush, fillPath);
            }

            var valueRect = new Rectangle(Width - valueWidth - 4, rowTop, valueWidth, rowHeight);
            TextRenderer.DrawText(e.Graphics, item.Value.ToString("N0"), valueFont, valueRect, AppTheme.TextPrimary,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }
}
