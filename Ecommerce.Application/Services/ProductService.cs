using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.DTOs.ProductDTOs;

namespace Ecommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IAppDbContext _context;
        public ProductService(IAppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.Name,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl= p.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            return new ProductResponseDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = product.Category!.Name,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl
            };
        }

        public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto)
        {
            
            // Validate Category tồn tại
            var category = await _context.Categories
        .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

            if (category == null)
                throw new Exception("Category không tồn tại");

            var product = new Product
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();


            return new ProductResponseDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl
            };
        }

        public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
                return false;

            product.CategoryId = dto.CategoryId;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.ImageUrl = dto.ImageUrl;
            product.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            // Soft delete
            product.IsActive = false;
            //_context.Products.Remove(product);hard delete
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
