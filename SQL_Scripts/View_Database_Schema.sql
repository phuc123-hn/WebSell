-- =====================================================
-- SQL SCRIPT - XEM CHI TIẾT CẤU TRÚC DATABASE
-- =====================================================

-- =====================================================
-- 1. DANH SÁCH TẤT CẢ CÁC BẢNG
-- =====================================================

SELECT 
    TABLE_SCHEMA + '.' + TABLE_NAME as 'Bảng',
    CASE WHEN TABLE_TYPE = 'BASE TABLE' THEN 'Bảng dữ liệu' ELSE 'View' END as 'Loại'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

-- =====================================================
-- 2. CẤU TRÚC BẢNG USERS
-- =====================================================

PRINT '===== BẢNG USERS ====='
SELECT 
    COLUMN_NAME as 'Cột',
    DATA_TYPE as 'Kiểu dữ liệu',
    CHARACTER_MAXIMUM_LENGTH as 'Độ dài',
    IS_NULLABLE as 'Cho phép NULL',
    COLUMNPROPERTY(OBJECT_ID('dbo.Users'), COLUMN_NAME, 'IsIdentity') as 'Identity'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

SELECT 'Người dùng' as 'Bảng', COUNT(*) as 'Số lượng' FROM dbo.Users;

-- =====================================================
-- 3. CẤU TRÚC BẢNG CATEGORIES
-- =====================================================

PRINT '===== BẢNG CATEGORIES ====='
SELECT 
    COLUMN_NAME as 'Cột',
    DATA_TYPE as 'Kiểu dữ liệu',
    CHARACTER_MAXIMUM_LENGTH as 'Độ dài',
    IS_NULLABLE as 'Cho phép NULL'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Categories' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

SELECT 'Danh mục' as 'Bảng', COUNT(*) as 'Số lượng', STRING_AGG([Name], ', ') as 'Danh sách' 
FROM dbo.Categories;

-- =====================================================
-- 4. CẤU TRÚC BẢNG PRODUCTS
-- =====================================================

PRINT '===== BẢNG PRODUCTS ====='
SELECT 
    COLUMN_NAME as 'Cột',
    DATA_TYPE as 'Kiểu dữ liệu',
    CHARACTER_MAXIMUM_LENGTH as 'Độ dài',
    IS_NULLABLE as 'Cho phép NULL'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

SELECT 'Sản phẩm' as 'Bảng', COUNT(*) as 'Số lượng' FROM dbo.Products;

-- =====================================================
-- 5. CẤU TRÚC BẢNG ORDERS
-- =====================================================

PRINT '===== BẢNG ORDERS ====='
SELECT 
    COLUMN_NAME as 'Cột',
    DATA_TYPE as 'Kiểu dữ liệu',
    CHARACTER_MAXIMUM_LENGTH as 'Độ dài',
    IS_NULLABLE as 'Cho phép NULL'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Orders' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

SELECT 'Đơn hàng' as 'Bảng', COUNT(*) as 'Số lượng' FROM dbo.Orders;

-- =====================================================
-- 6. CẤU TRÚC BẢNG ORDER DETAILS
-- =====================================================

PRINT '===== BẢNG ORDERDETAILS ====='
SELECT 
    COLUMN_NAME as 'Cột',
    DATA_TYPE as 'Kiểu dữ liệu',
    IS_NULLABLE as 'Cho phép NULL'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OrderDetails' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

SELECT 'Chi tiết đơn hàng' as 'Bảng', COUNT(*) as 'Số lượng' FROM dbo.OrderDetails;

-- =====================================================
-- 7. CẤU TRÚC BẢNG NEWS
-- =====================================================

PRINT '===== BẢNG NEWS ====='
SELECT 
    COLUMN_NAME as 'Cột',
    DATA_TYPE as 'Kiểu dữ liệu',
    CHARACTER_MAXIMUM_LENGTH as 'Độ dài',
    IS_NULLABLE as 'Cho phép NULL'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'News' AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION;

SELECT 'Tin tức' as 'Bảng', COUNT(*) as 'Số lượng' FROM dbo.News;

-- =====================================================
-- 8. FOREIGN KEYS (KHÓA NGOẠI)
-- =====================================================

