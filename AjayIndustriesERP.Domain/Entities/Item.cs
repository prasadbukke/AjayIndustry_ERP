/*
==============================================================

File : Item.cs

Purpose :
Represents the Item Master entity.

Notes :
- Shape is optional.
- Item can contain multiple Specifications.
- Stock is maintained in Inventory.
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

        /// <summary>
        /// Optional manufacturer/internal Part Number.
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// Optional primary Item image file path.
        /// Actual image binary is not stored in the database.
        /// </summary>
        

        #endregion

        #region Foreign Keys

        public int ItemCategoryId { get; set; }

        public int BrandId { get; set; }

        public int UomId { get; set; }

        /// <summary>
        /// Optional physical Shape of the Item.
        /// </summary>
        public int? ShapeId { get; set; }

        #endregion

        #region Navigation Properties

        public ItemCategory ItemCategory { get; set; } = null!;

        public Brand Brand { get; set; } = null!;

        public Uom Uom { get; set; } = null!;

        public Shape? Shape { get; set; }

        /// <summary>
        /// Specifications assigned to this Item.
        ///
        /// Examples:
        /// Diameter = 25 MM
        /// Length   = 6000 MM
        /// Grade    = EN8
        /// </summary>
        public ICollection<ItemSpecification> ItemSpecifications
        {
            get;
            set;
        } = new List<ItemSpecification>();

        /// <summary>
        /// Engineering Drawings linked with this Item.
        /// </summary>
        public ICollection<Drawing> Drawings
        {
            get;
            set;
        } = new List<Drawing>();

        #endregion
    }
}