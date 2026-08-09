param(
    [ValidateSet('Interactive','Server','Client')]
    [string]$Role = 'Interactive'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

$PackageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppSource = Join-Path $PackageRoot 'App'
$ConfigPath = Join-Path $PackageRoot 'Installer.config.json'
$PackagedMigrations = Join-Path $PackageRoot 'Migrations'
$PackagedBaseSchema = Join-Path $PackageRoot 'Database\000_base_schema.sql'
$RepositoryRoot = Split-Path -Parent $PackageRoot
$RepositoryMigrations = Join-Path $RepositoryRoot 'database\migrations'
$RepositoryBaseSchema = Join-Path $RepositoryRoot 'database\install\000_base_schema.sql'

$InstallRoot = 'C:\BikeZonePOS'
$DatabaseName = 'pos_system'
$SqlInstance = '.\SQLEXPRESS'
$SqlServiceName = 'MSSQL$SQLEXPRESS'
$SqlPort = 1433
$DatabaseLogin = 'bikezone_pos_app'
$ShortcutName = 'Bike Zone POS'
$LaunchAfterInstall = $true
$AutoDetectSqlInstance = $true
$InstallSqlIfMissing = $true

function Write-Step([string]$Message) {
    Write-Host ''
    Write-Host "== $Message" -ForegroundColor Cyan
}

function Write-Ok([string]$Message) {
    Write-Host "   OK: $Message" -ForegroundColor Green
}

function Get-PlainTextPassword([string]$Prompt) {
    $secure = Read-Host $Prompt -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

function Read-ConfirmedPassword([string]$Label) {
    while ($true) {
        $first = Get-PlainTextPassword "$Label password"
        $second = Get-PlainTextPassword "Confirm $Label password"
        if ([string]::IsNullOrWhiteSpace($first)) {
            Write-Host 'Password cannot be empty.' -ForegroundColor Yellow
            continue
        }
        if ($first -ne $second) {
            Write-Host 'Passwords do not match. Try again.' -ForegroundColor Yellow
            continue
        }
        return $first
    }
}

function Escape-SqlLiteral([string]$Value) {
    if ($null -eq $Value) { return '' }
    return $Value.Replace("'", "''")
}

function Escape-Xml([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

function Load-InstallerConfig {
    if (-not (Test-Path $ConfigPath)) { return }
    try { $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json }
    catch { throw 'Installer.config.json is not valid JSON.' }

    if ($config.installRoot) { $script:InstallRoot = [string]$config.installRoot }
    if ($config.databaseName) { $script:DatabaseName = [string]$config.databaseName }
    if ($config.sqlInstance) { $script:SqlInstance = [string]$config.sqlInstance }
    if ($config.sqlServiceName) { $script:SqlServiceName = [string]$config.sqlServiceName }
    if ($config.sqlPort) { $script:SqlPort = [int]$config.sqlPort }
    if ($config.databaseLogin) { $script:DatabaseLogin = [string]$config.databaseLogin }
    if ($null -ne $config.autoDetectSqlInstance) { $script:AutoDetectSqlInstance = [bool]$config.autoDetectSqlInstance }
    if ($config.shortcutName) { $script:ShortcutName = [string]$config.shortcutName }
    if ($null -ne $config.launchAfterInstall) { $script:LaunchAfterInstall = [bool]$config.launchAfterInstall }
    if ($null -ne $config.installSqlIfMissing) { $script:InstallSqlIfMissing = [bool]$config.installSqlIfMissing }
}

function Select-Role {
    if ($Role -ne 'Interactive') { return $Role }
    Write-Host ''
    Write-Host 'Select installation type:' -ForegroundColor White
    Write-Host '  1. SERVER - this PC hosts SQL database and can run the POS app'
    Write-Host '  2. CLIENT - this PC runs the POS app and connects to the LAN server'
    while ($true) {
        $choice = Read-Host 'Enter 1 or 2'
        if ($choice -eq '1') { return 'Server' }
        if ($choice -eq '2') { return 'Client' }
        Write-Host 'Invalid choice.' -ForegroundColor Yellow
    }
}

function Get-SqlServerTarget {
    $services = Get-Service -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -eq 'MSSQLSERVER' -or $_.Name -like 'MSSQL$*'
    }
    if (-not $services) { return $null }

    $preferred = $services | Sort-Object @{
        Expression = {
            if ($_.Name -eq 'MSSQL$SQLEXPRESS') { 0 }
            elseif ($_.Name -eq 'MSSQLSERVER') { 1 }
            else { 2 }
        }
    }, Name | Select-Object -First 1

    if ($preferred.Name -eq 'MSSQLSERVER') {
        return [PSCustomObject]@{ ServiceName = 'MSSQLSERVER'; InstanceName = '.'; InstanceShortName = 'MSSQLSERVER' }
    }

    $instanceSuffix = $preferred.Name.Substring('MSSQL$'.Length)
    return [PSCustomObject]@{ ServiceName = $preferred.Name; InstanceName = ".\$instanceSuffix"; InstanceShortName = $instanceSuffix }
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
            Write-Step 'SQL Server was not found'
            Write-Host 'The SQL Server Express installer will open. Install Database Engine, then run this POS installer again.' -ForegroundColor Yellow
            Start-Process -FilePath $bootstrapper -Wait
            Start-Sleep -Seconds 3
            $target = Get-SqlServerTarget
            if ($target) {
                $script:SqlServiceName = $target.ServiceName
                $script:SqlInstance = $target.InstanceName
                $service = Get-Service -Name $SqlServiceName -ErrorAction SilentlyContinue
            }
        }
    }

    if (-not $service) {
        throw 'SQL Server Database Engine was not found. Install SQL Server Express/Server and run Server setup again.'
    }
    if ($service.Status -ne 'Running') {
        Start-Service -Name $SqlServiceName
        $service.WaitForStatus('Running', '00:00:45')
    }
    Write-Ok "SQL Server: $SqlInstance ($SqlServiceName)"
}

function New-WindowsSqlConnection([string]$Database = 'master') {
    $cs = "Data Source=$SqlInstance;Initial Catalog=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15"
    $connection = New-Object System.Data.SqlClient.SqlConnection $cs
    $connection.Open()
    return $connection
}

function New-AppSqlConnection([string]$ServerAddress, [string]$Password, [string]$Database = $DatabaseName) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = "$ServerAddress,$SqlPort"
    $builder['Initial Catalog'] = $Database
    $builder['User ID'] = $DatabaseLogin
    $builder['Password'] = $Password
    $builder['Integrated Security'] = $false
    $builder['TrustServerCertificate'] = $true
    $builder.ConnectTimeout = 10
    $connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
    $connection.Open()
    return $connection
}

