/* POS System Database Migration 006 - expenses */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL THROW 51400, 'Migration 001 must be applied first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'005_customer_ledger_payments') THROW 51401, 'Migration 005 must be applied first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'006_expenses')
BEGIN
    IF OBJECT_ID(N'dbo.ExpenseCategories', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ExpenseCategories(
            expense_category_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExpenseCategories PRIMARY KEY,
            name NVARCHAR(150) NOT NULL,
            description NVARCHAR(500) NULL,
            is_active BIT NOT NULL CONSTRAINT DF_ExpenseCategories_active DEFAULT(1),
            created_at DATETIME2 NOT NULL CONSTRAINT DF_ExpenseCategories_created DEFAULT SYSUTCDATETIME()
        );
        CREATE UNIQUE INDEX UX_ExpenseCategories_name ON dbo.ExpenseCategories(name);
        INSERT dbo.ExpenseCategories(name) VALUES(N'Rent'),(N'Utilities'),(N'Transport'),(N'Maintenance'),(N'Office'),(N'Other');
    END;

    IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Expenses(
            expense_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY,
            expense_category_id INT NOT NULL,
            amount DECIMAL(24,8) NOT NULL,
            currency NVARCHAR(10) NOT NULL CONSTRAINT DF_Expenses_currency DEFAULT(N'USD'),
            expense_date DATETIME2 NOT NULL CONSTRAINT DF_Expenses_date DEFAULT SYSUTCDATETIME(),
            description NVARCHAR(1000) NULL,
            payment_method NVARCHAR(50) NOT NULL CONSTRAINT DF_Expenses_method DEFAULT(N'CASH'),
            reference_number NVARCHAR(100) NULL,
            user_id INT NOT NULL,
            cash_session_id INT NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_Expenses_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_Expenses_Category FOREIGN KEY(expense_category_id) REFERENCES dbo.ExpenseCategories(expense_category_id),
            CONSTRAINT FK_Expenses_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_Expenses_amount CHECK(amount > 0)
        );
        CREATE INDEX IX_Expenses_date ON dbo.Expenses(expense_date);
        CREATE INDEX IX_Expenses_category_date ON dbo.Expenses(expense_category_id, expense_date);
    END;

    INSERT dbo.SchemaMigrations(migration_key,description) VALUES(N'006_expenses',N'Create expense categories and expense transactions');
END;
COMMIT TRANSACTION;
