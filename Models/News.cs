using System;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    public class News
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề tin tức bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(5000)]
        public string Content { get; set; }

        public string ImageUrl { get; set; } // ~/Content/Images/news1.jpg

        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        public string Category { get; set; } // Technology / Defense

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string Summary { get; set; }
    }
}
