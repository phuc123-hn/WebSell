-- ============================================================================
-- DEMO: Cấp Voucher Sự Kiện Dựa Vào Điểm Quiz
-- ============================================================================
-- Tạo sự kiện "Hè 2024" với 2 voucher (1 nhỏ 1 lớn)
-- ============================================================================

-- 1. Tạo Sự Kiện
DECLARE @EventId INT;

INSERT INTO Events (Name, Description, StartDate, EndDate, ImageUrl, IsActive, CreatedAt)
VALUES (
    N'Hè 2024 - Thi đua Quiz',
    N'Tham gia quiz và nhận voucher giảm giá dựa vào điểm của bạn',
    GETDATE(),
    DATEADD(DAY, 30, GETDATE()),
    '/Content/Event/summer2024.jpg',
    1,
    GETDATE()
);

SET @EventId = @@IDENTITY;
PRINT 'Event ID: ' + CAST(@EventId AS VARCHAR(10));

-- 2. Tạo 2 Voucher cho Event (1 nhỏ 1 lớn)
-- VOUCHER NHỎ (10%)
INSERT INTO Vouchers (
    Code, Name, Description, 
    DiscountPercent, DiscountAmount, 
    StartDate, EndDate, 
    Quantity, UsedQuantity, 
    MinOrderAmount,
    IsActive, IsPrivate, OwnerUserId, 
    IsLuckySpinVoucher, RequireEventParticipation,
    EventId, 
    CreatedAt
)
VALUES (
    'SUMMER_SMALL_10',
    N'Giảm 10% - Voucher Nhỏ',
    N'Giảm giá 10% cho các bạn tham gia sự kiện Hè 2024',
    10,      -- DiscountPercent = 10%
    0,       -- DiscountAmount = 0
    GETDATE(),
    DATEADD(DAY, 90, GETDATE()),
    0,       -- Vô hạn
    0,
    100000,  -- Tối thiểu 100k
    1, 0, NULL,
    0, 0,
    @EventId,
    GETDATE()
);

-- VOUCHER LỚN (20%)
INSERT INTO Vouchers (
    Code, Name, Description, 
    DiscountPercent, DiscountAmount, 
    StartDate, EndDate, 
    Quantity, UsedQuantity, 
    MinOrderAmount,
    IsActive, IsPrivate, OwnerUserId, 
    IsLuckySpinVoucher, RequireEventParticipation,
    EventId, 
    CreatedAt
)
VALUES (
    'SUMMER_LARGE_20',
    N'Giảm 20% - Voucher Lớn',
    N'Giảm giá 20% cho các bạn đạt điểm cao trong sự kiện Hè 2024',
    20,      -- DiscountPercent = 20%
    0,       -- DiscountAmount = 0
    GETDATE(),
    DATEADD(DAY, 90, GETDATE()),
    0,       -- Vô hạn
    0,
    100000,  -- Tối thiểu 100k
    1, 0, NULL,
    0, 0,
    @EventId,
    GETDATE()
);

PRINT 'Voucher đã tạo';

-- 3. Tạo 5 câu hỏi cho Event
INSERT INTO EventQuestions (EventId, Question, OptionA, OptionB, OptionC, OptionD, CorrectAnswer)
VALUES 
    (@EventId, N'CPU là viết tắt của cụm từ nào?', N'Central Power Unit', N'Central Processing Unit', N'Computer Personal Unit', N'Central Protocol Unit', 'B'),
    (@EventId, N'RAM là bộ nhớ loại nào?', N'Read-Access Memory', N'Random Access Memory', N'Read-Only Memory', N'Rapid Access Memory', 'B'),
    (@EventId, N'SSD có tốc độ nhanh hơn HDD không?', N'Không, bằng nhau', N'Không, HDD nhanh hơn', N'Có, SSD nhanh hơn', N'Không xác định được', 'C'),
    (@EventId, N'GPU chủ yếu dùng để làm gì?', N'Xử lý âm thanh', N'Xử lý đồ họa', N'Quản lý bộ nhớ', N'Kết nối internet', 'B'),
    (@EventId, N'Công ty nào sản xuất Intel?', N'AMD', N'NVIDIA', N'Intel Corporation', N'Qualcomm', 'C');

