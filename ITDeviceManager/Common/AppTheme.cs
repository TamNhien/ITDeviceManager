using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace ITDeviceManager.Common;

public enum ButtonRole
{
    Primary,
    Secondary,
    Danger,
    Warning,
    Navigation,
    NavigationActive
}

public static class AppTheme
{
    public const int InputHeight = 30;
    public const int GridRowHeight = 38;
    private static readonly Padding InputMargin = new(3, 4, 3, 4);

    // V1.7.2: dark analytics palette inspired by the reference dashboard.
    public static readonly Color Background = Color.FromArgb(5, 9, 19);
    public static readonly Color Surface = Color.FromArgb(10, 15, 29);
    public static readonly Color SurfaceAlt = Color.FromArgb(13, 20, 36);
    public static readonly Color Border = Color.FromArgb(31, 42, 60);
    public static readonly Color BorderStrong = Color.FromArgb(52, 65, 86);
    public static readonly Color TextPrimary = Color.FromArgb(241, 245, 249);
    public static readonly Color TextSecondary = Color.FromArgb(148, 163, 184);
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryHover = Color.FromArgb(59, 130, 246);
    public static readonly Color PrimaryPressed = Color.FromArgb(29, 78, 216);
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);
    public static readonly Color DangerHover = Color.FromArgb(220, 38, 38);
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);
    public static readonly Color WarningHover = Color.FromArgb(217, 119, 6);
    public static readonly Color Success = Color.FromArgb(16, 185, 129);
    public static readonly Color Info = Color.FromArgb(14, 165, 233);
    public static readonly Color Purple = Color.FromArgb(139, 92, 246);
    public static readonly Color Sidebar = Color.FromArgb(5, 10, 21);
    public static readonly Color SidebarHover = Color.FromArgb(15, 23, 42);
    public static readonly Color SidebarActive = Color.FromArgb(30, 64, 175);
    public static readonly Color InputFocus = Color.FromArgb(17, 28, 50);
    public static readonly Color GridSelection = Color.FromArgb(24, 50, 82);
    public static readonly Color ChartGrid = Color.FromArgb(28, 39, 58);
    public static readonly Color ChartPink = Color.FromArgb(255, 0, 92);
    public static readonly Color ChartRed = Color.FromArgb(255, 35, 77);
    public static readonly Color ChartOrange = Color.FromArgb(255, 111, 60);
    public static readonly Color ChartYellow = Color.FromArgb(255, 211, 52);

    private sealed class ButtonState
    {
        public ButtonRole Role;
        public Color Current;
        public Color Target;
        public readonly System.Windows.Forms.Timer Timer = new() { Interval = 15 };
        public bool Hooked;
    }

    private static readonly ConditionalWeakTable<Button, ButtonState> ButtonStates = new();
    private static readonly ConditionalWeakTable<ComboBox, object> StyledComboBoxes = new();
    private static readonly ConditionalWeakTable<DataGridView, object> StyledGrids = new();

    public static void ApplyForm(Form form)
    {
        form.BackColor = Background;
        form.ForeColor = TextPrimary;
        form.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        ApplyTo(form);
    }

    public static void ApplyTo(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case Button button when button.Parent is not PasswordInput:
                    StyleButton(button);
                    break;
                case TextBox textBox when textBox.Parent is not PasswordInput && textBox.Parent is not TextInput:
                    StyleTextBox(textBox);
                    break;
                case ComboBox comboBox:
                    StyleComboBox(comboBox);
                    break;
                case DateTimePicker dateTimePicker:
                    dateTimePicker.Font = new Font("Segoe UI", 10F);
                    dateTimePicker.CalendarFont = new Font("Segoe UI", 10F);
                    dateTimePicker.BackColor = Surface;
                    dateTimePicker.ForeColor = TextPrimary;
                    dateTimePicker.Height = InputHeight;
                    dateTimePicker.Margin = InputMargin;
                    break;
                case NumericUpDown numericUpDown:
                    numericUpDown.Font = new Font("Segoe UI", 10F);
                    numericUpDown.BackColor = Surface;
                    numericUpDown.ForeColor = TextPrimary;
                    numericUpDown.BorderStyle = BorderStyle.FixedSingle;
                    numericUpDown.Height = InputHeight;
                    numericUpDown.Margin = InputMargin;
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case LinkLabel link:
                    link.LinkColor = Primary;
                    link.ActiveLinkColor = PrimaryPressed;
                    link.VisitedLinkColor = Primary;
                    link.Cursor = Cursors.Hand;
                    break;
                case CheckBox checkBox:
                    checkBox.ForeColor = TextPrimary;
                    break;
                case FlowLayoutPanel flow when flow.Dock is DockStyle.Top or DockStyle.Bottom:
                    flow.BackColor = Surface;
                    break;
            }

            if (control.HasChildren)
                ApplyTo(control);
        }
    }

    public static ButtonRole InferButtonRole(string text)
    {
        var value = (text ?? string.Empty).Trim().ToLowerInvariant();
        if (value.Contains("xóa") || value.Contains("đăng xuất")) return ButtonRole.Danger;
        if (value.Contains("thu hồi")) return ButtonRole.Warning;
        if (value is "hủy" or "làm mới" or "đóng") return ButtonRole.Secondary;
        return ButtonRole.Primary;
    }

    public static void SetButtonRole(Button button, ButtonRole role)
    {
        var state = ButtonStates.GetValue(button, static _ => new ButtonState());
        state.Role = role;
        EnsureButtonHooks(button, state);
        ApplyButtonPalette(button, state, immediate: true);
    }

    public static void StyleButton(Button button)
    {
        var state = ButtonStates.GetValue(button, static _ => new ButtonState());
        if (!state.Hooked)
            state.Role = InferButtonRole(button.Text);
        EnsureButtonHooks(button, state);
        ApplyButtonPalette(button, state, immediate: true);
    }

    private static void EnsureButtonHooks(Button button, ButtonState state)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        button.MinimumSize = new Size(0, 36);
        button.Padding = new Padding(10, 0, 10, 0);

        if (state.Hooked) return;
        state.Hooked = true;

        state.Timer.Tick += (_, _) =>
        {
            state.Current = Blend(state.Current, state.Target, 0.24f);
            button.BackColor = state.Current;
            if (ColorDistance(state.Current, state.Target) < 4)
            {
                state.Current = state.Target;
                button.BackColor = state.Target;
                state.Timer.Stop();
            }
        };

        button.MouseEnter += (_, _) => AnimateTo(button, state, Palette(state.Role).hover);
        button.MouseLeave += (_, _) => AnimateTo(button, state, Palette(state.Role).normal);
        button.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                AnimateTo(button, state, Palette(state.Role).pressed);
        };
        button.MouseUp += (_, _) => AnimateTo(button, state,
            button.ClientRectangle.Contains(button.PointToClient(Cursor.Position))
                ? Palette(state.Role).hover
                : Palette(state.Role).normal);
        button.Paint += (_, e) => DrawRoundedButton(button, e);
        button.Disposed += (_, _) => state.Timer.Dispose();
    }

    private static void ApplyButtonPalette(Button button, ButtonState state, bool immediate)
    {
        var palette = Palette(state.Role);
        button.ForeColor = palette.fore;
        button.TextAlign = state.Role is ButtonRole.Navigation or ButtonRole.NavigationActive
            ? ContentAlignment.MiddleLeft
            : ContentAlignment.MiddleCenter;

        if (state.Role is ButtonRole.Navigation or ButtonRole.NavigationActive)
            button.Padding = new Padding(18, 0, 12, 0);

        state.Target = palette.normal;
        if (immediate)
        {
            state.Timer.Stop();
            state.Current = palette.normal;
            button.BackColor = palette.normal;
        }
        // Do not clip the native HWND with Region: Region edges are pixel-aligned and
        // can look jagged on high-DPI displays. The button is painted with anti-aliasing instead.
        button.Region?.Dispose();
        button.Region = null;
        button.Invalidate();
    }

    private static void DrawRoundedButton(Button button, PaintEventArgs e)
    {
        if (button.Width <= 1 || button.Height <= 1) return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

        var parentBackColor = ResolveParentBackColor(button);
        e.Graphics.Clear(parentBackColor);

        var bounds = new Rectangle(1, 1, Math.Max(1, button.Width - 3), Math.Max(1, button.Height - 3));
        using var path = RoundedPath(bounds, 9);
        var fill = button.Enabled ? button.BackColor : Color.FromArgb(51, 65, 85);
        using var brush = new SolidBrush(fill);
        e.Graphics.FillPath(brush, path);

        var textColor = button.Enabled ? button.ForeColor : TextSecondary;
        var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
        flags |= button.TextAlign is ContentAlignment.MiddleLeft or ContentAlignment.TopLeft or ContentAlignment.BottomLeft
            ? TextFormatFlags.Left
            : TextFormatFlags.HorizontalCenter;

        var textBounds = new Rectangle(
            Math.Max(6, button.Padding.Left),
            0,
            Math.Max(1, button.Width - Math.Max(6, button.Padding.Left) - Math.Max(6, button.Padding.Right)),
            button.Height);
        TextRenderer.DrawText(e.Graphics, button.Text, button.Font, textBounds, textColor, flags);
    }

    private static Color ResolveParentBackColor(Control control)
    {
        var current = control.Parent;
        while (current is not null)
        {
            if (current.BackColor != Color.Transparent)
                return current.BackColor;
            current = current.Parent;
        }
        return Background;
    }

    private static (Color normal, Color hover, Color pressed, Color fore) Palette(ButtonRole role) => role switch
    {
        ButtonRole.Secondary => (Color.FromArgb(24, 34, 52), Color.FromArgb(35, 48, 70), Color.FromArgb(47, 61, 84), TextPrimary),
        ButtonRole.Danger => (Danger, DangerHover, Color.FromArgb(153, 27, 27), Color.White),
        ButtonRole.Warning => (Warning, WarningHover, Color.FromArgb(146, 64, 14), Color.White),
        ButtonRole.Navigation => (Sidebar, SidebarHover, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225)),
        ButtonRole.NavigationActive => (SidebarActive, PrimaryHover, PrimaryPressed, Color.White),
        _ => (Primary, PrimaryHover, PrimaryPressed, Color.White)
    };

    private static void AnimateTo(Button button, ButtonState state, Color target)
    {
        if (button.IsDisposed) return;
        state.Target = target;
        if (!state.Timer.Enabled)
            state.Timer.Start();
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = Surface;
        textBox.ForeColor = TextPrimary;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = new Font("Segoe UI", 10F);

        // Single-line inputs use one shared visual height so filter bars do not
        // look uneven next to ComboBox controls on Windows/DPI scaling.
        if (!textBox.Multiline)
        {
            textBox.AutoSize = false;
            textBox.Height = InputHeight;
            textBox.MinimumSize = new Size(0, InputHeight);
            textBox.Margin = InputMargin;
        }

        textBox.Enter += (_, _) => textBox.BackColor = InputFocus;
        textBox.Leave += (_, _) => textBox.BackColor = Surface;
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = Surface;
        comboBox.ForeColor = TextPrimary;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = new Font("Segoe UI", 10F);
        comboBox.Margin = InputMargin;

        // OwnerDrawFixed gives DropDownList a stable item/control height.
        // This keeps TextBox and ComboBox controls visually level in filter bars.
        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        comboBox.ItemHeight = 24;
        comboBox.Height = InputHeight;
        comboBox.MinimumSize = new Size(0, InputHeight);

        if (!StyledComboBoxes.TryGetValue(comboBox, out _))
        {
            StyledComboBoxes.Add(comboBox, new object());
            comboBox.DrawItem += (_, e) => DrawComboBoxItem(comboBox, e);
        }
    }

    private static void DrawComboBoxItem(ComboBox comboBox, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var background = selected ? GridSelection : Surface;
        var foreground = TextPrimary;

        using var backgroundBrush = new SolidBrush(background);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

        var text = comboBox.GetItemText(comboBox.Items[e.Index]);
        var textBounds = new Rectangle(
            e.Bounds.X + 7,
            e.Bounds.Y,
            Math.Max(0, e.Bounds.Width - 14),
            e.Bounds.Height);

        TextRenderer.DrawText(
            e.Graphics,
            text,
            comboBox.Font,
            textBounds,
            foreground,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        e.DrawFocusRectangle();
    }

    public static void StyleGrid(DataGridView grid)
    {
        // V1.6.2: use subtle full grid lines so rows/columns are easier to scan
        // without returning to the heavy classic WinForms table look.
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.BackgroundColor = Surface;
        grid.GridColor = Border;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = GridRowHeight;
        grid.RowTemplate.MinimumHeight = GridRowHeight;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = GridSelection;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
        grid.ColumnHeadersDefaultCellStyle.Padding = Padding.Empty;
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

        if (!StyledGrids.TryGetValue(grid, out _))
        {
            StyledGrids.Add(grid, new object());
            grid.DataBindingComplete += (_, _) =>
            {
                NormalizeGridHeaders(grid);
                NormalizeGridRows(grid);
            };
            grid.ColumnAdded += (_, _) => NormalizeGridHeaders(grid);
            grid.CellPainting += (_, e) => PaintGridHeaderCell(grid, e);
            grid.RowsAdded += (_, e) =>
            {
                var last = Math.Min(grid.Rows.Count - 1, e.RowIndex + e.RowCount - 1);
                for (var i = Math.Max(0, e.RowIndex); i <= last; i++)
                {
                    var row = grid.Rows[i];
                    if (row.IsNewRow) continue;
                    row.MinimumHeight = Math.Max(row.MinimumHeight, GridRowHeight);
                    if (grid.AutoSizeRowsMode == DataGridViewAutoSizeRowsMode.None && row.Height < GridRowHeight)
                        row.Height = GridRowHeight;
                }
            };
        }

        NormalizeGridHeaders(grid);
        NormalizeGridRows(grid);
    }

    private static void PaintGridHeaderCell(DataGridView grid, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex != -1 || e.ColumnIndex < 0) return;

        // WinForms reserves space on the right side of sortable headers for the
        // sort glyph. With native painting that makes MiddleCenter look slightly
        // left-shifted even though the Alignment property is correct. Paint the
        // header text ourselves so it is geometrically centered in the whole cell.
        var graphics = e.Graphics;
        if (graphics is null) return;

        var bounds = e.CellBounds;
        var headerStyle = grid.ColumnHeadersDefaultCellStyle;
        var backColor = headerStyle.BackColor.IsEmpty
            ? SurfaceAlt
            : headerStyle.BackColor;
        var foreColor = headerStyle.ForeColor.IsEmpty
            ? TextPrimary
            : headerStyle.ForeColor;

        using (var background = new SolidBrush(backColor))
            graphics.FillRectangle(background, bounds);

        using (var borderPen = new Pen(grid.GridColor))
        {
            var border = new Rectangle(bounds.X, bounds.Y, Math.Max(0, bounds.Width - 1), Math.Max(0, bounds.Height - 1));
            graphics.DrawRectangle(borderPen, border);
        }

        var text = grid.Columns[e.ColumnIndex].HeaderText ?? string.Empty;
        var font = headerStyle.Font ?? grid.Font;
        var textBounds = Rectangle.Inflate(bounds, -4, -2);
        TextRenderer.DrawText(
            graphics,
            text,
            font,
            textBounds,
            foreColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.WordBreak |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);

        e.Handled = true;
    }

    public static void NormalizeGridRows(DataGridView grid, int minimumHeight = GridRowHeight)
    {
        minimumHeight = Math.Max(22, minimumHeight);
        grid.RowTemplate.MinimumHeight = Math.Max(grid.RowTemplate.MinimumHeight, minimumHeight);
        grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, minimumHeight);

        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.IsNewRow) continue;
            row.MinimumHeight = Math.Max(row.MinimumHeight, minimumHeight);
            if (grid.AutoSizeRowsMode == DataGridViewAutoSizeRowsMode.None && row.Height < minimumHeight)
                row.Height = minimumHeight;
        }
    }

    public static void NormalizeGridHeaders(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (!string.IsNullOrWhiteSpace(column.HeaderText))
                column.HeaderText = column.HeaderText.Replace('_', ' ');
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Padding = Padding.Empty;

            // Keep short-value columns compact instead of giving every generated
            // column the same Fill share. Form-specific layouts can override this.
            if (column.AutoSizeMode is DataGridViewAutoSizeColumnMode.NotSet or DataGridViewAutoSizeColumnMode.Fill)
            {
                var header = (column.HeaderText ?? string.Empty).Trim().ToLowerInvariant();
                var compact = header.StartsWith("mã") ||
                              header.Contains("ngày") ||
                              header.Contains("chi phí") ||
                              header.Contains("giá mua") ||
                              header.Contains("trạng thái") ||
                              header.Contains("tình trạng") ||
                              header is "serial" or "quyền" or "hoạt động";
                if (compact)
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    column.MinimumWidth = 72;
                }
            }
        }
    }

    public static void SetFixedColumn(DataGridView grid, string name, int width, DataGridViewContentAlignment alignment = DataGridViewContentAlignment.MiddleLeft)
    {
        if (grid.Columns[name] is not { } column) return;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        column.Width = width;
        column.MinimumWidth = Math.Min(width, 60);
        column.DefaultCellStyle.Alignment = alignment;
        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    public static void SetFillColumn(DataGridView grid, string name, float fillWeight = 100F, int minimumWidth = 120, bool wrap = false)
    {
        if (grid.Columns[name] is not { } column) return;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        column.FillWeight = fillWeight;
        column.MinimumWidth = minimumWidth;
        column.DefaultCellStyle.WrapMode = wrap ? DataGridViewTriState.True : DataGridViewTriState.False;
        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    public static void ApplyRoundedRegion(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0) return;
        using var path = RoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
        control.Region?.Dispose();
        control.Region = new Region(path);
    }

    public static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var diameter = Math.Max(2, radius * 2);
        var rect = new Rectangle(bounds.X, bounds.Y, Math.Max(1, bounds.Width - 1), Math.Max(1, bounds.Height - 1));
        var path = new GraphicsPath();
        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static Color Blend(Color from, Color to, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return Color.FromArgb(
            (int)(from.A + (to.A - from.A) * amount),
            (int)(from.R + (to.R - from.R) * amount),
            (int)(from.G + (to.G - from.G) * amount),
            (int)(from.B + (to.B - from.B) * amount));
    }

    private static int ColorDistance(Color a, Color b)
        => Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
}

public sealed class ModernCard : Panel
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = AppTheme.Border;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 14;

    public ModernCard()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = AppTheme.Surface;
        Padding = new Padding(1);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
        e.Graphics.Clear(Parent?.BackColor ?? AppTheme.Background);

        if (Width <= 1 || Height <= 1) return;
        using var path = AppTheme.RoundedPath(new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3)), CornerRadius);
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillPath(brush, path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width <= 1 || Height <= 1) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using var path = AppTheme.RoundedPath(new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3)), CornerRadius);
        using var pen = new Pen(BorderColor, 1F);
        e.Graphics.DrawPath(pen, path);
    }
}
