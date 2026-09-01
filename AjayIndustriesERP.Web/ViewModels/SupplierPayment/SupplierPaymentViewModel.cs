// =============================================================
// File: SupplierPaymentViewModel.cs
// Module: Supplier Payment
// Layer: Web - ViewModels
//
// Purpose:
// Contains all ViewModels required by the redesigned
// Supplier Payment module.
//
// Final Structure:
//
// PurchaseInvoice
//      ↓
// SupplierPayment
//      ↓
// SupplierPaymentTransaction (1 : Many)
//
// Important Business Rules:
// - One Purchase Invoice has one Supplier Payment number.
// - Multiple actual payments remain under the same Payment No.
// - Company and Supplier are derived from Purchase Invoice.
// - Company is NOT manually selected by the user.
// - Payment Date / Amount / Mode / Bank / Reference / Remarks
//   belong to individual transactions.
// - Paid Amount, Outstanding and Payment Status are calculated.
// =============================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.ViewModels.SupplierPayment
{
    // =========================================================
    // INDEX
    // =========================================================

    #region Index

    /// <summary>
    /// One row on Supplier Payment Index.
    ///
    /// One row represents one Purchase Invoice /
    /// Supplier Payment account, regardless of how many
    /// payment transactions exist underneath it.
    /// </summary>
    public class SupplierPaymentIndexViewModel
    {
        public int Id { get; set; }


        // =====================================================
        // PAYMENT
        // =====================================================

        public string Code { get; set; }
            = string.Empty;


        // =====================================================
        // PURCHASE INVOICE
        // =====================================================

        public int PurchaseInvoiceId { get; set; }


        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;


        public string SupplierInvoiceNumber { get; set; }
            = string.Empty;


        // =====================================================
        // SUPPLIER
        // =====================================================

        public int SupplierId { get; set; }


        public string SupplierName { get; set; }
            = string.Empty;


        // =====================================================
        // AMOUNTS
        // =====================================================

        public decimal InvoiceTotal { get; set; }


        public decimal PaidAmount { get; set; }


        public decimal OutstandingAmount { get; set; }


        // =====================================================
        // PAYMENT STATUS
        // =====================================================

        /// <summary>
        /// Calculated values:
        ///
        /// Pending
        /// Partially Paid
        /// Completed
        /// </summary>
        public string PaymentStatus { get; set; }
            = string.Empty;


        // =====================================================
        // LAST TRANSACTION
        // =====================================================

        public DateTime? LastPaymentDate { get; set; }
    }

    #endregion


    // =========================================================
    // CREATE FIRST PAYMENT
    // =========================================================

    #region Create Payment

    /// <summary>
    /// Used when creating a new Supplier Payment account
    /// against a Finalized Purchase Invoice.
    ///
    /// This operation creates:
    /// 1. SupplierPayment header
    /// 2. First SupplierPaymentTransaction
    /// </summary>
    public class SupplierPaymentCreateViewModel
    {
        // =====================================================
        // PURCHASE INVOICE SELECTION
        // =====================================================

        [Display(Name = "Purchase Invoice")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Purchase Invoice.")]
        public int PurchaseInvoiceId { get; set; }


        /// <summary>
        /// Only Finalized Purchase Invoices which do not
        /// already have a Supplier Payment are included.
        /// </summary>
        public List<SelectListItem>
            PurchaseInvoices
        { get; set; }
                = new();


        // =====================================================
        // PURCHASE INVOICE PREVIEW
        //
        // Filled after invoice selection.
        // User does not edit these values.
        // =====================================================

        public string? PurchaseInvoiceCode { get; set; }


        public string? SupplierInvoiceNumber { get; set; }


        public string? SupplierName { get; set; }


        public DateTime? PurchaseInvoiceDate { get; set; }


        public DateTime? DueDate { get; set; }


        public decimal InvoiceTotal { get; set; }


        // =====================================================
        // FIRST TRANSACTION
        // =====================================================

        public SupplierPaymentTransactionInputViewModel
            Transaction
        { get; set; }
                = new();


        // =====================================================
        // PAYMENT MODES
        // =====================================================

        public List<SelectListItem>
            PaymentModes
        { get; set; }
                = new();
    }

    #endregion


    // =========================================================
    // ADD TRANSACTION
    // =========================================================

    #region Add Transaction

    /// <summary>
    /// Used to add another payment transaction under
    /// an existing Supplier Payment number.
    ///
    /// No new Supplier Payment Code is generated.
    /// </summary>
    public class SupplierPaymentAddTransactionViewModel
    {
        // =====================================================
        // SUPPLIER PAYMENT
        // =====================================================

        public int SupplierPaymentId { get; set; }


        public string PaymentCode { get; set; }
            = string.Empty;


        // =====================================================
        // PURCHASE INVOICE
        // =====================================================

        public int PurchaseInvoiceId { get; set; }


        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;


        public string SupplierInvoiceNumber { get; set; }
            = string.Empty;


        public string SupplierName { get; set; }
            = string.Empty;


        public DateTime PurchaseInvoiceDate { get; set; }


        public DateTime? DueDate { get; set; }


        // =====================================================
        // CURRENT PAYMENT POSITION
        // =====================================================

        public decimal InvoiceTotal { get; set; }


        public decimal PaidAmount { get; set; }


        public decimal OutstandingAmount { get; set; }


        public string PaymentStatus { get; set; }
            = string.Empty;


        // =====================================================
        // NEW TRANSACTION
        // =====================================================

        public SupplierPaymentTransactionInputViewModel
            Transaction
        { get; set; }
                = new();


        // =====================================================
        // PAYMENT MODES
        // =====================================================

        public List<SelectListItem>
            PaymentModes
        { get; set; }
                = new();
    }

    #endregion


    // =========================================================
    // TRANSACTION INPUT
    // =========================================================

    #region Transaction Input

    /// <summary>
    /// Common transaction input used for:
    ///
    /// - First Payment
    /// - Add Payment
    /// </summary>
    public class SupplierPaymentTransactionInputViewModel
    {
        // =====================================================
        // PAYMENT DATE
        // =====================================================

        [Required(
            ErrorMessage = "Payment Date is required.")]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }
            = DateTime.Today;


        // =====================================================
        // PAYMENT AMOUNT
        // =====================================================

        [Required(
            ErrorMessage = "Payment Amount is required.")]
        [Display(Name = "Payment Amount")]
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage =
                "Payment Amount must be greater than zero.")]
        public decimal Amount { get; set; }


        // =====================================================
        // PAYMENT MODE
        // =====================================================

        [Required(
            ErrorMessage = "Payment Mode is required.")]
        [Display(Name = "Payment Mode")]
        [StringLength(
            50,
            ErrorMessage =
                "Payment Mode cannot exceed 50 characters.")]
        public string PaymentMode { get; set; }
            = string.Empty;


        // =====================================================
        // BANK
        // =====================================================

        [Display(Name = "Bank Name")]
        [StringLength(
            150,
            ErrorMessage =
                "Bank Name cannot exceed 150 characters.")]
        public string? BankName { get; set; }


        // =====================================================
        // REFERENCE
        // =====================================================

        [Display(Name = "UTR / Cheque / Reference No.")]
        [StringLength(
            150,
            ErrorMessage =
                "Reference Number cannot exceed 150 characters.")]
        public string? ReferenceNumber { get; set; }


        // =====================================================
        // REMARKS
        // =====================================================

        [Display(Name = "Remarks")]
        [StringLength(
            1000,
            ErrorMessage =
                "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }
    }

    #endregion


    // =========================================================
    // DETAILS
    // =========================================================

    #region Details

    /// <summary>
    /// Complete Supplier Payment Details page.
    ///
    /// Displays one Supplier Payment header and its
    /// complete transaction history.
    /// </summary>
    public class SupplierPaymentDetailsViewModel
    {
        // =====================================================
        // PAYMENT HEADER
        // =====================================================

        public int Id { get; set; }


        public string Code { get; set; }
            = string.Empty;


        // =====================================================
        // PURCHASE INVOICE
        // =====================================================

        public int PurchaseInvoiceId { get; set; }


        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;


        public string SupplierInvoiceNumber { get; set; }
            = string.Empty;


        public DateTime PurchaseInvoiceDate { get; set; }


        public DateTime SupplierInvoiceDate { get; set; }


        public DateTime? DueDate { get; set; }


        // =====================================================
        // SUPPLIER
        // =====================================================

        public int SupplierId { get; set; }


        public string SupplierName { get; set; }
            = string.Empty;


        // =====================================================
        // PAYMENT POSITION
        // =====================================================

        public decimal InvoiceTotal { get; set; }


        public decimal PaidAmount { get; set; }


        public decimal OutstandingAmount { get; set; }


        public string PaymentStatus { get; set; }
            = string.Empty;


        // =====================================================
        // TRANSACTION HISTORY
        // =====================================================

        public List<SupplierPaymentTransactionRowViewModel>
            Transactions
        { get; set; }
                = new();
    }

    #endregion


    // =========================================================
    // TRANSACTION HISTORY ROW
    // =========================================================

    #region Transaction History

    /// <summary>
    /// One actual payment transaction displayed
    /// in Supplier Payment Details.
    /// </summary>
    public class SupplierPaymentTransactionRowViewModel
    {
        public int Id { get; set; }


        public DateTime PaymentDate { get; set; }


        public decimal Amount { get; set; }


        public string PaymentMode { get; set; }
            = string.Empty;


        public string? BankName { get; set; }


        public string? ReferenceNumber { get; set; }


        public string? Remarks { get; set; }
    }

    #endregion


    // =========================================================
    // DELETED LIST
    // =========================================================

    #region Deleted List

    /// <summary>
    /// One row on Deleted Supplier Payments page.
    ///
    /// Existing Payment No. and complete transaction
    /// history remain preserved after soft delete.
    /// </summary>
    public class SupplierPaymentDeletedViewModel
    {
        public int Id { get; set; }


        public string Code { get; set; }
            = string.Empty;


        public int PurchaseInvoiceId { get; set; }


        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;


        public string SupplierInvoiceNumber { get; set; }
            = string.Empty;


        public string SupplierName { get; set; }
            = string.Empty;


        public decimal InvoiceTotal { get; set; }


        public decimal PaidAmount { get; set; }


        public decimal OutstandingAmount { get; set; }


        public string PaymentStatus { get; set; }
            = string.Empty;


        public DateTime? DeletedOn { get; set; }
    }

    #endregion
}