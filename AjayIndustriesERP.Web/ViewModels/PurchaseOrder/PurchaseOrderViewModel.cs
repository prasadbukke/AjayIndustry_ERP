/*
==============================================================

File : PurchaseOrderViewModel.cs

Purpose :
Represents Purchase Order Create/Edit form data.

Features :
- Company
- Supplier
- PO Dates
- Payment / Delivery Terms
- Dynamic Purchase Order Items
- Transport / Other Charges
- Purchase Order Totals

==============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.PurchaseOrder
{
    /// <summary>
    /// Represents Purchase Order Create/Edit form data.
    /// </summary>
    public class PurchaseOrderViewModel
    {
        #region Purchase Order Information

        public int Id { get; set; }

        [Display(Name = "PO Number")]
        public string? Code { get; set; }

        [Required(
            ErrorMessage = "PO Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "PO Date")]
        public DateTime PODate { get; set; }
            = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Expected Delivery Date")]
        public DateTime? ExpectedDeliveryDate
        {
            get;
            set;
        }

        [ValidateNever]
        [Display(Name = "Status")]
        public PurchaseOrderStatus Status
        {
            get;
            set;
        } = PurchaseOrderStatus.Draft;

        #endregion


        #region Company / Supplier

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Company.")]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        #endregion


        #region Delivery / Terms

        [StringLength(
            1000,
            ErrorMessage =
                "Delivery Address cannot exceed 1000 characters.")]
        [Display(Name = "Delivery Address")]
        public string? DeliveryAddress
        {
            get;
            set;
        }

        [StringLength(
            500,
            ErrorMessage =
                "Payment Terms cannot exceed 500 characters.")]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms
        {
            get;
            set;
        }

        [StringLength(
            500,
            ErrorMessage =
                "Delivery Terms cannot exceed 500 characters.")]
        [Display(Name = "Delivery Terms")]
        public string? DeliveryTerms
        {
            get;
            set;
        }

        [StringLength(
            1000,
            ErrorMessage =
                "Remarks cannot exceed 1000 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Purchase Order Items

        /// <summary>
        /// Dynamic Purchase Order Item rows.
        /// </summary>
        public List<PurchaseOrderItemViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion


        #region Additional Charges

        [Range(
            typeof(decimal),
            "0",
            "9999999999999999.99",
            ErrorMessage =
                "Transport Charges cannot be negative.")]
        [Display(Name = "Transport Charges")]
        public decimal TransportCharges
        {
            get;
            set;
        }

        [Range(
            typeof(decimal),
            "0",
            "9999999999999999.99",
            ErrorMessage =
                "Other Charges cannot be negative.")]
        [Display(Name = "Other Charges")]
        public decimal OtherCharges
        {
            get;
            set;
        }

        [Display(Name = "Round Off")]
        public decimal RoundOffAmount
        {
            get;
            set;
        }

        #endregion


        #region Calculated Totals

        [ValidateNever]
        [Display(Name = "Sub Total")]
        public decimal SubTotal { get; set; }

        [ValidateNever]
        [Display(Name = "Discount")]
        public decimal DiscountAmount { get; set; }

        [ValidateNever]
        [Display(Name = "Taxable Amount")]
        public decimal TaxableAmount { get; set; }

        [ValidateNever]
        [Display(Name = "CGST")]
        public decimal CGSTAmount { get; set; }

        [ValidateNever]
        [Display(Name = "SGST")]
        public decimal SGSTAmount { get; set; }

        [ValidateNever]
        [Display(Name = "IGST")]
        public decimal IGSTAmount { get; set; }

        [ValidateNever]
        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }

        #endregion


        #region Status

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        #endregion


        #region Dropdown Lists

        [ValidateNever]
        public List<SelectListItem> Companies
        {
            get;
            set;
        } = new();

        [ValidateNever]
        public List<SelectListItem> Suppliers
        {
            get;
            set;
        } = new();

        [ValidateNever]
        public List<SelectListItem> ItemOptions
        {
            get;
            set;
        } = new();

        #endregion
    }
}