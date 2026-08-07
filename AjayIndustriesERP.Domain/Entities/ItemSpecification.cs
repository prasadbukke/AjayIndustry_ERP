/*
==============================================================

File : ItemSpecification.cs

Purpose :
Stores specification values assigned to an Item.

Examples :
Diameter = 25 MM
Length   = 6000 MM
Grade    = EN8

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    /// <summary>
    /// Represents a specification value assigned to an Item.
    /// </summary>
    public class ItemSpecification : BaseEntity
    {
        #region Primary Key

        public int ItemSpecificationId { get; set; }

        #endregion

        #region Foreign Keys

        public int ItemId { get; set; }

        public int SpecificationId { get; set; }

        /// <summary>
        /// Optional UOM.
        /// Text-based specifications such as Grade
        /// may not require a UOM.
        /// </summary>
        public int? UomId { get; set; }

        #endregion

        #region Specification Value

        public string SpecificationValue { get; set; } =
            string.Empty;

        public int SortOrder { get; set; }

        #endregion

        #region Navigation Properties

        public Item Item { get; set; } = null!;

        public Specification Specification { get; set; } =
            null!;

        public Uom? Uom { get; set; }

        #endregion
    }
}