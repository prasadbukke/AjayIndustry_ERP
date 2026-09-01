/*
=============================================================
File: SupplierOutstandingViewModel.cs
Module: Supplier Outstanding / Payables
Layer: Web - ViewModel

Purpose:
Provides read-only report models for Supplier Outstanding.

Important:
- This is NOT a CRUD module.
- No Entity is created.
- No Database Table is created.
- No Migration is required.
- Outstanding is calculated LIVE from:
      Finalized Purchase Invoice
      - Active Supplier Payment Transactions

Payment Status:
- Pending
- Partially Paid
- Completed

Report supports:
- Text Search
- Supplier Filter
- Payment Status Filter
- Due Status Filter
- Due Date Range
- Pagination
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.ViewModels.SupplierOutstanding
{
    #region Index ViewModel

    /// <summary>
    /// Main read-only Supplier Outstanding report model.
    /// </summary>
    public class SupplierOutstandingIndexViewModel
    {
        #region Filters

        /// <summary>
        /// Searches:
        /// - ERP Purchase Invoice No.
        /// - Supplier Invoice No.
        /// - Supplier Name
        /// </summary>
        public string? SearchText { get; set; }

        /// <summary>
        /// Optional Supplier filter.
        /// </summary>
        public int? SupplierId { get; set; }

        /// <summary>
        /// Payment Status filter:
        /// All / Pending / Partially Paid / Completed
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Due filter:
        /// All / Overdue / Due Soon / Upcoming
        /// </summary>
        public string? DueStatus { get; set; }

        /// <summary>
        /// Optional Due Date From filter.
        /// </summary>
        public DateTime? DueDateFrom { get; set; }

        /// <summary>
        /// Optional Due Date To filter.
        /// </summary>
        public DateTime? DueDateTo { get; set; }

        #endregion


        #region Dropdown Data

        /// <summary>
        /// Supplier dropdown items.
        /// </summary>
        public List<SelectListItem> Suppliers { get; set; }
            = new();

        /// <summary>
        /// Payment Status dropdown items.
        /// </summary>
        public List<SelectListItem> PaymentStatuses { get; set; }
            = new();

        /// <summary>
        /// Due Status dropdown items.
        /// </summary>
        public List<SelectListItem> DueStatuses { get; set; }
            = new();

        #endregion


        #region Report Data

        /// <summary>
        /// Paginated Supplier Outstanding rows.
        /// </summary>
        public PagedResult<SupplierOutstandingRowViewModel> Results { get; set; }
            = new();

        #endregion
    }

    #endregion


    #region Report Row ViewModel

    /// <summary>
    /// One Purchase Invoice row in Supplier Outstanding report.
    /// </summary>
    public class SupplierOutstandingRowViewModel
    {
        #region Purchase Invoice

        public int PurchaseInvoiceId { get; set; }

        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;

        public string? SupplierInvoiceNumber { get; set; }

        public DateTime PurchaseInvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        #endregion


        #region Supplier

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;

        #endregion


        #region Payment Position

        /// <summary>
        /// Purchase Invoice Grand Total.
        /// </summary>
        public decimal InvoiceTotal { get; set; }

        /// <summary>
        /// Sum of active, non-deleted payment transactions
        /// under active, non-deleted Supplier Payment.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// InvoiceTotal - PaidAmount.
        /// Minimum displayed value should be zero.
        /// </summary>
        public decimal OutstandingAmount { get; set; }

        /// <summary>
        /// Pending / Partially Paid / Completed.
        /// Calculated dynamically.
        /// </summary>
        public string PaymentStatus { get; set; }
            = string.Empty;

        #endregion


        #region Due Position

        /// <summary>
        /// Overdue / Due Soon / Upcoming / No Due Date.
        /// Calculated dynamically.
        /// </summary>
        public string DueStatus { get; set; }
            = string.Empty;

        /// <summary>
        /// Number of days overdue.
        /// Zero when invoice is not overdue.
        /// </summary>
        public int OverdueDays { get; set; }

        #endregion
    }

    #endregion
}