using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ITDeviceManager.Common;

/// <summary>
/// Fully managed dark drop-down selector used instead of the native Win32 ComboLBox popup.
/// The closed field and the popup list are painted by the application, so Windows never
/// exposes a COLOR_WINDOW (white) strip at the bottom of the drop-down while it opens.
/// </summary>
public sealed class DarkComboBox : Control
{
    private readonly DarkComboItemCollection _items;
    private object? _dataSource;
    private string _displayMember = string.Empty;
    private string _valueMember = string.Empty;
    private int _selectedIndex = -1;
    private ToolStripDropDown? _popup;
    private ToolStripControlHost? _popupHost;
    private DropDownSurface? _popupSurface;
    private bool _dropDownTransition;
    private long _lastPopupClosedTick;

    // AutoClose can close the ToolStripDropDown before the owner receives the same mouse click.
    // A very short guard prevents that click from immediately opening a second popup.
    private const int ReopenGuardMilliseconds = 120;

    public DarkComboBox()
    {
        _items = new DarkComboItemCollection(OnItemsChanged);
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable,
            true);

        BackColor = AppTheme.InputSurface;
        ForeColor = AppTheme.TextPrimary;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Size = new Size(160, AppTheme.InputHeight);
        MinimumSize = new Size(32, AppTheme.InputHeight);
        Margin = new Padding(3, 4, 3, 4);
        TabStop = true;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public DarkComboItemCollection Items => _items;

    [DefaultValue(null)]
    public object? DataSource
    {
        get => _dataSource;
        set
        {
            if (ReferenceEquals(_dataSource, value)) return;
            _dataSource = value;
            _items.ReplaceFrom(value as IEnumerable, notify: false);
            _selectedIndex = -1;
            SetSelectedIndex(_items.Count > 0 ? 0 : -1, raiseEvent: true);
            DataSourceChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    [DefaultValue("")]
    public string DisplayMember
    {
        get => _displayMember;
        set
        {
            _displayMember = value ?? string.Empty;
            Invalidate();
            _popupSurface?.Invalidate();
        }
    }

    [DefaultValue("")]
    public string ValueMember
    {
        get => _valueMember;
        set => _valueMember = value ?? string.Empty;
    }

    // Compatibility surface for existing form initializers. This managed selector supports
    // DropDownList semantics only; keeping the property avoids churn in form construction.
    [DefaultValue(ComboBoxStyle.DropDownList)]
    public ComboBoxStyle DropDownStyle { get; set; } = ComboBoxStyle.DropDownList;

    [DefaultValue(8)]
    public int MaxDropDownItems { get; set; } = 8;

    [Browsable(false)]
    public bool DroppedDown => _popup is { IsDisposed: false, Visible: true };

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetSelectedIndex(value, raiseEvent: true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (ReferenceEquals(_items[i], value) || Equals(_items[i], value))
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? SelectedValue
    {
        get
        {
            var item = SelectedItem;
            return item is null ? null : ReadMember(item, _valueMember);
        }
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item is null) continue;
                var candidate = ReadMember(item, _valueMember);
                if (Equals(candidate, value))
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }
    }

    [AllowNull]
    public override string Text
    {
        get => SelectedItem is null ? string.Empty : GetDisplayText(SelectedItem);
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                SelectedIndex = -1;
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (string.Equals(GetDisplayText(_items[i]), value, StringComparison.CurrentCulture))
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }
    }

    public event EventHandler? SelectedIndexChanged;
    public event EventHandler? DataSourceChanged;
    public event EventHandler? DropDown;
    public event EventHandler? DropDownClosed;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        var client = ClientRectangle;
        if (client.Width <= 1 || client.Height <= 1) return;

        var background = Enabled ? AppTheme.InputSurface : AppTheme.SurfaceAlt;
        var border = Focused || DroppedDown ? AppTheme.PrimaryHover : AppTheme.BorderStrong;
        var buttonWidth = Math.Clamp(SystemInformation.VerticalScrollBarWidth + 3, 22, 28);
        var buttonRect = new Rectangle(Math.Max(1, client.Width - buttonWidth - 1), 1, buttonWidth, Math.Max(1, client.Height - 2));

