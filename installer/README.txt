POS System installer package

Files in this folder:
- App = application files
- Database = SQL backup file
- install_pos_system.bat = easiest installer to run
- install_pos_system.ps1 = installer script
- INSTALL_STEPS.txt = full manual steps
- restore_database.sql = manual SQL restore script

Client database connection:
- The installer updates App\Database.config automatically for MSSQLSERVER or SQLEXPRESS.
- Change only App\Database.config manually when the SQL Server name is different.
- Do not edit Pos_System.exe.config for the database connection.

Quick install on another PC:

1. Copy this whole folder to the other PC.
2. Install SQL Server Database Engine or SQL Server Express.
3. Make sure SQL Server service exists: MSSQLSERVER or SQLEXPRESS.
4. Install sqlcmd if it is not already installed.
5. Right click install_pos_system.bat.
6. Choose Run as administrator.

The installer will:
- Copy the app to C:\BikeZonePOS\App
- Copy the backup to C:\BikeZonePOS\Database
- Restore database pos_system
- Create a desktop shortcut
- Open the application

If automatic install fails, open INSTALL_STEPS.txt and follow the manual steps.
