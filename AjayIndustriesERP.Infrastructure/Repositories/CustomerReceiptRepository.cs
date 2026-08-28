/*
============================================================
File: CustomerReceiptRepository.cs

Module:
Customer Receipt

Purpose:
Provides Entity Framework Core data access for
Customer Receipt module.

Responsibilities:
- Retrieve Customer Receipts.
- Search and paginate active Receipts.
- Search and paginate deleted Receipts.
- Load active Customers.
- Load Finalized Customer Invoices.
- Calculate Finalized Receipt allocations.
- Load active Company.
- Retrieve last Receipt code.
- Persist Customer Receipt changes.

Important:
- Business logic belongs in CustomerReceiptService.
- Repository performs data access only.
- Normal Receipt queries exclude soft-deleted Receipts.
- Only Finalized Receipt allocations affect Invoice
  outstanding calculation.
- Deleted Receipt codes are included during code lookup
  so Receipt numbers are never reused.
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
    public class CustomerReceiptRepository
        : ICustomerReceiptRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public CustomerReceiptRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Receipt Read

        public async Task<CustomerReceipt?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .CustomerReceipts
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Allocations
                        .Where(allocation =>
                            !allocation.IsDeleted &&
                            allocation.IsActive))
                    .ThenInclude(x =>
                        x.Invoice)
                .FirstOrDefaultAsync();
        }


        /*
         * IsDeleted is intentionally NOT filtered here.
         *
         * This tracked query is also used by Restore.
         * Service layer decides whether the Receipt must
         * currently be active or deleted.
         */
        public async Task<CustomerReceipt?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .CustomerReceipts
                .Where(x =>
                    x.Id == id)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Allocations)
                    .ThenInclude(x =>
                        x.Invoice)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Active Receipt Pagination

        public async Task<PagedResult<CustomerReceipt>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .CustomerReceipts
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query.CountAsync();


            var receipts =
                await query
                    .Include(x =>
                        x.Customer)
                    .OrderByDescending(x =>
                        x.ReceiptDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<CustomerReceipt>
            {
                Items =
                    receipts,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }


        public async Task<PagedResult<CustomerReceipt>>
            SearchPagedAsync(
                string? searchTerm,
                int pageNumber,
                int pageSize)
        {
            var search =
                (
                    searchTerm
                    ?? string.Empty
                )
                .Trim()
                .ToLower();


            var query =
                _context
                    .CustomerReceipts
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            if (!string.IsNullOrWhiteSpace(
                search))
            {
                query =
                    query.Where(x =>

                        x.Code
                            .ToLower()
                            .Contains(search)

                        ||

                        x.CustomerName
                            .ToLower()
                            .Contains(search)

                        ||

                        (
                            x.CustomerCode != null &&
                            x.CustomerCode
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.ReferenceNumber != null &&
                            x.ReferenceNumber
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.ChequeNumber != null &&
                            x.ChequeNumber
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.BankName != null &&
                            x.BankName
                                .ToLower()
                                .Contains(search)
                        )
                    );
            }


            var totalRecords =
                await query.CountAsync();


            var receipts =
                await query
                    .Include(x =>
                        x.Customer)
                    .OrderByDescending(x =>
                        x.ReceiptDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<CustomerReceipt>
            {
                Items =
                    receipts,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Deleted Receipt Pagination

        public async Task<PagedResult<CustomerReceipt>>
            GetDeletedPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .CustomerReceipts
                    .AsNoTracking()
                    .Where(x =>
                        x.IsDeleted);


            var totalRecords =
                await query.CountAsync();


            var receipts =
                await query
                    .Include(x =>
                        x.Customer)
                    .OrderByDescending(x =>
                        x.ModifiedOn ??
                        x.CreatedOn)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<CustomerReceipt>
            {
                Items =
                    receipts,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }


        public async Task<PagedResult<CustomerReceipt>>
            SearchDeletedPagedAsync(
                string? searchTerm,
                int pageNumber,
                int pageSize)
        {
            var search =
                (
                    searchTerm
                    ?? string.Empty
                )
                .Trim()
                .ToLower();


            var query =
                _context
                    .CustomerReceipts
                    .AsNoTracking()
                    .Where(x =>
                        x.IsDeleted);


            if (!string.IsNullOrWhiteSpace(
                search))
            {
                query =
                    query.Where(x =>

                        x.Code
                            .ToLower()
                            .Contains(search)

                        ||

                        x.CustomerName
                            .ToLower()
                            .Contains(search)

                        ||

                        (
                            x.CustomerCode != null &&
                            x.CustomerCode
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.ReferenceNumber != null &&
                            x.ReferenceNumber
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.ChequeNumber != null &&
                            x.ChequeNumber
                                .ToLower()
                                .Contains(search)
                        )
                    );
            }


            var totalRecords =
                await query.CountAsync();


            var receipts =
                await query
                    .Include(x =>
                        x.Customer)
                    .OrderByDescending(x =>
                        x.ModifiedOn ??
                        x.CreatedOn)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<CustomerReceipt>
            {
                Items =
                    receipts,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Customer Master

        public async Task<List<Customer>>
            GetCustomersForReceiptAsync()
        {
            return await _context
                .Customers
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
            GetCustomerForReceiptAsync(
                int customerId)
        {
            return await _context
                .Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == customerId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        #region Invoice Source

        public async Task<List<Invoice>>
            GetFinalizedInvoicesForReceiptAsync(
                int customerId)
        {
            return await _context
                .Invoices
                .AsNoTracking()
                .Where(x =>
                    x.CustomerId ==
                        customerId &&

                    !x.IsDeleted &&

                    x.IsActive &&

                    x.Status ==
                        InvoiceStatus.Finalized)
                .OrderBy(x =>
                    x.InvoiceDate)
                .ThenBy(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<Invoice?>
            GetFinalizedInvoiceForReceiptAsync(
                int invoiceId)
        {
            return await _context
                .Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == invoiceId &&

                    !x.IsDeleted &&

                    x.IsActive &&

                    x.Status ==
                        InvoiceStatus.Finalized);
        }

        #endregion


        #region Finalized Invoice Allocation

        public async Task<decimal>
            GetFinalizedAllocatedAmountAsync(
                int invoiceId,
                int? excludeCustomerReceiptId = null)
        {
            var allocatedAmount =
                await _context
                    .CustomerReceiptAllocations
                    .AsNoTracking()
                    .Where(x =>

                        x.InvoiceId ==
                            invoiceId &&

                        !x.IsDeleted &&

                        x.IsActive &&

                        x.CustomerReceipt != null &&

                        !x.CustomerReceipt.IsDeleted &&

                        x.CustomerReceipt.IsActive &&

                        x.CustomerReceipt.Status ==
                            CustomerReceiptStatus.Finalized &&

                        (
                            !excludeCustomerReceiptId.HasValue

                            ||

                            x.CustomerReceiptId !=
                                excludeCustomerReceiptId.Value
                        ))
                    .Select(x =>
                        (decimal?)
                            x.AllocatedAmount)
                    .SumAsync();


            return allocatedAmount
                ?? 0m;
        }

        #endregion


        #region Company

        public async Task<Company?>
            GetCompanyForReceiptAsync()
        {
            return await _context
                .Companies
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.CompanyId)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Receipt Code

        public async Task<string?>
            GetLastCodeAsync(
                string codePrefix)
        {
            return await _context
                .CustomerReceipts

                /*
                 * IsDeleted intentionally NOT filtered.
                 *
                 * Receipt document numbers must never
                 * be reused after soft delete.
                 */
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


        #region Write

        public async Task AddAsync(
            CustomerReceipt customerReceipt)
        {
            await _context
                .CustomerReceipts
                .AddAsync(
                    customerReceipt);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            CustomerReceipt customerReceipt)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}