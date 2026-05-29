
$ErrorActionPreference = "Stop"

$PackageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppSource = Join-Path $PackageRoot "App"
$BackupFile = Join-Path $PackageRoot "Database\pos_system.bak"
$ConfigPath = Join-Path $PackageRoot "Installer.config.json"
$InstallRoot = "C:\BikeZonePOS"
$DatabaseName = "pos_system"
$SqlInstance = "."
$SqlServiceName = "MSSQLSERVER"
$ShortcutName = "Bike Zone POS"
$LaunchAfterInstall = $true
$AutoDetectSqlInstance = $true
$ConnectionStringOverride = ""

function Write-Step($message) {
    Write-Host ""
    Write-Host "== $message" -ForegroundColor Cyan
}

function Load-InstallerConfig {
    if (-not (Test-Path $ConfigPath)) {
        return
    }

    try {
        $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    } catch {
        throw "Installer.config.json is not valid JSON. Fix the file and run the installer again."
    }

    if ($config.installRoot) { $script:InstallRoot = $config.installRoot }
    if ($config.databaseName) { $script:DatabaseName = $config.databaseName }
    if ($config.sqlInstance) { $script:SqlInstance = $config.sqlInstance }
    if ($config.sqlServiceName) { $script:SqlServiceName = $config.sqlServiceName }
    if ($null -ne $config.autoDetectSqlInstance) { $script:AutoDetectSqlInstance = [bool]$config.autoDetectSqlInstance }
    if ($config.shortcutName) { $script:ShortcutName = $config.shortcutName }
    if ($null -ne $config.launchAfterInstall) { $script:LaunchAfterInstall = [bool]$config.launchAfterInstall }
    if ($config.connectionString) { $script:ConnectionStringOverride = [string]$config.connectionString }
}

