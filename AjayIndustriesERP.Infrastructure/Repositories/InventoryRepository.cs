// ============================================================
// File: InventoryRepository.cs
// Module: Inventory / Stock Management
// Layer: Infrastructure - Repository
//
// Purpose:
// Handles Entity Framework Core database operations required
// by the Inventory / Stock Management module.
//
// Responsibilities:
// - Read Stock Transaction details.
// - Add Stock Transactions.
// - Calculate current Item stock.
// - Load Item Stock Ledger.
// - Read / Add / Update Item Stock Settings.
// - Retrieve last Stock Transaction Code.
//
// Important:
// - Business rules belong in InventoryService.
// - Current stock is calculated live from StockTransactionLines.
// - Read-only queries use AsNoTracking().
// - Soft-deleted / inactive stock movements are excluded from
//   stock balance and ledger calculations.
// ============================================================

using AjayIndustriesERP.Application.Features.Inventory;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class InventoryRepository
        : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;


        public InventoryRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // STOCK TRANSACTION DETAILS
        // =====================================================

        #region Stock Transaction

        public async Task<StockTransaction?>
            GetTransactionByIdAsync(
                int id)
        {
            return await _context.StockTransactions
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Item)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Warehouse)
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // ADD STOCK TRANSACTION
        // =====================================================

        public async Task AddTransactionAsync(
            StockTransaction stockTransaction)
        {
            await _context.StockTransactions
                .AddAsync(
                    stockTransaction);

            await _context.SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // CURRENT STOCK
        // =====================================================

        #region Current Stock

        public async Task<decimal> GetCurrentStockAsync(
            int itemId,
            int? warehouseId = null)
        {
            var query =
                _context.StockTransactionLines
                    .AsNoTracking()
                    .Where(x =>
                        x.ItemId == itemId &&
                        !x.IsDeleted &&
                        x.IsActive &&
                        !x.StockTransaction.IsDeleted &&
                        x.StockTransaction.IsActive);


            if (warehouseId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.WarehouseId ==
                        warehouseId.Value);
            }


            var quantityIn =
                await query.SumAsync(x =>
                    (decimal?)x.QuantityIn)
                ?? 0;


            var quantityOut =
                await query.SumAsync(x =>
                    (decimal?)x.QuantityOut)
                ?? 0;


            return quantityIn - quantityOut;
        }

        #endregion

        public async Task<List<CurrentStockResult>>
    GetCurrentStockListAsync()
        {
            var stockQuery =
                _context.StockTransactionLines
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        !x.StockTransaction.IsDeleted &&
                        x.StockTransaction.IsActive)
                    .GroupBy(x =>
                        new
                        {
                            x.ItemId,
                            x.WarehouseId,

                            x.Item.ItemCode,
                            x.Item.ItemName,
                            x.Item.Uom.UomName,

                            x.Warehouse.WarehouseCode,
                            x.Warehouse.WarehouseName
                        })
                    .Select(x =>
                        new
                        {
                            x.Key.ItemId,
                            x.Key.WarehouseId,

                            x.Key.ItemCode,
                            x.Key.ItemName,
                            UnitName =
                              x.Key.UomName,
                            x.Key.WarehouseCode,
                            x.Key.WarehouseName,

                            CurrentStock =
                                x.Sum(y =>
                                    y.QuantityIn -
                                    y.QuantityOut)
                        });


            var query =
                from stock in stockQuery

                join setting
                    in _context.ItemStockSettings
                        .AsNoTracking()
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)

                    on new
                    {
                        stock.ItemId,
                        stock.WarehouseId
                    }
                    equals new
                    {
                        setting.ItemId,
                        setting.WarehouseId
                    }
                    into settingGroup

                from setting
                    in settingGroup.DefaultIfEmpty()

                orderby
                    stock.ItemName,
                    stock.WarehouseName

                select new CurrentStockResult
                {
                    ItemId =
                        stock.ItemId,

                    ItemCode =
                        stock.ItemCode,

                    ItemName =
                        stock.ItemName,

                    /*
                     * UOM display will be connected after confirming
                     * the existing UOM entity/property naming.
                     */
                    UnitName =
    stock.UnitName,

                    WarehouseId =
                        stock.WarehouseId,

                    WarehouseCode =
                        stock.WarehouseCode,

                    WarehouseName =
                        stock.WarehouseName,

                    CurrentStock =
                        stock.CurrentStock,

                    MinimumStock =
                        setting != null
                            ? setting.MinimumStock
                            : null,

                    MaximumStock =
                        setting != null
                            ? setting.MaximumStock
                            : null,

                    ReorderLevel =
                        setting != null
                            ? setting.ReorderLevel
                            : null,

                    ReorderQuantity =
                        setting != null
                            ? setting.ReorderQuantity
                            : null
                };


            return await query.ToListAsync();
        }

        // =====================================================
        // STOCK LEDGER
        // =====================================================

        #region Stock Ledger

        public async Task<List<StockTransactionLine>>
            GetLedgerAsync(
                int itemId,
                int? warehouseId = null)
        {
            var query =
                _context.StockTransactionLines
                    .AsNoTracking()
                    .Where(x =>
                        x.ItemId == itemId &&
                        !x.IsDeleted &&
                        x.IsActive &&
                        !x.StockTransaction.IsDeleted &&
                        x.StockTransaction.IsActive);


            if (warehouseId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.WarehouseId ==
                        warehouseId.Value);
            }


            return await query
                .Include(x =>
                    x.StockTransaction)
               .Include(x => x.Item)
    .ThenInclude(x => x.Uom)
                .Include(x =>
                    x.Warehouse)
                .OrderBy(x =>
                    x.StockTransaction.TransactionDate)
                .ThenBy(x =>
                    x.StockTransactionId)
                .ThenBy(x =>
                    x.Id)
                .ToListAsync();
        }

        #endregion


        // =====================================================
        // ITEM STOCK SETTINGS
        // =====================================================

        #region Item Stock Settings

        public async Task<List<ItemStockSetting>>
            GetStockSettingsAsync()
        {
            return await _context.ItemStockSettings
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.Warehouse)
                .OrderBy(x =>
                    x.Item.ItemName)
                .ThenBy(x =>
                    x.Warehouse.WarehouseName)
                .ToListAsync();
        }


        public async Task<List<ItemStockSetting>>
            GetStockSettingsByItemAsync(
                int itemId)
        {
            return await _context.ItemStockSettings
                .AsNoTracking()
                .Where(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.Warehouse)
                .OrderBy(x =>
                    x.Warehouse.WarehouseName)
                .ToListAsync();
        }


        public async Task<ItemStockSetting?>
            GetStockSettingAsync(
                int itemId,
                int warehouseId)
        {
            return await _context.ItemStockSettings
                .AsNoTracking()
                .Where(x =>
                    x.ItemId == itemId &&
                    x.WarehouseId == warehouseId &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.Warehouse)
                .FirstOrDefaultAsync();
        }


        public async Task<ItemStockSetting?>
            GetStockSettingForUpdateAsync(
                int id)
        {
            return await _context.ItemStockSettings
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .FirstOrDefaultAsync();
        }


        public async Task AddStockSettingAsync(
            ItemStockSetting itemStockSetting)
        {
            await _context.ItemStockSettings
                .AddAsync(
                    itemStockSetting);

            await _context.SaveChangesAsync();
        }


        public async Task UpdateStockSettingAsync(
            ItemStockSetting itemStockSetting)
        {
            _context.ItemStockSettings
                .Update(
                    itemStockSetting);

            await _context.SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // LAST STOCK TRANSACTION CODE
        // =====================================================

        #region Transaction Number

        public async Task<string?>
            GetLastTransactionCodeAsync(
                string prefix)
        {
            return await _context.StockTransactions
                .AsNoTracking()
                .Where(x =>
                    x.Code.StartsWith(
                        prefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}