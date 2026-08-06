/*
==============================================================

File : ItemViewModel.cs

Purpose :
Represents Item Master form data.

Notes :
- Stock and Warehouse are managed in Inventory.
- SimilarItemNames is used to warn about possible spelling
  mistakes or duplicate-like Item Names.

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Item
{
    /// <summary>
    /// Represents Item Master Create and Edit form data.
    /// </summary>
    public class ItemViewModel
    {
        #region Item Information

        public int ItemId { get; set; }

        [Display(Name = "Item Code")]
        public string? ItemCode { get; set; }

        [Required(ErrorMessage = "Item Name is required.")]
        [StringLength(
            150,
            ErrorMessage = "Item Name cannot exceed 150 characters.")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "Description cannot exceed 500 characters.")]
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

        #endregion

        #region Status

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Similar Name Confirmation

        /// <summary>
        /// Indicates that the user has reviewed similar Item Names
        /// and still wants to continue.
        /// </summary>
        [Display(Name = "Create item despite similar names")]
        public bool ConfirmSimilarItemName { get; set; }

        /// <summary>
        /// Contains Item Names that are similar to the entered name.
        /// </summary>
        [ValidateNever]
        public List<string> SimilarItemNames { get; set; } = new();

        #endregion

        #region Dropdown Lists

        [ValidateNever]
        public List<SelectListItem> Categories { get; set; } = new();

        [ValidateNever]
        public List<SelectListItem> Brands { get; set; } = new();

        [ValidateNever]
        public List<SelectListItem> Uoms { get; set; } = new();

        #endregion
    }
}