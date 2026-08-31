using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.PurchaseInvoice
{
    public class PurchaseInvoiceFormViewModel
    {
        // =====================================================
        // IDENTIFICATION
        // =====================================================

        public int Id
        {
            get;
            set;
        }


        public string? Code
        {
            get;
            set;
        }


        // =====================================================
        // PURCHASE ORDER
        // =====================================================

        [Display(
            Name = "Purchase Order")]
        public int PurchaseOrderId
        {
            get;
            set;
        }


        public string? PurchaseOrderCode
        {
            get;
            set;
        }


        public List<SelectListItem>
            AvailablePurchaseOrders
        {
            get;
            set;
        } = new();


        // =====================================================
        // PURCHASE INVOICE DATE
        // =====================================================

        [Required]
        [DataType(
            DataType.Date)]
        [Display(
            Name = "Purchase Invoice Date")]
        public DateTime PurchaseInvoiceDate
        {
            get;
            set;
        } = DateTime.Today;


        // =====================================================
        // SUPPLIER INVOICE
        // =====================================================

        [Required]
        [StringLength(
            100)]
        [Display(
            Name = "Supplier Invoice No.")]
        public string SupplierInvoiceNumber
        {
            get;
            set;
        } = string.Empty;


        [Required]
        [DataType(
            DataType.Date)]
        [Display(
            Name = "Supplier Invoice Date")]
        public DateTime SupplierInvoiceDate
        {
            get;
            set;
        } = DateTime.Today;


        // =====================================================
        // SUPPLIER INVOICE PDF
        // =====================================================

        /*
         * New PDF selected from browser.
         *
         * Optional.
         *
         * Validation of:
         * - .pdf extension
         * - MIME type
         * - maximum file size
         *
         * will be done in Controller/File helper.
         */
        [Display(
            Name = "Supplier Invoice PDF")]
        public IFormFile? SupplierInvoicePdf
        {
            get;
            set;
        }


        /*
         * Existing PDF information used mainly during Edit.
         *
         * We do NOT trust these posted values for saving.
         * Controller reloads actual existing attachment
         * information from PurchaseInvoice entity.
         */
        public string? ExistingSupplierInvoicePdfPath
        {
            get;
            set;
        }


        public string? ExistingSupplierInvoicePdfOriginalName
        {
            get;
            set;
        }


        public DateTime? ExistingSupplierInvoicePdfUploadedOn
        {
            get;
            set;
        }


        public bool HasExistingSupplierInvoicePdf =>
            !string.IsNullOrWhiteSpace(
                ExistingSupplierInvoicePdfPath);


        // =====================================================
        // SUPPLIER DISPLAY
        // =====================================================

        public int SupplierId
        {
            get;
            set;
        }


        [Display(
            Name = "Supplier")]
        public string? SupplierName
        {
            get;
            set;
        }


        public string? SupplierCode
        {
            get;
            set;
        }


        public string? SupplierGstin
        {
            get;
            set;
        }


        public string? SupplierState
        {
            get;
            set;
        }


        public string? SupplierAddress
        {
            get;
            set;
        }


        // =====================================================
        // COMPANY DISPLAY
        // =====================================================

        public int CompanyId
        {
            get;
            set;
        }


        public string? CompanyName
        {
            get;
            set;
        }


        public string? CompanyGstin
        {
            get;
            set;
        }


        public string? CompanyState
        {
            get;
            set;
        }


        // =====================================================
        // PAYMENT INFORMATION
        // =====================================================

        public string? PaymentTerms
        {
            get;
            set;
        }


        public int? CreditDays
        {
            get;
            set;
        }


        [DataType(
            DataType.Date)]
        public DateTime? DueDate
        {
            get;
            set;
        }


        // =====================================================
        // GST
        // =====================================================

        public string? PlaceOfSupply
        {
            get;
            set;
        }


        public bool IsInterState
        {
            get;
            set;
        }


        // =====================================================
        // CHARGES
        // =====================================================

        [Range(
            typeof(decimal),
            "0",
            "999999999999.99")]
        [Display(
            Name = "Transport Charges")]
        public decimal TransportCharges
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999.99")]
        [Display(
            Name = "Other Charges")]
        public decimal OtherCharges
        {
            get;
            set;
        }


        /*
         * Actual Supplier Invoice Round Off.
         *
         * Can be positive or negative.
         */
        [Display(
            Name = "Round Off")]
        public decimal RoundOffAmount
        {
            get;
            set;
        }


        // =====================================================
        // TOTALS
        // =====================================================

        public decimal GrossAmount
        {
            get;
            set;
        }


        public decimal DiscountAmount
        {
            get;
            set;
        }


        public decimal TaxableAmount
        {
            get;
            set;
        }


        public decimal CgstAmount
        {
            get;
            set;
        }


        public decimal SgstAmount
        {
            get;
            set;
        }


        public decimal IgstAmount
        {
            get;
            set;
        }


        public decimal GrandTotal
        {
            get;
            set;
        }


        // =====================================================
        // REMARKS
        // =====================================================

        [StringLength(
            2000)]
        public string? Remarks
        {
            get;
            set;
        }


        // =====================================================
        // STATUS
        // =====================================================

        public string? Status
        {
            get;
            set;
        }


        // =====================================================
        // ITEMS
        // =====================================================

        public List<PurchaseInvoiceFormItemViewModel>
            Items
        {
            get;
            set;
        } = new();
    }


    // =========================================================
    // PURCHASE INVOICE FORM ITEM
    // =========================================================

    public class PurchaseInvoiceFormItemViewModel
    {
        // =====================================================
        // IDENTIFICATION
        // =====================================================

        public int Id
        {
            get;
            set;
        }


        public int SequenceNumber
        {
            get;
            set;
        }


        // =====================================================
        // SELECTION
        // =====================================================

        public bool IsSelected
        {
            get;
            set;
        } = true;


        // =====================================================
        // PURCHASE ORDER SOURCE
        // =====================================================

        public int PurchaseOrderItemId
        {
            get;
            set;
        }


        public string? PurchaseOrderCode
        {
            get;
            set;
        }


        public decimal PurchaseOrderQuantity
        {
            get;
            set;
        }


        // =====================================================
        // GRN SOURCE
        // =====================================================

        public int GoodsReceiptNoteId
        {
            get;
            set;
        }


        public string? GoodsReceiptNoteCode
        {
            get;
            set;
        }


        public DateTime? GoodsReceiptNoteDate
        {
            get;
            set;
        }


        public int GoodsReceiptNoteItemId
        {
            get;
            set;
        }


        public decimal GoodsReceiptQuantity
        {
            get;
            set;
        }


        public string? SupplierChallanNumber
        {
            get;
            set;
        }


        public DateTime? SupplierChallanDate
        {
            get;
            set;
        }


        // =====================================================
        // QUANTITY AVAILABILITY
        // =====================================================

        public decimal AlreadyBilledQuantity
        {
            get;
            set;
        }


        public decimal AvailableQuantity
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999.999")]
        [Display(
            Name = "Invoice Qty")]
        public decimal PurchaseInvoiceQuantity
        {
            get;
            set;
        }


        // =====================================================
        // ITEM SNAPSHOT
        // =====================================================

        public int ItemId
        {
            get;
            set;
        }


        public string? ItemCode
        {
            get;
            set;
        }


        public string? ItemName
        {
            get;
            set;
        }


        public string? Description
        {
            get;
            set;
        }


        public string? Specification
        {
            get;
            set;
        }


        public string? UnitName
        {
            get;
            set;
        }


        public string? HsnCode
        {
            get;
            set;
        }


        // =====================================================
        // DRAWING
        // =====================================================

        public int? DrawingId
        {
            get;
            set;
        }


        public string? DrawingNumber
        {
            get;
            set;
        }


        public string? DrawingRevision
        {
            get;
            set;
        }


        // =====================================================
        // COMMERCIAL
        // =====================================================

        /*
         * Actual Rate from Supplier Invoice.
         *
         * User enters this manually.
         * It is NOT automatically trusted from Purchase Order.
         */
        [Range(
            typeof(decimal),
            "0",
            "999999999999.99")]
        public decimal Rate
        {
            get;
            set;
        }


        public decimal GrossAmount
        {
            get;
            set;
        }


        public decimal DiscountPercent
        {
            get;
            set;
        }


        public decimal DiscountAmount
        {
            get;
            set;
        }


        public decimal TaxableAmount
        {
            get;
            set;
        }


        // =====================================================
        // GST
        // =====================================================

        public decimal GstRate
        {
            get;
            set;
        }


        public decimal CgstRate
        {
            get;
            set;
        }


        public decimal SgstRate
        {
            get;
            set;
        }


        public decimal IgstRate
        {
            get;
            set;
        }


        public decimal CgstAmount
        {
            get;
            set;
        }


        public decimal SgstAmount
        {
            get;
            set;
        }


        public decimal IgstAmount
        {
            get;
            set;
        }


        public decimal TotalTaxAmount
        {
            get;
            set;
        }


        // =====================================================
        // TOTAL
        // =====================================================

        public decimal LineTotal
        {
            get;
            set;
        }


        // =====================================================
        // MATERIAL STATUS DISPLAY
        // =====================================================

        /*
         * Informational only in current Phase.
         */
        public string? MaterialStatus
        {
            get;
            set;
        }
    }
}