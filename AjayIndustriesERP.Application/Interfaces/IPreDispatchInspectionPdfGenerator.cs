/*
============================================================
File: IPreDispatchInspectionPdfGenerator.cs

Purpose:
Defines PDF generation abstraction for the
Pre-Dispatch / Final Inspection Report.

Responsibilities:
- Accept finalized PDI snapshot data.
- Generate Final Inspection Report PDF bytes.

Important:
- Application layer contains only the abstraction.
- QuestPDF implementation belongs in Infrastructure.
- PDF layout logic must not be placed in Controller.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPreDispatchInspectionPdfGenerator
    {
        #region Generate PDF

        byte[] Generate(
            PreDispatchInspection
                preDispatchInspection);

        #endregion
    }
}