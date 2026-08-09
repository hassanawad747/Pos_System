/* POS Migration 017 - sale cost snapshots and profit reporting */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'016_security_operations') THROW 52700,'Migration 016 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'017_cost_profit_reporting')
BEGIN
    IF COL_LENGTH('dbo.Sale_Items','unit_cost_snapshot') IS NULL ALTER TABLE dbo.Sale_Items ADD unit_cost_snapshot DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.SaleItems','unit_cost_snapshot') IS NULL ALTER TABLE dbo.SaleItems ADD unit_cost_snapshot DECIMAL(24,8) NULL;

    EXEC(N'UPDATE si SET unit_cost_snapshot=ISNULL(p.purchase_price,0)
           FROM dbo.Sale_Items si JOIN dbo.Products p ON p.product_id=si.product_id
           WHERE si.unit_cost_snapshot IS NULL;');

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'017_cost_profit_reporting',N'Snapshot product cost on sale items and provide profit reporting views');
END;
COMMIT TRANSACTION;
GO

CREATE OR ALTER TRIGGER dbo.TR_SaleItems_CostSnapshot
ON dbo.Sale_Items
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE si
       SET unit_cost_snapshot=ISNULL(p.purchase_price,0)
    FROM dbo.Sale_Items si
    JOIN inserted i ON i.sale_item_id=si.sale_item_id
    JOIN dbo.Products p ON p.product_id=si.product_id
    WHERE si.unit_cost_snapshot IS NULL;
END;
GO

CREATE OR ALTER VIEW dbo.vw_SalesProfitDetail
AS
SELECT s.sale_id,
       CAST(ISNULL(s.sale_date,s.created_at) AS date) AS sale_date,
       ISNULL(NULLIF(s.currency,N''),CASE WHEN s.balance_lb IS NOT NULL AND s.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END) currency,
       si.product_id,
       ISNULL(si.name_product,p.name) product_name,
       si.quantity,
       CAST(ISNULL(si.unit_price,0)*ISNULL(si.quantity,0) AS DECIMAL(38,8)) revenue,
       CAST(ISNULL(si.unit_cost_snapshot,0)*ISNULL(si.quantity,0) AS DECIMAL(38,8)) cogs,
       CAST((ISNULL(si.unit_price,0)-ISNULL(si.unit_cost_snapshot,0))*ISNULL(si.quantity,0) AS DECIMAL(38,8)) gross_profit
FROM dbo.Sale_Items si
JOIN dbo.Sales s ON s.sale_id=si.sale_id
JOIN dbo.Products p ON p.product_id=si.product_id
WHERE ISNULL(s.sale_status,N'COMPLETED')<>N'VOID';
GO

CREATE OR ALTER VIEW dbo.vw_DailyProfitSummary
AS
WITH S AS(
 SELECT sale_date,currency,SUM(revenue) revenue,SUM(cogs) cogs,SUM(gross_profit) gross_profit
 FROM dbo.vw_SalesProfitDetail GROUP BY sale_date,currency
), E AS(
 SELECT CAST(expense_date AS date) expense_date,currency,SUM(CAST(amount AS DECIMAL(38,8))) expenses
 FROM dbo.Expenses GROUP BY CAST(expense_date AS date),currency
)
SELECT COALESCE(S.sale_date,E.expense_date) report_date,
       COALESCE(S.currency,E.currency) currency,
       ISNULL(S.revenue,0) revenue,
       ISNULL(S.cogs,0) cogs,
       ISNULL(S.gross_profit,0) gross_profit,
       ISNULL(E.expenses,0) expenses,
       CAST(ISNULL(S.gross_profit,0)-ISNULL(E.expenses,0) AS DECIMAL(38,8)) net_profit
FROM S FULL OUTER JOIN E ON E.expense_date=S.sale_date AND E.currency=S.currency;
GO
