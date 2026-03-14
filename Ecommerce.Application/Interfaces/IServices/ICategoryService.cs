using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.DTOs.Filters;
using Ecommerce.Application.Pagination;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<PagedResponse<CategoryResponseDto>>> GetAllAsync(CategoryFilterDto filter, PaginationDto pagination);
        Task<ApiResponse<CategoryResponseDto?>> GetByIdAsync(int id);
        Task<ApiResponse<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto);
        Task<ApiResponse<CategoryResponseDto>> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<ApiResponse<CategoryResponseDto>> DeleteAsync(int id);

    }
}
