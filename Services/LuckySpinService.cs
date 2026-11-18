using System;
using System.Linq;
using web1.Data;
using web1.Models;

namespace web1.Services
{
    public class LuckySpinService
    {
        private readonly ApplicationDbContext _db;
        private readonly Random _random = new Random();

        public LuckySpinService()
        {
            _db = new ApplicationDbContext();
        }

        /// <summary>
        /// Thực hiện quay may mắn cho user sau khi thanh toán thành công
        /// </summary>
        public LuckySpinHistory SpinLucky(int userId, int orderId)
        {
            try
            {
                // Lấy tất cả prize đang hoạt động
                var prizes = _db.LuckySpinPrizes.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToList();

                if (prizes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SpinLucky Error: No active prizes found");
                    return null;
                }

                // Quay may mắn dựa vào WinRate
                var selectedPrize = SpinWheelByWinRate(prizes);

                if (selectedPrize == null)
                    return null;

                // Tạo LuckySpinHistory record
                var spinHistory = new LuckySpinHistory
                {
                    UserId = userId,
                    OrderId = orderId == 0 ? (int?)null : orderId,
                    PrizeId = selectedPrize.Id,
                    Status = "Pending",
                    SpinDate = DateTime.Now
                };

                // Xử lý nếu quay được voucher
                if (selectedPrize.PrizeType == "Voucher" && selectedPrize.VoucherId.HasValue)
                {
                    var voucher = _db.Vouchers.Find(selectedPrize.VoucherId.Value);
                    if (voucher != null)
                    {
                        // Chỉ ghi reference voucher gốc vào WonVoucherId
                        // (không tạo private voucher để tránh DB constraint issues)
                        spinHistory.WonVoucherId = voucher.Id;
                        spinHistory.WonVoucher = voucher;
                        
                        // TỰ ĐỘNG CẤP VOUCHER
                        spinHistory.Status = "Claimed";
                        spinHistory.ClaimedDate = DateTime.Now;
                    }
                }

                // Xử lý nếu quay được sản phẩm
                if (selectedPrize.PrizeType == "Product" && selectedPrize.ProductId.HasValue)
                {
                    spinHistory.WonProductId = selectedPrize.ProductId.Value;
                }

                _db.LuckySpinHistories.Add(spinHistory);
                _db.SaveChanges();

                // Load navigation properties sau khi save
                spinHistory.Prize = _db.LuckySpinPrizes.Find(spinHistory.PrizeId);
                if (spinHistory.WonVoucherId.HasValue)
                    spinHistory.WonVoucher = _db.Vouchers.Find(spinHistory.WonVoucherId.Value);
                if (spinHistory.WonProductId.HasValue)
                    spinHistory.WonProduct = _db.Products.Find(spinHistory.WonProductId.Value);

                return spinHistory;
            }
            catch (Exception ex)
            {
                var errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] SpinLucky Exception: {ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException?.Message}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                
                // Log to file for debugging
                try
                {
                    string logPath = System.AppDomain.CurrentDomain.BaseDirectory + "logs\\spinerror.log";
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                    System.IO.File.AppendAllText(logPath, errorMsg + "\r\n---\r\n");
                }
                catch { }
                
                return null;
            }
        }


        /// <summary>
        /// Chọn giải thưởng dựa vào WinRate
        /// </summary>
        private LuckySpinPrize SpinWheelByWinRate(System.Collections.Generic.List<LuckySpinPrize> prizes)
        {
            // Tính tổng tỷ lệ
            decimal totalRate = prizes.Sum(p => p.WinRate);

            if (totalRate <= 0)
                return null;

            // Chọn số random từ 0 đến tổng tỷ lệ
            double randomValue = _random.NextDouble() * (double)totalRate;

            decimal cumulative = 0;
            foreach (var prize in prizes)
            {
                cumulative += prize.WinRate;
                if (randomValue <= (double)cumulative)
                {
                    return prize;
                }
            }

            return prizes.LastOrDefault();
        }

        /// <summary>
        /// Nhận voucher từ lịch sử quay
        /// </summary>
        public bool ClaimVoucher(int spinHistoryId, int userId)
        {
            try
            {
                var spinHistory = _db.LuckySpinHistories.Find(spinHistoryId);

                if (spinHistory == null || spinHistory.UserId != userId)
                    return false;

                if (spinHistory.Status != "Pending")
                    return false;

                spinHistory.Status = "Claimed";
                spinHistory.ClaimedDate = DateTime.Now;

                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
