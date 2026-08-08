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
            debit DECIMAL(24,8) NOT NULL CONSTRAINT DF_CustomerTransactions_debit DEFAULT (0),
            credit DECIMAL(24,8) NOT NULL CONSTRAINT DF_CustomerTransactions_credit DEFAULT (0),
            balance_after DECIMAL(24,8) NOT NULL CONSTRAINT DF_CustomerTransactions_balance DEFAULT (0),
            currency NVARCHAR(10) NOT NULL CONSTRAINT DF_CustomerTransactions_currency DEFAULT (N'USD'),
            description NVARCHAR(1000) NULL,
            user_id INT NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_CustomerTransactions_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_CustomerTransactions_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
            CONSTRAINT FK_CustomerTransactions_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_CustomerTransactions_amounts CHECK (debit >= 0 AND credit >= 0)
        );
        CREATE INDEX IX_CustomerTransactions_customer_created ON dbo.CustomerTransactions(customer_id, currency, created_at);
        CREATE INDEX IX_CustomerTransactions_reference ON dbo.CustomerTransactions(reference_type, reference_id);
    END;

    IF OBJECT_ID(N'dbo.CustomerPayments', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CustomerPayments
        (
            customer_payment_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerPayments PRIMARY KEY,
            customer_id INT NOT NULL,
            amount DECIMAL(24,8) NOT NULL,
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
        CREATE INDEX IX_CustomerPayments_customer_date ON dbo.CustomerPayments(customer_id, currency, payment_date);
    END;

    /* Import legacy current balances once instead of replaying historical sales and double-counting. */
    IF COL_LENGTH('dbo.Customers', 'balance_usd') IS NOT NULL
    BEGIN
        EXEC(N'
            INSERT INTO dbo.CustomerTransactions
                (customer_id, transaction_type, reference_type, reference_id, debit, credit, balance_after, currency, description, user_id, created_at)
            SELECT customer_id,
                   N''OPENING_BALANCE'', N''MIGRATION'', NULL,
                   CASE WHEN balance_usd > 0 THEN balance_usd ELSE 0 END,
                   CASE WHEN balance_usd < 0 THEN ABS(balance_usd) ELSE 0 END,
                   balance_usd, N''USD'',
                   N''Opening USD balance imported from legacy customer balance'', NULL, SYSUTCDATETIME()
            FROM dbo.Customers
            WHERE ISNULL(balance_usd, 0) <> 0;
        ');
    END;

    IF COL_LENGTH('dbo.Customers', 'balance_lb') IS NOT NULL
    BEGIN
        EXEC(N'
            INSERT INTO dbo.CustomerTransactions
                (customer_id, transaction_type, reference_type, reference_id, debit, credit, balance_after, currency, description, user_id, created_at)
            SELECT customer_id,
                   N''OPENING_BALANCE'', N''MIGRATION'', NULL,
                   CASE WHEN balance_lb > 0 THEN balance_lb ELSE 0 END,
                   CASE WHEN balance_lb < 0 THEN ABS(balance_lb) ELSE 0 END,
                   balance_lb, N''LBP'',
                   N''Opening LBP balance imported from legacy customer balance'', NULL, SYSUTCDATETIME()
            FROM dbo.Customers
            WHERE ISNULL(balance_lb, 0) <> 0;
        ');
    END;

    INSERT INTO dbo.SchemaMigrations(migration_key, description)
    VALUES(N'005_customer_ledger_payments', N'Create multi-currency customer transaction ledger and customer payment tables');
END;

COMMIT TRANSACTION;
GO

/* Future sales are recorded automatically. Sales.balance_usd/balance_lb identifies the selected transaction currency. */
CREATE OR ALTER TRIGGER dbo.TR_Sales_CustomerLedger
ON dbo.Sales
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ValidSales AS
    (
        SELECT i.sale_id,
               i.customer_id,
               CAST(ISNULL(i.total_amount, 0) AS DECIMAL(24,8)) AS total_amount,
               i.user_id,
               ISNULL(i.sale_date, SYSUTCDATETIME()) AS sale_date,
               CASE
                   WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL THEN N'LBP'
                   ELSE N'USD'
               END AS currency
        FROM inserted i
        WHERE i.customer_id IS NOT NULL AND ISNULL(i.total_amount, 0) > 0
    ),
    CurrentBalances AS
    (
        SELECT v.customer_id,
               v.currency,
               ISNULL((
                   SELECT TOP (1) ct.balance_after
                   FROM dbo.CustomerTransactions ct
                   WHERE ct.customer_id = v.customer_id
                     AND ct.currency = v.currency
                   ORDER BY ct.customer_transaction_id DESC
               ), 0) AS starting_balance
        FROM ValidSales v
        GROUP BY v.customer_id, v.currency
    ),
    OrderedSales AS
    (
        SELECT v.*,
               b.starting_balance +
               SUM(v.total_amount) OVER(PARTITION BY v.customer_id, v.currency ORDER BY v.sale_id ROWS UNBOUNDED PRECEDING) AS new_balance
        FROM ValidSales v
        INNER JOIN CurrentBalances b
            ON b.customer_id = v.customer_id AND b.currency = v.currency
    )
    INSERT INTO dbo.CustomerTransactions
        (customer_id, transaction_type, reference_type, reference_id, debit, credit, balance_after, currency, description, user_id, created_at)
    SELECT customer_id,
           N'SALE', N'SALE', sale_id,
           total_amount, 0, new_balance, currency,
           N'Sale invoice #' + CONVERT(NVARCHAR(30), sale_id),
           user_id, sale_date
    FROM OrderedSales;
END;
GO
