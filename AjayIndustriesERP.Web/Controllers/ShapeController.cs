/*
==============================================================

File : ShapeController.cs

Purpose :
Handles Shape Master UI requests.

Features :
- CRUD operations
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
using AjayIndustriesERP.Web.ViewModels.Shape;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Handles Shape Master CRUD operations.
    /// </summary>
    public class ShapeController : Controller
    {
        private readonly IShapeService _shapeService;

        public ShapeController(
            IShapeService shapeService)
        {
            _shapeService = shapeService;
        }

        #region Shape List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var shapes =
                    await _shapeService.SearchAsync(
                        searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = shapes.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(shapes);
            }

            var result =
                await _shapeService.GetPagedAsync(
                    pageNumber,
                    pageSize);

            ViewBag.SearchText = searchText;
            ViewBag.PageNumber = result.PageNumber;
            ViewBag.PageSize = result.PageSize;
            ViewBag.TotalRecords = result.TotalRecords;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.HasPrevious = result.HasPrevious;
            ViewBag.HasNext = result.HasNext;

            return View(result.Items);
        }

        #endregion

        #region Create Shape

        [HttpGet]
        public IActionResult Create()
        {
            return View(
                new ShapeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ShapeViewModel model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var similarResult =
                    await FindSimilarShapesAsync(
                        model.ShapeName);

                var exactMatch =
                    similarResult.FirstOrDefault(
                        x => x.IsExactMatch);

                if (exactMatch != null)
                {
                    model.SimilarShapeNames =
                        similarResult
                            .Select(x => x.DisplayText)
                            .ToList();

                    ModelState.AddModelError(
                        nameof(model.ShapeName),
                        "Shape Name already exists.");

                    return View(model);
                }

                if (similarResult.Count > 0 &&
                    !model.ConfirmSimilarShapeName)
                {
                    model.SimilarShapeNames =
                        similarResult
                            .Select(x => x.DisplayText)
                            .ToList();

                    return View(model);
                }

                var shape = new Shape
                {
                    ShapeName = model.ShapeName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _shapeService.CreateAsync(shape);

                TempData["Success"] =
                    "Shape created successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;

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

        #region Shape Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var shape =
                await _shapeService.GetByIdAsync(id);

            if (shape == null)
            {
                TempData["Error"] =
                    "Shape not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(shape);
        }

        #endregion

        #region Edit Shape

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var shape =
                await _shapeService.GetByIdAsync(id);

            if (shape == null)
            {
                TempData["Error"] =
                    "Shape not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var model = new ShapeViewModel
            {
                ShapeId = shape.ShapeId,
                ShapeCode = shape.ShapeCode,
                ShapeName = shape.ShapeName,
                Description = shape.Description,
                IsActive = shape.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ShapeViewModel model)
        {
            ModelState.Remove(
                nameof(ShapeViewModel.ShapeCode));

            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var similarResult =
                    await FindSimilarShapesAsync(
                        model.ShapeName,
                        model.ShapeId);

                var exactMatch =
                    similarResult.FirstOrDefault(
                        x => x.IsExactMatch);

                if (exactMatch != null)
                {
                    model.SimilarShapeNames =
                        similarResult
                            .Select(x => x.DisplayText)
                            .ToList();

                    ModelState.AddModelError(
                        nameof(model.ShapeName),
                        "Shape Name already exists.");

                    return View(model);
                }

                if (similarResult.Count > 0 &&
                    !model.ConfirmSimilarShapeName)
                {
                    model.SimilarShapeNames =
                        similarResult
                            .Select(x => x.DisplayText)
                            .ToList();

                    return View(model);
                }

                var shape = new Shape
                {
                    ShapeId = model.ShapeId,
                    ShapeName = model.ShapeName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _shapeService.UpdateAsync(shape);

                TempData["Success"] =
                    "Shape updated successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;

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

        #region Delete Shape

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _shapeService.DeleteAsync(id);

                TempData["Success"] =
                    "Shape deleted successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
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

        #region Live Similar-Name Check

        /// <summary>
        /// Returns exact and similar Shapes while the user
        /// types a Shape Name.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckSimilarName(
            string shapeName,
            int? shapeId = null)
        {
            var matches =
                await FindSimilarShapesAsync(
                    shapeName,
                    shapeId);

            return Json(new
            {
                success = true,
                hasMatches = matches.Count > 0,

                hasExactMatch =
                    matches.Any(x => x.IsExactMatch),

                records = matches.Select(x => new
                {
                    id = x.ShapeId,
                    displayText = x.DisplayText,
                    isExactMatch = x.IsExactMatch
                })
            });
        }

        #endregion

        #region Private Similarity Methods

        private async Task<List<ShapeSuggestion>>
            FindSimilarShapesAsync(
                string shapeName,
                int? excludedShapeId = null)
        {
            if (string.IsNullOrWhiteSpace(shapeName) ||
                shapeName.Trim().Length < 3)
            {
                return new List<ShapeSuggestion>();
            }

            var shapes =
                await _shapeService.GetAllAsync();

            var availableShapes = shapes
                .Where(x =>
                    !excludedShapeId.HasValue ||
                    x.ShapeId != excludedShapeId.Value)
                .ToList();

            var matches =
                NameSimilarityHelper.FindMatches(
                    availableShapes,
                    shapeName,
                    x => x.ShapeName,
                    5);

            return matches
                .Select(x => new ShapeSuggestion
                {
                    ShapeId = x.ShapeId,
                    DisplayText =
                        $"{x.ShapeCode} - {x.ShapeName}",

                    IsExactMatch =
                        NameSimilarityHelper.IsExactMatch(
                            shapeName,
                            x.ShapeName)
                })
                .ToList();
        }

        #endregion

        #region Model Normalization

        private static void NormalizeModel(
            ShapeViewModel model)
        {
            model.ShapeName =
                model.ShapeName?.Trim()
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
        /// Represents an internal similar Shape result.
        /// </summary>
        private sealed class ShapeSuggestion
        {
            public int ShapeId { get; set; }

            public string DisplayText { get; set; } =
                string.Empty;

            public bool IsExactMatch { get; set; }
        }

        #endregion
    }
}