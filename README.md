# IT Device Manager - V1.2.6

## V1.2.6 - Registration UI cleanup and removal of SelfTest

- BÃ¡Â»Â hoÃƒÂ n toÃƒÂ n project `ITDeviceManager.SelfTest` khÃ¡Â»Âi solution hiÃ¡Â»â€¡n tÃ¡ÂºÂ¡i.
- BÃ¡Â»Â `test.bat` vÃƒÂ  `scripts/test.ps1`; quy trÃƒÂ¬nh release khÃƒÂ´ng tÃ¡Â»Â± chÃ¡ÂºÂ¡y self-test/database test nÃ¡Â»Â¯a.
- `clean.bat` V1.2.6 tÃ¡Â»Â± xÃƒÂ³a cÃƒÂ¡c file/thÃ†Â° mÃ¡Â»Â¥c test cÃ…Â© cÃƒÂ²n sÃƒÂ³t sau khi copy Ã„â€˜ÃƒÂ¨.
- Form Ã„ÂÃ„Æ’ng kÃƒÂ½ bÃ¡Â»Â dÃƒÂ²ng ghi chÃƒÂº `TÃƒÂ i khoÃ¡ÂºÂ£n tÃ¡Â»Â± Ã„â€˜Ã„Æ’ng kÃƒÂ½ cÃƒÂ³ quyÃ¡Â»Ân Staff...` phÃƒÂ­a trÃƒÂªn nÃƒÂºt Ã„ÂÃ„Æ’ng kÃƒÂ½ vÃƒÂ  thu gÃ¡Â»Ân chiÃ¡Â»Âu cao form.
- TÃƒÂªn Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p cho phÃƒÂ©p chÃ¡Â»Â¯ Unicode/tiÃ¡ÂºÂ¿ng ViÃ¡Â»â€¡t, chÃ¡Â»Â¯ sÃ¡Â»â€˜, khoÃ¡ÂºÂ£ng trÃ¡ÂºÂ¯ng, `.`, `_`, `-`; `Ã„ÂÃ¡ÂºÂ¡t Br` lÃƒÂ  hÃ¡Â»Â£p lÃ¡Â»â€¡.
- ErrorProvider cÃ¡Â»Â§a tÃƒÂªn Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p tÃ¡Â»Â± biÃ¡ÂºÂ¿n mÃ¡ÂºÂ¥t ngay khi nÃ¡Â»â„¢i dung Ã„â€˜ÃƒÂ£ hÃ¡Â»Â£p lÃ¡Â»â€¡.
- GiÃ¡Â»Â¯ nguyÃƒÂªn sÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i, password strength, kiÃ¡Â»Æ’m tra nhÃ¡ÂºÂ­p lÃ¡ÂºÂ¡i mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u, Argon2id vÃƒÂ  Unicode SQL tÃ¡Â»Â« V1.2.5.


## V1.2.5 - Registration phone, live password strength, Unicode SQL

