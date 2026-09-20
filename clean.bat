@echo off
setlocal
cd /d "%~dp0"

if exist ".\ITDeviceManager\bin" rmdir /s /q ".\ITDeviceManager\bin"
if exist ".\ITDeviceManager\obj" rmdir /s /q ".\ITDeviceManager\obj"

rem V1.2.6 cleanup: remove retired self-test project and automatic test scripts.
if exist ".\ITDeviceManager.SelfTest" rmdir /s /q ".\ITDeviceManager.SelfTest"
if exist ".\test.bat" del /f /q ".\test.bat"
if exist ".\scripts\test.ps1" del /f /q ".\scripts\test.ps1"

echo Clean completed. Legacy SelfTest/test automation removed.
