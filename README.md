# IT Device Manager

**Äá» tÃ i:** XÃ¢y dá»±ng pháº§n má»m quáº£n lÃ½ thiáº¿t bá»‹ CNTT trong doanh nghiá»‡p báº±ng C# WinForms vÃ  Entity Framework.

## 1. CÃ´ng nghá»‡

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10
- SQL Server
- LINQ
- Argon2id cho password hashing
- MailKit cho SMTP

## 2. ThÆ° má»¥c lÃ m viá»‡c máº·c Ä‘á»‹nh

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

SQL Server máº·c Ä‘á»‹nh:

```text
Server: CANHTHIEN
Database: ITDeviceManagerDb
Authentication: Windows Authentication
```

## 3. Chá»©c nÄƒng chÃ­nh

- ÄÄƒng nháº­p / Ä‘Äƒng xuáº¥t.
- PhÃ¢n quyá»n Admin / Staff.
- Ghi nhá»› tÃªn Ä‘Äƒng nháº­p, khÃ´ng lÆ°u máº­t kháº©u.
- Hiá»‡n / áº©n máº­t kháº©u ngay trong Ã´ nháº­p.
- ÄÄƒng kÃ½ tÃ i khoáº£n Staff.
- QuÃªn máº­t kháº©u qua email.
- Äáº·t láº¡i máº­t kháº©u báº±ng token dÃ¹ng má»™t láº§n, thá»i háº¡n 15 phÃºt.
- Password hashing báº±ng Argon2id; salt náº±m trong chuá»—i PHC, khÃ´ng cÃ²n cá»™t `PasswordSalt`.
- Dashboard thá»‘ng kÃª.
- CRUD thiáº¿t bá»‹.
- CRUD loáº¡i thiáº¿t bá»‹.
- CRUD phÃ²ng ban.
- CRUD nhÃ¢n viÃªn.
- CRUD tÃ i khoáº£n.
- Cáº¥p phÃ¡t / thu há»“i thiáº¿t bá»‹.
- TÃ¬m kiáº¿m vÃ  lá»c dá»¯ liá»‡u.
- Validation dá»¯ liá»‡u báº±ng WinForms `ErrorProvider` vÃ  táº§ng nghiá»‡p vá»¥.
- LÆ°u dá»¯ liá»‡u Unicode tiáº¿ng Viá»‡t báº±ng SQL Server `nvarchar`.
- Tá»± Ä‘á»™ng bá»• sung dá»¯ liá»‡u máº«u idempotent khi á»©ng dá»¥ng khá»Ÿi Ä‘á»™ng.

## 4. ÄÃ¡p á»©ng yÃªu cáº§u Ä‘á»“ Ã¡n

| YÃªu cáº§u | Pháº§n Ä‘Ã¡p á»©ng |
|---|---|
| CRUD | Thiáº¿t bá»‹, loáº¡i thiáº¿t bá»‹, phÃ²ng ban, nhÃ¢n viÃªn, tÃ i khoáº£n |
| Entity Framework | EF Core + SQL Server |
| WinForms | ToÃ n bá»™ giao diá»‡n desktop dÃ¹ng Windows Forms |
| TÃ¬m kiáº¿m / lá»c | Thiáº¿t bá»‹, nhÃ¢n viÃªn vÃ  cÃ¡c bá»™ lá»c liÃªn quan |
| Login / phÃ¢n quyá»n | Admin / Staff |
| Validation | Username, email, Ä‘iá»‡n thoáº¡i, máº­t kháº©u, mÃ£/serial, ngÃ y thÃ¡ng vÃ  nghiá»‡p vá»¥ |

## 5. Cáº¥u hÃ¬nh `.env`

File tháº­t Ä‘áº·t táº¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

VÃ­ dá»¥:

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

`.gitignore` pháº£i giá»¯:

```gitignore
.env
.env.*
!.env.example
```

KhÃ´ng commit `.env` tháº­t lÃªn GitHub.

## 6. Build vÃ  cháº¡y

### Build

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\clean.bat
.\build.bat
```

Hoáº·c:

```powershell
dotnet restore .\ITDeviceManager.sln
dotnet build .\ITDeviceManager.sln -c Debug
```

### Cháº¡y

```powershell
.\run.bat
```

Hoáº·c:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## 7. Icon á»©ng dá»¥ng

Äáº·t file icon táº¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\ITDeviceManager\Assets\App.ico
```

