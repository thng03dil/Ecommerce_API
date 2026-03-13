
using Ecommerce_API.DTOs.Filters;
using Ecommerce_API.Models;
using Ecommerce_API.Pagination;

namespace Ecommerce_API.Repositories.Interfaces
{
    public interface ICategoryRepo
    {
        
            //Task<(IEnumerable<Category>,int totalCount)> GetAllAsync(PaginationDto pagedto);

            Task<Category?> GetByIdAsync(int id);

            Task CreateAsync(Category category);

            Task UpdateAsync(Category category);

            Task SaveChangesAsync();

            Task<Category?> GetByIdForUpdateAsync(int id);
            Task<bool> SlugExistsAsync(string slug, int excludeId);
            Task<(IEnumerable<Category>, int)> GetFilteredAsync(CategoryFilterDto filter, PaginationDto pagination);


    }
}
