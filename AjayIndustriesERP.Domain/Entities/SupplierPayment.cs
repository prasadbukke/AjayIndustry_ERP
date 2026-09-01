// =============================================================
// File: SupplierPayment.cs
// Module: Supplier Payment
// Layer: Domain
//
// Purpose:
// Represents one Supplier Payment account/header against
// one Finalized Purchase Invoice.
//
// Example:
//
// Purchase Invoice Total = ₹30,000
//
// SupplierPayment
// AI/SPAY/26-27/00001
//
// Transactions:
// 01-09-2026 -> ₹10,000
// 10-09-2026 -> ₹10,000
// 20-09-2026 -> ₹10,000
//
// Total Paid   = ₹30,000
// Outstanding  = ₹0
// Status       = Completed
//
// Important Business Rules:
// - One Purchase Invoice has only one SupplierPayment header.
// - One SupplierPayment can have multiple payment transactions.
// - Payment Mode / Bank / UTR / Amount / Payment Date belong
//   to SupplierPaymentTransaction, not this header.
// - Paid Amount is calculated from active transactions.
// - Outstanding Amount is calculated live:
//
//   PurchaseInvoice.GrandTotal
//   - Active SupplierPaymentTransaction Amounts
//
// - Payment status is NOT stored in this entity.
//   It is calculated:
//
//   Paid = 0
//   -> Pending
//
//   Paid > 0 and Paid < Invoice Total
//   -> Partially Paid
//
//   Paid >= Invoice Total
//   -> Completed
//
// - Outstanding and Paid Amount are NOT stored separately.
// - Supplier Payment delete remains Soft Delete.
// =============================================================

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class SupplierPayment : BaseEntity
    {
        // =====================================================
        // PRIMARY KEY
        // =====================================================

        #region Primary Key

        public int Id { get; set; }

        #endregion


        // =====================================================
        // PAYMENT IDENTIFICATION
        // =====================================================

        #region Payment Identification

        /// <summary>
        /// ERP generated Supplier Payment number.
        ///
        /// Example:
        /// AI/SPAY/26-27/00001
        ///
        /// This number remains the same for all transactions
        /// made against the linked Purchase Invoice.
        /// </summary>
        public string Code { get; set; }
            = string.Empty;

        #endregion


        // =====================================================
        // PURCHASE INVOICE
        // =====================================================

        #region Purchase Invoice

        /// <summary>
        /// Finalized Purchase Invoice against which
        /// this Supplier Payment account is maintained.
        ///
        /// One Purchase Invoice can have only one
        /// SupplierPayment header.
        /// </summary>
        public int PurchaseInvoiceId { get; set; }


        public PurchaseInvoice? PurchaseInvoice { get; set; }

        #endregion


        // =====================================================
        // SUPPLIER
        // =====================================================

        #region Supplier

        /// <summary>
        /// Supplier belonging to the Purchase Invoice.
        ///
        /// This value is derived and validated from
        /// PurchaseInvoice.SupplierId.
        /// </summary>
        public int SupplierId { get; set; }


        public Supplier? Supplier { get; set; }

        #endregion


        // =====================================================
        // COMPANY
        // =====================================================

        #region Company

        /// <summary>
        /// Company belonging to the Purchase Invoice.
        ///
        /// User does NOT manually select Company while
        /// creating Supplier Payment.
        ///
        /// CompanyId is derived automatically from:
        /// PurchaseInvoice.CompanyId
        /// </summary>
        public int CompanyId { get; set; }


        public Company? Company { get; set; }

        #endregion


        // =====================================================
        // PAYMENT TRANSACTIONS
        // =====================================================

        #region Payment Transactions

        /// <summary>
        /// Actual payments made against this Purchase Invoice.
        ///
        /// Example:
        ///
        /// Transaction 1 -> ₹10,000
        /// Transaction 2 -> ₹10,000
        /// Transaction 3 -> ₹10,000
        ///
        /// All transactions remain under the same
        /// Supplier Payment Number.
        /// </summary>
        public ICollection<SupplierPaymentTransaction>
            Transactions
        { get; set; }
                = new List<SupplierPaymentTransaction>();

        #endregion
    }
}