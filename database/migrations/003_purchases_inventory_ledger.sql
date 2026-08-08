/*
  POS System Database Migration 003
  Purpose:
    - create Purchases and PurchaseItems
    - create InventoryTransactions ledger
    - add indexes and foreign keys required by the purchase workflow

  IMPORTANT:
    Run migrations 001 and 002 first.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL
    THROW 51100, 'Migration 001 must be applied before migration 003.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'002_pos_foundation_upgrade')
    THROW 51101, 'Migration 002 must be applied before migration 003.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key = N'003_purchases_inventory_ledger')
BEGIN
    IF OBJECT_ID(N'dbo.Suppliers', N'U') IS NULL
        THROW 51102, 'dbo.Suppliers table was not found.', 1;

    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
        THROW 51103, 'dbo.Users table was not found.', 1;

    IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
        THROW 51104, 'dbo.Products table was not found.', 1;

    IF OBJECT_ID(N'dbo.Purchases', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Purchases
        (
            PurchaseId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Purchases PRIMARY KEY,
            InvoiceNumber NVARCHAR(60) NOT NULL,
            SupplierId INT NOT NULL,
            UserId INT NOT NULL,
            PurchaseDate DATETIME2 NOT NULL,
            Subtotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_Subtotal DEFAULT (0),
            DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_DiscountAmount DEFAULT (0),
            TaxAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_TaxAmount DEFAULT (0),
            TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_TotalAmount DEFAULT (0),
            PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_PaidAmount DEFAULT (0),
            RemainingAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchases_RemainingAmount DEFAULT (0),
            PaymentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Purchases_PaymentStatus DEFAULT (N'UNPAID'),
            PaymentMethod NVARCHAR(50) NULL,
            Notes NVARCHAR(1000) NULL,
            CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Purchases_CreatedAt DEFAULT SYSUTCDATETIME(),
            UpdatedAt DATETIME2 NULL,
            CONSTRAINT FK_Purchases_Suppliers_SupplierId
                FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(SupplierId),
            CONSTRAINT FK_Purchases_Users_UserId
                FOREIGN KEY (UserId) REFERENCES dbo.Users(User_Id),
            CONSTRAINT CK_Purchases_Amounts_NonNegative CHECK
                (Subtotal >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND TotalAmount >= 0 AND PaidAmount >= 0 AND RemainingAmount >= 0),
            CONSTRAINT CK_Purchases_Paid_NotAbove_Total CHECK (PaidAmount <= TotalAmount)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.Purchases')
          AND name = N'UX_Purchases_InvoiceNumber'
    )
        CREATE UNIQUE INDEX UX_Purchases_InvoiceNumber ON dbo.Purchases(InvoiceNumber);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.Purchases')
          AND name = N'IX_Purchases_PurchaseDate'
    )
        CREATE INDEX IX_Purchases_PurchaseDate ON dbo.Purchases(PurchaseDate);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.Purchases')
          AND name = N'IX_Purchases_SupplierId'
    )
        CREATE INDEX IX_Purchases_SupplierId ON dbo.Purchases(SupplierId);

    IF OBJECT_ID(N'dbo.PurchaseItems', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PurchaseItems
        (
            PurchaseItemId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseItems PRIMARY KEY,
            PurchaseId INT NOT NULL,
            ProductId INT NOT NULL,
            Quantity INT NOT NULL,
            UnitCost DECIMAL(18,2) NOT NULL,
            DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItems_DiscountAmount DEFAULT (0),
            TaxAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItems_TaxAmount DEFAULT (0),
            LineTotal DECIMAL(18,2) NOT NULL,
            CONSTRAINT FK_PurchaseItems_Purchases_PurchaseId
                FOREIGN KEY (PurchaseId) REFERENCES dbo.Purchases(PurchaseId) ON DELETE CASCADE,
            CONSTRAINT FK_PurchaseItems_Products_ProductId
                FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId),
            CONSTRAINT CK_PurchaseItems_Quantity_Positive CHECK (Quantity > 0),
            CONSTRAINT CK_PurchaseItems_Amounts_Valid CHECK
                (UnitCost >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND LineTotal >= 0)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.PurchaseItems')
          AND name = N'IX_PurchaseItems_PurchaseId'
    )
        CREATE INDEX IX_PurchaseItems_PurchaseId ON dbo.PurchaseItems(PurchaseId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.PurchaseItems')
          AND name = N'IX_PurchaseItems_ProductId'
    )
        CREATE INDEX IX_PurchaseItems_ProductId ON dbo.PurchaseItems(ProductId);

    IF OBJECT_ID(N'dbo.InventoryTransactions', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.InventoryTransactions
        (
            InventoryTransactionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InventoryTransactions PRIMARY KEY,
            ProductId INT NOT NULL,
            TransactionType NVARCHAR(30) NOT NULL,
            QuantityChange INT NOT NULL,
            OldQuantity INT NOT NULL,
            NewQuantity INT NOT NULL,
            ReferenceType NVARCHAR(30) NULL,
            ReferenceId INT NULL,
            UserId INT NULL,
            Notes NVARCHAR(1000) NULL,
            CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_InventoryTransactions_CreatedAt DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_InventoryTransactions_Products_ProductId
                FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId),
            CONSTRAINT FK_InventoryTransactions_Users_UserId
                FOREIGN KEY (UserId) REFERENCES dbo.Users(User_Id),
            CONSTRAINT CK_InventoryTransactions_Stock_NonNegative CHECK (OldQuantity >= 0 AND NewQuantity >= 0)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.InventoryTransactions')
          AND name = N'IX_InventoryTransactions_ProductId_CreatedAt'
    )
        CREATE INDEX IX_InventoryTransactions_ProductId_CreatedAt
            ON dbo.InventoryTransactions(ProductId, CreatedAt);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.InventoryTransactions')
          AND name = N'IX_InventoryTransactions_Reference'
    )
        CREATE INDEX IX_InventoryTransactions_Reference
            ON dbo.InventoryTransactions(ReferenceType, ReferenceId);

    INSERT INTO dbo.SchemaMigrations (migration_key, description)
    VALUES
    (
        N'003_purchases_inventory_ledger',
        N'Create purchases, purchase items and auditable inventory transaction ledger'
    );
END;

COMMIT TRANSACTION;
