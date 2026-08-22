/*
============================================================
File: ProductionJobService.cs

Purpose:
Implements Production Job creation and initial workflow.

Responsibilities:
- Validate Customer PO Item.
- Validate remaining production quantity.
- Find current Released Item Routing.
- Generate Production Job Code.
- Copy Routing Header snapshot.
- Copy Routing Steps into executable Production Job Steps.
- Create initial Pending Step History.
- Mark Draft Job as Ready.
- Soft-delete Draft Production Job.

Production Job Code:
AI/PJOB/{YY-YY}/{00001}

Example:
AI/PJOB/26-27/00001

Important:
- Production Job is an actual manufacturing transaction.
- Routing is only the reusable manufacturing template.
- Routing changes after Job creation must not modify Job Steps.
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
            _repository = repository;
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
                .GetByIdAsync(id);
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

        public async Task<List<CustomerPurchaseOrderItem>>
            GetCustomerPurchaseOrderItemsForProductionAsync()
        {
            return await _repository
                .GetCustomerPurchaseOrderItemsForProductionAsync();
        }


        public async Task<decimal>
            GetRemainingQuantityAsync(
                int customerPurchaseOrderItemId)
        {
            var customerPoItem =
                await _repository
                    .GetCustomerPurchaseOrderItemForProductionAsync(
                        customerPurchaseOrderItemId);


            if (customerPoItem == null)
            {
                throw new BusinessException(
                    "Customer PO Item is not available for Production.");
            }


            var allocatedQuantity =
                await _repository
                    .GetAllocatedJobQuantityAsync(
                        customerPurchaseOrderItemId);


            var remainingQuantity =
                customerPoItem.OrderedQuantity -
                allocatedQuantity;


            return remainingQuantity < 0
                ? 0
                : remainingQuantity;
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


        #region Create Production Job

        public async Task<ProductionJob>
            CreateAsync(
                ProductionJob productionJob)
        {
            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job information is required.");
            }


            if (productionJob.CustomerPurchaseOrderItemId <= 0)
            {
                throw new BusinessException(
                    "Customer PO Item is required.");
            }


            if (productionJob.JobQuantity <= 0)
            {
                throw new BusinessException(
                    "Production Job Quantity must be greater than zero.");
            }


            var customerPoItem =
                await _repository
                    .GetCustomerPurchaseOrderItemForProductionAsync(
                        productionJob.CustomerPurchaseOrderItemId);


            if (customerPoItem == null)
            {
                throw new BusinessException(
                    "Selected Customer PO Item is not available for Production.");
            }


            #region Quantity Validation

            var allocatedQuantity =
                await _repository
                    .GetAllocatedJobQuantityAsync(
                        customerPoItem.Id);


            var remainingQuantity =
                customerPoItem.OrderedQuantity -
                allocatedQuantity;


            if (remainingQuantity <= 0)
            {
                throw new BusinessException(
                    "The complete Customer PO Quantity is already allocated to Production Jobs.");
            }


            if (productionJob.JobQuantity >
                remainingQuantity)
            {
                throw new BusinessException(
                    $"Production Job Quantity cannot exceed remaining quantity {remainingQuantity:0.###}.");
            }

            #endregion


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
                routing.Steps
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (!routingSteps.Any())
            {
                throw new BusinessException(
                    "The Released Routing does not contain any active Process Steps.");
            }

            #endregion


            #region Header Preparation

            productionJob.Code =
                await GenerateJobCodeAsync();


            productionJob.ItemId =
                customerPoItem.ItemId;


            /*
             * Customer PO line snapshot is used here.
             * This preserves the Item information used by
             * the actual Customer Order.
             */

            productionJob.ItemCode =
                customerPoItem.ItemCode;

            productionJob.ItemName =
                customerPoItem.ItemName;

            productionJob.UnitName =
                NormalizeOptional(
                    customerPoItem.UnitName);


            productionJob.ItemProcessRoutingId =
                routing.Id;

            productionJob.RoutingCode =
                routing.Code;

            productionJob.RoutingRevisionNumber =
                routing.RevisionNumber;


            productionJob.Status =
                ProductionJobStatus.Draft;


            productionJob.Remarks =
                NormalizeOptional(
                    productionJob.Remarks);


            ValidatePlanningDates(
                productionJob);


            productionJob.IsActive =
                true;

            productionJob.IsDeleted =
                false;

            productionJob.CreatedOn =
                DateTime.UtcNow;

            productionJob.CreatedBy =
                "System";

            #endregion


            #region Copy Routing Steps

            productionJob.Steps.Clear();


            foreach (var routingStep
                in routingSteps)
            {
                if (routingStep.ProductionOperation == null)
                {
                    throw new BusinessException(
                        $"Operation information is missing for Routing Sequence {routingStep.SequenceNumber}.");
                }


                var jobStep =
                    new ProductionJobStep
                    {
                        SequenceNumber =
                            routingStep.SequenceNumber,

                        ProductionOperationId =
                            routingStep.ProductionOperationId,

                        OperationCode =
                            routingStep.ProductionOperation.Code,

                        OperationName =
                            routingStep.ProductionOperation.OperationName,

                        OperationType =
                            routingStep.ProductionOperation.OperationType,

                        DefaultMachineId =
                            routingStep.DefaultMachineId,

                        /*
                         * Actual Assigned Machine intentionally
                         * remains null at Job creation.
                         *
                         * During shop-floor execution user can
                         * select the Default Machine or another
                         * available Machine.
                         */

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

                        GoodQuantity =
                            null,

                        RejectedQuantity =
                            null,

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


                #region Initial Step History

                jobStep.History.Add(
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
                            null,

                        RejectedQuantity =
                            null,

                        Remarks =
                            "Production Job Step created from Released Routing.",

                        ChangedOn =
                            DateTime.UtcNow,

                        ChangedBy =
                            "System"
                    });

                #endregion


                productionJob.Steps.Add(
                    jobStep);
            }

            #endregion


            await _repository
                .AddAsync(
                    productionJob);


            return productionJob;
        }

        #endregion


        #region Mark Job Ready

        public async Task MarkReadyAsync(
            int id)
        {
            var productionJob =
                await _repository
                    .GetForUpdateAsync(id);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            if (productionJob.Status !=
                ProductionJobStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Production Job can be marked Ready.");
            }


            var activeSteps =
                productionJob.Steps
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .ToList();


            if (!activeSteps.Any())
            {
                throw new BusinessException(
                    "Production Job does not contain any Production Steps.");
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


        #region Delete Draft Job

        public async Task DeleteAsync(
     int id)
        {
            #region Load Production Job

            var productionJob =
                await _repository
                    .GetForUpdateAsync(id);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }

            #endregion


            #region Validate Delete Status

            var canDelete =
                productionJob.Status ==
                    ProductionJobStatus.Draft ||
                productionJob.Status ==
                    ProductionJobStatus.Completed ||
                productionJob.Status ==
                    ProductionJobStatus.Cancelled;


            if (!canDelete)
            {
                throw new BusinessException(
                    "Ready or In Progress Production Job cannot be deleted. Cancel the Production Job first.");
            }

            #endregion


            #region Soft Delete Production Job

            productionJob.IsDeleted =
                true;

            productionJob.IsActive =
                false;

            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            #region Soft Delete Production Steps

            foreach (var step in
                productionJob.Steps
                    .Where(x =>
                        !x.IsDeleted))
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

            #endregion


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion


        #region Planning Validation

        private static void ValidatePlanningDates(
            ProductionJob productionJob)
        {
            if (
                productionJob.PlannedStartOn.HasValue &&
                productionJob.PlannedCompletionOn.HasValue &&
                productionJob.PlannedCompletionOn.Value <
                productionJob.PlannedStartOn.Value
            )
            {
                throw new BusinessException(
                    "Planned Completion cannot be before Planned Start.");
            }


            if (productionJob.Remarks?.Length >
                1000)
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


            if (!int.TryParse(
                numberPart,
                out var lastNumber))
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

        #region Production Execution

        public async Task<List<Machine>>
            GetMachinesForExecutionAsync()
        {
            return await _repository
                .GetMachinesForExecutionAsync();
        }


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

            if (productionJob.Status !=
                    ProductionJobStatus.Ready &&
                productionJob.Status !=
                    ProductionJobStatus.InProgress)
            {
                throw new BusinessException(
                    "Only Ready or In Progress Production Job can start a Step.");
            }

            #endregion


            #region Find Step

            var step =
                productionJob.Steps
                    .FirstOrDefault(x =>
                        x.Id ==
                            productionJobStepId &&
                        !x.IsDeleted &&
                        x.IsActive);


            if (step == null)
            {
                throw new BusinessException(
                    "Production Job Step not found.");
            }


            if (step.Status !=
                ProductionJobStepStatus.Pending)
            {
                throw new BusinessException(
                    "Only Pending Production Step can be started.");
            }

            #endregion


            #region Validate Running Step

            var anotherRunningStep =
                productionJob.Steps
                    .Any(x =>
                        x.Id != step.Id &&
                        !x.IsDeleted &&
                        x.IsActive &&
                        x.Status ==
                            ProductionJobStepStatus.InProgress);


            if (anotherRunningStep)
            {
                throw new BusinessException(
                    "Another Production Step is already In Progress. Complete it before starting the next Step.");
            }

            #endregion


            #region Validate Sequence

            var previousIncompleteStep =
    productionJob.Steps
        .Where(x =>
            !x.IsDeleted &&
            x.IsActive &&
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

            step.ModifiedOn =
                DateTime.UtcNow;

            step.ModifiedBy =
                "System";

            #endregion


            #region Update Job

            if (!productionJob.StartedOn.HasValue)
            {
                productionJob.StartedOn =
                    DateTime.UtcNow;
            }


            productionJob.Status =
                ProductionJobStatus.InProgress;

            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            #region Add History

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

            #endregion


            #region Validate Job

            if (productionJob.Status !=
                ProductionJobStatus.InProgress)
            {
                throw new BusinessException(
                    "Production Job is not currently In Progress.");
            }

            #endregion


            #region Find Step

            var step =
                productionJob.Steps
                    .FirstOrDefault(x =>
                        x.Id ==
                            productionJobStepId &&
                        !x.IsDeleted &&
                        x.IsActive);


            if (step == null)
            {
                throw new BusinessException(
                    "Production Job Step not found.");
            }


            if (step.Status !=
                ProductionJobStepStatus.InProgress)
            {
                throw new BusinessException(
                    "Only In Progress Production Step can be completed.");
            }

            #endregion


            #region Quantity Validation

            if (goodQuantity < 0)
            {
                throw new BusinessException(
                    "Good Quantity cannot be negative.");
            }


            if (rejectedQuantity < 0)
            {
                throw new BusinessException(
                    "Rejected Quantity cannot be negative.");
            }


            if (goodQuantity +
                rejectedQuantity >
                productionJob.JobQuantity)
            {
                throw new BusinessException(
                    $"Good Quantity + Rejected Quantity cannot exceed Job Quantity {productionJob.JobQuantity:0.###}.");
            }

            #endregion


            #region Complete Step

            var previousStatus =
                step.Status;


            step.GoodQuantity =
                goodQuantity;

            step.RejectedQuantity =
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


            #region Add History

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

                    GoodQuantity =
                        goodQuantity,

                    RejectedQuantity =
                        rejectedQuantity,

                    Remarks =
                        NormalizeOptional(
                            remarks),

                    ChangedOn =
                        DateTime.UtcNow,

                    ChangedBy =
                        "System"
                });

            #endregion


            #region Complete Job When All Steps Finished

            var allStepsFinished =
    productionJob.Steps
        .Where(x =>
            !x.IsDeleted &&
            x.IsActive)
        .All(x =>
            x.Status ==
                ProductionJobStepStatus.Completed);


            if (allStepsFinished)
            {
                productionJob.Status =
                    ProductionJobStatus.Completed;

                productionJob.CompletedOn =
                    DateTime.UtcNow;
            }
            else
            {
                productionJob.Status =
                    ProductionJobStatus.InProgress;
            }


            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion




            await _repository
                .UpdateAsync(
                    productionJob);
        }

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


            if (normalizedReason.Length >
                1000)
            {
                throw new BusinessException(
                    "Cancellation Reason cannot exceed 1000 characters.");
            }

            #endregion


            #region Load Production Job

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

            if (productionJob.Status !=
                    ProductionJobStatus.Ready &&
                productionJob.Status !=
                    ProductionJobStatus.InProgress)
            {
                throw new BusinessException(
                    "Only Ready or In Progress Production Job can be cancelled.");
            }

            #endregion


            #region Find Running Step

            var runningStep =
                productionJob.Steps
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .FirstOrDefault(x =>
                        x.Status ==
                            ProductionJobStepStatus.InProgress);

            #endregion


            #region Cancel Running Step

            if (runningStep != null)
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
                                runningStep.AssignedMachineId.Value);
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


            #region Cancel Production Job

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

        #region Update Draft Job

        public async Task<ProductionJob> UpdateAsync(
            ProductionJob productionJob)
        {
            if (productionJob == null ||
                productionJob.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            if (productionJob.JobQuantity <= 0)
            {
                throw new BusinessException(
                    "Production Job Quantity must be greater than zero.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        productionJob.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Production Job not found.");
            }


            if (existing.Status !=
                ProductionJobStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Production Job can be edited.");
            }


            var customerPoItem =
                await _repository
                    .GetCustomerPurchaseOrderItemForProductionAsync(
                        existing.CustomerPurchaseOrderItemId);


            if (customerPoItem == null)
            {
                throw new BusinessException(
                    "Customer PO Item is no longer available for Production.");
            }


            var allocatedOtherJobs =
                await _repository
                    .GetAllocatedJobQuantityAsync(
                        existing.CustomerPurchaseOrderItemId,
                        existing.Id);


            var availableQuantity =
                customerPoItem.OrderedQuantity -
                allocatedOtherJobs;


            if (productionJob.JobQuantity >
                availableQuantity)
            {
                throw new BusinessException(
                    $"Production Job Quantity cannot exceed available quantity {availableQuantity:0.###}.");
            }


            ValidatePlanningDates(
                productionJob);


            existing.JobQuantity =
                productionJob.JobQuantity;

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


            await _repository
                .UpdateAsync(
                    existing);


            return existing;
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
            #region Load Deleted Production Job

            var productionJob =
                await _repository
                    .GetDeletedForUpdateAsync(id);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Deleted Production Job not found.");
            }

            #endregion


            #region Validate Quantity Before Restore

            // Cancelled Jobs are intentionally excluded from
            // allocated Production quantity.
            if (productionJob.Status !=
                ProductionJobStatus.Cancelled)
            {
                var sourceItem =
                    await _repository
                        .GetCustomerPurchaseOrderItemForProductionAsync(
                            productionJob.CustomerPurchaseOrderItemId);


                if (sourceItem == null)
                {
                    throw new BusinessException(
                        "Customer PO Item is no longer available for Production.");
                }


                var allocatedQuantity =
                    await _repository
                        .GetAllocatedJobQuantityAsync(
                            productionJob.CustomerPurchaseOrderItemId);


                var remainingQuantity =
                    sourceItem.OrderedQuantity -
                    allocatedQuantity;


                if (productionJob.JobQuantity >
                    remainingQuantity)
                {
                    throw new BusinessException(
                        $"Production Job cannot be restored. Only {remainingQuantity:0.###} {productionJob.UnitName} is currently available to plan.");
                }
            }

            #endregion


            #region Restore Production Job

            productionJob.IsDeleted =
                false;

            productionJob.IsActive =
                true;

            productionJob.ModifiedOn =
                DateTime.UtcNow;

            productionJob.ModifiedBy =
                "System";

            #endregion


            #region Restore Production Steps

            foreach (var step in
                productionJob.Steps)
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

            #endregion


            await _repository
                .UpdateAsync(
                    productionJob);
        }

        #endregion

        #region Pagination

        private static void NormalizePagination(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }


            if (pageSize != 10 &&
                pageSize != 25 &&
                pageSize != 50)
            {
                pageSize = 10;
            }
        }

        #endregion
    }
}