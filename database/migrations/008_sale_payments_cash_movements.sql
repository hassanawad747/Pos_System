/* POS System Database Migration 008 - sale payments and automatic cash movement integration */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL THROW 51600,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'007_cash_register_sessions') THROW 51601,'Migration 007 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'008_sale_payments_cash_movements')
BEGIN
    IF OBJECT_ID(N'dbo.SalePayments',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SalePayments(
            sale_payment_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalePayments PRIMARY KEY,
            sale_id INT NOT NULL,
            payment_method NVARCHAR(50) NOT NULL,
            amount DECIMAL(24,8) NOT NULL,
            currency NVARCHAR(10) NOT NULL,
            exchange_rate DECIMAL(24,8) NULL,
            reference_number NVARCHAR(100) NULL,
            cash_session_id INT NULL,
            is_legacy_auto BIT NOT NULL CONSTRAINT DF_SalePayments_legacy DEFAULT(0),
            created_at DATETIME2 NOT NULL CONSTRAINT DF_SalePayments_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_SalePayments_Sale FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
            CONSTRAINT FK_SalePayments_Session FOREIGN KEY(cash_session_id) REFERENCES dbo.CashSessions(cash_session_id),
            CONSTRAINT CK_SalePayments_amount CHECK(amount > 0),
            CONSTRAINT CK_SalePayments_currency CHECK(currency IN(N'USD',N'LBP'))
        );
        CREATE INDEX IX_SalePayments_sale ON dbo.SalePayments(sale_id);
        CREATE INDEX IX_SalePayments_session ON dbo.SalePayments(cash_session_id,created_at);
    END;
    INSERT dbo.SchemaMigrations(migration_key,description) VALUES(N'008_sale_payments_cash_movements',N'Create split sale payments and integrate cash movements');
END;
COMMIT TRANSACTION;
GO

/* Create one payment record automatically for the current legacy Sales screen.
   Later the split-payment UI can replace this legacy-auto row with multiple explicit rows. */
CREATE OR ALTER TRIGGER dbo.TR_Sales_DefaultSalePayment
ON dbo.Sales
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SalePayments(sale_id,payment_method,amount,currency,exchange_rate,cash_session_id,is_legacy_auto,created_at)
    SELECT i.sale_id,
           ISNULL(NULLIF(i.payment_method,N''),N'CASH'),
           CAST(CASE
                WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL
                    THEN CASE WHEN ISNULL(i.balance_lb,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_lb ELSE ISNULL(i.total_amount,0) END
                ELSE CASE WHEN ISNULL(i.balance_usd,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_usd ELSE ISNULL(i.total_amount,0) END
           END AS DECIMAL(24,8)),
           CASE WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END,
           NULL,
           (SELECT TOP(1) cs.cash_session_id FROM dbo.CashSessions cs WHERE cs.user_id=i.user_id AND cs.status=N'OPEN' ORDER BY cs.opened_at DESC),
           1,
           ISNULL(i.sale_date,SYSUTCDATETIME())
    FROM inserted i
    WHERE ISNULL(i.total_amount,0)>0
      AND (CASE
            WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL THEN CASE WHEN ISNULL(i.balance_lb,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_lb ELSE ISNULL(i.total_amount,0) END
            ELSE CASE WHEN ISNULL(i.balance_usd,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_usd ELSE ISNULL(i.total_amount,0) END
          END) > 0;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_SalePayments_CashMovement
ON dbo.SalePayments
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id,created_at)
    SELECT i.cash_session_id,N'SALE',i.amount,i.currency,N'SALE_PAYMENT',i.sale_payment_id,N'Sale payment',s.user_id,i.created_at
    FROM inserted i
    INNER JOIN dbo.Sales s ON s.sale_id=i.sale_id
    WHERE i.cash_session_id IS NOT NULL AND UPPER(i.payment_method)=N'CASH';
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_CustomerPayments_CashMovement
ON dbo.CustomerPayments
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id,created_at)
    SELECT cs.cash_session_id,N'CUSTOMER_PAYMENT',i.amount,i.currency,N'CUSTOMER_PAYMENT',i.customer_payment_id,N'Customer payment received',i.user_id,i.payment_date
    FROM inserted i
    CROSS APPLY(SELECT TOP(1) cash_session_id FROM dbo.CashSessions WHERE user_id=i.user_id AND status=N'OPEN' ORDER BY opened_at DESC) cs
    WHERE UPPER(i.payment_method)=N'CASH';
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_SupplierPayments_CashMovement
ON dbo.SupplierPayments
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id,created_at)
    SELECT cs.cash_session_id,N'WITHDRAWAL',i.amount,i.currency,N'SUPPLIER_PAYMENT',i.supplier_payment_id,N'Supplier payment',i.user_id,i.payment_date
    FROM inserted i
    CROSS APPLY(SELECT TOP(1) cash_session_id FROM dbo.CashSessions WHERE user_id=i.user_id AND status=N'OPEN' ORDER BY opened_at DESC) cs
    WHERE UPPER(i.payment_method)=N'CASH';
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_Expenses_CashMovement
ON dbo.Expenses
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id,created_at)
    SELECT COALESCE(i.cash_session_id,cs.cash_session_id),N'EXPENSE',i.amount,i.currency,N'EXPENSE',i.expense_id,i.description,i.user_id,i.expense_date
    FROM inserted i
    OUTER APPLY(SELECT TOP(1) cash_session_id FROM dbo.CashSessions WHERE user_id=i.user_id AND status=N'OPEN' ORDER BY opened_at DESC) cs
    WHERE UPPER(i.payment_method)=N'CASH' AND COALESCE(i.cash_session_id,cs.cash_session_id) IS NOT NULL;
END;
GO