NÃªn dÃ¹ng ICO tháº­t, khÃ´ng Ä‘á»•i pháº§n má»Ÿ rá»™ng tá»« PNG sang ICO.

## 8. GitHub vÃ  Release

Repository:

```text
TamNhien/ITDeviceManager
```

ÄÄƒng nháº­p GitHub CLI má»™t láº§n:

```powershell
gh auth login
```

Release:

```powershell
.\release.bat X.Y.Z
```

VÃ­ dá»¥:

```powershell
.\release.bat 1.3.0
```

Quy trÃ¬nh release hiá»‡n táº¡i:

```text
Restore
â†’ Build Release
â†’ Git add / commit
â†’ Push main
â†’ Publish win-x64
â†’ ÄÃ³ng gÃ³i ZIP
â†’ Táº¡o SHA-256
â†’ Táº¡o / push tag
â†’ Táº¡o GitHub Release
â†’ Upload release assets
```

KhÃ´ng cÃ²n project `ITDeviceManager.SelfTest`, `test.bat` hoáº·c `scripts\test.ps1`.

## 9. QuÃªn máº­t kháº©u

Email reset sá»­ dá»¥ng token ngáº«u nhiÃªn 256-bit, chá»‰ dÃ¹ng má»™t láº§n vÃ  háº¿t háº¡n sau 15 phÃºt.

Luá»“ng reset má»›i:

```text
Gmail
â†’ HTTPS bridge trÃªn GitHub Pages
â†’ itdevicemanager://reset-password?token=...
â†’ IT Device Manager
â†’ Form Ä‘áº·t láº¡i máº­t kháº©u
```

Trang bridge náº±m trong:

```text
docs\reset-password.html
```

Token Ä‘Æ°á»£c Ä‘áº·t trong URL fragment (`#token=...`) á»Ÿ trang bridge Ä‘á»ƒ khÃ´ng gá»­i token lÃªn mÃ¡y chá»§ GitHub Pages trong HTTP request.

## 10. Dá»¯ liá»‡u máº«u tá»± Ä‘á»™ng

`SampleDataSeeder` cháº¡y tá»± Ä‘á»™ng sau khi schema Ä‘Æ°á»£c táº¡o/nÃ¢ng cáº¥p. Seeder lÃ  **idempotent**: chá»‰ thÃªm nhá»¯ng dÃ²ng máº«u cÃ²n thiáº¿u vÃ  khÃ´ng xÃ³a dá»¯ liá»‡u tháº­t.

CÃ¡c báº£ng hiá»‡n táº¡i cÃ³ bá»™ 10 dÃ²ng máº«u:

- `Roles`: 10 vai trÃ² máº«u; `Admin` vÃ  `Staff` váº«n giá»¯ Ã½ nghÄ©a hiá»‡n táº¡i.
- `Departments`: 10 phÃ²ng/ban.
- `DeviceTypes`: 10 loáº¡i thiáº¿t bá»‹.
- `Users`: 10 tÃ i khoáº£n máº«u mang tÃªn tiáº¿ng Viá»‡t; cÃ¡c tÃ i khoáº£n máº«u bá»‹ vÃ´ hiá»‡u hÃ³a vÃ  máº­t kháº©u Ä‘Æ°á»£c sinh ngáº«u nhiÃªn rá»“i bá», nÃªn khÃ´ng táº¡o lá»‘i Ä‘Äƒng nháº­p máº·c Ä‘á»‹nh.
- `Employees`: 10 nhÃ¢n viÃªn máº«u tÃªn tiáº¿ng Viá»‡t.
- `Devices`: 10 thiáº¿t bá»‹ máº«u thá»±c táº¿.
- `DeviceAssignments`: 10 lá»‹ch sá»­ cáº¥p phÃ¡t/thu há»“i máº«u.
- `PasswordResetTokens`: 10 token máº«u Ä‘Ã£ dÃ¹ng/háº¿t háº¡n, khÃ´ng thá»ƒ dÃ¹ng Ä‘á»ƒ reset máº­t kháº©u.

