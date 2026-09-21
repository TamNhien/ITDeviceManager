using Microsoft.Data.SqlClient;

namespace ITDeviceManager.Data;

public static class DatabaseLocationService
{
    public static string DatabaseName
    {
        get
        {
            var builder = new SqlConnectionStringBuilder(AppSettings.ConnectionString);
            return string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "ITDeviceManagerDb" : builder.InitialCatalog;
        }
    }

    public static string DatabaseFilesDirectory
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("ITDM_DATABASE_FILES_DIRECTORY")?.Trim();
            if (!string.IsNullOrWhiteSpace(configured))
                return Path.GetFullPath(Environment.ExpandEnvironmentVariables(configured));

            var root = FindSolutionRoot();
            return Path.Combine(root ?? AppContext.BaseDirectory, "DatabaseFiles");
        }
    }

    public static string DataFilePath => Path.Combine(DatabaseFilesDirectory, $"{DatabaseName}.mdf");
    public static string LogFilePath => Path.Combine(DatabaseFilesDirectory, $"{DatabaseName}_log.ldf");

    public static async Task EnsureDatabaseExistsAtConfiguredLocationAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildMasterConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using (var exists = connection.CreateCommand())
        {
            exists.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE name=@name;";
            exists.Parameters.AddWithValue("@name", DatabaseName);
            if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) > 0)
                return;
        }

        Directory.CreateDirectory(DatabaseFilesDirectory);

        var quotedDb = QuoteIdentifier(DatabaseName);
        var logicalData = QuoteSqlLiteral(DatabaseName);
        var logicalLog = QuoteSqlLiteral(DatabaseName + "_log");
        var dataPath = QuoteSqlLiteral(DataFilePath);
        var logPath = QuoteSqlLiteral(LogFilePath);

        await using var create = connection.CreateCommand();
        create.CommandTimeout = 60;
        create.CommandText = $"""
CREATE DATABASE {quotedDb}
ON PRIMARY
(
    NAME = N'{logicalData}',
    FILENAME = N'{dataPath}'
)
LOG ON
(
    NAME = N'{logicalLog}',
    FILENAME = N'{logPath}'
);
""";

        try
        {
            await create.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 5120 or 5133 or 5170)
        {
            throw new InvalidOperationException(
                "SQL Server không thể tạo file database trong thư mục cấu hình. " +
                "Hãy cấp quyền Modify/Full Control cho tài khoản dịch vụ SQL Server đối với thư mục:\n" +
                DatabaseFilesDirectory +
                "\n\nVí dụ instance mặc định: NT SERVICE\\MSSQLSERVER.\n\n" + ex.Message,
                ex);
        }
    }

    public static async Task<(string DataPath, string LogPath)> GetCurrentPhysicalPathsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildMasterConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT type_desc, physical_name
FROM sys.master_files
WHERE database_id = DB_ID(@databaseName)
ORDER BY file_id;
""";
        command.Parameters.AddWithValue("@databaseName", DatabaseName);

        string? data = null;
        string? log = null;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var type = reader.GetString(0);
            var path = reader.GetString(1);
            if (type == "ROWS" && data is null) data = path;
            if (type == "LOG" && log is null) log = path;
        }

        return (
            string.IsNullOrWhiteSpace(data) ? DataFilePath : data,
            string.IsNullOrWhiteSpace(log) ? LogFilePath : log);
    }

    private static string BuildMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(AppSettings.ConnectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = 30
        };
        return builder.ConnectionString;
    }

    private static string? FindSolutionRoot()
    {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            DirectoryInfo? directory = new DirectoryInfo(start);
            for (var i = 0; directory is not null && i < 8; i++, directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ITDeviceManager.sln")))
                    return directory.FullName;
            }
        }
        return null;
    }

    private static string QuoteIdentifier(string identifier)
        => $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    private static string QuoteSqlLiteral(string value)
        => value.Replace("'", "''", StringComparison.Ordinal);
}