- ThÃƒÂªm **SÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i** vÃƒÂ o form Ã„â€˜Ã„Æ’ng kÃƒÂ½ vÃƒÂ  bÃ¡ÂºÂ£ng `Users.PhoneNumber`; tÃ¡Â»Â± chuÃ¡ÂºÂ©n hÃƒÂ³a sÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i, kiÃ¡Â»Æ’m tra hÃ¡Â»Â£p lÃ¡Â»â€¡ vÃƒÂ  chÃ¡ÂºÂ·n trÃƒÂ¹ng.
- HiÃ¡Â»Æ’n thÃ¡Â»â€¹ **Ã„â€˜Ã¡Â»â„¢ mÃ¡ÂºÂ¡nh mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u theo thÃ¡Â»Âi gian thÃ¡Â»Â±c** vÃƒÂ  tÃ¡Â»Â«ng Ã„â€˜iÃ¡Â»Âu kiÃ¡Â»â€¡n: tÃ¡Â»â€˜i thiÃ¡Â»Æ’u 12 kÃƒÂ½ tÃ¡Â»Â±, chÃ¡Â»Â¯ hoa, chÃ¡Â»Â¯ thÃ†Â°Ã¡Â»Âng, sÃ¡Â»â€˜, kÃƒÂ½ tÃ¡Â»Â± Ã„â€˜Ã¡ÂºÂ·c biÃ¡Â»â€¡t.
- KiÃ¡Â»Æ’m tra **mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u nhÃ¡ÂºÂ­p lÃ¡ÂºÂ¡i trÃƒÂ¹ng khÃ¡Â»â€ºp theo thÃ¡Â»Âi gian thÃ¡Â»Â±c** trÃ†Â°Ã¡Â»â€ºc khi bÃ¡ÂºÂ¥m Ã„ÂÃ„Æ’ng kÃƒÂ½; validation khi lÃ†Â°u vÃ¡ÂºÂ«n Ã„â€˜Ã†Â°Ã¡Â»Â£c giÃ¡Â»Â¯ Ã¡Â»Å¸ tÃ¡ÂºÂ§ng nghiÃ¡Â»â€¡p vÃ¡Â»Â¥.
- Password policy toÃƒÂ n hÃ¡Â»â€¡ thÃ¡Â»â€˜ng Ã„â€˜Ã†Â°Ã¡Â»Â£c nÃƒÂ¢ng tÃ¡Â»Â« chÃ¡Â»â€° kiÃ¡Â»Æ’m tra Ã„â€˜Ã¡Â»â„¢ dÃƒÂ i sang bÃ¡ÂºÂ¯t buÃ¡Â»â„¢c Ã„â€˜Ã¡Â»Â§ hoa/thÃ†Â°Ã¡Â»Âng/sÃ¡Â»â€˜/kÃƒÂ½ tÃ¡Â»Â± Ã„â€˜Ã¡ÂºÂ·c biÃ¡Â»â€¡t.
- `AppDbContext` Ã„â€˜ÃƒÂ¡nh dÃ¡ÂºÂ¥u rÃƒÂµ cÃƒÂ¡c trÃ†Â°Ã¡Â»Âng tiÃ¡ÂºÂ¿ng ViÃ¡Â»â€¡t lÃƒÂ  Unicode; schema upgrader tÃ¡Â»Â± chuyÃ¡Â»Æ’n cÃƒÂ¡c cÃ¡Â»â„¢t user-facing cÃ…Â© tÃ¡Â»Â« `varchar/char/text` sang `nvarchar` Ã„â€˜Ã¡Â»Æ’ lÃ†Â°u Ã„â€˜ÃƒÂºng chÃ¡Â»Â¯ cÃƒÂ³ dÃ¡ÂºÂ¥u.
- TÃ¡Â»Â± thÃƒÂªm `IX_Users_PhoneNumber` dÃ¡ÂºÂ¡ng unique filtered index. TÃƒÂ i khoÃ¡ÂºÂ£n cÃ…Â© (kÃ¡Â»Æ’ cÃ¡ÂºÂ£ admin) Ã„â€˜Ã†Â°Ã¡Â»Â£c phÃƒÂ©p cÃƒÂ³ `PhoneNumber = NULL`.
- CÃ¡ÂºÂ­p nhÃ¡ÂºÂ­t quÃ¡ÂºÂ£n lÃƒÂ½ tÃƒÂ i khoÃ¡ÂºÂ£n Ã„â€˜Ã¡Â»Æ’ xem/sÃ¡Â»Â­a sÃ¡Â»â€˜ Ã„â€˜iÃ¡Â»â€¡n thoÃ¡ÂºÂ¡i; tÃ¡Â»Â± test kiÃ¡Â»Æ’m tra phone, password policy, Unicode schema vÃƒÂ  database V1.2.5.
- **KhÃƒÂ´ng Ã„â€˜Ã¡Â»â€¢i `DbInitializer.cs` trong gÃƒÂ³i upgrade**, nÃƒÂªn mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u admin mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh bÃ¡ÂºÂ¡n Ã„â€˜ÃƒÂ£ tÃ¡Â»Â± chÃ¡Â»â€°nh trÃƒÂªn mÃƒÂ¡y khÃƒÂ´ng bÃ¡Â»â€¹ ghi Ã„â€˜ÃƒÂ¨.

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


**Ã„ÂÃ¡Â»Â tÃƒÂ i:** XÃƒÂ¢y dÃ¡Â»Â±ng phÃ¡ÂºÂ§n mÃ¡Â»Âm quÃ¡ÂºÂ£n lÃƒÂ½ thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹ CNTT trong doanh nghiÃ¡Â»â€¡p bÃ¡ÂºÂ±ng C# WinForms vÃƒÂ  Entity Framework.

## ThÃ†Â° mÃ¡Â»Â¥c lÃƒÂ m viÃ¡Â»â€¡c mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

V1.2.2 lÃƒÂ  bÃ¡ÂºÂ£n **hotfix + automation**, cÃƒÂ³ thÃ¡Â»Æ’ copy Ã„â€˜ÃƒÂ¨ lÃƒÂªn V1.2.1 vÃƒÂ  giÃ¡Â»Â¯ nguyÃƒÂªn database `ITDeviceManagerDb`.

