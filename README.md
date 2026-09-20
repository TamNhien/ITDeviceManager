# IT Device Manager - V1.2.6

## V1.2.6 - Registration UI cleanup and removal of SelfTest

- BÃƒÂ¡Ã‚Â»Ã‚Â hoÃƒÆ’Ã‚Â n toÃƒÆ’Ã‚Â n project `ITDeviceManager.SelfTest` khÃƒÂ¡Ã‚Â»Ã‚Âi solution hiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n tÃƒÂ¡Ã‚ÂºÃ‚Â¡i.
- BÃƒÂ¡Ã‚Â»Ã‚Â `test.bat` vÃƒÆ’Ã‚Â  `scripts/test.ps1`; quy trÃƒÆ’Ã‚Â¬nh release khÃƒÆ’Ã‚Â´ng tÃƒÂ¡Ã‚Â»Ã‚Â± chÃƒÂ¡Ã‚ÂºÃ‚Â¡y self-test/database test nÃƒÂ¡Ã‚Â»Ã‚Â¯a.
- `clean.bat` V1.2.6 tÃƒÂ¡Ã‚Â»Ã‚Â± xÃƒÆ’Ã‚Â³a cÃƒÆ’Ã‚Â¡c file/thÃƒâ€ Ã‚Â° mÃƒÂ¡Ã‚Â»Ã‚Â¥c test cÃƒâ€¦Ã‚Â© cÃƒÆ’Ã‚Â²n sÃƒÆ’Ã‚Â³t sau khi copy Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â¨.
- Form Ãƒâ€žÃ‚ÂÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½ bÃƒÂ¡Ã‚Â»Ã‚Â dÃƒÆ’Ã‚Â²ng ghi chÃƒÆ’Ã‚Âº `TÃƒÆ’Ã‚Â i khoÃƒÂ¡Ã‚ÂºÃ‚Â£n tÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½ cÃƒÆ’Ã‚Â³ quyÃƒÂ¡Ã‚Â»Ã‚Ân Staff...` phÃƒÆ’Ã‚Â­a trÃƒÆ’Ã‚Âªn nÃƒÆ’Ã‚Âºt Ãƒâ€žÃ‚ÂÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½ vÃƒÆ’Ã‚Â  thu gÃƒÂ¡Ã‚Â»Ã‚Ân chiÃƒÂ¡Ã‚Â»Ã‚Âu cao form.
- TÃƒÆ’Ã‚Âªn Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng nhÃƒÂ¡Ã‚ÂºÃ‚Â­p cho phÃƒÆ’Ã‚Â©p chÃƒÂ¡Ã‚Â»Ã‚Â¯ Unicode/tiÃƒÂ¡Ã‚ÂºÃ‚Â¿ng ViÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡t, chÃƒÂ¡Ã‚Â»Ã‚Â¯ sÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ, khoÃƒÂ¡Ã‚ÂºÃ‚Â£ng trÃƒÂ¡Ã‚ÂºÃ‚Â¯ng, `.`, `_`, `-`; `Ãƒâ€žÃ‚ÂÃƒÂ¡Ã‚ÂºÃ‚Â¡t Br` lÃƒÆ’Ã‚Â  hÃƒÂ¡Ã‚Â»Ã‚Â£p lÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡.
- ErrorProvider cÃƒÂ¡Ã‚Â»Ã‚Â§a tÃƒÆ’Ã‚Âªn Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng nhÃƒÂ¡Ã‚ÂºÃ‚Â­p tÃƒÂ¡Ã‚Â»Ã‚Â± biÃƒÂ¡Ã‚ÂºÃ‚Â¿n mÃƒÂ¡Ã‚ÂºÃ‚Â¥t ngay khi nÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢i dung Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â£ hÃƒÂ¡Ã‚Â»Ã‚Â£p lÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡.
- GiÃƒÂ¡Ã‚Â»Ã‚Â¯ nguyÃƒÆ’Ã‚Âªn sÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ Ãƒâ€žÃ¢â‚¬ËœiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n thoÃƒÂ¡Ã‚ÂºÃ‚Â¡i, password strength, kiÃƒÂ¡Ã‚Â»Ã†â€™m tra nhÃƒÂ¡Ã‚ÂºÃ‚Â­p lÃƒÂ¡Ã‚ÂºÃ‚Â¡i mÃƒÂ¡Ã‚ÂºÃ‚Â­t khÃƒÂ¡Ã‚ÂºÃ‚Â©u, Argon2id vÃƒÆ’Ã‚Â  Unicode SQL tÃƒÂ¡Ã‚Â»Ã‚Â« V1.2.5.


