/*
============================================================
File: PurchaseInvoice.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Represents one Supplier Purchase Invoice.

Source Flow:
Purchase Order
    → Goods Receipt Note
    → Purchase Invoice
    → Supplier Payment
    → Supplier Outstanding

Responsibilities:
- Store internal Purchase Invoice number.
- Store Supplier's actual Invoice Number and Date.
- Maintain Purchase Order traceability.
- Store Supplier historical snapshot.
- Store Company historical snapshot.
- Store payment terms and due date.
- Store GST / financial totals.
- Maintain Purchase Invoice workflow.
- Maintain Purchase Invoice line items.
- Maintain finalization information.

Important:
- Purchase Invoice is created only against actually
  received GRN quantities.
- Draft + Finalized active Purchase Invoices reserve
  received quantity.
- Deleted Purchase Invoices do not reserve quantity.
- Finalized Purchase Invoice cannot be edited/deleted.
- Supplier Payment will later be allocated against
  Finalized Purchase Invoices.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PurchaseInvoice
        : BaseEntity
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        /// <summary>
        /// Internal ERP Purchase Invoice number.
        ///
        /// Example:
        /// AI/PINV/26-27/00001
        /// </summary>
        public string Code
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// ERP posting / Purchase Invoice date.
        /// Used for internal financial-year numbering.
        /// </summary>
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

        #endregion


        #region Supplier Invoice Information

        /// <summary>
        /// Actual Invoice Number printed on
        /// Supplier's Tax Invoice / Bill.
        ///
        /// Duplicate Supplier Invoice Number for the
        /// same Supplier will be prevented by Service.
        /// </summary>
        public string SupplierInvoiceNumber
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// Actual Invoice Date printed on Supplier Bill.
        /// </summary>
        public DateTime SupplierInvoiceDate
        {
            get;
            set;
        }

        #endregion


        #region Purchase Order Reference

        /*
         * Current ERP Phase:
         *
         * One Purchase Invoice belongs to one Purchase Order.
         *
         * Multiple GRNs against that same PO can contribute
         * quantities to this Purchase Invoice.
         */

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


        /// <summary>
        /// Historical PO Number snapshot.
        /// </summary>
        public string PurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Supplier Reference / Snapshot

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


        /// <summary>
        /// Supplier Name snapshot for convenient display.
        /// </summary>
        public string SupplierName
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// Generic scalar snapshot of Supplier Master.
        ///
        /// Includes:
        /// SupplierCode
        /// GSTIN
        /// PAN
        /// Contact Information
        /// Address
        /// State
        /// PaymentTermsDays
        /// and future scalar Supplier fields.
        /// </summary>
        public string? SupplierSnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Company Reference / Snapshot

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


        /// <summary>
        /// Generic scalar Company snapshot.
        ///
        /// Includes:
        /// Address
        /// GST
        /// PAN
        /// State
        /// Contact Details
        /// Bank / ISO information
        /// and future scalar Company fields.
        /// </summary>
        public string? CompanySnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Payment Information

        /// <summary>
        /// Supplier payment terms snapshot.
        ///
        /// Example:
        /// 30 Days
        /// 45 Days
        /// Immediate
        /// </summary>
        public string? PaymentTerms
        {
            get;
            set;
        }


        /// <summary>
        /// Supplier PaymentTermsDays snapshot.
        /// </summary>
        public int? CreditDays
        {
            get;
            set;
        }


        /// <summary>
        /// Normally:
        ///
        /// SupplierInvoiceDate + CreditDays
        /// </summary>
        public DateTime? DueDate
        {
            get;
            set;
        }

        #endregion


        #region GST Information

        /// <summary>
        /// Supplier State used as Place of Supply /
        /// GST transaction reference.
        /// </summary>
        public string? PlaceOfSupply
        {
            get;
            set;
        }


        /// <summary>
        /// True:
        /// Supplier State != Company State
        /// → IGST
        ///
        /// False:
        /// Supplier State == Company State
        /// → CGST + SGST
        /// </summary>
        public bool IsInterState
        {
            get;
            set;
        }

        #endregion


        #region Financial Totals

        /// <summary>
        /// Sum of Purchase Invoice line quantities × rates.
        /// </summary>
        public decimal GrossAmount
        {
            get;
            set;
        }


        /// <summary>
        /// Current Purchase flow does not use discount,
        /// but the field is retained for accounting
        /// compatibility and future use.
        /// </summary>
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


        /// <summary>
        /// Freight / transport amount charged by Supplier.
        /// </summary>
        public decimal TransportCharges
        {
            get;
            set;
        }


        /// <summary>
        /// Other Supplier Invoice charges.
        /// </summary>
        public decimal OtherCharges
        {
            get;
            set;
        }


        /// <summary>
        /// Positive or negative Supplier Invoice round-off.
        /// </summary>
        public decimal RoundOffAmount
        {
            get;
            set;
        }


        /// <summary>
        /// Final payable amount to Supplier.
        ///
        /// This amount will later become the source amount
        /// for Supplier Payment / Outstanding calculation.
        /// </summary>
        public decimal GrandTotal
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Finalization

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

        #endregion


        #region Navigation

        public ICollection<PurchaseInvoiceItem> Items
        {
            get;
            set;
        } = new List<PurchaseInvoiceItem>();

        #endregion
    }
}