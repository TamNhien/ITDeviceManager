using System.ComponentModel;
using System.Globalization;

namespace ITDeviceManager.Common;

/// <summary>
/// Culture-stable dd/MM/yyyy editor backed by the project's TextInput rather
/// than the native Win32 DateTimePicker. It keeps date entry visually aligned
/// and avoids visual-style handle failures on affected Windows/.NET setups.
/// </summary>
public sealed class DateInput : UserControl
{
    private const string DatePattern = "dd/MM/yyyy";
    private bool _internalUpdate;

    private readonly TextInput _input = new()
    {
        Dock = DockStyle.Fill,
        Height = 36,
        PlaceholderText = DatePattern,
        TextAlign = HorizontalAlignment.Center,
        Margin = Padding.Empty
    };

    public DateInput()
    {
        Width = 180;
        Height = 36;
        MinimumSize = new Size(80, 36);
        Padding = Padding.Empty;
        Margin = new Padding(3, 4, 3, 4);
        TabStop = false;
        BackColor = AppTheme.InputSurface;

        Controls.Add(_input);
        _input.TextChanged += (_, _) =>
        {
            if (!_internalUpdate)
                ValueChanged?.Invoke(this, EventArgs.Empty);
        };

        Value = DateTime.Today;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime Value
    {
        get => TryGetValue(out var value) ? value : DateTime.Today;
        set
        {
            _internalUpdate = true;
            try
            {
                _input.Text = value.Date.ToString(DatePattern, CultureInfo.InvariantCulture);
            }
            finally
            {
                _internalUpdate = false;
            }

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ValueChanged;

    public bool TryGetValue(out DateTime value)
    {
        return DateTime.TryParseExact(
            _input.Text.Trim(),
            DatePattern,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out value);
    }

    public void FocusInput() => _input.FocusInput();
    public void SelectAll() => _input.SelectAll();
}
