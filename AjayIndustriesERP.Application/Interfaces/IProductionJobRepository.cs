/*
============================================================
File: IProductionJobRepository.cs

Purpose:
Defines database operations required by Production Job.

Responsibilities:
- Retrieve Production Jobs.
- Search and paginate Production Jobs.
- Retrieve confirmed Customer PO Items for production.
- Retrieve current Released Item Routing.
- Calculate already allocated Production Job Quantity.
- Retrieve the last Production Job Code.
- Persist Production Job changes.

Important:
- Business rules belong in ProductionJobService.
- Database access belongs only in Repository.
- One Customer PO Item may create multiple Production Jobs.
- Cancelled and deleted Jobs do not consume PO allocation.
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

        #endregion


        #region Search And Pagination

        Task<PagedResult<ProductionJob>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<PagedResult<ProductionJob>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Customer PO Source

        Task<List<CustomerPurchaseOrderItem>>
            GetCustomerPurchaseOrderItemsForProductionAsync();

        Task<CustomerPurchaseOrderItem?>
            GetCustomerPurchaseOrderItemForProductionAsync(
                int customerPurchaseOrderItemId);

        Task<decimal>
    GetAllocatedJobQuantityAsync(
        int customerPurchaseOrderItemId,
        int? excludeProductionJobId = null);

        #endregion


        #region Routing

        Task<ItemProcessRouting?>
            GetReleasedRoutingForItemAsync(
                int itemId);

        #endregion

        #region Draft Pipeline Lookups

        /// <summary>
        /// Returns active Production Operations that can be
        /// selected while editing a Draft Production Job Pipeline.
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

        Task<string?> GetLastJobCodeAsync(
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