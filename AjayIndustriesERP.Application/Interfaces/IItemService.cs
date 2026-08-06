using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemService
    {
        Task<List<Item>> GetAllAsync();

        Task<Item?> GetByIdAsync(int itemId);

        Task CreateAsync(Item item);

        Task UpdateAsync(Item item);

        Task DeleteAsync(int itemId);

        Task<List<Item>> SearchAsync(string searchText);

        Task<PagedResult<Item>> GetPagedAsync(int pageNumber, int pageSize);
    }
}