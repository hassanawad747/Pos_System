@echo off
setlocal

cd /d "%~dp0"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
    echo.
    echo Administrator permission is required.
    echo Right-click install_pos_system.bat and choose Run as administrator.
    echo.
    pause
    exit /b 1
)

set "MODE=Auto"
if /I "%~1"=="install" set "MODE=Install"
if /I "%~1"=="upgrade" set "MODE=Upgrade"
if /I "%~1"=="auto" set "MODE=Auto"

echo.
echo =============================================
echo  Bike Zone POS - Install / Update
echo  Mode: %MODE%
echo =============================================
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install_pos_system.ps1" -Mode "%MODE%"
set "EXITCODE=%errorlevel%"

echo.
if "%EXITCODE%"=="0" (
    echo POS setup/update finished successfully.
) else (
    echo POS setup/update failed. Error code: %EXITCODE%
)

echo.
pause
exit /b %EXITCODE%
