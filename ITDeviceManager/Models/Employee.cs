namespace ITDeviceManager.Models;

public class Employee
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<DeviceAssignment> Assignments { get; set; } = new List<DeviceAssignment>();
}