## V1.2.2 Ã„â€˜ÃƒÂ£ sÃ¡Â»Â­a gÃƒÂ¬

### Fix lÃ¡Â»â€”i `Invalid column name 'Email'`

V1.2.1 gÃ¡Â»Â­i `ALTER TABLE ... ADD Email` vÃƒÂ  `CREATE INDEX ... Email` trong cÃƒÂ¹ng mÃ¡Â»â„¢t SQL batch. SQL Server cÃƒÂ³ thÃ¡Â»Æ’ biÃƒÂªn dÃ¡Â»â€¹ch cÃƒÂ¢u `CREATE INDEX` trÃ†Â°Ã¡Â»â€ºc khi cÃƒÂ¢u `ALTER TABLE` Ã„â€˜Ã†Â°Ã¡Â»Â£c thÃ¡Â»Â±c thi, vÃƒÂ¬ vÃ¡ÂºÂ­y database V1.0/V1.1 bÃƒÂ¡o:

```text
Invalid column name 'Email'.
Invalid column name 'Email'.
```

V1.2.2 tÃƒÂ¡ch migration thÃƒÂ nh cÃƒÂ¡c command riÃƒÂªng, chÃ¡ÂºÂ¡y theo thÃ¡Â»Â© tÃ¡Â»Â±:

```text
1. TÃ¡ÂºÂ¡o Users.Email nÃ¡ÂºÂ¿u chÃ†Â°a cÃƒÂ³
2. TÃ¡ÂºÂ¡o IX_Users_Email nÃ¡ÂºÂ¿u chÃ†Â°a cÃƒÂ³
3. TÃ¡ÂºÂ¡o PasswordResetTokens nÃ¡ÂºÂ¿u chÃ†Â°a cÃƒÂ³
4. Sau Ã„â€˜ÃƒÂ³ mÃ¡Â»â€ºi truy vÃ¡ÂºÂ¥n Users bÃ¡ÂºÂ±ng Entity Framework
```

Migration lÃƒÂ  **idempotent**: chÃ¡ÂºÂ¡y lÃ¡ÂºÂ¡i nhiÃ¡Â»Âu lÃ¡ÂºÂ§n khÃƒÂ´ng tÃ¡ÂºÂ¡o trÃƒÂ¹ng cÃ¡Â»â„¢t/bÃ¡ÂºÂ£ng/index.

CÃƒÂ³ thÃƒÂªm file sÃ¡Â»Â­a thÃ¡Â»Â§ cÃƒÂ´ng nÃ¡ÂºÂ¿u cÃ¡ÂºÂ§n:

```text
database\repair_v1.2.2.sql
```

ThÃƒÂ´ng thÃ†Â°Ã¡Â»Âng **khÃƒÂ´ng cÃ¡ÂºÂ§n chÃ¡ÂºÂ¡y tay** vÃƒÂ¬ chÃ†Â°Ã†Â¡ng trÃƒÂ¬nh tÃ¡Â»Â± nÃƒÂ¢ng schema khi khÃ¡Â»Å¸i Ã„â€˜Ã¡Â»â„¢ng.

## Ã„ÂÃ¡ÂºÂ©y GitHub + tÃ¡ÂºÂ¡o Release tÃ¡Â»Â± Ã„â€˜Ã¡Â»â„¢ng

Repo mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh:

```text
TamNhien/ITDeviceManager
```

### ChuÃ¡ÂºÂ©n bÃ¡Â»â€¹ mÃ¡Â»â„¢t lÃ¡ÂºÂ§n

CÃƒÂ i Git, GitHub CLI vÃƒÂ  Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p:

```powershell
gh auth login
```

CÃ¡ÂºÂ¥u hÃƒÂ¬nh tÃƒÂªn/email Git nÃ¡ÂºÂ¿u mÃƒÂ¡y chÃ†Â°a cÃƒÂ³:

```powershell
git config --global user.name "TamNhien"
git config --global user.email "EMAIL_GITHUB_CUA_BAN"
```

### MÃ¡Â»â„¢t lÃ¡Â»â€¡nh release

VÃƒÂ­ dÃ¡Â»Â¥ V1.2.2:

```powershell
.\release.bat 1.2.2
```

HoÃ¡ÂºÂ·c bÃ¡ÂºÂ£n sau:

```powershell
.\release.bat 1.2.3
```

Script tÃ¡Â»Â± Ã„â€˜Ã¡Â»â„¢ng:

