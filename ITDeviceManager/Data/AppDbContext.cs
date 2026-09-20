using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using ITDeviceManager.Models;
using ITDeviceManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ITDeviceManager.Data;

public class AppDbContext : DbContext
{
    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceAssignment> DeviceAssignments => Set<DeviceAssignment>();
    public DbSet<DeviceMaintenance> DeviceMaintenances => Set<DeviceMaintenance>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(AppSettings.ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQL Server + EF Core use nvarchar for Unicode strings by convention.
        // Keep IsUnicode(true) explicit for user-visible Vietnamese text so future
        // refactors cannot accidentally switch these columns to varchar.
        modelBuilder.Entity<Role>().Property(x => x.Name).IsUnicode(true);
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();

        modelBuilder.Entity<User>().Property(x => x.Username).HasMaxLength(100).IsUnicode(true);
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(320).IsUnicode(true);
        modelBuilder.Entity<User>().Property(x => x.PhoneNumber).HasMaxLength(32).IsUnicode(true);
        modelBuilder.Entity<User>().Property(x => x.FullName).HasMaxLength(200).IsUnicode(true);
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL AND [Email] <> N''");
        modelBuilder.Entity<User>()
            .HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL AND [PhoneNumber] <> N''");

        modelBuilder.Entity<Department>().Property(x => x.Code).IsUnicode(true);
        modelBuilder.Entity<Department>().Property(x => x.Name).IsUnicode(true);
        modelBuilder.Entity<Department>().HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<Employee>().Property(x => x.Code).IsUnicode(true);
        modelBuilder.Entity<Employee>().Property(x => x.FullName).IsUnicode(true);
        modelBuilder.Entity<Employee>().Property(x => x.Email).IsUnicode(true);
        modelBuilder.Entity<Employee>().Property(x => x.Phone).IsUnicode(true);
        modelBuilder.Entity<Employee>().HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<DeviceType>().Property(x => x.Name).IsUnicode(true);

        modelBuilder.Entity<Device>().Property(x => x.Code).IsUnicode(true);
        modelBuilder.Entity<Device>().Property(x => x.Name).IsUnicode(true);
        modelBuilder.Entity<Device>().Property(x => x.SerialNumber).IsUnicode(true);
        modelBuilder.Entity<Device>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Device>()
            .HasIndex(x => x.SerialNumber)
            .IsUnique()
            .HasFilter("[SerialNumber] IS NOT NULL");
        modelBuilder.Entity<Device>()
            .Property(x => x.PurchasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DeviceAssignment>().Property(x => x.Note).IsUnicode(true);

        modelBuilder.Entity<DeviceMaintenance>().Property(x => x.Code).HasMaxLength(20).IsUnicode(true);
        modelBuilder.Entity<DeviceMaintenance>().Property(x => x.Provider).HasMaxLength(200).IsUnicode(true);
        modelBuilder.Entity<DeviceMaintenance>().Property(x => x.IssueDescription).HasMaxLength(1000).IsUnicode(true);
        modelBuilder.Entity<DeviceMaintenance>().Property(x => x.Resolution).HasMaxLength(1000).IsUnicode(true);
        modelBuilder.Entity<DeviceMaintenance>().Property(x => x.Note).HasMaxLength(1000).IsUnicode(true);
        modelBuilder.Entity<DeviceMaintenance>().Property(x => x.Cost).HasPrecision(18, 2);
        modelBuilder.Entity<DeviceMaintenance>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<DeviceMaintenance>().HasIndex(x => new { x.DeviceId, x.ReceivedDate });
        modelBuilder.Entity<DeviceMaintenance>().HasIndex(x => x.Status);

        modelBuilder.Entity<PasswordResetToken>().Property(x => x.TokenHash).HasMaxLength(64).IsUnicode(false);
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

        modelBuilder.Entity<AuditLog>().Property(x => x.Username).HasMaxLength(100).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.Action).HasMaxLength(80).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.EntityName).HasMaxLength(100).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.EntityKey).HasMaxLength(200).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.Description).HasMaxLength(1000).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.OldValuesJson).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.NewValuesJson).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.ComputerName).HasMaxLength(100).IsUnicode(true);
        modelBuilder.Entity<AuditLog>().Property(x => x.AppVersion).HasMaxLength(32).IsUnicode(false);
        modelBuilder.Entity<AuditLog>().HasIndex(x => x.OccurredAtUtc);
        modelBuilder.Entity<AuditLog>().HasIndex(x => new { x.UserId, x.OccurredAtUtc });
        modelBuilder.Entity<AuditLog>().HasIndex(x => x.Username);
        modelBuilder.Entity<AuditLog>().HasIndex(x => x.Action);
        modelBuilder.Entity<AuditLog>().HasIndex(x => x.EntityName);

        modelBuilder.Entity<DeviceAssignment>()
            .HasOne(x => x.Device)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeviceAssignment>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeviceMaintenance>()
            .HasOne(x => x.Device)
            .WithMany(x => x.Maintenances)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(x => x.User)
            .WithMany(x => x.PasswordResetTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditLogs deliberately do not have a foreign key to Users. The username/user id
        // are a historical snapshot, so deleting a user must never delete or invalidate logs.

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Staff" });

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Code = "CNTT", Name = "Phòng CNTT" },
            new Department { Id = 2, Code = "KT", Name = "Phòng Kế toán" },
            new Department { Id = 3, Code = "NS", Name = "Phòng Nhân sự" });

        modelBuilder.Entity<DeviceType>().HasData(
            new DeviceType { Id = 1, Name = "Máy tính" },
            new DeviceType { Id = 2, Name = "Laptop" },
            new DeviceType { Id = 3, Name = "Máy in" },
            new DeviceType { Id = 4, Name = "Màn hình" },
            new DeviceType { Id = 5, Name = "Thiết bị mạng" });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AddAuditEntriesToChangeTracker();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AddAuditEntriesToChangeTracker();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AddAuditEntriesToChangeTracker()
    {
        var actor = AppSession.CurrentUser;
        if (actor is null)
            return;

        ChangeTracker.DetectChanges();

        var entries = ChangeTracker.Entries()
            .Where(x => x.Entity is not AuditLog && x.Entity is not PasswordResetToken)
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return;

        var logs = new List<AuditLog>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified && !entry.Properties.Any(x => x.IsModified))
                continue;

            var action = ResolveAction(entry);
            var entityName = ResolveEntityName(entry.Entity);
            var oldValues = entry.State == EntityState.Added
                ? null
                : Snapshot(entry, useOriginalValues: true, onlyModified: entry.State == EntityState.Modified);
            var newValues = entry.State == EntityState.Deleted
                ? null
                : Snapshot(entry, useOriginalValues: false, onlyModified: entry.State == EntityState.Modified);

            logs.Add(new AuditLog
            {
                OccurredAtUtc = DateTimeOffset.UtcNow,
                UserId = actor.Id,
                Username = actor.Username,
                Action = action,
                EntityName = entityName,
                EntityKey = ResolveEntityKey(entry.Entity),
                Description = BuildDescription(action, entityName, entry.Entity),
                OldValuesJson = SerializeSnapshot(oldValues),
                NewValuesJson = SerializeSnapshot(newValues),
                ComputerName = Environment.MachineName,
                AppVersion = AuditService.GetAppVersion()
            });
        }

        if (logs.Count > 0)
            AuditLogs.AddRange(logs);
    }

    private static string ResolveAction(EntityEntry entry)
    {
        if (entry.Entity is DeviceAssignment)
        {
            if (entry.State == EntityState.Added)
                return "Cấp phát";
            if (entry.State == EntityState.Deleted)
                return "Xóa cấp phát";

            var returnedDate = entry.Property(nameof(DeviceAssignment.ReturnedDate));
            if (returnedDate.IsModified && returnedDate.OriginalValue is null && returnedDate.CurrentValue is not null)
                return "Thu hồi";

            return "Cập nhật cấp phát";
        }

        if (entry.Entity is DeviceMaintenance)
        {
            if (entry.State == EntityState.Added)
                return "Tạo phiếu";
            if (entry.State == EntityState.Deleted)
                return "Xóa phiếu";

            var status = entry.Property(nameof(DeviceMaintenance.Status));
            if (status.IsModified && status.CurrentValue is MaintenanceStatus currentStatus)
            {
                return currentStatus switch
                {
                    MaintenanceStatus.InProgress => "Bắt đầu xử lý",
                    MaintenanceStatus.Completed => "Hoàn thành",
                    MaintenanceStatus.Cancelled => "Hủy phiếu",
                    _ => "Cập nhật phiếu"
                };
            }

            return "Cập nhật phiếu";
        }

        if (entry.Entity is User && entry.State == EntityState.Modified)
        {
            var modified = entry.Properties.Where(x => x.IsModified).Select(x => x.Metadata.Name).ToList();
            if (modified.Count > 0 && modified.All(x => x == nameof(User.PasswordHash)))
                return "Đổi mật khẩu";
        }

        return entry.State switch
        {
            EntityState.Added => "Thêm",
            EntityState.Modified => "Cập nhật",
            EntityState.Deleted => "Xóa",
            _ => "Thay đổi"
        };
    }

    private static string ResolveEntityName(object entity) => entity switch
    {
        User => "Tài khoản",
        Department => "Phòng ban",
        Employee => "Nhân viên",
        DeviceType => "Loại thiết bị",
        Device => "Thiết bị",
        DeviceAssignment => "Cấp phát / Thu hồi",
        DeviceMaintenance => "Bảo trì / Sửa chữa",
        Role => "Vai trò",
        _ => entity.GetType().Name
    };

    private static string? ResolveEntityKey(object entity) => entity switch
    {
        User x => x.Username,
        Department x => x.Code,
        Employee x => x.Code,
        DeviceType x => x.Name,
        Device x => x.Code,
        DeviceAssignment x when x.Id > 0 => x.Id.ToString(CultureInfo.InvariantCulture),
        DeviceAssignment x => $"Device:{x.DeviceId}/Employee:{x.EmployeeId}",
        DeviceMaintenance x => x.Code,
        Role x => x.Name,
        _ => null
    };

    private static string BuildDescription(string action, string entityName, object entity)
    {
        var target = entity switch
        {
            User x => $"{x.Username} - {x.FullName}",
            Department x => $"{x.Code} - {x.Name}",
            Employee x => $"{x.Code} - {x.FullName}",
            DeviceType x => x.Name,
            Device x => $"{x.Code} - {x.Name}",
            DeviceAssignment x => $"thiết bị ID {x.DeviceId} / nhân viên ID {x.EmployeeId}",
            DeviceMaintenance x => $"{x.Code} / thiết bị ID {x.DeviceId}",
            Role x => x.Name,
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(target)
            ? $"{action}: {entityName}."
            : $"{action}: {entityName} {target}.";
    }

    private static Dictionary<string, string?> Snapshot(
        EntityEntry entry,
        bool useOriginalValues,
        bool onlyModified)
    {
        var values = new Dictionary<string, string?>();
        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            if (string.Equals(name, "Id", StringComparison.Ordinal) || IsSensitive(name))
                continue;
            if (onlyModified && !property.IsModified)
                continue;

            var value = useOriginalValues ? property.OriginalValue : property.CurrentValue;
            values[PropertyDisplayName(name)] = FormatAuditValue(value);
        }

        return values;
    }

    private static bool IsSensitive(string propertyName)
        => propertyName is nameof(User.PasswordHash) or nameof(PasswordResetToken.TokenHash);

    private static string? SerializeSnapshot(Dictionary<string, string?>? values)
        => values is null || values.Count == 0
            ? null
            : JsonSerializer.Serialize(values, AuditJsonOptions);

    private static string? FormatAuditValue(object? value) => value switch
    {
        null => null,
        bool x => x ? "Có" : "Không",
        DateTime x => x.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
        DateTimeOffset x => x.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture),
        decimal x => x.ToString("0.##", CultureInfo.InvariantCulture),
        DeviceStatus x => x.ToDisplayName(),
        MaintenanceType x => x.ToDisplayName(),
        MaintenanceStatus x => x.ToDisplayName(),
        Enum x => x.ToString(),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };

    private static string PropertyDisplayName(string name) => name switch
    {
        nameof(User.Username) => "Tên đăng nhập",
        nameof(User.FullName) => "Họ tên",
        nameof(User.Email) => "Email",
        nameof(User.PhoneNumber) => "Số điện thoại",
        nameof(User.RoleId) => "Vai trò (ID)",
        nameof(User.IsActive) => "Hoạt động",
        nameof(Department.Code) => "Mã",
        nameof(Department.Name) => "Tên",
        nameof(Employee.Phone) => "Số điện thoại",
        nameof(Employee.DepartmentId) => "Phòng ban (ID)",
        nameof(Device.SerialNumber) => "Serial",
        nameof(Device.PurchaseDate) => "Ngày mua",
        nameof(Device.PurchasePrice) => "Giá mua",
        nameof(Device.Status) => "Trạng thái",
        nameof(Device.DeviceTypeId) => "Loại thiết bị (ID)",
        nameof(DeviceAssignment.DeviceId) => "Thiết bị (ID)",
        nameof(DeviceAssignment.EmployeeId) => "Nhân viên (ID)",
        nameof(DeviceAssignment.AssignedDate) => "Ngày cấp",
        nameof(DeviceAssignment.ReturnedDate) => "Ngày trả",
        nameof(DeviceAssignment.Note) => "Ghi chú",
        nameof(DeviceMaintenance.Type) => "Loại xử lý",
        nameof(DeviceMaintenance.ReceivedDate) => "Ngày tiếp nhận",
        nameof(DeviceMaintenance.CompletedDate) => "Ngày hoàn thành",
        nameof(DeviceMaintenance.Provider) => "Đơn vị xử lý",
        nameof(DeviceMaintenance.Cost) => "Chi phí",
        nameof(DeviceMaintenance.IssueDescription) => "Mô tả lỗi / yêu cầu",
        nameof(DeviceMaintenance.Resolution) => "Nội dung xử lý",
        nameof(DeviceMaintenance.PreviousDeviceStatus) => "Trạng thái trước",
        nameof(DeviceMaintenance.ResultDeviceStatus) => "Trạng thái kết quả",
        nameof(DeviceMaintenance.CreatedAt) => "Ngày tạo",
        nameof(DeviceMaintenance.UpdatedAt) => "Ngày cập nhật",
        _ => name
    };
}
