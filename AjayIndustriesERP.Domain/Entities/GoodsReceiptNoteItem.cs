using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

// ============================================================
// File: GoodsReceiptNoteItem.cs
// Purpose:
// Represents one Purchase Order line inside a Goods Receipt Note.
//
// Stores:
// - Exact PurchaseOrderItem reference
// - Item snapshot
// - Ordered quantity
// - Previously received quantity
// - Remaining/balance quantity
// - Current receipt status
// - Received and pending quantities
// - Material status
//
// Important:
// PurchaseOrderItemId is used instead of only ItemId so previous
// receipts are calculated against the exact PO line.
//
// This allows multiple GRNs against the same PO item and supports
// partial receipt followed by future receipts.
// ============================================================

namespace AjayIndustriesERP.Domain.Entities
{
    public class GoodsReceiptNoteItem : BaseEntity
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;


        // =============================================
        // GRN
        // =============================================

        public int GoodsReceiptNoteId { get; set; }

        public GoodsReceiptNote GoodsReceiptNote { get; set; } = null!;


        // =============================================
        // PURCHASE ORDER ITEM
        // =============================================

        public int PurchaseOrderItemId { get; set; }

        public PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;


        // =============================================
        // ITEM
        // =============================================

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;


        // =============================================
        // ITEM SNAPSHOT
        // =============================================

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? Specification { get; set; }

        public string UnitName { get; set; } = string.Empty;


        // =============================================
        // QUANTITY SNAPSHOT
        // =============================================

        public decimal OrderedQuantity { get; set; }

        public decimal PreviouslyReceivedQuantity { get; set; }

        public decimal BalanceQuantity { get; set; }


        // =============================================
        // RECEIPT
        // =============================================

        public GoodsReceiptStatus ReceiptStatus { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public decimal PendingQuantity { get; set; }


        // =============================================
        // MATERIAL STATUS
        // =============================================

        public GoodsReceiptMaterialStatus? MaterialStatus { get; set; }


        // =============================================
        // REMARKS
        // =============================================

        public string? Remarks { get; set; }
    }
}