using Ecommerce_API.DTOs.CategoryDtos;
using Ecommerce_API.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_API.Validators.CategoryValidator
{
    public class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
    {
        private readonly AppDbContext _context;
        public CategoryCreateValidator(AppDbContext context)
        {
            _context = context;
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{PropertyName} cannot be empty")
                .MinimumLength(3).WithMessage("{PropertyName} must be at least 3 characters")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 300 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("{PropertyName} cannot be empty ")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters  ")
                .MustAsync(BeUniqueSlug)
                .WithMessage("{PropertyName} already existed ");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters  ");
        }
        private async Task<bool> BeUniqueSlug(string slug, CancellationToken cancellationToken)
        {
            
            var isExisted = await _context.Categories
                .AnyAsync(c => c.Slug == slug, cancellationToken);

            return !isExisted; 
        }
    }
}
