// =============================================================
// File: ISupplierPaymentRepository.cs
// Module: Supplier Payment
// Layer: Application - Repository Interface
//
// Purpose:
// Defines persistence operations required by the redesigned
// Supplier Payment module.
//
// Final Structure:
//
// PurchaseInvoice
//      ↓ 1 : 1
// SupplierPayment
//      ↓ 1 : Many
// SupplierPaymentTransaction
//
// Important Business Rules:
// - One Purchase Invoice can have only one SupplierPayment.
// - Multiple transactions can exist under the same Payment No.
// - Paid Amount is calculated from active transactions.
// - Outstanding Amount is calculated live.
// - A soft-deleted SupplierPayment still owns its
//   PurchaseInvoiceId and must be restored instead of creating
//   another SupplierPayment header.
// - Only Finalized Purchase Invoices are eligible for payment.
// =============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ISupplierPaymentRepository
    {
        // =====================================================
        // BASIC READ
        // =====================================================

        #region Basic Read

        /// <summary>
        /// Returns one active Supplier Payment including:
        /// - Purchase Invoice
        /// - Supplier
        /// - Company
        /// - Active Transactions
        /// </summary>
        Task<SupplierPayment?> GetByIdAsync(
            int id);


        /// <summary>
        /// Returns active Supplier Payment for a
        /// specific Purchase Invoice.
        /// </summary>
        Task<SupplierPayment?> GetByPurchaseInvoiceIdAsync(
            int purchaseInvoiceId);

        #endregion


        // =====================================================
        // INDEX / SEARCH
        // =====================================================

        #region Index / Search

        /// <summary>
        /// Returns paged active Supplier Payments.
        /// </summary>
        Task<PagedResult<SupplierPayment>> GetPagedAsync(
            int pageNumber,
            int pageSize);


        /// <summary>
        /// Searches Supplier Payments.
        ///
        /// Search will support:
        /// - Supplier Payment No.
        /// - ERP Purchase Invoice No.
        /// - Supplier Invoice No.
        /// - Supplier Name
        /// - Transaction Payment Date
        /// - Payment Mode
        /// - Bank Name
        ///
        /// Reference Number is not displayed on Index,
        /// but may still remain searchable if required
        /// by repository implementation.
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
        /// Returns Finalized Purchase Invoices eligible
        /// for creation of a NEW Supplier Payment header.
        ///
        /// An invoice is excluded if any SupplierPayment
        /// header already exists for it, including a
        /// soft-deleted SupplierPayment.
        /// </summary>
        Task<List<PurchaseInvoice>>
            GetAvailablePurchaseInvoicesAsync();


        /// <summary>
        /// Returns one Purchase Invoice for validation
        /// while creating a Supplier Payment.
        /// </summary>
        Task<PurchaseInvoice?> GetPurchaseInvoiceForPaymentAsync(
            int purchaseInvoiceId);


        /// <summary>
        /// Checks whether any SupplierPayment header already
        /// exists for the Purchase Invoice.
        ///
        /// IMPORTANT:
        /// This check includes soft-deleted SupplierPayments.
        ///
        /// This prevents:
        ///
        /// PI-001 -> SPAY-001
        /// PI-001 -> SPAY-002
        ///
        /// If SPAY-001 is deleted, it must be restored.
        /// </summary>
        Task<bool> ExistsForPurchaseInvoiceAsync(
            int purchaseInvoiceId);

        #endregion


        // =====================================================
        // PAID / OUTSTANDING CALCULATION
        // =====================================================

        #region Payment Calculation

        /// <summary>
        /// Returns total active transaction amount for the
        /// Supplier Payment.
        ///
        /// Paid Amount =
        /// SUM(
        ///     Active +
        ///     Non-deleted Transactions
        /// )
        /// </summary>
        Task<decimal> GetPaidAmountAsync(
            int supplierPaymentId);

        #endregion


        // =====================================================
        // CREATE
        // =====================================================

        #region Create

        /// <summary>
        /// Creates the Supplier Payment header.
        ///
        /// Normally the Service creates this together with
        /// the first SupplierPaymentTransaction.
        /// </summary>
        Task AddAsync(
            SupplierPayment supplierPayment);


        /// <summary>
        /// Adds another actual payment transaction under
        /// the existing Supplier Payment number.
        ///
        /// Example:
        ///
        /// SPAY-001
        ///   Transaction 1 -> ₹10,000
        ///   Transaction 2 -> ₹10,000
        ///   Transaction 3 -> ₹10,000
        /// </summary>
        Task AddTransactionAsync(
            SupplierPaymentTransaction transaction);

        #endregion


        // =====================================================
        // UPDATE / DELETE / RESTORE
        // =====================================================

        #region Update / Delete / Restore

        /// <summary>
        /// Returns an active Supplier Payment as tracked
        /// entity for Delete / other controlled updates.
        /// </summary>
        Task<SupplierPayment?> GetForUpdateAsync(
            int id);


        /// <summary>
        /// Saves changes to Supplier Payment header.
        /// </summary>
        Task UpdateAsync(
            SupplierPayment supplierPayment);


        /// <summary>
        /// Returns all soft-deleted Supplier Payments.
        /// </summary>
        Task<List<SupplierPayment>> GetDeletedAsync();


        /// <summary>
        /// Returns one deleted Supplier Payment including
        /// its transactions for Restore validation.
        /// </summary>
        Task<SupplierPayment?> GetDeletedForUpdateAsync(
            int id);

        #endregion


        // =====================================================
        // PAYMENT NUMBER GENERATION
        // =====================================================

        #region Payment Number Generation

        /// <summary>
        /// Returns last Supplier Payment Code for the
        /// supplied financial-year prefix.
        ///
        /// Example Prefix:
        /// AI/SPAY/26-27/
        /// </summary>
        Task<string?> GetLastCodeAsync(
            string prefix);

        #endregion
    }
}