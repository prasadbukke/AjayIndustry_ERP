/*
============================================================
File: MachineDetailsViewModel.cs

Purpose:
Provides read-only Machine information to Details page.

Responsibilities:
- Display Machine identification.
- Display manufacturer and technical information.
- Display current manually maintained Machine Status.
- Provide an extension point for future Production Job,
  Machine Allocation and Machine History information.

Important:
Production Job information is intentionally not stored inside
the Machine Master entity.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.Machine
{
    public class MachineDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }

        public string Code { get; set; } =
            string.Empty;

        #endregion


        #region Machine Information

        public string MachineName { get; set; } =
            string.Empty;

        public string MachineType { get; set; } =
            string.Empty;

        #endregion


        #region Manufacturer Information

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public string? SerialNumber { get; set; }

        #endregion


        #region Capacity And Location

        public string? Capacity { get; set; }

        public string? Location { get; set; }

        #endregion


        #region Operational Status

        public MachineStatus Status { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion
    }
}