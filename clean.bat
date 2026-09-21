@echo off
setlocal EnableExtensions
cd /d "%~dp0"

rem V2.0.0: fail clearly if the running app still owns bin\Debug files.
tasklist /FI "IMAGENAME eq ITDeviceManager.exe" /NH 2>nul | find /I "ITDeviceManager.exe" >nul
if not errorlevel 1 (
    echo [CLEAN BLOCKED] ITDeviceManager.exe is still running.
    echo Close the application before cleaning/building. Running process:
    tasklist /FI "IMAGENAME eq ITDeviceManager.exe"
    exit /b 2
)

call :RemoveDir ".\ITDeviceManager\bin"
if errorlevel 1 exit /b %errorlevel%
call :RemoveDir ".\ITDeviceManager\obj"
if errorlevel 1 exit /b %errorlevel%

rem Retired self-test project and automatic test scripts must stay removed.
call :RemoveDir ".\ITDeviceManager.SelfTest"
if errorlevel 1 exit /b %errorlevel%
call :RemoveFile ".\test.bat"
if errorlevel 1 exit /b %errorlevel%
call :RemoveFile ".\scripts\test.ps1"
if errorlevel 1 exit /b %errorlevel%

rem Keep a single root README.md as the canonical project documentation.
for %%F in ("README_HOTFIX*.txt" "README_RESET_LINK.md" "README_V*.md" "THAY_DOI_V*.md") do (
    if exist ".\%%~F" (
        del /f /q ".\%%~F" >nul 2>&1
        if exist ".\%%~F" (
            echo [CLEAN ERROR] Could not remove .\%%~F
            exit /b 3
        )
    )
)

echo Clean completed. Build outputs removed; database files, backups and .env were preserved.
exit /b 0

:RemoveDir
if not exist "%~1" exit /b 0
rmdir /s /q "%~1" >nul 2>&1
if exist "%~1" (
    echo [CLEAN ERROR] Could not remove %~1
    echo A process may still be using files in this directory.
    exit /b 3
)
exit /b 0

:RemoveFile
if not exist "%~1" exit /b 0
del /f /q "%~1" >nul 2>&1
if exist "%~1" (
    echo [CLEAN ERROR] Could not remove %~1
    exit /b 3
)
exit /b 0
