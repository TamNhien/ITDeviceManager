namespace ITDeviceManager.Forms;

public class LookupEditForm : AppForm
{
    private readonly TextBox _code = new() { Width = 260 };
    private readonly TextBox _name = new() { Width = 260 };
    public string ItemCode => _code.Text.Trim();
    public string ItemName => _name.Text.Trim();

    public LookupEditForm(string title, bool showCode, string? code = null, string? name = null)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, showCode ? 210 : 160);
        Font = new Font("Segoe UI", 10);
        _code.Text = code ?? string.Empty;
        _name.Text = name ?? string.Empty;

        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(30, 20, 20, 20) };
        if (showCode)
        {
            panel.Controls.Add(new Label { Text = "Mã *", AutoSize = true });
            panel.Controls.Add(_code);
        }
        panel.Controls.Add(new Label { Text = "Tên *", AutoSize = true, Margin = new Padding(3, 10, 3, 0) });
        panel.Controls.Add(_name);
        var buttons = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 15, 0, 0) };
        var save = new Button { Text = "Lưu", Width = 100, DialogResult = DialogResult.None };
        var cancel = new Button { Text = "Hủy", Width = 100, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange([save, cancel]);
        panel.Controls.Add(buttons);
        Controls.Add(panel);
        CancelButton = cancel;
        save.Click += (_, _) =>
        {
            if (showCode && string.IsNullOrWhiteSpace(_code.Text)) { MessageBox.Show("Vui lòng nhập mã."); return; }
            if (string.IsNullOrWhiteSpace(_name.Text)) { MessageBox.Show("Vui lòng nhập tên."); return; }
            DialogResult = DialogResult.OK;
            Close();
        };
    }
}
