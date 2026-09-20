namespace ITDeviceManager.Models;

public enum MaintenanceType
{
    Preventive = 1,
    Repair = 2,
    Warranty = 3,
    PartReplacement = 4,
    Inspection = 5
}

public enum MaintenanceStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public static class MaintenanceExtensions
{
    public static string ToDisplayName(this MaintenanceType type) => type switch
    {
        MaintenanceType.Preventive => "Bảo trì định kỳ",
        MaintenanceType.Repair => "Sửa chữa",
        MaintenanceType.Warranty => "Bảo hành",
        MaintenanceType.PartReplacement => "Thay linh kiện",
        MaintenanceType.Inspection => "Kiểm tra",
        _ => type.ToString()
    };

    public static string ToDisplayName(this MaintenanceStatus status) => status switch
    {
        MaintenanceStatus.Pending => "Chờ xử lý",
        MaintenanceStatus.InProgress => "Đang xử lý",
        MaintenanceStatus.Completed => "Hoàn thành",
        MaintenanceStatus.Cancelled => "Đã hủy",
        _ => status.ToString()
    };
}

public class DeviceMaintenance
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public MaintenanceType Type { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Provider { get; set; }
    public decimal? Cost { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;
    public DeviceStatus PreviousDeviceStatus { get; set; } = DeviceStatus.Available;
    public DeviceStatus? ResultDeviceStatus { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
