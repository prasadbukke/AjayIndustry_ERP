/*
============================================================
File: IProductionJobRepository.cs

Purpose:
Defines database operations required by Production Job.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Retrieve Production Jobs.
- Search and paginate Production Jobs.
- Retrieve confirmed Customer Purchase Orders for Production.
- Retrieve one Customer PO with all active PO Items.
- Check whether a Customer PO already has a Production Job.
- Retrieve current Released Item Routing.
- Retrieve Production Operations and Machines.
- Retrieve the last Production Job Code.
- Persist Production Job changes.

Important:
- Business rules belong in ProductionJobService.
- Database access belongs only in Repository.
- One Customer PO has one Production Job.
- Production Quantity planning belongs to ProductionJobItem.
- Old multiple Production Job quantity allocation is removed.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IProductionJobRepository
    {
        #region Read Operations

        Task<ProductionJob?> GetByIdAsync(
            int id);


        Task<ProductionJob?> GetForUpdateAsync(
            int id);


        /// <summary>
        /// Returns the existing active Production Job
        /// for the selected Customer PO.
        ///
        /// Used to enforce:
        /// One Customer PO = One Production Job.
        /// </summary>
        Task<ProductionJob?>
            GetByCustomerPurchaseOrderIdAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Search And Pagination

        Task<PagedResult<ProductionJob>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<ProductionJob>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Customer PO Source

        /// <summary>
        /// Returns confirmed Customer Purchase Orders
        /// available as Production sources.
        /// </summary>
        Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForProductionAsync();


        /// <summary>
        /// Returns one confirmed Customer PO with all
        /// active Customer PO Items and Item information.
        /// </summary>
        Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForProductionAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Routing

        Task<ItemProcessRouting?>
            GetReleasedRoutingForItemAsync(
                int itemId);

        #endregion


        #region Draft Pipeline Lookups

        /// <summary>
        /// Returns active Production Operations that can be
        /// selected while editing a Production Item Pipeline.
        /// </summary>
        Task<List<ProductionOperation>>
            GetProductionOperationsForPipelineAsync();

        #endregion


        #region Production Execution Lookups

        Task<List<Machine>>
            GetMachinesForExecutionAsync();


        Task<Machine?>
            GetMachineForExecutionAsync(
                int machineId);

        #endregion


        #region Job Code

        Task<string?>
            GetLastJobCodeAsync(
                string prefix);

        #endregion


        #region Deleted Jobs

        Task<List<ProductionJob>>
            GetDeletedAsync();


        Task<ProductionJob?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion


        #region Write Operations

        Task AddAsync(
            ProductionJob productionJob);


        Task UpdateAsync(
            ProductionJob productionJob);

        #endregion
    }
}