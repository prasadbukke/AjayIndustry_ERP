// ============================================================
// File: OpeningStockLineViewModel.cs
// Module: Inventory / Stock Management
// Layer: Web - ViewModel
//
// Purpose:
// Represents one Item + Warehouse line inside an
// Opening Stock transaction.
//
// Used For:
// - Item selection
// - Warehouse selection
// - Opening Quantity
// - Optional Rate
// - Item / Warehouse display information
// - Optional line remarks
//
// Important:
// - QuantityOut is not accepted from the Web form.
// - Amount is not accepted as an authoritative value.
// - InventoryService calculates Amount from Quantity x Rate.
// - Final Item / Warehouse / Quantity / Rate validation
//   remains in InventoryService.
// ============================================================

namespace AjayIndustriesERP.Web.ViewModels.Inventory
{
    public class OpeningStockLineViewModel
    {
        // =====================================================
        // ITEM
        // =====================================================

        public int ItemId { get; set; }

        public string ItemCode { get; set; }
            = string.Empty;

        public string ItemName { get; set; }
            = string.Empty;

        public string UnitName { get; set; }
            = string.Empty;


        // =====================================================
        // WAREHOUSE
        // =====================================================

        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; }
            = string.Empty;


        // =====================================================
        // OPENING STOCK
        // =====================================================

        public decimal Quantity { get; set; }

        public decimal? Rate { get; set; }


        // =====================================================
        // REMARKS
        // =====================================================

        public string? Remarks { get; set; }
    }
}