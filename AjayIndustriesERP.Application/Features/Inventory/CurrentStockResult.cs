// ============================================================
// File: CurrentStockResult.cs
// Module: Inventory / Stock Management
// Layer: Application - Feature Result
//
// Purpose:
// Represents one Item + Warehouse row on the
// Current Stock screen.
//
// Important:
// - This is not a database entity.
// - CurrentStock is calculated from StockTransactionLines.
// - Stock setting values may be null when no setting exists
//   for the Item + Warehouse combination.
// ============================================================

namespace AjayIndustriesERP.Application.Features.Inventory
{
    public class CurrentStockResult
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

        public string WarehouseCode { get; set; }
            = string.Empty;

        public string WarehouseName { get; set; }
            = string.Empty;


        // =====================================================
        // CURRENT STOCK
        // =====================================================

        public decimal CurrentStock { get; set; }


        // =====================================================
        // STOCK SETTINGS
        // =====================================================

        public decimal? MinimumStock { get; set; }

        public decimal? MaximumStock { get; set; }

        public decimal? ReorderLevel { get; set; }

        public decimal? ReorderQuantity { get; set; }
    }
}