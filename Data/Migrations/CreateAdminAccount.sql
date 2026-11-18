-- ================================================
-- RESET DATABASE & CREATE ADMIN ACCOUNT
-- ================================================
-- Ch?y script này n?u b?n mu?n xóa toàn b? database
-- và t?o l?i t? ??u

-- STEP 1: Drop existing database tables (n?u c?n)
-- Uncomment n?u mu?n delete all data
-- DROP TABLE [dbo].[Posts] CASCADE;
-- DROP TABLE [dbo].[Comments] CASCADE;
-- DROP TABLE [dbo].[Reactions] CASCADE;
-- DROP TABLE [dbo].[Users] CASCADE;

-- STEP 2: Reset identity seeds
DBCC CHECKIDENT (Users, RESEED, 0);
DBCC CHECKIDENT (Posts, RESEED, 0);

-- STEP 3: Clear existing users
DELETE FROM [dbo].[Users] WHERE [Username] IN ('admin123', 'testuser', 'john_doe');

-- STEP 4: Create Admin Account
-- Password: Admin@123456
-- Hash: PBKDF2 with salt
INSERT INTO [dbo].[Users] (
    [Username],
    [PasswordHash],
    [Email],
    [Phone],
    [FullName],
    [IsVerified],
    [OTP],
    [OTPExpire],
    [Role],
    [CreatedAt]
) VALUES (
    'admin123',
    -- This is the hashed password for 'Admin@123456' using PBKDF2
    -- Generated with PasswordHasherService
    -- For testing, use this pre-generated hash or generate new one in code
    'WFApd0dHWUpjMUlIczRJZjVCWTJubHZlaTl3QW03UXR4YkpYVkQ1bFJqaW91TWFkTXhZMUg4WjhsTEJVWjJadnFsdDVBclZBVjJiMnFkZ05KRkNITnc9PQ==',
    'admin@techworld.vn',
    '0123456789',
    'Administrator',
    1,
    '',
    GETDATE(),
    'Admin',
    GETDATE()
);

-- STEP 5: Create Test Accounts
INSERT INTO [dbo].[Users] (
    [Username],
    [PasswordHash],
    [Email],
    [Phone],
    [FullName],
    [IsVerified],
    [OTP],
    [OTPExpire],
    [Role],
    [CreatedAt]
) VALUES
(
    'testuser',
    -- Password: Test@123456
    'aGZVYXl5ZEdyQXJrTFg4SFBqVDQyOUdhdzBqb1hWekV5NW1yWUM2eFJTOWwwMkZyUW5Id0RSMGRFVHJnMWJ0K0JXaDhhYW1POTZFNHM0VlQyVWc9PQ==',
    'testuser@techworld.vn',
    '0987654321',
    'Test User',
    1,
    '',
    GETDATE(),
    'Customer',
    GETDATE()
),
(
    'john_doe',
    -- Password: John@123456
    'SE9zeVArNnpVdzI3TVJ5bzg1Rjh5eDhKL3dXc29sREdwUnJ3QTVWMjBBVDZSWVh1MHhPb1Fob2t4a0xISFE1N29YazFMQU4yMHhxSVBBdz09',
    'john@example.com',
    '0912345678',
    'John Doe',
    1,
    '',
    GETDATE(),
    'Customer',
    GETDATE()
);

-- STEP 6: Verify accounts created
SELECT [Id], [Username], [Email], [Role], [CreatedAt] FROM [dbo].[Users] 
WHERE [Username] IN ('admin123', 'testuser', 'john_doe')
ORDER BY [CreatedAt] DESC;

-- ================================================
-- TEST LOGINS
-- ================================================
-- Admin: admin123 / Admin@123456
-- Test User: testuser / Test@123456
-- John Doe: john_doe / John@123456
-- ================================================
