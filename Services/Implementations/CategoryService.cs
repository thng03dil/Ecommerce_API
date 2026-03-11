using Ecommerce_API.Data;
using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.DTOs.Common;
using Ecommerce_API.Models;
using Ecommerce_API.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ecommerce_API.Helpers.Pagination;
using Ecommerce_API.Exceptions;
using Ecommerce_API.Helpers.Responses;
using Ecommerce_API.Repositories.Interfaces;
using Ecommerce_API.Helpers.Extensions;
using Ecommerce_API.DTOs.Filters;

namespace Ecommerce_API.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IValidator<CategoryCreateDto> _createValidator;
        private readonly IValidator<CategoryUpdateDto> _updateValidator;
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(
            ICategoryRepo categoryRepo,
            IValidator<CategoryCreateDto> createValidator,
            IValidator<CategoryUpdateDto> updateValidator)
        {
            _categoryRepo = categoryRepo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PagedResponse<CategoryResponseDto>> GetAllAsync(CategoryFilterDto filter, PaginationDto pagedto)
        {
            //  var (items, totalItems) = await _categoryRepo.GetAllAsync( pagedto);
            var (items,totalItems) = await _categoryRepo.GetFilteredAsync(filter, pagedto);
            var data = items.Select(c=>MapToResponseDto(c)).ToList();

            return new PagedResponse<CategoryResponseDto>(data, pagedto.PageNumber, pagedto.PageSize, totalItems);
        }

        public async Task<ApiResponse<CategoryResponseDto?>> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);

            if (category == null)
                throw new NotFoundException("Category not found");
           
            var item = MapToResponseDto(category);
            return new ApiResponse<CategoryResponseDto?>(
                    true,
                    "Get data successfully",
                    item
                    );
        }

        public async Task<ApiResponse<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();
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
            return new ApiResponse<CategoryResponseDto>(
                    true,
                    "Create data successfully",
                    item
                    );
        }

        public async Task<ApiResponse<CategoryResponseDto>> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            dto.Id = id;

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var category = await _categoryRepo.GetByIdForUpdateAsync(dto.Id);
            if (category == null)
                throw new NotFoundException("Category not found");

            var slugExists = await _categoryRepo.SlugExistsAsync(dto.Slug,dto.Id);

            if (slugExists)
                throw new Ecommerce_API.Exceptions.ValidationException("slug", "Slug already exists");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Slug = dto.Slug;
            category.UpdatedAt = DateTime.UtcNow;

            var item = MapToResponseDto(category);
            return new ApiResponse<CategoryResponseDto>(
                   true,
                   "Update data successfully",
                   item
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
            return new ApiResponse<CategoryResponseDto>(
                true,
                "Delete data successfully",
                item
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
