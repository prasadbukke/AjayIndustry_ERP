/*
============================================================
File: ProductionJob.cs

Purpose:
Represents an actual manufacturing Job created for a
Customer Purchase Order Item.

Responsibilities:
- Link Production execution to Customer PO Item.
- Identify the Item and Job Quantity.
- Record which Released Routing Revision created the Job.
- Maintain Production Job lifecycle.
- Maintain planned and actual production timestamps.
- Contain executable Production Job Steps.

Important:
- One Customer PO Item may create multiple Production Jobs.
- Routing Steps are copied into ProductionJobSteps when the
  Job is created.
- Existing Job Steps must never change if Item Routing is
  revised later.
- Actual shop-floor execution belongs to this module,
  not Item Process Routing.
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


        #region Customer PO Item Relationship

        /// <summary>
        /// Source Customer Purchase Order line.
        ///
        /// One Customer PO Item may create multiple
        /// Production Jobs for batch-wise production.
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


        /// <summary>
        /// Item Master snapshot at Job creation time.
        /// </summary>
        public string ItemCode { get; set; } =
            string.Empty;


        public string ItemName { get; set; } =
            string.Empty;


        public string? UnitName { get; set; }

        #endregion


        #region Job Quantity

        /// <summary>
        /// Quantity assigned to this Production Job.
        ///
        /// Example:
        /// Customer PO Qty = 1000
        ///
        /// JOB-001 = 400
        /// JOB-002 = 300
        /// JOB-003 = 300
        /// </summary>
        public decimal JobQuantity { get; set; }

        #endregion


        #region Routing Reference

        /// <summary>
        /// Released Routing Revision used when the Job
        /// was generated.
        /// </summary>
        public int ItemProcessRoutingId { get; set; }


        public ItemProcessRouting ItemProcessRouting
        {
            get;
            set;
        } = null!;


        /*
         * Routing snapshot fields are intentionally stored.
         *
         * Even if Routing Master data changes later,
         * the Job continues showing exactly which Routing
         * Code / Revision created it.
         */

        public string RoutingCode { get; set; } =
            string.Empty;


        public int RoutingRevisionNumber { get; set; }

        #endregion


        #region Job Status

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
        /// Optional reason describing why the copied Production
        /// Pipeline was modified for this specific Job.
        ///
        /// This does not modify the Item Process Routing Master.
        /// </summary>
        public string? PipelineModificationReason { get; set; }


        /// <summary>
        /// Mandatory reason when a Ready or In Progress
        /// Production Job is cancelled.
        /// </summary>
        public string? CancellationReason { get; set; }

        #endregion


        #region Production Steps

        public ICollection<ProductionJobStep> Steps
        {
            get;
            set;
        } = new List<ProductionJobStep>();

        #endregion
    }
}