```text
CÃ¡ÂºÂ­p nhÃ¡ÂºÂ­t version project
        Ã¢â€ â€œ
KiÃ¡Â»Æ’m tra .env / secrets
        Ã¢â€ â€œ
Restore + build Release
        Ã¢â€ â€œ
Git add + commit
        Ã¢â€ â€œ
TÃ¡ÂºÂ¡o GitHub repo nÃ¡ÂºÂ¿u chÃ†Â°a tÃ¡Â»â€œn tÃ¡ÂºÂ¡i
        Ã¢â€ â€œ
Push branch main
        Ã¢â€ â€œ
Build Release win-x64
        Ã¢â€ â€œ
TÃ¡ÂºÂ¡o ZIP Ã¡Â»Â©ng dÃ¡Â»Â¥ng + ZIP source + SHA256SUMS
        Ã¢â€ â€œ
TÃ¡ÂºÂ¡o tag vX.Y.Z
        Ã¢â€ â€œ
Push tag
        Ã¢â€ â€œ
TÃ¡ÂºÂ¡o GitHub Release
        Ã¢â€ â€œ
Upload release assets
```

CÃƒÂ³ thÃ¡Â»Æ’ dÃƒÂ¹ng commit message riÃƒÂªng:

```powershell
.\release.bat 1.2.3 -Message "NÃƒÂ¢ng cÃ¡ÂºÂ¥p giao diÃ¡Â»â€¡n dashboard"
```


KhuyÃ¡ÂºÂ¿n nghÃ¡Â»â€¹ trÃƒÂªn mÃƒÂ¡y Ã„â€˜Ã¡Â»â€œ ÃƒÂ¡n cÃ¡Â»Â§a bÃ¡ÂºÂ¡n **khÃƒÂ´ng dÃƒÂ¹ng `-SkipDatabase`** Ã„â€˜Ã¡Â»Æ’ lÃ¡Â»â€”i schema bÃ¡Â»â€¹ chÃ¡ÂºÂ·n trÃ†Â°Ã¡Â»â€ºc khi push.

## Release assets

Script tÃ¡ÂºÂ¡o trong `dist\vX.Y.Z\` vÃƒÂ  upload lÃƒÂªn GitHub Release:

```text
ITDeviceManager-vX.Y.Z-win-x64.zip
ITDeviceManager-vX.Y.Z-source.zip
SHA256SUMS.txt
```

`dist/` Ã„â€˜ÃƒÂ£ nÃ¡ÂºÂ±m trong `.gitignore`.

## BÃ¡ÂºÂ£o vÃ¡Â»â€¡ `.env`

File thÃ¡ÂºÂ­t Ã„â€˜Ã¡ÂºÂ·t tÃ¡ÂºÂ¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

`.gitignore` cÃƒÂ³:

```gitignore
.env
.env.*
!.env.example
```

`release.bat` sÃ¡ÂºÂ½ **dÃ¡Â»Â«ng ngay** nÃ¡ÂºÂ¿u phÃƒÂ¡t hiÃ¡Â»â€¡n `.env` Ã„â€˜ang bÃ¡Â»â€¹ Git track, Ã„â€˜Ã¡Â»Æ’ trÃƒÂ¡nh Ã„â€˜Ã¡ÂºÂ©y Gmail App Password lÃƒÂªn GitHub.

MÃ¡ÂºÂ«u cÃ¡ÂºÂ¥u hÃƒÂ¬nh:

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

## CÃƒÂ´ng nghÃ¡Â»â€¡

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10.0.12
- SQL Server `CANHTHIEN`
- Argon2id
- MailKit 4.18.0

## ChÃ¡ÂºÂ¡y Ã¡Â»Â©ng dÃ¡Â»Â¥ng

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\run.bat
```

HoÃ¡ÂºÂ·c:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## Build riÃƒÂªng

```powershell
.\clean.bat
.\build.bat
```

## Icon Ã¡Â»Â©ng dÃ¡Â»Â¥ng

Ã„ÂÃ¡ÂºÂ·t icon tÃ¡ÂºÂ¡i:

```text
ITDeviceManager\Assets\App.ico
```

NÃƒÂªn chÃ¡Â»Â©a cÃƒÂ¡c kÃƒÂ­ch thÃ†Â°Ã¡Â»â€ºc `16x16`, `32x32`, `48x48`, `256x256`.

## ChÃ¡Â»Â©c nÃ„Æ’ng hiÃ¡Â»â€¡n cÃƒÂ³

