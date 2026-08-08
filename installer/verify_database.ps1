param(
    [string]$Server = '127.0.0.1,1433',
    [string]$Database = 'pos_system',
    [string]$Login = 'bikezone_pos_app',
    [string]$Password,
    [switch]$IntegratedSecurity,
    [string]$BackupRoot,
    [switch]$KeepBackup
)

$ErrorActionPreference='Stop'
if(-not $IntegratedSecurity -and [string]::IsNullOrWhiteSpace($Password)){
    $secure=Read-Host 'Database password' -AsSecureString
    $ptr=[Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try{$Password=[Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)}finally{[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)}
}
if(-not $IntegratedSecurity -and [string]::IsNullOrWhiteSpace($Password)){throw 'Database password is required.'}

$builder=New-Object Data.SqlClient.SqlConnectionStringBuilder
$builder['Data Source']=$Server;$builder['Initial Catalog']=$Database;$builder['Integrated Security']=[bool]$IntegratedSecurity;$builder['TrustServerCertificate']=$true;$builder['Connect Timeout']=15
if(-not $IntegratedSecurity){$builder['User ID']=$Login;$builder['Password']=$Password}
if([string]::IsNullOrWhiteSpace($BackupRoot)){
    $masterBuilder=New-Object Data.SqlClient.SqlConnectionStringBuilder $builder.ConnectionString;$masterBuilder['Initial Catalog']='master'
    $c=New-Object Data.SqlClient.SqlConnection $masterBuilder.ConnectionString
    try{$c.Open();$cmd=$c.CreateCommand();$cmd.CommandText="SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(4000));";$BackupRoot=[string]$cmd.ExecuteScalar()}finally{$c.Dispose()}
}
if([string]::IsNullOrWhiteSpace($BackupRoot) -and $IntegratedSecurity){$BackupRoot=[IO.Path]::GetTempPath()}
if([string]::IsNullOrWhiteSpace($BackupRoot)){throw 'SQL Server default backup directory could not be resolved.'}
$stamp=[DateTime]::Now.ToString('yyyyMMdd_HHmmss')
$backup=Join-Path $BackupRoot ("${Database}_verify_${stamp}.bak")

function SqlLiteral([string]$value){return $value.Replace("'","''")}
function Run([string]$sql){$c=New-Object Data.SqlClient.SqlConnection $builder.ConnectionString;try{$c.Open();$cmd=$c.CreateCommand();$cmd.CommandTimeout=600;$cmd.CommandText=$sql;[void]$cmd.ExecuteNonQuery()}finally{$c.Dispose()}}

$status='FAILED';$details=''
try{
    Write-Host "Creating verification backup: $backup" -ForegroundColor Cyan
    $safePath=SqlLiteral $backup
    Run "BACKUP DATABASE [$Database] TO DISK=N'$safePath' WITH COPY_ONLY, INIT, CHECKSUM, STATS=10;"
    Write-Host 'Running RESTORE VERIFYONLY with CHECKSUM...' -ForegroundColor Cyan
    Run "RESTORE VERIFYONLY FROM DISK=N'$safePath' WITH CHECKSUM;"
    $status='SUCCESS';$details='BACKUP DATABASE + RESTORE VERIFYONLY WITH CHECKSUM succeeded.'
    Write-Host 'Database backup verification succeeded.' -ForegroundColor Green
}
catch{
    $details=$_.Exception.Message
    Write-Host ('Database verification failed: '+$details) -ForegroundColor Red
    throw
}
finally{
    try{
        $c=New-Object Data.SqlClient.SqlConnection $builder.ConnectionString
        $c.Open();$cmd=$c.CreateCommand();$cmd.CommandText=@"
IF OBJECT_ID(N'dbo.DatabaseVerificationRuns',N'U') IS NOT NULL
INSERT dbo.DatabaseVerificationRuns(verification_type,database_name,status,details,machine_name,created_at)
VALUES(N'BACKUP_RESTORE_VERIFY',@db,@status,@details,@machine,SYSUTCDATETIME());
"@
        [void]$cmd.Parameters.Add('@db',[Data.SqlDbType]::NVarChar,120);$cmd.Parameters['@db'].Value=$Database
        [void]$cmd.Parameters.Add('@status',[Data.SqlDbType]::NVarChar,20);$cmd.Parameters['@status'].Value=$status
        [void]$cmd.Parameters.Add('@details',[Data.SqlDbType]::NVarChar,-1);$cmd.Parameters['@details'].Value=$details
        [void]$cmd.Parameters.Add('@machine',[Data.SqlDbType]::NVarChar,120);$cmd.Parameters['@machine'].Value=$env:COMPUTERNAME
        [void]$cmd.ExecuteNonQuery();$c.Dispose()
    }catch{}
    if(-not $KeepBackup){try{if(Test-Path $backup -ErrorAction SilentlyContinue){Remove-Item $backup -Force -ErrorAction SilentlyContinue}}catch{}}
}
