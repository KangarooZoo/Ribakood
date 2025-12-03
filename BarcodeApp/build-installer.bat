@echo off
REM Batch script to build the installer
REM This calls the PowerShell script

echo Building BarcodeApp installer...
echo.

powershell.exe -ExecutionPolicy Bypass -File "%~dp0build-installer.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
echo.
echo To create the installer:
echo 1. Install Inno Setup from https://jrsoftware.org/isinfo.php
echo 2. Open BarcodeApp.iss in Inno Setup Compiler
echo 3. Click Build ^> Compile
echo.
pause

