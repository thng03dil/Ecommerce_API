

namespace Ecommerce.Application.DTOs.Role
{
    public class RoleResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
