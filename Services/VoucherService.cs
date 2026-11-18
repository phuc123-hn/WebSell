using System;
using System.Collections.Generic;
using System.Linq;
using web1.Data;
using web1.Models;

namespace web1.Services
{
    public class VoucherService
    {
        private readonly ApplicationDbContext _db;

        public VoucherService()
        {
            _db = new ApplicationDbContext();
        }

        /// <summary>
        /// Kiểm tra voucher có hợp lệ không
        /// </summary>
        public VoucherValidationResult ValidateVoucher(string code, int userId, decimal orderAmount, List<int> appliedVoucherIds = null)
        {
            var result = new VoucherValidationResult();

            // Tìm voucher
            var voucher = _db.Vouchers.FirstOrDefault(v => v.Code.ToUpper() == code.ToUpper());

            if (voucher == null)
            {
                result.IsValid = false;
                result.Message = "Mã voucher không tồn tại";
                return result;
            }

            // Kiểm tra trạng thái
            if (!voucher.IsActive)
            {
                result.IsValid = false;
                result.Message = "Mã voucher không còn hiệu lực";
                return result;
            }

            // Kiểm tra hạn
            if (voucher.EndDate < DateTime.Now)
            {
                result.IsValid = false;
                result.Message = "Mã voucher đã hết hạn";
                return result;
            }

            if (voucher.StartDate > DateTime.Now)
            {
                result.IsValid = false;
                result.Message = "Mã voucher chưa được kích hoạt";
                return result;
            }

            // SECURITY: Kiểm tra voucher private
            // Private vouchers chỉ có thể được sử dụng bởi OwnerUserId
            if (voucher.IsPrivate)
            {
                if (!voucher.OwnerUserId.HasValue || voucher.OwnerUserId.Value != userId)
                {
                    result.IsValid = false;
                    result.Message = "Voucher này không dành cho bạn";
                    return result;
                }
            }

            // Kiểm tra số lượng (chỉ áp dụng cho public vouchers)
            if (!voucher.IsPrivate && voucher.Quantity > 0 && voucher.UsedQuantity >= voucher.Quantity)
            {
                result.IsValid = false;
                result.Message = "Mã voucher đã hết";
                return result;
            }

            // Kiểm tra đơn hàng tối thiểu
            if (orderAmount < voucher.MinOrderAmount)
            {
                result.IsValid = false;
                result.Message = $"Đơn hàng tối thiểu {voucher.MinOrderAmount.ToString("N0")}đ";
                return result;
            }

            // Kiểm tra voucher đã dùng chưa (chỉ cho non-private vouchers)
            if (!voucher.IsPrivate)
            {
                var hasUsed = _db.VoucherUsages.Any(vu => vu.VoucherId == voucher.Id && vu.UserId == userId);
                if (hasUsed)
                {
                    result.IsValid = false;
                    result.Message = "Voucher đã được sử dụng, không dùng lần nữa";
                    return result;
                }
            }

            // Kiểm tra voucher này đã được apply trong đơn hàng này chưa
            if (appliedVoucherIds != null && appliedVoucherIds.Contains(voucher.Id))
            {
                result.IsValid = false;
                result.Message = "Voucher này đã được áp dụng";
                return result;
            }

            result.IsValid = true;
            result.Voucher = voucher;
            result.DiscountAmount = CalculateDiscount(orderAmount, voucher);
            result.Message = $"Áp dụng thành công! Tiết kiệm {result.DiscountAmount.ToString("N0")}đ";

            return result;
        }

        /// <summary>
        /// Tính tiền giảm
        /// </summary>
        public decimal CalculateDiscount(decimal orderAmount, Voucher voucher)
        {
            if (voucher.DiscountPercent > 0)
            {
                return orderAmount * voucher.DiscountPercent / 100;
            }
            else if (voucher.DiscountAmount > 0)
            {
                return voucher.DiscountAmount;
            }

            return 0;
        }

        /// <summary>
        /// Áp dụng nhiều voucher cho order
        /// </summary>
        public bool ApplyVouchers(Order order, List<int> voucherIds)
        {
            try
            {
                decimal totalDiscount = 0;
                var appliedVouchers = new List<int>();

                foreach (var voucherId in voucherIds ?? new List<int>())
                {
                    var voucher = _db.Vouchers.Find(voucherId);
                    if (voucher == null)
                        continue;

                    var discount = CalculateDiscount(order.TotalAmount, voucher);
                    totalDiscount += discount;

                    // Thêm vào OrderVoucher
                    var orderVoucher = new OrderVoucher
                    {
                        OrderId = order.Id,
                        VoucherId = voucherId,
                        DiscountAmount = discount,
                        AppliedAt = DateTime.Now
                    };

                    _db.OrderVouchers.Add(orderVoucher);
                    appliedVouchers.Add(voucher.Id);

                    // Cập nhật AppliedVoucherCodes
                    order.AppliedVoucherCodes += (string.IsNullOrEmpty(order.AppliedVoucherCodes) ? "" : ",") + voucher.Code;
                }

                order.TotalVoucherDiscount = totalDiscount;
                order.TotalAmount -= (int)totalDiscount;

                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class VoucherValidationResult
    {
        public bool IsValid { get; set; } = false;
        public string Message { get; set; }
        public Voucher Voucher { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
    }
}
