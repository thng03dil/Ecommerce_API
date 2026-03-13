using Ecommerce_API.DTOs.ProductDtos;
using Ecommerce_API.Pagination;
using Ecommerce_API.Helpers.Responses;
using Ecommerce_API.DTOs.Filters;

namespace Ecommerce_API.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResponse<ProductResponseDto>>> GetAllAsync(ProductFilterDto filter, PaginationDto pagination);
        Task<ApiResponse<ProductResponseDto?>> GetByIdAsync(int id);
        Task<ApiResponse<ProductResponseDto>> CreateAsync(ProductCreateDto dto);
        Task<ApiResponse<ProductResponseDto>> UpdateAsync(int id, ProductUpdateDto dto);
        Task<ApiResponse<ProductResponseDto>> DeleteAsync(int id);
    }
}
