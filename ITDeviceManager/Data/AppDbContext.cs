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
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(AppSettings.ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Username).HasMaxLength(100);
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(320);
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL AND [Email] <> N''");
        modelBuilder.Entity<Department>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Device>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<PasswordResetToken>().Property(x => x.TokenHash).HasMaxLength(64);
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

        modelBuilder.Entity<Device>()
            .HasIndex(x => x.SerialNumber)
            .IsUnique()
            .HasFilter("[SerialNumber] IS NOT NULL");

        modelBuilder.Entity<Device>()
            .Property(x => x.PurchasePrice)
            .HasPrecision(18, 2);

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
