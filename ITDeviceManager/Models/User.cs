namespace ITDeviceManager.Models;

public class User : ISoftDeletable
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
    public string? DeletedByUsername { get; set; }

}
