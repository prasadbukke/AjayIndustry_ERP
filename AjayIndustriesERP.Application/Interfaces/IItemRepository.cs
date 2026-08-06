using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemRepository
    {
        Task<List<Item>> GetAllAsync();

        Task<Item?> GetByIdAsync(int itemId);

        Task AddAsync(Item item);

        Task UpdateAsync(Item item);

        Task DeleteAsync(Item item);

        Task<bool> ExistsByCodeAsync(string itemCode);

        Task<bool> ExistsByCodeAsync(string itemCode, int itemId);

        Task<bool> ExistsByNameAsync(string itemName);

        Task<bool> ExistsByNameAsync(string itemName, int itemId);

        Task<List<Item>> SearchAsync(string searchText);

        Task<PagedResult<Item>> GetPagedAsync(int pageNumber, int pageSize);

        Task<string?> GetLastItemCodeAsync();

        Task SaveChangesAsync();
    }
}