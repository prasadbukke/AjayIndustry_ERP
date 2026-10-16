// ============================================================
// File: StockSettingController.cs
// Module: Inventory / Stock Management
//
// Purpose:
// Handles Item + Warehouse Stock Settings.
//
// Responsibilities:
// - Index
// - Create GET / POST
// - Edit GET / POST
// - Load active Item / Warehouse masters
// - Web ViewModel <-> Domain mapping
//
// Important:
// - No Inventory business logic belongs here.
// - Final validation remains in InventoryService.
// ============================================================

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class StockSettingController : Controller
    {
        private readonly IInventoryService
            _inventoryService;

        private readonly IItemService
            _itemService;

        private readonly IWarehouseService
            _warehouseService;


        public StockSettingController(
            IInventoryService inventoryService,
            IItemService itemService,
            IWarehouseService warehouseService)
        {
            _inventoryService =
                inventoryService;

            _itemService =
                itemService;

            _warehouseService =
                warehouseService;
        }


        // =====================================================
        // INDEX
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stockSettings =
                await _inventoryService
                    .GetStockSettingsAsync();


            return View(
                stockSettings);
        }


        // =====================================================
        // CREATE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadMastersAsync();


            return View(
                new StockSettingCreateViewModel());
        }


        // =====================================================
        // CREATE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            StockSettingCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMastersAsync();

                return View(model);
            }


            try
            {
                await _inventoryService
                    .CreateStockSettingAsync(
                        MapToDomain(model));


                TempData["SuccessMessage"] =
                    "Stock Setting created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadMastersAsync();


                return View(model);
            }
        }


        // =====================================================
        // EDIT - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            try
            {
                var stockSetting =
                    await _inventoryService
                        .GetStockSettingByIdAsync(
                            id);


                if (stockSetting == null)
                {
                    return NotFound();
                }


                await LoadMastersAsync();


                return View(
                    MapToViewModel(
                        stockSetting));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Index));
            }
        }


        // =====================================================
        // EDIT - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            StockSettingCreateViewModel model)
        {
            if (id <= 0 ||
                id != model.Id)
            {
                TempData["ErrorMessage"] =
                    "Invalid Stock Setting.";


                return RedirectToAction(
                    nameof(Index));
            }


            if (!ModelState.IsValid)
            {
                await LoadMastersAsync();

                return View(model);
            }


            try
            {
                await _inventoryService
                    .UpdateStockSettingAsync(
                        MapToDomain(model));


                TempData["SuccessMessage"] =
                    "Stock Setting updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadMastersAsync();


                return View(model);
            }
        }


        // =====================================================
        // MASTER DATA
        // =====================================================

        private async Task LoadMastersAsync()
        {
            var items =
                await _itemService
                    .GetAllAsync();


            var warehouses =
                await _warehouseService
                    .GetAllAsync();


            ViewBag.Items =
                items
                    .Where(x =>
                        x.IsActive)
                    .OrderBy(x =>
                        x.ItemName)
                    .ThenBy(x =>
                        x.ItemCode)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.ItemId.ToString(),

                            Text =
                                $"{x.ItemCode} - {x.ItemName}"
                        })
                    .ToList();


            ViewBag.Warehouses =
                warehouses
                    .Where(x =>
                        x.IsActive)
                    .OrderBy(x =>
                        x.WarehouseName)
                    .ThenBy(x =>
                        x.WarehouseCode)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.WarehouseId.ToString(),

                            Text =
                                $"{x.WarehouseCode} - {x.WarehouseName}"
                        })
                    .ToList();
        }


        // =====================================================
        // WEB -> DOMAIN
        // =====================================================

        private static ItemStockSetting MapToDomain(
            StockSettingCreateViewModel model)
        {
            return new ItemStockSetting
            {
                Id =
                    model.Id,

                ItemId =
                    model.ItemId,

                WarehouseId =
                    model.WarehouseId,

                MinimumStock =
                    model.MinimumStock,

                ReorderLevel =
                    model.ReorderLevel,

                MaximumStock =
                    model.MaximumStock,

                ReorderQuantity =
                    model.ReorderQuantity
            };
        }


        // =====================================================
        // DOMAIN -> WEB
        // =====================================================

        private static StockSettingCreateViewModel
            MapToViewModel(
                ItemStockSetting stockSetting)
        {
            return new StockSettingCreateViewModel
            {
                Id =
                    stockSetting.Id,

                ItemId =
                    stockSetting.ItemId,

                WarehouseId =
                    stockSetting.WarehouseId,

                MinimumStock =
                    stockSetting.MinimumStock,

                ReorderLevel =
                    stockSetting.ReorderLevel,

                MaximumStock =
                    stockSetting.MaximumStock,

                ReorderQuantity =
                    stockSetting.ReorderQuantity
            };
        }
    }
}