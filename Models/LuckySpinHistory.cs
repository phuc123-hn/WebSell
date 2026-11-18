using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class LuckySpinHistory
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        // Prize được quay được
        public int PrizeId { get; set; }

        [ForeignKey("PrizeId")]
        public virtual LuckySpinPrize Prize { get; set; }

        // Nếu quay được voucher, lưu voucher được tạo
        public int? WonVoucherId { get; set; }

        [ForeignKey("WonVoucherId")]
        public virtual Voucher WonVoucher { get; set; }

        // Nếu quay được sản phẩm, lưu product được chọn
        public int? WonProductId { get; set; }

        [ForeignKey("WonProductId")]
        public virtual Product WonProduct { get; set; }

        // Trạng thái: Pending, Claimed, Expired
        public string Status { get; set; } = "Pending";

        public DateTime SpinDate { get; set; } = DateTime.Now;

        public DateTime? ClaimedDate { get; set; }
    }
}
