/*
============================================================
File: CustomerOutstandingReportService.cs

Module:
Customer Outstanding / Receivables Report

Purpose:
Contains business logic for Customer Outstanding Report.

Responsibilities:
- Validate report filters.
- Normalize Payment Status filter.
- Load report data from repository.
- Derive Invoice Payment Status.
- Calculate Invoice Age.
- Calculate Overdue Days.
- Prepare pagination information.

Important:
- This is a read-only report service.
- No new database entity or migration is required.
- Outstanding is derived from:
      Finalized Invoice GrandTotal
      -
      Finalized Customer Receipt Allocations.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class CustomerOutstandingReportService
        : ICustomerOutstandingReportService
    {
        #region Fields

        private readonly
            ICustomerOutstandingReportRepository
            _repository;

        #endregion


        #region Constructor

        public CustomerOutstandingReportService(
            ICustomerOutstandingReportRepository repository)
        {
            _repository =
                repository;
        }

        #endregion


        #region Customers

        public async Task<List<Customer>>
            GetCustomersForFilterAsync()
        {
            return await _repository
                .GetCustomersForFilterAsync();
        }

        #endregion


        #region Report

        public async Task<CustomerOutstandingReportResult>
            GetReportAsync(
                int? customerId,
                DateTime? fromDate,
                DateTime? toDate,
                string? paymentStatus,
                string? searchText,
                int pageNumber,
                int pageSize)
        {
            #region Normalize Pagination

            NormalizePagination(
                ref pageNumber,
                ref pageSize);

            #endregion


            #region Normalize Dates

            if (fromDate.HasValue)
            {
                fromDate =
                    fromDate.Value.Date;
            }


            if (toDate.HasValue)
            {
                toDate =
                    toDate.Value.Date;
            }


            if (
                fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value >
                toDate.Value
            )
            {
                throw new BusinessException(
                    "From Date cannot be greater than To Date.");
            }

            #endregion


            #region Normalize Customer

            if (
                customerId.HasValue &&
                customerId.Value <= 0
            )
            {
                customerId =
                    null;
            }

            #endregion


            #region Normalize Payment Status

            var normalizedPaymentStatus =
                NormalizePaymentStatus(
                    paymentStatus);

            #endregion


            #region Normalize Search

            searchText =
                string.IsNullOrWhiteSpace(
                    searchText)

                    ? null

                    : searchText.Trim();

            #endregion


            #region Repository Data

            var data =
                await _repository
                    .GetReportAsync(
                        customerId,
                        fromDate,
                        toDate,
                        normalizedPaymentStatus,
                        searchText,
                        pageNumber,
                        pageSize);

            #endregion


            #region Result

            var result =
                new CustomerOutstandingReportResult
                {
                    TotalInvoiceAmount =
                        RoundMoney(
                            data.TotalInvoiceAmount),

                    TotalReceivedAmount =
                        RoundMoney(
                            data.TotalReceivedAmount),

                    TotalOutstandingAmount =
                        RoundMoney(
                            data.TotalOutstandingAmount),

                    OutstandingInvoiceCount =
                        data.OutstandingInvoiceCount,


                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize,

                    TotalRecords =
                        data.TotalRecords
                };

            #endregion


            #region Pagination

            result.TotalPages =
                result.TotalRecords <= 0

                    ? 0

                    : (int)Math.Ceiling(
                        result.TotalRecords /
                        (double)result.PageSize);


            result.HasPrevious =
                result.PageNumber > 1;


            result.HasNext =
                result.PageNumber <
                result.TotalPages;

            #endregion


            #region Rows

            var today =
                DateTime.Today;


            foreach (var item
                in data.Items)
            {
                var invoiceAmount =
                    RoundMoney(
                        item.InvoiceAmount);


                var receivedAmount =
                    RoundMoney(
                        item.ReceivedAmount);


                var outstandingAmount =
                    RoundMoney(
                        item.OutstandingAmount);


                if (receivedAmount < 0)
                {
                    receivedAmount =
                        0;
                }


                if (outstandingAmount < 0)
                {
                    outstandingAmount =
                        0;
                }


                #region Payment Status

                var invoicePaymentStatus =
                    GetPaymentStatus(
                        invoiceAmount,
                        receivedAmount,
                        outstandingAmount);

                #endregion


                #region Age Days

                var invoiceDate =
                    item.InvoiceDate.Date;


                var ageDays =
                    today > invoiceDate

                        ? (today - invoiceDate)
                            .Days

                        : 0;

                #endregion


                #region Overdue

                var overdueDays =
                    0;


                var isOverdue =
                    false;


                if (
                    outstandingAmount > 0 &&
                    item.DueDate.HasValue
                )
                {
                    var dueDate =
                        item.DueDate.Value.Date;


                    if (today > dueDate)
                    {
                        overdueDays =
                            (today - dueDate)
                                .Days;


                        isOverdue =
                            true;
                    }
                }

                #endregion


                result.Items.Add(
                    new CustomerOutstandingReportResultItem
                    {
                        #region Customer

                        CustomerId =
                            item.CustomerId,

                        CustomerCode =
                            item.CustomerCode,

                        CustomerName =
                            item.CustomerName,

                        #endregion


                        #region Invoice

                        InvoiceId =
                            item.InvoiceId,

                        InvoiceCode =
                            item.InvoiceCode,

                        InvoiceDate =
                            item.InvoiceDate,

                        DueDate =
                            item.DueDate,

                        #endregion


                        #region Amounts

                        InvoiceAmount =
                            invoiceAmount,

                        ReceivedAmount =
                            receivedAmount,

                        OutstandingAmount =
                            outstandingAmount,

                        #endregion


                        #region Payment Status

                        PaymentStatus =
                            invoicePaymentStatus,

                        #endregion


                        #region Ageing

                        AgeDays =
                            ageDays,

                        OverdueDays =
                            overdueDays,

                        IsOverdue =
                            isOverdue

                        #endregion
                    });
            }

            #endregion


            return result;
        }

        #endregion


        #region Payment Status

        private static string GetPaymentStatus(
            decimal invoiceAmount,
            decimal receivedAmount,
            decimal outstandingAmount)
        {
            if (
                outstandingAmount <= 0

                ||

                receivedAmount >=
                invoiceAmount
            )
            {
                return "Paid";
            }


            if (receivedAmount <= 0)
            {
                return "Unpaid";
            }


            return "Partially Paid";
        }

        #endregion


        #region Payment Status Normalization

        private static string NormalizePaymentStatus(
            string? paymentStatus)
        {
            var value =
                (
                    paymentStatus
                    ?? "Outstanding"
                )
                .Trim();


            if (string.Equals(
                value,
                "All",
                StringComparison.OrdinalIgnoreCase))
            {
                return "All";
            }


            if (string.Equals(
                value,
                "Unpaid",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Unpaid";
            }


            if (
                string.Equals(
                    value,
                    "PartiallyPaid",
                    StringComparison.OrdinalIgnoreCase)

                ||

                string.Equals(
                    value,
                    "Partially Paid",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return "PartiallyPaid";
            }


            if (string.Equals(
                value,
                "Paid",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Paid";
            }


            /*
             * Default report behavior:
             * show only Receivables still pending.
             */
            return "Outstanding";
        }

        #endregion


        #region Pagination

        private static void NormalizePagination(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber =
                    1;
            }


            if (
                pageSize != 10 &&
                pageSize != 25 &&
                pageSize != 50
            )
            {
                pageSize =
                    25;
            }
        }

        #endregion


        #region Money

        private static decimal RoundMoney(
            decimal amount)
        {
            return Math.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero);
        }

        #endregion
    }
}