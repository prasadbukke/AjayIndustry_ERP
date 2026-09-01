/*
=============================================================
File: SupplierOutstandingController.cs
Module: Supplier Outstanding / Payables
Layer: Web - Controller

Purpose:
Handles Supplier Outstanding read-only report UI.

Architecture:
Controller
    ↓
ISupplierOutstandingService
    ↓
SupplierOutstandingService
    ↓
ISupplierOutstandingRepository
    ↓
SupplierOutstandingRepository

Important:
- No direct ApplicationDbContext access.
- No Create / Edit / Delete actions.
- No Entity / Table / Migration.
- Controller only:
    - accepts filters
    - calls Application Service
    - prepares dropdowns
    - maps service result to Web ViewModel
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Web.ViewModels.SupplierOutstanding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class SupplierOutstandingController : Controller
    {
        #region Fields

        private readonly ISupplierOutstandingService _service;

        #endregion


        #region Constructor

        public SupplierOutstandingController(
            ISupplierOutstandingService service)
        {
            _service = service;
        }

        #endregion


        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int? supplierId,
            string? paymentStatus,
            string? dueStatus,
            DateTime? dueDateFrom,
            DateTime? dueDateTo,
            int pageNumber = 1,
            int pageSize = 10)
        {
            #region Build Service Filter

            var filter =
                new SupplierOutstandingFilter
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

                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize
                };

            #endregion


            #region Load Report

            var report =
                await _service.GetReportAsync(
                    filter);

            #endregion


            #region Load Supplier Options

            var supplierOptions =
                await _service
                    .GetSupplierOptionsAsync();

            #endregion


            #region Map Report Rows

            var rows =
                report.Results.Items
                    .Select(row =>
                        new SupplierOutstandingRowViewModel
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
                                row.PaidAmount,

                            OutstandingAmount =
                                row.OutstandingAmount,

                            PaymentStatus =
                                row.PaymentStatus,

                            DueStatus =
                                row.DueStatus,

                            OverdueDays =
                                row.OverdueDays
                        })
                    .ToList();

            #endregion


            #region Build Web Paged Result

            /*
             * TotalPages / HasPrevious / HasNext
             * are calculated automatically by PagedResult.
             *
             * Do NOT assign them manually.
             */

            var pagedResult =
                new PagedResult<
                    SupplierOutstandingRowViewModel>
                {
                    Items =
                        rows,

                    PageNumber =
                        report.Results.PageNumber,

                    PageSize =
                        report.Results.PageSize,

                    TotalRecords =
                        report.Results.TotalRecords
                };

            #endregion


            #region Build Supplier Dropdown

            var suppliers =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = string.Empty,
                        Text = "All Suppliers",
                        Selected =
                            !report.SupplierId.HasValue
                    }
                };


            suppliers.AddRange(
                supplierOptions
                    .OrderBy(option =>
                        option.SupplierName)
                    .Select(option =>
                        new SelectListItem
                        {
                            Value =
                                option.SupplierId
                                    .ToString(),

                            Text =
                                option.SupplierName,

                            Selected =
                                report.SupplierId.HasValue &&
                                report.SupplierId.Value ==
                                    option.SupplierId
                        }));

            #endregion


            #region Build Payment Status Dropdown

            var paymentStatuses =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = string.Empty,
                        Text = "Outstanding Only",
                        Selected =
                            string.IsNullOrWhiteSpace(
                                report.PaymentStatus)
                    },

                    new()
                    {
                        Value = "All",
                        Text = "All",
                        Selected =
                            string.Equals(
                                report.PaymentStatus,
                                "All",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value = "Pending",
                        Text = "Pending",
                        Selected =
                            string.Equals(
                                report.PaymentStatus,
                                "Pending",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value = "Partially Paid",
                        Text = "Partially Paid",
                        Selected =
                            string.Equals(
                                report.PaymentStatus,
                                "Partially Paid",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value = "Completed",
                        Text = "Completed",
                        Selected =
                            string.Equals(
                                report.PaymentStatus,
                                "Completed",
                                StringComparison.OrdinalIgnoreCase)
                    }
                };

            #endregion


            #region Build Due Status Dropdown

            var dueStatuses =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = string.Empty,
                        Text = "All Due Dates",
                        Selected =
                            string.IsNullOrWhiteSpace(
                                report.DueStatus)
                    },

                    new()
                    {
                        Value = "Overdue",
                        Text = "Overdue",
                        Selected =
                            string.Equals(
                                report.DueStatus,
                                "Overdue",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value = "Due Soon",
                        Text = "Due Soon",
                        Selected =
                            string.Equals(
                                report.DueStatus,
                                "Due Soon",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value = "Upcoming",
                        Text = "Upcoming",
                        Selected =
                            string.Equals(
                                report.DueStatus,
                                "Upcoming",
                                StringComparison.OrdinalIgnoreCase)
                    }
                };

            #endregion


            #region Build ViewModel

            var viewModel =
                new SupplierOutstandingIndexViewModel
                {
                    SearchText =
                        report.SearchText,

                    SupplierId =
                        report.SupplierId,

                    PaymentStatus =
                        report.PaymentStatus,

                    DueStatus =
                        report.DueStatus,

                    DueDateFrom =
                        report.DueDateFrom,

                    DueDateTo =
                        report.DueDateTo,

                    Suppliers =
                        suppliers,

                    PaymentStatuses =
                        paymentStatuses,

                    DueStatuses =
                        dueStatuses,

                    Results =
                        pagedResult
                };

            #endregion


            return View(viewModel);
        }

        #endregion
    }
}