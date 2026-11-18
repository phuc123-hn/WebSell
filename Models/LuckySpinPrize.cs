using System;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    public class LuckySpinPrize
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // Tỷ lệ trúng (%)
        [Required]
        [Range(0, 100)]
        public decimal WinRate { get; set; }

        // Loại prize
        [Required]
        [StringLength(50)]
        public string PrizeType { get; set; } // "Message", "Voucher", "Product"

        // Nếu là voucher, lưu Voucher ID
        public int? VoucherId { get; set; }

        // Nếu là sản phẩm, lưu Product ID
        public int? ProductId { get; set; }

        // Thứ tự hiển thị
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
