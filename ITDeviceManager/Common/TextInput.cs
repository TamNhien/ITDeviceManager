using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ITDeviceManager.Common;

/// <summary>
/// Single-line text input hosted inside a fixed-height container so the text
/// remains vertically centered at different Windows DPI/font scales.
/// </summary>
public sealed class TextInput : UserControl
{
    private readonly TextBox _textBox = new()
    {
        BorderStyle = BorderStyle.None,
        Multiline = false,
        BackColor = AppTheme.Surface,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI", 10F),
        Margin = Padding.Empty,
        TabStop = false
    };

    public TextInput()
    {
        Width = 280;
        Height = 36;
        MinimumSize = new Size(0, 36);
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = AppTheme.Surface;
        Padding = Padding.Empty;
        TabStop = true;

        Controls.Add(_textBox);

        _textBox.TextChanged += (_, e) => OnTextChanged(e);
        _textBox.Enter += (_, _) => SetFocusedAppearance(true);
        _textBox.Leave += (_, _) => SetFocusedAppearance(false);
        SizeChanged += (_, _) => LayoutInnerTextBox();
        FontChanged += (_, _) =>
        {
            _textBox.Font = Font;
            LayoutInnerTextBox();
        };

        LayoutInnerTextBox();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [AllowNull]
    public override string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value ?? string.Empty;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectionStart
    {
        get => _textBox.SelectionStart;
        set => _textBox.SelectionStart = Math.Clamp(value, 0, _textBox.TextLength);
    }

    public void FocusInput() => _textBox.Focus();

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (_textBox is not null)
        {
            _textBox.Font = Font;
            LayoutInnerTextBox();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutInnerTextBox();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        _textBox.Focus();
    }

    private void LayoutInnerTextBox()
    {
        if (_textBox is null || IsDisposed) return;

        const int horizontalPadding = 10;
        var preferredHeight = _textBox.PreferredHeight;
        var top = Math.Max(0, (ClientSize.Height - preferredHeight) / 2);
        var width = Math.Max(1, ClientSize.Width - horizontalPadding * 2);

        _textBox.SetBounds(horizontalPadding, top, width, preferredHeight);
    }

    private void SetFocusedAppearance(bool focused)
    {
        var background = focused ? AppTheme.InputFocus : AppTheme.Surface;
        BackColor = background;
        _textBox.BackColor = background;
    }
}
