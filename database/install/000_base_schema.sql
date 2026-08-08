/* BikeZone POS - SQL-only fresh install base schema
   No CREATE DATABASE and no .bak restore logic.
   The installer creates the target database, then executes this file inside it.
   This base includes the legacy columns still used by WinForms screens; versioned migrations
   then add the newer domain tables and columns.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        user_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        username NVARCHAR(100) NOT NULL,
        password_hash NVARCHAR(500) NULL,
        role NVARCHAR(50) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Users_created_at DEFAULT SYSUTCDATETIME()
    );
    CREATE UNIQUE INDEX UX_Users_username ON dbo.Users(username);
END;

IF OBJECT_ID(N'dbo.Suppliers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Suppliers
    (
        supplier_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Suppliers PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        contact_info NVARCHAR(255) NULL,
        address NVARCHAR(500) NULL,
        email NVARCHAR(255) NULL,
        balance_usd DECIMAL(24,8) NOT NULL CONSTRAINT DF_Suppliers_balance_usd DEFAULT (0),
        balance_lb DECIMAL(24,8) NOT NULL CONSTRAINT DF_Suppliers_balance_lb DEFAULT (0),
        balance_updated_at DATETIME2 NULL
    );
END;

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        customer_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        phone NVARCHAR(50) NULL,
        email NVARCHAR(255) NULL,
        loyalty_points INT NOT NULL CONSTRAINT DF_Customers_loyalty DEFAULT (0),
        balance DECIMAL(24,8) NOT NULL CONSTRAINT DF_Customers_balance DEFAULT (0),
        balance_usd DECIMAL(24,8) NOT NULL CONSTRAINT DF_Customers_balance_usd DEFAULT (0),
        balance_lb DECIMAL(24,8) NOT NULL CONSTRAINT DF_Customers_balance_lb DEFAULT (0),
        balance_updated_at DATETIME2 NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Customers_created DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        category_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        category_name NVARCHAR(200) NOT NULL,
        description NVARCHAR(500) NULL
    );
END;

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        product_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        category_id INT NULL,
        /* price is retained for EF/backward compatibility. Legacy WinForms uses the USD/LBP columns below. */
        price DECIMAL(18,2) NOT NULL CONSTRAINT DF_Products_price DEFAULT (0),
        price_usd DECIMAL(24,8) NOT NULL CONSTRAINT DF_Products_price_usd DEFAULT (0),
        price_lb DECIMAL(24,8) NOT NULL CONSTRAINT DF_Products_price_lb DEFAULT (0),
        exchange_rate DECIMAL(24,8) NOT NULL CONSTRAINT DF_Products_exchange_rate DEFAULT (0),
        sale_price_usd DECIMAL(24,8) NOT NULL CONSTRAINT DF_Products_sale_price_usd DEFAULT (0),
        sale_price_lb DECIMAL(24,8) NOT NULL CONSTRAINT DF_Products_sale_price_lb DEFAULT (0),
        stock_quantity INT NOT NULL CONSTRAINT DF_Products_stock DEFAULT (0),
        barcode NVARCHAR(100) NULL,
        supplier_id INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Products_created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Products_Categories FOREIGN KEY(category_id) REFERENCES dbo.Categories(category_id),
        CONSTRAINT FK_Products_Suppliers FOREIGN KEY(supplier_id) REFERENCES dbo.Suppliers(supplier_id)
    );
END;

IF OBJECT_ID(N'dbo.Sales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sales
    (
        sale_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sales PRIMARY KEY,
        user_id INT NULL,
        customer_id INT NULL,
        sale_date DATETIME2 NOT NULL CONSTRAINT DF_Sales_date DEFAULT SYSUTCDATETIME(),
        total_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_Sales_total DEFAULT (0),
        payment_method NVARCHAR(50) NULL,
        created_by NVARCHAR(100) NULL,
        customer_name NVARCHAR(200) NULL,
        balance_usd DECIMAL(24,8) NULL,
        balance_lb DECIMAL(24,8) NULL,
        quantity INT NOT NULL CONSTRAINT DF_Sales_quantity DEFAULT (0),
        is_returned BIT NOT NULL CONSTRAINT DF_Sales_is_returned DEFAULT (0),
        status NVARCHAR(50) NULL,
        CONSTRAINT FK_Sales_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
        CONSTRAINT FK_Sales_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id)
    );
END;

/* EF-era table retained until all code is moved off it. */
IF OBJECT_ID(N'dbo.SaleItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaleItems
    (
        sale_item_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SaleItems PRIMARY KEY,
        sale_id INT NOT NULL,
        product_id INT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(18,2) NOT NULL,
        discount DECIMAL(18,2) NOT NULL CONSTRAINT DF_SaleItems_discount DEFAULT (0),
        original_unit_price DECIMAL(18,2) NULL,
        discount_amount DECIMAL(18,2) NULL,
        discount_type NVARCHAR(30) NULL,
        discount_value DECIMAL(18,2) NULL,
        discount_by NVARCHAR(100) NULL,
        CONSTRAINT FK_SaleItems_Sales FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
        CONSTRAINT FK_SaleItems_Products FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id)
    );
END;

