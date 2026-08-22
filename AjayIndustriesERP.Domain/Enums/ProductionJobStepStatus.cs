/*
============================================================
File: ProductionJobStepStatus.cs

Purpose:
Defines the execution status of a Production Job Step.

Responsibilities:
- Track waiting Production Steps.
- Track the currently running Production Step.
- Track successfully completed Production Steps.
- Identify a Step that was running when the Production Job
  was cancelled.

Workflow:
Pending -> InProgress -> Completed

Cancellation:
InProgress -> Cancelled

Important:
- Failed, Rejected and Skipped statuses are intentionally not
  used in the current Production workflow.
- Rejected Quantity remains available as a quantity/result,
  but Rejected is not a Step Status.
- Current Step must be Completed before the next Step starts.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum ProductionJobStepStatus
    {
        #region Production Step Status

        Pending = 1,

        InProgress = 2,

        Completed = 3,

        Cancelled = 4

        #endregion
    }
}