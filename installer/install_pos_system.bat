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

set "ROLE=Interactive"
if /I "%~1"=="server" set "ROLE=Server"
if /I "%~1"=="client" set "ROLE=Client"

echo.
echo =================================================
echo  Bike Zone POS - SQL-Only LAN Installer
echo  Role: %ROLE%
echo =================================================
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup_pos_lan.ps1" -Role "%ROLE%"
set "EXITCODE=%errorlevel%"

echo.
if "%EXITCODE%"=="0" (
    echo POS setup finished successfully.
) else (
    echo POS setup failed. Error code: %EXITCODE%
)

echo.
pause
exit /b %EXITCODE%
