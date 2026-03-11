using Ecommerce_API.Data;
using Ecommerce_API.DTOs.CategoryDtos;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace Ecommerce_API.Validators.CategoryValidators
{
    public class CategoryUpdateValidator : AbstractValidator<CategoryUpdateDto>
    {
        private readonly AppDbContext _context;

        public CategoryUpdateValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MinimumLength(3).WithMessage("{PropertyName} must be at least 3 characters.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
                .MustAsync(BeUniqueSlug).WithMessage("The slug already exists in another category.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");
        }
        private async Task<bool> BeUniqueSlug(CategoryUpdateDto dto, string slug, CancellationToken cancellationToken)
        {
            var isExisted = await _context.Categories
                .AnyAsync(c => c.Slug == slug && c.Id != dto.Id, cancellationToken);

            return !isExisted; 
    }
    }
}
