using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class OrderVoucher
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public int VoucherId { get; set; }

        [ForeignKey("VoucherId")]
        public virtual Voucher Voucher { get; set; }

        // Tiền giảm từ voucher này
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }
}
