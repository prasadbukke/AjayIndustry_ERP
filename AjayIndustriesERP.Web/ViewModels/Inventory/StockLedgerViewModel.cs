// ============================================================
// File: StockLedgerViewModel.cs
// Module: Inventory / Stock Management
// Layer: Web - ViewModel
//
// Purpose:
// Represents the complete Stock Ledger screen.
//
// Used For:
// - Item filter
// - Optional Warehouse filter
// - Display Item + Warehouse wise stock movement history
//
// Important:
// - Item selection is required before loading ledger.
// - Warehouse filter is optional.
// - Final stock data comes from InventoryService.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Inventory
{
    public class StockLedgerViewModel
    {
        // =====================================================
        // FILTERS
        // =====================================================

        [Display(Name = "Item")]
        public int? ItemId { get; set; }


        [Display(Name = "Warehouse")]
        public int? WarehouseId { get; set; }


        // =====================================================
        // LEDGER
        // =====================================================

        public List<StockLedgerLineViewModel> Entries { get; set; }
            = new();
    }
}