        using (var backgroundBrush = new SolidBrush(background))
            g.FillRectangle(backgroundBrush, client);

        using (var buttonBrush = new SolidBrush(DroppedDown ? AppTheme.InputFocus : AppTheme.InputDropDown))
            g.FillRectangle(buttonBrush, buttonRect);

        var textRect = new Rectangle(8, 1, Math.Max(1, buttonRect.Left - 12), Math.Max(1, client.Height - 2));
        TextRenderer.DrawText(
            g,
            Text,
            Font,
            textRect,
            Enabled ? AppTheme.TextPrimary : AppTheme.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        using (var separator = new Pen(AppTheme.Border, 1f))
            g.DrawLine(separator, buttonRect.Left, 2, buttonRect.Left, Math.Max(2, client.Height - 3));

        using (var borderPen = new Pen(border, 1f))
            g.DrawRectangle(borderPen, 0, 0, client.Width - 1, client.Height - 1);

        var cx = buttonRect.Left + buttonRect.Width / 2f;
        var cy = buttonRect.Top + buttonRect.Height / 2f + 1f;
        PointF[] arrow =
        [
            new(cx - 4.5f, cy - 2.5f),
            new(cx + 4.5f, cy - 2.5f),
            new(cx, cy + 2.5f)
        ];
        using var arrowBrush = new SolidBrush(AppTheme.TextSecondary);
        g.FillPolygon(arrowBrush, arrow);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!Enabled || e.Button != MouseButtons.Left) return;
        Focus();
        ToggleDropDown();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!Enabled) return;

        if (e.KeyCode is Keys.Space or Keys.Enter || (e.Alt && e.KeyCode == Keys.Down))
        {
            ToggleDropDown();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        if (e.KeyCode == Keys.Down && _items.Count > 0)
        {
            SelectedIndex = Math.Min(_items.Count - 1, Math.Max(0, _selectedIndex + 1));
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Up && _items.Count > 0)
        {
            SelectedIndex = Math.Max(0, _selectedIndex <= 0 ? 0 : _selectedIndex - 1);
            e.Handled = true;
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        if (!Enabled) CloseDropDown();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // The popup is intentionally reused while the control is alive. Disposing it from
            // ToolStripDropDown.Closed is unsafe because Closed can be raised inside AutoClose
            // message processing. Dispose exactly once with the owner instead.
            var popup = _popup;
            _popup = null;
            _popupHost = null;
            _popupSurface = null;
            if (popup is not null)
            {
                popup.Closed -= PopupClosed;
                if (!popup.IsDisposed) popup.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private void ToggleDropDown()
    {
        if (_dropDownTransition || IsDisposed || Disposing || !IsHandleCreated) return;

        if (DroppedDown)
        {
            CloseDropDown();
            return;
        }

        // Clicking the owner while an AutoClose popup is visible closes the popup before
        // OnMouseDown reaches this control. Do not reopen it from that very same click.
        if (Environment.TickCount64 - _lastPopupClosedTick < ReopenGuardMilliseconds) return;
        ShowDropDown();
    }

    private void ShowDropDown()
    {
        if (_items.Count == 0 || DroppedDown || _dropDownTransition || IsDisposed || Disposing || !IsHandleCreated) return;

        _dropDownTransition = true;
        try
        {
            var itemHeight = Math.Max(26, TextRenderer.MeasureText("Ag", Font).Height + 8);
            var visibleCount = Math.Max(1, Math.Min(_items.Count, Math.Max(1, MaxDropDownItems)));
            var popupHeight = visibleCount * itemHeight + 2;
            var popupWidth = Math.Max(Width, 80);

            EnsurePopup(itemHeight, popupWidth, popupHeight);
            var popup = _popup;
            var surface = _popupSurface;
            if (popup is null || surface is null || popup.IsDisposed) return;

            DropDown?.Invoke(this, EventArgs.Empty);
            Invalidate();

            var screenBounds = Screen.FromControl(this).WorkingArea;
            var below = PointToScreen(new Point(0, Height));
            var above = PointToScreen(new Point(0, -popupHeight));
            var showAbove = below.Y + popupHeight > screenBounds.Bottom && above.Y >= screenBounds.Top;
            var location = showAbove ? new Point(0, -popupHeight) : new Point(0, Height);

            popup.Show(this, location);
            if (!surface.IsDisposed && surface.CanFocus) surface.Focus();
        }
        catch (ObjectDisposedException)
        {
            ResetDisposedPopup();
        }
        catch (InvalidOperationException ex)
        {
            // Rapid open/close sequences can race ToolStripDropDown's internal AutoClose
            // message filter. Treat a transition failure as a cancelled popup operation
            // instead of allowing an unhandled UI-thread exception to terminate the app.
            System.Diagnostics.Debug.WriteLine($"DarkComboBox.ShowDropDown cancelled: {ex.Message}");
            ResetDisposedPopup();
        }
        finally
        {
            _dropDownTransition = false;
        }
    }

    private void EnsurePopup(int itemHeight, int popupWidth, int popupHeight)
    {
        if (_popup is null || _popup.IsDisposed || _popupSurface is null || _popupSurface.IsDisposed || _popupHost is null)
        {
            var surface = new DropDownSurface(this, itemHeight)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = AppTheme.InputDropDown
            };

            var host = new ToolStripControlHost(surface)
            {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            var popup = new ToolStripDropDown
            {
                AutoSize = false,
                AutoClose = true,
                Padding = new Padding(1),
                Margin = Padding.Empty,
                BackColor = AppTheme.InputDropDown,
                DropShadowEnabled = false,
                Renderer = DarkDropDownRenderer.Instance
            };
            popup.Items.Add(host);
            popup.Closed += PopupClosed;

            _popup = popup;
            _popupHost = host;
            _popupSurface = surface;
        }

        var contentSize = new Size(Math.Max(1, popupWidth - 2), Math.Max(1, popupHeight - 2));
        _popupSurface!.UpdateMetrics(itemHeight, contentSize);
        _popupHost!.Size = contentSize;
        _popup!.Size = new Size(popupWidth, popupHeight);
    }

    private void CloseDropDown()
    {
        var popup = _popup;
        if (popup is null || popup.IsDisposed || !popup.Visible || _dropDownTransition) return;

        _dropDownTransition = true;
        try
        {
            // Do not Dispose here. ToolStripDropDown.Close can synchronously raise Closed while
            // WinForms is still inside its AutoClose/message-filter path. The reusable popup is
            // disposed only when DarkComboBox itself is disposed.
            popup.Close(ToolStripDropDownCloseReason.CloseCalled);
        }
        catch (ObjectDisposedException)
        {
            ResetDisposedPopup();
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"DarkComboBox.CloseDropDown cancelled: {ex.Message}");
            ResetDisposedPopup();
        }
        finally
        {
            _dropDownTransition = false;
        }
    }

    private void PopupClosed(object? sender, ToolStripDropDownClosedEventArgs e)
    {
        if (sender is not ToolStripDropDown popup || !ReferenceEquals(popup, _popup)) return;

        _lastPopupClosedTick = Environment.TickCount64;
        DropDownClosed?.Invoke(this, EventArgs.Empty);
        if (!IsDisposed && !Disposing) Invalidate();
    }

    private void ResetDisposedPopup()
    {
        var popup = _popup;
        _popup = null;
        _popupHost = null;
        _popupSurface = null;
        if (popup is not null) popup.Closed -= PopupClosed;
    }

    private void CommitPopupSelection(int index)
    {
        SetSelectedIndex(index, raiseEvent: true);
        if (IsDisposed || Disposing) return;
        CloseDropDown();
        if (!IsDisposed && !Disposing && CanFocus) Focus();
    }

    private void SetSelectedIndex(int index, bool raiseEvent)
    {
        var normalized = index >= 0 && index < _items.Count ? index : -1;
        if (_selectedIndex == normalized) return;
        _selectedIndex = normalized;
        Invalidate();
        _popupSurface?.EnsureSelectionVisible();
        _popupSurface?.Invalidate();
        if (raiseEvent) SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnItemsChanged()
    {
        if (_selectedIndex >= _items.Count)
            _selectedIndex = _items.Count > 0 ? 0 : -1;
        else if (_selectedIndex < 0 && _items.Count > 0 && _dataSource is not null)
            _selectedIndex = 0;

        Invalidate();
        _popupSurface?.Invalidate();
    }

    internal string GetDisplayText(object? item)
    {
        if (item is null) return string.Empty;
        var value = ReadMember(item, _displayMember);
        return Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static object? ReadMember(object item, string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName)) return item;
        var property = TypeDescriptor.GetProperties(item).Find(memberName, ignoreCase: false);
        return property?.GetValue(item);
    }

    public sealed class DarkComboItemCollection : IList<object?>
    {
        private readonly List<object?> _inner = [];
        private readonly Action _changed;

        internal DarkComboItemCollection(Action changed) => _changed = changed;

        public object? this[int index]
        {
            get => _inner[index];
            set { _inner[index] = value; _changed(); }
        }

        public int Count => _inner.Count;
        public bool IsReadOnly => false;
        public void Add(object? item) { _inner.Add(item); _changed(); }
        public void Clear() { if (_inner.Count == 0) return; _inner.Clear(); _changed(); }
        public bool Contains(object? item) => _inner.Contains(item);
        public void CopyTo(object?[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
        public IEnumerator<object?> GetEnumerator() => _inner.GetEnumerator();
        public int IndexOf(object? item) => _inner.IndexOf(item);
        public void Insert(int index, object? item) { _inner.Insert(index, item); _changed(); }
        public bool Remove(object? item) { var removed = _inner.Remove(item); if (removed) _changed(); return removed; }
        public void RemoveAt(int index) { _inner.RemoveAt(index); _changed(); }
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

        internal void ReplaceFrom(IEnumerable? source, bool notify = true)
        {
            _inner.Clear();
            if (source is not null)
            {
                foreach (var item in source)
                    _inner.Add(item);
            }
            if (notify) _changed();
        }
    }

    private sealed class DropDownSurface : Control
    {
        private readonly DarkComboBox _owner;
        private int _itemHeight;
        private int _firstVisible;
        private int _hotIndex = -1;

        public DropDownSurface(DarkComboBox owner, int itemHeight)
        {
            _owner = owner;
            _itemHeight = itemHeight;
            BackColor = AppTheme.InputDropDown;
            ForeColor = AppTheme.TextPrimary;
            Font = owner.Font;
            TabStop = true;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);
            EnsureSelectionVisible();
        }

        internal void UpdateMetrics(int itemHeight, Size size)
        {
            _itemHeight = Math.Max(1, itemHeight);
            Size = size;
            Font = _owner.Font;
            EnsureSelectionVisible();
            Invalidate();
        }

        private int VisibleRows => Math.Max(1, Height / Math.Max(1, _itemHeight));
        private int MaxFirst => Math.Max(0, _owner.Items.Count - VisibleRows);

        internal void EnsureSelectionVisible()
        {
            var selected = _owner.SelectedIndex;
            if (selected < 0) return;
            if (selected < _firstVisible) _firstVisible = selected;
            if (selected >= _firstVisible + VisibleRows) _firstVisible = selected - VisibleRows + 1;
            _firstVisible = Math.Clamp(_firstVisible, 0, MaxFirst);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(AppTheme.InputDropDown);

            var visibleRows = VisibleRows;
            for (var row = 0; row < visibleRows; row++)
            {
                var index = _firstVisible + row;
                if (index >= _owner.Items.Count) break;

                var rect = new Rectangle(0, row * _itemHeight, Width, _itemHeight);
                var selected = index == _owner.SelectedIndex;
                var hot = index == _hotIndex;
                var background = selected || hot ? AppTheme.InputFocus : AppTheme.InputDropDown;
                using (var brush = new SolidBrush(background))
                    e.Graphics.FillRectangle(brush, rect);

                var textRect = new Rectangle(8, rect.Y, Math.Max(1, rect.Width - 16), rect.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    _owner.GetDisplayText(_owner.Items[index]),
                    Font,
                    textRect,
                    AppTheme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            if (_owner.Items.Count > visibleRows)
                DrawScrollIndicator(e.Graphics, visibleRows);
        }

        private void DrawScrollIndicator(Graphics g, int visibleRows)
        {
            var trackWidth = 4;
            var track = new Rectangle(Math.Max(0, Width - trackWidth - 2), 3, trackWidth, Math.Max(1, Height - 6));
            using (var trackBrush = new SolidBrush(AppTheme.Border))
                g.FillRectangle(trackBrush, track);

            var ratio = Math.Clamp((double)visibleRows / _owner.Items.Count, 0.08, 1.0);
            var thumbHeight = Math.Max(12, (int)Math.Round(track.Height * ratio));
            var travel = Math.Max(0, track.Height - thumbHeight);
            var positionRatio = MaxFirst == 0 ? 0d : (double)_firstVisible / MaxFirst;
            var thumbY = track.Y + (int)Math.Round(travel * positionRatio);
            using var thumbBrush = new SolidBrush(AppTheme.TextSecondary);
            g.FillRectangle(thumbBrush, track.X, thumbY, track.Width, thumbHeight);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var index = _firstVisible + Math.Max(0, e.Y / Math.Max(1, _itemHeight));
            var nextHot = index >= 0 && index < _owner.Items.Count ? index : -1;
            if (_hotIndex == nextHot) return;
            _hotIndex = nextHot;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hotIndex == -1) return;
            _hotIndex = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            var index = _firstVisible + e.Y / Math.Max(1, _itemHeight);
            if (index >= 0 && index < _owner.Items.Count)
                _owner.CommitPopupSelection(index);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            var delta = e.Delta > 0 ? -1 : 1;
            _firstVisible = Math.Clamp(_firstVisible + delta, 0, MaxFirst);
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var current = _owner.SelectedIndex < 0 ? 0 : _owner.SelectedIndex;

            if (e.KeyCode == Keys.Escape)
            {
                _owner.CloseDropDown();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                var target = _hotIndex >= 0 ? _hotIndex : current;
                if (target >= 0 && target < _owner.Items.Count)
                    _owner.CommitPopupSelection(target);
                e.Handled = true;
                return;
            }

            if (e.KeyCode is Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End)
            {
                var target = current;
                if (e.KeyCode == Keys.Up) target--;
                if (e.KeyCode == Keys.Down) target++;
                if (e.KeyCode == Keys.PageUp) target -= VisibleRows;
                if (e.KeyCode == Keys.PageDown) target += VisibleRows;
                if (e.KeyCode == Keys.Home) target = 0;
                if (e.KeyCode == Keys.End) target = _owner.Items.Count - 1;
                target = Math.Clamp(target, 0, Math.Max(0, _owner.Items.Count - 1));
                _hotIndex = target;
                if (_hotIndex < _firstVisible) _firstVisible = _hotIndex;
                if (_hotIndex >= _firstVisible + VisibleRows) _firstVisible = _hotIndex - VisibleRows + 1;
                _firstVisible = Math.Clamp(_firstVisible, 0, MaxFirst);
                Invalidate();
                e.Handled = true;
            }
        }
    }

    private sealed class DarkDropDownRenderer : ToolStripRenderer
    {
        public static readonly DarkDropDownRenderer Instance = new();

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(AppTheme.InputDropDown);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            var rect = new Rectangle(0, 0, Math.Max(0, e.ToolStrip.Width - 1), Math.Max(0, e.ToolStrip.Height - 1));
            using var pen = new Pen(AppTheme.BorderStrong, 1f);
            e.Graphics.DrawRectangle(pen, rect);
        }
    }
}
