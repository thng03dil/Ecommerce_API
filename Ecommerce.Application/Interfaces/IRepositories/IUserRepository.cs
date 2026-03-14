using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces.IRepositories
{
    public interface IUserRepository
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
