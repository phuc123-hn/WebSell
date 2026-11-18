using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    [Table("UserBans")]
    public class UserBan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int WarningCount { get; set; } = 0;

        [Required]
        public bool IsBanned { get; set; } = false;

        public DateTime? BanStartDate { get; set; }

        public DateTime? BanEndDate { get; set; }

        [StringLength(500)]
        public string BanReason { get; set; }

        public int? BanDays { get; set; } = 7; // M?c ??nh ban 7 ngày

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
