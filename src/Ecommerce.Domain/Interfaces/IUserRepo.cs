
using Ecommerce.Application.Common.Pagination;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface IUserRepo
    {
        Task<(IEnumerable<User>,int totalCount)> GetAllAsync(PaginationDto pagedto);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByIdWithPermissionsAsync(int id);
        Task<User?> GetByIdForUpdateAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistingAsync(string email);
        
        Task AddAsync(User user); 
        Task SaveChangesAsync();
    }
}
 