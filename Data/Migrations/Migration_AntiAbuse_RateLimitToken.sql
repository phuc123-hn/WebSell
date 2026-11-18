-- Migration: Replace OtpCode with RateLimitToken
-- Date: 2024

-- Drop old OtpCode table if exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpCodes')
BEGIN
    DROP TABLE [dbo].[OtpCodes];
END

-- Create new RateLimitToken table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RateLimitTokens')
BEGIN
    CREATE TABLE [dbo].[RateLimitTokens] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Identifier] NVARCHAR(255) NOT NULL,
        [ActionType] NVARCHAR(50) NOT NULL,
        [UserId] INT NULL,
        [IpAddress] NVARCHAR(45) NULL,
        [DeviceFingerprint] NVARCHAR(256) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [AttemptCount] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [ExpiresAt] DATETIME NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Normal',
        [RiskScore] INT NOT NULL DEFAULT 0,
        [RequireCaptcha] BIT NOT NULL DEFAULT 0,
        [BlockReason] NVARCHAR(500) NULL,

        CONSTRAINT [FK_RateLimitToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
    );

    -- Create index for faster lookups
    CREATE INDEX [IX_RateLimitTokens_Identifier_ActionType] 
    ON [dbo].[RateLimitTokens]([Identifier], [ActionType]);

    CREATE INDEX [IX_RateLimitTokens_CreatedAt] 
    ON [dbo].[RateLimitTokens]([CreatedAt]);

    CREATE INDEX [IX_RateLimitTokens_Status] 
    ON [dbo].[RateLimitTokens]([Status]);
END
