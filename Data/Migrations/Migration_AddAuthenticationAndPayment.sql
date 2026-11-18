-- SQL Migration Script ?? t?o b?ng m?i cho OTP, PasswordResetToken, PaymentTransaction, NewsImage
-- Ch?y script này trong SQL Server n?u automatic migration không ho?t ??ng
-- Ho?c s? d?ng Package Manager Console: Update-Database

-- 1. OtpCode Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpCodes')
BEGIN
    CREATE TABLE [dbo].[OtpCodes] (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        [Email] NVARCHAR(255) NOT NULL,
        [Code] NVARCHAR(6) NOT NULL,
        [ExpiryTime] DATETIME NOT NULL,
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [AttemptCount] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [OtpType] NVARCHAR(50) NOT NULL DEFAULT 'Register',
        [UserId] INT NULL,
        CONSTRAINT [FK_OtpCode_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_OtpCode_Email] ON [dbo].[OtpCodes]([Email]);
    CREATE NONCLUSTERED INDEX [IX_OtpCode_Email_Type] ON [dbo].[OtpCodes]([Email], [OtpType]);
END
GO

-- 2. PasswordResetToken Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResetTokens')
BEGIN
    CREATE TABLE [dbo].[PasswordResetTokens] (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NOT NULL,
        [Token] NVARCHAR(256) NOT NULL UNIQUE,
        [ExpiryTime] DATETIME NOT NULL,
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [UsedAt] DATETIME NULL,
        CONSTRAINT [FK_PasswordResetToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_PasswordResetToken_Token] ON [dbo].[PasswordResetTokens]([Token]);
    CREATE NONCLUSTERED INDEX [IX_PasswordResetToken_UserId] ON [dbo].[PasswordResetTokens]([UserId]);
END
GO

-- 3. PaymentTransaction Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentTransactions')
BEGIN
    CREATE TABLE [dbo].[PaymentTransactions] (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        [OrderId] INT NOT NULL,
        [PaymentMethod] NVARCHAR(50) NOT NULL,
        [TransactionId] NVARCHAR(100) NULL,
        [Amount] DECIMAL(18, 2) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [ResponseMessage] NVARCHAR(500) NULL,
        [RawResponse] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [ConfirmedAt] DATETIME NULL,
        CONSTRAINT [FK_PaymentTransaction_Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_OrderId] ON [dbo].[PaymentTransactions]([OrderId]);
    CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_TransactionId] ON [dbo].[PaymentTransactions]([TransactionId]);
    CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_Status] ON [dbo].[PaymentTransactions]([Status]);
END
GO

-- 4. NewsImage Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NewsImages')
BEGIN
    CREATE TABLE [dbo].[NewsImages] (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        [NewsId] INT NOT NULL,
        [ImageUrl] NVARCHAR(500) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_NewsImage_News] FOREIGN KEY ([NewsId]) REFERENCES [dbo].[News]([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_NewsImage_NewsId] ON [dbo].[NewsImages]([NewsId]);
    CREATE NONCLUSTERED INDEX [IX_NewsImage_NewsId_IsDeleted] ON [dbo].[NewsImages]([NewsId], [IsDeleted]);
END
GO

-- 5. Add IsLocked column to Users table n?u ch?a có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Users' AND COLUMN_NAME='IsLocked')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [IsLocked] BIT NOT NULL DEFAULT 0;
END
GO

-- 6. Create AuditLog Table (cho logging admin actions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [Entity] NVARCHAR(100) NOT NULL,
        [EntityId] INT NULL,
        [OldValue] NVARCHAR(MAX) NULL,
        [NewValue] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [IpAddress] NVARCHAR(50) NULL,
        CONSTRAINT [FK_AuditLog_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_AuditLog_UserId] ON [dbo].[AuditLogs]([UserId]);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Action] ON [dbo].[AuditLogs]([Action]);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Entity] ON [dbo].[AuditLogs]([Entity], [EntityId]);
END
GO

PRINT 'Migration script executed successfully!';
