using System.Security.Cryptography;
using System.Text;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

/// <summary>
/// Idempotent sample-data seeder. It runs automatically at application startup and
/// inserts only missing sample rows, so real user data is never deleted or reset.
/// When a future version adds a new business table, add its sample definition here;
/// Program.cs already invokes this seeder automatically after schema upgrades.
/// </summary>
public static class SampleDataSeeder
{
    private static readonly string[] RoleSamples =
    [
        "Admin",
        "Staff",
        "Quản lý CNTT",
        "Kỹ thuật viên",
        "Quản lý tài sản",
        "Helpdesk",
        "Kiểm toán",
        "Trưởng phòng",
        "Nhân sự xem",
        "Chỉ đọc",
    ];

    private static readonly (string Code, string Name)[] DepartmentSamples =
    [
        ("CNTT", "Phòng CNTT"),
        ("KT", "Phòng Kế toán"),
        ("NS", "Phòng Nhân sự"),
        ("KD", "Phòng Kinh doanh"),
        ("MKT", "Phòng Marketing"),
        ("HC", "Phòng Hành chính"),
        ("CSKH", "Phòng Chăm sóc khách hàng"),
        ("KHO", "Phòng Kho vận"),
        ("RND", "Phòng Nghiên cứu và Phát triển"),
        ("BGD", "Ban Giám đốc"),
    ];

    private static readonly string[] DeviceTypeSamples =
    [
        "Máy tính",
        "Laptop",
        "Máy in",
        "Màn hình",
        "Thiết bị mạng",
        "Máy chủ",
        "UPS",
        "Camera IP",
        "Máy chấm công",
        "Máy quét mã vạch",
    ];

    private static readonly (string LegacyKey, string Username, string FullName, string Email, string Phone)[] UserSamples =
    [
        ("nguyenminhanh", "Nguyễn Minh Anh", "Nguyễn Minh Anh", "nguyen.minh.anh@example.com", "0980000001"),
        ("tranquocbao", "Trần Quốc Bảo", "Trần Quốc Bảo", "tran.quoc.bao@example.com", "0980000002"),
        ("lehoangcuong", "Lê Hoàng Cường", "Lê Hoàng Cường", "le.hoang.cuong@example.com", "0980000003"),
        ("phamthuha", "Phạm Thu Hà", "Phạm Thu Hà", "pham.thu.ha@example.com", "0980000004"),
        ("voduchuy", "Võ Đức Huy", "Võ Đức Huy", "vo.duc.huy@example.com", "0980000005"),
        ("dangngoclan", "Đặng Ngọc Lan", "Đặng Ngọc Lan", "dang.ngoc.lan@example.com", "0980000006"),
        ("buiquangminh", "Bùi Quang Minh", "Bùi Quang Minh", "bui.quang.minh@example.com", "0980000007"),
        ("hothaonguyen", "Hồ Thảo Nguyên", "Hồ Thảo Nguyên", "ho.thao.nguyen@example.com", "0980000008"),
        ("duongthanhphuc", "Dương Thành Phúc", "Dương Thành Phúc", "duong.thanh.phuc@example.com", "0980000009"),
        ("nguyenkhanhvy", "Nguyễn Khánh Vy", "Nguyễn Khánh Vy", "nguyen.khanh.vy@example.com", "0980000010"),
    ];

    private static readonly (string Code, string FullName, string Email, string Phone, string DepartmentCode)[] EmployeeSamples =
    [
        ("NV001", "Nguyễn Minh Anh", "nma@example.com", "0901000001", "CNTT"),
        ("NV002", "Trần Quốc Bảo", "tqb@example.com", "0901000002", "CNTT"),
        ("NV003", "Lê Hoàng Cường", "lhc@example.com", "0901000003", "KT"),
        ("NV004", "Phạm Thu Hà", "pth@example.com", "0901000004", "NS"),
        ("NV005", "Võ Đức Huy", "vdh@example.com", "0901000005", "KD"),
        ("NV006", "Đặng Ngọc Lan", "dnl@example.com", "0901000006", "MKT"),
        ("NV007", "Bùi Quang Minh", "bqm@example.com", "0901000007", "HC"),
        ("NV008", "Hồ Thảo Nguyên", "htn@example.com", "0901000008", "CSKH"),
        ("NV009", "Dương Thành Phúc", "dtp@example.com", "0901000009", "KHO"),
        ("NV010", "Nguyễn Khánh Vy", "nkv@example.com", "0901000010", "RND"),
    ];

