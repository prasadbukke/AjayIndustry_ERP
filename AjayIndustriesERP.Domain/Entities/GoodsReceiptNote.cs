using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class GoodsReceiptNote : BaseEntity
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public DateTime GRNDate { get; set; }


        // =============================================
        // PURCHASE ORDER
        // =============================================

        public int PurchaseOrderId { get; set; }

        public PurchaseOrder PurchaseOrder { get; set; } = null!;


        // =============================================
        // SUPPLIER SNAPSHOT
        // =============================================

        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public string SupplierName { get; set; } = string.Empty;


        // =============================================
        // SUPPLIER CHALLAN
        // =============================================

        public string? SupplierChallanNumber { get; set; }

        public DateTime? SupplierChallanDate { get; set; }


        // =============================================
        // REMARKS
        // =============================================

        public string? Remarks { get; set; }


        // =============================================
        // ITEMS
        // =============================================

        public ICollection<GoodsReceiptNoteItem> Items { get; set; }
            = new List<GoodsReceiptNoteItem>();
    }
}