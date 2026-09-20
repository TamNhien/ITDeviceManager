# IT Device Manager - V1.2.6

## V1.2.6 - Registration UI cleanup and removal of SelfTest

- Bá» hoÃ n toÃ n project `ITDeviceManager.SelfTest` khá»i solution hiá»‡n táº¡i.
- Bá» `test.bat` vÃ  `scripts/test.ps1`; quy trÃ¬nh release khÃ´ng tá»± cháº¡y self-test/database test ná»¯a.
- `clean.bat` V1.2.6 tá»± xÃ³a cÃ¡c file/thÆ° má»¥c test cÅ© cÃ²n sÃ³t sau khi copy Ä‘Ã¨.
- Form ÄÄƒng kÃ½ bá» dÃ²ng ghi chÃº `TÃ i khoáº£n tá»± Ä‘Äƒng kÃ½ cÃ³ quyá»n Staff...` phÃ­a trÃªn nÃºt ÄÄƒng kÃ½ vÃ  thu gá»n chiá»u cao form.
- TÃªn Ä‘Äƒng nháº­p cho phÃ©p chá»¯ Unicode/tiáº¿ng Viá»‡t, chá»¯ sá»‘, khoáº£ng tráº¯ng, `.`, `_`, `-`; `Äáº¡t Br` lÃ  há»£p lá»‡.
- ErrorProvider cá»§a tÃªn Ä‘Äƒng nháº­p tá»± biáº¿n máº¥t ngay khi ná»™i dung Ä‘Ã£ há»£p lá»‡.
- Giá»¯ nguyÃªn sá»‘ Ä‘iá»‡n thoáº¡i, password strength, kiá»ƒm tra nháº­p láº¡i máº­t kháº©u, Argon2id vÃ  Unicode SQL tá»« V1.2.5.


## V1.2.5 - Registration phone, live password strength, Unicode SQL

- ThÃªm **Sá»‘ Ä‘iá»‡n thoáº¡i** vÃ o form Ä‘Äƒng kÃ½ vÃ  báº£ng `Users.PhoneNumber`; tá»± chuáº©n hÃ³a sá»‘ Ä‘iá»‡n thoáº¡i, kiá»ƒm tra há»£p lá»‡ vÃ  cháº·n trÃ¹ng.
- Hiá»ƒn thá»‹ **Ä‘á»™ máº¡nh máº­t kháº©u theo thá»i gian thá»±c** vÃ  tá»«ng Ä‘iá»u kiá»‡n: tá»‘i thiá»ƒu 12 kÃ½ tá»±, chá»¯ hoa, chá»¯ thÆ°á»ng, sá»‘, kÃ½ tá»± Ä‘áº·c biá»‡t.
- Kiá»ƒm tra **máº­t kháº©u nháº­p láº¡i trÃ¹ng khá»›p theo thá»i gian thá»±c** trÆ°á»›c khi báº¥m ÄÄƒng kÃ½; validation khi lÆ°u váº«n Ä‘Æ°á»£c giá»¯ á»Ÿ táº§ng nghiá»‡p vá»¥.
- Password policy toÃ n há»‡ thá»‘ng Ä‘Æ°á»£c nÃ¢ng tá»« chá»‰ kiá»ƒm tra Ä‘á»™ dÃ i sang báº¯t buá»™c Ä‘á»§ hoa/thÆ°á»ng/sá»‘/kÃ½ tá»± Ä‘áº·c biá»‡t.
- `AppDbContext` Ä‘Ã¡nh dáº¥u rÃµ cÃ¡c trÆ°á»ng tiáº¿ng Viá»‡t lÃ  Unicode; schema upgrader tá»± chuyá»ƒn cÃ¡c cá»™t user-facing cÅ© tá»« `varchar/char/text` sang `nvarchar` Ä‘á»ƒ lÆ°u Ä‘Ãºng chá»¯ cÃ³ dáº¥u.
- Tá»± thÃªm `IX_Users_PhoneNumber` dáº¡ng unique filtered index. TÃ i khoáº£n cÅ© (ká»ƒ cáº£ admin) Ä‘Æ°á»£c phÃ©p cÃ³ `PhoneNumber = NULL`.
- Cáº­p nháº­t quáº£n lÃ½ tÃ i khoáº£n Ä‘á»ƒ xem/sá»­a sá»‘ Ä‘iá»‡n thoáº¡i; tá»± test kiá»ƒm tra phone, password policy, Unicode schema vÃ  database V1.2.5.
- **KhÃ´ng Ä‘á»•i `DbInitializer.cs` trong gÃ³i upgrade**, nÃªn máº­t kháº©u admin máº·c Ä‘á»‹nh báº¡n Ä‘Ã£ tá»± chá»‰nh trÃªn mÃ¡y khÃ´ng bá»‹ ghi Ä‘Ã¨.

## V1.2.4 - Login UI alignment and credential-hint hardening

