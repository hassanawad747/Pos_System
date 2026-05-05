Bike Zone POS installer package

How to install on another PC:

1. Copy this whole folder to the other PC.
2. Make sure SQL Server Express is installed with instance name SQLEXPRESS.
3. Right click install_pos_system.bat.
4. Choose Run as administrator.

The installer will:
- Start SQL Server Express service.
- Copy the POS application to C:\BikeZonePOS\App.
- Restore pos_system.bak as database pos_system.
- Create a desktop shortcut named Bike Zone POS.
- Open the POS application.

If the installer says sqlcmd.exe is missing, install Microsoft SQL Server Command Line Utilities and run it again.
