ITDeviceManager V1.2.6 - Release script hotfix

Fix:
- Prevents "You cannot call a method on a null-valued expression" when git/gh commands return no stdout.
- Safe handling for missing origin, missing tags, and missing releases.
- Keeps .env secret checks.
- Keeps release workflow without SelfTest/test.bat.
- Safe to rerun: .\release.bat 1.2.6 after copying this hotfix.

Copy this package over:
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager

Then run:
.\release.bat 1.2.6
