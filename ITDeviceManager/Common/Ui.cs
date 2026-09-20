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
