/* POS System Database Migration 002 - existing schema foundation */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
    THROW 51000, 'Migration 001 must be applied before migration 002.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'002_pos_foundation_upgrade')
BEGIN
    IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
        THROW 51001, 'dbo.Products table was not found.', 1;

    UPDATE dbo.Products
    SET barcode = NULL
    WHERE barcode IS NOT NULL AND LTRIM(RTRIM(barcode)) = N'';

    IF EXISTS (SELECT barcode FROM dbo.Products WHERE barcode IS NOT NULL GROUP BY barcode HAVING COUNT(*) > 1)
        THROW 51002, 'Duplicate product barcodes exist. Clean duplicates before applying migration 002.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'UX_Products_barcode')
        CREATE UNIQUE INDEX UX_Products_barcode ON dbo.Products(barcode) WHERE barcode IS NOT NULL;

    IF COL_LENGTH('dbo.Products', 'purchase_price') IS NULL
        ALTER TABLE dbo.Products ADD purchase_price DECIMAL(18,2) NULL;

    IF COL_LENGTH('dbo.Products', 'selling_price') IS NULL
        ALTER TABLE dbo.Products ADD selling_price DECIMAL(18,2) NULL;

    EXEC(N'UPDATE dbo.Products SET selling_price = price WHERE selling_price IS NULL;');

    IF COL_LENGTH('dbo.Products', 'updated_at') IS NULL
        ALTER TABLE dbo.Products ADD updated_at DATETIME2 NULL;

    EXEC(N'UPDATE dbo.Products SET updated_at = created_at WHERE updated_at IS NULL AND created_at IS NOT NULL;');

    IF OBJECT_ID(N'dbo.Sales', N'U') IS NULL
        THROW 51003, 'dbo.Sales table was not found.', 1;

    IF COL_LENGTH('dbo.Sales', 'invoice_number') IS NULL
        ALTER TABLE dbo.Sales ADD invoice_number NVARCHAR(40) NULL;

    EXEC(N'
        UPDATE dbo.Sales
        SET invoice_number = N''INV-'' + RIGHT(REPLICATE(''0'', 10) + CONVERT(VARCHAR(10), sale_id), 10)
        WHERE invoice_number IS NULL OR LTRIM(RTRIM(invoice_number)) = N'''';

        IF EXISTS (
            SELECT invoice_number
            FROM dbo.Sales
            WHERE invoice_number IS NOT NULL
            GROUP BY invoice_number
            HAVING COUNT(*) > 1
        )
            THROW 51004, ''Duplicate invoice numbers exist.'', 1;
    ');

    IF COL_LENGTH('dbo.Sales', 'invoice_number') IS NULL
        THROW 51005, 'Sales.invoice_number was not created.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sales') AND name = N'UX_Sales_invoice_number')
        EXEC(N'CREATE UNIQUE INDEX UX_Sales_invoice_number ON dbo.Sales(invoice_number) WHERE invoice_number IS NOT NULL;');

    IF COL_LENGTH('dbo.Sales', 'created_at') IS NULL
        ALTER TABLE dbo.Sales ADD created_at DATETIME2 NULL;
    IF COL_LENGTH('dbo.Sales', 'updated_at') IS NULL
        ALTER TABLE dbo.Sales ADD updated_at DATETIME2 NULL;

    EXEC(N'UPDATE dbo.Sales SET created_at = sale_date WHERE created_at IS NULL;');
    EXEC(N'UPDATE dbo.Sales SET updated_at = created_at WHERE updated_at IS NULL;');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sales') AND name = N'IX_Sales_sale_date')
        CREATE INDEX IX_Sales_sale_date ON dbo.Sales(sale_date);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sales') AND name = N'IX_Sales_customer_id')
        CREATE INDEX IX_Sales_customer_id ON dbo.Sales(customer_id);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sales') AND name = N'IX_Sales_user_id')
        CREATE INDEX IX_Sales_user_id ON dbo.Sales(user_id);

    IF OBJECT_ID(N'dbo.SaleItems', N'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SaleItems') AND name = N'IX_SaleItems_sale_id')
            CREATE INDEX IX_SaleItems_sale_id ON dbo.SaleItems(sale_id);
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SaleItems') AND name = N'IX_SaleItems_product_id')
            CREATE INDEX IX_SaleItems_product_id ON dbo.SaleItems(product_id);
    END;

    INSERT INTO dbo.SchemaMigrations(migration_key, description)
    VALUES(N'002_pos_foundation_upgrade', N'Snake-case-safe barcode, invoice, cost, timestamp and index foundation');
END;

COMMIT TRANSACTION;