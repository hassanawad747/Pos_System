/* POS System Database Migration 007 - cash registers, sessions and cash movements */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NULL THROW 51500,'Migration 001 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'006_expenses') THROW 51501,'Migration 006 must be applied first.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE migration_key=N'007_cash_register_sessions')
BEGIN
    IF OBJECT_ID(N'dbo.CashRegisters',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CashRegisters(
            register_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CashRegisters PRIMARY KEY,
            name NVARCHAR(150) NOT NULL,
            computer_name NVARCHAR(150) NULL,
            location NVARCHAR(250) NULL,
            is_active BIT NOT NULL CONSTRAINT DF_CashRegisters_active DEFAULT(1),
            created_at DATETIME2 NOT NULL CONSTRAINT DF_CashRegisters_created DEFAULT SYSUTCDATETIME()
        );
        CREATE UNIQUE INDEX UX_CashRegisters_computer ON dbo.CashRegisters(computer_name) WHERE computer_name IS NOT NULL;
    END;

    IF OBJECT_ID(N'dbo.CashSessions',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CashSessions(
            cash_session_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CashSessions PRIMARY KEY,
            register_id INT NOT NULL,
            user_id INT NOT NULL,
            opened_at DATETIME2 NOT NULL CONSTRAINT DF_CashSessions_opened DEFAULT SYSUTCDATETIME(),
            closed_at DATETIME2 NULL,
            opening_usd DECIMAL(24,8) NOT NULL CONSTRAINT DF_CashSessions_opening_usd DEFAULT(0),
            opening_lbp DECIMAL(24,8) NOT NULL CONSTRAINT DF_CashSessions_opening_lbp DEFAULT(0),
            expected_usd DECIMAL(24,8) NULL,
            expected_lbp DECIMAL(24,8) NULL,
            actual_usd DECIMAL(24,8) NULL,
            actual_lbp DECIMAL(24,8) NULL,
            difference_usd DECIMAL(24,8) NULL,
            difference_lbp DECIMAL(24,8) NULL,
            status NVARCHAR(20) NOT NULL CONSTRAINT DF_CashSessions_status DEFAULT(N'OPEN'),
            notes NVARCHAR(1000) NULL,
            CONSTRAINT FK_CashSessions_Register FOREIGN KEY(register_id) REFERENCES dbo.CashRegisters(register_id),
            CONSTRAINT FK_CashSessions_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_CashSessions_status CHECK(status IN(N'OPEN',N'CLOSED'))
        );
        CREATE UNIQUE INDEX UX_CashSessions_one_open_register ON dbo.CashSessions(register_id) WHERE status=N'OPEN';
        CREATE INDEX IX_CashSessions_user_opened ON dbo.CashSessions(user_id,opened_at);
    END;

    IF OBJECT_ID(N'dbo.CashMovements',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CashMovements(
            cash_movement_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CashMovements PRIMARY KEY,
            cash_session_id INT NOT NULL,
            movement_type NVARCHAR(30) NOT NULL,
            amount DECIMAL(24,8) NOT NULL,
            currency NVARCHAR(10) NOT NULL,
            reference_type NVARCHAR(30) NULL,
            reference_id INT NULL,
            description NVARCHAR(1000) NULL,
            user_id INT NOT NULL,
            created_at DATETIME2 NOT NULL CONSTRAINT DF_CashMovements_created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_CashMovements_Session FOREIGN KEY(cash_session_id) REFERENCES dbo.CashSessions(cash_session_id),
            CONSTRAINT FK_CashMovements_User FOREIGN KEY(user_id) REFERENCES dbo.Users(user_id),
            CONSTRAINT CK_CashMovements_amount CHECK(amount >= 0),
            CONSTRAINT CK_CashMovements_currency CHECK(currency IN(N'USD',N'LBP'))
        );
        CREATE INDEX IX_CashMovements_session_created ON dbo.CashMovements(cash_session_id,created_at);
    END;

    INSERT dbo.SchemaMigrations(migration_key,description) VALUES(N'007_cash_register_sessions',N'Create cash registers, cashier shift sessions and cash movements');
END;
COMMIT TRANSACTION;
