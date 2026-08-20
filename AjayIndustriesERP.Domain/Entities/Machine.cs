/*
============================================================
File: Machine.cs

Purpose:
Represents a production Machine registered in Ajay Industries ERP.

Responsibilities:
- Store the internal ERP Machine Code.
- Store Machine identity and classification.
- Store manufacturer / model / serial information.
- Store optional capacity and physical location.
- Store the manually maintained operational Machine Status.
- Act as the Machine Master reference for future Production
  Job Step allocation and Production Pipeline tracking.

Important:
- Machine Code is generated automatically by Application Service.
- Machine Status is manually updated by ERP users.
- ERP is not directly connected to the physical Machine.
- Production Job / Step Status must not be stored in this entity.
- IsActive determines whether this Machine can be selected in
  future ERP transactions.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Machine : BaseEntity
    {
        #region Primary Identification

        public int Id { get; set; }


        /// <summary>
        /// Internal ERP generated Machine Code.
        /// Example: AI/MCH/00001
        /// </summary>
        public string Code { get; set; } =
            string.Empty;

        #endregion


        #region Machine Information

        public string MachineName { get; set; } =
            string.Empty;


        /// <summary>
        /// General Machine classification.
        /// Examples:
        /// CNC, VMC, Lathe, Drilling, Grinding, Cutting.
        /// </summary>
        public string MachineType { get; set; } =
            string.Empty;

        #endregion


        #region Manufacturer Information

        public string? Manufacturer { get; set; }


        public string? Model { get; set; }


        public string? SerialNumber { get; set; }

        #endregion


        #region Capacity And Location

        /// <summary>
        /// Free-text Machine capacity information.
        /// Examples:
        /// 500 mm Dia
        /// 1000 x 500 Table
        /// 5 Ton
        ///
        /// A structured capacity model may be introduced later
        /// if Production Planning requires it.
        /// </summary>
        public string? Capacity { get; set; }


        /// <summary>
        /// Physical shop-floor location or section.
        /// Example:
        /// Shop Floor A
        /// CNC Section
        /// Bay 2
        /// </summary>
        public string? Location { get; set; }

        #endregion


        #region Operational Status

        public MachineStatus Status { get; set; } =
            MachineStatus.Available;

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion
    }
}