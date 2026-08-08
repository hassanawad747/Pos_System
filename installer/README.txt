Bike Zone POS - Install / Update Package

Main files:
- App\ = latest compiled desktop application (Pos_System.exe + DLLs)
- Database\pos_system.bak = baseline database used ONLY for a fresh device
- Migrations\ = all versioned SQL database updates
- Installer.config.json = SQL/install settings
- install_pos_system.bat = main entry point (Run as administrator)
- install_pos_system.ps1 = PowerShell install/update engine
- VERSION.txt = generated build commit/date/migration list in GitHub artifact

Recommended command:
  install_pos_system.bat

AUTO MODE:
- If database pos_system does not exist: Fresh Install
  1. Restore Database\pos_system.bak
  2. Run every migration in Migrations\ in filename order
  3. Install latest App files
  4. Write Database.config
  5. Create desktop shortcut
  6. Start application

- If database pos_system already exists: Safe Upgrade
  1. DO NOT restore the baseline .bak over live data
  2. Create a pre-upgrade SQL backup
  3. Run every migration in Migrations\ in filename order
  4. Save previous application files under C:\BikeZonePOS\PreviousVersions
  5. Install latest App files
  6. Start application

Explicit modes:
  install_pos_system.bat install
  install_pos_system.bat upgrade

Default locations:
- C:\BikeZonePOS\App
- C:\BikeZonePOS\Database
- C:\BikeZonePOS\Database\Backups
- C:\BikeZonePOS\PreviousVersions

SQL Server:
- Existing SQL Server / SQL Server Express instances are auto-detected.
- sqlcmd.exe is NOT required by the new installer.
- If SQL Server is missing and SQL2022-SSEI-Expr.exe is included, the installer opens the Microsoft SQL Server Express installer.

Connection:
- Installer.config.json can override SQL instance/database/connection string.
- App\Database.config is generated automatically for the detected target.

Release process:
Every successful push build on the development branch creates a GitHub Actions artifact named:
  BikeZonePOS-Installer

The artifact contains the compiled application, database baseline, ALL migration scripts, setup scripts and version information. Future database migrations are included automatically; the workflow does not require a hard-coded migration list.
