# IT Device Manager

**Ã„ÂÃ¡Â»Â tÃƒÂ i:** XÃƒÂ¢y dÃ¡Â»Â±ng phÃ¡ÂºÂ§n mÃ¡Â»Âm quÃ¡ÂºÂ£n lÃƒÂ½ thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹ CNTT trong doanh nghiÃ¡Â»â€¡p bÃ¡ÂºÂ±ng C# WinForms vÃƒÂ  Entity Framework.

## 1. CÃƒÂ´ng nghÃ¡Â»â€¡

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10
- SQL Server
- LINQ
- Argon2id cho password hashing
- MailKit cho SMTP

## 2. ThÃ†Â° mÃ¡Â»Â¥c lÃƒÂ m viÃ¡Â»â€¡c mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

SQL Server mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh:

```text
Server: CANHTHIEN
Database: ITDeviceManagerDb
Authentication: Windows Authentication
```

## 3. ChÃ¡Â»Â©c nÃ„Æ’ng chÃƒÂ­nh

- Ã„ÂÃ„Æ’ng nhÃ¡ÂºÂ­p / Ã„â€˜Ã„Æ’ng xuÃ¡ÂºÂ¥t.
- PhÃƒÂ¢n quyÃ¡Â»Ân Admin / Staff.
- Ghi nhÃ¡Â»â€º tÃƒÂªn Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p, khÃƒÂ´ng lÃ†Â°u mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u.
- HiÃ¡Â»â€¡n / Ã¡ÂºÂ©n mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u ngay trong ÃƒÂ´ nhÃ¡ÂºÂ­p.
- Ã„ÂÃ„Æ’ng kÃƒÂ½ tÃƒÂ i khoÃ¡ÂºÂ£n Staff.
- QuÃƒÂªn mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u qua email.
- Ã„ÂÃ¡ÂºÂ·t lÃ¡ÂºÂ¡i mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u bÃ¡ÂºÂ±ng token dÃƒÂ¹ng mÃ¡Â»â„¢t lÃ¡ÂºÂ§n, thÃ¡Â»Âi hÃ¡ÂºÂ¡n 15 phÃƒÂºt.
- Password hashing bÃ¡ÂºÂ±ng Argon2id.
- Dashboard thÃ¡Â»â€˜ng kÃƒÂª.
- CRUD thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹.
- CRUD loÃ¡ÂºÂ¡i thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹.
- CRUD phÃƒÂ²ng ban.
- CRUD nhÃƒÂ¢n viÃƒÂªn.
- CRUD tÃƒÂ i khoÃ¡ÂºÂ£n.
- CÃ¡ÂºÂ¥p phÃƒÂ¡t / thu hÃ¡Â»â€œi thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹.
- TÃƒÂ¬m kiÃ¡ÂºÂ¿m vÃƒÂ  lÃ¡Â»Âc dÃ¡Â»Â¯ liÃ¡Â»â€¡u.
- Validation dÃ¡Â»Â¯ liÃ¡Â»â€¡u bÃ¡ÂºÂ±ng WinForms `ErrorProvider` vÃƒÂ  tÃ¡ÂºÂ§ng nghiÃ¡Â»â€¡p vÃ¡Â»Â¥.
- LÃ†Â°u dÃ¡Â»Â¯ liÃ¡Â»â€¡u Unicode tiÃ¡ÂºÂ¿ng ViÃ¡Â»â€¡t bÃ¡ÂºÂ±ng SQL Server `nvarchar`.

## 4. Ã„ÂÃƒÂ¡p Ã¡Â»Â©ng yÃƒÂªu cÃ¡ÂºÂ§u Ã„â€˜Ã¡Â»â€œ ÃƒÂ¡n