## V1.2.5 - Registration phone, live password strength, Unicode SQL

- ThÃƒÆ’Ã‚Âªm **SÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ Ãƒâ€žÃ¢â‚¬ËœiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n thoÃƒÂ¡Ã‚ÂºÃ‚Â¡i** vÃƒÆ’Ã‚Â o form Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½ vÃƒÆ’Ã‚Â  bÃƒÂ¡Ã‚ÂºÃ‚Â£ng `Users.PhoneNumber`; tÃƒÂ¡Ã‚Â»Ã‚Â± chuÃƒÂ¡Ã‚ÂºÃ‚Â©n hÃƒÆ’Ã‚Â³a sÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ Ãƒâ€žÃ¢â‚¬ËœiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n thoÃƒÂ¡Ã‚ÂºÃ‚Â¡i, kiÃƒÂ¡Ã‚Â»Ã†â€™m tra hÃƒÂ¡Ã‚Â»Ã‚Â£p lÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡ vÃƒÆ’Ã‚Â  chÃƒÂ¡Ã‚ÂºÃ‚Â·n trÃƒÆ’Ã‚Â¹ng.
- HiÃƒÂ¡Ã‚Â»Ã†â€™n thÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ **Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢ mÃƒÂ¡Ã‚ÂºÃ‚Â¡nh mÃƒÂ¡Ã‚ÂºÃ‚Â­t khÃƒÂ¡Ã‚ÂºÃ‚Â©u theo thÃƒÂ¡Ã‚Â»Ã‚Âi gian thÃƒÂ¡Ã‚Â»Ã‚Â±c** vÃƒÆ’Ã‚Â  tÃƒÂ¡Ã‚Â»Ã‚Â«ng Ãƒâ€žÃ¢â‚¬ËœiÃƒÂ¡Ã‚Â»Ã‚Âu kiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n: tÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœi thiÃƒÂ¡Ã‚Â»Ã†â€™u 12 kÃƒÆ’Ã‚Â½ tÃƒÂ¡Ã‚Â»Ã‚Â±, chÃƒÂ¡Ã‚Â»Ã‚Â¯ hoa, chÃƒÂ¡Ã‚Â»Ã‚Â¯ thÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Âng, sÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ, kÃƒÆ’Ã‚Â½ tÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚ÂºÃ‚Â·c biÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡t.
- KiÃƒÂ¡Ã‚Â»Ã†â€™m tra **mÃƒÂ¡Ã‚ÂºÃ‚Â­t khÃƒÂ¡Ã‚ÂºÃ‚Â©u nhÃƒÂ¡Ã‚ÂºÃ‚Â­p lÃƒÂ¡Ã‚ÂºÃ‚Â¡i trÃƒÆ’Ã‚Â¹ng khÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºp theo thÃƒÂ¡Ã‚Â»Ã‚Âi gian thÃƒÂ¡Ã‚Â»Ã‚Â±c** trÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºc khi bÃƒÂ¡Ã‚ÂºÃ‚Â¥m Ãƒâ€žÃ‚ÂÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½; validation khi lÃƒâ€ Ã‚Â°u vÃƒÂ¡Ã‚ÂºÃ‚Â«n Ãƒâ€žÃ¢â‚¬ËœÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Â£c giÃƒÂ¡Ã‚Â»Ã‚Â¯ ÃƒÂ¡Ã‚Â»Ã…Â¸ tÃƒÂ¡Ã‚ÂºÃ‚Â§ng nghiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡p vÃƒÂ¡Ã‚Â»Ã‚Â¥.
- Password policy toÃƒÆ’Ã‚Â n hÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡ thÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœng Ãƒâ€žÃ¢â‚¬ËœÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Â£c nÃƒÆ’Ã‚Â¢ng tÃƒÂ¡Ã‚Â»Ã‚Â« chÃƒÂ¡Ã‚Â»Ã¢â‚¬Â° kiÃƒÂ¡Ã‚Â»Ã†â€™m tra Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢ dÃƒÆ’Ã‚Â i sang bÃƒÂ¡Ã‚ÂºÃ‚Â¯t buÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢c Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã‚Â§ hoa/thÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Âng/sÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ/kÃƒÆ’Ã‚Â½ tÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚ÂºÃ‚Â·c biÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡t.
- `AppDbContext` Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â¡nh dÃƒÂ¡Ã‚ÂºÃ‚Â¥u rÃƒÆ’Ã‚Âµ cÃƒÆ’Ã‚Â¡c trÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Âng tiÃƒÂ¡Ã‚ÂºÃ‚Â¿ng ViÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡t lÃƒÆ’Ã‚Â  Unicode; schema upgrader tÃƒÂ¡Ã‚Â»Ã‚Â± chuyÃƒÂ¡Ã‚Â»Ã†â€™n cÃƒÆ’Ã‚Â¡c cÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢t user-facing cÃƒâ€¦Ã‚Â© tÃƒÂ¡Ã‚Â»Ã‚Â« `varchar/char/text` sang `nvarchar` Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã†â€™ lÃƒâ€ Ã‚Â°u Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Âºng chÃƒÂ¡Ã‚Â»Ã‚Â¯ cÃƒÆ’Ã‚Â³ dÃƒÂ¡Ã‚ÂºÃ‚Â¥u.
- TÃƒÂ¡Ã‚Â»Ã‚Â± thÃƒÆ’Ã‚Âªm `IX_Users_PhoneNumber` dÃƒÂ¡Ã‚ÂºÃ‚Â¡ng unique filtered index. TÃƒÆ’Ã‚Â i khoÃƒÂ¡Ã‚ÂºÃ‚Â£n cÃƒâ€¦Ã‚Â© (kÃƒÂ¡Ã‚Â»Ã†â€™ cÃƒÂ¡Ã‚ÂºÃ‚Â£ admin) Ãƒâ€žÃ¢â‚¬ËœÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Â£c phÃƒÆ’Ã‚Â©p cÃƒÆ’Ã‚Â³ `PhoneNumber = NULL`.
- CÃƒÂ¡Ã‚ÂºÃ‚Â­p nhÃƒÂ¡Ã‚ÂºÃ‚Â­t quÃƒÂ¡Ã‚ÂºÃ‚Â£n lÃƒÆ’Ã‚Â½ tÃƒÆ’Ã‚Â i khoÃƒÂ¡Ã‚ÂºÃ‚Â£n Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã†â€™ xem/sÃƒÂ¡Ã‚Â»Ã‚Â­a sÃƒÂ¡Ã‚Â»Ã¢â‚¬Ëœ Ãƒâ€žÃ¢â‚¬ËœiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n thoÃƒÂ¡Ã‚ÂºÃ‚Â¡i; tÃƒÂ¡Ã‚Â»Ã‚Â± test kiÃƒÂ¡Ã‚Â»Ã†â€™m tra phone, password policy, Unicode schema vÃƒÆ’Ã‚Â  database V1.2.5.
- **KhÃƒÆ’Ã‚Â´ng Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¢i `DbInitializer.cs` trong gÃƒÆ’Ã‚Â³i upgrade**, nÃƒÆ’Ã‚Âªn mÃƒÂ¡Ã‚ÂºÃ‚Â­t khÃƒÂ¡Ã‚ÂºÃ‚Â©u admin mÃƒÂ¡Ã‚ÂºÃ‚Â·c Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹nh bÃƒÂ¡Ã‚ÂºÃ‚Â¡n Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â£ tÃƒÂ¡Ã‚Â»Ã‚Â± chÃƒÂ¡Ã‚Â»Ã¢â‚¬Â°nh trÃƒÆ’Ã‚Âªn mÃƒÆ’Ã‚Â¡y khÃƒÆ’Ã‚Â´ng bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ ghi Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â¨.

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


