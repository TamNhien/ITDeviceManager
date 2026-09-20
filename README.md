# IT Device Manager

**Đề tài:** Xây dựng phần mềm quản lý thiết bị CNTT trong doanh nghiệp bằng C# WinForms và Entity Framework.

## 1. Công nghệ

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10
- SQL Server
- LINQ
- Argon2id cho password hashing
- MailKit cho SMTP

## 2. Thư mục làm việc mặc định

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

SQL Server mặc định:

```text
Server: CANHTHIEN
Database: ITDeviceManagerDb
Authentication: Windows Authentication
```

## 3. Chức năng chính

- Đăng nhập / đăng xuất.
- Phân quyền Admin / Staff.
- Ghi nhớ tên đăng nhập, không lưu mật khẩu.
- Hiện / ẩn mật khẩu ngay trong ô nhập.
- Đăng ký tài khoản Staff.
- Quên mật khẩu qua email.
- Đặt lại mật khẩu bằng token dùng một lần, thời hạn 15 phút.
- Password hashing bằng Argon2id; salt nằm trong chuỗi PHC, không còn cột `PasswordSalt`.
- Dashboard thống kê.
- CRUD thiết bị.
- CRUD loại thiết bị.
- CRUD phòng ban.
- CRUD nhân viên.
- CRUD tài khoản.
- Cấp phát / thu hồi thiết bị.
- Tìm kiếm và lọc dữ liệu.
- Validation dữ liệu bằng WinForms `ErrorProvider` và tầng nghiệp vụ.
- Lưu dữ liệu Unicode tiếng Việt bằng SQL Server `nvarchar`.
- Tự động bổ sung dữ liệu mẫu idempotent khi ứng dụng khởi động.
- Giao diện hiện đại với sidebar tối, dashboard card, DataGridView mới và hiệu ứng hover/nhấn mượt cho các nút.

## 4. Đáp ứng yêu cầu đồ án

| Yêu cầu | Phần đáp ứng |
|---|---|
| CRUD | Thiết bị, loại thiết bị, phòng ban, nhân viên, tài khoản |
| Entity Framework | EF Core + SQL Server |
| WinForms | Toàn bộ giao diện desktop dùng Windows Forms |
| Tìm kiếm / lọc | Thiết bị, nhân viên và các bộ lọc liên quan |
| Login / phân quyền | Admin / Staff |
| Validation | Username, email, điện thoại, mật khẩu, mã/serial, ngày tháng và nghiệp vụ |

## 5. Cấu hình `.env`

File thật đặt tại:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

Ví dụ:

```env
ITDM_CONNECTION_STRING=Server=CANHTHIEN;Database=ITDeviceManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
ITDM_SMTP_HOST=smtp.gmail.com
ITDM_SMTP_PORT=587
ITDM_SMTP_USERNAME=your-email@gmail.com
ITDM_SMTP_PASSWORD=your-gmail-app-password
ITDM_SMTP_FROM_EMAIL=your-email@gmail.com
ITDM_SMTP_FROM_NAME=IT Device Manager
ITDM_SMTP_SSL_ON_CONNECT=false
```

`.gitignore` phải giữ:

```gitignore
.env
.env.*
!.env.example
```

Không commit `.env` thật lên GitHub.

## 6. Build và chạy

### Build

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\clean.bat
.\build.bat
```

Hoặc:

```powershell
dotnet restore .\ITDeviceManager.sln
dotnet build .\ITDeviceManager.sln -c Debug
```

### Chạy

```powershell
.\run.bat
```

Hoặc:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## 7. Icon ứng dụng

Đặt file icon tại:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\ITDeviceManager\Assets\App.ico
```

Nên dùng ICO thật, không đổi phần mở rộng từ PNG sang ICO.

## 8. GitHub và Release

Repository:

```text
TamNhien/ITDeviceManager
```

Đăng nhập GitHub CLI một lần:

```powershell
gh auth login
```

Release:

```powershell
.\release.bat X.Y.Z
```

