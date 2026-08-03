using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.BrandName)
                .ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int BrandId)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(x =>
                    x.BrandId == BrandId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Brand Brand)
        {
            await _context.Brands.AddAsync(Brand);
        }

        public Task UpdateAsync(Brand Brand)
        {
            _context.Brands.Update(Brand);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Brand Brand)
        {
            Brand.IsDeleted = true;

            _context.Brands.Update(Brand);

            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByCodeAsync(string BrandCode)
        {
            return await _context.Brands.AnyAsync(x =>
                x.BrandCode == BrandCode &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCodeAsync(string BrandCode, int BrandId)
        {
            return await _context.Brands.AnyAsync(x =>
                x.BrandCode == BrandCode &&
                x.BrandId != BrandId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string BrandName)
        {
            return await _context.Brands.AnyAsync(x =>
                x.BrandName == BrandName &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string BrandName, int BrandId)
        {
            return await _context.Brands.AnyAsync(x =>
                x.BrandName == BrandName &&
                x.BrandId != BrandId &&
                !x.IsDeleted);
        }

        public async Task<List<Brand>> SearchAsync(string searchText)
        {
            searchText = searchText.Trim().ToLower();

            return await _context.Brands
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.BrandCode.ToLower().Contains(searchText) ||
                        x.BrandName.ToLower().Contains(searchText)
                    ))
                .OrderBy(x => x.BrandName)
                .ToListAsync();
        }

        public async Task<PagedResult<Brand>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Brands
                .Where(x => !x.IsDeleted);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.BrandName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Brand>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<string?> GetLastBrandCodeAsync()
        {
            return await _context.Brands
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.BrandId)
                .Select(x => x.BrandCode)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}