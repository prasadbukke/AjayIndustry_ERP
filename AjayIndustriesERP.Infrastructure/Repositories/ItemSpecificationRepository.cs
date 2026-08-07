/*
==============================================================

File : ItemSpecificationRepository.cs

Purpose :
Handles Item Specification database operations.

Features :
- Loads Specification and UOM navigation data.
- Adds and updates Specification rows.
- Supports soft delete.
- Prevents duplicate active Specifications for an Item.

==============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    /// <summary>
    /// Provides persistence operations for Item Specifications.
    /// </summary>
    public class ItemSpecificationRepository :
        IItemSpecificationRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemSpecificationRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<List<ItemSpecification>>
            GetByItemIdAsync(int itemId)
        {
            return await _context.ItemSpecifications
                .Include(x => x.Specification)
                .Include(x => x.Uom)
                .Where(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x =>
                    x.Specification.SpecificationName)
                .ToListAsync();
        }

        public async Task<ItemSpecification?> GetByIdAsync(
            int itemSpecificationId)
        {
            return await _context.ItemSpecifications
                .Include(x => x.Specification)
                .Include(x => x.Uom)
                .FirstOrDefaultAsync(x =>
                    x.ItemSpecificationId ==
                        itemSpecificationId &&
                    !x.IsDeleted);
        }

        #endregion

        #region Write Operations

        public async Task AddAsync(
            ItemSpecification itemSpecification)
        {
            await _context.ItemSpecifications
                .AddAsync(itemSpecification);
        }

        public async Task AddRangeAsync(
            IEnumerable<ItemSpecification> itemSpecifications)
        {
            var records =
                itemSpecifications.ToList();

            if (records.Count == 0)
            {
                return;
            }

            await _context.ItemSpecifications
                .AddRangeAsync(records);
        }

        public Task UpdateAsync(
            ItemSpecification itemSpecification)
        {
            _context.ItemSpecifications.Update(
                itemSpecification);

            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(
            ItemSpecification itemSpecification)
        {
            itemSpecification.IsDeleted = true;

            _context.ItemSpecifications.Update(
                itemSpecification);

            return Task.CompletedTask;
        }

        public Task SoftDeleteRangeAsync(
            IEnumerable<ItemSpecification> itemSpecifications)
        {
            var records =
                itemSpecifications.ToList();

            if (records.Count == 0)
            {
                return Task.CompletedTask;
            }

            foreach (var record in records)
            {
                record.IsDeleted = true;
            }

            _context.ItemSpecifications.UpdateRange(
                records);

            return Task.CompletedTask;
        }

        #endregion

        #region Duplicate Validation

        public async Task<bool> ExistsAsync(
            int itemId,
            int specificationId)
        {
            return await _context.ItemSpecifications
                .AnyAsync(x =>
                    x.ItemId == itemId &&
                    x.SpecificationId ==
                        specificationId &&
                    !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(
            int itemId,
            int specificationId,
            int itemSpecificationId)
        {
            return await _context.ItemSpecifications
                .AnyAsync(x =>
                    x.ItemId == itemId &&
                    x.SpecificationId ==
                        specificationId &&
                    x.ItemSpecificationId !=
                        itemSpecificationId &&
                    !x.IsDeleted);
        }

        #endregion

        #region Save Changes

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}