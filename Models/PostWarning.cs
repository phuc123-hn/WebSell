using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    [Table("PostWarnings")]
    public class PostWarning
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [Required]
        public int AdminId { get; set; }

        [ForeignKey("AdminId")]
        public virtual User Admin { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime WarningDate { get; set; } = DateTime.Now;

        // 0 = Warning, 1 = Removed, 2 = User Banned
        [Required]
        public int Action { get; set; } = 0;
    }
}
