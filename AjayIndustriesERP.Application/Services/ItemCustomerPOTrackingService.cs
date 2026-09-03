/*
=============================================================
File: ItemCustomerPOTrackingService.cs
Module: Item Customer PO Tracking
Layer: Application - Service

Purpose:
Provides business logic for Item-wise Customer PO Tracking.

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

Architecture:
Controller
    ↓
IItemCustomerPOTrackingService
    ↓
ItemCustomerPOTrackingService
    ↓
IItemCustomerPOTrackingRepository
    ↓
Infrastructure Repository

Important:
- Production Job Status and Production PO Status
  are kept separate.
- Read-only module.
- No DbContext access in Service.
- No Entity.
- No Table.
- No Migration.
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;

namespace AjayIndustriesERP.Application.Services
{
    public class ItemCustomerPOTrackingService
        : IItemCustomerPOTrackingService
    {
        #region Constants

        private const int AutocompleteMinimumCharacters = 2;

        private const int AutocompleteMaxResults = 10;

        #endregion


        #region Fields

        private readonly IItemCustomerPOTrackingRepository
            _repository;

        #endregion


        #region Constructor

        public ItemCustomerPOTrackingService(
            IItemCustomerPOTrackingRepository repository)
        {
            _repository = repository;
        }

        #endregion


        #region Get Report

        public async Task<ItemCustomerPOTrackingResult>
            GetReportAsync(
                ItemCustomerPOTrackingFilter filter)
        {
            #region Normalize Filter

            filter =
                NormalizeFilter(
                    filter);

            #endregion


            #region Build Repository Filter

            var repositoryFilter =
                BuildRepositoryFilter(
                    filter);

            #endregion


            #region Load Report Data

            var pagedRows =
                await _repository
                    .GetPagedAsync(
                        repositoryFilter);


            var summary =
                await _repository
                    .GetSummaryAsync(
                        repositoryFilter);

            #endregion


            #region Map Result Rows

            var mappedRows =
                pagedRows.Items
                    .Select(MapRow)
                    .ToList();

            #endregion


            #region Build Paged Result

            var resultRows =
                new PagedResult<
                    ItemCustomerPOTrackingResultRow>
                {
                    Items =
                        mappedRows,

                    PageNumber =
                        pagedRows.PageNumber,

                    PageSize =
                        pagedRows.PageSize,

                    TotalRecords =
                        pagedRows.TotalRecords
                };

            #endregion


            #region Build Final Result

            return new
                ItemCustomerPOTrackingResult
            {
                #region Filters

                ItemId =
                        filter.ItemId,

                ItemSearchText =
                        filter.ItemSearchText,

                CustomerPurchaseOrderId =
                        filter.CustomerPurchaseOrderId,

                PurchaseOrderNumber =
                        filter.PurchaseOrderNumber,

                CustomerId =
                        filter.CustomerId,

                Priority =
                        filter.Priority,

                PurchaseOrderStatus =
                        filter.PurchaseOrderStatus,

                ProductionPOStatus =
                        filter.ProductionPOStatus,

                PurchaseOrderDateFrom =
                        filter.PurchaseOrderDateFrom,

                PurchaseOrderDateTo =
                        filter.PurchaseOrderDateTo,

                #endregion


                #region Main Summary

                TotalPurchaseOrders =
                        summary.TotalPurchaseOrders,

                TotalOrderedQuantity =
                        summary.TotalOrderedQuantity,

                #endregion


                #region Priority Summary

                CriticalPriorityPurchaseOrders =
                        summary.CriticalPriorityPurchaseOrders,

                UrgentPurchaseOrders =
                        summary.UrgentPurchaseOrders,

                HighPriorityPurchaseOrders =
                        summary.HighPriorityPurchaseOrders,

                NormalPriorityPurchaseOrders =
                        summary.NormalPriorityPurchaseOrders,

                #endregion


                #region Production PO Summary

                ProductionPendingPurchaseOrders =
                        summary.ProductionPendingPurchaseOrders,

                ProductionInProgressPurchaseOrders =
                        summary.ProductionInProgressPurchaseOrders,

                ProductionCompletedPurchaseOrders =
                        summary.ProductionCompletedPurchaseOrders,

                #endregion


                #region Results

                Results =
                        resultRows

                #endregion
            };

            #endregion
        }

        #endregion


        #region Get Export Rows

        public async Task<
            List<ItemCustomerPOTrackingResultRow>>
            GetExportRowsAsync(
                ItemCustomerPOTrackingFilter filter)
        {
            #region Normalize Filter

            filter =
                NormalizeFilter(
                    filter);

            #endregion


            #region Build Repository Filter

            var repositoryFilter =
                BuildRepositoryFilter(
                    filter);

            #endregion


            #region Load All Matching Rows

            /*
             * Repository Export method is not paginated.
             *
             * Excel Export therefore receives ALL rows
             * matching current filters.
             */

            var rows =
                await _repository
                    .GetExportRowsAsync(
                        repositoryFilter);

            #endregion


            #region Map Rows

            return rows
                .Select(MapRow)
                .ToList();

            #endregion
        }

        #endregion


        #region Search Items

        public async Task<
            List<ItemCustomerPOTrackingItemSuggestionResult>>
            SearchItemsAsync(
                string? searchText)
        {
            #region Normalize Search

            searchText =
                CleanText(
                    searchText);

            #endregion


            #region Minimum Length

            if (string.IsNullOrWhiteSpace(
                    searchText) ||
                searchText.Length <
                    AutocompleteMinimumCharacters)
            {
                return new();
            }

            #endregion


            #region Repository Search

            var items =
                await _repository
                    .SearchItemsAsync(
                        searchText,
                        AutocompleteMaxResults);

            #endregion


            #region Map Suggestions

            return items
                .Select(item =>
                    new
                    ItemCustomerPOTrackingItemSuggestionResult
                    {
                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        DisplayText =
                            BuildItemDisplayText(
                                item.ItemName,
                                item.ItemCode)
                    })
                .ToList();

            #endregion
        }

        #endregion


        #region Search Customer Purchase Orders

        public async Task<
            List<ItemCustomerPOTrackingPOSuggestionResult>>
            SearchPurchaseOrdersAsync(
                string? searchText)
        {
            #region Normalize Search

            searchText =
                CleanText(
                    searchText);

            #endregion


            #region Minimum Length

            if (string.IsNullOrWhiteSpace(
                    searchText) ||
                searchText.Length <
                    AutocompleteMinimumCharacters)
            {
                return new();
            }

            #endregion


            #region Repository Search

            var purchaseOrders =
                await _repository
                    .SearchPurchaseOrdersAsync(
                        searchText,
                        AutocompleteMaxResults);

            #endregion


            #region Map Suggestions

            return purchaseOrders
                .Select(po =>
                    new
                    ItemCustomerPOTrackingPOSuggestionResult
                    {
                        CustomerPurchaseOrderId =
                            po.CustomerPurchaseOrderId,

                        PurchaseOrderNumber =
                            po.PurchaseOrderNumber,

                        CustomerName =
                            po.CustomerName,

                        DisplayText =
                            BuildPurchaseOrderDisplayText(
                                po.PurchaseOrderNumber,
                                po.CustomerName)
                    })
                .ToList();

            #endregion
        }

        #endregion


        #region Get Customer Options

        public async Task<
            List<ItemCustomerPOTrackingCustomerOptionResult>>
            GetCustomerOptionsAsync()
        {
            var customers =
                await _repository
                    .GetCustomerOptionsAsync();


            return customers
                .Select(customer =>
                    new
                    ItemCustomerPOTrackingCustomerOptionResult
                    {
                        CustomerId =
                            customer.CustomerId,

                        CustomerName =
                            customer.CustomerName
                    })
                .OrderBy(customer =>
                    customer.CustomerName)
                .ToList();
        }

        #endregion


        #region Get Customer PO Statuses

        public async Task<List<string>>
            GetPurchaseOrderStatusesAsync()
        {
            var statuses =
                await _repository
                    .GetPurchaseOrderStatusesAsync();


            return statuses
                .Where(status =>
                    !string.IsNullOrWhiteSpace(
                        status))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(status =>
                    status)
                .ToList();
        }

        #endregion


        #region Build Repository Filter

        private static
            ItemCustomerPOTrackingRepositoryFilter
            BuildRepositoryFilter(
                ItemCustomerPOTrackingFilter filter)
        {
            return new
                ItemCustomerPOTrackingRepositoryFilter
            {
                #region Item

                ItemId =
                        filter.ItemId,

                ItemSearchText =
                        filter.ItemSearchText,

                #endregion


                #region Customer PO

                CustomerPurchaseOrderId =
                        filter.CustomerPurchaseOrderId,

                PurchaseOrderNumber =
                        filter.PurchaseOrderNumber,

                #endregion


                #region Customer

                CustomerId =
                        filter.CustomerId,

                #endregion


                #region Priority

                Priority =
                        filter.Priority,

                #endregion


                #region Customer PO Status

                PurchaseOrderStatus =
                        filter.PurchaseOrderStatus,

                #endregion


                #region Production PO Status

                ProductionPOStatus =
                        filter.ProductionPOStatus,

                #endregion


                #region Dates

                PurchaseOrderDateFrom =
                        filter.PurchaseOrderDateFrom,

                PurchaseOrderDateTo =
                        filter.PurchaseOrderDateTo,

                #endregion


                #region Pagination

                PageNumber =
                        filter.PageNumber,

                PageSize =
                        filter.PageSize

                #endregion
            };
        }

        #endregion


        #region Map Repository Row

        private static
            ItemCustomerPOTrackingResultRow
            MapRow(
                ItemCustomerPOTrackingRepositoryRow row)
        {
            return new
                ItemCustomerPOTrackingResultRow
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
            };
        }

        #endregion


        #region Normalize Filter

        private static
            ItemCustomerPOTrackingFilter
            NormalizeFilter(
                ItemCustomerPOTrackingFilter? filter)
        {
            filter ??=
                new ItemCustomerPOTrackingFilter();


            #region IDs

            if (filter.ItemId.HasValue &&
                filter.ItemId.Value <= 0)
            {
                filter.ItemId = null;
            }


            if (filter.CustomerPurchaseOrderId.HasValue &&
                filter.CustomerPurchaseOrderId.Value <= 0)
            {
                filter.CustomerPurchaseOrderId = null;
            }


            if (filter.CustomerId.HasValue &&
                filter.CustomerId.Value <= 0)
            {
                filter.CustomerId = null;
            }

            #endregion


            #region Text Filters

            filter.ItemSearchText =
                CleanText(
                    filter.ItemSearchText);


            filter.PurchaseOrderNumber =
                CleanText(
                    filter.PurchaseOrderNumber);


            filter.Priority =
                CleanText(
                    filter.Priority);


            filter.PurchaseOrderStatus =
                CleanText(
                    filter.PurchaseOrderStatus);


            filter.ProductionPOStatus =
                NormalizeProductionPOStatus(
                    filter.ProductionPOStatus);

            #endregion


            #region PO Date Range

            if (filter.PurchaseOrderDateFrom.HasValue)
            {
                filter.PurchaseOrderDateFrom =
                    filter
                        .PurchaseOrderDateFrom
                        .Value
                        .Date;
            }


            if (filter.PurchaseOrderDateTo.HasValue)
            {
                filter.PurchaseOrderDateTo =
                    filter
                        .PurchaseOrderDateTo
                        .Value
                        .Date;
            }


            if (filter.PurchaseOrderDateFrom.HasValue &&
                filter.PurchaseOrderDateTo.HasValue &&
                filter.PurchaseOrderDateFrom.Value >
                filter.PurchaseOrderDateTo.Value)
            {
                var temporaryDate =
                    filter.PurchaseOrderDateFrom;


                filter.PurchaseOrderDateFrom =
                    filter.PurchaseOrderDateTo;


                filter.PurchaseOrderDateTo =
                    temporaryDate;
            }

            #endregion


            #region Pagination

            if (filter.PageNumber < 1)
            {
                filter.PageNumber = 1;
            }


            filter.PageSize =
                NormalizePageSize(
                    filter.PageSize);

            #endregion


            return filter;
        }

        #endregion


        #region Normalize Production PO Status

        private static string?
            NormalizeProductionPOStatus(
                string? status)
        {
            status =
                CleanText(
                    status);


            if (string.IsNullOrWhiteSpace(
                status))
            {
                return null;
            }


            if (status.Equals(
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Pending";
            }


            if (status.Equals(
                    "In Progress",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "InProgress",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "In Progress";
            }


            if (status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase)
                ||
                status.Equals(
                    "Complete",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Completed";
            }


            return null;
        }

        #endregion


        #region Normalize Page Size

        private static int NormalizePageSize(
            int pageSize)
        {
            return pageSize switch
            {
                10 => 10,
                25 => 25,
                50 => 50,
                100 => 100,
                _ => 10
            };
        }

        #endregion


        #region Clean Text

        private static string? CleanText(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return null;
            }


            return value.Trim();
        }

        #endregion


        #region Build Item Display Text

        private static string BuildItemDisplayText(
            string itemName,
            string itemCode)
        {
            if (string.IsNullOrWhiteSpace(
                itemCode))
            {
                return itemName;
            }


            if (string.IsNullOrWhiteSpace(
                itemName))
            {
                return itemCode;
            }


            return
                $"{itemName} - {itemCode}";
        }

        #endregion


        #region Build Customer PO Display Text

        private static string
            BuildPurchaseOrderDisplayText(
                string purchaseOrderNumber,
                string customerName)
        {
            if (string.IsNullOrWhiteSpace(
                customerName))
            {
                return purchaseOrderNumber;
            }


            return
                $"{purchaseOrderNumber} - {customerName}";
        }

        #endregion
    }
}