using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Services;

public sealed class ExcelImportPreviewRow
{
    public int RowNumber { get; init; }
    public string SheetName { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Reference { get; init; } = string.Empty;
    public bool IsValid { get; internal set; }
    public string ErrorText { get; internal set; } = string.Empty;
    internal DeviceImportPayload? Device { get; init; }
    internal EmployeeImportPayload? Employee { get; init; }
}

public sealed record ExcelImportPreviewResult(IReadOnlyList<ExcelImportPreviewRow> Rows)
{
    public int TotalCount => Rows.Count;
    public int ValidCount => Rows.Count(x => x.IsValid);
    public int InvalidCount => Rows.Count(x => !x.IsValid);
    public int DeviceCount => Rows.Count(x => x.Device is not null);
    public int EmployeeCount => Rows.Count(x => x.Employee is not null);
}

public sealed record ExcelImportCommitResult(int ImportedDevices, int ImportedEmployees, int SkippedRows)
{
    public int ImportedTotal => ImportedDevices + ImportedEmployees;
}

internal sealed record DeviceImportPayload(
    string Code,
    string Name,
    string? SerialNumber,
    int DeviceTypeId,
    DateTime? PurchaseDate,
    decimal? PurchasePrice,
    DateTime? WarrantyEndDate,
    int? MaintenanceIntervalMonths,
    DateTime? NextMaintenanceDate,
    DeviceStatus Status,
    int? DepartmentId);

internal sealed record EmployeeImportPayload(
    string Code,
    string FullName,
    string? Email,
    string? Phone,
    int DepartmentId);

internal sealed record ExistingDevice(string Code, string? SerialNumber, bool IsDeleted);
internal sealed record ExistingEmployee(string Code, bool IsDeleted);
internal sealed record DeviceTypeLookup(int Id, string Name);
internal sealed record DepartmentLookup(int Id, string Code, string Name);

public static class ExcelImportService
{
    public const string DeviceSheetName = "ThietBi";
    public const string EmployeeSheetName = "NhanVien";

    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    public static void CreateTemplate(string path)
    {
        using var workbook = new XLWorkbook();
        CreateInstructionSheet(workbook);
        CreateDeviceSheet(workbook);
        CreateEmployeeSheet(workbook);
        workbook.SaveAs(path);
    }

    public static async Task<ExcelImportPreviewResult> PreviewAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new FileNotFoundException("Không tìm thấy file Excel cần nhập.", path);

        await using var db = new AppDbContext();

        var existingDevices = await db.Devices.IgnoreQueryFilters().AsNoTracking()
            .Select(x => new ExistingDevice(x.Code, x.SerialNumber, x.IsDeleted))
            .ToListAsync(cancellationToken);
        var existingEmployees = await db.Employees.IgnoreQueryFilters().AsNoTracking()
            .Select(x => new ExistingEmployee(x.Code, x.IsDeleted))
            .ToListAsync(cancellationToken);
        var activeTypes = await db.DeviceTypes.AsNoTracking()
            .Select(x => new DeviceTypeLookup(x.Id, x.Name))
            .ToListAsync(cancellationToken);
        var activeDepartments = await db.Departments.AsNoTracking()
            .Select(x => new DepartmentLookup(x.Id, x.Code, x.Name))
            .ToListAsync(cancellationToken);

        var rows = new List<ExcelImportPreviewRow>();
        using var workbook = new XLWorkbook(path);

        if (workbook.TryGetWorksheet(DeviceSheetName, out var deviceSheet))
            rows.AddRange(ParseDeviceRows(deviceSheet, existingDevices, activeTypes, activeDepartments));

        if (workbook.TryGetWorksheet(EmployeeSheetName, out var employeeSheet))
            rows.AddRange(ParseEmployeeRows(employeeSheet, existingEmployees, activeDepartments));

        if (rows.Count == 0)
            throw new InvalidDataException($"File Excel không có dữ liệu trong sheet '{DeviceSheetName}' hoặc '{EmployeeSheetName}'. Hãy dùng file mẫu do chương trình tạo.");

        MarkWorkbookDuplicates(rows);
        return new ExcelImportPreviewResult(rows);
    }

