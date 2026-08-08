/* POS Migration 016 - security, reliability and operations */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'015_business_intelligence_views') THROW 52600,'Migration 015 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'016_security_operations')
BEGIN
    IF OBJECT_ID(N'dbo.ActionPermissions',N'U') IS NULL
    CREATE TABLE dbo.ActionPermissions(
        action_permission_id INT IDENTITY(1,1) PRIMARY KEY,
        role_name NVARCHAR(50) NOT NULL,
        action_key NVARCHAR(100) NOT NULL,
        is_allowed BIT NOT NULL CONSTRAINT DF_ActionPermissions_Allowed DEFAULT 0,
        updated_by INT NULL,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_ActionPermissions_Updated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ActionPermissions_User FOREIGN KEY(updated_by) REFERENCES dbo.Users(user_id),
        CONSTRAINT UQ_ActionPermissions UNIQUE(role_name,action_key)
    );

    IF OBJECT_ID(N'dbo.ErrorLogs',N'U') IS NULL
    CREATE TABLE dbo.ErrorLogs(
        error_log_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        occurred_at DATETIME2 NOT NULL CONSTRAINT DF_ErrorLogs_Occurred DEFAULT SYSUTCDATETIME(),
        user_id INT NULL,
        machine_name NVARCHAR(120) NULL,
        form_name NVARCHAR(120) NULL,
        operation NVARCHAR(120) NULL,
        message NVARCHAR(MAX) NOT NULL,
        stack_trace NVARCHAR(MAX) NULL,
        inner_message NVARCHAR(MAX) NULL,
        severity NVARCHAR(20) NOT NULL CONSTRAINT DF_ErrorLogs_Severity DEFAULT N'ERROR',
        CONSTRAINT FK_ErrorLogs_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );
    CREATE INDEX IX_ErrorLogs_Occurred ON dbo.ErrorLogs(occurred_at DESC);

    IF OBJECT_ID(N'dbo.PasswordMigrationAudit',N'U') IS NULL
    CREATE TABLE dbo.PasswordMigrationAudit(
        password_migration_audit_id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL,
        source_format NVARCHAR(40) NOT NULL,
        target_format NVARCHAR(40) NOT NULL,
        migrated_at DATETIME2 NOT NULL CONSTRAINT DF_PasswordMigrationAudit_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PasswordMigrationAudit_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );

    IF OBJECT_ID(N'dbo.DatabaseVerificationRuns',N'U') IS NULL
    CREATE TABLE dbo.DatabaseVerificationRuns(
        verification_run_id INT IDENTITY(1,1) PRIMARY KEY,
        verification_type NVARCHAR(30) NOT NULL,
        database_name NVARCHAR(120) NOT NULL,
        status NVARCHAR(20) NOT NULL,
        details NVARCHAR(MAX) NULL,
        machine_name NVARCHAR(120) NULL,
        user_id INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_DatabaseVerificationRuns_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DatabaseVerificationRuns_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );

    INSERT dbo.ActionPermissions(role_name,action_key,is_allowed)
    SELECT v.role_name,v.action_key,v.is_allowed
    FROM (VALUES
      (N'admin',N'SALE.CREATE',1),(N'admin',N'SALE.RETURN',1),(N'admin',N'SALE.DISCOUNT',1),(N'admin',N'PRODUCT.PRICE_EDIT',1),(N'admin',N'PRODUCT.STOCK_ADJUST',1),(N'admin',N'SUPPLIER.PAYMENT',1),(N'admin',N'CUSTOMER.ADJUSTMENT',1),(N'admin',N'USER.PERMISSION_EDIT',1),(N'admin',N'SETTINGS.EDIT',1),
      (N'cashier',N'SALE.CREATE',1),(N'cashier',N'SALE.RETURN',0),(N'cashier',N'SALE.DISCOUNT',0),(N'cashier',N'PRODUCT.PRICE_EDIT',0),(N'cashier',N'PRODUCT.STOCK_ADJUST',0),(N'cashier',N'SUPPLIER.PAYMENT',0),(N'cashier',N'CUSTOMER.ADJUSTMENT',0),(N'cashier',N'USER.PERMISSION_EDIT',0),(N'cashier',N'SETTINGS.EDIT',0)
    )v(role_name,action_key,is_allowed)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.ActionPermissions ap WHERE ap.role_name=v.role_name AND ap.action_key=v.action_key);

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'016_security_operations',N'Action permissions, central error logs, password migration audit and database verification history');
END;
COMMIT TRANSACTION;
GO
