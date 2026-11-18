using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã voucher bắt buộc")]
        [StringLength(50)]
        [Index(IsUnique = true)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên voucher bắt buộc")]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm từ 0 đến 100")]
        public decimal DiscountPercent { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Required(ErrorMessage = "Ngày bắt đầu bắt buộc")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc bắt buộc")]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int UsedQuantity { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal MinOrderAmount { get; set; } = 0;

        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public bool IsActive { get; set; } = true;

        public bool RequireEventParticipation { get; set; } = false;

        // NEW: Lucky Spin Voucher
        public bool IsLuckySpinVoucher { get; set; } = false; // Voucher từ vòng quay may mắn
        
        // NEW: Private Voucher - Chỉ người dùng cụ thể mới dùng được
        public bool IsPrivate { get; set; } = false;
        
        // NEW: User sở hữu voucher private này
        public int? OwnerUserId { get; set; }
        
        [ForeignKey("OwnerUserId")]
        public virtual User OwnerUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<VoucherUsage> VoucherUsages { get; set; }
    }
}