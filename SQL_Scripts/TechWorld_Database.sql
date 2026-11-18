-- =====================================================
-- SQL SCRIPT - TECHWORLD DATABASE
-- =====================================================
-- Kết nối đến database: (LocalDB)\mssqllocaldb
-- File: C:\Users\VivoBook\Documents\WebSell\web1\web1\App_Data\TechWorld.mdf

-- =====================================================
-- 1. XEM CẤU TRÚC CÁC BẢNG
-- =====================================================

-- Xem danh sách tất cả bảng
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo';

-- =====================================================
-- 2. BẢNG USERS (NGƯỜI DÙNG)
-- =====================================================

-- Xem cấu trúc bảng Users
EXEC sp_help 'dbo.Users';

-- Xem danh sách tất cả người dùng
SELECT 
    Id,
    Username,
    Email,
    Phone,
    FullName,
    Role,
    IsVerified,
    CreatedAt
FROM dbo.Users
ORDER BY CreatedAt DESC;

-- Đếm số lượng người dùng theo vai trò
SELECT Role, COUNT(*) as 'Số lượng'
FROM dbo.Users
GROUP BY Role;

-- =====================================================
-- 3. BẢNG CATEGORIES (DANH MỤC)
-- =====================================================

-- Xem cấu trúc bảng Categories
EXEC sp_help 'dbo.Categories';

-- Xem danh sách danh mục
SELECT Id, Name, Description, CreatedAt
FROM dbo.Categories
ORDER BY CreatedAt DESC;

-- =====================================================
-- 4. BẢNG PRODUCTS (SẢN PHẨM)
-- =====================================================

-- Xem cấu trúc bảng Products
EXEC sp_help 'dbo.Products';

-- Xem danh sách sản phẩm
SELECT 
    p.Id,
    p.Name,
    c.Name as 'Danh mục',
    p.Price,
    p.Stock,
    p.IsHot,
    p.CreatedAt
FROM dbo.Products p
LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
ORDER BY p.CreatedAt DESC;

-- =====================================================
-- 5. BẢNG ORDERS (ĐƠN HÀNG)
-- =====================================================

-- Xem cấu trúc bảng Orders
EXEC sp_help 'dbo.Orders';

-- Xem danh sách đơn hàng
SELECT 
    o.Id,
    u.Username,
    o.OrderDate,
    o.TotalAmount,
    o.Status,
    o.PaymentMethod,
    o.IsPaid
FROM dbo.Orders o
LEFT JOIN dbo.Users u ON o.UserId = u.Id
ORDER BY o.OrderDate DESC;

-- =====================================================
-- 6. BẢNG ORDER DETAILS (CHI TIẾT ĐƠN HÀNG)
-- =====================================================

-- Xem cấu trúc bảng OrderDetails
EXEC sp_help 'dbo.OrderDetails';

-- Xem chi tiết đơn hàng
SELECT 
    od.OrderId,
    od.Id,
    p.Name as 'Sản phẩm',
    od.Quantity,
    od.UnitPrice,
    od.TotalPrice
FROM dbo.OrderDetails od
LEFT JOIN dbo.Products p ON od.ProductId = p.Id
ORDER BY od.OrderId DESC;

-- =====================================================
-- 7. BẢNG NEWS (TIN TỨC)
-- =====================================================

-- Xem cấu trúc bảng News
EXEC sp_help 'dbo.News';

-- Xem danh sách tin tức
SELECT 
    Id,
    Title,
    CreatedAt,
    UpdatedAt
FROM dbo.News
ORDER BY CreatedAt DESC;

-- =====================================================
-- 8. THỐNG KÊ TỔNG QUAN
-- =====================================================

-- Tổng số người dùng
SELECT COUNT(*) as 'Tổng người dùng' FROM dbo.Users;

-- Tổng số sản phẩm
SELECT COUNT(*) as 'Tổng sản phẩm' FROM dbo.Products;

-- Tổng số đơn hàng
SELECT COUNT(*) as 'Tổng đơn hàng' FROM dbo.Orders;

-- Tổng doanh thu
SELECT SUM(TotalAmount) as 'Tổng doanh thu' FROM dbo.Orders WHERE Status != 'Cancelled';

-- Đơn hàng theo trạng thái
SELECT Status, COUNT(*) as 'Số lượng'
FROM dbo.Orders
GROUP BY Status;

-- =====================================================
-- 9. BACKUP DATABASE
-- =====================================================

-- Tạo backup file (chạy với quyền admin)
-- BACKUP DATABASE TechWorld 
-- TO DISK = 'C:\Backup\TechWorld_Backup.bak'
-- WITH INIT, COMPRESSION;

-- =====================================================
-- 10. HƯỚNG DẪN KẾT NỐI LOCALDB
-- =====================================================

/*
Cách mở SQL Server Management Studio với LocalDB:

1. Mở SQL Server Management Studio
2. Server name: (LocalDB)\mssqllocaldb
3. Authentication: Windows Authentication
4. Click Connect

Hoặc dùng Command Line:
   sqlcmd -S (LocalDB)\mssqllocaldb

Database name: TechWorld
File location: C:\Users\VivoBook\Documents\WebSell\web1\web1\App_Data\TechWorld.mdf
Log file: C:\Users\VivoBook\Documents\WebSell\web1\web1\App_Data\TechWorld_log.ldf
*/