    private static readonly (string Code, string Name, string Serial, DateTime PurchaseDate, decimal Price, string TypeName, string DepartmentCode, DeviceStatus InitialStatus)[] DeviceSamples =
    [
        ("TB001", "Dell OptiPlex 7020", "DL7020-260001", new DateTime(2026, 1, 10), 18500000m, "Máy tính", "CNTT", DeviceStatus.Available),
        ("TB002", "Dell Latitude 5450", "DL5450-260002", new DateTime(2026, 1, 18), 24500000m, "Laptop", "CNTT", DeviceStatus.Available),
        ("TB003", "HP LaserJet Pro 4003dn", "HP4003-260003", new DateTime(2026, 2, 5), 8200000m, "Máy in", "KT", DeviceStatus.Available),
        ("TB004", "Dell P2425H", "DP2425-260004", new DateTime(2026, 2, 12), 5200000m, "Màn hình", "NS", DeviceStatus.Available),
        ("TB005", "Cisco CBS350-24T-4G", "CBS350-260005", new DateTime(2026, 3, 1), 13800000m, "Thiết bị mạng", "CNTT", DeviceStatus.Available),
        ("TB006", "HPE ProLiant ML30 Gen11", "HPEML30-260006", new DateTime(2026, 3, 15), 48500000m, "Máy chủ", "CNTT", DeviceStatus.Repair),
        ("TB007", "APC Smart-UPS 1500VA", "APC1500-260007", new DateTime(2026, 4, 2), 11200000m, "UPS", "CNTT", DeviceStatus.Broken),
        ("TB008", "Hikvision Camera IP 4MP", "HK4MP-260008", new DateTime(2026, 4, 18), 3900000m, "Camera IP", "HC", DeviceStatus.Retired),
        ("TB009", "Ronald Jack X628C", "RJX628-260009", new DateTime(2026, 5, 6), 3500000m, "Máy chấm công", "NS", DeviceStatus.Available),
        ("TB010", "Zebra DS2208", "ZDS2208-260010", new DateTime(2026, 5, 20), 4200000m, "Máy quét mã vạch", "KHO", DeviceStatus.Available),
    ];

    private static readonly (string DeviceCode, string EmployeeCode, DateTime AssignedDate, DateTime? ReturnedDate, string Note)[] AssignmentSamples =
    [
        ("TB001", "NV001", new DateTime(2026, 6, 1), null, "Bàn giao máy tính để bàn phục vụ công việc tại Phòng CNTT."),
        ("TB002", "NV002", new DateTime(2026, 6, 6), null, "Bàn giao laptop phục vụ công việc và họp trực tuyến."),
        ("TB003", "NV003", new DateTime(2026, 6, 11), null, "Bàn giao máy in dùng chung cho Phòng Kế toán."),
        ("TB004", "NV004", new DateTime(2026, 6, 16), null, "Bàn giao màn hình bổ sung cho vị trí làm việc."),
        ("TB005", "NV005", new DateTime(2026, 6, 21), null, "Bàn giao switch phục vụ hệ thống mạng nội bộ."),
        ("TB006", "NV006", new DateTime(2026, 6, 26), new DateTime(2026, 7, 26), "Bàn giao máy chủ phục vụ hệ thống nội bộ; đã thu hồi để bảo trì."),
        ("TB007", "NV007", new DateTime(2026, 7, 1), new DateTime(2026, 7, 31), "Bàn giao UPS bảo vệ nguồn cho thiết bị hạ tầng; đã thu hồi sau sự cố."),
        ("TB008", "NV008", new DateTime(2026, 7, 6), new DateTime(2026, 8, 5), "Bàn giao camera giám sát khu vực hành chính; đã thu hồi để thanh lý."),
        ("TB009", "NV009", new DateTime(2026, 7, 11), new DateTime(2026, 8, 10), "Bàn giao máy chấm công cho Phòng Nhân sự; đã hoàn tất thu hồi."),
        ("TB010", "NV010", new DateTime(2026, 7, 16), new DateTime(2026, 8, 15), "Bàn giao máy quét mã vạch cho Phòng Kho vận; đã hoàn tất thu hồi."),
    ];

