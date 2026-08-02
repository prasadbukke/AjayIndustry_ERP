using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IWarehouseRepository
    {
        Task<List<Warehouse>> GetAllAsync();

        Task<Warehouse?> GetByIdAsync(int warehouseId);

        Task AddAsync(Warehouse warehouse);

        Task UpdateAsync(Warehouse warehouse);

        Task DeleteAsync(Warehouse warehouse);

        Task<bool> ExistsByCodeAsync(string warehouseCode);

        Task<bool> ExistsByCodeAsync(string warehouseCode, int warehouseId);

        Task<bool> ExistsByNameAsync(string warehouseName);

        Task<bool> ExistsByNameAsync(string warehouseName, int warehouseId);

        Task<List<Warehouse>> SearchAsync(string searchText);

        Task<PagedResult<Warehouse>> GetPagedAsync(int pageNumber, int pageSize);

        Task<string?> GetLastWarehouseCodeAsync();

        Task SaveChangesAsync();
    }
}