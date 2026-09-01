/*
=============================================================
File: ISupplierOutstandingService.cs
Module: Supplier Outstanding / Payables
Layer: Application - Interface

Purpose:
Defines business service contract for:

1. Supplier Outstanding / Payables Report
2. Home Dashboard Supplier Payment Due Alerts

Important:
- Read-only service.
- No Create / Edit / Delete.
- No Entity / Table / Migration.

Report Responsibilities:
- Filter normalization
- Payment status calculation
- Due status calculation
- Overdue days calculation
- Final report mapping

Dashboard Alert Rule:
- Finalized Purchase Invoice
- Active / Not Deleted
- Outstanding > 0
- Due Date exists
- Includes:
      Overdue
      Due Today
      Due within next 5 days
- Fully paid invoices are excluded automatically.
=============================================================
*/

using AjayIndustriesERP.Application.Common;

namespace AjayIndustriesERP.Application.Interfaces
{
    #region Service Interface

    public interface ISupplierOutstandingService
    {
        /// <summary>
        /// Returns paginated Supplier Outstanding report.
        /// </summary>
        Task<SupplierOutstandingResult>
            GetReportAsync(
                SupplierOutstandingFilter filter);


        /// <summary>
        /// Returns supplier options for report filter.
        /// </summary>
        Task<List<SupplierOutstandingSupplierOption>>
            GetSupplierOptionsAsync();


        /// <summary>
        /// Returns unpaid Supplier Purchase Invoices that are:
        ///
        /// - Overdue
        /// - Due today
        /// - Due within next 5 days
        ///
        /// Used by Home Dashboard popup.
        /// </summary>
        Task<List<SupplierOutstandingDueAlertResult>>
            GetDueAlertsAsync();
    }

    #endregion


    #region Service Filter

    /// <summary>
    /// Business filter model accepted by
    /// Supplier Outstanding service.
    /// </summary>
    public class SupplierOutstandingFilter
    {
        #region Search Filters

        public string? SearchText { get; set; }

        public int? SupplierId { get; set; }

        /// <summary>
        /// Blank = Outstanding Only
        /// All
        /// Pending
        /// Partially Paid
        /// Completed
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Blank = All Due Dates
        /// Overdue
        /// Due Soon
        /// Upcoming
        /// </summary>
        public string? DueStatus { get; set; }

        public DateTime? DueDateFrom { get; set; }

        public DateTime? DueDateTo { get; set; }

        #endregion


        #region Pagination

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        #endregion
    }

    #endregion


    #region Report Result

    /// <summary>
    /// Final Supplier Outstanding report result.
    /// </summary>
    public class SupplierOutstandingResult
    {
        public PagedResult<SupplierOutstandingResultRow> Results { get; set; }
            = new();

        public string? SearchText { get; set; }

        public int? SupplierId { get; set; }

        public string? PaymentStatus { get; set; }

        public string? DueStatus { get; set; }

        public DateTime? DueDateFrom { get; set; }

        public DateTime? DueDateTo { get; set; }
    }

    #endregion


    #region Report Result Row

    /// <summary>
    /// Final Supplier Outstanding report row.
    /// </summary>
    public class SupplierOutstandingResultRow
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


        #region Amounts

        public decimal InvoiceTotal { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal OutstandingAmount { get; set; }

        #endregion


        #region Calculated Status

        public string PaymentStatus { get; set; }
            = string.Empty;

        public string DueStatus { get; set; }
            = string.Empty;

        public int OverdueDays { get; set; }

        #endregion
    }

    #endregion


    #region Dashboard Due Alert Result

    /// <summary>
    /// One Supplier Purchase Invoice displayed in
    /// Home Dashboard payment due popup.
    /// </summary>
    public class SupplierOutstandingDueAlertResult
    {
        #region Purchase Invoice

        public int PurchaseInvoiceId { get; set; }

        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;

        public string? SupplierInvoiceNumber { get; set; }

        public DateTime PurchaseInvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        #endregion


        #region Supplier

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;

        #endregion


        #region Payment Position

        public decimal InvoiceTotal { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal OutstandingAmount { get; set; }

        public string PaymentStatus { get; set; }
            = string.Empty;

        #endregion


        #region Due Position

        /// <summary>
        /// Overdue / Due Soon.
        /// </summary>
        public string DueStatus { get; set; }
            = string.Empty;

        /// <summary>
        /// Number of days overdue.
        /// Zero when invoice is not overdue.
        /// </summary>
        public int OverdueDays { get; set; }

        /// <summary>
        /// Number of days remaining until Due Date.
        ///
        /// Examples:
        /// 0 = Due Today
        /// 1 = Due Tomorrow
        /// 5 = Due after 5 days
        ///
        /// Overdue invoice returns 0 here;
        /// OverdueDays contains overdue duration.
        /// </summary>
        public int DaysUntilDue { get; set; }

        #endregion
    }

    #endregion
}