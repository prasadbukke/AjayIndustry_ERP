/*
=============================================================
File: IItemCustomerPOTrackingRepository.cs
Module: Item Customer PO Tracking
Layer: Application - Interface

Purpose:
Defines read-only repository contract for Item-wise
Customer Purchase Order Tracking.

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

Production Tracking is intentionally separated into:

1. Production Job Progress
   Example:
   5 / 10 Completed

2. Production PO Status
   Pending
   In Progress
   Completed

Production PO Status Rule:

Pending:
- No active Production Jobs exist
  OR
- Jobs exist but none have started/completed.

In Progress:
- One or more Jobs are In Progress / Completed
  BUT all Jobs are not Completed.

Completed:
- ALL active non-cancelled Production Jobs
  belonging to the Customer PO are Completed.

Important:
- Production Job Status and Production PO Status
  are NOT the same thing.
- Cancelled Production Jobs are ignored.
- Read-only module.
- No Entity.
- No Table.
- No Migration.
=============================================================
*/

using AjayIndustriesERP.Application.Common;

namespace AjayIndustriesERP.Application.Interfaces
{
    #region Repository Interface

    public interface IItemCustomerPOTrackingRepository
    {
        Task<PagedResult<ItemCustomerPOTrackingRepositoryRow>>
            GetPagedAsync(
                ItemCustomerPOTrackingRepositoryFilter filter);


        Task<List<ItemCustomerPOTrackingRepositoryRow>>
            GetExportRowsAsync(
                ItemCustomerPOTrackingRepositoryFilter filter);


        Task<ItemCustomerPOTrackingRepositorySummary>
            GetSummaryAsync(
                ItemCustomerPOTrackingRepositoryFilter filter);


        Task<List<ItemCustomerPOTrackingItemSuggestion>>
            SearchItemsAsync(
                string searchText,
                int maxResults = 10);


        Task<List<ItemCustomerPOTrackingPOSuggestion>>
            SearchPurchaseOrdersAsync(
                string searchText,
                int maxResults = 10);


        Task<List<ItemCustomerPOTrackingCustomerOption>>
            GetCustomerOptionsAsync();


        Task<List<string>>
            GetPurchaseOrderStatusesAsync();
    }

    #endregion


    #region Repository Filter

    public class ItemCustomerPOTrackingRepositoryFilter
    {
        #region Item Filter

        public int? ItemId { get; set; }

        public string? ItemSearchText { get; set; }

        #endregion


        #region Customer PO Filter

        public int? CustomerPurchaseOrderId { get; set; }

        public string? PurchaseOrderNumber { get; set; }

        #endregion


        #region Customer Filter

        public int? CustomerId { get; set; }

        #endregion


        #region Priority Filter

        /// <summary>
        /// Uses same Priority saved during Customer PO Entry.
        /// </summary>
        public string? Priority { get; set; }

        #endregion


        #region Customer PO Status Filter

        public string? PurchaseOrderStatus { get; set; }

        #endregion


        #region Production PO Status Filter

        /// <summary>
        /// Blank = All
        ///
        /// Pending
        /// In Progress
        /// Completed
        ///
        /// IMPORTANT:
        /// This is PO-level Production Status,
        /// not individual Production Job Status.
        /// </summary>
        public string? ProductionPOStatus { get; set; }

        #endregion


        #region PO Date Filter

        public DateTime? PurchaseOrderDateFrom { get; set; }

        public DateTime? PurchaseOrderDateTo { get; set; }

        #endregion


        #region Pagination

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        #endregion
    }

    #endregion


    #region Repository Row

    public class ItemCustomerPOTrackingRepositoryRow
    {
        #region Customer PO

        public int CustomerPurchaseOrderId { get; set; }

        public string PurchaseOrderNumber { get; set; }
            = string.Empty;

        public DateTime PurchaseOrderDate { get; set; }

        public string PurchaseOrderStatus { get; set; }
            = string.Empty;

        #endregion


        #region Customer

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }
            = string.Empty;

        #endregion


        #region Item

        public int ItemId { get; set; }

        public string ItemCode { get; set; }
            = string.Empty;

        public string ItemName { get; set; }
            = string.Empty;

        #endregion


        #region Drawing

        public int? DrawingId { get; set; }

        public string? DrawingNumber { get; set; }

        public string? DrawingFilePath { get; set; }

        #endregion


        #region Quantity

        public decimal OrderedQuantity { get; set; }

        #endregion


        #region Delivery Date

        public DateTime DeliveryDate { get; set; }

        #endregion


        #region Priority

        public string Priority { get; set; }
            = string.Empty;

        #endregion


        #region Production Job Progress

        /// <summary>
        /// Total active non-cancelled Production Jobs
        /// belonging to this Customer PO.
        /// </summary>
        public int TotalProductionJobs { get; set; }


        /// <summary>
        /// Number of Production Jobs whose
        /// actual Job Status is Completed.
        /// </summary>
        public int CompletedProductionJobs { get; set; }

        #endregion


        #region Production PO Status

        /// <summary>
        /// PO-level Production Status.
        ///
        /// Pending
        /// In Progress
        /// Completed
        ///
        /// Example:
        ///
        /// Total Jobs     = 10
        /// Completed Jobs = 5
        ///
        /// ProductionPOStatus = In Progress
        /// </summary>
        public string ProductionPOStatus { get; set; }
            = "Pending";

        #endregion
    }

    #endregion


    #region Repository Summary

    public class ItemCustomerPOTrackingRepositorySummary
    {
        #region Main Summary

        public int TotalPurchaseOrders { get; set; }

        public decimal TotalOrderedQuantity { get; set; }

        #endregion


        #region Priority Summary

        public int CriticalPriorityPurchaseOrders { get; set; }

        public int UrgentPurchaseOrders { get; set; }

        public int HighPriorityPurchaseOrders { get; set; }

        public int NormalPriorityPurchaseOrders { get; set; }

        #endregion


        #region Production PO Summary

        /// <summary>
        /// Distinct Customer POs whose Production
        /// PO Status is Pending.
        /// </summary>
        public int ProductionPendingPurchaseOrders { get; set; }


        /// <summary>
        /// Distinct Customer POs whose Production
        /// PO Status is In Progress.
        /// </summary>
        public int ProductionInProgressPurchaseOrders { get; set; }


        /// <summary>
        /// Distinct Customer POs whose ALL active
        /// Production Jobs are Completed.
        /// </summary>
        public int ProductionCompletedPurchaseOrders { get; set; }

        #endregion
    }

    #endregion


    #region Item Suggestion

    public class ItemCustomerPOTrackingItemSuggestion
    {
        public int ItemId { get; set; }

        public string ItemCode { get; set; }
            = string.Empty;

        public string ItemName { get; set; }
            = string.Empty;
    }

    #endregion


    #region Customer PO Suggestion

    public class ItemCustomerPOTrackingPOSuggestion
    {
        public int CustomerPurchaseOrderId { get; set; }

        public string PurchaseOrderNumber { get; set; }
            = string.Empty;

        public string CustomerName { get; set; }
            = string.Empty;
    }

    #endregion


    #region Customer Option

    public class ItemCustomerPOTrackingCustomerOption
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }
            = string.Empty;
    }

    #endregion
}