namespace ITDeviceManager.Models;

public class Employee : ISoftDeletable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<DeviceAssignment> Assignments { get; set; } = new List<DeviceAssignment>();
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
    public string? DeletedByUsername { get; set; }

}
