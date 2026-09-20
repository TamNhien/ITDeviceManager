# ITDeviceManager V1.2.7 - Password reset link hotfix

## Why the Gmail button did not open

The old email button used `itdevicemanager://...` directly. Gmail and many webmail clients block or strip custom URI schemes in HTML email for security, so the button can be visible but not launch the app.

## V1.2.7 fix

The email now links to an HTTPS GitHub Pages bridge:

`https://tamnhien.github.io/ITDeviceManager/reset-password.html#token=...`

The token is stored after `#`, so it is not sent to GitHub Pages in the HTTP request. The bridge page then opens:

`itdevicemanager://reset-password?token=...`

The page also provides an explicit **Mở IT Device Manager** button if the browser requires a user confirmation.

## Apply

Copy this upgrade over:

`D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager`

Then run:

```powershell
.\clean.bat
.\build.bat
```

Publish GitHub Pages and release the source with:

```powershell
.\release.bat 1.2.7
```

The release script pushes `docs/reset-password.html` and configures GitHub Pages to publish from `main:/docs`.

After the Pages deployment is available, run the application and request a **new** password reset email. Old emails still contain the old custom-scheme URL and cannot be changed retroactively.

## Optional .env override

```env
ITDM_PASSWORD_RESET_WEB_URL=https://tamnhien.github.io/ITDeviceManager/reset-password.html
```

The above URL is already the default, so this variable is optional.
