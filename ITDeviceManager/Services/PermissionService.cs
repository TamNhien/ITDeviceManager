using System.Data;
using ITDeviceManager.Data;
using ITDeviceManager.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ITDeviceManager.Services;

public sealed record RolePermissionRow(int PermissionId, string Code, string Group, string Name, string Description, bool Granted);
public sealed record RoleSummary(int Id, string Name);

public static class PermissionService
{
    private static readonly object Sync = new();
    private static int? _cachedRoleId;
    private static HashSet<string>? _cachedPermissions;

    public static void Reset()
    {
        lock (Sync)
        {
            _cachedRoleId = null;
            _cachedPermissions = null;
        }
    }

    public static bool Has(string permissionCode)
    {
        var user = AppSession.CurrentUser;
        if (user is null)
            return false;

        if (string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        EnsureLoaded(user.RoleId);
        lock (Sync)
            return _cachedPermissions?.Contains(permissionCode) == true;
    }

    public static void Demand(string permissionCode, string? friendlyAction = null)
    {
        if (Has(permissionCode))
            return;

        throw new UnauthorizedAccessException(
            friendlyAction is null
                ? "Tài khoản hiện tại không có quyền thực hiện thao tác này."
                : $"Tài khoản hiện tại không có quyền {friendlyAction}.");
    }

    public static async Task<IReadOnlyList<RoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(AppSettings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM dbo.Roles ORDER BY CASE WHEN Name=N'Admin' THEN 0 ELSE 1 END, Name;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<RoleSummary>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new RoleSummary(reader.GetInt32(0), reader.GetString(1)));
        return result;
    }

