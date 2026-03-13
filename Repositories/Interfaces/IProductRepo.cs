using Ecommerce_API.Pagination;
using Ecommerce_API.DTOs.Filters;
using Ecommerce_API.Models;

namespace Ecommerce_API.Repositories.Interfaces
{
    public interface IProductRepo
    {
       // Task<(IEnumerable<Product>, int totalCount)> GetAllAsync(PaginationDto pagedto);

        Task<int> CountAsync();

        Task<bool> CategoryExistsAsync(int categoryId);

        Task LoadCategoryAsync(Product product);

        Task CreateAsync(Product product);

        Task UpdateAsync(Category category);

        Task SaveChangesAsync();

        Task<Product?> GetByIdAsync(int id);

        Task<(IEnumerable<Product>, int)> GetFilteredAsync(ProductFilterDto filter, PaginationDto pagination);

    }
}
