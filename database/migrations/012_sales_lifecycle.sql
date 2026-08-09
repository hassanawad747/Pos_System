/* POS Migration 012 - held sales, quotations, returns and exchanges */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL THROW 52200,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'011_whish_payment_support') THROW 52201,'Migration 011 must be applied first.',1;

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'012_sales_lifecycle')
BEGIN
    IF OBJECT_ID(N'dbo.HeldSales',N'U') IS NULL
    CREATE TABLE dbo.HeldSales(
        held_sale_id INT IDENTITY(1,1) PRIMARY KEY,
        hold_number NVARCHAR(40) NOT NULL,
        customer_id INT NULL,
        user_id INT NOT NULL,
        currency NVARCHAR(10) NOT NULL CONSTRAINT DF_HeldSales_Currency DEFAULT N'USD',
        exchange_rate DECIMAL(24,8) NULL,
        subtotal DECIMAL(24,8) NOT NULL CONSTRAINT DF_HeldSales_Subtotal DEFAULT 0,
        discount_total DECIMAL(24,8) NOT NULL CONSTRAINT DF_HeldSales_Discount DEFAULT 0,
        tax_total DECIMAL(24,8) NOT NULL CONSTRAINT DF_HeldSales_Tax DEFAULT 0,
        total_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_HeldSales_Total DEFAULT 0,
        notes NVARCHAR(500) NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_HeldSales_Status DEFAULT N'HELD',
        held_at DATETIME2 NOT NULL CONSTRAINT DF_HeldSales_HeldAt DEFAULT SYSUTCDATETIME(),
        resumed_at DATETIME2 NULL,
        CONSTRAINT FK_HeldSales_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
        CONSTRAINT FK_HeldSales_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HeldSales') AND name=N'UX_HeldSales_HoldNumber')
        CREATE UNIQUE INDEX UX_HeldSales_HoldNumber ON dbo.HeldSales(hold_number);

    IF OBJECT_ID(N'dbo.HeldSaleItems',N'U') IS NULL
    CREATE TABLE dbo.HeldSaleItems(
        held_sale_item_id INT IDENTITY(1,1) PRIMARY KEY,
        held_sale_id INT NOT NULL,
        product_id INT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(24,8) NOT NULL,
        original_unit_price DECIMAL(24,8) NULL,
        discount_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_HeldItems_Discount DEFAULT 0,
        tax_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_HeldItems_Tax DEFAULT 0,
        line_total DECIMAL(24,8) NOT NULL,
        CONSTRAINT FK_HeldSaleItems_HeldSales FOREIGN KEY(held_sale_id) REFERENCES dbo.HeldSales(held_sale_id) ON DELETE CASCADE,
        CONSTRAINT FK_HeldSaleItems_Products FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT CK_HeldSaleItems_Qty CHECK(quantity>0)
    );

    IF OBJECT_ID(N'dbo.Quotations',N'U') IS NULL
    CREATE TABLE dbo.Quotations(
        quotation_id INT IDENTITY(1,1) PRIMARY KEY,
        quotation_number NVARCHAR(40) NOT NULL,
        customer_id INT NULL,
        user_id INT NOT NULL,
        currency NVARCHAR(10) NOT NULL,
        exchange_rate DECIMAL(24,8) NULL,
        subtotal DECIMAL(24,8) NOT NULL,
        discount_total DECIMAL(24,8) NOT NULL CONSTRAINT DF_Quotations_Discount DEFAULT 0,
        tax_total DECIMAL(24,8) NOT NULL CONSTRAINT DF_Quotations_Tax DEFAULT 0,
        total_amount DECIMAL(24,8) NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_Quotations_Status DEFAULT N'OPEN',
        valid_until DATETIME2 NULL,
        notes NVARCHAR(500) NULL,
        converted_sale_id INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Quotations_Created DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NULL,
        CONSTRAINT FK_Quotations_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
        CONSTRAINT FK_Quotations_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
        CONSTRAINT FK_Quotations_Sales FOREIGN KEY(converted_sale_id) REFERENCES dbo.Sales(sale_id)
    );
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Quotations') AND name=N'UX_Quotations_Number')
        CREATE UNIQUE INDEX UX_Quotations_Number ON dbo.Quotations(quotation_number);

    IF OBJECT_ID(N'dbo.QuotationItems',N'U') IS NULL
    CREATE TABLE dbo.QuotationItems(
        quotation_item_id INT IDENTITY(1,1) PRIMARY KEY,
        quotation_id INT NOT NULL,
        product_id INT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(24,8) NOT NULL,
        discount_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_QuotationItems_Discount DEFAULT 0,
        tax_amount DECIMAL(24,8) NOT NULL CONSTRAINT DF_QuotationItems_Tax DEFAULT 0,
        line_total DECIMAL(24,8) NOT NULL,
        CONSTRAINT FK_QuotationItems_Quotation FOREIGN KEY(quotation_id) REFERENCES dbo.Quotations(quotation_id) ON DELETE CASCADE,
        CONSTRAINT FK_QuotationItems_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT CK_QuotationItems_Qty CHECK(quantity>0)
    );

    IF OBJECT_ID(N'dbo.SalesReturns',N'U') IS NULL
    CREATE TABLE dbo.SalesReturns(
        sales_return_id INT IDENTITY(1,1) PRIMARY KEY,
        return_number NVARCHAR(40) NOT NULL,
        sale_id INT NOT NULL,
        customer_id INT NULL,
        user_id INT NOT NULL,
        return_type NVARCHAR(20) NOT NULL,
        refund_method NVARCHAR(30) NULL,
        currency NVARCHAR(10) NOT NULL,
        exchange_rate DECIMAL(24,8) NULL,
        total_amount DECIMAL(24,8) NOT NULL,
        reason NVARCHAR(500) NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_SalesReturns_Status DEFAULT N'COMPLETED',
        created_at DATETIME2 NOT NULL CONSTRAINT DF_SalesReturns_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SalesReturns_Sales FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
        CONSTRAINT FK_SalesReturns_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id),
        CONSTRAINT FK_SalesReturns_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SalesReturns') AND name=N'UX_SalesReturns_Number')
        CREATE UNIQUE INDEX UX_SalesReturns_Number ON dbo.SalesReturns(return_number);

    IF OBJECT_ID(N'dbo.SalesReturnItems',N'U') IS NULL
    CREATE TABLE dbo.SalesReturnItems(
        sales_return_item_id INT IDENTITY(1,1) PRIMARY KEY,
        sales_return_id INT NOT NULL,
        sale_item_id INT NULL,
        product_id INT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(24,8) NOT NULL,
        line_total DECIMAL(24,8) NOT NULL,
        restock BIT NOT NULL CONSTRAINT DF_SalesReturnItems_Restock DEFAULT 1,
        CONSTRAINT FK_SalesReturnItems_Return FOREIGN KEY(sales_return_id) REFERENCES dbo.SalesReturns(sales_return_id) ON DELETE CASCADE,
        CONSTRAINT FK_SalesReturnItems_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT CK_SalesReturnItems_Qty CHECK(quantity>0)
    );

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'012_sales_lifecycle',N'Held sales, quotations, partial/full returns and exchange-ready sales lifecycle');
END;

COMMIT TRANSACTION;
GO
