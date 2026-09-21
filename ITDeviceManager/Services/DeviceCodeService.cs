using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using ITDeviceManager.Models;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace ITDeviceManager.Services;

// Legacy snapshot contract used by DeviceLabelForm/DeviceScanForm.
// Kept intentionally for backward compatibility with the pre-V2 label/scan flow.
public sealed record DeviceLabelSnapshot(
    int Id,
    string Code,
    string Name,
    string TypeName,
    string? SerialNumber,
    string Status,
    string? DepartmentName);

public sealed record ParsedDeviceCode(int? Id, string? Code, string? SerialNumber, string RawText);

// V2 descriptor used by the QR / Barcode management module.
public sealed record DeviceCodeDescriptor(
    int Id,
    string Code,
    string Name,
    string DeviceType,
    string? SerialNumber,
    DeviceStatus Status);

public sealed record DeviceCodeFiles(string QrPath, string BarcodePath);

public static class DeviceCodeService
{
    private const string LegacyQrPrefix = "ITDM:DEVICE;V=1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string StorageDirectory => AppSettings.QrDirectory;

    public static string EnsureStorageDirectory()
    {
        Directory.CreateDirectory(StorageDirectory);
        return StorageDirectory;
    }

    public static string GetQrPath(DeviceCodeDescriptor device)
        => Path.Combine(StorageDirectory, $"{SafeFileStem(device.Code)}_QR.png");

    public static string GetBarcodePath(DeviceCodeDescriptor device)
        => Path.Combine(StorageDirectory, $"{SafeFileStem(device.Code)}_BARCODE.png");

    public static DeviceCodeFiles GenerateBoth(DeviceCodeDescriptor device)
        => new(GenerateQr(device), GenerateBarcode(device));

