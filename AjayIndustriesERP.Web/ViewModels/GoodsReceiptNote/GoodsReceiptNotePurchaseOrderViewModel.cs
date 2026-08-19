// ============================================================
// File: GoodsReceiptNotePurchaseOrderViewModel.cs
// Purpose:
// Carries Purchase Order information from the GRN Service to
// the Create GRN screen after a Purchase Order is selected.
//
// Used For:
// - Selected PO information
// - Supplier information
// - All PO item lines with their receipt history
//
// This model will later be returned through the GRN AJAX
// endpoint when the user selects a Purchase Order.
// ============================================================

using AjayIndustriesERP.Web.ViewModels.GoodsReceiptNote;

namespace AjayIndustriesERP.Application.ViewModels.GoodsReceiptNote
{
    public class GoodsReceiptNotePurchaseOrderViewModel
    {
        public int PurchaseOrderId { get; set; }

        public string PurchaseOrderCode { get; set; }
            = string.Empty;

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;

        public List<GoodsReceiptNoteItemViewModel> Items { get; set; }
            = new();
    }
}