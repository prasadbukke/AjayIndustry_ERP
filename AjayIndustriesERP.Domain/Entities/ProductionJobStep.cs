/*
============================================================
File: ProductionJobStep.cs

Purpose:
Represents one executable manufacturing Step inside an
actual Production Job.

Responsibilities:
- Preserve Routing Step sequence and Operation snapshot.
- Preserve planned/default Machine.
- Store actual Assigned Machine.
- Store estimated Setup and Cycle Time.
- Track Production Step execution status.
- Track actual Start and Completion timestamps.
- Track Good and Rejected quantities.
- Store execution remarks.
- Maintain Step execution history.

Important:
- This is an execution transaction, not a Routing template.
- Routing changes after Job creation must not modify this Step.
- Default Machine and Assigned Machine are intentionally
  separate.
- Job Step Status and Machine Status are separate concepts.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ProductionJobStep : BaseEntity
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


        #region Sequence

        /// <summary>
        /// Copied from Item Process Routing.
        ///
        /// Example:
        /// 10, 20, 30, 40...
        /// </summary>
        public int SequenceNumber { get; set; }

        #endregion


        #region Production Operation

        public int ProductionOperationId { get; set; }


        public ProductionOperation ProductionOperation
        {
            get;
            set;
        } = null!;


        /*
         * Operation snapshot.
         */

        public string OperationCode { get; set; } =
            string.Empty;


        public string OperationName { get; set; } =
            string.Empty;


        public ProductionOperationType OperationType
        {
            get;
            set;
        }

        #endregion


        #region Default Machine

        /// <summary>
        /// Machine copied from the Item Process Routing.
        /// </summary>
        public int? DefaultMachineId { get; set; }


        public Machine? DefaultMachine { get; set; }

        #endregion


        #region Actual Assigned Machine

        /// <summary>
        /// Actual Machine selected for executing this Job Step.
        ///
        /// It may be different from DefaultMachineId.
        /// </summary>
        public int? AssignedMachineId { get; set; }


        public Machine? AssignedMachine { get; set; }

        #endregion


        #region Estimated Time Snapshot

        public decimal? SetupTimeMinutes { get; set; }


        public decimal? CycleTimeMinutes { get; set; }

        #endregion


        #region Routing Instructions Snapshot

        public string? OperationInstruction { get; set; }


        public string? RoutingRemarks { get; set; }

        #endregion


        #region Step Status

        public ProductionJobStepStatus Status { get; set; } =
            ProductionJobStepStatus.Pending;

        #endregion


        #region Actual Execution Time

        public DateTime? StartedOn { get; set; }


        public DateTime? CompletedOn { get; set; }

        #endregion


        #region Production Quantity

        /// <summary>
        /// Successfully processed quantity for this Step.
        /// </summary>
        public decimal? GoodQuantity { get; set; }


        /// <summary>
        /// Quantity rejected during this Step.
        /// </summary>
        public decimal? RejectedQuantity { get; set; }

        #endregion


        #region Execution Remarks

        /// <summary>
        /// Actual shop-floor execution remarks.
        /// </summary>
        public string? ExecutionRemarks { get; set; }

        #endregion


        #region Execution History

        public ICollection<ProductionJobStepHistory> History
        {
            get;
            set;
        } = new List<ProductionJobStepHistory>();

        #endregion
    }
}