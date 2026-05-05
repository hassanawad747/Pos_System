@echo off
setlocal

cd /d "%~dp0"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
    echo.
    echo Please run this file as Administrator.
    echo Right click install_pos_system.bat and choose Run as administrator.
    echo.
    pause
    exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install_pos_system.ps1"
set "EXITCODE=%errorlevel%"

echo.
if "%EXITCODE%"=="0" (
    echo Install finished.
) else (
    echo Install failed. Error code: %EXITCODE%
)
pause
exit /b %EXITCODE%
