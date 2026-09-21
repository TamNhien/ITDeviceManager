namespace ITDeviceManager.Models;

public class DeviceType : ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
    public string? DeletedByUsername { get; set; }

}
