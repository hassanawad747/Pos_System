USE [pos_system];
GO

SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.Customers', 'balance_usd') IS NULL
BEGIN
    ALTER TABLE dbo.Customers
        ADD balance_usd DECIMAL(24, 8) NOT NULL
            CONSTRAINT DF_Customers_balance_usd DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Customers', 'balance_lb') IS NULL
BEGIN
    ALTER TABLE dbo.Customers
        ADD balance_lb DECIMAL(24, 8) NOT NULL
            CONSTRAINT DF_Customers_balance_lb DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Customers', 'balance_updated_at') IS NULL
BEGIN
    ALTER TABLE dbo.Customers
        ADD balance_updated_at DATETIME2 NULL;
END
GO

IF COL_LENGTH('dbo.Suppliers', 'balance_usd') IS NULL
BEGIN
    ALTER TABLE dbo.Suppliers
        ADD balance_usd DECIMAL(24, 8) NOT NULL
            CONSTRAINT DF_Suppliers_balance_usd DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Suppliers', 'balance_lb') IS NULL
BEGIN
    ALTER TABLE dbo.Suppliers
        ADD balance_lb DECIMAL(24, 8) NOT NULL
            CONSTRAINT DF_Suppliers_balance_lb DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Suppliers', 'balance_updated_at') IS NULL
BEGIN
    ALTER TABLE dbo.Suppliers
        ADD balance_updated_at DATETIME2 NULL;
END
GO

IF COL_LENGTH('dbo.Suppliers', 'email') IS NULL
BEGIN
    ALTER TABLE dbo.Suppliers
        ADD email NVARCHAR(255) NULL;
END
GO

UPDATE dbo.Customers
SET balance_usd = CASE WHEN ISNULL(balance_usd, 0) = 0 THEN ISNULL(balance, 0) ELSE balance_usd END,
    balance_updated_at = COALESCE(balance_updated_at, created_at, SYSDATETIME())
WHERE ISNULL(balance, 0) <> 0
   OR balance_updated_at IS NULL;
GO

UPDATE dbo.Suppliers
SET balance_updated_at = COALESCE(balance_updated_at, SYSDATETIME())
WHERE ISNULL(balance_usd, 0) <> 0
   OR ISNULL(balance_lb, 0) <> 0;
GO
