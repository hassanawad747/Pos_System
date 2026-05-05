$ErrorActionPreference = "Stop"

$PackageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppSource = Join-Path $PackageRoot "App"
$BackupFile = Join-Path $PackageRoot "Database\pos_system.bak"
$InstallRoot = "C:\BikeZonePOS"
$AppTarget = Join-Path $InstallRoot "App"
$DataTarget = Join-Path $InstallRoot "Database"
$DatabaseName = "pos_system"
$SqlInstance = ".\SQLEXPRESS"
$SqlServiceName = "MSSQL`$SQLEXPRESS"

function Write-Step($message) {
    Write-Host ""
    Write-Host "== $message" -ForegroundColor Cyan
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

if (-not (Test-Path $AppSource)) {
    throw "Missing App folder beside this installer."
}

if (-not (Test-Path $BackupFile)) {
    throw "Missing database backup: $BackupFile"
}

Write-Step "Checking SQL Server Express service"
$service = Get-Service -Name $SqlServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    throw "SQL Server Express service was not found. Install SQL Server Express with instance name SQLEXPRESS, then run this installer again."
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

Write-Step "Restoring database"
$targetBackup = Join-Path $DataTarget "pos_system.bak"
$dataFile = Join-Path $DataTarget "$DatabaseName.mdf"
$logFile = Join-Path $DataTarget ($DatabaseName + "_log.ldf")
$escapedBackup = $targetBackup.Replace("'", "''")
$escapedData = $dataFile.Replace("'", "''")
$escapedLog = $logFile.Replace("'", "''")

$fileListOutput = & $script:SqlCmd -S $SqlInstance -E -b -Q "RESTORE FILELISTONLY FROM DISK = N'$escapedBackup';"
if ($LASTEXITCODE -ne 0) {
    throw "Could not read backup file list."
}

$logicalNames = @()
foreach ($line in $fileListOutput) {
    if ($line -match "^\s*(\S+)\s+\S+\s+[DL]\s+") {
        $logicalNames += $matches[1]
    }
}

if ($logicalNames.Count -lt 2) {
    throw "Could not detect logical database file names from backup."
}

$logicalData = $logicalNames[0].Replace("'", "''")
$logicalLog = $logicalNames[1].Replace("'", "''")

Invoke-Sql "IF DB_ID(N'$DatabaseName') IS NOT NULL BEGIN ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$DatabaseName]; END"
Invoke-Sql "RESTORE DATABASE [$DatabaseName] FROM DISK = N'$escapedBackup' WITH MOVE N'$logicalData' TO N'$escapedData', MOVE N'$logicalLog' TO N'$escapedLog', REPLACE, RECOVERY"
Invoke-Sql "ALTER DATABASE [$DatabaseName] SET MULTI_USER"

Write-Step "Creating desktop shortcut"
$shortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) "Bike Zone POS.lnk"
$targetExe = Join-Path $AppTarget "Pos_System.exe"
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = $AppTarget
$shortcut.Description = "Bike Zone POS"
$shortcut.Save()

Write-Step "Starting application"
Start-Process -FilePath $targetExe -WorkingDirectory $AppTarget

Write-Host ""
Write-Host "Bike Zone POS installed successfully." -ForegroundColor Green
