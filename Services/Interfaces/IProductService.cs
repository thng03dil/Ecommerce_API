using Ecommerce_API.DTOs.ProductDtos;
using Ecommerce_API.Helpers.Pagination;
using Ecommerce_API.DTOs.Common;
using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.Helpers.Responses;
using Ecommerce_API.DTOs.Filters;

namespace Ecommerce_API.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResponse<ProductResponseDto>> GetAllAsync(ProductFilterDto filter, PaginationDto pagedto);
        Task<ApiResponse<ProductResponseDto?>> GetByIdAsync(int id);
        Task<ApiResponse<ProductResponseDto>> CreateAsync(ProductCreateDto dto);
        Task<ApiResponse<ProductResponseDto>> UpdateAsync(int id, ProductUpdateDto dto);
        Task<ApiResponse<ProductResponseDto>> DeleteAsync(int id);
    }
}
