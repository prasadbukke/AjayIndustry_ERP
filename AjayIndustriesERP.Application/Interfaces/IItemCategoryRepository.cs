using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemCategoryRepository
    {
        Task<List<ItemCategory>> GetAllAsync();

        Task<ItemCategory?> GetByIdAsync(int itemCategoryId);

        Task AddAsync(ItemCategory itemCategory);

        Task UpdateAsync(ItemCategory itemCategory);

        Task DeleteAsync(ItemCategory itemCategory);

        Task<bool> ExistsByCodeAsync(string categoryCode);

        Task<bool> ExistsByCodeAsync(string categoryCode, int itemCategoryId);

        Task<bool> ExistsByNameAsync(string categoryName);

        Task<bool> ExistsByNameAsync(string categoryName, int itemCategoryId);

        Task<List<ItemCategory>> SearchAsync(string searchText);

        Task<PagedResult<ItemCategory>> GetPagedAsync(int pageNumber, int pageSize);

        Task<string?> GetLastCategoryCodeAsync();

        Task SaveChangesAsync();
    }
}