**Ãƒâ€žÃ‚ÂÃƒÂ¡Ã‚Â»Ã‚Â tÃƒÆ’Ã‚Â i:** XÃƒÆ’Ã‚Â¢y dÃƒÂ¡Ã‚Â»Ã‚Â±ng phÃƒÂ¡Ã‚ÂºÃ‚Â§n mÃƒÂ¡Ã‚Â»Ã‚Âm quÃƒÂ¡Ã‚ÂºÃ‚Â£n lÃƒÆ’Ã‚Â½ thiÃƒÂ¡Ã‚ÂºÃ‚Â¿t bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ CNTT trong doanh nghiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡p bÃƒÂ¡Ã‚ÂºÃ‚Â±ng C# WinForms vÃƒÆ’Ã‚Â  Entity Framework.

## ThÃƒâ€ Ã‚Â° mÃƒÂ¡Ã‚Â»Ã‚Â¥c lÃƒÆ’Ã‚Â m viÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡c mÃƒÂ¡Ã‚ÂºÃ‚Â·c Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹nh

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
```

V1.2.2 lÃƒÆ’Ã‚Â  bÃƒÂ¡Ã‚ÂºÃ‚Â£n **hotfix + automation**, cÃƒÆ’Ã‚Â³ thÃƒÂ¡Ã‚Â»Ã†â€™ copy Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â¨ lÃƒÆ’Ã‚Âªn V1.2.1 vÃƒÆ’Ã‚Â  giÃƒÂ¡Ã‚Â»Ã‚Â¯ nguyÃƒÆ’Ã‚Âªn database `ITDeviceManagerDb`.

## V1.2.2 Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â£ sÃƒÂ¡Ã‚Â»Ã‚Â­a gÃƒÆ’Ã‚Â¬

### Fix lÃƒÂ¡Ã‚Â»Ã¢â‚¬â€i `Invalid column name 'Email'`

V1.2.1 gÃƒÂ¡Ã‚Â»Ã‚Â­i `ALTER TABLE ... ADD Email` vÃƒÆ’Ã‚Â  `CREATE INDEX ... Email` trong cÃƒÆ’Ã‚Â¹ng mÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢t SQL batch. SQL Server cÃƒÆ’Ã‚Â³ thÃƒÂ¡Ã‚Â»Ã†â€™ biÃƒÆ’Ã‚Âªn dÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ch cÃƒÆ’Ã‚Â¢u `CREATE INDEX` trÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºc khi cÃƒÆ’Ã‚Â¢u `ALTER TABLE` Ãƒâ€žÃ¢â‚¬ËœÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Â£c thÃƒÂ¡Ã‚Â»Ã‚Â±c thi, vÃƒÆ’Ã‚Â¬ vÃƒÂ¡Ã‚ÂºÃ‚Â­y database V1.0/V1.1 bÃƒÆ’Ã‚Â¡o:

```text
Invalid column name 'Email'.
Invalid column name 'Email'.
```

V1.2.2 tÃƒÆ’Ã‚Â¡ch migration thÃƒÆ’Ã‚Â nh cÃƒÆ’Ã‚Â¡c command riÃƒÆ’Ã‚Âªng, chÃƒÂ¡Ã‚ÂºÃ‚Â¡y theo thÃƒÂ¡Ã‚Â»Ã‚Â© tÃƒÂ¡Ã‚Â»Ã‚Â±:

```text
1. TÃƒÂ¡Ã‚ÂºÃ‚Â¡o Users.Email nÃƒÂ¡Ã‚ÂºÃ‚Â¿u chÃƒâ€ Ã‚Â°a cÃƒÆ’Ã‚Â³
2. TÃƒÂ¡Ã‚ÂºÃ‚Â¡o IX_Users_Email nÃƒÂ¡Ã‚ÂºÃ‚Â¿u chÃƒâ€ Ã‚Â°a cÃƒÆ’Ã‚Â³
3. TÃƒÂ¡Ã‚ÂºÃ‚Â¡o PasswordResetTokens nÃƒÂ¡Ã‚ÂºÃ‚Â¿u chÃƒâ€ Ã‚Â°a cÃƒÆ’Ã‚Â³
4. Sau Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â³ mÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºi truy vÃƒÂ¡Ã‚ÂºÃ‚Â¥n Users bÃƒÂ¡Ã‚ÂºÃ‚Â±ng Entity Framework
```

Migration lÃƒÆ’Ã‚Â  **idempotent**: chÃƒÂ¡Ã‚ÂºÃ‚Â¡y lÃƒÂ¡Ã‚ÂºÃ‚Â¡i nhiÃƒÂ¡Ã‚Â»Ã‚Âu lÃƒÂ¡Ã‚ÂºÃ‚Â§n khÃƒÆ’Ã‚Â´ng tÃƒÂ¡Ã‚ÂºÃ‚Â¡o trÃƒÆ’Ã‚Â¹ng cÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢t/bÃƒÂ¡Ã‚ÂºÃ‚Â£ng/index.

CÃƒÆ’Ã‚Â³ thÃƒÆ’Ã‚Âªm file sÃƒÂ¡Ã‚Â»Ã‚Â­a thÃƒÂ¡Ã‚Â»Ã‚Â§ cÃƒÆ’Ã‚Â´ng nÃƒÂ¡Ã‚ÂºÃ‚Â¿u cÃƒÂ¡Ã‚ÂºÃ‚Â§n:

```text
database\repair_v1.2.2.sql
```

ThÃƒÆ’Ã‚Â´ng thÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Âng **khÃƒÆ’Ã‚Â´ng cÃƒÂ¡Ã‚ÂºÃ‚Â§n chÃƒÂ¡Ã‚ÂºÃ‚Â¡y tay** vÃƒÆ’Ã‚Â¬ chÃƒâ€ Ã‚Â°Ãƒâ€ Ã‚Â¡ng trÃƒÆ’Ã‚Â¬nh tÃƒÂ¡Ã‚Â»Ã‚Â± nÃƒÆ’Ã‚Â¢ng schema khi khÃƒÂ¡Ã‚Â»Ã…Â¸i Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢ng.

## Ãƒâ€žÃ‚ÂÃƒÂ¡Ã‚ÂºÃ‚Â©y GitHub + tÃƒÂ¡Ã‚ÂºÃ‚Â¡o Release tÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢ng

Repo mÃƒÂ¡Ã‚ÂºÃ‚Â·c Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹nh:

```text
TamNhien/ITDeviceManager
```

### ChuÃƒÂ¡Ã‚ÂºÃ‚Â©n bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ mÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢t lÃƒÂ¡Ã‚ÂºÃ‚Â§n

CÃƒÆ’Ã‚Â i Git, GitHub CLI vÃƒÆ’Ã‚Â  Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng nhÃƒÂ¡Ã‚ÂºÃ‚Â­p:

```powershell
gh auth login
```

CÃƒÂ¡Ã‚ÂºÃ‚Â¥u hÃƒÆ’Ã‚Â¬nh tÃƒÆ’Ã‚Âªn/email Git nÃƒÂ¡Ã‚ÂºÃ‚Â¿u mÃƒÆ’Ã‚Â¡y chÃƒâ€ Ã‚Â°a cÃƒÆ’Ã‚Â³:

```powershell
git config --global user.name "TamNhien"
git config --global user.email "EMAIL_GITHUB_CUA_BAN"
```

### MÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢t lÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡nh release

VÃƒÆ’Ã‚Â­ dÃƒÂ¡Ã‚Â»Ã‚Â¥ V1.2.2:

```powershell
.\release.bat 1.2.2
```

HoÃƒÂ¡Ã‚ÂºÃ‚Â·c bÃƒÂ¡Ã‚ÂºÃ‚Â£n sau:

```powershell
.\release.bat 1.2.3
```

Script tÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢ng:

```text
CÃƒÂ¡Ã‚ÂºÃ‚Â­p nhÃƒÂ¡Ã‚ÂºÃ‚Â­t version project
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
KiÃƒÂ¡Ã‚Â»Ã†â€™m tra .env / secrets
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
Restore + build Release
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
Git add + commit
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
TÃƒÂ¡Ã‚ÂºÃ‚Â¡o GitHub repo nÃƒÂ¡Ã‚ÂºÃ‚Â¿u chÃƒâ€ Ã‚Â°a tÃƒÂ¡Ã‚Â»Ã¢â‚¬Å“n tÃƒÂ¡Ã‚ÂºÃ‚Â¡i
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
Push branch main
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
Build Release win-x64
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
TÃƒÂ¡Ã‚ÂºÃ‚Â¡o ZIP ÃƒÂ¡Ã‚Â»Ã‚Â©ng dÃƒÂ¡Ã‚Â»Ã‚Â¥ng + ZIP source + SHA256SUMS
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
TÃƒÂ¡Ã‚ÂºÃ‚Â¡o tag vX.Y.Z
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
Push tag
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
TÃƒÂ¡Ã‚ÂºÃ‚Â¡o GitHub Release
        ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬Å“