| YÃƒÂªu cÃ¡ÂºÂ§u | PhÃ¡ÂºÂ§n Ã„â€˜ÃƒÂ¡p Ã¡Â»Â©ng |
|---|---|
| CRUD | ThiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, loÃ¡ÂºÂ¡i thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, phÃƒÂ²ng ban, nhÃƒÂ¢n viÃƒÂªn, tÃƒÂ i khoÃ¡ÂºÂ£n |
| Entity Framework | EF Core + SQL Server |
| WinForms | ToÃƒÂ n bÃ¡Â»â„¢ giao diÃ¡Â»â€¡n desktop dÃƒÂ¹ng Windows Forms |
| TÃƒÂ¬m kiÃ¡ÂºÂ¿m / lÃ¡Â»Âc | ThiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, nhÃƒÂ¢n viÃƒÂªn vÃƒÂ  cÃƒÂ¡c bÃ¡Â»â„¢ lÃ¡Â»Âc liÃƒÂªn quan |
| Login / phÃƒÂ¢n quyÃ¡Â»Ân | Admin / Staff |
| Validation | Username, email, Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i, mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u, mÃƒÂ£/serial, ngÃƒÂ y thÃƒÂ¡ng vÃƒÂ  nghiÃ¡Â»â€¡p vÃ¡Â»Â¥ |

## 5. CÃ¡ÂºÂ¥u hÃƒÂ¬nh `.env`

File thÃ¡ÂºÂ­t Ã„â€˜Ã¡ÂºÂ·t tÃ¡ÂºÂ¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

VÃƒÂ­ dÃ¡Â»Â¥:

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

`.gitignore` phÃ¡ÂºÂ£i giÃ¡Â»Â¯:

```gitignore
.env
.env.*
!.env.example
```

KhÃƒÂ´ng commit `.env` thÃ¡ÂºÂ­t lÃƒÂªn GitHub.

## 6. Build vÃƒÂ  chÃ¡ÂºÂ¡y

### Build

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\clean.bat
.\build.bat
```

HoÃ¡ÂºÂ·c:

```powershell
dotnet restore .\ITDeviceManager.sln
dotnet build .\ITDeviceManager.sln -c Debug
```

### ChÃ¡ÂºÂ¡y

```powershell
.\run.bat
```

HoÃ¡ÂºÂ·c:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## 7. Icon Ã¡Â»Â©ng dÃ¡Â»Â¥ng

Ã„ÂÃ¡ÂºÂ·t file icon tÃ¡ÂºÂ¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\ITDeviceManager\Assets\App.ico
```

NÃƒÂªn dÃƒÂ¹ng ICO thÃ¡ÂºÂ­t, khÃƒÂ´ng Ã„â€˜Ã¡Â»â€¢i phÃ¡ÂºÂ§n mÃ¡Â»Å¸ rÃ¡Â»â„¢ng tÃ¡Â»Â« PNG sang ICO.

## 8. GitHub vÃƒÂ  Release

Repository:

```text
TamNhien/ITDeviceManager
```

Ã„ÂÃ„Æ’ng nhÃ¡ÂºÂ­p GitHub CLI mÃ¡Â»â„¢t lÃ¡ÂºÂ§n:

```powershell
gh auth login
```

Release:

```powershell
.\release.bat X.Y.Z
```

VÃƒÂ­ dÃ¡Â»Â¥:

```powershell
.\release.bat 1.2.7
```

Quy trÃƒÂ¬nh release hiÃ¡Â»â€¡n tÃ¡ÂºÂ¡i:

```text
Restore
Ã¢â€ â€™ Build Release
Ã¢â€ â€™ Git add / commit
Ã¢â€ â€™ Push main
Ã¢â€ â€™ Publish win-x64
Ã¢â€ â€™ Ã„ÂÃƒÂ³ng gÃƒÂ³i ZIP
Ã¢â€ â€™ TÃ¡ÂºÂ¡o SHA-256
Ã¢â€ â€™ TÃ¡ÂºÂ¡o / push tag
Ã¢â€ â€™ TÃ¡ÂºÂ¡o GitHub Release
Ã¢â€ â€™ Upload release assets
```

KhÃƒÂ´ng cÃƒÂ²n project `ITDeviceManager.SelfTest`, `test.bat` hoÃ¡ÂºÂ·c `scripts\test.ps1`.

## 9. QuÃƒÂªn mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u

