using System;
using System.Collections.Generic;
using System.Linq;
using web1.Data;
using web1.Models;

namespace web1.Services
{
    public class EventVoucherService
    {
        private readonly ApplicationDbContext _db;

        public EventVoucherService()
        {
            _db = new ApplicationDbContext();
        }

        /// <summary>
        /// C?p voucher cho user khi hoàn thành quiz s? ki?n
        /// N?u s? ki?n có 2 voucher: score >= 7 nh?n voucher cao, score < 7 nh?n voucher nh?
        /// N?u s? ki?n có 1 voucher: c?p voucher ?ó
        /// N?u s? ki?n có >2 voucher: c?p t?t c?
        /// </summary>
        public List<Voucher> GrantEventVouchersToUser(int eventId, int userId, int score)
        {
            try
            {
                var grantedVouchers = new List<Voucher>();

                // L?y t?t c? voucher c?a event (ch?a b? xóa, active)
                var eventVouchers = _db.Vouchers
                    .Where(v => v.EventId == eventId && v.IsActive)
                    .OrderBy(v => v.DiscountPercent)
                    .ToList();

                if (eventVouchers.Count == 0)
                    return grantedVouchers;

                if (eventVouchers.Count == 1)
                {
                    // N?u ch? có 1 voucher: c?p nó
                    var voucher = CreatePrivateEventVoucher(eventVouchers[0], userId);
                    grantedVouchers.Add(voucher);
                }
                else if (eventVouchers.Count == 2)
                {
                    // N?u có 2 voucher: d?a vào score
                    // Voucher ??u tiên (sau sort) là nh?, voucher th? 2 là l?n
                    if (score >= 7)
                    {
                        // Score cao: c?p voucher l?n (th? 2)
                        var voucher = CreatePrivateEventVoucher(eventVouchers[1], userId);
                        grantedVouchers.Add(voucher);
                    }
                    else
                    {
                        // Score th?p: c?p voucher nh? (th? 1)
                        var voucher = CreatePrivateEventVoucher(eventVouchers[0], userId);
                        grantedVouchers.Add(voucher);
                    }
                }
                else
                {
                    // N?u có >2 voucher: c?p t?t c?
                    foreach (var voucher in eventVouchers)
                    {
                        var privateVoucher = CreatePrivateEventVoucher(voucher, userId);
                        grantedVouchers.Add(privateVoucher);
                    }
                }

                return grantedVouchers;
            }
            catch (Exception)
            {
                return new List<Voucher>();
            }
        }

        /// <summary>
        /// T?o private voucher t? event voucher g?c
        /// </summary>
        private Voucher CreatePrivateEventVoucher(Voucher baseVoucher, int userId)
        {
            string uniqueCode = GenerateUniqueEventVoucherCode(baseVoucher.Code, userId);

            var privateVoucher = new Voucher
            {
                Code = uniqueCode,
                Name = baseVoucher.Name,
                Description = baseVoucher.Description + " (T? s? ki?n)",
                DiscountPercent = baseVoucher.DiscountPercent,
                DiscountAmount = baseVoucher.DiscountAmount,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30), // H?n dùng 30 ngày
                Quantity = 1,
                MinOrderAmount = baseVoucher.MinOrderAmount,
                IsActive = true,
                IsLuckySpinVoucher = false,
                IsPrivate = true,
                OwnerUserId = userId,
                CreatedAt = DateTime.Now
            };

            _db.Vouchers.Add(privateVoucher);
            _db.SaveChanges();

            return privateVoucher;
        }

        /// <summary>
        /// T?o mã voucher unique cho event voucher
        /// </summary>
        private string GenerateUniqueEventVoucherCode(string baseCode, int userId)
        {
            // Format: EVENT_{OriginalCode}_{UserId}_{RandomString}
            // Ví d?: EVENT_SALE30_2_A7C5E9F
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
            string uniqueCode = $"EVENT_{baseCode}_{userId}_{randomPart}";

            // ??m b?o code không t?n t?i trong database
            while (_db.Vouchers.Any(v => v.Code == uniqueCode))
            {
                randomPart = Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
                uniqueCode = $"EVENT_{baseCode}_{userId}_{randomPart}";
            }

            return uniqueCode;
        }
    }
}
