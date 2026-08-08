param(
    [ValidateSet('Auto','Install','Upgrade')]
    [string]$Mode = 'Auto'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

$PackageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppSource = Join-Path $PackageRoot 'App'
$BackupFile = Join-Path $PackageRoot 'Database\pos_system.bak'
$ConfigPath = Join-Path $PackageRoot 'Installer.config.json'
$PackagedMigrations = Join-Path $PackageRoot 'Migrations'
$RepositoryMigrations = Join-Path (Split-Path -Parent $PackageRoot) 'database\migrations'

$InstallRoot = 'C:\BikeZonePOS'
$DatabaseName = 'pos_system'
$SqlInstance = '.'
$SqlServiceName = 'MSSQLSERVER'
$ShortcutName = 'Bike Zone POS'
$LaunchAfterInstall = $true
$AutoDetectSqlInstance = $true
$ConnectionStringOverride = ''
$InstallSqlIfMissing = $true
$CreatePreUpgradeBackup = $true

function Write-Step([string]$Message) {
    Write-Host ''
    Write-Host "== $Message" -ForegroundColor Cyan
}

function Write-Ok([string]$Message) {
    Write-Host "   OK: $Message" -ForegroundColor Green
}

function Load-InstallerConfig {
    if (-not (Test-Path $ConfigPath)) { return }

    try {
        $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    }
    catch {
        throw 'Installer.config.json is not valid JSON.'
    }

    if ($config.installRoot) { $script:InstallRoot = [string]$config.installRoot }
    if ($config.databaseName) { $script:DatabaseName = [string]$config.databaseName }
    if ($config.sqlInstance) { $script:SqlInstance = [string]$config.sqlInstance }
    if ($config.sqlServiceName) { $script:SqlServiceName = [string]$config.sqlServiceName }
    if ($null -ne $config.autoDetectSqlInstance) { $script:AutoDetectSqlInstance = [bool]$config.autoDetectSqlInstance }
    if ($config.shortcutName) { $script:ShortcutName = [string]$config.shortcutName }
    if ($null -ne $config.launchAfterInstall) { $script:LaunchAfterInstall = [bool]$config.launchAfterInstall }
    if ($config.connectionString) { $script:ConnectionStringOverride = [string]$config.connectionString }
    if ($null -ne $config.installSqlIfMissing) { $script:InstallSqlIfMissing = [bool]$config.installSqlIfMissing }
    if ($null -ne $config.createPreUpgradeBackup) { $script:CreatePreUpgradeBackup = [bool]$config.createPreUpgradeBackup }
}

function Get-SqlServerTarget {
    $services = Get-Service -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -eq 'MSSQLSERVER' -or $_.Name -like 'MSSQL$*'
    }

    if (-not $services) { return $null }

    $preferred = $services | Sort-Object @{
        Expression = {
            if ($_.Name -eq 'MSSQLSERVER') { 0 }
            elseif ($_.Name -eq 'MSSQL$SQLEXPRESS') { 1 }
            else { 2 }
        }
    }, Name | Select-Object -First 1

    if ($preferred.Name -eq 'MSSQLSERVER') {
        return [PSCustomObject]@{ ServiceName = 'MSSQLSERVER'; InstanceName = '.' }
    }

    $instanceSuffix = $preferred.Name.Substring('MSSQL$'.Length)
    return [PSCustomObject]@{ ServiceName = $preferred.Name; InstanceName = ".\$instanceSuffix" }
}

function Ensure-SqlServer {
    if ($AutoDetectSqlInstance) {
        $target = Get-SqlServerTarget
        if ($target) {
            $script:SqlServiceName = $target.ServiceName
            $script:SqlInstance = $target.InstanceName
        }
    }

    $service = Get-Service -Name $SqlServiceName -ErrorAction SilentlyContinue
    if (-not $service -and $InstallSqlIfMissing) {
        $bootstrapper = Join-Path $PackageRoot 'SQL2022-SSEI-Expr.exe'
        if (Test-Path $bootstrapper) {
            Write-Step 'SQL Server was not found - opening SQL Server Express installer'
            Write-Host 'Complete the SQL Server Express installation, then return here.' -ForegroundColor Yellow
            Start-Process -FilePath $bootstrapper -Wait
            Start-Sleep -Seconds 3

            if ($AutoDetectSqlInstance) {
                $target = Get-SqlServerTarget
                if ($target) {
                    $script:SqlServiceName = $target.ServiceName
                    $script:SqlInstance = $target.InstanceName
                }
            }
            $service = Get-Service -Name $SqlServiceName -ErrorAction SilentlyContinue
        }
    }

    if (-not $service) {
        throw 'SQL Server Database Engine was not found. Install SQL Server Express/Server and run setup again.'
    }

    if ($service.Status -ne 'Running') {
        Start-Service -Name $SqlServiceName
        $service.WaitForStatus('Running', '00:00:45')
    }

    Write-Ok "SQL Server: $SqlInstance ($SqlServiceName)"
}

