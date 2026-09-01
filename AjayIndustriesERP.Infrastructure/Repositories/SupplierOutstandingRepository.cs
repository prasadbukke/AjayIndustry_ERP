/*
=============================================================
File: SupplierOutstandingRepository.cs
Module: Supplier Outstanding / Payables
Layer: Infrastructure - Repository

Purpose:
Provides read-only Supplier Outstanding data for:

1. Supplier Outstanding / Payables Report
2. Home Dashboard Supplier Payment Due Popup

Data Sources:
- PurchaseInvoices
- SupplierPayments
- SupplierPaymentTransactions

Important:
- No new Entity.
- No new Table.
- No Migration.
- No Add / Update / Delete.
- Only Finalized Purchase Invoices are considered.
- Paid Amount is calculated LIVE.

A payment transaction counts only when:
- SupplierPayment IsActive = true
- SupplierPayment IsDeleted = false
- SupplierPaymentTransaction IsActive = true
- SupplierPaymentTransaction IsDeleted = false

Dashboard Alert Rule:
- Finalized Purchase Invoice
- Active / Not Deleted
- Due Date exists
- Outstanding > 0
- Due Date <= Today + 5 days
- Includes Overdue + Due Soon
- Fully paid invoices are excluded automatically
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


        #region Get Paged Report

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

            var query =
                BuildBaseQuery();

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

            var rows =
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


        #region Get Dashboard Due Alerts

        public async Task<
            List<SupplierOutstandingRepositoryRow>>
            GetDueAlertsAsync(
                DateTime today,
                int dueSoonDays)
        {
            #region Date Safety

            today =
                today.Date;

            if (dueSoonDays < 0)
            {
                dueSoonDays = 0;
            }

            var dueSoonDate =
                today.AddDays(
                    dueSoonDays);

            #endregion


            #region Alert Query

            /*
             * Dashboard popup includes:
             *
             * - Overdue invoices
             * - Due today
             * - Due within next configured days
             *
             * But ONLY when:
             *
             * Outstanding =
             * InvoiceTotal - PaidAmount > 0
             */

            var alerts =
                await BuildBaseQuery()
                    .Where(row =>
                        row.DueDate.HasValue &&

                        row.DueDate.Value <=
                            dueSoonDate &&

                        row.InvoiceTotal -
                        row.PaidAmount > 0m)
                    .OrderBy(row =>
                        row.DueDate)
                    .ThenBy(row =>
                        row.SupplierName)
                    .ThenBy(row =>
                        row.PurchaseInvoiceCode)
                    .ToListAsync();

            #endregion


            return alerts;
        }

        #endregion


        #region Base Query

        /// <summary>
        /// Builds common live outstanding query used by:
        ///
        /// - Supplier Outstanding report
        /// - Home Dashboard popup
        ///
        /// Important:
        /// Soft-deleted Supplier Payments and Transactions
        /// are never counted.
        /// </summary>
        private IQueryable<SupplierOutstandingRepositoryRow>
            BuildBaseQuery()
        {
            return
                _context.PurchaseInvoices
                    .AsNoTracking()
                    .Where(pi =>
                        pi.IsActive &&
                        !pi.IsDeleted &&
                        pi.Status ==
                            PurchaseInvoiceStatus.Finalized)
                    .Select(pi =>
                        new SupplierOutstandingRepositoryRow
                        {
                            #region Purchase Invoice

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

                            #endregion


                            #region Supplier

                            SupplierId =
                                pi.SupplierId,

                            SupplierName =
                                pi.SupplierName,

                            #endregion


                            #region Amounts

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

                            #endregion
                        });
        }

        #endregion
    }
}