Ví dụ:

```powershell
.\release.bat 1.4.0
```

Quy trình release hiện tại:

```text
Restore
→ Build Release
→ Git add / commit
→ Push main
→ Publish win-x64
→ Đóng gói ZIP
→ Tạo SHA-256
→ Tạo / push tag
→ Tạo GitHub Release
→ Upload release assets
```

Không còn project `ITDeviceManager.SelfTest`, `test.bat` hoặc `scripts\test.ps1`.

## 9. Quên mật khẩu

Email reset sử dụng token ngẫu nhiên 256-bit, chỉ dùng một lần và hết hạn sau 15 phút.

Luồng reset mới:

```text
Gmail
→ HTTPS bridge trên GitHub Pages
→ itdevicemanager://reset-password?token=...
→ IT Device Manager
→ Form đặt lại mật khẩu
```

Trang bridge nằm trong:

```text
docs\reset-password.html
```

Token được đặt trong URL fragment (`#token=...`) ở trang bridge để không gửi token lên máy chủ GitHub Pages trong HTTP request.

## 10. Dữ liệu mẫu tự động

`SampleDataSeeder` chạy tự động sau khi schema được tạo/nâng cấp. Seeder là **idempotent**: chỉ thêm những dòng mẫu còn thiếu và không xóa dữ liệu thật.

Các bảng hiện tại có bộ 10 dòng mẫu:

- `Roles`: 10 vai trò mẫu; `Admin` và `Staff` vẫn giữ ý nghĩa hiện tại.
- `Departments`: 10 phòng/ban.
- `DeviceTypes`: 10 loại thiết bị.
- `Users`: 10 tài khoản mẫu mang tên tiếng Việt; các tài khoản mẫu bị vô hiệu hóa và mật khẩu được sinh ngẫu nhiên rồi bỏ, nên không tạo lối đăng nhập mặc định.
- `Employees`: 10 nhân viên mẫu tên tiếng Việt.
- `Devices`: 10 thiết bị mẫu thực tế.
- `DeviceAssignments`: 10 lịch sử cấp phát/thu hồi mẫu.
- `PasswordResetTokens`: 10 token mẫu đã dùng/hết hạn, không thể dùng để reset mật khẩu.

Khi phiên bản sau bổ sung bảng nghiệp vụ mới, định nghĩa mẫu được thêm tập trung trong `SampleDataSeeder`; chương trình sẽ tự chèn dữ liệu lúc khởi động, không cần chạy SQL seed bằng tay. Với bảng có quan hệ/constraint đặc thù, không tạo dữ liệu ngẫu nhiên mù để tránh phá khóa ngoại hoặc unique constraint.

## 11. Bảo mật mật khẩu

- Không lưu mật khẩu plaintext.
- Không dùng MD5, SHA-1 hoặc SHA-256 thuần để lưu mật khẩu.
- Mật khẩu được hash bằng Argon2id.
- Chuỗi `$argon2id$...` chứa version, cost parameters, salt ngẫu nhiên và hash; vì vậy V1.3.0 đã loại bỏ cột `Users.PasswordSalt`.
- Migration V1.3.0 chỉ xóa cột salt khi **toàn bộ** tài khoản hiện có đã là Argon2id; nếu còn tài khoản legacy, ứng dụng dừng với thông báo rõ ràng thay vì làm mất khả năng đăng nhập.
- Chính sách mật khẩu hiện tại:
  - 12–128 ký tự.
  - Ít nhất 1 chữ hoa.
  - Ít nhất 1 chữ thường.
  - Ít nhất 1 chữ số.
  - Ít nhất 1 ký tự đặc biệt.
- Form đăng ký hiển thị độ mạnh mật khẩu theo thời gian thực.
- Form đăng ký kiểm tra mật khẩu nhập lại trùng khớp theo thời gian thực.

---

# Lịch sử phiên bản

