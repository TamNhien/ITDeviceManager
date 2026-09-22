using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Services;

public enum DeviceAlertType
{
    Warranty = 1,
    Maintenance = 2
}

public sealed record DeviceAlertItem(
    int DeviceId,
    string DeviceCode,
    string DeviceName,
    string? SerialNumber,
    DeviceAlertType Type,
    DateTime DueDate,
    int DaysRemaining,
    int? MaintenanceIntervalMonths)
{
    public bool IsOverdue => DaysRemaining < 0;

    public string TypeName => Type switch
    {
        DeviceAlertType.Warranty => "Bảo hành",
        DeviceAlertType.Maintenance => "Bảo trì",
        _ => Type.ToString()
    };

    public string SeverityName => DaysRemaining switch
    {
        < 0 => "Quá hạn",
        0 => "Hôm nay",
        <= 7 => "Sắp đến hạn",
        _ => "Cần theo dõi"
    };

    public string RemainingText => DaysRemaining switch
    {
        < 0 => $"Quá {-DaysRemaining} ngày",
        0 => "Hôm nay",
        1 => "Còn 1 ngày",
        _ => $"Còn {DaysRemaining} ngày"
    };
}

public sealed record DeviceAlertSummary(int WarrantyCount, int MaintenanceCount)
{
    public int TotalCount => WarrantyCount + MaintenanceCount;
}

public static class DeviceAlertService
{
    public const int DefaultLeadDays = 30;
    public const int WarrantyExpiredGraceDays = 30;

    public static async Task<IReadOnlyList<DeviceAlertItem>> GetAlertsAsync(
        int leadDays = DefaultLeadDays,
        CancellationToken cancellationToken = default)
    {
        leadDays = Math.Clamp(leadDays, 1, 365);
        var today = DateTime.Today;
        var endDate = today.AddDays(leadDays);
        var warrantyFloor = today.AddDays(-WarrantyExpiredGraceDays);

        await using var db = new AppDbContext();
        var devices = await db.Devices
            .AsNoTracking()
            .Where(x => x.Status != DeviceStatus.Retired &&
                        ((x.WarrantyEndDate.HasValue && x.WarrantyEndDate.Value >= warrantyFloor && x.WarrantyEndDate.Value <= endDate) ||
                         (x.NextMaintenanceDate.HasValue && x.NextMaintenanceDate.Value <= endDate)))
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.SerialNumber,
                x.WarrantyEndDate,
                x.NextMaintenanceDate,
                x.MaintenanceIntervalMonths
            })
            .ToListAsync(cancellationToken);

        var alerts = new List<DeviceAlertItem>(devices.Count * 2);
        foreach (var device in devices)
        {
            if (device.WarrantyEndDate is DateTime warrantyDate &&
                warrantyDate.Date >= warrantyFloor && warrantyDate.Date <= endDate)
            {
                var due = warrantyDate.Date;
                alerts.Add(new DeviceAlertItem(
                    device.Id,
                    device.Code,
                    device.Name,
                    device.SerialNumber,
                    DeviceAlertType.Warranty,
                    due,
                    (due - today).Days,
                    device.MaintenanceIntervalMonths));
            }

            if (device.NextMaintenanceDate is DateTime maintenanceDate && maintenanceDate.Date <= endDate)
            {
                var due = maintenanceDate.Date;
                alerts.Add(new DeviceAlertItem(
                    device.Id,
                    device.Code,
                    device.Name,
                    device.SerialNumber,
                    DeviceAlertType.Maintenance,
                    due,
                    (due - today).Days,
                    device.MaintenanceIntervalMonths));
            }
        }

        return alerts
            .OrderBy(x => x.DaysRemaining)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.DeviceCode)
            .ToList();
    }

    public static async Task<DeviceAlertSummary> GetSummaryAsync(
        int leadDays = DefaultLeadDays,
        CancellationToken cancellationToken = default)
    {
        var alerts = await GetAlertsAsync(leadDays, cancellationToken);
        return new DeviceAlertSummary(
            alerts.Count(x => x.Type == DeviceAlertType.Warranty),
            alerts.Count(x => x.Type == DeviceAlertType.Maintenance));
    }
}
