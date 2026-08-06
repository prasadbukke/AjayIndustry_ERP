/*
==============================================================

File : Item.cs

Purpose :
Represents the Item Master entity.

Notes :
- Stock quantities are maintained in the Inventory module.
- Warehouse-wise stock is maintained separately.
- GST and pricing are not part of Item Master.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    /// <summary>
    /// Represents an item registered in the ERP system.
    /// </summary>
    public class Item : BaseEntity
    {
        #region Primary Key

        public int ItemId { get; set; }

        #endregion

        #region Item Information

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        #endregion

        #region Foreign Keys

        public int ItemCategoryId { get; set; }

        public int BrandId { get; set; }

        public int UomId { get; set; }

        #endregion

        #region Navigation Properties

        public ItemCategory ItemCategory { get; set; } = null!;

        public Brand Brand { get; set; } = null!;

        public Uom Uom { get; set; } = null!;

        #endregion
    }
}