> Lịch sử được giữ **trong duy nhất file `README.md` này** và sắp xếp **tăng dần theo phiên bản**. Từ các bản sau chỉ cập nhật tiếp vào cuối mục này, không tạo `README_HOTFIX.txt`, `README_V*.md` hoặc file lịch sử phiên bản riêng.

## V1.0.0

- Khởi tạo project .NET 10 WinForms.
- CRUD thiết bị, loại thiết bị, phòng ban, nhân viên và tài khoản.
- Dashboard.
- Cấp phát / thu hồi thiết bị.
- Tìm kiếm / lọc dữ liệu.
- Login / phân quyền Admin và Staff.
- Validation dữ liệu.
- Entity Framework Core + SQL Server.
- Password hashing ban đầu bằng PBKDF2 + salt.

## V1.1.0

- Chuyển password hashing từ PBKDF2 sang Argon2id.
- Tự nâng cấp hash PBKDF2 cũ sau khi đăng nhập thành công.
- Password policy 12–128 ký tự.
- Thêm xác nhận mật khẩu khi tạo / đổi mật khẩu.
- Giới hạn 5 lần đăng nhập sai và khóa tạm trong phiên ứng dụng.
- Không hiển thị exception nội bộ trực tiếp tại form đăng nhập.
- Chuyển SQL Server mặc định sang `CANHTHIEN` + Windows Authentication.
- Cho phép override connection string bằng `ITDM_CONNECTION_STRING`.

## V1.2.0

- Đưa nút hiện / ẩn mật khẩu vào ngay trong ô password.
- Thêm đăng ký tài khoản Staff.
- Thêm ghi nhớ username, không lưu password.
- Thêm quên mật khẩu qua email.
- Thêm reset password bằng custom URI protocol `itdevicemanager://`.
- Thêm `Users.Email`.
- Thêm bảng `PasswordResetTokens`.
- Token reset dùng một lần, hết hạn 15 phút.
- SMTP qua MailKit.
- Thêm `build.bat`, `run.bat`, `clean.bat`.
- Chuẩn bị hỗ trợ `App.ico`.

## V1.2.1

- Fix lỗi WinForms `WFO1000` của custom password input.
- Fix các nullable warning xuất hiện trong log build.
- Thêm tự đọc file `.env` khi chạy bằng F5, `dotnet run` hoặc `run.bat`.
- Thêm `.env.example`.
- Bảo vệ `.env` bằng `.gitignore`.

## V1.2.2

- Fix migration database cũ gây lỗi `Invalid column name 'Email'`.
- Tách các bước tạo `Users.Email`, `IX_Users_Email` và `PasswordResetTokens` thành các SQL command riêng.
- Migration schema chạy idempotent.
- Thêm quy trình GitHub release tự động.
- Tự commit, push, tag, publish win-x64, tạo ZIP và checksum.
- Chặn release nếu `.env` bị Git track.

## V1.2.3

- Fix kiểm tra `.env` khiến PowerShell hiểu trạng thái “không được Git track” thành lỗi.
- Fix cùng lỗi trong release script.
- Thêm `App.ico` Windows thật, multi-resolution.
- Fix lỗi compiler `CS7065: Icon stream is not in the expected format`.

## V1.2.4

- Canh giữa nút Đăng nhập.
- Canh thẳng hàng dòng `Chưa có tài khoản? / Đăng ký tài khoản`.
- Canh giữa tiêu đề đăng nhập.
- Không hiển thị mật khẩu admin mặc định trên giao diện đăng nhập.

## V1.2.5

- Thêm số điện thoại vào form đăng ký và tài khoản.
- Chuẩn hóa và validate số điện thoại.
- Chặn trùng email và số điện thoại.
- Thêm unique filtered index `IX_Users_PhoneNumber`.
- Hiển thị độ mạnh mật khẩu theo thời gian thực.
- Password policy bắt buộc chữ hoa, chữ thường, số và ký tự đặc biệt.
- Kiểm tra mật khẩu nhập lại trùng khớp theo thời gian thực.
- Đánh dấu dữ liệu người dùng nhập là Unicode trong EF Core.
- Nâng các cột nội dung cũ sang `nvarchar` để lưu đúng tiếng Việt có dấu.

