/*
==============================================================

File : ItemController.cs

Purpose :
Handles Item Master UI requests.

Notes :
- Category, Brand and UOM dropdowns contain only active records.
- Similar Item Names generate a warning before Create or Edit.
- Exact duplicate validation remains inside ItemService.

==============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Item;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Handles Item Master CRUD operations.
    /// </summary>
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IItemCategoryService _itemCategoryService;
        private readonly IBrandService _brandService;
        private readonly IUomService _uomService;

        public ItemController(
            IItemService itemService,
            IItemCategoryService itemCategoryService,
            IBrandService brandService,
            IUomService uomService)
        {
            _itemService = itemService;
            _itemCategoryService = itemCategoryService;
            _brandService = brandService;
            _uomService = uomService;
        }

        #region Item List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var items =
                    await _itemService.SearchAsync(searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = items.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(items);
            }

            var result =
                await _itemService.GetPagedAsync(
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

        #region Create Item

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ItemViewModel();

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemViewModel model)
        {
            model.ItemName = model.ItemName?.Trim() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                return View(model);
            }

            try
            {
                /*
                 * Similar names generate a warning.
                 * User can review them and confirm continuation.
                 */
                if (!model.ConfirmSimilarItemName)
                {
                    model.SimilarItemNames =
                        await FindSimilarItemNamesAsync(
                            model.ItemName);

                    if (model.SimilarItemNames.Count > 0)
                    {
                        await LoadDropdowns(model);

                        return View(model);
                    }
                }

                var item = new Item
                {
                    ItemName = model.ItemName,
                    Description = model.Description,
                    ItemCategoryId = model.ItemCategoryId,
                    BrandId = model.BrandId,
                    UomId = model.UomId,
                    IsActive = model.IsActive
                };

                await _itemService.CreateAsync(item);

                TempData["Success"] =
                    "Item created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;

                await LoadDropdowns(model);

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";

                await LoadDropdowns(model);

                return View(model);
            }
        }

        #endregion

        #region Item Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item =
                await _itemService.GetByIdAsync(id);

            if (item == null)
            {
                TempData["Error"] = "Item not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        #endregion

        #region Edit Item

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item =
                await _itemService.GetByIdAsync(id);

            if (item == null)
            {
                TempData["Error"] = "Item not found.";

                return RedirectToAction(nameof(Index));
            }

            var model = new ItemViewModel
            {
                ItemId = item.ItemId,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                Description = item.Description,
                ItemCategoryId = item.ItemCategoryId,
                BrandId = item.BrandId,
                UomId = item.UomId,
                IsActive = item.IsActive
            };

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemViewModel model)
        {
            ModelState.Remove(nameof(ItemViewModel.ItemCode));

            model.ItemName = model.ItemName?.Trim() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                return View(model);
            }

            try
            {
                if (!model.ConfirmSimilarItemName)
                {
                    model.SimilarItemNames =
                        await FindSimilarItemNamesAsync(
                            model.ItemName,
                            model.ItemId);

                    if (model.SimilarItemNames.Count > 0)
                    {
                        await LoadDropdowns(model);

                        return View(model);
                    }
                }

                var item = new Item
                {
                    ItemId = model.ItemId,
                    ItemName = model.ItemName,
                    Description = model.Description,
                    ItemCategoryId = model.ItemCategoryId,
                    BrandId = model.BrandId,
                    UomId = model.UomId,
                    IsActive = model.IsActive
                };

                await _itemService.UpdateAsync(item);

                TempData["Success"] =
                    "Item updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;

                await LoadDropdowns(model);

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";

                await LoadDropdowns(model);

                return View(model);
            }
        }

        #endregion

        #region Delete Item

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _itemService.DeleteAsync(id);

                TempData["Success"] =
                    "Item deleted successfully.";
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

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Similar Item Name Check

        /// <summary>
        /// Returns similar Item Names while the user enters an Item Name.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckSimilarName(
            string itemName,
            int? itemId = null)
        {
            var similarItems =
                await FindSimilarItemNamesAsync(
                    itemName,
                    itemId);

            return Json(new
            {
                hasSimilarItems = similarItems.Count > 0,
                items = similarItems
            });
        }

        #endregion

        #region Dropdown Loading

        private async Task LoadDropdowns(ItemViewModel model)
        {
            model.Categories =
                (await _itemCategoryService.GetAllAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.CategoryName)
                .Select(x => new SelectListItem
                {
                    Value = x.ItemCategoryId.ToString(),
                    Text = $"{x.CategoryCode} - {x.CategoryName}",
                    Selected =
                        x.ItemCategoryId == model.ItemCategoryId
                })
                .ToList();

            model.Brands =
                (await _brandService.GetAllAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.BrandName)
                .Select(x => new SelectListItem
                {
                    Value = x.BrandId.ToString(),
                    Text = $"{x.BrandCode} - {x.BrandName}",
                    Selected =
                        x.BrandId == model.BrandId
                })
                .ToList();

            model.Uoms =
                (await _uomService.GetAllAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.UomName)
                .Select(x => new SelectListItem
                {
                    Value = x.UomId.ToString(),
                    Text = $"{x.UomCode} - {x.UomName}",
                    Selected =
                        x.UomId == model.UomId
                })
                .ToList();
        }

        #endregion

        #region Similar Name Methods

        private async Task<List<string>> FindSimilarItemNamesAsync(
            string itemName,
            int? excludedItemId = null)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return new List<string>();
            }

            var normalizedInput =
                NormalizeName(itemName);

            if (normalizedInput.Length < 3)
            {
                return new List<string>();
            }

            var items =
                await _itemService.GetAllAsync();

            return items
                .Where(x =>
                    !excludedItemId.HasValue ||
                    x.ItemId != excludedItemId.Value)
                .Where(x =>
                    IsSimilarName(
                        normalizedInput,
                        NormalizeName(x.ItemName)))
                .OrderBy(x => x.ItemName)
                .Select(x =>
                    $"{x.ItemCode} - {x.ItemName}")
                .Take(5)
                .ToList();
        }

        private static bool IsSimilarName(
            string firstName,
            string secondName)
        {
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(secondName))
            {
                return false;
            }

            /*
             * Exact duplicate validation is handled by ItemService.
             */
            if (firstName == secondName)
            {
                return false;
            }

            if (firstName[0] != secondName[0])
            {
                return false;
            }

            var lengthDifference =
                Math.Abs(firstName.Length - secondName.Length);

            if (lengthDifference > 2)
            {
                return false;
            }

            var distance =
                CalculateLevenshteinDistance(
                    firstName,
                    secondName);

            var maximumLength =
                Math.Max(
                    firstName.Length,
                    secondName.Length);

            var similarity =
                1D - ((double)distance / maximumLength);

            /*
             * For short names, maximum two spelling changes are allowed.
             * Example: Steel and Still.
             */
            if (maximumLength <= 6)
            {
                return distance <= 2;
            }

            return similarity >= 0.75D;
        }

        private static string NormalizeName(string value)
        {
            var normalizedValue =
                value.Trim().ToLowerInvariant();

            return Regex.Replace(
                normalizedValue,
                @"\s+",
                " ");
        }

        private static int CalculateLevenshteinDistance(
            string source,
            string target)
        {
            var sourceLength = source.Length;
            var targetLength = target.Length;

            var matrix =
                new int[sourceLength + 1, targetLength + 1];

            for (var row = 0; row <= sourceLength; row++)
            {
                matrix[row, 0] = row;
            }

            for (var column = 0;
                 column <= targetLength;
                 column++)
            {
                matrix[0, column] = column;
            }

            for (var row = 1;
                 row <= sourceLength;
                 row++)
            {
                for (var column = 1;
                     column <= targetLength;
                     column++)
                {
                    var cost =
                        source[row - 1] ==
                        target[column - 1]
                            ? 0
                            : 1;

                    matrix[row, column] = Math.Min(
                        Math.Min(
                            matrix[row - 1, column] + 1,
                            matrix[row, column - 1] + 1),
                        matrix[row - 1, column - 1] + cost);
                }
            }

            return matrix[sourceLength, targetLength];
        }

        #endregion
    }
}