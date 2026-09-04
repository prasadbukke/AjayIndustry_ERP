/*
=============================================================
File: ItemCustomerPOTrackingRepository.cs
Module: Item Customer PO Tracking
Layer: Infrastructure - Repository

Purpose:
Provides read-only Item-wise Customer PO Tracking.

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

Production Tracking is separated into:

1. Production Job Progress
   Example:
   1 / 1 Completed

2. Production PO Status
   Pending
   In Progress
   Completed

Production PO Status Rule:

Pending:
- No active non-cancelled Production Jobs exist
  OR
- Production Jobs exist but none are
  InProgress / Completed.

In Progress:
- At least one Production Job has started
  or completed,
  BUT all Production Jobs are not Completed.

Completed:
- ALL active non-cancelled Production Jobs
  belonging to the complete Customer PO
  are Completed.

Current Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Items

Important:
- ProductionJob directly stores CustomerPurchaseOrderId.
- Production PO calculation is CUSTOMER PO LEVEL.
- It is NOT Customer PO Item level.
- Item filter does not reduce Production Job count.
- Cancelled Production Jobs are ignored.
- Read-only module.
- No Entity.
- No Table.
- No Migration.
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class ItemCustomerPOTrackingRepository
        : IItemCustomerPOTrackingRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public ItemCustomerPOTrackingRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Get Paged

        public async Task<
            PagedResult<ItemCustomerPOTrackingRepositoryRow>>
            GetPagedAsync(
                ItemCustomerPOTrackingRepositoryFilter filter)
        {
            var rows =
                await GetFilteredRowsAsync(
                    filter);


            var pageNumber =
                filter.PageNumber < 1
                    ? 1
                    : filter.PageNumber;


            var pageSize =
                filter.PageSize <= 0
                    ? 10
                    : filter.PageSize;


            var totalRecords =
                rows.Count;


            var pagedRows =
                rows
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToList();


            return new
                PagedResult<ItemCustomerPOTrackingRepositoryRow>
            {
                Items =
                    pagedRows,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Get Export Rows

        public async Task<
            List<ItemCustomerPOTrackingRepositoryRow>>
            GetExportRowsAsync(
                ItemCustomerPOTrackingRepositoryFilter filter)
        {
            /*
             * Export is not paginated.
             *
             * ALL matching rows are returned.
             */

            return await
                GetFilteredRowsAsync(
                    filter);
        }

        #endregion


        #region Get Summary

        public async Task<
            ItemCustomerPOTrackingRepositorySummary>
            GetSummaryAsync(
                ItemCustomerPOTrackingRepositoryFilter filter)
        {
            var rows =
                await GetFilteredRowsAsync(
                    filter);


            #region Distinct Customer POs

            var poGroups =
                rows
                    .GroupBy(row =>
                        row.CustomerPurchaseOrderId)
                    .ToList();

            #endregion


            #region Highest Matching Priority Per PO

            var poPriorities =
                poGroups
                    .Select(group =>
                        group
                            .OrderBy(row =>
                                GetPriorityRank(
                                    row.Priority))
                            .Select(row =>
                                row.Priority)
                            .FirstOrDefault()
                        ?? nameof(
                            CustomerPurchaseOrderPriority.Normal))
                    .ToList();

            #endregion


            #region Production PO Status Per PO

            /*
             * Every tracking row belonging to the same
             * Customer PO already carries the SAME
             * PO-level Production status.
             *
             * Therefore take one status per PO.
             */

            var poProductionStatuses =
                poGroups
                    .Select(group =>
                        group
                            .Select(row =>
                                row.ProductionPOStatus)
                            .FirstOrDefault()
                        ?? "Pending")
                    .ToList();

            #endregion


            return new
                ItemCustomerPOTrackingRepositorySummary
            {
                #region Main Summary

                TotalPurchaseOrders =
                    poGroups.Count,

                TotalOrderedQuantity =
                    rows.Sum(row =>
                        row.OrderedQuantity),

                #endregion


                #region Priority Summary

                CriticalPriorityPurchaseOrders =
                    poPriorities.Count(priority =>
                        PriorityEquals(
                            priority,
                            CustomerPurchaseOrderPriority.Critical)),

                UrgentPurchaseOrders =
                    poPriorities.Count(priority =>
                        PriorityEquals(
                            priority,
                            CustomerPurchaseOrderPriority.Urgent)),

                HighPriorityPurchaseOrders =
                    poPriorities.Count(priority =>
                        PriorityEquals(
                            priority,
                            CustomerPurchaseOrderPriority.High)),

                NormalPriorityPurchaseOrders =
                    poPriorities.Count(priority =>
                        PriorityEquals(
                            priority,
                            CustomerPurchaseOrderPriority.Normal)),

                #endregion


                #region Production PO Summary

                ProductionPendingPurchaseOrders =
                    poProductionStatuses.Count(status =>
                        string.Equals(
                            status,
                            "Pending",
                            StringComparison.OrdinalIgnoreCase)),

                ProductionInProgressPurchaseOrders =
                    poProductionStatuses.Count(status =>
                        string.Equals(
                            status,
                            "In Progress",
                            StringComparison.OrdinalIgnoreCase)),

                ProductionCompletedPurchaseOrders =
                    poProductionStatuses.Count(status =>
                        string.Equals(
                            status,
                            "Completed",
                            StringComparison.OrdinalIgnoreCase))

                #endregion
            };
        }

        #endregion


        #region Search Items

        public async Task<
            List<ItemCustomerPOTrackingItemSuggestion>>
            SearchItemsAsync(
                string searchText,
                int maxResults = 10)
        {
            #region Validation

            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return new();
            }


            searchText =
                searchText.Trim();


            if (maxResults <= 0)
            {
                maxResults = 10;
            }

            #endregion


            #region Query

            var items =
                await _context
                    .CustomerPurchaseOrderItems
                    .AsNoTracking()
                    .Where(item =>
                        !item.IsDeleted &&
                        item.IsActive &&

                        !item.CustomerPurchaseOrder
                            .IsDeleted &&

                        item.CustomerPurchaseOrder
                            .IsActive &&

                        (
                            item.ItemName
                                .Contains(searchText)
                            ||
                            item.ItemCode
                                .Contains(searchText)
                        ))
                    .Select(item =>
                        new
                        {
                            item.ItemId,
                            item.ItemCode,
                            item.ItemName
                        })
                    .Distinct()
                    .OrderBy(item =>
                        item.ItemName)
                    .ThenBy(item =>
                        item.ItemCode)
                    .Take(maxResults)
                    .ToListAsync();

            #endregion


            return items
                .Select(item =>
                    new
                    ItemCustomerPOTrackingItemSuggestion
                    {
                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName
                    })
                .ToList();
        }

        #endregion


        #region Search Customer Purchase Orders

        public async Task<
            List<ItemCustomerPOTrackingPOSuggestion>>
            SearchPurchaseOrdersAsync(
                string searchText,
                int maxResults = 10)
        {
            #region Validation

            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return new();
            }


            searchText =
                searchText.Trim();


            if (maxResults <= 0)
            {
                maxResults = 10;
            }

            #endregion


            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(po =>
                    !po.IsDeleted &&
                    po.IsActive &&

                    po.CustomerPurchaseOrderNumber
                        .Contains(searchText))
                .OrderByDescending(po =>
                    po.CustomerPurchaseOrderDate)
                .ThenByDescending(po =>
                    po.Id)
                .Select(po =>
                    new
                    ItemCustomerPOTrackingPOSuggestion
                    {
                        CustomerPurchaseOrderId =
                            po.Id,

                        PurchaseOrderNumber =
                            po.CustomerPurchaseOrderNumber,

                        CustomerName =
                            po.CustomerName
                    })
                .Take(maxResults)
                .ToListAsync();
        }

        #endregion


        #region Get Customer Options

        public async Task<
            List<ItemCustomerPOTrackingCustomerOption>>
            GetCustomerOptionsAsync()
        {
            var customers =
                await _context
                    .CustomerPurchaseOrders
                    .AsNoTracking()
                    .Where(po =>
                        !po.IsDeleted &&
                        po.IsActive)
                    .Select(po =>
                        new
                        {
                            po.CustomerId,
                            po.CustomerName
                        })
                    .Distinct()
                    .OrderBy(customer =>
                        customer.CustomerName)
                    .ToListAsync();


            return customers
                .Select(customer =>
                    new
                    ItemCustomerPOTrackingCustomerOption
                    {
                        CustomerId =
                            customer.CustomerId,

                        CustomerName =
                            customer.CustomerName
                    })
                .ToList();
        }

        #endregion


        #region Get Customer PO Statuses

        public Task<List<string>>
            GetPurchaseOrderStatusesAsync()
        {
            var statuses =
                Enum
                    .GetNames<CustomerPurchaseOrderStatus>()
                    .ToList();


            return Task.FromResult(
                statuses);
        }

        #endregion


        #region Get Filtered Rows

        private async Task<
            List<ItemCustomerPOTrackingRepositoryRow>>
            GetFilteredRowsAsync(
                ItemCustomerPOTrackingRepositoryFilter filter)
        {
            #region Base Customer PO Item Query

            var query =
                _context
                    .CustomerPurchaseOrderItems
                    .AsNoTracking()
                    .Where(item =>
                        !item.IsDeleted &&
                        item.IsActive &&

                        !item.CustomerPurchaseOrder
                            .IsDeleted &&

                        item.CustomerPurchaseOrder
                            .IsActive);

            #endregion


            #region Item Filter

            if (filter.ItemId.HasValue &&
                filter.ItemId.Value > 0)
            {
                var itemId =
                    filter.ItemId.Value;


                query =
                    query.Where(item =>
                        item.ItemId ==
                            itemId);
            }
            else if (!string.IsNullOrWhiteSpace(
                filter.ItemSearchText))
            {
                var itemSearch =
                    filter.ItemSearchText.Trim();


                query =
                    query.Where(item =>
                        item.ItemName
                            .Contains(itemSearch)
                        ||
                        item.ItemCode
                            .Contains(itemSearch));
            }

            #endregion


            #region Customer PO Filter

            if (filter.CustomerPurchaseOrderId.HasValue &&
                filter.CustomerPurchaseOrderId.Value > 0)
            {
                var customerPurchaseOrderId =
                    filter.CustomerPurchaseOrderId.Value;


                query =
                    query.Where(item =>
                        item.CustomerPurchaseOrderId ==
                            customerPurchaseOrderId);
            }
            else if (!string.IsNullOrWhiteSpace(
                filter.PurchaseOrderNumber))
            {
                var purchaseOrderNumber =
                    filter.PurchaseOrderNumber.Trim();


                query =
                    query.Where(item =>
                        item.CustomerPurchaseOrder
                            .CustomerPurchaseOrderNumber
                            .Contains(
                                purchaseOrderNumber));
            }

            #endregion


            #region Customer Filter

            if (filter.CustomerId.HasValue &&
                filter.CustomerId.Value > 0)
            {
                var customerId =
                    filter.CustomerId.Value;


                query =
                    query.Where(item =>
                        item.CustomerPurchaseOrder
                            .CustomerId ==
                                customerId);
            }

            #endregion


            #region Priority Filter

            if (!string.IsNullOrWhiteSpace(
                    filter.Priority) &&
                Enum.TryParse<
                    CustomerPurchaseOrderPriority>(
                    filter.Priority.Trim(),
                    true,
                    out var priorityFilter))
            {
                /*
                 * Same Priority source as Customer PO Entry.
                 *
                 * Item Priority when entered,
                 * otherwise Header Priority.
                 */

                query =
                    query.Where(item =>
                        (
                            item.Priority
                            ??
                            item.CustomerPurchaseOrder.Priority
                        )
                        ==
                        priorityFilter);
            }

            #endregion


            #region Customer PO Status Filter

            query =
                ApplyPurchaseOrderStatusFilter(
                    query,
                    filter.PurchaseOrderStatus);

            #endregion


            #region PO Date From

            if (filter.PurchaseOrderDateFrom.HasValue)
            {
                var fromDate =
                    filter
                        .PurchaseOrderDateFrom
                        .Value
                        .Date;


                query =
                    query.Where(item =>
                        item.CustomerPurchaseOrder
                            .CustomerPurchaseOrderDate >=
                                fromDate);
            }

            #endregion


            #region PO Date To

            if (filter.PurchaseOrderDateTo.HasValue)
            {
                var toDateExclusive =
                    filter
                        .PurchaseOrderDateTo
                        .Value
                        .Date
                        .AddDays(1);


                query =
                    query.Where(item =>
                        item.CustomerPurchaseOrder
                            .CustomerPurchaseOrderDate <
                                toDateExclusive);
            }

            #endregion


            #region Load Matching Tracking Rows

            /*
             * These are the rows that will finally
             * appear on screen after normal filters.
             *
             * IMPORTANT:
             * Production Job count is NOT calculated
             * only from these Item rows.
             */

            var rawRows =
                await query
                    .Select(item =>
                        new RawTrackingRow
                        {
                            CustomerPurchaseOrderItemId =
                                item.Id,

                            CustomerPurchaseOrderId =
                                item.CustomerPurchaseOrderId,

                            PurchaseOrderNumber =
                                item.CustomerPurchaseOrder
                                    .CustomerPurchaseOrderNumber,

                            PurchaseOrderDate =
                                item.CustomerPurchaseOrder
                                    .CustomerPurchaseOrderDate,

                            PurchaseOrderStatus =
                                item.CustomerPurchaseOrder
                                    .Status,

                            CustomerId =
                                item.CustomerPurchaseOrder
                                    .CustomerId,

                            CustomerName =
                                item.CustomerPurchaseOrder
                                    .CustomerName,

                            ItemId =
                                item.ItemId,

                            ItemCode =
                                item.ItemCode,

                            ItemName =
                                item.ItemName,

                            OrderedQuantity =
                                item.OrderedQuantity,

                            DeliveryDate =
                                item.RequiredDeliveryDate
                                ??
                                item.CustomerPurchaseOrder
                                    .RequiredDeliveryDate,

                            EffectivePriority =
                                item.Priority
                                ??
                                item.CustomerPurchaseOrder
                                    .Priority,

                            DrawingId =
                                item.Item
                                    .Drawings
                                    .Where(drawing =>
                                        !drawing.IsDeleted &&
                                        drawing.IsActive)
                                    .OrderByDescending(drawing =>
                                        drawing.DrawingId)
                                    .Select(drawing =>
                                        (int?)drawing.DrawingId)
                                    .FirstOrDefault(),

                            DrawingNumber =
                                item.Item
                                    .Drawings
                                    .Where(drawing =>
                                        !drawing.IsDeleted &&
                                        drawing.IsActive)
                                    .OrderByDescending(drawing =>
                                        drawing.DrawingId)
                                    .Select(drawing =>
                                        drawing.DrawingNumber)
                                    .FirstOrDefault(),

                            DrawingFilePath =
                                item.Item
                                    .Drawings
                                    .Where(drawing =>
                                        !drawing.IsDeleted &&
                                        drawing.IsActive)
                                    .OrderByDescending(drawing =>
                                        drawing.DrawingId)
                                    .Select(drawing =>
                                        drawing.FilePath)
                                    .FirstOrDefault()
                        })
                    .ToListAsync();

            #endregion


            #region No Matching Rows

            if (rawRows.Count == 0)
            {
                return new();
            }

            #endregion


            #region Distinct Matching Customer PO IDs

            var customerPurchaseOrderIds =
                rawRows
                    .Select(row =>
                        row.CustomerPurchaseOrderId)
                    .Distinct()
                    .ToList();

            #endregion


            #region Load ALL Production Jobs For Matching POs

            /*
             * CURRENT PRODUCTION ARCHITECTURE:
             *
             * CustomerPurchaseOrder
             *          ↓
             * ProductionJob
             *
             * ProductionJob directly stores
             * CustomerPurchaseOrderId.
             *
             * Therefore no Customer PO Item mapping
             * is required here.
             *
             * Item filter still does NOT reduce
             * Production Job count because we load Jobs
             * for every complete Customer PO represented
             * in rawRows.
             */

            var productionJobs =
                await _context
                    .ProductionJobs
                    .AsNoTracking()
                    .Where(job =>
                        customerPurchaseOrderIds
                            .Contains(
                                job.CustomerPurchaseOrderId)
                        &&
                        !job.IsDeleted
                        &&
                        job.IsActive
                        &&
                        job.Status !=
                            ProductionJobStatus.Cancelled)
                    .Select(job =>
                        new ProductionJobTrackingRow
                        {
                            CustomerPurchaseOrderId =
                                job.CustomerPurchaseOrderId,

                            Status =
                                job.Status
                        })
                    .ToListAsync();

            #endregion


            #region Group Production Jobs By Customer PO

            var productionJobsByPO =
                productionJobs
                    .GroupBy(job =>
                        job.CustomerPurchaseOrderId)
                    .ToDictionary(
                        group =>
                            group.Key,

                        group =>
                            group.ToList());

            #endregion


            #region Build PO Production Summary Lookup

            var productionSummaryByPO =
                new Dictionary<
                    int,
                    ProductionPOSummaryRow>();


            foreach (var customerPurchaseOrderId
                     in customerPurchaseOrderIds)
            {
                productionJobsByPO.TryGetValue(
                    customerPurchaseOrderId,
                    out var poJobs);


                poJobs ??=
                    new List<
                        ProductionJobTrackingRow>();


                productionSummaryByPO[
                    customerPurchaseOrderId] =
                        BuildProductionPOSummary(
                            poJobs);
            }

            #endregion


            #region Build Final Tracking Rows

            var rows =
                new List<
                    ItemCustomerPOTrackingRepositoryRow>();


            foreach (var rawRow in rawRows)
            {
                var productionSummary =
                    productionSummaryByPO[
                        rawRow.CustomerPurchaseOrderId];


                rows.Add(
                    new
                    ItemCustomerPOTrackingRepositoryRow
                    {
                        #region Customer PO

                        CustomerPurchaseOrderId =
                            rawRow.CustomerPurchaseOrderId,

                        PurchaseOrderNumber =
                            rawRow.PurchaseOrderNumber,

                        PurchaseOrderDate =
                            rawRow.PurchaseOrderDate,

                        PurchaseOrderStatus =
                            rawRow.PurchaseOrderStatus
                                .ToString(),

                        #endregion


                        #region Customer

                        CustomerId =
                            rawRow.CustomerId,

                        CustomerName =
                            rawRow.CustomerName,

                        #endregion


                        #region Item

                        ItemId =
                            rawRow.ItemId,

                        ItemCode =
                            rawRow.ItemCode,

                        ItemName =
                            rawRow.ItemName,

                        #endregion


                        #region Drawing

                        DrawingId =
                            rawRow.DrawingId,

                        DrawingNumber =
                            rawRow.DrawingNumber,

                        DrawingFilePath =
                            rawRow.DrawingFilePath,

                        #endregion


                        #region Quantity

                        OrderedQuantity =
                            rawRow.OrderedQuantity,

                        #endregion


                        #region Delivery Date

                        DeliveryDate =
                            rawRow.DeliveryDate,

                        #endregion


                        #region Priority

                        Priority =
                            rawRow
                                .EffectivePriority
                                .ToString(),

                        #endregion


                        #region Production Job Progress

                        TotalProductionJobs =
                            productionSummary
                                .TotalProductionJobs,

                        CompletedProductionJobs =
                            productionSummary
                                .CompletedProductionJobs,

                        #endregion


                        #region Production PO Status

                        ProductionPOStatus =
                            productionSummary
                                .ProductionPOStatus

                        #endregion
                    });
            }

            #endregion


            #region Production PO Status Filter

            if (!string.IsNullOrWhiteSpace(
                filter.ProductionPOStatus))
            {
                var productionPOStatus =
                    NormalizeProductionPOStatus(
                        filter.ProductionPOStatus);


                if (!string.IsNullOrWhiteSpace(
                    productionPOStatus))
                {
                    rows =
                        rows
                            .Where(row =>
                                row.ProductionPOStatus
                                    .Equals(
                                        productionPOStatus,
                                        StringComparison
                                            .OrdinalIgnoreCase))
                            .ToList();
                }
            }

            #endregion


            #region Sorting

            rows =
                rows
                    .OrderBy(row =>
                        GetPriorityRank(
                            row.Priority))
                    .ThenBy(row =>
                        row.PurchaseOrderDate)
                    .ThenBy(row =>
                        row.DeliveryDate)
                    .ThenBy(row =>
                        row.PurchaseOrderNumber)
                    .ThenBy(row =>
                        row.ItemName)
                    .ToList();

            #endregion


            return rows;
        }

        #endregion


        #region Build Production PO Summary

        private static ProductionPOSummaryRow
            BuildProductionPOSummary(
                List<ProductionJobTrackingRow> jobs)
        {
            #region Total Jobs

            var totalJobs =
                jobs.Count;

            #endregion


            #region Completed Jobs

            var completedJobs =
                jobs.Count(job =>
                    job.Status ==
                        ProductionJobStatus.Completed);

            #endregion


            #region No Jobs

            if (totalJobs == 0)
            {
                return new
                    ProductionPOSummaryRow
                {
                    TotalProductionJobs =
                        0,

                    CompletedProductionJobs =
                        0,

                    ProductionPOStatus =
                        "Pending"
                };
            }

            #endregion


            #region All Jobs Completed

            /*
             * This is the ONLY condition where
             * Production PO becomes Completed.
             */

            if (completedJobs ==
                totalJobs)
            {
                return new
                    ProductionPOSummaryRow
                {
                    TotalProductionJobs =
                        totalJobs,

                    CompletedProductionJobs =
                        completedJobs,

                    ProductionPOStatus =
                        "Completed"
                };
            }

            #endregion


            #region Production Has Started

            /*
             * Production is considered started when
             * at least one Job is:
             *
             * InProgress
             * OR
             * Completed
             */

            var hasStartedProduction =
                jobs.Any(job =>
                    job.Status ==
                        ProductionJobStatus.InProgress
                    ||
                    job.Status ==
                        ProductionJobStatus.Completed);

            #endregion


            #region In Progress

            if (hasStartedProduction)
            {
                return new
                    ProductionPOSummaryRow
                {
                    TotalProductionJobs =
                        totalJobs,

                    CompletedProductionJobs =
                        completedJobs,

                    ProductionPOStatus =
                        "In Progress"
                };
            }

            #endregion


            #region Pending

            /*
             * Jobs exist, but they are only
             * Draft / Ready.
             */

            return new
                ProductionPOSummaryRow
            {
                TotalProductionJobs =
                    totalJobs,

                CompletedProductionJobs =
                    completedJobs,

                ProductionPOStatus =
                    "Pending"
            };

            #endregion
        }

        #endregion


        #region Customer PO Status Filter Helper

        private static IQueryable<
            AjayIndustriesERP.Domain.Entities.CustomerPurchaseOrderItem>
            ApplyPurchaseOrderStatusFilter(
                IQueryable<
                    AjayIndustriesERP.Domain.Entities.CustomerPurchaseOrderItem>
                    query,
                string? status)
        {
            #region Default / Current

            if (string.IsNullOrWhiteSpace(
                    status)
                ||
                status.Equals(
                    "Current",
                    StringComparison.OrdinalIgnoreCase))
            {
                return query
                    .Where(item =>
                        item.CustomerPurchaseOrder.Status ==
                            CustomerPurchaseOrderStatus.Draft
                        ||
                        item.CustomerPurchaseOrder.Status ==
                            CustomerPurchaseOrderStatus.Confirmed);
            }

            #endregion


            #region All

            if (status.Equals(
                "All",
                StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            #endregion


            #region Exact Status

            if (Enum.TryParse<
                    CustomerPurchaseOrderStatus>(
                    status.Trim(),
                    true,
                    out var parsedStatus))
            {
                return query
                    .Where(item =>
                        item.CustomerPurchaseOrder.Status ==
                            parsedStatus);
            }

            #endregion


            return query;
        }

        #endregion


        #region Normalize Production PO Status

        private static string?
            NormalizeProductionPOStatus(
                string? status)
        {
            if (string.IsNullOrWhiteSpace(
                status))
            {
                return null;
            }


            status =
                status.Trim();


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


        #region Priority Rank

        private static int GetPriorityRank(
            string? priority)
        {
            if (!Enum.TryParse<
                    CustomerPurchaseOrderPriority>(
                    priority,
                    true,
                    out var parsedPriority))
            {
                return 99;
            }


            return parsedPriority switch
            {
                CustomerPurchaseOrderPriority.Critical
                    => 1,

                CustomerPurchaseOrderPriority.Urgent
                    => 2,

                CustomerPurchaseOrderPriority.High
                    => 3,

                CustomerPurchaseOrderPriority.Normal
                    => 4,

                _ => 99
            };
        }

        #endregion


        #region Priority Compare

        private static bool PriorityEquals(
            string? priority,
            CustomerPurchaseOrderPriority expectedPriority)
        {
            if (!Enum.TryParse<
                    CustomerPurchaseOrderPriority>(
                    priority,
                    true,
                    out var parsedPriority))
            {
                return false;
            }


            return parsedPriority ==
                expectedPriority;
        }

        #endregion


        #region Raw Tracking Row

        private sealed class RawTrackingRow
        {
            #region Customer PO Item

            public int CustomerPurchaseOrderItemId
            {
                get;
                set;
            }

            #endregion


            #region Customer PO

            public int CustomerPurchaseOrderId
            {
                get;
                set;
            }


            public string PurchaseOrderNumber
            {
                get;
                set;
            } = string.Empty;


            public DateTime PurchaseOrderDate
            {
                get;
                set;
            }


            public CustomerPurchaseOrderStatus
                PurchaseOrderStatus
            {
                get;
                set;
            }

            #endregion


            #region Customer

            public int CustomerId
            {
                get;
                set;
            }


            public string CustomerName
            {
                get;
                set;
            } = string.Empty;

            #endregion


            #region Item

            public int ItemId
            {
                get;
                set;
            }


            public string ItemCode
            {
                get;
                set;
            } = string.Empty;


            public string ItemName
            {
                get;
                set;
            } = string.Empty;

            #endregion


            #region Drawing

            public int? DrawingId
            {
                get;
                set;
            }


            public string? DrawingNumber
            {
                get;
                set;
            }


            public string? DrawingFilePath
            {
                get;
                set;
            }

            #endregion


            #region Quantity / Delivery

            public decimal OrderedQuantity
            {
                get;
                set;
            }


            public DateTime DeliveryDate
            {
                get;
                set;
            }

            #endregion


            #region Priority

            public CustomerPurchaseOrderPriority
                EffectivePriority
            {
                get;
                set;
            }

            #endregion
        }

        #endregion


        #region Production Job Tracking Row

        private sealed class ProductionJobTrackingRow
        {
            public int CustomerPurchaseOrderId
            {
                get;
                set;
            }


            public ProductionJobStatus Status
            {
                get;
                set;
            }
        }

        #endregion


        #region Production PO Summary Row

        private sealed class ProductionPOSummaryRow
        {
            public int TotalProductionJobs
            {
                get;
                set;
            }


            public int CompletedProductionJobs
            {
                get;
                set;
            }


            public string ProductionPOStatus
            {
                get;
                set;
            } = "Pending";
        }

        #endregion
    }
}