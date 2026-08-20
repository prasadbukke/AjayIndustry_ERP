/*
============================================================
File: IItemProcessRoutingService.cs

Purpose:
Defines Item Process Routing business operations.

Responsibilities:
- Retrieve and search Routings.
- Load Item / Operation / Machine lookup data.
- Create first Draft Routing.
- Edit Draft Routing.
- Release Routing for Production use.
- Create a new Draft revision from a Released Routing.
- Soft-delete Draft Routing.
- Restore deleted Draft Routing.

Important:
- Only Draft Routing is editable.
- Only Released Routing can later create Production Jobs.
- Releasing a newer revision supersedes the older Released
  revision.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemProcessRoutingService
    {
        #region Read Operations

        Task<ItemProcessRouting?> GetByIdAsync(
            int id);

        Task<PagedResult<ItemProcessRouting>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Master Lookups

        Task<List<Item>>
            GetItemsForRoutingAsync();

        Task<List<ProductionOperation>>
            GetOperationsForRoutingAsync();

        Task<List<Machine>>
            GetMachinesForRoutingAsync();

        #endregion


        #region Routing Workflow

        Task<ItemProcessRouting> CreateAsync(
            ItemProcessRouting routing);

        Task<ItemProcessRouting> UpdateAsync(
            ItemProcessRouting routing);

        Task ReleaseAsync(
            int id);

        Task<ItemProcessRouting> CreateRevisionAsync(
            int releasedRoutingId);

        #endregion


        #region Delete And Restore

        Task DeleteAsync(
            int id);

        Task<List<ItemProcessRouting>>
            GetDeletedAsync();

        Task RestoreAsync(
            int id);

        #endregion
    }
}