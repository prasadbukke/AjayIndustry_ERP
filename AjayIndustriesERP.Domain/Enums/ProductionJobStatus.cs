/*
============================================================
File: ProductionJobStatus.cs

Purpose:
Defines the lifecycle status of a Production Job.

Responsibilities:
- Identify whether a Production Job is being prepared.
- Identify whether the Job is ready for shop-floor execution.
- Track active production.
- Identify completed or cancelled Jobs.

Workflow:
Draft -> Ready -> InProgress -> Completed

Alternative:
Draft / Ready / InProgress -> Cancelled

Important:
- Draft Job is still being prepared.
- Ready Job can be started on the shop floor.
- InProgress means production execution has started.
- Completed means all required Production Steps are complete.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum ProductionJobStatus
    {
        #region Production Job Status

        Draft = 1,

        Ready = 2,

        InProgress = 3,

        Completed = 4,

        Cancelled = 5

        #endregion
    }
}