/*
============================================================
File: CustomerPurchaseOrderItemViewModel.cs

Purpose:
Represents one Customer Purchase Order Item in Create/Edit UI.

Responsibilities:
- Hold selected existing Item Master Id.
- Display Item snapshot information.
- Accept Customer-specific Item references.
- Accept Ordered Quantity.
- Accept optional line Delivery Date override.
- Accept optional line Priority override.
- Accept line remarks.

Important:
- ItemCode / ItemName / Specification / UnitName are display
  values only.
- Application Service reloads trusted Item Master information
  before saving.
- ItemId references the existing Item Master.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.CustomerPurchaseOrder
{
    public class CustomerPurchaseOrderItemViewModel
    {
        #region Identification

        public int Id { get; set; }

        #endregion


        #region Item Master

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select an Item.")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }


        public string ItemCode { get; set; } =
            string.Empty;


        public string ItemName { get; set; } =
            string.Empty;


        public string? Specification { get; set; }


        public string UnitName { get; set; } =
            string.Empty;

        #endregion


        #region Customer Item Reference

        [StringLength(100)]
        [Display(Name = "Customer Item Code")]
        public string? CustomerItemCode { get; set; }


        [StringLength(150)]
        [Display(Name = "Customer Drawing Number")]
        public string? CustomerDrawingNumber { get; set; }


        [StringLength(50)]
        [Display(Name = "Revision")]
        public string? Revision { get; set; }

        #endregion


        #region Quantity

        [Range(
            typeof(decimal),
            "0.001",
            "999999999999999.999",
            ErrorMessage =
                "Ordered Quantity must be greater than zero.")]
        [Display(Name = "Ordered Quantity")]
        public decimal OrderedQuantity { get; set; }

        #endregion


        #region Delivery Date Override

        [DataType(DataType.Date)]
        [Display(Name = "Required Delivery Date")]
        public DateTime? RequiredDeliveryDate { get; set; }

        #endregion


        #region Priority Override

        [Display(Name = "Priority")]
        public CustomerPurchaseOrderPriority? Priority { get; set; }

        #endregion


        #region Remarks

        [StringLength(1000)]
        public string? Remarks { get; set; }

        #endregion
    }
}