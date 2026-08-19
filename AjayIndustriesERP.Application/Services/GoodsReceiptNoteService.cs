// ============================================================
// File: GoodsReceiptNoteService.cs
// Purpose:
// Contains all business rules required for the
// Goods Receipt Note (GRN) module.
//
// Responsibilities:
// - Load Purchase Orders
// - Prepare PO items for GRN
// - Prepare existing GRN for Edit
// - Calculate Previous / Balance quantities
// - Process Not / Partial / Full Received
// - Validate Material Status
// - Validate Supplier Challan duplicate
// - Protect GRN receipt history during Edit
// - Generate financial-year based GRN number
// - Create / Update trusted GRN snapshots
//
// Edit Rules:
// - Purchase Order cannot be changed.
// - Only the latest GRN against a PO can be edited.
// - Current GRN is excluded while recalculating
//   Previously Received quantity.
//
// Phase 1:
// - Stock is NOT updated.
// - PO Status is NOT updated.
// - Material Status is stored only.
// ============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class GoodsReceiptNoteService
        : IGoodsReceiptNoteService
    {
        private readonly IGoodsReceiptNoteRepository _repository;

        public GoodsReceiptNoteService(
            IGoodsReceiptNoteRepository repository)
        {
            _repository = repository;
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<List<GoodsReceiptNote>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }


        // =====================================================
        // SEARCH GRNs
        // =====================================================
        //
        // Repository returns complete GRN history for every
        // Purchase Order matching the search.
        // =====================================================

        public async Task<List<GoodsReceiptNote>>
            SearchAsync(
                string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _repository
                    .GetAllAsync();
            }


            return await _repository
                .SearchAsync(
                    searchText);
        }


        // =====================================================
        // PAGED GRN PURCHASE ORDER GROUPS
        // =====================================================
        //
        // Pagination unit is one Purchase Order group,
        // not one individual GRN.
        // =====================================================

        public async Task<PagedResult<GoodsReceiptNote>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }


            if (pageSize < 1)
            {
                pageSize = 10;
            }


            return await _repository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }


        // =====================================================
        // GET DETAILS
        // =====================================================

        public async Task<GoodsReceiptNote?> GetByIdAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid GRN.");
            }

            return await _repository.GetByIdAsync(id);
        }


        // ============================================================
        // GET ITEM RECEIPT HISTORY
        // Purpose:
        // Returns actual receipt history for all items of the selected
        // Purchase Order up to the selected GRN.
        //
        // This allows the Details page to show the complete material
        // receipt timeline without opening every previous GRN.
        // ============================================================

        public async Task<List<GoodsReceiptNoteItem>>
            GetReceiptHistoryAsync(
                int purchaseOrderId,
                int upToGoodsReceiptNoteId)
        {
            if (purchaseOrderId <= 0 ||
                upToGoodsReceiptNoteId <= 0)
            {
                throw new BusinessException(
                    "Invalid GRN receipt history request.");
            }


            return await _repository
                .GetReceiptHistoryAsync(
                    purchaseOrderId,
                    upToGoodsReceiptNoteId);
        }

        // =====================================================
        // PURCHASE ORDER LIST
        // =====================================================

        public async Task<List<PurchaseOrder>>
            GetPurchaseOrdersForReceiptAsync()
        {
            return await _repository
                .GetPurchaseOrdersForReceiptAsync();
        }


        // =====================================================
        // PREPARE NEW GRN FROM PO
        // =====================================================

        public async Task<GoodsReceiptNote>
            PrepareForPurchaseOrderAsync(
                int purchaseOrderId)
        {
            var purchaseOrder =
                await GetValidatedPurchaseOrderAsync(
                    purchaseOrderId);


            var goodsReceiptNote =
                new GoodsReceiptNote
                {
                    GRNDate =
                        DateTime.Today,

                    PurchaseOrderId =
                        purchaseOrder.Id,

                    PurchaseOrder =
                        purchaseOrder,

                    SupplierId =
                        purchaseOrder.SupplierId,

                    SupplierName =
                        purchaseOrder.SupplierName
                };


            foreach (var purchaseOrderItem
                     in purchaseOrder.Items
                         .OrderBy(x => x.Id))
            {
                var previousReceived =
                    await _repository
                        .GetPreviouslyReceivedQuantityAsync(
                            purchaseOrderItem.Id);


                previousReceived =
                    NormalizePreviousReceived(
                        previousReceived);


                var balanceQuantity =
                    CalculateBalanceQuantity(
                        purchaseOrderItem.Quantity,
                        previousReceived);


                goodsReceiptNote.Items.Add(
                    new GoodsReceiptNoteItem
                    {
                        PurchaseOrderItemId =
                            purchaseOrderItem.Id,

                        ItemId =
                            purchaseOrderItem.ItemId,

                        ItemCode =
                            purchaseOrderItem.ItemCode,

                        ItemName =
                            purchaseOrderItem.ItemName,

                        Specification =
                            purchaseOrderItem.Specification,

                        UnitName =
                            purchaseOrderItem.UnitName,

                        OrderedQuantity =
                            purchaseOrderItem.Quantity,

                        PreviouslyReceivedQuantity =
                            previousReceived,

                        BalanceQuantity =
                            balanceQuantity,

                        ReceiptStatus =
                            GoodsReceiptStatus.NotReceived,

                        ReceivedQuantity =
                            0,

                        PendingQuantity =
                            balanceQuantity
                    });
            }


            return goodsReceiptNote;
        }


        // =====================================================
        // PREPARE GRN FOR EDIT
        // =====================================================

        public async Task<GoodsReceiptNote>
            PrepareForEditAsync(
                int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid GRN.");
            }


            var existingGrn =
                await _repository
                    .GetByIdAsync(id);


            if (existingGrn == null)
            {
                throw new BusinessException(
                    "GRN not found.");
            }


            await ValidateLatestGrnForEditAsync(
                existingGrn);


            var purchaseOrder =
                await GetValidatedPurchaseOrderAsync(
                    existingGrn.PurchaseOrderId);


            var result =
                new GoodsReceiptNote
                {
                    Id =
                        existingGrn.Id,

                    Code =
                        existingGrn.Code,

                    GRNDate =
                        existingGrn.GRNDate,

                    PurchaseOrderId =
                        purchaseOrder.Id,

                    PurchaseOrder =
                        purchaseOrder,

                    SupplierId =
                        purchaseOrder.SupplierId,

                    SupplierName =
                        purchaseOrder.SupplierName,

                    SupplierChallanNumber =
                        existingGrn.SupplierChallanNumber,

                    SupplierChallanDate =
                        existingGrn.SupplierChallanDate,

                    Remarks =
                        existingGrn.Remarks
                };


            foreach (var purchaseOrderItem
                     in purchaseOrder.Items
                         .OrderBy(x => x.Id))
            {
                var currentItem =
                    existingGrn.Items
                        .FirstOrDefault(x =>
                            x.PurchaseOrderItemId ==
                            purchaseOrderItem.Id);


                var previousReceived =
                    await _repository
                        .GetPreviouslyReceivedQuantityAsync(
                            purchaseOrderItem.Id,
                            existingGrn.Id);


                previousReceived =
                    NormalizePreviousReceived(
                        previousReceived);


                var balanceQuantity =
                    CalculateBalanceQuantity(
                        purchaseOrderItem.Quantity,
                        previousReceived);


                result.Items.Add(
                    new GoodsReceiptNoteItem
                    {
                        Id =
                            currentItem?.Id ?? 0,

                        Code =
                            currentItem?.Code ??
                            string.Empty,

                        PurchaseOrderItemId =
                            purchaseOrderItem.Id,

                        ItemId =
                            purchaseOrderItem.ItemId,

                        ItemCode =
                            purchaseOrderItem.ItemCode,

                        ItemName =
                            purchaseOrderItem.ItemName,

                        Specification =
                            purchaseOrderItem.Specification,

                        UnitName =
                            purchaseOrderItem.UnitName,

                        OrderedQuantity =
                            purchaseOrderItem.Quantity,

                        PreviouslyReceivedQuantity =
                            previousReceived,

                        BalanceQuantity =
                            balanceQuantity,

                        ReceiptStatus =
                            currentItem?.ReceiptStatus ??
                            GoodsReceiptStatus.NotReceived,

                        ReceivedQuantity =
                            currentItem?.ReceivedQuantity ??
                            0,

                        PendingQuantity =
                            currentItem?.PendingQuantity ??
                            balanceQuantity,

                        MaterialStatus =
                            currentItem?.MaterialStatus,

                        Remarks =
                            currentItem?.Remarks
                    });
            }


            return result;
        }


        // =====================================================
        // CREATE
        // =====================================================

        public async Task<GoodsReceiptNote> CreateAsync(
            GoodsReceiptNote goodsReceiptNote)
        {
            if (goodsReceiptNote == null)
            {
                throw new BusinessException(
                    "Invalid GRN information.");
            }


            if (goodsReceiptNote.GRNDate == default)
            {
                throw new BusinessException(
                    "GRN Date is required.");
            }


            var purchaseOrder =
                await GetValidatedPurchaseOrderAsync(
                    goodsReceiptNote.PurchaseOrderId);


            var supplierChallanNumber =
                await ValidateSupplierChallanAsync(
                    purchaseOrder,
                    goodsReceiptNote.SupplierChallanNumber);


            ValidateSubmittedItems(
                goodsReceiptNote,
                purchaseOrder);


            var grnCode =
                await GenerateGoodsReceiptNoteCodeAsync(
                    goodsReceiptNote.GRNDate);


            var newGrn =
                new GoodsReceiptNote
                {
                    Code =
                        grnCode,

                    GRNDate =
                        goodsReceiptNote.GRNDate,

                    PurchaseOrderId =
                        purchaseOrder.Id,

                    SupplierId =
                        purchaseOrder.SupplierId,

                    SupplierName =
                        purchaseOrder.SupplierName,

                    SupplierChallanNumber =
                        supplierChallanNumber,

                    SupplierChallanDate =
                        goodsReceiptNote.SupplierChallanDate,

                    Remarks =
                        NormalizeNullable(
                            goodsReceiptNote.Remarks)
                };


            var hasReceivedItem =
                false;

            var lineNumber =
                1;


            foreach (var poItem
                     in purchaseOrder.Items
                         .OrderBy(x => x.Id))
            {
                var submitted =
                    goodsReceiptNote.Items
                        .First(x =>
                            x.PurchaseOrderItemId ==
                            poItem.Id);


                var previousReceived =
                    await _repository
                        .GetPreviouslyReceivedQuantityAsync(
                            poItem.Id);


                previousReceived =
                    NormalizePreviousReceived(
                        previousReceived);


                var balance =
                    CalculateBalanceQuantity(
                        poItem.Quantity,
                        previousReceived);


                var receipt =
                    ProcessReceipt(
                        submitted,
                        poItem,
                        balance);


                if (receipt.HasReceived)
                {
                    hasReceivedItem = true;
                }


                newGrn.Items.Add(
                    new GoodsReceiptNoteItem
                    {
                        Code =
                            $"{grnCode}/{lineNumber:00}",

                        PurchaseOrderItemId =
                            poItem.Id,

                        ItemId =
                            poItem.ItemId,

                        ItemCode =
                            poItem.ItemCode,

                        ItemName =
                            poItem.ItemName,

                        Specification =
                            poItem.Specification,

                        UnitName =
                            poItem.UnitName,

                        OrderedQuantity =
                            poItem.Quantity,

                        PreviouslyReceivedQuantity =
                            previousReceived,

                        BalanceQuantity =
                            balance,

                        ReceiptStatus =
                            submitted.ReceiptStatus,

                        ReceivedQuantity =
                            receipt.ReceivedQuantity,

                        PendingQuantity =
                            receipt.PendingQuantity,

                        MaterialStatus =
                            receipt.MaterialStatus,

                        Remarks =
                            NormalizeNullable(
                                submitted.Remarks)
                    });


                lineNumber++;
            }


            EnsureAtLeastOneReceived(
                hasReceivedItem);


            await _repository.AddAsync(
                newGrn);


            return newGrn;
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<GoodsReceiptNote> UpdateAsync(
            GoodsReceiptNote goodsReceiptNote)
        {
            if (goodsReceiptNote == null ||
                goodsReceiptNote.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid GRN information.");
            }


            var existingGrn =
                await _repository
                    .GetForUpdateAsync(
                        goodsReceiptNote.Id);


            if (existingGrn == null)
            {
                throw new BusinessException(
                    "GRN not found.");
            }


            // Purchase Order cannot be changed.

            if (goodsReceiptNote.PurchaseOrderId !=
                existingGrn.PurchaseOrderId)
            {
                throw new BusinessException(
                    "Purchase Order cannot be changed after GRN creation.");
            }


            await ValidateLatestGrnForEditAsync(
                existingGrn);


            var purchaseOrder =
                await GetValidatedPurchaseOrderAsync(
                    existingGrn.PurchaseOrderId);


            var supplierChallanNumber =
                await ValidateSupplierChallanAsync(
                    purchaseOrder,
                    goodsReceiptNote.SupplierChallanNumber,
                    existingGrn.Id);


            ValidateSubmittedItems(
                goodsReceiptNote,
                purchaseOrder);


            // =================================================
            // HEADER
            // =================================================

            existingGrn.GRNDate =
                goodsReceiptNote.GRNDate;

            existingGrn.SupplierId =
                purchaseOrder.SupplierId;

            existingGrn.SupplierName =
                purchaseOrder.SupplierName;

            existingGrn.SupplierChallanNumber =
                supplierChallanNumber;

            existingGrn.SupplierChallanDate =
                goodsReceiptNote.SupplierChallanDate;

            existingGrn.Remarks =
                NormalizeNullable(
                    goodsReceiptNote.Remarks);

            existingGrn.ModifiedOn =
                DateTime.UtcNow;

            existingGrn.ModifiedBy =
                "System";


            var hasReceivedItem =
                false;

            var lineNumber =
                1;


            foreach (var poItem
                     in purchaseOrder.Items
                         .OrderBy(x => x.Id))
            {
                var submitted =
                    goodsReceiptNote.Items
                        .First(x =>
                            x.PurchaseOrderItemId ==
                            poItem.Id);


                // Current GRN must be excluded.
                var previousReceived =
                    await _repository
                        .GetPreviouslyReceivedQuantityAsync(
                            poItem.Id,
                            existingGrn.Id);


                previousReceived =
                    NormalizePreviousReceived(
                        previousReceived);


                var balance =
                    CalculateBalanceQuantity(
                        poItem.Quantity,
                        previousReceived);


                var receipt =
                    ProcessReceipt(
                        submitted,
                        poItem,
                        balance);


                if (receipt.HasReceived)
                {
                    hasReceivedItem = true;
                }


                var existingItem =
                    existingGrn.Items
                        .FirstOrDefault(x =>
                            x.PurchaseOrderItemId ==
                            poItem.Id);


                if (existingItem == null)
                {
                    existingItem =
                        new GoodsReceiptNoteItem
                        {
                            Code =
                                $"{existingGrn.Code}/{lineNumber:00}",

                            PurchaseOrderItemId =
                                poItem.Id
                        };


                    existingGrn.Items.Add(
                        existingItem);
                }


                existingItem.ItemId =
                    poItem.ItemId;

                existingItem.ItemCode =
                    poItem.ItemCode;

                existingItem.ItemName =
                    poItem.ItemName;

                existingItem.Specification =
                    poItem.Specification;

                existingItem.UnitName =
                    poItem.UnitName;

                existingItem.OrderedQuantity =
                    poItem.Quantity;

                existingItem.PreviouslyReceivedQuantity =
                    previousReceived;

                existingItem.BalanceQuantity =
                    balance;

                existingItem.ReceiptStatus =
                    submitted.ReceiptStatus;

                existingItem.ReceivedQuantity =
                    receipt.ReceivedQuantity;

                existingItem.PendingQuantity =
                    receipt.PendingQuantity;

                existingItem.MaterialStatus =
                    receipt.MaterialStatus;

                existingItem.Remarks =
                    NormalizeNullable(
                        submitted.Remarks);

                existingItem.ModifiedOn =
                    DateTime.UtcNow;

                existingItem.ModifiedBy =
                    "System";


                lineNumber++;
            }


            EnsureAtLeastOneReceived(
                hasReceivedItem);


            await _repository.UpdateAsync(
                existingGrn);


            return existingGrn;
        }


        // =====================================================
        // LOAD + VALIDATE PURCHASE ORDER
        // =====================================================

        private async Task<PurchaseOrder>
            GetValidatedPurchaseOrderAsync(
                int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
            {
                throw new BusinessException(
                    "Please select a valid Purchase Order.");
            }


            var purchaseOrder =
                await _repository
                    .GetPurchaseOrderForReceiptAsync(
                        purchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            ValidatePurchaseOrderForReceipt(
                purchaseOrder);


            if (purchaseOrder.Items == null ||
                purchaseOrder.Items.Count == 0)
            {
                throw new BusinessException(
                    "Selected Purchase Order does not contain any items.");
            }


            return purchaseOrder;
        }


        // =====================================================
        // PURCHASE ORDER VALIDATION
        // =====================================================

        private static void ValidatePurchaseOrderForReceipt(
            PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.IsDeleted ||
                !purchaseOrder.IsActive)
            {
                throw new BusinessException(
                    "Selected Purchase Order is not active.");
            }


            if (purchaseOrder.Status !=
                    PurchaseOrderStatus.Sent &&
                purchaseOrder.Status !=
                    PurchaseOrderStatus.PartiallyReceived)
            {
                throw new BusinessException(
                    "GRN can only be created or updated against a Sent " +
                    "or Partially Received Purchase Order.");
            }
        }


        // =====================================================
        // LATEST GRN VALIDATION
        // =====================================================

        private async Task ValidateLatestGrnForEditAsync(
            GoodsReceiptNote goodsReceiptNote)
        {
            var hasLaterGrn =
                await _repository
                    .HasLaterGoodsReceiptNoteAsync(
                        goodsReceiptNote.PurchaseOrderId,
                        goodsReceiptNote.Id);


            if (hasLaterGrn)
            {
                throw new BusinessException(
                    "This GRN cannot be edited because a later GRN " +
                    "exists against the same Purchase Order. " +
                    "Only the latest GRN can be edited.");
            }
        }


        // =====================================================
        // CHALLAN VALIDATION
        // =====================================================

        private async Task<string?>
            ValidateSupplierChallanAsync(
                PurchaseOrder purchaseOrder,
                string? challanNumber,
                int? excludeGoodsReceiptNoteId = null)
        {
            var normalized =
                NormalizeNullable(
                    challanNumber);


            if (string.IsNullOrWhiteSpace(
                normalized))
            {
                return null;
            }


            var exists =
                await _repository
                    .SupplierChallanNumberExistsAsync(
                        purchaseOrder.SupplierId,
                        normalized,
                        excludeGoodsReceiptNoteId);


            if (exists)
            {
                throw new BusinessException(
                    $"Supplier Challan No '{normalized}' already exists " +
                    $"for supplier {purchaseOrder.SupplierName}.");
            }


            return normalized;
        }


        // =====================================================
        // SUBMITTED ITEM VALIDATION
        // =====================================================

        private static void ValidateSubmittedItems(
            GoodsReceiptNote goodsReceiptNote,
            PurchaseOrder purchaseOrder)
        {
            if (goodsReceiptNote.Items == null ||
                goodsReceiptNote.Items.Count == 0)
            {
                throw new BusinessException(
                    "GRN item information is required.");
            }


            var duplicateItem =
                goodsReceiptNote.Items
                    .GroupBy(x =>
                        x.PurchaseOrderItemId)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateItem != null)
            {
                throw new BusinessException(
                    "Duplicate Purchase Order item found in GRN.");
            }


            foreach (var poItem in purchaseOrder.Items)
            {
                if (!goodsReceiptNote.Items.Any(x =>
                    x.PurchaseOrderItemId ==
                    poItem.Id))
                {
                    throw new BusinessException(
                        $"Receipt Status is required for {poItem.ItemName}.");
                }
            }
        }


        // =====================================================
        // PROCESS RECEIPT
        // =====================================================

        private static ReceiptCalculation ProcessReceipt(
            GoodsReceiptNoteItem submitted,
            PurchaseOrderItem poItem,
            decimal balanceQuantity)
        {
            if (submitted.ReceiptStatus ==
                GoodsReceiptStatus.NotReceived)
            {
                return new ReceiptCalculation
                {
                    ReceivedQuantity = 0,
                    PendingQuantity = balanceQuantity,
                    MaterialStatus = null,
                    HasReceived = false
                };
            }


            if (submitted.ReceiptStatus ==
                GoodsReceiptStatus.PartialReceived)
            {
                if (balanceQuantity <= 0)
                {
                    throw new BusinessException(
                        $"{poItem.ItemName} has no pending quantity to receive.");
                }


                if (submitted.ReceivedQuantity <= 0)
                {
                    throw new BusinessException(
                        $"Received quantity must be greater than zero " +
                        $"for {poItem.ItemName}.");
                }


                if (submitted.ReceivedQuantity >=
                    balanceQuantity)
                {
                    throw new BusinessException(
                        $"Partial Received quantity for {poItem.ItemName} " +
                        $"must be less than Balance Quantity " +
                        $"({balanceQuantity:0.###} {poItem.UnitName}). " +
                        $"Select Full Received when complete balance is received.");
                }


                return new ReceiptCalculation
                {
                    ReceivedQuantity =
                        submitted.ReceivedQuantity,

                    PendingQuantity =
                        balanceQuantity -
                        submitted.ReceivedQuantity,

                    MaterialStatus =
                        ValidateMaterialStatus(
                            submitted,
                            poItem.ItemName),

                    HasReceived =
                        true
                };
            }


            if (submitted.ReceiptStatus ==
                GoodsReceiptStatus.FullReceived)
            {
                if (balanceQuantity <= 0)
                {
                    throw new BusinessException(
                        $"{poItem.ItemName} has already been fully received.");
                }


                return new ReceiptCalculation
                {
                    ReceivedQuantity =
                        balanceQuantity,

                    PendingQuantity =
                        0,

                    MaterialStatus =
                        ValidateMaterialStatus(
                            submitted,
                            poItem.ItemName),

                    HasReceived =
                        true
                };
            }


            throw new BusinessException(
                $"Invalid Receipt Status selected for {poItem.ItemName}.");
        }


        // =====================================================
        // MATERIAL STATUS
        // =====================================================

        private static GoodsReceiptMaterialStatus
            ValidateMaterialStatus(
                GoodsReceiptNoteItem item,
                string itemName)
        {
            if (!item.MaterialStatus.HasValue)
            {
                throw new BusinessException(
                    $"Material Status is required for {itemName}.");
            }


            if (!Enum.IsDefined(
                typeof(GoodsReceiptMaterialStatus),
                item.MaterialStatus.Value))
            {
                throw new BusinessException(
                    $"Invalid Material Status selected for {itemName}.");
            }


            return item.MaterialStatus.Value;
        }


        // =====================================================
        // AT LEAST ONE RECEIVED
        // =====================================================

        private static void EnsureAtLeastOneReceived(
            bool hasReceivedItem)
        {
            if (!hasReceivedItem)
            {
                throw new BusinessException(
                    "At least one Purchase Order item must be " +
                    "Partial Received or Full Received.");
            }
        }


        // =====================================================
        // BALANCE
        // =====================================================

        private static decimal CalculateBalanceQuantity(
            decimal orderedQuantity,
            decimal previouslyReceivedQuantity)
        {
            var balance =
                orderedQuantity -
                previouslyReceivedQuantity;


            return balance < 0
                ? 0
                : balance;
        }


        private static decimal NormalizePreviousReceived(
            decimal quantity)
        {
            return quantity < 0
                ? 0
                : quantity;
        }


        // =====================================================
        // GRN CODE
        // =====================================================

        private async Task<string>
            GenerateGoodsReceiptNoteCodeAsync(
                DateTime grnDate)
        {
            var financialYear =
                GetFinancialYear(
                    grnDate);


            var prefix =
                $"AI/GRN/{financialYear}/";


            var lastCode =
                await _repository
                    .GetLastGoodsReceiptNoteCodeAsync(
                        prefix);


            var sequence =
                1;


            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var sequencePart =
                    lastCode.Substring(
                        prefix.Length);


                if (int.TryParse(
                    sequencePart,
                    out var lastSequence))
                {
                    sequence =
                        lastSequence + 1;
                }
            }


            return
                $"{prefix}{sequence:00000}";
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
                $"{startYear % 100:00}-" +
                $"{endYear % 100:00}";
        }


        private static string? NormalizeNullable(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        // =====================================================
        // INTERNAL RECEIPT CALCULATION RESULT
        // =====================================================

        private class ReceiptCalculation
        {
            public decimal ReceivedQuantity { get; set; }

            public decimal PendingQuantity { get; set; }

            public GoodsReceiptMaterialStatus?
                MaterialStatus
            { get; set; }

            public bool HasReceived { get; set; }
        }
    }
}