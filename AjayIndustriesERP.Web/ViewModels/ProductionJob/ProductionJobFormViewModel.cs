/*
============================================================
File: ProductionJobFormViewModel.cs

Purpose:
Provides data required to create a Production Job.

Responsibilities:
- Select a confirmed Customer PO Item.
- Accept Production Job Quantity.
- Accept optional Production planning dates.
- Accept optional remarks.
- Provide Customer PO Item source information.

Important:
- Item and Routing are derived automatically from the
  selected Customer PO Item.
- User does not manually select Routing.
- Production Job Quantity cannot exceed remaining Customer
  PO Quantity.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobFormViewModel
    {

        #region Edit Information

        public int Id { get; set; }

        public string? Code { get; set; }

        public ProductionJobStatus Status { get; set; } =
            ProductionJobStatus.Draft;


        public string? CustomerPurchaseOrderCode { get; set; }

        public string? CustomerPurchaseOrderNumber { get; set; }

        public string? CustomerName { get; set; }


        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        public string? UnitName { get; set; }


        public string? RoutingCode { get; set; }

        public int? RoutingRevisionNumber { get; set; }

        #endregion

        #region Customer PO Item

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Customer PO Item is required.")]
        [Display(Name = "Customer PO Item")]
        public int CustomerPurchaseOrderItemId { get; set; }

        #endregion


        #region Job Quantity

        [Range(
            typeof(decimal),
            "0.001",
            "999999999999999.999",
            ErrorMessage = "Production Job Quantity must be greater than zero.")]
        [Display(Name = "Job Quantity")]
        public decimal JobQuantity { get; set; }

        #endregion


        #region Planning

        [Display(Name = "Planned Start")]
        [DataType(DataType.Date)]
        public DateTime? PlannedStartOn { get; set; }


        [Display(Name = "Planned Completion")]
        [DataType(DataType.Date)]
        public DateTime? PlannedCompletionOn { get; set; }

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion


        #region Source Options

        public List<ProductionJobSourceOptionViewModel>
            SourceItems
        { get; set; } = new();

        #endregion
    }
}