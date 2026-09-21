using System.Diagnostics;
using ITDeviceManager.Common;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public sealed class BackupRestoreForm : AppForm
{
    private readonly Label _serverValue = CreateValueLabel();
    private readonly Label _databaseValue = CreateValueLabel();
    private readonly Label _sqlBackupPathValue = CreateValueLabel();
    private readonly Label _status = new()
    {
        AutoSize = false,
        Height = 28,
        Dock = DockStyle.Bottom,
        ForeColor = AppTheme.TextSecondary,
        TextAlign = ContentAlignment.MiddleLeft,
        Text = "Sẵn sàng."
    };

    private readonly TextBox _restorePath = new()
    {
        ReadOnly = true,
        Dock = DockStyle.Fill,
        PlaceholderText = "Chọn file .bak cần phục hồi..."
    };

    private readonly CheckBox _safetyBackup = new()
    {
        AutoSize = true,
        Checked = true,
        Text = "Tạo bản sao lưu an toàn trước khi phục hồi",
        ForeColor = AppTheme.TextPrimary,
        BackColor = Color.Transparent
    };

    private readonly Button _backupButton = new() { Text = "Sao lưu ngay...", Width = 150, Height = 38 };
    private readonly Button _openFolderButton = new() { Text = "Mở thư mục backup", Width = 160, Height = 38 };
    private readonly Button _browseRestoreButton = new() { Text = "Chọn file...", Width = 120, Height = 34 };
    private readonly Button _verifyButton = new() { Text = "Kiểm tra backup", Width = 150, Height = 38 };
    private readonly Button _restoreButton = new() { Text = "Phục hồi database", Width = 170, Height = 38 };

    public BackupRestoreForm()
    {
        Text = "Sao lưu / Phục hồi SQL Server";
        BackColor = AppTheme.Background;
        Padding = new Padding(4);

        AppTheme.SetButtonRole(_backupButton, ButtonRole.Primary);
        AppTheme.SetButtonRole(_openFolderButton, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_browseRestoreButton, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_verifyButton, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_restoreButton, ButtonRole.Danger);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = AppTheme.Background,
            Padding = new Padding(8),
            AutoScroll = true
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 255));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(BuildIntro(), 0, 0);
        root.Controls.Add(BuildBackupCard(), 0, 1);
        root.Controls.Add(BuildRestoreCard(), 0, 2);

        Controls.Add(root);
        Controls.Add(_status);

        _backupButton.Click += async (_, _) => await BackupAsync();
        _openFolderButton.Click += (_, _) => OpenBackupFolder();
        _browseRestoreButton.Click += (_, _) => BrowseRestoreFile();
        _verifyButton.Click += async (_, _) => await VerifySelectedBackupAsync(showSuccessMessage: true);
        _restoreButton.Click += async (_, _) => await RestoreAsync();

        Shown += async (_, _) => await LoadSqlInfoAsync();
    }

    private Control BuildIntro()
    {
        var card = NewCard();
        var title = new Label
        {
            Text = "Backup / Restore SQL Server",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 15F),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(18, 16)
        };
        var description = new Label
        {
            Text = "Sao lưu database thành file .bak, kiểm tra tính hợp lệ và phục hồi an toàn khi cần.",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(20, 49)
        };

        var serverLabel = CreateCaption("SQL Server:", new Point(20, 76));
        _serverValue.Location = new Point(105, 76);
        _serverValue.Width = 260;

        var dbLabel = CreateCaption("Database:", new Point(390, 76));
        _databaseValue.Location = new Point(465, 76);
        _databaseValue.Width = 240;

        card.Controls.Add(title);
        card.Controls.Add(description);
        card.Controls.Add(serverLabel);
        card.Controls.Add(_serverValue);
        card.Controls.Add(dbLabel);
        card.Controls.Add(_databaseValue);
        return card;
    }

    private Control BuildBackupCard()
    {
        var card = NewCard();
        card.Padding = new Padding(18);

        var title = new Label
        {
            Text = "Sao lưu cơ sở dữ liệu",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = AppTheme.TextPrimary
        };
        var note = new Label
        {
            Text = "Backup dùng COPY_ONLY + CHECKSUM và được RESTORE VERIFYONLY ngay sau khi tạo.",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = AppTheme.TextSecondary
        };
        var pathPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.Transparent
        };
        pathPanel.Controls.Add(CreateCaption("Thư mục mặc định:", new Point(0, 12)));
        _sqlBackupPathValue.Location = new Point(140, 12);
        _sqlBackupPathValue.Width = 780;
        pathPanel.Controls.Add(_sqlBackupPathValue);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 6, 0, 0)
        };
        buttons.Controls.Add(_backupButton);
        buttons.Controls.Add(_openFolderButton);

        card.Controls.Add(buttons);
        card.Controls.Add(pathPanel);
        card.Controls.Add(note);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildRestoreCard()
    {
        var card = NewCard();
        card.Padding = new Padding(18);

        var title = new Label
        {
            Text = "Phục hồi cơ sở dữ liệu",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = AppTheme.TextPrimary
        };
        var warning = new Label
        {
            Text = "Cảnh báo: Phục hồi sẽ thay thế toàn bộ dữ liệu hiện tại. Chỉ Admin mới được thực hiện thao tác này.",
            Dock = DockStyle.Top,
            Height = 30,
            ForeColor = Color.FromArgb(251, 191, 36)
        };

        var fileRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            ColumnCount = 3,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 6, 0, 5)
        };
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        var fileCaption = new Label
        {
            Text = "File .bak:",
            Dock = DockStyle.Fill,
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _restorePath.Margin = new Padding(0, 2, 10, 2);
        _browseRestoreButton.Dock = DockStyle.Fill;
        fileRow.Controls.Add(fileCaption, 0, 0);
        fileRow.Controls.Add(_restorePath, 1, 0);
        fileRow.Controls.Add(_browseRestoreButton, 2, 0);

        var safetyPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 42,
            BackColor = Color.Transparent
        };
        _safetyBackup.Location = new Point(0, 10);
        safetyPanel.Controls.Add(_safetyBackup);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 54,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 7, 0, 0)
        };
        buttons.Controls.Add(_verifyButton);
        buttons.Controls.Add(_restoreButton);

        var footer = new Label
        {
            Text = "Sau khi phục hồi thành công, ứng dụng sẽ khởi động lại để tự chạy các nâng cấp schema cần thiết.",
            Dock = DockStyle.Top,
            Height = 30,
            ForeColor = AppTheme.TextSecondary
        };

        card.Controls.Add(footer);
        card.Controls.Add(buttons);
        card.Controls.Add(safetyPanel);
        card.Controls.Add(fileRow);
        card.Controls.Add(warning);
        card.Controls.Add(title);
        return card;
    }

    private async Task LoadSqlInfoAsync()
    {
        _serverValue.Text = DatabaseBackupService.ServerName;
        _databaseValue.Text = DatabaseBackupService.DatabaseName;

        try
        {
            var sqlPath = await DatabaseBackupService.GetSqlServerDefaultBackupDirectoryAsync();
            _sqlBackupPathValue.Text = string.IsNullOrWhiteSpace(sqlPath)
                ? DatabaseBackupService.DefaultUserBackupDirectory
                : sqlPath;
        }
        catch
        {
            _sqlBackupPathValue.Text = DatabaseBackupService.DefaultUserBackupDirectory;
        }
    }

    private async Task BackupAsync()
    {
        Directory.CreateDirectory(DatabaseBackupService.DefaultUserBackupDirectory);
        using var dialog = new SaveFileDialog
        {
            Title = "Lưu bản sao lưu SQL Server",
            Filter = "SQL Server backup (*.bak)|*.bak|Tất cả file (*.*)|*.*",
            DefaultExt = "bak",
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = DatabaseBackupService.DefaultUserBackupDirectory,
            FileName = DatabaseBackupService.SuggestBackupFileName()
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        await RunBusyAsync("Đang sao lưu database...", async () =>
        {
            var result = await DatabaseBackupService.BackupAsync(dialog.FileName);
            var finalPath = result.CopiedToRequestedPath ? result.RequestedPath : result.SqlBackupPath;

            await AuditService.TryWriteAsync(
                "Sao lưu",
                "Cơ sở dữ liệu",
                $"Sao lưu database {DatabaseBackupService.DatabaseName} ra file {Path.GetFileName(finalPath)}.",
                DatabaseBackupService.DatabaseName);

            var sizeText = FormatBytes(result.BackupSizeBytes);
            var message = result.CopiedToRequestedPath
                ? $"Sao lưu thành công và đã kiểm tra backup.\n\nFile: {finalPath}\nDung lượng: {sizeText}"
                : $"SQL Server đã sao lưu thành công và kiểm tra backup, nhưng Windows không cho ứng dụng sao chép file tới thư mục bạn chọn.\n\nFile đang nằm tại:\n{result.SqlBackupPath}\n\nDung lượng: {sizeText}";

            MessageBox.Show(this, message, "Sao lưu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _status.Text = $"Sao lưu thành công: {Path.GetFileName(finalPath)}";
        });
    }

    private void BrowseRestoreFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn file backup SQL Server",
            Filter = "SQL Server backup (*.bak)|*.bak|Tất cả file (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            InitialDirectory = Directory.Exists(DatabaseBackupService.DefaultUserBackupDirectory)
                ? DatabaseBackupService.DefaultUserBackupDirectory
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _restorePath.Text = dialog.FileName;
            _status.Text = "Đã chọn file backup. Nên bấm 'Kiểm tra backup' trước khi phục hồi.";
        }
    }

    private async Task<DatabaseBackupInfo?> VerifySelectedBackupAsync(bool showSuccessMessage)
    {
        var path = _restorePath.Text.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show(this, "Hãy chọn file .bak trước.", "Chưa chọn file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        DatabaseBackupInfo? result = null;
        await RunBusyAsync("Đang kiểm tra file backup...", async () =>
        {
            result = await DatabaseBackupService.VerifyUserSelectedBackupAsync(path);
            if (!string.Equals(result.DatabaseName, DatabaseBackupService.DatabaseName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Backup thuộc database '{result.DatabaseName}', không phải '{DatabaseBackupService.DatabaseName}'.");
            }

            _status.Text = $"Backup hợp lệ: {result.DatabaseName} - {FormatBytes(result.BackupSizeBytes)}";
            if (showSuccessMessage)
            {
                MessageBox.Show(
                    this,
                    $"File backup hợp lệ.\n\nDatabase: {result.DatabaseName}\nLoại: {result.BackupType}\nThời gian: {result.BackupFinishDate:dd/MM/yyyy HH:mm:ss}\nDung lượng: {FormatBytes(result.BackupSizeBytes)}",
                    "Kiểm tra backup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        });
        return result;
    }

    private async Task RestoreAsync()
    {
        if (!AppSession.IsAdmin)
        {
            MessageBox.Show(this, "Chỉ Admin được phép phục hồi database.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var path = _restorePath.Text.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show(this, "Hãy chọn file .bak cần phục hồi.", "Chưa chọn file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var info = await VerifySelectedBackupAsync(showSuccessMessage: false);
        if (info is null)
            return;

        var confirm = MessageBox.Show(
            this,
            $"Bạn sắp PHỤC HỒI database '{DatabaseBackupService.DatabaseName}'.\n\n" +
            "Toàn bộ dữ liệu hiện tại sẽ bị thay thế bằng dữ liệu trong file backup.\n" +
            (_safetyBackup.Checked
                ? "Ứng dụng sẽ tạo một backup an toàn của dữ liệu hiện tại trước khi phục hồi."
                : "Bạn đã TẮT backup an toàn trước khi phục hồi.") +
            "\n\nBạn có chắc chắn muốn tiếp tục?",
            "Xác nhận phục hồi database",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        if (!_safetyBackup.Checked)
        {
            var secondConfirm = MessageBox.Show(
                this,
                "Backup an toàn đang bị tắt. Nếu file phục hồi có vấn đề, bạn có thể mất dữ liệu hiện tại.\n\nVẫn tiếp tục?",
                "Xác nhận lần cuối",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop,
                MessageBoxDefaultButton.Button2);
            if (secondConfirm != DialogResult.Yes)
                return;
        }

        await RunBusyAsync("Đang phục hồi database. Không tắt ứng dụng...", async () =>
        {
            if (_safetyBackup.Checked)
            {
                Directory.CreateDirectory(DatabaseBackupService.DefaultUserBackupDirectory);
                var safetyPath = Path.Combine(
                    DatabaseBackupService.DefaultUserBackupDirectory,
                    DatabaseBackupService.SuggestBackupFileName("before_restore"));
                var safetyResult = await DatabaseBackupService.BackupAsync(safetyPath);
                var safetyFinal = safetyResult.CopiedToRequestedPath ? safetyResult.RequestedPath : safetyResult.SqlBackupPath;
                await AuditService.TryWriteAsync(
                    "Sao lưu an toàn",
                    "Cơ sở dữ liệu",
                    $"Tạo backup an toàn trước phục hồi: {Path.GetFileName(safetyFinal)}.",
                    DatabaseBackupService.DatabaseName);
            }

            await DatabaseBackupService.RestoreAsync(path);
            await AuditService.TryWriteAsync(
                "Phục hồi",
                "Cơ sở dữ liệu",
                $"Phục hồi database {DatabaseBackupService.DatabaseName} từ file {Path.GetFileName(path)}.",
                DatabaseBackupService.DatabaseName);

            MessageBox.Show(
                this,
                "Phục hồi database thành công. Ứng dụng sẽ khởi động lại để kiểm tra và nâng cấp schema nếu cần.",
                "Phục hồi thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Application.Restart();
        });
    }

    private async Task RunBusyAsync(string message, Func<Task> action)
    {
        SetBusy(true, message);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _status.Text = "Thao tác thất bại.";
            MessageBox.Show(this, ex.Message, "Lỗi Backup / Restore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, _status.Text);
        }
    }

    private void SetBusy(bool busy, string statusText)
    {
        _backupButton.Enabled = !busy;
        _openFolderButton.Enabled = !busy;
        _browseRestoreButton.Enabled = !busy;
        _verifyButton.Enabled = !busy;
        _restoreButton.Enabled = !busy;
        _safetyBackup.Enabled = !busy;
        UseWaitCursor = busy;
        _status.Text = statusText;
        _status.ForeColor = busy ? AppTheme.Info : AppTheme.TextSecondary;
    }

    private void OpenBackupFolder()
    {
        try
        {
            var directory = DatabaseBackupService.DefaultUserBackupDirectory;
            Directory.CreateDirectory(directory);
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể mở thư mục", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static ModernCard NewCard() => new()
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 0, 0, 12),
        BackColor = AppTheme.Surface,
        BorderColor = AppTheme.BorderStrong,
        CornerRadius = 16
    };

    private static Label CreateCaption(string text, Point location) => new()
    {
        Text = text,
        AutoSize = true,
        Location = location,
        ForeColor = AppTheme.TextSecondary,
        Font = new Font("Segoe UI", 9.5F)
    };

    private static Label CreateValueLabel() => new()
    {
        AutoSize = false,
        Height = 24,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI Semibold", 9.5F),
        AutoEllipsis = true
    };

    private static string FormatBytes(long? bytes)
    {
        if (bytes is null || bytes < 0)
            return "Không xác định";

        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes.Value;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }
}