function Find-SqlCmd {
    $cmd = Get-Command sqlcmd.exe -ErrorAction SilentlyContinue
    if ($cmd) {
        return $cmd.Source
    }

    $candidates = @(
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\160\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\150\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\130\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\140\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\130\Tools\Binn\sqlcmd.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

function Invoke-Sql($query) {
    & $script:SqlCmd -S $SqlInstance -E -b -Q $query
    if ($LASTEXITCODE -ne 0) {
        throw "SQL command failed."
    }
}

function Get-SqlServerTarget {
    $services = Get-Service -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -eq "MSSQLSERVER" -or $_.Name -like "MSSQL`$*"
    }

    if (-not $services) {
        return $null
    }

    $preferred = $services | Sort-Object @{
        Expression = {
            if ($_.Name -eq "MSSQLSERVER") { 0 }
            elseif ($_.Name -eq "MSSQL`$SQLEXPRESS") { 1 }
            else { 2 }
        }
    }, Name | Select-Object -First 1

    if ($preferred.Name -eq "MSSQLSERVER") {
        return [PSCustomObject]@{
            ServiceName = "MSSQLSERVER"
            InstanceName = "."
        }
    }

    $instanceSuffix = $preferred.Name.Substring("MSSQL$".Length)
    return [PSCustomObject]@{
        ServiceName = $preferred.Name
        InstanceName = ".\$instanceSuffix"
    }
}

function Build-ConnectionString {
    if (-not [string]::IsNullOrWhiteSpace($script:ConnectionStringOverride)) {
        return $script:ConnectionStringOverride
    }

    return "Data Source=$SqlInstance;Initial Catalog=$DatabaseName;Integrated Security=True;TrustServerCertificate=True"
}

Load-InstallerConfig

$AppTarget = Join-Path $InstallRoot "App"
$DataTarget = Join-Path $InstallRoot "Database"

if (-not (Test-Path $AppSource)) {
    throw "Missing App folder beside this installer."
}

if (-not (Test-Path $BackupFile)) {
    throw "Missing database backup: $BackupFile"
}

Write-Step "Checking SQL Server service"
if ($AutoDetectSqlInstance) {
    $sqlTarget = Get-SqlServerTarget
    if ($sqlTarget) {
        $SqlServiceName = $sqlTarget.ServiceName
        $SqlInstance = $sqlTarget.InstanceName
    }
}

$service = Get-Service -Name $SqlServiceName -ErrorAction SilentlyContinue

if (-not $service) {
    throw "SQL Server service was not found. Install SQL Server Express or SQL Server Database Engine, then run this installer again."
}

if ($service.Status -ne "Running") {
    Start-Service -Name $SqlServiceName
    $service.WaitForStatus("Running", "00:00:30")
}

$script:SqlCmd = Find-SqlCmd
if (-not $script:SqlCmd) {
    throw "sqlcmd.exe was not found. Install SQL Server Command Line Utilities, then run this installer again."
}

Write-Step "Copying application files"
New-Item -ItemType Directory -Force -Path $AppTarget | Out-Null
New-Item -ItemType Directory -Force -Path $DataTarget | Out-Null
Copy-Item -Path (Join-Path $AppSource "*") -Destination $AppTarget -Recurse -Force
Copy-Item -Path $BackupFile -Destination (Join-Path $DataTarget "pos_system.bak") -Force

$databaseConfigPath = Join-Path $AppTarget "Database.config"
$finalConnectionString = Build-ConnectionString
$databaseConfig = @"
<connectionStrings>
  <add name="Pos_System.Properties.Settings.pos_systemConnectionString" connectionString="$finalConnectionString" providerName="System.Data.SqlClient" />
</connectionStrings>
"@
Set-Content -Path $databaseConfigPath -Value $databaseConfig -Encoding UTF8

Write-Step "Restoring database"
$targetBackup = Join-Path $DataTarget "pos_system.bak"
$dataFile = Join-Path $DataTarget "$DatabaseName.mdf"
$logFile = Join-Path $DataTarget ($DatabaseName + "_log.ldf")
$escapedBackup = $targetBackup.Replace("'", "''")
$escapedData = $dataFile.Replace("'", "''")
$escapedLog = $logFile.Replace("'", "''")

$fileListOutput = & $script:SqlCmd -S $SqlInstance -E -b -W -h -1 -s "|" -Q "RESTORE FILELISTONLY FROM DISK = N'$escapedBackup';"
if ($LASTEXITCODE -ne 0) {
    throw "Could not read backup file list."
}

$logicalData = $null
$logicalLog = $null
foreach ($line in $fileListOutput) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    $parts = $line -split "\|"
    if ($parts.Count -lt 3) {
        continue
    }

    $logicalName = $parts[0].Trim()
    $fileType = $parts[2].Trim().ToUpperInvariant()

    if ($fileType -eq "D" -and -not $logicalData) {
        $logicalData = $logicalName
    } elseif ($fileType -eq "L" -and -not $logicalLog) {
        $logicalLog = $logicalName
    }
}

if ([string]::IsNullOrWhiteSpace($logicalData) -or [string]::IsNullOrWhiteSpace($logicalLog)) {
    throw "Could not detect logical database file names from backup."
}

$logicalData = $logicalData.Replace("'", "''")
$logicalLog = $logicalLog.Replace("'", "''")

Invoke-Sql "IF DB_ID(N'$DatabaseName') IS NOT NULL BEGIN ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$DatabaseName]; END"
Invoke-Sql "RESTORE DATABASE [$DatabaseName] FROM DISK = N'$escapedBackup' WITH MOVE N'$logicalData' TO N'$escapedData', MOVE N'$logicalLog' TO N'$escapedLog', REPLACE, RECOVERY"
Invoke-Sql "ALTER DATABASE [$DatabaseName] SET MULTI_USER"

Write-Step "Creating desktop shortcut"
$shortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) ($ShortcutName + ".lnk")
$targetExe = Join-Path $AppTarget "Pos_System.exe"
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = $AppTarget
$shortcut.Description = $ShortcutName
$shortcut.IconLocation = "$targetExe,0"
$shortcut.Save()

if ($LaunchAfterInstall) {
    Write-Step "Starting application"
    Start-Process -FilePath $targetExe -WorkingDirectory $AppTarget
}

Write-Host ""
Write-Host "Bike Zone POS installed successfully." -ForegroundColor Green
