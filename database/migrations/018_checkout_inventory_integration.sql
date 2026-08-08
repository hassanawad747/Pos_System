/* POS Migration 018 - complete checkout, inventory and lifecycle integration */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL
    THROW 52800,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'017_cost_profit_reporting')
    THROW 52801,'Migration 017 must be applied first.',1;

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'018_checkout_inventory_integration')
BEGIN
    /* Idempotent checkout and immutable transaction snapshots. */
    IF COL_LENGTH('dbo.Sales','operation_key') IS NULL ALTER TABLE dbo.Sales ADD operation_key UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('dbo.Sales','loyalty_points_earned') IS NULL ALTER TABLE dbo.Sales ADD loyalty_points_earned DECIMAL(24,8) NOT NULL CONSTRAINT DF_Sales_LoyaltyEarned DEFAULT 0;
    IF COL_LENGTH('dbo.Sales','loyalty_points_redeemed') IS NULL ALTER TABLE dbo.Sales ADD loyalty_points_redeemed DECIMAL(24,8) NOT NULL CONSTRAINT DF_Sales_LoyaltyRedeemed DEFAULT 0;
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Sales') AND name=N'UX_Sales_OperationKey')
        EXEC(N'CREATE UNIQUE INDEX UX_Sales_OperationKey ON dbo.Sales(operation_key) WHERE operation_key IS NOT NULL;');

    IF COL_LENGTH('dbo.Sale_Items','quantity_decimal') IS NULL ALTER TABLE dbo.Sale_Items ADD quantity_decimal DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sale_Items','product_unit_id') IS NULL ALTER TABLE dbo.Sale_Items ADD product_unit_id INT NULL;
    IF COL_LENGTH('dbo.Sale_Items','base_quantity') IS NULL ALTER TABLE dbo.Sale_Items ADD base_quantity DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sale_Items','warehouse_id') IS NULL ALTER TABLE dbo.Sale_Items ADD warehouse_id INT NULL;
    IF COL_LENGTH('dbo.Sale_Items','batch_number') IS NULL ALTER TABLE dbo.Sale_Items ADD batch_number NVARCHAR(80) NULL;
    IF COL_LENGTH('dbo.Sale_Items','serial_number') IS NULL ALTER TABLE dbo.Sale_Items ADD serial_number NVARCHAR(120) NULL;
    IF COL_LENGTH('dbo.Sale_Items','tax_rate_id') IS NULL ALTER TABLE dbo.Sale_Items ADD tax_rate_id INT NULL;
    IF COL_LENGTH('dbo.Sale_Items','tax_rate_percent') IS NULL ALTER TABLE dbo.Sale_Items ADD tax_rate_percent DECIMAL(9,4) NULL;
    IF COL_LENGTH('dbo.Sale_Items','tax_inclusive') IS NULL ALTER TABLE dbo.Sale_Items ADD tax_inclusive BIT NULL;
    IF COL_LENGTH('dbo.Sale_Items','tax_amount') IS NULL ALTER TABLE dbo.Sale_Items ADD tax_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_SaleItems_TaxAmount DEFAULT 0;
    IF COL_LENGTH('dbo.Sale_Items','promotion_id') IS NULL ALTER TABLE dbo.Sale_Items ADD promotion_id INT NULL;
    EXEC(N'UPDATE dbo.Sale_Items SET quantity_decimal=quantity,base_quantity=quantity WHERE quantity_decimal IS NULL OR base_quantity IS NULL;');

    IF COL_LENGTH('dbo.Products','stock_quantity_decimal') IS NULL ALTER TABLE dbo.Products ADD stock_quantity_decimal DECIMAL(24,8) NULL;
    EXEC(N'UPDATE dbo.Products SET stock_quantity_decimal=stock_quantity WHERE stock_quantity_decimal IS NULL;');
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.ProductStock') AND name=N'CK_ProductStock_Quantity_Nonnegative')
    BEGIN
        ALTER TABLE dbo.ProductStock WITH NOCHECK ADD CONSTRAINT CK_ProductStock_Quantity_Nonnegative CHECK(quantity>=0);
        ALTER TABLE dbo.ProductStock CHECK CONSTRAINT CK_ProductStock_Quantity_Nonnegative;
    END;
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.ProductBatches') AND name=N'CK_ProductBatches_Quantity_Nonnegative')
    BEGIN
        ALTER TABLE dbo.ProductBatches WITH NOCHECK ADD CONSTRAINT CK_ProductBatches_Quantity_Nonnegative CHECK(quantity>=0);
        ALTER TABLE dbo.ProductBatches CHECK CONSTRAINT CK_ProductBatches_Quantity_Nonnegative;
    END;

    IF COL_LENGTH('dbo.ProductSerials','sale_item_id') IS NULL ALTER TABLE dbo.ProductSerials ADD sale_item_id INT NULL;
    IF COL_LENGTH('dbo.Categories','tax_rate_id') IS NULL ALTER TABLE dbo.Categories ADD tax_rate_id INT NULL;

    IF COL_LENGTH('dbo.HeldSales','completed_sale_id') IS NULL ALTER TABLE dbo.HeldSales ADD completed_sale_id INT NULL;
    IF COL_LENGTH('dbo.HeldSales','completed_at') IS NULL ALTER TABLE dbo.HeldSales ADD completed_at DATETIME2 NULL;
    IF COL_LENGTH('dbo.HeldSales','operation_key') IS NULL ALTER TABLE dbo.HeldSales ADD operation_key UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('dbo.Quotations','operation_key') IS NULL ALTER TABLE dbo.Quotations ADD operation_key UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('dbo.HeldSaleItems','product_unit_id') IS NULL ALTER TABLE dbo.HeldSaleItems ADD product_unit_id INT NULL;
    IF COL_LENGTH('dbo.HeldSaleItems','warehouse_id') IS NULL ALTER TABLE dbo.HeldSaleItems ADD warehouse_id INT NULL;
    IF COL_LENGTH('dbo.HeldSaleItems','batch_number') IS NULL ALTER TABLE dbo.HeldSaleItems ADD batch_number NVARCHAR(80) NULL;
    IF COL_LENGTH('dbo.HeldSaleItems','serial_number') IS NULL ALTER TABLE dbo.HeldSaleItems ADD serial_number NVARCHAR(120) NULL;
    IF COL_LENGTH('dbo.QuotationItems','original_unit_price') IS NULL ALTER TABLE dbo.QuotationItems ADD original_unit_price DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.QuotationItems','product_unit_id') IS NULL ALTER TABLE dbo.QuotationItems ADD product_unit_id INT NULL;
    IF COL_LENGTH('dbo.QuotationItems','warehouse_id') IS NULL ALTER TABLE dbo.QuotationItems ADD warehouse_id INT NULL;
    IF COL_LENGTH('dbo.QuotationItems','batch_number') IS NULL ALTER TABLE dbo.QuotationItems ADD batch_number NVARCHAR(80) NULL;
    IF COL_LENGTH('dbo.QuotationItems','serial_number') IS NULL ALTER TABLE dbo.QuotationItems ADD serial_number NVARCHAR(120) NULL;
    EXEC(N'UPDATE dbo.QuotationItems SET original_unit_price=unit_price WHERE original_unit_price IS NULL;');

    IF COL_LENGTH('dbo.SalesReturns','exchange_sale_id') IS NULL ALTER TABLE dbo.SalesReturns ADD exchange_sale_id INT NULL;
    IF COL_LENGTH('dbo.SalesReturns','refund_status') IS NULL ALTER TABLE dbo.SalesReturns ADD refund_status NVARCHAR(20) NULL;

    IF COL_LENGTH('dbo.Purchases','currency') IS NULL ALTER TABLE dbo.Purchases ADD currency NVARCHAR(10) NOT NULL CONSTRAINT DF_Purchases_Currency DEFAULT N'USD';
    IF COL_LENGTH('dbo.Purchases','exchange_rate') IS NULL ALTER TABLE dbo.Purchases ADD exchange_rate DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Purchases','operation_key') IS NULL ALTER TABLE dbo.Purchases ADD operation_key UNIQUEIDENTIFIER NULL;
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Purchases') AND name=N'UX_Purchases_OperationKey')
        EXEC(N'CREATE UNIQUE INDEX UX_Purchases_OperationKey ON dbo.Purchases(operation_key) WHERE operation_key IS NOT NULL;');
    IF COL_LENGTH('dbo.SupplierPayments','exchange_rate') IS NULL ALTER TABLE dbo.SupplierPayments ADD exchange_rate DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.CustomerPayments','exchange_rate') IS NULL ALTER TABLE dbo.CustomerPayments ADD exchange_rate DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Expenses','exchange_rate') IS NULL ALTER TABLE dbo.Expenses ADD exchange_rate DECIMAL(24,8) NULL;

    IF COL_LENGTH('dbo.InventoryTransactions','quantity_change_decimal') IS NULL ALTER TABLE dbo.InventoryTransactions ADD quantity_change_decimal DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.InventoryTransactions','old_quantity_decimal') IS NULL ALTER TABLE dbo.InventoryTransactions ADD old_quantity_decimal DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.InventoryTransactions','new_quantity_decimal') IS NULL ALTER TABLE dbo.InventoryTransactions ADD new_quantity_decimal DECIMAL(24,8) NULL;
    EXEC(N'UPDATE dbo.InventoryTransactions SET quantity_change_decimal=quantity_change,old_quantity_decimal=old_quantity,new_quantity_decimal=new_quantity WHERE quantity_change_decimal IS NULL;');

    IF OBJECT_ID(N'dbo.SaleExchangeLinks',N'U') IS NULL
    CREATE TABLE dbo.SaleExchangeLinks(
        sale_exchange_link_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SaleExchangeLinks PRIMARY KEY,
        sales_return_id INT NOT NULL,
        replacement_sale_id INT NOT NULL,
        original_total DECIMAL(24,8) NOT NULL,
        replacement_total DECIMAL(24,8) NOT NULL,
        amount_due DECIMAL(24,8) NOT NULL,
        currency NVARCHAR(10) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_SaleExchangeLinks_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SaleExchangeLinks_Return FOREIGN KEY(sales_return_id) REFERENCES dbo.SalesReturns(sales_return_id),
        CONSTRAINT FK_SaleExchangeLinks_Sale FOREIGN KEY(replacement_sale_id) REFERENCES dbo.Sales(sale_id),
        CONSTRAINT UQ_SaleExchangeLinks_Return UNIQUE(sales_return_id)
    );

    IF OBJECT_ID(N'dbo.RefundTransactions',N'U') IS NULL
    CREATE TABLE dbo.RefundTransactions(
        refund_transaction_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefundTransactions PRIMARY KEY,
        sales_return_id INT NOT NULL,
        refund_method NVARCHAR(30) NOT NULL,
        amount DECIMAL(24,8) NOT NULL,
        currency NVARCHAR(10) NOT NULL,
        exchange_rate DECIMAL(24,8) NULL,
        reference_number NVARCHAR(100) NULL,
        cash_session_id INT NULL,
        user_id INT NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_RefundTransactions_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_RefundTransactions_Return FOREIGN KEY(sales_return_id) REFERENCES dbo.SalesReturns(sales_return_id),
        CONSTRAINT FK_RefundTransactions_Session FOREIGN KEY(cash_session_id) REFERENCES dbo.CashSessions(cash_session_id),
        CONSTRAINT FK_RefundTransactions_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
        CONSTRAINT CK_RefundTransactions_Amount CHECK(amount>=0)
    );

    INSERT dbo.ActionPermissions(role_name,action_key,is_allowed)
    SELECT v.role_name,v.action_key,v.is_allowed
    FROM (VALUES
      (N'admin',N'SALE.VOID',1),(N'admin',N'SALE.EXCHANGE',1),(N'admin',N'SALE.REFUND',1),(N'admin',N'PURCHASE.CREATE',1),
      (N'admin',N'STOCK.COUNT',1),(N'admin',N'STOCK.TRANSFER',1),(N'admin',N'STOCK.LOSS',1),(N'admin',N'PRICING.MANAGE',1),
      (N'admin',N'TAX.MANAGE',1),(N'admin',N'CURRENCY.MANAGE',1),(N'admin',N'BACKUP.VERIFY',1),(N'admin',N'CASH.MANAGE',1),(N'admin',N'EXPENSE.CREATE',1),
      (N'cashier',N'SALE.VOID',0),(N'cashier',N'SALE.EXCHANGE',0),(N'cashier',N'SALE.REFUND',0),(N'cashier',N'PURCHASE.CREATE',0),
      (N'cashier',N'STOCK.COUNT',0),(N'cashier',N'STOCK.TRANSFER',0),(N'cashier',N'STOCK.LOSS',0),(N'cashier',N'PRICING.MANAGE',0),
      (N'cashier',N'TAX.MANAGE',0),(N'cashier',N'CURRENCY.MANAGE',0),(N'cashier',N'BACKUP.VERIFY',0),(N'cashier',N'CASH.MANAGE',1),(N'cashier',N'EXPENSE.CREATE',0)
    )v(role_name,action_key,is_allowed)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.ActionPermissions p WHERE p.role_name=v.role_name AND p.action_key=v.action_key);

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'018_checkout_inventory_integration',N'Atomic checkout snapshots, idempotency, exchanges, refunds, unit stock and operational reporting');
END;

