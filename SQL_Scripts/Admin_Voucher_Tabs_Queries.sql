-- ============================================================================
-- SQL QUERIES: Admin Voucher Tabs
-- ============================================================================
-- Kiểm tra & quản lý vouchers theo từng tab
-- ============================================================================

-- 1. KIỂM TRA VOUCHER SỰ KIỆN (Tab 1)
-- ============================================================================
SELECT 
    v.Id, v.Code, v.Name, 
    v.DiscountPercent, v.DiscountAmount,
    v.Quantity, v.UsedQuantity,
    (v.Quantity - v.UsedQuantity) AS Remaining,
    v.EndDate, 
    e.Name AS EventName,
    v.IsActive,
    v.CreatedAt
FROM Vouchers v
LEFT JOIN Events e ON v.EventId = e.Id
WHERE v.EventId IS NOT NULL
ORDER BY v.CreatedAt DESC;

-- Count: Tổng số voucher sự kiện
SELECT COUNT(*) AS EventVoucherCount FROM Vouchers WHERE EventId IS NOT NULL;

---

-- 2. KIỂM TRA VOUCHER CHUNG (Tab 2)
-- ============================================================================
SELECT 
    v.Id, v.Code, v.Name, 
    v.DiscountPercent, v.DiscountAmount,
    v.Quantity, v.UsedQuantity,
    (v.Quantity - v.UsedQuantity) AS Remaining,
    v.EndDate,
    v.IsLuckySpinVoucher,
    v.IsActive,
    v.CreatedAt
FROM Vouchers v
WHERE v.EventId IS NULL AND v.IsPrivate = 0
ORDER BY v.CreatedAt DESC;

-- Count: Tổng số voucher chung
SELECT COUNT(*) AS PublicVoucherCount 
FROM Vouchers 
WHERE EventId IS NULL AND IsPrivate = 0;

---

-- 3. KIỂM TRA VOUCHER SPRINT (Tab 3) - WITH USER OWNER
-- ============================================================================
SELECT 
    v.Id, 
    v.Code, 
    v.Name, 
    v.DiscountPercent, 
    v.DiscountAmount,
    v.Quantity, 
    v.UsedQuantity,
    v.EndDate,
    CASE 
        WHEN v.Code LIKE 'SPIN_%' THEN 'Lucky Spin'
        WHEN v.Code LIKE 'EVENT_%' THEN 'Event'
        ELSE 'Private'
    END AS VoucherType,
    v.IsLuckySpinVoucher,
    v.OwnerUserId,
    u.Email,
    u.FullName AS UserName,
    u.Phone,
    v.IsActive,
    v.CreatedAt
FROM Vouchers v
LEFT JOIN Users u ON v.OwnerUserId = u.Id
WHERE v.IsPrivate = 1
ORDER BY v.CreatedAt DESC;

-- Count: Tổng số voucher sprint
SELECT COUNT(*) AS SprintVoucherCount FROM Vouchers WHERE IsPrivate = 1;

---

-- 4. THỐNG KÊ TỔNG (Overview)
-- ============================================================================
SELECT 
    COUNT(CASE WHEN v.EventId IS NOT NULL THEN 1 END) AS EventVouchers,
    COUNT(CASE WHEN v.EventId IS NULL AND v.IsPrivate = 0 THEN 1 END) AS PublicVouchers,
    COUNT(CASE WHEN v.IsPrivate = 1 THEN 1 END) AS SprintVouchers,
    COUNT(*) AS TotalVouchers,
    SUM(CASE WHEN v.EndDate >= GETDATE() THEN 1 ELSE 0 END) AS ActiveVouchers,
    SUM(CASE WHEN v.EndDate < GETDATE() THEN 1 ELSE 0 END) AS ExpiredVouchers
FROM Vouchers v;

---