- Centered the Login button within the 290 px credential column.
- Rebuilt the register row with a two-column TableLayoutPanel so both texts share the same vertical centerline.
- Centered the login title and default-account hint for a more consistent visual hierarchy.
- The login screen no longer prints the default administrator password; only the username hint is shown.
- Existing authentication, Argon2id hashing, remember-username, forgot-password, SMTP reset, test, GitHub push and release automation remain unchanged.

## V1.2.3 - test/Git secret check + Windows icon hotfix

- Fixed `scripts/test.ps1` so an untracked `.env` is treated as **safe**, not as a PowerShell failure. The check no longer uses `git ls-files --error-unmatch`, which writes an expected error to stderr when `.env` is not tracked.
- Applied the same `.env` tracking fix to `scripts/release.ps1`.
- Added an actual multi-resolution Windows `App.ico` (16/24/32/48/64/128/256 px) using classic ICO/BMP frames for maximum Win32 resource compiler compatibility. **Do not rename a PNG to `.ico`.**
- `test.bat` now validates the ICO container before `dotnet build` and gives a clear message if the file is only a renamed PNG/JPG.
- Keeps the upgrade-in-place path and existing SQL Server/database data unchanged.


**Äá» tÃ i:** XÃ¢y dá»±ng pháº§n má»m quáº£n lÃ½ thiáº¿t bá»‹ CNTT trong doanh nghiá»‡p báº±ng C# WinForms vÃ  Entity Framework.

## ThÆ° má»¥c lÃ m viá»‡c máº·c Ä‘á»‹nh

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

V1.2.2 lÃ  báº£n **hotfix + automation**, cÃ³ thá»ƒ copy Ä‘Ã¨ lÃªn V1.2.1 vÃ  giá»¯ nguyÃªn database `ITDeviceManagerDb`.

## V1.2.2 Ä‘Ã£ sá»­a gÃ¬

### Fix lá»—i `Invalid column name 'Email'`

V1.2.1 gá»­i `ALTER TABLE ... ADD Email` vÃ  `CREATE INDEX ... Email` trong cÃ¹ng má»™t SQL batch. SQL Server cÃ³ thá»ƒ biÃªn dá»‹ch cÃ¢u `CREATE INDEX` trÆ°á»›c khi cÃ¢u `ALTER TABLE` Ä‘Æ°á»£c thá»±c thi, vÃ¬ váº­y database V1.0/V1.1 bÃ¡o:

```text
Invalid column name 'Email'.
Invalid column name 'Email'.
```

V1.2.2 tÃ¡ch migration thÃ nh cÃ¡c command riÃªng, cháº¡y theo thá»© tá»±:

```text
1. Táº¡o Users.Email náº¿u chÆ°a cÃ³
2. Táº¡o IX_Users_Email náº¿u chÆ°a cÃ³
3. Táº¡o PasswordResetTokens náº¿u chÆ°a cÃ³
4. Sau Ä‘Ã³ má»›i truy váº¥n Users báº±ng Entity Framework
```

Migration lÃ  **idempotent**: cháº¡y láº¡i nhiá»u láº§n khÃ´ng táº¡o trÃ¹ng cá»™t/báº£ng/index.

CÃ³ thÃªm file sá»­a thá»§ cÃ´ng náº¿u cáº§n:

```text
database\repair_v1.2.2.sql
```

ThÃ´ng thÆ°á»ng **khÃ´ng cáº§n cháº¡y tay** vÃ¬ chÆ°Æ¡ng trÃ¬nh tá»± nÃ¢ng schema khi khá»Ÿi Ä‘á»™ng.

## Äáº©y GitHub + táº¡o Release tá»± Ä‘á»™ng

Repo máº·c Ä‘á»‹nh:

```text
TamNhien/ITDeviceManager
```

### Chuáº©n bá»‹ má»™t láº§n

CÃ i Git, GitHub CLI vÃ  Ä‘Äƒng nháº­p:

```powershell
gh auth login
```

Cáº¥u hÃ¬nh tÃªn/email Git náº¿u mÃ¡y chÆ°a cÃ³:

```powershell
git config --global user.name "TamNhien"
git config --global user.email "EMAIL_GITHUB_CUA_BAN"
```

### Má»™t lá»‡nh release

VÃ­ dá»¥ V1.2.2:

```powershell
.\release.bat 1.2.2
```

Hoáº·c báº£n sau:

```powershell
.\release.bat 1.2.3
```

Script tá»± Ä‘á»™ng:

```text
Cáº­p nháº­t version project
        â†“
Kiá»ƒm tra .env / secrets
        â†“
Restore + build Release
        â†“
Git add + commit
        â†“
Táº¡o GitHub repo náº¿u chÆ°a tá»“n táº¡i
        â†“
Push branch main
        â†“
Build Release win-x64
        â†“
Táº¡o ZIP á»©ng dá»¥ng + ZIP source + SHA256SUMS
        â†“
Táº¡o tag vX.Y.Z
        â†“
Push tag
        â†“
Táº¡o GitHub Release
        â†“
Upload release assets
```

