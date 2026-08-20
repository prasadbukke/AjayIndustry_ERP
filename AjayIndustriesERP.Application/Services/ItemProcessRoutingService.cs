/*
============================================================
File: ItemProcessRoutingService.cs

Purpose:
Implements Item Process Routing business rules.

Responsibilities:
- Create first Item Routing as Draft.
- Generate Routing Code and Revision Number.
- Validate Item / Operation / Machine references.
- Validate sequence and estimated time.
- Edit Draft Routing Steps.
- Release Draft Routing.
- Supersede previous Released revision.
- Create new Draft revision by copying Released Routing.
- Soft-delete and restore Draft Routing.

Routing Code:
AI/RTE/00001

Workflow:
Draft -> Released -> Superseded

Important:
- Routing is a reusable manufacturing template.
- Customer PO information does not belong here.
- Actual Production execution does not belong here.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class ItemProcessRoutingService
        : IItemProcessRoutingService
    {
        #region Fields

        private readonly IItemProcessRoutingRepository
            _repository;

        #endregion


        #region Constructor

        public ItemProcessRoutingService(
            IItemProcessRoutingRepository repository)
        {
            _repository = repository;
        }

        #endregion


        #region Read Operations

        public async Task<ItemProcessRouting?>
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


        public async Task<PagedResult<ItemProcessRouting>>
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


        #region Master Lookups

        public async Task<List<Item>>
            GetItemsForRoutingAsync()
        {
            return await _repository
                .GetItemsForRoutingAsync();
        }


        public async Task<List<ProductionOperation>>
            GetOperationsForRoutingAsync()
        {
            return await _repository
                .GetOperationsForRoutingAsync();
        }


        public async Task<List<Machine>>
            GetMachinesForRoutingAsync()
        {
            return await _repository
                .GetMachinesForRoutingAsync();
        }

        #endregion


        #region Create First Routing

        public async Task<ItemProcessRouting>
            CreateAsync(
                ItemProcessRouting routing)
        {
            if (routing == null)
            {
                throw new BusinessException(
                    "Routing information is required.");
            }


            await ValidateItemAsync(
                routing.ItemId);


            var existingRouting =
                await _repository
                    .ActiveRoutingExistsForItemAsync(
                        routing.ItemId);


            if (existingRouting)
            {
                throw new BusinessException(
                    "This Item already has a Process Routing. " +
                    "Use New Revision to change an existing Released Routing.");
            }


            NormalizeRouting(
                routing);


            ValidateHeader(
                routing);


            await ValidateStepsAsync(
                routing.Steps);


            routing.Code =
                await GenerateRoutingCodeAsync();


            var latestRevision =
                await _repository
                    .GetLatestRevisionNumberAsync(
                        routing.ItemId);


            routing.RevisionNumber =
                latestRevision + 1;


            routing.Status =
                ItemProcessRoutingStatus.Draft;


            routing.IsActive =
                true;

            routing.IsDeleted =
                false;

            routing.CreatedOn =
                DateTime.UtcNow;

            routing.CreatedBy =
                "System";


            PrepareNewSteps(
                routing.Steps);


            await _repository
                .AddAsync(
                    routing);


            return routing;
        }

        #endregion


        #region Update Draft Routing

        public async Task<ItemProcessRouting>
            UpdateAsync(
                ItemProcessRouting routing)
        {
            if (routing == null ||
                routing.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Item Process Routing.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        routing.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Item Process Routing not found.");
            }


            if (existing.Status !=
                ItemProcessRoutingStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Routing can be edited.");
            }


            /*
             * Item cannot be changed after Routing creation.
             */

            routing.ItemId =
                existing.ItemId;


            NormalizeRouting(
                routing);


            ValidateHeader(
                routing);


            await ValidateStepsAsync(
                routing.Steps);


            existing.EffectiveFrom =
                routing.EffectiveFrom;

            existing.Remarks =
                routing.Remarks;


            SyncSteps(
                existing,
                routing.Steps);


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


        #region Release Routing

        public async Task ReleaseAsync(
            int id)
        {
            var routing =
                await _repository
                    .GetForUpdateAsync(id);


            if (routing == null)
            {
                throw new BusinessException(
                    "Item Process Routing not found.");
            }


            if (routing.Status !=
                ItemProcessRoutingStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Routing can be released.");
            }


            var activeSteps =
                routing.Steps
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (!activeSteps.Any())
            {
                throw new BusinessException(
                    "At least one Routing Step is required before Release.");
            }


            await ValidateStepsAsync(
                activeSteps);


            /*
             * Find currently Released revision of same Item.
             */

            var previousReleased =
                await _repository
                    .GetReleasedRoutingForItemForUpdateAsync(
                        routing.ItemId,
                        routing.Id);


            if (previousReleased != null)
            {
                previousReleased.Status =
                    ItemProcessRoutingStatus.Superseded;

                previousReleased.IsActive =
                    false;

                previousReleased.ModifiedOn =
                    DateTime.UtcNow;

                previousReleased.ModifiedBy =
                    "System";
            }


            routing.Status =
                ItemProcessRoutingStatus.Released;

            routing.IsActive =
                true;


            if (!routing.EffectiveFrom.HasValue)
            {
                routing.EffectiveFrom =
                    DateTime.UtcNow.Date;
            }


            routing.ModifiedOn =
                DateTime.UtcNow;

            routing.ModifiedBy =
                "System";


            /*
             * Both records are tracked by same DbContext.
             * One SaveChanges commits the transition atomically.
             */

            await _repository
                .UpdateAsync(
                    routing);
        }

        #endregion


        #region Create New Revision

        public async Task<ItemProcessRouting>
            CreateRevisionAsync(
                int releasedRoutingId)
        {
            var source =
                await _repository
                    .GetForUpdateAsync(
                        releasedRoutingId);


            if (source == null)
            {
                throw new BusinessException(
                    "Item Process Routing not found.");
            }


            if (source.Status !=
                ItemProcessRoutingStatus.Released)
            {
                throw new BusinessException(
                    "A new Revision can only be created from the current Released Routing.");
            }


            var draftExists =
                await _repository
                    .DraftRoutingExistsForItemAsync(
                        source.ItemId);


            if (draftExists)
            {
                throw new BusinessException(
                    "A Draft Routing revision already exists for this Item.");
            }


            var latestRevision =
                await _repository
                    .GetLatestRevisionNumberAsync(
                        source.ItemId);


            var newRouting =
                new ItemProcessRouting
                {
                    Code =
                        await GenerateRoutingCodeAsync(),

                    ItemId =
                        source.ItemId,

                    RevisionNumber =
                        latestRevision + 1,

                    Status =
                        ItemProcessRoutingStatus.Draft,

                    EffectiveFrom =
                        null,

                    Remarks =
                        source.Remarks,

                    IsActive =
                        true,

                    IsDeleted =
                        false,

                    CreatedOn =
                        DateTime.UtcNow,

                    CreatedBy =
                        "System"
                };


            foreach (var step in source.Steps
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.SequenceNumber))
            {
                newRouting.Steps.Add(
                    new ItemProcessRoutingStep
                    {
                        SequenceNumber =
                            step.SequenceNumber,

                        ProductionOperationId =
                            step.ProductionOperationId,

                        DefaultMachineId =
                            step.DefaultMachineId,

                        SetupTimeMinutes =
                            step.SetupTimeMinutes,

                        CycleTimeMinutes =
                            step.CycleTimeMinutes,

                        OperationInstruction =
                            step.OperationInstruction,

                        Remarks =
                            step.Remarks,

                        IsActive =
                            true,

                        IsDeleted =
                            false,

                        CreatedOn =
                            DateTime.UtcNow,

                        CreatedBy =
                            "System"
                    });
            }


            await _repository
                .AddAsync(
                    newRouting);


            return newRouting;
        }

        #endregion


        #region Delete Draft Routing

        public async Task DeleteAsync(
            int id)
        {
            var routing =
                await _repository
                    .GetForUpdateAsync(id);


            if (routing == null)
            {
                throw new BusinessException(
                    "Item Process Routing not found.");
            }


            if (routing.Status !=
                ItemProcessRoutingStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Routing can be deleted.");
            }


            routing.IsDeleted =
                true;

            routing.IsActive =
                false;

            routing.ModifiedOn =
                DateTime.UtcNow;

            routing.ModifiedBy =
                "System";


            foreach (var step in routing.Steps)
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


            await _repository
                .UpdateAsync(
                    routing);
        }

        #endregion


        #region Deleted Routings

        public async Task<List<ItemProcessRouting>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }


        public async Task RestoreAsync(
            int id)
        {
            var routing =
                await _repository
                    .GetDeletedForUpdateAsync(id);


            if (routing == null)
            {
                throw new BusinessException(
                    "Deleted Routing not found.");
            }


            if (routing.Status !=
                ItemProcessRoutingStatus.Draft)
            {
                throw new BusinessException(
                    "Only deleted Draft Routing can be restored.");
            }


            var draftExists =
                await _repository
                    .DraftRoutingExistsForItemAsync(
                        routing.ItemId,
                        routing.Id);


            if (draftExists)
            {
                throw new BusinessException(
                    "Another Draft Routing already exists for this Item.");
            }


            routing.IsDeleted =
                false;

            routing.IsActive =
                true;

            routing.ModifiedOn =
                DateTime.UtcNow;

            routing.ModifiedBy =
                "System";


            foreach (var step in routing.Steps)
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


            await _repository
                .UpdateAsync(
                    routing);
        }

        #endregion


        #region Routing Validation

        private static void ValidateHeader(
            ItemProcessRouting routing)
        {
            if (routing.ItemId <= 0)
            {
                throw new BusinessException(
                    "Item is required.");
            }


            if (routing.Remarks?.Length >
                1000)
            {
                throw new BusinessException(
                    "Routing Remarks cannot exceed 1000 characters.");
            }
        }


        private async Task ValidateItemAsync(
            int itemId)
        {
            if (itemId <= 0)
            {
                throw new BusinessException(
                    "Item is required.");
            }


            var item =
                await _repository
                    .GetItemForRoutingAsync(
                        itemId);


            if (item == null)
            {
                throw new BusinessException(
                    "Selected Item is not available.");
            }
        }


        private async Task ValidateStepsAsync(
            ICollection<ItemProcessRoutingStep> steps)
        {
            if (steps == null ||
                !steps.Any(x =>
                    !x.IsDeleted))
            {
                throw new BusinessException(
                    "At least one Routing Step is required.");
            }


            var activeSteps =
                steps
                    .Where(x =>
                        !x.IsDeleted)
                    .ToList();


            var duplicateSequence =
                activeSteps
                    .GroupBy(x =>
                        x.SequenceNumber)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateSequence != null)
            {
                throw new BusinessException(
                    $"Sequence {duplicateSequence.Key} is used more than once.");
            }


            foreach (var step in activeSteps)
            {
                if (step.SequenceNumber <= 0)
                {
                    throw new BusinessException(
                        "Routing Step Sequence must be greater than zero.");
                }


                if (step.ProductionOperationId <= 0)
                {
                    throw new BusinessException(
                        $"Operation is required for Sequence {step.SequenceNumber}.");
                }


                var operation =
                    await _repository
                        .GetOperationForRoutingAsync(
                            step.ProductionOperationId);


                if (operation == null)
                {
                    throw new BusinessException(
                        $"Selected Operation for Sequence {step.SequenceNumber} is not available.");
                }


                if (step.DefaultMachineId.HasValue)
                {
                    var machine =
                        await _repository
                            .GetMachineForRoutingAsync(
                                step.DefaultMachineId.Value);


                    if (machine == null)
                    {
                        throw new BusinessException(
                            $"Selected Machine for Sequence {step.SequenceNumber} is not available.");
                    }
                }


                if (step.SetupTimeMinutes < 0)
                {
                    throw new BusinessException(
                        $"Setup Time cannot be negative for Sequence {step.SequenceNumber}.");
                }


                if (step.CycleTimeMinutes < 0)
                {
                    throw new BusinessException(
                        $"Cycle Time cannot be negative for Sequence {step.SequenceNumber}.");
                }


                if (step.OperationInstruction?.Length >
                    1000)
                {
                    throw new BusinessException(
                        $"Operation Instruction cannot exceed 1000 characters for Sequence {step.SequenceNumber}.");
                }


                if (step.Remarks?.Length >
                    1000)
                {
                    throw new BusinessException(
                        $"Remarks cannot exceed 1000 characters for Sequence {step.SequenceNumber}.");
                }
            }
        }

        #endregion


        #region Step Synchronization

        private static void SyncSteps(
            ItemProcessRouting existing,
            ICollection<ItemProcessRoutingStep> submittedSteps)
        {
            var submittedExistingIds =
                submittedSteps
                    .Where(x =>
                        x.Id > 0)
                    .Select(x =>
                        x.Id)
                    .ToHashSet();


            /*
             * Existing Step removed from UI
             * -> Soft Delete.
             */

            foreach (var existingStep in
                existing.Steps
                    .Where(x =>
                        !x.IsDeleted &&
                        !submittedExistingIds.Contains(
                            x.Id)))
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


            foreach (var submittedStep in
                submittedSteps
                    .Where(x =>
                        !x.IsDeleted))
            {
                if (submittedStep.Id > 0)
                {
                    var existingStep =
                        existing.Steps
                            .FirstOrDefault(x =>
                                x.Id ==
                                submittedStep.Id);


                    if (existingStep == null ||
                        existingStep.IsDeleted)
                    {
                        throw new BusinessException(
                            "Invalid Routing Step.");
                    }


                    existingStep.SequenceNumber =
                        submittedStep.SequenceNumber;

                    existingStep.ProductionOperationId =
                        submittedStep.ProductionOperationId;

                    existingStep.DefaultMachineId =
                        submittedStep.DefaultMachineId;

                    existingStep.SetupTimeMinutes =
                        submittedStep.SetupTimeMinutes;

                    existingStep.CycleTimeMinutes =
                        submittedStep.CycleTimeMinutes;

                    existingStep.OperationInstruction =
                        NormalizeOptional(
                            submittedStep.OperationInstruction);

                    existingStep.Remarks =
                        NormalizeOptional(
                            submittedStep.Remarks);

                    existingStep.ModifiedOn =
                        DateTime.UtcNow;

                    existingStep.ModifiedBy =
                        "System";
                }
                else
                {
                    existing.Steps.Add(
                        new ItemProcessRoutingStep
                        {
                            SequenceNumber =
                                submittedStep.SequenceNumber,

                            ProductionOperationId =
                                submittedStep.ProductionOperationId,

                            DefaultMachineId =
                                submittedStep.DefaultMachineId,

                            SetupTimeMinutes =
                                submittedStep.SetupTimeMinutes,

                            CycleTimeMinutes =
                                submittedStep.CycleTimeMinutes,

                            OperationInstruction =
                                NormalizeOptional(
                                    submittedStep.OperationInstruction),

                            Remarks =
                                NormalizeOptional(
                                    submittedStep.Remarks),

                            IsActive =
                                true,

                            IsDeleted =
                                false,

                            CreatedOn =
                                DateTime.UtcNow,

                            CreatedBy =
                                "System"
                        });
                }
            }
        }

        #endregion


        #region New Step Preparation

        private static void PrepareNewSteps(
            ICollection<ItemProcessRoutingStep> steps)
        {
            foreach (var step in steps)
            {
                step.OperationInstruction =
                    NormalizeOptional(
                        step.OperationInstruction);

                step.Remarks =
                    NormalizeOptional(
                        step.Remarks);

                step.IsActive =
                    true;

                step.IsDeleted =
                    false;

                step.CreatedOn =
                    DateTime.UtcNow;

                step.CreatedBy =
                    "System";
            }
        }

        #endregion


        #region Normalization

        private static void NormalizeRouting(
            ItemProcessRouting routing)
        {
            routing.Remarks =
                NormalizeOptional(
                    routing.Remarks);


            foreach (var step in routing.Steps)
            {
                step.OperationInstruction =
                    NormalizeOptional(
                        step.OperationInstruction);

                step.Remarks =
                    NormalizeOptional(
                        step.Remarks);
            }
        }


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


        #region Routing Code

        private async Task<string>
            GenerateRoutingCodeAsync()
        {
            const string prefix =
                "AI/RTE/";


            var lastCode =
                await _repository
                    .GetLastRoutingCodeAsync();


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
                    "Unable to generate Routing Code.");
            }


            return
                $"{prefix}{lastNumber + 1:00000}";
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