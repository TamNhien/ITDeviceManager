namespace ITDeviceManager.Models;

public class DeviceAssignment
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime AssignedDate { get; set; } = DateTime.Today;
    public DateTime? ReturnedDate { get; set; }
    public string? Note { get; set; }
}
