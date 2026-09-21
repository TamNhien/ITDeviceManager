using System.Diagnostics;
using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public sealed class DeviceCodesForm : AppForm
{
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false
    };

    private readonly TextInput _search = new()
    {
        Width = 300,
        PlaceholderText = "Quét barcode/QR hoặc nhập mã, tên, serial...",
        TextAlign = HorizontalAlignment.Center
    };

    private readonly Label _deviceSummary = new()
    {
        Dock = DockStyle.Top,
        Height = 54,
        ForeColor = AppTheme.TextPrimary,
        Font = new Font("Segoe UI Semibold", 10.5F),
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(10, 0, 10, 0)
    };

    private readonly PictureBox _qrPreview = NewPreviewBox();
    private readonly PictureBox _barcodePreview = NewPreviewBox();
    private readonly Label _qrStatus = NewPreviewStatus("Chưa có file QR.");
    private readonly Label _barcodeStatus = NewPreviewStatus("Chưa có file Barcode.");
    private readonly Label _status = new()
    {
        Dock = DockStyle.Bottom,
        Height = 26,
        ForeColor = AppTheme.TextSecondary,
        TextAlign = ContentAlignment.MiddleLeft,
        Text = "Sẵn sàng."
    };

    private readonly Button _generateQr = Ui.Button("Tạo QR", 105);
    private readonly Button _generateBarcode = Ui.Button("Tạo Barcode", 125);
    private readonly Button _generateBoth = Ui.Button("Tạo cả hai", 115);
    private readonly Button _generateAll = Ui.Button("Tạo cho danh sách", 145);
    private readonly Button _openFolder = Ui.Button("Mở thư mục QR", 145);
    private readonly Button _scan = Ui.Button("Quét mã", 105);
    private readonly Button _refresh = Ui.Button("Làm mới", 100);

    private List<DeviceRow> _rows = [];
    private bool _loading;
    private bool _suppressSearchChanged;

    public DeviceCodesForm()
    {
        Text = "QR / Barcode thiết bị";
        BackColor = AppTheme.Background;
        Padding = new Padding(4);

        Ui.ConfigureGrid(_grid);
        _grid.AutoGenerateColumns = false;
        ConfigureColumns();

        AppTheme.SetButtonRole(_generateQr, ButtonRole.Primary);
        AppTheme.SetButtonRole(_generateBarcode, ButtonRole.Primary);
        AppTheme.SetButtonRole(_generateBoth, ButtonRole.Primary);
        AppTheme.SetButtonRole(_generateAll, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_openFolder, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_scan, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_refresh, ButtonRole.Secondary);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppTheme.Background,
            Padding = new Padding(6)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        root.Controls.Add(BuildToolbar(), 0, 0);
        root.Controls.Add(BuildBody(), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);

        Controls.Add(root);
        Controls.Add(_status);

        _search.TextChanged += async (_, _) =>
        {
            if (!_suppressSearchChanged)
                await LoadDataAsync();
        };
        _search.KeyDown += async (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await ResolveScannerInputAsync();
        };
        _grid.SelectionChanged += (_, _) => RefreshPreview();
        _grid.CellDoubleClick += async (_, _) => await GenerateSelectedBothAsync();

        _generateQr.Click += async (_, _) => await GenerateSelectedQrAsync();
        _generateBarcode.Click += async (_, _) => await GenerateSelectedBarcodeAsync();
        _generateBoth.Click += async (_, _) => await GenerateSelectedBothAsync();
        _generateAll.Click += async (_, _) => await GenerateVisibleAsync();
        _openFolder.Click += (_, _) => OpenStorageDirectory();
        _scan.Click += async (_, _) => await ScanDeviceAsync();
        _refresh.Click += async (_, _) => await LoadDataAsync();

        Load += async (_, _) =>
        {
            DeviceCodeService.EnsureStorageDirectory();
            ApplyPermissionState();
            await LoadDataAsync();
        };
        FormClosed += (_, _) =>
        {
            DisposePreviewImage(_qrPreview);
            DisposePreviewImage(_barcodePreview);
        };
    }

    private Control BuildToolbar()
    {
        var card = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(14)
        };

        var searchCaption = new Label
        {
            Text = "Tìm / quét mã:",
            AutoSize = true,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(14, 17)
        };
        _search.Location = new Point(115, 8);
        _search.Height = 36;

        card.Controls.Add(searchCaption);
        card.Controls.Add(_search);
        return card;
    }

    private Control BuildBody()
    {
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Background,
            Padding = new Padding(0, 8, 0, 8)
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var gridCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(10),
            Margin = new Padding(0, 0, 4, 0)
        };
        var gridTitle = new Label
        {
            Text = "Danh sách thiết bị",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI Semibold", 11F),
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        gridCard.Controls.Add(_grid);
        gridCard.Controls.Add(gridTitle);

        var previewCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = new Padding(10),
            Margin = new Padding(4, 0, 0, 0)
        };
        var previewTitle = new Label
        {
            Text = "Xem trước mã thiết bị",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI Semibold", 11F),
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var previews = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = AppTheme.Surface,
            Padding = new Padding(0, 4, 0, 0)
        };
        previews.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
        previews.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        previews.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
        previews.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        previews.Controls.Add(_qrPreview, 0, 0);
        previews.Controls.Add(_qrStatus, 0, 1);
        previews.Controls.Add(_barcodePreview, 0, 2);
        previews.Controls.Add(_barcodeStatus, 0, 3);

        previewCard.Controls.Add(previews);
        previewCard.Controls.Add(_deviceSummary);
        previewCard.Controls.Add(previewTitle);

        body.Controls.Add(gridCard, 0, 0);
        body.Controls.Add(previewCard, 1, 0);
        return body;
    }

    private Control BuildActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = AppTheme.Surface,
            Padding = new Padding(6, 7, 6, 5)
        };
        actions.Controls.AddRange([
            _generateQr,
            _generateBarcode,
            _generateBoth,
            _generateAll,
            _openFolder,
            _scan,
            _refresh
        ]);
        return actions;
    }

    private void ConfigureColumns()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Code",
            HeaderText = "Mã",
            DataPropertyName = nameof(DeviceRow.Code),
            Width = 82
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "Thiết bị",
            DataPropertyName = nameof(DeviceRow.Name),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 120F
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DeviceType",
            HeaderText = "Loại",
            DataPropertyName = nameof(DeviceRow.DeviceType),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 75F
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SerialNumber",
            HeaderText = "Serial",
            DataPropertyName = nameof(DeviceRow.SerialNumber),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 95F
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "StatusName",
            HeaderText = "Trạng thái",
            DataPropertyName = nameof(DeviceRow.StatusName),
            Width = 126
        });
    }

    private async Task LoadDataAsync()
    {
        if (_loading || IsDisposed) return;
        _loading = true;
        try
        {
            var keyword = _search.Text.Trim();
            await using var db = new AppDbContext();
            var query = db.Devices.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.Code.Contains(keyword) ||
                    x.Name.Contains(keyword) ||
                    (x.SerialNumber != null && x.SerialNumber.Contains(keyword)));
            }

            var raw = await query
                .OrderBy(x => x.Code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    DeviceType = x.DeviceType.Name,
                    x.SerialNumber,
                    x.Status
                })
                .ToListAsync();

            _rows = raw.Select(x => new DeviceRow(
                x.Id,
                x.Code,
                x.Name,
                x.DeviceType,
                x.SerialNumber,
                x.Status,
                x.Status.ToDisplayName())).ToList();
            _grid.DataSource = _rows;
            _status.Text = $"{_rows.Count:N0} thiết bị. Double-click một dòng để tạo lại QR + Barcode.";

            if (_rows.Count > 0 && _grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                _grid.CurrentCell = _grid.Rows[0].Cells[0];
            }
            else
            {
                ClearPreview();
            }
        }
        catch (Exception ex)
        {
            _status.Text = "Không thể tải danh sách thiết bị.";
            MessageBox.Show(this, ex.Message, "QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task ResolveScannerInputAsync()
    {
        if (!DeviceCodeService.TryExtractDeviceCode(_search.Text, out var code))
            return;

        if (!string.Equals(_search.Text.Trim(), code, StringComparison.OrdinalIgnoreCase))
        {
            _suppressSearchChanged = true;
            try
            {
                _search.Text = code;
                _search.SelectionStart = _search.TextLength;
            }
            finally
            {
                _suppressSearchChanged = false;
            }
            await LoadDataAsync();
        }

        var index = _rows.FindIndex(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
        if (index >= 0 && index < _grid.Rows.Count)
        {
            _grid.ClearSelection();
            _grid.Rows[index].Selected = true;
            _grid.CurrentCell = _grid.Rows[index].Cells[0];
            _grid.FirstDisplayedScrollingRowIndex = index;
            RefreshPreview();
        }
    }

    private async Task ScanDeviceAsync()
    {
        PermissionService.Demand(PermissionCodes.QrBarcodeView, "quét QR / Barcode thiết bị");
        using var form = new DeviceScanForm();
        if (form.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(form.SelectedDeviceCode))
            return;

        _suppressSearchChanged = true;
        try
        {
            _search.Text = form.SelectedDeviceCode;
            _search.SelectionStart = _search.TextLength;
        }
        finally
        {
            _suppressSearchChanged = false;
        }

        await LoadDataAsync();
        await ResolveScannerInputAsync();
    }

    private void ApplyPermissionState()
    {
        var canView = PermissionService.Has(PermissionCodes.QrBarcodeView);
        _scan.Enabled = canView;
        _scan.Visible = canView;

        var canGenerate = PermissionService.Has(PermissionCodes.QrBarcodeGenerate);
        foreach (var button in new[] { _generateQr, _generateBarcode, _generateBoth, _generateAll })
        {
            button.Enabled = canGenerate;
            button.Visible = canGenerate;
        }
    }

    private DeviceRow? SelectedRow() => _grid.CurrentRow?.DataBoundItem as DeviceRow;

    private async Task GenerateSelectedQrAsync()
    {
        var row = SelectedRow();
        if (row is null) return;
        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "tạo mã QR thiết bị");
        await RunGenerationAsync(row, DeviceCodeService.GenerateQr, "QR");
    }

    private async Task GenerateSelectedBarcodeAsync()
    {
        var row = SelectedRow();
        if (row is null) return;
        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "tạo Barcode thiết bị");
        await RunGenerationAsync(row, DeviceCodeService.GenerateBarcode, "Barcode");
    }

    private async Task GenerateSelectedBothAsync()
    {
        var row = SelectedRow();
        if (row is null) return;
        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "tạo mã QR / Barcode thiết bị");

        await RunBusyAsync(async () =>
        {
            var descriptor = row.ToDescriptor();
            var files = await Task.Run(() => DeviceCodeService.GenerateBoth(descriptor));
            await AuditService.TryWriteAsync(
                "Tạo QR / Barcode",
                "Thiết bị",
                $"Tạo lại QR và Barcode cho thiết bị {row.Code}.",
                row.Code,
                newValuesJson: $"{{\"qr\":\"{EscapeJson(files.QrPath)}\",\"barcode\":\"{EscapeJson(files.BarcodePath)}\"}}");
            _status.Text = $"Đã tạo QR + Barcode: {row.Code}.";
            RefreshPreview();
        });
    }

    private async Task RunGenerationAsync(DeviceRow row, Func<DeviceCodeDescriptor, string> generator, string kind)
    {
        await RunBusyAsync(async () =>
        {
            var path = await Task.Run(() => generator(row.ToDescriptor()));
            await AuditService.TryWriteAsync(
                $"Tạo {kind}",
                "Thiết bị",
                $"Tạo {kind} cho thiết bị {row.Code}.",
                row.Code,
                newValuesJson: $"{{\"file\":\"{EscapeJson(path)}\"}}");
            _status.Text = $"Đã tạo {kind}: {Path.GetFileName(path)}";
            RefreshPreview();
        });
    }

    private async Task GenerateVisibleAsync()
    {
        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "tạo hàng loạt mã QR / Barcode");
        if (_rows.Count == 0) return;

        var confirm = MessageBox.Show(
            this,
            $"Tạo lại QR và Barcode cho {_rows.Count:N0} thiết bị đang hiển thị?\n\nFile cũ cùng mã thiết bị sẽ được thay thế.",
            "Tạo hàng loạt QR / Barcode",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes) return;

        await RunBusyAsync(async () =>
        {
            var descriptors = _rows.Select(x => x.ToDescriptor()).ToArray();
            await Task.Run(() =>
            {
                foreach (var descriptor in descriptors)
                    DeviceCodeService.GenerateBoth(descriptor);
            });
            await AuditService.TryWriteAsync(
                "Tạo hàng loạt QR / Barcode",
                "Thiết bị",
                $"Tạo QR và Barcode cho {_rows.Count:N0} thiết bị đang hiển thị.",
                "Danh sách thiết bị");
            _status.Text = $"Đã tạo QR + Barcode cho {_rows.Count:N0} thiết bị.";
            RefreshPreview();
        });
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        var buttons = new[] { _generateQr, _generateBarcode, _generateBoth, _generateAll, _openFolder, _scan, _refresh };
        foreach (var button in buttons) button.Enabled = false;
        UseWaitCursor = true;
        try
        {
            await action();
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(this, ex.Message, "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể tạo QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _status.Text = "Tạo mã thất bại.";
        }
        finally
        {
            UseWaitCursor = false;
            foreach (var button in buttons) button.Enabled = true;
            ApplyPermissionState();
        }
    }

    private void RefreshPreview()
    {
        var row = SelectedRow();
        if (row is null)
        {
            ClearPreview();
            return;
        }

        var descriptor = row.ToDescriptor();
        _deviceSummary.Text = $"{row.Code} - {row.Name}\r\n{row.DeviceType} | {row.StatusName}";

        var qrPath = DeviceCodeService.GetQrPath(descriptor);
        var barcodePath = DeviceCodeService.GetBarcodePath(descriptor);
        SetPreviewImage(_qrPreview, qrPath);
        SetPreviewImage(_barcodePreview, barcodePath);
        _qrStatus.Text = File.Exists(qrPath) ? Path.GetFileName(qrPath) : "Chưa có file QR - bấm Tạo QR.";
        _barcodeStatus.Text = File.Exists(barcodePath) ? Path.GetFileName(barcodePath) : "Chưa có file Barcode - bấm Tạo Barcode.";
    }

    private void ClearPreview()
    {
        _deviceSummary.Text = "Chọn một thiết bị để xem QR / Barcode.";
        DisposePreviewImage(_qrPreview);
        DisposePreviewImage(_barcodePreview);
        _qrStatus.Text = "Chưa có file QR.";
        _barcodeStatus.Text = "Chưa có file Barcode.";
    }

    private static void SetPreviewImage(PictureBox box, string path)
    {
        DisposePreviewImage(box);
        if (!File.Exists(path)) return;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var source = Image.FromStream(stream);
        box.Image = new Bitmap(source);
    }

    private static void DisposePreviewImage(PictureBox box)
    {
        var old = box.Image;
        box.Image = null;
        old?.Dispose();
    }

    private void OpenStorageDirectory()
    {
        try
        {
            var directory = DeviceCodeService.EnsureStorageDirectory();
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể mở thư mục QR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static PictureBox NewPreviewBox() => new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.White,
        SizeMode = PictureBoxSizeMode.Zoom,
        Margin = new Padding(8)
    };

    private static Label NewPreviewStatus(string text) => new()
    {
        Dock = DockStyle.Fill,
        Text = text,
        ForeColor = AppTheme.TextSecondary,
        TextAlign = ContentAlignment.MiddleCenter,
        AutoEllipsis = true
    };

    private static string EscapeJson(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private sealed record DeviceRow(
        int Id,
        string Code,
        string Name,
        string DeviceType,
        string? SerialNumber,
        DeviceStatus Status,
        string StatusName)
    {
        public DeviceCodeDescriptor ToDescriptor()
            => new(Id, Code, Name, DeviceType, SerialNumber, Status);
    }
}
