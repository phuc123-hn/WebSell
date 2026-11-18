using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; }

        public string Type { get; set; } // Electronics / Components

        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
