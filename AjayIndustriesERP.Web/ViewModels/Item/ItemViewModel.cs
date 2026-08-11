/*
==============================================================

File : ItemViewModel.cs

Purpose :
Represents Item Master Create/Edit form data.

Features :
- Category
- Brand
- UOM
- Optional Shape
- Dynamic Item Specifications
- Similar Item Name validation

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Item
{
    /// <summary>
    /// Represents Item Master form data.
    /// </summary>
    public class ItemViewModel
    {
        #region Item Information

        public int ItemId { get; set; }

        [Display(Name = "Item Code")]
        public string? ItemCode { get; set; }

        [Required(
            ErrorMessage = "Item Name is required.")]
        [StringLength(
            150,
            ErrorMessage =
                "Item Name cannot exceed 150 characters.")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } =
            string.Empty;

        [StringLength(100,  ErrorMessage ="Part Number cannot exceed 100 characters.")]
        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [StringLength(
            500,
            ErrorMessage =
                "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        #endregion

        #region Foreign Keys

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Category.")]
        [Display(Name = "Category")]
        public int ItemCategoryId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Brand.")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a UOM.")]
        [Display(Name = "UOM")]
        public int UomId { get; set; }

        /// <summary>
        /// Optional physical Shape.
        /// </summary>
        [Display(Name = "Shape")]
        public int? ShapeId { get; set; }

        #endregion

        #region Item Specifications

        /// <summary>
        /// Dynamic Specification rows assigned to the Item.
        ///
        /// Examples:
        /// Diameter = 25 MM
        /// Length   = 6000 MM
        /// Grade    = EN8
        /// </summary>
        public List<ItemSpecificationRowViewModel>
            ItemSpecifications
        {
            get;
            set;
        } = new();

        #endregion

        #region Status

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Similar Item Validation

        [Display(
            Name = "Create item despite similar names")]
        public bool ConfirmSimilarItemName { get; set; }

        [ValidateNever]
        public List<string> SimilarItemNames
        {
            get;
            set;
        } = new();

        #endregion

        #region Main Dropdown Lists

        [ValidateNever]
        public List<SelectListItem> Categories
        {
            get;
            set;
        } = new();

        [ValidateNever]
        public List<SelectListItem> Brands
        {
            get;
            set;
        } = new();

        [ValidateNever]
        public List<SelectListItem> Uoms
        {
            get;
            set;
        } = new();

        [ValidateNever]
        public List<SelectListItem> Shapes
        {
            get;
            set;
        } = new();

        #endregion

        #region Specification Dropdown Lists

        /// <summary>
        /// Available Specification Master records.
        /// Used by every dynamic Specification row.
        /// </summary>
        [ValidateNever]
        public List<SelectListItem> SpecificationOptions
        {
            get;
            set;
        } = new();

        /// <summary>
        /// Available UOM records for Specification values.
        /// UOM remains optional.
        /// </summary>
        [ValidateNever]
        public List<SelectListItem> SpecificationUoms
        {
            get;
            set;
        } = new();

        #endregion
    }
}