Email reset sÃ¡Â»Â­ dÃ¡Â»Â¥ng token ngÃ¡ÂºÂ«u nhiÃƒÂªn 256-bit, chÃ¡Â»â€° dÃƒÂ¹ng mÃ¡Â»â„¢t lÃ¡ÂºÂ§n vÃƒÂ  hÃ¡ÂºÂ¿t hÃ¡ÂºÂ¡n sau 15 phÃƒÂºt.

LuÃ¡Â»â€œng reset mÃ¡Â»â€ºi:

```text
Gmail
Ã¢â€ â€™ HTTPS bridge trÃƒÂªn GitHub Pages
Ã¢â€ â€™ itdevicemanager://reset-password?token=...
Ã¢â€ â€™ IT Device Manager
Ã¢â€ â€™ Form Ã„â€˜Ã¡ÂºÂ·t lÃ¡ÂºÂ¡i mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u
```

Trang bridge nÃ¡ÂºÂ±m trong:

```text
docs\reset-password.html
```

Token Ã„â€˜Ã†Â°Ã¡Â»Â£c Ã„â€˜Ã¡ÂºÂ·t trong URL fragment (`#token=...`) Ã¡Â»Å¸ trang bridge Ã„â€˜Ã¡Â»Æ’ khÃƒÂ´ng gÃ¡Â»Â­i token lÃƒÂªn mÃƒÂ¡y chÃ¡Â»Â§ GitHub Pages trong HTTP request.

## 10. BÃ¡ÂºÂ£o mÃ¡ÂºÂ­t mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u

- KhÃƒÂ´ng lÃ†Â°u mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u plaintext.
- KhÃƒÂ´ng dÃƒÂ¹ng MD5, SHA-1 hoÃ¡ÂºÂ·c SHA-256 thuÃ¡ÂºÂ§n Ã„â€˜Ã¡Â»Æ’ lÃ†Â°u mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u.
- MÃ¡ÂºÂ­t khÃ¡ÂºÂ©u Ã„â€˜Ã†Â°Ã¡Â»Â£c hash bÃ¡ÂºÂ±ng Argon2id.
- TÃƒÂ i khoÃ¡ÂºÂ£n PBKDF2 cÃ…Â© cÃƒÂ³ thÃ¡Â»Æ’ Ã„â€˜Ã†Â°Ã¡Â»Â£c nÃƒÂ¢ng cÃ¡ÂºÂ¥p sang Argon2id sau khi Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p thÃƒÂ nh cÃƒÂ´ng.
- ChÃƒÂ­nh sÃƒÂ¡ch mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u hiÃ¡Â»â€¡n tÃ¡ÂºÂ¡i:
  - 12Ã¢â‚¬â€œ128 kÃƒÂ½ tÃ¡Â»Â±.
  - ÃƒÂt nhÃ¡ÂºÂ¥t 1 chÃ¡Â»Â¯ hoa.
  - ÃƒÂt nhÃ¡ÂºÂ¥t 1 chÃ¡Â»Â¯ thÃ†Â°Ã¡Â»Âng.
  - ÃƒÂt nhÃ¡ÂºÂ¥t 1 chÃ¡Â»Â¯ sÃ¡Â»â€˜.
  - ÃƒÂt nhÃ¡ÂºÂ¥t 1 kÃƒÂ½ tÃ¡Â»Â± Ã„â€˜Ã¡ÂºÂ·c biÃ¡Â»â€¡t.
- Form Ã„â€˜Ã„Æ’ng kÃƒÂ½ hiÃ¡Â»Æ’n thÃ¡Â»â€¹ Ã„â€˜Ã¡Â»â„¢ mÃ¡ÂºÂ¡nh mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u theo thÃ¡Â»Âi gian thÃ¡Â»Â±c.
- Form Ã„â€˜Ã„Æ’ng kÃƒÂ½ kiÃ¡Â»Æ’m tra mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u nhÃ¡ÂºÂ­p lÃ¡ÂºÂ¡i trÃƒÂ¹ng khÃ¡Â»â€ºp theo thÃ¡Â»Âi gian thÃ¡Â»Â±c.

---

