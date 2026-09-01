// =============================================================
// File: SupplierPaymentTransaction.cs
// Module: Supplier Payment
// Layer: Domain
//
// Purpose:
// Represents one actual payment transaction made against
// a Supplier Payment / Purchase Invoice.
//
// Example:
//
// Purchase Invoice Total = ₹30,000
//
// Supplier Payment:
// AI/SPAY/26-27/00001
//
// Transactions:
// 01-09-2026 -> ₹10,000
// 10-09-2026 -> ₹10,000
// 20-09-2026 -> ₹10,000
//
// All three transactions belong to the same SupplierPayment.
//
// Responsibilities:
// - Store actual Payment Date
// - Store actual Amount paid
// - Store Payment Mode
// - Store Bank Name
// - Store UTR / Cheque / Transaction Reference
// - Store transaction-specific Remarks
//
// Important Business Rules:
// - Transaction Amount must be greater than zero.
// - Transaction Amount cannot exceed current Outstanding.
// - Multiple transactions are allowed under one SupplierPayment.
// - Fully paid invoice cannot accept another transaction.
// - Deleted transactions do not count toward Paid Amount.
// - Paid Amount is calculated from active transactions only.
// - Outstanding is calculated live and is not stored.
//
// Paid Amount:
//
// Sum(
//     Active + Non-deleted
//     SupplierPaymentTransaction.Amount
// )
//
// Outstanding:
//
// PurchaseInvoice.GrandTotal
// - Paid Amount
//
// =============================================================

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class SupplierPaymentTransaction : BaseEntity
    {
        // =====================================================
        // PRIMARY KEY
        // =====================================================

        #region Primary Key

        public int Id { get; set; }

        #endregion


        // =====================================================
        // SUPPLIER PAYMENT
        // =====================================================

        #region Supplier Payment

        /// <summary>
        /// Parent Supplier Payment.
        ///
        /// Multiple transactions can belong to the
        /// same Supplier Payment number.
        /// </summary>
        public int SupplierPaymentId { get; set; }


        public SupplierPayment? SupplierPayment { get; set; }

        #endregion


        // =====================================================
        // TRANSACTION DATE
        // =====================================================

        #region Transaction Date

        /// <summary>
        /// Actual date on which payment was made.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        #endregion


        // =====================================================
        // TRANSACTION AMOUNT
        // =====================================================

        #region Transaction Amount

        /// <summary>
        /// Actual amount paid in this transaction.
        ///
        /// Example:
        /// ₹10,000
        ///
        /// This value must not exceed the current
        /// Purchase Invoice Outstanding amount.
        /// </summary>
        public decimal Amount { get; set; }

        #endregion


        // =====================================================
        // PAYMENT MODE
        // =====================================================

        #region Payment Mode

        /// <summary>
        /// Payment mode used for this transaction.
        ///
        /// Examples:
        /// Cash
        /// Bank Transfer
        /// UPI
        /// Cheque
        /// NEFT
        /// RTGS
        /// </summary>
        public string PaymentMode { get; set; }
            = string.Empty;

        #endregion


        // =====================================================
        // BANK INFORMATION
        // =====================================================

        #region Bank Information

        /// <summary>
        /// Bank name when applicable.
        ///
        /// Optional for Cash payments.
        /// </summary>
        public string? BankName { get; set; }


        /// <summary>
        /// Transaction reference.
        ///
        /// Examples:
        /// UTR Number
        /// Cheque Number
        /// UPI Reference
        /// Bank Transaction Number
        /// </summary>
        public string? ReferenceNumber { get; set; }

        #endregion


        // =====================================================
        // REMARKS
        // =====================================================

        #region Remarks

        /// <summary>
        /// Optional remarks specific to this transaction.
        /// </summary>
        public string? Remarks { get; set; }

        #endregion
    }
}