@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo [Clean] Checking for running ITDeviceManager process...
tasklist /FI "IMAGENAME eq ITDeviceManager.exe" 2>NUL | find /I "ITDeviceManager.exe" >NUL
if not errorlevel 1 (
    echo [Clean] ITDeviceManager.exe is still running. Stopping it to release bin/obj file locks...
    taskkill /F /IM ITDeviceManager.exe >NUL 2>&1
    if errorlevel 1 (
        echo [Clean] ERROR: Could not stop ITDeviceManager.exe. Close the application and run clean.bat again.
        exit /b 1
    )
    timeout /t 1 /nobreak >NUL
)

if exist ".\ITDeviceManager\bin" rmdir /s /q ".\ITDeviceManager\bin"
if exist ".\ITDeviceManager\bin" (
    echo [Clean] ERROR: Could not remove .\ITDeviceManager\bin. A process may still be locking build files.
    exit /b 1
)

if exist ".\ITDeviceManager\obj" rmdir /s /q ".\ITDeviceManager\obj"
if exist ".\ITDeviceManager\obj" (
    echo [Clean] ERROR: Could not remove .\ITDeviceManager\obj.
    exit /b 1
)

rem Keep local SQL Server and generated QR data. These folders are never deleted by clean.bat.
rem   .\DatabaseFiles
rem   .\Backups
rem   .\QR

rem Remove retired self-test project and automatic test scripts.
if exist ".\ITDeviceManager.SelfTest" rmdir /s /q ".\ITDeviceManager.SelfTest"
if exist ".\test.bat" del /f /q ".\test.bat"
if exist ".\scripts\test.ps1" del /f /q ".\scripts\test.ps1"

rem Keep a single root README.md as the canonical project documentation.
for %%F in ("README_HOTFIX*.txt" "README_RESET_LINK.md" "README_V*.md" "THAY_DOI_V*.md") do (
    if exist ".\%%~F" del /f /q ".\%%~F"
)

echo [Clean] Clean completed successfully. bin/obj removed; DatabaseFiles/Backups/QR preserved.
exit /b 0
