/* POS System Database Migration 009 - X/Z cash report summary */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL THROW 51700,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'008_sale_payments_cash_movements') THROW 51701,'Migration 008 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'009_xz_cash_reports')
BEGIN
    INSERT dbo.SchemaMigrations(migration_key,description) VALUES(N'009_xz_cash_reports',N'Create X/Z cashier session summary view');
END;
COMMIT TRANSACTION;
GO

CREATE OR ALTER VIEW dbo.vw_CashSessionSummary
AS
SELECT
    s.cash_session_id,
    s.register_id,
    r.name AS register_name,
    r.computer_name,
    s.user_id,
    u.username,
    s.opened_at,
    s.closed_at,
    s.status,
    s.opening_usd,
    s.opening_lbp,
    ISNULL(SUM(CASE WHEN m.currency=N'USD' AND m.movement_type IN(N'SALE',N'CUSTOMER_PAYMENT',N'DEPOSIT') THEN m.amount ELSE 0 END),0) AS cash_in_usd,
    ISNULL(SUM(CASE WHEN m.currency=N'LBP' AND m.movement_type IN(N'SALE',N'CUSTOMER_PAYMENT',N'DEPOSIT') THEN m.amount ELSE 0 END),0) AS cash_in_lbp,
    ISNULL(SUM(CASE WHEN m.currency=N'USD' AND m.movement_type IN(N'REFUND',N'EXPENSE',N'WITHDRAWAL') THEN m.amount ELSE 0 END),0) AS cash_out_usd,
    ISNULL(SUM(CASE WHEN m.currency=N'LBP' AND m.movement_type IN(N'REFUND',N'EXPENSE',N'WITHDRAWAL') THEN m.amount ELSE 0 END),0) AS cash_out_lbp,
    s.opening_usd + ISNULL(SUM(CASE WHEN m.currency=N'USD' THEN CASE WHEN m.movement_type IN(N'REFUND',N'EXPENSE',N'WITHDRAWAL') THEN -m.amount ELSE m.amount END ELSE 0 END),0) AS calculated_expected_usd,
    s.opening_lbp + ISNULL(SUM(CASE WHEN m.currency=N'LBP' THEN CASE WHEN m.movement_type IN(N'REFUND',N'EXPENSE',N'WITHDRAWAL') THEN -m.amount ELSE m.amount END ELSE 0 END),0) AS calculated_expected_lbp,
    s.actual_usd,
    s.actual_lbp,
    s.difference_usd,
    s.difference_lbp,
    COUNT(m.cash_movement_id) AS movement_count
FROM dbo.CashSessions s
INNER JOIN dbo.CashRegisters r ON r.register_id=s.register_id
INNER JOIN dbo.Users u ON u.user_id=s.user_id
LEFT JOIN dbo.CashMovements m ON m.cash_session_id=s.cash_session_id
GROUP BY s.cash_session_id,s.register_id,r.name,r.computer_name,s.user_id,u.username,s.opened_at,s.closed_at,s.status,s.opening_usd,s.opening_lbp,s.actual_usd,s.actual_lbp,s.difference_usd,s.difference_lbp;
GO
