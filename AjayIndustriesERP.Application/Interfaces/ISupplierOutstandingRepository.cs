/*
=============================================================
File: ISupplierOutstandingRepository.cs
Module: Supplier Outstanding / Payables
Layer: Application - Interface

Purpose:
Defines read-only repository contract for:
1. Supplier Outstanding report
2. Home Dashboard supplier payment due alerts

Important:
- This is NOT a CRUD repository.
- No Add / Update / Delete.
- No new Entity / Table / Migration.
- Data comes from existing:
      PurchaseInvoices
      SupplierPayments
      SupplierPaymentTransactions

Dashboard Alert Rule:
- Purchase Invoice must be Finalized.
- Purchase Invoice must be Active / Not Deleted.
- Outstanding must be greater than zero.
- Due Date must exist.
- Due Date <= Today + DueSoonDays.
- Includes both:
      Overdue
      Due Soon
- Fully paid invoices must NOT appear.
=============================================================
*/

using AjayIndustriesERP.Application.Common;

namespace AjayIndustriesERP.Application.Interfaces
{
    #region Repository Interface

    public interface ISupplierOutstandingRepository
    {
        /// <summary>
        /// Returns paginated Supplier Outstanding rows
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


        /// <summary>
        /// Returns unpaid supplier invoices that are:
        /// - overdue
        /// - due today
        /// - due within configured upcoming days
        ///
        /// Used by Home Dashboard popup.
        /// </summary>
        Task<List<SupplierOutstandingRepositoryRow>>
            GetDueAlertsAsync(
                DateTime today,
                int dueSoonDays);
    }

    #endregion


    #region Repository Filter

    public class SupplierOutstandingRepositoryFilter
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


        #region Date Context

        public DateTime Today { get; set; }

        public int DueSoonDays { get; set; } = 5;

        #endregion


        #region Pagination

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        #endregion
    }

    #endregion


    #region Repository Row

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

    public class SupplierOutstandingSupplierOption
    {
        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;
    }

    #endregion
}