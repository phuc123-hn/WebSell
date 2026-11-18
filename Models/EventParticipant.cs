using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class EventParticipant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Range(0, 10)]
        public int Score { get; set; } = 0;

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        public DateTime ParticipatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }
    }
}
