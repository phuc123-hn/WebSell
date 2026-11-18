using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string ShippingAddress { get; set; }

        [StringLength(20)]
        public string ShippingPhone { get; set; }

        [StringLength(100)]
        public string ShippingName { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Processing / Shipped / Delivered / Cancelled

        public string PaymentMethod { get; set; } = "COD"; // COD / VNPay / Bank Transfer

        public bool IsPaid { get; set; } = false;

        public string Notes { get; set; }

        // NEW: Lưu các voucher được sử dụng (JSON hoặc comma-separated)
        [StringLength(500)]
        public string AppliedVoucherCodes { get; set; } = ""; // VD: "SALE5,SALE10,DISC20"

        // NEW: Tổng tiền giảm từ vouchers
        [Range(0, double.MaxValue)]
        public decimal TotalVoucherDiscount { get; set; } = 0;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Navigation cho Multiple Vouchers
        public virtual ICollection<OrderVoucher> OrderVouchers { get; set; }
    }
}
