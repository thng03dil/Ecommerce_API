using System.ComponentModel.DataAnnotations;

namespace Ecommerce_API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        [StringLength(150)]
        public string slug { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
