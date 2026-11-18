using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    [Table("Reactions")]
    public class Reaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Reaction Type: like, love, haha, wow, angry
        [Required]
        [StringLength(20)]
        public string ReactionType { get; set; } // "like", "love", "haha", "wow", "angry"

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
