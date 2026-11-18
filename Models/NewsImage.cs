using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class NewsImage
    {
        [Key]
        public int Id { get; set; }

        public int NewsId { get; set; }

        [ForeignKey("NewsId")]
        public virtual News News { get; set; }

        [Required(ErrorMessage = "???ng d?n ?nh b?t bu?c")]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Range(0, 999)]
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }
}