function New-SqlConnection([string]$Database = 'master') {
    $connectionString = "Data Source=$SqlInstance;Initial Catalog=$Database;Integrated Security=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $connection.Open()
    return $connection
}

function Invoke-SqlNonQuery([string]$Query, [string]$Database = 'master', [int]$Timeout = 300) {
    $connection = New-SqlConnection $Database
    try {
        $command = $connection.CreateCommand()
        $command.CommandTimeout = $Timeout
        $command.CommandText = $Query
        [void]$command.ExecuteNonQuery()
    }
    finally {
        $connection.Dispose()
    }
}

function Invoke-SqlScalar([string]$Query, [string]$Database = 'master') {
    $connection = New-SqlConnection $Database
    try {
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 60
        $command.CommandText = $Query
        return $command.ExecuteScalar()
    }
    finally {
        $connection.Dispose()
    }
}

function Invoke-SqlFile([string]$Path, [string]$Database) {
    $text = Get-Content -Raw $Path
    $batches = [regex]::Split($text, '(?im)^\s*GO\s*(?:--.*)?$')
    $connection = New-SqlConnection $Database
    try {
        foreach ($batch in $batches) {
            if ([string]::IsNullOrWhiteSpace($batch)) { continue }
            $command = $connection.CreateCommand()
            $command.CommandTimeout = 300
            $command.CommandText = $batch
            [void]$command.ExecuteNonQuery()
        }
    }
    finally {
        $connection.Dispose()
    }
}

function Test-DatabaseExists {
    $escapedName = $DatabaseName.Replace("'", "''")
    return ([int](Invoke-SqlScalar "SELECT CASE WHEN DB_ID(N'$escapedName') IS NULL THEN 0 ELSE 1 END" 'master')) -eq 1
}

function Build-ConnectionString {
    if (-not [string]::IsNullOrWhiteSpace($ConnectionStringOverride)) {
        return $ConnectionStringOverride
    }
    return "Data Source=$SqlInstance;Initial Catalog=$DatabaseName;Integrated Security=True;TrustServerCertificate=True"
}

function Get-MigrationDirectory {
    if (Test-Path $PackagedMigrations) { return $PackagedMigrations }
    if (Test-Path $RepositoryMigrations) { return $RepositoryMigrations }
    throw 'No Migrations directory was found in the installer package or repository.'
}

function Restore-FreshDatabase {
    if (-not (Test-Path $BackupFile)) {
        throw "Fresh install requires database backup: $BackupFile"
    }

    Write-Step 'Restoring initial database'
    New-Item -ItemType Directory -Force -Path $DataTarget | Out-Null
    $targetBackup = Join-Path $DataTarget 'pos_system.bak'
    Copy-Item $BackupFile $targetBackup -Force

    $escapedBackup = $targetBackup.Replace("'", "''")
    $connection = New-SqlConnection 'master'
    try {
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 300
        $command.CommandText = "RESTORE FILELISTONLY FROM DISK = N'$escapedBackup';"
        $reader = $command.ExecuteReader()
        $logicalData = $null
        $logicalLog = $null
        while ($reader.Read()) {
            $logicalName = [string]$reader['LogicalName']
            $fileType = [string]$reader['Type']
            if ($fileType -eq 'D' -and -not $logicalData) { $logicalData = $logicalName }
            if ($fileType -eq 'L' -and -not $logicalLog) { $logicalLog = $logicalName }
        }
        $reader.Close()
    }
    finally {
        $connection.Dispose()
    }

    if (-not $logicalData -or -not $logicalLog) {
        throw 'Could not detect logical files inside pos_system.bak.'
    }

    $dataFile = Join-Path $DataTarget "$DatabaseName.mdf"
    $logFile = Join-Path $DataTarget ($DatabaseName + '_log.ldf')
    $escapedData = $dataFile.Replace("'", "''")
    $escapedLog = $logFile.Replace("'", "''")
    $logicalData = $logicalData.Replace("'", "''")
    $logicalLog = $logicalLog.Replace("'", "''")

    Invoke-SqlNonQuery "RESTORE DATABASE [$DatabaseName] FROM DISK = N'$escapedBackup' WITH MOVE N'$logicalData' TO N'$escapedData', MOVE N'$logicalLog' TO N'$escapedLog', REPLACE, RECOVERY" 'master' 600
    Write-Ok "Database $DatabaseName restored"
}

function Backup-ExistingDatabase {
    if (-not $CreatePreUpgradeBackup) { return }

    Write-Step 'Creating safety backup before database upgrade'
    $backupDirectory = Join-Path $DataTarget 'Backups'
    New-Item -ItemType Directory -Force -Path $backupDirectory | Out-Null
    $stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $backupPath = Join-Path $backupDirectory ("${DatabaseName}_pre_upgrade_${stamp}.bak")
    $escapedPath = $backupPath.Replace("'", "''")

    try {
        Invoke-SqlNonQuery "BACKUP DATABASE [$DatabaseName] TO DISK = N'$escapedPath' WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10" 'master' 900
        Write-Ok "Safety backup: $backupPath"
    }
    catch {
        throw "Safety backup failed. Upgrade stopped to protect your data. $($_.Exception.Message)"
    }
}