Upload release assets
```

CÃƒÆ’Ã‚Â³ thÃƒÂ¡Ã‚Â»Ã†â€™ dÃƒÆ’Ã‚Â¹ng commit message riÃƒÆ’Ã‚Âªng:

```powershell
.\release.bat 1.2.3 -Message "NÃƒÆ’Ã‚Â¢ng cÃƒÂ¡Ã‚ÂºÃ‚Â¥p giao diÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n dashboard"
```


KhuyÃƒÂ¡Ã‚ÂºÃ‚Â¿n nghÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ trÃƒÆ’Ã‚Âªn mÃƒÆ’Ã‚Â¡y Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â‚¬Å“ ÃƒÆ’Ã‚Â¡n cÃƒÂ¡Ã‚Â»Ã‚Â§a bÃƒÂ¡Ã‚ÂºÃ‚Â¡n **khÃƒÆ’Ã‚Â´ng dÃƒÆ’Ã‚Â¹ng `-SkipDatabase`** Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã†â€™ lÃƒÂ¡Ã‚Â»Ã¢â‚¬â€i schema bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ chÃƒÂ¡Ã‚ÂºÃ‚Â·n trÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºc khi push.

## Release assets

Script tÃƒÂ¡Ã‚ÂºÃ‚Â¡o trong `dist\vX.Y.Z\` vÃƒÆ’Ã‚Â  upload lÃƒÆ’Ã‚Âªn GitHub Release:

```text
ITDeviceManager-vX.Y.Z-win-x64.zip
ITDeviceManager-vX.Y.Z-source.zip
SHA256SUMS.txt
```

`dist/` Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â£ nÃƒÂ¡Ã‚ÂºÃ‚Â±m trong `.gitignore`.

## BÃƒÂ¡Ã‚ÂºÃ‚Â£o vÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡ `.env`

File thÃƒÂ¡Ã‚ÂºÃ‚Â­t Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚ÂºÃ‚Â·t tÃƒÂ¡Ã‚ÂºÃ‚Â¡i:

```text
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager\.env
```

`.gitignore` cÃƒÆ’Ã‚Â³:

```gitignore
.env
.env.*
!.env.example
```

`release.bat` sÃƒÂ¡Ã‚ÂºÃ‚Â½ **dÃƒÂ¡Ã‚Â»Ã‚Â«ng ngay** nÃƒÂ¡Ã‚ÂºÃ‚Â¿u phÃƒÆ’Ã‚Â¡t hiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n `.env` Ãƒâ€žÃ¢â‚¬Ëœang bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ Git track, Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã†â€™ trÃƒÆ’Ã‚Â¡nh Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚ÂºÃ‚Â©y Gmail App Password lÃƒÆ’Ã‚Âªn GitHub.

MÃƒÂ¡Ã‚ÂºÃ‚Â«u cÃƒÂ¡Ã‚ÂºÃ‚Â¥u hÃƒÆ’Ã‚Â¬nh:

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

## CÃƒÆ’Ã‚Â´ng nghÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡

- C# 14
- .NET 10 LTS (`net10.0-windows`)
- Windows Forms
- Entity Framework Core 10.0.12
- SQL Server `CANHTHIEN`
- Argon2id
- MailKit 4.18.0

## ChÃƒÂ¡Ã‚ÂºÃ‚Â¡y ÃƒÂ¡Ã‚Â»Ã‚Â©ng dÃƒÂ¡Ã‚Â»Ã‚Â¥ng

```powershell
cd D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
.\run.bat
```

HoÃƒÂ¡Ã‚ÂºÃ‚Â·c:

```powershell
dotnet run --project .\ITDeviceManager\ITDeviceManager.csproj
```

## Build riÃƒÆ’Ã‚Âªng

```powershell
.\clean.bat
.\build.bat
```

## Icon ÃƒÂ¡Ã‚Â»Ã‚Â©ng dÃƒÂ¡Ã‚Â»Ã‚Â¥ng

Ãƒâ€žÃ‚ÂÃƒÂ¡Ã‚ÂºÃ‚Â·t icon tÃƒÂ¡Ã‚ÂºÃ‚Â¡i:

```text
ITDeviceManager\Assets\App.ico
```

NÃƒÆ’Ã‚Âªn chÃƒÂ¡Ã‚Â»Ã‚Â©a cÃƒÆ’Ã‚Â¡c kÃƒÆ’Ã‚Â­ch thÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºc `16x16`, `32x32`, `48x48`, `256x256`.

## ChÃƒÂ¡Ã‚Â»Ã‚Â©c nÃƒâ€žÃ†â€™ng hiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n cÃƒÆ’Ã‚Â³

- Ãƒâ€žÃ‚ÂÃƒâ€žÃ†â€™ng nhÃƒÂ¡Ã‚ÂºÃ‚Â­p / Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng xuÃƒÂ¡Ã‚ÂºÃ‚Â¥t.
- Admin / Staff.
- Ghi nhÃƒÂ¡Ã‚Â»Ã¢â‚¬Âº username, khÃƒÆ’Ã‚Â´ng lÃƒâ€ Ã‚Â°u password.
- HiÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡n/ÃƒÂ¡Ã‚ÂºÃ‚Â©n password trong ÃƒÆ’Ã‚Â´ nhÃƒÂ¡Ã‚ÂºÃ‚Â­p.
- Ãƒâ€žÃ‚ÂÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½ tÃƒÆ’Ã‚Â i khoÃƒÂ¡Ã‚ÂºÃ‚Â£n Staff.
- QuÃƒÆ’Ã‚Âªn mÃƒÂ¡Ã‚ÂºÃ‚Â­t khÃƒÂ¡Ã‚ÂºÃ‚Â©u qua email.
- Reset qua `itdevicemanager://reset-password?...`.
- Token reset 256-bit, dÃƒÆ’Ã‚Â¹ng mÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢t lÃƒÂ¡Ã‚ÂºÃ‚Â§n, hÃƒÂ¡Ã‚ÂºÃ‚Â¿t hÃƒÂ¡Ã‚ÂºÃ‚Â¡n 15 phÃƒÆ’Ã‚Âºt.
- Argon2id password hashing.
- Dashboard.
- CRUD thiÃƒÂ¡Ã‚ÂºÃ‚Â¿t bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹, loÃƒÂ¡Ã‚ÂºÃ‚Â¡i thiÃƒÂ¡Ã‚ÂºÃ‚Â¿t bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹, phÃƒÆ’Ã‚Â²ng ban, nhÃƒÆ’Ã‚Â¢n viÃƒÆ’Ã‚Âªn, tÃƒÆ’Ã‚Â i khoÃƒÂ¡Ã‚ÂºÃ‚Â£n.
- CÃƒÂ¡Ã‚ÂºÃ‚Â¥p phÃƒÆ’Ã‚Â¡t / thu hÃƒÂ¡Ã‚Â»Ã¢â‚¬Å“i thiÃƒÂ¡Ã‚ÂºÃ‚Â¿t bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹.
- TÃƒÆ’Ã‚Â¬m kiÃƒÂ¡Ã‚ÂºÃ‚Â¿m / lÃƒÂ¡Ã‚Â»Ã‚Âc.
- Validation.

## LÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ch sÃƒÂ¡Ã‚Â»Ã‚Â­ phiÃƒÆ’Ã‚Âªn bÃƒÂ¡Ã‚ÂºÃ‚Â£n

### V1.2.2

- Fix migration SQL Server gÃƒÆ’Ã‚Â¢y `Invalid column name 'Email'` trÃƒÆ’Ã‚Âªn database cÃƒâ€¦Ã‚Â©.
- TÃƒÆ’Ã‚Â¡ch migration schema thÃƒÆ’Ã‚Â nh nhiÃƒÂ¡Ã‚Â»Ã‚Âu SQL command an toÃƒÆ’Ã‚Â n.
- ThÃƒÆ’Ã‚Âªm `ITDeviceManager.SelfTest` khÃƒÆ’Ã‚Â´ng phÃƒÂ¡Ã‚Â»Ã‚Â¥ thuÃƒÂ¡Ã‚Â»Ã¢â€žÂ¢c test framework bÃƒÆ’Ã‚Âªn ngoÃƒÆ’Ã‚Â i.
- ThÃƒÆ’Ã‚Âªm `test.bat` / `scripts/test.ps1`.
- Build Release vÃƒÂ¡Ã‚Â»Ã¢â‚¬Âºi warning Ãƒâ€žÃ¢â‚¬ËœÃƒâ€ Ã‚Â°ÃƒÂ¡Ã‚Â»Ã‚Â£c coi lÃƒÆ’Ã‚Â  error trong quy trÃƒÆ’Ã‚Â¬nh test.
- ThÃƒÆ’Ã‚Âªm database schema self-test.
- ThÃƒÆ’Ã‚Âªm `release.bat` / `scripts/release.ps1`.
- TÃƒÂ¡Ã‚Â»Ã‚Â± tÃƒÂ¡Ã‚ÂºÃ‚Â¡o repo `TamNhien/ITDeviceManager` nÃƒÂ¡Ã‚ÂºÃ‚Â¿u chÃƒâ€ Ã‚Â°a tÃƒÂ¡Ã‚Â»Ã¢â‚¬Å“n tÃƒÂ¡Ã‚ÂºÃ‚Â¡i.
- TÃƒÂ¡Ã‚Â»Ã‚Â± commit, push main, tag vÃƒÆ’Ã‚Â  tÃƒÂ¡Ã‚ÂºÃ‚Â¡o GitHub Release.
- TÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒÆ’Ã‚Â³ng gÃƒÆ’Ã‚Â³i win-x64, source ZIP vÃƒÆ’Ã‚Â  SHA-256 checksums.
- Release bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ chÃƒÂ¡Ã‚ÂºÃ‚Â·n nÃƒÂ¡Ã‚ÂºÃ‚Â¿u `.env` bÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹ Git track.

