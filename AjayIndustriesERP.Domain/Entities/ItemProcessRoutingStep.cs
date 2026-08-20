/*
============================================================
File: ItemProcessRoutingStep.cs

Purpose:
Represents one ordered manufacturing Step inside an
Item Process Routing.

Responsibilities:
- Maintain Step sequence.
- Link the Step to Production Operation Master.
- Store an optional Default Machine.
- Store estimated Setup Time.
- Store estimated Cycle Time per piece.
- Store optional Operation Instructions and Remarks.

Important:
- DefaultMachineId is only the preferred/default Machine.
- Actual Machine may be changed during Production execution.
- SetupTimeMinutes is a one-time setup estimate.
- CycleTimeMinutes is an estimated time per piece.
- Actual Start / End / Duration do NOT belong here.
- Same Production Operation may appear multiple times within
  a Routing if required.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ItemProcessRoutingStep : BaseEntity
    {
        #region Primary Identification

        public int Id { get; set; }

        #endregion


        #region Routing Relationship

        public int ItemProcessRoutingId { get; set; }


        public ItemProcessRouting ItemProcessRouting
        {
            get;
            set;
        } = null!;

        #endregion


        #region Sequence

        /// <summary>
        /// Manufacturing sequence.
        ///
        /// Recommended values:
        /// 10, 20, 30, 40...
        ///
        /// Gaps allow future Operations to be inserted without
        /// renumbering every Step.
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

        #endregion


        #region Default Machine

        /// <summary>
        /// Preferred Machine for this Item and Operation.
        ///
        /// Optional because Inspection or manual Operations
        /// may not require a Machine.
        /// </summary>
        public int? DefaultMachineId { get; set; }


        public Machine? DefaultMachine { get; set; }

        #endregion


        #region Estimated Time

        /// <summary>
        /// One-time setup duration in minutes.
        /// Example: 15.000 minutes.
        /// </summary>
        public decimal? SetupTimeMinutes { get; set; }


        /// <summary>
        /// Estimated processing duration per piece in minutes.
        /// Example: 3.500 minutes per piece.
        /// </summary>
        public decimal? CycleTimeMinutes { get; set; }

        #endregion


        #region Instructions

        public string? OperationInstruction { get; set; }


        public string? Remarks { get; set; }

        #endregion
    }
}