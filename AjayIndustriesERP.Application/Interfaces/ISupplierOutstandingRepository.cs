/*
=============================================================
File: ISupplierOutstandingRepository.cs
Module: Supplier Outstanding / Payables
Layer: Application - Interface

Purpose:
Defines read-only repository contract for Supplier Outstanding.

Important:
- This is NOT a CRUD repository.
- No Add / Update / Delete methods.
- No new Entity / Table / Migration.
- Data comes from existing:
      PurchaseInvoices
      SupplierPayments
      SupplierPaymentTransactions

Repository Responsibilities:
- Read finalized Purchase Invoices.
- Calculate active paid amount.
- Apply report filters.
- Apply pagination.
- Return supplier filter options.

Service Responsibilities:
- Normalize input.
- Calculate display status.
- Calculate due status / overdue days.
- Prepare final report result.
=============================================================
*/

using AjayIndustriesERP.Application.Common;

namespace AjayIndustriesERP.Application.Interfaces
{
    #region Repository Interface

    public interface ISupplierOutstandingRepository
    {
        /// <summary>
        /// Returns paginated Supplier Outstanding source rows
        /// after applying report filters.
        /// </summary>
        Task<PagedResult<SupplierOutstandingRepositoryRow>>
            GetPagedAsync(
                SupplierOutstandingRepositoryFilter filter);

        /// <summary>
        /// Returns suppliers having finalized
        /// Purchase Invoices.
        /// </summary>
        Task<List<SupplierOutstandingSupplierOption>>
            GetSupplierOptionsAsync();
    }

    #endregion


    #region Repository Filter

    /// <summary>
    /// Query filters used by Supplier Outstanding repository.
    /// </summary>
    public class SupplierOutstandingRepositoryFilter
    {
        #region Search Filters

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
        /// Payment filter:
        /// Outstanding Only / All / Pending /
        /// Partially Paid / Completed.
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Due filter:
        /// All / Overdue / Due Soon / Upcoming.
        /// </summary>
        public string? DueStatus { get; set; }

        /// <summary>
        /// Optional Due Date From.
        /// </summary>
        public DateTime? DueDateFrom { get; set; }

        /// <summary>
        /// Optional Due Date To.
        /// </summary>
        public DateTime? DueDateTo { get; set; }

        #endregion


        #region Date Context

        /// <summary>
        /// Current business date supplied by service.
        /// Used for Overdue / Due Soon / Upcoming filters.
        /// </summary>
        public DateTime Today { get; set; }

        /// <summary>
        /// Number of days considered as Due Soon.
        /// Current business rule = 5 days.
        /// </summary>
        public int DueSoonDays { get; set; } = 5;

        #endregion


        #region Pagination

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        #endregion
    }

    #endregion


    #region Repository Row

    /// <summary>
    /// Read-only source row returned from Infrastructure.
    ///
    /// Display-specific values such as:
    /// - Payment Status
    /// - Due Status
    /// - Overdue Days
    ///
    /// are calculated by Application Service.
    /// </summary>
    public class SupplierOutstandingRepositoryRow
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

        /// <summary>
        /// Purchase Invoice Grand Total.
        /// </summary>
        public decimal InvoiceTotal { get; set; }

        /// <summary>
        /// Sum of active, non-deleted transactions
        /// belonging to active, non-deleted Supplier Payment.
        /// </summary>
        public decimal PaidAmount { get; set; }

        #endregion
    }

    #endregion


    #region Supplier Option

    /// <summary>
    /// Lightweight Supplier option used for report filter.
    /// </summary>
    public class SupplierOutstandingSupplierOption
    {
        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;
    }

    #endregion
}