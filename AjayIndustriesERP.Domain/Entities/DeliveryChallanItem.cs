/*
============================================================
File: DeliveryChallanItem.cs

Purpose:
Represents one dispatched Item / PDI lot inside
a Delivery Challan.

Responsibilities:
- Link Challan Item to Finalized PDI.
- Store Production Job snapshot.
- Store Customer PO snapshot.
- Store Item / Part snapshot.
- Store Customer Drawing snapshot.
- Store PDI Accepted Quantity snapshot.
- Store actual Dispatch Quantity.
- Store UOM snapshot.

Important:
- Finalized PDI is the trusted dispatch source.
- Already Dispatched Quantity is NOT stored here.
  It will be calculated from existing active Challans.
- Available Dispatch Quantity is NOT stored here.
  It will be calculated by Application Service.

Formula:

Available To Dispatch
=
PDI Accepted Quantity
-
Already Dispatched Quantity

- Price / GST / Invoice values do NOT belong here.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class DeliveryChallanItem : BaseEntity
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public int DeliveryChallanId
        {
            get;
            set;
        }


        public DeliveryChallan DeliveryChallan
        {
            get;
            set;
        } = null!;


        public int SequenceNumber
        {
            get;
            set;
        }

        #endregion


        #region PDI Source

        /*
         * Delivery is allowed only against
         * a Finalized PDI Report.
         */

        public int PreDispatchInspectionId
        {
            get;
            set;
        }


        public PreDispatchInspection PreDispatchInspection
        {
            get;
            set;
        } = null!;


        /*
         * Snapshot of PDI Report Number.
         *
         * Example:
         * AI/PDI/26-27/00001
         */

        public string PreDispatchInspectionCode
        {
            get;
            set;
        } = string.Empty;


        /*
         * Accepted Quantity from the PDI
         * at the time this Challan is prepared.
         */

        public decimal PdiAcceptedQuantity
        {
            get;
            set;
        }

        #endregion


        #region Production Job Snapshot

        public int ProductionJobId
        {
            get;
            set;
        }


        public string ProductionJobCode
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Customer PO Snapshot

        public int CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public string CustomerPurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;


        public string CustomerPurchaseOrderNumber
        {
            get;
            set;
        } = string.Empty;


        public string? CustomerItemCode
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


        /*
         * Part Number priority will follow PDI snapshot:
         *
         * Customer Item Code
         *      ↓
         * Item Part Number
         *      ↓
         * ERP Item Code
         */

        public string? PartNumber
        {
            get;
            set;
        }


        public string? UnitName
        {
            get;
            set;
        }

        #endregion

        #region Product Information

        /*
         * Temporary manually entered Product ID / Reference.
         *
         * Currently:
         * - User enters this value manually.
         *
         * Future:
         * - Separate Product Master will be created.
         * - ProductId FK will then be introduced.
         * - This field can remain as historical snapshot/reference.
         */

        public string? ProductReference
        {
            get;
            set;
        }

        #region HSN Information

        /*
         * HSN Number used for the dispatched item.
         *
         * Currently entered manually.
         * Later this may come from Item / Product Master.
         */

        public string? HsnNumber
        {
            get;
            set;
        }

        #endregion

        #endregion


        #region Customer Drawing Snapshot

        /*
         * Delivery Challan is customer-facing,
         * therefore Customer Drawing is stored
         * as the primary Drawing reference.
         */

        public int? CustomerDrawingId
        {
            get;
            set;
        }


        public string? CustomerDrawingNumber
        {
            get;
            set;
        }


        public string? CustomerDrawingRevision
        {
            get;
            set;
        }

        #endregion


        #region Dispatch Quantity

        /*
         * Actual Quantity dispatched through
         * this Delivery Challan line.
         *
         * Must always be:
         *
         * > 0
         *
         * and
         *
         * <= Available PDI Accepted Quantity
         */

        public decimal DispatchQuantity
        {
            get;
            set;
        }

        #endregion
    }
}