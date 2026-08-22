/*
============================================================
File: CustomerPurchaseOrderRepository.cs

Purpose:
Provides Entity Framework Core data access for the Customer
Purchase Order module.

Responsibilities:
- Retrieve Customer Purchase Orders.
- Retrieve complete Customer PO with Customer and Items.
- Retrieve tracked Customer PO for Edit.
- Provide Search + Pagination.
- Load active Customer Master records.
- Load active Item Master records.
- Detect duplicate Customer + Customer PO Number.
- Retrieve the last generated Customer PO Code.
- Persist Customer Purchase Order changes.

Important:
- Business logic belongs in CustomerPurchaseOrderService.
- Database access belongs only in Repository layer.
- Soft-deleted Customer POs are excluded from normal queries.
- Deleted Customer PO codes are included during code lookup so
  document numbers are never reused.
- Existing Customer Master and Item Master are reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class CustomerPurchaseOrderRepository
        : ICustomerPurchaseOrderRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public CustomerPurchaseOrderRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Read Operations

       

        public async Task<List<CustomerPurchaseOrder>>
            GetAllAsync()
        {
            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Items)
                .OrderByDescending(x =>
                    x.CustomerPurchaseOrderDate)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Customer)
                .Include(x =>
    x.Items)
    .ThenInclude(x =>
        x.Item)
    .ThenInclude(x =>
        x.Drawings
            .Where(drawing =>
                !drawing.IsDeleted &&
                drawing.IsActive))
                .FirstOrDefaultAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .CustomerPurchaseOrders
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Item)
                .FirstOrDefaultAsync();
        }




        #endregion


        #region Pagination

        public async Task<PagedResult<CustomerPurchaseOrder>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .CustomerPurchaseOrders
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query.CountAsync();


            var customerPurchaseOrders =
                await query
                    .Include(x =>
                        x.Customer)
                    .OrderByDescending(x =>
                        x.CustomerPurchaseOrderDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<CustomerPurchaseOrder>
            {
                Items =
                    customerPurchaseOrders,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<CustomerPurchaseOrder>>
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
                    .CustomerPurchaseOrders
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            x.CustomerName
                                .ToLower()
                                .Contains(search)

                            ||

                            x.CustomerPurchaseOrderNumber
                                .ToLower()
                                .Contains(search)

                            ||

                            (
                                x.CustomerReference != null &&
                                x.CustomerReference
                                    .ToLower()
                                    .Contains(search)
                            )
                        ));


            var totalRecords =
                await query.CountAsync();


            var customerPurchaseOrders =
                await query
                    .Include(x =>
                        x.Customer)
                    .OrderByDescending(x =>
                        x.CustomerPurchaseOrderDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<CustomerPurchaseOrder>
            {
                Items =
                    customerPurchaseOrders,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Customer Master Loading

        public async Task<List<Customer>>
            GetCustomersForOrderAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.CustomerName)
                .ThenBy(x =>
                    x.Code)
                .ToListAsync();
        }


        public async Task<Customer?>
            GetCustomerForOrderAsync(
                int customerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == customerId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        #region Item Master Loading

        public async Task<List<Item>>
            GetItemsForOrderAsync()
        {
            return await _context.Items
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Uom)
                .OrderBy(x =>
                    x.ItemName)
                .ThenBy(x =>
                    x.ItemCode)
                .ToListAsync();
        }


        public async Task<Item?>
    GetItemForOrderAsync(
        int itemId)
        {
            return await _context.Items
                .AsNoTracking()
                .Where(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted &&
                    x.IsActive)

                .Include(x =>
                    x.Uom)

                .Include(x =>
                    x.ItemSpecifications)
                    .ThenInclude(x =>
                        x.Specification)

                .Include(x =>
                    x.ItemSpecifications)
                    .ThenInclude(x =>
                        x.Uom)

                .Include(x =>
                    x.Drawings
                        .Where(drawing =>
                            !drawing.IsDeleted &&
                            drawing.IsActive))

                .FirstOrDefaultAsync();
        }

        #endregion


        #region Duplicate Validation

        public async Task<bool>
            CustomerPurchaseOrderNumberExistsAsync(
                int customerId,
                string customerPurchaseOrderNumber,
                int? excludeCustomerPurchaseOrderId = null)
        {
            var normalizedNumber =
                customerPurchaseOrderNumber
                    .Trim()
                    .ToUpper();


            return await _context.CustomerPurchaseOrders
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CustomerId == customerId &&

                    x.CustomerPurchaseOrderNumber
                        .ToUpper() ==
                        normalizedNumber &&

                    (
                        !excludeCustomerPurchaseOrderId.HasValue ||
                        x.Id !=
                            excludeCustomerPurchaseOrderId.Value
                    ));
        }

        #endregion


        #region Customer PO Code Lookup

        public async Task<string?>
            GetLastCustomerPurchaseOrderCodeAsync(
                string codePrefix)
        {
            return await _context.CustomerPurchaseOrders

                // =================================================
                // IsDeleted intentionally NOT filtered.
                //
                // Customer PO document numbers must never be reused.
                // =================================================

                .Where(x =>
                    x.Code.StartsWith(
                        codePrefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Write Operations

        public async Task AddAsync(
            CustomerPurchaseOrder customerPurchaseOrder)
        {
            await _context.CustomerPurchaseOrders
                .AddAsync(
                    customerPurchaseOrder);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            CustomerPurchaseOrder customerPurchaseOrder)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion

        #region Restore Support

        public async Task<CustomerPurchaseOrder?>
            GetAnyByIdForUpdateAsync(
                int id)
        {
            return await _context.CustomerPurchaseOrders
                .Where(x =>
                    x.Id == id)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Item)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Deleted Customer Purchase Orders

        public async Task<List<CustomerPurchaseOrder>>
            GetDeletedAsync()
        {
            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .Include(x =>
                    x.Customer)
                .OrderByDescending(x =>
                    x.ModifiedOn ?? x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .CustomerPurchaseOrders
                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}