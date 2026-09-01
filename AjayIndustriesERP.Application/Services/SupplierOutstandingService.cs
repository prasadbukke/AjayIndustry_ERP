/*
=============================================================
File: SupplierOutstandingService.cs
Module: Supplier Outstanding / Payables
Layer: Application - Service

Purpose:
Implements business logic for Supplier Outstanding report.

Important:
- Read-only service.
- No Create / Edit / Delete.
- No Entity / Table / Migration.
- Repository handles database query/filtering.
- Service handles:
    - filter normalization
    - pagination validation
    - payment status calculation
    - outstanding calculation
    - due status calculation
    - overdue days calculation
    - final report result mapping

Payment Status:
- Pending
- Partially Paid
- Completed

Due Status:
- Overdue
- Due Soon
- Upcoming
- No Due Date

Due Soon Rule:
- Today through next 5 days.
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
                        DateTime.Today,

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


            #region Map Rows

            var today =
                DateTime.Today;


            var rows =
                repositoryResult.Items
                    .Select(row =>
                    {
                        var paidAmount =
                            row.PaidAmount;

                        if (paidAmount < 0m)
                        {
                            paidAmount = 0m;
                        }


                        var outstandingAmount =
                            row.InvoiceTotal -
                            paidAmount;

                        if (outstandingAmount < 0m)
                        {
                            outstandingAmount = 0m;
                        }


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
             * Do NOT assign them here.
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


            if (date <= today.AddDays(
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


            /*
             * Unknown value falls back to
             * Outstanding Only.
             */
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