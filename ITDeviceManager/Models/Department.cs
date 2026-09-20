namespace ITDeviceManager.Models;

public class Department
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
