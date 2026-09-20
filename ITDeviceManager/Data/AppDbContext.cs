using ITDeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public class AppDbContext : DbContext
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceAssignment> DeviceAssignments => Set<DeviceAssignment>();
    public DbSet<DeviceMaintenance> DeviceMaintenances => Set<DeviceMaintenance>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

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
}
