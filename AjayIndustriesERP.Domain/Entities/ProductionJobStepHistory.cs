/*
============================================================
File: ProductionJobStepHistory.cs

Purpose:
Maintains an immutable execution history for a
Production Job Step.

Responsibilities:
- Record Step Status transitions.
- Record Machine information used during the event.
- Record Good and Rejected Quantity snapshots.
- Record execution remarks.
- Record when and by whom the event occurred.

Important:
- History records are append-only.
- Existing history should not normally be edited or deleted.
- This table provides future production traceability and
  audit information.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ProductionJobStepHistory
    {
        #region Primary Identification

        public int Id { get; set; }

        #endregion


        #region Production Job Step Relationship

        public int ProductionJobStepId { get; set; }


        public ProductionJobStep ProductionJobStep
        {
            get;
            set;
        } = null!;

        #endregion


        #region Status Transition

        public ProductionJobStepStatus? PreviousStatus
        {
            get;
            set;
        }


        public ProductionJobStepStatus NewStatus
        {
            get;
            set;
        }

        #endregion


        #region Machine Snapshot

        public int? MachineId { get; set; }


        public string? MachineCode { get; set; }


        public string? MachineName { get; set; }

        #endregion


        #region Quantity Snapshot

        public decimal? GoodQuantity { get; set; }


        public decimal? RejectedQuantity { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion


        #region Audit

        public DateTime ChangedOn { get; set; } =
            DateTime.UtcNow;


        public string ChangedBy { get; set; } =
            "System";

        #endregion
    }
}