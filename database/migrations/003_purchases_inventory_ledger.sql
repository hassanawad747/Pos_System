/* POS System Database Migration 003 - purchases and inventory ledger */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
    THROW 51100, 'Migration 001 must be applied first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'002_pos_foundation_upgrade')
    THROW 51101, 'Migration 002 must be applied first.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'003_purchases_inventory_ledger')
BEGIN
    IF OBJECT_ID(N'dbo.Purchases', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Purchases
        (
            purchase_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Purchases PRIMARY KEY,
            invoice_number NVARCHAR(60) NOT NULL,
            supplier_id INT NOT NULL,
            user_id INT NOT NULL,
            purchase_date DATETIME2 NOT NULL,
            subtotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_subtotal DEFAULT (0),
            discount_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_discount DEFAULT (0),
            tax_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_tax DEFAULT (0),
            total_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_total DEFAULT (0),
            paid_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_paid DEFAULT (0),
            remaining_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_remaining DEFAULT (0),
            payment_status NVARCHAR(20) NOT NULL CONSTRAINT DF_Purchases_status DEFAULT (N'UNPAID'),
            payment_method NVARCHAR(50) NULL,
            notes NVARCHAR(1000) NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_Purchases_created DEFAULT SYSUTCDATETIME(),
            updated_at DATETIME2 NULL,
            CONSTRAINT FK_Purchases_Suppliers FOREIGN KEY (supplier_id) REFERENCES dbo.Suppliers(supplier_id),
            CONSTRAINT FK_Purchases_Users FOREIGN KEY (user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_Purchases_amounts CHECK (subtotal >= 0 AND discount_amount >= 0 AND tax_amount >= 0 AND total_amount >= 0 AND paid_amount >= 0 AND remaining_amount >= 0 AND paid_amount <= total_amount)
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Purchases') AND name=N'UX_Purchases_invoice_number')
        CREATE UNIQUE INDEX UX_Purchases_invoice_number ON dbo.Purchases(invoice_number);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Purchases') AND name=N'IX_Purchases_supplier_id')
        CREATE INDEX IX_Purchases_supplier_id ON dbo.Purchases(supplier_id);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Purchases') AND name=N'IX_Purchases_purchase_date')
        CREATE INDEX IX_Purchases_purchase_date ON dbo.Purchases(purchase_date);

    IF OBJECT_ID(N'dbo.PurchaseItems', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PurchaseItems
        (
            purchase_item_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseItems PRIMARY KEY,
            purchase_id INT NOT NULL,
            product_id INT NOT NULL,
            quantity INT NOT NULL,
            unit_cost DECIMAL(18,2) NOT NULL,
            discount_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItems_discount DEFAULT (0),
            tax_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItems_tax DEFAULT (0),
            line_total DECIMAL(18,2) NOT NULL,
            CONSTRAINT FK_PurchaseItems_Purchases FOREIGN KEY (purchase_id) REFERENCES dbo.Purchases(purchase_id) ON DELETE CASCADE,
            CONSTRAINT FK_PurchaseItems_Products FOREIGN KEY (product_id) REFERENCES dbo.Products(product_id),
            CONSTRAINT CK_PurchaseItems_values CHECK (quantity > 0 AND unit_cost >= 0 AND discount_amount >= 0 AND tax_amount >= 0 AND line_total >= 0)
        );
    END;

    IF OBJECT_ID(N'dbo.InventoryTransactions', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.InventoryTransactions
        (
            inventory_transaction_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InventoryTransactions PRIMARY KEY,
            product_id INT NOT NULL,
            transaction_type NVARCHAR(30) NOT NULL,
            quantity_change INT NOT NULL,
            old_quantity INT NOT NULL,
            new_quantity INT NOT NULL,
            reference_type NVARCHAR(30) NULL,
            reference_id INT NULL,
            user_id INT NULL,
            notes NVARCHAR(1000) NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_InventoryTransactions_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_InventoryTransactions_Products FOREIGN KEY (product_id) REFERENCES dbo.Products(product_id),
            CONSTRAINT FK_InventoryTransactions_Users FOREIGN KEY (user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_InventoryTransactions_stock CHECK (old_quantity >= 0 AND new_quantity >= 0)
        );
    END;

    CREATE INDEX IX_PurchaseItems_purchase_id ON dbo.PurchaseItems(purchase_id);
    CREATE INDEX IX_PurchaseItems_product_id ON dbo.PurchaseItems(product_id);
    CREATE INDEX IX_InventoryTransactions_product_created ON dbo.InventoryTransactions(product_id, created_at);
    CREATE INDEX IX_InventoryTransactions_reference ON dbo.InventoryTransactions(reference_type, reference_id);

    INSERT INTO dbo.SchemaMigrations(migration_key, description)
    VALUES(N'003_purchases_inventory_ledger', N'Create purchases, purchase items and inventory transaction ledger using snake_case columns');
END;

COMMIT TRANSACTION;