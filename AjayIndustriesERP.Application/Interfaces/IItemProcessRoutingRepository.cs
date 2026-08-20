/*
============================================================
File: IItemProcessRoutingRepository.cs

Purpose:
Defines database operations required by Item Process Routing.

Responsibilities:
- Retrieve Routing headers and steps.
- Search and paginate Routings.
- Load Item, Operation and Machine lookup data.
- Retrieve Draft / Released Routing information.
- Retrieve latest Item revision.
- Retrieve last Routing Code.
- Retrieve deleted Routings.
- Persist Routing changes.

Important:
- Business rules belong in ItemProcessRoutingService.
- Database access belongs only in Repository.
- Deleted Routing Codes and revisions are included when
  generating future Codes / Revision Numbers.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemProcessRoutingRepository
    {
        #region Read Operations

        Task<List<ItemProcessRouting>> GetAllAsync();

        Task<ItemProcessRouting?> GetByIdAsync(
            int id);

        Task<ItemProcessRouting?> GetForUpdateAsync(
            int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<ItemProcessRouting>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<PagedResult<ItemProcessRouting>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Master Lookups

        Task<List<Item>> GetItemsForRoutingAsync();

        Task<Item?> GetItemForRoutingAsync(
            int itemId);


        Task<List<ProductionOperation>>
            GetOperationsForRoutingAsync();

        Task<ProductionOperation?>
            GetOperationForRoutingAsync(
                int operationId);


        Task<List<Machine>>
            GetMachinesForRoutingAsync();

        Task<Machine?> GetMachineForRoutingAsync(
            int machineId);

        #endregion


        #region Routing State

        Task<bool> ActiveRoutingExistsForItemAsync(
            int itemId);

        Task<bool> DraftRoutingExistsForItemAsync(
            int itemId,
            int? excludeRoutingId = null);

        Task<ItemProcessRouting?>
            GetReleasedRoutingForItemForUpdateAsync(
                int itemId,
                int? excludeRoutingId = null);

        #endregion


        #region Revision And Code

        Task<int> GetLatestRevisionNumberAsync(
            int itemId);

        Task<string?> GetLastRoutingCodeAsync();

        #endregion


        #region Deleted Routings

        Task<List<ItemProcessRouting>>
            GetDeletedAsync();

        Task<ItemProcessRouting?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion


        #region Write Operations

        Task AddAsync(
            ItemProcessRouting routing);

        Task UpdateAsync(
            ItemProcessRouting routing);

        #endregion
    }
}