COMMIT TRANSACTION;
GO

CREATE OR ALTER VIEW dbo.vw_InventoryLosses
AS
SELECT it.inventory_transaction_id,it.created_at,it.transaction_type,it.product_id,p.name product_name,
       it.warehouse_id,w.name warehouse_name,
       COALESCE(it.quantity_change_decimal,CONVERT(DECIMAL(24,8),it.quantity_change)) quantity_change,
       it.batch_number,it.reference_type,it.reference_id,it.user_id,u.username,it.notes
FROM dbo.InventoryTransactions it
JOIN dbo.Products p ON p.product_id=it.product_id
LEFT JOIN dbo.Warehouses w ON w.warehouse_id=it.warehouse_id
LEFT JOIN dbo.Users u ON u.user_id=it.user_id
WHERE it.transaction_type IN(N'DAMAGE',N'EXPIRED');
GO

CREATE OR ALTER VIEW dbo.vw_ExpiringBatches
AS
SELECT b.product_batch_id,b.product_id,p.name product_name,b.warehouse_id,w.name warehouse_name,b.batch_number,
       b.quantity,b.expiry_date,DATEDIFF(day,CAST(SYSUTCDATETIME() AS date),b.expiry_date) days_to_expiry,
       CASE WHEN b.expiry_date<CAST(SYSUTCDATETIME() AS date) THEN N'EXPIRED'
            WHEN b.expiry_date<=DATEADD(day,30,CAST(SYSUTCDATETIME() AS date)) THEN N'EXPIRING'
            ELSE N'OK' END expiry_status
