// ============================================================
// File: IInventoryService.cs
// Module: Inventory / Stock Management
// Layer: Application - Service Interface
//
// Purpose:
// Defines Inventory business operations used by the Web layer.
//
// Responsibilities:
// - Create Opening Stock.
// - Read Stock Transaction details.
// - Calculate Current Stock.
// - Load Item Stock Ledger.
// - Maintain Item + Warehouse Stock Settings.
//
// Important:
// - Actual Current Stock is NOT stored separately.
// - Current Stock is calculated from StockTransactionLines.
// - Opening Stock can be entered more than once for the same
//   Item + Warehouse.
// - Opening Stock Rate is optional.
// - Web ViewModels remain in the Web layer.
// - Item Master and Warehouse Master data access is reused
//   through their existing repositories/services.
// ============================================================

using AjayIndustriesERP.Application.Features.Inventory;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInventoryService
    {
        // =====================================================
        // STOCK TRANSACTION
        // =====================================================

        #region Stock Transaction

        /// <summary>
        /// Returns one Stock Transaction including
        /// Item and Warehouse line details.
        /// </summary>
        Task<StockTransaction?> GetTransactionByIdAsync(
            int id);

        #endregion


        // =====================================================
        // OPENING STOCK
        // =====================================================

        #region Opening Stock

        /// <summary>
        /// Creates an Opening Stock transaction.
        ///
        /// Multiple Opening Stock transactions are allowed
        /// for the same Item + Warehouse.
        ///
        /// Rate is optional.
        /// </summary>
        Task<StockTransaction> CreateOpeningStockAsync(
            StockTransaction stockTransaction);

        #endregion

        Task<List<CurrentStockResult>>
    GetCurrentStockListAsync();


        // =====================================================
        // CURRENT STOCK
        // =====================================================

        #region Current Stock

        /// <summary>
        /// Returns current stock for one Item.
        ///
        /// When warehouseId is null:
        ///     Total stock across all Warehouses is returned.
        ///
        /// When warehouseId is supplied:
        ///     Stock for that Warehouse only is returned.
        /// </summary>
        Task<decimal> GetCurrentStockAsync(
            int itemId,
            int? warehouseId = null);

        #endregion


        // =====================================================
        // STOCK LEDGER
        // =====================================================

        #region Stock Ledger

        /// <summary>
        /// Returns Stock Ledger movements for one Item.
        ///
        /// Warehouse filter is optional.
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
        /// Returns active Stock Settings for one Item.
        /// </summary>
        Task<List<ItemStockSetting>>
            GetStockSettingsByItemAsync(
                int itemId);


        /// <summary>
        /// Returns Stock Setting for one
        /// Item + Warehouse combination.
        /// </summary>
        Task<ItemStockSetting?>
            GetStockSettingAsync(
                int itemId,
                int warehouseId);


        /// <summary>
        /// Returns one Stock Setting by its Id.
        ///
        /// Used while opening the Edit screen.
        /// </summary>
        Task<ItemStockSetting?>
            GetStockSettingByIdAsync(
                int id);


        /// <summary>
        /// Creates Stock planning settings for
        /// one Item + Warehouse.
        /// </summary>
        Task<ItemStockSetting> CreateStockSettingAsync(
            ItemStockSetting itemStockSetting);


        /// <summary>
        /// Updates existing Stock planning settings.
        /// </summary>
        Task<ItemStockSetting> UpdateStockSettingAsync(
            ItemStockSetting itemStockSetting);

        #endregion
    }
}