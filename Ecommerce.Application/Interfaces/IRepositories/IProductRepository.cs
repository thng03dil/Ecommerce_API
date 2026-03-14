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
    public interface IProductRepository
    {

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
