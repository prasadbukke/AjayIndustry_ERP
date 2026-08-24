/*
============================================================
File: IPreDispatchInspectionService.cs

Purpose:
Defines business operations for the
Pre-Dispatch / Final Inspection module.

Responsibilities:
- Read PDI Reports.
- Search and paginate PDI Reports.
- Load eligible Production Job sources.
- Prepare trusted PDI source information.
- Calculate remaining quantity available for inspection.
- Create and edit Draft PDI Reports.
- Finalize PDI Reports.
- Generate Final Inspection Report PDF.
- Soft-delete and restore PDI Reports.

Important:
- Business rules belong in the Application Service.
- Production Job is the primary PDI source.
- Customer / PO / Item / Drawing data must be trusted
  from ERP source records and not from browser input.
- One Production Job may have multiple PDI Reports.
- Finalized PDI Reports are locked.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPreDispatchInspectionService
    {
        #region Read Operations

        Task<PreDispatchInspection?>
            GetByIdAsync(
                int id);


        Task<PagedResult<PreDispatchInspection>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Production Job Source

        /*
         * Returns Production Jobs eligible for
         * Pre-Dispatch Inspection.
         *
         * Normally only completed Production Jobs
         * having remaining quantity available for
         * inspection should be returned.
         */

        Task<List<ProductionJob>>
            GetProductionJobsForInspectionAsync();


        /*
         * Loads the selected Production Job with
         * trusted source information required by
         * the PDI Create screen.
         */

        Task<ProductionJob?>
            GetProductionJobForInspectionAsync(
                int productionJobId);


        /*
         * Remaining quantity available for a new
         * or edited PDI Report.
         *
         * Example:
         *
         * Job Qty       = 100
         * PDI-001 Qty   = 30
         * PDI-002 Qty   = 40
         * Remaining     = 30
         *
         * excludePreDispatchInspectionId is used
         * while editing an existing Draft PDI.
         */

        Task<decimal>
            GetRemainingInspectionQuantityAsync(
                int productionJobId,
                int? excludePreDispatchInspectionId = null);

        #endregion


        #region Prepare Draft Source

        /*
         * Creates an in-memory PDI structure from
         * the selected Production Job.
         *
         * This method DOES NOT save the Report.
         *
         * It prepares trusted snapshot values:
         *
         * - Production Job
         * - Customer
         * - Customer PO
         * - Customer Item Code / Part Number
         * - Item
         * - Unit
         * - Current Workshop Drawing
         * - Current Customer Drawing
         * - Item Specification Lines
         *
         * The Controller can use this prepared data
         * to build the Create form.
         */

        Task<PreDispatchInspection?>
            PrepareDraftAsync(
                int productionJobId);

        #endregion


        #region Create

        Task<PreDispatchInspection>
            CreateAsync(
                PreDispatchInspection
                    preDispatchInspection);

        #endregion


        #region Update Draft

        Task<PreDispatchInspection>
            UpdateAsync(
                PreDispatchInspection
                    preDispatchInspection);

        #endregion


        #region Finalize

        /*
         * Finalizes the PDI Report.
         *
         * Finalization will validate:
         *
         * - Inspection Quantity
         * - Accepted / Rework / Rejected Quantity
         * - Inspection Lines
         * - Observations
         * - Line Results
         * - Overall Result
         *
         * Once Finalized, the Report becomes
         * read-only.
         */

        Task<PreDispatchInspection>
            FinalizeAsync(
                int id);

        #endregion


        #region PDF

        /*
         * Document Number is temporary and is NOT stored
         * in the PDI database.
         *
         * Current default from Web UI:
         *
         * AI / QA / 04
         *
         * Later this value will come from the
         * Document Number Master.
         */

        Task<byte[]> GeneratePdfAsync(
            int id,
            string documentNumber);

        #endregion


        #region Delete

        Task DeleteAsync(
            int id);

        #endregion


        #region Deleted Reports

        Task<List<PreDispatchInspection>>
            GetDeletedAsync();


        Task RestoreAsync(
            int id);

        #endregion
    }
}