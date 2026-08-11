/*
==============================================================

File : DrawingRepository.cs

Purpose :
Handles Drawing persistence and revision history.

Important :
- Normal list/search returns only current revisions.
- Historical revisions are retrieved separately.
- Duplicate checks include deleted records.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class DrawingRepository :
        IDrawingRepository
    {
        private readonly ApplicationDbContext
            _context;

        public DrawingRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #region Current Query

        private IQueryable<Drawing>
            CurrentDrawingQuery()
        {
            return _context.Drawings
                .Include(x => x.Item)
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion

        #region Read

        public async Task<List<Drawing>>
            GetAllAsync()
        {
            return await CurrentDrawingQuery()
                .AsNoTracking()
                .OrderBy(x =>
                    x.DrawingNumber)
                .ToListAsync();
        }

        public async Task<Drawing?> GetByIdAsync(
            int drawingId)
        {
            /*
             * GetById intentionally does not require
             * IsActive because Details may refer to
             * a historical revision.
             */
            return await _context.Drawings
                .Include(x => x.Item)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.DrawingId ==
                        drawingId);
        }

        public async Task<List<Drawing>>
            GetByItemIdAsync(
                int itemId)
        {
            return await CurrentDrawingQuery()
                .AsNoTracking()
                .Where(x =>
                    x.ItemId == itemId)
                
                .OrderBy(x =>
                    x.DrawingNumber)
                .ToListAsync();
        }

        public async Task<List<Drawing>>
            SearchAsync(
                string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await GetAllAsync();
            }

            var search =
                searchText
                    .Trim()
                    .ToLower();

            return await CurrentDrawingQuery()
                .AsNoTracking()
                .Where(x =>

                    x.DrawingNumber
                        .ToLower()
                        .Contains(search)

                    ||

                    (
                        x.DrawingName != null &&
                        x.DrawingName
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    x.RevisionNumber!
                        .ToLower()
                        .Contains(search)

                    ||

                    (
                        x.DrawingType != null &&
                        x.DrawingType
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    (
                        x.FileName != null &&
                        x.FileName
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    (
                        x.Description != null &&
                        x.Description
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    x.Item.ItemCode
                        .ToLower()
                        .Contains(search)

                    ||

                    x.Item.ItemName
                        .ToLower()
                        .Contains(search)

                    ||

                    (
                        x.Item.PartNumber != null &&
                        x.Item.PartNumber
                            .ToLower()
                            .Contains(search)
                    )
                )
                .OrderBy(x =>
                    x.DrawingNumber)
                .ToListAsync();
        }

        public async Task<PagedResult<Drawing>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                CurrentDrawingQuery()
                    .AsNoTracking();

            var totalRecords =
                await query.CountAsync();

            var items =
                await query
                    .OrderBy(x =>
                        x.DrawingNumber)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return new PagedResult<Drawing>
            {
                Items = items,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion

        #region Revision History

        public async Task<List<Drawing>>
            GetRevisionHistoryAsync(
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);

            return await _context.Drawings
                .Include(x => x.Item)
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.DrawingNumber
                        .ToUpper() ==
                    normalizedNumber)
                .OrderByDescending(x =>
                    x.IsActive)
                .ThenByDescending(x =>
                    x.DrawingId)
                .ToListAsync();
        }

        public async Task<List<Drawing>>
            GetByDrawingNumberForUpdateAsync(
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);

            /*
             * Tracking query used by Service when
             * switching current revision.
             */
            return await _context.Drawings
                .Where(x =>
                    !x.IsDeleted &&
                    x.DrawingNumber
                        .ToUpper() ==
                    normalizedNumber)
                .OrderBy(x =>
                    x.DrawingId)
                .ToListAsync();
        }

        public async Task<List<string>>
    GetRevisionNumbersIncludingDeletedAsync(
        string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);

            /*
             * IsDeleted is intentionally NOT filtered.
             *
             * Deleted Revision Numbers remain reserved
             * and must never be generated again.
             */
            return await _context.Drawings
                .Where(x =>
                    x.DrawingNumber
                        .ToUpper() ==
                    normalizedNumber)
                .Where(x =>
                    x.RevisionNumber != null &&
                    x.RevisionNumber != "")
                .Select(x =>
                    x.RevisionNumber!)
                .ToListAsync();
        }

        #endregion

        

        #region Duplicate Checks

        public async Task<bool>
            ExistsByDrawingNumberAsync(
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);

            /*
             * IsDeleted intentionally not checked.
             * A Drawing Number is never reused.
             */
            return await _context.Drawings
                .AnyAsync(x =>
                    x.DrawingNumber
                        .ToUpper() ==
                    normalizedNumber);
        }

        public async Task<bool>
            ExistsByRevisionAsync(
                string drawingNumber,
                string revisionNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);

            var normalizedRevision =
                NormalizeRevision(
                    revisionNumber);

            /*
             * Deleted revision numbers are also
             * permanently reserved.
             */
            return await _context.Drawings
                .AnyAsync(x =>
                    x.DrawingNumber
                        .ToUpper() ==
                    normalizedNumber
                    &&
                    x.RevisionNumber!
                        .ToUpper() ==
                    normalizedRevision);
        }

        #endregion

        #region Write

        public async Task AddAsync(
            Drawing drawing)
        {
            await _context.Drawings
                .AddAsync(drawing);
        }

        public async Task AddRangeAsync(
            IEnumerable<Drawing> drawings)
        {
            await _context.Drawings
                .AddRangeAsync(drawings);
        }

        public Task UpdateAsync(
            Drawing drawing)
        {
            _context.Drawings.Update(
                drawing);

            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(
            IEnumerable<Drawing> drawings)
        {
            _context.Drawings.UpdateRange(
                drawings);

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Helpers

        private static string
            NormalizeDrawingNumber(
                string value)
        {
            return value
                .Trim()
                .ToUpperInvariant();
        }

        private static string
            NormalizeRevision(
                string value)
        {
            return value
                .Trim()
                .ToUpperInvariant();
        }

        #endregion

        #region Transaction

        public async Task ExecuteInTransactionAsync(
            Func<Task> action)
        {
            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                await action();

                await transaction
                    .CommitAsync();
            }
            catch
            {
                await transaction
                    .RollbackAsync();

                throw;
            }
        }

        #endregion

        #region Deleted Drawings

        public async Task<List<Drawing>>
            GetDeletedDrawingsAsync()
        {
            var deletedRows =
                await _context.Drawings
                    .Include(x => x.Item)
                    .AsNoTracking()
                    .Where(x =>
                        x.IsDeleted)
                    .OrderByDescending(x =>
                        x.DrawingId)
                    .ToListAsync();

            /*
             * One Drawing Number has many revision rows.
             * Deleted list should show only one row
             * per Drawing identity.
             */
            return deletedRows
                .GroupBy(
                    x => x.DrawingNumber,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                    group
                        .OrderByDescending(x =>
                            x.DrawingId)
                        .First())
                .OrderBy(x =>
                    x.DrawingNumber)
                .ToList();
        }

        public async Task<List<Drawing>>
            GetDeletedHistoryForUpdateAsync(
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);

            return await _context.Drawings
                .Where(x =>
                    x.IsDeleted &&
                    x.DrawingNumber
                        .ToUpper() ==
                    normalizedNumber)
                .OrderBy(x =>
                    x.DrawingId)
                .ToListAsync();
        }

        #endregion
    }
}