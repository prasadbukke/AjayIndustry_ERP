/*
============================================================
File: ProductionOperation.cs

Purpose:
Represents a reusable Production Operation / Process Master.

Responsibilities:
- Store ERP generated Operation Code.
- Store Operation Name.
- Classify Operation as Production or Inspection.
- Store optional description and remarks.
- Act as the Operation reference for future Item Process
  Routing and Production Job Steps.

Important:
- Operation Code is generated automatically.
- Setup Time and Cycle Time are NOT stored here because they
  vary by Item.
- Machine assignment is NOT stored here.
- Machine selection will be defined through Item Process
  Routing / Production execution.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ProductionOperation : BaseEntity
    {
        #region Primary Identification

        public int Id { get; set; }


        /// <summary>
        /// Internal ERP generated Operation Code.
        /// Example: AI/OPR/00001
        /// </summary>
        public string Code { get; set; } =
            string.Empty;

        #endregion


        #region Operation Information

        public string OperationName { get; set; } =
            string.Empty;


        public ProductionOperationType OperationType
        {
            get;
            set;
        } = ProductionOperationType.Production;


        public string? Description { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion
    }
}