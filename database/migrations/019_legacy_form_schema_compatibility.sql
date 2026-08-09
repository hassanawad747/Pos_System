/* POS Migration 019 - columns required by the maintained customer and user screens */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL
    THROW 52900,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'018_checkout_inventory_integration')
    THROW 52901,'Migration 018 must be applied first.',1;

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'019_legacy_form_schema_compatibility')
BEGIN
    IF COL_LENGTH('dbo.Customers','created_by') IS NULL
        ALTER TABLE dbo.Customers ADD created_by NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.Users','created_by') IS NULL
        ALTER TABLE dbo.Users ADD created_by NVARCHAR(100) NULL;

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'019_legacy_form_schema_compatibility',N'Add creator audit columns required by customer and user management screens');
END;

COMMIT TRANSACTION;
GO
