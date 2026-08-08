/*
  POS System Database Migration 002
  Purpose:
    - normalize and protect product barcodes
    - add stable invoice numbers to sales
    - add purchase/selling price foundation to products
    - add created/updated timestamps for future modules
    - add useful indexes for common POS queries

  IMPORTANT:
    Run migration 001 first so dbo.SchemaMigrations exists.
    This migration intentionally stops if duplicate non-empty barcodes exist.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
BEGIN
    THROW 51000, 'Migration 001 must be applied before migration 002.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.SchemaMigrations
    WHERE migration_key = N'002_pos_foundation_upgrade'
)
BEGIN
    /* -----------------------------------------------------------
       PRODUCTS FOUNDATION
       ----------------------------------------------------------- */

    IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
        THROW 51001, 'dbo.Products table was not found.', 1;

    -- Convert blank barcodes to NULL so they do not conflict with the
    -- filtered unique index used for real barcodes.
    IF COL_LENGTH('dbo.Products', 'Barcode') IS NOT NULL
    BEGIN
        UPDATE dbo.Products
        SET Barcode = NULL
        WHERE Barcode IS NOT NULL
          AND LTRIM(RTRIM(Barcode)) = N'';

        IF EXISTS
        (
            SELECT Barcode
            FROM dbo.Products
            WHERE Barcode IS NOT NULL
            GROUP BY Barcode
            HAVING COUNT(*) > 1
        )
        BEGIN
            THROW 51002, 'Duplicate product barcodes exist. Clean duplicates before applying migration 002.', 1;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.indexes
            WHERE object_id = OBJECT_ID(N'dbo.Products')
              AND name = N'UX_Products_Barcode'
        )
        BEGIN
            CREATE UNIQUE INDEX UX_Products_Barcode
                ON dbo.Products (Barcode)
                WHERE Barcode IS NOT NULL;
        END;
    END;

    IF COL_LENGTH('dbo.Products', 'PurchasePrice') IS NULL
        ALTER TABLE dbo.Products ADD PurchasePrice DECIMAL(18,2) NULL;

    IF COL_LENGTH('dbo.Products', 'SellingPrice') IS NULL
        ALTER TABLE dbo.Products ADD SellingPrice DECIMAL(18,2) NULL;

    -- Preserve current behavior by copying the existing Price into
    -- SellingPrice. PurchasePrice stays NULL until the purchase module
    -- establishes a real cost instead of inventing one.
    IF COL_LENGTH('dbo.Products', 'Price') IS NOT NULL
       AND COL_LENGTH('dbo.Products', 'SellingPrice') IS NOT NULL
    BEGIN
        UPDATE dbo.Products
        SET SellingPrice = Price
        WHERE SellingPrice IS NULL;
    END;

    IF COL_LENGTH('dbo.Products', 'UpdatedAt') IS NULL
        ALTER TABLE dbo.Products ADD UpdatedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.Products', 'CreatedAt') IS NOT NULL
       AND COL_LENGTH('dbo.Products', 'UpdatedAt') IS NOT NULL
    BEGIN
        UPDATE dbo.Products
        SET UpdatedAt = CreatedAt
        WHERE UpdatedAt IS NULL;
    END;

    /* -----------------------------------------------------------
       SALES FOUNDATION
       ----------------------------------------------------------- */

    IF OBJECT_ID(N'dbo.Sales', N'U') IS NULL
        THROW 51003, 'dbo.Sales table was not found.', 1;

    IF COL_LENGTH('dbo.Sales', 'InvoiceNumber') IS NULL
        ALTER TABLE dbo.Sales ADD InvoiceNumber NVARCHAR(40) NULL;

    -- Backfill existing sales with deterministic readable invoice numbers.
    -- Future sales should use a dedicated invoice-number service/sequence.
    IF COL_LENGTH('dbo.Sales', 'SaleId') IS NOT NULL
       AND COL_LENGTH('dbo.Sales', 'InvoiceNumber') IS NOT NULL
    BEGIN
        UPDATE dbo.Sales
        SET InvoiceNumber = N'INV-' + RIGHT(REPLICATE('0', 10) + CONVERT(VARCHAR(10), SaleId), 10)
        WHERE InvoiceNumber IS NULL OR LTRIM(RTRIM(InvoiceNumber)) = N'';
    END;

    IF EXISTS
    (
        SELECT InvoiceNumber
        FROM dbo.Sales
        WHERE InvoiceNumber IS NOT NULL
        GROUP BY InvoiceNumber
        HAVING COUNT(*) > 1
    )
    BEGIN
        THROW 51004, 'Duplicate invoice numbers exist. Resolve them before applying migration 002.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.Sales')
          AND name = N'UX_Sales_InvoiceNumber'
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Sales_InvoiceNumber
            ON dbo.Sales (InvoiceNumber)
            WHERE InvoiceNumber IS NOT NULL;
    END;

    IF COL_LENGTH('dbo.Sales', 'CreatedAt') IS NULL
        ALTER TABLE dbo.Sales ADD CreatedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.Sales', 'UpdatedAt') IS NULL
        ALTER TABLE dbo.Sales ADD UpdatedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.Sales', 'SaleDate') IS NOT NULL
       AND COL_LENGTH('dbo.Sales', 'CreatedAt') IS NOT NULL
    BEGIN
        UPDATE dbo.Sales
        SET CreatedAt = SaleDate
        WHERE CreatedAt IS NULL;
    END;

    IF COL_LENGTH('dbo.Sales', 'CreatedAt') IS NOT NULL
       AND COL_LENGTH('dbo.Sales', 'UpdatedAt') IS NOT NULL
    BEGIN
        UPDATE dbo.Sales
        SET UpdatedAt = CreatedAt
        WHERE UpdatedAt IS NULL;
    END;

    IF COL_LENGTH('dbo.Sales', 'SaleDate') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Sales')
             AND name = N'IX_Sales_SaleDate'
       )
    BEGIN
        CREATE INDEX IX_Sales_SaleDate ON dbo.Sales (SaleDate);
    END;

    IF COL_LENGTH('dbo.Sales', 'CustomerId') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Sales')
             AND name = N'IX_Sales_CustomerId'
       )
    BEGIN
        CREATE INDEX IX_Sales_CustomerId ON dbo.Sales (CustomerId);
    END;

    IF COL_LENGTH('dbo.Sales', 'UserId') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Sales')
             AND name = N'IX_Sales_UserId'
       )
    BEGIN
        CREATE INDEX IX_Sales_UserId ON dbo.Sales (UserId);
    END;

    /* -----------------------------------------------------------
       SALE ITEMS INDEXES
       ----------------------------------------------------------- */

    IF OBJECT_ID(N'dbo.SaleItems', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH('dbo.SaleItems', 'SaleId') IS NOT NULL
           AND NOT EXISTS
           (
               SELECT 1
               FROM sys.indexes
               WHERE object_id = OBJECT_ID(N'dbo.SaleItems')
                 AND name = N'IX_SaleItems_SaleId'
           )
        BEGIN
            CREATE INDEX IX_SaleItems_SaleId ON dbo.SaleItems (SaleId);
        END;

        IF COL_LENGTH('dbo.SaleItems', 'ProductId') IS NOT NULL
           AND NOT EXISTS
           (
               SELECT 1
               FROM sys.indexes
               WHERE object_id = OBJECT_ID(N'dbo.SaleItems')
                 AND name = N'IX_SaleItems_ProductId'
           )
        BEGIN
            CREATE INDEX IX_SaleItems_ProductId ON dbo.SaleItems (ProductId);
        END;
    END;

    INSERT INTO dbo.SchemaMigrations (migration_key, description)
    VALUES
    (
        N'002_pos_foundation_upgrade',
        N'Barcode integrity, invoice numbers, product cost foundation, timestamps and POS query indexes'
    );
END;

COMMIT TRANSACTION;