CÃ³ thá»ƒ dÃ¹ng commit message riÃªng:

```powershell
.\release.bat 1.2.3 -Message "NÃ¢ng cáº¥p giao diá»‡n dashboard"
```


Khuyáº¿n nghá»‹ trÃªn mÃ¡y Ä‘á»“ Ã¡n cá»§a báº¡n **khÃ´ng dÃ¹ng `-SkipDatabase`** Ä‘á»ƒ lá»—i schema bá»‹ cháº·n trÆ°á»›c khi push.

## Release assets

Script táº¡o trong `dist\vX.Y.Z\` vÃ  upload lÃªn GitHub Release:

```text
ITDeviceManager-vX.Y.Z-win-x64.zip
ITDeviceManager-vX.Y.Z-source.zip
SHA256SUMS.txt
```

`dist/` Ä‘Ã£ náº±m trong `.gitignore`.

## Báº£o vá»‡ `.env`

File tháº­t Ä‘áº·t táº¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

`.gitignore` cÃ³:

```gitignore
.env
.env.*
!.env.example
```

`release.bat` sáº½ **dá»«ng ngay** náº¿u phÃ¡t hiá»‡n `.env` Ä‘ang bá»‹ Git track, Ä‘á»ƒ trÃ¡nh Ä‘áº©y Gmail App Password lÃªn GitHub.

Máº«u cáº¥u hÃ¬nh:

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

## CÃ´ng nghá»‡

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10.0.12
- SQL Server `CANHTHIEN`
- Argon2id
- MailKit 4.18.0

## Cháº¡y á»©ng dá»¥ng

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\run.bat
```

Hoáº·c:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## Build riÃªng

```powershell
.\clean.bat
.\build.bat
```

## Icon á»©ng dá»¥ng

Äáº·t icon táº¡i:

```text
ITDeviceManager\Assets\App.ico
```

NÃªn chá»©a cÃ¡c kÃ­ch thÆ°á»›c `16x16`, `32x32`, `48x48`, `256x256`.

## Chá»©c nÄƒng hiá»‡n cÃ³

- ÄÄƒng nháº­p / Ä‘Äƒng xuáº¥t.
- Admin / Staff.
- Ghi nhá»› username, khÃ´ng lÆ°u password.
- Hiá»‡n/áº©n password trong Ã´ nháº­p.
- ÄÄƒng kÃ½ tÃ i khoáº£n Staff.
- QuÃªn máº­t kháº©u qua email.
- Reset qua `itdevicemanager://reset-password?...`.
- Token reset 256-bit, dÃ¹ng má»™t láº§n, háº¿t háº¡n 15 phÃºt.
- Argon2id password hashing.
- Dashboard.
- CRUD thiáº¿t bá»‹, loáº¡i thiáº¿t bá»‹, phÃ²ng ban, nhÃ¢n viÃªn, tÃ i khoáº£n.
- Cáº¥p phÃ¡t / thu há»“i thiáº¿t bá»‹.
- TÃ¬m kiáº¿m / lá»c.
- Validation.

## Lá»‹ch sá»­ phiÃªn báº£n

### V1.2.2

- Fix migration SQL Server gÃ¢y `Invalid column name 'Email'` trÃªn database cÅ©.
- TÃ¡ch migration schema thÃ nh nhiá»u SQL command an toÃ n.
- ThÃªm `ITDeviceManager.SelfTest` khÃ´ng phá»¥ thuá»™c test framework bÃªn ngoÃ i.
- ThÃªm `test.bat` / `scripts/test.ps1`.
- Build Release vá»›i warning Ä‘Æ°á»£c coi lÃ  error trong quy trÃ¬nh test.
- ThÃªm database schema self-test.
- ThÃªm `release.bat` / `scripts/release.ps1`.
- Tá»± táº¡o repo `TamNhien/ITDeviceManager` náº¿u chÆ°a tá»“n táº¡i.
- Tá»± commit, push main, tag vÃ  táº¡o GitHub Release.
- Tá»± Ä‘Ã³ng gÃ³i win-x64, source ZIP vÃ  SHA-256 checksums.
- Release bá»‹ cháº·n náº¿u `.env` bá»‹ Git track.

### V1.2.1

- Fix WFO1000 cá»§a custom password input.
- Fix nullable warnings trong log build.
- Tá»± Ä‘á»c `.env`.
- ThÃªm `.env.example` vÃ  báº£o vá»‡ `.env` báº±ng `.gitignore`.

### V1.2.0

- Password eye button trong Ã´ password.
- ÄÄƒng kÃ½ / ghi nhá»› tÃ i khoáº£n / quÃªn máº­t kháº©u qua email.
- Custom reset-password URI.
- ThÃªm `Users.Email` vÃ  `PasswordResetTokens`.

### V1.1.0

- PBKDF2 -> Argon2id.
- Tá»± nÃ¢ng cáº¥p hash cÅ© sau Ä‘Äƒng nháº­p.
- SQL Server máº·c Ä‘á»‹nh `CANHTHIEN`.
