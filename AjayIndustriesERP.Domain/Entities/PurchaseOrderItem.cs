using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PurchaseOrderItem : BaseEntity
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;


        // Purchase Order Reference
        public int PurchaseOrderId { get; set; }

        public PurchaseOrder PurchaseOrder { get; set; } = null!;


        // Item Reference
        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;


        // Item Snapshot
        public string? ItemCode { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Specification { get; set; }

        public string? UnitName { get; set; }


        // Purchase Information
        // HSN belongs to Purchase Module as per Architecture Freeze
        public string? HSNCode { get; set; }


        // Drawing - Optional
        public int? DrawingId { get; set; }

        public Drawing? Drawing { get; set; }

        public string? DrawingNumber { get; set; }

        public string? DrawingRevision { get; set; }


        // Quantity & Rate
        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }


        // Discount
        public decimal DiscountPercent { get; set; }

        public decimal DiscountAmount { get; set; }


        // Tax
        public decimal TaxableAmount { get; set; }

        public decimal GSTPercent { get; set; }

        public decimal CGSTAmount { get; set; }

        public decimal SGSTAmount { get; set; }

        public decimal IGSTAmount { get; set; }


        // Total
        public decimal LineTotal { get; set; }


        // Additional
        public DateTime? RequiredDate { get; set; }

        public string? Remarks { get; set; }
    }
}