using Ecommerce_API.Models;

namespace Ecommerce_API.Repositories.Interfaces
{
    public interface IUserRepo
    {

        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistingAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);

        Task UpdateAsync(User user);

        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
