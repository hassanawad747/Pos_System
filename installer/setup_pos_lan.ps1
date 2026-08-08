param(
    [ValidateSet('Interactive','Server','Client')]
    [string]$Role = 'Interactive'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppSource = Join-Path $Root 'App'
$BaseSchemaPackaged = Join-Path $Root 'Database\000_base_schema.sql'
$MigrationsPackaged = Join-Path $Root 'Migrations'
$RepoRoot = Split-Path -Parent $Root
$BaseSchemaRepo = Join-Path $RepoRoot 'database\install\000_base_schema.sql'
$MigrationsRepo = Join-Path $RepoRoot 'database\migrations'
$ConfigPath = Join-Path $Root 'Installer.config.json'

$InstallRoot = 'C:\BikeZonePOS'
$DatabaseName = 'pos_system'
$SqlInstance = '.\SQLEXPRESS'
$SqlServiceName = 'MSSQL$SQLEXPRESS'
$SqlPort = 1433
$DatabaseLogin = 'bikezone_pos_app'
$ShortcutName = 'Bike Zone POS'
$LaunchAfterInstall = $true
$InstallSqlIfMissing = $true

function Step([string]$text) { Write-Host "`n== $text" -ForegroundColor Cyan }
function Ok([string]$text) { Write-Host "   OK: $text" -ForegroundColor Green }

function Load-Config {
    if (-not (Test-Path $ConfigPath)) { return }
    $cfg = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    if ($cfg.installRoot) { $script:InstallRoot = [string]$cfg.installRoot }
    if ($cfg.databaseName) { $script:DatabaseName = [string]$cfg.databaseName }
    if ($cfg.sqlInstance) { $script:SqlInstance = [string]$cfg.sqlInstance }
    if ($cfg.sqlServiceName) { $script:SqlServiceName = [string]$cfg.sqlServiceName }
    if ($cfg.sqlPort) { $script:SqlPort = [int]$cfg.sqlPort }
    if ($cfg.databaseLogin) { $script:DatabaseLogin = [string]$cfg.databaseLogin }
    if ($cfg.shortcutName) { $script:ShortcutName = [string]$cfg.shortcutName }
    if ($null -ne $cfg.launchAfterInstall) { $script:LaunchAfterInstall = [bool]$cfg.launchAfterInstall }
    if ($null -ne $cfg.installSqlIfMissing) { $script:InstallSqlIfMissing = [bool]$cfg.installSqlIfMissing }
}

function Choose-Role {
    if ($Role -ne 'Interactive') { return $Role }
    Write-Host "`nChoose this computer's role:" -ForegroundColor White
    Write-Host '  1 - SERVER  (SQL database + POS application)'
    Write-Host '  2 - CLIENT  (POS application only; database stays on server)'
    while ($true) {
        $answer = Read-Host 'Choose 1 or 2'
        if ($answer -eq '1') { return 'Server' }
        if ($answer -eq '2') { return 'Client' }
        Write-Host 'Please enter 1 or 2.' -ForegroundColor Yellow
    }
}

function Read-Secret([string]$prompt) {
    $secure = Read-Host $prompt -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

function Read-NewPassword([string]$name) {
    while ($true) {
        $a = Read-Secret "Enter $name password"
        $b = Read-Secret "Confirm $name password"
        if ([string]::IsNullOrWhiteSpace($a)) { Write-Host 'Password cannot be empty.' -ForegroundColor Yellow; continue }
        if ($a -ne $b) { Write-Host 'Passwords do not match.' -ForegroundColor Yellow; continue }
        return $a
    }
}

function SqlLiteral([string]$value) { if ($null -eq $value) { return '' }; return $value.Replace("'", "''") }
function XmlEscape([string]$value) { return [Security.SecurityElement]::Escape($value) }

function Find-SqlInstance {
    $services = @(Get-Service -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq 'MSSQLSERVER' -or $_.Name -like 'MSSQL$*' })
    if ($services.Count -eq 0) { return $null }
    $svc = $services | Sort-Object @{ Expression = { if ($_.Name -eq 'MSSQL$SQLEXPRESS') {0} elseif ($_.Name -eq 'MSSQLSERVER') {1} else {2} } }, Name | Select-Object -First 1
    if ($svc.Name -eq 'MSSQLSERVER') {
        return [PSCustomObject]@{ Service='MSSQLSERVER'; DataSource='.'; ShortName='MSSQLSERVER' }
    }
    $short = $svc.Name.Substring('MSSQL$'.Length)
    return [PSCustomObject]@{ Service=$svc.Name; DataSource=".\$short"; ShortName=$short }
}

function Ensure-SqlEngine {
    $found = Find-SqlInstance
    if (-not $found -and $InstallSqlIfMissing) {
        $bootstrap = Join-Path $Root 'SQL2022-SSEI-Expr.exe'
        if (Test-Path $bootstrap) {
            Step 'SQL Server is not installed'
            Write-Host 'SQL Server Express setup will open. Install the Database Engine, then continue.' -ForegroundColor Yellow
            Start-Process $bootstrap -Wait
            Start-Sleep -Seconds 3
            $found = Find-SqlInstance
        }
    }
    if (-not $found) { throw 'SQL Server Database Engine was not found on this SERVER computer.' }

    $script:SqlServiceName = $found.Service
    $script:SqlInstance = $found.DataSource
    $svc = Get-Service $SqlServiceName
    if ($svc.Status -ne 'Running') {
        Start-Service $SqlServiceName
        (Get-Service $SqlServiceName).WaitForStatus('Running','00:00:45')
    }
    Ok "SQL service $SqlServiceName; local source $SqlInstance"
}

function New-WindowsConnection([string]$db='master') {
    $cs = "Data Source=$SqlInstance;Initial Catalog=$db;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15"
    $c = New-Object Data.SqlClient.SqlConnection $cs
    $c.Open()
    return $c
}

function Sql-NonQuery([string]$sql,[string]$db='master',[int]$timeout=300) {
    $c = New-WindowsConnection $db
    try { $cmd=$c.CreateCommand(); $cmd.CommandTimeout=$timeout; $cmd.CommandText=$sql; [void]$cmd.ExecuteNonQuery() }
    finally { $c.Dispose() }
}

function Sql-Scalar([string]$sql,[string]$db='master') {
    $c = New-WindowsConnection $db
    try { $cmd=$c.CreateCommand(); $cmd.CommandText=$sql; return $cmd.ExecuteScalar() }
    finally { $c.Dispose() }
}

function Sql-File([string]$path,[string]$db) {
    $text = Get-Content $path -Raw
    $batches = [regex]::Split($text,'(?im)^\s*GO\s*(?:--.*)?$')
    $c = New-WindowsConnection $db
    try {
        foreach ($batch in $batches) {
            if ([string]::IsNullOrWhiteSpace($batch)) { continue }
            $cmd=$c.CreateCommand(); $cmd.CommandTimeout=300; $cmd.CommandText=$batch; [void]$cmd.ExecuteNonQuery()
        }
    }
    finally { $c.Dispose() }
}

function Base-Schema-Path {
    if (Test-Path $BaseSchemaPackaged) { return $BaseSchemaPackaged }
    if (Test-Path $BaseSchemaRepo) { return $BaseSchemaRepo }
    throw 'Database\000_base_schema.sql is missing.'
}
function Migrations-Path {
    if (Test-Path $MigrationsPackaged) { return $MigrationsPackaged }
    if (Test-Path $MigrationsRepo) { return $MigrationsRepo }
    throw 'Migrations folder is missing.'
}

function Ensure-Database {
    $dbLiteral = SqlLiteral $DatabaseName
    $exists = [int](Sql-Scalar "SELECT CASE WHEN DB_ID(N'$dbLiteral') IS NULL THEN 0 ELSE 1 END" 'master')
    if ($exists -eq 0) {
        Step 'Creating database from SQL script'
        $safeDb = $DatabaseName.Replace(']',']]')
        Sql-NonQuery "CREATE DATABASE [$safeDb];" 'master'
        Sql-File (Base-Schema-Path) $DatabaseName
        Ok 'Base database schema created from 000_base_schema.sql'
    } else {
        Ok 'Existing database detected; data will be preserved'
    }
}

function Apply-Migrations {
    Step 'Applying versioned SQL updates'
    $files = @(Get-ChildItem (Migrations-Path) -Filter '*.sql' -File | Sort-Object Name)
    if ($files.Count -eq 0) { throw 'No migration SQL files found.' }
    foreach ($f in $files) { Write-Host "   -> $($f.Name)"; Sql-File $f.FullName $DatabaseName }
    Ok "$($files.Count) migration script(s) processed"
}

function Registry-Dword([string]$path,[string]$name,[int]$value) {
    if (-not (Test-Path $path)) { throw "Registry path not found: $path" }
    $prop = Get-ItemProperty -Path $path -Name $name -ErrorAction SilentlyContinue
    if ($null -eq $prop) { New-ItemProperty -Path $path -Name $name -PropertyType DWord -Value $value -Force | Out-Null }
    else { Set-ItemProperty -Path $path -Name $name -Value $value }
}

function Sql-Registry-InstanceId {
    $found = Find-SqlInstance
    $names='HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL'
    if (-not $found -or -not (Test-Path $names)) { return $null }
    $props=Get-ItemProperty $names
    return [string]$props.($found.ShortName)
}

function Configure-LanSql {
    Step 'Enabling SQL Server LAN access'
    $id=Sql-Registry-InstanceId
    if ([string]::IsNullOrWhiteSpace($id)) { throw 'Could not resolve SQL Server registry instance ID.' }
    $server="HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$id\MSSQLServer"
    Registry-Dword $server 'LoginMode' 2
    $tcp="$server\SuperSocketNetLib\Tcp"
    Registry-Dword $tcp 'Enabled' 1
    $ipAll=Join-Path $tcp 'IPAll'
    Set-ItemProperty $ipAll -Name TcpDynamicPorts -Value ''
    Set-ItemProperty $ipAll -Name TcpPort -Value ([string]$SqlPort)

    $rule="BikeZone POS SQL TCP $SqlPort"
    if (-not (Get-NetFirewallRule -DisplayName $rule -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName $rule -Direction Inbound -Protocol TCP -LocalPort $SqlPort -Action Allow -Profile Any | Out-Null
    }
    Restart-Service $SqlServiceName -Force
    (Get-Service $SqlServiceName).WaitForStatus('Running','00:00:45')
    Start-Sleep 2
    Ok "Mixed Mode + TCP port $SqlPort + firewall rule enabled"
}

function Ensure-AppLogin([string]$password) {
    Step 'Creating application database login'
    $loginLiteral=SqlLiteral $DatabaseLogin
    $passwordLiteral=SqlLiteral $password
    $safeLogin=$DatabaseLogin.Replace(']',']]')
    Sql-NonQuery @"
IF SUSER_ID(N'$loginLiteral') IS NULL
    CREATE LOGIN [$safeLogin] WITH PASSWORD=N'$passwordLiteral', CHECK_POLICY=ON, CHECK_EXPIRATION=OFF;
ELSE
    ALTER LOGIN [$safeLogin] WITH PASSWORD=N'$passwordLiteral';
"@ 'master'
    Sql-NonQuery @"
IF USER_ID(N'$loginLiteral') IS NULL CREATE USER [$safeLogin] FOR LOGIN [$safeLogin];
IF IS_ROLEMEMBER(N'db_owner',N'$loginLiteral') <> 1 ALTER ROLE [db_owner] ADD MEMBER [$safeLogin];
"@ $DatabaseName
    Ok "SQL login $DatabaseLogin ready"
}

function Pbkdf2([string]$password) {
    $salt=New-Object byte[] 16
    $rng=[Security.Cryptography.RandomNumberGenerator]::Create(); try{$rng.GetBytes($salt)}finally{$rng.Dispose()}
    $d=New-Object Security.Cryptography.Rfc2898DeriveBytes($password,$salt,100000); try{$hash=$d.GetBytes(32)}finally{$d.Dispose()}
    return 'PBKDF2$1$100000$'+[Convert]::ToBase64String($salt)+'$'+[Convert]::ToBase64String($hash)
}

function Ensure-FirstAdmin {
    $count=[int](Sql-Scalar 'SELECT COUNT(*) FROM dbo.Users;' $DatabaseName)
    if ($count -gt 0) { Ok 'Existing POS users preserved'; return }
    Step 'Creating first POS administrator'
    do{$username=Read-Host 'POS Admin username'}while([string]::IsNullOrWhiteSpace($username))
    $pw=Read-NewPassword 'POS Admin'
    $hash=Pbkdf2 $pw
    $c=New-WindowsConnection $DatabaseName
    try {
        $cmd=$c.CreateCommand(); $cmd.CommandText='INSERT dbo.Users(username,password_hash,role,created_at) VALUES(@u,@p,N''admin'',SYSUTCDATETIME())'
        [void]$cmd.Parameters.Add('@u',[Data.SqlDbType]::NVarChar,100); $cmd.Parameters['@u'].Value=$username
        [void]$cmd.Parameters.Add('@p',[Data.SqlDbType]::NVarChar,500); $cmd.Parameters['@p'].Value=$hash
        [void]$cmd.ExecuteNonQuery()
    } finally {$c.Dispose()}
    Ok "POS admin '$username' created"
}

function App-ConnectionString([string]$server,[string]$password) {
    $b=New-Object Data.SqlClient.SqlConnectionStringBuilder
    $b.DataSource="$server,$SqlPort"; $b.InitialCatalog=$DatabaseName; $b.UserID=$DatabaseLogin; $b.Password=$password
    $b.IntegratedSecurity=$false; $b.TrustServerCertificate=$true; $b.ConnectTimeout=10
    return $b.ConnectionString
}

function Test-AppDb([string]$server,[string]$password) {
    $c=New-Object Data.SqlClient.SqlConnection (App-ConnectionString $server $password)
    try{$c.Open(); $cmd=$c.CreateCommand(); $cmd.CommandText='SELECT 1'; [void]$cmd.ExecuteScalar()}
    finally{$c.Dispose()}
    Ok "Database connection succeeded: $server,$SqlPort"
}

function Install-App([string]$connectionString) {
    if (-not (Test-Path $AppSource)) { throw 'App folder is missing from installer package.' }
    Step 'Installing desktop application'
    Get-Process Pos_System -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    $target=Join-Path $InstallRoot 'App'; New-Item -ItemType Directory -Force $target | Out-Null
    Copy-Item (Join-Path $AppSource '*') $target -Recurse -Force
    $xml=XmlEscape $connectionString
    @"
<connectionStrings>
  <add name="Pos_System.Properties.Settings.pos_systemConnectionString" connectionString="$xml" providerName="System.Data.SqlClient" />
</connectionStrings>
"@ | Set-Content (Join-Path $target 'Database.config') -Encoding UTF8
    Ok "Application files installed: $target"
}

function Shortcut {
    $target=Join-Path $InstallRoot 'App\Pos_System.exe'
    $desktop=[Environment]::GetFolderPath('Desktop'); $path=Join-Path $desktop ($ShortcutName+'.lnk')
    $shell=New-Object -ComObject WScript.Shell; $s=$shell.CreateShortcut($path); $s.TargetPath=$target; $s.WorkingDirectory=(Split-Path $target); $s.IconLocation="$target,0"; $s.Save()
    Ok 'Desktop shortcut created'
}

function Launch {
    if ($LaunchAfterInstall) { $exe=Join-Path $InstallRoot 'App\Pos_System.exe'; Start-Process $exe -WorkingDirectory (Split-Path $exe) }
}

function Lan-IPv4 {
    return @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254*' } | Select-Object -ExpandProperty IPAddress)
}

Load-Config
$selected=Choose-Role
Write-Host "`n=================================================" -ForegroundColor DarkCyan
Write-Host ' Bike Zone POS - SQL-Only LAN Setup' -ForegroundColor Cyan
Write-Host " Role: $selected" -ForegroundColor White
Write-Host '=================================================' -ForegroundColor DarkCyan

if ($selected -eq 'Server') {
    Ensure-SqlEngine
    Configure-LanSql
    $dbPassword=Read-NewPassword 'Database'
    Ensure-Database
    Apply-Migrations
    Ensure-AppLogin $dbPassword
    Ensure-FirstAdmin
    Test-AppDb '127.0.0.1' $dbPassword
    Install-App (App-ConnectionString '127.0.0.1' $dbPassword)
    Shortcut
    Write-Host "`nSERVER READY" -ForegroundColor Green
    Write-Host "Computer name : $env:COMPUTERNAME"
    foreach($ip in (Lan-IPv4)){Write-Host "LAN IP        : $ip"}
    Write-Host "SQL port      : $SqlPort"
    Write-Host "Database      : $DatabaseName"
    Write-Host "SQL login     : $DatabaseLogin"
    Write-Host 'Use the same Database password on every CLIENT installer.' -ForegroundColor Yellow
    Launch
} else {
    Step 'Client database settings'
    do{$server=Read-Host 'Server computer name or LAN IP'}while([string]::IsNullOrWhiteSpace($server))
    $dbPassword=Read-Secret 'Database password from SERVER setup'
    if([string]::IsNullOrWhiteSpace($dbPassword)){throw 'Database password cannot be empty.'}
    Test-AppDb $server $dbPassword
    Install-App (App-ConnectionString $server $dbPassword)
    Shortcut
    Write-Host "`nCLIENT READY - connected to $server,$SqlPort" -ForegroundColor Green
    Launch
}
