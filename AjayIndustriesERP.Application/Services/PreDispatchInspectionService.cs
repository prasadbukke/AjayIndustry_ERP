/*
============================================================
File: PreDispatchInspectionService.cs

Purpose:
Implements Pre-Dispatch / Final Inspection business rules.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Pre-Dispatch Inspection

Responsibilities:
- Read and search PDI Reports.
- Load Production Jobs containing completed Production Items.
- Calculate remaining Inspection Quantity Item-wise.
- Prepare PDI Draft from Production Job Item.
- Auto-load Customer / PO / Item information.
- Auto-load current Workshop Drawing.
- Auto-load current Customer Drawing.
- Auto-load Item Specifications as Inspection Lines.
- Create and edit Draft PDI Reports.
- Validate Inspection Lines and Observations.
- Finalize PDI Report.
- Calculate Overall Inspection Result.
- Soft-delete and restore Draft PDI Reports.
- Generate sequential PDI Code.

PDI Code:
AI/PDI/{YY-YY}/{00001}

Example:
AI/PDI/26-27/00001

Important:
- Production Job is the parent Production transaction.
- Production Job Item is the actual PDI source.
- PDI quantity allocation is ProductionJobItem-wise.
- Source information is trusted from ERP.
- Browser-posted Customer / Item / Drawing snapshot
  values are not trusted.
- One Production Job Item may have multiple PDI Reports.
- Finalized PDI Reports are locked.
- Final Drawing snapshot is refreshed before finalization.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class PreDispatchInspectionService
        : IPreDispatchInspectionService
    {
        #region Fields

        private readonly
            IPreDispatchInspectionRepository
            _repository;

        private readonly
            ICustomerDrawingService
            _customerDrawingService;

        private readonly
            IPreDispatchInspectionPdfGenerator
            _pdfGenerator;

        #endregion


        #region Constructor

        public PreDispatchInspectionService(
            IPreDispatchInspectionRepository repository,
            ICustomerDrawingService customerDrawingService,
            IPreDispatchInspectionPdfGenerator pdfGenerator)
        {
            _repository =
                repository;

            _customerDrawingService =
                customerDrawingService;

            _pdfGenerator =
                pdfGenerator;
        }

        #endregion


        #region Read Operations

        public async Task<PreDispatchInspection?>
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


        public async Task<PagedResult<PreDispatchInspection>>
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


        #region Production Source

        public async Task<List<ProductionJob>>
            GetProductionJobsForInspectionAsync()
        {
            var productionJobs =
                await _repository
                    .GetProductionJobsForInspectionAsync();


            var availableJobs =
                new List<ProductionJob>();


            foreach (var productionJob
                     in productionJobs)
            {
                if (
                    productionJob.IsDeleted
                    ||
                    !productionJob.IsActive
                    ||
                    productionJob.Status ==
                        ProductionJobStatus.Cancelled
                )
                {
                    continue;
                }


                var eligibleItems =
                    productionJob
                        .Items
                        .Where(item =>
                            !item.IsDeleted
                            &&
                            item.IsActive
                            &&
                            item.ProductionQuantity > 0m
                            &&
                            item.CompletedQuantity >=
                                item.ProductionQuantity)
                        .ToList();


                var hasAvailableQuantity =
                    false;


                foreach (var productionJobItem
                         in eligibleItems)
                {
                    var allocatedQuantity =
                        await _repository
                            .GetAllocatedInspectionQuantityAsync(
                                productionJobItem.Id);


                    var remainingQuantity =
                        productionJobItem
                            .CompletedQuantity
                        -
                        allocatedQuantity;


                    if (remainingQuantity > 0m)
                    {
                        hasAvailableQuantity =
                            true;

                        break;
                    }
                }


                if (hasAvailableQuantity)
                {
                    availableJobs.Add(
                        productionJob);
                }
            }


            return availableJobs;
        }


        /*
         * IMPORTANT:
         *
         * productionJobItemId is the actual source ID.
         *
         * Repository returns the parent ProductionJob
         * containing that ProductionJobItem.
         */

        public async Task<ProductionJob?>
            GetProductionJobForInspectionAsync(
                int productionJobItemId)
        {
            if (productionJobItemId <= 0)
            {
                return null;
            }


            return await _repository
                .GetProductionJobForInspectionAsync(
                    productionJobItemId);
        }


        public async Task<decimal>
            GetRemainingInspectionQuantityAsync(
                int productionJobItemId,
                int? excludePreDispatchInspectionId = null)
        {
            #region Load Production Source

            var productionJob =
                await _repository
                    .GetProductionJobForInspectionAsync(
                        productionJobItemId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Production Item is not available for Inspection.");
            }


            var productionJobItem =
                GetProductionJobItem(
                    productionJob,
                    productionJobItemId);


            ValidateProductionSource(
                productionJob,
                productionJobItem);

            #endregion


            #region Calculate Allocated Quantity

            var allocatedQuantity =
                await _repository
                    .GetAllocatedInspectionQuantityAsync(
                        productionJobItemId,
                        excludePreDispatchInspectionId);


            var remainingQuantity =
                productionJobItem
                    .CompletedQuantity
                -
                allocatedQuantity;

            #endregion


            return remainingQuantity < 0m
                ? 0m
                : remainingQuantity;
        }

        #endregion


        #region Prepare Draft Source

        public async Task<PreDispatchInspection?>
            PrepareDraftAsync(
                int productionJobItemId)
        {
            #region Load Production Source

            var productionJob =
                await _repository
                    .GetProductionJobForInspectionAsync(
                        productionJobItemId);


            if (productionJob == null)
            {
                return null;
            }


            var productionJobItem =
                GetProductionJobItem(
                    productionJob,
                    productionJobItemId);


            ValidateProductionSource(
                productionJob,
                productionJobItem);

            #endregion


            #region Remaining Quantity

            var remainingQuantity =
                await GetRemainingInspectionQuantityAsync(
                    productionJobItemId);


            if (remainingQuantity <= 0m)
            {
                throw new BusinessException(
                    "The complete produced Quantity of this Item is already allocated to PDI Reports.");
            }

            #endregion


            #region Prepare Header

            var preDispatchInspection =
                new PreDispatchInspection
                {
                    ProductionJobId =
                        productionJob.Id,

                    ProductionJobItemId =
                        productionJobItem.Id,

                    InspectionDate =
                        DateTime.Today,

                    InspectionQuantity =
                        remainingQuantity,

                    AcceptedQuantity =
                        0m,

                    ReworkQuantity =
                        0m,

                    RejectedQuantity =
                        0m,

                    Status =
                        PreDispatchInspectionStatus.Draft,

                    Result =
                        PreDispatchInspectionResult.Pending,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };


            await ApplyTrustedSourceAsync(
                preDispatchInspection,
                productionJob,
                productionJobItem);

            #endregion


            #region Prepare Specification Lines

            PrepareSpecificationLines(
                preDispatchInspection,
                productionJobItem);

            #endregion


            return preDispatchInspection;
        }

        #endregion


        #region Create

        public async Task<PreDispatchInspection>
            CreateAsync(
                PreDispatchInspection
                    preDispatchInspection)
        {
            #region Basic Validation

            if (preDispatchInspection == null)
            {
                throw new BusinessException(
                    "Pre-Dispatch Inspection information is required.");
            }


            if (
                preDispatchInspection
                    .ProductionJobItemId <= 0
            )
            {
                throw new BusinessException(
                    "Please select a Production Item.");
            }

            #endregion


            #region Load Trusted Source

            var productionJob =
                await _repository
                    .GetProductionJobForInspectionAsync(
                        preDispatchInspection
                            .ProductionJobItemId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Selected Production Item is not available for Inspection.");
            }


            var productionJobItem =
                GetProductionJobItem(
                    productionJob,
                    preDispatchInspection
                        .ProductionJobItemId);


            ValidateProductionSource(
                productionJob,
                productionJobItem);

            #endregion


            #region Quantity Validation

            var remainingQuantity =
                await GetRemainingInspectionQuantityAsync(
                    productionJobItem.Id);


            ValidateInspectionQuantity(
                preDispatchInspection
                    .InspectionQuantity,
                remainingQuantity);

            #endregion


            #region Prepare Trusted Header

            var prepared =
                new PreDispatchInspection
                {
                    ProductionJobId =
                        productionJob.Id,

                    ProductionJobItemId =
                        productionJobItem.Id,

                    InspectionDate =
                        preDispatchInspection
                            .InspectionDate,

                    InspectionQuantity =
                        preDispatchInspection
                            .InspectionQuantity,

                    AcceptedQuantity =
                        preDispatchInspection
                            .AcceptedQuantity,

                    ReworkQuantity =
                        preDispatchInspection
                            .ReworkQuantity,

                    RejectedQuantity =
                        preDispatchInspection
                            .RejectedQuantity,

                    InvoiceNumber =
                        NormalizeOptional(
                            preDispatchInspection
                                .InvoiceNumber),

                    InvoiceDate =
                        preDispatchInspection
                            .InvoiceDate,

                    InvoiceQuantity =
                        preDispatchInspection
                            .InvoiceQuantity,

                    SupplierRemarks =
                        NormalizeOptional(
                            preDispatchInspection
                                .SupplierRemarks),

                    InspectionRemarks =
                        NormalizeOptional(
                            preDispatchInspection
                                .InspectionRemarks),

                    InspectedBy =
                        NormalizeOptional(
                            preDispatchInspection
                                .InspectedBy),

                    ReviewedBy =
                        NormalizeOptional(
                            preDispatchInspection
                                .ReviewedBy),

                    Status =
                        PreDispatchInspectionStatus.Draft,

                    Result =
                        PreDispatchInspectionResult.Pending,

                    IsActive =
                        true,

                    IsDeleted =
                        false,

                    CreatedOn =
                        DateTime.UtcNow,

                    CreatedBy =
                        "System"
                };


            await ApplyTrustedSourceAsync(
                prepared,
                productionJob,
                productionJobItem);

            #endregion


            #region Draft Validation

            ValidateDraftHeader(
                prepared);

            #endregion


            #region Prepare Inspection Lines

            CopySubmittedLinesForCreate(
                preDispatchInspection,
                prepared);

            #endregion


            #region Generate PDI Code

            prepared.Code =
                await GenerateCodeAsync();

            #endregion


            #region Save

            await _repository
                .AddAsync(
                    prepared);

            #endregion


            return prepared;
        }

        #endregion


        #region Update Draft

        public async Task<PreDispatchInspection>
            UpdateAsync(
                PreDispatchInspection
                    preDispatchInspection)
        {
            #region Basic Validation

            if (
                preDispatchInspection == null
                ||
                preDispatchInspection.Id <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Pre-Dispatch Inspection Report.");
            }

            #endregion


            #region Load Existing PDI

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        preDispatchInspection.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Pre-Dispatch Inspection Report not found.");
            }


            if (
                existing.Status !=
                    PreDispatchInspectionStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft PDI Report can be edited.");
            }


            if (existing.ProductionJobItemId <= 0)
            {
                throw new BusinessException(
                    "Production Item reference is missing from PDI Report.");
            }

            #endregion


            #region Load Production Source

            var productionJob =
                await _repository
                    .GetProductionJobForInspectionAsync(
                        existing
                            .ProductionJobItemId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Source Production Item is not available for Inspection.");
            }


            var productionJobItem =
                GetProductionJobItem(
                    productionJob,
                    existing
                        .ProductionJobItemId);


            ValidateProductionSource(
                productionJob,
                productionJobItem);

            #endregion


            #region Quantity Validation

            var remainingQuantity =
                await GetRemainingInspectionQuantityAsync(
                    existing
                        .ProductionJobItemId,
                    existing.Id);


            ValidateInspectionQuantity(
                preDispatchInspection
                    .InspectionQuantity,
                remainingQuantity);

            #endregion


            #region Update Editable Header

            existing.InspectionDate =
                preDispatchInspection
                    .InspectionDate;


            existing.InspectionQuantity =
                preDispatchInspection
                    .InspectionQuantity;


            existing.AcceptedQuantity =
                preDispatchInspection
                    .AcceptedQuantity;


            existing.ReworkQuantity =
                preDispatchInspection
                    .ReworkQuantity;


            existing.RejectedQuantity =
                preDispatchInspection
                    .RejectedQuantity;


            existing.InvoiceNumber =
                NormalizeOptional(
                    preDispatchInspection
                        .InvoiceNumber);


            existing.InvoiceDate =
                preDispatchInspection
                    .InvoiceDate;


            existing.InvoiceQuantity =
                preDispatchInspection
                    .InvoiceQuantity;


            existing.SupplierRemarks =
                NormalizeOptional(
                    preDispatchInspection
                        .SupplierRemarks);


            existing.InspectionRemarks =
                NormalizeOptional(
                    preDispatchInspection
                        .InspectionRemarks);


            existing.InspectedBy =
                NormalizeOptional(
                    preDispatchInspection
                        .InspectedBy);


            existing.ReviewedBy =
                NormalizeOptional(
                    preDispatchInspection
                        .ReviewedBy);


            existing.Result =
                PreDispatchInspectionResult.Pending;


            existing.ModifiedOn =
                DateTime.UtcNow;


            existing.ModifiedBy =
                "System";

            #endregion


            #region Refresh Trusted Source

            await ApplyTrustedSourceAsync(
                existing,
                productionJob,
                productionJobItem);

            #endregion


            #region Validate Draft

            ValidateDraftHeader(
                existing);

            #endregion


            #region Synchronize Inspection Lines

            SynchronizeDraftLines(
                existing,
                preDispatchInspection);

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    existing);

            #endregion


            return existing;
        }

        #endregion


        #region Finalize

        public async Task<PreDispatchInspection>
            FinalizeAsync(
                int id)
        {
            #region Load PDI

            var preDispatchInspection =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (preDispatchInspection == null)
            {
                throw new BusinessException(
                    "Pre-Dispatch Inspection Report not found.");
            }


            if (
                preDispatchInspection.Status !=
                    PreDispatchInspectionStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft PDI Report can be finalized.");
            }


            if (
                preDispatchInspection
                    .ProductionJobItemId <= 0
            )
            {
                throw new BusinessException(
                    "Production Item reference is missing from PDI Report.");
            }

            #endregion


            #region Refresh Trusted Source

            var productionJob =
                await _repository
                    .GetProductionJobForInspectionAsync(
                        preDispatchInspection
                            .ProductionJobItemId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Source Production Item is not available for Inspection.");
            }


            var productionJobItem =
                GetProductionJobItem(
                    productionJob,
                    preDispatchInspection
                        .ProductionJobItemId);


            ValidateProductionSource(
                productionJob,
                productionJobItem);


            await ApplyTrustedSourceAsync(
                preDispatchInspection,
                productionJob,
                productionJobItem);

            #endregion


            #region Final Validation

            ValidateDraftHeader(
                preDispatchInspection);


            ValidateFinalInspection(
                preDispatchInspection);

            #endregion


            #region Calculate Result

            preDispatchInspection.Result =
                CalculateOverallResult(
                    preDispatchInspection);

            #endregion


            #region Finalize Report

            preDispatchInspection.Status =
                PreDispatchInspectionStatus.Finalized;


            preDispatchInspection.FinalizedOn =
                DateTime.UtcNow;


            preDispatchInspection.FinalizedBy =
                "System";


            preDispatchInspection.ModifiedOn =
                DateTime.UtcNow;


            preDispatchInspection.ModifiedBy =
                "System";

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    preDispatchInspection);

            #endregion


            return preDispatchInspection;
        }

        #endregion


        #region PDF

        public async Task<byte[]>
            GeneratePdfAsync(
                int id,
                string documentNumber)
        {
            #region Load Report

            var preDispatchInspection =
                await _repository
                    .GetByIdAsync(
                        id);


            if (preDispatchInspection == null)
            {
                throw new BusinessException(
                    "Pre-Dispatch Inspection Report not found.");
            }

            #endregion


            #region Validate Finalized Status

            if (
                preDispatchInspection.Status !=
                    PreDispatchInspectionStatus.Finalized
            )
            {
                throw new BusinessException(
                    "Only Finalized PDI Report can generate the Final Inspection PDF.");
            }

            #endregion


            #region Validate Document Number

            if (string.IsNullOrWhiteSpace(
                documentNumber))
            {
                throw new BusinessException(
                    "Document Number is required.");
            }


            documentNumber =
                documentNumber.Trim();


            if (documentNumber.Length > 100)
            {
                throw new BusinessException(
                    "Document Number cannot exceed 100 characters.");
            }

            #endregion


            #region Generate PDF

            var pdfBytes =
                _pdfGenerator
                    .Generate(
                        preDispatchInspection,
                        documentNumber);

            #endregion


            #region Validate PDF

            if (
                pdfBytes == null
                ||
                pdfBytes.Length == 0
            )
            {
                throw new BusinessException(
                    "Unable to generate Final Inspection Report PDF.");
            }

            #endregion


            return pdfBytes;
        }

        #endregion


        #region Delete

        public async Task DeleteAsync(
            int id)
        {
            #region Load PDI

            var preDispatchInspection =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (preDispatchInspection == null)
            {
                throw new BusinessException(
                    "Pre-Dispatch Inspection Report not found.");
            }

            #endregion


            #region Validate Delete

            if (
                preDispatchInspection.Status !=
                    PreDispatchInspectionStatus.Draft
            )
            {
                throw new BusinessException(
                    "Finalized PDI Report cannot be deleted.");
            }

            #endregion


            #region Soft Delete

            preDispatchInspection.IsDeleted =
                true;


            preDispatchInspection.IsActive =
                false;


            preDispatchInspection.ModifiedOn =
                DateTime.UtcNow;


            preDispatchInspection.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    preDispatchInspection);
        }

        #endregion


        #region Deleted Reports

        public async Task<List<PreDispatchInspection>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }


        public async Task RestoreAsync(
            int id)
        {
            #region Load Deleted Report

            var preDispatchInspection =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);


            if (preDispatchInspection == null)
            {
                throw new BusinessException(
                    "Deleted PDI Report not found.");
            }

            #endregion


            #region Validate Restore

            if (
                preDispatchInspection.Status !=
                    PreDispatchInspectionStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft PDI Report can be restored.");
            }


            if (
                preDispatchInspection
                    .ProductionJobItemId <= 0
            )
            {
                throw new BusinessException(
                    "Production Item reference is missing from PDI Report.");
            }

            #endregion


            #region Quantity Validation

            var productionJob =
                await _repository
                    .GetProductionJobForInspectionAsync(
                        preDispatchInspection
                            .ProductionJobItemId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Source Production Item is not available for Inspection.");
            }


            var productionJobItem =
                GetProductionJobItem(
                    productionJob,
                    preDispatchInspection
                        .ProductionJobItemId);


            ValidateProductionSource(
                productionJob,
                productionJobItem);


            var allocatedQuantity =
                await _repository
                    .GetAllocatedInspectionQuantityAsync(
                        preDispatchInspection
                            .ProductionJobItemId);


            var remainingQuantity =
                productionJobItem
                    .CompletedQuantity
                -
                allocatedQuantity;


            if (remainingQuantity < 0m)
            {
                remainingQuantity =
                    0m;
            }


            if (
                preDispatchInspection
                    .InspectionQuantity >
                remainingQuantity
            )
            {
                throw new BusinessException(
                    $"PDI Report cannot be restored. " +
                    $"Only {remainingQuantity:0.###} " +
                    $"{preDispatchInspection.UnitName} " +
                    $"is currently available for Inspection.");
            }

            #endregion


            #region Restore

            preDispatchInspection.IsDeleted =
                false;


            preDispatchInspection.IsActive =
                true;


            preDispatchInspection.ModifiedOn =
                DateTime.UtcNow;


            preDispatchInspection.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    preDispatchInspection);
        }

        #endregion


        #region Production Source Validation

        private static ProductionJobItem
            GetProductionJobItem(
                ProductionJob productionJob,
                int productionJobItemId)
        {
            var productionJobItem =
                productionJob
                    .Items
                    .FirstOrDefault(item =>
                        item.Id ==
                            productionJobItemId
                        &&
                        !item.IsDeleted
                        &&
                        item.IsActive);


            if (productionJobItem == null)
            {
                throw new BusinessException(
                    "Production Item information is missing from Production Job.");
            }


            return productionJobItem;
        }


        private static void
            ValidateProductionSource(
                ProductionJob productionJob,
                ProductionJobItem productionJobItem)
        {
            #region Production Job

            if (
                productionJob.IsDeleted
                ||
                !productionJob.IsActive
            )
            {
                throw new BusinessException(
                    "Inactive Production Job cannot be used for PDI.");
            }


            if (
                productionJob.Status ==
                    ProductionJobStatus.Cancelled
            )
            {
                throw new BusinessException(
                    "Cancelled Production Job cannot be used for PDI.");
            }

            #endregion


            #region Production Job Item

            if (
                productionJobItem.IsDeleted
                ||
                !productionJobItem.IsActive
            )
            {
                throw new BusinessException(
                    "Inactive Production Item cannot be used for PDI.");
            }


            if (
                productionJobItem
                    .ProductionQuantity <= 0m
            )
            {
                throw new BusinessException(
                    "Production Quantity is not available for the selected Item.");
            }


            if (
                productionJobItem
                    .CompletedQuantity <
                productionJobItem
                    .ProductionQuantity
            )
            {
                throw new BusinessException(
                    "Current Production Quantity of the selected Item is not completed.");
            }


            if (
                productionJobItem
                    .CompletedQuantity <= 0m
            )
            {
                throw new BusinessException(
                    "Completed Production Quantity is not available for Inspection.");
            }

            #endregion
        }

        #endregion


        #region Trusted Source Mapping

        private async Task ApplyTrustedSourceAsync(
            PreDispatchInspection
                preDispatchInspection,
            ProductionJob productionJob,
            ProductionJobItem productionJobItem)
        {
            #region Validate Source

            ValidateProductionSource(
                productionJob,
                productionJobItem);

            #endregion


            #region Source References

            var customerPoItem =
                productionJobItem
                    .CustomerPurchaseOrderItem;


            var customerPurchaseOrder =
                productionJob
                    .CustomerPurchaseOrder;


            var item =
                productionJobItem
                    .Item;


            if (customerPoItem == null)
            {
                throw new BusinessException(
                    "Customer PO Item information is missing from Production Item.");
            }


            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Customer PO information is missing from Production Job.");
            }


            if (item == null)
            {
                throw new BusinessException(
                    "Item Master information is missing from Production Item.");
            }

            #endregion


            #region Production Snapshot

            preDispatchInspection.ProductionJobId =
                productionJob.Id;


            preDispatchInspection.ProductionJobItemId =
                productionJobItem.Id;


            preDispatchInspection.ProductionJobCode =
                productionJob.Code;

            #endregion


            #region Customer Snapshot

            preDispatchInspection.CustomerId =
                customerPurchaseOrder
                    .CustomerId;


            preDispatchInspection.CustomerName =
                customerPurchaseOrder
                    .CustomerName;

            #endregion


            #region Customer PO Snapshot

            preDispatchInspection
                .CustomerPurchaseOrderItemId =
                customerPoItem.Id;


            preDispatchInspection
                .CustomerPurchaseOrderCode =
                customerPurchaseOrder.Code;


            preDispatchInspection
                .CustomerPurchaseOrderNumber =
                customerPurchaseOrder
                    .CustomerPurchaseOrderNumber;


            preDispatchInspection.CustomerItemCode =
                NormalizeOptional(
                    customerPoItem
                        .CustomerItemCode);

            #endregion


            #region Item Snapshot

            preDispatchInspection.ItemId =
                productionJobItem.ItemId;


            preDispatchInspection.ItemCode =
                productionJobItem.ItemCode;


            preDispatchInspection.ItemName =
                productionJobItem.ItemName;


            preDispatchInspection.UnitName =
                NormalizeOptional(
                    productionJobItem
                        .UnitName);


            /*
             * Part Number priority:
             *
             * 1. Customer Item Code
             * 2. Item Master Part Number
             * 3. ERP Item Code
             */

            preDispatchInspection.PartNumber =
                !string.IsNullOrWhiteSpace(
                    customerPoItem.CustomerItemCode)
                    ? customerPoItem
                        .CustomerItemCode
                        .Trim()
                    : !string.IsNullOrWhiteSpace(
                        item.PartNumber)
                        ? item.PartNumber.Trim()
                        : productionJobItem
                            .ItemCode;

            #endregion


            #region Current Workshop Drawing Snapshot

            var currentWorkshopDrawing =
                item.Drawings
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderByDescending(x =>
                        x.DrawingId)
                    .FirstOrDefault();


            preDispatchInspection.WorkshopDrawingId =
                currentWorkshopDrawing?
                    .DrawingId;


            preDispatchInspection
                .WorkshopDrawingNumber =
                NormalizeOptional(
                    currentWorkshopDrawing?
                        .DrawingNumber);


            preDispatchInspection
                .WorkshopDrawingRevision =
                NormalizeOptional(
                    currentWorkshopDrawing?
                        .RevisionNumber);

            #endregion


            #region Current Customer Drawing Snapshot

            CustomerDrawing?
                currentCustomerDrawing =
                    null;


            if (
                customerPurchaseOrder
                    .CustomerId > 0
                &&
                productionJobItem
                    .ItemId > 0
            )
            {
                currentCustomerDrawing =
                    await _customerDrawingService
                        .GetByCustomerAndItemAsync(
                            customerPurchaseOrder
                                .CustomerId,
                            productionJobItem
                                .ItemId);
            }


            preDispatchInspection.CustomerDrawingId =
                currentCustomerDrawing?
                    .CustomerDrawingId;


            preDispatchInspection
                .CustomerDrawingNumber =
                NormalizeOptional(
                    currentCustomerDrawing?
                        .DrawingNumber);


            preDispatchInspection
                .CustomerDrawingRevision =
                NormalizeOptional(
                    currentCustomerDrawing?
                        .RevisionNumber);

            #endregion
        }

        #endregion


        #region Prepare Specification Lines

        private static void PrepareSpecificationLines(
            PreDispatchInspection
                preDispatchInspection,
            ProductionJobItem productionJobItem)
        {
            #region Clear Existing Lines

            preDispatchInspection
                .Lines
                .Clear();

            #endregion


            #region Load Item Specifications

            var specifications =
                productionJobItem
                    .Item?
                    .ItemSpecifications
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SortOrder)
                    .ToList()
                ??
                new List<ItemSpecification>();

            #endregion


            #region Create Inspection Lines

            var sequenceNumber =
                1;


            foreach (var itemSpecification
                     in specifications)
            {
                var parameter =
                    itemSpecification
                        .Specification?
                        .SpecificationName;


                if (string.IsNullOrWhiteSpace(
                    parameter))
                {
                    continue;
                }


                var specification =
                    BuildSpecificationValue(
                        itemSpecification);


                var line =
                    new PreDispatchInspectionLine
                    {
                        SequenceNumber =
                            sequenceNumber,

                        Parameter =
                            parameter.Trim(),

                        Specification =
                            specification,

                        InspectionMethod =
                            null,

                        Result =
                            PreDispatchInspectionLineResult.Pending,

                        Remarks =
                            null,

                        IsActive =
                            true,

                        IsDeleted =
                            false
                    };


                AddDefaultObservations(
                    line);


                preDispatchInspection
                    .Lines
                    .Add(
                        line);


                sequenceNumber++;
            }

            #endregion


            #region Blank Fallback Line

            if (
                preDispatchInspection
                    .Lines
                    .Count == 0
            )
            {
                var line =
                    new PreDispatchInspectionLine
                    {
                        SequenceNumber =
                            1,

                        Parameter =
                            string.Empty,

                        Specification =
                            string.Empty,

                        Result =
                            PreDispatchInspectionLineResult.Pending,

                        IsActive =
                            true,

                        IsDeleted =
                            false
                    };


                AddDefaultObservations(
                    line);


                preDispatchInspection
                    .Lines
                    .Add(
                        line);
            }

            #endregion
        }

        #endregion


        #region Create Line Mapping

        private static void CopySubmittedLinesForCreate(
            PreDispatchInspection source,
            PreDispatchInspection target)
        {
            var submittedLines =
                source.Lines
                    .Where(x =>
                        !IsCompletelyBlankLine(
                            x))
                    .ToList();


            var sequenceNumber =
                1;


            foreach (var submittedLine
                     in submittedLines)
            {
                var line =
                    CreateLineFromSubmitted(
                        submittedLine,
                        sequenceNumber);


                target.Lines.Add(
                    line);


                sequenceNumber++;
            }
        }


        private static PreDispatchInspectionLine
            CreateLineFromSubmitted(
                PreDispatchInspectionLine submittedLine,
                int sequenceNumber)
        {
            #region Prepare Line

            var line =
                new PreDispatchInspectionLine
                {
                    SequenceNumber =
                        sequenceNumber,

                    Parameter =
                        NormalizeRequired(
                            submittedLine.Parameter),

                    Specification =
                        NormalizeRequired(
                            submittedLine.Specification),

                    InspectionMethod =
                        NormalizeOptional(
                            submittedLine
                                .InspectionMethod),

                    Result =
                        NormalizeLineResult(
                            submittedLine.Result),

                    Remarks =
                        NormalizeOptional(
                            submittedLine.Remarks),

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


            #region Prepare Observations

            CopyObservationsForCreate(
                submittedLine,
                line);

            #endregion


            return line;
        }

        #endregion


        #region Draft Line Synchronization

        private static void SynchronizeDraftLines(
            PreDispatchInspection existing,
            PreDispatchInspection submitted)
        {
            #region Active Existing Lines

            var activeExistingLines =
                existing.Lines
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .ToList();

            #endregion


            #region Submitted Line IDs

            var submittedExistingIds =
                submitted.Lines
                    .Where(x =>
                        x.Id > 0
                        &&
                        !IsCompletelyBlankLine(
                            x))
                    .Select(x =>
                        x.Id)
                    .ToHashSet();

            #endregion


            #region Remove Missing Lines

            foreach (var existingLine
                     in activeExistingLines)
            {
                if (
                    submittedExistingIds
                        .Contains(
                            existingLine.Id)
                )
                {
                    continue;
                }


                existingLine.IsDeleted =
                    true;


                existingLine.IsActive =
                    false;


                existingLine.ModifiedOn =
                    DateTime.UtcNow;


                existingLine.ModifiedBy =
                    "System";


                foreach (var observation
                         in existingLine
                             .Observations
                             .Where(x =>
                                 !x.IsDeleted))
                {
                    observation.IsDeleted =
                        true;

                    observation.IsActive =
                        false;

                    observation.ModifiedOn =
                        DateTime.UtcNow;

                    observation.ModifiedBy =
                        "System";
                }
            }

            #endregion


            #region Update Existing Lines

            foreach (var submittedLine
                     in submitted.Lines
                         .Where(x =>
                             x.Id > 0
                             &&
                             !IsCompletelyBlankLine(
                                 x)))
            {
                var existingLine =
                    activeExistingLines
                        .FirstOrDefault(x =>
                            x.Id ==
                            submittedLine.Id);


                if (existingLine == null)
                {
                    throw new BusinessException(
                        "Invalid PDI Inspection Line.");
                }


                existingLine.Parameter =
                    NormalizeRequired(
                        submittedLine.Parameter);


                existingLine.Specification =
                    NormalizeRequired(
                        submittedLine.Specification);


                existingLine.InspectionMethod =
                    NormalizeOptional(
                        submittedLine
                            .InspectionMethod);


                existingLine.Result =
                    NormalizeLineResult(
                        submittedLine.Result);


                existingLine.Remarks =
                    NormalizeOptional(
                        submittedLine.Remarks);


                existingLine.ModifiedOn =
                    DateTime.UtcNow;


                existingLine.ModifiedBy =
                    "System";


                SynchronizeObservations(
                    existingLine,
                    submittedLine);
            }

            #endregion


            #region Add New Lines

            var nextSequenceNumber =
                existing.Lines.Count == 0
                    ? 1
                    : existing.Lines
                        .Max(x =>
                            x.SequenceNumber)
                      + 1;


            foreach (var submittedLine
                     in submitted.Lines
                         .Where(x =>
                             x.Id <= 0
                             &&
                             !IsCompletelyBlankLine(
                                 x)))
            {
                var newLine =
                    CreateLineFromSubmitted(
                        submittedLine,
                        nextSequenceNumber);


                existing.Lines.Add(
                    newLine);


                nextSequenceNumber++;
            }

            #endregion
        }

        #endregion


        #region Observation Synchronization

        private static void CopyObservationsForCreate(
            PreDispatchInspectionLine source,
            PreDispatchInspectionLine target)
        {
            #region Normal Observations

            var normalObservations =
                source.Observations
                    .Where(x =>
                        !x.IsIntervalReading)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            var intervalObservations =
                source.Observations
                    .Where(x =>
                        x.IsIntervalReading)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();

            #endregion


            #region Default Observation Layout

            if (
                normalObservations.Count == 0
                &&
                intervalObservations.Count == 0
            )
            {
                AddDefaultObservations(
                    target);

                return;
            }

            #endregion


            #region Copy Normal Observations

            var sequenceNumber =
                1;


            foreach (var observation
                     in normalObservations)
            {
                target.Observations.Add(
                    CreateObservation(
                        sequenceNumber,
                        false,
                        observation.Value));


                sequenceNumber++;
            }

            #endregion


            #region Copy Interval Observations

            sequenceNumber =
                1;


            foreach (var observation
                     in intervalObservations)
            {
                target.Observations.Add(
                    CreateObservation(
                        sequenceNumber,
                        true,
                        observation.Value));


                sequenceNumber++;
            }

            #endregion
        }


        private static void SynchronizeObservations(
            PreDispatchInspectionLine existingLine,
            PreDispatchInspectionLine submittedLine)
        {
            #region Active Existing Observations

            var activeExisting =
                existingLine
                    .Observations
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .ToList();

            #endregion


            #region Submitted Existing IDs

            var submittedIds =
                submittedLine
                    .Observations
                    .Where(x =>
                        x.Id > 0)
                    .Select(x =>
                        x.Id)
                    .ToHashSet();

            #endregion


            #region Remove Missing Observations

            foreach (var existingObservation
                     in activeExisting)
            {
                if (
                    submittedIds.Contains(
                        existingObservation.Id)
                )
                {
                    continue;
                }


                existingObservation.IsDeleted =
                    true;


                existingObservation.IsActive =
                    false;


                existingObservation.ModifiedOn =
                    DateTime.UtcNow;


                existingObservation.ModifiedBy =
                    "System";
            }

            #endregion


            #region Update Existing Observations

            foreach (var submittedObservation
                     in submittedLine
                         .Observations
                         .Where(x =>
                             x.Id > 0))
            {
                var existingObservation =
                    activeExisting
                        .FirstOrDefault(x =>
                            x.Id ==
                            submittedObservation.Id);


                if (existingObservation == null)
                {
                    throw new BusinessException(
                        "Invalid PDI Observation.");
                }


                existingObservation.Value =
                    NormalizeOptional(
                        submittedObservation.Value);


                existingObservation.ModifiedOn =
                    DateTime.UtcNow;


                existingObservation.ModifiedBy =
                    "System";
            }

            #endregion


            #region Add New Normal Observations

            var nextNormalSequence =
                existingLine.Observations
                    .Where(x =>
                        !x.IsIntervalReading)
                    .Select(x =>
                        x.SequenceNumber)
                    .DefaultIfEmpty(0)
                    .Max()
                + 1;


            foreach (var submittedObservation
                     in submittedLine
                         .Observations
                         .Where(x =>
                             x.Id <= 0
                             &&
                             !x.IsIntervalReading))
            {
                existingLine
                    .Observations
                    .Add(
                        CreateObservation(
                            nextNormalSequence,
                            false,
                            submittedObservation.Value));


                nextNormalSequence++;
            }

            #endregion


            #region Add New Interval Observations

            var nextIntervalSequence =
                existingLine.Observations
                    .Where(x =>
                        x.IsIntervalReading)
                    .Select(x =>
                        x.SequenceNumber)
                    .DefaultIfEmpty(0)
                    .Max()
                + 1;


            foreach (var submittedObservation
                     in submittedLine
                         .Observations
                         .Where(x =>
                             x.Id <= 0
                             &&
                             x.IsIntervalReading))
            {
                existingLine
                    .Observations
                    .Add(
                        CreateObservation(
                            nextIntervalSequence,
                            true,
                            submittedObservation.Value));


                nextIntervalSequence++;
            }

            #endregion
        }

        #endregion


        #region Default Observation Layout

        private static void AddDefaultObservations(
            PreDispatchInspectionLine line)
        {
            #region Standard Observations

            for (
                var sequenceNumber = 1;
                sequenceNumber <= 7;
                sequenceNumber++
            )
            {
                line.Observations.Add(
                    CreateObservation(
                        sequenceNumber,
                        false,
                        null));
            }

            #endregion


            #region Interval Readings

            for (
                var sequenceNumber = 1;
                sequenceNumber <= 3;
                sequenceNumber++
            )
            {
                line.Observations.Add(
                    CreateObservation(
                        sequenceNumber,
                        true,
                        null));
            }

            #endregion
        }


        private static
            PreDispatchInspectionObservation
            CreateObservation(
                int sequenceNumber,
                bool isIntervalReading,
                string? value)
        {
            return new PreDispatchInspectionObservation
            {
                SequenceNumber =
                    sequenceNumber,

                IsIntervalReading =
                    isIntervalReading,

                Value =
                    NormalizeOptional(
                        value),

                IsActive =
                    true,

                IsDeleted =
                    false,

                CreatedOn =
                    DateTime.UtcNow,

                CreatedBy =
                    "System"
            };
        }

        #endregion


        #region Draft Validation

        private static void ValidateDraftHeader(
            PreDispatchInspection
                preDispatchInspection)
        {
            #region Inspection Date

            if (
                preDispatchInspection
                    .InspectionDate ==
                default
            )
            {
                throw new BusinessException(
                    "Inspection Date is required.");
            }

            #endregion


            #region Quantities

            if (
                preDispatchInspection
                    .InspectionQuantity <= 0m
            )
            {
                throw new BusinessException(
                    "Inspection Quantity must be greater than zero.");
            }


            if (
                preDispatchInspection
                    .AcceptedQuantity < 0m
            )
            {
                throw new BusinessException(
                    "Accepted Quantity cannot be negative.");
            }


            if (
                preDispatchInspection
                    .ReworkQuantity < 0m
            )
            {
                throw new BusinessException(
                    "Rework Quantity cannot be negative.");
            }


            if (
                preDispatchInspection
                    .RejectedQuantity < 0m
            )
            {
                throw new BusinessException(
                    "Rejected Quantity cannot be negative.");
            }


            var resultQuantity =
                preDispatchInspection
                    .AcceptedQuantity
                +
                preDispatchInspection
                    .ReworkQuantity
                +
                preDispatchInspection
                    .RejectedQuantity;


            if (
                resultQuantity >
                preDispatchInspection
                    .InspectionQuantity
            )
            {
                throw new BusinessException(
                    "Accepted + Rework + Rejected Quantity cannot exceed Inspection Quantity.");
            }

            #endregion


            #region Invoice

            if (
                preDispatchInspection
                    .InvoiceQuantity
                    .HasValue
                &&
                preDispatchInspection
                    .InvoiceQuantity
                    .Value < 0m
            )
            {
                throw new BusinessException(
                    "Invoice Quantity cannot be negative.");
            }


            if (
                preDispatchInspection
                    .InvoiceNumber?
                    .Length > 100
            )
            {
                throw new BusinessException(
                    "Invoice Number cannot exceed 100 characters.");
            }

            #endregion


            #region Remarks

            if (
                preDispatchInspection
                    .SupplierRemarks?
                    .Length > 1000
            )
            {
                throw new BusinessException(
                    "Supplier Remarks cannot exceed 1000 characters.");
            }


            if (
                preDispatchInspection
                    .InspectionRemarks?
                    .Length > 2000
            )
            {
                throw new BusinessException(
                    "Inspection Remarks cannot exceed 2000 characters.");
            }

            #endregion


            #region Approval

            if (
                preDispatchInspection
                    .InspectedBy?
                    .Length > 150
            )
            {
                throw new BusinessException(
                    "Inspected By cannot exceed 150 characters.");
            }


            if (
                preDispatchInspection
                    .ReviewedBy?
                    .Length > 150
            )
            {
                throw new BusinessException(
                    "Reviewed By cannot exceed 150 characters.");
            }

            #endregion
        }


        private static void ValidateInspectionQuantity(
            decimal inspectionQuantity,
            decimal remainingQuantity)
        {
            if (inspectionQuantity <= 0m)
            {
                throw new BusinessException(
                    "Inspection Quantity must be greater than zero.");
            }


            if (
                inspectionQuantity >
                remainingQuantity
            )
            {
                throw new BusinessException(
                    $"Inspection Quantity cannot exceed remaining quantity {remainingQuantity:0.###}.");
            }
        }

        #endregion


        #region Final Inspection Validation

        private static void ValidateFinalInspection(
            PreDispatchInspection
                preDispatchInspection)
        {
            #region Quantity Completion

            var completedQuantity =
                preDispatchInspection
                    .AcceptedQuantity
                +
                preDispatchInspection
                    .ReworkQuantity
                +
                preDispatchInspection
                    .RejectedQuantity;


            if (
                completedQuantity !=
                preDispatchInspection
                    .InspectionQuantity
            )
            {
                throw new BusinessException(
                    "Accepted + Rework + Rejected Quantity must equal Inspection Quantity before Finalization.");
            }

            #endregion


            #region Inspection Lines

            var activeLines =
                preDispatchInspection
                    .Lines
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (activeLines.Count == 0)
            {
                throw new BusinessException(
                    "At least one Inspection Parameter is required before Finalization.");
            }


            foreach (var line
                     in activeLines)
            {
                ValidateFinalLine(
                    line);
            }

            #endregion


            #region Failed Line Quantity Validation

            var hasFailedLine =
                activeLines.Any(x =>
                    x.Result ==
                    PreDispatchInspectionLineResult.Fail);


            if (
                hasFailedLine
                &&
                preDispatchInspection
                    .ReworkQuantity <= 0m
                &&
                preDispatchInspection
                    .RejectedQuantity <= 0m
            )
            {
                throw new BusinessException(
                    "A failed Inspection Parameter requires Rework or Rejected Quantity.");
            }

            #endregion


            #region Inspection Approval

            if (
                string.IsNullOrWhiteSpace(
                    preDispatchInspection
                        .InspectedBy)
            )
            {
                throw new BusinessException(
                    "Inspected By is required before Finalization.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    preDispatchInspection
                        .ReviewedBy)
            )
            {
                throw new BusinessException(
                    "Reviewed / Approved By is required before Finalization.");
            }

            #endregion
        }


        private static void ValidateFinalLine(
            PreDispatchInspectionLine line)
        {
            #region Parameter

            if (
                string.IsNullOrWhiteSpace(
                    line.Parameter)
            )
            {
                throw new BusinessException(
                    $"Inspection Parameter is required for Sequence {line.SequenceNumber}.");
            }


            if (line.Parameter.Length > 250)
            {
                throw new BusinessException(
                    $"Inspection Parameter at Sequence {line.SequenceNumber} cannot exceed 250 characters.");
            }

            #endregion


            #region Specification

            if (
                string.IsNullOrWhiteSpace(
                    line.Specification)
            )
            {
                throw new BusinessException(
                    $"Specification is required for '{line.Parameter}'.");
            }


            if (line.Specification.Length > 500)
            {
                throw new BusinessException(
                    $"Specification for '{line.Parameter}' cannot exceed 500 characters.");
            }

            #endregion


            #region Result

            if (
                line.Result ==
                PreDispatchInspectionLineResult.Pending
            )
            {
                throw new BusinessException(
                    $"Inspection Result is required for '{line.Parameter}'.");
            }

            #endregion


            #region Inspection Method

            if (
                line.Result !=
                    PreDispatchInspectionLineResult.NotApplicable
                &&
                string.IsNullOrWhiteSpace(
                    line.InspectionMethod)
            )
            {
                throw new BusinessException(
                    $"Inspection Method is required for '{line.Parameter}'.");
            }

            #endregion


            #region Observations

            if (
                line.Result !=
                PreDispatchInspectionLineResult.NotApplicable
            )
            {
                var hasObservation =
                    line.Observations
                        .Any(x =>
                            !x.IsDeleted
                            &&
                            x.IsActive
                            &&
                            !string.IsNullOrWhiteSpace(
                                x.Value));


                if (!hasObservation)
                {
                    throw new BusinessException(
                        $"At least one Observation is required for '{line.Parameter}'.");
                }
            }

            #endregion
        }

        #endregion


        #region Overall Result

        private static PreDispatchInspectionResult
            CalculateOverallResult(
                PreDispatchInspection
                    preDispatchInspection)
        {
            #region Pass

            if (
                preDispatchInspection
                    .AcceptedQuantity ==
                preDispatchInspection
                    .InspectionQuantity
                &&
                preDispatchInspection
                    .ReworkQuantity == 0m
                &&
                preDispatchInspection
                    .RejectedQuantity == 0m
            )
            {
                return
                    PreDispatchInspectionResult.Pass;
            }

            #endregion


            #region Fail

            if (
                preDispatchInspection
                    .AcceptedQuantity <= 0m
            )
            {
                return
                    PreDispatchInspectionResult.Fail;
            }

            #endregion


            #region Partial

            return
                PreDispatchInspectionResult.Partial;

            #endregion
        }

        #endregion


        #region PDI Code

        private async Task<string>
            GenerateCodeAsync()
        {
            var today =
                DateTime.Today;


            var fiscalYear =
                GetFiscalYear(
                    today);


            var prefix =
                $"AI/PDI/{fiscalYear}/";


            var lastCode =
                await _repository
                    .GetLastCodeAsync(
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
                    "Unable to generate PDI Report Code.");
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


        #region Specification Helpers

        private static string BuildSpecificationValue(
            ItemSpecification
                itemSpecification)
        {
            var value =
                itemSpecification
                    .SpecificationValue?
                    .Trim()
                ??
                string.Empty;


            var uomName =
                itemSpecification
                    .Uom?
                    .UomName;


            if (string.IsNullOrWhiteSpace(
                uomName))
            {
                return value;
            }


            return string.IsNullOrWhiteSpace(
                value)
                ? uomName.Trim()
                : $"{value} {uomName.Trim()}";
        }

        #endregion


        #region Line Helpers

        private static bool IsCompletelyBlankLine(
            PreDispatchInspectionLine line)
        {
            var hasObservation =
                line.Observations
                    .Any(x =>
                        !string.IsNullOrWhiteSpace(
                            x.Value));


            return
                string.IsNullOrWhiteSpace(
                    line.Parameter)
                &&
                string.IsNullOrWhiteSpace(
                    line.Specification)
                &&
                string.IsNullOrWhiteSpace(
                    line.InspectionMethod)
                &&
                string.IsNullOrWhiteSpace(
                    line.Remarks)
                &&
                !hasObservation;
        }


        private static
            PreDispatchInspectionLineResult
            NormalizeLineResult(
                PreDispatchInspectionLineResult result)
        {
            return Enum.IsDefined(
                typeof(
                    PreDispatchInspectionLineResult),
                result)
                ? result
                : PreDispatchInspectionLineResult.Pending;
        }

        #endregion


        #region Normalization

        private static string NormalizeRequired(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? string.Empty
                : value.Trim();
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