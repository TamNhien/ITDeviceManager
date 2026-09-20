using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ITDeviceManager.Common;

public sealed class PasswordInput : UserControl
{
    private readonly TextBox _textBox = new()
    {
        BorderStyle = BorderStyle.None,
        Dock = DockStyle.Fill,
        UseSystemPasswordChar = true,
        Margin = Padding.Empty,
        BackColor = AppTheme.Surface,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI", 10F)
    };

    private readonly EyeButton _toggle = new()
    {
        Dock = DockStyle.Right,
        Width = 38,
        TabStop = false,
        Cursor = Cursors.Hand,
        AccessibleName = "Hiện mật khẩu"
    };

    public PasswordInput()
    {
        Width = 280;
        Height = 34;
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = AppTheme.Surface;
        Padding = new Padding(10, 7, 0, 4);

        Controls.Add(_textBox);
        Controls.Add(_toggle);

        _toggle.Click += (_, _) => TogglePasswordVisibility();
        _textBox.TextChanged += (_, e) => PasswordChanged?.Invoke(this, e);
        Enter += (_, _) => BackColor = Color.FromArgb(239, 246, 255);
        Leave += (_, _) => BackColor = AppTheme.Surface;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Password
    {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool PasswordVisible => !_textBox.UseSystemPasswordChar;

    public event EventHandler? PasswordChanged;

    public void FocusInput() => _textBox.Focus();

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
            BackColor = AppTheme.Surface;
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