PRINT 'Câu hỏi đã tạo';

-- 4. Kiểm tra dữ liệu
PRINT '';
PRINT '========== KIỂM TRA DỮ LIỆU ==========';
PRINT '';

PRINT 'Sự kiện được tạo:';
SELECT Id, Name, StartDate, EndDate FROM Events WHERE Id = @EventId;

PRINT '';
PRINT 'Voucher được gắn với Event:';
SELECT Id, Code, Name, DiscountPercent, EventId FROM Vouchers WHERE EventId = @EventId ORDER BY DiscountPercent;

PRINT '';
PRINT 'Câu hỏi của Event:';
SELECT Id, Question, CorrectAnswer FROM EventQuestions WHERE EventId = @EventId;

-- 5. Kịch bản Test
PRINT '';
PRINT '========== KỊCH BẢN TEST ==========';
PRINT '';
PRINT 'KỊCH BẢN: 2 user làm quiz sự kiện này';
PRINT '';
PRINT 'User 1 (Điểm cao):';
PRINT '  - Trả lời đúng 4/5 câu';
PRINT '  - Score = 8/10 (>= 7)';
PRINT '  - --> Nhận SUMMER_LARGE_20 (Giảm 20%)';
PRINT '';
PRINT 'User 2 (Điểm thấp):';
PRINT '  - Trả lời đúng 2/5 câu';
PRINT '  - Score = 4/10 (< 7)';
PRINT '  - --> Nhận SUMMER_SMALL_10 (Giảm 10%)';
PRINT '';
PRINT 'Sau khi hoàn thành quiz:';
PRINT '  - Hệ thống tự động cấp private voucher cho từng user';
PRINT '  - Private voucher có mã: EVENT_{OriginalCode}_{UserId}_{RandomString}';
PRINT '  - Hạn sử dụng: 30 ngày';
PRINT '';

-- 6. Query để xem private voucher được cấp
PRINT '';
PRINT '========== QUERY XEM PRIVATE VOUCHER ĐÃ CẤP ==========';
PRINT '';
PRINT 'Chạy query này sau khi user làm quiz:';
PRINT '';
PRINT 'SELECT * FROM Vouchers ';
PRINT 'WHERE IsPrivate = 1 ';
PRINT '  AND IsLuckySpinVoucher = 0 ';
PRINT '  AND Code LIKE ''EVENT_%'' ';
PRINT '  AND OwnerUserId IN (SELECT Id FROM Users WHERE Email = ''user@email.com'') ';
PRINT 'ORDER BY CreatedAt DESC;';
PRINT '';

-- 7. Xóa dữ liệu (nếu cần rollback)
PRINT '';
PRINT '========== ROLLBACK (Xóa dữ liệu test) ==========';
PRINT '';
PRINT 'Để xóa sự kiện này, chạy:';
PRINT '';
PRINT 'DELETE FROM EventQuestions WHERE EventId = ' + CAST(@EventId AS VARCHAR(10)) + ';';
PRINT 'DELETE FROM EventParticipants WHERE EventId = ' + CAST(@EventId AS VARCHAR(10)) + ';';
PRINT 'DELETE FROM Vouchers WHERE EventId = ' + CAST(@EventId AS VARCHAR(10)) + ';';
PRINT 'DELETE FROM Events WHERE Id = ' + CAST(@EventId AS VARCHAR(10)) + ';';
PRINT '';

PRINT '';
PRINT '✔ Demo data created successfully!';
PRINT 'Event ID: ' + CAST(@EventId AS VARCHAR(10));