using Ecommerce_API.DTOs.ProductDtos;
using Ecommerce_API.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_API.Validators.ProductValidators
{
    public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
    {
        private readonly AppDbContext _context;
        public ProductCreateValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{PropertyName} cannot be empty")
                .MaximumLength(150).WithMessage("{PropertyName} must not exceed 300 characters");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("{PropertyName} cannot be empty")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(x => x.Stock)
                .NotEmpty().WithMessage("{PropertyName} cannot be empty")
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than 0 or equal to 0.");

            RuleFor(x => x.CategoryId)
                 .NotEmpty().WithMessage("{PropertyName} cannot be empty")
                 .GreaterThan(0).WithMessage("{PropertyName} must be a valid ID")
                 .MustAsync(CategoryExists)
                 .WithMessage("The selected Category does not exist.");

            RuleFor(x => x.Description)
               .MaximumLength(1000).WithMessage("{PropertyName} must not exceed 1000 characters");
        }
        private async Task<bool> CategoryExists(int categoryId, CancellationToken token)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId && !c.IsDeleted, token);
        }
    }
}
