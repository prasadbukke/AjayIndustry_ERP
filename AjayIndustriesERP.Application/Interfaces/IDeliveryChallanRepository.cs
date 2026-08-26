/*
============================================================
File: IDeliveryChallanRepository.cs

Purpose:
Defines data-access contract for Delivery Challan.

Responsibilities:
- Retrieve Delivery Challans.
- Search and paginate active Challans.
- Load Finalized PDI Reports eligible for dispatch.
- Load one Finalized PDI source.
- Calculate quantity already allocated to Challans.
- Retrieve last Challan Code.
- Persist Challan changes.
- Retrieve deleted Challans for restore.

Important:
- Business rules belong in DeliveryChallanService.
- Repository only performs data access.
- Draft and Finalized active Challans both reserve
  Dispatch Quantity.
- One Finalized PDI may be dispatched through
  multiple Delivery Challans.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IDeliveryChallanRepository
    {
        #region Read Operations

        Task<DeliveryChallan?>
            GetByIdAsync(
                int id);


        Task<DeliveryChallan?>
            GetForUpdateAsync(
                int id);


        Task<PagedResult<DeliveryChallan>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<DeliveryChallan>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Finalized PDI Source

        /*
         * Returns Finalized PDI Reports having
         * Accepted Quantity available for dispatch.
         *
         * Remaining quantity filtering will be
         * completed in Application Service.
         */

        Task<List<PreDispatchInspection>>
            GetFinalizedPdisForDispatchAsync();


        /*
         * Loads one Finalized PDI Report as the
         * trusted source for Challan creation.
         */

        Task<PreDispatchInspection?>
            GetFinalizedPdiForDispatchAsync(
                int preDispatchInspectionId);


        /*
         * Calculates quantity already reserved /
         * dispatched against one PDI.
         *
         * Active Draft Challans are included because
         * a Draft must also reserve its quantity and
         * prevent another Challan from over-dispatching.
         *
         * During Edit, the current Challan can be
         * excluded from the calculation.
         */

        Task<decimal>
            GetAllocatedDispatchQuantityAsync(
                int preDispatchInspectionId,
                int? excludeDeliveryChallanId = null);

        #endregion


        #region Challan Code

        /*
         * Used for sequential Challan Code generation.
         *
         * Deleted Challans must also be considered so
         * document numbers are never reused.
         */

        Task<string?>
            GetLastCodeAsync(
                string prefix);

        #endregion


        #region Persistence

        Task AddAsync(
            DeliveryChallan deliveryChallan);


        Task UpdateAsync(
            DeliveryChallan deliveryChallan);

        #endregion


        #region Deleted Challans

        Task<List<DeliveryChallan>>
            GetDeletedAsync();


        Task<DeliveryChallan?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion

        #region Master Snapshot Sources

        /*
         * Loads the current active Customer Master record
         * used for Delivery Challan snapshot creation.
         */
        Task<Customer?>
            GetCustomerForDispatchAsync(
                int customerId);


        /*
         * Loads the current active Company / Workshop Master
         * used for Delivery Challan snapshot creation.
         *
         * Current ERP assumption:
         * Ajay Industries has one active Company record used
         * as the dispatching workshop/company.
         */
        Task<Company?>
            GetCompanyForDispatchAsync();

        #endregion
    }
}