/*
=============================================================
File: ItemCustomerPOTrackingViewModel.cs
Module: Item Customer PO Tracking
Layer: Web - ViewModel

Purpose:
ViewModels used by Item-wise Customer PO Tracking screen.

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
   Example:
   5 / 10 Completed

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
- Production Job Progress and Production PO Status
  are separate.
- Due Date uses effective Delivery Date.
- Completion Date uses actual Production completion date.
- Read-only module.
- No Entity.
- No Table.
- No Migration.
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.ViewModels.ItemCustomerPOTracking
{
    #region Index ViewModel

    public class ItemCustomerPOTrackingIndexViewModel
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

        public List<SelectListItem> Customers { get; set; }
            = new();

        #endregion


        #region Priority Filter

        public string? Priority { get; set; }

        public List<SelectListItem> Priorities { get; set; }
            = new();

        #endregion


        #region Customer PO Status Filter

        public string? PurchaseOrderStatus { get; set; }

        public List<SelectListItem>
            PurchaseOrderStatuses
        {
            get;
            set;
        } = new();

        #endregion


        #region Production PO Status Filter

        /// <summary>
        /// Blank = All
        ///
        /// Pending
        /// In Progress
        /// Completed
        /// </summary>
        public string? ProductionPOStatus { get; set; }


        public List<SelectListItem>
            ProductionPOStatuses
        {
            get;
            set;
        } = new();

        #endregion


        #region PO Date Filter

        public DateTime? PurchaseOrderDateFrom { get; set; }

        public DateTime? PurchaseOrderDateTo { get; set; }

        #endregion


        #region Due Date Filter

        /// <summary>
        /// Uses existing effective Delivery Date.
        /// </summary>
        public DateTime? DueDateFrom { get; set; }

        public DateTime? DueDateTo { get; set; }

        #endregion


        #region Completion Date Filter

        /// <summary>
        /// Filters actual Production completion date.
        /// </summary>
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

        public PagedResult<ItemCustomerPOTrackingRowViewModel>
            Results
        {
            get;
            set;
        } = new();

        #endregion
    }

    #endregion


    #region Result Row

    public class ItemCustomerPOTrackingRowViewModel
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
        /// Displayed to user as Due Date.
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        #endregion


        #region Production Completion Date

        /// <summary>
        /// Actual Production completion date.
        ///
        /// Pending / In Progress
        ///     → null
        ///
        /// Completed
        ///     → actual final Production completion date.
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
        /// belonging to the complete Customer PO.
        /// </summary>
        public int TotalProductionJobs { get; set; }


        /// <summary>
        /// Completed Production Jobs
        /// belonging to the complete Customer PO.
        /// </summary>
        public int CompletedProductionJobs { get; set; }

        #endregion


        #region Production PO Status

        /// <summary>
        /// PO-level Production Status:
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


    #region Item Autocomplete

    public class ItemCustomerPOTrackingItemSuggestionViewModel
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


    #region Customer PO Autocomplete

    public class ItemCustomerPOTrackingPOSuggestionViewModel
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
}