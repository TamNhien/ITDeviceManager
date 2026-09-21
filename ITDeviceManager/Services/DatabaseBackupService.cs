using Microsoft.Data.SqlClient;

namespace ITDeviceManager.Services;

public sealed record DatabaseBackupInfo(
    string DatabaseName,
    string BackupType,
    DateTime? BackupFinishDate,
    long? BackupSizeBytes,
    string FilePath);

public sealed record DatabaseBackupResult(
    string RequestedPath,
    string SqlBackupPath,
    bool CopiedToRequestedPath,
    long? BackupSizeBytes);

public static class DatabaseBackupService
{
    public static string DatabaseName
    {
        get
        {
            var builder = new SqlConnectionStringBuilder(AppSettings.ConnectionString);
            return string.IsNullOrWhiteSpace(builder.InitialCatalog)
                ? "ITDeviceManagerDb"
                : builder.InitialCatalog;
        }
    }

    public static string ServerName
    {
        get
        {
            var builder = new SqlConnectionStringBuilder(AppSettings.ConnectionString);
            return string.IsNullOrWhiteSpace(builder.DataSource) ? "(không xác định)" : builder.DataSource;
        }
    }

    public static string DefaultUserBackupDirectory
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("ITDM_BACKUP_DIRECTORY")?.Trim();
            if (!string.IsNullOrWhiteSpace(configured))
                return Path.GetFullPath(Environment.ExpandEnvironmentVariables(configured));

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "ITDeviceManager", "Backups");
        }
    }

    public static string SuggestBackupFileName(string? suffix = null)
    {
        var safeSuffix = string.IsNullOrWhiteSpace(suffix)
            ? string.Empty
            : "_" + SanitizeFileNamePart(suffix.Trim());
        return $"{SanitizeFileNamePart(DatabaseName)}{safeSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
    }

    public static async Task<string?> GetSqlServerDefaultBackupDirectoryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildMasterConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 30;
        command.CommandText = "SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'));";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        var path = result == DBNull.Value ? null : Convert.ToString(result)?.Trim();
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    public static async Task<DatabaseBackupResult> BackupAsync(
        string requestedPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestedPath))
            throw new ArgumentException("Đường dẫn sao lưu không hợp lệ.", nameof(requestedPath));

        requestedPath = Path.GetFullPath(requestedPath);
        var requestedDirectory = Path.GetDirectoryName(requestedPath)
            ?? throw new InvalidOperationException("Không xác định được thư mục sao lưu.");
        Directory.CreateDirectory(requestedDirectory);

        try
        {
            await ExecuteBackupToSqlPathAsync(requestedPath, cancellationToken);
            await VerifyBackupAsync(requestedPath, cancellationToken);
            return new DatabaseBackupResult(
                requestedPath,
                requestedPath,
                true,
                TryGetLocalFileSize(requestedPath));
        }
        catch (SqlException directError)
        {
            var sqlDefaultDirectory = await GetSqlServerDefaultBackupDirectoryAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(sqlDefaultDirectory))
                throw BuildBackupPermissionException(requestedPath, directError, null);

            var stagingPath = Path.Combine(sqlDefaultDirectory, Path.GetFileName(requestedPath));
            if (string.Equals(stagingPath, requestedPath, StringComparison.OrdinalIgnoreCase))
                throw BuildBackupPermissionException(requestedPath, directError, sqlDefaultDirectory);

            await ExecuteBackupToSqlPathAsync(stagingPath, cancellationToken);
            await VerifyBackupAsync(stagingPath, cancellationToken);

            try
            {
                File.Copy(stagingPath, requestedPath, overwrite: true);
                var size = TryGetLocalFileSize(requestedPath) ?? TryGetLocalFileSize(stagingPath);
                TryDeleteLocalFile(stagingPath);
                return new DatabaseBackupResult(requestedPath, stagingPath, true, size);
            }
            catch (Exception copyError) when (copyError is IOException or UnauthorizedAccessException)
            {
                return new DatabaseBackupResult(
                    requestedPath,
                    stagingPath,
                    false,
                    TryGetLocalFileSize(stagingPath));
            }
        }
    }

    public static async Task<DatabaseBackupInfo> VerifyUserSelectedBackupAsync(
        string selectedBackupPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(selectedBackupPath))
            throw new ArgumentException("Đường dẫn file backup không hợp lệ.", nameof(selectedBackupPath));

        selectedBackupPath = Path.GetFullPath(selectedBackupPath);
        if (!File.Exists(selectedBackupPath))
            throw new FileNotFoundException("Không tìm thấy file backup đã chọn.", selectedBackupPath);

        var sqlReadablePath = await EnsureSqlServerCanReadBackupAsync(selectedBackupPath, cancellationToken);
        var staged = !string.Equals(sqlReadablePath, selectedBackupPath, StringComparison.OrdinalIgnoreCase);
        try
        {
            var info = await VerifyBackupAsync(sqlReadablePath, cancellationToken);
            return info with { FilePath = selectedBackupPath };
        }
        finally
        {
            if (staged)
                TryDeleteLocalFile(sqlReadablePath);
        }
    }

    public static async Task<DatabaseBackupInfo> VerifyBackupAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupPath))
            throw new ArgumentException("Đường dẫn file backup không hợp lệ.", nameof(backupPath));

        backupPath = Path.GetFullPath(backupPath);
        await using var connection = new SqlConnection(BuildMasterConnectionString());
        await connection.OpenAsync(cancellationToken);

        string databaseName;
        string backupType;
        DateTime? finishDate;
        long? backupSize;

        await using (var header = connection.CreateCommand())
        {
            header.CommandTimeout = 60;
            header.CommandText = "RESTORE HEADERONLY FROM DISK = @path;";
            header.Parameters.AddWithValue("@path", backupPath);

            await using var reader = await header.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("File backup không chứa bộ sao lưu SQL Server hợp lệ.");

            databaseName = Convert.ToString(reader["DatabaseName"])?.Trim() ?? string.Empty;
            backupType = Convert.ToString(reader["BackupTypeDescription"])?.Trim() ?? "Database";
            finishDate = reader["BackupFinishDate"] is DBNull
                ? null
                : Convert.ToDateTime(reader["BackupFinishDate"]);
            backupSize = reader["BackupSize"] is DBNull
                ? null
                : Convert.ToInt64(reader["BackupSize"]);
        }

        await using (var verify = connection.CreateCommand())
        {
            verify.CommandTimeout = 0;
            verify.CommandText = "RESTORE VERIFYONLY FROM DISK = @path WITH CHECKSUM;";
            verify.Parameters.AddWithValue("@path", backupPath);
            await verify.ExecuteNonQueryAsync(cancellationToken);
        }

        return new DatabaseBackupInfo(databaseName, backupType, finishDate, backupSize, backupPath);
    }

    public static async Task RestoreAsync(
        string selectedBackupPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(selectedBackupPath))
            throw new ArgumentException("Chưa chọn file backup.", nameof(selectedBackupPath));

        selectedBackupPath = Path.GetFullPath(selectedBackupPath);
        if (!File.Exists(selectedBackupPath))
            throw new FileNotFoundException("Không tìm thấy file backup đã chọn.", selectedBackupPath);

        var sqlReadablePath = await EnsureSqlServerCanReadBackupAsync(selectedBackupPath, cancellationToken);
        var staged = !string.Equals(sqlReadablePath, selectedBackupPath, StringComparison.OrdinalIgnoreCase);

        try
        {
            var info = await VerifyBackupAsync(sqlReadablePath, cancellationToken);
            if (!string.Equals(info.DatabaseName, DatabaseName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"File backup thuộc database '{info.DatabaseName}', không phải '{DatabaseName}'. " +
                    "Để an toàn, ứng dụng chỉ cho phép phục hồi đúng database hiện tại.");
            }

            SqlConnection.ClearAllPools();
            await using var connection = new SqlConnection(BuildMasterConnectionString());
            await connection.OpenAsync(cancellationToken);

            var quotedDatabase = QuoteIdentifier(DatabaseName);
            try
            {
                await using var restore = connection.CreateCommand();
                restore.CommandTimeout = 0;
                restore.CommandText = $"""
                    ALTER DATABASE {quotedDatabase} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE {quotedDatabase}
                    FROM DISK = @path
                    WITH REPLACE, RECOVERY;
                    ALTER DATABASE {quotedDatabase} SET MULTI_USER;
                    """;
                restore.Parameters.AddWithValue("@path", sqlReadablePath);
                await restore.ExecuteNonQueryAsync(cancellationToken);
            }
            catch
            {
                try
                {
                    await using var recover = connection.CreateCommand();
                    recover.CommandTimeout = 60;
                    recover.CommandText = $"ALTER DATABASE {quotedDatabase} SET MULTI_USER WITH ROLLBACK IMMEDIATE;";
                    await recover.ExecuteNonQueryAsync(cancellationToken);
                }
                catch
                {
                    // Preserve the original restore exception. A restart or DBA intervention
                    // may be required if SQL Server could not switch the database back.
                }

                throw;
            }
            finally
            {
                SqlConnection.ClearAllPools();
            }
        }
        finally
        {
            if (staged)
                TryDeleteLocalFile(sqlReadablePath);
        }
    }

    private static async Task ExecuteBackupToSqlPathAsync(string sqlPath, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(BuildMasterConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 0;
        command.CommandText = $"""
            BACKUP DATABASE {QuoteIdentifier(DatabaseName)}
            TO DISK = @path
            WITH COPY_ONLY, INIT, CHECKSUM,
                 NAME = @name,
                 DESCRIPTION = @description,
                 STATS = 10;
            """;
        command.Parameters.AddWithValue("@path", sqlPath);
        command.Parameters.AddWithValue("@name", $"IT Device Manager {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        command.Parameters.AddWithValue("@description", $"Backup database {DatabaseName} từ IT Device Manager");
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string> EnsureSqlServerCanReadBackupAsync(
        string selectedPath,
        CancellationToken cancellationToken)
    {
        try
        {
            await ReadBackupHeaderOnlyAsync(selectedPath, cancellationToken);
            return selectedPath;
        }
        catch (SqlException directError)
        {
            var sqlDefaultDirectory = await GetSqlServerDefaultBackupDirectoryAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(sqlDefaultDirectory))
                throw BuildRestorePermissionException(selectedPath, directError, null);

            var stagedName = $"ITDM_restore_{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(selectedPath)}";
            var stagedPath = Path.Combine(sqlDefaultDirectory, SanitizeFileNamePart(stagedName, keepExtension: true));

            try
            {
                File.Copy(selectedPath, stagedPath, overwrite: true);
            }
            catch (Exception copyError) when (copyError is IOException or UnauthorizedAccessException)
            {
                throw BuildRestorePermissionException(selectedPath, directError, sqlDefaultDirectory, copyError);
            }

            try
            {
                await ReadBackupHeaderOnlyAsync(stagedPath, cancellationToken);
                return stagedPath;
            }
            catch
            {
                TryDeleteLocalFile(stagedPath);
                throw;
            }
        }
    }

    private static async Task ReadBackupHeaderOnlyAsync(string path, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(BuildMasterConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 60;
        command.CommandText = "RESTORE HEADERONLY FROM DISK = @path;";
        command.Parameters.AddWithValue("@path", path);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("File backup không hợp lệ hoặc không đọc được header.");
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

    private static string QuoteIdentifier(string identifier)
        => $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    private static Exception BuildBackupPermissionException(
        string requestedPath,
        SqlException sqlError,
        string? sqlDefaultDirectory)
    {
        var detail = string.IsNullOrWhiteSpace(sqlDefaultDirectory)
            ? string.Empty
            : $"\nThư mục backup mặc định của SQL Server: {sqlDefaultDirectory}";
        return new InvalidOperationException(
            "SQL Server không thể ghi file backup vào đường dẫn đã chọn. " +
            "Tài khoản dịch vụ SQL Server cần quyền ghi vào thư mục đích." + detail +
            $"\n\nĐường dẫn: {requestedPath}\n\nSQL Server: {sqlError.Message}",
            sqlError);
    }

    private static Exception BuildRestorePermissionException(
        string selectedPath,
        SqlException sqlError,
        string? sqlDefaultDirectory,
        Exception? copyError = null)
    {
        var detail = string.IsNullOrWhiteSpace(sqlDefaultDirectory)
            ? string.Empty
            : $"\nThư mục backup mặc định của SQL Server: {sqlDefaultDirectory}";
        var copyDetail = copyError is null ? string.Empty : $"\nKhông thể sao chép file vào thư mục SQL Server: {copyError.Message}";
        return new InvalidOperationException(
            "SQL Server không thể đọc file backup đã chọn. " +
            "Hãy cấp quyền đọc file cho tài khoản dịch vụ SQL Server hoặc đặt file trong thư mục backup của SQL Server." +
            detail + copyDetail +
            $"\n\nFile: {selectedPath}\n\nSQL Server: {sqlError.Message}",
            sqlError);
    }

    private static long? TryGetLocalFileSize(string path)
    {
        try
        {
            return File.Exists(path) ? new FileInfo(path).Length : null;
        }
        catch
        {
            return null;
        }
    }

    private static void TryDeleteLocalFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // A staging .bak can be removed manually if Windows ACLs prevent cleanup.
        }
    }

    private static string SanitizeFileNamePart(string value, bool keepExtension = false)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        var sanitized = new string(chars);
        if (keepExtension)
            return sanitized;
        return Path.GetFileNameWithoutExtension(sanitized);
    }
}
