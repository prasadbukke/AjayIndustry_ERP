// ============================================================
// File: OpeningStockCreateViewModel.cs
// Module: Inventory / Stock Management
// Layer: Web - ViewModel
//
// Purpose:
// Represents the complete Opening Stock entry form.
//
// Used For:
// - Opening Stock Date
// - Optional Remarks
// - Multiple Item + Warehouse Opening Stock lines
//
// Important:
// - One Opening Stock transaction can contain multiple lines.
// - Same Item + Warehouse combination cannot repeat inside
//   the same transaction.
// - Same Item + Warehouse can be entered again in a future
//   Opening Stock transaction.
// - Final business validation is performed by InventoryService.
// - Transaction Code / Type / Reference are not accepted from
//   the Web form as authoritative values.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Inventory
{
    public class OpeningStockCreateViewModel
    {
        // =====================================================
        // OPENING STOCK DATE
        // =====================================================

        [Required]
        [Display(Name = "Opening Stock Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }
            = DateTime.Today;


        // =====================================================
        // REMARKS
        // =====================================================

        [StringLength(
            1000,
            ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }


        // =====================================================
        // STOCK ITEMS
        // =====================================================

        public List<OpeningStockLineViewModel> Items { get; set; }
            = new();
    }
}