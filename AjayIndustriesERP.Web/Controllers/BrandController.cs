/*
==============================================================

File : BrandController.cs

Purpose :
Handles Brand UI requests.

==============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Brand;

using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService itemCategoryService)
        {
            _brandService = itemCategoryService;
        }

        #region Brand List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var Brands =
                    await _brandService.SearchAsync(searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;

                ViewBag.TotalRecords = Brands.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(Brands);
            }

            var result =
                await _brandService.GetPagedAsync(pageNumber, pageSize);

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

        #region Create Brand

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BrandViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel model)
        {
            ModelState.Remove(nameof(BrandViewModel.BrandCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var brand = new Brand
                {
                    BrandCode = model.BrandCode ?? string.Empty,
                    BrandName = model.BrandName,
                    Description = model.Description,

                    IsActive = model.IsActive
                };

                await _brandService.CreateAsync(brand);

                TempData["Success"] = "brand created successfully.";

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

        #region Brand Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var Brand =
                await _brandService.GetByIdAsync(id);

            if (Brand == null)
            {
                TempData["Error"] = "brand not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(Brand);
        }

        #endregion

        #region Edit Brand

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var itemCategory =
                await _brandService.GetByIdAsync(id);

            if (itemCategory == null)
            {
                TempData["Error"] = "brand not found.";

                return RedirectToAction(nameof(Index));
            }

            var model = new BrandViewModel
            {
                BrandId = itemCategory.BrandId,
                BrandCode = itemCategory.BrandCode,
                BrandName = itemCategory.BrandName,
                Description = itemCategory.Description,

                IsActive = itemCategory.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandViewModel model)
        {
            ModelState.Remove(nameof(BrandViewModel.BrandCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var brand = new Brand
                {
                    BrandId = model.BrandId,
                    BrandCode = model.BrandCode ?? string.Empty,
                    BrandName = model.BrandName,
                    Description = model.Description,

                    IsActive = model.IsActive
                };

                await _brandService.UpdateAsync(brand);

                TempData["Success"] = "brand updated successfully.";

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

        #region Delete Brand

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _brandService.DeleteAsync(id);

                TempData["Success"] = "brand deleted successfully.";
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