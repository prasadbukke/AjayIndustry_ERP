using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using System.Reflection;
using System.Text.Json;

namespace AjayIndustriesERP.Application.Services
{
    public class PurchaseInvoiceService
        : IPurchaseInvoiceService
    {
        #region Fields / Constants

        private readonly IPurchaseInvoiceRepository
            _repository;

        private const string SystemUser =
            "System";

        #endregion


        #region Constructor

        public PurchaseInvoiceService(
            IPurchaseInvoiceRepository repository)
        {
            _repository =
                repository;
        }

        #endregion


        // =====================================================
        // READ
        // =====================================================

        #region Read

        public async Task<PurchaseInvoice?>
            GetByIdAsync(
                int id)
        {
            return await _repository
                .GetByIdAsync(
                    id);
        }


        public async Task<PagedResult<PurchaseInvoice>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            NormalizePaging(
                ref pageNumber,
                ref pageSize);


            return await _repository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }


        /*
         * Search / Filter Purchase Invoices.
         *
         * Supported Filters:
         *
         * searchText:
         * - ERP Purchase Invoice Code
         * - Supplier Invoice Number
         * - Supplier Name
         * - Purchase Order Code
         *
         * purchaseInvoiceDate:
         * - ERP Purchase Invoice Date
         *
         * supplierInvoiceDate:
         * - Supplier's Invoice Date
         *
         * All filters are optional.
         */
        public async Task<PagedResult<PurchaseInvoice>>
            SearchPagedAsync(
                string? searchText,
                DateTime? purchaseInvoiceDate,
                DateTime? supplierInvoiceDate,
                int pageNumber,
                int pageSize)
        {
            NormalizePaging(
                ref pageNumber,
                ref pageSize);


            searchText =
                searchText?.Trim();


            /*
             * If no filter is supplied,
             * use normal paged listing.
             */
            if (
                string.IsNullOrWhiteSpace(
                    searchText) &&
                !purchaseInvoiceDate.HasValue &&
                !supplierInvoiceDate.HasValue
            )
            {
                return await GetPagedAsync(
                    pageNumber,
                    pageSize);
            }


            return await _repository
                .SearchPagedAsync(
                    searchText,
                    purchaseInvoiceDate,
                    supplierInvoiceDate,
                    pageNumber,
                    pageSize);
        }

        #endregion


        // =====================================================
        // PURCHASE ORDER SOURCE
        // =====================================================

        #region Purchase Order Source

        public async Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync()
        {
            var purchaseOrders =
                await _repository
                    .GetPurchaseOrdersForInvoiceAsync();


            var result =
                new List<PurchaseOrder>();


            foreach (var purchaseOrder
                in purchaseOrders)
            {
                if (purchaseOrder.Status ==
                    PurchaseOrderStatus.Cancelled)
                {
                    continue;
                }


                var availableItems =
                    await GetAvailableGoodsReceiptItemsAsync(
                        purchaseOrder.Id);


                if (availableItems.Count > 0)
                {
                    result.Add(
                        purchaseOrder);
                }
            }


            return result;
        }


