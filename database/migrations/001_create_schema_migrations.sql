/*
  POS System Database Migration 001
  Purpose: introduce a central migration history table without changing business data.
  Safe to run more than once.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaMigrations
    (
        migration_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaMigrations PRIMARY KEY,
        migration_key NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        applied_at DATETIME2 NOT NULL CONSTRAINT DF_SchemaMigrations_applied_at DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_SchemaMigrations_migration_key UNIQUE (migration_key)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.SchemaMigrations
    WHERE migration_key = N'001_create_schema_migrations'
)
BEGIN
    INSERT INTO dbo.SchemaMigrations (migration_key, description)
    VALUES (N'001_create_schema_migrations', N'Create central database migration history table');
END;

COMMIT TRANSACTION;