## V1.2.6

- Bỏ project `ITDeviceManager.SelfTest`.
- Bỏ `test.bat` và `scripts\test.ps1`.
- Release không còn tự chạy self-test/database test.
- Cho phép username Unicode, gồm chữ tiếng Việt, số, khoảng trắng, `.`, `_`, `-`.
- Fix `ErrorProvider` của username cập nhật đúng khi dữ liệu hợp lệ.
- Bỏ dòng ghi chú phía trên nút Đăng ký và thu gọn form đăng ký.
- Fix release script khi lệnh Git không trả stdout, tránh lỗi gọi `.Trim()` trên `$null`.
- Fix tạo SHA-256 trên môi trường không có `Get-FileHash` bằng `System.Security.Cryptography.SHA256` của .NET.

## V1.2.7

- Fix nút `Đặt lại mật khẩu` trong Gmail không mở được custom URI trực tiếp.
- Email reset chuyển sang link HTTPS trên GitHub Pages.
- Thêm `docs\reset-password.html` làm bridge từ HTTPS sang `itdevicemanager://`.
- Token đặt trong fragment `#token=...` của URL bridge.
- Release script hỗ trợ publish GitHub Pages từ `main:/docs`.

## V1.2.8

- Bổ sung hiển thị **độ mạnh mật khẩu theo thời gian thực** trên form Đặt lại mật khẩu.
- Hiển thị từng điều kiện: tối thiểu 12 ký tự, chữ hoa, chữ thường, số và ký tự đặc biệt.
- Kiểm tra **mật khẩu nhập lại trùng khớp theo thời gian thực**.
- Nút `Đặt lại mật khẩu` chỉ được bật khi token còn hiệu lực, mật khẩu đạt policy và hai ô mật khẩu trùng nhau.
- Giữ validation phía nghiệp vụ trước khi thực hiện reset để tránh bypass kiểm tra trên giao diện.

## V1.2.9

- Sửa form `Đặt lại mật khẩu` dùng layout cố định để khối kiểm tra mật khẩu luôn hiển thị.
- Hiển thị realtime `Độ mạnh mật khẩu`, đủ 5 điều kiện và trạng thái nhập lại trùng khớp.
- Nút `Đặt lại mật khẩu` chỉ bật khi token hợp lệ, mật khẩu đạt policy và hai ô trùng nhau.
- Gói upgrade được đóng ZIP dạng phẳng: giải nén rồi copy trực tiếp vào root project để chắc chắn ghi đè đúng file.

## V1.3.0

- Loại bỏ hoàn toàn `PasswordSalt` khỏi model, Entity Framework và SQL Server.
- `PasswordHasher` chỉ còn Argon2id; salt ngẫu nhiên được lưu bên trong chuỗi PHC `$argon2id$...`.
- Migration `SchemaUpgradeV130` tự kiểm tra tất cả tài khoản đã dùng Argon2id trước khi drop cột `Users.PasswordSalt`.
- Thêm `SampleDataSeeder` chạy tự động, idempotent sau mỗi lần khởi động.
- Bổ sung 10 dòng dữ liệu mẫu có tên tiếng Việt/giá trị thực tế cho từng bảng hiện tại.
- Tài khoản mẫu được vô hiệu hóa và dùng mật khẩu ngẫu nhiên không được lưu plaintext.
- Token reset mẫu đều ở trạng thái đã dùng/hết hạn để không tạo rủi ro bảo mật.
- Bổ sung `database\upgrade_v1.3.0.sql` và cập nhật `database\verify_schema.sql`.

## V1.3.1