    public static async Task<ExcelImportCommitResult> ImportAsync(
        IEnumerable<ExcelImportPreviewRow> previewRows,
        CancellationToken cancellationToken = default)
    {
        PermissionService.Demand(PermissionCodes.ExcelImport, "nhập dữ liệu từ Excel");

        var candidates = previewRows.Where(x => x.IsValid).ToList();
        if (candidates.Count == 0)
            return new ExcelImportCommitResult(0, 0, 0);

        var wantsDevices = candidates.Any(x => x.Device is not null);
        var wantsEmployees = candidates.Any(x => x.Employee is not null);
        if (wantsDevices)
            PermissionService.Demand(PermissionCodes.DeviceCreate, "thêm thiết bị bằng Excel");
        if (wantsEmployees)
            PermissionService.Demand(PermissionCodes.EmployeeCreate, "thêm nhân viên bằng Excel");

        await using var db = new AppDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var skipped = 0;
        var importedDevices = 0;
        var importedEmployees = 0;

        try
        {
            var currentDeviceCodes = (await db.Devices.IgnoreQueryFilters().AsNoTracking()
                    .Select(x => x.Code)
                    .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentSerials = (await db.Devices.IgnoreQueryFilters().AsNoTracking()
                    .Where(x => x.SerialNumber != null)
                    .Select(x => x.SerialNumber!)
                    .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentEmployeeCodes = (await db.Employees.IgnoreQueryFilters().AsNoTracking()
                    .Select(x => x.Code)
                    .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var activeTypeIds = (await db.DeviceTypes.AsNoTracking().Select(x => x.Id).ToListAsync(cancellationToken)).ToHashSet();
            var activeDepartmentIds = (await db.Departments.AsNoTracking().Select(x => x.Id).ToListAsync(cancellationToken)).ToHashSet();

            foreach (var row in candidates.OrderBy(x => x.EntityType).ThenBy(x => x.RowNumber))
            {
                if (row.Device is { } device)
                {
                    if (currentDeviceCodes.Contains(device.Code) ||
                        (device.SerialNumber is not null && currentSerials.Contains(device.SerialNumber)) ||
                        !activeTypeIds.Contains(device.DeviceTypeId) ||
                        (device.DepartmentId.HasValue && !activeDepartmentIds.Contains(device.DepartmentId.Value)))
                    {
                        skipped++;
                        continue;
                    }

                    db.Devices.Add(new Device
                    {
                        Code = device.Code,
                        Name = device.Name,
                        SerialNumber = device.SerialNumber,
                        DeviceTypeId = device.DeviceTypeId,
                        PurchaseDate = device.PurchaseDate,
                        PurchasePrice = device.PurchasePrice,
                        WarrantyEndDate = device.WarrantyEndDate,
                        MaintenanceIntervalMonths = device.MaintenanceIntervalMonths,
                        NextMaintenanceDate = device.NextMaintenanceDate ??
                            (device.MaintenanceIntervalMonths is int interval && device.PurchaseDate is DateTime purchase
                                ? purchase.AddMonths(interval)
                                : null),
                        Status = device.Status,
                        DepartmentId = device.DepartmentId
                    });
                    currentDeviceCodes.Add(device.Code);
                    if (device.SerialNumber is not null)
                        currentSerials.Add(device.SerialNumber);
                    importedDevices++;
                    continue;
                }

                if (row.Employee is { } employee)
                {
                    if (currentEmployeeCodes.Contains(employee.Code) || !activeDepartmentIds.Contains(employee.DepartmentId))
                    {
                        skipped++;
                        continue;
                    }

                    db.Employees.Add(new Employee
                    {
                        Code = employee.Code,
                        FullName = employee.FullName,
                        Email = employee.Email,
                        Phone = employee.Phone,
                        DepartmentId = employee.DepartmentId
                    });
                    currentEmployeeCodes.Add(employee.Code);
                    importedEmployees++;
                }
            }

            if (importedDevices + importedEmployees > 0)
                await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await AuditService.TryWriteAsync(
            "Nhập Excel",
            "Nhập dữ liệu",
            $"Đã nhập {importedDevices} thiết bị, {importedEmployees} nhân viên; bỏ qua {skipped} dòng thay đổi sau bước xem trước.");

        return new ExcelImportCommitResult(importedDevices, importedEmployees, skipped);
    }

    private static IEnumerable<ExcelImportPreviewRow> ParseDeviceRows(
        IXLWorksheet sheet,
        IReadOnlyCollection<ExistingDevice> existingDevices,
        IReadOnlyCollection<DeviceTypeLookup> activeTypes,
        IReadOnlyCollection<DepartmentLookup> activeDepartments)
    {
        var headers = ReadHeaders(sheet);
        RequireHeaders(headers, "Mã thiết bị", "Tên thiết bị", "Loại thiết bị");

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = sheet.Row(rowNumber);
            if (IsEmptyRow(row))
                continue;

            var errors = new List<string>();
            var code = GetText(row, headers, "Mã thiết bị").ToUpperInvariant();
            var name = GetText(row, headers, "Tên thiết bị");
            var typeText = GetText(row, headers, "Loại thiết bị");
            var serial = NullIfBlank(GetText(row, headers, "Serial"));
            var departmentText = GetText(row, headers, "Phòng ban");

            if (!Regex.IsMatch(code, @"^TB\d+$"))
                errors.Add("Mã thiết bị phải có dạng TB001.");
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Tên thiết bị bắt buộc.");

            var existingByCode = existingDevices.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
            if (existingByCode is not null)
                errors.Add(existingByCode.IsDeleted ? "Mã thiết bị đang nằm trong Thùng rác; hãy khôi phục thay vì import." : "Mã thiết bị đã tồn tại.");
            if (serial is not null)
            {
                var existingBySerial = existingDevices.FirstOrDefault(x => x.SerialNumber is not null && string.Equals(x.SerialNumber, serial, StringComparison.OrdinalIgnoreCase));
                if (existingBySerial is not null)
                    errors.Add(existingBySerial.IsDeleted ? "Serial thuộc thiết bị đang nằm trong Thùng rác." : "Serial đã tồn tại.");
            }

            var typeMatches = activeTypes.Where(x => string.Equals(x.Name, typeText, StringComparison.OrdinalIgnoreCase)).ToList();
            var typeId = typeMatches.Count == 1 ? typeMatches[0].Id : 0;
            if (string.IsNullOrWhiteSpace(typeText))
                errors.Add("Loại thiết bị bắt buộc.");
            else if (typeMatches.Count == 0)
                errors.Add($"Không tìm thấy loại thiết bị '{typeText}'.");
            else if (typeMatches.Count > 1)
                errors.Add($"Loại thiết bị '{typeText}' bị trùng tên trong danh mục.");

            int? departmentId = null;
            if (!string.IsNullOrWhiteSpace(departmentText))
            {
                var depMatches = activeDepartments.Where(x =>
                    string.Equals(x.Code, departmentText, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, departmentText, StringComparison.OrdinalIgnoreCase)).ToList();
                if (depMatches.Count == 1)
                    departmentId = depMatches[0].Id;
                else if (depMatches.Count == 0)
                    errors.Add($"Không tìm thấy phòng ban '{departmentText}'.");
                else
                    errors.Add($"Phòng ban '{departmentText}' không xác định duy nhất.");
            }

            var purchaseDate = TryReadDate(row, headers, "Ngày mua", errors, "Ngày mua");
            var warrantyEndDate = TryReadDate(row, headers, "Hạn bảo hành", errors, "Hạn bảo hành");
            var nextMaintenanceDate = TryReadDate(row, headers, "Bảo trì kế tiếp", errors, "Bảo trì kế tiếp");
            var purchasePrice = TryReadDecimal(row, headers, "Giá mua", errors, "Giá mua", 1_000_000_000_000m);
            var interval = TryReadInt(row, headers, "Chu kỳ BT (tháng)", errors, "Chu kỳ BT", 1, 120);
            var status = ParseStatus(GetText(row, headers, "Trạng thái"), errors);

            if (purchaseDate.HasValue && purchaseDate.Value.Date > DateTime.Today)
                errors.Add("Ngày mua không được lớn hơn hôm nay.");
            if (purchaseDate.HasValue && warrantyEndDate.HasValue && warrantyEndDate.Value.Date < purchaseDate.Value.Date)
                errors.Add("Hạn bảo hành không được trước ngày mua.");
            if (purchaseDate.HasValue && nextMaintenanceDate.HasValue && nextMaintenanceDate.Value.Date < purchaseDate.Value.Date)
                errors.Add("Bảo trì kế tiếp không được trước ngày mua.");

            yield return new ExcelImportPreviewRow
            {
                RowNumber = rowNumber,
                SheetName = sheet.Name,
                EntityType = "Thiết bị",
                Code = code,
                Name = name,
                Reference = string.Join(" • ", new[] { typeText, serial, departmentText }.Where(x => !string.IsNullOrWhiteSpace(x))),
                IsValid = errors.Count == 0,
                ErrorText = string.Join(" ", errors),
                Device = new DeviceImportPayload(
                    code,
                    name,
                    serial,
                    typeId,
                    purchaseDate,
                    purchasePrice,
                    warrantyEndDate,
                    interval,
                    nextMaintenanceDate,
                    status,
                    departmentId)
            };
        }
    }

    private static IEnumerable<ExcelImportPreviewRow> ParseEmployeeRows(
        IXLWorksheet sheet,
        IReadOnlyCollection<ExistingEmployee> existingEmployees,
        IReadOnlyCollection<DepartmentLookup> activeDepartments)
    {
        var headers = ReadHeaders(sheet);
        RequireHeaders(headers, "Mã nhân viên", "Họ tên", "Phòng ban");

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = sheet.Row(rowNumber);
            if (IsEmptyRow(row))
                continue;

            var errors = new List<string>();
            var code = GetText(row, headers, "Mã nhân viên").ToUpperInvariant();
            var fullName = GetText(row, headers, "Họ tên");
            var email = NullIfBlank(GetText(row, headers, "Email"));
            var rawPhone = GetText(row, headers, "Điện thoại");
            var phone = NullIfBlank(PhoneNumberValidator.Normalize(rawPhone));
            var departmentText = GetText(row, headers, "Phòng ban");

            if (!Regex.IsMatch(code, @"^NV\d+$"))
                errors.Add("Mã nhân viên phải có dạng NV001.");
            if (string.IsNullOrWhiteSpace(fullName))
                errors.Add("Họ tên bắt buộc.");
            if (email is not null && !EmailAddressValidator.IsValid(email))
                errors.Add("Email không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(rawPhone) && (phone is null || !PhoneNumberValidator.IsValid(phone)))
                errors.Add("Điện thoại không hợp lệ.");

            var existing = existingEmployees.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
                errors.Add(existing.IsDeleted ? "Mã nhân viên đang nằm trong Thùng rác; hãy khôi phục thay vì import." : "Mã nhân viên đã tồn tại.");

            var depMatches = activeDepartments.Where(x =>
                string.Equals(x.Code, departmentText, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Name, departmentText, StringComparison.OrdinalIgnoreCase)).ToList();
            var departmentId = depMatches.Count == 1 ? depMatches[0].Id : 0;
            if (string.IsNullOrWhiteSpace(departmentText))
                errors.Add("Phòng ban bắt buộc.");
            else if (depMatches.Count == 0)
                errors.Add($"Không tìm thấy phòng ban '{departmentText}'.");
            else if (depMatches.Count > 1)
                errors.Add($"Phòng ban '{departmentText}' không xác định duy nhất.");

            yield return new ExcelImportPreviewRow
            {
                RowNumber = rowNumber,
                SheetName = sheet.Name,
                EntityType = "Nhân viên",
                Code = code,
                Name = fullName,
                Reference = string.Join(" • ", new[] { email, phone, departmentText }.Where(x => !string.IsNullOrWhiteSpace(x))),
                IsValid = errors.Count == 0,
                ErrorText = string.Join(" ", errors),
                Employee = new EmployeeImportPayload(code, fullName, email, phone, departmentId)
            };
        }
    }

    private static void MarkWorkbookDuplicates(List<ExcelImportPreviewRow> rows)
    {
        foreach (var group in rows.Where(x => !string.IsNullOrWhiteSpace(x.Code))
                     .GroupBy(x => (x.EntityType, Code: x.Code), new EntityCodeComparer())
                     .Where(x => x.Count() > 1))
        {
            foreach (var row in group)
                AddError(row, "Mã bị trùng trong chính file Excel.");
        }

        var deviceSerialGroups = rows.Where(x => x.Device?.SerialNumber is not null)
            .GroupBy(x => x.Device!.SerialNumber!, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1);
        foreach (var group in deviceSerialGroups)
            foreach (var row in group)
                AddError(row, "Serial bị trùng trong chính file Excel.");
    }

    private static void AddError(ExcelImportPreviewRow row, string message)
    {
        row.IsValid = false;
        row.ErrorText = string.IsNullOrWhiteSpace(row.ErrorText) ? message : row.ErrorText + " " + message;
    }

    private static Dictionary<string, int> ReadHeaders(IXLWorksheet sheet)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastCell = sheet.Row(1).LastCellUsed();
        if (lastCell is null)
            return map;

        for (var col = 1; col <= lastCell.Address.ColumnNumber; col++)
        {
            var raw = sheet.Cell(1, col).GetFormattedString();
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            map[NormalizeKey(raw)] = col;
        }
        return map;
    }

    private static void RequireHeaders(IReadOnlyDictionary<string, int> headers, params string[] names)
    {
        var missing = names.Where(x => !headers.ContainsKey(NormalizeKey(x))).ToList();
        if (missing.Count > 0)
            throw new InvalidDataException("Thiếu cột bắt buộc: " + string.Join(", ", missing) + ". Hãy dùng file mẫu do chương trình tạo.");
    }

    private static string GetText(IXLRow row, IReadOnlyDictionary<string, int> headers, string header)
    {
        if (!headers.TryGetValue(NormalizeKey(header), out var column))
            return string.Empty;
        return row.Cell(column).GetFormattedString().Trim();
    }

    private static DateTime? TryReadDate(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, List<string> errors, string friendlyName)
    {
        if (!headers.TryGetValue(NormalizeKey(header), out var column))
            return null;
        var cell = row.Cell(column);
        if (cell.IsEmpty())
            return null;
        if (cell.TryGetValue<DateTime>(out var direct))
            return direct.Date;

        var text = cell.GetFormattedString().Trim();
        if (DateTime.TryParseExact(text, ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"], ViCulture, DateTimeStyles.None, out var parsed))
            return parsed.Date;
        errors.Add($"{friendlyName} phải có dạng dd/MM/yyyy.");
        return null;
    }

    private static decimal? TryReadDecimal(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, List<string> errors, string friendlyName, decimal max)
    {
        if (!headers.TryGetValue(NormalizeKey(header), out var column))
            return null;
        var cell = row.Cell(column);
        if (cell.IsEmpty())
            return null;
        if (cell.TryGetValue<decimal>(out var direct) && direct > 0 && direct <= max)
            return direct;

        var normalized = cell.GetFormattedString().Trim().Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty);
        if (decimal.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed > 0 && parsed <= max)
            return parsed;
        errors.Add($"{friendlyName} phải là số dương, tối đa {max:N0}.");
        return null;
    }

    private static int? TryReadInt(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, List<string> errors, string friendlyName, int min, int max)
    {
        if (!headers.TryGetValue(NormalizeKey(header), out var column))
            return null;
        var cell = row.Cell(column);
        if (cell.IsEmpty())
            return null;
        var text = cell.GetFormattedString().Trim();
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= min && value <= max)
            return value;
        errors.Add($"{friendlyName} phải từ {min} đến {max}.");
        return null;
    }

    private static DeviceStatus ParseStatus(string text, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(text))
            return DeviceStatus.Available;

        foreach (var status in Enum.GetValues<DeviceStatus>())
        {
            if (string.Equals(text, status.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, status.ToDisplayName(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, ((int)status).ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                return status;
        }

        errors.Add("Trạng thái không hợp lệ. Dùng: Chưa sử dụng, Đang sử dụng, Đang sửa chữa, Hỏng hoặc Thanh lý.");
        return DeviceStatus.Available;
    }

    private static bool IsEmptyRow(IXLRow row)
        => row.CellsUsed().All(x => string.IsNullOrWhiteSpace(x.GetFormattedString()));

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeKey(string value)
    {
        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(c))
                builder.Append(char.ToLowerInvariant(c));
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static void CreateInstructionSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("HuongDan");
        sheet.Cell("A1").Value = "HƯỚNG DẪN IMPORT IT DEVICE MANAGER";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 14;
        sheet.Cell("A3").Value = "1. Không đổi tên sheet ThietBi và NhanVien.";
        sheet.Cell("A4").Value = "2. Không đổi tên các cột ở dòng 1.";
        sheet.Cell("A5").Value = "3. Loại thiết bị phải tồn tại trong danh mục Loại thiết bị.";
        sheet.Cell("A6").Value = "4. Phòng ban nhập bằng mã (VD: CNTT) hoặc đúng tên phòng ban.";
        sheet.Cell("A7").Value = "5. Ngày nhập theo dd/MM/yyyy. Dòng lỗi sẽ không được ghi vào database.";
        sheet.Cell("A8").Value = "6. Mã/serial đã nằm trong Thùng rác phải được khôi phục, không được import lại.";
        sheet.Column(1).Width = 110;
    }

    private static void CreateDeviceSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add(DeviceSheetName);
        var headers = new[]
        {
            "Mã thiết bị", "Tên thiết bị", "Loại thiết bị", "Serial", "Ngày mua", "Giá mua",
            "Hạn bảo hành", "Chu kỳ BT (tháng)", "Bảo trì kế tiếp", "Trạng thái", "Phòng ban"
        };
        WriteHeaders(sheet, headers);
        sheet.Cell(2, 1).Value = "TB100";
        sheet.Cell(2, 2).Value = "Laptop Dell Latitude";
        sheet.Cell(2, 3).Value = "Laptop";
        sheet.Cell(2, 4).Value = "DEMO-SN-100";
        sheet.Cell(2, 5).Value = DateTime.Today.AddMonths(-6);
        sheet.Cell(2, 6).Value = 18000000d;
        sheet.Cell(2, 7).Value = DateTime.Today.AddMonths(18);
        sheet.Cell(2, 8).Value = 6;
        sheet.Cell(2, 9).Value = DateTime.Today.AddMonths(6);
        sheet.Cell(2, 10).Value = "Chưa sử dụng";
        sheet.Cell(2, 11).Value = "CNTT";
        foreach (var column in new[] { 5, 7, 9 })
            sheet.Column(column).Style.DateFormat.Format = "dd/MM/yyyy";

        SetColumnWidths(sheet, new double[]
        {
            16, // Mã thiết bị
            28, // Tên thiết bị
            20, // Loại thiết bị
            20, // Serial
            16, // Ngày mua
            16, // Giá mua
            18, // Hạn bảo hành
            20, // Chu kỳ BT (tháng)
            18, // Bảo trì kế tiếp
            18, // Trạng thái
            18  // Phòng ban
        });
        CenterDataColumns(sheet, headers.Length);
        sheet.SheetView.FreezeRows(1);
    }

    private static void CreateEmployeeSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add(EmployeeSheetName);
        var headers = new[] { "Mã nhân viên", "Họ tên", "Email", "Điện thoại", "Phòng ban" };
        WriteHeaders(sheet, headers);
        sheet.Cell(2, 1).Value = "NV100";
        sheet.Cell(2, 2).Value = "Nguyễn Văn Mẫu";
        sheet.Cell(2, 3).Value = "nv100@example.com";
        sheet.Cell(2, 4).Value = "0901234567";
        sheet.Cell(2, 5).Value = "CNTT";
        SetColumnWidths(sheet, new double[]
        {
            18, // Mã nhân viên
            28, // Họ tên
            30, // Email
            18, // Điện thoại
            18  // Phòng ban
        });
        CenterDataColumns(sheet, headers.Length);
        sheet.SheetView.FreezeRows(1);
    }

    private static void WriteHeaders(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
            sheet.Cell(1, i + 1).Value = headers[i];
        var range = sheet.Range(1, 1, 1, headers.Count);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("DCE6F1");
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Alignment.WrapText = true;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        sheet.Row(1).Height = 36;
    }

    private static void SetColumnWidths(IXLWorksheet sheet, IReadOnlyList<double> widths)
    {
        for (var i = 0; i < widths.Count; i++)
            sheet.Column(i + 1).Width = widths[i];
    }

    private static void CenterDataColumns(IXLWorksheet sheet, int columnCount)
    {
        var columns = sheet.Columns(1, columnCount);
        columns.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        columns.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    private sealed class EntityCodeComparer : IEqualityComparer<(string EntityType, string Code)>
    {
        public bool Equals((string EntityType, string Code) x, (string EntityType, string Code) y)
            => string.Equals(x.EntityType, y.EntityType, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(x.Code, y.Code, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string EntityType, string Code) obj)
            => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.EntityType), StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Code));
    }
}
