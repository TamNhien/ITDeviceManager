namespace ITDeviceManager.Forms;

public class AppForm : Form
{
    protected AppForm()
    {
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
}
