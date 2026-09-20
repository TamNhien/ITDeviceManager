using System.Security.Cryptography;
using System.Text;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

/// <summary>
/// Idempotent demo-data seeder. It runs automatically at application startup and
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

    private static readonly (string Username, string FullName, string Email, string Phone)[] UserSamples =
    [
        ("demo.nguyenminhanh", "Nguyễn Minh Anh", "nguyen.minh.anh@example.com", "+84980000001"),
        ("demo.tranquocbao", "Trần Quốc Bảo", "tran.quoc.bao@example.com", "+84980000002"),
        ("demo.lehoangcuong", "Lê Hoàng Cường", "le.hoang.cuong@example.com", "+84980000003"),
        ("demo.phamthuha", "Phạm Thu Hà", "pham.thu.ha@example.com", "+84980000004"),
        ("demo.voduchuy", "Võ Đức Huy", "vo.duc.huy@example.com", "+84980000005"),
        ("demo.dangngoclan", "Đặng Ngọc Lan", "dang.ngoc.lan@example.com", "+84980000006"),
        ("demo.buiquangminh", "Bùi Quang Minh", "bui.quang.minh@example.com", "+84980000007"),
        ("demo.hothaonguyen", "Hồ Thảo Nguyên", "ho.thao.nguyen@example.com", "+84980000008"),
        ("demo.duongthanhphuc", "Dương Thành Phúc", "duong.thanh.phuc@example.com", "+84980000009"),
        ("demo.nguyenkhanhvy", "Nguyễn Khánh Vy", "nguyen.khanh.vy@example.com", "+84980000010"),
    ];

    private static readonly (string Code, string FullName, string Email, string Phone, string DepartmentCode)[] EmployeeSamples =
    [
        ("NVM001", "Nguyễn Minh Anh", "nma@example.com", "0901000001", "CNTT"),
        ("NVM002", "Trần Quốc Bảo", "tqb@example.com", "0901000002", "CNTT"),
        ("NVM003", "Lê Hoàng Cường", "lhc@example.com", "0901000003", "KT"),
        ("NVM004", "Phạm Thu Hà", "pth@example.com", "0901000004", "NS"),
        ("NVM005", "Võ Đức Huy", "vdh@example.com", "0901000005", "KD"),
        ("NVM006", "Đặng Ngọc Lan", "dnl@example.com", "0901000006", "MKT"),
        ("NVM007", "Bùi Quang Minh", "bqm@example.com", "0901000007", "HC"),
        ("NVM008", "Hồ Thảo Nguyên", "htn@example.com", "0901000008", "CSKH"),
        ("NVM009", "Dương Thành Phúc", "dtp@example.com", "0901000009", "KHO"),
        ("NVM010", "Nguyễn Khánh Vy", "nkv@example.com", "0901000010", "RND"),
    ];

    private static readonly (string Code, string Name, string Serial, DateTime PurchaseDate, decimal Price, string TypeName, string DepartmentCode)[] DeviceSamples =
    [
        ("TBM001", "Dell OptiPlex 7020", "DEMO-SN-0001", new DateTime(2026, 1, 10), 18500000m, "Máy tính", "CNTT"),
        ("TBM002", "Dell Latitude 5450", "DEMO-SN-0002", new DateTime(2026, 1, 18), 24500000m, "Laptop", "CNTT"),
        ("TBM003", "HP LaserJet Pro 4003dn", "DEMO-SN-0003", new DateTime(2026, 2, 5), 8200000m, "Máy in", "KT"),
        ("TBM004", "Dell P2425H", "DEMO-SN-0004", new DateTime(2026, 2, 12), 5200000m, "Màn hình", "NS"),
        ("TBM005", "Cisco CBS350-24T-4G", "DEMO-SN-0005", new DateTime(2026, 3, 1), 13800000m, "Thiết bị mạng", "CNTT"),
        ("TBM006", "HPE ProLiant ML30 Gen11", "DEMO-SN-0006", new DateTime(2026, 3, 15), 48500000m, "Máy chủ", "CNTT"),
        ("TBM007", "APC Smart-UPS 1500VA", "DEMO-SN-0007", new DateTime(2026, 4, 2), 11200000m, "UPS", "CNTT"),
        ("TBM008", "Hikvision Camera IP 4MP", "DEMO-SN-0008", new DateTime(2026, 4, 18), 3900000m, "Camera IP", "HC"),
        ("TBM009", "Ronald Jack X628C", "DEMO-SN-0009", new DateTime(2026, 5, 6), 3500000m, "Máy chấm công", "NS"),
        ("TBM010", "Zebra DS2208", "DEMO-SN-0010", new DateTime(2026, 5, 20), 4200000m, "Máy quét mã vạch", "KHO"),
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
        var existing = await db.Departments.Select(x => x.Code).ToHashSetAsync();
        foreach (var sample in DepartmentSamples)
        {
            if (existing.Add(sample.Code))
                db.Departments.Add(new Department { Code = sample.Code, Name = sample.Name });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDeviceTypesAsync(AppDbContext db)
    {
        var existing = await db.DeviceTypes.Select(x => x.Name).ToHashSetAsync();
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

        var existing = await db.Users.Select(x => x.Username).ToHashSetAsync();
        foreach (var sample in UserSamples)
        {
            if (!existing.Add(sample.Username))
                continue;

            var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "Aa1!";
            db.Users.Add(new User
            {
                Username = sample.Username,
                FullName = sample.FullName,
                Email = sample.Email,
                PhoneNumber = sample.Phone,
                PasswordHash = PasswordHasher.HashPassword(randomPassword),
                RoleId = staffRoleId,
                // Demo accounts are intentionally disabled so sample credentials never create an access path.
                IsActive = false
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureEmployeesAsync(AppDbContext db)
    {
        var departmentIds = await db.Departments.ToDictionaryAsync(x => x.Code, x => x.Id);
        var existing = await db.Employees.Select(x => x.Code).ToHashSetAsync();

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
        var existing = await db.Devices.Select(x => x.Code).ToHashSetAsync();

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
                Status = DeviceStatus.Available,
                DeviceTypeId = typeId,
                DepartmentId = departmentId
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureAssignmentsAsync(AppDbContext db)
    {
        var devices = await db.Devices
            .Where(x => x.Code.StartsWith("TBM"))
            .ToDictionaryAsync(x => x.Code, x => x);
        var employees = await db.Employees
            .Where(x => x.Code.StartsWith("NVM"))
            .ToDictionaryAsync(x => x.Code, x => x.Id);
        var existingNotes = await db.DeviceAssignments
            .Where(x => x.Note != null && x.Note.StartsWith("Dữ liệu mẫu cấp phát #"))
            .Select(x => x.Note!)
            .ToHashSetAsync();

        for (var i = 0; i < 10; i++)
        {
            var note = $"Dữ liệu mẫu cấp phát #{i + 1:D2}";
            var deviceCode = $"TBM{i + 1:D3}";
            var employeeCode = $"NVM{i + 1:D3}";

            if (existingNotes.Contains(note) ||
                !devices.TryGetValue(deviceCode, out var device) ||
                !employees.TryGetValue(employeeCode, out var employeeId))
                continue;

            var assignedDate = new DateTime(2026, 6, 1).AddDays(i * 5);
            DateTime? returnedDate = i >= 5 ? assignedDate.AddDays(30) : null;

            db.DeviceAssignments.Add(new DeviceAssignment
            {
                DeviceId = device.Id,
                EmployeeId = employeeId,
                AssignedDate = assignedDate,
                ReturnedDate = returnedDate,
                Note = note
            });

            device.Status = returnedDate is null ? DeviceStatus.InUse : DeviceStatus.Available;
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsurePasswordResetTokensAsync(AppDbContext db)
    {
        var userIds = await db.Users
            .Where(x => x.Username.StartsWith("demo."))
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
