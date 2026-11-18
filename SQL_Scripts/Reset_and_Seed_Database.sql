-- =====================================================
-- SQL SCRIPT - KHÔI PHỤC VÀ RESET DATABASE
-- =====================================================

-- =====================================================
-- 1. XÓA VÀ TẠO LẠI CÁC BẢNG (RESET)
-- =====================================================

-- Xóa tất cả constraint và bảng
IF OBJECT_ID('dbo.OrderDetails', 'U') IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.News', 'U') IS NOT NULL DROP TABLE dbo.News;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

-- =====================================================
-- 2. TẠO LẠI CÁC BẢNG
-- =====================================================

-- Tạo bảng Users
CREATE TABLE dbo.Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Phone NVARCHAR(10) NOT NULL,
    FullName NVARCHAR(100),
    IsVerified BIT NOT NULL DEFAULT 0,
    OTP NVARCHAR(10),
    OTPExpire DATETIME,
    [Role] NVARCHAR(20) NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Tạo bảng Categories
CREATE TABLE dbo.Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Tạo bảng Products
CREATE TABLE dbo.Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10,2) NOT NULL,
    [Stock] INT NOT NULL DEFAULT 0,
    CategoryId INT NOT NULL,
    ImageUrl NVARCHAR(500),
    IsHot BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id) ON DELETE CASCADE
);

-- Tạo bảng Orders
CREATE TABLE dbo.Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    ShippingAddress NVARCHAR(500),
    ShippingPhone NVARCHAR(10),
    ShippingName NVARCHAR(100),
    TotalAmount DECIMAL(10,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'COD',
    IsPaid BIT NOT NULL DEFAULT 0,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);

-- Tạo bảng OrderDetails
CREATE TABLE dbo.OrderDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);

-- Tạo bảng News
CREATE TABLE dbo.News (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX),
    ImageUrl NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

-- =====================================================
-- 3. THÊM DỮ LIỆU MẪU
-- =====================================================

-- Thêm tài khoản Admin
INSERT INTO dbo.Users (Username, PasswordHash, Email, Phone, FullName, IsVerified, [Role], CreatedAt)
VALUES (
    'admin123',
    'X9zcfHqABBGNvMESmxf8QVe8MCrxSTVBIVxsK0TUHRI=', -- SHA256('admin123')
    'admin@techworld.vn',
    '0123456789',
    'Administrator',
    1,
    'Admin',
    GETDATE()
);

-- Thêm tài khoản khách hàng test
INSERT INTO dbo.Users (Username, PasswordHash, Email, Phone, FullName, IsVerified, [Role], CreatedAt)
VALUES (
    'testuser',
    'X9zcfHqABBGNvMESmxf8QVe8MCrxSTVBIVxsK0TUHRI=', -- SHA256('testuser123')
    'testuser@techworld.vn',
    '0987654321',
    'Test Customer',
    1,
    'Customer',
    GETDATE()
);

-- Thêm danh mục sản phẩm
INSERT INTO dbo.Categories ([Name], Description, CreatedAt)
VALUES 
    (N'Laptop', N'Máy tính xách tay', GETDATE()),
    (N'Điện thoại', N'Điện thoại di động', GETDATE()),
    (N'Phụ kiện', N'Phụ kiện công nghệ', GETDATE()),
    (N'Tablet', N'Máy tính bảng', GETDATE());

-- Thêm sản phẩm mẫu
INSERT INTO dbo.Products ([Name], Description, Price, [Stock], CategoryId, IsHot, CreatedAt)
VALUES 
    (N'Dell XPS 13', N'Laptop mỏng nhẹ cao cấp', 1500000, 10, 1, 1, GETDATE()),
    (N'MacBook Pro M3', N'Laptop cao cấp từ Apple', 3000000, 5, 1, 1, GETDATE()),
    (N'iPhone 15', N'Điện thoại thông minh mới nhất', 1000000, 15, 2, 1, GETDATE()),
    (N'Samsung Galaxy S24', N'Điện thoại Android cao cấp', 900000, 20, 2, 0, GETDATE()),
    (N'iPad Pro', N'Máy tính bảng cao cấp', 2000000, 8, 4, 0, GETDATE()),
    (N'AirPods Pro', N'Tai nghe không dây cao cấp', 300000, 30, 3, 0, GETDATE());

-- Thêm tin tức mẫu
INSERT INTO dbo.News (Title, Content, CreatedAt)
VALUES 
    (N'Khuyến mãi lớn tháng 3', N'Giảm giá tất cả sản phẩm lên đến 30%', GETDATE()),
    (N'Đã cập nhật sản phẩm mới', N'Thêm nhiều sản phẩm công nghệ mới', GETDATE());

-- =====================================================
-- 4. KIỂM TRA DỮ LIỆU
-- =====================================================

SELECT N'Người dùng' as N'Bảng', COUNT(*) as N'Số lượng' FROM dbo.Users
UNION ALL
SELECT N'Danh mục', COUNT(*) FROM dbo.Categories
UNION ALL
SELECT N'Sản phẩm', COUNT(*) FROM dbo.Products
UNION ALL
SELECT N'Đơn hàng', COUNT(*) FROM dbo.Orders
UNION ALL
SELECT N'Tin tức', COUNT(*) FROM dbo.News;

-- =====================================================
-- 5. THÔNG TIN KẾT NỐI
-- =====================================================

/*
CONNECTION STRING:
Data Source=(LocalDB)\mssqllocaldb;AttachDbFilename=|DataDirectory|\TechWorld.mdf;Integrated Security=true;

DATABASE INFO:
- Name: TechWorld
- Location: C:\Users\VivoBook\Documents\WebSell\web1\web1\App_Data\
- Files: TechWorld.mdf, TechWorld_log.ldf

DEFAULT ACCOUNTS:
- Username: admin123, Password: admin123 (Admin)
- Username: testuser, Password: testuser123 (Customer)
*/