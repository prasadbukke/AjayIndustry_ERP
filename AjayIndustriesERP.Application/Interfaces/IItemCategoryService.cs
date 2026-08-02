using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemCategoryService
    {
        Task<List<ItemCategory>> GetAllAsync();

        Task<ItemCategory?> GetByIdAsync(int itemCategoryId);

        Task CreateAsync(ItemCategory itemCategory);

        Task UpdateAsync(ItemCategory itemCategory);

        Task DeleteAsync(int itemCategoryId);

        Task<List<ItemCategory>> SearchAsync(string searchText);

        Task<PagedResult<ItemCategory>> GetPagedAsync(int pageNumber, int pageSize);
    }
}