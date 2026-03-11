using Ecommerce_API.Data;
using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.Helpers;
using Ecommerce_API.Models;
using Ecommerce_API.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_API.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CategoryCreateDto> _createValidator;
        private readonly IValidator<CategoryUpdateDto> _updateValidator;

        public CategoryService(
            AppDbContext context,
            IValidator<CategoryCreateDto> createValidator,
            IValidator<CategoryUpdateDto> updateValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync(Pagination pagination)
        {
            var query = _context.Categories
                .AsNoTracking()
                .Where(x => !x.IsDeleted);


            return await query
                .OrderBy(c => c.Id)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ProductCount = c.Products.Count()
                })
                .ToListAsync();
        }

        public async Task<CategoryResponseDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (category == null)
                throw new Exception("Category not found");

            return MapToResponseDto(category);
        }

        public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = dto.Slug
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return MapToResponseDto(category);
        }

        public async Task UpdateAsync(int id, CategoryUpdateDto dto)
        {
            dto.Id = id;

            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (category == null)
                throw new Exception("Category not found");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Slug = dto.Slug;
            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories
        .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
                throw new KeyNotFoundException("Category not found");

            category.IsDeleted = true;

            await _context.SaveChangesAsync();
        }
        private static CategoryResponseDto MapToResponseDto(Category c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            ProductCount = c.Products?.Count() ?? 0
        };
    }
}
