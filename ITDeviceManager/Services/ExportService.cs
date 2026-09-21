using System.Diagnostics;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ITDeviceManager.Services;

public static class ExportService
{
    private sealed record ExportColumn(string Header, float Weight);
    private sealed record ExportSnapshot(IReadOnlyList<ExportColumn> Columns, IReadOnlyList<IReadOnlyList<string>> Rows);

    static ExportService()
    {
        // V1.6.0 is an academic/learning project. Review QuestPDF licensing before using this code
        // in a production organisation that is not eligible for the Community license.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static async Task ExportExcelAsync(DataGridView grid, string reportTitle, IWin32Window owner)
    {
        var snapshot = CaptureGrid(grid);
        if (!ValidateSnapshot(snapshot, owner)) return;

        using var dialog = new SaveFileDialog
        {
            Title = "Xuất báo cáo Excel",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            DefaultExt = "xlsx",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = BuildDefaultFileName(reportTitle, "xlsx")
        };

        if (dialog.ShowDialog(owner) != DialogResult.OK)
            return;

        try
        {
            UseWaitCursor(owner, true);
            await Task.Run(() => WriteExcel(dialog.FileName, reportTitle, snapshot));
            await WriteAuditAsync("Xuất Excel", reportTitle, snapshot.Rows.Count, Path.GetFileName(dialog.FileName));
            ShowSuccess(owner, "Excel", dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner,
                $"Không thể xuất Excel.\n\n{ex.Message}",
                "Xuất Excel thất bại",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor(owner, false);
        }
    }

    public static async Task ExportPdfAsync(DataGridView grid, string reportTitle, IWin32Window owner)
    {
        var snapshot = CaptureGrid(grid);
        if (!ValidateSnapshot(snapshot, owner)) return;

        using var dialog = new SaveFileDialog
        {
            Title = "Xuất báo cáo PDF",
            Filter = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = "pdf",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = BuildDefaultFileName(reportTitle, "pdf")
        };

        if (dialog.ShowDialog(owner) != DialogResult.OK)
            return;

        try
        {
            UseWaitCursor(owner, true);
            await Task.Run(() => WritePdf(dialog.FileName, reportTitle, snapshot));
            await WriteAuditAsync("Xuất PDF", reportTitle, snapshot.Rows.Count, Path.GetFileName(dialog.FileName));
            ShowSuccess(owner, "PDF", dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner,
                $"Không thể xuất PDF.\n\n{ex.Message}",
                "Xuất PDF thất bại",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor(owner, false);
        }
    }

    private static ExportSnapshot CaptureGrid(DataGridView grid)
    {
        var columns = grid.Columns
            .Cast<DataGridViewColumn>()
            .Where(x => x.Visible)
            .OrderBy(x => x.DisplayIndex)
            .Select(x => new ExportColumn(NormalizeHeader(x.HeaderText), GetColumnWeight(x)))
            .ToList();

        var visibleColumns = grid.Columns
            .Cast<DataGridViewColumn>()
            .Where(x => x.Visible)
            .OrderBy(x => x.DisplayIndex)
            .ToList();

        var rows = new List<IReadOnlyList<string>>();
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.IsNewRow || !row.Visible) continue;

            var values = new List<string>(visibleColumns.Count);
            foreach (var column in visibleColumns)
            {
                var cell = row.Cells[column.Index];
                var value = cell.FormattedValue?.ToString() ?? cell.Value?.ToString() ?? string.Empty;
                values.Add(value.Trim());
            }
            rows.Add(values);
        }

        return new ExportSnapshot(columns, rows);
    }

    private static bool ValidateSnapshot(ExportSnapshot snapshot, IWin32Window owner)
    {
        if (snapshot.Columns.Count == 0 || snapshot.Rows.Count == 0)
        {
            MessageBox.Show(owner,
                "Không có dữ liệu đang hiển thị để xuất.",
                "Xuất báo cáo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }
        return true;
    }

    private static void WriteExcel(string path, string reportTitle, ExportSnapshot snapshot)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("BaoCao");
        var columnCount = snapshot.Columns.Count;

        worksheet.Cell(1, 1).Value = reportTitle;
        worksheet.Range(1, 1, 1, columnCount).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#0F172A");
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Cell(2, 1).Value = $"Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}  |  Số dòng: {snapshot.Rows.Count:N0}";
        worksheet.Range(2, 1, 2, columnCount).Merge();
        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#64748B");
        worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        const int headerRow = 4;
        for (var c = 0; c < columnCount; c++)
            worksheet.Cell(headerRow, c + 1).Value = snapshot.Columns[c].Header;

        var header = worksheet.Range(headerRow, 1, headerRow, columnCount);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Alignment.WrapText = true;

        for (var r = 0; r < snapshot.Rows.Count; r++)
        {
            for (var c = 0; c < columnCount; c++)
                worksheet.Cell(headerRow + 1 + r, c + 1).Value = snapshot.Rows[r][c];
        }

        var dataRange = worksheet.Range(headerRow + 1, 1, headerRow + snapshot.Rows.Count, columnCount);
        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        dataRange.Style.Alignment.WrapText = true;
        dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        dataRange.Style.Border.BottomBorderColor = XLColor.FromHtml("#E2E8F0");

        for (var r = 0; r < snapshot.Rows.Count; r++)
        {
            if (r % 2 == 1)
                worksheet.Range(headerRow + 1 + r, 1, headerRow + 1 + r, columnCount)
                    .Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
        }

        worksheet.Range(headerRow, 1, headerRow + snapshot.Rows.Count, columnCount).SetAutoFilter();
        worksheet.SheetView.FreezeRows(headerRow);
        worksheet.Columns(1, columnCount).AdjustToContents(1, headerRow + snapshot.Rows.Count);

        for (var c = 1; c <= columnCount; c++)
        {
            var column = worksheet.Column(c);
            if (column.Width < 10) column.Width = 10;
            if (column.Width > 45) column.Width = 45;
        }

        worksheet.Row(1).Height = 26;
        worksheet.Row(headerRow).Height = 28;
        worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        worksheet.PageSetup.FitToPages(1, 0);
        worksheet.PageSetup.Margins.Top = 0.4;
        worksheet.PageSetup.Margins.Bottom = 0.4;

        workbook.SaveAs(path);
    }

    private static void WritePdf(string path, string reportTitle, ExportSnapshot snapshot)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(snapshot.Columns.Count >= 8 ? PageSizes.A3.Landscape() : PageSizes.A4.Landscape());
                page.Margin(24);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Lato").FontSize(snapshot.Columns.Count >= 8 ? 7.2F : 8.2F));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text(reportTitle)
                        .FontSize(16)
                        .SemiBold()
                        .FontColor(Colors.Grey.Darken4);
                    column.Item().PaddingTop(3).AlignCenter().Text(
                            $"Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}  |  Số dòng: {snapshot.Rows.Count:N0}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(8);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var column in snapshot.Columns)
                            columns.RelativeColumn(Math.Max(0.55F, column.Weight));
                    });

                    table.Header(header =>
                    {
                        foreach (var column in snapshot.Columns)
                        {
                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Border(0.5F)
                                .BorderColor(Colors.Blue.Darken3)
                                .PaddingVertical(6)
                                .PaddingHorizontal(4)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(column.Header)
                                .SemiBold()
                                .FontColor(Colors.White);
                        }
                    });

                    for (var rowIndex = 0; rowIndex < snapshot.Rows.Count; rowIndex++)
                    {
                        var row = snapshot.Rows[rowIndex];
                        for (var columnIndex = 0; columnIndex < snapshot.Columns.Count; columnIndex++)
                        {
                            var cell = table.Cell()
                                .Background(rowIndex % 2 == 1 ? Colors.Grey.Lighten4 : Colors.White)
                                .BorderBottom(0.5F)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(5)
                                .PaddingHorizontal(4)
                                .AlignMiddle();

                            if (IsCenterAligned(snapshot.Columns[columnIndex].Header))
                                cell = cell.AlignCenter();

                            cell.Text(row[columnIndex]);
                        }
                    }
                });

                page.Footer().PaddingTop(8).AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("Trang ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(path);
    }

    private static async Task WriteAuditAsync(string action, string reportTitle, int rowCount, string fileName)
    {
        await AuditService.TryWriteAsync(
            action,
            "Báo cáo",
            $"{action} '{reportTitle}' ({rowCount:N0} dòng) ra tệp {fileName}.",
            reportTitle);
    }

    private static void ShowSuccess(IWin32Window owner, string format, string path)
    {
        var result = MessageBox.Show(owner,
            $"Đã xuất {format} thành công.\n\n{path}\n\nBạn có muốn mở thư mục chứa tệp không?",
            $"Xuất {format} thành công",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);

        if (result != DialogResult.Yes) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            });
        }
        catch
        {
            // Export succeeded; failure to open Explorer is non-critical.
        }
    }

    private static void UseWaitCursor(IWin32Window owner, bool enabled)
    {
        if (owner is Control control)
            control.UseWaitCursor = enabled;
        Application.UseWaitCursor = enabled;
        Application.DoEvents();
    }

    private static string NormalizeHeader(string value)
        => string.IsNullOrWhiteSpace(value) ? "Cột" : value.Replace('_', ' ').Trim();

    private static float GetColumnWeight(DataGridViewColumn column)
    {
        var header = NormalizeHeader(column.HeaderText).ToLowerInvariant();
        if (header.Contains("ghi chú") || header.Contains("nội dung") || header.Contains("mô tả") || header.Contains("kết quả")) return 2.3F;
        if (header.Contains("thiết bị") || header.Contains("nhân viên") || header.Contains("phòng ban") || header.Contains("đơn vị")) return 1.7F;
        if (header.Contains("email") || header.Contains("serial")) return 1.45F;
        if (header.Contains("ngày") || header.Contains("thời gian") || header.Contains("trạng thái") || header.Contains("tình trạng")) return 1.05F;
        if (header == "mã" || header.Contains("mã phiếu") || header.Contains("quyền") || header.Contains("chi phí") || header.Contains("giá mua")) return 0.9F;
        return 1.2F;
    }

    private static bool IsCenterAligned(string header)
    {
        var normalized = header.ToLowerInvariant();
        return normalized.Contains("ngày") ||
               normalized.Contains("thời gian") ||
               normalized.Contains("trạng thái") ||
               normalized.Contains("tình trạng") ||
               normalized.StartsWith("mã") ||
               normalized == "quyền" ||
               normalized.Contains("hoạt động");
    }

    private static string BuildDefaultFileName(string reportTitle, string extension)
    {
        var safe = RemoveDiacritics(reportTitle);
        foreach (var invalid in Path.GetInvalidFileNameChars())
            safe = safe.Replace(invalid, '-');
        safe = string.Join("_", safe.Split([' ', '/', '\\', ':'], StringSplitOptions.RemoveEmptyEntries));
        return $"{safe}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }
        return builder.ToString()
            .Normalize(System.Text.NormalizationForm.FormC)
            .Replace('Đ', 'D')
            .Replace('đ', 'd');
    }
}
