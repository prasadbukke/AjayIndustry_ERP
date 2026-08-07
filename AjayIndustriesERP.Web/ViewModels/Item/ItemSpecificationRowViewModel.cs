/*
==============================================================

File : ItemSpecificationRowViewModel.cs

Purpose :
Represents one Specification row inside Item Master.

Example :
Diameter | 25   | MM
Length   | 6000 | MM
Grade    | EN8  | -

==============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Item
{
    /// <summary>
    /// Represents one specification assigned to an Item.
    /// </summary>
    public class ItemSpecificationRowViewModel
    {
        #region Primary Key

        public int ItemSpecificationId { get; set; }

        #endregion

        #region Specification

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Specification.")]
        public int SpecificationId { get; set; }

        [Required(
            ErrorMessage = "Specification Value is required.")]
        [StringLength(
            200,
            ErrorMessage =
                "Specification Value cannot exceed 200 characters.")]
        public string SpecificationValue { get; set; } =
            string.Empty;

        /// <summary>
        /// Optional UOM.
        ///
        /// Examples:
        /// Diameter = 25 MM
        /// Grade    = EN8 without UOM
        /// </summary>
        public int? UomId { get; set; }

        public int SortOrder { get; set; }

        #endregion
    }
}