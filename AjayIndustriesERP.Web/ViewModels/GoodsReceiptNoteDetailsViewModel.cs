// ============================================================
// File: GoodsReceiptNoteDetailsViewModel.cs
// Purpose:
// Provides all data required by the GRN Details page.
//
// Responsibilities:
// - Holds the selected GRN header and item information.
// - Holds item-wise receipt history from previous/current GRNs.
// - Allows the latest GRN Details page to show when each
//   Purchase Order item was received and in what quantity.
//
// Important:
// This is a Web-layer ViewModel only.
// No business logic or database access belongs here.
//
// PurchaseOrderItemId is used as the history key so receipt
// history always belongs to the exact PO line.
// ============================================================

using GRNEntity =
    AjayIndustriesERP.Domain.Entities.GoodsReceiptNote;

using GRNItemEntity =
    AjayIndustriesERP.Domain.Entities.GoodsReceiptNoteItem;

namespace AjayIndustriesERP.Web.ViewModels.GoodsReceiptNote
{
    public class GoodsReceiptNoteDetailsViewModel
    {
        public GRNEntity GoodsReceiptNote { get; set; }
            = null!;

        public Dictionary<int, List<GRNItemEntity>>
            ReceiptHistory
        { get; set; }
            = new();
    }
}