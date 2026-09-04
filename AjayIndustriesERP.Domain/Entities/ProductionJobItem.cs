/*
============================================================
File: ProductionJobItem.cs

Purpose:
Represents one Customer PO Item manufactured under
one parent Production Job.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Link one Customer PO Item to the parent Production Job.
- Preserve Item snapshot.
- Preserve Customer PO Ordered Quantity.
- Store Admin planned Production Quantity.
- Store cumulative Completed Quantity.
- Preserve Released Routing snapshot.
- Own the Item-wise Production Pipeline.
- Maintain optional Pipeline modification reason.

Quantity Meaning:

OrderedQuantity
    Original Customer PO Item quantity.
    Read-only Production source.

ProductionQuantity
    Cumulative quantity currently released/planned
    by Admin for Production.

CompletedQuantity
    Cumulative final GOOD quantity successfully produced.

Example:

OrderedQuantity      = 100
ProductionQuantity   = 50
CompletedQuantity    = 50

The current 50 quantity Production cycle may be complete,
but the Item is NOT fully complete because:

CompletedQuantity < OrderedQuantity

Admin may later increase:

ProductionQuantity = 100

Worker then processes the remaining 50 through the
same Item Pipeline.

Important:
- ProductionQuantity is decided by Admin before shop-floor
  execution.
- Worker cannot change ProductionQuantity from Pipeline.
- ProductionQuantity cannot exceed OrderedQuantity.
- ProductionQuantity cannot be lower than CompletedQuantity.
- Item is fully complete only when CompletedQuantity reaches
  OrderedQuantity.
- Routing changes later must not modify copied Job Steps.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ProductionJobItem : BaseEntity
    {
        #region Primary Identification

        public int Id { get; set; }

        #endregion


        #region Production Job Relationship

        public int ProductionJobId { get; set; }


        public ProductionJob ProductionJob
        {
            get;
            set;
        } = null!;

        #endregion


        #region Customer PO Item Relationship

        /// <summary>
        /// Exact source Customer Purchase Order Item.
        /// </summary>
        public int CustomerPurchaseOrderItemId { get; set; }


        public CustomerPurchaseOrderItem CustomerPurchaseOrderItem
        {
            get;
            set;
        } = null!;

        #endregion


        #region Item Relationship

        public int ItemId { get; set; }


        public Item Item { get; set; } =
            null!;

        #endregion


        #region Item Snapshot

        /// <summary>
        /// Item Code snapshot captured when the
        /// Production Job is created.
        /// </summary>
        public string ItemCode { get; set; } =
            string.Empty;


        /// <summary>
        /// Item Name snapshot captured when the
        /// Production Job is created.
        /// </summary>
        public string ItemName { get; set; } =
            string.Empty;


        public string? UnitName { get; set; }

        #endregion


        #region Production Quantity

        /// <summary>
        /// Trusted Ordered Quantity copied from the
        /// Customer Purchase Order Item.
        /// </summary>
        public decimal OrderedQuantity { get; set; }


        /// <summary>
        /// Cumulative Production Quantity currently
        /// planned / released by Admin.
        ///
        /// Example:
        ///
        /// Ordered = 100
        ///
        /// First plan:
        /// ProductionQuantity = 50
        ///
        /// Later:
        /// ProductionQuantity = 100
        /// </summary>
        public decimal ProductionQuantity { get; set; }


        /// <summary>
        /// Cumulative final GOOD quantity successfully
        /// produced for this Item.
        ///
        /// This quantity ultimately comes from completion
        /// of the final Production Pipeline Step.
        /// </summary>
        public decimal CompletedQuantity { get; set; }

        #endregion


        #region Calculated Quantity Progress

        /// <summary>
        /// Full Customer PO quantity still pending.
        ///
        /// OrderedQuantity - CompletedQuantity
        /// </summary>
        [NotMapped]
        public decimal PendingQuantity =>
            Math.Max(
                0m,
                OrderedQuantity -
                CompletedQuantity);


        /// <summary>
        /// Quantity still pending against the current
        /// Admin Production plan.
        ///
        /// ProductionQuantity - CompletedQuantity
        /// </summary>
        [NotMapped]
        public decimal ProductionPendingQuantity =>
            Math.Max(
                0m,
                ProductionQuantity -
                CompletedQuantity);


        /// <summary>
        /// Indicates that the current Admin planned
        /// Production Quantity has been completed.
        ///
        /// This does NOT mean the full Customer PO
        /// Ordered Quantity is complete.
        /// </summary>
        [NotMapped]
        public bool IsCurrentProductionCompleted =>
            ProductionQuantity > 0m
            &&
            CompletedQuantity >=
                ProductionQuantity;


        /// <summary>
        /// Indicates that the complete Customer PO
        /// Ordered Quantity has been produced.
        /// </summary>
        [NotMapped]
        public bool IsProductionCompleted =>
            OrderedQuantity > 0m
            &&
            CompletedQuantity >=
                OrderedQuantity;

        #endregion


        #region Routing Reference

        /// <summary>
        /// Released Item Routing Revision used when
        /// this Production Job Item was created.
        /// </summary>
        public int ItemProcessRoutingId { get; set; }


        public ItemProcessRouting ItemProcessRouting
        {
            get;
            set;
        } = null!;


        /// <summary>
        /// Routing Code snapshot.
        /// </summary>
        public string RoutingCode { get; set; } =
            string.Empty;


        /// <summary>
        /// Routing Revision snapshot.
        /// </summary>
        public int RoutingRevisionNumber { get; set; }

        #endregion


        #region Pipeline Remarks

        /// <summary>
        /// Optional reason when the copied Production
        /// Pipeline is modified before Production starts
        /// for this specific Item.
        ///
        /// Item Process Routing Master remains unchanged.
        /// </summary>
        public string? PipelineModificationReason { get; set; }

        #endregion


        #region Production Steps

        /// <summary>
        /// Executable Item-wise Production Pipeline.
        /// </summary>
        public ICollection<ProductionJobStep> Steps
        {
            get;
            set;
        } = new List<ProductionJobStep>();

        #endregion
    }
}