@echo off
setlocal
cd /d "%~dp0"
echo [ITDeviceManager] Restoring packages...
dotnet restore ".\ITDeviceManager.sln"
if errorlevel 1 goto :error

echo [ITDeviceManager] Building Debug...
dotnet build ".\ITDeviceManager.sln" -c Debug --no-restore
if errorlevel 1 goto :error

echo.
echo BUILD OK
exit /b 0

:error
echo.
echo BUILD FAILED - exit code %errorlevel%
pause
exit /b %errorlevel%