Khi phiÃªn báº£n sau bá»• sung báº£ng nghiá»‡p vá»¥ má»›i, Ä‘á»‹nh nghÄ©a máº«u Ä‘Æ°á»£c thÃªm táº­p trung trong `SampleDataSeeder`; chÆ°Æ¡ng trÃ¬nh sáº½ tá»± chÃ¨n dá»¯ liá»‡u lÃºc khá»Ÿi Ä‘á»™ng, khÃ´ng cáº§n cháº¡y SQL seed báº±ng tay. Vá»›i báº£ng cÃ³ quan há»‡/constraint Ä‘áº·c thÃ¹, khÃ´ng táº¡o dá»¯ liá»‡u ngáº«u nhiÃªn mÃ¹ Ä‘á»ƒ trÃ¡nh phÃ¡ khÃ³a ngoáº¡i hoáº·c unique constraint.

## 11. Báº£o máº­t máº­t kháº©u

- KhÃ´ng lÆ°u máº­t kháº©u plaintext.
- KhÃ´ng dÃ¹ng MD5, SHA-1 hoáº·c SHA-256 thuáº§n Ä‘á»ƒ lÆ°u máº­t kháº©u.
- Máº­t kháº©u Ä‘Æ°á»£c hash báº±ng Argon2id.
- Chuá»—i `$argon2id$...` chá»©a version, cost parameters, salt ngáº«u nhiÃªn vÃ  hash; vÃ¬ váº­y V1.3.0 Ä‘Ã£ loáº¡i bá» cá»™t `Users.PasswordSalt`.
- Migration V1.3.0 chá»‰ xÃ³a cá»™t salt khi **toÃ n bá»™** tÃ i khoáº£n hiá»‡n cÃ³ Ä‘Ã£ lÃ  Argon2id; náº¿u cÃ²n tÃ i khoáº£n legacy, á»©ng dá»¥ng dá»«ng vá»›i thÃ´ng bÃ¡o rÃµ rÃ ng thay vÃ¬ lÃ m máº¥t kháº£ nÄƒng Ä‘Äƒng nháº­p.
- ChÃ­nh sÃ¡ch máº­t kháº©u hiá»‡n táº¡i:
  - 12â€“128 kÃ½ tá»±.
  - Ãt nháº¥t 1 chá»¯ hoa.
  - Ãt nháº¥t 1 chá»¯ thÆ°á»ng.
  - Ãt nháº¥t 1 chá»¯ sá»‘.
  - Ãt nháº¥t 1 kÃ½ tá»± Ä‘áº·c biá»‡t.
- Form Ä‘Äƒng kÃ½ hiá»ƒn thá»‹ Ä‘á»™ máº¡nh máº­t kháº©u theo thá»i gian thá»±c.
- Form Ä‘Äƒng kÃ½ kiá»ƒm tra máº­t kháº©u nháº­p láº¡i trÃ¹ng khá»›p theo thá»i gian thá»±c.

---

# Lá»‹ch sá»­ phiÃªn báº£n

> Lá»‹ch sá»­ Ä‘Æ°á»£c giá»¯ **trong duy nháº¥t file `README.md` nÃ y** vÃ  sáº¯p xáº¿p **tÄƒng dáº§n theo phiÃªn báº£n**. Tá»« cÃ¡c báº£n sau chá»‰ cáº­p nháº­t tiáº¿p vÃ o cuá»‘i má»¥c nÃ y, khÃ´ng táº¡o `README_HOTFIX.txt`, `README_V*.md` hoáº·c file lá»‹ch sá»­ phiÃªn báº£n riÃªng.

## V1.0.0

- Khá»Ÿi táº¡o project .NET 10 WinForms.
- CRUD thiáº¿t bá»‹, loáº¡i thiáº¿t bá»‹, phÃ²ng ban, nhÃ¢n viÃªn vÃ  tÃ i khoáº£n.
- Dashboard.
- Cáº¥p phÃ¡t / thu há»“i thiáº¿t bá»‹.
- TÃ¬m kiáº¿m / lá»c dá»¯ liá»‡u.
- Login / phÃ¢n quyá»n Admin vÃ  Staff.
- Validation dá»¯ liá»‡u.
- Entity Framework Core + SQL Server.
- Password hashing ban Ä‘áº§u báº±ng PBKDF2 + salt.