function Invoke-WindowsSqlNonQuery([string]$Query, [string]$Database = 'master', [int]$Timeout = 300) {
    $connection = New-WindowsSqlConnection $Database
    try {
        $command = $connection.CreateCommand()
        $command.CommandTimeout = $Timeout
        $command.CommandText = $Query
        [void]$command.ExecuteNonQuery()
    }
    finally { $connection.Dispose() }
}

function Invoke-WindowsSqlScalar([string]$Query, [string]$Database = 'master') {
    $connection = New-WindowsSqlConnection $Database
    try {
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 60
        $command.CommandText = $Query
        return $command.ExecuteScalar()
    }
    finally { $connection.Dispose() }
}

function Invoke-WindowsSqlFile([string]$Path, [string]$Database) {
    $text = Get-Content -Raw $Path
    $batches = [regex]::Split($text, '(?im)^\s*GO\s*(?:--.*)?$')
    $connection = New-WindowsSqlConnection $Database
    try {
        foreach ($batch in $batches) {
            if ([string]::IsNullOrWhiteSpace($batch)) { continue }
            $command = $connection.CreateCommand()
            $command.CommandTimeout = 300
            $command.CommandText = $batch
            [void]$command.ExecuteNonQuery()
        }
    }
    finally { $connection.Dispose() }
}

function Get-BaseSchemaPath {
    if (Test-Path $PackagedBaseSchema) { return $PackagedBaseSchema }
    if (Test-Path $RepositoryBaseSchema) { return $RepositoryBaseSchema }
    throw '000_base_schema.sql was not found.'
}

