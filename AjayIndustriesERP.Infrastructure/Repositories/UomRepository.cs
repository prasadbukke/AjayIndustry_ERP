using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class UomRepository : IUomRepository
    {
        private readonly ApplicationDbContext _context;

        public UomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Uom>> GetAllAsync()
        {
            return await _context.Uoms
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.UomName)
                .ToListAsync();
        }

        public async Task<Uom?> GetByIdAsync(int uomId)
        {
            return await _context.Uoms
                .FirstOrDefaultAsync(x =>
                    x.UomId == uomId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Uom uom)
        {
            await _context.Uoms.AddAsync(uom);
        }

        public Task UpdateAsync(Uom uom)
        {
            _context.Uoms.Update(uom);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Uom uom)
        {
            uom.IsDeleted = true;

            _context.Uoms.Update(uom);

            return Task.CompletedTask;
        }
        public async Task<bool> ExistsByCodeAsync(string uomCode)
        {
            return await _context.Uoms.AnyAsync(x =>
                x.UomCode == uomCode &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCodeAsync(string uomCode, int uomId)
        {
            return await _context.Uoms.AnyAsync(x =>
                x.UomCode == uomCode &&
                x.UomId != uomId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string uomName)
        {
            return await _context.Uoms.AnyAsync(x =>
                x.UomName == uomName &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string uomName, int uomId)
        {
            return await _context.Uoms.AnyAsync(x =>
                x.UomName == uomName &&
                x.UomId != uomId &&
                !x.IsDeleted);
        }
        /// <summary>
        /// Searches UOM by Code or Name.
        /// </summary>
        public async Task<List<Uom>> SearchAsync(string searchText)
        {
            searchText = searchText.Trim().ToLower();

            return await _context.Uoms
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.UomCode.ToLower().Contains(searchText) ||
                        x.UomName.ToLower().Contains(searchText)
                    ))
                .OrderBy(x => x.UomName)
                .ToListAsync();
        }
        /// <summary>
        /// Returns paginated UOM list.
        /// </summary>
        public async Task<PagedResult<Uom>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Uoms
                .Where(x => !x.IsDeleted);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.UomName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Uom>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
        /// <summary>
        /// Returns last generated UOM code.
        /// </summary>
        public async Task<string?> GetLastUomCodeAsync()
        {
            return await _context.Uoms
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.UomId)
                .Select(x => x.UomCode)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}