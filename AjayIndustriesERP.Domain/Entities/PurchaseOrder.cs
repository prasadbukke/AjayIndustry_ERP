using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        public int Id { get; set; }

        // PO Number
        public string Code { get; set; } = string.Empty;

        // PO Information
        public DateTime PODate { get; set; } = DateTime.Today;

        public DateTime? ExpectedDeliveryDate { get; set; }

        public PurchaseOrderStatus Status { get; set; }
            = PurchaseOrderStatus.Draft;


        // Supplier Reference
        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;

        // Company Reference
        public int CompanyId { get; set; }

        public Company Company { get; set; } = null!;


        // Company Snapshot
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyAddress { get; set; }

        public string? CompanyState { get; set; }

        public string? CompanyGSTIN { get; set; }

        public string? CompanyPhone { get; set; }

        public string? CompanyEmail { get; set; }


        // Supplier Snapshot
        public string SupplierName { get; set; } = string.Empty;

        public string? SupplierAddress { get; set; }

        public string? SupplierGSTIN { get; set; }

        public string? SupplierContactPerson { get; set; }

        public string? SupplierPhone { get; set; }

        public string? SupplierEmail { get; set; }


        // Delivery / Terms
        public string? DeliveryAddress { get; set; }

        public string? PaymentTerms { get; set; }

        public string? DeliveryTerms { get; set; }

        public string? TermsAndConditions { get; set; }

        public string? Remarks { get; set; }


        // Amounts
        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxableAmount { get; set; }

        public decimal CGSTAmount { get; set; }

        public decimal SGSTAmount { get; set; }

        public decimal IGSTAmount { get; set; }

        public decimal TransportCharges { get; set; }

        public decimal OtherCharges { get; set; }

        public decimal RoundOffAmount { get; set; }

        public decimal GrandTotal { get; set; }


        // Workflow
        public DateTime? ConfirmedOn { get; set; }

        public DateTime? SentToSupplierOn { get; set; }

        public DateTime? ClosedOn { get; set; }

        public DateTime? CancelledOn { get; set; }

        public string? CancellationReason { get; set; }


        // Purchase Order Items
        public ICollection<PurchaseOrderItem> Items { get; set; }
            = new List<PurchaseOrderItem>();
    }
}