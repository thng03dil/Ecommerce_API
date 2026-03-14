using Ecommerce_API.Data;
using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.Pagination;
using Ecommerce_API.Models;
using Ecommerce_API.Services.Interfaces;
using FluentValidation;
using Ecommerce_API.Exceptions;
using Ecommerce_API.Helpers.Responses;
using Ecommerce_API.Repositories.Interfaces;
using Ecommerce_API.Helpers.Extensions;
using Ecommerce_API.DTOs.Filters;
using Ecommerce_API.DTOs.ProductDtos;

namespace Ecommerce_API.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<ApiResponse<PagedResponse<CategoryResponseDto>>> GetAllAsync(CategoryFilterDto filter, PaginationDto pagination)
        {
            //  var (items, totalItems) = await _categoryRepo.GetAllAsync( pagination);
            var (items,totalItems) = await _categoryRepo.GetFilteredAsync(filter, pagination);

            var data = items.Select(c=>MapToResponseDto(c)).ToList();

            var pagedData = new PagedResponse<CategoryResponseDto>(data, pagination.PageNumber, pagination.PageSize, totalItems);
            return ApiResponse<PagedResponse<CategoryResponseDto>>.SuccessResponse(pagedData, "Get data successfully");
        }
    

        public async Task<ApiResponse<CategoryResponseDto?>> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException("Category not found");
           
            var item = MapToResponseDto(category);
            return ApiResponse<CategoryResponseDto?>.SuccessResponse(
                     item,
                     "Create data successfully"
                    );
        }

        public async Task<ApiResponse<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto)
        {
          
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = dto.Slug,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            await _categoryRepo.CreateAsync(category);

            var item = MapToResponseDto(category);
            return  ApiResponse<CategoryResponseDto>.SuccessResponse(
                   item,
                    "Create data successfully"
                    );
        }

        public async Task<ApiResponse<CategoryResponseDto>> UpdateAsync(int id, CategoryUpdateDto dto)
        {
     
            var category = await _categoryRepo.GetByIdForUpdateAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");
          
            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Slug = dto.Slug;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepo.UpdateAsync(category);

            var item = MapToResponseDto(category);
            return ApiResponse<CategoryResponseDto>.SuccessResponse(
                   item,
                   "Update data successfully"
                   );
        }

        public async Task<ApiResponse<CategoryResponseDto>> DeleteAsync(int id)
        {
            var category = await _categoryRepo.GetByIdForUpdateAsync(id);

            if (category == null)
                throw new NotFoundException("Category not found");

            category.IsDeleted = true;

            await _categoryRepo.SaveChangesAsync();

            var item = MapToResponseDto(category);
            return ApiResponse<CategoryResponseDto>.SuccessResponse(
                     item,
                    "Delete data successfully"
            );
        }
        private static CategoryResponseDto MapToResponseDto(Category c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            ProductCount = c.Products?.Count() ?? 0,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
