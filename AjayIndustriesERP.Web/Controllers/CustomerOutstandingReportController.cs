/*
============================================================
File: CustomerOutstandingReportController.cs

Module:
Customer Outstanding / Receivables Report

Purpose:
Handles Web requests for Customer Outstanding Report.

Responsibilities:
- Display read-only Customer Outstanding report.
- Apply Customer, Date, Payment Status and Search filters.
- Load Customer dropdown.
- Prepare summary totals.
- Prepare paginated report rows.

Important:
- This is a read-only reporting controller.
- No database write operation exists here.
- Outstanding is derived from:
      Finalized Invoice GrandTotal
      -
      Finalized Customer Receipt Allocations.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Web.ViewModels.CustomerOutstandingReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class CustomerOutstandingReportController
        : Controller
    {
        #region Fields

        private readonly
            ICustomerOutstandingReportService
            _reportService;

        #endregion


        #region Constructor

        public CustomerOutstandingReportController(
            ICustomerOutstandingReportService reportService)
        {
            _reportService =
                reportService;
        }

        #endregion


        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            int? customerId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string paymentStatus = "Outstanding",
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 25)
        {
            var viewModel =
                new CustomerOutstandingReportViewModel
                {
                    CustomerId =
                        customerId,

                    FromDate =
                        fromDate,

                    ToDate =
                        toDate,

                    PaymentStatus =
                        string.IsNullOrWhiteSpace(
                            paymentStatus)
                            ? "Outstanding"
                            : paymentStatus,

                    SearchText =
                        searchText,

                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize
                };


            try
            {
                #region Report

                var result =
                    await _reportService
                        .GetReportAsync(
                            customerId,
                            fromDate,
                            toDate,
                            paymentStatus,
                            searchText,
                            pageNumber,
                            pageSize);

                #endregion


                #region Summary

                viewModel.TotalInvoiceAmount =
                    result.TotalInvoiceAmount;

                viewModel.TotalReceivedAmount =
                    result.TotalReceivedAmount;

                viewModel.TotalOutstandingAmount =
                    result.TotalOutstandingAmount;

                viewModel.OutstandingInvoiceCount =
                    result.OutstandingInvoiceCount;

                #endregion


                #region Pagination

                viewModel.PageNumber =
                    result.PageNumber;

                viewModel.PageSize =
                    result.PageSize;

                viewModel.TotalRecords =
                    result.TotalRecords;

                viewModel.TotalPages =
                    result.TotalPages;

                viewModel.HasPrevious =
                    result.HasPrevious;

                viewModel.HasNext =
                    result.HasNext;

                #endregion


                #region Rows

                foreach (var item
                    in result.Items)
                {
                    viewModel.Items.Add(
                        new CustomerOutstandingReportRowViewModel
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
                                item.InvoiceAmount,

                            ReceivedAmount =
                                item.ReceivedAmount,

                            OutstandingAmount =
                                item.OutstandingAmount,

                            #endregion


                            #region Status

                            PaymentStatus =
                                item.PaymentStatus,

                            #endregion


                            #region Ageing

                            AgeDays =
                                item.AgeDays,

                            OverdueDays =
                                item.OverdueDays,

                            IsOverdue =
                                item.IsOverdue

                            #endregion
                        });
                }

                #endregion
            }
            catch (BusinessException ex)
            {
                ViewBag.ErrorMessage =
                    ex.Message;
            }


            #region Customer Dropdown

            await PopulateCustomersAsync(
                viewModel);

            #endregion


            #region Payment Status Dropdown

            PopulatePaymentStatuses(
                viewModel);

            #endregion


            return View(
                viewModel);
        }

        #endregion


        #region Customer Dropdown

        private async Task PopulateCustomersAsync(
            CustomerOutstandingReportViewModel viewModel)
        {
            var customers =
                await _reportService
                    .GetCustomersForFilterAsync();


            var options =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = "",

                        Text =
                            "-- All Customers --"
                    }
                };


            foreach (var customer
                in customers
                    .OrderBy(x =>
                        x.CustomerName)
                    .ThenBy(x =>
                        x.Code))
            {
                options.Add(
                    new SelectListItem
                    {
                        Value =
                            customer.Id
                                .ToString(),

                        Text =
                            string.IsNullOrWhiteSpace(
                                customer.Code)

                                ? customer.CustomerName

                                : $"{customer.Code} | " +
                                  $"{customer.CustomerName}",

                        Selected =
                            customer.Id ==
                            viewModel.CustomerId
                    });
            }


            viewModel.AvailableCustomers =
                options;
        }

        #endregion


        #region Payment Status Dropdown

        private static void PopulatePaymentStatuses(
            CustomerOutstandingReportViewModel viewModel)
        {
            viewModel.AvailablePaymentStatuses =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            "Outstanding",

                        Text =
                            "Outstanding Only",

                        Selected =
                            string.Equals(
                                viewModel.PaymentStatus,
                                "Outstanding",
                                StringComparison.OrdinalIgnoreCase)
                    },


                    new()
                    {
                        Value =
                            "All",

                        Text =
                            "All Invoices",

                        Selected =
                            string.Equals(
                                viewModel.PaymentStatus,
                                "All",
                                StringComparison.OrdinalIgnoreCase)
                    },


                    new()
                    {
                        Value =
                            "Unpaid",

                        Text =
                            "Unpaid",

                        Selected =
                            string.Equals(
                                viewModel.PaymentStatus,
                                "Unpaid",
                                StringComparison.OrdinalIgnoreCase)
                    },


                    new()
                    {
                        Value =
                            "PartiallyPaid",

                        Text =
                            "Partially Paid",

                        Selected =
                            string.Equals(
                                viewModel.PaymentStatus,
                                "PartiallyPaid",
                                StringComparison.OrdinalIgnoreCase)
                    },


                    new()
                    {
                        Value =
                            "Paid",

                        Text =
                            "Paid",

                        Selected =
                            string.Equals(
                                viewModel.PaymentStatus,
                                "Paid",
                                StringComparison.OrdinalIgnoreCase)
                    }
                };
        }

        #endregion
    }
}