using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public sealed class DeviceScanForm : AppForm
{
    private readonly TextBox _scanInput = new()
    {
        Dock = DockStyle.Top,
        Height = 34,
        PlaceholderText = "Quét QR / Barcode hoặc nhập mã thiết bị / serial...",
        TextAlign = HorizontalAlignment.Center
    };
    private readonly Label _status = new()
    {
        Dock = DockStyle.Top,
        Height = 44,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = AppTheme.TextSecondary
    };
    private readonly TableLayoutPanel _details = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 6,
        Padding = new Padding(16),
        BackColor = AppTheme.Surface
    };
    private readonly Button _useButton = Ui.Button("Chọn thiết bị", 125);

    private DeviceLabelSnapshot? _found;

    public string? SelectedDeviceCode => _found?.Code;

    public DeviceScanForm()
    {
        Text = "Quét QR / Barcode";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(660, 500);
        Size = new Size(760, 560);
        BackColor = AppTheme.Background;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 132,
            Padding = new Padding(20, 16, 20, 10),
            BackColor = AppTheme.Surface
        };
        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Quét / tìm thiết bị",
            Font = new Font("Segoe UI Semibold", 17F),
            ForeColor = AppTheme.TextPrimary
        };
        var hint = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Hỗ trợ máy quét USB kiểu bàn phím. Nhấn Enter sau khi quét hoặc nhập thủ công.",
            ForeColor = AppTheme.TextSecondary,
            Font = new Font("Segoe UI", 9.5F)
        };
        header.Controls.Add(_scanInput);
        header.Controls.Add(hint);
        header.Controls.Add(title);

        _details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        _details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (var i = 0; i < _details.RowCount; i++)
            _details.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / _details.RowCount));
        SetDetails(null);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 8, 18, 8),
            BackColor = AppTheme.Surface
        };
        var close = Ui.Button("Đóng", 100);
        var find = Ui.Button("Tìm", 100);
        AppTheme.SetButtonRole(close, ButtonRole.Secondary);
        AppTheme.SetButtonRole(find, ButtonRole.Secondary);
        _useButton.Enabled = false;
        buttons.Controls.AddRange([close, _useButton, find]);

        close.Click += (_, _) => Close();
        find.Click += async (_, _) => await LookupAsync();
        _useButton.Click += (_, _) =>
        {
            if (_found is null) return;
            DialogResult = DialogResult.OK;
            Close();
        };
        _scanInput.KeyDown += async (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await LookupAsync();
        };

        Controls.Add(_details);
        Controls.Add(_status);
        Controls.Add(buttons);
        Controls.Add(header);

        Shown += (_, _) =>
        {
            PermissionService.Demand(PermissionCodes.QrBarcodeView, "quét QR / Barcode thiết bị");
            _scanInput.Focus();
        };
    }

    private async Task LookupAsync()
    {
        PermissionService.Demand(PermissionCodes.QrBarcodeView, "quét QR / Barcode thiết bị");
        var parsed = DeviceCodeService.ParseScanText(_scanInput.Text);
        if (string.IsNullOrWhiteSpace(parsed.RawText))
        {
            _status.Text = "Hãy quét mã hoặc nhập mã thiết bị / serial.";
            _status.ForeColor = AppTheme.Warning;
            return;
        }

        await using var db = new AppDbContext();
        var query = db.Devices
            .AsNoTracking()
            .Include(x => x.DeviceType)
            .Include(x => x.Department)
            .AsQueryable();

        Device? device = null;
        if (parsed.Id is int id)
        {
            var expectedCode = parsed.Code;
            device = await query.SingleOrDefaultAsync(x =>
                x.Id == id && (expectedCode == null || x.Code == expectedCode));
        }

        if (device is null && !string.IsNullOrWhiteSpace(parsed.Code))
        {
            var code = parsed.Code.Trim();
            device = await query.FirstOrDefaultAsync(x => x.Code == code || x.SerialNumber == code);
        }

        if (device is null && !string.IsNullOrWhiteSpace(parsed.SerialNumber))
        {
            var serial = parsed.SerialNumber.Trim();
            device = await query.FirstOrDefaultAsync(x => x.SerialNumber == serial || x.Code == serial);
        }

        if (device is null)
        {
            _found = null;
            _useButton.Enabled = false;
            _status.Text = "Không tìm thấy thiết bị khớp với mã vừa quét.";
            _status.ForeColor = AppTheme.Danger;
            SetDetails(null);
            _scanInput.SelectAll();
            _scanInput.Focus();
            return;
        }

        _found = new DeviceLabelSnapshot(
            device.Id,
            device.Code,
            device.Name,
            device.DeviceType.Name,
            device.SerialNumber,
            device.Status.ToDisplayName(),
            device.Department?.Name);
        _useButton.Enabled = true;
        _status.Text = $"Đã nhận diện: {_found.Code} · {_found.Name}";
        _status.ForeColor = AppTheme.Success;
        SetDetails(_found);
        _scanInput.SelectAll();
        _scanInput.Focus();

        await AuditService.TryWriteAsync(
            "Quét QR/Barcode",
            "Device",
            $"Nhận diện thiết bị {_found.Code} từ QR/Barcode.",
            _found.Code);
    }

    private void SetDetails(DeviceLabelSnapshot? device)
    {
        _details.SuspendLayout();
        _details.Controls.Clear();

        var rows = device is null
            ? new (string Label, string Value)[]
            {
                ("Mã", "—"), ("Tên thiết bị", "—"), ("Loại", "—"),
                ("Serial", "—"), ("Trạng thái", "—"), ("Phòng ban", "—")
            }
            : new (string Label, string Value)[]
            {
                ("Mã", device.Code), ("Tên thiết bị", device.Name), ("Loại", device.TypeName),
                ("Serial", string.IsNullOrWhiteSpace(device.SerialNumber) ? "—" : device.SerialNumber!),
                ("Trạng thái", device.Status),
                ("Phòng ban", string.IsNullOrWhiteSpace(device.DepartmentName) ? "—" : device.DepartmentName!)
            };

        for (var row = 0; row < rows.Length; row++)
        {
            var key = new Label
            {
                Dock = DockStyle.Fill,
                Text = rows[row].Label,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppTheme.TextSecondary,
                Font = new Font("Segoe UI Semibold", 10F)
            };
            var value = new Label
            {
                Dock = DockStyle.Fill,
                Text = rows[row].Value,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 10.5F),
                AutoEllipsis = true
            };
            _details.Controls.Add(key, 0, row);
            _details.Controls.Add(value, 1, row);
        }

        _details.ResumeLayout();
    }
}
