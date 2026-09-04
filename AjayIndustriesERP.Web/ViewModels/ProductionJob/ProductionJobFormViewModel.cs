/*
============================================================
File: ProductionJobFormViewModel.cs

Purpose:
Provides data required to create and edit one
PO-level Production Job.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Items
        ↓
Production Job Steps

Responsibilities:
- Select one confirmed Customer Purchase Order.
- Display Customer PO information.
- Auto-load all Customer PO Items.
- Display Item Ordered Quantity.
- Accept Item-wise Production Quantity from Admin.
- Display cumulative Completed Quantity.
- Display remaining Ordered Quantity.
- Display Released Routing information.
- Accept optional Production planning dates.
- Accept optional remarks.

Quantity Meaning:

OrderedQuantity
    Customer PO Item quantity.
    Read-only.

ProductionQuantity
    Cumulative quantity currently planned by Admin.

CompletedQuantity
    Cumulative actual finished Production quantity.

PendingQuantity
    OrderedQuantity - CompletedQuantity.

Example:

OrderedQuantity      = 100
ProductionQuantity   = 50
CompletedQuantity    = 50
PendingQuantity      = 50

Later Admin may increase:

ProductionQuantity = 100

Important:
- One Customer PO has one Production Job.
- Production Quantity is decided by Admin.
- Worker cannot change Production Quantity from Pipeline.
- Production Quantity cannot exceed Ordered Quantity.
- Production Quantity cannot be less than Completed Quantity.
- Item / Ordered Quantity / Routing are trusted from database.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobFormViewModel
    {
        #region Production Job

        public int Id { get; set; }


        public string? Code { get; set; }


        public ProductionJobStatus Status { get; set; } =
            ProductionJobStatus.Draft;

        #endregion


        #region Customer Purchase Order

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Customer Purchase Order is required.")]
        [Display(
            Name =
                "Customer Purchase Order")]
        public int CustomerPurchaseOrderId
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderCode
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderNumber
        {
            get;
            set;
        }


        public string? CustomerName
        {
            get;
            set;
        }


        public DateTime? ReceivedDate
        {
            get;
            set;
        }


        public DateTime? RequiredDeliveryDate
        {
            get;
            set;
        }

        #endregion


        #region Planning

        [Display(
            Name =
                "Planned Start")]
        [DataType(
            DataType.Date)]
        public DateTime? PlannedStartOn
        {
            get;
            set;
        }


        [Display(
            Name =
                "Planned Completion")]
        [DataType(
            DataType.Date)]
        public DateTime? PlannedCompletionOn
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage =
                "Remarks cannot exceed 1000 characters.")]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Production Items

        /// <summary>
        /// All active Items belonging to the selected
        /// Customer Purchase Order.
        ///
        /// Admin enters ProductionQuantity Item-wise.
        /// </summary>
        public List<ProductionJobFormItemViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion


        #region Customer PO Dropdown

        /// <summary>
        /// Confirmed Customer Purchase Orders available
        /// for Production Job creation.
        ///
        /// Customer POs already having a Production Job
        /// are not included.
        /// </summary>
        public List<SelectListItem>
            CustomerPurchaseOrders
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Production Job Form Item
    ============================================================
    */

    public class ProductionJobFormItemViewModel
    {
        #region Identification

        /// <summary>
        /// Existing ProductionJobItem Id.
        ///
        /// Zero during Create.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Trusted Customer PO Item source.
        /// </summary>
        public int CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public int ItemId
        {
            get;
            set;
        }

        #endregion


        #region Item Information

        public string ItemCode
        {
            get;
            set;
        } = string.Empty;


        public string ItemName
        {
            get;
            set;
        } = string.Empty;


        public string? UnitName
        {
            get;
            set;
        }

        #endregion


        #region Quantity

        /// <summary>
        /// Trusted Customer PO Ordered Quantity.
        /// </summary>
        public decimal OrderedQuantity
        {
            get;
            set;
        }


        /// <summary>
        /// Cumulative Production target decided by Admin.
        ///
        /// Example:
        ///
        /// Ordered = 100
        ///
        /// First:
        /// ProductionQuantity = 50
        ///
        /// Later:
        /// ProductionQuantity = 100
        /// </summary>
        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage =
                "Production Quantity cannot be negative.")]
        [Display(
            Name =
                "Production Quantity")]
        public decimal ProductionQuantity
        {
            get;
            set;
        }


        /// <summary>
        /// Cumulative finished Production quantity.
        /// Read-only on UI.
        /// </summary>
        public decimal CompletedQuantity
        {
            get;
            set;
        }


        /// <summary>
        /// Full Customer PO quantity still pending.
        /// </summary>
        public decimal PendingQuantity =>
            Math.Max(
                0m,
                OrderedQuantity -
                CompletedQuantity);


        /// <summary>
        /// Quantity pending against current Admin plan.
        /// </summary>
        public decimal ProductionPendingQuantity =>
            Math.Max(
                0m,
                ProductionQuantity -
                CompletedQuantity);

        #endregion


        #region Routing

        public int ItemProcessRoutingId
        {
            get;
            set;
        }


        public string? RoutingCode
        {
            get;
            set;
        }


        public int? RoutingRevisionNumber
        {
            get;
            set;
        }


        public bool HasReleasedRouting
        {
            get;
            set;
        }

        #endregion


        #region Delivery

        public DateTime? RequiredDeliveryDate
        {
            get;
            set;
        }

        #endregion
    }
}