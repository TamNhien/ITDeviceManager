namespace ITDeviceManager.Models;

public class Device
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
}