## V1.1.0

- Chuyá»ƒn password hashing tá»« PBKDF2 sang Argon2id.
- Tá»± nÃ¢ng cáº¥p hash PBKDF2 cÅ© sau khi Ä‘Äƒng nháº­p thÃ nh cÃ´ng.
- Password policy 12â€“128 kÃ½ tá»±.
- ThÃªm xÃ¡c nháº­n máº­t kháº©u khi táº¡o / Ä‘á»•i máº­t kháº©u.
- Giá»›i háº¡n 5 láº§n Ä‘Äƒng nháº­p sai vÃ  khÃ³a táº¡m trong phiÃªn á»©ng dá»¥ng.
- KhÃ´ng hiá»ƒn thá»‹ exception ná»™i bá»™ trá»±c tiáº¿p táº¡i form Ä‘Äƒng nháº­p.
- Chuyá»ƒn SQL Server máº·c Ä‘á»‹nh sang `CANHTHIEN` + Windows Authentication.
- Cho phÃ©p override connection string báº±ng `ITDM_CONNECTION_STRING`.

## V1.2.0

- ÄÆ°a nÃºt hiá»‡n / áº©n máº­t kháº©u vÃ o ngay trong Ã´ password.
- ThÃªm Ä‘Äƒng kÃ½ tÃ i khoáº£n Staff.
- ThÃªm ghi nhá»› username, khÃ´ng lÆ°u password.
- ThÃªm quÃªn máº­t kháº©u qua email.
- ThÃªm reset password báº±ng custom URI protocol `itdevicemanager://`.
- ThÃªm `Users.Email`.
- ThÃªm báº£ng `PasswordResetTokens`.
- Token reset dÃ¹ng má»™t láº§n, háº¿t háº¡n 15 phÃºt.
- SMTP qua MailKit.
- ThÃªm `build.bat`, `run.bat`, `clean.bat`.
- Chuáº©n bá»‹ há»— trá»£ `App.ico`.

## V1.2.1

- Fix lá»—i WinForms `WFO1000` cá»§a custom password input.
- Fix cÃ¡c nullable warning xuáº¥t hiá»‡n trong log build.
- ThÃªm tá»± Ä‘á»c file `.env` khi cháº¡y báº±ng F5, `dotnet run` hoáº·c `run.bat`.
- ThÃªm `.env.example`.
- Báº£o vá»‡ `.env` báº±ng `.gitignore`.

## V1.2.2

- Fix migration database cÅ© gÃ¢y lá»—i `Invalid column name 'Email'`.
- TÃ¡ch cÃ¡c bÆ°á»›c táº¡o `Users.Email`, `IX_Users_Email` vÃ  `PasswordResetTokens` thÃ nh cÃ¡c SQL command riÃªng.
- Migration schema cháº¡y idempotent.
- ThÃªm quy trÃ¬nh GitHub release tá»± Ä‘á»™ng.
- Tá»± commit, push, tag, publish win-x64, táº¡o ZIP vÃ  checksum.
- Cháº·n release náº¿u `.env` bá»‹ Git track.

## V1.2.3

- Fix kiá»ƒm tra `.env` khiáº¿n PowerShell hiá»ƒu tráº¡ng thÃ¡i â€œkhÃ´ng Ä‘Æ°á»£c Git trackâ€ thÃ nh lá»—i.
- Fix cÃ¹ng lá»—i trong release script.
- ThÃªm `App.ico` Windows tháº­t, multi-resolution.
- Fix lá»—i compiler `CS7065: Icon stream is not in the expected format`.

## V1.2.4

- Canh giá»¯a nÃºt ÄÄƒng nháº­p.
- Canh tháº³ng hÃ ng dÃ²ng `ChÆ°a cÃ³ tÃ i khoáº£n? / ÄÄƒng kÃ½ tÃ i khoáº£n`.
- Canh giá»¯a tiÃªu Ä‘á» Ä‘Äƒng nháº­p.
- KhÃ´ng hiá»ƒn thá»‹ máº­t kháº©u admin máº·c Ä‘á»‹nh trÃªn giao diá»‡n Ä‘Äƒng nháº­p.

## V1.2.5

