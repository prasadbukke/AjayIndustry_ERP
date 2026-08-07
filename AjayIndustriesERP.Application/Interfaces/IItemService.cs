/*
==============================================================

File : IItemService.cs

Purpose :
Defines business operations for Item Master.

Features :
- Item CRUD
- Search and pagination
- Item Specification child rows
- Soft delete

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines business operations for Item Master.
    /// </summary>
    public interface IItemService
    {
        #region Read Operations

        Task<List<Item>> GetAllAsync();

        Task<Item?> GetByIdAsync(
            int itemId);

        Task<List<Item>> SearchAsync(
            string searchText);

        Task<PagedResult<Item>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Returns active/non-deleted Specification rows
        /// assigned to the Item.
        /// </summary>
        Task<List<ItemSpecification>> GetSpecificationsAsync(
            int itemId);

        #endregion

        #region Write Operations

        Task CreateAsync(Item item);

        Task UpdateAsync(Item item);

        Task DeleteAsync(int itemId);

        #endregion
    }
}