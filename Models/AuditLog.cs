using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required(ErrorMessage = "Action b?t bu?c")]
        [StringLength(100)]
        public string Action { get; set; } // Create, Update, Delete, Ban, Warn, Pay, etc.

        [Required(ErrorMessage = "Entity b?t bu?c")]
        [StringLength(100)]
        public string Entity { get; set; } // News, User, Order, Post, etc.

        public int? EntityId { get; set; }

        [StringLength(1000)]
        public string OldValue { get; set; } // JSON c?a giá tr? c? (n?u Update)

        [StringLength(1000)]
        public string NewValue { get; set; } // JSON c?a giá tr? m?i

        [StringLength(500)]
        public string Description { get; set; } // Mô t? chi ti?t

        [StringLength(50)]
        public string IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
