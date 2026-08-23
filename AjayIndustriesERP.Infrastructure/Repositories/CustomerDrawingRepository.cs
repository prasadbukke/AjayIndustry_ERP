/*
==============================================================

File : CustomerDrawingRepository.cs

Purpose :
Handles Customer Drawing persistence and revision history.

Responsibilities :
- Return only Current Customer Drawing revisions
  for normal list/search/pagination.
- Retrieve complete revision history separately.
- Scope Drawing identity by Customer.
- Preserve deleted Revision Numbers.
- Support revision activation transaction.
- Support complete Customer Drawing delete/restore workflow.

Final Design :
- Every CustomerDrawing row represents one revision.
- CustomerId + DrawingNumber identifies one
  Customer Drawing.
- CustomerId + ItemId identifies the current
  Customer Drawing assigned to that Item.
- Only one revision is Current / Active.
- Historical revisions are preserved.
- Deleted Revision Numbers are never reused.
- Same Drawing Number may exist for different Customers.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class CustomerDrawingRepository :
        ICustomerDrawingRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public CustomerDrawingRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Current Query

        private IQueryable<CustomerDrawing>
            CurrentCustomerDrawingQuery()
        {
            return _context.CustomerDrawings

                .Include(x =>
                    x.Customer)

                .Include(x =>
                    x.Item)

                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        #region Current Drawing Read

        public async Task<List<CustomerDrawing>>
            GetAllAsync()
        {
            return await CurrentCustomerDrawingQuery()

                .AsNoTracking()

                .OrderBy(x =>
                    x.Customer.CustomerName)

                .ThenBy(x =>
                    x.DrawingNumber)

                .ToListAsync();
        }


        public async Task<CustomerDrawing?>
            GetByIdAsync(
                int customerDrawingId)
        {
            /*
             * IsActive is intentionally NOT required.
             *
             * Details/Edit/Activate/Delete Revision
             * may refer to a historical revision.
             */
            return await _context.CustomerDrawings

                .Include(x =>
                    x.Customer)

                .Include(x =>
                    x.Item)

                .AsNoTracking()

                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.CustomerDrawingId ==
                        customerDrawingId);
        }


        public async Task<CustomerDrawing?>
            GetByCustomerAndItemAsync(
                int customerId,
                int itemId)
        {
            /*
             * Only Current revision is returned.
             *
             * Historical revisions for the same
             * Customer + Item are intentionally hidden.
             */
            return await CurrentCustomerDrawingQuery()

                .AsNoTracking()

                .FirstOrDefaultAsync(x =>
                    x.CustomerId ==
                        customerId
                    &&
                    x.ItemId ==
                        itemId);
        }


        public async Task<List<CustomerDrawing>>
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


            return await CurrentCustomerDrawingQuery()

                .AsNoTracking()

                .Where(x =>

                    /*
                     * Customer
                     */
                    x.Customer.CustomerName
                        .ToLower()
                        .Contains(search)

                    ||

                    /*
                     * Drawing Number
                     */
                    x.DrawingNumber
                        .ToLower()
                        .Contains(search)

                    ||

                    /*
                     * Drawing Name
                     */
                    (
                        x.DrawingName != null &&
                        x.DrawingName
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    /*
                     * Revision
                     */
                    (
                        x.RevisionNumber != null &&
                        x.RevisionNumber
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    /*
                     * Drawing Type
                     */
                    (
                        x.DrawingType != null &&
                        x.DrawingType
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    /*
                     * File Name
                     */
                    (
                        x.FileName != null &&
                        x.FileName
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    /*
                     * Description
                     */
                    (
                        x.Description != null &&
                        x.Description
                            .ToLower()
                            .Contains(search)
                    )

                    ||

                    /*
                     * Item Code
                     */
                    x.Item.ItemCode
                        .ToLower()
                        .Contains(search)

                    ||

                    /*
                     * Item Name
                     */
                    x.Item.ItemName
                        .ToLower()
                        .Contains(search)

                    ||

                    /*
                     * Part Number
                     */
                    (
                        x.Item.PartNumber != null &&
                        x.Item.PartNumber
                            .ToLower()
                            .Contains(search)
                    )
                )

                .OrderBy(x =>
                    x.Customer.CustomerName)

                .ThenBy(x =>
                    x.DrawingNumber)

                .ToListAsync();
        }


        public async Task<PagedResult<CustomerDrawing>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                CurrentCustomerDrawingQuery()
                    .AsNoTracking();


            var totalRecords =
                await query.CountAsync();


            var items =
                await query

                    .OrderBy(x =>
                        x.Customer.CustomerName)

                    .ThenBy(x =>
                        x.DrawingNumber)

                    .Skip(
                        (pageNumber - 1) *
                        pageSize)

                    .Take(
                        pageSize)

                    .ToListAsync();


            return new PagedResult<CustomerDrawing>
            {
                Items =
                    items,

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

        public async Task<List<CustomerDrawing>>
            GetRevisionHistoryAsync(
                int customerId,
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);


            return await _context.CustomerDrawings

                .Include(x =>
                    x.Customer)

                .Include(x =>
                    x.Item)

                .AsNoTracking()

                .Where(x =>
                    !x.IsDeleted
                    &&
                    x.CustomerId ==
                        customerId
                    &&
                    x.DrawingNumber
                        .ToUpper() ==
                        normalizedNumber)

                /*
                 * Current revision first,
                 * then newest historical revision.
                 */
                .OrderByDescending(x =>
                    x.IsActive)

                .ThenByDescending(x =>
                    x.CustomerDrawingId)

                .ToListAsync();
        }


        public async Task<List<CustomerDrawing>>
            GetByDrawingNumberForUpdateAsync(
                int customerId,
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);


            /*
             * Tracking query.
             *
             * Used while:
             * - updating Drawing-level information
             * - adding revisions
             * - activating revision
             * - deleting revision
             * - deleting complete Drawing
             */
            return await _context.CustomerDrawings

                .Where(x =>
                    !x.IsDeleted
                    &&
                    x.CustomerId ==
                        customerId
                    &&
                    x.DrawingNumber
                        .ToUpper() ==
                        normalizedNumber)

                .OrderBy(x =>
                    x.CustomerDrawingId)

                .ToListAsync();
        }


        public async Task<List<string>>
            GetRevisionNumbersIncludingDeletedAsync(
                int customerId,
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);


            /*
             * IsDeleted is intentionally NOT filtered.
             *
             * Once RV-01 / RV-02 / etc. has existed,
             * that Revision Number remains permanently
             * reserved for this Customer Drawing.
             */
            return await _context.CustomerDrawings

                .Where(x =>
                    x.CustomerId ==
                        customerId
                    &&
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
                int customerId,
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);


            /*
             * IsDeleted is intentionally NOT checked.
             *
             * Drawing Number is permanently reserved
             * inside the selected Customer.
             *
             * Customer A / DWG-100
             * Customer B / DWG-100
             *
             * are allowed.
             */
            return await _context.CustomerDrawings

                .AnyAsync(x =>
                    x.CustomerId ==
                        customerId
                    &&
                    x.DrawingNumber
                        .ToUpper() ==
                        normalizedNumber);
        }


        public async Task<bool>
            ExistsByRevisionAsync(
                int customerId,
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
             * Deleted Revision Numbers are also
             * permanently reserved.
             */
            return await _context.CustomerDrawings

                .AnyAsync(x =>
                    x.CustomerId ==
                        customerId
                    &&
                    x.DrawingNumber
                        .ToUpper() ==
                        normalizedNumber
                    &&
                    x.RevisionNumber != null
                    &&
                    x.RevisionNumber
                        .ToUpper() ==
                        normalizedRevision);
        }

        #endregion


        #region Write

        public async Task AddAsync(
            CustomerDrawing customerDrawing)
        {
            await _context.CustomerDrawings
                .AddAsync(
                    customerDrawing);
        }


        public async Task AddRangeAsync(
            IEnumerable<CustomerDrawing>
                customerDrawings)
        {
            await _context.CustomerDrawings
                .AddRangeAsync(
                    customerDrawings);
        }


        public Task UpdateAsync(
            CustomerDrawing customerDrawing)
        {
            _context.CustomerDrawings
                .Update(
                    customerDrawing);


            return Task.CompletedTask;
        }


        public Task UpdateRangeAsync(
            IEnumerable<CustomerDrawing>
                customerDrawings)
        {
            _context.CustomerDrawings
                .UpdateRange(
                    customerDrawings);


            return Task.CompletedTask;
        }


        public async Task SaveChangesAsync()
        {
            await _context
                .SaveChangesAsync();
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


        #region Deleted Customer Drawings

        public async Task<List<CustomerDrawing>>
            GetDeletedDrawingsAsync()
        {
            var deletedRows =
                await _context.CustomerDrawings

                    .Include(x =>
                        x.Customer)

                    .Include(x =>
                        x.Item)

                    .AsNoTracking()

                    .Where(x =>
                        x.IsDeleted)

                    .OrderByDescending(x =>
                        x.CustomerDrawingId)

                    .ToListAsync();


            /*
             * One Customer Drawing contains many
             * revision rows.
             *
             * Deleted page should show only one row
             * per:
             *
             * Customer + Drawing Number
             *
             * Customer is part of identity because
             * another Customer may use the same
             * Drawing Number.
             */
            return deletedRows

                .GroupBy(x =>
                    new
                    {
                        x.CustomerId,

                        DrawingNumber =
                            NormalizeDrawingNumber(
                                x.DrawingNumber)
                    })

                .Select(group =>
                    group

                        /*
                         * Prefer the revision that was Current
                         * before complete Drawing deletion.
                         */
                        .OrderByDescending(x =>
                            x.IsActive)

                        .ThenByDescending(x =>
                            x.CustomerDrawingId)

                        .First())

                .OrderBy(x =>
                    x.Customer.CustomerName)

                .ThenBy(x =>
                    x.DrawingNumber)

                .ToList();
        }


        public async Task<List<CustomerDrawing>>
            GetDeletedHistoryForUpdateAsync(
                int customerId,
                string drawingNumber)
        {
            var normalizedNumber =
                NormalizeDrawingNumber(
                    drawingNumber);


            /*
             * Tracking entities are required because
             * Restore updates the complete revision history.
             */
            return await _context.CustomerDrawings

                .Where(x =>
                    x.IsDeleted
                    &&
                    x.CustomerId ==
                        customerId
                    &&
                    x.DrawingNumber
                        .ToUpper() ==
                        normalizedNumber)

                .OrderBy(x =>
                    x.CustomerDrawingId)

                .ToListAsync();
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
    }
}