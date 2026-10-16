// ============================================================
// File: StockLedgerLineViewModel.cs
// Module: Inventory / Stock Management
// Layer: Web - ViewModel
//
// Purpose:
// Represents one row in the Stock Ledger.
//
// Used For:
// - Transaction information
// - Item / Warehouse information
// - Quantity In / Out
// - Running Balance
// - Optional Rate / Amount
// - Remarks
//
// Important:
// This ViewModel belongs only to the Web layer.
// Running Balance is prepared from ordered ledger rows before
// rendering the View.
// ============================================================

namespace AjayIndustriesERP.Web.ViewModels.Inventory
{
    public class StockLedgerLineViewModel
    {
        // =====================================================
        // TRANSACTION
        // =====================================================

        public int TransactionId { get; set; }

        public string TransactionCode { get; set; }
            = string.Empty;

        public DateTime TransactionDate { get; set; }

        public string TransactionType { get; set; }
            = string.Empty;

        public string? ReferenceNumber { get; set; }


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
        // STOCK MOVEMENT
        // =====================================================

        public decimal QuantityIn { get; set; }

        public decimal QuantityOut { get; set; }

        public decimal RunningBalance { get; set; }


        // =====================================================
        // VALUE
        // =====================================================

        public decimal? Rate { get; set; }

        public decimal? Amount { get; set; }


        // =====================================================
        // REMARKS
        // =====================================================

        public string? Remarks { get; set; }
    }
}