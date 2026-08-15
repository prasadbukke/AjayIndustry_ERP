/*
==============================================================

File : PurchaseOrderItemViewModel.cs

Purpose :
Represents one dynamic Purchase Order Item row.

Features :
- Item
- Optional Drawing
- HSN
- Quantity
- Rate
- Discount
- GST
- Required Date
- Calculated Line Amounts

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.PurchaseOrder
{
    /// <summary>
    /// Represents one Purchase Order Item row.
    /// </summary>
    public class PurchaseOrderItemViewModel
    {
        #region Identity

        public int Id { get; set; }

        #endregion


        #region Item

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select an Item.")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [ValidateNever]
        [Display(Name = "Item Code")]
        public string? ItemCode { get; set; }

        [ValidateNever]
        [Display(Name = "Item Name")]
        public string? ItemName { get; set; }

        [ValidateNever]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [ValidateNever]
        [Display(Name = "Specification")]
        public string? Specification { get; set; }

        [ValidateNever]
        [Display(Name = "UOM")]
        public string? UnitName { get; set; }

        #endregion


        #region Purchase Information

        [StringLength(
            50,
            ErrorMessage =
                "HSN Code cannot exceed 50 characters.")]
        [Display(Name = "HSN")]
        public string? HSNCode { get; set; }

        #endregion


        #region Drawing

        /// <summary>
        /// Drawing is optional because not every
        /// purchased Item requires a Drawing.
        /// </summary>
        [Display(Name = "Drawing")]
        public int? DrawingId { get; set; }

        [ValidateNever]
        [Display(Name = "Drawing Number")]
        public string? DrawingNumber
        {
            get;
            set;
        }

        [ValidateNever]
        [Display(Name = "Revision")]
        public string? DrawingRevision
        {
            get;
            set;
        }

        [ValidateNever]
        public List<SelectListItem> DrawingOptions
        {
            get;
            set;
        } = new();

        #endregion


        #region Quantity / Rate

        [Range(
            typeof(decimal),
            "0.001",
            "999999999999999.999",
            ErrorMessage =
                "Quantity must be greater than zero.")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "9999999999999999.99",
            ErrorMessage =
                "Rate cannot be negative.")]
        [Display(Name = "Rate")]
        public decimal UnitPrice { get; set; }

        #endregion


        #region Discount

        [Range(
            typeof(decimal),
            "0",
            "100",
            ErrorMessage =
                "Discount must be between 0 and 100.")]
        [Display(Name = "Discount %")]
        public decimal DiscountPercent
        {
            get;
            set;
        }

        [ValidateNever]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount
        {
            get;
            set;
        }

        #endregion


        #region GST

        [Range(
            typeof(decimal),
            "0",
            "100",
            ErrorMessage =
                "GST must be between 0 and 100.")]
        [Display(Name = "GST %")]
        public decimal GSTPercent { get; set; }

        [ValidateNever]
        [Display(Name = "Taxable Amount")]
        public decimal TaxableAmount
        {
            get;
            set;
        }

        [ValidateNever]
        [Display(Name = "CGST")]
        public decimal CGSTAmount { get; set; }

        [ValidateNever]
        [Display(Name = "SGST")]
        public decimal SGSTAmount { get; set; }

        [ValidateNever]
        [Display(Name = "IGST")]
        public decimal IGSTAmount { get; set; }

        #endregion


        #region Total

        [ValidateNever]
        [Display(Name = "Line Total")]
        public decimal LineTotal { get; set; }

        #endregion


        #region Additional

        [DataType(DataType.Date)]
        [Display(Name = "Required Date")]
        public DateTime? RequiredDate
        {
            get;
            set;
        }

        [StringLength(
            500,
            ErrorMessage =
                "Item Remarks cannot exceed 500 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        #endregion
    }
}