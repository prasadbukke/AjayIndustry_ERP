// ============================================================
// File: InventoryController.cs
// Module: Inventory / Stock Management
// Layer: Web - Controller
//
// Purpose:
// Handles Inventory Web requests.
//
// Phase 1 Responsibilities:
// - Opening Stock Create GET
// - Opening Stock Create POST
// - Stock Transaction Details
// - Web ViewModel -> Domain mapping
// - Load active Item and Warehouse masters
// - Display business validation messages
//
// Important:
// - No Inventory business logic belongs in Controller.
// - Final validation remains in InventoryService.
// - Only active Items and Warehouses are shown for selection.
// ============================================================

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService
            _inventoryService;

        private readonly IItemService
            _itemService;

        private readonly IWarehouseService
            _warehouseService;


        public InventoryController(
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
        // INDEX - CURRENT STOCK
        // =====================================================

        #region Current Stock

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentStock =
                await _inventoryService
                    .GetCurrentStockListAsync();


            return View(
                currentStock);
        }

        #endregion


        // =====================================================
        // CREATE OPENING STOCK - GET
        // =====================================================

        #region Create Opening Stock

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOpeningStockMastersAsync();


            var model =
                new OpeningStockCreateViewModel
                {
                    TransactionDate =
                        DateTime.Today
                };


            return View(model);
        }


        // =====================================================
        // CREATE OPENING STOCK - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            OpeningStockCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadOpeningStockMastersAsync();

                return View(model);
            }


            try
            {
                var created =
                    await _inventoryService
                        .CreateOpeningStockAsync(
                            MapOpeningStockToDomain(
                                model));


                TempData["SuccessMessage"] =
                    $"Opening Stock {created.Code} created successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            created.Id
                    });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadOpeningStockMastersAsync();


                return View(model);
            }
        }

        #endregion


        // =====================================================
        // STOCK TRANSACTION DETAILS
        // =====================================================

        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            try
            {
                var stockTransaction =
                    await _inventoryService
                        .GetTransactionByIdAsync(
                            id);


                if (stockTransaction == null)
                {
                    return NotFound();
                }


                return View(
                    stockTransaction);
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Create));
            }
        }

        #endregion


        // =====================================================
        // STOCK LEDGER
        // =====================================================

        #region Stock Ledger

        [HttpGet]
        public async Task<IActionResult> Ledger(
            int? itemId,
            int? warehouseId)
        {
            await LoadLedgerMastersAsync();


            var selectedItemId =
                itemId.HasValue &&
                itemId.Value > 0
                    ? itemId
                    : null;


            var selectedWarehouseId =
                warehouseId.HasValue &&
                warehouseId.Value > 0
                    ? warehouseId
                    : null;


            var model =
                new StockLedgerViewModel
                {
                    ItemId =
                        selectedItemId,

                    WarehouseId =
                        selectedWarehouseId
                };


            /*
             * Ledger is loaded only after Item selection.
             */

            if (!selectedItemId.HasValue)
            {
                return View(model);
            }


            try
            {
                var ledger =
                    await _inventoryService
                        .GetLedgerAsync(
                            selectedItemId.Value,
                            selectedWarehouseId);


                decimal runningBalance =
                    0;


                foreach (var line in ledger)
                {
                    runningBalance +=
                        line.QuantityIn -
                        line.QuantityOut;


                    model.Entries.Add(
                        new StockLedgerLineViewModel
                        {
                            TransactionId =
                                line.StockTransactionId,

                            TransactionCode =
                                line.StockTransaction.Code,

                            TransactionDate =
                                line.StockTransaction
                                    .TransactionDate,

                            TransactionType =
                                line.StockTransaction
                                    .TransactionType
                                    .ToString(),

                            ReferenceNumber =
                                line.StockTransaction
                                    .ReferenceNumber,

                            ItemId =
                                line.ItemId,

                            ItemCode =
                                line.Item.ItemCode,

                            ItemName =
                                line.Item.ItemName,

                            UnitName =
                                line.Item.Uom?.UomName
                                ?? string.Empty,

                            WarehouseId =
                                line.WarehouseId,

                            WarehouseCode =
                                line.Warehouse
                                    .WarehouseCode,

                            WarehouseName =
                                line.Warehouse
                                    .WarehouseName,

                            QuantityIn =
                                line.QuantityIn,

                            QuantityOut =
                                line.QuantityOut,

                            RunningBalance =
                                runningBalance,

                            Rate =
                                line.Rate,

                            Amount =
                                line.Amount,

                            Remarks =
                                line.Remarks
                        });
                }


                return View(model);
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(model);
            }
        }

        #endregion

        // =====================================================
        // LOAD OPENING STOCK MASTER DATA
        // =====================================================

        #region Master Data

        private async Task LoadOpeningStockMastersAsync()
        {
            var items =
                await _itemService
                    .GetAllAsync();


            var warehouses =
                await _warehouseService
                    .GetAllAsync();


            /*
             * Existing Item / Warehouse services return
             * non-deleted records.
             *
             * Opening Stock selection must show only
             * currently active masters.
             */

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
        // LOAD STOCK LEDGER MASTER DATA
        // =====================================================

        private async Task LoadLedgerMastersAsync()
        {
            var items =
                await _itemService
                    .GetAllAsync();


            var warehouses =
                await _warehouseService
                    .GetAllAsync();


            /*
             * Ledger is historical information.
             *
             * Therefore inactive but non-deleted Item / Warehouse
             * records are also available for filtering.
             */

            ViewBag.LedgerItems =
                items
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


            ViewBag.LedgerWarehouses =
                warehouses
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

        #endregion


        // =====================================================
        // WEB -> DOMAIN
        // =====================================================

        #region Mapping

        private static StockTransaction
            MapOpeningStockToDomain(
                OpeningStockCreateViewModel model)
        {
            var stockTransaction =
                new StockTransaction
                {
                    TransactionDate =
                        model.TransactionDate,

                    Remarks =
                        model.Remarks
                };


            foreach (var item in model.Items)
            {
                stockTransaction.Items.Add(
                    new StockTransactionLine
                    {
                        ItemId =
                            item.ItemId,

                        WarehouseId =
                            item.WarehouseId,

                        QuantityIn =
                            item.Quantity,

                        Rate =
                            item.Rate,

                        Remarks =
                            item.Remarks
                    });
            }


            return stockTransaction;
        }

        #endregion
    }
}