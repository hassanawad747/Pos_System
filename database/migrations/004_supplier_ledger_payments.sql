/* POS System Database Migration 004 - supplier ledger and payments */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
    THROW 51200, 'Migration 001 must be applied first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'003_purchases_inventory_ledger')
    THROW 51201, 'Migration 003 must be applied first.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'004_supplier_ledger_payments')
BEGIN
    IF OBJECT_ID(N'dbo.SupplierTransactions', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SupplierTransactions
        (
            supplier_transaction_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierTransactions PRIMARY KEY,
            supplier_id INT NOT NULL,
            transaction_type NVARCHAR(30) NOT NULL,
            reference_type NVARCHAR(30) NULL,
            reference_id INT NULL,
            debit DECIMAL(18,2) NOT NULL CONSTRAINT DF_SupplierTransactions_debit DEFAULT (0),
            credit DECIMAL(18,2) NOT NULL CONSTRAINT DF_SupplierTransactions_credit DEFAULT (0),
            balance_after DECIMAL(18,2) NOT NULL,
            currency NVARCHAR(10) NOT NULL CONSTRAINT DF_SupplierTransactions_currency DEFAULT (N'USD'),
            description NVARCHAR(1000) NULL,
            user_id INT NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_SupplierTransactions_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_SupplierTransactions_Suppliers FOREIGN KEY (supplier_id) REFERENCES dbo.Suppliers(supplier_id),
            CONSTRAINT FK_SupplierTransactions_Users FOREIGN KEY (user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_SupplierTransactions_amounts CHECK (debit >= 0 AND credit >= 0)
        );
    END;

    IF OBJECT_ID(N'dbo.SupplierPayments', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SupplierPayments
        (
            supplier_payment_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierPayments PRIMARY KEY,
            supplier_id INT NOT NULL,
            amount DECIMAL(18,2) NOT NULL,
            currency NVARCHAR(10) NOT NULL CONSTRAINT DF_SupplierPayments_currency DEFAULT (N'USD'),
            payment_method NVARCHAR(50) NULL,
            reference_number NVARCHAR(100) NULL,
            notes NVARCHAR(1000) NULL,
            user_id INT NOT NULL,
            payment_date DATETIME2 NOT NULL CONSTRAINT DF_SupplierPayments_date DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_SupplierPayments_Suppliers FOREIGN KEY (supplier_id) REFERENCES dbo.Suppliers(supplier_id),
            CONSTRAINT FK_SupplierPayments_Users FOREIGN KEY (user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_SupplierPayments_amount CHECK (amount > 0)
        );
    END;

    CREATE INDEX IX_SupplierTransactions_supplier_created ON dbo.SupplierTransactions(supplier_id, created_at);
    CREATE INDEX IX_SupplierTransactions_reference ON dbo.SupplierTransactions(reference_type, reference_id);
    CREATE INDEX IX_SupplierPayments_supplier_date ON dbo.SupplierPayments(supplier_id, payment_date);

    INSERT INTO dbo.SchemaMigrations(migration_key, description)
    VALUES(N'004_supplier_ledger_payments', N'Create supplier ledger and payment history tables');
END;

COMMIT TRANSACTION;