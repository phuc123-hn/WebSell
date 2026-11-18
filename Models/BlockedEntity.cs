using System;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    /// <summary>
    /// Blocked Entity - L?u IP/Device b? ch?n t?m th?i
    /// Tách riêng bi?t kh?i OTP và RateLimit
    /// </summary>
    public class BlockedEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Identifier { get; set; } // IP address ho?c Device Fingerprint

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } // IpAddress, Device, Email

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } // Login, Register, Payment, etc.

        [Required]
        [StringLength(50)]
        public string Reason { get; set; } // TooManyAttempts, SuspiciousActivity, ManualBlock, etc.

        // Th?i gian h?t h?n block
        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Admin có th? ghi chú
        [StringLength(500)]
        public string Notes { get; set; }

        // Risk score lúc block
        [Range(0, 100)]
        public int RiskScore { get; set; } = 0;

        // Có ph?i block v?nh vi?n?
        public bool IsPermanent { get; set; } = false;
    }
}
