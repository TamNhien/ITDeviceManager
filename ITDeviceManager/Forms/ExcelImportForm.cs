using ITDeviceManager.Common;
using ITDeviceManager.Services;

namespace ITDeviceManager.Forms;

public class ExcelImportForm : AppForm
{
    private readonly DataGridView _grid = new();
    private readonly Label _fileLabel = new()
    {
        Text = "Chưa chọn file Excel.",
        AutoEllipsis = true,
        ForeColor = AppTheme.TextSecondary,
        Height = 28,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly Label _totalLabel = SummaryLabel("Tổng: 0");
    private readonly Label _validLabel = SummaryLabel("Hợp lệ: 0");
    private readonly Label _invalidLabel = SummaryLabel("Lỗi: 0");
    private readonly Label _kindLabel = SummaryLabel("Thiết bị: 0 • Nhân viên: 0");
    private readonly Button _importButton;
    private ExcelImportPreviewResult? _preview;

    public bool ImportCompleted { get; private set; }

    public ExcelImportForm()
    {
        Text = "Nhập Excel";
        Font = new Font("Segoe UI", 10F);
        MinimumSize = new Size(900, 620);

        Ui.ConfigureGrid(_grid);
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.CellFormatting += GridCellFormatting;

        var createTemplate = Ui.Button("Tạo file mẫu", 125);
        var chooseFile = Ui.Button("Chọn Excel", 110);
        _importButton = Ui.Button("Nhập dữ liệu hợp lệ", 165);
        var clear = Ui.Button("Làm mới", 100);
        AppTheme.SetButtonRole(createTemplate, ButtonRole.Secondary);
        AppTheme.SetButtonRole(chooseFile, ButtonRole.Secondary);
        AppTheme.SetButtonRole(_importButton, ButtonRole.Primary);
        _importButton.Enabled = false;

        createTemplate.Click += (_, _) => CreateTemplate();
        chooseFile.Click += async (_, _) => await ChooseAndPreviewAsync();
        _importButton.Click += async (_, _) => await ImportAsync();
        clear.Click += (_, _) => ClearPreview();

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 112,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 6, 0, 4)
        };
        top.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        top.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        actions.Controls.AddRange([createTemplate, chooseFile, _importButton, clear]);

        var fileRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fileRow.Controls.Add(Ui.Label("File đang chọn:"), 0, 0);
        fileRow.Controls.Add(_fileLabel, 1, 0);

        top.Controls.Add(actions, 0, 0);
        top.Controls.Add(fileRow, 0, 1);

