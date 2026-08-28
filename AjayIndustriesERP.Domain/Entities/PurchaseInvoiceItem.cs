/*
============================================================
File: PurchaseInvoiceItem.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Represents one Purchase Invoice line.

Source Flow:
Purchase Order Item
    → Goods Receipt Note Item
    → Purchase Invoice Item

Responsibilities:
- Maintain exact GRN receipt traceability.
- Maintain exact Purchase Order Item traceability.
- Store Item / Drawing / HSN snapshots.
- Store actual billed quantity.
- Store trusted PO rate.
- Store GST calculation values.
- Store calculated line total.

Important:
- One Purchase Invoice Item represents one exact
  GoodsReceiptNoteItem source.
- Same Purchase Order Item may appear in multiple GRNs.
- Therefore GoodsReceiptNoteItemId is mandatory.
- Draft + Finalized active Purchase Invoice Items reserve
  received GRN quantity.
- Deleted Purchase Invoice Items do not reserve quantity.
- Browser-posted source snapshots and calculated amounts
  will not be trusted by PurchaseInvoiceService.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PurchaseInvoiceItem
        : BaseEntity
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public int PurchaseInvoiceId
        {
            get;
            set;
        }


        public int SequenceNumber
        {
            get;
            set;
        }

        #endregion


        #region Purchase Order Source

        /// <summary>
        /// Exact Purchase Order Item against which
        /// material was received.
        /// </summary>
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


        /// <summary>
        /// Purchase Order Item quantity snapshot.
        ///
        /// Informational only.
        /// Invoice quantity validation is performed
        /// against actual GRN received quantity.
        /// </summary>
        public decimal PurchaseOrderQuantity
        {
            get;
            set;
        }

        #endregion


        #region GRN Source

        /// <summary>
        /// Exact GRN containing the received material.
        /// </summary>
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


        /// <summary>
        /// Exact GRN Item is the primary quantity source.
        /// </summary>
        public int GoodsReceiptNoteItemId
        {
            get;
            set;
        }


        /// <summary>
        /// Original quantity received on this GRN line.
        ///
        /// Stored as historical snapshot.
        /// </summary>
        public decimal GoodsReceiptQuantity
        {
            get;
            set;
        }


        /// <summary>
        /// Optional Supplier Challan snapshot from GRN.
        /// </summary>
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

        #endregion


        #region Item Snapshot

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


        /// <summary>
        /// HSN snapshot from Purchase Order Item.
        /// </summary>
        public string? HsnCode
        {
            get;
            set;
        }

        #endregion


        #region Drawing Snapshot

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

        #endregion


        #region Quantity

        /// <summary>
        /// Quantity being billed in this Purchase Invoice.
        ///
        /// Must not exceed:
        ///
        /// GRN Received Quantity
        /// -
        /// Quantity already allocated to other active
        /// Draft / Finalized Purchase Invoices.
        /// </summary>
        public decimal PurchaseInvoiceQuantity
        {
            get;
            set;
        }

        #endregion


        #region Commercial Values

        /// <summary>
        /// Trusted rate copied from PurchaseOrderItem.UnitPrice.
        /// </summary>
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


        /*
         * Purchase Order currently does not use Discount.
         *
         * These fields are retained for future accounting
         * compatibility and should remain zero in current phase.
         */

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

        #endregion


        #region GST

        /// <summary>
        /// GST percentage copied from PurchaseOrderItem.
        /// </summary>
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

        #endregion


        #region Total

        public decimal LineTotal
        {
            get;
            set;
        }

        #endregion


        #region Navigation

        public PurchaseInvoice PurchaseInvoice
        {
            get;
            set;
        } = null!;


        public PurchaseOrderItem PurchaseOrderItem
        {
            get;
            set;
        } = null!;


        public GoodsReceiptNote GoodsReceiptNote
        {
            get;
            set;
        } = null!;


        public GoodsReceiptNoteItem GoodsReceiptNoteItem
        {
            get;
            set;
        } = null!;


        public Item Item
        {
            get;
            set;
        } = null!;

        #endregion
    }
}