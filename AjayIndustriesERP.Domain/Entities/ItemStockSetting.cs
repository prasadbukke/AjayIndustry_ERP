/*
============================================================

File: ItemStockSetting.cs

Purpose:
Stores Inventory planning settings for an Item
at Warehouse level.

Responsibilities:
- Maintain Minimum Stock level.
- Maintain Maximum Stock level.
- Maintain Reorder Level.
- Maintain suggested Reorder Quantity.
- Link settings to Item Master.
- Link settings to Warehouse Master.

Important:
- Actual stock quantity is NOT stored here.
- Current stock is calculated from Stock Transactions.
- One Item can have different stock settings for
  different Warehouses.
- Item + Warehouse combination will be kept unique
  through Entity Framework configuration.

============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ItemStockSetting : BaseEntity
    {
        #region Primary Key

        public int Id { get; set; }

        #endregion


        #region Item Relationship

        public int ItemId { get; set; }

        public Item Item { get; set; } =
            null!;

        #endregion


        #region Warehouse Relationship

        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } =
            null!;

        #endregion


        #region Stock Levels

        /// <summary>
        /// Minimum stock level that should normally be maintained.
        /// </summary>
        public decimal MinimumStock { get; set; }


        /// <summary>
        /// Maximum preferred stock level for this Item
        /// in this Warehouse.
        /// </summary>
        public decimal MaximumStock { get; set; }


        /// <summary>
        /// Stock level at which replenishment should be considered.
        /// </summary>
        public decimal ReorderLevel { get; set; }


        /// <summary>
        /// Suggested quantity to reorder when stock reaches
        /// the Reorder Level.
        /// </summary>
        public decimal ReorderQuantity { get; set; }

        #endregion
    }
}