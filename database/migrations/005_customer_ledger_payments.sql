/* POS System Database Migration 005 - customer ledger and payments */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
    THROW 51300, 'Migration 001 must be applied first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'004_supplier_ledger_payments')
    THROW 51301, 'Migration 004 must be applied first.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'005_customer_ledger_payments')
BEGIN
    IF OBJECT_ID(N'dbo.CustomerTransactions', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CustomerTransactions
        (
            customer_transaction_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerTransactions PRIMARY KEY,
            customer_id INT NOT NULL,
            transaction_type NVARCHAR(30) NOT NULL,
            reference_type NVARCHAR(30) NULL,
            reference_id INT NULL,
            debit DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerTransactions_debit DEFAULT (0),
            credit DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerTransactions_credit DEFAULT (0),
            balance_after DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerTransactions_balance DEFAULT (0),
            currency NVARCHAR(10) NOT NULL CONSTRAINT DF_CustomerTransactions_currency DEFAULT (N'USD'),
            description NVARCHAR(1000) NULL,
            user_id INT NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_CustomerTransactions_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_CustomerTransactions_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
            CONSTRAINT FK_CustomerTransactions_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_CustomerTransactions_amounts CHECK (debit >= 0 AND credit >= 0)
        );
        CREATE INDEX IX_CustomerTransactions_customer_created ON dbo.CustomerTransactions(customer_id, created_at);
        CREATE INDEX IX_CustomerTransactions_reference ON dbo.CustomerTransactions(reference_type, reference_id);
    END;

    IF OBJECT_ID(N'dbo.CustomerPayments', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CustomerPayments
        (
            customer_payment_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerPayments PRIMARY KEY,
            customer_id INT NOT NULL,
            amount DECIMAL(18,2) NOT NULL,
            currency NVARCHAR(10) NOT NULL CONSTRAINT DF_CustomerPayments_currency DEFAULT (N'USD'),
            payment_method NVARCHAR(50) NOT NULL CONSTRAINT DF_CustomerPayments_method DEFAULT (N'CASH'),
            reference_number NVARCHAR(100) NULL,
            notes NVARCHAR(1000) NULL,
            user_id INT NOT NULL,
            payment_date DATETIME2 NOT NULL CONSTRAINT DF_CustomerPayments_date DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_CustomerPayments_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
            CONSTRAINT FK_CustomerPayments_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_CustomerPayments_amount CHECK (amount > 0)
        );
        CREATE INDEX IX_CustomerPayments_customer_date ON dbo.CustomerPayments(customer_id, payment_date);
    END;

    INSERT INTO dbo.SchemaMigrations(migration_key, description)
    VALUES(N'005_customer_ledger_payments', N'Create customer transaction ledger and customer payment tables');
END;

COMMIT TRANSACTION;
