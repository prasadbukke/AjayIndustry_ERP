/*
==============================================================

File : ItemRepository.cs

Purpose :
Handles Item Master database operations.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    /// <summary>
    /// Provides database operations for Item Master.
    /// </summary>
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<List<Item>> GetAllAsync()
        {
            return await ItemQuery()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ItemName)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int itemId)
        {
            return await ItemQuery()
                .FirstOrDefaultAsync(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted);
        }

        #region Search

        public async Task<List<Item>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await GetAllAsync();
            }

            var normalizedSearch =
                searchText
                    .Trim()
                    .ToLower();

            return await ItemQuery()
                .Where(x =>

                    // Item Code
                    x.ItemCode
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    // Item Name
                    x.ItemName
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    // Description
                    (
                        x.Description != null &&
                        x.Description
                            .ToLower()
                            .Contains(normalizedSearch)
                    )

                    ||

                    // Category
                    x.ItemCategory.CategoryCode
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    x.ItemCategory.CategoryName
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    // Brand
                    x.Brand.BrandCode
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    x.Brand.BrandName
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    // Main UOM
                    x.Uom.UomCode
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    x.Uom.UomName
                        .ToLower()
                        .Contains(normalizedSearch)

                    ||

                    // Shape
                    (
                        x.Shape != null &&
                        (
                            x.Shape.ShapeCode
                                .ToLower()
                                .Contains(normalizedSearch)

                            ||

                            x.Shape.ShapeName
                                .ToLower()
                                .Contains(normalizedSearch)
                        )
                    )

                    ||

                    // Item Specifications
                    x.ItemSpecifications.Any(s =>

                        !s.IsDeleted

                        &&

                        (
                            // Specification Code
                            s.Specification.SpecificationCode
                                .ToLower()
                                .Contains(normalizedSearch)

                            ||

                            // Specification Name
                            s.Specification.SpecificationName
                                .ToLower()
                                .Contains(normalizedSearch)

                            ||

                            // Specification Value
                            s.SpecificationValue
                                .ToLower()
                                .Contains(normalizedSearch)

                            ||

                            // Specification UOM
                            (
                                s.Uom != null &&
                                (
                                    s.Uom.UomCode
                                        .ToLower()
                                        .Contains(normalizedSearch)

                                    ||

                                    s.Uom.UomName
                                        .ToLower()
                                        .Contains(normalizedSearch)
                                )
                            )
                        )
                    )
                )
                .OrderBy(x => x.ItemName)
                .ThenBy(x => x.ItemCode)
                .ToListAsync();
        }

        #endregion

        public async Task<PagedResult<Item>> GetPagedAsync(
            int pageNumber,
            int pageSize)
        {
            var query = ItemQuery()
                .Where(x => !x.IsDeleted);

            var totalRecords =
                await query.CountAsync();

            var items = await query
                .OrderBy(x => x.ItemName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Item>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion

        #region Write Operations

        public async Task AddAsync(Item item)
        {
            await _context.Items.AddAsync(item);
        }

        public Task UpdateAsync(Item item)
        {
            _context.Items.Update(item);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Item item)
        {
            item.IsDeleted = true;

            _context.Items.Update(item);

            return Task.CompletedTask;
        }

        #endregion

        #region Duplicate Validation

        public async Task<bool> ExistsByCodeAsync(
            string itemCode)
        {
            var normalizedCode =
                itemCode.Trim().ToLower();

            /*
             * Deleted Items are included because
             * Item Codes must never be reused.
             */
            return await _context.Items
                .AnyAsync(x =>
                    x.ItemCode.ToLower() ==
                    normalizedCode);
        }

        public async Task<bool> ExistsByCodeAsync(
            string itemCode,
            int itemId)
        {
            var normalizedCode =
                itemCode.Trim().ToLower();

            return await _context.Items
                .AnyAsync(x =>
                    x.ItemCode.ToLower() ==
                    normalizedCode &&
                    x.ItemId != itemId);
        }

        public async Task<bool> ExistsByNameAsync(
            string itemName)
        {
            var normalizedName =
                itemName.Trim().ToLower();

            return await _context.Items
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ItemName.ToLower() ==
                    normalizedName);
        }

        public async Task<bool> ExistsByNameAsync(
            string itemName,
            int itemId)
        {
            var normalizedName =
                itemName.Trim().ToLower();

            return await _context.Items
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ItemName.ToLower() ==
                    normalizedName &&
                    x.ItemId != itemId);
        }

        #endregion

        #region Item Code Generation

        public async Task<string?> GetLastItemCodeAsync()
        {
            /*
             * Deleted records are intentionally included
             * to prevent Item Code reuse.
             */
            return await _context.Items
                .OrderByDescending(x => x.ItemId)
                .Select(x => x.ItemCode)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Save Changes

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Private Query

        private IQueryable<Item> ItemQuery()
        {
            return _context.Items

                .Include(x => x.ItemCategory)

                .Include(x => x.Brand)

                .Include(x => x.Uom)

                .Include(x => x.Shape)

                /*
                 * Load active Item Specification rows
                 * for Item list/details/search display.
                 */
                .Include(x =>
                    x.ItemSpecifications
                        .Where(s => !s.IsDeleted))
                    .ThenInclude(x =>
                        x.Specification)

                .Include(x =>
                    x.ItemSpecifications
                        .Where(s => !s.IsDeleted))
                    .ThenInclude(x =>
                        x.Uom)

                .Where(x =>
                    !x.IsDeleted);
        }

        #endregion

        #region Duplicate Configuration Lookup

        public async Task<List<Item>>
            GetByNameForDuplicateCheckAsync(
                string itemName,
                int? excludedItemId = null)
        {
            var normalizedName =
                itemName.Trim().ToLower();

            var query =
                _context.Items
                    .Include(x => x.Shape)
                    .Include(x => x.ItemSpecifications
                        .Where(s => !s.IsDeleted))
                        .ThenInclude(x => x.Specification)
                    .Include(x => x.ItemSpecifications
                        .Where(s => !s.IsDeleted))
                        .ThenInclude(x => x.Uom)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ItemName.ToLower() ==
                            normalizedName);

            if (excludedItemId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.ItemId !=
                        excludedItemId.Value);
            }

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion
    }
}