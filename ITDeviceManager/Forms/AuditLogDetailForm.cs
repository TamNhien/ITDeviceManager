using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public class AuditLogDetailForm : AppForm
{
    private readonly long _id;
    private readonly Label _time = ValueLabel();
    private readonly Label _user = ValueLabel();
    private readonly Label _action = ValueLabel();
    private readonly Label _entity = ValueLabel();
    private readonly Label _key = ValueLabel();
    private readonly Label _computer = ValueLabel();
    private readonly Label _version = ValueLabel();
    private readonly Label _logId = ValueLabel();
    private readonly TextBox _description = ReadOnlyBox(false);
    private readonly TextBox _oldValues = ReadOnlyBox(true);
    private readonly TextBox _newValues = ReadOnlyBox(true);

    public AuditLogDetailForm(long id)
    {
        _id = id;
        Text = "Chi tiết nhật ký hoạt động";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 700);
        MinimumSize = new Size(760, 620);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 8,
            BackColor = AppTheme.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        AddPair(root, 0, "Thời gian", _time, "Người thực hiện", _user);
        AddPair(root, 1, "Hành động", _action, "Đối tượng", _entity);
        AddPair(root, 2, "Mã đối tượng", _key, "Máy tính", _computer);
        AddPair(root, 3, "Phiên bản", _version, "ID nhật ký", _logId);

        var descriptionHost = FieldPanel("Nội dung", _description);
        root.Controls.Add(descriptionHost, 0, 4);
        root.SetColumnSpan(descriptionHost, 2);

        var oldHost = FieldPanel("Dữ liệu trước thay đổi", _oldValues);
        var newHost = FieldPanel("Dữ liệu sau thay đổi", _newValues);
        root.Controls.Add(oldHost, 0, 5);
        root.Controls.Add(newHost, 1, 5);
        root.SetRowSpan(oldHost, 2);
        root.SetRowSpan(newHost, 2);

        var close = Ui.Button("Đóng", 110);
        close.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        close.Click += (_, _) => Close();
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        buttons.Controls.Add(close);
        root.Controls.Add(buttons, 0, 7);
        root.SetColumnSpan(buttons, 2);

        Controls.Add(root);
        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await using var db = new AppDbContext();
        var log = await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == _id);
        if (log is null)
        {
            MessageBox.Show("Không tìm thấy nhật ký hoạt động.", "Thông báo");
            Close();
            return;
        }

        _time.Text = log.OccurredAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        _user.Text = log.Username;
        _action.Text = log.Action;
        _entity.Text = log.EntityName;
        _key.Text = log.EntityKey ?? "-";
        _computer.Text = log.ComputerName ?? "-";
        _version.Text = log.AppVersion ?? "-";
        _logId.Text = log.Id.ToString();
        _description.Text = log.Description;
        _oldValues.Text = EmptyAsDash(log.OldValuesJson);
        _newValues.Text = EmptyAsDash(log.NewValuesJson);
    }

    private static void AddPair(
        TableLayoutPanel root,
        int row,
        string leftTitle,
        Control leftValue,
        string rightTitle,
        Control rightValue)
    {
        var left = InlineField(leftTitle, leftValue);
        root.Controls.Add(left, 0, row);

        if (!string.IsNullOrWhiteSpace(rightTitle))
        {
            var right = InlineField(rightTitle, rightValue);
            root.Controls.Add(right, 1, row);
        }
    }

    private static Control InlineField(string title, Control value)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0, 0, 10, 2)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.Controls.Add(new Label
        {
            Text = title + ":",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = AppTheme.TextSecondary
        }, 0, 0);
        value.Dock = DockStyle.Fill;
        panel.Controls.Add(value, 1, 0);
        return panel;
    }

    private static Control FieldPanel(string title, Control content)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 24, 10, 4) };
        panel.Controls.Add(content);
        panel.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = AppTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        });
        return panel;
    }

    private static Label ValueLabel() => new()
    {
        AutoEllipsis = true,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = AppTheme.TextPrimary
    };

    private static TextBox ReadOnlyBox(bool monospace) => new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = AppTheme.Surface,
        ForeColor = AppTheme.TextPrimary,
        Font = monospace ? new Font("Consolas", 9.5F) : new Font("Segoe UI", 9.5F),
        Dock = DockStyle.Fill
    };

    private static string EmptyAsDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "Không có dữ liệu thay đổi." : value;
}