- Ã„ÂÃ„Æ’ng nhÃ¡ÂºÂ­p / Ã„â€˜Ã„Æ’ng xuÃ¡ÂºÂ¥t.
- Admin / Staff.
- Ghi nhÃ¡Â»â€º username, khÃƒÂ´ng lÃ†Â°u password.
- HiÃ¡Â»â€¡n/Ã¡ÂºÂ©n password trong ÃƒÂ´ nhÃ¡ÂºÂ­p.
- Ã„ÂÃ„Æ’ng kÃƒÂ½ tÃƒÂ i khoÃ¡ÂºÂ£n Staff.
- QuÃƒÂªn mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u qua email.
- Reset qua `itdevicemanager://reset-password?...`.
- Token reset 256-bit, dÃƒÂ¹ng mÃ¡Â»â„¢t lÃ¡ÂºÂ§n, hÃ¡ÂºÂ¿t hÃ¡ÂºÂ¡n 15 phÃƒÂºt.
- Argon2id password hashing.
- Dashboard.
- CRUD thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, loÃ¡ÂºÂ¡i thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹, phÃƒÂ²ng ban, nhÃƒÂ¢n viÃƒÂªn, tÃƒÂ i khoÃ¡ÂºÂ£n.
- CÃ¡ÂºÂ¥p phÃƒÂ¡t / thu hÃ¡Â»â€œi thiÃ¡ÂºÂ¿t bÃ¡Â»â€¹.
- TÃƒÂ¬m kiÃ¡ÂºÂ¿m / lÃ¡Â»Âc.
- Validation.

## LÃ¡Â»â€¹ch sÃ¡Â»Â­ phiÃƒÂªn bÃ¡ÂºÂ£n

### V1.2.2

- Fix migration SQL Server gÃƒÂ¢y `Invalid column name 'Email'` trÃƒÂªn database cÃ…Â©.
- TÃƒÂ¡ch migration schema thÃƒÂ nh nhiÃ¡Â»Âu SQL command an toÃƒÂ n.
- ThÃƒÂªm `ITDeviceManager.SelfTest` khÃƒÂ´ng phÃ¡Â»Â¥ thuÃ¡Â»â„¢c test framework bÃƒÂªn ngoÃƒÂ i.
- ThÃƒÂªm `test.bat` / `scripts/test.ps1`.
- Build Release vÃ¡Â»â€ºi warning Ã„â€˜Ã†Â°Ã¡Â»Â£c coi lÃƒÂ  error trong quy trÃƒÂ¬nh test.
- ThÃƒÂªm database schema self-test.
- ThÃƒÂªm `release.bat` / `scripts/release.ps1`.
- TÃ¡Â»Â± tÃ¡ÂºÂ¡o repo `TamNhien/ITDeviceManager` nÃ¡ÂºÂ¿u chÃ†Â°a tÃ¡Â»â€œn tÃ¡ÂºÂ¡i.
- TÃ¡Â»Â± commit, push main, tag vÃƒÂ  tÃ¡ÂºÂ¡o GitHub Release.
- TÃ¡Â»Â± Ã„â€˜ÃƒÂ³ng gÃƒÂ³i win-x64, source ZIP vÃƒÂ  SHA-256 checksums.
- Release bÃ¡Â»â€¹ chÃ¡ÂºÂ·n nÃ¡ÂºÂ¿u `.env` bÃ¡Â»â€¹ Git track.

### V1.2.1

- Fix WFO1000 cÃ¡Â»Â§a custom password input.
- Fix nullable warnings trong log build.
- TÃ¡Â»Â± Ã„â€˜Ã¡Â»Âc `.env`.
- ThÃƒÂªm `.env.example` vÃƒÂ  bÃ¡ÂºÂ£o vÃ¡Â»â€¡ `.env` bÃ¡ÂºÂ±ng `.gitignore`.

### V1.2.0

- Password eye button trong ÃƒÂ´ password.
- Ã„ÂÃ„Æ’ng kÃƒÂ½ / ghi nhÃ¡Â»â€º tÃƒÂ i khoÃ¡ÂºÂ£n / quÃƒÂªn mÃ¡ÂºÂ­t khÃ¡ÂºÂ©u qua email.
- Custom reset-password URI.
- ThÃƒÂªm `Users.Email` vÃƒÂ  `PasswordResetTokens`.

### V1.1.0

- PBKDF2 -> Argon2id.
- TÃ¡Â»Â± nÃƒÂ¢ng cÃ¡ÂºÂ¥p hash cÃ…Â© sau Ã„â€˜Ã„Æ’ng nhÃ¡ÂºÂ­p.
- SQL Server mÃ¡ÂºÂ·c Ã„â€˜Ã¡Â»â€¹nh `CANHTHIEN`.
