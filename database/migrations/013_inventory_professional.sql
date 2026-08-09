/* POS Migration 013 - professional inventory */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'012_sales_lifecycle') THROW 52300,'Migration 012 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'013_inventory_professional')
BEGIN
    IF OBJECT_ID(N'dbo.Branches',N'U') IS NULL
    CREATE TABLE dbo.Branches(
        branch_id INT IDENTITY(1,1) PRIMARY KEY,
        branch_code NVARCHAR(30) NOT NULL,
        name NVARCHAR(120) NOT NULL,
        address NVARCHAR(250) NULL,
        phone NVARCHAR(50) NULL,
        is_active BIT NOT NULL CONSTRAINT DF_Branches_Active DEFAULT 1,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Branches_Created DEFAULT SYSUTCDATETIME()
    );
    IF NOT EXISTS(SELECT 1 FROM dbo.Branches) INSERT dbo.Branches(branch_code,name) VALUES(N'MAIN',N'Main Branch');
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Branches') AND name=N'UX_Branches_Code') CREATE UNIQUE INDEX UX_Branches_Code ON dbo.Branches(branch_code);

    IF OBJECT_ID(N'dbo.Warehouses',N'U') IS NULL
    CREATE TABLE dbo.Warehouses(
        warehouse_id INT IDENTITY(1,1) PRIMARY KEY,
        branch_id INT NOT NULL,
        warehouse_code NVARCHAR(30) NOT NULL,
        name NVARCHAR(120) NOT NULL,
        is_default BIT NOT NULL CONSTRAINT DF_Warehouses_Default DEFAULT 0,
        is_active BIT NOT NULL CONSTRAINT DF_Warehouses_Active DEFAULT 1,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Warehouses_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Warehouses_Branches FOREIGN KEY(branch_id) REFERENCES dbo.Branches(branch_id)
    );
    IF NOT EXISTS(SELECT 1 FROM dbo.Warehouses)
        INSERT dbo.Warehouses(branch_id,warehouse_code,name,is_default) SELECT TOP(1) branch_id,N'MAIN',N'Main Warehouse',1 FROM dbo.Branches ORDER BY branch_id;
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Warehouses') AND name=N'UX_Warehouses_Code') CREATE UNIQUE INDEX UX_Warehouses_Code ON dbo.Warehouses(branch_id,warehouse_code);

    IF OBJECT_ID(N'dbo.ProductStock',N'U') IS NULL
    CREATE TABLE dbo.ProductStock(
        product_stock_id INT IDENTITY(1,1) PRIMARY KEY,
        warehouse_id INT NOT NULL,
        product_id INT NOT NULL,
        quantity DECIMAL(24,8) NOT NULL CONSTRAINT DF_ProductStock_Qty DEFAULT 0,
        reserved_quantity DECIMAL(24,8) NOT NULL CONSTRAINT DF_ProductStock_Reserved DEFAULT 0,
        minimum_stock DECIMAL(24,8) NULL,
        maximum_stock DECIMAL(24,8) NULL,
        reorder_point DECIMAL(24,8) NULL,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_ProductStock_Updated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ProductStock_Warehouse FOREIGN KEY(warehouse_id) REFERENCES dbo.Warehouses(warehouse_id),
        CONSTRAINT FK_ProductStock_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT UQ_ProductStock UNIQUE(warehouse_id,product_id)
    );
    INSERT dbo.ProductStock(warehouse_id,product_id,quantity)
    SELECT w.warehouse_id,p.product_id,ISNULL(p.stock_quantity,0)
    FROM dbo.Products p CROSS JOIN (SELECT TOP(1) warehouse_id FROM dbo.Warehouses WHERE is_default=1 ORDER BY warehouse_id) w
    WHERE NOT EXISTS(SELECT 1 FROM dbo.ProductStock ps WHERE ps.warehouse_id=w.warehouse_id AND ps.product_id=p.product_id);

    IF OBJECT_ID(N'dbo.StockTransfers',N'U') IS NULL
    CREATE TABLE dbo.StockTransfers(
        stock_transfer_id INT IDENTITY(1,1) PRIMARY KEY,
        transfer_number NVARCHAR(40) NOT NULL,
        from_warehouse_id INT NOT NULL,
        to_warehouse_id INT NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_StockTransfers_Status DEFAULT N'DRAFT',
        notes NVARCHAR(500) NULL,
        user_id INT NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_StockTransfers_Created DEFAULT SYSUTCDATETIME(),
        completed_at DATETIME2 NULL,
        CONSTRAINT FK_StockTransfers_From FOREIGN KEY(from_warehouse_id) REFERENCES dbo.Warehouses(warehouse_id),
        CONSTRAINT FK_StockTransfers_To FOREIGN KEY(to_warehouse_id) REFERENCES dbo.Warehouses(warehouse_id),
        CONSTRAINT FK_StockTransfers_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
        CONSTRAINT CK_StockTransfers_Different CHECK(from_warehouse_id<>to_warehouse_id)
    );
    CREATE UNIQUE INDEX UX_StockTransfers_Number ON dbo.StockTransfers(transfer_number);

    IF OBJECT_ID(N'dbo.StockTransferItems',N'U') IS NULL
    CREATE TABLE dbo.StockTransferItems(
        stock_transfer_item_id INT IDENTITY(1,1) PRIMARY KEY,
        stock_transfer_id INT NOT NULL,
        product_id INT NOT NULL,
        quantity DECIMAL(24,8) NOT NULL,
        CONSTRAINT FK_StockTransferItems_Transfer FOREIGN KEY(stock_transfer_id) REFERENCES dbo.StockTransfers(stock_transfer_id) ON DELETE CASCADE,
        CONSTRAINT FK_StockTransferItems_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT CK_StockTransferItems_Qty CHECK(quantity>0)
    );

    IF OBJECT_ID(N'dbo.StockCounts',N'U') IS NULL
    CREATE TABLE dbo.StockCounts(
        stock_count_id INT IDENTITY(1,1) PRIMARY KEY,
        count_number NVARCHAR(40) NOT NULL,
        warehouse_id INT NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_StockCounts_Status DEFAULT N'OPEN',
        notes NVARCHAR(500) NULL,
        user_id INT NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_StockCounts_Created DEFAULT SYSUTCDATETIME(),
        completed_at DATETIME2 NULL,
        CONSTRAINT FK_StockCounts_Warehouse FOREIGN KEY(warehouse_id) REFERENCES dbo.Warehouses(warehouse_id),
        CONSTRAINT FK_StockCounts_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );
    CREATE UNIQUE INDEX UX_StockCounts_Number ON dbo.StockCounts(count_number);

    IF OBJECT_ID(N'dbo.StockCountItems',N'U') IS NULL
    CREATE TABLE dbo.StockCountItems(
        stock_count_item_id INT IDENTITY(1,1) PRIMARY KEY,
        stock_count_id INT NOT NULL,
        product_id INT NOT NULL,
        expected_quantity DECIMAL(24,8) NOT NULL,
        actual_quantity DECIMAL(24,8) NOT NULL,
        difference AS (actual_quantity-expected_quantity) PERSISTED,
        reason NVARCHAR(250) NULL,
        CONSTRAINT FK_StockCountItems_Count FOREIGN KEY(stock_count_id) REFERENCES dbo.StockCounts(stock_count_id) ON DELETE CASCADE,
        CONSTRAINT FK_StockCountItems_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id)
    );

    IF OBJECT_ID(N'dbo.Units',N'U') IS NULL
    CREATE TABLE dbo.Units(
        unit_id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(20) NOT NULL UNIQUE,
        name NVARCHAR(80) NOT NULL,
        unit_type NVARCHAR(30) NULL,
        is_active BIT NOT NULL CONSTRAINT DF_Units_Active DEFAULT 1
    );
    IF NOT EXISTS(SELECT 1 FROM dbo.Units) INSERT dbo.Units(code,name,unit_type) VALUES(N'PCS',N'Piece',N'COUNT'),(N'BOX',N'Box',N'COUNT'),(N'PACK',N'Pack',N'COUNT'),(N'KG',N'Kilogram',N'WEIGHT'),(N'G',N'Gram',N'WEIGHT'),(N'L',N'Liter',N'VOLUME'),(N'M',N'Meter',N'LENGTH');

    IF OBJECT_ID(N'dbo.ProductUnits',N'U') IS NULL
    CREATE TABLE dbo.ProductUnits(
        product_unit_id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NOT NULL,
        unit_id INT NOT NULL,
        conversion_factor DECIMAL(24,8) NOT NULL CONSTRAINT DF_ProductUnits_Factor DEFAULT 1,
        is_base_unit BIT NOT NULL CONSTRAINT DF_ProductUnits_Base DEFAULT 0,
        selling_price DECIMAL(24,8) NULL,
        CONSTRAINT FK_ProductUnits_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_ProductUnits_Unit FOREIGN KEY(unit_id) REFERENCES dbo.Units(unit_id),
        CONSTRAINT UQ_ProductUnits UNIQUE(product_id,unit_id),
        CONSTRAINT CK_ProductUnits_Factor CHECK(conversion_factor>0)
    );

    IF OBJECT_ID(N'dbo.ProductBarcodes',N'U') IS NULL
    CREATE TABLE dbo.ProductBarcodes(
        product_barcode_id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NOT NULL,
        product_unit_id INT NULL,
        barcode NVARCHAR(100) NOT NULL,
        is_primary BIT NOT NULL CONSTRAINT DF_ProductBarcodes_Primary DEFAULT 0,
        CONSTRAINT FK_ProductBarcodes_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_ProductBarcodes_ProductUnit FOREIGN KEY(product_unit_id) REFERENCES dbo.ProductUnits(product_unit_id)
    );
    CREATE UNIQUE INDEX UX_ProductBarcodes_Barcode ON dbo.ProductBarcodes(barcode);

    IF OBJECT_ID(N'dbo.ProductBatches',N'U') IS NULL
    CREATE TABLE dbo.ProductBatches(
        product_batch_id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NOT NULL,
        warehouse_id INT NOT NULL,
        batch_number NVARCHAR(80) NOT NULL,
        expiry_date DATE NULL,
        quantity DECIMAL(24,8) NOT NULL CONSTRAINT DF_ProductBatches_Qty DEFAULT 0,
        unit_cost DECIMAL(24,8) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_ProductBatches_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ProductBatches_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_ProductBatches_Warehouse FOREIGN KEY(warehouse_id) REFERENCES dbo.Warehouses(warehouse_id),
        CONSTRAINT UQ_ProductBatches UNIQUE(product_id,warehouse_id,batch_number)
    );

    IF OBJECT_ID(N'dbo.ProductSerials',N'U') IS NULL
    CREATE TABLE dbo.ProductSerials(
        product_serial_id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NOT NULL,
        warehouse_id INT NOT NULL,
        serial_number NVARCHAR(120) NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_ProductSerials_Status DEFAULT N'IN_STOCK',
        purchase_item_id INT NULL,
        sale_id INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_ProductSerials_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ProductSerials_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_ProductSerials_Warehouse FOREIGN KEY(warehouse_id) REFERENCES dbo.Warehouses(warehouse_id),
        CONSTRAINT FK_ProductSerials_Sale FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id)
    );
    CREATE UNIQUE INDEX UX_ProductSerials_Number ON dbo.ProductSerials(serial_number);

    IF COL_LENGTH('dbo.InventoryTransactions','warehouse_id') IS NULL ALTER TABLE dbo.InventoryTransactions ADD warehouse_id INT NULL;
    IF COL_LENGTH('dbo.InventoryTransactions','unit_id') IS NULL ALTER TABLE dbo.InventoryTransactions ADD unit_id INT NULL;
    IF COL_LENGTH('dbo.InventoryTransactions','batch_number') IS NULL ALTER TABLE dbo.InventoryTransactions ADD batch_number NVARCHAR(80) NULL;

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'013_inventory_professional',N'Branches, warehouses, stock counts/transfers, units, multiple barcodes, batches, expiry, serials and reorder data');
END;
COMMIT TRANSACTION;
GO
