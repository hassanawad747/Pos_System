POS System installer package

Files in this folder:
- App = application files
- Database = SQL backup file
- Installer.config.json = installer settings and optional connection string override
- install_pos_system.bat = easiest installer to run
- install_pos_system.ps1 = installer script
- INSTALL_STEPS.txt = full manual steps
- restore_database.sql = manual SQL restore script

Client database connection:
- The installer updates App\Database.config automatically.
- Edit Installer.config.json if you want to change SQL instance, install path, shortcut name, or connection string.
- If connectionString in Installer.config.json is empty, the installer builds it from sqlInstance and databaseName.
- If connectionString in Installer.config.json has a value, that exact connection string is written into App\Database.config.
- Do not edit Pos_System.exe.config for the database connection.

Quick install on another PC:

1. Copy this whole folder to the other PC.
2. Optional: open Installer.config.json and edit the SQL settings or connection string.
3. Install SQL Server Database Engine or SQL Server Express.
4. Make sure SQL Server service exists: MSSQLSERVER or SQLEXPRESS.
5. Install sqlcmd if it is not already installed.
6. Right click install_pos_system.bat.
7. Choose Run as administrator.

The installer will:
- Copy the app to C:\BikeZonePOS\App
- Copy the backup to C:\BikeZonePOS\Database
- Restore database pos_system
- Write App\Database.config from Installer.config.json
- Create a desktop shortcut
- Open the application

If automatic install fails, open INSTALL_STEPS.txt and follow the manual steps.