/* Current WinForms Sales screen uses dbo.Sale_Items. */
IF OBJECT_ID(N'dbo.Sale_Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sale_Items
    (
        sale_item_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sale_Items PRIMARY KEY,
        sale_id INT NOT NULL,
        product_id INT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(24,8) NOT NULL,
        original_unit_price DECIMAL(24,8) NULL,
        discount_amount DECIMAL(24,8) NULL,
        discount_type NVARCHAR(30) NULL,
        discount_value DECIMAL(18,4) NULL,
        discount_by NVARCHAR(100) NULL,
        name_product NVARCHAR(200) NULL,
        customer_name NVARCHAR(200) NULL,
        created_by NVARCHAR(100) NULL,
        sale_date DATETIME2 NOT NULL CONSTRAINT DF_Sale_Items_date DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Sale_Items_Sales FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
        CONSTRAINT FK_Sale_Items_Products FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id)
    );
    CREATE INDEX IX_Sale_Items_sale_id ON dbo.Sale_Items(sale_id);
    CREATE INDEX IX_Sale_Items_product_id ON dbo.Sale_Items(product_id);
END;

IF OBJECT_ID(N'dbo.Settings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Settings
    (
        setting_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Settings PRIMARY KEY,
        key_name NVARCHAR(150) NOT NULL,
        value NVARCHAR(MAX) NULL
    );
    CREATE UNIQUE INDEX UX_Settings_key_name ON dbo.Settings(key_name);
END;

IF OBJECT_ID(N'dbo.Reports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reports
    (
        ReportId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reports PRIMARY KEY,
        ReportType NVARCHAR(50) NULL,
        GeneratedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Reports_created DEFAULT SYSUTCDATETIME(),
        FilePath NVARCHAR(1000) NULL,
        CONSTRAINT FK_Reports_Users FOREIGN KEY(GeneratedBy) REFERENCES dbo.Users(user_id)
    );
END;

IF OBJECT_ID(N'dbo.Discounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Discounts
    (
        DiscountId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Discounts PRIMARY KEY,
        Name NVARCHAR(200) NULL,
        Description NVARCHAR(1000) NULL,
        DiscountType NVARCHAR(30) NULL,
        Value DECIMAL(18,2) NOT NULL CONSTRAINT DF_Discounts_value DEFAULT (0),
        TargetType NVARCHAR(50) NULL,
        ProductId INT NULL,
        CategoryId INT NULL,
        Barcode NVARCHAR(100) NULL,
        StartDate DATETIME2 NULL,
        EndDate DATETIME2 NULL,
        Active BIT NOT NULL CONSTRAINT DF_Discounts_active DEFAULT (1)
    );
END;

/* Current return workflow uses snake_case names and permits partial rows with only sale/product/qty/date. */
IF OBJECT_ID(N'dbo.Returns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Returns
    (
        return_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Returns PRIMARY KEY,
        sale_id INT NOT NULL,
        product_id INT NOT NULL,
        customer_id INT NULL,
        user_id INT NULL,
        return_date DATETIME2 NOT NULL CONSTRAINT DF_Returns_date DEFAULT SYSUTCDATETIME(),
        quantity INT NOT NULL,
        reason NVARCHAR(1000) NULL,
        refund_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_Returns_refund DEFAULT (0),
        CONSTRAINT FK_Returns_Sales FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
        CONSTRAINT FK_Returns_Products FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_Returns_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
        CONSTRAINT FK_Returns_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );
    CREATE INDEX IX_Returns_sale_product ON dbo.Returns(sale_id, product_id);
END;

IF OBJECT_ID(N'dbo.InventoryLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryLogs
    (
        LogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InventoryLogs PRIMARY KEY,
        ProductId INT NOT NULL,
        ChangeType NVARCHAR(50) NULL,
        QuantityChange INT NOT NULL,
        UserId INT NOT NULL,
        Timestamp DATETIME2 NOT NULL CONSTRAINT DF_InventoryLogs_timestamp DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_InventoryLogs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_InventoryLogs_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(user_id)
    );
END;

IF OBJECT_ID(N'dbo.BackupLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BackupLogs
    (
        BackupId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BackupLogs PRIMARY KEY,
        BackupDate DATETIME2 NOT NULL CONSTRAINT DF_BackupLogs_date DEFAULT SYSUTCDATETIME(),
        BackupType NVARCHAR(50) NULL,
        FilePath NVARCHAR(1000) NULL,
        Status NVARCHAR(50) NULL,
        PerformedBy INT NOT NULL,
        CONSTRAINT FK_BackupLogs_Users FOREIGN KEY(PerformedBy) REFERENCES dbo.Users(user_id)
    );
END;

IF OBJECT_ID(N'dbo.SalesBackups', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesBackups
    (
        SalesBackupId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesBackups PRIMARY KEY,
        SaleId INT NOT NULL,
        UserId INT NOT NULL,
        CustomerId INT NOT NULL,
        BackupDate DATETIME2 NOT NULL CONSTRAINT DF_SalesBackups_date DEFAULT SYSUTCDATETIME(),
        TotalAmount DECIMAL(18,2) NOT NULL,
        PaymentMethod NVARCHAR(50) NULL,
        CONSTRAINT FK_SalesBackups_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(user_id),
        CONSTRAINT FK_SalesBackups_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(customer_id)
    );
END;

IF OBJECT_ID(N'dbo.SaleItemsBackups', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaleItemsBackups
    (
        SaleItemsBackupId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SaleItemsBackups PRIMARY KEY,
        SalesBackupId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        BackupDate DATETIME2 NOT NULL CONSTRAINT DF_SaleItemsBackups_date DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SaleItemsBackups_SalesBackups FOREIGN KEY(SalesBackupId) REFERENCES dbo.SalesBackups(SalesBackupId) ON DELETE CASCADE,
        CONSTRAINT FK_SaleItemsBackups_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(product_id)
    );
END;
