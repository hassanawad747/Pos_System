SET NOCOUNT ON;

IF DB_ID(N'pos_system_ci') IS NOT NULL
BEGIN
    ALTER DATABASE pos_system_ci SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE pos_system_ci;
END;
GO

CREATE DATABASE pos_system_ci;
GO
USE pos_system_ci;
GO

CREATE TABLE dbo.Users
(
    User_Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Password_Hash NVARCHAR(500) NULL,
    Role NVARCHAR(50) NULL,
    Created_At DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.Suppliers
(
    supplier_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    name NVARCHAR(200) NOT NULL,
    contact_info NVARCHAR(255) NULL,
    address NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.Customers
(
    customer_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    name NVARCHAR(200) NOT NULL,
    phone NVARCHAR(50) NULL,
    email NVARCHAR(255) NULL,
    loyalty_points INT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.Categories
(
    category_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    category_name NVARCHAR(200) NOT NULL,
    description NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.Products
(
    product_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    name NVARCHAR(200) NOT NULL,
    category_id INT NULL,
    price DECIMAL(18,2) NOT NULL DEFAULT 0,
    stock_quantity INT NOT NULL DEFAULT 0,
    barcode NVARCHAR(100) NULL,
    supplier_id INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY(category_id) REFERENCES dbo.Categories(category_id),
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY(supplier_id) REFERENCES dbo.Suppliers(supplier_id)
);
GO

CREATE TABLE dbo.Sales
(
    sale_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    user_id INT NULL,
    customer_id INT NULL,
    sale_date DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    total_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    payment_method NVARCHAR(50) NULL,
    CONSTRAINT FK_Sales_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(User_Id),
    CONSTRAINT FK_Sales_Customers FOREIGN KEY(customer_id) REFERENCES dbo.Customers(customer_id)
);
GO

CREATE TABLE dbo.SaleItems
(
    sale_item_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    sale_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL,
    discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    original_unit_price DECIMAL(18,2) NULL,
    discount_amount DECIMAL(18,2) NULL,
    discount_type NVARCHAR(30) NULL,
    discount_value DECIMAL(18,2) NULL,
    discount_by NVARCHAR(100) NULL,
    CONSTRAINT FK_SaleItems_Sales FOREIGN KEY(sale_id) REFERENCES dbo.Sales(sale_id),
    CONSTRAINT FK_SaleItems_Products FOREIGN KEY(product_id) REFERENCES dbo.Products(product_id)
);
GO

INSERT INTO dbo.Users (Username, Password_Hash, Role) VALUES (N'ci_admin', N'test', N'admin');
INSERT INTO dbo.Suppliers (name) VALUES (N'CI Supplier');
INSERT INTO dbo.Customers (name) VALUES (N'CI Customer');
INSERT INTO dbo.Categories (category_name) VALUES (N'CI Category');
INSERT INTO dbo.Products (name, category_id, price, stock_quantity, barcode, supplier_id)
VALUES (N'CI Product', 1, 15.00, 10, N'CI-001', 1);
INSERT INTO dbo.Sales (user_id, customer_id, total_amount, payment_method)
VALUES (1, 1, 15.00, N'Cash');
INSERT INTO dbo.SaleItems (sale_id, product_id, quantity, unit_price)
VALUES (1, 1, 1, 15.00);
GO
