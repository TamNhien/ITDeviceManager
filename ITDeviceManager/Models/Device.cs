namespace ITDeviceManager.Models;

public class Device : ISoftDeletable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Available;
    public int DeviceTypeId { get; set; }
    public DeviceType DeviceType { get; set; } = null!;
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<DeviceAssignment> Assignments { get; set; } = new List<DeviceAssignment>();
    public ICollection<DeviceMaintenance> Maintenances { get; set; } = new List<DeviceMaintenance>();
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
    public string? DeletedByUsername { get; set; }

}
