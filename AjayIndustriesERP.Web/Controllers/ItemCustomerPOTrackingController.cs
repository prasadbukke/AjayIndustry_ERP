/*
=============================================================
File: ItemCustomerPOTrackingController.cs
Module: Item Customer PO Tracking
Layer: Web - Controller

Purpose:
Handles read-only Item-wise Customer PO Tracking.

Tracking Information:
- Customer PO
- Customer
- PO Date
- Item
- Drawing
- Ordered Quantity
- Delivery Date
- Priority
- PO Status

Production Tracking:

1. Production Job Progress
   Example:
   5 / 10 Completed

2. Production PO Status
   Pending
   In Progress
   Completed

Filters:
- Item
- Customer PO Number
- Customer
- Priority
- Customer PO Status
- Production PO Status
- PO Date From / To

Architecture:
Controller
    ↓
IItemCustomerPOTrackingService
    ↓
Service
    ↓
Repository
    ↓
DbContext

Important:
- No direct DbContext access.
- No Entity.
- No Table.
- No Migration.
- Read-only module.
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.ItemCustomerPOTracking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class ItemCustomerPOTrackingController : Controller
    {
        #region Fields

        private readonly IItemCustomerPOTrackingService
            _service;

        #endregion


        #region Constructor

        public ItemCustomerPOTrackingController(
            IItemCustomerPOTrackingService service)
        {
            _service = service;
        }

        #endregion


        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            int? itemId,
            string? itemSearchText,
            int? customerPurchaseOrderId,
            string? purchaseOrderNumber,
            int? customerId,
            string? priority,
            string? purchaseOrderStatus,
            string? productionPOStatus,
            DateTime? purchaseOrderDateFrom,
            DateTime? purchaseOrderDateTo,
            int pageNumber = 1,
            int pageSize = 10)
        {
            #region Build Filter

            var filter =
                new ItemCustomerPOTrackingFilter
                {
                    ItemId =
                        itemId,

                    ItemSearchText =
                        itemSearchText,

                    CustomerPurchaseOrderId =
                        customerPurchaseOrderId,

                    PurchaseOrderNumber =
                        purchaseOrderNumber,

                    CustomerId =
                        customerId,

                    Priority =
                        priority,

                    PurchaseOrderStatus =
                        purchaseOrderStatus,

                    ProductionPOStatus =
                        productionPOStatus,

                    PurchaseOrderDateFrom =
                        purchaseOrderDateFrom,

                    PurchaseOrderDateTo =
                        purchaseOrderDateTo,

                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize
                };

            #endregion


            #region Load Report

            var report =
                await _service
                    .GetReportAsync(
                        filter);

            #endregion


            #region Load Filter Options

            var customerOptions =
                await _service
                    .GetCustomerOptionsAsync();


            var purchaseOrderStatuses =
                await _service
                    .GetPurchaseOrderStatusesAsync();

            #endregion


            #region Map Result Rows

            var rows =
                report.Results.Items
                    .Select(row =>
                        new ItemCustomerPOTrackingRowViewModel
                        {
                            #region Customer PO

                            CustomerPurchaseOrderId =
                                row.CustomerPurchaseOrderId,

                            PurchaseOrderNumber =
                                row.PurchaseOrderNumber,

                            PurchaseOrderDate =
                                row.PurchaseOrderDate,

                            PurchaseOrderStatus =
                                row.PurchaseOrderStatus,

                            #endregion


                            #region Customer

                            CustomerId =
                                row.CustomerId,

                            CustomerName =
                                row.CustomerName,

                            #endregion


                            #region Item

                            ItemId =
                                row.ItemId,

                            ItemCode =
                                row.ItemCode,

                            ItemName =
                                row.ItemName,

                            #endregion


                            #region Drawing

                            DrawingId =
                                row.DrawingId,

                            DrawingNumber =
                                row.DrawingNumber,

                            DrawingFilePath =
                                row.DrawingFilePath,

                            #endregion


                            #region Quantity

                            OrderedQuantity =
                                row.OrderedQuantity,

                            #endregion


                            #region Delivery Date

                            DeliveryDate =
                                row.DeliveryDate,

                            #endregion


                            #region Priority

                            Priority =
                                row.Priority,

                            #endregion


                            #region Production Job Progress

                            TotalProductionJobs =
                                row.TotalProductionJobs,

                            CompletedProductionJobs =
                                row.CompletedProductionJobs,

                            #endregion


                            #region Production PO Status

                            ProductionPOStatus =
                                row.ProductionPOStatus

                            #endregion
                        })
                    .ToList();

            #endregion


            #region Build Paged Result

            var pagedResult =
                new PagedResult<
                    ItemCustomerPOTrackingRowViewModel>
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


            #region Customer Dropdown

            var customers =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            string.Empty,

                        Text =
                            "All Customers",

                        Selected =
                            !report.CustomerId.HasValue
                    }
                };


            customers.AddRange(
                customerOptions
                    .OrderBy(customer =>
                        customer.CustomerName)
                    .Select(customer =>
                        new SelectListItem
                        {
                            Value =
                                customer.CustomerId
                                    .ToString(),

                            Text =
                                customer.CustomerName,

                            Selected =
                                report.CustomerId.HasValue
                                &&
                                report.CustomerId.Value ==
                                    customer.CustomerId
                        }));

            #endregion


            #region Priority Dropdown

            /*
             * Priority options come directly from
             * CustomerPurchaseOrderPriority enum.
             *
             * Therefore Customer PO Entry and Tracking
             * use the same source.
             */

            var priorities =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            string.Empty,

                        Text =
                            "All Priorities",

                        Selected =
                            string.IsNullOrWhiteSpace(
                                report.Priority)
                    }
                };


            foreach (var priorityName in
                     Enum.GetNames<
                         CustomerPurchaseOrderPriority>())
            {
                priorities.Add(
                    new SelectListItem
                    {
                        Value =
                            priorityName,

                        Text =
                            priorityName,

                        Selected =
                            string.Equals(
                                report.Priority,
                                priorityName,
                                StringComparison.OrdinalIgnoreCase)
                    });
            }

            #endregion


            #region Customer PO Status Dropdown

            var statuses =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            string.Empty,

                        Text =
                            "Current",

                        Selected =
                            string.IsNullOrWhiteSpace(
                                report.PurchaseOrderStatus)
                            ||
                            string.Equals(
                                report.PurchaseOrderStatus,
                                "Current",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value =
                            "All",

                        Text =
                            "All",

                        Selected =
                            string.Equals(
                                report.PurchaseOrderStatus,
                                "All",
                                StringComparison.OrdinalIgnoreCase)
                    }
                };


            foreach (var status in
                     purchaseOrderStatuses)
            {
                if (string.IsNullOrWhiteSpace(
                    status))
                {
                    continue;
                }


                if (status.Equals(
                        "All",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    status.Equals(
                        "Current",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                statuses.Add(
                    new SelectListItem
                    {
                        Value =
                            status,

                        Text =
                            status,

                        Selected =
                            string.Equals(
                                report.PurchaseOrderStatus,
                                status,
                                StringComparison.OrdinalIgnoreCase)
                    });
            }

            #endregion


            #region Production PO Status Dropdown

            var productionPOStatuses =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            string.Empty,

                        Text =
                            "All Production PO Status",

                        Selected =
                            string.IsNullOrWhiteSpace(
                                report.ProductionPOStatus)
                    },

                    new()
                    {
                        Value =
                            "Pending",

                        Text =
                            "Pending",

                        Selected =
                            string.Equals(
                                report.ProductionPOStatus,
                                "Pending",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value =
                            "In Progress",

                        Text =
                            "In Progress",

                        Selected =
                            string.Equals(
                                report.ProductionPOStatus,
                                "In Progress",
                                StringComparison.OrdinalIgnoreCase)
                    },

                    new()
                    {
                        Value =
                            "Completed",

                        Text =
                            "Completed",

                        Selected =
                            string.Equals(
                                report.ProductionPOStatus,
                                "Completed",
                                StringComparison.OrdinalIgnoreCase)
                    }
                };

            #endregion


            #region Build ViewModel

            var viewModel =
                new ItemCustomerPOTrackingIndexViewModel
                {
                    #region Filters

                    ItemId =
                        report.ItemId,

                    ItemSearchText =
                        report.ItemSearchText,

                    CustomerPurchaseOrderId =
                        report.CustomerPurchaseOrderId,

                    PurchaseOrderNumber =
                        report.PurchaseOrderNumber,

                    CustomerId =
                        report.CustomerId,

                    Priority =
                        report.Priority,

                    PurchaseOrderStatus =
                        report.PurchaseOrderStatus,

                    ProductionPOStatus =
                        report.ProductionPOStatus,

                    PurchaseOrderDateFrom =
                        report.PurchaseOrderDateFrom,

                    PurchaseOrderDateTo =
                        report.PurchaseOrderDateTo,

                    #endregion


                    #region Dropdowns

                    Customers =
                        customers,

                    Priorities =
                        priorities,

                    PurchaseOrderStatuses =
                        statuses,

                    ProductionPOStatuses =
                        productionPOStatuses,

                    #endregion


                    #region Main Summary

                    TotalPurchaseOrders =
                        report.TotalPurchaseOrders,

                    TotalOrderedQuantity =
                        report.TotalOrderedQuantity,

                    #endregion


                    #region Priority Summary

                    CriticalPriorityPurchaseOrders =
                        report.CriticalPriorityPurchaseOrders,

                    UrgentPurchaseOrders =
                        report.UrgentPurchaseOrders,

                    HighPriorityPurchaseOrders =
                        report.HighPriorityPurchaseOrders,

                    NormalPriorityPurchaseOrders =
                        report.NormalPriorityPurchaseOrders,

                    #endregion


                    #region Production PO Summary

                    ProductionPendingPurchaseOrders =
                        report.ProductionPendingPurchaseOrders,

                    ProductionInProgressPurchaseOrders =
                        report.ProductionInProgressPurchaseOrders,

                    ProductionCompletedPurchaseOrders =
                        report.ProductionCompletedPurchaseOrders,

                    #endregion


                    #region Results

                    Results =
                        pagedResult

                    #endregion
                };

            #endregion


            return View(
                viewModel);
        }

        #endregion


        #region Item Autocomplete

        [HttpGet]
        public async Task<IActionResult> SearchItems(
            string? term)
        {
            var suggestions =
                await _service
                    .SearchItemsAsync(
                        term);


            var result =
                suggestions
                    .Select(item =>
                        new
                        {
                            id =
                                item.ItemId,

                            itemCode =
                                item.ItemCode,

                            itemName =
                                item.ItemName,

                            text =
                                item.DisplayText
                        })
                    .ToList();


            return Json(
                result);
        }

        #endregion


        #region Customer PO Autocomplete

        [HttpGet]
        public async Task<IActionResult>
            SearchPurchaseOrders(
                string? term)
        {
            var suggestions =
                await _service
                    .SearchPurchaseOrdersAsync(
                        term);


            var result =
                suggestions
                    .Select(po =>
                        new
                        {
                            id =
                                po.CustomerPurchaseOrderId,

                            purchaseOrderNumber =
                                po.PurchaseOrderNumber,

                            customerName =
                                po.CustomerName,

                            text =
                                po.DisplayText
                        })
                    .ToList();


            return Json(
                result);
        }

        #endregion


        #region Export Excel

        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            int? itemId,
            string? itemSearchText,
            int? customerPurchaseOrderId,
            string? purchaseOrderNumber,
            int? customerId,
            string? priority,
            string? purchaseOrderStatus,
            string? productionPOStatus,
            DateTime? purchaseOrderDateFrom,
            DateTime? purchaseOrderDateTo)
        {
            #region Build Filter

            var filter =
                new ItemCustomerPOTrackingFilter
                {
                    ItemId =
                        itemId,

                    ItemSearchText =
                        itemSearchText,

                    CustomerPurchaseOrderId =
                        customerPurchaseOrderId,

                    PurchaseOrderNumber =
                        purchaseOrderNumber,

                    CustomerId =
                        customerId,

                    Priority =
                        priority,

                    PurchaseOrderStatus =
                        purchaseOrderStatus,

                    ProductionPOStatus =
                        productionPOStatus,

                    PurchaseOrderDateFrom =
                        purchaseOrderDateFrom,

                    PurchaseOrderDateTo =
                        purchaseOrderDateTo,

                    PageNumber =
                        1,

                    PageSize =
                        10
                };

            #endregion


            #region Load ALL Matching Rows

            /*
             * Export method receives ALL matching rows.
             * It is not limited to current page.
             *
             * Actual XLSX generation will replace
             * this temporary JSON response later.
             */

            var rows =
                await _service
                    .GetExportRowsAsync(
                        filter);

            #endregion


            return Json(
                new
                {
                    success =
                        true,

                    totalRecords =
                        rows.Count
                });
        }

        #endregion
    }
}