function Apply-AllMigrations {
    $migrationDirectory = Get-MigrationDirectory
    $migrationFiles = Get-ChildItem $migrationDirectory -Filter '*.sql' -File | Sort-Object Name
    if (-not $migrationFiles) { throw "No migration .sql files found in $migrationDirectory" }

    Write-Step 'Applying database migrations'
    foreach ($migration in $migrationFiles) {
        Write-Host "   -> $($migration.Name)"
        Invoke-SqlFile $migration.FullName $DatabaseName
    }
    Write-Ok "$($migrationFiles.Count) migration file(s) processed"
}

function Stop-RunningApplication {
    Get-Process -Name 'Pos_System' -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Step 'Closing running POS application before update'
        $_.CloseMainWindow() | Out-Null
        if (-not $_.WaitForExit(5000)) { Stop-Process -Id $_.Id -Force }
    }
}

function Install-ApplicationFiles {
    Write-Step 'Installing new desktop application version'
    if (-not (Test-Path $AppSource)) { throw 'Installer package is missing the App folder.' }

    Stop-RunningApplication
    New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
    New-Item -ItemType Directory -Force -Path $DataTarget | Out-Null

    if (Test-Path $AppTarget) {
        $versionBackupRoot = Join-Path $InstallRoot 'PreviousVersions'
        New-Item -ItemType Directory -Force -Path $versionBackupRoot | Out-Null
        $versionBackup = Join-Path $versionBackupRoot (Get-Date -Format 'yyyyMMdd_HHmmss')
        Copy-Item $AppTarget $versionBackup -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $AppTarget | Out-Null
    Copy-Item -Path (Join-Path $AppSource '*') -Destination $AppTarget -Recurse -Force

    $databaseConfigPath = Join-Path $AppTarget 'Database.config'
    $finalConnectionString = Build-ConnectionString
    $databaseConfig = @"
<connectionStrings>
  <add name="Pos_System.Properties.Settings.pos_systemConnectionString" connectionString="$finalConnectionString" providerName="System.Data.SqlClient" />
</connectionStrings>
"@
    Set-Content -Path $databaseConfigPath -Value $databaseConfig -Encoding UTF8
    Write-Ok "Application installed to $AppTarget"
}

function Create-DesktopShortcut {
    Write-Step 'Creating desktop shortcut'
    $targetExe = Join-Path $AppTarget 'Pos_System.exe'
    if (-not (Test-Path $targetExe)) { throw "Application executable not found: $targetExe" }

    $shortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) ($ShortcutName + '.lnk')
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $targetExe
    $shortcut.WorkingDirectory = $AppTarget
    $shortcut.Description = $ShortcutName
    $shortcut.IconLocation = "$targetExe,0"
    $shortcut.Save()
    Write-Ok $shortcutPath
}

Load-InstallerConfig
$AppTarget = Join-Path $InstallRoot 'App'
$DataTarget = Join-Path $InstallRoot 'Database'

Write-Host '=============================================' -ForegroundColor DarkCyan
Write-Host ' Bike Zone POS - Install / Update' -ForegroundColor Cyan
Write-Host '=============================================' -ForegroundColor DarkCyan

Write-Step 'Checking SQL Server'
Ensure-SqlServer

$databaseExists = Test-DatabaseExists
$effectiveMode = $Mode
if ($Mode -eq 'Auto') {
    if ($databaseExists) { $effectiveMode = 'Upgrade' } else { $effectiveMode = 'Install' }
}

Write-Step "Mode: $effectiveMode"
if ($effectiveMode -eq 'Install' -and $databaseExists) {
    throw "Database [$DatabaseName] already exists. Use Auto or Upgrade mode to preserve existing data."
}
if ($effectiveMode -eq 'Upgrade' -and -not $databaseExists) {
    throw "Database [$DatabaseName] does not exist. Use Auto or Install mode for a new device."
}

if ($effectiveMode -eq 'Upgrade') {
    Backup-ExistingDatabase
}
else {
    Restore-FreshDatabase
}

Apply-AllMigrations
Install-ApplicationFiles
Create-DesktopShortcut

$targetExe = Join-Path $AppTarget 'Pos_System.exe'
if ($LaunchAfterInstall) {
    Write-Step 'Starting new POS version'
    Start-Process -FilePath $targetExe -WorkingDirectory $AppTarget
}

Write-Host ''
Write-Host '=============================================' -ForegroundColor DarkGreen
Write-Host " POS $effectiveMode completed successfully." -ForegroundColor Green
Write-Host " Database: $DatabaseName on $SqlInstance" -ForegroundColor Green
Write-Host " App:      $targetExe" -ForegroundColor Green
Write-Host '=============================================' -ForegroundColor DarkGreen
