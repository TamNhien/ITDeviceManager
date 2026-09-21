using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ITDeviceManager.Models;
using ZXing;
using ZXing.Common;

namespace ITDeviceManager.Services;

public sealed record DeviceLabelSnapshot(
    int Id,
    string Code,
    string Name,
    string TypeName,
    string? SerialNumber,
    string Status,
    string? DepartmentName);

public sealed record ParsedDeviceCode(int? Id, string? Code, string? SerialNumber, string RawText);

public static class DeviceCodeService
{
    private const string QrPrefix = "ITDM:DEVICE;V=1";

    public static string BuildQrPayload(DeviceLabelSnapshot device)
    {
        static string Esc(string? value) => Uri.EscapeDataString(value?.Trim() ?? string.Empty);

        return string.Join(";",
            QrPrefix,
            $"ID={device.Id}",
            $"CODE={Esc(device.Code)}",
            $"SERIAL={Esc(device.SerialNumber)}");
    }

    public static ParsedDeviceCode ParseScanText(string? input)
    {
        var raw = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return new ParsedDeviceCode(null, null, null, string.Empty);

        if (!raw.StartsWith("ITDM:DEVICE;", StringComparison.OrdinalIgnoreCase))
            return new ParsedDeviceCode(null, raw, raw, raw);

        int? id = null;
        string? code = null;
        string? serial = null;

        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = part.IndexOf('=');
            if (index <= 0 || index == part.Length - 1)
                continue;

            var key = part[..index].Trim();
            var value = Uri.UnescapeDataString(part[(index + 1)..].Trim());
            if (key.Equals("ID", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var parsedId))
                id = parsedId;
            else if (key.Equals("CODE", StringComparison.OrdinalIgnoreCase))
                code = value;
            else if (key.Equals("SERIAL", StringComparison.OrdinalIgnoreCase))
                serial = value;
        }

        return new ParsedDeviceCode(id, NullIfWhiteSpace(code), NullIfWhiteSpace(serial), raw);
    }

    public static Bitmap CreateQrBitmap(string payload, int size = 320)
        => CreateBarcodeBitmap(payload, BarcodeFormat.QR_CODE, size, size, margin: 2);

    public static Bitmap CreateCode128Bitmap(string value, int width = 620, int height = 150)
        => CreateBarcodeBitmap(value, BarcodeFormat.CODE_128, width, height, margin: 10);

    public static Bitmap CreateDeviceLabel(DeviceLabelSnapshot device, int width = 1100, int height = 620)
    {
        var canvas = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(canvas);
        graphics.Clear(Color.White);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var borderPen = new Pen(Color.FromArgb(31, 41, 55), 4F);
        graphics.DrawRectangle(borderPen, 8, 8, width - 16, height - 16);

        using var titleFont = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Pixel);
        using var codeFont = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Pixel);
        using var nameFont = new Font("Segoe UI", 25F, FontStyle.Bold, GraphicsUnit.Pixel);
        using var labelFont = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
        using var smallFont = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Pixel);
        using var darkBrush = new SolidBrush(Color.FromArgb(17, 24, 39));
        using var secondaryBrush = new SolidBrush(Color.FromArgb(75, 85, 99));

        graphics.DrawString("IT DEVICE MANAGER", titleFont, darkBrush, new PointF(36, 28));
        graphics.DrawString("Nhãn tài sản CNTT", smallFont, secondaryBrush, new PointF(38, 66));

        using var qr = CreateQrBitmap(BuildQrPayload(device), 300);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        graphics.DrawImage(qr, new Rectangle(36, 112, 300, 300));

        var infoX = 372F;
        graphics.DrawString(device.Code, codeFont, darkBrush, new PointF(infoX, 110));
        DrawWrapped(graphics, device.Name, nameFont, darkBrush, new RectangleF(infoX, 160, width - infoX - 44, 78));

        var y = 252F;
        DrawField(graphics, "Loại", device.TypeName, labelFont, darkBrush, secondaryBrush, infoX, ref y, width - infoX - 44);
        DrawField(graphics, "Serial", string.IsNullOrWhiteSpace(device.SerialNumber) ? "—" : device.SerialNumber!, labelFont, darkBrush, secondaryBrush, infoX, ref y, width - infoX - 44);
        DrawField(graphics, "Trạng thái", device.Status, labelFont, darkBrush, secondaryBrush, infoX, ref y, width - infoX - 44);
        DrawField(graphics, "Phòng ban", string.IsNullOrWhiteSpace(device.DepartmentName) ? "—" : device.DepartmentName!, labelFont, darkBrush, secondaryBrush, infoX, ref y, width - infoX - 44);

        var barcodeValue = NormalizeBarcodeValue(device.Code);
        using var barcode = CreateCode128Bitmap(barcodeValue, 830, 120);
        graphics.DrawImage(barcode, new Rectangle(220, 440, 830, 120));
        using var barcodeTextFont = new Font("Consolas", 18F, FontStyle.Bold, GraphicsUnit.Pixel);
        var textSize = graphics.MeasureString(barcodeValue, barcodeTextFont);
        graphics.DrawString(barcodeValue, barcodeTextFont, darkBrush, new PointF((width - textSize.Width) / 2F, 568));

        return canvas;
    }

    public static string GetDefaultLabelDirectory()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ITDeviceManager", "Labels");

    private static Bitmap CreateBarcodeBitmap(string contents, BarcodeFormat format, int width, int height, int margin)
    {
        if (string.IsNullOrWhiteSpace(contents))
            throw new ArgumentException("Nội dung mã không được để trống.", nameof(contents));

        var writer = new BarcodeWriterPixelData
        {
            Format = format,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = margin,
                PureBarcode = true
            }
        };

        var pixelData = writer.Write(contents);
        var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb);
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, pixelData.Width, pixelData.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppRgb);

        try
        {
            Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return bitmap;
    }

    private static string NormalizeBarcodeValue(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new InvalidOperationException("Thiết bị chưa có mã để tạo barcode.");
        if (normalized.Length > 80)
            normalized = normalized[..80];
        return normalized;
    }

    private static void DrawField(
        Graphics graphics,
        string label,
        string value,
        Font font,
        Brush valueBrush,
        Brush labelBrush,
        float x,
        ref float y,
        float width)
    {
        graphics.DrawString(label + ":", font, labelBrush, new PointF(x, y));
        DrawWrapped(graphics, value, font, valueBrush, new RectangleF(x + 132, y, Math.Max(80, width - 132), 48));
        y += 48;
    }

    private static void DrawWrapped(Graphics graphics, string text, Font font, Brush brush, RectangleF bounds)
    {
        using var format = new StringFormat
        {
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.LineLimit
        };
        graphics.DrawString(text, font, brush, bounds, format);
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