    public static string GenerateQr(DeviceCodeDescriptor device)
    {
        EnsureStorageDirectory();
        var path = GetQrPath(device);
        var payload = BuildQrPayload(device);

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Width = 720,
                Height = 720,
                Margin = 3,
                CharacterSet = "UTF-8",
                ErrorCorrection = ErrorCorrectionLevel.M
            }
        };

        var qrPixels = writer.Write(payload);
        using var symbol = PixelDataToBitmap(qrPixels.Pixels, qrPixels.Width, qrPixels.Height);
        using var labeled = BuildQrLabel(symbol, device);
        SavePngAtomically(labeled, path);
        return path;
    }

    public static string GenerateBarcode(DeviceCodeDescriptor device)
    {
        EnsureStorageDirectory();
        var path = GetBarcodePath(device);

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = 960,
                Height = 260,
                Margin = 18,
                PureBarcode = true
            }
        };

        var barcodePixels = writer.Write(NormalizeBarcodeValue(device.Code));
        using var symbol = PixelDataToBitmap(barcodePixels.Pixels, barcodePixels.Width, barcodePixels.Height);
        using var labeled = BuildBarcodeLabel(symbol, device);
        SavePngAtomically(labeled, path);
        return path;
    }

    // V2 JSON payload used by DeviceCodesForm.
    public static string BuildQrPayload(DeviceCodeDescriptor device)
    {
        var payload = new
        {
            schema = "ITDeviceManager.Device",
            version = 1,
            id = device.Id,
            code = device.Code,
            name = device.Name,
            serialNumber = device.SerialNumber,
            deviceType = device.DeviceType,
            status = device.Status.ToDisplayName()
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    // Legacy payload overload retained for DeviceLabelForm.
    public static string BuildQrPayload(DeviceLabelSnapshot device)
    {
        static string Esc(string? value) => Uri.EscapeDataString(value?.Trim() ?? string.Empty);

        return string.Join(";",
            LegacyQrPrefix,
            $"ID={device.Id}",
            $"CODE={Esc(device.Code)}",
            $"SERIAL={Esc(device.SerialNumber)}");
    }

    public static bool TryExtractDeviceCode(string? scannedValue, out string code)
    {
        code = string.Empty;
        var value = scannedValue?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // New V2 JSON QR payload.
        if (value.StartsWith('{'))
        {
            try
            {
                using var document = JsonDocument.Parse(value);
                if (document.RootElement.TryGetProperty("code", out var codeElement))
                {
                    var parsed = codeElement.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(parsed))
                    {
                        code = parsed;
                        return true;
                    }
                }
            }
            catch (JsonException)
            {
                return false;
            }

            return false;
        }

        // Legacy QR payload used by DeviceLabelForm.
        if (value.StartsWith("ITDM:DEVICE;", StringComparison.OrdinalIgnoreCase))
        {
            var parsed = ParseScanText(value);
            code = parsed.Code ?? parsed.SerialNumber ?? string.Empty;
            return !string.IsNullOrWhiteSpace(code);
        }

        // Plain barcode / manually typed device code.
        code = value;
        return true;
    }

    // Backward-compatible scanner parser. Accepts V2 JSON, legacy ITDM payload,
    // or a plain barcode/device code.
    public static ParsedDeviceCode ParseScanText(string? input)
    {
        var raw = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return new ParsedDeviceCode(null, null, null, string.Empty);

        if (raw.StartsWith('{'))
        {
            try
            {
                using var document = JsonDocument.Parse(raw);
                var root = document.RootElement;

                int? id = null;
                string? code = null;
                string? serial = null;

                if (root.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var parsedId))
                    id = parsedId;

                if (root.TryGetProperty("code", out var codeElement))
                    code = NullIfWhiteSpace(codeElement.GetString());

                if (root.TryGetProperty("serialNumber", out var serialElement))
                    serial = NullIfWhiteSpace(serialElement.GetString());

                if (id is not null || code is not null || serial is not null)
                    return new ParsedDeviceCode(id, code, serial, raw);
            }
            catch (JsonException)
            {
                // Fall through and let the raw value behave like normal scanner text.
            }
        }

        if (!raw.StartsWith("ITDM:DEVICE;", StringComparison.OrdinalIgnoreCase))
            return new ParsedDeviceCode(null, raw, raw, raw);

        int? legacyId = null;
        string? legacyCode = null;
        string? legacySerial = null;

        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = part.IndexOf('=');
            if (index <= 0 || index == part.Length - 1)
                continue;

            var key = part[..index].Trim();
            var encodedValue = part[(index + 1)..].Trim();
            string decodedValue;
            try
            {
                decodedValue = Uri.UnescapeDataString(encodedValue);
            }
            catch (UriFormatException)
            {
                decodedValue = encodedValue;
            }

            if (key.Equals("ID", StringComparison.OrdinalIgnoreCase) && int.TryParse(decodedValue, out var parsedId))
                legacyId = parsedId;
            else if (key.Equals("CODE", StringComparison.OrdinalIgnoreCase))
                legacyCode = decodedValue;
            else if (key.Equals("SERIAL", StringComparison.OrdinalIgnoreCase))
                legacySerial = decodedValue;
        }

        return new ParsedDeviceCode(
            legacyId,
            NullIfWhiteSpace(legacyCode),
            NullIfWhiteSpace(legacySerial),
            raw);
    }

    public static Bitmap CreateQrBitmap(string payload, int size = 320)
        => CreateBarcodeBitmap(payload, BarcodeFormat.QR_CODE, size, size, margin: 2);

    public static Bitmap CreateCode128Bitmap(string value, int width = 620, int height = 150)
        => CreateBarcodeBitmap(NormalizeBarcodeValue(value), BarcodeFormat.CODE_128, width, height, margin: 10);

    // Legacy combined label preview/print flow used by DeviceLabelForm.
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
        => EnsureStorageDirectory();

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
        return PixelDataToBitmap(pixelData.Pixels, pixelData.Width, pixelData.Height);
    }

    private static Bitmap PixelDataToBitmap(byte[] pixels, int width, int height)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
        var area = new Rectangle(0, 0, width, height);
        var bitmapData = bitmap.LockBits(area, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
        try
        {
            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }
        return bitmap;
    }

    private static Bitmap BuildQrLabel(Bitmap symbol, DeviceCodeDescriptor device)
    {
        const int footerHeight = 118;
        var canvas = new Bitmap(symbol.Width, symbol.Height + footerHeight, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(canvas);
        graphics.Clear(Color.White);
        graphics.DrawImageUnscaled(symbol, 0, 0);

        using var titleFont = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Pixel);
        using var detailFont = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.FromArgb(17, 24, 39));
        using var secondary = new SolidBrush(Color.FromArgb(75, 85, 99));

        graphics.DrawString($"{device.Code} - {device.Name}", titleFont, brush, new RectangleF(20, symbol.Height + 12, symbol.Width - 40, 34));
        var serial = string.IsNullOrWhiteSpace(device.SerialNumber) ? "Không có serial" : device.SerialNumber;
        graphics.DrawString($"{device.DeviceType}  |  Serial: {serial}", detailFont, secondary, new RectangleF(20, symbol.Height + 50, symbol.Width - 40, 26));
        graphics.DrawString($"Trạng thái: {device.Status.ToDisplayName()}", detailFont, secondary, new RectangleF(20, symbol.Height + 78, symbol.Width - 40, 26));
        return canvas;
    }

    private static Bitmap BuildBarcodeLabel(Bitmap symbol, DeviceCodeDescriptor device)
    {
        const int headerHeight = 72;
        const int footerHeight = 74;
        var canvas = new Bitmap(symbol.Width, symbol.Height + headerHeight + footerHeight, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(canvas);
        graphics.Clear(Color.White);

        using var codeFont = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Pixel);
        using var nameFont = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.FromArgb(17, 24, 39));
        using var secondary = new SolidBrush(Color.FromArgb(75, 85, 99));
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        graphics.DrawString(device.Code, codeFont, brush, new RectangleF(0, 8, canvas.Width, 38), center);
        graphics.DrawString(device.Name, nameFont, secondary, new RectangleF(16, 42, canvas.Width - 32, 28), center);
        graphics.DrawImageUnscaled(symbol, 0, headerHeight);

        var serial = string.IsNullOrWhiteSpace(device.SerialNumber) ? "Không có serial" : device.SerialNumber;
        graphics.DrawString($"{device.DeviceType}  |  Serial: {serial}", nameFont, secondary, new RectangleF(16, headerHeight + symbol.Height + 8, canvas.Width - 32, 28), center);
        graphics.DrawString(device.Status.ToDisplayName(), nameFont, secondary, new RectangleF(16, headerHeight + symbol.Height + 36, canvas.Width - 32, 28), center);
        return canvas;
    }

    private static void SavePngAtomically(Image image, string path)
    {
        var directory = Path.GetDirectoryName(path) ?? StorageDirectory;
        Directory.CreateDirectory(directory);
        var temp = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            image.Save(temp, ImageFormat.Png);
            File.Move(temp, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
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

    private static string SafeFileStem(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var result = new string(chars).Trim('.', ' ');
        return string.IsNullOrWhiteSpace(result) ? "device" : result;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
