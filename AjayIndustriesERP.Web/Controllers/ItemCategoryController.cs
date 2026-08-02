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
using AjayIndustriesERP.Web.ViewModels.ItemCategory;
using AjayIndustriesERP.Web.ViewModels.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class ItemCategoryController : Controller
    {
        private readonly IItemCategoryService _itemCategoryService;

        public ItemCategoryController(IItemCategoryService itemCategoryService)
        {
            _itemCategoryService = itemCategoryService;
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
                    await _itemCategoryService.SearchAsync(searchText);

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
                await _itemCategoryService.GetPagedAsync(pageNumber, pageSize);

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
            return View(new ItemCategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCategoryViewModel model)
        {
            ModelState.Remove(nameof(ItemCategoryViewModel.CategoryCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var itemCategory = new ItemCategory
                {
                    CategoryCode = model.CategoryCode ?? string.Empty,
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    
                    IsActive = model.IsActive
                };

                await _itemCategoryService.CreateAsync(itemCategory);

                TempData["Success"] = "itemCategory created successfully.";

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
                await _itemCategoryService.GetByIdAsync(id);

            if (warehouse == null)
            {
                TempData["Error"] = "itemCategory not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        #endregion

        #region Edit Warehouse

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var itemCategory =
                await _itemCategoryService.GetByIdAsync(id);

            if (itemCategory == null)
            {
                TempData["Error"] = "itemCategory not found.";

                return RedirectToAction(nameof(Index));
            }

            var model = new ItemCategoryViewModel
            {
                ItemCategoryId = itemCategory.ItemCategoryId,
                CategoryCode = itemCategory.CategoryCode,
                CategoryName = itemCategory.CategoryName,
                Description = itemCategory.Description,
                
                IsActive = itemCategory.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemCategoryViewModel model)
        {
            ModelState.Remove(nameof(ItemCategoryViewModel.CategoryCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var itemCategory = new ItemCategory
                {
                    ItemCategoryId = model.ItemCategoryId,
                    CategoryCode = model.CategoryCode ?? string.Empty,
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    
                    IsActive = model.IsActive
                };

                await _itemCategoryService.UpdateAsync(itemCategory);

                TempData["Success"] = "itemCategory updated successfully.";

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
                await _itemCategoryService.DeleteAsync(id);

                TempData["Success"] = "itemCategory deleted successfully.";
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