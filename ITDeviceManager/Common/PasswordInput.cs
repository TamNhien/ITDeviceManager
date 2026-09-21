using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ITDeviceManager.Common;

public sealed class PasswordInput : UserControl
{
    private const int HorizontalPadding = 10;
    private const int ToggleWidth = 38;

    private readonly TextBox _textBox = new()
    {
        BorderStyle = BorderStyle.None,
        UseSystemPasswordChar = true,
        Multiline = false,
        Margin = Padding.Empty,
        BackColor = AppTheme.InputSurface,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI", 10F),
        TabStop = false
    };

    private readonly EyeButton _toggle = new()
    {
        Width = ToggleWidth,
        TabStop = false,
        Cursor = Cursors.Hand,
        AccessibleName = "Hiện mật khẩu"
    };

    public PasswordInput()
    {
        Width = 280;
        Height = 36;
        MinimumSize = new Size(0, 36);
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = AppTheme.InputSurface;
        Padding = Padding.Empty;
        TabStop = true;

        Controls.Add(_textBox);
        Controls.Add(_toggle);

        _toggle.Click += (_, _) => TogglePasswordVisibility();
        _textBox.TextChanged += (_, e) => PasswordChanged?.Invoke(this, e);
        _textBox.Enter += (_, _) => SetFocusedAppearance(true);
        _textBox.Leave += (_, _) => SetFocusedAppearance(false);
        SizeChanged += (_, _) => LayoutChildren();
        FontChanged += (_, _) =>
        {
            _textBox.Font = Font;
            LayoutChildren();
        };

        LayoutChildren();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Password
    {
        get => _textBox.Text;
        set => _textBox.Text = value ?? string.Empty;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool PasswordVisible => !_textBox.UseSystemPasswordChar;

    public event EventHandler? PasswordChanged;

    public void FocusInput() => _textBox.Focus();

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (_textBox is not null)
        {
            _textBox.Font = Font;
            LayoutChildren();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutChildren();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        _textBox.Focus();
    }

    private void LayoutChildren()
    {
        if (_textBox is null || _toggle is null || IsDisposed) return;

        var toggleWidth = Math.Min(ToggleWidth, Math.Max(1, ClientSize.Width / 3));
        _toggle.SetBounds(Math.Max(0, ClientSize.Width - toggleWidth), 0, toggleWidth, ClientSize.Height);

        var preferredHeight = _textBox.PreferredHeight;
        var top = Math.Max(0, (ClientSize.Height - preferredHeight) / 2);
        var width = Math.Max(1, ClientSize.Width - toggleWidth - HorizontalPadding - 2);
        _textBox.SetBounds(HorizontalPadding, top, width, preferredHeight);
    }

    private void SetFocusedAppearance(bool focused)
    {
        var background = focused ? AppTheme.InputFocus : AppTheme.InputSurface;
        BackColor = background;
        _textBox.BackColor = background;
        _toggle.BackColor = background;
    }

    private void TogglePasswordVisibility()
    {
        _textBox.UseSystemPasswordChar = !_textBox.UseSystemPasswordChar;
        _toggle.PasswordVisible = !_textBox.UseSystemPasswordChar;
        _toggle.AccessibleName = _textBox.UseSystemPasswordChar ? "Hiện mật khẩu" : "Ẩn mật khẩu";
        _textBox.Focus();
        _textBox.SelectionStart = _textBox.TextLength;
    }

    private sealed class EyeButton : Button
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PasswordVisible { get; set; }

        public EyeButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = AppTheme.InputSurface;
            UseVisualStyleBackColor = false;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(BackColor);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(AppTheme.TextSecondary, 1.7f);
            using var brush = new SolidBrush(AppTheme.TextSecondary);

            var cx = ClientSize.Width / 2f;
            var cy = ClientSize.Height / 2f;
            var eye = new RectangleF(cx - 10f, cy - 6f, 20f, 12f);

            using var path = new GraphicsPath();
            path.AddBezier(eye.Left, cy, eye.Left + 5, eye.Top, eye.Right - 5, eye.Top, eye.Right, cy);
            path.AddBezier(eye.Right, cy, eye.Right - 5, eye.Bottom, eye.Left + 5, eye.Bottom, eye.Left, cy);
            pevent.Graphics.DrawPath(pen, path);
            pevent.Graphics.FillEllipse(brush, cx - 2.5f, cy - 2.5f, 5f, 5f);

            if (PasswordVisible)
                pevent.Graphics.DrawLine(pen, eye.Left - 1, eye.Bottom + 2, eye.Right + 1, eye.Top - 2);
        }
    }
}
