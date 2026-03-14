using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.DTOs.ProductDTOs;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Interfaces.IRepositories;
using Ecommerce.Application.Pagination;
using Ecommerce.Application.DTOs.Filters;
using Ecommerce.Application.Interfaces.Repositories;
namespace Ecommerce.Application.Services
{
    public class ProductService : IProductService
    {

        private readonly IProductRepository _productRepo;
        public ProductService(
            IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<ApiResponse<PagedResponse<ProductResponseDto>>> GetAllAsync(ProductFilterDto filter, PaginationDto pagination)
        {
            //  var (products, totalItems) = await _productRepo.GetAllAsync( pagination);
            var (products, totalItems) = await _productRepo.GetFilteredAsync(filter, pagination);

            var data = products.Select(c => MapToResponseDto(c)).ToList();
            var pagedData = new PagedResponse<ProductResponseDto>(data, pagination.PageNumber, pagination.PageSize, totalItems);
            return ApiResponse<PagedResponse<ProductResponseDto>>.SuccessResponse(pagedData, "Get data successfully");
        }

        public async Task<ApiResponse<ProductResponseDto?>> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found");

            var item = MapToResponseDto(product);
            return ApiResponse<ProductResponseDto?>.SuccessResponse(
                    item,
                    "Get data successfully"
                );
        }

        public async Task<ApiResponse<ProductResponseDto>> CreateAsync(ProductCreateDto dto)
        {

            // Validate Category exist
            var categoryExist = await _productRepo.CategoryExistsAsync(dto.CategoryId);

            if (!categoryExist) throw new NotFoundException("Category not found");

            var product = new Product
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                Stock = dto.Stock,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _productRepo.CreateAsync(product);

            await _productRepo.LoadCategoryAsync(product);

            var item = MapToResponseDto(product);
            return ApiResponse<ProductResponseDto>.SuccessResponse(
                     item,
                     "Create data successfully"
                    );
        }

        public async Task<ApiResponse<ProductResponseDto>> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            if (dto.CategoryId != 0)
            {
                var categoryExists = await _productRepo.CategoryExistsAsync(dto.CategoryId);

                if (!categoryExists)
                    throw new NotFoundException("Category not found");

                product.CategoryId = dto.CategoryId;
            }

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Description = dto.Description;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepo.SaveChangesAsync();

            var item = MapToResponseDto(product);
            return ApiResponse<ProductResponseDto>.SuccessResponse(
                      item,
                      "Update data successfully"
                     );
        }

        public async Task<ApiResponse<ProductResponseDto>> DeleteAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found");

            product.IsDeleted = true;
            await _productRepo.SaveChangesAsync();

            var item = MapToResponseDto(product);

            return ApiResponse<ProductResponseDto>.SuccessResponse(
                    item,
                   "Delete data successfully"
                   );
        }
        private static ProductResponseDto MapToResponseDto(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category!.Name
        };
    }
}
