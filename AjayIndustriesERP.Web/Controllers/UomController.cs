/*
==============================================================

File : UomController.cs

Purpose :
Handles UOM UI requests.

==============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Uom;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class UomController : Controller
    {
        private readonly IUomService _uomService;

        public UomController(IUomService uomService)
        {
            _uomService = uomService;
        }

        #region UOM List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var uoms = await _uomService.SearchAsync(searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;

                ViewBag.TotalRecords = uoms.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(uoms);
            }

            var result =
                await _uomService.GetPagedAsync(pageNumber, pageSize);

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

        #region Create UOM

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UomViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UomViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var uom = new Uom
                {
                    UomCode = model.UomCode,
                    UomName = model.UomName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _uomService.CreateAsync(uom);

                TempData["Success"] = "UOM created successfully.";

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

        #region Edit UOM

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var uom = await _uomService.GetByIdAsync(id);

            if (uom == null)
            {
                TempData["Error"] = "UOM not found.";

                return RedirectToAction(nameof(Index));
            }

            var model = new UomViewModel
            {
                UomId = uom.UomId,
                UomCode = uom.UomCode,
                UomName = uom.UomName,
                Description = uom.Description,
                IsActive = uom.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UomViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var uom = new Uom
                {
                    UomId = model.UomId,
                    UomCode = model.UomCode,
                    UomName = model.UomName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _uomService.UpdateAsync(uom);

                TempData["Success"] = "UOM updated successfully.";

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

        #region UOM Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var uom = await _uomService.GetByIdAsync(id);

            if (uom == null)
            {
                TempData["Error"] = "UOM not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(uom);
        }

        #endregion

        #region Delete UOM

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _uomService.DeleteAsync(id);

                TempData["Success"] = "UOM deleted successfully.";
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