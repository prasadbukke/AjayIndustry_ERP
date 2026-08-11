/*
==============================================================

File : IDrawingService.cs

Purpose :
Defines Drawing business operations including
revision-history management.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IDrawingService
    {
        #region Read

        Task<List<Drawing>> GetAllAsync();

        Task<Drawing?> GetByIdAsync(
            int drawingId);

        Task<List<Drawing>> GetByItemIdAsync(
            int itemId);

        Task<List<Drawing>>
            GetRevisionHistoryAsync(
                int drawingId);

        Task<List<Drawing>> SearchAsync(
            string searchText);

        Task<PagedResult<Drawing>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);

        #endregion

        #region Write

        /// <summary>
        /// Creates a completely new Drawing Number
        /// with its first revision.
        /// </summary>
        Task CreateAsync(
            Drawing drawing);

        /// <summary>
        /// Updates common Drawing information and
        /// optionally adds new revisions.
        ///
        /// DrawingNumber itself never changes.
        /// </summary>
        Task UpdateAsync(
            Drawing drawing,
            IReadOnlyCollection<Drawing>
                newRevisions);

        /// <summary>
        /// Soft deletes the complete Drawing Number
        /// including its revision history.
        /// </summary>
        Task DeleteAsync(
            int drawingId);

        /// <summary>
        /// Makes an existing historical revision
        /// the Current revision.
        ///
        /// Existing Current revision is automatically
        /// made inactive.
        /// </summary>
        Task ActivateRevisionAsync(
            int drawingId);

        /// <summary>
        /// Soft deletes an inactive Drawing revision.
        ///
        /// Current revision cannot be deleted.
        /// </summary>
        Task DeleteRevisionAsync(
            int drawingId);

        #endregion

        #region Restore

        Task<List<Drawing>>
            GetDeletedDrawingsAsync();

        Task RestoreAsync(
            int drawingId);

        #endregion
    }
}