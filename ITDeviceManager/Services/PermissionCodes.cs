namespace ITDeviceManager.Services;

public sealed record PermissionDefinition(string Code, string Group, string Name, string Description);

public static class PermissionCodes
{
    public const string DashboardView = "Dashboard.View";

    public const string DeviceView = "Device.View";
    public const string DeviceCreate = "Device.Create";
    public const string DeviceUpdate = "Device.Update";
    public const string DeviceDelete = "Device.Delete";

    public const string DeviceTypeView = "DeviceType.View";
    public const string DeviceTypeCreate = "DeviceType.Create";
    public const string DeviceTypeUpdate = "DeviceType.Update";
    public const string DeviceTypeDelete = "DeviceType.Delete";

    public const string EmployeeView = "Employee.View";
    public const string EmployeeCreate = "Employee.Create";
    public const string EmployeeUpdate = "Employee.Update";
    public const string EmployeeDelete = "Employee.Delete";

    public const string DepartmentView = "Department.View";
    public const string DepartmentCreate = "Department.Create";
    public const string DepartmentUpdate = "Department.Update";
    public const string DepartmentDelete = "Department.Delete";

    public const string AssignmentView = "Assignment.View";
    public const string AssignmentCreate = "Assignment.Create";
    public const string AssignmentUpdate = "Assignment.Update";
    public const string AssignmentReturn = "Assignment.Return";

    public const string MaintenanceView = "Maintenance.View";
    public const string MaintenanceCreate = "Maintenance.Create";
    public const string MaintenanceUpdate = "Maintenance.Update";
    public const string MaintenanceDelete = "Maintenance.Delete";

    public const string UserView = "User.View";
    public const string UserCreate = "User.Create";
    public const string UserUpdate = "User.Update";
    public const string UserDelete = "User.Delete";

    public const string AuditView = "Audit.View";
    public const string ReportExport = "Report.Export";
    public const string BackupManage = "Backup.Manage";
    public const string PermissionManage = "Permission.Manage";

    public const string QrBarcodeView = "QrBarcode.View";
    public const string QrBarcodeGenerate = "QrBarcode.Generate";

    public const string RecycleBinView = "RecycleBin.View";
    public const string RecycleBinRestore = "RecycleBin.Restore";

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(DashboardView, "Tổng quan", "Xem tổng quan", "Xem KPI, biểu đồ và cấp phát gần đây."),

        new(DeviceView, "Thiết bị", "Xem thiết bị", "Xem danh sách và chi tiết thiết bị."),
        new(DeviceCreate, "Thiết bị", "Thêm thiết bị", "Tạo thiết bị mới."),
        new(DeviceUpdate, "Thiết bị", "Sửa thiết bị", "Cập nhật thông tin và trạng thái thiết bị."),
        new(DeviceDelete, "Thiết bị", "Xóa thiết bị", "Xóa thiết bị khi không bị ràng buộc nghiệp vụ."),

        new(DeviceTypeView, "Loại thiết bị", "Xem loại thiết bị", "Xem danh mục loại thiết bị."),
        new(DeviceTypeCreate, "Loại thiết bị", "Thêm loại thiết bị", "Tạo loại thiết bị mới."),
        new(DeviceTypeUpdate, "Loại thiết bị", "Sửa loại thiết bị", "Cập nhật loại thiết bị."),
        new(DeviceTypeDelete, "Loại thiết bị", "Xóa loại thiết bị", "Xóa loại thiết bị khi hợp lệ."),

        new(EmployeeView, "Nhân viên", "Xem nhân viên", "Xem danh sách nhân viên."),
        new(EmployeeCreate, "Nhân viên", "Thêm nhân viên", "Tạo hồ sơ nhân viên."),
        new(EmployeeUpdate, "Nhân viên", "Sửa nhân viên", "Cập nhật hồ sơ nhân viên."),
        new(EmployeeDelete, "Nhân viên", "Xóa nhân viên", "Xóa nhân viên khi không còn ràng buộc."),

        new(DepartmentView, "Phòng ban", "Xem phòng ban", "Xem danh mục phòng ban."),
        new(DepartmentCreate, "Phòng ban", "Thêm phòng ban", "Tạo phòng ban mới."),
        new(DepartmentUpdate, "Phòng ban", "Sửa phòng ban", "Cập nhật phòng ban."),
        new(DepartmentDelete, "Phòng ban", "Xóa phòng ban", "Xóa phòng ban khi hợp lệ."),

        new(AssignmentView, "Cấp phát / Thu hồi", "Xem cấp phát", "Xem lịch sử cấp phát và thu hồi."),
        new(AssignmentCreate, "Cấp phát / Thu hồi", "Cấp phát thiết bị", "Cấp thiết bị cho nhân viên."),
        new(AssignmentUpdate, "Cấp phát / Thu hồi", "Sửa cấp phát", "Cập nhật ghi chú/thông tin cấp phát."),
        new(AssignmentReturn, "Cấp phát / Thu hồi", "Thu hồi thiết bị", "Thu hồi thiết bị đã cấp."),

        new(MaintenanceView, "Bảo trì / Sửa chữa", "Xem bảo trì", "Xem phiếu bảo trì, sửa chữa, bảo hành."),
        new(MaintenanceCreate, "Bảo trì / Sửa chữa", "Tạo phiếu", "Tạo phiếu bảo trì/sửa chữa/bảo hành."),
        new(MaintenanceUpdate, "Bảo trì / Sửa chữa", "Xử lý phiếu", "Sửa, bắt đầu, hoàn thành hoặc hủy phiếu."),
        new(MaintenanceDelete, "Bảo trì / Sửa chữa", "Xóa phiếu", "Xóa phiếu bảo trì khi hợp lệ."),

        new(UserView, "Tài khoản", "Xem tài khoản", "Xem danh sách tài khoản."),
        new(UserCreate, "Tài khoản", "Thêm tài khoản", "Tạo tài khoản người dùng."),
        new(UserUpdate, "Tài khoản", "Sửa tài khoản", "Cập nhật thông tin, vai trò và trạng thái tài khoản."),
        new(UserDelete, "Tài khoản", "Xóa tài khoản", "Xóa tài khoản khác khi hợp lệ."),

        new(AuditView, "Hệ thống", "Xem nhật ký hoạt động", "Xem Audit Log và chi tiết thay đổi."),
        new(ReportExport, "Hệ thống", "Xuất Excel / PDF", "Xuất dữ liệu đang lọc ra Excel hoặc PDF."),
        new(BackupManage, "Hệ thống", "Sao lưu / Phục hồi", "Thực hiện backup và restore SQL Server."),
        new(PermissionManage, "Hệ thống", "Quản lý phân quyền", "Cấu hình quyền chi tiết cho từng vai trò."),

        new(QrBarcodeView, "QR / Barcode", "Xem QR / Barcode", "Xem danh sách, preview và thư mục mã của thiết bị."),
        new(QrBarcodeGenerate, "QR / Barcode", "Tạo QR / Barcode", "Tạo hoặc tạo lại file QR và Code 128 cho thiết bị."),

        new(RecycleBinView, "Thùng rác", "Xem Thùng rác", "Xem dữ liệu đã xóa mềm và thông tin người xóa."),
        new(RecycleBinRestore, "Thùng rác", "Khôi phục dữ liệu", "Khôi phục dữ liệu đã xóa mềm về màn hình nghiệp vụ.")
    ];
}
