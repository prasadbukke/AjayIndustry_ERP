/*
============================================================
File: IPreDispatchInspectionRepository.cs

Purpose:
Defines database operations required by the
Pre-Dispatch / Final Inspection module.

Responsibilities:
- Read PDI Reports.
- Search and paginate PDI Reports.
- Load Production Job source information.
- Calculate already allocated Inspection Quantity.
- Generate sequential PDI Code.
- Persist PDI Header, Lines and Observations.
- Support Draft editing.
- Support Finalization.
- Support soft delete and restore.

Important:
- Business rules must not be implemented here.
- One Production Job may have multiple PDI Reports.
- PDI Lines and Observations are loaded with the Report
  where required.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPreDispatchInspectionRepository
    {
        #region Read Operations

        Task<PreDispatchInspection?>
            GetByIdAsync(
                int id);


        Task<PreDispatchInspection?>
            GetForUpdateAsync(
                int id);


        Task<PagedResult<PreDispatchInspection>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<PreDispatchInspection>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Production Job Source

        /*
         * Returns Production Jobs eligible to be used
         * as source for a PDI Report.
         *
         * Final business validation will be handled
         * inside Application Service.
         */

        Task<List<ProductionJob>>
            GetProductionJobsForInspectionAsync();


        /*
         * Loads one Production Job with the source data
         * required for PDI auto-fill:
         *
         * - Customer PO
         * - Customer
         * - Item
         * - Item Specifications
         * - Current Workshop Drawing
         */

        Task<ProductionJob?>
            GetProductionJobForInspectionAsync(
                int productionJobId);


        /*
         * Calculates Inspection Quantity already allocated
         * against active PDI Reports for this Production Job.
         *
         * excludePreDispatchInspectionId is used during Edit
         * so the current Report does not count itself.
         */

        Task<decimal>
            GetAllocatedInspectionQuantityAsync(
                int productionJobId,
                int? excludePreDispatchInspectionId = null);

        #endregion


        #region PDI Code

        /*
         * Used for code generation:
         *
         * AI/PDI/{YY-YY}/{00001}
         */

        Task<string?>
            GetLastCodeAsync(
                string prefix);

        #endregion


        #region Persistence

        Task AddAsync(
            PreDispatchInspection
                preDispatchInspection);


        Task UpdateAsync(
            PreDispatchInspection
                preDispatchInspection);

        #endregion


        #region Deleted Reports

        Task<List<PreDispatchInspection>>
            GetDeletedAsync();


        Task<PreDispatchInspection?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion
    }
}