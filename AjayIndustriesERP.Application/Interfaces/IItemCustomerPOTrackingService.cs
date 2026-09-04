/*
=============================================================
File: IItemCustomerPOTrackingService.cs
Module: Item Customer PO Tracking
Layer: Application - Interface

Purpose:
Defines business service contract for Item-wise
Customer Purchase Order Tracking.

Tracking Information:
- Customer PO
- Customer
- PO Date
- Item
- Drawing
- Ordered Quantity
- Due / Delivery Date
- Actual Production Completion Date
- Priority
- PO Status

Production Tracking:

1. Production Job Progress

2. Production PO Status
   Pending
   In Progress
   Completed

3. Production Completion Date

Production Completion Date Rule:

Pending / In Progress
    → null

Completed
    → actual final Production completion date.

Important:
- Production Job Status and Production PO Status
  are separate concepts.
- Due Date filter uses existing effective Delivery Date.
- Completion Date filter uses actual Production completion date.
- Read-only module.
- No Entity.
- No Table.
- No Migration.
=============================================================
*/

using AjayIndustriesERP.Application.Common;

namespace AjayIndustriesERP.Application.Interfaces
{
    #region Service Interface

    public interface IItemCustomerPOTrackingService
    {
        /// <summary>
        /// Returns paginated Item Customer PO tracking report.
        /// </summary>
        Task<ItemCustomerPOTrackingResult>
            GetReportAsync(
                ItemCustomerPOTrackingFilter filter);


        /// <summary>
        /// Returns ALL matching rows without pagination.
        /// Used for Excel Export.
        /// </summary>
        Task<List<ItemCustomerPOTrackingResultRow>>
            GetExportRowsAsync(
                ItemCustomerPOTrackingFilter filter);


        /// <summary>
        /// Returns Item autocomplete suggestions.
        /// </summary>
        Task<List<ItemCustomerPOTrackingItemSuggestionResult>>
            SearchItemsAsync(
                string? searchText);


        /// <summary>
        /// Returns Customer PO Number autocomplete suggestions.
        /// </summary>
        Task<List<ItemCustomerPOTrackingPOSuggestionResult>>
            SearchPurchaseOrdersAsync(
                string? searchText);


        /// <summary>
        /// Returns Customer filter options.
        /// </summary>
        Task<List<ItemCustomerPOTrackingCustomerOptionResult>>
            GetCustomerOptionsAsync();


        /// <summary>
        /// Returns Customer PO status options.
        /// </summary>
        Task<List<string>>
            GetPurchaseOrderStatusesAsync();
    }

    #endregion


    #region Service Filter

    public class ItemCustomerPOTrackingFilter
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
        /// Same Priority saved during Customer PO Entry.
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
        /// This is PO-level Production Status,
        /// not individual Production Job Status.
        /// </summary>
        public string? ProductionPOStatus { get; set; }

        #endregion


        #region PO Date Filter

        public DateTime? PurchaseOrderDateFrom { get; set; }


        public DateTime? PurchaseOrderDateTo { get; set; }

        #endregion


        #region Due Date Filter

        /// <summary>
        /// User-facing filter name:
        /// Due Date.
        ///
        /// Uses effective Delivery Date:
        ///
        /// Item Required Delivery Date
        /// otherwise
        /// Customer PO Required Delivery Date.
        /// </summary>
        public DateTime? DueDateFrom { get; set; }


        public DateTime? DueDateTo { get; set; }

        #endregion


        #region Completion Date Filter

        /// <summary>
        /// Filters actual Production completion date.
        ///
        /// Pending / In Progress rows have no
        /// Completion Date.
        /// </summary>
        public DateTime? CompletionDateFrom { get; set; }


        public DateTime? CompletionDateTo { get; set; }

        #endregion


        #region Pagination

        public int PageNumber { get; set; } = 1;


        public int PageSize { get; set; } = 10;

        #endregion
    }

    #endregion


    #region Report Result

    public class ItemCustomerPOTrackingResult
    {
        #region Filters

        public int? ItemId { get; set; }


        public string? ItemSearchText { get; set; }


        public int? CustomerPurchaseOrderId { get; set; }


        public string? PurchaseOrderNumber { get; set; }


        public int? CustomerId { get; set; }


        public string? Priority { get; set; }


        public string? PurchaseOrderStatus { get; set; }


        public string? ProductionPOStatus { get; set; }


        public DateTime? PurchaseOrderDateFrom { get; set; }


        public DateTime? PurchaseOrderDateTo { get; set; }


        public DateTime? DueDateFrom { get; set; }


        public DateTime? DueDateTo { get; set; }


        public DateTime? CompletionDateFrom { get; set; }


        public DateTime? CompletionDateTo { get; set; }

        #endregion


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

        public int ProductionPendingPurchaseOrders { get; set; }


        public int ProductionInProgressPurchaseOrders { get; set; }


        public int ProductionCompletedPurchaseOrders { get; set; }

        #endregion


        #region Results

        public PagedResult<ItemCustomerPOTrackingResultRow>
            Results
        {
            get;
            set;
        } = new();

        #endregion
    }

    #endregion


    #region Result Row

    public class ItemCustomerPOTrackingResultRow
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


        #region Due / Delivery Date

        /// <summary>
        /// Existing effective Delivery Date.
        ///
        /// This is displayed to the user as Due Date.
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        #endregion


        #region Production Completion Date

        /// <summary>
        /// Actual Production completion date.
        ///
        /// Pending / In Progress:
        /// null
        ///
        /// Completed:
        /// actual final Production completion date.
        /// </summary>
        public DateTime? CompletionDate { get; set; }

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
        /// Number of Jobs whose actual
        /// Production Job Status is Completed.
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
        /// </summary>
        public string ProductionPOStatus { get; set; }
            = "Pending";

        #endregion
    }

    #endregion


    #region Item Suggestion Result

    public class ItemCustomerPOTrackingItemSuggestionResult
    {
        public int ItemId { get; set; }


        public string ItemCode { get; set; }
            = string.Empty;


        public string ItemName { get; set; }
            = string.Empty;


        public string DisplayText { get; set; }
            = string.Empty;
    }

    #endregion


    #region Customer PO Suggestion Result

    public class ItemCustomerPOTrackingPOSuggestionResult
    {
        public int CustomerPurchaseOrderId { get; set; }


        public string PurchaseOrderNumber { get; set; }
            = string.Empty;


        public string CustomerName { get; set; }
            = string.Empty;


        public string DisplayText { get; set; }
            = string.Empty;
    }

    #endregion


    #region Customer Option Result

    public class ItemCustomerPOTrackingCustomerOptionResult
    {
        public int CustomerId { get; set; }


        public string CustomerName { get; set; }
            = string.Empty;
    }

    #endregion
}