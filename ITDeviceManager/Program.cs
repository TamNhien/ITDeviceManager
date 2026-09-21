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
            // V1.9.0: when the database does not exist yet, create its MDF/LDF in
            // DatabaseFiles instead of SQL Server's Program Files DATA directory.
            DatabaseLocationService.EnsureDatabaseExistsAtConfiguredLocationAsync().GetAwaiter().GetResult();

            using var db = new AppDbContext();

            // V2.1.0 query filters reference soft-delete columns, so create those
            // columns before any EF query runs against an older database.
            SchemaUpgradeV210.EnsureColumnsAsync(db).GetAwaiter().GetResult();

            // Run schema upgrades before EF starts using columns introduced by newer versions.
            // V1.3.x removes legacy PasswordSalt, normalizes phone numbers, then seeds sample data.
            SchemaUpgradeV125.UpgradeAsync(db).GetAwaiter().GetResult();
            DbInitializer.InitializeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV130.UpgradeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV132.UpgradeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV133.UpgradeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV134.UpgradeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV135.UpgradeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV143.UpgradeAsync(db).GetAwaiter().GetResult();
            SchemaUpgradeV150.UpgradeAsync(db).GetAwaiter().GetResult();
            SampleDataSeeder.SeedAsync(db).GetAwaiter().GetResult();
            // Seed roles first, then attach the V1.9.0 default permission matrix.
            SchemaUpgradeV190.UpgradeAsync(db).GetAwaiter().GetResult();
            // V2.0.0 adds QR/Barcode permissions without changing device data.
            SchemaUpgradeV200.UpgradeAsync(db).GetAwaiter().GetResult();
            // V2.1.0 finishes soft-delete indexes and Thùng rác permissions.
            SchemaUpgradeV210.UpgradeAsync(db).GetAwaiter().GetResult();
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
