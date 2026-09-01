// =============================================================
// File: ISupplierPaymentService.cs
// Module: Supplier Payment
// Layer: Application - Service Interface
//
// Purpose:
// Defines business operations for Supplier Payment.
//
// Final Business Flow:
//
// Finalized Purchase Invoice
//          ↓
// Supplier Payment Header
//          ↓
// First Payment Transaction
//          ↓
// Additional Payment Transactions
//          ↓
// Transaction Edit when correction is required
//          ↓
// Paid / Outstanding / Status calculated live
//
// Important Business Rules:
// - One Purchase Invoice has only one Supplier Payment No.
// - Multiple transactions use the SAME Supplier Payment No.
// - Only Finalized Purchase Invoices can receive payments.
// - Company and Supplier are derived from Purchase Invoice.
// - Transaction Amount must be greater than zero.
// - Transaction Amount cannot cause total payment to exceed
//   Purchase Invoice Total.
// - Existing transaction can be edited.
// - Payment No. / Invoice / Supplier / Company cannot be
//   changed through transaction edit.
// - Paid Amount is calculated from active transactions.
// - Outstanding is calculated live and is not stored.
// - Payment Status is calculated live:
//      Pending
//      Partially Paid
//      Completed
// - Supplier Payment delete is Soft Delete.
// =============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ISupplierPaymentService
    {
        // =====================================================
        // BASIC READ
        // =====================================================

        #region Basic Read

        /// <summary>
        /// Returns one active Supplier Payment including
        /// Purchase Invoice and active transactions.
        /// </summary>
        Task<SupplierPayment?> GetByIdAsync(
            int id);


        /// <summary>
        /// Returns active Supplier Payment linked to the
        /// specified Purchase Invoice.
        /// </summary>
        Task<SupplierPayment?> GetByPurchaseInvoiceIdAsync(
            int purchaseInvoiceId);


        /// <summary>
        /// Returns all soft-deleted Supplier Payments.
        /// </summary>
        Task<List<SupplierPayment>> GetDeletedAsync();

        #endregion


        // =====================================================
        // INDEX / SEARCH
        // =====================================================

        #region Index / Search

        /// <summary>
        /// Returns paginated active Supplier Payments.
        /// </summary>
        Task<PagedResult<SupplierPayment>> GetPagedAsync(
            int pageNumber,
            int pageSize);


        /// <summary>
        /// Searches Supplier Payments by:
        ///
        /// - Supplier Payment No.
        /// - ERP Purchase Invoice No.
        /// - Supplier Invoice No.
        /// - Supplier Name
        /// - Transaction Date
        /// - Payment Mode
        /// - Bank Name
        /// - Reference Number
        /// </summary>
        Task<PagedResult<SupplierPayment>> SearchPagedAsync(
            string? searchText,
            int pageNumber,
            int pageSize);

        #endregion


        // =====================================================
        // PURCHASE INVOICE SOURCE
        // =====================================================

        #region Purchase Invoice Source

        /// <summary>
        /// Returns Finalized Purchase Invoices which do not
        /// already have a Supplier Payment header.
        ///
        /// Soft-deleted Supplier Payment headers are also
        /// considered existing and must be restored.
        /// </summary>
        Task<List<PurchaseInvoice>>
            GetAvailablePurchaseInvoicesAsync();

        #endregion


        // =====================================================
        // PAYMENT CALCULATIONS
        // =====================================================

        #region Payment Calculations

        /// <summary>
        /// Returns total amount paid through active,
        /// non-deleted transactions.
        /// </summary>
        Task<decimal> GetPaidAmountAsync(
            int supplierPaymentId);


        /// <summary>
        /// Returns current Purchase Invoice Outstanding.
        ///
        /// Outstanding =
        /// PurchaseInvoice.GrandTotal
        /// - Paid Amount
        /// </summary>
        Task<decimal> GetOutstandingAmountAsync(
            int supplierPaymentId);


        /// <summary>
        /// Returns calculated payment status.
        ///
        /// Pending:
        /// Paid = 0
        ///
        /// Partially Paid:
        /// Paid > 0 and Paid < Invoice Total
        ///
        /// Completed:
        /// Paid >= Invoice Total
        /// </summary>
        Task<string> GetPaymentStatusAsync(
            int supplierPaymentId);

        #endregion


        // =====================================================
        // CREATE PAYMENT
        // =====================================================

        #region Create Payment

        /// <summary>
        /// Creates:
        ///
        /// 1. Supplier Payment Header
        /// 2. First Supplier Payment Transaction
        ///
        /// in the same operation.
        ///
        /// SupplierId and CompanyId are derived from the
        /// selected Purchase Invoice.
        ///
        /// Payment Code is generated by the Service.
        /// </summary>
        Task<SupplierPayment> CreateAsync(
            int purchaseInvoiceId,
            SupplierPaymentTransaction firstTransaction);

        #endregion


        // =====================================================
        // ADD PAYMENT TRANSACTION
        // =====================================================

        #region Add Payment Transaction

        /// <summary>
        /// Adds another actual payment transaction under
        /// the SAME Supplier Payment Number.
        ///
        /// No new Supplier Payment Code is generated.
        /// </summary>
        Task<SupplierPaymentTransaction> AddTransactionAsync(
            int supplierPaymentId,
            SupplierPaymentTransaction transaction);

        #endregion


        // =====================================================
        // EDIT PAYMENT TRANSACTION
        // =====================================================

        #region Edit Payment Transaction

        /// <summary>
        /// Updates an existing payment transaction.
        ///
        /// Editable:
        /// - Payment Date
        /// - Amount
        /// - Payment Mode
        /// - Bank Name
        /// - Reference Number
        /// - Remarks
        ///
        /// Fixed:
        /// - Supplier Payment No.
        /// - Purchase Invoice
        /// - Supplier
        /// - Company
        ///
        /// Maximum edited amount is calculated as:
        ///
        /// Invoice Total
        /// - Sum(other active transactions)
        ///
        /// Example:
        ///
        /// Invoice Total        = ₹30,000
        /// Other Transactions   = ₹15,000
        /// Current Transaction  = ₹10,000
        ///
        /// Maximum Edit Amount  = ₹15,000
        /// </summary>
        Task<SupplierPaymentTransaction> UpdateTransactionAsync(
            int supplierPaymentId,
            int transactionId,
            SupplierPaymentTransaction transaction);

        #endregion


        // =====================================================
        // DELETE / RESTORE
        // =====================================================

        #region Delete / Restore

        /// <summary>
        /// Soft-deletes the Supplier Payment header.
        ///
        /// Existing transaction history is preserved.
        /// </summary>
        Task DeleteAsync(
            int id);


        /// <summary>
        /// Restores a soft-deleted Supplier Payment.
        ///
        /// Existing Payment No. and transaction history
        /// are preserved.
        /// </summary>
        Task RestoreAsync(
            int id);

        #endregion
    }
}