    public static async Task<IReadOnlyList<RolePermissionRow>> GetRolePermissionsAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(AppSettings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.Id, p.Code, p.GroupName, p.Name, p.Description,
                   CONVERT(bit, CASE WHEN rp.RoleId IS NULL THEN 0 ELSE 1 END) AS Granted
            FROM dbo.Permissions p
            LEFT JOIN dbo.RolePermissions rp ON rp.PermissionId = p.Id AND rp.RoleId = @roleId
            ORDER BY p.SortOrder, p.GroupName, p.Name;
            """;
        command.Parameters.AddWithValue("@roleId", roleId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<RolePermissionRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new RolePermissionRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                reader.GetBoolean(5)));
        }
        return result;
    }

    public static async Task SaveRolePermissionsAsync(
        int roleId,
        IEnumerable<int> grantedPermissionIds,
        CancellationToken cancellationToken = default)
    {
        Demand(PermissionCodes.PermissionManage, "quản lý phân quyền");
        var ids = grantedPermissionIds.Distinct().ToArray();

        await using var connection = new SqlConnection(AppSettings.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var roleCommand = connection.CreateCommand())
        {
            roleCommand.CommandText = "SELECT Name FROM dbo.Roles WHERE Id=@id;";
            roleCommand.Parameters.AddWithValue("@id", roleId);
            var roleName = Convert.ToString(await roleCommand.ExecuteScalarAsync(cancellationToken));
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Vai trò Admin luôn có toàn bộ quyền và không thể bị giới hạn.");
        }

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var delete = connection.CreateCommand())
            {
                delete.Transaction = (SqlTransaction)transaction;
                delete.CommandText = "DELETE FROM dbo.RolePermissions WHERE RoleId=@roleId;";
                delete.Parameters.AddWithValue("@roleId", roleId);
                await delete.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var permissionId in ids)
            {
                await using var insert = connection.CreateCommand();
                insert.Transaction = (SqlTransaction)transaction;
                insert.CommandText = "INSERT INTO dbo.RolePermissions(RoleId, PermissionId) VALUES(@roleId,@permissionId);";
                insert.Parameters.AddWithValue("@roleId", roleId);
                insert.Parameters.AddWithValue("@permissionId", permissionId);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        Reset();
    }

    public static void ApplyUiPermissions(Form form)
    {
        foreach (var button in EnumerateControls(form).OfType<Button>())
        {
            var permission = ResolveButtonPermission(form, button.Text.Trim());
            if (permission is null)
                continue;

            var allowed = Has(permission);
            button.Enabled = allowed;
            button.Visible = allowed;
        }
    }

    public static void EnsureCanSaveChanges(ChangeTracker changeTracker)
    {
        if (AppSession.CurrentUser is null || AppSession.IsAdmin)
            return;

        var changed = changeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var assignmentWorkflow = changed.Any(e => e.Entity is DeviceAssignment);
        var maintenanceWorkflow = changed.Any(e => e.Entity is DeviceMaintenance);

        foreach (var entry in changed)
        {
            if (entry.Entity is AuditLog or PasswordResetToken)
                continue;

            var required = RequiredPermission(entry, assignmentWorkflow, maintenanceWorkflow);
            if (required is not null)
                Demand(required, "thay đổi dữ liệu này");
        }
    }

    private static string? RequiredPermission(EntityEntry entry, bool assignmentWorkflow, bool maintenanceWorkflow)
    {
        if (entry.State == EntityState.Modified && entry.Entity is ISoftDeletable softEntity)
        {
            var isDeleted = entry.Property(nameof(ISoftDeletable.IsDeleted));
            if (isDeleted.IsModified && isDeleted.OriginalValue is bool oldValue && isDeleted.CurrentValue is bool newValue)
            {
                if (oldValue && !newValue)
                    return PermissionCodes.RecycleBinRestore;
                if (!oldValue && newValue)
                    return SoftDeleteService.ResolveDeletePermission(softEntity);
            }
        }

        return entry.Entity switch
        {
            Device when assignmentWorkflow || maintenanceWorkflow => null,
            Device => ByState(entry.State, PermissionCodes.DeviceCreate, PermissionCodes.DeviceUpdate, PermissionCodes.DeviceDelete),
            DeviceType => ByState(entry.State, PermissionCodes.DeviceTypeCreate, PermissionCodes.DeviceTypeUpdate, PermissionCodes.DeviceTypeDelete),
            Employee => ByState(entry.State, PermissionCodes.EmployeeCreate, PermissionCodes.EmployeeUpdate, PermissionCodes.EmployeeDelete),
            Department => ByState(entry.State, PermissionCodes.DepartmentCreate, PermissionCodes.DepartmentUpdate, PermissionCodes.DepartmentDelete),
            User => ByState(entry.State, PermissionCodes.UserCreate, PermissionCodes.UserUpdate, PermissionCodes.UserDelete),
            DeviceMaintenance => ByState(entry.State, PermissionCodes.MaintenanceCreate, PermissionCodes.MaintenanceUpdate, PermissionCodes.MaintenanceDelete),
            DeviceAssignment => ResolveAssignmentPermission(entry),
            Role => PermissionCodes.PermissionManage,
            _ => null
        };
    }

    private static string ResolveAssignmentPermission(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            return PermissionCodes.AssignmentCreate;
        if (entry.State == EntityState.Deleted)
            return PermissionCodes.AssignmentUpdate;

        var returnedDate = entry.Property(nameof(DeviceAssignment.ReturnedDate));
        if (returnedDate.IsModified && returnedDate.CurrentValue is not null)
            return PermissionCodes.AssignmentReturn;
        return PermissionCodes.AssignmentUpdate;
    }

    private static string? ByState(EntityState state, string create, string update, string delete)
        => state switch
        {
            EntityState.Added => create,
            EntityState.Modified => update,
            EntityState.Deleted => delete,
            _ => null
        };

    private static string? ResolveButtonPermission(Form form, string buttonText)
    {
        var type = form.GetType().Name;
        if (buttonText.Contains("Xuất", StringComparison.OrdinalIgnoreCase))
            return PermissionCodes.ReportExport;

        return type switch
        {
            "DevicesForm" when buttonText.Contains("QR / Barcode", StringComparison.OrdinalIgnoreCase) => PermissionCodes.QrBarcodeGenerate,
            "DevicesForm" when buttonText.Contains("Quét", StringComparison.OrdinalIgnoreCase) => PermissionCodes.QrBarcodeView,
            "DevicesForm" => CrudButton(buttonText, PermissionCodes.DeviceCreate, PermissionCodes.DeviceUpdate, PermissionCodes.DeviceDelete),
            "DeviceTypesForm" => CrudButton(buttonText, PermissionCodes.DeviceTypeCreate, PermissionCodes.DeviceTypeUpdate, PermissionCodes.DeviceTypeDelete),
            "EmployeesForm" => CrudButton(buttonText, PermissionCodes.EmployeeCreate, PermissionCodes.EmployeeUpdate, PermissionCodes.EmployeeDelete),
            "DepartmentsForm" => CrudButton(buttonText, PermissionCodes.DepartmentCreate, PermissionCodes.DepartmentUpdate, PermissionCodes.DepartmentDelete),
            "UsersForm" => CrudButton(buttonText, PermissionCodes.UserCreate, PermissionCodes.UserUpdate, PermissionCodes.UserDelete),
            "AssignmentsForm" when buttonText.Contains("Cấp phát", StringComparison.OrdinalIgnoreCase) => PermissionCodes.AssignmentCreate,
            "AssignmentsForm" when buttonText.Contains("Thu hồi", StringComparison.OrdinalIgnoreCase) => PermissionCodes.AssignmentReturn,
            "MaintenancesForm" when buttonText.Contains("Xóa", StringComparison.OrdinalIgnoreCase) => PermissionCodes.MaintenanceDelete,
            "MaintenancesForm" when buttonText.Contains("Thêm", StringComparison.OrdinalIgnoreCase) || buttonText.Contains("Tạo", StringComparison.OrdinalIgnoreCase) => PermissionCodes.MaintenanceCreate,
            "MaintenancesForm" when buttonText.Contains("Sửa", StringComparison.OrdinalIgnoreCase) || buttonText.Contains("Bắt đầu", StringComparison.OrdinalIgnoreCase) || buttonText.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase) || buttonText.Contains("Hủy", StringComparison.OrdinalIgnoreCase) => PermissionCodes.MaintenanceUpdate,
            "BackupRestoreForm" => PermissionCodes.BackupManage,
            "DeviceCodesForm" when buttonText.Contains("Tạo", StringComparison.OrdinalIgnoreCase) => PermissionCodes.QrBarcodeGenerate,
            "RecycleBinForm" when buttonText.Contains("Khôi phục", StringComparison.OrdinalIgnoreCase) => PermissionCodes.RecycleBinRestore,
            _ => null
        };
    }

    private static string? CrudButton(string text, string create, string update, string delete)
    {
        if (text.Contains("Thêm", StringComparison.OrdinalIgnoreCase)) return create;
        if (text.Contains("Sửa", StringComparison.OrdinalIgnoreCase)) return update;
        if (text.Contains("Xóa", StringComparison.OrdinalIgnoreCase)) return delete;
        return null;
    }

    private static IEnumerable<Control> EnumerateControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var descendant in EnumerateControls(child))
                yield return descendant;
        }
    }

    private static void EnsureLoaded(int roleId)
    {
        lock (Sync)
        {
            if (_cachedRoleId == roleId && _cachedPermissions is not null)
                return;
        }

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var connection = new SqlConnection(AppSettings.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT p.Code
                FROM dbo.RolePermissions rp
                INNER JOIN dbo.Permissions p ON p.Id = rp.PermissionId
                WHERE rp.RoleId = @roleId;
                """;
            command.Parameters.AddWithValue("@roleId", roleId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetString(0));
        }
        catch (SqlException)
        {
            // Fail closed for non-admin users when the permission schema is unavailable.
        }

        lock (Sync)
        {
            _cachedRoleId = roleId;
            _cachedPermissions = result;
        }
    }
}
