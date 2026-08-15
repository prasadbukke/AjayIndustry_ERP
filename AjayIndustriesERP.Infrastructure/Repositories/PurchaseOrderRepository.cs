/*
==============================================================

File : PurchaseOrderRepository.cs

Purpose :
Handles Purchase Order database operations.

Important :
- Purchase Order Code duplicate checks include deleted records.
- Purchase Order Codes must never be reused.
- Purchase Orders use soft delete.
- Purchase Order Items are also soft deleted with the PO.
- Details load Supplier, Items, Item and Drawing information.

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
    /// Provides persistence operations for Purchase Orders.
    /// </summary>
    public class PurchaseOrderRepository :
        IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }


        #region Read Operations

        public async Task<List<PurchaseOrder>>
            GetAllAsync()
        {
            return await _context.PurchaseOrders
                .Where(x => !x.IsDeleted)
                .Include(x => x.Company)
                .Include(x => x.Supplier)
                .OrderByDescending(x => x.PODate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }


        public async Task<PurchaseOrder?> GetByIdAsync(
            int purchaseOrderId)
        {
            return await _context.PurchaseOrders
                .Include(x => x.Company)
                .Include(x => x.Supplier)

                .Include(x => x.Items
                    .Where(i => !i.IsDeleted))
                    .ThenInclude(x => x.Item)

                .Include(x => x.Items
                    .Where(i => !i.IsDeleted))
                    .ThenInclude(x => x.Drawing)

                .FirstOrDefaultAsync(x =>
                    x.Id == purchaseOrderId &&
                    !x.IsDeleted);
        }


        public async Task<List<PurchaseOrder>> SearchAsync(
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

            return await _context.PurchaseOrders
                .Where(x =>
                    !x.IsDeleted
                    &&
                    (
                        x.Code
                            .ToLower()
                            .Contains(search)

                        ||

                        x.SupplierName
                            .ToLower()
                            .Contains(search)

                        ||

                        (
                            x.SupplierGSTIN != null &&
                            x.SupplierGSTIN
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.SupplierContactPerson != null &&
                            x.SupplierContactPerson
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.SupplierPhone != null &&
                            x.SupplierPhone
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.Remarks != null &&
                            x.Remarks
                                .ToLower()
                                .Contains(search)
                        )
                    ))
                .Include(x => x.Company)
                .Include(x => x.Supplier)

                .OrderByDescending(x => x.PODate)
                .ThenByDescending(x => x.Id)

                .ToListAsync();
        }


        public async Task<PagedResult<PurchaseOrder>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context.PurchaseOrders
                    .Where(x => !x.IsDeleted);

            var totalRecords =
                await query.CountAsync();

            var records =
                await query
                    .Include(x => x.Company)
                    .Include(x => x.Supplier)

                    .OrderByDescending(x => x.PODate)
                    .ThenByDescending(x => x.Id)

                    .Skip(
                        (pageNumber - 1) *
                        pageSize)

                    .Take(pageSize)

                    .ToListAsync();

            return new PagedResult<PurchaseOrder>
            {
                Items = records,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion


        #region Write Operations

        public async Task AddAsync(
            PurchaseOrder purchaseOrder)
        {
            await _context.PurchaseOrders
                .AddAsync(purchaseOrder);
        }


        public Task UpdateAsync(
            PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Update(
                purchaseOrder);

            return Task.CompletedTask;
        }


        public async Task DeleteAsync(
            PurchaseOrder purchaseOrder)
        {
            /*
             * Purchase Order uses soft delete.
             * Child Purchase Order Items are also
             * soft deleted so transaction data remains
             * consistent.
             */

            purchaseOrder.IsDeleted = true;

            var purchaseOrderItems =
                await _context.PurchaseOrderItems
                    .Where(x =>
                        x.PurchaseOrderId ==
                        purchaseOrder.Id &&
                        !x.IsDeleted)
                    .ToListAsync();

            foreach (var item in purchaseOrderItems)
            {
                item.IsDeleted = true;

                _context.PurchaseOrderItems
                    .Update(item);
            }

            _context.PurchaseOrders.Update(
                purchaseOrder);
        }

        #endregion


        #region Purchase Order Code Validation

        public async Task<bool> ExistsByCodeAsync(
            string purchaseOrderCode)
        {
            var normalizedCode =
                purchaseOrderCode
                    .Trim()
                    .ToUpperInvariant();

            /*
             * Deleted Purchase Orders are deliberately
             * included.
             *
             * Purchase Order Codes must never be reused.
             */
            return await _context.PurchaseOrders
                .AnyAsync(x =>
                    x.Code
                        .ToUpper() ==
                    normalizedCode);
        }


        public async Task<bool> ExistsByCodeAsync(
            string purchaseOrderCode,
            int purchaseOrderId)
        {
            var normalizedCode =
                purchaseOrderCode
                    .Trim()
                    .ToUpperInvariant();

            return await _context.PurchaseOrders
                .AnyAsync(x =>
                    x.Code
                        .ToUpper() ==
                    normalizedCode &&
                    x.Id != purchaseOrderId);
        }

        #endregion


        #region Code Generation

        public async Task<string?>
            GetLastPurchaseOrderCodeAsync(
                string codePrefix)
        {
            /*
             * Deleted Purchase Orders are deliberately included.
             *
             * Numbering is Financial-Year specific.
             * Example prefix:
             * AI/PO/26-27/
             */
            return await _context.PurchaseOrders
                .Where(x =>
                    x.Code.StartsWith(codePrefix))
                .OrderByDescending(x =>
                    x.Code)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
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