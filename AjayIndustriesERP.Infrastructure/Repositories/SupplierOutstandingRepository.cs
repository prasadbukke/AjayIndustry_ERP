/*
=============================================================
File: SupplierOutstandingRepository.cs
Module: Supplier Outstanding / Payables
Layer: Infrastructure - Repository

Purpose:
Provides read-only Supplier Outstanding data from existing:

- PurchaseInvoices
- SupplierPayments
- SupplierPaymentTransactions

Important:
- No new Entity.
- No new Table.
- No Migration.
- No Add / Update / Delete.
- Only finalized Purchase Invoices are considered.
- Paid Amount is calculated LIVE.

Payment transaction counts only when:
- SupplierPayment IsActive = true
- SupplierPayment IsDeleted = false
- Transaction IsActive = true
- Transaction IsDeleted = false

Default:
- Shows only invoices with Outstanding > 0.

Payment Filters:
- Outstanding Only
- All
- Pending
- Partially Paid
- Completed

Due Filters:
- Overdue
- Due Soon
- Upcoming
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class SupplierOutstandingRepository
        : ISupplierOutstandingRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public SupplierOutstandingRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Get Paged

        public async Task<
            PagedResult<SupplierOutstandingRepositoryRow>>
            GetPagedAsync(
                SupplierOutstandingRepositoryFilter filter)
        {
            #region Pagination Safety

            var pageNumber =
                filter.PageNumber < 1
                    ? 1
                    : filter.PageNumber;

            var pageSize =
                filter.PageSize <= 0
                    ? 10
                    : filter.PageSize;

            #endregion


            #region Date Context

            var today =
                filter.Today.Date;

            var dueSoonDate =
                today.AddDays(
                    filter.DueSoonDays);

            #endregion


            #region Base Query

            /*
             * Finalized Purchase Invoice
             *
             * Paid Amount =
             * Sum of active/non-deleted transactions
             * under active/non-deleted Supplier Payment.
             */

            var query =
                _context.PurchaseInvoices
                    .AsNoTracking()
                    .Where(pi =>
                        pi.IsActive &&
                        !pi.IsDeleted &&
                        pi.Status ==
                            PurchaseInvoiceStatus.Finalized)
                    .Select(pi => new
                    {
                        PurchaseInvoiceId =
                            pi.Id,

                        PurchaseInvoiceCode =
                            pi.Code,

                        SupplierInvoiceNumber =
                            pi.SupplierInvoiceNumber,

                        PurchaseInvoiceDate =
                            pi.PurchaseInvoiceDate,

                        DueDate =
                            pi.DueDate,

                        SupplierId =
                            pi.SupplierId,

                        SupplierName =
                            pi.SupplierName,

                        InvoiceTotal =
                            pi.GrandTotal,

                        PaidAmount =
                            _context.SupplierPayments
                                .Where(sp =>
                                    sp.IsActive &&
                                    !sp.IsDeleted &&
                                    sp.PurchaseInvoiceId ==
                                        pi.Id)
                                .SelectMany(sp =>
                                    sp.Transactions
                                        .Where(transaction =>
                                            transaction.IsActive &&
                                            !transaction.IsDeleted))
                                .Sum(transaction =>
                                    (decimal?)transaction.Amount)
                            ?? 0m
                    });

            #endregion


            #region Search Filter

            if (!string.IsNullOrWhiteSpace(
                filter.SearchText))
            {
                var searchText =
                    filter.SearchText.Trim();

                query =
                    query.Where(row =>
                        row.PurchaseInvoiceCode
                            .Contains(searchText) ||

                        (
                            row.SupplierInvoiceNumber != null &&
                            row.SupplierInvoiceNumber
                                .Contains(searchText)
                        ) ||

                        row.SupplierName
                            .Contains(searchText));
            }

            #endregion


            #region Supplier Filter

            if (filter.SupplierId.HasValue &&
                filter.SupplierId.Value > 0)
            {
                var supplierId =
                    filter.SupplierId.Value;

                query =
                    query.Where(row =>
                        row.SupplierId ==
                            supplierId);
            }

            #endregion


            #region Due Date Range Filter

            if (filter.DueDateFrom.HasValue)
            {
                var dueDateFrom =
                    filter.DueDateFrom.Value.Date;

                query =
                    query.Where(row =>
                        row.DueDate.HasValue &&
                        row.DueDate.Value >=
                            dueDateFrom);
            }


            if (filter.DueDateTo.HasValue)
            {
                var dueDateTo =
                    filter.DueDateTo.Value.Date;

                query =
                    query.Where(row =>
                        row.DueDate.HasValue &&
                        row.DueDate.Value <=
                            dueDateTo);
            }

            #endregion


            #region Payment Status Filter

            /*
             * Blank / Unknown:
             * Outstanding Only
             *
             * Outstanding =
             * InvoiceTotal - PaidAmount > 0
             */

            if (string.Equals(
                filter.PaymentStatus,
                "All",
                StringComparison.OrdinalIgnoreCase))
            {
                /*
                 * No payment status filter.
                 */
            }
            else if (string.Equals(
                filter.PaymentStatus,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(row =>
                        row.InvoiceTotal > 0m &&
                        row.PaidAmount <= 0m);
            }
            else if (string.Equals(
                filter.PaymentStatus,
                "Partially Paid",
                StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(row =>
                        row.PaidAmount > 0m &&
                        row.PaidAmount <
                            row.InvoiceTotal);
            }
            else if (string.Equals(
                filter.PaymentStatus,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(row =>
                        row.InvoiceTotal > 0m &&
                        row.PaidAmount >=
                            row.InvoiceTotal);
            }
            else
            {
                query =
                    query.Where(row =>
                        row.InvoiceTotal -
                        row.PaidAmount > 0m);
            }

            #endregion


            #region Due Status Filter

            if (string.Equals(
                filter.DueStatus,
                "Overdue",
                StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(row =>
                        row.DueDate.HasValue &&
                        row.DueDate.Value <
                            today);
            }
            else if (string.Equals(
                filter.DueStatus,
                "Due Soon",
                StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(row =>
                        row.DueDate.HasValue &&
                        row.DueDate.Value >=
                            today &&
                        row.DueDate.Value <=
                            dueSoonDate);
            }
            else if (string.Equals(
                filter.DueStatus,
                "Upcoming",
                StringComparison.OrdinalIgnoreCase))
            {
                query =
                    query.Where(row =>
                        row.DueDate.HasValue &&
                        row.DueDate.Value >
                            dueSoonDate);
            }

            #endregion


            #region Total Records

            var totalRecords =
                await query.CountAsync();

            #endregion


            #region Sorting + Pagination

            /*
             * Priority:
             *
             * 1. Due Date available records first
             * 2. Earliest Due Date first
             * 3. Latest Purchase Invoice first
             */

            var data =
                await query
                    .OrderBy(row =>
                        row.DueDate.HasValue
                            ? 0
                            : 1)
                    .ThenBy(row =>
                        row.DueDate)
                    .ThenByDescending(row =>
                        row.PurchaseInvoiceDate)
                    .ThenByDescending(row =>
                        row.PurchaseInvoiceId)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            #endregion


            #region Map Repository Rows

            var rows =
                data
                    .Select(row =>
                        new SupplierOutstandingRepositoryRow
                        {
                            PurchaseInvoiceId =
                                row.PurchaseInvoiceId,

                            PurchaseInvoiceCode =
                                row.PurchaseInvoiceCode,

                            SupplierInvoiceNumber =
                                row.SupplierInvoiceNumber,

                            PurchaseInvoiceDate =
                                row.PurchaseInvoiceDate,

                            DueDate =
                                row.DueDate,

                            SupplierId =
                                row.SupplierId,

                            SupplierName =
                                row.SupplierName,

                            InvoiceTotal =
                                row.InvoiceTotal,

                            PaidAmount =
                                row.PaidAmount
                        })
                    .ToList();

            #endregion


            #region Paged Result

            return new
                PagedResult<SupplierOutstandingRepositoryRow>
            {
                Items =
                        rows,

                PageNumber =
                        pageNumber,

                PageSize =
                        pageSize,

                TotalRecords =
                        totalRecords
            };

            #endregion
        }

        #endregion


        #region Get Supplier Options

        public async Task<
            List<SupplierOutstandingSupplierOption>>
            GetSupplierOptionsAsync()
        {
            var suppliers =
                await _context.PurchaseInvoices
                    .AsNoTracking()
                    .Where(pi =>
                        pi.IsActive &&
                        !pi.IsDeleted &&
                        pi.Status ==
                            PurchaseInvoiceStatus.Finalized)
                    .Select(pi =>
                        new SupplierOutstandingSupplierOption
                        {
                            SupplierId =
                                pi.SupplierId,

                            SupplierName =
                                pi.SupplierName
                        })
                    .Distinct()
                    .OrderBy(row =>
                        row.SupplierName)
                    .ToListAsync();


            return suppliers;
        }

        #endregion
    }
}