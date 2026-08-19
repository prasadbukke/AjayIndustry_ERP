/*
============================================================
File: CustomerPurchaseOrderFormViewModel.cs

Purpose:
Provides Customer Purchase Order data to Create/Edit forms.

Responsibilities:
- Accept Customer PO header information.
- Hold Customer and Item dropdown options.
- Hold multiple Customer PO Item rows.
- Support both Create and Edit forms.

Important:
- Customer PO Code is system generated.
- Status is controlled by workflow and is not posted as an
  editable dropdown.
- Customers and Items are loaded from existing ERP masters.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.CustomerPurchaseOrder
{
    public class CustomerPurchaseOrderFormViewModel
    {
        #region Identification

        public int Id { get; set; }


        [Display(Name = "Internal Order No")]
        public string? Code { get; set; }

        #endregion


        #region Customer

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Customer.")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }


        public string? CustomerName { get; set; }

        #endregion


        #region Customer PO Information

        [Required(
            ErrorMessage = "Customer PO Number is required.")]
        [StringLength(100)]
        [Display(Name = "Customer PO Number")]
        public string CustomerPurchaseOrderNumber { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Customer PO Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Customer PO Date")]
        public DateTime CustomerPurchaseOrderDate { get; set; } =
            DateTime.Today;


        [Required(
            ErrorMessage = "PO Received Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "PO Received Date")]
        public DateTime ReceivedDate { get; set; } =
            DateTime.Today;


        [Required(
            ErrorMessage = "Required Delivery Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Required Delivery Date")]
        public DateTime RequiredDeliveryDate { get; set; } =
            DateTime.Today;

        #endregion


        #region Priority

        [Required]
        [Display(Name = "Priority")]
        public CustomerPurchaseOrderPriority Priority { get; set; } =
            CustomerPurchaseOrderPriority.Normal;

        #endregion


        #region Status

        public CustomerPurchaseOrderStatus Status { get; set; } =
            CustomerPurchaseOrderStatus.Draft;

        #endregion


        #region Reference And Remarks

        [StringLength(200)]
        [Display(Name = "Customer Reference")]
        public string? CustomerReference { get; set; }


        [StringLength(1000)]
        public string? Remarks { get; set; }

        #endregion


        #region Purchase Order Items

        public List<CustomerPurchaseOrderItemViewModel> Items { get; set; } =
            new();

        #endregion


        #region Dropdown Data

        public List<SelectListItem> Customers { get; set; } =
            new();


        public List<SelectListItem> AvailableItems { get; set; } =
            new();

        #endregion
    }
}