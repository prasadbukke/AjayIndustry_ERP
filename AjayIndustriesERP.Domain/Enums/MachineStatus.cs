/*
============================================================
File: MachineStatus.cs

Purpose:
Defines the current operational status of a Machine.

Responsibilities:
- Represent the manually maintained operational condition
  of a Machine.
- Support Machine availability display.
- Support future Production Job and Machine Allocation screens.

Status Meaning:
Available   = Machine is available for production work.
Running     = Machine is currently being used.
Breakdown   = Machine cannot operate because of a breakdown.
Maintenance = Machine is under planned/unplanned maintenance.
Offline     = Machine is intentionally not available.

Important:
- Machine Status is updated manually by ERP users.
- ERP is not directly connected to the physical Machine.
- This status must not be confused with Production Job Step
  Status such as Pending / Running / Completed / Failed.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum MachineStatus
    {
        #region Status Values

        Available = 1,

        Running = 2,

        Breakdown = 3,

        Maintenance = 4,

        Offline = 5

        #endregion
    }
}