# LÃ¡Â»â€¹ch sÃ¡Â»Â­ phiÃƒÂªn bÃ¡ÂºÂ£n

> LÃ¡Â»â€¹ch sÃ¡Â»Â­ Ã„â€˜Ã†Â°Ã¡Â»Â£c giÃ¡Â»Â¯ **trong duy nhÃ¡ÂºÂ¥t file `README.md` nÃƒÂ y** vÃƒÂ  sÃ¡ÂºÂ¯p xÃ¡ÂºÂ¿p **tÃ„Æ’ng dÃ¡ÂºÂ§n theo phiÃƒÂªn bÃ¡ÂºÂ£n**. TÃ¡Â»Â« cÃƒÂ¡c bÃ¡ÂºÂ£n sau chÃ¡Â»â€° cÃ¡ÂºÂ­p nhÃ¡ÂºÂ­t tiÃ¡ÂºÂ¿p vÃƒÂ o cuÃ¡Â»â€˜i mÃ¡Â»Â¥c nÃƒÂ y, khÃƒÂ´ng tÃ¡ÂºÂ¡o `README_HOTFIX.txt`, `README_V*.md` hoÃ¡ÂºÂ·c file lÃ¡Â»â€¹ch sÃ¡Â»Â­ phiÃƒÂªn bÃ¡ÂºÂ£n riÃƒÂªng.

## V1.0.0

- KhÃ¡Â»Å¸i tÃ¡ÂºÂ¡o project .NET 10 WinForms.
- CRUD thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, loÃ¡ÂºÂ¡i thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, phÃƒÂ²ng ban, nhÃƒÂ¢n viÃƒÂªn vÃƒÂ  tÃƒÂ i khoÃ¡ÂºÂ£n.
- Dashboard.
- CÃ¡ÂºÂ¥p phÃƒÂ¡t / thu hÃ¡Â»â€œi thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹.
- TÃƒÂ¬m kiÃ¡ÂºÂ¿m / lÃ¡Â»Âc dÃ¡Â»Â¯ liÃ¡Â»â€¡u.
- Login / phÃƒÂ¢n quyÃ¡Â»Ân Admin vÃƒÂ  Staff.
- Validation dÃ¡Â»Â¯ liÃ¡Â»â€¡u.
- Entity Framework Core + SQL Server.
- Password hashing ban Ã„â€˜Ã¡ÂºÂ§u bÃ¡ÂºÂ±ng PBKDF2 + salt.

## V1.1.0

- ChuyÃ¡Â»Æ’n password hashing tÃ¡Â»Â« PBKDF2 sang Argon2id.
- TÃ¡Â»Â± nÃƒÂ¢ng cÃ¡ÂºÂ¥p hash PBKDF2 cÃ…Â© sau khi Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p thÃƒÂ nh cÃƒÂ´ng.
- Password policy 12Ã¢â‚¬â€œ128 kÃƒÂ½ tÃ¡Â»Â±.
- ThÃƒÂªm xÃƒÂ¡c nhÃ¡ÂºÂ­n mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u khi tÃ¡ÂºÂ¡o / Ã„â€˜Ã¡Â»â€¢i mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u.
- GiÃ¡Â»â€ºi hÃ¡ÂºÂ¡n 5 lÃ¡ÂºÂ§n Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p sai vÃƒÂ  khÃƒÂ³a tÃ¡ÂºÂ¡m trong phiÃƒÂªn Ã¡Â»Â©ng dÃ¡Â»Â¥ng.
- KhÃƒÂ´ng hiÃ¡Â»Æ’n thÃ¡Â»â€¹ exception nÃ¡Â»â„¢i bÃ¡Â»â„¢ trÃ¡Â»Â±c tiÃ¡ÂºÂ¿p tÃ¡ÂºÂ¡i form Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p.
- ChuyÃ¡Â»Æ’n SQL Server mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh sang `CANHTHIEN` + Windows Authentication.
- Cho phÃƒÂ©p override connection string bÃ¡ÂºÂ±ng `ITDM_CONNECTION_STRING`.

## V1.2.0

