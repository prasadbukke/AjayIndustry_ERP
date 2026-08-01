using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetAllAsync();

        Task<Warehouse?> GetByIdAsync(int warehouseId);

        Task CreateAsync(Warehouse warehouse);

        Task UpdateAsync(Warehouse warehouse);

        Task DeleteAsync(int warehouseId);

        Task<List<Warehouse>> SearchAsync(string searchText);

        Task<PagedResult<Warehouse>> GetPagedAsync(int pageNumber, int pageSize);
    }
}