namespace ITDeviceManager.Common;

public static class Ui
{
    public static Button Button(string text, int width = 110)
        => new()
        {
            Text = text,
            Width = width,
            Height = 36,
            Margin = new Padding(6),
            FlatStyle = FlatStyle.System
        };

    public static Label Label(string text, bool bold = false)
        => new()
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, bold ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(3, 8, 3, 3)
        };

    public static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = SystemColors.Window;
    }

    public static bool ConfirmDelete(string itemName)
        => MessageBox.Show(
            $"Bạn có chắc muốn xóa {itemName}?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes;
}
