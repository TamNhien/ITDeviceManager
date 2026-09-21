using System.Drawing.Imaging;
using System.Drawing.Printing;
using ITDeviceManager.Common;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Forms;

public sealed class DeviceLabelForm : AppForm
{
    private readonly int _deviceId;
    private readonly PictureBox _preview = new()
    {
        Dock = DockStyle.Fill,
        BackColor = AppTheme.SurfaceAlt,
        SizeMode = PictureBoxSizeMode.Zoom,
        Padding = new Padding(16)
    };
    private readonly Label _summary = new()
    {
        Dock = DockStyle.Top,
        Height = 56,
        TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = AppTheme.TextSecondary,
        Font = new Font("Segoe UI", 9.5F)
    };

    private DeviceLabelSnapshot? _device;
    private Bitmap? _labelBitmap;

    public DeviceLabelForm(int deviceId)
    {
        _deviceId = deviceId;
        Text = "Nhãn QR / Barcode thiết bị";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 680);
        Size = new Size(1040, 760);
        BackColor = AppTheme.Background;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 88,
            Padding = new Padding(24, 16, 24, 8),
            BackColor = AppTheme.Surface
        };
        var title = new Label
        {
            AutoSize = true,
            Text = "Nhãn tài sản QR / Barcode",
            Font = new Font("Segoe UI Semibold", 17F),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(24, 14)
        };
        _summary.Location = new Point(26, 48);
        _summary.Width = 820;
        _summary.Dock = DockStyle.None;
        header.Controls.Add(title);
        header.Controls.Add(_summary);

        var previewCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(24),
            Padding = new Padding(16),
            BackColor = AppTheme.Surface
        };
        previewCard.Controls.Add(_preview);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 8, 18, 8),
            BackColor = AppTheme.Surface
        };
        var close = Ui.Button("Đóng", 100);
        var print = Ui.Button("In nhãn", 110);
        var save = Ui.Button("Lưu PNG", 110);
        var copy = Ui.Button("Sao chép mã QR", 145);
        AppTheme.SetButtonRole(close, ButtonRole.Secondary);
        AppTheme.SetButtonRole(copy, ButtonRole.Secondary);
        buttons.Controls.AddRange([close, print, save, copy]);

        close.Click += (_, _) => Close();
        save.Click += async (_, _) => await SavePngAsync();
        print.Click += async (_, _) => await PrintAsync();
        copy.Click += (_, _) => CopyQrPayload();

        Controls.Add(previewCard);
        Controls.Add(buttons);
        Controls.Add(header);

        Shown += async (_, _) => await LoadDeviceAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _preview.Image = null;
            _labelBitmap?.Dispose();
            _labelBitmap = null;
        }
        base.Dispose(disposing);
    }

    private async Task LoadDeviceAsync()
    {
        try
        {
            PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "sử dụng QR / Barcode thiết bị");

            await using var db = new AppDbContext();
            var device = await db.Devices
                .AsNoTracking()
                .Include(x => x.DeviceType)
                .Include(x => x.Department)
                .SingleOrDefaultAsync(x => x.Id == _deviceId);

            if (device is null)
            {
                MessageBox.Show("Không tìm thấy thiết bị.", "QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            _device = new DeviceLabelSnapshot(
                device.Id,
                device.Code,
                device.Name,
                device.DeviceType.Name,
                device.SerialNumber,
                device.Status.ToDisplayName(),
                device.Department?.Name);

            _summary.Text = $"{_device.Code} · {_device.Name} · {_device.Status}";
            ReplacePreview(DeviceCodeService.CreateDeviceLabel(_device));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể tạo nhãn thiết bị.\n\n" + ex.Message, "QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private async Task SavePngAsync()
    {
        if (_device is null || _labelBitmap is null)
            return;

        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "lưu nhãn QR / Barcode");
        var directory = DeviceCodeService.GetDefaultLabelDirectory();
        Directory.CreateDirectory(directory);

        using var dialog = new SaveFileDialog
        {
            Title = "Lưu nhãn QR / Barcode",
            Filter = "Ảnh PNG (*.png)|*.png",
            DefaultExt = "png",
            AddExtension = true,
            FileName = $"{SanitizeFileName(_device.Code)}_QR_Barcode.png",
            InitialDirectory = directory
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _labelBitmap.Save(dialog.FileName, ImageFormat.Png);
        await AuditService.TryWriteAsync(
            "Xuất nhãn QR/Barcode",
            "Device",
            $"Xuất nhãn PNG cho thiết bị {_device.Code}.",
            _device.Code,
            newValuesJson: System.Text.Json.JsonSerializer.Serialize(new { FileName = Path.GetFileName(dialog.FileName) }));

        MessageBox.Show("Đã lưu nhãn PNG thành công.", "QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task PrintAsync()
    {
        if (_device is null || _labelBitmap is null)
            return;

        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "in nhãn QR / Barcode");

        using var document = new PrintDocument
        {
            DocumentName = $"ITDeviceManager - {_device.Code}"
        };
        document.PrintPage += (_, e) => DrawPrintPage(e);

        using var dialog = new PrintDialog
        {
            Document = document,
            UseEXDialog = true,
            AllowSomePages = false,
            AllowSelection = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            document.Print();
            await AuditService.TryWriteAsync(
                "In nhãn QR/Barcode",
                "Device",
                $"In nhãn QR/Barcode cho thiết bị {_device.Code}.",
                _device.Code);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể in nhãn.\n\n" + ex.Message, "QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DrawPrintPage(PrintPageEventArgs e)
    {
        if (_labelBitmap is null)
            return;

        var graphics = e.Graphics;
        if (graphics is null)
            return;

        var bounds = e.MarginBounds;
        var scale = Math.Min(bounds.Width / (float)_labelBitmap.Width, bounds.Height / (float)_labelBitmap.Height);
        var width = (int)Math.Round(_labelBitmap.Width * scale);
        var height = (int)Math.Round(_labelBitmap.Height * scale);
        var x = bounds.Left + (bounds.Width - width) / 2;
        var y = bounds.Top + (bounds.Height - height) / 2;

        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(_labelBitmap, new Rectangle(x, y, width, height));
        e.HasMorePages = false;
    }

    private void CopyQrPayload()
    {
        if (_device is null)
            return;

        PermissionService.Demand(PermissionCodes.QrBarcodeGenerate, "sao chép mã QR thiết bị");
        Clipboard.SetText(DeviceCodeService.BuildQrPayload(_device));
        MessageBox.Show("Đã sao chép nội dung QR vào Clipboard.", "QR / Barcode", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ReplacePreview(Bitmap bitmap)
    {
        _preview.Image = null;
        _labelBitmap?.Dispose();
        _labelBitmap = bitmap;
        _preview.Image = _labelBitmap;
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
