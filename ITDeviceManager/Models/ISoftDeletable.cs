namespace ITDeviceManager.Models;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
    int? DeletedByUserId { get; set; }
    string? DeletedByUsername { get; set; }
}
