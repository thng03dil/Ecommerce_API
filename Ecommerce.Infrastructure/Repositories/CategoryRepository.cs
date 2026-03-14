using Ecommerce.Application.DTOs.Filters;
using Ecommerce.Application.Pagination;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {

        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<(IEnumerable<Category>, int totalCount)> GetAllAsync(PaginationDto pagedto)
        {
            var query = _context.Categories
                    .AsNoTracking()
                    .Include(c => c.Products)
                    .Where(x => !x.IsDeleted);

            var totalItem = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Id)
                .Skip((pagedto.PageNumber - 1) * pagedto.PageSize)
                .Take(pagedto.PageSize)
                .ToListAsync();
            return (items, totalItem);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                    .AsNoTracking()
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task CreateAsync(Category category)
        {

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category?> GetByIdForUpdateAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
        public async Task<bool> SlugExistsAsync(string slug, int excludeId)
        {
            return await _context.Categories
                .AnyAsync(x => x.Slug == slug && x.Id != excludeId);
        }
        public async Task UpdateAsync(Category category)
        {

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        //filter
        public async Task<(IEnumerable<Category>, int)> GetFilteredAsync(
            CategoryFilterDto filter, PaginationDto pagination)
        {
            var query = _context.Categories
                    .AsNoTracking()
                    .Include(c => c.Products)
                    .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(c => c.Name.Contains(filter.Keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.Slug))
            {
                query = query.Where(c => c.Slug.Contains(filter.Slug));
            }
            query = query.ApplySorting(
                filter.SortBy ?? "Id",
                filter.SortOrder ?? "asc");

            var total = await query.CountAsync();

            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
