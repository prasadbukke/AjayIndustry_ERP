/*
============================================================
File: ProductionJobService.cs

Purpose:
Implements Production Job planning and execution.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step
        ↓
Production Job Step History

Core Rules:

1. One Customer PO has one Production Job.

2. Every active Customer PO Item is created under
   the same Production Job.

3. OrderedQuantity comes from Customer PO.

4. ProductionQuantity is planned by Admin.

5. Worker cannot change ProductionQuantity.

6. CompletedQuantity is cumulative final Step GOOD output.

7. Partial Production is supported.

Example:

OrderedQuantity      = 100
ProductionQuantity   = 50
CompletedQuantity    = 50

Current 50 production is complete,
but the full Item is NOT complete.

Later Admin may increase:

ProductionQuantity = 100

The same Item Pipeline is reopened for the
remaining Production Quantity.

8. Parent Production Job becomes Completed only when
   all active ProductionJobItems complete their full
   OrderedQuantity.

9. Different ProductionJobItems have independent Pipelines.

10. Routing changes after Production Job creation do not
    modify copied Production Steps.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class ProductionJobService
        : IProductionJobService
    {
        #region Fields

        private readonly IProductionJobRepository
            _repository;

        #endregion


        #region Constructor

        public ProductionJobService(
            IProductionJobRepository repository)
        {
            _repository =
                repository;
        }

        #endregion


        #region Read Operations

        public async Task<ProductionJob?>
            GetByIdAsync(
                int id)
        {
            if (id <= 0)
            {
                return null;
            }


            return await _repository
                .GetByIdAsync(
                    id);
        }


        public async Task<PagedResult<ProductionJob>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize)
        {
            NormalizePagination(
                ref pageNumber,
                ref pageSize);


            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _repository
                    .GetPagedAsync(
                        pageNumber,
                        pageSize);
            }


            return await _repository
                .SearchPagedAsync(
                    searchText.Trim(),
                    pageNumber,
                    pageSize);
        }

        #endregion


        #region Customer PO Source

        public async Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForProductionAsync()
        {
            return await _repository
                .GetCustomerPurchaseOrdersForProductionAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForProductionAsync(
                int customerPurchaseOrderId)
        {
            if (customerPurchaseOrderId <= 0)
            {
                return null;
            }


            return await _repository
                .GetCustomerPurchaseOrderForProductionAsync(
                    customerPurchaseOrderId);
        }


        public async Task<ItemProcessRouting?>
            GetReleasedRoutingForItemAsync(
                int itemId)
        {
            if (itemId <= 0)
            {
                return null;
            }


            return await _repository
                .GetReleasedRoutingForItemAsync(
                    itemId);
        }

        #endregion


        #region Pipeline Lookups

        public async Task<List<ProductionOperation>>
            GetProductionOperationsForPipelineAsync()
        {
            return await _repository
                .GetProductionOperationsForPipelineAsync();
        }

        #endregion


        #region Production Execution Lookups

        public async Task<List<Machine>>
            GetMachinesForExecutionAsync()
        {
            return await _repository
                .GetMachinesForExecutionAsync();
        }

        #endregion


        #region Create Production Job

        public async Task<ProductionJob>
            CreateAsync(
                ProductionJob productionJob)
        {
            #region Basic Validation

            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job information is required.");
            }


            if (productionJob.CustomerPurchaseOrderId <= 0)
            {
                throw new BusinessException(
                    "Customer Purchase Order is required.");
            }

            #endregion


            #region Customer PO

            var customerPurchaseOrder =
                await _repository
                    .GetCustomerPurchaseOrderForProductionAsync(
                        productionJob.CustomerPurchaseOrderId);


            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order is not available for Production.");
            }


            var existingJob =
                await _repository
                    .GetByCustomerPurchaseOrderIdAsync(
                        customerPurchaseOrder.Id);


            if (existingJob != null)
            {
                throw new BusinessException(
                    $"Production Job {existingJob.Code} already exists for this Customer PO.");
            }


            var customerPoItems =
                customerPurchaseOrder
                    .Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.Id)
                    .ToList();


            if (!customerPoItems.Any())
            {
                throw new BusinessException(
                    "Selected Customer PO does not contain any active Items.");
            }

            #endregion


            #region Capture Submitted Production Quantities

            /*
             * Controller submits Admin planned Production
             * Quantity Item-wise.
             *
             * Source Item / Ordered Qty / Routing are always
             * trusted from database, never from POST.
             */

            var submittedQuantityLookup =
                productionJob
                    .Items
                    .Where(x =>
                        x.CustomerPurchaseOrderItemId > 0)
                    .GroupBy(x =>
                        x.CustomerPurchaseOrderItemId)
                    .ToDictionary(
                        group =>
                            group.Key,
                        group =>
                            group.Last()
                                .ProductionQuantity);


            /*
             * Rebuild Items only from trusted PO source.
             */
            productionJob.Items.Clear();

            #endregion


            #region Production Job Header

            productionJob.Code =
                await GenerateJobCodeAsync();


            productionJob.CustomerPurchaseOrderId =
                customerPurchaseOrder.Id;


            productionJob.Status =
                ProductionJobStatus.Draft;


            productionJob.Remarks =
                NormalizeOptional(
                    productionJob.Remarks);


            ValidatePlanningDates(
                productionJob);


            productionJob.StartedOn =
                null;

            productionJob.CompletedOn =
                null;

            productionJob.CancelledOn =
                null;

            productionJob.CancellationReason =
                null;


            productionJob.IsActive =
                true;

            productionJob.IsDeleted =
                false;

            productionJob.CreatedOn =
                DateTime.UtcNow;

            productionJob.CreatedBy =
                "System";

            #endregion


            #region Create Production Job Items

            foreach (var customerPoItem
                in customerPoItems)
            {
                var productionQuantity =
                    submittedQuantityLookup
                        .TryGetValue(
                            customerPoItem.Id,
                            out var submittedQuantity)
                        ? submittedQuantity
                        : 0m;


                ValidateProductionQuantity(
                    orderedQuantity:
                        customerPoItem.OrderedQuantity,
                    productionQuantity:
                        productionQuantity,
                    completedQuantity:
                        0m,
                    itemDisplay:
                        $"{customerPoItem.ItemCode} - {customerPoItem.ItemName}");


                #region Released Routing

                var routing =
                    await _repository
                        .GetReleasedRoutingForItemAsync(
                            customerPoItem.ItemId);


                if (routing == null)
                {
                    throw new BusinessException(
                        $"No Released Item Process Routing exists for {customerPoItem.ItemCode} - {customerPoItem.ItemName}.");
                }


                var routingSteps =
                    routing
                        .Steps
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)
                        .OrderBy(x =>
                            x.SequenceNumber)
                        .ToList();


                if (!routingSteps.Any())
                {
                    throw new BusinessException(
                        $"Released Routing for {customerPoItem.ItemCode} - {customerPoItem.ItemName} does not contain any active Process Steps.");
                }

                #endregion


                #region Production Job Item

                var productionJobItem =
                    new ProductionJobItem
                    {
                        CustomerPurchaseOrderItemId =
                            customerPoItem.Id,

                        ItemId =
                            customerPoItem.ItemId,

                        ItemCode =
                            customerPoItem.ItemCode,

                        ItemName =
                            customerPoItem.ItemName,

                        UnitName =
                            NormalizeOptional(
                                customerPoItem.UnitName),

                        OrderedQuantity =
                            customerPoItem.OrderedQuantity,

                        ProductionQuantity =
                            productionQuantity,

                        CompletedQuantity =
                            0m,

                        ItemProcessRoutingId =
                            routing.Id,

                        RoutingCode =
                            routing.Code,

                        RoutingRevisionNumber =
                            routing.RevisionNumber,

                        PipelineModificationReason =
                            null,

                        IsActive =
                            true,

                        IsDeleted =
                            false,

                        CreatedOn =
                            DateTime.UtcNow,

                        CreatedBy =
                            "System"
                    };

                #endregion


                #region Copy Routing Steps

                foreach (var routingStep
                    in routingSteps)
                {
                    if (routingStep.ProductionOperation == null)
                    {
                        throw new BusinessException(
                            $"Operation information is missing for Routing Sequence {routingStep.SequenceNumber}.");
                    }


                    var productionStep =
                        new ProductionJobStep
                        {
                            SequenceNumber =
                                routingStep.SequenceNumber,

                            ProductionOperationId =
                                routingStep.ProductionOperationId,

                            OperationCode =
                                routingStep
                                    .ProductionOperation
                                    .Code,

                            OperationName =
                                routingStep
                                    .ProductionOperation
                                    .OperationName,

                            OperationType =
                                routingStep
                                    .ProductionOperation
                                    .OperationType,

                            DefaultMachineId =
                                routingStep.DefaultMachineId,

                            AssignedMachineId =
                                null,

                            SetupTimeMinutes =
                                routingStep.SetupTimeMinutes,

                            CycleTimeMinutes =
                                routingStep.CycleTimeMinutes,

                            OperationInstruction =
                                NormalizeOptional(
                                    routingStep.OperationInstruction),

                            RoutingRemarks =
                                NormalizeOptional(
                                    routingStep.Remarks),

                            Status =
                                ProductionJobStepStatus.Pending,

                            StartedOn =
                                null,

                            CompletedOn =
                                null,

                            GoodQuantity =
                                0m,

                            RejectedQuantity =
                                0m,

                            ExecutionRemarks =
                                null,

                            IsActive =
                                true,

                            IsDeleted =
                                false,

                            CreatedOn =
                                DateTime.UtcNow,

                            CreatedBy =
                                "System"
                        };


                    productionStep.History.Add(
                        new ProductionJobStepHistory
                        {
                            PreviousStatus =
                                null,

                            NewStatus =
                                ProductionJobStepStatus.Pending,

                            MachineId =
                                null,

                            MachineCode =
                                null,

                            MachineName =
                                null,

                            GoodQuantity =
                                0m,

                            RejectedQuantity =
                                0m,

                            Remarks =
                                "Production Job Step created from Released Routing.",

                            ChangedOn =
                                DateTime.UtcNow,

                            ChangedBy =
                                "System"
                        });


                    productionJobItem
                        .Steps
                        .Add(
                            productionStep);
                }

                #endregion


                productionJob
                    .Items
                    .Add(
                        productionJobItem);
            }

            #endregion


            await _repository
                .AddAsync(
                    productionJob);


            return productionJob;
        }

        #endregion


        #region Update Production Planning

        public async Task<ProductionJob>
            UpdateAsync(
                ProductionJob productionJob)
        {
            #region Basic Validation

            if (
                productionJob == null ||
                productionJob.Id <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }

            #endregion


            #region Load Existing Job

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        productionJob.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            if (
                existing.Status ==
                    ProductionJobStatus.Completed
                ||
                existing.Status ==
                    ProductionJobStatus.Cancelled
            )
            {
                throw new BusinessException(
                    "Completed or Cancelled Production Job cannot be edited.");
            }

            #endregion


            #region Validate Planning

            ValidatePlanningDates(
                productionJob);

            #endregion


            #region Update Item Production Quantities

            foreach (var submittedItem
                in productionJob.Items)
            {
                ProductionJobItem?
                    existingItem =
                        null;


                if (submittedItem.Id > 0)
                {
                    existingItem =
                        existing.Items
                            .FirstOrDefault(x =>
                                x.Id ==
                                    submittedItem.Id
                                &&
                                !x.IsDeleted
                                &&
                                x.IsActive);
                }
                else if (
                    submittedItem
                        .CustomerPurchaseOrderItemId > 0
                )
                {
                    existingItem =
                        existing.Items
                            .FirstOrDefault(x =>
                                x.CustomerPurchaseOrderItemId ==
                                    submittedItem
                                        .CustomerPurchaseOrderItemId
                                &&
                                !x.IsDeleted
                                &&
                                x.IsActive);
                }


                if (existingItem == null)
                {
                    throw new BusinessException(
                        "Invalid Production Job Item.");
                }


                var newProductionQuantity =
                    submittedItem
                        .ProductionQuantity;


                ValidateProductionQuantity(
                    existingItem.OrderedQuantity,
                    newProductionQuantity,
                    existingItem.CompletedQuantity,
                    $"{existingItem.ItemCode} - {existingItem.ItemName}");


                /*
                 * Once Job leaves Draft,
                 * Production Quantity can only increase.
                 *
                 * This avoids reducing an already released
                 * shop-floor target.
                 */
                if (
                    existing.Status !=
                        ProductionJobStatus.Draft
                    &&
                    newProductionQuantity <
                        existingItem.ProductionQuantity
                )
                {
                    throw new BusinessException(
                        $"Production Quantity for {existingItem.ItemCode} cannot be reduced after the Job is released.");
                }


                var quantityIncreased =
                    newProductionQuantity >
                    existingItem.ProductionQuantity;


                if (
                    quantityIncreased
                    &&
                    existing.Status ==
                        ProductionJobStatus.InProgress
                )
                {
                    var activeSteps =
                        GetActiveSteps(
                            existingItem);


                    if (
                        activeSteps.Any(x =>
                            x.Status ==
                                ProductionJobStepStatus.InProgress)
                    )
                    {
                        throw new BusinessException(
                            $"Production Quantity for {existingItem.ItemCode} cannot be increased while an Operation is In Progress.");
                    }


                    /*
                     * New Production Quantity is released only
                     * after the previous planned quantity cycle
                     * has completed.
                     */
                    if (
                        !existingItem
                            .IsCurrentProductionCompleted
                    )
                    {
                        throw new BusinessException(
                            $"Complete the current planned Production Quantity for {existingItem.ItemCode} before increasing it.");
                    }


                    if (
                        activeSteps.Any()
                        &&
                        !activeSteps.All(x =>
                            x.Status ==
                                ProductionJobStepStatus.Completed)
                    )
                    {
                        throw new BusinessException(
                            $"Complete the current Production Pipeline for {existingItem.ItemCode} before increasing Production Quantity.");
                    }
                }


                existingItem.ProductionQuantity =
                    newProductionQuantity;


                /*
                 * When Admin increases Production Quantity after
                 * a completed production cycle, reopen the same
                 * Pipeline for the next quantity.
                 */
                if (
                    quantityIncreased
                    &&
                    existing.Status ==
                        ProductionJobStatus.InProgress
                )
                {
                    ReopenItemPipeline(
                        existingItem,
                        "Production Quantity increased by Admin. Pipeline reopened for remaining Production.");
                }


                existingItem.ModifiedOn =
                    DateTime.UtcNow;

                existingItem.ModifiedBy =
                    "System";
            }

            #endregion


            #region Update Job

            existing.PlannedStartOn =
                productionJob.PlannedStartOn;


            existing.PlannedCompletionOn =
                productionJob.PlannedCompletionOn;


            existing.Remarks =
                NormalizeOptional(
                    productionJob.Remarks);


            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    existing);


            return existing;
        }

        #endregion


        #region Draft Pipeline Editing

        public async Task UpdateDraftPipelineAsync(
            int productionJobId,
            int productionJobItemId,
            List<ProductionJobStep> steps,
            string? modificationReason)
        {
            #region Load Job

            var productionJob =
                await _repository
                    .GetForUpdateAsync(
                        productionJobId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }

            #endregion


            #region Find Production Item

            var productionJobItem =
                productionJob
                    .Items
                    .FirstOrDefault(x =>
                        x.Id ==
                            productionJobItemId
                        &&
                        !x.IsDeleted
                        &&
                        x.IsActive);


            if (productionJobItem == null)
            {
                throw new BusinessException(
                    "Production Job Item not found.");
            }

            #endregion


            #region Validate Pipeline Editing

            var activeExistingSteps =
                GetActiveSteps(
                    productionJobItem);


            var productionStarted =
                activeExistingSteps
                    .Any(step =>
                        step.StartedOn.HasValue
                        ||
                        step.History.Any(history =>
                            history.NewStatus ==
                                ProductionJobStepStatus.InProgress
                            ||
                            history.NewStatus ==
                                ProductionJobStepStatus.Completed));


            /*
             * Important:
             *
             * Parent Production Job may already be InProgress
             * because another Item under the same Job has started.
             *
             * That must NOT lock this Item's Pipeline.
             *
             * Pipeline becomes locked only after Production starts
             * for this specific ProductionJobItem.
             */
            var canEditPipeline =
                (
                    productionJob.Status ==
                        ProductionJobStatus.Draft
                    ||
                    productionJob.Status ==
                        ProductionJobStatus.Ready
                    ||
                    productionJob.Status ==
                        ProductionJobStatus.InProgress
                )
                &&
                !productionStarted;


            if (!canEditPipeline)
            {
                throw new BusinessException(
                    "Production Pipeline can be edited only before Production starts for this Item.");
            }

            #endregion


            #region Modification Reason

            modificationReason =
                NormalizeOptional(
                    modificationReason);


            if (modificationReason?.Length > 1000)
            {
                throw new BusinessException(
                    "Pipeline Modification Reason cannot exceed 1000 characters.");
            }

            #endregion


            #region Submitted Pipeline Validation

            if (
                steps == null
                ||
                steps.Count == 0
            )
            {
                throw new BusinessException(
                    "Production Pipeline must contain at least one Operation.");
            }


            var operations =
                await _repository
                    .GetProductionOperationsForPipelineAsync();


            var operationLookup =
                operations
                    .ToDictionary(x =>
                        x.Id);


            var submittedExistingIds =
                steps
                    .Where(x =>
                        x.Id > 0)
                    .Select(x =>
                        x.Id)
                    .ToList();


            if (
                submittedExistingIds.Count
                !=
                submittedExistingIds
                    .Distinct()
                    .Count()
            )
            {
                throw new BusinessException(
                    "Duplicate Production Pipeline Step found.");
            }


            foreach (var submittedStep
                in steps.Where(x =>
                    x.Id > 0))
            {
                var existingStep =
                    activeExistingSteps
                        .FirstOrDefault(x =>
                            x.Id ==
                                submittedStep.Id);


                if (existingStep == null)
                {
                    throw new BusinessException(
                        "Invalid Production Pipeline Step.");
                }


                if (
                    existingStep.Status !=
                        ProductionJobStepStatus.Pending
                )
                {
                    throw new BusinessException(
                        "Only Pending Production Steps can be modified.");
                }


                if (
                    existingStep.ProductionOperationId
                    !=
                    submittedStep.ProductionOperationId
                )
                {
                    throw new BusinessException(
                        "Existing Production Operation cannot be changed directly. Remove it and add the required Operation.");
                }
            }


            foreach (var submittedStep
                in steps.Where(x =>
                    x.Id <= 0))
            {
                if (
                    !operationLookup.ContainsKey(
                        submittedStep
                            .ProductionOperationId)
                )
                {
                    throw new BusinessException(
                        "Selected Production Operation is invalid or inactive.");
                }
            }

            #endregion


            #region Temporary Sequence Reset

            foreach (var existingStep
                in activeExistingSteps
                    .Where(x =>
                        x.Id > 0))
            {
                existingStep.SequenceNumber =
                    -existingStep.Id;


                existingStep.ModifiedOn =
                    DateTime.UtcNow;

                existingStep.ModifiedBy =
                    "System";
            }


            await _repository
                .UpdateAsync(
                    productionJob);

            #endregion


            #region Remove Operations

            foreach (var existingStep
                in activeExistingSteps)
            {
                if (
                    !submittedExistingIds.Contains(
                        existingStep.Id)
                )
                {
                    existingStep.IsDeleted =
                        true;

                    existingStep.IsActive =
                        false;

                    existingStep.ModifiedOn =
                        DateTime.UtcNow;

                    existingStep.ModifiedBy =
                        "System";
                }
            }

            #endregion


            #region Add And Reorder Operations

            var sequenceNumber =
                1;


            foreach (var submittedStep
                in steps)
            {
                if (submittedStep.Id > 0)
                {
                    var existingStep =
                        activeExistingSteps
                            .First(x =>
                                x.Id ==
                                    submittedStep.Id);


                    existingStep.SequenceNumber =
                        sequenceNumber;

                    existingStep.IsDeleted =
                        false;

                    existingStep.IsActive =
                        true;

                    existingStep.ModifiedOn =
                        DateTime.UtcNow;

                    existingStep.ModifiedBy =
                        "System";
                }
                else
                {
                    var operation =
                        operationLookup[
                            submittedStep
                                .ProductionOperationId];


                    var newStep =
                        new ProductionJobStep
                        {
                            ProductionJobItemId =
                                productionJobItem.Id,

                            SequenceNumber =
                                sequenceNumber,

                            ProductionOperationId =
                                operation.Id,

                            OperationCode =
                                operation.Code,

                            OperationName =
                                operation.OperationName,

                            OperationType =
                                operation.OperationType,

                            DefaultMachineId =
                                null,

                            AssignedMachineId =
                                null,

                            SetupTimeMinutes =
                                null,

                            CycleTimeMinutes =
                                null,

                            OperationInstruction =
                                null,

                            RoutingRemarks =
                                null,

                            Status =
                                ProductionJobStepStatus.Pending,

                            StartedOn =
                                null,

                            CompletedOn =
                                null,

                            GoodQuantity =
                                0m,

                            RejectedQuantity =
                                0m,

                            ExecutionRemarks =
                                null,

                            IsActive =
                                true,

                            IsDeleted =
                                false,

                            CreatedOn =
                                DateTime.UtcNow,

                            CreatedBy =
                                "System"
                        };


                    newStep.History.Add(
                        new ProductionJobStepHistory
                        {
                            PreviousStatus =
                                null,

                            NewStatus =
                                ProductionJobStepStatus.Pending,

                            MachineId =
                                null,

                            MachineCode =
                                null,

                            MachineName =
                                null,

                            GoodQuantity =
                                0m,

                            RejectedQuantity =
                                0m,

                            Remarks =
                                "Production Job Step added during Pipeline modification.",

                            ChangedOn =
                                DateTime.UtcNow,

                            ChangedBy =
                                "System"
                        });


                    productionJobItem
                        .Steps
                        .Add(
                            newStep);
                }


                sequenceNumber++;
            }

            #endregion


            #region Update Item

            productionJobItem
                .PipelineModificationReason =
                    modificationReason;


            productionJobItem.ModifiedOn =
                DateTime.UtcNow;

            productionJobItem.ModifiedBy =
                "System";


            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Mark Job Ready

        public async Task MarkReadyAsync(
            int id)
        {
            var productionJob =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            if (
                productionJob.Status !=
                    ProductionJobStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft Production Job can be marked Ready.");
            }


            var activeItems =
                GetActiveItems(
                    productionJob);


            if (!activeItems.Any())
            {
                throw new BusinessException(
                    "Production Job does not contain any Production Items.");
            }


            var plannedItems =
                activeItems
                    .Where(x =>
                        x.ProductionQuantity >
                            x.CompletedQuantity)
                    .ToList();


            if (!plannedItems.Any())
            {
                throw new BusinessException(
                    "Enter Production Quantity for at least one Item before marking the Job Ready.");
            }


            foreach (var item
                in plannedItems)
            {
                var activeSteps =
                    GetActiveSteps(
                        item);


                if (!activeSteps.Any())
                {
                    throw new BusinessException(
                        $"Production Pipeline is missing for {item.ItemCode} - {item.ItemName}.");
                }
            }


            productionJob.Status =
                ProductionJobStatus.Ready;


            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Start Production Step

        public async Task StartStepAsync(
            int productionJobId,
            int productionJobStepId,
            int? assignedMachineId,
            string? remarks)
        {
            #region Load Job

            var productionJob =
                await _repository
                    .GetForUpdateAsync(
                        productionJobId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }

            #endregion


            #region Validate Job Status

            if (
                productionJob.Status !=
                    ProductionJobStatus.Ready
                &&
                productionJob.Status !=
                    ProductionJobStatus.InProgress
            )
            {
                throw new BusinessException(
                    "Only Ready or In Progress Production Job can start a Step.");
            }

            #endregion


            #region Find Item And Step

            var productionJobItem =
                productionJob
                    .Items
                    .FirstOrDefault(item =>
                        !item.IsDeleted
                        &&
                        item.IsActive
                        &&
                        item.Steps.Any(step =>
                            step.Id ==
                                productionJobStepId
                            &&
                            !step.IsDeleted
                            &&
                            step.IsActive));


            if (productionJobItem == null)
            {
                throw new BusinessException(
                    "Production Job Item not found.");
            }


            var step =
                productionJobItem
                    .Steps
                    .FirstOrDefault(x =>
                        x.Id ==
                            productionJobStepId
                        &&
                        !x.IsDeleted
                        &&
                        x.IsActive);


            if (step == null)
            {
                throw new BusinessException(
                    "Production Job Step not found.");
            }

            #endregion


            #region Validate Production Quantity

            if (
                productionJobItem
                    .ProductionQuantity <= 0m
            )
            {
                throw new BusinessException(
                    $"Production Quantity is not planned for {productionJobItem.ItemCode}.");
            }


            if (
                productionJobItem
                    .IsCurrentProductionCompleted
            )
            {
                throw new BusinessException(
                    $"Current planned Production Quantity for {productionJobItem.ItemCode} is already completed.");
            }

            #endregion


            #region Validate Step Status

            if (
                step.Status !=
                    ProductionJobStepStatus.Pending
            )
            {
                throw new BusinessException(
                    "Only Pending Production Step can be started.");
            }

            #endregion


            #region Validate Running Step Inside Same Item

            var anotherRunningStep =
                productionJobItem
                    .Steps
                    .Any(x =>
                        x.Id != step.Id
                        &&
                        !x.IsDeleted
                        &&
                        x.IsActive
                        &&
                        x.Status ==
                            ProductionJobStepStatus.InProgress);


            if (anotherRunningStep)
            {
                throw new BusinessException(
                    "Another Production Step is already In Progress for this Item. Complete it before starting the next Step.");
            }

            #endregion


            #region Validate Sequence

            var previousIncompleteStep =
                productionJobItem
                    .Steps
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive
                        &&
                        x.SequenceNumber <
                            step.SequenceNumber)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .FirstOrDefault(x =>
                        x.Status !=
                            ProductionJobStepStatus.Completed);


            if (previousIncompleteStep != null)
            {
                throw new BusinessException(
                    $"Complete previous Step '{previousIncompleteStep.OperationName}' before starting '{step.OperationName}'.");
            }

            #endregion


            #region Validate Machine

            Machine? assignedMachine =
                null;


            if (assignedMachineId.HasValue)
            {
                assignedMachine =
                    await _repository
                        .GetMachineForExecutionAsync(
                            assignedMachineId.Value);


                if (assignedMachine == null)
                {
                    throw new BusinessException(
                        "Selected Machine is not available.");
                }
            }

            #endregion


            #region Start Step

            var previousStatus =
                step.Status;


            step.AssignedMachineId =
                assignedMachineId;


            step.Status =
                ProductionJobStepStatus.InProgress;


            step.StartedOn =
                DateTime.UtcNow;


            step.CompletedOn =
                null;


            step.ExecutionRemarks =
                NormalizeOptional(
                    remarks);


            step.ModifiedOn =
                DateTime.UtcNow;

            step.ModifiedBy =
                "System";

            #endregion


            #region Update Parent Job

            if (!productionJob.StartedOn.HasValue)
            {
                productionJob.StartedOn =
                    DateTime.UtcNow;
            }


            productionJob.Status =
                ProductionJobStatus.InProgress;


            productionJob.CompletedOn =
                null;


            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            #region History

            step.History.Add(
                new ProductionJobStepHistory
                {
                    PreviousStatus =
                        previousStatus,

                    NewStatus =
                        ProductionJobStepStatus.InProgress,

                    MachineId =
                        assignedMachine?.Id,

                    MachineCode =
                        assignedMachine?.Code,

                    MachineName =
                        assignedMachine?.MachineName,

                    GoodQuantity =
                        step.GoodQuantity,

                    RejectedQuantity =
                        step.RejectedQuantity,

                    Remarks =
                        NormalizeOptional(
                            remarks),

                    ChangedOn =
                        DateTime.UtcNow,

                    ChangedBy =
                        "System"
                });

            #endregion


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Complete Production Step

        public async Task CompleteStepAsync(
            int productionJobId,
            int productionJobStepId,
            decimal goodQuantity,
            decimal rejectedQuantity,
            string? remarks)
        {
            #region Load Job

            var productionJob =
                await _repository
                    .GetForUpdateAsync(
                        productionJobId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            if (
                productionJob.Status !=
                    ProductionJobStatus.InProgress
            )
            {
                throw new BusinessException(
                    "Production Job is not currently In Progress.");
            }

            #endregion


            #region Find Item And Step

            var productionJobItem =
                productionJob
                    .Items
                    .FirstOrDefault(item =>
                        !item.IsDeleted
                        &&
                        item.IsActive
                        &&
                        item.Steps.Any(step =>
                            step.Id ==
                                productionJobStepId
                            &&
                            !step.IsDeleted
                            &&
                            step.IsActive));


            if (productionJobItem == null)
            {
                throw new BusinessException(
                    "Production Job Item not found.");
            }


            var activeSteps =
                GetActiveSteps(
                    productionJobItem);


            var step =
                activeSteps
                    .FirstOrDefault(x =>
                        x.Id ==
                            productionJobStepId);


            if (step == null)
            {
                throw new BusinessException(
                    "Production Job Step not found.");
            }


            if (
                step.Status !=
                    ProductionJobStepStatus.InProgress
            )
            {
                throw new BusinessException(
                    "Only In Progress Production Step can be completed.");
            }

            #endregion


            #region Quantity Validation

            if (goodQuantity < 0m)
            {
                throw new BusinessException(
                    "Good Quantity cannot be negative.");
            }


            if (rejectedQuantity < 0m)
            {
                throw new BusinessException(
                    "Rejected Quantity cannot be negative.");
            }


            var currentGood =
                step.GoodQuantity
                ??
                0m;


            var currentRejected =
                step.RejectedQuantity
                ??
                0m;


            /*
             * Required GOOD quantity for a Step:
             *
             * Current Admin Production Quantity
             * +
             * cumulative downstream rejects.
             *
             * This allows upstream replacement Production
             * when a later Operation rejects parts.
             */
            var requiredGoodQuantity =
                GetRequiredGoodQuantity(
                    productionJobItem,
                    step,
                    activeSteps);


            var remainingGoodQuantity =
                requiredGoodQuantity -
                currentGood;


            if (remainingGoodQuantity < 0m)
            {
                remainingGoodQuantity =
                    0m;
            }


            if (
                goodQuantity >
                remainingGoodQuantity
            )
            {
                throw new BusinessException(
                    $"Good Quantity cannot exceed pending required quantity {remainingGoodQuantity:0.###}.");
            }


            /*
             * For every Step except the first,
             * processed input cannot exceed GOOD quantity
             * produced by the previous Step.
             */
            var previousStep =
                activeSteps
                    .Where(x =>
                        x.SequenceNumber <
                            step.SequenceNumber)
                    .OrderByDescending(x =>
                        x.SequenceNumber)
                    .FirstOrDefault();


            if (previousStep != null)
            {
                var previousGood =
                    previousStep.GoodQuantity
                    ??
                    0m;


                var alreadyProcessedHere =
                    currentGood +
                    currentRejected;


                var availableInput =
                    previousGood -
                    alreadyProcessedHere;


                if (availableInput < 0m)
                {
                    availableInput =
                        0m;
                }


                if (
                    goodQuantity +
                    rejectedQuantity >
                    availableInput
                )
                {
                    throw new BusinessException(
                        $"Good Quantity + Rejected Quantity cannot exceed available previous Step quantity {availableInput:0.###}.");
                }
            }

            #endregion


            #region Complete Step Entry

            var previousStatus =
                step.Status;


            /*
             * Quantities stored on Step are cumulative.
             *
             * Popup values represent THIS completion entry.
             */
            step.GoodQuantity =
                currentGood +
                goodQuantity;


            step.RejectedQuantity =
                currentRejected +
                rejectedQuantity;


            step.ExecutionRemarks =
                NormalizeOptional(
                    remarks);


            step.Status =
                ProductionJobStepStatus.Completed;


            step.CompletedOn =
                DateTime.UtcNow;


            step.ModifiedOn =
                DateTime.UtcNow;

            step.ModifiedBy =
                "System";

            #endregion


            #region Machine Snapshot

            Machine? assignedMachine =
                null;


            if (step.AssignedMachineId.HasValue)
            {
                assignedMachine =
                    await _repository
                        .GetMachineForExecutionAsync(
                            step.AssignedMachineId.Value);
            }

            #endregion


            #region History

            step.History.Add(
                new ProductionJobStepHistory
                {
                    PreviousStatus =
                        previousStatus,

                    NewStatus =
                        ProductionJobStepStatus.Completed,

                    MachineId =
                        assignedMachine?.Id,

                    MachineCode =
                        assignedMachine?.Code,

                    MachineName =
                        assignedMachine?.MachineName,

                    /*
                     * History stores cumulative Step snapshot.
                     */
                    GoodQuantity =
                        step.GoodQuantity,

                    RejectedQuantity =
                        step.RejectedQuantity,

                    Remarks =
                        NormalizeOptional(
                            remarks),

                    ChangedOn =
                        DateTime.UtcNow,

                    ChangedBy =
                        "System"
                });

            #endregion


            #region Item Pipeline Completion

            var allItemStepsCompleted =
                activeSteps
                    .All(x =>
                        x.Status ==
                            ProductionJobStepStatus.Completed);


            if (allItemStepsCompleted)
            {
                var finalStep =
                    activeSteps
                        .OrderByDescending(x =>
                            x.SequenceNumber)
                        .First();


                productionJobItem.CompletedQuantity =
                    Math.Min(
                        productionJobItem.OrderedQuantity,
                        finalStep.GoodQuantity
                        ??
                        0m);


                /*
                 * Example:
                 *
                 * Production Qty = 50
                 * Final Good     = 47
                 *
                 * Current Production plan is NOT complete.
                 *
                 * Reopen the same Pipeline automatically
                 * for replacement / remaining quantity.
                 */
                if (
                    productionJobItem.CompletedQuantity <
                    productionJobItem.ProductionQuantity
                )
                {
                    ReopenItemPipeline(
                        productionJobItem,
                        "Current planned Production Quantity is not yet achieved. Pipeline reopened for remaining quantity.");
                }
            }

            #endregion


            #region Parent Production Job Status

            var activeItems =
                GetActiveItems(
                    productionJob);


            var allItemsFullyProduced =
                activeItems.Any()
                &&
                activeItems.All(x =>
                    x.IsProductionCompleted);


            if (allItemsFullyProduced)
            {
                productionJob.Status =
                    ProductionJobStatus.Completed;


                productionJob.CompletedOn =
                    DateTime.UtcNow;
            }
            else
            {
                /*
                 * Important:
                 *
                 * 50 / 100 complete does NOT complete PJOB.
                 *
                 * Parent remains InProgress until every Item
                 * reaches full Ordered Quantity.
                 */
                productionJob.Status =
                    ProductionJobStatus.InProgress;


                productionJob.CompletedOn =
                    null;
            }


            productionJobItem.ModifiedOn =
                DateTime.UtcNow;

            productionJobItem.ModifiedBy =
                "System";


            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Cancel Production Job

        public async Task CancelAsync(
            int productionJobId,
            string reason)
        {
            #region Validate Reason

            var normalizedReason =
                NormalizeOptional(
                    reason);


            if (string.IsNullOrWhiteSpace(
                normalizedReason))
            {
                throw new BusinessException(
                    "Cancellation Reason is required.");
            }


            if (normalizedReason.Length > 1000)
            {
                throw new BusinessException(
                    "Cancellation Reason cannot exceed 1000 characters.");
            }

            #endregion


            #region Load Job

            var productionJob =
                await _repository
                    .GetForUpdateAsync(
                        productionJobId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            if (
                productionJob.Status !=
                    ProductionJobStatus.Ready
                &&
                productionJob.Status !=
                    ProductionJobStatus.InProgress
            )
            {
                throw new BusinessException(
                    "Only Ready or In Progress Production Job can be cancelled.");
            }

            #endregion


            #region Cancel Running Steps

            var runningSteps =
                productionJob
                    .Items
                    .Where(item =>
                        !item.IsDeleted &&
                        item.IsActive)
                    .SelectMany(item =>
                        item.Steps)
                    .Where(step =>
                        !step.IsDeleted
                        &&
                        step.IsActive
                        &&
                        step.Status ==
                            ProductionJobStepStatus.InProgress)
                    .ToList();


            foreach (var runningStep
                in runningSteps)
            {
                var previousStatus =
                    runningStep.Status;


                runningStep.Status =
                    ProductionJobStepStatus.Cancelled;


                runningStep.ExecutionRemarks =
                    normalizedReason;


                runningStep.ModifiedOn =
                    DateTime.UtcNow;

                runningStep.ModifiedBy =
                    "System";


                Machine? assignedMachine =
                    null;


                if (runningStep.AssignedMachineId.HasValue)
                {
                    assignedMachine =
                        await _repository
                            .GetMachineForExecutionAsync(
                                runningStep
                                    .AssignedMachineId
                                    .Value);
                }


                runningStep.History.Add(
                    new ProductionJobStepHistory
                    {
                        PreviousStatus =
                            previousStatus,

                        NewStatus =
                            ProductionJobStepStatus.Cancelled,

                        MachineId =
                            assignedMachine?.Id,

                        MachineCode =
                            assignedMachine?.Code,

                        MachineName =
                            assignedMachine?.MachineName,

                        GoodQuantity =
                            runningStep.GoodQuantity,

                        RejectedQuantity =
                            runningStep.RejectedQuantity,

                        Remarks =
                            normalizedReason,

                        ChangedOn =
                            DateTime.UtcNow,

                        ChangedBy =
                            "System"
                    });
            }

            #endregion


            #region Cancel Parent Job

            productionJob.Status =
                ProductionJobStatus.Cancelled;


            productionJob.CancelledOn =
                DateTime.UtcNow;


            productionJob.CancellationReason =
                normalizedReason;


            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Delete Production Job

        public async Task DeleteAsync(
            int id)
        {
            var productionJob =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            var canDelete =
                productionJob.Status ==
                    ProductionJobStatus.Draft
                ||
                productionJob.Status ==
                    ProductionJobStatus.Completed
                ||
                productionJob.Status ==
                    ProductionJobStatus.Cancelled;


            if (!canDelete)
            {
                throw new BusinessException(
                    "Ready or In Progress Production Job cannot be deleted. Cancel the Production Job first.");
            }


            productionJob.IsDeleted =
                true;

            productionJob.IsActive =
                false;

            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";


            foreach (var item
                in productionJob.Items)
            {
                item.IsDeleted =
                    true;

                item.IsActive =
                    false;

                item.ModifiedOn =
                    DateTime.UtcNow;

                item.ModifiedBy =
                    "System";


                foreach (var step
                    in item.Steps)
                {
                    step.IsDeleted =
                        true;

                    step.IsActive =
                        false;

                    step.ModifiedOn =
                        DateTime.UtcNow;

                    step.ModifiedBy =
                        "System";
                }
            }


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Deleted Jobs

        public async Task<List<ProductionJob>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }


        public async Task RestoreAsync(
            int id)
        {
            var productionJob =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Deleted Production Job not found.");
            }


            productionJob.IsDeleted =
                false;

            productionJob.IsActive =
                true;

            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";


            foreach (var item
                in productionJob.Items)
            {
                item.IsDeleted =
                    false;

                item.IsActive =
                    true;

                item.ModifiedOn =
                    DateTime.UtcNow;

                item.ModifiedBy =
                    "System";


                foreach (var step
                    in item.Steps)
                {
                    step.IsDeleted =
                        false;

                    step.IsActive =
                        true;

                    step.ModifiedOn =
                        DateTime.UtcNow;

                    step.ModifiedBy =
                        "System";
                }
            }


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Quantity Helpers

        private static void ValidateProductionQuantity(
            decimal orderedQuantity,
            decimal productionQuantity,
            decimal completedQuantity,
            string itemDisplay)
        {
            if (productionQuantity < 0m)
            {
                throw new BusinessException(
                    $"Production Quantity for {itemDisplay} cannot be negative.");
            }


            if (
                productionQuantity >
                orderedQuantity
            )
            {
                throw new BusinessException(
                    $"Production Quantity for {itemDisplay} cannot exceed Ordered Quantity {orderedQuantity:0.###}.");
            }


            if (
                productionQuantity <
                completedQuantity
            )
            {
                throw new BusinessException(
                    $"Production Quantity for {itemDisplay} cannot be less than Completed Quantity {completedQuantity:0.###}.");
            }
        }


        private static decimal
            GetRequiredGoodQuantity(
                ProductionJobItem productionJobItem,
                ProductionJobStep currentStep,
                List<ProductionJobStep> activeSteps)
        {
            var downstreamRejectedQuantity =
                activeSteps
                    .Where(step =>
                        step.SequenceNumber >
                            currentStep.SequenceNumber)
                    .Sum(step =>
                        step.RejectedQuantity
                        ??
                        0m);


            return
                productionJobItem.ProductionQuantity
                +
                downstreamRejectedQuantity;
        }

        #endregion


        #region Pipeline Reopen

        private static void ReopenItemPipeline(
            ProductionJobItem productionJobItem,
            string reason)
        {
            var activeSteps =
                GetActiveSteps(
                    productionJobItem);


            foreach (var step
                in activeSteps)
            {
                /*
                 * Reopen only completed cycle Steps.
                 *
                 * Good / Rejected quantities remain cumulative.
                 * Previous execution remains available in History.
                 */
                if (
                    step.Status !=
                        ProductionJobStepStatus.Completed
                )
                {
                    continue;
                }


                var previousStatus =
                    step.Status;


                step.Status =
                    ProductionJobStepStatus.Pending;


                step.StartedOn =
                    null;


                step.CompletedOn =
                    null;


                step.AssignedMachineId =
                    null;


                step.ExecutionRemarks =
                    null;


                step.ModifiedOn =
                    DateTime.UtcNow;

                step.ModifiedBy =
                    "System";


                step.History.Add(
                    new ProductionJobStepHistory
                    {
                        PreviousStatus =
                            previousStatus,

                        NewStatus =
                            ProductionJobStepStatus.Pending,

                        MachineId =
                            null,

                        MachineCode =
                            null,

                        MachineName =
                            null,

                        GoodQuantity =
                            step.GoodQuantity,

                        RejectedQuantity =
                            step.RejectedQuantity,

                        Remarks =
                            reason,

                        ChangedOn =
                            DateTime.UtcNow,

                        ChangedBy =
                            "System"
                    });
            }
        }

        #endregion


        #region Collection Helpers

        private static List<ProductionJobItem>
            GetActiveItems(
                ProductionJob productionJob)
        {
            return productionJob
                .Items
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.Id)
                .ToList();
        }


        private static List<ProductionJobStep>
            GetActiveSteps(
                ProductionJobItem productionJobItem)
        {
            return productionJobItem
                .Steps
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.SequenceNumber)
                .ToList();
        }

        #endregion


        #region Planning Validation

        private static void ValidatePlanningDates(
            ProductionJob productionJob)
        {
            if (
                productionJob.PlannedStartOn.HasValue
                &&
                productionJob.PlannedCompletionOn.HasValue
                &&
                productionJob.PlannedCompletionOn.Value
                <
                productionJob.PlannedStartOn.Value
            )
            {
                throw new BusinessException(
                    "Planned Completion cannot be before Planned Start.");
            }


            if (
                productionJob.Remarks?.Length >
                1000
            )
            {
                throw new BusinessException(
                    "Production Job Remarks cannot exceed 1000 characters.");
            }
        }

        #endregion


        #region Production Job Code

        private async Task<string>
            GenerateJobCodeAsync()
        {
            var today =
                DateTime.Today;


            var fiscalYear =
                GetFiscalYear(
                    today);


            var prefix =
                $"AI/PJOB/{fiscalYear}/";


            var lastCode =
                await _repository
                    .GetLastJobCodeAsync(
                        prefix);


            if (string.IsNullOrWhiteSpace(
                lastCode))
            {
                return
                    $"{prefix}00001";
            }


            var numberPart =
                lastCode.Substring(
                    prefix.Length);


            if (
                !int.TryParse(
                    numberPart,
                    out var lastNumber)
            )
            {
                throw new BusinessException(
                    "Unable to generate Production Job Code.");
            }


            return
                $"{prefix}{lastNumber + 1:00000}";
        }


        private static string GetFiscalYear(
            DateTime date)
        {
            var startYear =
                date.Month >= 4
                    ? date.Year
                    : date.Year - 1;


            var endYear =
                startYear + 1;


            return
                $"{startYear % 100:00}-{endYear % 100:00}";
        }

        #endregion


        #region Normalization

        private static string?
            NormalizeOptional(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }

        #endregion


        #region Pagination

        private static void NormalizePagination(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber =
                    1;
            }


            if (
                pageSize != 10
                &&
                pageSize != 25
                &&
                pageSize != 50
            )
            {
                pageSize =
                    10;
            }
        }

        #endregion
    }
}