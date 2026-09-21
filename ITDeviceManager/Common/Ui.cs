using ITDeviceManager.Services;

namespace ITDeviceManager.Common;

public static class Ui
{
    public static Button Button(string text, int width = 110)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 38,
            Margin = new Padding(6)
        };
        AppTheme.SetButtonRole(button, AppTheme.InferButtonRole(text));
        return button;
    }

    public static Button ExportButton(DataGridView grid, string reportTitle, int width = 108)
    {
        var button = Button("Xuất file", width);
        AppTheme.SetButtonRole(button, ButtonRole.Secondary);

        var menu = new ContextMenuStrip
        {
            Font = new Font("Segoe UI", 10F),
            ShowImageMargin = false
        };
        var excel = menu.Items.Add("Xuất Excel (.xlsx)");
        var pdf = menu.Items.Add("Xuất PDF (.pdf)");

        excel.Click += async (_, _) => await ExportService.ExportExcelAsync(grid, reportTitle, grid.FindForm() as IWin32Window ?? grid);
        pdf.Click += async (_, _) => await ExportService.ExportPdfAsync(grid, reportTitle, grid.FindForm() as IWin32Window ?? grid);
        button.Click += (_, _) => menu.Show(button, new Point(0, button.Height));

        return button;
    }

    public static Label Label(string text, bool bold = false)
        => new()
        {
            Text = text,
            AutoSize = true,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 10, bold ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(3, 9, 3, 3)
        };

    public static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        AppTheme.StyleGrid(grid);
    }

    public static bool ConfirmDelete(string itemName)
        => MessageBox.Show(
            $"Bạn có chắc muốn xóa {itemName}?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes;
}
