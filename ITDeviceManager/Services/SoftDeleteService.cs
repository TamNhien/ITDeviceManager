using ITDeviceManager.Models;

namespace ITDeviceManager.Services;

public static class SoftDeleteService
{
    public static void MarkDeleted(ISoftDeletable entity)
    {
        var permission = ResolveDeletePermission(entity);
        if (permission is not null)
            PermissionService.Demand(permission, "xóa dữ liệu này");

        if (entity.IsDeleted)
            return;

        var actor = AppSession.CurrentUser;
        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTimeOffset.UtcNow;
        entity.DeletedByUserId = actor?.Id;
        entity.DeletedByUsername = actor?.Username;
    }

    public static void Restore(ISoftDeletable entity)
    {
        PermissionService.Demand(PermissionCodes.RecycleBinRestore, "khôi phục dữ liệu từ Thùng rác");

        if (!entity.IsDeleted)
            return;

        entity.IsDeleted = false;
        entity.DeletedAtUtc = null;
        entity.DeletedByUserId = null;
        entity.DeletedByUsername = null;
    }

    public static string? ResolveDeletePermission(ISoftDeletable entity) => entity switch
    {
        Device => PermissionCodes.DeviceDelete,
        Employee => PermissionCodes.EmployeeDelete,
        DeviceType => PermissionCodes.DeviceTypeDelete,
        Department => PermissionCodes.DepartmentDelete,
        User => PermissionCodes.UserDelete,
        DeviceMaintenance => PermissionCodes.MaintenanceDelete,
        _ => null
    };
}
