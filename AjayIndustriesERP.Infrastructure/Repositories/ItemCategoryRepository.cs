using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class ItemCategoryRepository : IItemCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ItemCategory>> GetAllAsync()
        {
            return await _context.ItemCategories
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CategoryName)
                .ToListAsync();
        }

        public async Task<ItemCategory?> GetByIdAsync(int ItemCategoryId)
        {
            return await _context.ItemCategories
                .FirstOrDefaultAsync(x =>
                    x.ItemCategoryId == ItemCategoryId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(ItemCategory ItemCategory)
        {
            await _context.ItemCategories.AddAsync(ItemCategory);
        }

        public Task UpdateAsync(ItemCategory ItemCategory)
        {
            _context.ItemCategories.Update(ItemCategory);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(ItemCategory ItemCategory)
        {
            ItemCategory.IsDeleted = true;

            _context.ItemCategories.Update(ItemCategory);

            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByCodeAsync(string CategoryCode)
        {
            return await _context.ItemCategories.AnyAsync(x =>
                x.CategoryCode == CategoryCode &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCodeAsync(string CategoryCode, int ItemCategoryId)
        {
            return await _context.ItemCategories.AnyAsync(x =>
                x.CategoryCode == CategoryCode &&
                x.ItemCategoryId != ItemCategoryId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string CategoryName)
        {
            return await _context.ItemCategories.AnyAsync(x =>
                x.CategoryName == CategoryName &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string CategoryName, int ItemCategoryId)
        {
            return await _context.ItemCategories.AnyAsync(x =>
                x.CategoryName == CategoryName &&
                x.ItemCategoryId != ItemCategoryId &&
                !x.IsDeleted);
        }

        public async Task<List<ItemCategory>> SearchAsync(string searchText)
        {
            searchText = searchText.Trim().ToLower();

            return await _context.ItemCategories
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.CategoryCode.ToLower().Contains(searchText) ||
                        x.CategoryName.ToLower().Contains(searchText)
                    ))
                .OrderBy(x => x.CategoryName)
                .ToListAsync();
        }

        public async Task<PagedResult<ItemCategory>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.ItemCategories
                .Where(x => !x.IsDeleted);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ItemCategory>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<string?> GetLastCategoryCodeAsync()
        {
            // Deleted recordsसुद्धा consider करायचे.
            // जुना Category Code पुन्हा वापरला जाणार नाही.
            return await _context.ItemCategories
                .OrderByDescending(x => x.ItemCategoryId)
                .Select(x => x.CategoryCode)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}