- ThÃªm sá»‘ Ä‘iá»‡n thoáº¡i vÃ o form Ä‘Äƒng kÃ½ vÃ  tÃ i khoáº£n.
- Chuáº©n hÃ³a vÃ  validate sá»‘ Ä‘iá»‡n thoáº¡i.
- Cháº·n trÃ¹ng email vÃ  sá»‘ Ä‘iá»‡n thoáº¡i.
- ThÃªm unique filtered index `IX_Users_PhoneNumber`.
- Hiá»ƒn thá»‹ Ä‘á»™ máº¡nh máº­t kháº©u theo thá»i gian thá»±c.
- Password policy báº¯t buá»™c chá»¯ hoa, chá»¯ thÆ°á»ng, sá»‘ vÃ  kÃ½ tá»± Ä‘áº·c biá»‡t.
- Kiá»ƒm tra máº­t kháº©u nháº­p láº¡i trÃ¹ng khá»›p theo thá»i gian thá»±c.
- ÄÃ¡nh dáº¥u dá»¯ liá»‡u ngÆ°á»i dÃ¹ng nháº­p lÃ  Unicode trong EF Core.
- NÃ¢ng cÃ¡c cá»™t ná»™i dung cÅ© sang `nvarchar` Ä‘á»ƒ lÆ°u Ä‘Ãºng tiáº¿ng Viá»‡t cÃ³ dáº¥u.

## V1.2.6

- Bá» project `ITDeviceManager.SelfTest`.
- Bá» `test.bat` vÃ  `scripts\test.ps1`.
- Release khÃ´ng cÃ²n tá»± cháº¡y self-test/database test.
- Cho phÃ©p username Unicode, gá»“m chá»¯ tiáº¿ng Viá»‡t, sá»‘, khoáº£ng tráº¯ng, `.`, `_`, `-`.
- Fix `ErrorProvider` cá»§a username cáº­p nháº­t Ä‘Ãºng khi dá»¯ liá»‡u há»£p lá»‡.
- Bá» dÃ²ng ghi chÃº phÃ­a trÃªn nÃºt ÄÄƒng kÃ½ vÃ  thu gá»n form Ä‘Äƒng kÃ½.
- Fix release script khi lá»‡nh Git khÃ´ng tráº£ stdout, trÃ¡nh lá»—i gá»i `.Trim()` trÃªn `$null`.
- Fix táº¡o SHA-256 trÃªn mÃ´i trÆ°á»ng khÃ´ng cÃ³ `Get-FileHash` báº±ng `System.Security.Cryptography.SHA256` cá»§a .NET.

## V1.2.7

- Fix nÃºt `Äáº·t láº¡i máº­t kháº©u` trong Gmail khÃ´ng má»Ÿ Ä‘Æ°á»£c custom URI trá»±c tiáº¿p.
- Email reset chuyá»ƒn sang link HTTPS trÃªn GitHub Pages.
- ThÃªm `docs\reset-password.html` lÃ m bridge tá»« HTTPS sang `itdevicemanager://`.
- Token Ä‘áº·t trong fragment `#token=...` cá»§a URL bridge.
- Release script há»— trá»£ publish GitHub Pages tá»« `main:/docs`.

## V1.2.8

- Bá»• sung hiá»ƒn thá»‹ **Ä‘á»™ máº¡nh máº­t kháº©u theo thá»i gian thá»±c** trÃªn form Äáº·t láº¡i máº­t kháº©u.
- Hiá»ƒn thá»‹ tá»«ng Ä‘iá»u kiá»‡n: tá»‘i thiá»ƒu 12 kÃ½ tá»±, chá»¯ hoa, chá»¯ thÆ°á»ng, sá»‘ vÃ  kÃ½ tá»± Ä‘áº·c biá»‡t.
- Kiá»ƒm tra **máº­t kháº©u nháº­p láº¡i trÃ¹ng khá»›p theo thá»i gian thá»±c**.
- NÃºt `Äáº·t láº¡i máº­t kháº©u` chá»‰ Ä‘Æ°á»£c báº­t khi token cÃ²n hiá»‡u lá»±c, máº­t kháº©u Ä‘áº¡t policy vÃ  hai Ã´ máº­t kháº©u trÃ¹ng nhau.
- Giá»¯ validation phÃ­a nghiá»‡p vá»¥ trÆ°á»›c khi thá»±c hiá»‡n reset Ä‘á»ƒ trÃ¡nh bypass kiá»ƒm tra trÃªn giao diá»‡n.

