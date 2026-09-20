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

    protected override void OnShown(EventArgs e)
    {
        AppTheme.ApplyForm(this);
        base.OnShown(e);
    }
}
