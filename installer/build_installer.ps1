param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
if([string]::IsNullOrWhiteSpace($OutputDirectory)){$OutputDirectory=Join-Path $repo 'dist\BikeZonePOS-Installer'}
$release=Join-Path $repo 'bin\Release'
if(-not(Test-Path (Join-Path $release 'Pos_System.exe'))){throw 'Build bin\Release\Pos_System.exe before packaging.'}
if(Test-Path $OutputDirectory){Remove-Item -LiteralPath $OutputDirectory -Recurse -Force}
New-Item -ItemType Directory -Force -Path (Join-Path $OutputDirectory 'App'),(Join-Path $OutputDirectory 'Database'),(Join-Path $OutputDirectory 'Migrations')|Out-Null
Copy-Item (Join-Path $release '*') (Join-Path $OutputDirectory 'App') -Recurse -Force
$packagedDatabaseConfig=Join-Path $OutputDirectory 'App\Database.config'
if(Test-Path $packagedDatabaseConfig){Remove-Item -LiteralPath $packagedDatabaseConfig -Force}
Copy-Item (Join-Path $repo 'database\install\000_base_schema.sql') (Join-Path $OutputDirectory 'Database\000_base_schema.sql') -Force
Copy-Item (Join-Path $repo 'database\migrations\*.sql') (Join-Path $OutputDirectory 'Migrations') -Force
$files=@('install_pos_system.bat','setup_pos_lan.ps1','create_pos_icon.ps1','verify_database.ps1','Installer.config.json','README.txt','INSTALL_STEPS.txt')
foreach($name in $files){$source=Join-Path $PSScriptRoot $name;if(-not(Test-Path $source)){throw "Installer file missing: $name"};Copy-Item $source $OutputDirectory -Force}
$localized=@(Get-ChildItem $PSScriptRoot -Filter '*.txt' -File|Where-Object{$_.Name -notin @('README.txt','INSTALL_STEPS.txt')})
if($localized.Count -eq 0){throw 'Localized installer instructions are missing.'}
foreach($file in $localized){Copy-Item $file.FullName $OutputDirectory -Force}
$bootstrap=Join-Path $PSScriptRoot 'SQL2022-SSEI-Expr.exe';if(Test-Path $bootstrap){Copy-Item $bootstrap $OutputDirectory -Force}
$migrationNames=@(Get-ChildItem (Join-Path $repo 'database\migrations') -Filter '*.sql' -File|Sort-Object Name|Select-Object -ExpandProperty Name)
if($migrationNames.Count -eq 0){throw 'No migrations were packaged.'}
if(@(Get-ChildItem $OutputDirectory -Filter '*.bak' -File -Recurse).Count -ne 0){throw 'Installer package must not contain .bak files.'}
$version=@("BuildCommit=$(git -C $repo rev-parse HEAD)","BuildDateUtc=$([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ'))",'InstallModel=SQL_ONLY_LAN_SERVER_CLIENT','DatabaseBaseSchema=000_base_schema.sql',"DatabaseMigrations=$($migrationNames -join ',')",'ApplicationBuild=Release','BrandingIcon=BikeZonePOS.ico generated during install','DatabaseVerificationTool=verify_database.ps1','ModernizationTarget=net10.0-windows (validated; legacy production executable remains default until parity)')
$version|Set-Content (Join-Path $OutputDirectory 'VERSION.txt') -Encoding UTF8
Write-Host "Installer package ready: $OutputDirectory" -ForegroundColor Green