- Ã„ÂÃ†Â°a nÃƒÂºt hiÃ¡Â»â€¡n / Ã¡ÂºÂ©n mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u vÃƒÂ o ngay trong ÃƒÂ´ password.
- ThÃƒÂªm Ã„â€˜Ã„Æ’ng kÃƒÂ½ tÃƒÂ i khoÃ¡ÂºÂ£n Staff.
- ThÃƒÂªm ghi nhÃ¡Â»â€º username, khÃƒÂ´ng lÃ†Â°u password.
- ThÃƒÂªm quÃƒÂªn mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u qua email.
- ThÃƒÂªm reset password bÃ¡ÂºÂ±ng custom URI protocol `itdevicemanager://`.
- ThÃƒÂªm `Users.Email`.
- ThÃƒÂªm bÃ¡ÂºÂ£ng `PasswordResetTokens`.
- Token reset dÃƒÂ¹ng mÃ¡Â»â„¢t lÃ¡ÂºÂ§n, hÃ¡ÂºÂ¿t hÃ¡ÂºÂ¡n 15 phÃƒÂºt.
- SMTP qua MailKit.
- ThÃƒÂªm `build.bat`, `run.bat`, `clean.bat`.
- ChuÃ¡ÂºÂ©n bÃ¡Â»â€¹ hÃ¡Â»â€” trÃ¡Â»Â£ `App.ico`.

## V1.2.1

- Fix lÃ¡Â»â€”i WinForms `WFO1000` cÃ¡Â»Â§a custom password input.
- Fix cÃƒÂ¡c nullable warning xuÃ¡ÂºÂ¥t hiÃ¡Â»â€¡n trong log build.
- ThÃƒÂªm tÃ¡Â»Â± Ã„â€˜Ã¡Â»Âc file `.env` khi chÃ¡ÂºÂ¡y bÃ¡ÂºÂ±ng F5, `dotnet run` hoÃ¡ÂºÂ·c `run.bat`.
- ThÃƒÂªm `.env.example`.
- BÃ¡ÂºÂ£o vÃ¡Â»â€¡ `.env` bÃ¡ÂºÂ±ng `.gitignore`.

## V1.2.2

- Fix migration database cÃ…Â© gÃƒÂ¢y lÃ¡Â»â€”i `Invalid column name 'Email'`.
- TÃƒÂ¡ch cÃƒÂ¡c bÃ†Â°Ã¡Â»â€ºc tÃ¡ÂºÂ¡o `Users.Email`, `IX_Users_Email` vÃƒÂ  `PasswordResetTokens` thÃƒÂ nh cÃƒÂ¡c SQL command riÃƒÂªng.
- Migration schema chÃ¡ÂºÂ¡y idempotent.
- ThÃƒÂªm quy trÃƒÂ¬nh GitHub release tÃ¡Â»Â± Ã„â€˜Ã¡Â»â„¢ng.
- TÃ¡Â»Â± commit, push, tag, publish win-x64, tÃ¡ÂºÂ¡o ZIP vÃƒÂ  checksum.
- ChÃ¡ÂºÂ·n release nÃ¡ÂºÂ¿u `.env` bÃ¡Â»â€¹ Git track.

## V1.2.3

- Fix kiÃ¡Â»Æ’m tra `.env` khiÃ¡ÂºÂ¿n PowerShell hiÃ¡Â»Æ’u trÃ¡ÂºÂ¡ng thÃƒÂ¡i Ã¢â‚¬Å“khÃƒÂ´ng Ã„â€˜Ã†Â°Ã¡Â»Â£c Git trackÃ¢â‚¬Â thÃƒÂ nh lÃ¡Â»â€”i.
- Fix cÃƒÂ¹ng lÃ¡Â»â€”i trong release script.
- ThÃƒÂªm `App.ico` Windows thÃ¡ÂºÂ­t, multi-resolution.
- Fix lÃ¡Â»â€”i compiler `CS7065: Icon stream is not in the expected format`.

## V1.2.4

