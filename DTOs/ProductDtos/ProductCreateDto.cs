using System.ComponentModel.DataAnnotations;

namespace Ecommerce_API.DTOs.ProductDtos
{
    public class ProductCreateDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
