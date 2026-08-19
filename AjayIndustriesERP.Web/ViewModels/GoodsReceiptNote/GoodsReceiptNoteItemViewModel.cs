// ============================================================
// File: GoodsReceiptNoteItemViewModel.cs
// Purpose:
// Represents one Purchase Order item on the GRN Create screen.
//
// Used For:
// - Display Item / Specification / UOM information
// - Display Ordered Quantity
// - Display Previous Received and Balance when applicable
// - Capture Receipt Status
// - Capture Received Now quantity only for Partial Received
// - Display calculated Pending quantity
// - Capture Material Status
// - Capture item-level remarks
//
// UI Rules:
// - Previous Received is shown only when previous quantity > 0.
// - Balance is shown with Previous Received on subsequent GRNs.
// - Partial Received:
//      Received Now = Show
//      Pending      = Show / Readonly
// - Full Received:
//      Received Now = Hide
//      Pending      = Hide
// - Not Received:
//      Received Now = Hide
//      Pending      = Hide
//      Material Status = Hide
//
// Important:
// This ViewModel belongs only to the Web layer.
// Final calculations and validation remain in
// GoodsReceiptNoteService.
// ============================================================

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.GoodsReceiptNote
{
    public class GoodsReceiptNoteItemViewModel
    {
        public int PurchaseOrderItemId { get; set; }

        public int ItemId { get; set; }


        // =====================================================
        // ITEM INFORMATION
        // =====================================================

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? Specification { get; set; }

        public string UnitName { get; set; } = string.Empty;


        // =====================================================
        // QUANTITY INFORMATION
        // =====================================================

        public decimal OrderedQuantity { get; set; }

        public decimal PreviouslyReceivedQuantity { get; set; }

        public decimal BalanceQuantity { get; set; }


        // =====================================================
        // RECEIPT INPUT
        // =====================================================

        public GoodsReceiptStatus ReceiptStatus { get; set; }
            = GoodsReceiptStatus.NotReceived;

        public decimal ReceivedQuantity { get; set; }

        public decimal PendingQuantity { get; set; }


        // =====================================================
        // MATERIAL STATUS
        // =====================================================

        public GoodsReceiptMaterialStatus? MaterialStatus { get; set; }


        // =====================================================
        // REMARKS
        // =====================================================

        public string? Remarks { get; set; }
    }
}