-- 5. VOUCHER SỰ KIỆN - CHI TIẾT THEO EVENT
-- ============================================================================
SELECT 
    e.Id,
    e.Name AS EventName,
    COUNT(v.Id) AS VoucherCount,
    SUM(CASE WHEN v.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    SUM(CASE WHEN v.EndDate >= GETDATE() THEN 1 ELSE 0 END) AS NotExpiredCount
FROM Events e
LEFT JOIN Vouchers v ON e.Id = v.EventId
GROUP BY e.Id, e.Name
ORDER BY VoucherCount DESC;

---

-- 6. VOUCHER SPRINT - CHI TIẾT THEO USER
-- ============================================================================
SELECT 
    u.Id,
    u.Email,
    u.FullName,
    COUNT(v.Id) AS VoucherCount,
    SUM(CASE WHEN v.IsLuckySpinVoucher = 1 THEN 1 ELSE 0 END) AS LuckySpinVouchers,
    SUM(CASE WHEN v.Code LIKE 'EVENT_%' THEN 1 ELSE 0 END) AS EventVouchers,
    SUM(CASE WHEN v.EndDate >= GETDATE() THEN 1 ELSE 0 END) AS ActiveVouchers
FROM Users u
LEFT JOIN Vouchers v ON u.Id = v.OwnerUserId AND v.IsPrivate = 1
GROUP BY u.Id, u.Email, u.FullName
HAVING COUNT(v.Id) > 0
ORDER BY VoucherCount DESC;

---

-- 7. VOUCHER HẾT HẠN (Sắp hết hạn)
-- ============================================================================
SELECT 
    v.Id,
    v.Code,
    v.Name,
    v.EndDate,
    DATEDIFF(DAY, GETDATE(), v.EndDate) AS DaysLeft,
    CASE 
        WHEN v.EventId IS NOT NULL THEN 'Event'
        WHEN v.IsPrivate = 1 THEN 'Sprint'
        ELSE 'Public'
    END AS VoucherType,
    CASE WHEN v.EndDate >= GETDATE() THEN 'Active' ELSE 'Expired' END AS Status
FROM Vouchers v
WHERE v.EndDate <= DATEADD(DAY, 7, GETDATE()) -- Sắp hết hạn trong 7 ngày
ORDER BY v.EndDate ASC;

---

-- 8. MIGRATION: Chuyển Voucher giữa Tab
-- ============================================================================
-- Chuyển từ Sprint sang Chung (khi user không muốn private nữa)
-- UPDATE Vouchers 
-- SET IsPrivate = 0, OwnerUserId = NULL 
-- WHERE Id = @VoucherId AND IsPrivate = 1;

-- Chuyển từ Chung sang Sprint (tạo private cho user)
-- UPDATE Vouchers 
-- SET IsPrivate = 1, OwnerUserId = @UserId 
-- WHERE Id = @VoucherId AND IsPrivate = 0;

-- Gắn Voucher Chung vào Event (chuyển sang Event Tab)
-- UPDATE Vouchers 
-- SET EventId = @EventId 
-- WHERE Id = @VoucherId AND EventId IS NULL;

---

-- 9. KIỂM TRA VOUCHER KHÔNG HỢP LỆ (Orphaned)
-- ============================================================================
-- Voucher Sprint nhưng không có user owner
SELECT * FROM Vouchers 
WHERE IsPrivate = 1 AND OwnerUserId IS NULL;

-- Voucher Event nhưng event không tồn tại
SELECT v.* FROM Vouchers v
LEFT JOIN Events e ON v.EventId = e.Id
WHERE v.EventId IS NOT NULL AND e.Id IS NULL;

---

-- 10. CLEANUP: Xóa Voucher Hết Hạn (>30 ngày)
-- ============================================================================
-- Chỉ xóa private vouchers hết hạn
-- DELETE FROM Vouchers 
-- WHERE IsPrivate = 1 
-- AND EndDate < DATEADD(DAY, -30, GETDATE())
-- AND UsedQuantity = 0;  -- Chưa sử dụng

-- Hoặc chỉ soft-delete (nếu có IsDeleted column)
-- UPDATE Vouchers 
-- SET IsActive = 0
-- WHERE EndDate < GETDATE() AND IsActive = 1;

---

-- 11. REPORT: Doanh Thu Theo Voucher Type
-- ============================================================================
SELECT 
    CASE 
        WHEN v.EventId IS NOT NULL THEN 'Event'
        WHEN v.IsPrivate = 1 THEN 'Sprint'
        ELSE 'Public'
    END AS VoucherType,
    COUNT(vu.Id) AS UsageCount,
    SUM(o.TotalVoucherDiscount) AS TotalDiscount,
    COUNT(DISTINCT vu.UserId) AS UniqueUsers
FROM Vouchers v
LEFT JOIN VoucherUsages vu ON v.Id = vu.VoucherId
LEFT JOIN Orders o ON vu.OrderId = o.Id
GROUP BY 
    CASE 
        WHEN v.EventId IS NOT NULL THEN 'Event'
        WHEN v.IsPrivate = 1 THEN 'Sprint'
        ELSE 'Public'
    END
ORDER BY UsageCount DESC;

---

-- 12. EXPORT: Danh Sách Voucher Sprint Cho User
-- ============================================================================
-- Lấy tất cả vouchers của 1 user (ID = 5)
SELECT 
    v.Id,
    v.Code,
    v.Name,
    v.DiscountPercent,
    v.DiscountAmount,
    v.StartDate,
    v.EndDate,
    v.MinOrderAmount,
    CASE WHEN v.EndDate >= GETDATE() THEN 'Còn hạn' ELSE 'Hết hạn' END AS Status,
    CASE 
        WHEN v.IsLuckySpinVoucher = 1 THEN 'Lucky Spin'
        WHEN v.Code LIKE 'EVENT_%' THEN 'Event'
        ELSE 'Private'
    END AS Source
FROM Vouchers v
WHERE v.OwnerUserId = 5 AND v.IsPrivate = 1
ORDER BY v.EndDate DESC;

---

-- ============================================================================
-- EXECUTION NOTES
-- ============================================================================
-- 1. Thay @VoucherId, @UserId, @EventId bằng giá trị thực
-- 2. Uncomment DELETE/UPDATE khi chắc chắn muốn thay đổi
-- 3. Luôn backup trước khi chạy mutation queries
-- 4. Kiểm tra transaction logs nếu có lỗi
-- ============================================================================