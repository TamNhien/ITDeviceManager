using System.ComponentModel;
using System.Globalization;

namespace ITDeviceManager.Common;

/// <summary>
/// Nullable dd/MM/yyyy editor backed only by the project TextInput.
/// An empty field represents null. Keeping this control free of native themed
/// CheckBox/DateTimePicker surfaces avoids visual-style handle creation failures
/// on affected Windows/.NET configurations.
/// </summary>
public sealed class NullableDateInput : UserControl
{
    private const string DatePattern = "dd/MM/yyyy";
    private bool _internalUpdate;

    private readonly TextInput _input = new()
    {
        Dock = DockStyle.Fill,
        Height = 36,
        PlaceholderText = DatePattern,
        TextAlign = HorizontalAlignment.Left,
        Margin = Padding.Empty
    };

    public NullableDateInput()
    {
        Width = 260;
        Height = 36;
        MinimumSize = new Size(80, 36);
        BackColor = AppTheme.InputSurface;
        Padding = Padding.Empty;
        Margin = new Padding(3, 4, 3, 4);
        TabStop = false;

        Controls.Add(_input);
        _input.TextChanged += (_, _) =>
        {
            if (!_internalUpdate)
                ValueChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime? Value
    {
        get => TryGetValue(out var value) ? value : null;
        set
        {
            _internalUpdate = true;
            try
            {
                _input.Text = value.HasValue
                    ? value.Value.Date.ToString(DatePattern, CultureInfo.InvariantCulture)
                    : string.Empty;
            }
            finally
            {
                _internalUpdate = false;
            }

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [Browsable(false)]
    public bool HasInput => !string.IsNullOrWhiteSpace(_input.Text);

    public event EventHandler? ValueChanged;

    public bool TryGetValue(out DateTime? value)
    {
        var text = _input.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            value = null;
            return true;
        }

        if (DateTime.TryParseExact(
                text,
                DatePattern,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            value = parsed.Date;
            return true;
        }

        value = null;
        return false;
    }

    public void FocusInput() => _input.FocusInput();
    public void SelectAll() => _input.SelectAll();
}
