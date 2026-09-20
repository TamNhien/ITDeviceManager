using ITDeviceManager.Data;
using ITDeviceManager.Forms;
using ITDeviceManager.Services;

namespace ITDeviceManager;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Load .env before any AppSettings values are read. Existing Windows environment variables win.
            EnvFileLoader.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể đọc file .env.\n\n" + ex.Message,
                "Lỗi cấu hình",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
        ProtocolRegistrar.RegisterForCurrentUser();

        try
        {
            using var db = new AppDbContext();
            DbInitializer.InitializeAsync(db).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể khởi tạo cơ sở dữ liệu.\n\n" +
                "Hãy kiểm tra SQL Server 'CANHTHIEN', Windows Authentication và ITDM_CONNECTION_STRING trong .env/AppSettings.cs.\n\n" +
                ex.Message,
                "Lỗi cơ sở dữ liệu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // Password-reset links launch the application using a private URI scheme.
        // The token is single-use and only its SHA-256 hash is stored in SQL Server.
        if (ProtocolRegistrar.TryGetResetToken(args, out var resetToken))
        {
            using var resetForm = new ResetPasswordForm(resetToken);
            resetForm.ShowDialog();
            return;
        }

        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK)
                break;

            using var main = new MainForm();
            main.ShowDialog();

            if (!main.LogoutRequested)
                break;
        }
    }
}
