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
        private readonly IPurchaseInvoiceRepository
            _repository;

        private const string SystemUser =
            "System";


        public PurchaseInvoiceService(
            IPurchaseInvoiceRepository repository)
        {
            _repository =
                repository;
        }


        // =====================================================
        // READ
        // =====================================================

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


        public async Task<PagedResult<PurchaseInvoice>>
            SearchPagedAsync(
                string? searchText,
                int pageNumber,
                int pageSize)
        {
            NormalizePaging(
                ref pageNumber,
                ref pageSize);


            searchText =
                searchText?.Trim();


            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await GetPagedAsync(
                    pageNumber,
                    pageSize);
            }


            return await _repository
                .SearchPagedAsync(
                    searchText,
                    pageNumber,
                    pageSize);
        }


        // =====================================================
        // PURCHASE ORDERS AVAILABLE FOR INVOICE
        // =====================================================

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


        // =====================================================
        // GRN AVAILABILITY
        // =====================================================

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


                if (remaining > 0)
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


        // =====================================================
        // PREPARE DRAFT
        // =====================================================

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
                 * Rate is NOT taken from Purchase Order.
                 *
                 * Supplier's actual Invoice Rate is not known
                 * until Supplier Invoice is received.
                 *
                 * Therefore Create screen initially gets 0.00
                 * and user enters actual Supplier Invoice Rate.
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


        // =====================================================
        // CREATE
        // =====================================================

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
                    Code =
                        await GenerateNextCodeAsync(
                            submitted.PurchaseInvoiceDate),


                    PurchaseInvoiceDate =
                        submitted.PurchaseInvoiceDate,

                    Status =
                        PurchaseInvoiceStatus.Draft,


                    SupplierInvoiceNumber =
                        supplierInvoiceNumber,

                    SupplierInvoiceDate =
                        submitted.SupplierInvoiceDate,


                    PurchaseOrderId =
                        purchaseOrder.Id,

                    PurchaseOrderCode =
                        purchaseOrder.Code,


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
                            submitted.SupplierInvoiceDate,
                            creditDays),


                    PlaceOfSupply =
                        company.State,

                    IsInterState =
                        IsInterStateTransaction(
                            company.State,
                            supplier.State),


                    TransportCharges =
                        submitted.TransportCharges,

                    OtherCharges =
                        submitted.OtherCharges,

                    RoundOffAmount =
                        submitted.RoundOffAmount,


                    Remarks =
                        submitted.Remarks?.Trim(),


                    CreatedOn =
                        DateTime.Now,

                    CreatedBy =
                        SystemUser,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };


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


            CalculateHeaderTotals(
                purchaseInvoice);


            await _repository
    .AddAsync(
        purchaseInvoice);

            return purchaseInvoice;
        }


        // =====================================================
        // UPDATE
        // =====================================================

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


            if (existing.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Invoice can be edited.");
            }


            ValidateSubmittedHeader(
                submitted);


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


            if (string.IsNullOrWhiteSpace(
                    existing.SupplierSnapshotJson) ||
                string.IsNullOrWhiteSpace(
                    existing.CompanySnapshotJson))
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


            existing.PurchaseInvoiceDate =
                submitted.PurchaseInvoiceDate;


            existing.SupplierInvoiceNumber =
                supplierInvoiceNumber;


            existing.SupplierInvoiceDate =
                submitted.SupplierInvoiceDate;


            existing.DueDate =
                CalculateDueDate(
                    submitted.SupplierInvoiceDate,
                    existing.CreditDays);


            existing.PlaceOfSupply =
                companyState;


            existing.IsInterState =
                IsInterStateTransaction(
                    companyState,
                    supplierState);


            existing.TransportCharges =
                submitted.TransportCharges;


            existing.OtherCharges =
                submitted.OtherCharges;


            existing.RoundOffAmount =
                submitted.RoundOffAmount;


            existing.Remarks =
                submitted.Remarks?.Trim();


            var preparedItems =
                await PrepareTrustedItemsAsync(
                    existing,
                    submitted.Items,
                    existing.Id);


            SyncItems(
                existing,
                preparedItems);


            CalculateHeaderTotals(
                existing);


            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            await _repository
    .UpdateAsync(
        existing);

            return existing;
        }


        // =====================================================
        // FINALIZE
        // =====================================================

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
                    "Deleted Purchase Invoice cannot be finalized.");
            }


            if (existing.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Invoice can be finalized.");
            }


            if (string.IsNullOrWhiteSpace(
                    existing.SupplierSnapshotJson) ||
                string.IsNullOrWhiteSpace(
                    existing.CompanySnapshotJson))
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


            if (purchaseOrder.Supplier == null ||
                purchaseOrder.Supplier.SupplierId !=
                existing.SupplierId)
            {
                throw new BusinessException(
                    "Purchase Order Supplier does not match Purchase Invoice Supplier.");
            }


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


            /*
             * IMPORTANT:
             *
             * Saved manual Supplier Invoice Rate must be
             * preserved during Finalize.
             */
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


        // =====================================================
        // DELETE
        // =====================================================

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


            if (existing.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Invoice can be deleted.");
            }


            if (existing.IsDeleted)
            {
                throw new BusinessException(
                    "Purchase Invoice is already deleted.");
            }


            existing.IsDeleted =
                true;


            existing.IsActive =
                false;


            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            await _repository
                .UpdateAsync(
                    existing);
        }


        // =====================================================
        // DELETED
        // =====================================================

        public async Task<List<PurchaseInvoice>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }


        // =====================================================
        // RESTORE
        // =====================================================

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


            if (existing.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only deleted Draft Purchase Invoice can be restored.");
            }


            if (string.IsNullOrWhiteSpace(
                    existing.SupplierSnapshotJson) ||
                string.IsNullOrWhiteSpace(
                    existing.CompanySnapshotJson))
            {
                throw new BusinessException(
                    "Purchase Invoice snapshot information is missing.");
            }


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


            /*
             * Preserve manually entered Supplier Invoice Rate.
             */
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


            /*
             * While deleted, this invoice does not reserve
             * GRN quantity. Revalidate before restore.
             */
            var preparedItems =
                await PrepareTrustedItemsAsync(
                    existing,
                    submittedItems,
                    existing.Id);


            existing.IsDeleted =
                false;


            existing.IsActive =
                true;


            SyncItems(
                existing,
                preparedItems);


            CalculateHeaderTotals(
                existing);


            existing.ModifiedOn =
                DateTime.Now;


            existing.ModifiedBy =
                SystemUser;


            await _repository
                .UpdateAsync(
                    existing);
        }


        // =====================================================
        // PREPARE TRUSTED ITEMS
        // =====================================================

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
                 * Actual Supplier Invoice Rate is manually
                 * entered by the user.
                 */
                if (submittedItem.Rate <= 0m)
                {
                    throw new BusinessException(
                        "Rate must be greater than zero for all selected Purchase Invoice items.");
                }


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


        // =====================================================
        // TRUSTED SOURCE SNAPSHOT
        // =====================================================

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


            /*
             * NOTE:
             *
             * Item / HSN / GST / Drawing / GRN / PO details
             * come from trusted database sources.
             *
             * Rate is intentionally NOT taken from PO.
             * Rate is the actual rate printed on Supplier Invoice.
             */
            var item =
                new PurchaseInvoiceItem
                {
                    PurchaseOrderItemId =
                        sourceItem.PurchaseOrderItemId,

                    PurchaseOrderCode =
                        purchaseInvoice.PurchaseOrderCode,

                    PurchaseOrderQuantity =
                        purchaseOrderItem.Quantity,


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


                    DrawingId =
                        purchaseOrderItem.DrawingId,

                    DrawingNumber =
                        purchaseOrderItem.DrawingNumber,

                    DrawingRevision =
                        purchaseOrderItem.DrawingRevision,


                    PurchaseInvoiceQuantity =
                        purchaseInvoiceQuantity,


                    /*
                     * MANUAL SUPPLIER INVOICE RATE
                     */
                    Rate =
                        rate,


                    /*
                     * Purchase discount is disabled
                     * in current Purchase architecture.
                     */
                    DiscountPercent =
                        0m,

                    DiscountAmount =
                        0m,


                    GstRate =
                        purchaseOrderItem.GSTPercent,


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


        // =====================================================
        // LINE CALCULATION
        // =====================================================

        private static void CalculateLine(
            PurchaseInvoiceItem item,
            bool isInterState)
        {
            item.GrossAmount =
                RoundMoney(
                    item.PurchaseInvoiceQuantity *
                    item.Rate);


            item.DiscountPercent =
                0m;


            item.DiscountAmount =
                0m;


            item.TaxableAmount =
                item.GrossAmount;


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


            item.TotalTaxAmount =
                RoundMoney(
                    item.CgstAmount +
                    item.SgstAmount +
                    item.IgstAmount);


            item.LineTotal =
                RoundMoney(
                    item.TaxableAmount +
                    item.TotalTaxAmount);
        }


        // =====================================================
        // HEADER TOTALS
        // =====================================================

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
             * Current PO architecture does not calculate
             * separate GST on Transport / Other Charges.
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


        // =====================================================
        // SYNC ITEMS
        // =====================================================

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


            foreach (var preparedItem
                in preparedItems)
            {
                var existingItem =
                    purchaseInvoice.Items
                        .FirstOrDefault(x =>
                            x.GoodsReceiptNoteItemId ==
                            preparedItem.GoodsReceiptNoteItemId);


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


            target.PurchaseOrderItemId =
                source.PurchaseOrderItemId;

            target.PurchaseOrderCode =
                source.PurchaseOrderCode;

            target.PurchaseOrderQuantity =
                source.PurchaseOrderQuantity;


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


            target.DrawingId =
                source.DrawingId;

            target.DrawingNumber =
                source.DrawingNumber;

            target.DrawingRevision =
                source.DrawingRevision;


            target.PurchaseInvoiceQuantity =
                source.PurchaseInvoiceQuantity;


            /*
             * Preserve actual manually entered
             * Supplier Invoice Rate.
             */
            target.Rate =
                source.Rate;


            target.GrossAmount =
                source.GrossAmount;

            target.DiscountPercent =
                source.DiscountPercent;

            target.DiscountAmount =
                source.DiscountAmount;

            target.TaxableAmount =
                source.TaxableAmount;


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


            target.LineTotal =
                source.LineTotal;
        }


        // =====================================================
        // HEADER VALIDATION
        // =====================================================

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


        // =====================================================
        // CODE GENERATION
        // =====================================================

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


        // =====================================================
        // SNAPSHOTS
        // =====================================================

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


        // =====================================================
        // GST / PAYMENT
        // =====================================================

        private static bool
            IsInterStateTransaction(
                string? companyState,
                string? supplierState)
        {
            if (string.IsNullOrWhiteSpace(
                    companyState) ||
                string.IsNullOrWhiteSpace(
                    supplierState))
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
                    .Where(char.IsLetterOrDigit)
                    .Select(char.ToUpperInvariant)
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


        // =====================================================
        // COMMON
        // =====================================================

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
    }
}