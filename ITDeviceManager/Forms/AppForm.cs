using ITDeviceManager.Common;

namespace ITDeviceManager.Forms;

public class AppForm : Form
{
    protected AppForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.TextPrimary;
        Font = new Font("Segoe UI", 10F);

        try
        {
            var executable = Application.ExecutablePath;
            if (!string.IsNullOrWhiteSpace(executable) && File.Exists(executable))
                Icon = Icon.ExtractAssociatedIcon(executable);
        }
        catch
        {
            // Keep the standard Windows icon if the executable icon cannot be loaded.
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // Apply dark colors/styles as soon as the form HWND exists, while the form
        // is still not visible. This prevents native child controls from painting
        // one light/default frame before OnShown.
        AppTheme.ApplyForm(this);
    }

    protected override void OnLoad(EventArgs e)
    {
        // First pass styles every control created by the form constructor before the
        // first visible paint. base.OnLoad raises Load handlers; a second pass then
        // covers controls that may have been added synchronously by those handlers.
        AppTheme.ApplyForm(this);
        base.OnLoad(e);
        AppTheme.ApplyForm(this);
    }
}