### V1.2.1

- Fix WFO1000 cÃƒÂ¡Ã‚Â»Ã‚Â§a custom password input.
- Fix nullable warnings trong log build.
- TÃƒÂ¡Ã‚Â»Ã‚Â± Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã‚Âc `.env`.
- ThÃƒÆ’Ã‚Âªm `.env.example` vÃƒÆ’Ã‚Â  bÃƒÂ¡Ã‚ÂºÃ‚Â£o vÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¡ `.env` bÃƒÂ¡Ã‚ÂºÃ‚Â±ng `.gitignore`.

### V1.2.0

- Password eye button trong ÃƒÆ’Ã‚Â´ password.
- Ãƒâ€žÃ‚ÂÃƒâ€žÃ†â€™ng kÃƒÆ’Ã‚Â½ / ghi nhÃƒÂ¡Ã‚Â»Ã¢â‚¬Âº tÃƒÆ’Ã‚Â i khoÃƒÂ¡Ã‚ÂºÃ‚Â£n / quÃƒÆ’Ã‚Âªn mÃƒÂ¡Ã‚ÂºÃ‚Â­t khÃƒÂ¡Ã‚ÂºÃ‚Â©u qua email.
- Custom reset-password URI.
- ThÃƒÆ’Ã‚Âªm `Users.Email` vÃƒÆ’Ã‚Â  `PasswordResetTokens`.

### V1.1.0

- PBKDF2 -> Argon2id.
- TÃƒÂ¡Ã‚Â»Ã‚Â± nÃƒÆ’Ã‚Â¢ng cÃƒÂ¡Ã‚ÂºÃ‚Â¥p hash cÃƒâ€¦Ã‚Â© sau Ãƒâ€žÃ¢â‚¬ËœÃƒâ€žÃ†â€™ng nhÃƒÂ¡Ã‚ÂºÃ‚Â­p.
- SQL Server mÃƒÂ¡Ã‚ÂºÃ‚Â·c Ãƒâ€žÃ¢â‚¬ËœÃƒÂ¡Ã‚Â»Ã¢â‚¬Â¹nh `CANHTHIEN`.