- Canh giÃ¡Â»Â¯a nÃƒÂºt Ã„ÂÃ„Æ’ng nhÃ¡ÂºÂ­p.
- Canh thÃ¡ÂºÂ³ng hÃƒÂ ng dÃƒÂ²ng `ChÃ†Â°a cÃƒÂ³ tÃƒÂ i khoÃ¡ÂºÂ£n? / Ã„ÂÃ„Æ’ng kÃƒÂ½ tÃƒÂ i khoÃ¡ÂºÂ£n`.
- Canh giÃ¡Â»Â¯a tiÃƒÂªu Ã„â€˜Ã¡Â»Â Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p.
- KhÃƒÂ´ng hiÃ¡Â»Æ’n thÃ¡Â»â€¹ mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u admin mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh trÃƒÂªn giao diÃ¡Â»â€¡n Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p.

## V1.2.5

- ThÃƒÂªm sÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i vÃƒÂ o form Ã„â€˜Ã„Æ’ng kÃƒÂ½ vÃƒÂ  tÃƒÂ i khoÃ¡ÂºÂ£n.
- ChuÃ¡ÂºÂ©n hÃƒÂ³a vÃƒÂ  validate sÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i.
- ChÃ¡ÂºÂ·n trÃƒÂ¹ng email vÃƒÂ  sÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i.
- ThÃƒÂªm unique filtered index `IX_Users_PhoneNumber`.
- HiÃ¡Â»Æ’n thÃ¡Â»â€¹ Ã„â€˜Ã¡Â»â„¢ mÃ¡ÂºÂ¡nh mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u theo thÃ¡Â»Âi gian thÃ¡Â»Â±c.
- Password policy bÃ¡ÂºÂ¯t buÃ¡Â»â„¢c chÃ¡Â»Â¯ hoa, chÃ¡Â»Â¯ thÃ†Â°Ã¡Â»Âng, sÃ¡Â»â€˜ vÃƒÂ  kÃƒÂ½ tÃ¡Â»Â± Ã„â€˜Ã¡ÂºÂ·c biÃ¡Â»â€¡t.
- KiÃ¡Â»Æ’m tra mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u nhÃ¡ÂºÂ­p lÃ¡ÂºÂ¡i trÃƒÂ¹ng khÃ¡Â»â€ºp theo thÃ¡Â»Âi gian thÃ¡Â»Â±c.
- Ã„ÂÃƒÂ¡nh dÃ¡ÂºÂ¥u dÃ¡Â»Â¯ liÃ¡Â»â€¡u ngÃ†Â°Ã¡Â»Âi dÃƒÂ¹ng nhÃ¡ÂºÂ­p lÃƒÂ  Unicode trong EF Core.
- NÃƒÂ¢ng cÃƒÂ¡c cÃ¡Â»â„¢t nÃ¡Â»â„¢i dung cÃ…Â© sang `nvarchar` Ã„â€˜Ã¡Â»Æ’ lÃ†Â°u Ã„â€˜ÃƒÂºng tiÃ¡ÂºÂ¿ng ViÃ¡Â»â€¡t cÃƒÂ³ dÃ¡ÂºÂ¥u.

## V1.2.6

- BÃ¡Â»Â project `ITDeviceManager.SelfTest`.
- BÃ¡Â»Â `test.bat` vÃƒÂ  `scripts\test.ps1`.
- Release khÃƒÂ´ng cÃƒÂ²n tÃ¡Â»Â± chÃ¡ÂºÂ¡y self-test/database test.
- Cho phÃƒÂ©p username Unicode, gÃ¡Â»â€œm chÃ¡Â»Â¯ tiÃ¡ÂºÂ¿ng ViÃ¡Â»â€¡t, sÃ¡Â»â€˜, khoÃ¡ÂºÂ£ng trÃ¡ÂºÂ¯ng, `.`, `_`, `-`.
- Fix `ErrorProvider` cÃ¡Â»Â§a username cÃ¡ÂºÂ­p nhÃ¡ÂºÂ­t Ã„â€˜ÃƒÂºng khi dÃ¡Â»Â¯ liÃ¡Â»â€¡u hÃ¡Â»Â£p lÃ¡Â»â€¡.
- BÃ¡Â»Â dÃƒÂ²ng ghi chÃƒÂº phÃƒÂ­a trÃƒÂªn nÃƒÂºt Ã„ÂÃ„Æ’ng kÃƒÂ½ vÃƒÂ  thu gÃ¡Â»Ân form Ã„â€˜Ã„Æ’ng kÃƒÂ½.
- Fix release script khi lÃ¡Â»â€¡nh Git khÃƒÂ´ng trÃ¡ÂºÂ£ stdout, trÃƒÂ¡nh lÃ¡Â»â€”i gÃ¡Â»Âi `.Trim()` trÃƒÂªn `$null`.
- Fix tÃ¡ÂºÂ¡o SHA-256 trÃƒÂªn mÃƒÂ´i trÃ†Â°Ã¡Â»Âng khÃƒÂ´ng cÃƒÂ³ `Get-FileHash` bÃ¡ÂºÂ±ng `System.Security.Cryptography.SHA256` cÃ¡Â»Â§a .NET.

