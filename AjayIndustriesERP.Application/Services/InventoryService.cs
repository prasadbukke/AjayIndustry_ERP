// ============================================================
// File: InventoryService.cs
// Module: Inventory / Stock Management
// Layer: Application - Service
//
// Purpose:
// Contains Phase 1 Inventory business rules.
//
// Responsibilities:
// - Create Opening Stock.
// - Generate Stock Transaction number.
// - Validate Item and Warehouse.
// - Calculate Current Stock.
// - Load Stock Ledger.
// - Create / Update Item Stock Settings.
//
// Opening Stock Rules:
// - One transaction can contain multiple Item/Warehouse lines.
// - Same Item + Warehouse cannot repeat inside the same
//   Opening Stock transaction.
// - Same Item + Warehouse CAN appear again in a future
//   Opening Stock transaction.
// - Opening Quantity must be greater than zero.
// - Rate is optional.
// - If Rate is supplied, it must be greater than zero.
// - Amount is calculated by the system.
// - QuantityOut is always zero.
//
// Stock Setting Rules:
// - Item + Warehouse are fixed after creation.
// - MinimumStock >= 0
// - ReorderLevel >= MinimumStock
// - MaximumStock >= ReorderLevel
// - ReorderQuantity > 0
// ============================================================

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Features.Inventory;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class InventoryService
        : IInventoryService
    {
        private readonly IInventoryRepository
            _inventoryRepository;

        private readonly IItemRepository
            _itemRepository;

        private readonly IWarehouseRepository
            _warehouseRepository;


        public InventoryService(
            IInventoryRepository inventoryRepository,
            IItemRepository itemRepository,
            IWarehouseRepository warehouseRepository)
        {
            _inventoryRepository =
                inventoryRepository;

            _itemRepository =
                itemRepository;

            _warehouseRepository =
                warehouseRepository;
        }


        // =====================================================
        // STOCK TRANSACTION DETAILS
        // =====================================================

        #region Stock Transaction

        public async Task<StockTransaction?>
            GetTransactionByIdAsync(
                int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Stock Transaction.");
            }


            return await _inventoryRepository
                .GetTransactionByIdAsync(
                    id);
        }

        #endregion


        // =====================================================
        // OPENING STOCK
        // =====================================================

        #region Opening Stock

        public async Task<StockTransaction>
            CreateOpeningStockAsync(
                StockTransaction stockTransaction)
        {
            if (stockTransaction == null)
            {
                throw new BusinessException(
                    "Invalid Opening Stock information.");
            }


            if (stockTransaction.TransactionDate == default)
            {
                throw new BusinessException(
                    "Opening Stock Date is required.");
            }


            if (stockTransaction.Items == null ||
                stockTransaction.Items.Count == 0)
            {
                throw new BusinessException(
                    "At least one Opening Stock item is required.");
            }


            ValidateDuplicateOpeningStockLines(
                stockTransaction.Items);


            var transactionCode =
                await GenerateStockTransactionCodeAsync(
                    stockTransaction.TransactionDate);


            var newTransaction =
                new StockTransaction
                {
                    Code =
                        transactionCode,

                    TransactionDate =
                        stockTransaction.TransactionDate,

                    TransactionType =
                        StockTransactionType.OpeningStock,

                    ReferenceType =
                        StockReferenceType.OpeningStock,

                    ReferenceId =
                        null,

                    ReferenceNumber =
                        null,

                    Remarks =
                        NormalizeNullable(
                            stockTransaction.Remarks)
                };


            foreach (var submittedLine
                     in stockTransaction.Items)
            {
                var item =
                    await GetValidatedItemAsync(
                        submittedLine.ItemId);


                var warehouse =
                    await GetValidatedWarehouseAsync(
                        submittedLine.WarehouseId);


                if (submittedLine.QuantityIn <= 0)
                {
                    throw new BusinessException(
                        $"Opening Stock quantity must be greater than zero " +
                        $"for {item.ItemName}.");
                }


                decimal? rate =
                    submittedLine.Rate;


                if (rate.HasValue &&
                    rate.Value <= 0)
                {
                    throw new BusinessException(
                        $"Rate must be greater than zero for {item.ItemName}.");
                }


                decimal? amount =
                    null;


                if (rate.HasValue)
                {
                    amount =
                        submittedLine.QuantityIn *
                        rate.Value;
                }


                newTransaction.Items.Add(
                    new StockTransactionLine
                    {
                        ItemId =
                            item.ItemId,

                        WarehouseId =
                            warehouse.WarehouseId,

                        QuantityIn =
                            submittedLine.QuantityIn,

                        QuantityOut =
                            0,

                        Rate =
                            rate,

                        Amount =
                            amount,

                        Remarks =
                            NormalizeNullable(
                                submittedLine.Remarks)
                    });
            }


            await _inventoryRepository
                .AddTransactionAsync(
                    newTransaction);


            return newTransaction;
        }

        #endregion

        // ============================================================
        // CURRENT STOCK LIST
        // ============================================================

        public async Task<List<CurrentStockResult>>
            GetCurrentStockListAsync()
        {
            return await _inventoryRepository
                .GetCurrentStockListAsync();
        }


        // =====================================================
        // CURRENT STOCK
        // =====================================================

        #region Current Stock

        public async Task<decimal>
            GetCurrentStockAsync(
                int itemId,
                int? warehouseId = null)
        {
            await GetValidatedItemAsync(
                itemId);


            if (warehouseId.HasValue)
            {
                await GetValidatedWarehouseAsync(
                    warehouseId.Value);
            }


            return await _inventoryRepository
                .GetCurrentStockAsync(
                    itemId,
                    warehouseId);
        }

        #endregion


        // =====================================================
        // STOCK LEDGER
        // =====================================================

        #region Stock Ledger

        public async Task<List<StockTransactionLine>>
            GetLedgerAsync(
                int itemId,
                int? warehouseId = null)
        {
            await GetValidatedItemAsync(
                itemId);


            if (warehouseId.HasValue)
            {
                await GetValidatedWarehouseAsync(
                    warehouseId.Value);
            }


            return await _inventoryRepository
                .GetLedgerAsync(
                    itemId,
                    warehouseId);
        }

        #endregion


        // =====================================================
        // ITEM STOCK SETTINGS
        // =====================================================

        #region Item Stock Settings

        public async Task<List<ItemStockSetting>>
            GetStockSettingsAsync()
        {
            return await _inventoryRepository
                .GetStockSettingsAsync();
        }


        public async Task<List<ItemStockSetting>>
            GetStockSettingsByItemAsync(
                int itemId)
        {
            await GetValidatedItemAsync(
                itemId);


            return await _inventoryRepository
                .GetStockSettingsByItemAsync(
                    itemId);
        }


        public async Task<ItemStockSetting?>
            GetStockSettingAsync(
                int itemId,
                int warehouseId)
        {
            await GetValidatedItemAsync(
                itemId);


            await GetValidatedWarehouseAsync(
                warehouseId);


            return await _inventoryRepository
                .GetStockSettingAsync(
                    itemId,
                    warehouseId);
        }


        public async Task<ItemStockSetting?>
            GetStockSettingByIdAsync(
                int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Stock Setting.");
            }


            return await _inventoryRepository
                .GetStockSettingForUpdateAsync(
                    id);
        }


        public async Task<ItemStockSetting>
            CreateStockSettingAsync(
                ItemStockSetting itemStockSetting)
        {
            if (itemStockSetting == null)
            {
                throw new BusinessException(
                    "Invalid Stock Setting information.");
            }


            var item =
                await GetValidatedItemAsync(
                    itemStockSetting.ItemId);


            var warehouse =
                await GetValidatedWarehouseAsync(
                    itemStockSetting.WarehouseId);


            ValidateStockSettingValues(
                itemStockSetting);


            var existing =
                await _inventoryRepository
                    .GetStockSettingAsync(
                        item.ItemId,
                        warehouse.WarehouseId);


            if (existing != null)
            {
                throw new BusinessException(
                    $"Stock Setting already exists for " +
                    $"{item.ItemName} in {warehouse.WarehouseName}.");
            }


            var newSetting =
                new ItemStockSetting
                {
                    ItemId =
                        item.ItemId,

                    WarehouseId =
                        warehouse.WarehouseId,

                    MinimumStock =
                        itemStockSetting.MinimumStock,

                    MaximumStock =
                        itemStockSetting.MaximumStock,

                    ReorderLevel =
                        itemStockSetting.ReorderLevel,

                    ReorderQuantity =
                        itemStockSetting.ReorderQuantity
                };


            await _inventoryRepository
                .AddStockSettingAsync(
                    newSetting);


            return newSetting;
        }


        public async Task<ItemStockSetting>
            UpdateStockSettingAsync(
                ItemStockSetting itemStockSetting)
        {
            if (itemStockSetting == null ||
                itemStockSetting.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Stock Setting information.");
            }


            var existing =
                await _inventoryRepository
                    .GetStockSettingForUpdateAsync(
                        itemStockSetting.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Stock Setting not found.");
            }


            // =================================================
            // ITEM + WAREHOUSE CANNOT CHANGE AFTER CREATION
            // =================================================

            if (itemStockSetting.ItemId !=
                    existing.ItemId ||
                itemStockSetting.WarehouseId !=
                    existing.WarehouseId)
            {
                throw new BusinessException(
                    "Item and Warehouse cannot be changed " +
                    "after Stock Setting creation.");
            }


            await GetValidatedItemAsync(
                existing.ItemId);


            await GetValidatedWarehouseAsync(
                existing.WarehouseId);


            ValidateStockSettingValues(
                itemStockSetting);


            existing.MinimumStock =
                itemStockSetting.MinimumStock;

            existing.MaximumStock =
                itemStockSetting.MaximumStock;

            existing.ReorderLevel =
                itemStockSetting.ReorderLevel;

            existing.ReorderQuantity =
                itemStockSetting.ReorderQuantity;

            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";


            await _inventoryRepository
                .UpdateStockSettingAsync(
                    existing);


            return existing;
        }

        #endregion


        // =====================================================
        // ITEM VALIDATION
        // =====================================================

        #region Item Validation

        private async Task<Item>
            GetValidatedItemAsync(
                int itemId)
        {
            if (itemId <= 0)
            {
                throw new BusinessException(
                    "Please select a valid Item.");
            }


            var item =
                await _itemRepository
                    .GetByIdAsync(
                        itemId);


            if (item == null)
            {
                throw new BusinessException(
                    "Item not found.");
            }


            /*
             * ItemRepository.GetByIdAsync excludes deleted Items,
             * but does not exclude inactive Items.
             *
             * Inventory therefore validates IsActive here.
             */
            if (!item.IsActive)
            {
                throw new BusinessException(
                    $"Item {item.ItemName} is not active.");
            }


            return item;
        }

        #endregion


        // =====================================================
        // WAREHOUSE VALIDATION
        // =====================================================

        #region Warehouse Validation

        private async Task<Warehouse>
            GetValidatedWarehouseAsync(
                int warehouseId)
        {
            if (warehouseId <= 0)
            {
                throw new BusinessException(
                    "Please select a valid Warehouse.");
            }


            var warehouse =
                await _warehouseRepository
                    .GetByIdAsync(
                        warehouseId);


            if (warehouse == null)
            {
                throw new BusinessException(
                    "Warehouse not found.");
            }


            /*
             * WarehouseRepository.GetByIdAsync excludes deleted
             * Warehouses, but does not exclude inactive Warehouses.
             */
            if (!warehouse.IsActive)
            {
                throw new BusinessException(
                    $"Warehouse {warehouse.WarehouseName} is not active.");
            }


            return warehouse;
        }

        #endregion


        // =====================================================
        // OPENING STOCK DUPLICATE LINE VALIDATION
        // =====================================================

        #region Opening Stock Validation

        private static void ValidateDuplicateOpeningStockLines(
            ICollection<StockTransactionLine> lines)
        {
            var duplicate =
                lines
                    .GroupBy(x =>
                        new
                        {
                            x.ItemId,
                            x.WarehouseId
                        })
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicate != null)
            {
                throw new BusinessException(
                    "The same Item and Warehouse combination " +
                    "cannot be entered more than once in the " +
                    "same Opening Stock transaction.");
            }
        }

        #endregion


        // =====================================================
        // STOCK SETTING VALIDATION
        // =====================================================

        #region Stock Setting Validation

        private static void ValidateStockSettingValues(
            ItemStockSetting itemStockSetting)
        {
            if (itemStockSetting.MinimumStock < 0)
            {
                throw new BusinessException(
                    "Minimum Stock cannot be negative.");
            }


            if (itemStockSetting.ReorderLevel <
                itemStockSetting.MinimumStock)
            {
                throw new BusinessException(
                    "Reorder Level must be greater than or equal " +
                    "to Minimum Stock.");
            }


            if (itemStockSetting.MaximumStock <
                itemStockSetting.ReorderLevel)
            {
                throw new BusinessException(
                    "Maximum Stock must be greater than or equal " +
                    "to Reorder Level.");
            }


            if (itemStockSetting.ReorderQuantity <= 0)
            {
                throw new BusinessException(
                    "Reorder Quantity must be greater than zero.");
            }
        }

        #endregion


        // =====================================================
        // STOCK TRANSACTION CODE
        // =====================================================

        #region Transaction Code

        private async Task<string>
            GenerateStockTransactionCodeAsync(
                DateTime transactionDate)
        {
            var financialYear =
                GetFinancialYear(
                    transactionDate);


            var prefix =
                $"AI/STK/{financialYear}/";


            var lastCode =
                await _inventoryRepository
                    .GetLastTransactionCodeAsync(
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

        #endregion


        // =====================================================
        // NORMALIZATION
        // =====================================================

        #region Normalization

        private static string? NormalizeNullable(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }

        #endregion
    }
}