/* POS Migration 015 - BI reporting views */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'014_pricing_tax_currency_loyalty') THROW 52500,'Migration 014 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'015_business_intelligence_views')
BEGIN
    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'015_business_intelligence_views',N'Business intelligence reporting views for sales, stock, customers, suppliers and cashier performance');
END;
COMMIT TRANSACTION;
GO

CREATE OR ALTER VIEW dbo.vw_SalesDailySummary
AS
SELECT CAST(ISNULL(s.sale_date,s.created_at) AS date) AS sale_date,
       ISNULL(NULLIF(s.currency,N''),CASE WHEN s.balance_lb IS NOT NULL AND s.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END) AS currency,
       COUNT_BIG(*) AS invoice_count,
       SUM(CAST(ISNULL(s.total_amount,0) AS DECIMAL(38,8))) AS revenue,
       SUM(CAST(ISNULL(s.discount_total,0) AS DECIMAL(38,8))) AS discounts,
       SUM(CAST(ISNULL(s.tax_total,0) AS DECIMAL(38,8))) AS tax,
       SUM(CAST(ISNULL(s.paid_amount,ISNULL(s.total_amount,0)-ISNULL(s.remaining_amount,0)) AS DECIMAL(38,8))) AS paid,
       SUM(CAST(ISNULL(s.remaining_amount,0) AS DECIMAL(38,8))) AS outstanding
FROM dbo.Sales s
WHERE ISNULL(s.sale_status,N'COMPLETED')<>N'VOID'
GROUP BY CAST(ISNULL(s.sale_date,s.created_at) AS date),
         ISNULL(NULLIF(s.currency,N''),CASE WHEN s.balance_lb IS NOT NULL AND s.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END);
GO

CREATE OR ALTER VIEW dbo.vw_ProductSalesPerformance
AS
SELECT p.product_id,p.name AS product_name,
       SUM(CAST(ISNULL(si.quantity,0) AS DECIMAL(38,8))) AS quantity_sold,
       SUM(CAST(ISNULL(si.unit_price,0)*ISNULL(si.quantity,0) AS DECIMAL(38,8))) AS gross_sales,
       SUM(CAST(ISNULL(si.discount_amount,0) AS DECIMAL(38,8))) AS discount_amount,
       MAX(s.sale_date) AS last_sale_at
FROM dbo.Products p
LEFT JOIN dbo.Sale_Items si ON si.product_id=p.product_id
LEFT JOIN dbo.Sales s ON s.sale_id=si.sale_id
GROUP BY p.product_id,p.name;
GO

CREATE OR ALTER VIEW dbo.vw_StockHealth
AS
SELECT p.product_id,p.name AS product_name,w.warehouse_id,w.name AS warehouse_name,
       ISNULL(ps.quantity,0) AS quantity,
       ps.minimum_stock,ps.maximum_stock,ps.reorder_point,
       CASE WHEN ISNULL(ps.quantity,0)<=ISNULL(ps.reorder_point,ISNULL(ps.minimum_stock,0)) THEN 1 ELSE 0 END AS needs_reorder,
       CAST(ISNULL(ps.quantity,0)*ISNULL(p.purchase_price,0) AS DECIMAL(38,8)) AS stock_value_cost,
       CAST(ISNULL(ps.quantity,0)*ISNULL(p.selling_price,p.price) AS DECIMAL(38,8)) AS stock_value_retail
FROM dbo.ProductStock ps
JOIN dbo.Products p ON p.product_id=ps.product_id
JOIN dbo.Warehouses w ON w.warehouse_id=ps.warehouse_id;
GO

CREATE OR ALTER VIEW dbo.vw_CustomerBusinessSummary
AS
SELECT c.customer_id,c.name,
       COUNT(DISTINCT s.sale_id) AS invoice_count,
       SUM(CAST(ISNULL(s.total_amount,0) AS DECIMAL(38,8))) AS lifetime_sales,
       MAX(s.sale_date) AS last_sale_at,
       ISNULL(c.balance_usd,0) AS balance_usd,
       ISNULL(c.balance_lb,0) AS balance_lbp,
       ISNULL(la.points_balance,0) AS loyalty_points
FROM dbo.Customers c
LEFT JOIN dbo.Sales s ON s.customer_id=c.customer_id
LEFT JOIN dbo.LoyaltyAccounts la ON la.customer_id=c.customer_id
GROUP BY c.customer_id,c.name,c.balance_usd,c.balance_lb,la.points_balance;
GO

CREATE OR ALTER VIEW dbo.vw_SupplierBusinessSummary
AS
SELECT s.supplier_id,s.name,
       COUNT(DISTINCT p.purchase_id) AS purchase_count,
       SUM(CAST(ISNULL(p.total_amount,0) AS DECIMAL(38,8))) AS lifetime_purchases,
       MAX(p.purchase_date) AS last_purchase_at,
       SUM(CAST(ISNULL(st.credit,0)-ISNULL(st.debit,0) AS DECIMAL(38,8))) AS ledger_balance
FROM dbo.Suppliers s
LEFT JOIN dbo.Purchases p ON p.supplier_id=s.supplier_id
LEFT JOIN dbo.SupplierTransactions st ON st.supplier_id=s.supplier_id
GROUP BY s.supplier_id,s.name;
GO

CREATE OR ALTER VIEW dbo.vw_HourlySales
AS
SELECT DATEPART(HOUR,ISNULL(s.sale_date,s.created_at)) AS sale_hour,
       COUNT_BIG(*) AS invoice_count,
       SUM(CAST(ISNULL(s.total_amount,0) AS DECIMAL(38,8))) AS revenue
FROM dbo.Sales s
WHERE ISNULL(s.sale_status,N'COMPLETED')<>N'VOID'
GROUP BY DATEPART(HOUR,ISNULL(s.sale_date,s.created_at));
GO
