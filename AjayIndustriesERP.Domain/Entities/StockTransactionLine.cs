// ============================================================
// File: StockTransactionLine.cs
// Purpose:
// Represents one item-wise stock movement inside a
// StockTransaction.
//
// Stores:
// - Item
// - Warehouse
// - Quantity IN
// - Quantity OUT
// - Optional stock rate / value
//
// Important:
// Current stock is calculated as:
//
// Total QuantityIn - Total QuantityOut
//
// Warehouse is stored at line level because a Warehouse Transfer
// can contain an OUT movement from one warehouse and an IN
// movement into another warehouse inside the same transaction.
// ============================================================

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class StockTransactionLine : BaseEntity
    {
        // =============================================
        // PRIMARY KEY
        // =============================================

        public int Id { get; set; }


        // =============================================
        // STOCK TRANSACTION
        // =============================================

        public int StockTransactionId { get; set; }

        public StockTransaction StockTransaction { get; set; } = null!;


        // =============================================
        // ITEM
        // =============================================

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;


        // =============================================
        // WAREHOUSE
        // =============================================

        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } = null!;


        // =============================================
        // STOCK QUANTITY
        // =============================================

        /// <summary>
        /// Quantity added to stock.
        /// Zero when this line represents Stock OUT.
        /// </summary>
        public decimal QuantityIn { get; set; }


        /// <summary>
        /// Quantity removed from stock.
        /// Zero when this line represents Stock IN.
        /// </summary>
        public decimal QuantityOut { get; set; }


        // =============================================
        // STOCK VALUE
        // =============================================

        /// <summary>
        /// Optional unit stock rate used for inventory valuation.
        /// </summary>
        public decimal? Rate { get; set; }


        /// <summary>
        /// Optional total stock value for this movement.
        /// Usually Quantity x Rate.
        /// </summary>
        public decimal? Amount { get; set; }


        // =============================================
        // REMARKS
        // =============================================

        public string? Remarks { get; set; }
    }
}