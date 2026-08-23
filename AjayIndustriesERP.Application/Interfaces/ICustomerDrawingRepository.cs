/*
==============================================================

File : ICustomerDrawingRepository.cs

Purpose :
Defines Customer Drawing persistence operations.

Final Design :
- Customer Drawing follows Drawing Master revision workflow.
- Every row represents one revision.
- One Customer + One Item = One Customer Drawing Number.
- Drawing Number is permanent within that Customer.
- Revision Numbers are system generated.
- Revision Numbers are never reused.
- Only one revision can be Current.
- Historical revisions are preserved.
- Complete Customer Drawing can be soft deleted/restored.

Important :
- CustomerId is part of Customer Drawing identity.
- Same Drawing Number may exist for different Customers.
- Same Item may have different Customer Drawings
  for different Customers.
- Normal list/search returns only Current revisions.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerDrawingRepository
    {
        #region Current Drawing Read

        Task<List<CustomerDrawing>>
            GetAllAsync();


        Task<CustomerDrawing?>
            GetByIdAsync(
                int customerDrawingId);


        Task<CustomerDrawing?>
            GetByCustomerAndItemAsync(
                int customerId,
                int itemId);


        Task<List<CustomerDrawing>>
            SearchAsync(
                string searchText);


        Task<PagedResult<CustomerDrawing>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);

        #endregion


        #region Revision History

        /*
         * Read-only active revision history for one
         * Customer Drawing identity.
         *
         * CustomerId is required because another Customer
         * may use the same Drawing Number.
         */
        Task<List<CustomerDrawing>>
            GetRevisionHistoryAsync(
                int customerId,
                string drawingNumber);

        Task<CustomerDrawing?> GetRevisionAsync(
    int customerId,
    string drawingNumber,
    string revisionNumber);


        /*
         * Tracking query used by Service while:
         * - updating Drawing-level information
         * - adding revisions
         * - activating revision
         * - deleting revision
         * - deleting complete Customer Drawing
         */
        Task<List<CustomerDrawing>>
            GetByDrawingNumberForUpdateAsync(
                int customerId,
                string drawingNumber);


        /*
         * Includes soft-deleted revisions.
         *
         * Revision Numbers must never be reused.
         */
        Task<List<string>>
            GetRevisionNumbersIncludingDeletedAsync(
                int customerId,
                string drawingNumber);

        #endregion


        #region Duplicate Checks

        /*
         * Customer Drawing Number is permanently reserved
         * within one Customer.
         *
         * Deleted records are intentionally included.
         */
        Task<bool>
            ExistsByDrawingNumberAsync(
                int customerId,
                string drawingNumber);


        /*
         * Revision Number is permanently reserved
         * within one Customer Drawing.
         *
         * Deleted revisions are intentionally included.
         */
        Task<bool>
            ExistsByRevisionAsync(
                int customerId,
                string drawingNumber,
                string revisionNumber);

        #endregion


        #region Write

        Task AddAsync(
            CustomerDrawing customerDrawing);


        Task AddRangeAsync(
            IEnumerable<CustomerDrawing>
                customerDrawings);


        Task UpdateAsync(
            CustomerDrawing customerDrawing);


        Task UpdateRangeAsync(
            IEnumerable<CustomerDrawing>
                customerDrawings);


        Task SaveChangesAsync();

        #endregion


        #region Transaction

        /*
         * Required while switching Current revision.
         *
         * Current revision must first be deactivated
         * and then selected historical revision activated.
         */
        Task ExecuteInTransactionAsync(
            Func<Task> action);

        #endregion


        #region Deleted Customer Drawings

        /*
         * Returns one representative row per deleted
         * Customer Drawing identity.
         */
        Task<List<CustomerDrawing>>
            GetDeletedDrawingsAsync();


        /*
         * Returns complete deleted revision history
         * as tracking entities for Restore.
         */
        Task<List<CustomerDrawing>>
            GetDeletedHistoryForUpdateAsync(
                int customerId,
                string drawingNumber);

        #endregion
    }
}