        public async Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId)
        {
            return await _repository
                .GetPurchaseOrderForInvoiceAsync(
                    purchaseOrderId);
        }

        #endregion


        // =====================================================
        // GRN AVAILABILITY
        // =====================================================

        #region GRN Availability

        public async Task<List<GoodsReceiptNoteItem>>
            GetAvailableGoodsReceiptItemsAsync(
                int purchaseOrderId,
                int? excludePurchaseInvoiceId = null)
        {
            var sourceItems =
                await _repository
                    .GetReceivedGoodsReceiptItemsForInvoiceAsync(
                        purchaseOrderId);


            var result =
                new List<GoodsReceiptNoteItem>();


            foreach (var sourceItem
                in sourceItems)
            {
                var remaining =
                    await GetRemainingPurchaseInvoiceQuantityAsync(
                        sourceItem.Id,
                        excludePurchaseInvoiceId);


                if (remaining > 0m)
                {
                    result.Add(
                        sourceItem);
                }
            }


            return result;
        }


        public async Task<decimal>
            GetRemainingPurchaseInvoiceQuantityAsync(
                int goodsReceiptNoteItemId,
                int? excludePurchaseInvoiceId = null)
        {
            var sourceItem =
                await _repository
                    .GetGoodsReceiptNoteItemForInvoiceAsync(
                        goodsReceiptNoteItemId);


            if (sourceItem == null)
            {
                return 0m;
            }


            var allocated =
                await _repository
                    .GetAllocatedPurchaseInvoiceQuantityAsync(
                        goodsReceiptNoteItemId,
                        excludePurchaseInvoiceId);


            var remaining =
                sourceItem.ReceivedQuantity -
                allocated;


            return remaining > 0m
                ? remaining
                : 0m;
        }

        #endregion


        // =====================================================
        // PREPARE CREATE DRAFT
        // =====================================================

        #region Prepare Draft

        public async Task<PurchaseInvoice>
            PrepareDraftAsync(
                int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
            {
                throw new BusinessException(
                    "Please select a valid Purchase Order.");
            }


            var purchaseOrder =
                await _repository
                    .GetPurchaseOrderForInvoiceAsync(
                        purchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            ValidatePurchaseOrder(
                purchaseOrder);


            if (purchaseOrder.Supplier == null)
            {
                throw new BusinessException(
                    "Supplier information is not available for this Purchase Order.");
            }


            if (purchaseOrder.Company == null)
            {
                throw new BusinessException(
                    "Company information is not available for this Purchase Order.");
            }


            var sourceItems =
                await GetAvailableGoodsReceiptItemsAsync(
                    purchaseOrderId);


            if (sourceItems.Count == 0)
            {
                throw new BusinessException(
                    "No received GRN quantity is available for Purchase Invoice.");
            }


            var supplier =
                purchaseOrder.Supplier;


            var company =
                purchaseOrder.Company;


            var creditDays =
                supplier.PaymentTermsDays;


            string? paymentTerms =
                purchaseOrder.PaymentTerms;


            if (creditDays.HasValue)
            {
                paymentTerms =
                    $"{creditDays.Value} Days";
            }


            var purchaseInvoice =
                new PurchaseInvoice
                {
                    PurchaseOrderId =
                        purchaseOrder.Id,

                    PurchaseOrderCode =
                        purchaseOrder.Code,


                    PurchaseInvoiceDate =
                        DateTime.Today,

                    SupplierInvoiceDate =
                        DateTime.Today,

                    SupplierInvoiceNumber =
                        string.Empty,


                    SupplierId =
                        supplier.SupplierId,

                    SupplierName =
                        supplier.SupplierName,

                    SupplierSnapshotJson =
                        SerializeScalarSnapshot(
                            supplier),


                    CompanyId =
                        company.CompanyId,

                    CompanyName =
                        company.CompanyName,

                    CompanySnapshotJson =
                        SerializeScalarSnapshot(
                            company),


                    PaymentTerms =
                        paymentTerms,

                    CreditDays =
                        creditDays,

                    DueDate =
                        CalculateDueDate(
                            DateTime.Today,
                            creditDays),


                    PlaceOfSupply =
                        company.State,

                    IsInterState =
                        IsInterStateTransaction(
                            company.State,
                            supplier.State),


                    TransportCharges =
                        purchaseOrder.TransportCharges,

                    OtherCharges =
                        purchaseOrder.OtherCharges,

                    RoundOffAmount =
                        0m,


                    Status =
                        PurchaseInvoiceStatus.Draft,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };


            var sequenceNumber =
                1;


            foreach (var sourceItem
                in sourceItems)
            {
                var remainingQuantity =
                    await GetRemainingPurchaseInvoiceQuantityAsync(
                        sourceItem.Id);


                if (remainingQuantity <= 0m)
                {
                    continue;
                }


                /*
                 * IMPORTANT:
                 *
                 * Purchase Order Rate is NOT automatically
                 * used for Supplier Purchase Invoice.
                 *
                 * Actual Supplier Invoice Rate must be
                 * entered manually by user.
                 */
                var preparedItem =
                    CreateTrustedSourceSnapshot(
                        purchaseInvoice,
                        sourceItem,
                        remainingQuantity,
                        purchaseInvoice.IsInterState,
                        0m);


                preparedItem.SequenceNumber =
                    sequenceNumber++;


                preparedItem.IsActive =
                    true;

                preparedItem.IsDeleted =
                    false;


                purchaseInvoice.Items.Add(
                    preparedItem);
            }


            CalculateHeaderTotals(
                purchaseInvoice);


            return purchaseInvoice;
        }

        #endregion


        // =====================================================
        // CREATE
        // =====================================================

        #region Create

        public async Task<PurchaseInvoice>
            CreateAsync(
                PurchaseInvoice submitted)
        {
            ValidateSubmittedHeader(
                submitted);


            var purchaseOrder =
                await _repository
                    .GetPurchaseOrderForInvoiceAsync(
                        submitted.PurchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            ValidatePurchaseOrder(
                purchaseOrder);


            if (purchaseOrder.Supplier == null)
            {
                throw new BusinessException(
                    "Supplier information is not available.");
            }


            if (purchaseOrder.Company == null)
            {
                throw new BusinessException(
                    "Company information is not available.");
            }


            var supplier =
                purchaseOrder.Supplier;


            var company =
                purchaseOrder.Company;


            var supplierInvoiceNumber =
                submitted.SupplierInvoiceNumber.Trim();


            // =================================================
            // SUPPLIER INVOICE NUMBER DUPLICATE VALIDATION
            // =================================================

            var duplicateExists =
                await _repository
                    .SupplierInvoiceNumberExistsAsync(
                        supplier.SupplierId,
                        supplierInvoiceNumber,
                        null);


            if (duplicateExists)
            {
                throw new BusinessException(
                    $"Supplier Invoice Number '{supplierInvoiceNumber}' already exists for {supplier.SupplierName}.");
            }


            // =================================================
            // PAYMENT TERMS
            // =================================================

            var creditDays =
                supplier.PaymentTermsDays;


            string? paymentTerms =
                purchaseOrder.PaymentTerms;


            if (creditDays.HasValue)
            {
                paymentTerms =
                    $"{creditDays.Value} Days";
            }


            // =================================================
            // BUILD TRUSTED PURCHASE INVOICE HEADER
            // =================================================

            var purchaseInvoice =
                new PurchaseInvoice
                {
                    // -----------------------------------------
                    // ERP Purchase Invoice
                    // -----------------------------------------

                    Code =
                        await GenerateNextCodeAsync(
                            submitted.PurchaseInvoiceDate),

                    PurchaseInvoiceDate =
                        submitted.PurchaseInvoiceDate,

                    Status =
                        PurchaseInvoiceStatus.Draft,


                    // -----------------------------------------
                    // Supplier Invoice
                    // -----------------------------------------

                    SupplierInvoiceNumber =
                        supplierInvoiceNumber,

                    SupplierInvoiceDate =
                        submitted.SupplierInvoiceDate,


                    // -----------------------------------------
                    // Supplier Invoice PDF
                    //
                    // Physical file is handled by Web layer.
                    // Service stores only file metadata.
                    // -----------------------------------------

                    SupplierInvoicePdfPath =
                        NormalizeNullableText(
                            submitted.SupplierInvoicePdfPath),

                    SupplierInvoicePdfOriginalName =
                        NormalizeNullableText(
                            submitted.SupplierInvoicePdfOriginalName),

                    SupplierInvoicePdfUploadedOn =
                        submitted.SupplierInvoicePdfUploadedOn,


                    // -----------------------------------------
                    // Purchase Order
                    // -----------------------------------------

                    PurchaseOrderId =
                        purchaseOrder.Id,

                    PurchaseOrderCode =
                        purchaseOrder.Code,


                    // -----------------------------------------
                    // Supplier Snapshot
                    // -----------------------------------------

                    SupplierId =
                        supplier.SupplierId,

                    SupplierName =
                        supplier.SupplierName,

                    SupplierSnapshotJson =
                        SerializeScalarSnapshot(
                            supplier),


                    // -----------------------------------------
                    // Company Snapshot
                    // -----------------------------------------

                    CompanyId =
                        company.CompanyId,

                    CompanyName =
                        company.CompanyName,

                    CompanySnapshotJson =
                        SerializeScalarSnapshot(
                            company),


                    // -----------------------------------------
                    // Payment Terms
                    // -----------------------------------------

                    PaymentTerms =
                        paymentTerms,

                    CreditDays =
                        creditDays,

                    DueDate =
                        CalculateDueDate(
                            submitted.SupplierInvoiceDate,
                            creditDays),


                    // -----------------------------------------
                    // GST
                    // -----------------------------------------

                    PlaceOfSupply =
                        company.State,

                    IsInterState =
                        IsInterStateTransaction(
                            company.State,
                            supplier.State),


                    // -----------------------------------------
                    // Additional Charges
                    // -----------------------------------------

                    TransportCharges =
                        submitted.TransportCharges,

                    OtherCharges =
                        submitted.OtherCharges,

                    RoundOffAmount =
                        submitted.RoundOffAmount,


                    // -----------------------------------------
                    // Remarks
                    // -----------------------------------------

                    Remarks =
                        submitted.Remarks?.Trim(),


                    // -----------------------------------------
                    // Audit
                    // -----------------------------------------

                    CreatedOn =
                        DateTime.Now,

                    CreatedBy =
                        SystemUser,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };


            // =================================================
            // TRUSTED ITEM PREPARATION
            // =================================================

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    purchaseInvoice,
                    submitted.Items,
                    excludePurchaseInvoiceId:
                        null);


            foreach (var item
                in preparedItems)
            {
                purchaseInvoice.Items.Add(
                    item);
            }


            // =================================================
            // TOTALS
            // =================================================

            CalculateHeaderTotals(
                purchaseInvoice);


            // =================================================
            // SAVE
            // =================================================

            await _repository
                .AddAsync(
                    purchaseInvoice);


            return purchaseInvoice;
        }

        #endregion


        // =====================================================
        // UPDATE
        // =====================================================

        #region Update

        public async Task<PurchaseInvoice>
            UpdateAsync(
                PurchaseInvoice submitted)
        {
            var existing =
                await _repository
                    .GetForUpdateAsync(
                        submitted.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Purchase Invoice not found.");
            }


            if (existing.IsDeleted ||
                !existing.IsActive)
            {
                throw new BusinessException(
                    "Deleted Purchase Invoice cannot be edited.");
            }


            /*
             * Financial / item editing is allowed only
             * while Purchase Invoice is Draft.
             */
            if (existing.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Invoice can be edited.");
            }


            ValidateSubmittedHeader(
                submitted);


            // =================================================
            // PURCHASE ORDER CANNOT CHANGE
            // =================================================

            if (existing.PurchaseOrderId !=
                submitted.PurchaseOrderId)
            {
                throw new BusinessException(
                    "Purchase Order cannot be changed after Purchase Invoice creation.");
            }


            EnsureInvoiceDateMatchesCodeFinancialYear(
                existing.Code,
                submitted.PurchaseInvoiceDate);


            var purchaseOrder =
                await _repository
                    .GetPurchaseOrderForInvoiceAsync(
                        existing.PurchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            ValidatePurchaseOrder(
                purchaseOrder);


            if (purchaseOrder.Supplier == null)
            {
                throw new BusinessException(
                    "Supplier information is not available.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    existing.SupplierSnapshotJson) ||
                string.IsNullOrWhiteSpace(
                    existing.CompanySnapshotJson)
            )
            {
                throw new BusinessException(
                    "Purchase Invoice snapshot information is missing.");
            }


            if (existing.SupplierId !=
                purchaseOrder.Supplier.SupplierId)
            {
                throw new BusinessException(
                    "Purchase Order Supplier does not match Purchase Invoice Supplier.");
            }


            // =================================================
            // SUPPLIER INVOICE NUMBER VALIDATION
            // =================================================

            var supplierInvoiceNumber =
                submitted.SupplierInvoiceNumber.Trim();


            var duplicateExists =
                await _repository
                    .SupplierInvoiceNumberExistsAsync(
                        existing.SupplierId,
                        supplierInvoiceNumber,
                        existing.Id);


            if (duplicateExists)
            {
                throw new BusinessException(
                    $"Supplier Invoice Number '{supplierInvoiceNumber}' already exists for this Supplier.");
            }


            var supplierState =
                GetSnapshotString(
                    existing.SupplierSnapshotJson,
                    "State");


            var companyState =
                GetSnapshotString(
                    existing.CompanySnapshotJson,
                    "State");


            // =================================================
            // HEADER
            // =================================================

            existing.PurchaseInvoiceDate =
                submitted.PurchaseInvoiceDate;


            existing.SupplierInvoiceNumber =
                supplierInvoiceNumber;


            existing.SupplierInvoiceDate =
                submitted.SupplierInvoiceDate;


            // =================================================
            // SUPPLIER INVOICE PDF
            //
            // Controller sends:
            //
            // - old metadata if PDF is not changed
            // - new metadata if replacement PDF uploaded
            // =================================================

            existing.SupplierInvoicePdfPath =
                NormalizeNullableText(
                    submitted.SupplierInvoicePdfPath);


            existing.SupplierInvoicePdfOriginalName =
                NormalizeNullableText(
                    submitted.SupplierInvoicePdfOriginalName);


            existing.SupplierInvoicePdfUploadedOn =
                submitted.SupplierInvoicePdfUploadedOn;


            // =================================================
            // DUE DATE
            // =================================================

            existing.DueDate =
                CalculateDueDate(
                    submitted.SupplierInvoiceDate,
                    existing.CreditDays);


            // =================================================
            // GST
            // =================================================

            existing.PlaceOfSupply =
                companyState;


            existing.IsInterState =
                IsInterStateTransaction(
                    companyState,
                    supplierState);


            // =================================================
            // CHARGES
            // =================================================

            existing.TransportCharges =
                submitted.TransportCharges;


            existing.OtherCharges =
                submitted.OtherCharges;


            existing.RoundOffAmount =
                submitted.RoundOffAmount;


            // =================================================
            // REMARKS
            // =================================================

            existing.Remarks =
                submitted.Remarks?.Trim();


            // =================================================
            // TRUSTED ITEMS
            // =================================================

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    existing,
                    submitted.Items,
                    existing.Id);


            SyncItems(
                existing,
                preparedItems);


            // =================================================
            // TOTALS
            // =================================================

            CalculateHeaderTotals(
                existing);


            // =================================================
            // AUDIT
            // =================================================

            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            await _repository
                .UpdateAsync(
                    existing);


            return existing;
        }

        #endregion


        // =====================================================
        // APPROVE / FINALIZE
        // =====================================================

        #region Finalize

        /*
         * UI button may say "Approve".
         *
         * Internally workflow remains:
         *
         * Draft
         *   ↓
         * Finalized
         *
         * No separate Approved enum/status is needed.
         */
        public async Task<PurchaseInvoice>
            FinalizeAsync(
                int id)
        {
            var existing =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Purchase Invoice not found.");
            }


            if (existing.IsDeleted ||
                !existing.IsActive)
            {
                throw new BusinessException(
                    "Deleted Purchase Invoice cannot be approved.");
            }


            if (existing.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Invoice can be approved.");
            }


            if (
                string.IsNullOrWhiteSpace(
                    existing.SupplierSnapshotJson) ||
                string.IsNullOrWhiteSpace(
                    existing.CompanySnapshotJson)
            )
            {
                throw new BusinessException(
                    "Purchase Invoice snapshot information is missing.");
            }


            var activeItems =
                existing.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (activeItems.Count == 0)
            {
                throw new BusinessException(
                    "Purchase Invoice must contain at least one item.");
            }


            var purchaseOrder =
                await _repository
                    .GetPurchaseOrderForInvoiceAsync(
                        existing.PurchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            ValidatePurchaseOrder(
                purchaseOrder);


            if (
                purchaseOrder.Supplier == null ||
                purchaseOrder.Supplier.SupplierId !=
                existing.SupplierId
            )
            {
                throw new BusinessException(
                    "Purchase Order Supplier does not match Purchase Invoice Supplier.");
            }


            // =================================================
            // DUPLICATE SUPPLIER INVOICE NUMBER
            // =================================================

            var duplicateExists =
                await _repository
                    .SupplierInvoiceNumberExistsAsync(
                        existing.SupplierId,
                        existing.SupplierInvoiceNumber,
                        existing.Id);


            if (duplicateExists)
            {
                throw new BusinessException(
                    $"Supplier Invoice Number '{existing.SupplierInvoiceNumber}' already exists for this Supplier.");
            }


            // =================================================
            // BUILD TRANSACTION VALUES FOR REVALIDATION
            // =================================================

            var submittedItems =
                activeItems
                    .Select(x =>
                        new PurchaseInvoiceItem
                        {
                            GoodsReceiptNoteItemId =
                                x.GoodsReceiptNoteItemId,

                            PurchaseInvoiceQuantity =
                                x.PurchaseInvoiceQuantity,

                            Rate =
                                x.Rate
                        })
                    .ToList();


            var supplierState =
                GetSnapshotString(
                    existing.SupplierSnapshotJson,
                    "State");


            var companyState =
                GetSnapshotString(
                    existing.CompanySnapshotJson,
                    "State");


            existing.IsInterState =
                IsInterStateTransaction(
                    companyState,
                    supplierState);


            existing.PlaceOfSupply =
                companyState;


            // =================================================
            // FINAL QUANTITY REVALIDATION
            // =================================================

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    existing,
                    submittedItems,
                    existing.Id);


            SyncItems(
                existing,
                preparedItems);


            CalculateHeaderTotals(
                existing);


            // =================================================
            // APPROVE / FINALIZE
            // =================================================

            existing.Status =
                PurchaseInvoiceStatus.Finalized;


            existing.FinalizedOn =
                DateTime.Now;


            existing.FinalizedBy =
                SystemUser;


            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            await _repository
                .UpdateAsync(
                    existing);


            return existing;
        }

        #endregion


        // =====================================================
        // DELETE
        // =====================================================

        #region Delete

        /*
         * IMPORTANT BUSINESS RULE:
         *
         * Delete is allowed for:
         *
         * - Draft Purchase Invoice
         * - Finalized Purchase Invoice
         *
         * This is SOFT DELETE only.
         *
         * Therefore:
         * - Record remains in database.
         * - Supplier PDF remains on disk.
         * - Original status remains unchanged.
         * - Record can later be restored.
         *
         * Deleted Invoice no longer reserves GRN quantity
         * because repository reservation query considers
         * only active / non-deleted Purchase Invoices.
         */
        public async Task DeleteAsync(
            int id)
        {
            var existing =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Purchase Invoice not found.");
            }


            if (existing.IsDeleted)
            {
                throw new BusinessException(
                    "Purchase Invoice is already deleted.");
            }


            // =================================================
            // NO DRAFT-ONLY RESTRICTION HERE
            //
            // Both Draft and Finalized may be soft deleted.
            // =================================================


            existing.IsDeleted =
                true;


            existing.IsActive =
                false;


            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            /*
             * IMPORTANT:
             *
             * Do NOT:
             * - change Status
             * - remove items
             * - remove PDF metadata
             *
             * Restore must bring back exactly the same
             * business document.
             */
            await _repository
                .UpdateAsync(
                    existing);
        }

        #endregion


        // =====================================================
        // DELETED LIST
        // =====================================================

        #region Deleted

        public async Task<List<PurchaseInvoice>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }

        #endregion


        // =====================================================
        // RESTORE
        // =====================================================

        #region Restore

        /*
         * Restore is allowed for deleted:
         *
         * - Draft Purchase Invoice
         * - Finalized Purchase Invoice
         *
         * Original Status is preserved.
         *
         * Example:
         *
         * Finalized Invoice
         *      ↓ Delete
         * Deleted + Status Finalized
         *      ↓ Restore
         * Active + Status Finalized
         */
        public async Task RestoreAsync(
            int id)
        {
            var existing =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Deleted Purchase Invoice not found.");
            }


            // =================================================
            // VALIDATE ORIGINAL STATUS
            // =================================================

            if (
                existing.Status !=
                    PurchaseInvoiceStatus.Draft &&
                existing.Status !=
                    PurchaseInvoiceStatus.Finalized
            )
            {
                throw new BusinessException(
                    "This Purchase Invoice cannot be restored.");
            }


            // =================================================
            // SNAPSHOT VALIDATION
            // =================================================

            if (
                string.IsNullOrWhiteSpace(
                    existing.SupplierSnapshotJson) ||
                string.IsNullOrWhiteSpace(
                    existing.CompanySnapshotJson)
            )
            {
                throw new BusinessException(
                    "Purchase Invoice snapshot information is missing.");
            }


            // =================================================
            // SUPPLIER INVOICE NUMBER REVALIDATION
            //
            // While this invoice was deleted, another active
            // invoice may have used the same Supplier Invoice
            // Number.
            // =================================================

            var duplicateExists =
                await _repository
                    .SupplierInvoiceNumberExistsAsync(
                        existing.SupplierId,
                        existing.SupplierInvoiceNumber,
                        existing.Id);


            if (duplicateExists)
            {
                throw new BusinessException(
                    $"Supplier Invoice Number '{existing.SupplierInvoiceNumber}' is already being used.");
            }


            // =================================================
            // ACTIVE ITEMS
            // =================================================

            var existingActiveItems =
                existing.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (existingActiveItems.Count == 0)
            {
                throw new BusinessException(
                    "Purchase Invoice has no items to restore.");
            }


            // =================================================
            // PRESERVE QUANTITY + ACTUAL SUPPLIER RATE
            // =================================================

            var submittedItems =
                existingActiveItems
                    .Select(x =>
                        new PurchaseInvoiceItem
                        {
                            GoodsReceiptNoteItemId =
                                x.GoodsReceiptNoteItemId,

                            PurchaseInvoiceQuantity =
                                x.PurchaseInvoiceQuantity,

                            Rate =
                                x.Rate
                        })
                    .ToList();


            var supplierState =
                GetSnapshotString(
                    existing.SupplierSnapshotJson,
                    "State");


            var companyState =
                GetSnapshotString(
                    existing.CompanySnapshotJson,
                    "State");


            existing.IsInterState =
                IsInterStateTransaction(
                    companyState,
                    supplierState);


            existing.PlaceOfSupply =
                companyState;


            // =================================================
            // GRN QUANTITY REVALIDATION
            //
            // Deleted invoice does not reserve quantity.
            //
            // During the deleted period that GRN quantity may
            // have been consumed by another Purchase Invoice.
            //
            // Therefore Restore MUST validate availability
            // again.
            // =================================================

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    existing,
                    submittedItems,
                    existing.Id);


            // =================================================
            // RESTORE PARENT
            // =================================================

            existing.IsDeleted =
                false;


            existing.IsActive =
                true;


            // =================================================
            // RESTORE / SYNC ITEMS
            // =================================================

            SyncItems(
                existing,
                preparedItems);


            // =================================================
            // RECALCULATE TOTALS
            // =================================================

            CalculateHeaderTotals(
                existing);


            // =================================================
            // STATUS IS INTENTIONALLY NOT CHANGED
            //
            // Draft remains Draft.
            // Finalized remains Finalized.
            // =================================================


            // =================================================
            // AUDIT
            // =================================================

            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            await _repository
                .UpdateAsync(
                    existing);
        }

        #endregion


        // =====================================================
        // TRUSTED ITEM PREPARATION
        // =====================================================

        #region Prepare Trusted Items

        private async Task<List<PurchaseInvoiceItem>>
            PrepareTrustedItemsAsync(
                PurchaseInvoice purchaseInvoice,
                IEnumerable<PurchaseInvoiceItem> submittedItems,
                int? excludePurchaseInvoiceId)
        {
            var submittedList =
                submittedItems?
                    .ToList()
                ??
                new List<PurchaseInvoiceItem>();


            if (submittedList.Count == 0)
            {
                throw new BusinessException(
                    "Please select at least one received GRN item.");
            }


            // =================================================
            // SAME GRN ITEM CANNOT APPEAR TWICE
            // =================================================

            var duplicateSource =
                submittedList
                    .GroupBy(x =>
                        x.GoodsReceiptNoteItemId)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateSource != null)
            {
                throw new BusinessException(
                    "The same GRN item cannot be added more than once.");
            }


            var result =
                new List<PurchaseInvoiceItem>();


            var sequenceNumber =
                1;


            foreach (var submittedItem
                in submittedList)
            {
                // =================================================
                // BASIC INPUT VALIDATION
                // =================================================

                if (submittedItem.GoodsReceiptNoteItemId <= 0)
                {
                    throw new BusinessException(
                        "Invalid GRN item selected.");
                }


                if (submittedItem.PurchaseInvoiceQuantity <= 0m)
                {
                    throw new BusinessException(
                        "Purchase Invoice Quantity must be greater than zero.");
                }


                /*
                 * Rate comes from actual Supplier Invoice.
                 */
                if (submittedItem.Rate <= 0m)
                {
                    throw new BusinessException(
                        "Rate must be greater than zero for all selected Purchase Invoice items.");
                }


                // =================================================
                // RELOAD TRUSTED GRN SOURCE
                // =================================================

                var sourceItem =
                    await _repository
                        .GetGoodsReceiptNoteItemForInvoiceAsync(
                            submittedItem.GoodsReceiptNoteItemId);


                if (sourceItem == null)
                {
                    throw new BusinessException(
                        "Selected GRN item was not found.");
                }


                if (sourceItem.ReceivedQuantity <= 0m)
                {
                    throw new BusinessException(
                        $"GRN item '{sourceItem.ItemName}' has no received quantity.");
                }


                if (sourceItem.GoodsReceiptNote == null)
                {
                    throw new BusinessException(
                        "GRN information is not available.");
                }


                if (sourceItem.GoodsReceiptNote.PurchaseOrderId !=
                    purchaseInvoice.PurchaseOrderId)
                {
                    throw new BusinessException(
                        "Selected GRN item does not belong to the selected Purchase Order.");
                }


                // =================================================
                // AVAILABLE QUANTITY
                // =================================================

                var allocatedQuantity =
                    await _repository
                        .GetAllocatedPurchaseInvoiceQuantityAsync(
                            sourceItem.Id,
                            excludePurchaseInvoiceId);


                var availableQuantity =
                    sourceItem.ReceivedQuantity -
                    allocatedQuantity;


                if (availableQuantity < 0m)
                {
                    availableQuantity =
                        0m;
                }


                if (submittedItem.PurchaseInvoiceQuantity >
                    availableQuantity)
                {
                    throw new BusinessException(
                        $"Invoice quantity for '{sourceItem.ItemName}' cannot exceed available quantity {availableQuantity:N3}.");
                }


                // =================================================
                // BUILD TRUSTED SNAPSHOT
                // =================================================

                var preparedItem =
                    CreateTrustedSourceSnapshot(
                        purchaseInvoice,
                        sourceItem,
                        submittedItem.PurchaseInvoiceQuantity,
                        purchaseInvoice.IsInterState,
                        submittedItem.Rate);


                preparedItem.SequenceNumber =
                    sequenceNumber++;


                preparedItem.IsActive =
                    true;


                preparedItem.IsDeleted =
                    false;


                CalculateLine(
                    preparedItem,
                    purchaseInvoice.IsInterState);


                result.Add(
                    preparedItem);
            }


            return result;
        }

        #endregion


        // =====================================================
        // TRUSTED SOURCE SNAPSHOT
        // =====================================================

        #region Trusted Source Snapshot

        private static PurchaseInvoiceItem
            CreateTrustedSourceSnapshot(
                PurchaseInvoice purchaseInvoice,
                GoodsReceiptNoteItem sourceItem,
                decimal purchaseInvoiceQuantity,
                bool isInterState,
                decimal rate)
        {
            var purchaseOrderItem =
                sourceItem.PurchaseOrderItem;


            if (purchaseOrderItem == null)
            {
                throw new BusinessException(
                    $"Purchase Order item information is missing for '{sourceItem.ItemName}'.");
            }


            if (sourceItem.GoodsReceiptNote == null)
            {
                throw new BusinessException(
                    $"GRN information is missing for '{sourceItem.ItemName}'.");
            }


            var item =
                new PurchaseInvoiceItem
                {
                    // -----------------------------------------
                    // Purchase Order Source
                    // -----------------------------------------

                    PurchaseOrderItemId =
                        sourceItem.PurchaseOrderItemId,

                    PurchaseOrderCode =
                        purchaseInvoice.PurchaseOrderCode,

                    PurchaseOrderQuantity =
                        purchaseOrderItem.Quantity,


                    // -----------------------------------------
                    // GRN Source
                    // -----------------------------------------

                    GoodsReceiptNoteId =
                        sourceItem.GoodsReceiptNoteId,

                    GoodsReceiptNoteCode =
                        sourceItem.GoodsReceiptNote.Code,

                    GoodsReceiptNoteItemId =
                        sourceItem.Id,

                    GoodsReceiptQuantity =
                        sourceItem.ReceivedQuantity,

                    SupplierChallanNumber =
                        sourceItem.GoodsReceiptNote
                            .SupplierChallanNumber,

                    SupplierChallanDate =
                        sourceItem.GoodsReceiptNote
                            .SupplierChallanDate,


                    // -----------------------------------------
                    // Item Snapshot
                    // -----------------------------------------

                    ItemId =
                        sourceItem.ItemId,

                    ItemCode =
                        sourceItem.ItemCode,

                    ItemName =
                        sourceItem.ItemName,

                    Description =
                        purchaseOrderItem.Description,

                    Specification =
                        sourceItem.Specification,

                    UnitName =
                        sourceItem.UnitName,

                    HsnCode =
                        purchaseOrderItem.HSNCode,


                    // -----------------------------------------
                    // Drawing Snapshot
                    // -----------------------------------------

                    DrawingId =
                        purchaseOrderItem.DrawingId,

                    DrawingNumber =
                        purchaseOrderItem.DrawingNumber,

                    DrawingRevision =
                        purchaseOrderItem.DrawingRevision,


                    // -----------------------------------------
                    // Invoice Quantity
                    // -----------------------------------------

                    PurchaseInvoiceQuantity =
                        purchaseInvoiceQuantity,


                    // -----------------------------------------
                    // Actual Supplier Invoice Rate
                    //
                    // NOT Purchase Order Unit Price.
                    // -----------------------------------------

                    Rate =
                        rate,


                    // -----------------------------------------
                    // Discount
                    // -----------------------------------------

                    DiscountPercent =
                        0m,

                    DiscountAmount =
                        0m,


                    // -----------------------------------------
                    // GST
                    // -----------------------------------------

                    GstRate =
                        purchaseOrderItem.GSTPercent,


                    // -----------------------------------------
                    // State
                    // -----------------------------------------

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };


            CalculateLine(
                item,
                isInterState);


            return item;
        }

        #endregion


        // =====================================================
        // LINE CALCULATION
        // =====================================================

        #region Line Calculation

        private static void CalculateLine(
            PurchaseInvoiceItem item,
            bool isInterState)
        {
            // =================================================
            // GROSS
            // =================================================

            item.GrossAmount =
                RoundMoney(
                    item.PurchaseInvoiceQuantity *
                    item.Rate);


            // =================================================
            // DISCOUNT
            //
            // Disabled for current Purchase Invoice phase.
            // =================================================

            item.DiscountPercent =
                0m;


            item.DiscountAmount =
                0m;


            item.TaxableAmount =
                item.GrossAmount;


            // =================================================
            // RESET GST
            // =================================================

            item.CgstRate =
                0m;

            item.SgstRate =
                0m;

            item.IgstRate =
                0m;


            item.CgstAmount =
                0m;

            item.SgstAmount =
                0m;

            item.IgstAmount =
                0m;


            // =================================================
            // GST CALCULATION
            // =================================================

            if (isInterState)
            {
                item.IgstRate =
                    item.GstRate;


                item.IgstAmount =
                    RoundMoney(
                        item.TaxableAmount *
                        item.IgstRate /
                        100m);
            }
            else
            {
                item.CgstRate =
                    item.GstRate /
                    2m;


                item.SgstRate =
                    item.GstRate /
                    2m;


                item.CgstAmount =
                    RoundMoney(
                        item.TaxableAmount *
                        item.CgstRate /
                        100m);


                item.SgstAmount =
                    RoundMoney(
                        item.TaxableAmount *
                        item.SgstRate /
                        100m);
            }


            // =================================================
            // TAX TOTAL
            // =================================================

            item.TotalTaxAmount =
                RoundMoney(
                    item.CgstAmount +
                    item.SgstAmount +
                    item.IgstAmount);


            // =================================================
            // LINE TOTAL
            // =================================================

            item.LineTotal =
                RoundMoney(
                    item.TaxableAmount +
                    item.TotalTaxAmount);
        }

        #endregion


        // =====================================================
        // HEADER TOTALS
        // =====================================================

        #region Header Totals

        private static void CalculateHeaderTotals(
            PurchaseInvoice purchaseInvoice)
        {
            var activeItems =
                purchaseInvoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .ToList();


            purchaseInvoice.GrossAmount =
                RoundMoney(
                    activeItems.Sum(x =>
                        x.GrossAmount));


            purchaseInvoice.DiscountAmount =
                RoundMoney(
                    activeItems.Sum(x =>
                        x.DiscountAmount));


            purchaseInvoice.TaxableAmount =
                RoundMoney(
                    activeItems.Sum(x =>
                        x.TaxableAmount));


            purchaseInvoice.CgstAmount =
                RoundMoney(
                    activeItems.Sum(x =>
                        x.CgstAmount));


            purchaseInvoice.SgstAmount =
                RoundMoney(
                    activeItems.Sum(x =>
                        x.SgstAmount));


            purchaseInvoice.IgstAmount =
                RoundMoney(
                    activeItems.Sum(x =>
                        x.IgstAmount));


            /*
             * Current Purchase Order architecture:
             *
             * Transport Charges and Other Charges do not
             * have separate GST calculation.
             */
            purchaseInvoice.GrandTotal =
                RoundMoney(
                    purchaseInvoice.TaxableAmount +
                    purchaseInvoice.CgstAmount +
                    purchaseInvoice.SgstAmount +
                    purchaseInvoice.IgstAmount +
                    purchaseInvoice.TransportCharges +
                    purchaseInvoice.OtherCharges +
                    purchaseInvoice.RoundOffAmount);
        }

        #endregion


        // =====================================================
        // ITEM SYNC
        // =====================================================

        #region Sync Items

        private static void SyncItems(
            PurchaseInvoice purchaseInvoice,
            List<PurchaseInvoiceItem> preparedItems)
        {
            var now =
                DateTime.Now;


            var preparedSourceIds =
                preparedItems
                    .Select(x =>
                        x.GoodsReceiptNoteItemId)
                    .ToHashSet();


            // =================================================
            // SOFT DELETE REMOVED LINES
            // =================================================

            foreach (var existingItem
                in purchaseInvoice.Items.ToList())
            {
                if (!preparedSourceIds.Contains(
                    existingItem.GoodsReceiptNoteItemId))
                {
                    if (!existingItem.IsDeleted)
                    {
                        existingItem.IsDeleted =
                            true;


                        existingItem.IsActive =
                            false;


                        existingItem.ModifiedOn =
                            now;


                        existingItem.ModifiedBy =
                            SystemUser;
                    }
                }
            }


            // =================================================
            // ADD / UPDATE LINES
            // =================================================

            foreach (var preparedItem
                in preparedItems)
            {
                var existingItem =
                    purchaseInvoice.Items
                        .FirstOrDefault(x =>
                            x.GoodsReceiptNoteItemId ==
                            preparedItem.GoodsReceiptNoteItemId);


                // ---------------------------------------------
                // New line
                // ---------------------------------------------

                if (existingItem == null)
                {
                    preparedItem.CreatedOn =
                        now;


                    preparedItem.CreatedBy =
                        SystemUser;


                    preparedItem.IsActive =
                        true;


                    preparedItem.IsDeleted =
                        false;


                    purchaseInvoice.Items.Add(
                        preparedItem);


                    continue;
                }


                // ---------------------------------------------
                // Existing line
                // ---------------------------------------------

                CopyItemValues(
                    existingItem,
                    preparedItem);


                existingItem.IsDeleted =
                    false;


                existingItem.IsActive =
                    true;


                existingItem.ModifiedOn =
                    now;


                existingItem.ModifiedBy =
                    SystemUser;
            }
        }


        private static void CopyItemValues(
            PurchaseInvoiceItem target,
            PurchaseInvoiceItem source)
        {
            target.SequenceNumber =
                source.SequenceNumber;


            // =================================================
            // PURCHASE ORDER
            // =================================================

            target.PurchaseOrderItemId =
                source.PurchaseOrderItemId;

            target.PurchaseOrderCode =
                source.PurchaseOrderCode;

            target.PurchaseOrderQuantity =
                source.PurchaseOrderQuantity;


            // =================================================
            // GRN
            // =================================================

            target.GoodsReceiptNoteId =
                source.GoodsReceiptNoteId;

            target.GoodsReceiptNoteCode =
                source.GoodsReceiptNoteCode;

            target.GoodsReceiptNoteItemId =
                source.GoodsReceiptNoteItemId;

            target.GoodsReceiptQuantity =
                source.GoodsReceiptQuantity;

            target.SupplierChallanNumber =
                source.SupplierChallanNumber;

            target.SupplierChallanDate =
                source.SupplierChallanDate;


            // =================================================
            // ITEM
            // =================================================

            target.ItemId =
                source.ItemId;

            target.ItemCode =
                source.ItemCode;

            target.ItemName =
                source.ItemName;

            target.Description =
                source.Description;

            target.Specification =
                source.Specification;

            target.UnitName =
                source.UnitName;

            target.HsnCode =
                source.HsnCode;


            // =================================================
            // DRAWING
            // =================================================

            target.DrawingId =
                source.DrawingId;

            target.DrawingNumber =
                source.DrawingNumber;

            target.DrawingRevision =
                source.DrawingRevision;


            // =================================================
            // QUANTITY / RATE
            // =================================================

            target.PurchaseInvoiceQuantity =
                source.PurchaseInvoiceQuantity;


            /*
             * Preserve manually entered
             * Supplier Invoice Rate.
             */
            target.Rate =
                source.Rate;


            // =================================================
            // COMMERCIAL
            // =================================================

            target.GrossAmount =
                source.GrossAmount;

            target.DiscountPercent =
                source.DiscountPercent;

            target.DiscountAmount =
                source.DiscountAmount;

            target.TaxableAmount =
                source.TaxableAmount;


            // =================================================
            // GST
            // =================================================

            target.GstRate =
                source.GstRate;

            target.CgstRate =
                source.CgstRate;

            target.SgstRate =
                source.SgstRate;

            target.IgstRate =
                source.IgstRate;

            target.CgstAmount =
                source.CgstAmount;

            target.SgstAmount =
                source.SgstAmount;

            target.IgstAmount =
                source.IgstAmount;

            target.TotalTaxAmount =
                source.TotalTaxAmount;


            // =================================================
            // TOTAL
            // =================================================

            target.LineTotal =
                source.LineTotal;
        }

        #endregion


        // =====================================================
        // VALIDATION
        // =====================================================

        #region Validation

        private static void ValidateSubmittedHeader(
            PurchaseInvoice purchaseInvoice)
        {
            if (purchaseInvoice.PurchaseOrderId <= 0)
            {
                throw new BusinessException(
                    "Please select a Purchase Order.");
            }


            if (string.IsNullOrWhiteSpace(
                purchaseInvoice.SupplierInvoiceNumber))
            {
                throw new BusinessException(
                    "Supplier Invoice Number is required.");
            }


            if (purchaseInvoice.TransportCharges < 0m)
            {
                throw new BusinessException(
                    "Transport Charges cannot be negative.");
            }


            if (purchaseInvoice.OtherCharges < 0m)
            {
                throw new BusinessException(
                    "Other Charges cannot be negative.");
            }
        }


        private static void ValidatePurchaseOrder(
            PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.IsDeleted ||
                !purchaseOrder.IsActive)
            {
                throw new BusinessException(
                    "Purchase Order is not active.");
            }


            if (purchaseOrder.Status ==
                PurchaseOrderStatus.Cancelled)
            {
                throw new BusinessException(
                    "Cancelled Purchase Order cannot be used for Purchase Invoice.");
            }
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE CODE GENERATION
        // =====================================================

        #region Code Generation

        private async Task<string>
            GenerateNextCodeAsync(
                DateTime purchaseInvoiceDate)
        {
            var financialYear =
                GetFinancialYear(
                    purchaseInvoiceDate);


            var prefix =
                $"AI/PINV/{financialYear}/";


            var lastCode =
                await _repository
                    .GetLastCodeAsync(
                        prefix);


            var nextNumber =
                1;


            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var lastPart =
                    lastCode
                        .Split(
                            '/',
                            StringSplitOptions
                                .RemoveEmptyEntries)
                        .LastOrDefault();


                if (int.TryParse(
                    lastPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber + 1;
                }
            }


            return
                $"{prefix}{nextNumber:00000}";
        }


        private static string GetFinancialYear(
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


        private static void
            EnsureInvoiceDateMatchesCodeFinancialYear(
                string code,
                DateTime invoiceDate)
        {
            if (string.IsNullOrWhiteSpace(
                code))
            {
                return;
            }


            var parts =
                code.Split(
                    '/',
                    StringSplitOptions
                        .RemoveEmptyEntries);


            if (parts.Length < 4)
            {
                return;
            }


            var codeFinancialYear =
                parts[2];


            var expectedFinancialYear =
                GetFinancialYear(
                    invoiceDate);


            if (!string.Equals(
                codeFinancialYear,
                expectedFinancialYear,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    $"Purchase Invoice Date must remain within financial year {codeFinancialYear}.");
            }
        }

        #endregion


        // =====================================================
        // SNAPSHOT HELPERS
        // =====================================================

        #region Snapshot Helpers

        private static string
            SerializeScalarSnapshot(
                object source)
        {
            var dictionary =
                new Dictionary<string, object?>();


            var properties =
                source.GetType()
                    .GetProperties(
                        BindingFlags.Public |
                        BindingFlags.Instance);


            foreach (var property
                in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }


                var type =
                    Nullable.GetUnderlyingType(
                        property.PropertyType)
                    ??
                    property.PropertyType;


                if (!IsScalarType(
                    type))
                {
                    continue;
                }


                dictionary[property.Name] =
                    property.GetValue(
                        source);
            }


            return JsonSerializer
                .Serialize(
                    dictionary);
        }


        private static bool IsScalarType(
            Type type)
        {
            return
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(Guid);
        }


        private static string?
            GetSnapshotString(
                string? json,
                string propertyName)
        {
            if (string.IsNullOrWhiteSpace(
                json))
            {
                return null;
            }


            try
            {
                using var document =
                    JsonDocument.Parse(
                        json);


                if (!document.RootElement
                    .TryGetProperty(
                        propertyName,
                        out var value))
                {
                    return null;
                }


                if (value.ValueKind ==
                    JsonValueKind.Null)
                {
                    return null;
                }


                if (value.ValueKind ==
                    JsonValueKind.String)
                {
                    return value.GetString();
                }


                return value.ToString();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        #endregion


        // =====================================================
        // SUPPLIER INVOICE PDF METADATA
        // =====================================================

        #region Supplier Invoice PDF Metadata

        /*
         * Physical PDF handling belongs to Web layer.
         *
         * Application layer stores only:
         *
         * - relative path
         * - original filename
         * - uploaded timestamp
         */
        private static string?
            NormalizeNullableText(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return null;
            }


            return value.Trim();
        }

        #endregion


        // =====================================================
        // GST / PAYMENT HELPERS
        // =====================================================

        #region GST / Payment Helpers

        private static bool
            IsInterStateTransaction(
                string? companyState,
                string? supplierState)
        {
            if (
                string.IsNullOrWhiteSpace(
                    companyState) ||
                string.IsNullOrWhiteSpace(
                    supplierState)
            )
            {
                return false;
            }


            return !string.Equals(
                NormalizeState(
                    companyState),
                NormalizeState(
                    supplierState),
                StringComparison.OrdinalIgnoreCase);
        }


        private static string NormalizeState(
            string state)
        {
            return new string(
                state
                    .Where(
                        char.IsLetterOrDigit)
                    .Select(
                        char.ToUpperInvariant)
                    .ToArray());
        }


        private static DateTime?
            CalculateDueDate(
                DateTime supplierInvoiceDate,
                int? creditDays)
        {
            if (!creditDays.HasValue)
            {
                return null;
            }


            if (creditDays.Value < 0)
            {
                return null;
            }


            return supplierInvoiceDate
                .Date
                .AddDays(
                    creditDays.Value);
        }

        #endregion


        // =====================================================
        // COMMON HELPERS
        // =====================================================

        #region Common Helpers

        private static decimal RoundMoney(
            decimal amount)
        {
            return Math.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero);
        }


        private static void NormalizePaging(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber =
                    1;
            }


            if (pageSize <= 0)
            {
                pageSize =
                    10;
            }


            if (pageSize > 100)
            {
                pageSize =
                    100;
            }
        }

        #endregion
    }
}