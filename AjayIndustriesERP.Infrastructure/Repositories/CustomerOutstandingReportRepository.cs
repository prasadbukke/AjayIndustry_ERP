/*
============================================================
File: CustomerOutstandingReportRepository.cs

Module:
Customer Outstanding / Receivables Report

Purpose:
Provides read-only data access for Customer Outstanding
and Receivables Report.

Responsibilities:
- Load active Customers for report filter.
- Load Finalized Invoices.
- Aggregate Finalized Customer Receipt allocations.
- Calculate Invoice outstanding.
- Apply report filters.
- Calculate filtered summary.
- Return paginated report rows.

Important:
- This is a read-only repository.
- No new database table or migration is required.
- Outstanding is derived from:
      Invoice GrandTotal
      -
      Finalized Customer Receipt Allocations.
- Draft Customer Receipts do NOT affect Outstanding.
- Deleted / inactive Receipts and Allocations do NOT
  affect Outstanding.
- No per-Invoice database query is performed.
============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class CustomerOutstandingReportRepository
        : ICustomerOutstandingReportRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public CustomerOutstandingReportRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Customers

        public async Task<List<Customer>>
            GetCustomersForFilterAsync()
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

        #endregion


        #region Report

        public async Task<CustomerOutstandingReportData>
            GetReportAsync(
                int? customerId,
                DateTime? fromDate,
                DateTime? toDate,
                string? paymentStatus,
                string? searchText,
                int pageNumber,
                int pageSize)
        {
            #region Normalize Search

            var search =
                (
                    searchText
                    ?? string.Empty
                )
                .Trim();

            #endregion


            #region Invoice Query

            var invoiceQuery =
                _context
                    .Invoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        x.Status ==
                            InvoiceStatus.Finalized);

            #endregion


            #region Invoice Customer Filter

            if (
                customerId.HasValue &&
                customerId.Value > 0
            )
            {
                invoiceQuery =
                    invoiceQuery.Where(x =>
                        x.CustomerId ==
                        customerId.Value);
            }

            #endregion


            #region Invoice Date Filter

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;


                invoiceQuery =
                    invoiceQuery.Where(x =>
                        x.InvoiceDate >=
                        from);
            }


            if (toDate.HasValue)
            {
                var toExclusive =
                    toDate.Value
                        .Date
                        .AddDays(
                            1);


                invoiceQuery =
                    invoiceQuery.Where(x =>
                        x.InvoiceDate <
                        toExclusive);
            }

            #endregion


            #region Invoice Search Filter

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                invoiceQuery =
                    invoiceQuery.Where(x =>

                        x.Code.Contains(
                            search)

                        ||

                        x.CustomerName.Contains(
                            search));
            }

            #endregion


            #region Load Invoice Rows

            var invoices =
                await invoiceQuery
                    .Select(x =>
                        new
                        {
                            CustomerId =
                                x.CustomerId,

                            CustomerName =
                                x.CustomerName,


                            InvoiceId =
                                x.Id,

                            InvoiceCode =
                                x.Code,

                            InvoiceDate =
                                x.InvoiceDate,

                            DueDate =
                                x.DueDate,

                            InvoiceAmount =
                                x.GrandTotal
                        })
                    .ToListAsync();

            #endregion


            #region Receipt Allocation Query

            /*
             * Separate aggregate query.
             *
             * We deliberately do NOT LEFT JOIN this grouped
             * query inside another EF query because some
             * EF Core / SQL Server combinations cannot
             * translate payment-status predicates over
             * that grouped LEFT JOIN.
             */
            var allocationQuery =
                _context
                    .CustomerReceiptAllocations
                    .AsNoTracking()
                    .Where(x =>

                        !x.IsDeleted &&

                        x.IsActive &&

                        x.CustomerReceipt != null &&

                        !x.CustomerReceipt.IsDeleted &&

                        x.CustomerReceipt.IsActive &&

                        x.CustomerReceipt.Status ==
                            CustomerReceiptStatus.Finalized &&

                        x.Invoice != null &&

                        !x.Invoice.IsDeleted &&

                        x.Invoice.IsActive &&

                        x.Invoice.Status ==
                            InvoiceStatus.Finalized);

            #endregion


            #region Allocation Customer Filter

            if (
                customerId.HasValue &&
                customerId.Value > 0
            )
            {
                allocationQuery =
                    allocationQuery.Where(x =>
                        x.Invoice!.CustomerId ==
                        customerId.Value);
            }

            #endregion


            #region Allocation Date Filter

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;


                allocationQuery =
                    allocationQuery.Where(x =>
                        x.Invoice!.InvoiceDate >=
                        from);
            }


            if (toDate.HasValue)
            {
                var toExclusive =
                    toDate.Value
                        .Date
                        .AddDays(
                            1);


                allocationQuery =
                    allocationQuery.Where(x =>
                        x.Invoice!.InvoiceDate <
                        toExclusive);
            }

            #endregion


            #region Allocation Search Filter

            if (!string.IsNullOrWhiteSpace(
                search))
            {
                allocationQuery =
                    allocationQuery.Where(x =>

                        x.Invoice!.Code.Contains(
                            search)

                        ||

                        x.Invoice.CustomerName.Contains(
                            search));
            }

            #endregion


            #region Load Allocation Totals

            var allocationTotals =
                await allocationQuery
                    .GroupBy(x =>
                        x.InvoiceId)
                    .Select(group =>
                        new
                        {
                            InvoiceId =
                                group.Key,

                            ReceivedAmount =
                                group.Sum(x =>
                                    x.AllocatedAmount)
                        })
                    .ToListAsync();


            var allocationMap =
                allocationTotals
                    .ToDictionary(
                        x =>
                            x.InvoiceId,

                        x =>
                            RoundMoney(
                                x.ReceivedAmount));

            #endregion


            #region Merge Invoice And Receipt Data

            var reportRows =
                new List<CustomerOutstandingReportItem>();


            foreach (var invoice
                in invoices)
            {
                allocationMap.TryGetValue(
                    invoice.InvoiceId,
                    out var receivedAmount);


                receivedAmount =
                    RoundMoney(
                        receivedAmount);


                if (receivedAmount < 0)
                {
                    receivedAmount =
                        0;
                }


                var invoiceAmount =
                    RoundMoney(
                        invoice.InvoiceAmount);


                var outstandingAmount =
                    RoundMoney(
                        invoiceAmount -
                        receivedAmount);


                /*
                 * Over-allocation should already be blocked
                 * by CustomerReceiptService.
                 *
                 * Defensive normalization only.
                 */
                if (outstandingAmount < 0)
                {
                    outstandingAmount =
                        0;
                }


                reportRows.Add(
                    new CustomerOutstandingReportItem
                    {
                        CustomerId =
                            invoice.CustomerId,

                        CustomerName =
                            invoice.CustomerName,


                        InvoiceId =
                            invoice.InvoiceId,

                        InvoiceCode =
                            invoice.InvoiceCode,

                        InvoiceDate =
                            invoice.InvoiceDate,

                        DueDate =
                            invoice.DueDate,


                        InvoiceAmount =
                            invoiceAmount,

                        ReceivedAmount =
                            receivedAmount,

                        OutstandingAmount =
                            outstandingAmount
                    });
            }

            #endregion


            #region Payment Status Filter

            var normalizedStatus =
                (
                    paymentStatus
                    ?? "Outstanding"
                )
                .Trim();


            IEnumerable<CustomerOutstandingReportItem>
                filteredRows =
                    reportRows;


            switch (normalizedStatus)
            {
                case "Unpaid":

                    filteredRows =
                        filteredRows.Where(x =>
                            x.ReceivedAmount <=
                            0m);

                    break;


                case "PartiallyPaid":

                    filteredRows =
                        filteredRows.Where(x =>

                            x.ReceivedAmount >
                                0m

                            &&

                            x.OutstandingAmount >
                                0m);

                    break;


                case "Paid":

                    filteredRows =
                        filteredRows.Where(x =>
                            x.OutstandingAmount <=
                            0m);

                    break;


                case "All":

                    /*
                     * No status filter.
                     */

                    break;


                case "Outstanding":

                default:

                    filteredRows =
                        filteredRows.Where(x =>
                            x.OutstandingAmount >
                            0m);

                    break;
            }

            #endregion


            #region Materialize Filtered Rows

            var filteredList =
                filteredRows
                    .ToList();

            #endregion


            #region Summary

            var totalInvoiceAmount =
                RoundMoney(
                    filteredList
                        .Sum(x =>
                            x.InvoiceAmount));


            var totalReceivedAmount =
                RoundMoney(
                    filteredList
                        .Sum(x =>
                            x.ReceivedAmount));


            var totalOutstandingAmount =
                RoundMoney(
                    filteredList
                        .Sum(x =>
                            x.OutstandingAmount));


            var outstandingInvoiceCount =
                filteredList
                    .Count(x =>
                        x.OutstandingAmount >
                        0m);

            #endregion


            #region Pagination

            var totalRecords =
                filteredList.Count;


            var pageItems =
                filteredList
                    .OrderByDescending(x =>
                        x.InvoiceDate)
                    .ThenByDescending(x =>
                        x.InvoiceId)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToList();

            #endregion


            #region Customer Codes

            var customerIds =
                pageItems
                    .Select(x =>
                        x.CustomerId)
                    .Distinct()
                    .ToList();


            Dictionary<int, string>
                customerCodeMap;


            if (customerIds.Count == 0)
            {
                customerCodeMap =
                    new Dictionary<int, string>();
            }
            else
            {
                customerCodeMap =
                    await _context
                        .Customers
                        .AsNoTracking()
                        .Where(x =>
                            customerIds.Contains(
                                x.Id))
                        .Select(x =>
                            new
                            {
                                x.Id,
                                x.Code
                            })
                        .ToDictionaryAsync(
                            x =>
                                x.Id,

                            x =>
                                x.Code);
            }

            #endregion


            #region Apply Customer Codes

            foreach (var item
                in pageItems)
            {
                if (customerCodeMap.TryGetValue(
                    item.CustomerId,
                    out var customerCode))
                {
                    item.CustomerCode =
                        customerCode;
                }
            }

            #endregion


            #region Result

            return new CustomerOutstandingReportData
            {
                TotalInvoiceAmount =
                    totalInvoiceAmount,

                TotalReceivedAmount =
                    totalReceivedAmount,

                TotalOutstandingAmount =
                    totalOutstandingAmount,

                OutstandingInvoiceCount =
                    outstandingInvoiceCount,


                TotalRecords =
                    totalRecords,

                Items =
                    pageItems
            };

            #endregion
        }

        #endregion


        #region Helpers

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