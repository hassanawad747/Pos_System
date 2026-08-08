/* POS Migration 014 - pricing, tax, currency, promotions and loyalty */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'013_inventory_professional') THROW 52400,'Migration 013 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'014_pricing_tax_currency_loyalty')
BEGIN
    IF OBJECT_ID(N'dbo.Currencies',N'U') IS NULL
    CREATE TABLE dbo.Currencies(
        currency_code NVARCHAR(10) PRIMARY KEY,
        name NVARCHAR(80) NOT NULL,
        symbol NVARCHAR(10) NULL,
        decimal_places INT NOT NULL CONSTRAINT DF_Currencies_Decimals DEFAULT 2,
        is_base BIT NOT NULL CONSTRAINT DF_Currencies_Base DEFAULT 0,
        is_active BIT NOT NULL CONSTRAINT DF_Currencies_Active DEFAULT 1
    );
    IF NOT EXISTS(SELECT 1 FROM dbo.Currencies WHERE currency_code=N'USD') INSERT dbo.Currencies VALUES(N'USD',N'US Dollar',N'$',2,1,1);
    IF NOT EXISTS(SELECT 1 FROM dbo.Currencies WHERE currency_code=N'LBP') INSERT dbo.Currencies VALUES(N'LBP',N'Lebanese Pound',N'LBP',0,0,1);

    IF OBJECT_ID(N'dbo.ExchangeRates',N'U') IS NULL
    CREATE TABLE dbo.ExchangeRates(
        exchange_rate_id INT IDENTITY(1,1) PRIMARY KEY,
        from_currency NVARCHAR(10) NOT NULL,
        to_currency NVARCHAR(10) NOT NULL,
        rate DECIMAL(24,8) NOT NULL,
        effective_from DATETIME2 NOT NULL,
        effective_to DATETIME2 NULL,
        user_id INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_ExchangeRates_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ExchangeRates_From FOREIGN KEY(from_currency) REFERENCES dbo.Currencies(currency_code),
        CONSTRAINT FK_ExchangeRates_To FOREIGN KEY(to_currency) REFERENCES dbo.Currencies(currency_code),
        CONSTRAINT FK_ExchangeRates_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
        CONSTRAINT CK_ExchangeRates_Rate CHECK(rate>0)
    );
    CREATE INDEX IX_ExchangeRates_PairEffective ON dbo.ExchangeRates(from_currency,to_currency,effective_from DESC);

    IF OBJECT_ID(N'dbo.TaxRates',N'U') IS NULL
    CREATE TABLE dbo.TaxRates(
        tax_rate_id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(30) NOT NULL UNIQUE,
        name NVARCHAR(100) NOT NULL,
        rate_percent DECIMAL(9,4) NOT NULL,
        is_inclusive BIT NOT NULL CONSTRAINT DF_TaxRates_Inclusive DEFAULT 0,
        is_active BIT NOT NULL CONSTRAINT DF_TaxRates_Active DEFAULT 1,
        effective_from DATETIME2 NULL,
        effective_to DATETIME2 NULL,
        CONSTRAINT CK_TaxRates_Rate CHECK(rate_percent>=0 AND rate_percent<=100)
    );
    IF NOT EXISTS(SELECT 1 FROM dbo.TaxRates WHERE code=N'EXEMPT') INSERT dbo.TaxRates(code,name,rate_percent) VALUES(N'EXEMPT',N'Tax Exempt',0);
    IF NOT EXISTS(SELECT 1 FROM dbo.TaxRates WHERE code=N'STD') INSERT dbo.TaxRates(code,name,rate_percent) VALUES(N'STD',N'Standard Tax',10);

    IF OBJECT_ID(N'dbo.ProductTaxRates',N'U') IS NULL
    CREATE TABLE dbo.ProductTaxRates(
        product_id INT NOT NULL PRIMARY KEY,
        tax_rate_id INT NOT NULL,
        CONSTRAINT FK_ProductTaxRates_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductTaxRates_Tax FOREIGN KEY(tax_rate_id) REFERENCES dbo.TaxRates(tax_rate_id)
    );

    IF OBJECT_ID(N'dbo.CustomerPriceLevels',N'U') IS NULL
    CREATE TABLE dbo.CustomerPriceLevels(
        price_level_id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(30) NOT NULL UNIQUE,
        name NVARCHAR(100) NOT NULL,
        discount_percent DECIMAL(9,4) NOT NULL CONSTRAINT DF_PriceLevels_Discount DEFAULT 0,
        is_active BIT NOT NULL CONSTRAINT DF_PriceLevels_Active DEFAULT 1
    );
    IF NOT EXISTS(SELECT 1 FROM dbo.CustomerPriceLevels WHERE code=N'RETAIL') INSERT dbo.CustomerPriceLevels(code,name,discount_percent) VALUES(N'RETAIL',N'Retail',0),(N'WHOLESALE',N'Wholesale',0);
    IF COL_LENGTH('dbo.Customers','price_level_id') IS NULL ALTER TABLE dbo.Customers ADD price_level_id INT NULL;

    IF OBJECT_ID(N'dbo.ProductPrices',N'U') IS NULL
    CREATE TABLE dbo.ProductPrices(
        product_price_id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NOT NULL,
        price_level_id INT NOT NULL,
        currency NVARCHAR(10) NOT NULL,
        unit_id INT NULL,
        price DECIMAL(24,8) NOT NULL,
        effective_from DATETIME2 NULL,
        effective_to DATETIME2 NULL,
        CONSTRAINT FK_ProductPrices_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_ProductPrices_Level FOREIGN KEY(price_level_id) REFERENCES dbo.CustomerPriceLevels(price_level_id),
        CONSTRAINT FK_ProductPrices_Currency FOREIGN KEY(currency) REFERENCES dbo.Currencies(currency_code),
        CONSTRAINT FK_ProductPrices_Unit FOREIGN KEY(unit_id) REFERENCES dbo.Units(unit_id)
    );
    CREATE INDEX IX_ProductPrices_Lookup ON dbo.ProductPrices(product_id,price_level_id,currency,effective_from DESC);

    IF OBJECT_ID(N'dbo.Promotions',N'U') IS NULL
    CREATE TABLE dbo.Promotions(
        promotion_id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(40) NOT NULL UNIQUE,
        name NVARCHAR(120) NOT NULL,
        promotion_type NVARCHAR(30) NOT NULL,
        priority INT NOT NULL CONSTRAINT DF_Promotions_Priority DEFAULT 100,
        start_at DATETIME2 NULL,
        end_at DATETIME2 NULL,
        is_active BIT NOT NULL CONSTRAINT DF_Promotions_Active DEFAULT 1,
        stackable BIT NOT NULL CONSTRAINT DF_Promotions_Stackable DEFAULT 0,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_Promotions_Created DEFAULT SYSUTCDATETIME()
    );

    IF OBJECT_ID(N'dbo.PromotionRules',N'U') IS NULL
    CREATE TABLE dbo.PromotionRules(
        promotion_rule_id INT IDENTITY(1,1) PRIMARY KEY,
        promotion_id INT NOT NULL,
        product_id INT NULL,
        category_id INT NULL,
        customer_id INT NULL,
        min_quantity DECIMAL(24,8) NULL,
        min_invoice_amount DECIMAL(24,8) NULL,
        discount_percent DECIMAL(9,4) NULL,
        discount_amount DECIMAL(24,8) NULL,
        buy_quantity DECIMAL(24,8) NULL,
        get_quantity DECIMAL(24,8) NULL,
        CONSTRAINT FK_PromotionRules_Promotion FOREIGN KEY(promotion_id) REFERENCES dbo.Promotions(promotion_id) ON DELETE CASCADE,
        CONSTRAINT FK_PromotionRules_Product FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id),
        CONSTRAINT FK_PromotionRules_Category FOREIGN KEY(category_id) REFERENCES dbo.Categories(category_id),
        CONSTRAINT FK_PromotionRules_Customer FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id)
    );

    IF OBJECT_ID(N'dbo.LoyaltyAccounts',N'U') IS NULL
    CREATE TABLE dbo.LoyaltyAccounts(
        loyalty_account_id INT IDENTITY(1,1) PRIMARY KEY,
        customer_id INT NOT NULL UNIQUE,
        points_balance DECIMAL(24,8) NOT NULL CONSTRAINT DF_Loyalty_Balance DEFAULT 0,
        lifetime_points DECIMAL(24,8) NOT NULL CONSTRAINT DF_Loyalty_Lifetime DEFAULT 0,
        tier NVARCHAR(30) NULL,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_Loyalty_Updated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LoyaltyAccounts_Customer FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id)
    );

    IF OBJECT_ID(N'dbo.LoyaltyTransactions',N'U') IS NULL
    CREATE TABLE dbo.LoyaltyTransactions(
        loyalty_transaction_id INT IDENTITY(1,1) PRIMARY KEY,
        loyalty_account_id INT NOT NULL,
        transaction_type NVARCHAR(20) NOT NULL,
        points DECIMAL(24,8) NOT NULL,
        balance_after DECIMAL(24,8) NOT NULL,
        reference_type NVARCHAR(30) NULL,
        reference_id INT NULL,
        description NVARCHAR(250) NULL,
        user_id INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_LoyaltyTransactions_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LoyaltyTransactions_Account FOREIGN KEY(loyalty_account_id) REFERENCES dbo.LoyaltyAccounts(loyalty_account_id),
        CONSTRAINT FK_LoyaltyTransactions_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id)
    );

    IF COL_LENGTH('dbo.Sales','currency') IS NULL ALTER TABLE dbo.Sales ADD currency NVARCHAR(10) NULL;
    IF COL_LENGTH('dbo.Sales','exchange_rate') IS NULL ALTER TABLE dbo.Sales ADD exchange_rate DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sales','subtotal') IS NULL ALTER TABLE dbo.Sales ADD subtotal DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sales','discount_total') IS NULL ALTER TABLE dbo.Sales ADD discount_total DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sales','tax_total') IS NULL ALTER TABLE dbo.Sales ADD tax_total DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sales','paid_amount') IS NULL ALTER TABLE dbo.Sales ADD paid_amount DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sales','remaining_amount') IS NULL ALTER TABLE dbo.Sales ADD remaining_amount DECIMAL(24,8) NULL;
    IF COL_LENGTH('dbo.Sales','payment_status') IS NULL ALTER TABLE dbo.Sales ADD payment_status NVARCHAR(20) NULL;
    IF COL_LENGTH('dbo.Sales','sale_status') IS NULL ALTER TABLE dbo.Sales ADD sale_status NVARCHAR(20) NULL;
    IF COL_LENGTH('dbo.Sales','notes') IS NULL ALTER TABLE dbo.Sales ADD notes NVARCHAR(500) NULL;

    INSERT dbo.SchemaMigrations(migration_key,description)
    VALUES(N'014_pricing_tax_currency_loyalty',N'Currencies, rate history, taxes, price levels, promotions and loyalty');
END;
COMMIT TRANSACTION;
GO
