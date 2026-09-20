@echo off
setlocal
cd /d "%~dp0"
if exist ".\ITDeviceManager\bin" rmdir /s /q ".\ITDeviceManager\bin"
if exist ".\ITDeviceManager\obj" rmdir /s /q ".\ITDeviceManager\obj"
echo Clean completed.
