// ============================================================
// File: GoodsReceiptNoteCreateViewModel.cs
// Purpose:
// Represents the complete Goods Receipt Note Create form.
//
// Used For:
// - GRN Date
// - Purchase Order selection
// - Display selected Supplier information
// - Supplier Challan details
// - GRN Remarks
// - Collection of all Purchase Order item receipt entries
//
// Flow:
// User selects PO
//      ↓
// Controller loads PO through GoodsReceiptNoteService
//      ↓
// All PO items are displayed
//      ↓
// User selects receipt/material statuses
//      ↓
// Controller maps this ViewModel to Domain entities
//      ↓
// GoodsReceiptNoteService performs final business validation
//
// Important:
// Supplier and quantity information displayed on the page is
// not treated as authoritative.
// GoodsReceiptNoteService reloads Purchase Order data and
// recalculates final values before saving.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.GoodsReceiptNote
{
    public class GoodsReceiptNoteCreateViewModel
    {

        // =====================================================
        // GRN ID
        //
        // 0  = Create
        // >0 = Edit
        // =====================================================

        public int Id { get; set; }
        // =====================================================
        // GRN
        // =====================================================

        [Required]
        [Display(Name = "GRN Date")]
        [DataType(DataType.Date)]
        public DateTime GRNDate { get; set; }
            = DateTime.Today;


        // =====================================================
        // PURCHASE ORDER
        // =====================================================

        [Required(ErrorMessage = "Please select Purchase Order.")]
        [Display(Name = "Purchase Order")]
        public int PurchaseOrderId { get; set; }


        // =====================================================
        // SUPPLIER DISPLAY INFORMATION
        // =====================================================
        //
        // Auto-filled after selecting PO.
        // Final Supplier is taken from Purchase Order in Service.
        // =====================================================

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;


        // =====================================================
        // SUPPLIER CHALLAN
        // =====================================================

        [Display(Name = "Supplier Challan No")]
        [StringLength(
            100,
            ErrorMessage = "Supplier Challan No cannot exceed 100 characters.")]
        public string? SupplierChallanNumber { get; set; }


        [Display(Name = "Supplier Challan Date")]
        [DataType(DataType.Date)]
        public DateTime? SupplierChallanDate { get; set; }


        // =====================================================
        // REMARKS
        // =====================================================

        [StringLength(
            1000,
            ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }


        // =====================================================
        // PO ITEMS
        // =====================================================

        public List<GoodsReceiptNoteItemViewModel> Items { get; set; }
            = new();
    }
}