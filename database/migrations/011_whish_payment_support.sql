/* POS System Database Migration 011 - Whish payment support foundation */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL
    THROW 51900,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'010_split_payment_checkout_support')
    THROW 51901,'Migration 010 must be applied first.',1;

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'011_whish_payment_support')
BEGIN
    IF COL_LENGTH('dbo.SalePayments','provider_name') IS NULL
        ALTER TABLE dbo.SalePayments ADD provider_name NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.SalePayments','payer_phone') IS NULL
        ALTER TABLE dbo.SalePayments ADD payer_phone NVARCHAR(40) NULL;
    IF COL_LENGTH('dbo.SalePayments','provider_status') IS NULL
        ALTER TABLE dbo.SalePayments ADD provider_status NVARCHAR(40) NULL;
    IF COL_LENGTH('dbo.SalePayments','provider_transaction_id') IS NULL
        ALTER TABLE dbo.SalePayments ADD provider_transaction_id NVARCHAR(150) NULL;
    IF COL_LENGTH('dbo.SalePayments','provider_message') IS NULL
        ALTER TABLE dbo.SalePayments ADD provider_message NVARCHAR(1000) NULL;
    IF COL_LENGTH('dbo.SalePayments','provider_verified_at') IS NULL
        ALTER TABLE dbo.SalePayments ADD provider_verified_at DATETIME2 NULL;

    IF OBJECT_ID(N'dbo.WhishPaymentAttempts',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.WhishPaymentAttempts
        (
            whish_attempt_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WhishPaymentAttempts PRIMARY KEY,
            sale_id INT NULL,
            payer_phone NVARCHAR(40) NOT NULL,
            amount DECIMAL(24,8) NOT NULL,
            currency NVARCHAR(10) NOT NULL,
            client_reference NVARCHAR(100) NOT NULL,
            provider_transaction_id NVARCHAR(150) NULL,
            provider_status NVARCHAR(40) NOT NULL,
            provider_message NVARCHAR(1000) NULL,
            requested_by INT NULL,
            requested_at DATETIME2 NOT NULL CONSTRAINT DF_WhishPaymentAttempts_requested DEFAULT SYSUTCDATETIME(),
            verified_at DATETIME2 NULL,
            CONSTRAINT FK_WhishPaymentAttempts_Sales FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
            CONSTRAINT FK_WhishPaymentAttempts_Users FOREIGN KEY(requested_by) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_WhishPaymentAttempts_amount CHECK(amount > 0),
            CONSTRAINT CK_WhishPaymentAttempts_currency CHECK(currency IN(N'USD',N'LBP'))
        );
        CREATE INDEX IX_WhishPaymentAttempts_sale ON dbo.WhishPaymentAttempts(sale_id,requested_at);
        CREATE INDEX IX_WhishPaymentAttempts_reference ON dbo.WhishPaymentAttempts(client_reference);
    END;

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'011_whish_payment_support',N'Add provider audit fields and Whish payment-attempt tracking');
END;

COMMIT TRANSACTION;
GO