- Đổi 10 username dữ liệu mẫu từ dạng `demo.nguyenminhanh` sang tên người Việt tự nhiên như `Nguyễn Minh Anh`, `Trần Quốc Bảo`, `Lê Hoàng Cường`...
- Tự nâng cấp các tài khoản mẫu V1.3.0 đã tồn tại: đổi `demo.*` sang tên mới khi ứng dụng khởi động, không tạo thêm bản ghi trùng.
- Giữ các tài khoản seed ở trạng thái `IsActive = false` và mật khẩu ngẫu nhiên Argon2id để dữ liệu mẫu không tạo đường đăng nhập.
- Đổi serial thiết bị mẫu khỏi tiền tố `DEMO-SN-*` sang serial mô phỏng thực tế hơn.
- Seeder token reset lấy đúng 10 tài khoản mẫu mới, không còn phụ thuộc tiền tố `demo.`.


## V1.3.2

- Chuẩn hóa số điện thoại Việt Nam về dạng nội địa `0xxxxxxxxx`, không hiển thị/lưu tiền tố `+84`.
- Ví dụ `+84 776 905 500` hoặc `84776905500` được tự đổi thành `0776905500`.
- Tự chuyển dữ liệu `Users.PhoneNumber` và `Employees.Phone` cũ từ `+84...` / `84...` sang `0...` khi khởi động.
- Chặn migration nếu việc đổi `+84` sang `0` tạo ra số điện thoại trùng trong bảng `Users`, tránh vi phạm unique index.
- 10 tài khoản mẫu cũng dùng số điện thoại dạng nội địa như `0980000001`, không còn `+84`.
- Form Đăng ký và Quản lý tài khoản vẫn cho phép nhập `+84`, nhưng dữ liệu được chuẩn hóa thành dạng `0xxxxxxxxx` trước khi lưu.

## V1.3.3

- Chuẩn hóa mã nhân viên mẫu từ `NVM001...NVM010` thành `NV001...NV010`.
- Tự nâng cấp 10 mã mẫu cũ `NVMxxx` sang `NVxxx` khi ứng dụng khởi động; lịch sử cấp phát giữ nguyên vì quan hệ dùng `EmployeeId`.
- Form nhân viên chuẩn hóa mã về chữ hoa và yêu cầu dạng `NV` + chữ số, ví dụ `NV001`.
- Dashboard `Cấp phát gần đây` hiển thị ngày theo đúng `dd/MM/yyyy`, luôn đủ 2 chữ số cho ngày và tháng.
- Form cấp phát và form thiết bị dùng `DateTimePicker` với định dạng cố định `dd/MM/yyyy`.

## V1.3.4

- Chuẩn hóa 10 mã thiết bị seed từ `TBM001...TBM010` thành `TB001...TB010`; lịch sử cấp phát giữ nguyên vì quan hệ dùng `DeviceId`.
- Form thiết bị chuẩn hóa mã về chữ hoa và yêu cầu dạng `TB` + chữ số, ví dụ `TB001`.
- Thay toàn bộ ghi chú `Dữ liệu mẫu cấp phát #xx` bằng nội dung nghiệp vụ tự nhiên, phù hợp từng thiết bị/phòng ban.
- Dữ liệu thiết bị mẫu dùng tên sản phẩm thực tế, không còn tiền tố/ghi chú mang chữ `demo` hoặc `dữ liệu mẫu` trên giao diện.
- Dashboard bổ sung hai thẻ thống kê `Hỏng` và `Thanh lý`, tổng cộng 6 trạng thái: Tổng thiết bị, Đang sử dụng, Chưa sử dụng, Đang sửa chữa, Hỏng, Thanh lý.
- Bộ seed mới gán trạng thái thực tế hơn cho thiết bị đã thu hồi để dashboard có dữ liệu minh họa Sửa chữa/Hỏng/Thanh lý; migration không tự ghi đè trạng thái thiết bị hiện có.
- Bổ sung `SchemaUpgradeV134` và `database\upgrade_v1.3.4.sql` để tự nâng dữ liệu cũ khi ứng dụng khởi động.

## V1.3.5

