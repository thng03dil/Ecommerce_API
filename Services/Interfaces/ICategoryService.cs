using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.Pagination;
using Ecommerce_API.DTOs.Filters;
using Ecommerce_API.Helpers.Responses;
using Ecommerce_API.DTOs.ProductDtos;

namespace Ecommerce_API.Services.Interfaces
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

