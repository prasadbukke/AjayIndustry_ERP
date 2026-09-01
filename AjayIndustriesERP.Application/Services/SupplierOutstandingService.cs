/*
=============================================================
File: SupplierOutstandingService.cs
Module: Supplier Outstanding / Payables
Layer: Application - Service

Purpose:
Implements business logic for:

1. Supplier Outstanding / Payables Report
2. Home Dashboard Supplier Payment Due Alerts

Important:
- Read-only service.
- No Create / Edit / Delete.
- No Entity / Table / Migration.

Report Responsibilities:
- Filter normalization
- Pagination validation
- Outstanding calculation
- Payment status calculation
- Due status calculation
- Overdue days calculation

Dashboard Alert Rule:
- Finalized Purchase Invoice
- Active / Not Deleted
- Outstanding > 0
- Due Date exists
- Due Date <= Today + 5 days
- Includes:
      Overdue
      Due Today
      Due within next 5 days
- Fully paid invoices are excluded.

Payment Status:
- Pending
- Partially Paid
- Completed

Due Status:
- Overdue
- Due Soon
- Upcoming
- No Due Date
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;

namespace AjayIndustriesERP.Application.Services
{
    public class SupplierOutstandingService
        : ISupplierOutstandingService
    {
        #region Constants

        private const int DueSoonDays = 5;

        #endregion


        #region Fields

        private readonly ISupplierOutstandingRepository _repository;

        #endregion


        #region Constructor

        public SupplierOutstandingService(
            ISupplierOutstandingRepository repository)
        {
            _repository = repository;
        }

        #endregion


        #region Get Report

        public async Task<SupplierOutstandingResult>
            GetReportAsync(
                SupplierOutstandingFilter filter)
        {
            #region Null Safety

            filter ??=
                new SupplierOutstandingFilter();

            #endregion


            #region Normalize Search

            var searchText =
                string.IsNullOrWhiteSpace(
                    filter.SearchText)
                    ? null
                    : filter.SearchText.Trim();

            #endregion


            #region Normalize Supplier

            int? supplierId =
                filter.SupplierId.HasValue &&
                filter.SupplierId.Value > 0
                    ? filter.SupplierId.Value
                    : null;

            #endregion


            #region Normalize Payment Status

            var paymentStatus =
                NormalizePaymentStatus(
                    filter.PaymentStatus);

            #endregion


            #region Normalize Due Status

            var dueStatus =
                NormalizeDueStatus(
                    filter.DueStatus);

            #endregion


            #region Normalize Dates

            var dueDateFrom =
                filter.DueDateFrom?.Date;

            var dueDateTo =
                filter.DueDateTo?.Date;

            #endregion


            #region Pagination Validation

            var pageNumber =
                filter.PageNumber < 1
                    ? 1
                    : filter.PageNumber;


            var allowedPageSizes =
                new[]
                {
                    10,
                    25,
                    50,
                    100
                };


            var pageSize =
                allowedPageSizes.Contains(
                    filter.PageSize)
                    ? filter.PageSize
                    : 10;

            #endregion


            #region Repository Filter

            var today =
                DateTime.Today;


            var repositoryFilter =
                new SupplierOutstandingRepositoryFilter
                {
                    SearchText =
                        searchText,

                    SupplierId =
                        supplierId,

                    PaymentStatus =
                        paymentStatus,

                    DueStatus =
                        dueStatus,

                    DueDateFrom =
                        dueDateFrom,

                    DueDateTo =
                        dueDateTo,

                    Today =
                        today,

                    DueSoonDays =
                        DueSoonDays,

                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize
                };

            #endregion


            #region Load Repository Data

            var repositoryResult =
                await _repository.GetPagedAsync(
                    repositoryFilter);

            #endregion


            #region Map Report Rows

            var rows =
                repositoryResult.Items
                    .Select(row =>
                    {
                        var paidAmount =
                            NormalizePaidAmount(
                                row.PaidAmount);


                        var outstandingAmount =
                            CalculateOutstandingAmount(
                                row.InvoiceTotal,
                                paidAmount);


                        return new
                            SupplierOutstandingResultRow
                        {
                            #region Purchase Invoice

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

                            #endregion


                            #region Supplier

                            SupplierId =
                                row.SupplierId,

                            SupplierName =
                                row.SupplierName,

                            #endregion


                            #region Amounts

                            InvoiceTotal =
                                row.InvoiceTotal,

                            PaidAmount =
                                paidAmount,

                            OutstandingAmount =
                                outstandingAmount,

                            #endregion


                            #region Status

                            PaymentStatus =
                                CalculatePaymentStatus(
                                    row.InvoiceTotal,
                                    paidAmount),

                            DueStatus =
                                CalculateDueStatus(
                                    row.DueDate,
                                    today),

                            OverdueDays =
                                CalculateOverdueDays(
                                    row.DueDate,
                                    today)

                            #endregion
                        };
                    })
                    .ToList();

            #endregion


            #region Build Paged Result

            /*
             * TotalPages / HasPrevious / HasNext
             * are calculated automatically by PagedResult.
             *
             * Do NOT assign them manually.
             */

            var pagedResult =
                new PagedResult<
                    SupplierOutstandingResultRow>
                {
                    Items =
                        rows,

                    PageNumber =
                        repositoryResult.PageNumber,

                    PageSize =
                        repositoryResult.PageSize,

                    TotalRecords =
                        repositoryResult.TotalRecords
                };

            #endregion


            #region Final Result

            return new SupplierOutstandingResult
            {
                Results =
                    pagedResult,

                SearchText =
                    searchText,

                SupplierId =
                    supplierId,

                PaymentStatus =
                    paymentStatus,

                DueStatus =
                    dueStatus,

                DueDateFrom =
                    dueDateFrom,

                DueDateTo =
                    dueDateTo
            };

            #endregion
        }

        #endregion


        #region Get Supplier Options

        public async Task<
            List<SupplierOutstandingSupplierOption>>
            GetSupplierOptionsAsync()
        {
            return await
                _repository
                    .GetSupplierOptionsAsync();
        }

        #endregion


        #region Get Dashboard Due Alerts

        public async Task<
            List<SupplierOutstandingDueAlertResult>>
            GetDueAlertsAsync()
        {
            #region Date Context

            var today =
                DateTime.Today;

            #endregion


            #region Load Repository Alerts

            var repositoryAlerts =
                await _repository
                    .GetDueAlertsAsync(
                        today,
                        DueSoonDays);

            #endregion


            #region Map Dashboard Alerts

            var alerts =
                repositoryAlerts
                    .Where(row =>
                        row.DueDate.HasValue)
                    .Select(row =>
                    {
                        var dueDate =
                            row.DueDate!.Value.Date;


                        var paidAmount =
                            NormalizePaidAmount(
                                row.PaidAmount);


                        var outstandingAmount =
                            CalculateOutstandingAmount(
                                row.InvoiceTotal,
                                paidAmount);


                        return new
                            SupplierOutstandingDueAlertResult
                        {
                            #region Purchase Invoice

                            PurchaseInvoiceId =
                                row.PurchaseInvoiceId,

                            PurchaseInvoiceCode =
                                row.PurchaseInvoiceCode,

                            SupplierInvoiceNumber =
                                row.SupplierInvoiceNumber,

                            PurchaseInvoiceDate =
                                row.PurchaseInvoiceDate,

                            DueDate =
                                dueDate,

                            #endregion


                            #region Supplier

                            SupplierId =
                                row.SupplierId,

                            SupplierName =
                                row.SupplierName,

                            #endregion


                            #region Payment Position

                            InvoiceTotal =
                                row.InvoiceTotal,

                            PaidAmount =
                                paidAmount,

                            OutstandingAmount =
                                outstandingAmount,

                            PaymentStatus =
                                CalculatePaymentStatus(
                                    row.InvoiceTotal,
                                    paidAmount),

                            #endregion


                            #region Due Position

                            DueStatus =
                                CalculateDueStatus(
                                    dueDate,
                                    today),

                            OverdueDays =
                                CalculateOverdueDays(
                                    dueDate,
                                    today),

                            DaysUntilDue =
                                CalculateDaysUntilDue(
                                    dueDate,
                                    today)

                            #endregion
                        };
                    })
                    .Where(row =>
                        row.OutstandingAmount > 0m)
                    .OrderBy(row =>
                        row.DueDate)
                    .ThenBy(row =>
                        row.SupplierName)
                    .ThenBy(row =>
                        row.PurchaseInvoiceCode)
                    .ToList();

            #endregion


            return alerts;
        }

        #endregion


        #region Normalize Paid Amount

        private static decimal NormalizePaidAmount(
            decimal paidAmount)
        {
            if (paidAmount < 0m)
            {
                return 0m;
            }

            return paidAmount;
        }

        #endregion


        #region Calculate Outstanding Amount

        private static decimal CalculateOutstandingAmount(
            decimal invoiceTotal,
            decimal paidAmount)
        {
            var outstandingAmount =
                invoiceTotal -
                paidAmount;


            if (outstandingAmount < 0m)
            {
                return 0m;
            }


            return outstandingAmount;
        }

        #endregion


        #region Payment Status Calculation

        private static string CalculatePaymentStatus(
            decimal invoiceTotal,
            decimal paidAmount)
        {
            if (paidAmount <= 0m)
            {
                return "Pending";
            }


            if (paidAmount >= invoiceTotal)
            {
                return "Completed";
            }


            return "Partially Paid";
        }

        #endregion


        #region Due Status Calculation

        private static string CalculateDueStatus(
            DateTime? dueDate,
            DateTime today)
        {
            if (!dueDate.HasValue)
            {
                return "No Due Date";
            }


            var date =
                dueDate.Value.Date;


            if (date < today)
            {
                return "Overdue";
            }


            if (date <=
                today.AddDays(
                    DueSoonDays))
            {
                return "Due Soon";
            }


            return "Upcoming";
        }

        #endregion


        #region Overdue Days Calculation

        private static int CalculateOverdueDays(
            DateTime? dueDate,
            DateTime today)
        {
            if (!dueDate.HasValue)
            {
                return 0;
            }


            var date =
                dueDate.Value.Date;


            if (date >= today)
            {
                return 0;
            }


            return (today - date).Days;
        }

        #endregion


        #region Days Until Due Calculation

        private static int CalculateDaysUntilDue(
            DateTime dueDate,
            DateTime today)
        {
            var date =
                dueDate.Date;


            if (date <= today)
            {
                return 0;
            }


            return (date - today).Days;
        }

        #endregion


        #region Normalize Payment Status

        private static string?
            NormalizePaymentStatus(
                string? paymentStatus)
        {
            if (string.IsNullOrWhiteSpace(
                paymentStatus))
            {
                /*
                 * Null means:
                 * Outstanding Only.
                 */

                return null;
            }


            var value =
                paymentStatus.Trim();


            if (value.Equals(
                "All",
                StringComparison.OrdinalIgnoreCase))
            {
                return "All";
            }


            if (value.Equals(
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Pending";
            }


            if (value.Equals(
                "Partially Paid",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Partially Paid";
            }


            if (value.Equals(
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Completed";
            }


            return null;
        }

        #endregion


        #region Normalize Due Status

        private static string?
            NormalizeDueStatus(
                string? dueStatus)
        {
            if (string.IsNullOrWhiteSpace(
                dueStatus))
            {
                return null;
            }


            var value =
                dueStatus.Trim();


            if (value.Equals(
                "Overdue",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Overdue";
            }


            if (value.Equals(
                "Due Soon",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Due Soon";
            }


            if (value.Equals(
                "Upcoming",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Upcoming";
            }


            return null;
        }

        #endregion
    }
}