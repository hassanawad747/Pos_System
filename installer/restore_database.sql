USE master;
GO

IF DB_ID(N'pos_system') IS NOT NULL
BEGIN
    ALTER DATABASE [pos_system] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [pos_system];
END
GO

RESTORE DATABASE [pos_system]
FROM DISK = N'C:\BikeZonePOS\Database\pos_system.bak'
WITH
    MOVE N'pos_system' TO N'C:\BikeZonePOS\Database\pos_system.mdf',
    MOVE N'pos_system_log' TO N'C:\BikeZonePOS\Database\pos_system_log.ldf',
    REPLACE,
    RECOVERY;
GO

ALTER DATABASE [pos_system] SET MULTI_USER;
GO