## V1.2.9

- Sá»­a form `Äáº·t láº¡i máº­t kháº©u` dÃ¹ng layout cá»‘ Ä‘á»‹nh Ä‘á»ƒ khá»‘i kiá»ƒm tra máº­t kháº©u luÃ´n hiá»ƒn thá»‹.
- Hiá»ƒn thá»‹ realtime `Äá»™ máº¡nh máº­t kháº©u`, Ä‘á»§ 5 Ä‘iá»u kiá»‡n vÃ  tráº¡ng thÃ¡i nháº­p láº¡i trÃ¹ng khá»›p.
- NÃºt `Äáº·t láº¡i máº­t kháº©u` chá»‰ báº­t khi token há»£p lá»‡, máº­t kháº©u Ä‘áº¡t policy vÃ  hai Ã´ trÃ¹ng nhau.
- GÃ³i upgrade Ä‘Æ°á»£c Ä‘Ã³ng ZIP dáº¡ng pháº³ng: giáº£i nÃ©n rá»“i copy trá»±c tiáº¿p vÃ o root project Ä‘á»ƒ cháº¯c cháº¯n ghi Ä‘Ã¨ Ä‘Ãºng file.

## V1.3.0

- Loáº¡i bá» hoÃ n toÃ n `PasswordSalt` khá»i model, Entity Framework vÃ  SQL Server.
- `PasswordHasher` chá»‰ cÃ²n Argon2id; salt ngáº«u nhiÃªn Ä‘Æ°á»£c lÆ°u bÃªn trong chuá»—i PHC `$argon2id$...`.
- Migration `SchemaUpgradeV130` tá»± kiá»ƒm tra táº¥t cáº£ tÃ i khoáº£n Ä‘Ã£ dÃ¹ng Argon2id trÆ°á»›c khi drop cá»™t `Users.PasswordSalt`.
- ThÃªm `SampleDataSeeder` cháº¡y tá»± Ä‘á»™ng, idempotent sau má»—i láº§n khá»Ÿi Ä‘á»™ng.
- Bá»• sung 10 dÃ²ng dá»¯ liá»‡u máº«u cÃ³ tÃªn tiáº¿ng Viá»‡t/giÃ¡ trá»‹ thá»±c táº¿ cho tá»«ng báº£ng hiá»‡n táº¡i.
- TÃ i khoáº£n máº«u Ä‘Æ°á»£c vÃ´ hiá»‡u hÃ³a vÃ  dÃ¹ng máº­t kháº©u ngáº«u nhiÃªn khÃ´ng Ä‘Æ°á»£c lÆ°u plaintext.
- Token reset máº«u Ä‘á»u á»Ÿ tráº¡ng thÃ¡i Ä‘Ã£ dÃ¹ng/háº¿t háº¡n Ä‘á»ƒ khÃ´ng táº¡o rá»§i ro báº£o máº­t.
- Bá»• sung `database\upgrade_v1.3.0.sql` vÃ  cáº­p nháº­t `database\verify_schema.sql`.

---

## Quy Æ°á»›c tá»« cÃ¡c phiÃªn báº£n tiáº¿p theo

- Chá»‰ duy trÃ¬ **má»™t file `README.md` duy nháº¥t** á»Ÿ root project.
- KhÃ´ng táº¡o thÃªm `README_HOTFIX.txt`, `README_V*.md`, `THAY_DOI_V*.md` hoáº·c file lá»‹ch sá»­ riÃªng.
- Má»—i báº£n má»›i thÃªm má»™t má»¥c má»›i á»Ÿ **cuá»‘i Lá»‹ch sá»­ phiÃªn báº£n**.
- Thá»© tá»± luÃ´n tÄƒng dáº§n: `V1.0.0 â†’ V1.1.0 â†’ V1.2.0 â†’ ...`.
- Hotfix cá»§a má»™t phiÃªn báº£n Ä‘Æ°á»£c gá»™p vÃ o chÃ­nh má»¥c phiÃªn báº£n Ä‘Ã³ thay vÃ¬ táº¡o README riÃªng.
