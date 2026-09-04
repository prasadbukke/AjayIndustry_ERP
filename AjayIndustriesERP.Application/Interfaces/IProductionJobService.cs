/*
============================================================
File: IProductionJobService.cs

Purpose:
Defines Production Job business operations.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Retrieve and search Production Jobs.
- Retrieve confirmed Customer Purchase Orders.
- Retrieve selected Customer PO with all active Items.
- Create one Production Job for one Customer PO.
- Copy each Item's Released Routing into its own Pipeline.
- Maintain Admin Production Quantity planning Item-wise.
- Edit Item-wise Pipeline before Production starts.
- Mark Draft Production Job as Ready.
- Execute Production Steps.
- Soft-delete / restore Production Jobs.

Important:
- One Customer PO has one Production Job.
- All Customer PO Items belong under that Production Job.
- Ordered Quantity comes from Customer PO.
- Production Quantity is planned by Admin.
- Completed Quantity is cumulative actual Production output.
- Worker does not change Production Quantity from Pipeline.
- Production Job becomes Completed only when all Items
  complete their full Customer PO Ordered Quantity.
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

        /// <summary>
        /// Returns confirmed Customer Purchase Orders
        /// available for Production Job creation.
        ///
        /// Customer POs already linked to a Production Job
        /// must not be available for creating another Job.
        /// </summary>
        Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForProductionAsync();


        /// <summary>
        /// Returns one confirmed Customer PO with all
        /// active Customer PO Items.
        /// </summary>
        Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForProductionAsync(
                int customerPurchaseOrderId);


        /// <summary>
        /// Returns the current Released Routing for an Item.
        /// </summary>
        Task<ItemProcessRouting?>
            GetReleasedRoutingForItemAsync(
                int itemId);

        #endregion


        #region Edit Production Planning

        /// <summary>
        /// Updates Draft Production Job planning.
        ///
        /// Production Quantity is updated Item-wise through
        /// ProductionJob.Items.
        ///
        /// Production Quantity rules:
        ///
        /// CompletedQuantity
        ///     <=
        /// ProductionQuantity
        ///     <=
        /// OrderedQuantity
        /// </summary>
        Task<ProductionJob> UpdateAsync(
            ProductionJob productionJob);

        #endregion


        #region Draft Pipeline Editing

        /// <summary>
        /// Returns active Production Operations that can be
        /// added to an Item Production Pipeline.
        /// </summary>
        Task<List<ProductionOperation>>
            GetProductionOperationsForPipelineAsync();


        /// <summary>
        /// Updates one Production Job Item Pipeline.
        ///
        /// Pipeline modification is allowed only before
        /// Production starts for that Item.
        ///
        /// The supplied Steps represent the final desired
        /// Pipeline for the selected ProductionJobItem.
        ///
        /// This does not modify Item Process Routing Master.
        /// </summary>
        Task UpdateDraftPipelineAsync(
            int productionJobId,
            int productionJobItemId,
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

        /// <summary>
        /// Creates one Production Job for one Customer PO.
        ///
        /// ProductionJob.Items contains all Customer PO Items
        /// and their Admin planned Production Quantities.
        /// </summary>
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


        /// <summary>
        /// Starts one Production Step.
        ///
        /// Production Quantity is already planned by Admin.
        /// Worker only selects / confirms Machine and starts
        /// the Step.
        /// </summary>
        Task StartStepAsync(
            int productionJobId,
            int productionJobStepId,
            int? assignedMachineId,
            string? remarks);


        /// <summary>
        /// Completes one Production Step and records
        /// Good / Rejected Quantity.
        ///
        /// Quantity is cumulative against the Item's current
        /// planned Production Quantity.
        /// </summary>
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