/*
============================================================
File: PurchaseInvoice.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Represents Supplier's actual Invoice / Bill recorded
against material received through GRN.

Business Flow:
Purchase Order
    → GRN
    → Purchase Invoice
    → Supplier Payment
    → Supplier Outstanding

Important:
- One Purchase Invoice belongs to one Purchase Order.
- Multiple GRN lines of that PO may be billed.
- Supplier Invoice Number is the supplier's actual bill no.
- Code is ERP's internal Purchase Invoice number.
- Supplier's original invoice PDF may optionally be attached.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PurchaseInvoice : BaseEntity
    {
        // =====================================================
        // PRIMARY KEY
        // =====================================================

        public int Id
        {
            get;
            set;
        }


        // =====================================================
        // ERP PURCHASE INVOICE
        // =====================================================

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


        public PurchaseInvoiceStatus Status
        {
            get;
            set;
        } = PurchaseInvoiceStatus.Draft;


        // =====================================================
        // SUPPLIER'S ACTUAL INVOICE
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
         * Relative path of the uploaded Supplier Invoice PDF.
         *
         * Example:
         * /uploads/purchase-invoices/AI_PINV_26-27_00001_xxx.pdf
         *
         * Actual file is stored on disk.
         * Database stores only the relative path.
         */
        public string? SupplierInvoicePdfPath
        {
            get;
            set;
        }


        /*
         * Original filename uploaded by user.
         *
         * Example:
         * Supplier-Invoice-125.pdf
         */
        public string? SupplierInvoicePdfOriginalName
        {
            get;
            set;
        }


        /*
         * Date/time when Supplier Invoice PDF was attached.
         */
        public DateTime? SupplierInvoicePdfUploadedOn
        {
            get;
            set;
        }


        // =====================================================
        // PURCHASE ORDER
        // =====================================================

        public int PurchaseOrderId
        {
            get;
            set;
        }


        public PurchaseOrder PurchaseOrder
        {
            get;
            set;
        } = null!;


        /*
         * Snapshot / quick display reference.
         */
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


        public Supplier Supplier
        {
            get;
            set;
        } = null!;


        public string SupplierName
        {
            get;
            set;
        } = string.Empty;


        /*
         * Frozen Supplier snapshot at Purchase Invoice creation.
         */
        public string? SupplierSnapshotJson
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


        public Company Company
        {
            get;
            set;
        } = null!;


        public string CompanyName
        {
            get;
            set;
        } = string.Empty;


        /*
         * Frozen Company snapshot at Purchase Invoice creation.
         */
        public string? CompanySnapshotJson
        {
            get;
            set;
        }


        // =====================================================
        // PAYMENT TERMS
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

        /*
         * For purchase transaction, recipient is our Company.
         * Therefore Place of Supply is currently Company State.
         */
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


        /*
         * Supplier's actual Invoice Round Off.
         * Can be positive or negative.
         */
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
        // ITEMS
        // =====================================================

        public ICollection<PurchaseInvoiceItem> Items
        {
            get;
            set;
        } = new List<PurchaseInvoiceItem>();
    }
}