PRINT '===== KHÓA NGOẠI ====='
SELECT 
    CONSTRAINT_NAME as 'Tên ràng buộc',
    TABLE_NAME as 'Bảng',
    COLUMN_NAME as 'Cột',
    REFERENCED_TABLE_NAME as 'Bảng tham chiếu',
    REFERENCED_COLUMN_NAME as 'Cột tham chiếu'
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'dbo' AND REFERENCED_TABLE_NAME IS NOT NULL;

-- =====================================================
-- 9. INDEXES
-- =====================================================

PRINT '===== CHỈ MỤC (INDEXES) ====='
SELECT 
    OBJECT_NAME(i.object_id) as 'Bảng',
    i.name as 'Chỉ mục',
    COLUMN_NAME as 'Cột'
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON OBJECT_NAME(i.object_id) = c.TABLE_NAME 
    AND ic.column_id = ORDINAL_POSITION
WHERE OBJECT_SCHEMA_NAME(i.object_id) = 'dbo'
ORDER BY OBJECT_NAME(i.object_id), i.name;

-- =====================================================
-- 10. THÔNG TIN CHUNG DATABASE
-- =====================================================

PRINT '===== THÔNG TIN DATABASE ====='
SELECT 
    DB_NAME() as 'Tên database',
    @@VERSION as 'SQL Server version',
    @@SERVERNAME as 'Server name',
    USER_NAME() as 'Người dùng hiện tại';

-- Kích thước database
SELECT 
    name as 'Tên file',
    type_desc as 'Loại',
    size * 8 / 1024 as 'Kích thước (MB)'
FROM sys.master_files
WHERE database_id = DB_ID()
ORDER BY name;

-- =====================================================
-- 11. THỐNG KÊ DỮ LIỆU CHI TIẾT
-- =====================================================

PRINT '===== THỐNG KÊ CHI TIẾT ====='

-- Người dùng theo vai trò
PRINT 'Người dùng theo vai trò:'
SELECT [Role], COUNT(*) as 'Số lượng'
FROM dbo.Users
GROUP BY [Role];

-- Sản phẩm theo danh mục
PRINT 'Sản phẩm theo danh mục:'
SELECT c.[Name] as 'Danh mục', COUNT(p.Id) as 'Số lượng'
FROM dbo.Categories c
LEFT JOIN dbo.Products p ON c.Id = p.CategoryId
GROUP BY c.[Name];

-- Đơn hàng theo trạng thái
PRINT 'Đơn hàng theo trạng thái:'
SELECT [Status], COUNT(*) as 'Số lượng'
FROM dbo.Orders
GROUP BY [Status];

-- Doanh thu theo phương thức thanh toán
PRINT 'Doanh thu theo phương thức thanh toán:'
SELECT 
    PaymentMethod,
    COUNT(*) as 'Số đơn',
    SUM(TotalAmount) as 'Tổng tiền'
FROM dbo.Orders
WHERE [Status] != 'Cancelled'
GROUP BY PaymentMethod;

-- Top 5 sản phẩm bán chạy
PRINT 'Top 5 sản phẩm bán chạy:'
SELECT TOP 5
    p.[Name],
    SUM(od.Quantity) as 'Số lượng bán',
    SUM(od.TotalPrice) as 'Doanh thu'
FROM dbo.OrderDetails od
INNER JOIN dbo.Products p ON od.ProductId = p.Id
GROUP BY p.[Name]
ORDER BY SUM(od.Quantity) DESC;

-- =====================================================
-- 12. XEM HẾT CHI TIẾT MỘT BẢNG
-- =====================================================

-- Xem tất cả người dùng chi tiết
PRINT '===== CHI TIẾT NGƯỜI DÙNG ====='
SELECT *
FROM dbo.Users
ORDER BY CreatedAt DESC;

-- Xem tất cả sản phẩm chi tiết
PRINT '===== CHI TIẾT SẢN PHẨM ====='
SELECT 
    p.Id,
    p.[Name],
    c.[Name] as 'Danh mục',
    p.Price,
    p.[Stock],
    p.IsHot,
    p.CreatedAt
FROM dbo.Products p
LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
ORDER BY p.CreatedAt DESC;

-- Xem tất cả đơn hàng chi tiết
PRINT '===== CHI TIẾT ĐƠN HÀNG ====='
SELECT 
    o.Id,
    u.Username,
    o.OrderDate,
    o.ShippingName,
    o.TotalAmount,
    o.[Status],
    o.PaymentMethod,
    o.IsPaid
FROM dbo.Orders o
LEFT JOIN dbo.Users u ON o.UserId = u.Id
ORDER BY o.OrderDate DESC;