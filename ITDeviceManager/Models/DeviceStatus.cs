namespace ITDeviceManager.Models;

public enum DeviceStatus
{
    Available = 1,
    InUse = 2,
    Repair = 3,
    Broken = 4,
    Retired = 5
}

public static class DeviceStatusExtensions
{
    public static string ToDisplayName(this DeviceStatus status) => status switch
    {
        DeviceStatus.Available => "Chưa sử dụng",
        DeviceStatus.InUse => "Đang sử dụng",
        DeviceStatus.Repair => "Đang sửa chữa",
        DeviceStatus.Broken => "Hỏng",
        DeviceStatus.Retired => "Thanh lý",
        _ => status.ToString()
    };
}
