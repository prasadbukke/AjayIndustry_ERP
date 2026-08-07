/*
==============================================================

File : SpecificationController.cs

Purpose :
Handles Specification Master UI requests.

Features :
- CRUD
- Search and pagination
- Exact duplicate blocking
- Live similar-name suggestions
- Similar-name confirmation
- Soft delete

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Specification;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Handles Specification Master CRUD operations.
    /// </summary>
    public class SpecificationController : Controller
    {
        private readonly ISpecificationService
            _specificationService;

        public SpecificationController(
            ISpecificationService specificationService)
        {
            _specificationService =
                specificationService;
        }

        #region Specification List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var specifications =
                    await _specificationService
                        .SearchAsync(searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords =
                    specifications.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(specifications);
            }

            var result =
                await _specificationService
                    .GetPagedAsync(
                        pageNumber,
                        pageSize);

            ViewBag.SearchText = searchText;
            ViewBag.PageNumber =
                result.PageNumber;
            ViewBag.PageSize =
                result.PageSize;
            ViewBag.TotalRecords =
                result.TotalRecords;
            ViewBag.TotalPages =
                result.TotalPages;
            ViewBag.HasPrevious =
                result.HasPrevious;
            ViewBag.HasNext =
                result.HasNext;

            return View(result.Items);
        }

        #endregion

        #region Create Specification

        [HttpGet]
        public IActionResult Create()
        {
            return View(
                new SpecificationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SpecificationViewModel model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var similarSpecifications =
                    await FindSimilarSpecificationsAsync(
                        model.SpecificationName);

                var exactMatch =
                    similarSpecifications
                        .FirstOrDefault(
                            x => x.IsExactMatch);

                if (exactMatch != null)
                {
                    model.SimilarSpecificationNames =
                        similarSpecifications
                            .Select(
                                x => x.DisplayText)
                            .ToList();

                    ModelState.AddModelError(
                        nameof(
                            model.SpecificationName),
                        "Specification Name already exists.");

                    return View(model);
                }

                if (similarSpecifications.Count > 0 &&
                    !model
                        .ConfirmSimilarSpecificationName)
                {
                    model.SimilarSpecificationNames =
                        similarSpecifications
                            .Select(
                                x => x.DisplayText)
                            .ToList();

                    return View(model);
                }

                var specification =
                    new Specification
                    {
                        SpecificationName =
                            model.SpecificationName,

                        Description =
                            model.Description,

                        IsActive =
                            model.IsActive
                    };

                await _specificationService
                    .CreateAsync(specification);

                TempData["Success"] =
                    "Specification created successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";

                return View(model);
            }
        }

        #endregion

        #region Specification Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var specification =
                await _specificationService
                    .GetByIdAsync(id);

            if (specification == null)
            {
                TempData["Error"] =
                    "Specification not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(specification);
        }

        #endregion

        #region Edit Specification

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var specification =
                await _specificationService
                    .GetByIdAsync(id);

            if (specification == null)
            {
                TempData["Error"] =
                    "Specification not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var model =
                new SpecificationViewModel
                {
                    SpecificationId =
                        specification.SpecificationId,

                    SpecificationCode =
                        specification.SpecificationCode,

                    SpecificationName =
                        specification.SpecificationName,

                    Description =
                        specification.Description,

                    IsActive =
                        specification.IsActive
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            SpecificationViewModel model)
        {
            ModelState.Remove(
                nameof(
                    SpecificationViewModel
                        .SpecificationCode));

            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var similarSpecifications =
                    await FindSimilarSpecificationsAsync(
                        model.SpecificationName,
                        model.SpecificationId);

                var exactMatch =
                    similarSpecifications
                        .FirstOrDefault(
                            x => x.IsExactMatch);

                if (exactMatch != null)
                {
                    model.SimilarSpecificationNames =
                        similarSpecifications
                            .Select(
                                x => x.DisplayText)
                            .ToList();

                    ModelState.AddModelError(
                        nameof(
                            model.SpecificationName),
                        "Specification Name already exists.");

                    return View(model);
                }

                if (similarSpecifications.Count > 0 &&
                    !model
                        .ConfirmSimilarSpecificationName)
                {
                    model.SimilarSpecificationNames =
                        similarSpecifications
                            .Select(
                                x => x.DisplayText)
                            .ToList();

                    return View(model);
                }

                var specification =
                    new Specification
                    {
                        SpecificationId =
                            model.SpecificationId,

                        SpecificationName =
                            model.SpecificationName,

                        Description =
                            model.Description,

                        IsActive =
                            model.IsActive
                    };

                await _specificationService
                    .UpdateAsync(specification);

                TempData["Success"] =
                    "Specification updated successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";

                return View(model);
            }
        }

        #endregion

        #region Delete Specification

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _specificationService
                    .DeleteAsync(id);

                TempData["Success"] =
                    "Specification deleted successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        #endregion

        #region Live Similar Name Check

        /// <summary>
        /// Returns exact and similar Specification Names
        /// while the user is typing.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckSimilarName(
            string specificationName,
            int? specificationId = null)
        {
            var matches =
                await FindSimilarSpecificationsAsync(
                    specificationName,
                    specificationId);

            return Json(new
            {
                success = true,

                hasMatches =
                    matches.Count > 0,

                hasExactMatch =
                    matches.Any(
                        x => x.IsExactMatch),

                records =
                    matches.Select(x => new
                    {
                        id =
                            x.SpecificationId,

                        displayText =
                            x.DisplayText,

                        isExactMatch =
                            x.IsExactMatch
                    })
            });
        }

        #endregion

        #region Similar Specification Methods

        private async Task<
            List<SpecificationSuggestion>>
            FindSimilarSpecificationsAsync(
                string specificationName,
                int? excludedSpecificationId = null)
        {
            if (string.IsNullOrWhiteSpace(
                    specificationName) ||
                specificationName
                    .Trim()
                    .Length < 3)
            {
                return new List<
                    SpecificationSuggestion>();
            }

            var specifications =
                await _specificationService
                    .GetAllAsync();

            var availableSpecifications =
                specifications
                    .Where(x =>
                        !excludedSpecificationId
                            .HasValue ||

                        x.SpecificationId !=
                        excludedSpecificationId.Value)
                    .ToList();

            var matches =
                NameSimilarityHelper.FindMatches(
                    availableSpecifications,
                    specificationName,
                    x => x.SpecificationName,
                    5);

            return matches
                .Select(x =>
                    new SpecificationSuggestion
                    {
                        SpecificationId =
                            x.SpecificationId,

                        DisplayText =
                            $"{x.SpecificationCode} - {x.SpecificationName}",

                        IsExactMatch =
                            NameSimilarityHelper
                                .IsExactMatch(
                                    specificationName,
                                    x.SpecificationName)
                    })
                .ToList();
        }

        #endregion

        #region Model Normalization

        private static void NormalizeModel(
            SpecificationViewModel model)
        {
            model.SpecificationName =
                model.SpecificationName?
                    .Trim()
                ?? string.Empty;

            model.Description =
                string.IsNullOrWhiteSpace(
                    model.Description)
                    ? null
                    : model.Description.Trim();
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Represents internal similar Specification data.
        /// </summary>
        private sealed class SpecificationSuggestion
        {
            public int SpecificationId
            {
                get;
                set;
            }

            public string DisplayText
            {
                get;
                set;
            } = string.Empty;

            public bool IsExactMatch
            {
                get;
                set;
            }
        }

        #endregion
    }
}