    private static readonly (string Code, string DeviceCode, MaintenanceType Type, DateTime ReceivedDate, DateTime CompletedDate, string Provider, decimal? Cost, string Issue, string Resolution)[] MaintenanceSamples =
    [
        ("BT001", "TB001", MaintenanceType.Preventive, new DateTime(2026, 3, 10), new DateTime(2026, 3, 10), "Bộ phận CNTT nội bộ", 350000m, "Vệ sinh hệ thống, kiểm tra quạt tản nhiệt và ổ lưu trữ.", "Đã vệ sinh, kiểm tra nhiệt độ và cập nhật firmware; thiết bị hoạt động ổn định."),
        ("BT002", "TB002", MaintenanceType.Warranty, new DateTime(2026, 3, 22), new DateTime(2026, 3, 25), "Trung tâm bảo hành chính hãng", null, "Bàn phím có một số phím phản hồi không ổn định.", "Đã kiểm tra và thay cụm bàn phím theo chính sách bảo hành."),
        ("BT003", "TB003", MaintenanceType.Inspection, new DateTime(2026, 3, 15), new DateTime(2026, 3, 15), "Bộ phận CNTT nội bộ", null, "Bản in xuất hiện vệt mờ sau thời gian sử dụng liên tục.", "Đã vệ sinh đường giấy và căn chỉnh chất lượng in; bản in trở lại bình thường."),
        ("BT004", "TB004", MaintenanceType.Preventive, new DateTime(2026, 3, 28), new DateTime(2026, 3, 28), "Bộ phận CNTT nội bộ", null, "Kiểm tra định kỳ chất lượng hiển thị và cổng kết nối.", "Đã kiểm tra điểm ảnh, cổng HDMI/DisplayPort và nguồn; không phát hiện bất thường."),
        ("BT005", "TB005", MaintenanceType.Inspection, new DateTime(2026, 4, 10), new DateTime(2026, 4, 10), "Bộ phận CNTT nội bộ", null, "Kiểm tra log hệ thống và trạng thái các cổng mạng.", "Đã kiểm tra log, cập nhật cấu hình sao lưu và xác nhận toàn bộ cổng hoạt động ổn định."),
        ("BT006", "TB006", MaintenanceType.Repair, new DateTime(2026, 4, 18), new DateTime(2026, 4, 22), "Đơn vị dịch vụ kỹ thuật", 1250000m, "Máy chủ cảnh báo nhiệt độ và quạt làm mát hoạt động không ổn định.", "Đã thay quạt làm mát, vệ sinh hệ thống và chạy kiểm tra tải; nhiệt độ trở lại mức bình thường."),
        ("BT007", "TB007", MaintenanceType.PartReplacement, new DateTime(2026, 5, 2), new DateTime(2026, 5, 3), "Đơn vị dịch vụ kỹ thuật", 1800000m, "Thời gian lưu điện giảm rõ rệt khi mất nguồn.", "Đã thay bộ ắc quy và kiểm tra tải; thời gian lưu điện đạt yêu cầu vận hành."),
        ("BT008", "TB008", MaintenanceType.Warranty, new DateTime(2026, 5, 5), new DateTime(2026, 5, 8), "Trung tâm bảo hành chính hãng", null, "Camera mất kết nối ngẫu nhiên sau thời gian hoạt động dài.", "Đã kiểm tra nguồn, cập nhật firmware và thay đầu nối mạng; kết nối ổn định sau kiểm thử."),
        ("BT009", "TB009", MaintenanceType.Inspection, new DateTime(2026, 5, 12), new DateTime(2026, 5, 12), "Bộ phận CNTT nội bộ", null, "Kiểm tra độ chính xác đồng bộ thời gian và dữ liệu chấm công.", "Đã đồng bộ thời gian, sao lưu cấu hình và kiểm tra giao tiếp mạng thành công."),
        ("BT010", "TB010", MaintenanceType.Inspection, new DateTime(2026, 5, 25), new DateTime(2026, 5, 25), "Bộ phận CNTT nội bộ", null, "Kiểm tra khả năng đọc mã vạch và chất lượng cáp kết nối.", "Đã kiểm tra nhiều loại mã vạch, vệ sinh mắt đọc và xác nhận cáp kết nối hoạt động tốt."),
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        await EnsureRolesAsync(db);
        await EnsureDepartmentsAsync(db);
        await EnsureDeviceTypesAsync(db);
        await EnsureUsersAsync(db);
        await EnsureEmployeesAsync(db);
        await EnsureDevicesAsync(db);
        await EnsureAssignmentsAsync(db);
        await EnsureMaintenancesAsync(db);
        await EnsurePasswordResetTokensAsync(db);
    }

