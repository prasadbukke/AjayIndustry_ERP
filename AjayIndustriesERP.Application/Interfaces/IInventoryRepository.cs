// ============================================================
// File: IInventoryRepository.cs
// Module: Inventory / Stock Management
// Layer: Application - Repository Interface
//
// Purpose:
// Defines database operations required by the
// Inventory / Stock Management module.
//
// Responsibilities:
// - Read Stock Transaction details.
// - Save Stock Transactions.
// - Calculate current stock.
// - Load Item Stock Ledger.
// - Maintain Item + Warehouse Stock Settings.
// - Generate Stock Transaction number.
//
// Important:
// - Business rules do NOT belong in this repository.
// - Current Stock is NOT stored in a separate table.
// - Current Stock is calculated from active StockTransactionLines:
//
//      Current Stock = SUM(QuantityIn) - SUM(QuantityOut)
//
// - Item Master operations use IItemRepository.
// - Warehouse Master operations use IWarehouseRepository.
// ============================================================

using AjayIndustriesERP.Application.Features.Inventory;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInventoryRepository
    {
        // =====================================================
        // STOCK TRANSACTION
        // =====================================================

        #region Stock Transaction

        /// <summary>
        /// Returns one Stock Transaction including its
        /// Item and Warehouse lines.
        /// </summary>
        Task<StockTransaction?> GetTransactionByIdAsync(
            int id);


        /// <summary>
        /// Adds a new Stock Transaction including its lines.
        /// </summary>
        Task AddTransactionAsync(
            StockTransaction stockTransaction);

        #endregion


        // =====================================================
        // CURRENT STOCK
        // =====================================================

        #region Current Stock

        /// <summary>
        /// Calculates current stock for one Item.
        ///
        /// If warehouseId is supplied, stock is calculated
        /// only for that Warehouse.
        ///
        /// If warehouseId is null, stock is calculated across
        /// all Warehouses.
        ///
        /// Current Stock =
        /// SUM(QuantityIn) - SUM(QuantityOut)
        /// </summary>
        Task<decimal> GetCurrentStockAsync(
            int itemId,
            int? warehouseId = null);

        #endregion

        Task<List<CurrentStockResult>> GetCurrentStockListAsync();

        // =====================================================
        // STOCK LEDGER
        // =====================================================

        #region Stock Ledger

        /// <summary>
        /// Returns active Stock Transaction Lines for an Item.
        ///
        /// If warehouseId is supplied, only movements for that
        /// Warehouse are returned.
        ///
        /// Used for Item Stock Ledger / history.
        /// </summary>
        Task<List<StockTransactionLine>> GetLedgerAsync(
            int itemId,
            int? warehouseId = null);

        #endregion


        // =====================================================
        // ITEM STOCK SETTINGS
        // =====================================================

        #region Item Stock Settings

        /// <summary>
        /// Returns all active Item Stock Settings.
        /// </summary>
        Task<List<ItemStockSetting>>
            GetStockSettingsAsync();


        /// <summary>
        /// Returns all active Stock Settings for one Item.
        /// </summary>
        Task<List<ItemStockSetting>>
            GetStockSettingsByItemAsync(
                int itemId);


        /// <summary>
        /// Returns Stock Setting for one specific
        /// Item + Warehouse combination.
        /// </summary>
        Task<ItemStockSetting?>
            GetStockSettingAsync(
                int itemId,
                int warehouseId);


        /// <summary>
        /// Returns tracked Item Stock Setting for update.
        /// </summary>
        Task<ItemStockSetting?>
            GetStockSettingForUpdateAsync(
                int id);


        /// <summary>
        /// Adds a new Item Stock Setting.
        /// </summary>
        Task AddStockSettingAsync(
            ItemStockSetting itemStockSetting);


        /// <summary>
        /// Updates an existing Item Stock Setting.
        /// </summary>
        Task UpdateStockSettingAsync(
            ItemStockSetting itemStockSetting);

        #endregion


        // =====================================================
        // STOCK TRANSACTION NUMBER
        // =====================================================

        #region Transaction Number

        /// <summary>
        /// Returns the last Stock Transaction Code for the
        /// supplied financial-year prefix.
        ///
        /// Used by Inventory Service for generating the next
        /// Stock Transaction number.
        /// </summary>
        Task<string?> GetLastTransactionCodeAsync(
            string prefix);

        #endregion
    }
}