/*
============================================================
File: InvoiceItem.cs

Module:
Invoice

Purpose:
Represents one Invoice line item.

Responsibilities:
- Maintain Invoice line sequence.
- Maintain Completed Production Job traceability.
- Maintain Customer Purchase Order traceability.
- Optionally maintain Delivery Challan traceability.
- Store Item / Product snapshot.
- Store Invoice quantity and commercial values.
- Store GST calculation values.
- Store calculated line total.

Important:
- Production Job is the primary source for new Invoice lines.
- Delivery Challan is optional.
- Invoice can be created even when PDI / Challan is not done,
  subject to Service-level warning confirmation.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class InvoiceItem
        : BaseEntity
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public int InvoiceId
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


        #region Delivery Challan Source - Optional

        /*
         * Delivery Challan is no longer mandatory
         * for creating an Invoice.
         */
        public int? DeliveryChallanId
        {
            get;
            set;
        }


        public string? DeliveryChallanCode
        {
            get;
            set;
        }


        public int? DeliveryChallanItemId
        {
            get;
            set;
        }


        public decimal? DeliveryChallanQuantity
        {
            get;
            set;
        }

        #endregion


        #region Product / Item Snapshot

        public string? ProductReference
        {
            get;
            set;
        }


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


        public string? PartNumber
        {
            get;
            set;
        }


        public string? CustomerItemCode
        {
            get;
            set;
        }


        public string? UnitName
        {
            get;
            set;
        }


        public string? HsnNumber
        {
            get;
            set;
        }

        #endregion


        #region Customer Purchase Order Snapshot

        public int? CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderCode
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderNumber
        {
            get;
            set;
        }

        #endregion


        #region Production Job Source

        /*
         * Completed Production Job is the primary
         * source for new Invoice lines.
         */
        public int? ProductionJobId
        {
            get;
            set;
        }


        public string? ProductionJobCode
        {
            get;
            set;
        }

        #endregion


        #region Quantity

        public decimal InvoiceQuantity
        {
            get;
            set;
        }

        #endregion


        #region Commercial Values

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

        #endregion


        #region GST

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


        #region Line Total

        public decimal LineTotal
        {
            get;
            set;
        }

        #endregion


        #region Navigation Properties

        public Invoice Invoice
        {
            get;
            set;
        } = null!;


        /*
         * Optional because Invoice may be created
         * before Delivery Challan.
         */
        public DeliveryChallan? DeliveryChallan
        {
            get;
            set;
        }


        public DeliveryChallanItem? DeliveryChallanItem
        {
            get;
            set;
        }

        #endregion
    }
}