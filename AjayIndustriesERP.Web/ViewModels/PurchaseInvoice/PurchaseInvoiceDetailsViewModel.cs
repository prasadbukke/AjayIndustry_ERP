/*
============================================================
File: PurchaseInvoiceDetailsViewModel.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
ViewModel used by Purchase Invoice Details screen.

Responsibilities:
- Display ERP Purchase Invoice information.
- Display Supplier's actual Invoice information.
- Display attached Supplier Invoice PDF information.
- Display Purchase Order reference.
- Display Supplier / Company snapshot information.
- Display Payment Terms and Due Date.
- Display GRN-wise billed material traceability.
- Display GST and financial totals.
- Display workflow / finalization information.

Important:
- This is display-only ViewModel.
- Purchase Invoice Details shows:
      Purchase Order
      → GRN
      → Purchase Invoice
      → Supplier Invoice PDF
============================================================
*/

namespace AjayIndustriesERP.Web.ViewModels.PurchaseInvoice
{
    public class PurchaseInvoiceDetailsViewModel
    {
        // =====================================================
        // IDENTIFICATION
        // =====================================================

        public int Id
        {
            get;
            set;
        }


        public string Code
        {
            get;
            set;
        } = string.Empty;


        public DateTime PurchaseInvoiceDate
        {
            get;
            set;
        }


        public string Status
        {
            get;
            set;
        } = string.Empty;


        // =====================================================
        // SUPPLIER INVOICE
        // =====================================================

        public string SupplierInvoiceNumber
        {
            get;
            set;
        } = string.Empty;


        public DateTime SupplierInvoiceDate
        {
            get;
            set;
        }


        // =====================================================
        // SUPPLIER INVOICE PDF
        // =====================================================

        /*
         * Relative web path.
         *
         * Example:
         * /uploads/purchase-invoices/xxxx.pdf
         */
        public string? SupplierInvoicePdfPath
        {
            get;
            set;
        }


        /*
         * Original filename uploaded by user.
         */
        public string? SupplierInvoicePdfOriginalName
        {
            get;
            set;
        }


        public DateTime? SupplierInvoicePdfUploadedOn
        {
            get;
            set;
        }


        public bool HasSupplierInvoicePdf =>
            !string.IsNullOrWhiteSpace(
                SupplierInvoicePdfPath);


        // =====================================================
        // PURCHASE ORDER
        // =====================================================

        public int PurchaseOrderId
        {
            get;
            set;
        }


        public string PurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;


        // =====================================================
        // SUPPLIER
        // =====================================================

        public int SupplierId
        {
            get;
            set;
        }


        public string SupplierName
        {
            get;
            set;
        } = string.Empty;


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


        public string? SupplierPan
        {
            get;
            set;
        }


        public string? SupplierContactPerson
        {
            get;
            set;
        }


        public string? SupplierMobileNumber
        {
            get;
            set;
        }


        public string? SupplierEmail
        {
            get;
            set;
        }


        public string? SupplierAddress
        {
            get;
            set;
        }


        public string? SupplierState
        {
            get;
            set;
        }


        // =====================================================
        // COMPANY
        // =====================================================

        public int CompanyId
        {
            get;
            set;
        }


        public string CompanyName
        {
            get;
            set;
        } = string.Empty;


        public string? CompanyGstin
        {
            get;
            set;
        }


        public string? CompanyPan
        {
            get;
            set;
        }


        public string? CompanyAddress
        {
            get;
            set;
        }


        public string? CompanyState
        {
            get;
            set;
        }


        public string? CompanyPhone
        {
            get;
            set;
        }


        public string? CompanyEmail
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


        public string GstType
        {
            get
            {
                return IsInterState
                    ? "IGST"
                    : "CGST + SGST";
            }
        }


        // =====================================================
        // FINANCIAL TOTALS
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


        public decimal TransportCharges
        {
            get;
            set;
        }


        public decimal OtherCharges
        {
            get;
            set;
        }


        public decimal RoundOffAmount
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

        public string? Remarks
        {
            get;
            set;
        }


        // =====================================================
        // FINALIZATION
        // =====================================================

        public DateTime? FinalizedOn
        {
            get;
            set;
        }


        public string? FinalizedBy
        {
            get;
            set;
        }


        // =====================================================
        // AUDIT
        // =====================================================

        public DateTime CreatedOn
        {
            get;
            set;
        }


        public string? CreatedBy
        {
            get;
            set;
        }


        public DateTime? ModifiedOn
        {
            get;
            set;
        }


        public string? ModifiedBy
        {
            get;
            set;
        }


        // =====================================================
        // ITEMS
        // =====================================================

        public List<PurchaseInvoiceDetailsItemViewModel>
            Items
        {
            get;
            set;
        } = new();
    }


    // =========================================================
    // PURCHASE INVOICE DETAILS ITEM
    // =========================================================

    public class PurchaseInvoiceDetailsItemViewModel
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
        // PURCHASE ORDER SOURCE
        // =====================================================

        public int PurchaseOrderItemId
        {
            get;
            set;
        }


        public string PurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;


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


        public string GoodsReceiptNoteCode
        {
            get;
            set;
        } = string.Empty;


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


        public string? MaterialStatus
        {
            get;
            set;
        }


        // =====================================================
        // ITEM
        // =====================================================

        public int ItemId
        {
            get;
            set;
        }


        public string ItemCode
        {
            get;
            set;
        } = string.Empty;


        public string ItemName
        {
            get;
            set;
        } = string.Empty;


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
        // QUANTITY
        // =====================================================

        public decimal PurchaseInvoiceQuantity
        {
            get;
            set;
        }


        // =====================================================
        // COMMERCIAL
        // =====================================================

        /*
         * Actual Supplier Invoice Rate entered by user.
         */
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
    }
}