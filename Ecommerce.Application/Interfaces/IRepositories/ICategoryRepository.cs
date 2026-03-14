using Ecommerce.Application.DTOs.Filters;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.Pagination;

namespace Ecommerce.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);

        Task CreateAsync(Category category);

        Task UpdateAsync(Category category);

        Task SaveChangesAsync();

        Task<Category?> GetByIdForUpdateAsync(int id);
        Task<bool> SlugExistsAsync(string slug, int excludeId);
        Task<(IEnumerable<Category>, int)> GetFilteredAsync(CategoryFilterDto filter, PaginationDto pagination);

    }
}