FROM dbo.ProductBatches b
JOIN dbo.Products p ON p.product_id=b.product_id
JOIN dbo.Warehouses w ON w.warehouse_id=b.warehouse_id
WHERE b.quantity>0 AND b.expiry_date IS NOT NULL;
GO

CREATE OR ALTER VIEW dbo.vw_ReturnAnalysis
AS
SELECT CAST(r.created_at AS date) return_date,r.currency,r.return_type,r.refund_method,
       COUNT_BIG(*) return_count,SUM(r.total_amount) return_amount,SUM(ri.quantity) returned_units
FROM dbo.SalesReturns r
JOIN dbo.SalesReturnItems ri ON ri.sales_return_id=r.sales_return_id
WHERE r.status=N'COMPLETED'
GROUP BY CAST(r.created_at AS date),r.currency,r.return_type,r.refund_method;
GO

CREATE OR ALTER VIEW dbo.vw_DiscountAnalysis
AS
SELECT CAST(s.sale_date AS date) sale_date,
       ISNULL(NULLIF(s.currency,N''),CASE WHEN s.balance_lb IS NOT NULL AND s.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END) currency,
       si.discount_type,COUNT_BIG(*) discounted_lines,SUM(ISNULL(si.discount_amount,0)) discount_amount
FROM dbo.Sale_Items si JOIN dbo.Sales s ON s.sale_id=si.sale_id
WHERE ISNULL(si.discount_amount,0)>0 AND ISNULL(s.sale_status,N'COMPLETED')<>N'VOID'
GROUP BY CAST(s.sale_date AS date),ISNULL(NULLIF(s.currency,N''),CASE WHEN s.balance_lb IS NOT NULL AND s.balance_usd IS NULL THEN N'LBP' ELSE N'USD' END),si.discount_type;
GO

CREATE OR ALTER VIEW dbo.vw_CashierPerformance
AS
SELECT s.user_id,u.username,CAST(s.sale_date AS date) sale_date,COUNT_BIG(*) invoice_count,
       SUM(s.total_amount) revenue,SUM(ISNULL(s.discount_total,0)) discounts,SUM(ISNULL(s.tax_total,0)) tax
FROM dbo.Sales s LEFT JOIN dbo.Users u ON u.user_id=s.user_id
WHERE ISNULL(s.sale_status,N'COMPLETED')<>N'VOID'
GROUP BY s.user_id,u.username,CAST(s.sale_date AS date);
GO
