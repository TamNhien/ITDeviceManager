using System.ComponentModel;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    // V1.7.8: dark input chrome avoids native themed borders/background erase passes.
    // TextBox borders are custom-painted; ComboBox popup erases dark before item paint;
    // DateTimePicker and its MonthCalendar use explicit dark painting/colors.
    public static readonly Color Background = Color.FromArgb(8, 14, 26);
    public static readonly Color Surface = Color.FromArgb(14, 22, 38);
    public static readonly Color SurfaceAlt = Color.FromArgb(18, 28, 48);
    // Input surfaces are intentionally a little lighter than the application background/surfaces.
    // This gives fields, drop-down lists and date controls a softer, clearly interactive layer
    // without bringing back the bright native-control patches of the light theme.
    public static readonly Color InputSurface = Color.FromArgb(22, 34, 54);
    public static readonly Color InputDropDown = Color.FromArgb(25, 39, 61);
    public static readonly Color Border = Color.FromArgb(42, 56, 78);
    public static readonly Color BorderStrong = Color.FromArgb(58, 74, 100);
    public static readonly Color TextPrimary = Color.FromArgb(226, 232, 240);
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
    public static readonly Color Sidebar = Color.FromArgb(7, 13, 25);
    public static readonly Color SidebarHover = Color.FromArgb(18, 28, 48);
    public static readonly Color SidebarActive = Color.FromArgb(30, 64, 175);
    public static readonly Color InputFocus = Color.FromArgb(31, 47, 72);
    public static readonly Color GridSelection = Color.FromArgb(28, 48, 76);
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
    private static readonly ConditionalWeakTable<TextBox, object> StyledTextBoxes = new();
    private static readonly ConditionalWeakTable<TextBox, DarkTextBoxChrome> TextBoxChrome = new();
    private static readonly ConditionalWeakTable<ComboBox, object> StyledComboBoxes = new();
    private static readonly ConditionalWeakTable<ComboBox, DarkComboBoxChrome> ComboBoxChrome = new();
    private static readonly ConditionalWeakTable<DateTimePicker, object> StyledDatePickers = new();
    private static readonly ConditionalWeakTable<DateTimePicker, DarkDateTimePickerChrome> DatePickerChrome = new();
    private static readonly ConditionalWeakTable<NumericUpDown, object> StyledNumericInputs = new();
    private static readonly ConditionalWeakTable<NumericUpDown, DarkNumericChrome> NumericChrome = new();
    private static readonly ConditionalWeakTable<DataGridView, object> StyledGrids = new();

    private const int EmSetRectNp = 0x00B4;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref NativeRect lParam);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string? pszSubAppName, string? pszSubIdList);

    private const int WmPaint = 0x000F;
    private const int WmEraseBkgnd = 0x0014;
    private const int WmNcPaint = 0x0085;
    private const int WmPrintClient = 0x0318;
    private const int WmSetFocus = 0x0007;
    private const int WmKillFocus = 0x0008;
    private const int DtmGetMonthCal = 0x1008;
    private const int McmSetColor = 0x100A;
    private const int McscBackground = 0;
    private const int McscText = 1;
    private const int McscTitleBk = 2;
    private const int McscTitleText = 3;
    private const int McscMonthBk = 4;
    private const int McscTrailingText = 5;

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintStruct
    {
        public IntPtr Hdc;
        [MarshalAs(UnmanagedType.Bool)] public bool Erase;
        public NativeRect PaintRect;
        [MarshalAs(UnmanagedType.Bool)] public bool Restore;
        [MarshalAs(UnmanagedType.Bool)] public bool IncUpdate;
        public int Reserved1;
        public int Reserved2;
        public int Reserved3;
        public int Reserved4;
        public int Reserved5;
        public int Reserved6;
        public int Reserved7;
        public int Reserved8;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ComboBoxInfo
    {
        public int Size;
        public NativeRect ItemRect;
        public NativeRect ButtonRect;
        public int ButtonState;
        public IntPtr ComboHandle;
        public IntPtr ItemHandle;
        public IntPtr ListHandle;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetComboBoxInfo(IntPtr hwndCombo, ref ComboBoxInfo info);

    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hwnd, out PaintStruct paint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EndPaint(IntPtr hwnd, ref PaintStruct paint);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessagePtr(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ValidateRect(IntPtr hWnd, IntPtr lpRect);

    private sealed class DarkTextBoxChrome : NativeWindow, IDisposable
    {
        private readonly TextBox _owner;

        public DarkTextBoxChrome(TextBox owner)
        {
            _owner = owner;
            Attach();
            _owner.HandleCreated += OwnerHandleCreated;
            _owner.HandleDestroyed += OwnerHandleDestroyed;
            _owner.SizeChanged += OwnerVisualChanged;
            _owner.EnabledChanged += OwnerVisualChanged;
            _owner.GotFocus += OwnerVisualChanged;
            _owner.LostFocus += OwnerVisualChanged;
        }

        private void OwnerHandleCreated(object? sender, EventArgs e) => Attach();
        private void OwnerHandleDestroyed(object? sender, EventArgs e) => ReleaseHandle();
        private void OwnerVisualChanged(object? sender, EventArgs e) => _owner.Invalidate();

        private void Attach()
        {
            if (_owner.IsHandleCreated && Handle != _owner.Handle)
            {
                if (Handle != IntPtr.Zero) ReleaseHandle();
                AssignHandle(_owner.Handle);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmEraseBkgnd)
            {
                if (m.WParam != IntPtr.Zero)
                {
                    using var g = Graphics.FromHdc(m.WParam);
                    g.Clear(_owner.Enabled ? InputSurface : SurfaceAlt);
                }
                m.Result = (IntPtr)1;
                return;
            }

            if (m.Msg == WmNcPaint)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg is WmPaint or WmSetFocus or WmKillFocus)
                DrawBorder();
        }

        private void DrawBorder()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated || _owner.Width <= 1 || _owner.Height <= 1) return;
            using var g = Graphics.FromHwnd(_owner.Handle);
            using var pen = new Pen(_owner.Focused ? PrimaryHover : BorderStrong, 1f);
            g.DrawRectangle(pen, 0, 0, Math.Max(0, _owner.ClientSize.Width - 1), Math.Max(0, _owner.ClientSize.Height - 1));
        }

        public void Dispose()
        {
            _owner.HandleCreated -= OwnerHandleCreated;
            _owner.HandleDestroyed -= OwnerHandleDestroyed;
            _owner.SizeChanged -= OwnerVisualChanged;
            _owner.EnabledChanged -= OwnerVisualChanged;
            _owner.GotFocus -= OwnerVisualChanged;
            _owner.LostFocus -= OwnerVisualChanged;
            if (Handle != IntPtr.Zero) ReleaseHandle();
        }
    }

    private sealed class DarkNumericChrome : NativeWindow, IDisposable
    {
        private readonly NumericUpDown _owner;

        public DarkNumericChrome(NumericUpDown owner)
        {
            _owner = owner;
            Attach();
            _owner.HandleCreated += OwnerHandleCreated;
            _owner.HandleDestroyed += OwnerHandleDestroyed;
            _owner.SizeChanged += OwnerVisualChanged;
            _owner.EnabledChanged += OwnerVisualChanged;
            _owner.GotFocus += OwnerVisualChanged;
            _owner.LostFocus += OwnerVisualChanged;
        }

        private void OwnerHandleCreated(object? sender, EventArgs e) => Attach();
        private void OwnerHandleDestroyed(object? sender, EventArgs e) => ReleaseHandle();
        private void OwnerVisualChanged(object? sender, EventArgs e) => _owner.Invalidate();

        private void Attach()
        {
            if (_owner.IsHandleCreated && Handle != _owner.Handle)
            {
                if (Handle != IntPtr.Zero) ReleaseHandle();
                AssignHandle(_owner.Handle);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmEraseBkgnd)
            {
                if (m.WParam != IntPtr.Zero)
                {
                    using var g = Graphics.FromHdc(m.WParam);
                    g.Clear(_owner.Enabled ? InputSurface : SurfaceAlt);
                }
                m.Result = (IntPtr)1;
                return;
            }

            if (m.Msg == WmNcPaint)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
            if (m.Msg is WmPaint or WmSetFocus or WmKillFocus)
                DrawBorder();
        }

        private void DrawBorder()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated || _owner.Width <= 1 || _owner.Height <= 1) return;
            using var g = Graphics.FromHwnd(_owner.Handle);
            using var pen = new Pen(_owner.Focused ? PrimaryHover : BorderStrong, 1f);
            g.DrawRectangle(pen, 0, 0, Math.Max(0, _owner.ClientSize.Width - 1), Math.Max(0, _owner.ClientSize.Height - 1));
        }

        public void Dispose()
        {
            _owner.HandleCreated -= OwnerHandleCreated;
            _owner.HandleDestroyed -= OwnerHandleDestroyed;
            _owner.SizeChanged -= OwnerVisualChanged;
            _owner.EnabledChanged -= OwnerVisualChanged;
            _owner.GotFocus -= OwnerVisualChanged;
            _owner.LostFocus -= OwnerVisualChanged;
            if (Handle != IntPtr.Zero) ReleaseHandle();
        }
    }

    private sealed class DarkComboListChrome : NativeWindow, IDisposable
    {
        private IntPtr _listHandle;

        public void AttachTo(IntPtr listHandle)
        {
            if (listHandle == IntPtr.Zero || listHandle == _listHandle) return;
            if (Handle != IntPtr.Zero) ReleaseHandle();
            _listHandle = listHandle;
            _ = SetWindowTheme(listHandle, string.Empty, string.Empty);
            AssignHandle(listHandle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmEraseBkgnd)
            {
                if (m.WParam != IntPtr.Zero)
                {
                    using var g = Graphics.FromHdc(m.WParam);
                    g.Clear(InputDropDown);
                }
                m.Result = (IntPtr)1;
                return;
            }

            if (m.Msg == WmNcPaint)
            {
                DrawBorder();
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WmPaint)
                DrawBorder();
        }

        private void DrawBorder()
        {
            if (_listHandle == IntPtr.Zero) return;
            using var g = Graphics.FromHwnd(_listHandle);
            using var pen = new Pen(BorderStrong, 1f);
            var bounds = Rectangle.Round(g.VisibleClipBounds);
            if (bounds.Width > 1 && bounds.Height > 1)
                g.DrawRectangle(pen, 0, 0, bounds.Width - 1, bounds.Height - 1);
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero) ReleaseHandle();
            _listHandle = IntPtr.Zero;
        }
    }

    private sealed class DarkComboBoxChrome : NativeWindow, IDisposable
    {
        private readonly ComboBox _owner;
        private readonly DarkComboListChrome _listChrome = new();

        public DarkComboBoxChrome(ComboBox owner)
        {
            _owner = owner;
            Attach();
            _owner.HandleCreated += OwnerHandleCreated;
            _owner.HandleDestroyed += OwnerHandleDestroyed;
            _owner.SizeChanged += OwnerVisualChanged;
            _owner.EnabledChanged += OwnerVisualChanged;
            _owner.GotFocus += OwnerVisualChanged;
            _owner.LostFocus += OwnerVisualChanged;
            _owner.SelectedIndexChanged += OwnerVisualChanged;
            _owner.TextChanged += OwnerVisualChanged;
            _owner.DropDown += OwnerDropDown;
            _owner.DropDownClosed += OwnerDropDownClosed;
        }

        private void OwnerHandleCreated(object? sender, EventArgs e) => Attach();
        private void OwnerHandleDestroyed(object? sender, EventArgs e) => ReleaseHandle();
        private void OwnerVisualChanged(object? sender, EventArgs e) => _owner.Invalidate();

        private void OwnerDropDown(object? sender, EventArgs e)
        {
            AttachDropDownList();
            _owner.Invalidate();
        }

        private void OwnerDropDownClosed(object? sender, EventArgs e) => _owner.Invalidate();

        private void AttachDropDownList()
        {
            if (!_owner.IsHandleCreated) return;
            var info = new ComboBoxInfo { Size = Marshal.SizeOf<ComboBoxInfo>() };
            if (GetComboBoxInfo(_owner.Handle, ref info) && info.ListHandle != IntPtr.Zero)
                _listChrome.AttachTo(info.ListHandle);
        }

        private void Attach()
        {
            if (_owner.IsHandleCreated && Handle != _owner.Handle)
            {
                if (Handle != IntPtr.Zero) ReleaseHandle();
                AssignHandle(_owner.Handle);
                AttachDropDownList();
            }
        }

        protected override void WndProc(ref Message m)
        {
            // DropDownList controls are painted completely by us. Calling the native
            // WM_PAINT first causes a one-frame white flash whenever focus moves.
            if (_owner.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                if (m.Msg == WmEraseBkgnd)
                {
                    if (m.WParam != IntPtr.Zero)
                    {
                        using var eraseGraphics = Graphics.FromHdc(m.WParam);
                        eraseGraphics.Clear(_owner.Enabled ? InputSurface : SurfaceAlt);
                    }
                    m.Result = (IntPtr)1;
                    return;
                }

                if (m.Msg == WmPaint)
                {
                    PaintWholeControl();
                    m.Result = IntPtr.Zero;
                    return;
                }

                if (m.Msg == WmNcPaint)
                {
                    m.Result = IntPtr.Zero;
                    return;
                }

                if (m.Msg == WmPrintClient && m.WParam != IntPtr.Zero)
                {
                    using var printGraphics = Graphics.FromHdc(m.WParam);
                    DrawChrome(printGraphics, drawText: true);
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

            base.WndProc(ref m);

            if (_owner.DropDownStyle == ComboBoxStyle.DropDownList &&
                m.Msg is WmSetFocus or WmKillFocus)
            {
                PaintWholeControlImmediate();
                return;
            }

            // Editable ComboBox variants still need native text/caret handling. Paint
            // only their chrome after Windows has drawn the editable portion.
            if (_owner.DropDownStyle != ComboBoxStyle.DropDownList &&
                m.Msg is WmPaint or WmNcPaint or WmPrintClient)
            {
                DrawChromeOverNative();
            }
        }

        private void PaintWholeControl()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated) return;
            var hdc = BeginPaint(_owner.Handle, out var paint);
            if (hdc == IntPtr.Zero) return;

            try
            {
                using var g = Graphics.FromHdc(hdc);
                DrawChrome(g, drawText: true);
            }
            finally
            {
                _ = EndPaint(_owner.Handle, ref paint);
            }
        }

        private void PaintWholeControlImmediate()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated) return;
            var hdc = GetDC(_owner.Handle);
            if (hdc == IntPtr.Zero) return;
            try
            {
                using var g = Graphics.FromHdc(hdc);
                DrawChrome(g, drawText: true);
            }
            finally
            {
                _ = ReleaseDC(_owner.Handle, hdc);
            }
        }

        private void DrawChromeOverNative()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated) return;
            using var g = Graphics.FromHwnd(_owner.Handle);
            DrawChrome(g, drawText: false);
        }

        private void DrawChrome(Graphics g, bool drawText)
        {
            if (_owner.Width <= 2 || _owner.Height <= 2) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var background = _owner.Enabled ? InputSurface : SurfaceAlt;
            var border = _owner.Focused ? PrimaryHover : BorderStrong;
            var buttonBack = _owner.DroppedDown ? InputFocus : InputDropDown;
            var buttonWidth = Math.Clamp(SystemInformation.VerticalScrollBarWidth + 3, 22, 28);
            var client = new Rectangle(0, 0, Math.Max(1, _owner.ClientSize.Width), Math.Max(1, _owner.ClientSize.Height));
            var buttonRect = new Rectangle(Math.Max(1, client.Width - buttonWidth - 1), 1, buttonWidth, Math.Max(1, client.Height - 2));

            if (drawText)
            {
                using var backgroundBrush = new SolidBrush(background);
                g.FillRectangle(backgroundBrush, client);

                var textRect = new Rectangle(8, 1, Math.Max(1, buttonRect.Left - 12), Math.Max(1, client.Height - 2));
                TextRenderer.DrawText(
                    g,
                    _owner.Text ?? string.Empty,
                    _owner.Font,
                    textRect,
                    _owner.Enabled ? TextPrimary : TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            using (var fill = new SolidBrush(buttonBack))
                g.FillRectangle(fill, buttonRect);

            using (var separator = new Pen(Border, 1f))
                g.DrawLine(separator, buttonRect.Left, 2, buttonRect.Left, Math.Max(2, client.Height - 3));

            using (var pen = new Pen(border, 1f))
                g.DrawRectangle(pen, 0, 0, Math.Max(0, client.Width - 1), Math.Max(0, client.Height - 1));

            var cx = buttonRect.Left + buttonRect.Width / 2f;
            var cy = buttonRect.Top + buttonRect.Height / 2f + 1f;
            PointF[] arrow =
            [
                new(cx - 4.5f, cy - 2.5f),
                new(cx + 4.5f, cy - 2.5f),
                new(cx, cy + 2.5f)
            ];
            using var arrowBrush = new SolidBrush(TextSecondary);
            g.FillPolygon(arrowBrush, arrow);
        }

        public void Dispose()
        {
            _owner.HandleCreated -= OwnerHandleCreated;
            _owner.HandleDestroyed -= OwnerHandleDestroyed;
            _owner.SizeChanged -= OwnerVisualChanged;
            _owner.EnabledChanged -= OwnerVisualChanged;
            _owner.GotFocus -= OwnerVisualChanged;
            _owner.LostFocus -= OwnerVisualChanged;
            _owner.SelectedIndexChanged -= OwnerVisualChanged;
            _owner.TextChanged -= OwnerVisualChanged;
            _owner.DropDown -= OwnerDropDown;
            _owner.DropDownClosed -= OwnerDropDownClosed;
            _listChrome.Dispose();
            if (Handle != IntPtr.Zero) ReleaseHandle();
        }
    }

    private sealed class DarkDateTimePickerChrome : NativeWindow, IDisposable
    {
        private readonly DateTimePicker _owner;

        public DarkDateTimePickerChrome(DateTimePicker owner)
        {
            _owner = owner;
            Attach();
            _owner.HandleCreated += OwnerHandleCreated;
            _owner.HandleDestroyed += OwnerHandleDestroyed;
            _owner.SizeChanged += OwnerVisualChanged;
            _owner.EnabledChanged += OwnerVisualChanged;
            _owner.GotFocus += OwnerVisualChanged;
            _owner.LostFocus += OwnerVisualChanged;
            _owner.ValueChanged += OwnerVisualChanged;
            _owner.DropDown += OwnerVisualChanged;
            _owner.CloseUp += OwnerVisualChanged;
        }

        private void OwnerHandleCreated(object? sender, EventArgs e) => Attach();
        private void OwnerHandleDestroyed(object? sender, EventArgs e) => ReleaseHandle();
        private void OwnerVisualChanged(object? sender, EventArgs e) => _owner.Invalidate();

        private void Attach()
        {
            if (_owner.IsHandleCreated && Handle != _owner.Handle)
            {
                if (Handle != IntPtr.Zero) ReleaseHandle();
                AssignHandle(_owner.Handle);
                ApplyDarkCalendarTheme(_owner);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // DateTimePicker ignores BackColor while visual styles are active. If
            // native WM_PAINT runs first it paints a white client area, then our old
            // overlay repaints only the button/border. Paint the complete client in
            // one pass instead so focus changes cannot flash white.
            if (m.Msg == WmEraseBkgnd)
            {
                if (m.WParam != IntPtr.Zero)
                {
                    using var eraseGraphics = Graphics.FromHdc(m.WParam);
                    eraseGraphics.Clear(_owner.Enabled ? InputSurface : SurfaceAlt);
                }
                m.Result = (IntPtr)1;
                return;
            }

            if (m.Msg == WmPaint)
            {
                PaintWholeControl();
                m.Result = IntPtr.Zero;
                return;
            }

            if (m.Msg == WmNcPaint)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            if (m.Msg == WmPrintClient && m.WParam != IntPtr.Zero)
            {
                using var printGraphics = Graphics.FromHdc(m.WParam);
                DrawControl(printGraphics);
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg is WmSetFocus or WmKillFocus)
                PaintWholeControlImmediate();
        }

        private void PaintWholeControl()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated) return;
            var hdc = BeginPaint(_owner.Handle, out var paint);
            if (hdc == IntPtr.Zero) return;

            try
            {
                using var g = Graphics.FromHdc(hdc);
                DrawControl(g);
            }
            finally
            {
                _ = EndPaint(_owner.Handle, ref paint);
            }
        }

        private void PaintWholeControlImmediate()
        {
            if (_owner.IsDisposed || !_owner.IsHandleCreated) return;
            var hdc = GetDC(_owner.Handle);
            if (hdc == IntPtr.Zero) return;
            try
            {
                using var g = Graphics.FromHdc(hdc);
                DrawControl(g);
            }
            finally
            {
                _ = ReleaseDC(_owner.Handle, hdc);
            }
        }

        private void DrawControl(Graphics g)
        {
            if (_owner.Width <= 2 || _owner.Height <= 2) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var client = new Rectangle(0, 0, Math.Max(1, _owner.ClientSize.Width), Math.Max(1, _owner.ClientSize.Height));
            var background = _owner.Enabled ? InputSurface : SurfaceAlt;
            var foreground = _owner.Enabled && (!_owner.ShowCheckBox || _owner.Checked) ? TextPrimary : TextSecondary;
            var border = _owner.Focused ? PrimaryHover : BorderStrong;
            var buttonWidth = Math.Clamp(SystemInformation.VerticalScrollBarWidth + 3, 22, 28);
            var buttonRect = new Rectangle(Math.Max(1, client.Width - buttonWidth - 1), 1, buttonWidth, Math.Max(1, client.Height - 2));

            using (var backgroundBrush = new SolidBrush(background))
                g.FillRectangle(backgroundBrush, client);

            var textLeft = 8;
            if (_owner.ShowCheckBox)
            {
                var boxSize = Math.Clamp(client.Height - 12, 12, 16);
                var box = new Rectangle(6, Math.Max(2, (client.Height - boxSize) / 2), boxSize, boxSize);
                using var boxBrush = new SolidBrush(InputDropDown);
                using var boxPen = new Pen(_owner.Focused ? PrimaryHover : BorderStrong, 1f);
                g.FillRectangle(boxBrush, box);
                g.DrawRectangle(boxPen, box);

                if (_owner.Checked)
                {
                    using var checkPen = new Pen(PrimaryHover, 2f)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };
                    g.DrawLines(checkPen,
                    [
                        new Point(box.Left + 3, box.Top + box.Height / 2),
                        new Point(box.Left + box.Width / 2 - 1, box.Bottom - 4),
                        new Point(box.Right - 3, box.Top + 3)
                    ]);
                }

                textLeft = box.Right + 7;
            }

            var textRect = new Rectangle(
                textLeft,
                1,
                Math.Max(1, buttonRect.Left - textLeft - 4),
                Math.Max(1, client.Height - 2));

            TextRenderer.DrawText(
                g,
                FormatDateValue(_owner),
                _owner.Font,
                textRect,
                foreground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            using (var fill = new SolidBrush(InputDropDown))
                g.FillRectangle(fill, buttonRect);
            using (var separator = new Pen(Border, 1f))
                g.DrawLine(separator, buttonRect.Left, 2, buttonRect.Left, Math.Max(2, client.Height - 3));

            var icon = Rectangle.Inflate(buttonRect, -6, -6);
            if (icon.Width > 6 && icon.Height > 6)
            {
                using var iconPen = new Pen(TextSecondary, 1.4f);
                g.DrawRectangle(iconPen, icon.Left, icon.Top + 2, icon.Width - 1, icon.Height - 3);
                g.DrawLine(iconPen, icon.Left, icon.Top + 5, icon.Right - 1, icon.Top + 5);
                g.DrawLine(iconPen, icon.Left + 3, icon.Top, icon.Left + 3, icon.Top + 4);
                g.DrawLine(iconPen, icon.Right - 4, icon.Top, icon.Right - 4, icon.Top + 4);
            }

            using var borderPen = new Pen(border, 1f);
            g.DrawRectangle(borderPen, 0, 0, Math.Max(0, client.Width - 1), Math.Max(0, client.Height - 1));
        }

        private static string FormatDateValue(DateTimePicker picker)
        {
            var culture = CultureInfo.CurrentCulture;
            return picker.Format switch
            {
                DateTimePickerFormat.Long => picker.Value.ToString(culture.DateTimeFormat.LongDatePattern, culture),
                DateTimePickerFormat.Time => picker.Value.ToString(culture.DateTimeFormat.LongTimePattern, culture),
                DateTimePickerFormat.Custom when !string.IsNullOrWhiteSpace(picker.CustomFormat)
                    => picker.Value.ToString(picker.CustomFormat, culture),
                _ => picker.Value.ToString(culture.DateTimeFormat.ShortDatePattern, culture)
            };
        }

        public void Dispose()
        {
            _owner.HandleCreated -= OwnerHandleCreated;
            _owner.HandleDestroyed -= OwnerHandleDestroyed;
            _owner.SizeChanged -= OwnerVisualChanged;
            _owner.EnabledChanged -= OwnerVisualChanged;
            _owner.GotFocus -= OwnerVisualChanged;
            _owner.LostFocus -= OwnerVisualChanged;
            _owner.ValueChanged -= OwnerVisualChanged;
            _owner.DropDown -= OwnerVisualChanged;
            _owner.CloseUp -= OwnerVisualChanged;
            if (Handle != IntPtr.Zero) ReleaseHandle();
        }
    }

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
                    StyleDateTimePicker(dateTimePicker);
                    break;
                case NumericUpDown numericUpDown:
                    StyleNumericUpDown(numericUpDown);
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

        // Hover remains smooth, but press/release feedback must be immediate.
        // A click should never wait for the animation timer before changing color.
        button.MouseEnter += (_, _) => AnimateTo(button, state, Palette(state.Role).hover);
        button.MouseLeave += (_, _) => AnimateTo(button, state, Palette(state.Role).normal);
        button.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                SetButtonColorImmediate(button, state, Palette(state.Role).pressed);
        };
        button.MouseUp += (_, _) => SetButtonColorImmediate(button, state,
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
        // Navigation state changes (active/inactive) are semantic, not decorative
        // animations. Present them in the same input event instead of one frame later.
        if (button.IsHandleCreated)
            button.Update();
    }

    private static void SetButtonColorImmediate(Button button, ButtonState state, Color color)
    {
        if (button.IsDisposed) return;
        state.Timer.Stop();
        state.Current = color;
        state.Target = color;
        button.BackColor = color;
        button.Invalidate();
        if (button.IsHandleCreated)
            button.Update();
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
        textBox.BackColor = InputSurface;
        textBox.ForeColor = TextPrimary;
        // Never use the native FixedSingle border in dark mode: USER32 paints
        // that non-client edge with a bright system color during focus transitions.
        // Draw our own 1px border inside the client area instead.
        textBox.BorderStyle = BorderStyle.None;
        textBox.Font = new Font("Segoe UI", 10F);

        // WinForms does not expose vertical alignment for a stretched single-line
        // TextBox. For inputs that were originally single-line we use a one-line
        // multiline edit and set its formatting rectangle explicitly. This keeps
        // the text visually centered at 30 px across Windows DPI/font scales.
        var shouldCenterVertically = !textBox.Multiline;
        if (shouldCenterVertically)
        {
            textBox.Multiline = true;
            textBox.WordWrap = false;
            textBox.AcceptsReturn = false;
            textBox.AcceptsTab = false;
            textBox.ScrollBars = ScrollBars.None;
            textBox.AutoSize = false;
            textBox.Height = InputHeight;
            textBox.MinimumSize = new Size(0, InputHeight);
            textBox.Margin = InputMargin;
        }

        if (!StyledTextBoxes.TryGetValue(textBox, out _))
        {
            StyledTextBoxes.Add(textBox, new object());
            // Keep the input fill stable when focus moves. A full-surface color
            // change reads as a flash in dark mode, especially when tabbing quickly.
            textBox.Enter += (_, _) => textBox.Invalidate();
            textBox.Leave += (_, _) => textBox.Invalidate();

            if (shouldCenterVertically)
            {
                textBox.HandleCreated += (_, _) => CenterTextBoxContent(textBox);
                textBox.SizeChanged += (_, _) => CenterTextBoxContent(textBox);
                textBox.FontChanged += (_, _) => CenterTextBoxContent(textBox);
                textBox.KeyDown += (_, e) =>
                {
                    if (e.KeyCode != Keys.Enter) return;
                    textBox.FindForm()?.AcceptButton?.PerformClick();
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                };
            }
        }

        _ = TextBoxChrome.GetValue(textBox, static owner => new DarkTextBoxChrome(owner));

        if (shouldCenterVertically)
            CenterTextBoxContent(textBox);
    }

    private static void CenterTextBoxContent(TextBox textBox)
    {
        if (textBox.IsDisposed || !textBox.IsHandleCreated || !textBox.Multiline) return;

        var textHeight = TextRenderer.MeasureText("Ag", textBox.Font, Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Height;
        var top = Math.Max(2, (textBox.ClientSize.Height - textHeight) / 2);
        var bottom = Math.Min(textBox.ClientSize.Height - 2, top + textHeight + 1);
        var horizontalPadding = 7;
        var rect = new NativeRect
        {
            Left = horizontalPadding,
            Top = top,
            Right = Math.Max(horizontalPadding + 1, textBox.ClientSize.Width - horizontalPadding),
            Bottom = Math.Max(top + 1, bottom)
        };
        _ = SendMessage(textBox.Handle, EmSetRectNp, IntPtr.Zero, ref rect);
        textBox.Invalidate();
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = InputSurface;
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
            comboBox.HandleCreated += (_, _) => ApplyDarkNativeTheme(comboBox);
            comboBox.SelectedIndexChanged += (_, _) => comboBox.Invalidate();
            comboBox.TextChanged += (_, _) => comboBox.Invalidate();
        }

        _ = ComboBoxChrome.GetValue(comboBox, static owner => new DarkComboBoxChrome(owner));
        ApplyDarkNativeTheme(comboBox);
        comboBox.Invalidate();
    }

    private static void StyleDateTimePicker(DateTimePicker dateTimePicker)
    {
        dateTimePicker.Font = new Font("Segoe UI", 10F);
        dateTimePicker.CalendarFont = new Font("Segoe UI", 10F);
        dateTimePicker.BackColor = InputSurface;
        dateTimePicker.ForeColor = TextPrimary;
        dateTimePicker.CalendarMonthBackground = InputDropDown;
        dateTimePicker.CalendarForeColor = TextPrimary;
        dateTimePicker.CalendarTitleBackColor = SurfaceAlt;
        dateTimePicker.CalendarTitleForeColor = TextPrimary;
        dateTimePicker.CalendarTrailingForeColor = TextSecondary;
        dateTimePicker.Height = InputHeight;
        dateTimePicker.Margin = InputMargin;

        if (!StyledDatePickers.TryGetValue(dateTimePicker, out _))
        {
            StyledDatePickers.Add(dateTimePicker, new object());
            dateTimePicker.HandleCreated += (_, _) => ApplyDarkNativeTheme(dateTimePicker);
            dateTimePicker.DropDown += (_, _) =>
            {
                ApplyDarkCalendarTheme(dateTimePicker);
                dateTimePicker.Invalidate();
            };
        }

        _ = DatePickerChrome.GetValue(dateTimePicker, static owner => new DarkDateTimePickerChrome(owner));
        ApplyDarkNativeTheme(dateTimePicker);
        dateTimePicker.Invalidate();
    }

    private static void StyleNumericUpDown(NumericUpDown numericUpDown)
    {
        numericUpDown.Font = new Font("Segoe UI", 10F);
        numericUpDown.BackColor = InputSurface;
        numericUpDown.ForeColor = TextPrimary;
        numericUpDown.BorderStyle = BorderStyle.None;
        numericUpDown.Height = InputHeight;
        numericUpDown.Margin = InputMargin;

        foreach (Control child in numericUpDown.Controls)
        {
            child.BackColor = InputSurface;
            child.ForeColor = TextPrimary;
            if (child.IsHandleCreated)
                _ = SetWindowTheme(child.Handle, string.Empty, string.Empty);
        }

        if (!StyledNumericInputs.TryGetValue(numericUpDown, out _))
        {
            StyledNumericInputs.Add(numericUpDown, new object());
            numericUpDown.HandleCreated += (_, _) => ApplyDarkNativeTheme(numericUpDown);
        }

        _ = NumericChrome.GetValue(numericUpDown, static owner => new DarkNumericChrome(owner));
        ApplyDarkNativeTheme(numericUpDown);
    }

    private static void ApplyDarkNativeTheme(Control control)
    {
        if (control.IsDisposed || !control.IsHandleCreated) return;

        // Disable visual-style painting on the native edit/combo/date surface.
        // Windows can otherwise paint a white themed frame for one frame before
        // our owner-drawn dark chrome receives WM_PAINT. Popup/list/calendar
        // windows are themed separately when they are created.
        _ = SetWindowTheme(control.Handle, string.Empty, string.Empty);
        control.Invalidate();
    }

    private static void ApplyDarkCalendarTheme(DateTimePicker picker)
    {
        if (picker.IsDisposed || !picker.IsHandleCreated) return;
        var calendar = SendMessagePtr(picker.Handle, DtmGetMonthCal, IntPtr.Zero, IntPtr.Zero);
        if (calendar == IntPtr.Zero) return;

        _ = SetWindowTheme(calendar, string.Empty, string.Empty);
        SetMonthCalendarColor(calendar, McscBackground, InputDropDown);
        SetMonthCalendarColor(calendar, McscMonthBk, InputDropDown);
        SetMonthCalendarColor(calendar, McscText, TextPrimary);
        SetMonthCalendarColor(calendar, McscTitleBk, SurfaceAlt);
        SetMonthCalendarColor(calendar, McscTitleText, TextPrimary);
        SetMonthCalendarColor(calendar, McscTrailingText, TextSecondary);
    }

    private static void SetMonthCalendarColor(IntPtr calendar, int part, Color color)
    {
        var colorRef = color.R | (color.G << 8) | (color.B << 16);
        _ = SendMessagePtr(calendar, McmSetColor, (IntPtr)part, (IntPtr)colorRef);
    }

    private static void DrawComboBoxItem(ComboBox comboBox, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var background = selected ? InputFocus : InputDropDown;
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
