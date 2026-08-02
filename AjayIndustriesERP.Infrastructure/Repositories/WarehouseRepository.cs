using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly ApplicationDbContext _context;

        public WarehouseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.WarehouseName)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int warehouseId)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouseId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
        }

        public Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Warehouse warehouse)
        {
            warehouse.IsDeleted = true;

            _context.Warehouses.Update(warehouse);

            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByCodeAsync(string warehouseCode)
        {
            return await _context.Warehouses.AnyAsync(x =>
                x.WarehouseCode == warehouseCode &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCodeAsync(string warehouseCode, int warehouseId)
        {
            return await _context.Warehouses.AnyAsync(x =>
                x.WarehouseCode == warehouseCode &&
                x.WarehouseId != warehouseId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string warehouseName)
        {
            return await _context.Warehouses.AnyAsync(x =>
                x.WarehouseName == warehouseName &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string warehouseName, int warehouseId)
        {
            return await _context.Warehouses.AnyAsync(x =>
                x.WarehouseName == warehouseName &&
                x.WarehouseId != warehouseId &&
                !x.IsDeleted);
        }

        public async Task<List<Warehouse>> SearchAsync(string searchText)
        {
            searchText = searchText.Trim().ToLower();

            return await _context.Warehouses
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.WarehouseCode.ToLower().Contains(searchText) ||
                        x.WarehouseName.ToLower().Contains(searchText)
                    ))
                .OrderBy(x => x.WarehouseName)
                .ToListAsync();
        }

        public async Task<PagedResult<Warehouse>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Warehouses
                .Where(x => !x.IsDeleted);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.WarehouseName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Warehouse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<string?> GetLastWarehouseCodeAsync()
        {
            return await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.WarehouseId)
                .Select(x => x.WarehouseCode)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}