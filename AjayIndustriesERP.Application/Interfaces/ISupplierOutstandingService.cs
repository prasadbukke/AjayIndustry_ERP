/*
=============================================================
File: ISupplierOutstandingService.cs
Module: Supplier Outstanding / Payables
Layer: Application - Interface

Purpose:
Defines business service contract for Supplier Outstanding.

Important:
- Read-only reporting service.
- No Create / Edit / Delete.
- No Entity / Table / Migration.
- Repository handles database query/filtering.
- Service handles:
    - filter normalization
    - dropdown preparation
    - payment status calculation
    - due status calculation
    - overdue days calculation
    - final report model preparation

Data Source:
- Finalized PurchaseInvoices
- SupplierPayments
- SupplierPaymentTransactions
=============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;

namespace AjayIndustriesERP.Application.Interfaces
{
    #region Service Interface

    public interface ISupplierOutstandingService
    {
        /// <summary>
        /// Returns paginated Supplier Outstanding report data
        /// after applying business rules and filters.
        /// </summary>
        Task<SupplierOutstandingResult>
            GetReportAsync(
                SupplierOutstandingFilter filter);

        /// <summary>
        /// Returns supplier options for report filter.
        /// </summary>
        Task<List<SupplierOutstandingSupplierOption>>
            GetSupplierOptionsAsync();
    }

    #endregion


    #region Service Filter

    /// <summary>
    /// Business filter model accepted by Supplier Outstanding service.
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


    #region Service Result

    /// <summary>
    /// Final business result returned to Web layer.
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


    #region Result Row

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
}