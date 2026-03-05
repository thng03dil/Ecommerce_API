using System.ComponentModel.DataAnnotations;

namespace Ecommerce_API.DTOs
{
    public class CategoryUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;
    }
}
