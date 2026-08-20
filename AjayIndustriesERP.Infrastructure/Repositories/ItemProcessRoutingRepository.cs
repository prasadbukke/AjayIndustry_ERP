/*
============================================================
File: ItemProcessRoutingRepository.cs

Purpose:
Provides Entity Framework Core data access for
Item Process Routing.

Responsibilities:
- Retrieve Routing headers with Item information.
- Retrieve Routing Steps with Operation and Machine.
- Search and paginate Routings.
- Provide Item / Operation / Machine lookup data.
- Find current Draft and Released Routings.
- Generate revision/code source information.
- Retrieve deleted Routings.
- Persist Routing changes.

Important:
- Main Index contains non-deleted Routings.
- Superseded Routings remain visible for history.
- Deleted Routings are displayed separately.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class ItemProcessRoutingRepository
        : IItemProcessRoutingRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public ItemProcessRoutingRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Read Operations

        public async Task<List<ItemProcessRouting>>
            GetAllAsync()
        {
            return await _context
                .ItemProcessRoutings
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted)
                .Include(x =>
                    x.Item)
                .OrderBy(x =>
                    x.Item.ItemName)
                .ThenByDescending(x =>
                    x.RevisionNumber)
                .ToListAsync();
        }


        public async Task<ItemProcessRouting?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .ItemProcessRoutings
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted))
                    .ThenInclude(x =>
                        x.ProductionOperation)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted))
                    .ThenInclude(x =>
                        x.DefaultMachine)
                .FirstOrDefaultAsync();
        }


        public async Task<ItemProcessRouting?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .ItemProcessRoutings
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.Steps)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<ItemProcessRouting>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .ItemProcessRoutings
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query.CountAsync();


            var routings =
                await query
                    .Include(x =>
                        x.Item)
                    .OrderBy(x =>
                        x.Item.ItemName)
                    .ThenByDescending(x =>
                        x.RevisionNumber)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<ItemProcessRouting>
            {
                Items = routings,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }


        public async Task<PagedResult<ItemProcessRouting>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize)
        {
            var search =
                searchText
                    .Trim()
                    .ToLower();


            var query =
                _context
                    .ItemProcessRoutings
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(search)

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
                                x.Remarks != null &&
                                x.Remarks
                                    .ToLower()
                                    .Contains(search)
                            )
                        ));


            var totalRecords =
                await query.CountAsync();


            var routings =
                await query
                    .Include(x =>
                        x.Item)
                    .OrderBy(x =>
                        x.Item.ItemName)
                    .ThenByDescending(x =>
                        x.RevisionNumber)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<ItemProcessRouting>
            {
                Items = routings,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion


        #region Master Lookups

        public async Task<List<Item>>
            GetItemsForRoutingAsync()
        {
            return await _context
                .Items
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.ItemName)
                .ThenBy(x =>
                    x.ItemCode)
                .ToListAsync();
        }


        public async Task<Item?>
            GetItemForRoutingAsync(
                int itemId)
        {
            return await _context
                .Items
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }


        public async Task<List<ProductionOperation>>
            GetOperationsForRoutingAsync()
        {
            return await _context
                .ProductionOperations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.OperationName)
                .ToListAsync();
        }


        public async Task<ProductionOperation?>
            GetOperationForRoutingAsync(
                int operationId)
        {
            return await _context
                .ProductionOperations
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == operationId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }


        public async Task<List<Machine>>
            GetMachinesForRoutingAsync()
        {
            return await _context
                .Machines
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.MachineName)
                .ThenBy(x =>
                    x.Code)
                .ToListAsync();
        }


        public async Task<Machine?>
            GetMachineForRoutingAsync(
                int machineId)
        {
            return await _context
                .Machines
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == machineId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        #region Routing State

        public async Task<bool>
            ActiveRoutingExistsForItemAsync(
                int itemId)
        {
            return await _context
                .ItemProcessRoutings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted);
        }


        public async Task<bool>
            DraftRoutingExistsForItemAsync(
                int itemId,
                int? excludeRoutingId = null)
        {
            return await _context
                .ItemProcessRoutings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted &&
                    x.Status ==
                        ItemProcessRoutingStatus.Draft &&
                    (
                        !excludeRoutingId.HasValue ||
                        x.Id != excludeRoutingId.Value
                    ));
        }


        public async Task<ItemProcessRouting?>
            GetReleasedRoutingForItemForUpdateAsync(
                int itemId,
                int? excludeRoutingId = null)
        {
            return await _context
                .ItemProcessRoutings
                .Where(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted &&
                    x.Status ==
                        ItemProcessRoutingStatus.Released &&
                    (
                        !excludeRoutingId.HasValue ||
                        x.Id != excludeRoutingId.Value
                    ))
                .OrderByDescending(x =>
                    x.RevisionNumber)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Revision And Code

        public async Task<int>
            GetLatestRevisionNumberAsync(
                int itemId)
        {
            var latestRevision =
                await _context
                    .ItemProcessRoutings

                    // Deleted revisions intentionally included.
                    // Revision numbers are never reused.

                    .Where(x =>
                        x.ItemId == itemId)
                    .Select(x =>
                        (int?)x.RevisionNumber)
                    .MaxAsync();


            return latestRevision ?? 0;
        }


        public async Task<string?>
            GetLastRoutingCodeAsync()
        {
            const string prefix =
                "AI/RTE/";


            return await _context
                .ItemProcessRoutings

                // Deleted records intentionally included.
                // Routing Codes are never reused.

                .Where(x =>
                    x.Code.StartsWith(prefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Deleted Routings

        public async Task<List<ItemProcessRouting>>
            GetDeletedAsync()
        {
            return await _context
                .ItemProcessRoutings
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .Include(x =>
                    x.Item)
                .OrderByDescending(x =>
                    x.ModifiedOn ?? x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<ItemProcessRouting?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .ItemProcessRoutings
                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)
                .Include(x =>
                    x.Steps)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Write Operations

        public async Task AddAsync(
            ItemProcessRouting routing)
        {
            await _context
                .ItemProcessRoutings
                .AddAsync(routing);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            ItemProcessRouting routing)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}