- Tự động thay 10 serial mẫu cũ `DEMO-SN-0001...DEMO-SN-0010` bằng serial thiết bị thực tế mô phỏng theo từng mã `TB001...TB010`.
- Migration chỉ sửa serial bắt đầu bằng `DEMO-SN-`, không ghi đè serial thật do người dùng nhập.
- Hỗ trợ cả database chưa kịp đổi mã `TBMxxx` sang `TBxxx`.
- Bổ sung `SchemaUpgradeV135`, `database\upgrade_v1.3.5.sql` và kiểm tra schema để phát hiện serial `DEMO-SN-*` còn sót.


## V1.3.6

- Fix quy trình GitHub Release làm hỏng tiếng Việt trong `README.md` khi chạy bằng Windows PowerShell 5.1.
- `scripts\release.ps1` không còn dùng `Get-Content` mặc định để đọc file UTF-8; toàn bộ đọc/ghi README và project metadata dùng `System.IO.File` với UTF-8 rõ ràng.
- Thêm kiểm tra trước khi commit để dừng release nếu `README.md` xuất hiện dấu hiệu mojibake/encoding sai.
- Thêm `.gitattributes` để chuẩn hóa line ending; cảnh báo LF/CRLF không còn bị nhầm với lỗi encoding.
- GitHub nhận đúng byte UTF-8 từ working tree; không cần đổi font trên GitHub.

## V1.4.0

- Đại tu giao diện WinForms theo phong cách hiện đại, dùng bảng màu thống nhất cho toàn ứng dụng.
- Thiết kế lại `MainForm` với sidebar tối, khu vực người dùng, trạng thái menu đang chọn và header nội dung.
- Thiết kế lại màn hình đăng nhập dạng card, bố cục cân đối và đồng bộ với icon ứng dụng.
- Nâng cấp Dashboard thành 6 card thống kê hiện đại cho Tổng thiết bị, Đang sử dụng, Chưa sử dụng, Đang sửa chữa, Hỏng và Thanh lý.
- Nâng cấp `DataGridView`: header phẳng, dòng xen kẽ, selection nhẹ, khoảng cách và typography dễ đọc hơn.
- Chuẩn hóa giao diện TextBox, ComboBox, DateTimePicker, NumericUpDown, LinkLabel và PasswordInput.
- Thêm hiệu ứng nút toàn hệ thống: bo góc, hover chuyển màu mượt, trạng thái nhấn và màu riêng cho nút chính/phụ/nguy hiểm/thu hồi/navigation.
- Menu sidebar có hiệu ứng hover và active state; nút Đăng xuất dùng style cảnh báo riêng.
- Bổ sung `AppTheme` và `ModernCard` để các form tiếp theo có thể dùng chung design system mà không lặp style.
- Không thay đổi schema database hoặc dữ liệu nghiệp vụ trong V1.4.0; có thể copy đè trực tiếp lên V1.3.6.

## V1.4.1

- Fix lỗi build .NET 10 WinForms `WFO1000` trên hai property `ModernCard.BorderColor` và `ModernCard.CornerRadius`.
- Đánh dấu hai property runtime bằng `Browsable(false)` và `DesignerSerializationVisibility.Hidden` để WinForms Designer không cố serialize chúng vào mã Designer.
- Giữ nguyên toàn bộ giao diện hiện đại và hiệu ứng nút của V1.4.0; không thay đổi database hoặc dữ liệu nghiệp vụ.

---

## Quy ước từ các phiên bản tiếp theo

- Chỉ duy trì **một file `README.md` duy nhất** ở root project.
- Không tạo thêm `README_HOTFIX.txt`, `README_V*.md`, `THAY_DOI_V*.md` hoặc file lịch sử riêng.
- Mỗi bản mới thêm một mục mới ở **cuối Lịch sử phiên bản**.
- Thứ tự luôn tăng dần: `V1.0.0 → V1.1.0 → V1.2.0 → ...`.
- Hotfix của một phiên bản được gộp vào chính mục phiên bản đó thay vì tạo README riêng.