function Get-MigrationDirectory {
    if (Test-Path $PackagedMigrations) { return $PackagedMigrations }
    if (Test-Path $RepositoryMigrations) { return $RepositoryMigrations }
    throw 'Migrations directory was not found.'
}

function Test-DatabaseExists {
    $escaped = Escape-SqlLiteral $DatabaseName
    return ([int](Invoke-WindowsSqlScalar "SELECT CASE WHEN DB_ID(N'$escaped') IS NULL THEN 0 ELSE 1 END" 'master')) -eq 1
}

function New-DatabaseFromSql {
    if (Test-DatabaseExists) {
        Write-Ok "Database $DatabaseName already exists; base schema will not overwrite it"
        return
    }

    Write-Step 'Creating empty POS database from SQL scripts'
    $safeDb = $DatabaseName.Replace(']', ']]')
    Invoke-WindowsSqlNonQuery "CREATE DATABASE [$safeDb];" 'master' 300
    Invoke-WindowsSqlFile (Get-BaseSchemaPath) $DatabaseName
    Write-Ok "Fresh database created from 000_base_schema.sql"
}

function Apply-AllMigrations {
    $dir = Get-MigrationDirectory
    $files = Get-ChildItem $dir -Filter '*.sql' -File | Sort-Object Name
    if (-not $files) { throw "No migration SQL files found in $dir" }

    Write-Step 'Applying database migrations'
    foreach ($file in $files) {
        Write-Host "   -> $($file.Name)"
        Invoke-WindowsSqlFile $file.FullName $DatabaseName
    }
    Write-Ok "$($files.Count) migration file(s) processed"
}

function Get-SqlInstanceRegistryId {
    $target = Get-SqlServerTarget
    if (-not $target) { return $null }
    $namesPath = 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL'
    if (-not (Test-Path $namesPath)) { return $null }
    $properties = Get-ItemProperty $namesPath
    $shortName = $target.InstanceShortName
    if ($shortName -eq 'MSSQLSERVER') { return [string]$properties.MSSQLSERVER }
    return [string]$properties.$shortName
}

function Configure-SqlForLan {
    Write-Step 'Configuring SQL Server for LAN clients'
    $instanceId = Get-SqlInstanceRegistryId
    if ([string]::IsNullOrWhiteSpace($instanceId)) {
        throw 'Could not find SQL Server instance registry settings.'
    }

    $serverPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$instanceId\MSSQLServer"
    if (Test-Path $serverPath) {
        Set-ItemProperty -Path $serverPath -Name LoginMode -Type DWord -Value 2
    }

    $tcpRoot = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$instanceId\MSSQLServer\SuperSocketNetLib\Tcp"
    if (-not (Test-Path $tcpRoot)) { throw "SQL TCP registry path not found: $tcpRoot" }
    Set-ItemProperty -Path $tcpRoot -Name Enabled -Type DWord -Value 1

    $ipAll = Join-Path $tcpRoot 'IPAll'
    Set-ItemProperty -Path $ipAll -Name TcpDynamicPorts -Value ''
    Set-ItemProperty -Path $ipAll -Name TcpPort -Value ([string]$SqlPort)

    $ruleName = "BikeZone POS SQL TCP $SqlPort"
    if (-not (Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP -LocalPort $SqlPort -Action Allow -Profile Any | Out-Null
    }

    Restart-Service -Name $SqlServiceName -Force
    (Get-Service -Name $SqlServiceName).WaitForStatus('Running', '00:00:45')
    Start-Sleep -Seconds 2
    Write-Ok "TCP enabled, static port $SqlPort, Mixed Mode enabled, firewall opened"
}

function Ensure-DatabaseLogin([string]$Password) {
    Write-Step "Creating/updating database login $DatabaseLogin"
    $loginLiteral = Escape-SqlLiteral $DatabaseLogin
    $passwordLiteral = Escape-SqlLiteral $Password
    $safeDb = $DatabaseName.Replace(']', ']]')
    $safeLogin = $DatabaseLogin.Replace(']', ']]')

    Invoke-WindowsSqlNonQuery @"
IF SUSER_ID(N'$loginLiteral') IS NULL
    CREATE LOGIN [$safeLogin] WITH PASSWORD=N'$passwordLiteral', CHECK_POLICY=ON, CHECK_EXPIRATION=OFF;
ELSE
    ALTER LOGIN [$safeLogin] WITH PASSWORD=N'$passwordLiteral';
"@ 'master'

    Invoke-WindowsSqlNonQuery @"
IF USER_ID(N'$loginLiteral') IS NULL
    CREATE USER [$safeLogin] FOR LOGIN [$safeLogin];
IF IS_ROLEMEMBER(N'db_owner', N'$loginLiteral') <> 1
    ALTER ROLE [db_owner] ADD MEMBER [$safeLogin];
"@ $DatabaseName
    Write-Ok 'Dedicated SQL login is ready for server and clients'
}

function New-Pbkdf2Hash([string]$Password) {
    $salt = New-Object byte[] 16
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($salt) } finally { $rng.Dispose() }
    $derive = New-Object Security.Cryptography.Rfc2898DeriveBytes($Password, $salt, 100000)
    try { $hash = $derive.GetBytes(32) } finally { $derive.Dispose() }
    return 'PBKDF2$1$100000$' + [Convert]::ToBase64String($salt) + '$' + [Convert]::ToBase64String($hash)
}

