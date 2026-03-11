using Ecommerce_API.Data;
using Ecommerce_API.DTOs.ProductDtos;
using Ecommerce_API.DTOs.Common;
using Ecommerce_API.Helpers.Pagination;
using Ecommerce_API.Models;
using Ecommerce_API.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ecommerce_API.Exceptions;
using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.Helpers.Responses;
using Ecommerce_API.Repositories.Interfaces;
using Ecommerce_API.Helpers.Extensions;
using Ecommerce_API.DTOs.Filters;

namespace Ecommerce_API.Services.Implementations
{
    public class ProductService : IProductService
    {

        private readonly IProductRepo _productRepo;
        private readonly IValidator<ProductCreateDto> _createValidator;
        private readonly IValidator<ProductUpdateDto> _updateValidator;
        public ProductService(
            IProductRepo productRepo,
            IValidator<ProductCreateDto> createValidator,
            IValidator<ProductUpdateDto> updateValidator)
        {
            _productRepo = productRepo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PagedResponse<ProductResponseDto>> GetAllAsync(ProductFilterDto filter, PaginationDto pagedto)
        {
            //  var (products, totalItems) = await _productRepo.GetAllAsync( pagedto);
            var (products, totalItems) = await _productRepo.GetFilteredAsync(filter, pagedto);

            var data = products.Select(c => MapToResponseDto(c)).ToList();
            return new PagedResponse<ProductResponseDto>(data, pagedto.PageNumber, pagedto.PageSize, totalItems);
        }

        public async Task<ApiResponse<ProductResponseDto?>> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found");

            var item = MapToResponseDto(product);
            return new ApiResponse<ProductResponseDto?>(
                    true,
                    "Get data successfully",
                    item
                    );
        }

        public async Task<ApiResponse<ProductResponseDto>> CreateAsync(ProductCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();
            // Validate Category exist
             var categoryExist = await _productRepo.CategoryExistsAsync(dto.CategoryId);

            if (!categoryExist ) throw new NotFoundException("Category not found");

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
            return new ApiResponse<ProductResponseDto>(
                    true,
                    "Create data successfully",
                    item
                    );
        }

        public async Task<ApiResponse<ProductResponseDto>> UpdateAsync(int id, ProductUpdateDto dto)
        {
            dto.Id = id;

            var result = await _updateValidator.ValidateAsync(dto);
            result.ThrowIfInvalid();

            var product = await _productRepo.GetByIdAsync(dto.Id);
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
            return new ApiResponse<ProductResponseDto>(
                    true,
                    "Get data successfully",
                    item
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

            return new ApiResponse<ProductResponseDto>(
                   true,
                   "Delete data successfully",
                   item
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