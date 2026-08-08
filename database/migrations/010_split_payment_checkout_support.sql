/* POS System Database Migration 010 - split payment checkout support */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL
    THROW 51800,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'009_xz_cash_reports')
    THROW 51801,'Migration 009 must be applied first.',1;

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'010_split_payment_checkout_support')
BEGIN
    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'010_split_payment_checkout_support',N'Allow explicit split-payment checkout without creating the legacy automatic payment row');
END;

COMMIT TRANSACTION;
GO

/* Legacy single-payment sales still receive one automatic SalePayments row.
   Explicit split-payment checkout writes payment_method = SPLIT and inserts its own rows. */
CREATE OR ALTER TRIGGER dbo.TR_Sales_DefaultSalePayment
ON dbo.Sales
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT dbo.SalePayments
        (sale_id,payment_method,amount,currency,exchange_rate,cash_session_id,is_legacy_auto,created_at)
    SELECT i.sale_id,
           ISNULL(NULLIF(i.payment_method,N''),N'CASH'),
           CAST(CASE
                WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL
                    THEN CASE WHEN ISNULL(i.balance_lb,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_lb ELSE ISNULL(i.total_amount,0) END
                ELSE CASE WHEN ISNULL(i.balance_usd,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_usd ELSE ISNULL(i.total_amount,0) END
           END AS DECIMAL(24,8)),
           CASE WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END,
           NULL,
           (SELECT TOP(1) cs.cash_session_id
            FROM dbo.CashSessions cs
            WHERE cs.user_id=i.user_id AND cs.status=N'OPEN'
            ORDER BY cs.opened_at DESC),
           1,
           ISNULL(i.sale_date,SYSUTCDATETIME())
    FROM inserted i
    WHERE ISNULL(i.total_amount,0)>0
      AND UPPER(ISNULL(i.payment_method,N'')) <> N'SPLIT'
      AND (CASE
            WHEN i.balance_lb IS NOT NULL AND i.balance_usd IS NULL
                THEN CASE WHEN ISNULL(i.balance_lb,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_lb ELSE ISNULL(i.total_amount,0) END
            ELSE CASE WHEN ISNULL(i.balance_usd,0)>0 THEN ISNULL(i.total_amount,0)-i.balance_usd ELSE ISNULL(i.total_amount,0) END
          END) > 0;
END;
GO
