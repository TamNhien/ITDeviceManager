using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV134
{
    private static readonly string[] AssignmentNotes =
    [
        "Bàn giao máy tính để bàn phục vụ công việc tại Phòng CNTT.",
        "Bàn giao laptop phục vụ công việc và họp trực tuyến.",
        "Bàn giao máy in dùng chung cho Phòng Kế toán.",
        "Bàn giao màn hình bổ sung cho vị trí làm việc.",
        "Bàn giao switch phục vụ hệ thống mạng nội bộ.",
        "Bàn giao máy chủ phục vụ hệ thống nội bộ; đã thu hồi để bảo trì.",
        "Bàn giao UPS bảo vệ nguồn cho thiết bị hạ tầng; đã thu hồi sau sự cố.",
        "Bàn giao camera giám sát khu vực hành chính; đã thu hồi để thanh lý.",
        "Bàn giao máy chấm công cho Phòng Nhân sự; đã hoàn tất thu hồi.",
        "Bàn giao máy quét mã vạch cho Phòng Kho vận; đã hoàn tất thu hồi."
    ];

    public static async Task UpgradeAsync(AppDbContext db)
    {
        // Rename only the ten seeded device codes from TBMxxx to TBxxx.
        // Assignments use DeviceId foreign keys, so history remains intact.
        for (var i = 1; i <= 10; i++)
        {
            var oldCode = $"TBM{i:D3}";
            var newCode = $"TB{i:D3}";

            var oldDevice = await db.Devices.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Code == oldCode);
            if (oldDevice is not null)
            {
                var newDevice = await db.Devices.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Code == newCode);
                if (newDevice is null)
                {
                    oldDevice.Code = newCode;
                    await db.SaveChangesAsync();
                }
                else if (newDevice.Id != oldDevice.Id)
                {
                    await db.DeviceAssignments
                        .Where(x => x.DeviceId == oldDevice.Id)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DeviceId, newDevice.Id));

                    db.Devices.Remove(oldDevice);
                    await db.SaveChangesAsync();
                }
            }

            var legacyNote = $"Dữ liệu mẫu cấp phát #{i:D2}";
            var replacement = AssignmentNotes[i - 1];
            await db.DeviceAssignments
                .Where(x => x.Note == legacyNote)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Note, replacement));
        }

    }
}