    private static async Task EnsureRolesAsync(AppDbContext db)
    {
        var existing = await db.Roles.Select(x => x.Name).ToHashSetAsync();
        foreach (var name in RoleSamples)
        {
            if (existing.Add(name))
                db.Roles.Add(new Role { Name = name });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDepartmentsAsync(AppDbContext db)
    {
        var existing = await db.Departments.IgnoreQueryFilters().Select(x => x.Code).ToHashSetAsync();
        foreach (var sample in DepartmentSamples)
        {
            if (existing.Add(sample.Code))
                db.Departments.Add(new Department { Code = sample.Code, Name = sample.Name });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDeviceTypesAsync(AppDbContext db)
    {
        var existing = await db.DeviceTypes.IgnoreQueryFilters().Select(x => x.Name).ToHashSetAsync();
        foreach (var name in DeviceTypeSamples)
        {
            if (existing.Add(name))
                db.DeviceTypes.Add(new DeviceType { Name = name });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureUsersAsync(AppDbContext db)
    {
        var staffRoleId = await db.Roles
            .Where(x => x.Name == "Staff")
            .Select(x => x.Id)
            .SingleAsync();

        var users = await db.Users.IgnoreQueryFilters().ToListAsync();
        var byUsername = users.ToDictionary(x => x.Username, StringComparer.OrdinalIgnoreCase);

        foreach (var sample in UserSamples)
        {
            // Upgrade rows seeded by V1.3.0 so the database no longer shows the old demo-prefixed usernames.
            var legacyUsername = $"demo.{sample.LegacyKey}";
            if (byUsername.TryGetValue(legacyUsername, out var legacy))
            {
                if (!byUsername.ContainsKey(sample.Username))
                {
                    byUsername.Remove(legacyUsername);
                    legacy.Username = sample.Username;
                    legacy.FullName = sample.FullName;
                    legacy.Email = sample.Email;
                    legacy.PhoneNumber = sample.Phone;
                    legacy.RoleId = staffRoleId;
                    legacy.IsActive = false;
                    byUsername[sample.Username] = legacy;
                    continue;
                }

                // If the natural username already exists, remove only the inactive legacy seed row
                // and its expired/used reset-token samples so no legacy sample account remains visible.
                if (!legacy.IsActive && string.Equals(legacy.FullName, sample.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    var legacyTokens = await db.PasswordResetTokens
                        .Where(x => x.UserId == legacy.Id)
                        .ToListAsync();
                    db.PasswordResetTokens.RemoveRange(legacyTokens);
                    db.Users.Remove(legacy);
                    byUsername.Remove(legacyUsername);
                }
            }

            if (byUsername.TryGetValue(sample.Username, out var existingUser))
            {
                existingUser.FullName = sample.FullName;
                existingUser.Email ??= sample.Email;
                existingUser.PhoneNumber ??= sample.Phone;
                continue;
            }

            var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "Aa1!";
            var user = new User
            {
                Username = sample.Username,
                FullName = sample.FullName,
                Email = sample.Email,
                PhoneNumber = sample.Phone,
                PasswordHash = PasswordHasher.HashPassword(randomPassword),
                RoleId = staffRoleId,
                // Sample accounts are intentionally disabled so seeded credentials never create an access path.
                IsActive = false
            };
            db.Users.Add(user);
            byUsername[sample.Username] = user;
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureEmployeesAsync(AppDbContext db)
    {
        var departmentIds = await db.Departments.ToDictionaryAsync(x => x.Code, x => x.Id);
        var existing = await db.Employees.IgnoreQueryFilters().Select(x => x.Code).ToHashSetAsync();

        foreach (var sample in EmployeeSamples)
        {
            if (!existing.Add(sample.Code) || !departmentIds.TryGetValue(sample.DepartmentCode, out var departmentId))
                continue;

            db.Employees.Add(new Employee
            {
                Code = sample.Code,
                FullName = sample.FullName,
                Email = sample.Email,
                Phone = sample.Phone,
                DepartmentId = departmentId
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDevicesAsync(AppDbContext db)
    {
        var typeIds = await db.DeviceTypes.ToDictionaryAsync(x => x.Name, x => x.Id);
        var departmentIds = await db.Departments.ToDictionaryAsync(x => x.Code, x => x.Id);
        var existing = await db.Devices.IgnoreQueryFilters().Select(x => x.Code).ToHashSetAsync();

        foreach (var sample in DeviceSamples)
        {
            if (!existing.Add(sample.Code) ||
                !typeIds.TryGetValue(sample.TypeName, out var typeId) ||
                !departmentIds.TryGetValue(sample.DepartmentCode, out var departmentId))
                continue;

            db.Devices.Add(new Device
            {
                Code = sample.Code,
                Name = sample.Name,
                SerialNumber = sample.Serial,
                PurchaseDate = sample.PurchaseDate,
                PurchasePrice = sample.Price,
                Status = sample.InitialStatus,
                DeviceTypeId = typeId,
                DepartmentId = departmentId
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureAssignmentsAsync(AppDbContext db)
    {
        var deviceCodes = AssignmentSamples.Select(x => x.DeviceCode).ToArray();
        var employeeCodes = AssignmentSamples.Select(x => x.EmployeeCode).ToArray();

        var devices = await db.Devices
            .Where(x => deviceCodes.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, x => x);
        var employees = await db.Employees
            .Where(x => employeeCodes.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, x => x.Id);

        foreach (var sample in AssignmentSamples)
        {
            if (!devices.TryGetValue(sample.DeviceCode, out var device) ||
                !employees.TryGetValue(sample.EmployeeCode, out var employeeId))
                continue;

            var existing = await db.DeviceAssignments
                .SingleOrDefaultAsync(x =>
                    x.DeviceId == device.Id &&
                    x.EmployeeId == employeeId &&
                    x.AssignedDate == sample.AssignedDate);

            if (existing is null)
            {
                db.DeviceAssignments.Add(new DeviceAssignment
                {
                    DeviceId = device.Id,
                    EmployeeId = employeeId,
                    AssignedDate = sample.AssignedDate,
                    ReturnedDate = sample.ReturnedDate,
                    Note = sample.Note
                });

                // Only initialize status when the sample assignment is first created.
                // Never overwrite a status the user changed later (Repair/Broken/Retired).
                if (sample.ReturnedDate is null && device.Status == DeviceStatus.Available)
                    device.Status = DeviceStatus.InUse;
            }
            else if (string.IsNullOrWhiteSpace(existing.Note) ||
                     existing.Note.StartsWith("Dữ liệu mẫu cấp phát #", StringComparison.OrdinalIgnoreCase))
            {
                existing.Note = sample.Note;
            }
        }

        await db.SaveChangesAsync();
    }


    private static async Task EnsureMaintenancesAsync(AppDbContext db)
    {
        var deviceCodes = MaintenanceSamples.Select(x => x.DeviceCode).ToArray();
        var deviceIds = await db.Devices
            .Where(x => deviceCodes.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, x => x.Id);
        var existingCodes = await db.DeviceMaintenances.IgnoreQueryFilters().Select(x => x.Code).ToHashSetAsync();

        foreach (var sample in MaintenanceSamples)
        {
            if (!existingCodes.Add(sample.Code) || !deviceIds.TryGetValue(sample.DeviceCode, out var deviceId))
                continue;

            db.DeviceMaintenances.Add(new DeviceMaintenance
            {
                Code = sample.Code,
                DeviceId = deviceId,
                Type = sample.Type,
                ReceivedDate = sample.ReceivedDate,
                CompletedDate = sample.CompletedDate,
                Provider = sample.Provider,
                Cost = sample.Cost,
                IssueDescription = sample.Issue,
                Resolution = sample.Resolution,
                Status = MaintenanceStatus.Completed,
                PreviousDeviceStatus = DeviceStatus.Available,
                ResultDeviceStatus = DeviceStatus.Available,
                Note = null,
                CreatedAt = sample.ReceivedDate,
                UpdatedAt = sample.CompletedDate
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsurePasswordResetTokensAsync(AppDbContext db)
    {
        var sampleUsernames = UserSamples.Select(x => x.Username).ToArray();
        var userIds = await db.Users
            .Where(x => sampleUsernames.Contains(x.Username))
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .Take(10)
            .ToListAsync();
        if (userIds.Count == 0)
            return;

        var existingHashes = await db.PasswordResetTokens.Select(x => x.TokenHash).ToHashSetAsync();
        var baseTime = new DateTimeOffset(2025, 1, 1, 8, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < 10; i++)
        {
            var tokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes($"ITDM-SAMPLE-RESET-{i + 1:D2}")));

            if (!existingHashes.Add(tokenHash))
                continue;

            var created = baseTime.AddDays(i);
            db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = userIds[i % userIds.Count],
                TokenHash = tokenHash,
                CreatedAtUtc = created,
                ExpiresAtUtc = created.AddMinutes(15),
                UsedAtUtc = created.AddMinutes(10)
            });
        }

        await db.SaveChangesAsync();
    }
}
