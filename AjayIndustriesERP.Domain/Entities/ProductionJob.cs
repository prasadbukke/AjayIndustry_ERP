/*
============================================================
File: ProductionJob.cs

Purpose:
Represents one Production Job created for one
Customer Purchase Order.

Responsibilities:
- Link Production execution to one Customer PO.
- Maintain one Production Job Code for the complete PO.
- Maintain Production Job lifecycle.
- Maintain planned and actual production timestamps.
- Own all Item-wise Production Jobs.
- Act as the parent for Item-wise Production Pipelines.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Important:
- One Customer PO has only one active Production Job.
- All Customer PO Items are created under the same
  Production Job.
- Item Quantity, Routing and Pipeline belong to
  ProductionJobItem.
- Production Job becomes Completed only when all
  ProductionJobItems complete their full Ordered Quantity.
- Routing changes after Job creation must never modify
  existing Production Job Item Steps.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ProductionJob : BaseEntity
    {
        #region Primary Identification

        public int Id { get; set; }


        /// <summary>
        /// Internal ERP generated Production Job Code.
        ///
        /// Example:
        /// AI/PJOB/26-27/00001
        /// </summary>
        public string Code { get; set; } =
            string.Empty;

        #endregion


        #region Customer Purchase Order Relationship

        /// <summary>
        /// Source Customer Purchase Order.
        ///
        /// One Customer Purchase Order has one
        /// Production Job.
        /// </summary>
        public int CustomerPurchaseOrderId { get; set; }


        public CustomerPurchaseOrder CustomerPurchaseOrder
        {
            get;
            set;
        } = null!;

        #endregion


        #region Job Status

        /// <summary>
        /// Production Job lifecycle.
        ///
        /// Draft
        ///     Admin is preparing Production planning.
        ///
        /// Ready
        ///     Production is released to shop-floor.
        ///
        /// InProgress
        ///     Production execution has started.
        ///
        /// Completed
        ///     Every ProductionJobItem has completed
        ///     its full Customer PO Ordered Quantity.
        ///
        /// Cancelled
        ///     Production Job has been cancelled.
        /// </summary>
        public ProductionJobStatus Status { get; set; } =
            ProductionJobStatus.Draft;

        #endregion


        #region Production Planning

        public DateTime? PlannedStartOn { get; set; }


        public DateTime? PlannedCompletionOn { get; set; }

        #endregion


        #region Actual Production Time

        public DateTime? StartedOn { get; set; }


        public DateTime? CompletedOn { get; set; }


        public DateTime? CancelledOn { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }


        /// <summary>
        /// Mandatory reason when a Ready or In Progress
        /// Production Job is cancelled.
        /// </summary>
        public string? CancellationReason { get; set; }

        #endregion


        #region Production Job Items

        /// <summary>
        /// All Customer PO Items manufactured under this
        /// Production Job.
        ///
        /// Each ProductionJobItem owns:
        /// - Ordered Quantity
        /// - Production Quantity
        /// - Completed Quantity
        /// - Routing snapshot
        /// - Production Pipeline
        /// </summary>
        public ICollection<ProductionJobItem> Items
        {
            get;
            set;
        } = new List<ProductionJobItem>();

        #endregion
    }
}