function Ensure-InitialPosAdmin {
    $count = [int](Invoke-WindowsSqlScalar 'SELECT COUNT(*) FROM dbo.Users;' $DatabaseName)
    if ($count -gt 0) {
        Write-Ok 'Existing POS users detected; no new admin account created'
        return
    }

    Write-Step 'Creating first POS administrator account'
    do { $username = Read-Host 'POS Admin username' } while ([string]::IsNullOrWhiteSpace($username))
    $password = Read-ConfirmedPassword 'POS Admin'
    $hash = New-Pbkdf2Hash $password

    $connection = New-WindowsSqlConnection $DatabaseName
    try {
        $command = $connection.CreateCommand()
        $command.CommandText = 'INSERT INTO dbo.Users(username,password_hash,role,created_at) VALUES(@u,@p,@r,SYSUTCDATETIME());'
        [void]$command.Parameters.Add('@u',[System.Data.SqlDbType]::NVarChar,100)
        [void]$command.Parameters.Add('@p',[System.Data.SqlDbType]::NVarChar,500)
        [void]$command.Parameters.Add('@r',[System.Data.SqlDbType]::NVarChar,50)
        $command.Parameters['@u'].Value = $username
        $command.Parameters['@p'].Value = $hash
        $command.Parameters['@r'].Value = 'admin'
        [void]$command.ExecuteNonQuery()
    }
    finally { $connection.Dispose() }
    Write-Ok "POS administrator '$username' created"
}

function Test-AppConnection([string]$ServerAddress, [string]$Password) {
    $connection = New-AppSqlConnection $ServerAddress $Password $DatabaseName
    try {
        $command = $connection.CreateCommand()
        $command.CommandText = 'SELECT 1;'
        [void]$command.ExecuteScalar()
    }
    finally { $connection.Dispose() }
    Write-Ok "Connected to $ServerAddress,$SqlPort / $DatabaseName"
}

function Build-AppConnectionString([string]$ServerAddress, [string]$Password) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = "$ServerAddress,$SqlPort"
    $builder['Initial Catalog'] = $DatabaseName
    $builder['User ID'] = $DatabaseLogin
    $builder['Password'] = $Password
    $builder['Integrated Security'] = $false
    $builder['TrustServerCertificate'] = $true
    $builder.ConnectTimeout = 10
    return $builder.ConnectionString
}

function Stop-RunningApplication {
    Get-Process -Name 'Pos_System' -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Step 'Closing running POS application before update'
        $_.CloseMainWindow() | Out-Null
        if (-not $_.WaitForExit(5000)) { Stop-Process -Id $_.Id -Force }
    }
}

