/*
==============================================================

File : IDrawingRepository.cs

Purpose :
Defines persistence operations for Drawing and
Drawing Revision History.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IDrawingRepository
    {
        #region Current Drawing Queries

        Task<List<Drawing>> GetAllAsync();

        Task<Drawing?> GetByIdAsync(
            int drawingId);

        Task<List<Drawing>> GetByItemIdAsync(
            int itemId);

        Task<List<Drawing>> SearchAsync(
            string searchText);

        Task<PagedResult<Drawing>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        #endregion

        #region Revision History

        Task<List<Drawing>>
            GetRevisionHistoryAsync(
                string drawingNumber);

        Task<List<Drawing>>
            GetByDrawingNumberForUpdateAsync(
                string drawingNumber);

        /// <summary>
        /// Returns all Revision Numbers for a Drawing Number.
        ///
        /// Deleted revisions are intentionally included
        /// so Revision Numbers are never reused.
        /// </summary>
        Task<List<string>>
            GetRevisionNumbersIncludingDeletedAsync(
                string drawingNumber);
        #region Deleted Drawings

        Task<List<Drawing>>
            GetDeletedDrawingsAsync();

        Task<List<Drawing>>
            GetDeletedHistoryForUpdateAsync(
                string drawingNumber);

        #endregion

        #endregion

        

        #region Duplicate Checks

        /// <summary>
        /// Checks Drawing Number across all records,
        /// including inactive and deleted revisions.
        /// </summary>
        Task<bool> ExistsByDrawingNumberAsync(
            string drawingNumber);

        /// <summary>
        /// Checks whether the exact revision was ever used
        /// for the Drawing Number.
        /// Deleted records are included.
        /// </summary>
        Task<bool> ExistsByRevisionAsync(
            string drawingNumber,
            string revisionNumber);

        #endregion

        #region Write

        Task AddAsync(
            Drawing drawing);

        Task AddRangeAsync(
            IEnumerable<Drawing> drawings);

        Task UpdateAsync(
            Drawing drawing);

        Task UpdateRangeAsync(
            IEnumerable<Drawing> drawings);

        Task SaveChangesAsync();

        #endregion

        #region Transaction

        /// <summary>
        /// Executes Drawing state changes inside
        /// a database transaction.
        /// </summary>
        Task ExecuteInTransactionAsync(
            Func<Task> action);

        #endregion
    }
}