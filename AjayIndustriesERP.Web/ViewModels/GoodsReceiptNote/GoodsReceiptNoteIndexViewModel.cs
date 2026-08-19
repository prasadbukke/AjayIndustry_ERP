// ============================================================
// File: GoodsReceiptNoteIndexViewModel.cs
// Purpose:
// Provides grouped data required by the GRN Index page.
//
// UI Design:
// One Purchase Order = One parent row.
//
// Each Purchase Order parent row contains complete GRN history:
//
// PO
//   ├── GRN-001
//   ├── GRN-002
//   └── GRN-003
//
// Important:
// This is only a Web presentation model.
// Database entities and transaction relationships are unchanged.
// ============================================================

namespace AjayIndustriesERP.Web.ViewModels.GoodsReceiptNote
{
    public class GoodsReceiptNoteIndexViewModel
    {
        public int PurchaseOrderId { get; set; }

        public string PurchaseOrderCode { get; set; }
            = string.Empty;

        public string SupplierName { get; set; }
            = string.Empty;

        public int LatestGoodsReceiptNoteId { get; set; }

        public string LatestGoodsReceiptNoteCode { get; set; }
            = string.Empty;

        public DateTime LatestGoodsReceiptNoteDate { get; set; }

        // ========================================================
        // PURCHASE ORDER RECEIPT STATUS
        // ========================================================
        //
        // Complete:
        // Every PO item has zero Pending Quantity after latest GRN.
        //
        // Pending:
        // At least one PO item still has quantity remaining.
        //
        // This is calculated from the latest GRN item snapshots.
        // It is not stored as a separate database status.
        // ========================================================

        public bool IsReceiptComplete { get; set; }

        public string ReceiptStatus =>
            IsReceiptComplete
                ? "Complete"
                : "Pending";

        public List<GoodsReceiptNoteHistoryViewModel>
            History
        { get; set; }
            = new();


        public int TotalGoodsReceiptNotes =>
            History.Count;
    }


    // ========================================================
    // GRN HISTORY ROW
    // ========================================================

    public class GoodsReceiptNoteHistoryViewModel
    {
        public int Id { get; set; }

        public string Code { get; set; }
            = string.Empty;

        public DateTime GRNDate { get; set; }

        public string? SupplierChallanNumber { get; set; }

        public DateTime? SupplierChallanDate { get; set; }

        public bool IsLatest { get; set; }
    }
}