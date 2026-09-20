@echo off
setlocal
cd /d "%~dp0"
echo [ITDeviceManager] Starting application...
dotnet run --project ".\ITDeviceManager\ITDeviceManager.csproj"
if errorlevel 1 pause
