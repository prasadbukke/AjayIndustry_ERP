/*
============================================================
File: ProductionOperationDetailsViewModel.cs

Purpose:
Provides read-only Production Operation information to Details page.

Responsibilities:
- Display Operation Code.
- Display Operation Name.
- Display Operation Type.
- Display Description and Remarks.
- Provide an extension point for future Item Routing usage.

Important:
- Machine assignment is not part of Operation Master.
- Setup Time and Cycle Time belong to Item Process Routing.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.ProductionOperation
{
    public class ProductionOperationDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }

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
        }


        public string? Description { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion
    }
}