/*
============================================================
File: IProductionJobService.cs

Purpose:
Defines Production Job business operations.

Responsibilities:
- Retrieve and search Production Jobs.
- Retrieve confirmed Customer PO Items.
- Calculate remaining production quantity.
- Create Production Job from Customer PO Item.
- Copy Released Routing into executable Job Steps.
- Mark Draft Job as Ready.
- Soft-delete Draft Job.

Important:
- Production Job creation must use a Released Routing.
- Total active Production Job Quantity cannot exceed
  Customer PO Ordered Quantity.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IProductionJobService
    {
        #region Read Operations

        Task<ProductionJob?> GetByIdAsync(
            int id);

        Task<PagedResult<ProductionJob>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Customer PO Source

        Task<List<CustomerPurchaseOrderItem>>
            GetCustomerPurchaseOrderItemsForProductionAsync();

        Task<decimal> GetRemainingQuantityAsync(
            int customerPurchaseOrderItemId);

        Task<ItemProcessRouting?>
            GetReleasedRoutingForItemAsync(
                int itemId);

        #endregion


        #region Edit

        Task<ProductionJob> UpdateAsync(
            ProductionJob productionJob);

        #endregion

        #region Draft Pipeline Editing

        /// <summary>
        /// Returns active Production Operations that can be
        /// added to a Draft Production Job Pipeline.
        /// </summary>
        Task<List<ProductionOperation>>
            GetProductionOperationsForPipelineAsync();


        /// <summary>
        /// Updates the executable Pipeline of a Draft Production Job.
        ///
        /// Allowed only while Production Job Status is Draft.
        ///
        /// The supplied Steps represent the final desired Pipeline.
        /// Sequence numbers are normalized as 1, 2, 3, 4...
        ///
        /// This does not modify the Item Process Routing Master.
        /// </summary>
        Task UpdateDraftPipelineAsync(
            int productionJobId,
            List<ProductionJobStep> steps,
            string? modificationReason);

        #endregion

        #region Deleted Jobs

        Task<List<ProductionJob>>
            GetDeletedAsync();

        Task RestoreAsync(
            int id);

        #endregion

        #region Production Job Workflow

        Task<ProductionJob> CreateAsync(
            ProductionJob productionJob);

        Task MarkReadyAsync(
            int id);

        Task DeleteAsync(
            int id);

        #endregion

        #region Production Execution

        Task<List<Machine>>
            GetMachinesForExecutionAsync();

        Task StartStepAsync(
            int productionJobId,
            int productionJobStepId,
            int? assignedMachineId,
            string? remarks);

        Task CompleteStepAsync(
            int productionJobId,
            int productionJobStepId,
            decimal goodQuantity,
            decimal rejectedQuantity,
            string? remarks);

        Task CancelAsync(
            int productionJobId,
            string reason);

        #endregion
    }
}