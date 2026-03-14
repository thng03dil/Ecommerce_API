using Ecommerce.Application.DTOs.ProductDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.Pagination;
using Ecommerce.Application.DTOs.Filters;

namespace Ecommerce.Application.Interfaces.Services
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
