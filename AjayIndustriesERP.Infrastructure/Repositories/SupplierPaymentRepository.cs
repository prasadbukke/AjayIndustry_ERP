// =============================================================
// File: SupplierPaymentRepository.cs
// Module: Supplier Payment
// Layer: Infrastructure - Repository
//
// Purpose:
// Handles database operations for Supplier Payment headers
// and their multiple payment transactions.
//
// Final Structure:
//
// PurchaseInvoice
//      ↓ 1 : 1
// SupplierPayment
//      ↓ 1 : Many
// SupplierPaymentTransaction
//
// Important Business Rules:
// - One Purchase Invoice can have only one SupplierPayment.
// - Multiple payment transactions remain under same Payment No.
// - Soft-deleted SupplierPayment still reserves its
//   PurchaseInvoiceId and must be restored.
// - Paid Amount is calculated only from active,
//   non-deleted transactions.
// - Outstanding is not stored.
// - Only Finalized Purchase Invoices are eligible.
// =============================================================

using System.Globalization;
using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class SupplierPaymentRepository
        : ISupplierPaymentRepository
    {
        private readonly ApplicationDbContext _context;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        #region Constructor

        public SupplierPaymentRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        // =====================================================
        // BASIC READ
        // =====================================================

        #region Basic Read

        public async Task<SupplierPayment?> GetByIdAsync(
            int id)
        {
            return await _context.SupplierPayments

                .AsNoTracking()

                .Include(x =>
                    x.PurchaseInvoice)

                .Include(x =>
                    x.Supplier)

                .Include(x =>
                    x.Company)

                .Include(x =>
                    x.Transactions
                        .Where(t =>
                            t.IsActive &&
                            !t.IsDeleted))

                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDeleted);
        }


        public async Task<SupplierPayment?>
            GetByPurchaseInvoiceIdAsync(
                int purchaseInvoiceId)
        {
            return await _context.SupplierPayments

                .AsNoTracking()

                .Include(x =>
                    x.PurchaseInvoice)

                .Include(x =>
                    x.Supplier)

                .Include(x =>
                    x.Company)

                .Include(x =>
                    x.Transactions
                        .Where(t =>
                            t.IsActive &&
                            !t.IsDeleted))

                .FirstOrDefaultAsync(x =>
                    x.PurchaseInvoiceId ==
                        purchaseInvoiceId &&
                    x.IsActive &&
                    !x.IsDeleted);
        }

        #endregion


        // =====================================================
        // INDEX / PAGINATION
        // =====================================================

        #region Index / Pagination

        public async Task<PagedResult<SupplierPayment>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            pageNumber =
                pageNumber < 1
                    ? 1
                    : pageNumber;


            pageSize =
                pageSize < 1
                    ? 10
                    : pageSize;


            var query =
                BuildActiveIndexQuery();


            var totalRecords =
                await query.CountAsync();


            var items =
                await query

                    .OrderByDescending(x =>
                        x.Transactions
                            .Where(t =>
                                t.IsActive &&
                                !t.IsDeleted)
                            .Select(t =>
                                (DateTime?)t.PaymentDate)
                            .Max()
                        ?? DateTime.MinValue)

                    .ThenByDescending(x =>
                        x.Id)

                    .Skip(
                        (pageNumber - 1) *
                        pageSize)

                    .Take(
                        pageSize)

                    .ToListAsync();


            return new PagedResult<SupplierPayment>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }


        public async Task<PagedResult<SupplierPayment>>
            SearchPagedAsync(
                string? searchText,
                int pageNumber,
                int pageSize)
        {
            pageNumber =
                pageNumber < 1
                    ? 1
                    : pageNumber;


            pageSize =
                pageSize < 1
                    ? 10
                    : pageSize;


            var query =
                BuildActiveIndexQuery();


            searchText =
                searchText?.Trim();


            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                // =============================================
                // DATE SEARCH
                //
                // Supported:
                // 31-08-2026
                // 31/08/2026
                // 2026-08-31
                // 31-08-26
                // 31/08/26
                // =============================================

                if (TryParseSearchDate(
                    searchText,
                    out var searchDate))
                {
                    var fromDate =
                        searchDate.Date;


                    var toDate =
                        fromDate.AddDays(1);


                    query =
                        query.Where(x =>
                            x.Transactions.Any(t =>
                                t.IsActive &&
                                !t.IsDeleted &&
                                t.PaymentDate >= fromDate &&
                                t.PaymentDate < toDate));
                }
                else
                {
                    // =========================================
                    // NORMAL TEXT SEARCH
                    //
                    // Supports:
                    // - Payment No.
                    // - ERP Purchase Invoice No.
                    // - Supplier Invoice No.
                    // - Supplier Name
                    // - Payment Mode
                    // - Bank Name
                    // - Reference / UTR / Cheque No.
                    // =========================================

                    var search =
                        searchText.ToLower();


                    query =
                        query.Where(x =>

                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            (
                                x.PurchaseInvoice != null &&
                                x.PurchaseInvoice.Code
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.PurchaseInvoice != null &&
                                x.PurchaseInvoice
                                    .SupplierInvoiceNumber != null &&
                                x.PurchaseInvoice
                                    .SupplierInvoiceNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.PurchaseInvoice != null &&
                                x.PurchaseInvoice
                                    .SupplierName != null &&
                                x.PurchaseInvoice
                                    .SupplierName
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            x.Transactions.Any(t =>
                                t.IsActive &&
                                !t.IsDeleted &&
                                t.PaymentMode
                                    .ToLower()
                                    .Contains(search))

                            ||

                            x.Transactions.Any(t =>
                                t.IsActive &&
                                !t.IsDeleted &&
                                t.BankName != null &&
                                t.BankName
                                    .ToLower()
                                    .Contains(search))

                            ||

                            x.Transactions.Any(t =>
                                t.IsActive &&
                                !t.IsDeleted &&
                                t.ReferenceNumber != null &&
                                t.ReferenceNumber
                                    .ToLower()
                                    .Contains(search))

                            ||

                            x.Transactions.Any(t =>
                                t.IsActive &&
                                !t.IsDeleted &&
                                t.Remarks != null &&
                                t.Remarks
                                    .ToLower()
                                    .Contains(search))
                        );
                }
            }


            var totalRecords =
                await query.CountAsync();


            var items =
                await query

                    .OrderByDescending(x =>
                        x.Transactions
                            .Where(t =>
                                t.IsActive &&
                                !t.IsDeleted)
                            .Select(t =>
                                (DateTime?)t.PaymentDate)
                            .Max()
                        ?? DateTime.MinValue)

                    .ThenByDescending(x =>
                        x.Id)

                    .Skip(
                        (pageNumber - 1) *
                        pageSize)

                    .Take(
                        pageSize)

                    .ToListAsync();


            return new PagedResult<SupplierPayment>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE SOURCE
        // =====================================================

        #region Purchase Invoice Source

        public async Task<List<PurchaseInvoice>>
            GetAvailablePurchaseInvoicesAsync()
        {
            /*
             * Only Finalized Purchase Invoices are eligible.
             *
             * IMPORTANT:
             * IgnoreQueryFilters is used for SupplierPayments
             * in the EXISTS check.
             *
             * Therefore even if SPAY-001 is soft-deleted,
             * PI-001 cannot create SPAY-002.
             *
             * User must Restore SPAY-001 instead.
             */

            return await _context.PurchaseInvoices

                .AsNoTracking()

                .Where(pi =>
                    pi.IsActive &&
                    !pi.IsDeleted &&
                    pi.Status ==
                        PurchaseInvoiceStatus.Finalized)

                .Where(pi =>
                    !_context.SupplierPayments
                        .IgnoreQueryFilters()
                        .Any(sp =>
                            sp.PurchaseInvoiceId ==
                                pi.Id))

                .OrderBy(pi =>
                    pi.DueDate)

                .ThenBy(pi =>
                    pi.PurchaseInvoiceDate)

                .ThenBy(pi =>
                    pi.Id)

                .ToListAsync();
        }


        public async Task<PurchaseInvoice?>
            GetPurchaseInvoiceForPaymentAsync(
                int purchaseInvoiceId)
        {
            return await _context.PurchaseInvoices

                .AsNoTracking()

                .FirstOrDefaultAsync(pi =>
                    pi.Id ==
                        purchaseInvoiceId &&
                    pi.IsActive &&
                    !pi.IsDeleted);
        }


        public async Task<bool>
            ExistsForPurchaseInvoiceAsync(
                int purchaseInvoiceId)
        {
            return await _context.SupplierPayments

                .IgnoreQueryFilters()

                .AnyAsync(sp =>
                    sp.PurchaseInvoiceId ==
                        purchaseInvoiceId);
        }

        #endregion


        // =====================================================
        // PAID AMOUNT
        // =====================================================

        #region Paid Amount

        public async Task<decimal>
            GetPaidAmountAsync(
                int supplierPaymentId)
        {
            /*
             * Paid Amount =
             *
             * SUM(
             *     Active +
             *     Non-deleted Transactions
             * )
             */

            return await _context
                .SupplierPaymentTransactions

                .Where(t =>
                    t.SupplierPaymentId ==
                        supplierPaymentId &&
                    t.IsActive &&
                    !t.IsDeleted)

                .SumAsync(t =>
                    (decimal?)t.Amount)

                ?? 0m;
        }

        #endregion


        // =====================================================
        // CREATE
        // =====================================================

        #region Create

        public async Task AddAsync(
            SupplierPayment supplierPayment)
        {
            /*
             * SupplierPayment may already contain
             * the first transaction in:
             *
             * supplierPayment.Transactions
             *
             * EF Core will insert the complete graph.
             */

            await _context.SupplierPayments
                .AddAsync(
                    supplierPayment);


            await _context
                .SaveChangesAsync();
        }


        public async Task AddTransactionAsync(
            SupplierPaymentTransaction transaction)
        {
            await _context
                .SupplierPaymentTransactions
                .AddAsync(
                    transaction);


            await _context
                .SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // UPDATE
        // =====================================================

        #region Update

        public async Task<SupplierPayment?>
            GetForUpdateAsync(
                int id)
        {
            return await _context.SupplierPayments

                .Include(x =>
                    x.PurchaseInvoice)

                .Include(x =>
                    x.Transactions)

                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsDeleted);
        }


        public async Task UpdateAsync(
            SupplierPayment supplierPayment)
        {
            /*
             * Entity is normally already tracked through
             * GetForUpdateAsync / GetDeletedForUpdateAsync.
             *
             * Update() also allows controlled detached usage.
             */

            _context.SupplierPayments
                .Update(
                    supplierPayment);


            await _context
                .SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // DELETED / RESTORE
        // =====================================================

        #region Deleted / Restore

        public async Task<List<SupplierPayment>>
            GetDeletedAsync()
        {
            return await _context.SupplierPayments

                .IgnoreQueryFilters()

                .AsNoTracking()

                .Include(x =>
                    x.PurchaseInvoice)

                .Include(x =>
                    x.Supplier)

                .Include(x =>
                    x.Company)

                .Include(x =>
                    x.Transactions)

                .Where(x =>
                    x.IsDeleted)

                .OrderByDescending(x =>
                    x.ModifiedOn)

                .ThenByDescending(x =>
                    x.Id)

                .ToListAsync();
        }


        public async Task<SupplierPayment?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context.SupplierPayments

                .IgnoreQueryFilters()

                .Include(x =>
                    x.PurchaseInvoice)

                .Include(x =>
                    x.Transactions)

                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted);
        }

        #endregion


        // =====================================================
        // PAYMENT CODE GENERATION
        // =====================================================

        #region Payment Code Generation

        public async Task<string?>
            GetLastCodeAsync(
                string prefix)
        {
            return await _context.SupplierPayments

                .IgnoreQueryFilters()

                .Where(x =>
                    x.Code.StartsWith(
                        prefix))

                .OrderByDescending(x =>
                    x.Code)

                .Select(x =>
                    x.Code)

                .FirstOrDefaultAsync();
        }

        #endregion


        // =====================================================
        // PRIVATE QUERY HELPERS
        // =====================================================

        #region Private Query Helpers

        private IQueryable<SupplierPayment>
            BuildActiveIndexQuery()
        {
            /*
             * PurchaseInvoice is required on Index because:
             *
             * - ERP Invoice No. is displayed
             * - Supplier Invoice No. is displayed
             * - Supplier frozen name is displayed
             * - Invoice Total is displayed
             *
             * Transactions are required because:
             *
             * - Paid Amount = SUM transactions
             * - Outstanding = Invoice Total - Paid
             * - Payment status is calculated from totals
             */

            return _context.SupplierPayments

                .AsNoTracking()

                .Include(x =>
                    x.PurchaseInvoice)

                .Include(x =>
                    x.Supplier)

                .Include(x =>
                    x.Company)

                .Include(x =>
                    x.Transactions
                        .Where(t =>
                            t.IsActive &&
                            !t.IsDeleted))

                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted);
        }


        private static bool TryParseSearchDate(
            string value,
            out DateTime date)
        {
            var formats =
                new[]
                {
                    "dd-MM-yyyy",
                    "dd/MM/yyyy",
                    "yyyy-MM-dd",
                    "dd-MM-yy",
                    "dd/MM/yy"
                };


            return DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        #endregion
    }
}