        var summary = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4, 8, 4, 4),
            WrapContents = false
        };
        summary.Controls.AddRange([_totalLabel, _validLabel, _invalidLabel, _kindLabel]);

        var note = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            ForeColor = AppTheme.TextSecondary,
            Text = "Chỉ các dòng Hợp lệ mới được ghi. Mã/serial trùng database hoặc dữ liệu trong Thùng rác sẽ bị chặn. Loại thiết bị và phòng ban phải tồn tại trước khi import.",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 6, 0)
        };

        Controls.Add(_grid);
        Controls.Add(note);
        Controls.Add(summary);
        Controls.Add(top);
    }

    private static Label SummaryLabel(string text)
        => new()
        {
            Text = text,
            AutoSize = false,
            Width = 170,
            Height = 32,
            Margin = new Padding(4, 0, 8, 0),
            ForeColor = AppTheme.TextPrimary,
            BackColor = AppTheme.SurfaceAlt,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 9.5F)
        };

    private void CreateTemplate()
    {
        try
        {
            PermissionService.Demand(PermissionCodes.ExcelImport, "tạo file mẫu nhập Excel");
            using var dialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "ITDeviceManager_Import_Template.xlsx",
                AddExtension = true,
                DefaultExt = "xlsx",
                OverwritePrompt = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            ExcelImportService.CreateTemplate(dialog.FileName);
            MessageBox.Show(
                "Đã tạo file mẫu.\n\nHãy giữ nguyên tên sheet/cột, điền dữ liệu rồi quay lại chọn file để xem trước.",
                "Tạo file mẫu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể tạo file mẫu.\n\n" + ex.Message, "Nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ChooseAndPreviewAsync()
    {
        try
        {
            PermissionService.Demand(PermissionCodes.ExcelImport, "xem trước dữ liệu Excel");
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            UseWaitCursor = true;
            _fileLabel.Text = dialog.FileName;
            _preview = await ExcelImportService.PreviewAsync(dialog.FileName);
            BindPreview();
        }
        catch (Exception ex)
        {
            _preview = null;
            BindPreview();
            MessageBox.Show("Không thể đọc file Excel.\n\n" + ex.Message, "Nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task ImportAsync()
    {
        if (_preview is null || _preview.ValidCount == 0)
            return;

        if (MessageBox.Show(
                $"Sẽ nhập {_preview.ValidCount:N0} dòng hợp lệ.\n\n{_preview.InvalidCount:N0} dòng lỗi sẽ được bỏ qua. Tiếp tục?",
                "Xác nhận nhập Excel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            UseWaitCursor = true;
            _importButton.Enabled = false;
            var result = await ExcelImportService.ImportAsync(_preview.Rows);
            ImportCompleted |= result.ImportedTotal > 0;

            MessageBox.Show(
                $"Nhập Excel hoàn tất.\n\n" +
                $"Thiết bị: {result.ImportedDevices:N0}\n" +
                $"Nhân viên: {result.ImportedEmployees:N0}\n" +
                $"Bỏ qua do dữ liệu thay đổi sau bước xem trước: {result.SkippedRows:N0}",
                "Nhập Excel",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ClearPreview();
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Không có quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể nhập dữ liệu. Database đã được rollback nếu giao dịch chưa hoàn tất.\n\n" + ex.Message, "Nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _importButton.Enabled = _preview?.ValidCount > 0;
        }
    }

    private void BindPreview()
    {
        if (_preview is null)
        {
            _grid.DataSource = null;
            _totalLabel.Text = "Tổng: 0";
            _validLabel.Text = "Hợp lệ: 0";
            _invalidLabel.Text = "Lỗi: 0";
            _kindLabel.Text = "Thiết bị: 0 • Nhân viên: 0";
            _importButton.Enabled = false;
            return;
        }

        _grid.DataSource = _preview.Rows.Select(x => new
        {
            Dòng = x.RowNumber,
            Sheet = x.SheetName,
            Loại_dữ_liệu = x.EntityType,
            Mã = x.Code,
            Tên = x.Name,
            Tham_chiếu = x.Reference,
            Kết_quả = x.IsValid ? "Hợp lệ" : "Lỗi",
            Chi_tiết_lỗi = x.ErrorText
        }).ToList();

        SetColumn("Dòng", 62, DataGridViewContentAlignment.MiddleCenter);
        SetColumn("Sheet", 90, DataGridViewContentAlignment.MiddleCenter);
        SetColumn("Loại_dữ_liệu", 100, DataGridViewContentAlignment.MiddleCenter);
        SetColumn("Mã", 92, DataGridViewContentAlignment.MiddleCenter);
        SetColumn("Tên", 190, DataGridViewContentAlignment.MiddleLeft);
        SetColumn("Tham_chiếu", 230, DataGridViewContentAlignment.MiddleLeft);
        SetColumn("Kết_quả", 82, DataGridViewContentAlignment.MiddleCenter);
        if (_grid.Columns["Chi_tiết_lỗi"] is { } errorColumn)
        {
            errorColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            errorColumn.MinimumWidth = 260;
        }

        _totalLabel.Text = $"Tổng: {_preview.TotalCount:N0}";
        _validLabel.Text = $"Hợp lệ: {_preview.ValidCount:N0}";
        _invalidLabel.Text = $"Lỗi: {_preview.InvalidCount:N0}";
        _kindLabel.Text = $"Thiết bị: {_preview.DeviceCount:N0} • Nhân viên: {_preview.EmployeeCount:N0}";
        _importButton.Enabled = _preview.ValidCount > 0;
    }

    private void SetColumn(string name, int width, DataGridViewContentAlignment alignment)
    {
        if (_grid.Columns[name] is not { } column)
            return;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        column.Width = width;
        column.DefaultCellStyle.Alignment = alignment;
    }

    private static void GridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Kết_quả")
            return;
        var text = Convert.ToString(e.Value);
        e.CellStyle.ForeColor = string.Equals(text, "Hợp lệ", StringComparison.OrdinalIgnoreCase)
            ? AppTheme.Success
            : AppTheme.Danger;
        e.CellStyle.Font = new Font("Segoe UI Semibold", 9.5F);
    }

    private void ClearPreview()
    {
        _preview = null;
        _fileLabel.Text = "Chưa chọn file Excel.";
        BindPreview();
    }
}