## V1.2.7

- Fix nÃƒÂºt `Ã„ÂÃ¡ÂºÂ·t lÃ¡ÂºÂ¡i mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u` trong Gmail khÃƒÂ´ng mÃ¡Â»Å¸ Ã„â€˜Ã†Â°Ã¡Â»Â£c custom URI trÃ¡Â»Â±c tiÃ¡ÂºÂ¿p.
- Email reset chuyÃ¡Â»Æ’n sang link HTTPS trÃƒÂªn GitHub Pages.
- ThÃƒÂªm `docs\reset-password.html` lÃƒÂ m bridge tÃ¡Â»Â« HTTPS sang `itdevicemanager://`.
- Token Ã„â€˜Ã¡ÂºÂ·t trong fragment `#token=...` cÃ¡Â»Â§a URL bridge.
- Release script hÃ¡Â»â€” trÃ¡Â»Â£ publish GitHub Pages tÃ¡Â»Â« `main:/docs`.

---

## Quy Ã†Â°Ã¡Â»â€ºc tÃ¡Â»Â« cÃƒÂ¡c phiÃƒÂªn bÃ¡ÂºÂ£n tiÃ¡ÂºÂ¿p theo

- ChÃ¡Â»â€° duy trÃƒÂ¬ **mÃ¡Â»â„¢t file `README.md` duy nhÃ¡ÂºÂ¥t** Ã¡Â»Å¸ root project.
- KhÃƒÂ´ng tÃ¡ÂºÂ¡o thÃƒÂªm `README_HOTFIX.txt`, `README_V*.md`, `THAY_DOI_V*.md` hoÃ¡ÂºÂ·c file lÃ¡Â»â€¹ch sÃ¡Â»Â­ riÃƒÂªng.
- MÃ¡Â»â€”i bÃ¡ÂºÂ£n mÃ¡Â»â€ºi thÃƒÂªm mÃ¡Â»â„¢t mÃ¡Â»Â¥c mÃ¡Â»â€ºi Ã¡Â»Å¸ **cuÃ¡Â»â€˜i LÃ¡Â»â€¹ch sÃ¡Â»Â­ phiÃƒÂªn bÃ¡ÂºÂ£n**.
- ThÃ¡Â»Â© tÃ¡Â»Â± luÃƒÂ´n tÃ„Æ’ng dÃ¡ÂºÂ§n: `V1.0.0 Ã¢â€ â€™ V1.1.0 Ã¢â€ â€™ V1.2.0 Ã¢â€ â€™ ...`.
- Hotfix cÃ¡Â»Â§a mÃ¡Â»â„¢t phiÃƒÂªn bÃ¡ÂºÂ£n Ã„â€˜Ã†Â°Ã¡Â»Â£c gÃ¡Â»â„¢p vÃƒÂ o chÃƒÂ­nh mÃ¡Â»Â¥c phiÃƒÂªn bÃ¡ÂºÂ£n Ã„â€˜ÃƒÂ³ thay vÃƒÂ¬ tÃ¡ÂºÂ¡o README riÃƒÂªng.
