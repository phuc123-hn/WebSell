-- ================================================
-- EF MIGRATIONS INITIALIZATION SCRIPT
-- Run this to enable migrations on existing database
-- ================================================

-- Step 1: Enable Migrations (Run in Package Manager Console)
-- Enable-Migrations -ContextTypeName web1.Data.ApplicationDbContext

-- Step 2: Create Initial Migration (Run in Package Manager Console)
-- Add-Migration InitialCreate -Force

-- Step 3: Update Database (Run in Package Manager Console)
-- Update-Database

-- ================================================
-- ALTERNATIVE: If migrations fail, run this SQL directly
-- ================================================

-- Create __MigrationHistory table if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory')
BEGIN
    CREATE TABLE __MigrationHistory (
        MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
        ContextKey NVARCHAR(300) NOT NULL,
        Model VARBINARY(MAX) NOT NULL,
        ProductVersion NVARCHAR(32) NOT NULL
    );
END

-- ================================================
-- ADD CRITICAL INDEXES FOR PERFORMANCE
-- ================================================

-- Index for Posts filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Post_IsDeleted_CreatedAt')
    CREATE NONCLUSTERED INDEX IX_Post_IsDeleted_CreatedAt ON Posts(IsDeleted, CreatedAt DESC)
    INCLUDE (UserId, Title);

-- Index for Comments by PostId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comment_PostId')
    CREATE NONCLUSTERED INDEX IX_Comment_PostId ON Comments(PostId)
    INCLUDE (UserId, IsDeleted);

-- Index for Reactions (unique constraint)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reaction_PostId_UserId')
    CREATE UNIQUE NONCLUSTERED INDEX IX_Reaction_PostId_UserId ON Reactions(PostId, UserId);

-- Index for UserBan checks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserBan_UserId_IsBanned')
    CREATE NONCLUSTERED INDEX IX_UserBan_UserId_IsBanned ON UserBans(UserId, IsBanned, BanEndDate);

-- Index for User email/username lookup
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Email')
    CREATE NONCLUSTERED INDEX IX_User_Email ON Users(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Username')
    CREATE NONCLUSTERED INDEX IX_User_Username ON Users(Username);

-- ================================================
-- VERIFY INDEXES CREATED
-- ================================================
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
WHERE t.name IN ('Posts', 'Comments', 'Reactions', 'UserBans', 'Users')
ORDER BY t.name, i.name;
