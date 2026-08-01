/*
==============================================================

File : WarehouseController.cs

Purpose :
Handles Warehouse UI requests.

==============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        #region Warehouse List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var warehouses =
                    await _warehouseService.SearchAsync(searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;

                ViewBag.TotalRecords = warehouses.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(warehouses);
            }

            var result =
                await _warehouseService.GetPagedAsync(pageNumber, pageSize);

            ViewBag.SearchText = searchText;
            ViewBag.PageNumber = result.PageNumber;
            ViewBag.PageSize = result.PageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalRecords = result.TotalRecords;
            ViewBag.HasPrevious = result.HasPrevious;
            ViewBag.HasNext = result.HasNext;

            return View(result.Items);
        }

        #endregion

        #region Create Warehouse

        [HttpGet]
        public IActionResult Create()
        {
            return View(new WarehouseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseViewModel model)
        {
            ModelState.Remove(nameof(WarehouseViewModel.WarehouseCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var warehouse = new Warehouse
                {
                    WarehouseCode = model.WarehouseCode ?? string.Empty,
                    WarehouseName = model.WarehouseName,
                    Description = model.Description,
                    WarehouseType = model.WarehouseType,
                    IsDefault = model.IsDefault,
                    IsActive = model.IsActive
                };

                await _warehouseService.CreateAsync(warehouse);

                TempData["Success"] = "Warehouse created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return View(model);
            }
        }

        #endregion

        #region Warehouse Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var warehouse =
                await _warehouseService.GetByIdAsync(id);

            if (warehouse == null)
            {
                TempData["Error"] = "Warehouse not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        #endregion

        #region Edit Warehouse

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse =
                await _warehouseService.GetByIdAsync(id);

            if (warehouse == null)
            {
                TempData["Error"] = "Warehouse not found.";

                return RedirectToAction(nameof(Index));
            }

            var model = new WarehouseViewModel
            {
                WarehouseId = warehouse.WarehouseId,
                WarehouseCode = warehouse.WarehouseCode,
                WarehouseName = warehouse.WarehouseName,
                Description = warehouse.Description,
                WarehouseType = warehouse.WarehouseType,
                IsDefault = warehouse.IsDefault,
                IsActive = warehouse.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WarehouseViewModel model)
        {
            ModelState.Remove(nameof(WarehouseViewModel.WarehouseCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var warehouse = new Warehouse
                {
                    WarehouseId = model.WarehouseId,
                    WarehouseCode = model.WarehouseCode ?? string.Empty,
                    WarehouseName = model.WarehouseName,
                    Description = model.Description,
                    WarehouseType = model.WarehouseType,
                    IsDefault = model.IsDefault,
                    IsActive = model.IsActive
                };

                await _warehouseService.UpdateAsync(warehouse);

                TempData["Success"] = "Warehouse updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return View(model);
            }
        }

        #endregion

        #region Delete Warehouse

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _warehouseService.DeleteAsync(id);

                TempData["Success"] = "Warehouse deleted successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}