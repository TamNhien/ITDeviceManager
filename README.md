# IT Device Manager - V1.2.1

**Đề tài:** Xây dựng phần mềm quản lý thiết bị CNTT trong doanh nghiệp bằng C# WinForms và Entity Framework.

## Thư mục làm việc mặc định

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

V1.2.1 là bản **hotfix / upgrade-in-place**. Có thể copy đè lên V1.2.0 và giữ nguyên database `ITDeviceManagerDb`.

## V1.2.1 - Build fix + hỗ trợ .env

### Đã sửa lỗi build .NET 10 WinForms

Sửa 2 lỗi:

```text
WFO1000: Property 'Password' does not configure the code serialization...
WFO1000: Property 'PasswordVisible' does not configure the code serialization...
```

`PasswordInput` hiện đánh dấu các property runtime bằng `DesignerSerializationVisibility.Hidden`, vì vậy WinForms Designer không cố serialize password/property trạng thái vào mã Designer.

### Đã xử lý các cảnh báo nullable trong log V1.2.0

- Truy cập cột `DataGridView.Columns["Id"]` qua biến cục bộ sau khi null-check.
- Kiểm tra `SelectedValue` của ComboBox trước khi lưu `RoleId`, `DeviceTypeId`, `Status`, `DepartmentId`.
- Không unbox trực tiếp giá trị có thể null.

### Đọc file `.env` tự động

Không cần khai báo `$env:...` thủ công mỗi lần mở PowerShell nữa.

Đặt file ở đúng đường dẫn:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

Ứng dụng tự tìm `.env` khi chạy bằng:

- `run.bat`
- `dotnet run`
- Visual Studio / F5
- EXE trong `bin\Debug\net10.0-windows`

Ứng dụng tìm `.env` từ thư mục hiện tại và thư mục EXE rồi đi ngược lên các thư mục cha.

> Nếu cùng một key đã tồn tại trong Windows Environment Variables, giá trị Windows Environment Variable sẽ được ưu tiên hơn `.env`.

### Mẫu `.env`

```env
ITDM_SMTP_HOST=smtp.gmail.com
ITDM_SMTP_PORT=587
ITDM_SMTP_USERNAME=your-email@gmail.com
ITDM_SMTP_PASSWORD=your-gmail-app-password
ITDM_SMTP_FROM_EMAIL=your-email@gmail.com
ITDM_SMTP_FROM_NAME=IT Device Manager
ITDM_SMTP_SSL_ON_CONNECT=false
```

Có thể cấu hình SQL Server trong `.env` nếu muốn ghi đè cấu hình mặc định:

```env
ITDM_CONNECTION_STRING=Server=CANHTHIEN;Database=ITDeviceManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

Nếu không khai báo `ITDM_CONNECTION_STRING`, chương trình vẫn mặc định dùng:

```text
Server: CANHTHIEN
Database: ITDeviceManagerDb
Authentication: Windows Authentication
```

### Bảo vệ file bí mật

`.gitignore` đã thêm:

```gitignore
.env
.env.*
!.env.example
```

Vì vậy **không commit `.env` thật**. Source cung cấp `.env.example` để làm mẫu nhưng không chứa Gmail/App Password thật.

## Công nghệ

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10.0.12
- SQL Server `CANHTHIEN`
- Argon2id cho password hashing
- MailKit 4.18.0 cho SMTP

## Các chức năng hiện có

- Đăng nhập / đăng xuất
- Phân quyền Admin / Staff
- Ghi nhớ tên đăng nhập, không lưu mật khẩu
- Nút hiện/ẩn mật khẩu nằm trong ô password
- Đăng ký tài khoản Staff
- Quên mật khẩu qua email
- Token reset 256-bit, dùng một lần, hết hạn sau 15 phút
- Link `itdevicemanager://reset-password?...` mở form đặt lại mật khẩu
- Argon2id cho mật khẩu
- Dashboard
- CRUD thiết bị
- CRUD loại thiết bị
- CRUD phòng ban
- CRUD nhân viên
- CRUD tài khoản
- Cấp phát / thu hồi thiết bị
- Tìm kiếm / lọc
- Validation dữ liệu

## Cấu hình Gmail SMTP

Với Gmail:

```text
Host: smtp.gmail.com
Port: 587
SSL on connect: false
```

Nên dùng **Google App Password**, không dùng mật khẩu Gmail chính.

Ví dụ `.env`:

```env
ITDM_SMTP_HOST=smtp.gmail.com
ITDM_SMTP_PORT=587
ITDM_SMTP_USERNAME=your-email@gmail.com
ITDM_SMTP_PASSWORD=your-app-password
ITDM_SMTP_FROM_EMAIL=your-email@gmail.com
ITDM_SMTP_FROM_NAME=IT Device Manager
ITDM_SMTP_SSL_ON_CONNECT=false
```

Nếu giá trị có ký tự đặc biệt hoặc khoảng trắng ở đầu/cuối, có thể dùng dấu nháy kép:

```env
ITDM_SMTP_PASSWORD="your-password"
ITDM_SMTP_FROM_NAME="IT Device Manager"
```

## Admin cũ cần thêm email

Nếu database được nâng từ V1.0/V1.1, tài khoản `admin` cũ có thể chưa có email:

```text
Đăng nhập Admin
-> Tài khoản
-> chọn admin
-> Sửa
-> nhập Email
-> Lưu
```

Sau đó chức năng Quên mật khẩu mới gửi email cho admin được.

## Database

V1.2.1 không xóa database và không thay đổi schema so với V1.2.0.

V1.2.0 đã có:

```text
Users.Email
PasswordResetTokens
```

Database mặc định:

```text
Server=CANHTHIEN
Database=ITDeviceManagerDb
Trusted_Connection=True
TrustServerCertificate=True
```

## Icon ứng dụng

Đặt icon tại:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\ITDeviceManager\Assets\App.ico
```

Nên có các kích thước:

```text
16x16
32x32
48x48
256x256
```

Project đã cấu hình tự dùng `Assets\App.ico` cho EXE nếu file tồn tại.

## Nâng cấp bằng copy đè

1. Đóng ứng dụng và Visual Studio nếu đang Debug.
2. Giữ nguyên file `.env` hiện tại của bạn.
3. Giải nén gói V1.2.1.
4. Copy toàn bộ nội dung bên trong vào:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

5. Chọn **Replace the files in the destination**.
6. Gói nâng cấp không chứa `.env` thật nên không ghi đè credential của bạn.

## Kiểm tra build

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

## Chạy ứng dụng

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\run.bat
```

Hoặc:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## Lịch sử phiên bản

### V1.2.1

- Fix `WFO1000` của `PasswordInput` trên .NET 10 WinForms.
- Fix các nullable warning đã xuất hiện trong log build V1.2.0.
- Tự động đọc `.env` mà không cần package dotenv bên ngoài.
- Thêm `.env.example`.
- Bảo vệ `.env` bằng `.gitignore`.

### V1.2.0

- Password eye button nằm trong ô nhập mật khẩu.
- Đăng ký tài khoản.
- Ghi nhớ username.
- Quên mật khẩu qua email.
- Reset password qua custom URI protocol.
- Token reset dùng một lần.
- Bổ sung email cho tài khoản.
- SMTP qua MailKit.

### V1.1.0

- Nâng password hashing từ PBKDF2 sang Argon2id.
- Hỗ trợ tự nâng cấp hash cũ sau đăng nhập thành công.
- SQL Server mặc định `CANHTHIEN`.
