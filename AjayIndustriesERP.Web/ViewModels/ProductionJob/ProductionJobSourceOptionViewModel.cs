/*
============================================================
File: ProductionJobSourceOptionViewModel.cs

Purpose:
Represents one Customer PO Item available for Production Job
creation.

Responsibilities:
- Display Customer PO information.
- Display Item and Ordered Quantity.
- Display already allocated Production Quantity.
- Display remaining Production Quantity.
- Display current Released Routing information.

Important:
- This ViewModel belongs only to the Web layer.
- Production Job business validation remains in
  ProductionJobService.
============================================================
*/

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobSourceOptionViewModel
    {
        #region Customer PO Item

        public int CustomerPurchaseOrderItemId { get; set; }

        #endregion


        #region Customer PO

        public string CustomerPurchaseOrderCode { get; set; } =
            string.Empty;

        public string CustomerPurchaseOrderNumber { get; set; } =
            string.Empty;

        public string CustomerName { get; set; } =
            string.Empty;

        #endregion


        #region Item

        public int ItemId { get; set; }

        public string ItemCode { get; set; } =
            string.Empty;

        public string ItemName { get; set; } =
            string.Empty;

        public string? UnitName { get; set; }

        #endregion


        #region Quantity

        public decimal OrderedQuantity { get; set; }

        public decimal AllocatedQuantity { get; set; }

        public decimal RemainingQuantity { get; set; }

        #endregion


        #region Routing

        public bool HasReleasedRouting { get; set; }

        public string? RoutingCode { get; set; }

        public int? RoutingRevisionNumber { get; set; }

        #endregion
    }
}