function Install-ApplicationFiles([string]$ConnectionString) {
    Write-Step 'Installing desktop application files'
    if (-not (Test-Path $AppSource)) { throw 'Installer package is missing the App folder.' }
    Stop-RunningApplication

    $appTarget = Join-Path $InstallRoot 'App'
    New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
    New-Item -ItemType Directory -Force -Path $appTarget | Out-Null
    Copy-Item -Path (Join-Path $AppSource '*') -Destination $appTarget -Recurse -Force

    $escapedConnectionString = Escape-Xml $ConnectionString
    $databaseConfig = @"
<connectionStrings>
  <add name="Pos_System.Properties.Settings.pos_systemConnectionString" connectionString="$escapedConnectionString" providerName="System.Data.SqlClient" />
</connectionStrings>
"@
    Set-Content -Path (Join-Path $appTarget 'Database.config') -Value $databaseConfig -Encoding UTF8
    Write-Ok "Application installed to $appTarget"
}

function Create-DesktopShortcut {
    $appTarget = Join-Path $InstallRoot 'App'
    $targetExe = Join-Path $appTarget 'Pos_System.exe'
    if (-not (Test-Path $targetExe)) { throw "Application executable not found: $targetExe" }
    Write-Step 'Creating desktop shortcut'
    $shortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) ($ShortcutName + '.lnk')
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $targetExe
    $shortcut.WorkingDirectory = $appTarget
    $shortcut.Description = $ShortcutName
    $shortcut.IconLocation = "$targetExe,0"
    $shortcut.Save()
    Write-Ok $shortcutPath
}

function Start-InstalledApp {
    if (-not $LaunchAfterInstall) { return }
    $appTarget = Join-Path $InstallRoot 'App'
    $targetExe = Join-Path $appTarget 'Pos_System.exe'
    Write-Step 'Starting Bike Zone POS'
    Start-Process -FilePath $targetExe -WorkingDirectory $appTarget
}

Load-InstallerConfig
$selectedRole = Select-Role

Write-Host '=================================================' -ForegroundColor DarkCyan
Write-Host ' Bike Zone POS - LAN Server / Client Installer' -ForegroundColor Cyan
Write-Host '=================================================' -ForegroundColor DarkCyan
Write-Host " Selected role: $selectedRole" -ForegroundColor White

if ($selectedRole -eq 'Server') {
    Write-Step 'Checking local SQL Server'
    Ensure-SqlServer
    Configure-SqlForLan

    $databasePassword = Read-ConfirmedPassword 'Database'
    New-DatabaseFromSql
    Apply-AllMigrations
    Ensure-DatabaseLogin $databasePassword
    Ensure-InitialPosAdmin

    $serverName = $env:COMPUTERNAME
    Test-AppConnection '127.0.0.1' $databasePassword
    $connectionString = Build-AppConnectionString '127.0.0.1' $databasePassword
    Install-ApplicationFiles $connectionString
    Create-DesktopShortcut

    Write-Host ''
    Write-Host 'SERVER INSTALL COMPLETED' -ForegroundColor Green
    Write-Host "Database server name: $serverName" -ForegroundColor Green
    Write-Host "LAN SQL port:         $SqlPort" -ForegroundColor Green
    Write-Host "Database:             $DatabaseName" -ForegroundColor Green
    Write-Host "Database login:       $DatabaseLogin" -ForegroundColor Green
    Write-Host 'Use the SAME database password when installing client PCs.' -ForegroundColor Yellow
    Write-Host "Clients may use server name '$serverName' or this server LAN IPv4 address." -ForegroundColor Yellow
    Start-InstalledApp
}
else {
    Write-Step 'Client connection information'
    do { $serverAddress = Read-Host 'Enter SERVER computer name or LAN IP address' } while ([string]::IsNullOrWhiteSpace($serverAddress))
    $databasePassword = Get-PlainTextPassword 'Enter Database password created on SERVER'
    if ([string]::IsNullOrWhiteSpace($databasePassword)) { throw 'Database password cannot be empty.' }

    Write-Step 'Testing LAN database connection'
    Test-AppConnection $serverAddress $databasePassword
    $connectionString = Build-AppConnectionString $serverAddress $databasePassword
    Install-ApplicationFiles $connectionString
    Create-DesktopShortcut

    Write-Host ''
    Write-Host 'CLIENT INSTALL COMPLETED' -ForegroundColor Green
    Write-Host "Connected server: $serverAddress,$SqlPort" -ForegroundColor Green
    Write-Host "Database:         $DatabaseName" -ForegroundColor Green
    Start-InstalledApp
}
