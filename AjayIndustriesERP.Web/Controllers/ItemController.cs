/*
==============================================================

File : ItemController.cs

Purpose :
Handles Item Master UI requests.

Features :
- CRUD
- Search and pagination
- Category / Brand / UOM / Shape dropdowns
- Dynamic Item Specifications
- Specification UOM support
- Live similar Item Name detection
- Exact duplicate blocking
- Similar-name confirmation

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Item;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        private readonly IShapeService _shapeService;
        private readonly ISpecificationService _specificationService;

        public ItemController(
            IItemService itemService,
            IItemCategoryService itemCategoryService,
            IBrandService brandService,
            IUomService uomService,
            IShapeService shapeService,
            ISpecificationService specificationService)
        {
            _itemService = itemService;
            _itemCategoryService = itemCategoryService;
            _brandService = brandService;
            _uomService = uomService;
            _shapeService = shapeService;
            _specificationService = specificationService;
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
                    await _itemService.SearchAsync(
                        searchText);

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
        public async Task<IActionResult> Create(
            ItemViewModel model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                return View(model);
            }

            try
            {
                var similarItems =
                    await FindSimilarItemsAsync(
                        model.ItemName);

                

                if (similarItems.Count > 0 &&
                    !model.ConfirmSimilarItemName)
                {
                    model.SimilarItemNames =
                        similarItems
                            .Select(x => x.DisplayText)
                            .ToList();

                    await LoadDropdowns(model);

                    return View(model);
                }

                var item =
                    MapToEntity(model);

                await _itemService.CreateAsync(item);

                TempData["Success"] =
                    "Item created successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;

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
        public async Task<IActionResult> Details(
            int id)
        {
            var item =
                await _itemService.GetByIdAsync(id);

            if (item == null)
            {
                TempData["Error"] =
                    "Item not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            /*
             * ItemRepository loads the main navigation
             * properties. Item Specifications are loaded
             * separately through the Item aggregate service.
             */
            item.ItemSpecifications =
                await _itemService
                    .GetSpecificationsAsync(id);

            return View(item);
        }

        #endregion

        #region Edit Item

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var item =
                await _itemService.GetByIdAsync(id);

            if (item == null)
            {
                TempData["Error"] =
                    "Item not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var itemSpecifications =
                await _itemService
                    .GetSpecificationsAsync(id);

            var model =
                new ItemViewModel
                {
                    ItemId =
                        item.ItemId,

                    ItemCode =
                        item.ItemCode,

                    ItemName =
                        item.ItemName,

                    Description =
                        item.Description,

                    ItemCategoryId =
                        item.ItemCategoryId,

                    BrandId =
                        item.BrandId,

                    UomId =
                        item.UomId,

                    ShapeId =
                        item.ShapeId,

                    IsActive =
                        item.IsActive,

                    ItemSpecifications =
                        itemSpecifications
                            .OrderBy(x => x.SortOrder)
                            .Select(x =>
                                new ItemSpecificationRowViewModel
                                {
                                    ItemSpecificationId =
                                        x.ItemSpecificationId,

                                    SpecificationId =
                                        x.SpecificationId,

                                    SpecificationValue =
                                        x.SpecificationValue,

                                    UomId =
                                        x.UomId,

                                    SortOrder =
                                        x.SortOrder
                                })
                            .ToList()
                };

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ItemViewModel model)
        {
            ModelState.Remove(
                nameof(ItemViewModel.ItemCode));

            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                return View(model);
            }

            try
            {
                var similarItems =
                    await FindSimilarItemsAsync(
                        model.ItemName,
                        model.ItemId);

                

                if (similarItems.Count > 0 &&
                    !model.ConfirmSimilarItemName)
                {
                    model.SimilarItemNames =
                        similarItems
                            .Select(x => x.DisplayText)
                            .ToList();

                    await LoadDropdowns(model);

                    return View(model);
                }

                var item =
                    MapToEntity(model);

                await _itemService.UpdateAsync(item);

                TempData["Success"] =
                    "Item updated successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;

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
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _itemService.DeleteAsync(id);

                TempData["Success"] =
                    "Item deleted successfully.";
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

        #region Live Similar Item Name Check

        [HttpGet]
        public async Task<IActionResult> CheckSimilarName(
    string itemName,
    int? itemId = null)
        {
            var matches =
                await FindSimilarItemsAsync(
                    itemName,
                    itemId);

            return Json(new
            {
                hasSimilarItems =
                    matches.Count > 0,

                /*
                 * Same Name is informational only.
                 *
                 * It is NOT automatically a duplicate because
                 * Shape / Specifications may be different.
                 */
                hasSameName =
                    matches.Any(
                        x => x.IsExactMatch),

                items =
                    matches
                        .Select(x =>
                            x.DisplayText)
                        .ToList()
            });
        }

        #endregion

        #region Dropdown Loading

        /// <summary>
        /// Loads all dropdown data required by
        /// Item Create/Edit forms.
        /// </summary>
        private async Task LoadDropdowns(
            ItemViewModel model)
        {
            #region Category

            model.Categories =
                (await _itemCategoryService
                    .GetAllAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.CategoryName)
                .Select(x =>
                    new SelectListItem
                    {
                        Value =
                            x.ItemCategoryId
                                .ToString(),

                        Text =
                            $"{x.CategoryCode} - {x.CategoryName}",

                        Selected =
                            x.ItemCategoryId ==
                            model.ItemCategoryId
                    })
                .ToList();

            #endregion

            #region Brand

            model.Brands =
                (await _brandService
                    .GetAllAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.BrandName)
                .Select(x =>
                    new SelectListItem
                    {
                        Value =
                            x.BrandId.ToString(),

                        Text =
                            $"{x.BrandCode} - {x.BrandName}",

                        Selected =
                            x.BrandId ==
                            model.BrandId
                    })
                .ToList();

            #endregion

            #region Main UOM

            var allUoms =
                await _uomService.GetAllAsync();

            model.Uoms =
                allUoms
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.UomName)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.UomId.ToString(),

                            Text =
                                $"{x.UomCode} - {x.UomName}",

                            Selected =
                                x.UomId ==
                                model.UomId
                        })
                    .ToList();

            #endregion

            #region Shape

            model.Shapes =
                (await _shapeService
                    .GetAllAsync())
                .Where(x => x.IsActive)
                .OrderBy(x => x.ShapeName)
                .Select(x =>
                    new SelectListItem
                    {
                        Value =
                            x.ShapeId.ToString(),

                        Text =
                            $"{x.ShapeCode} - {x.ShapeName}",

                        Selected =
                            x.ShapeId ==
                            model.ShapeId
                    })
                .ToList();

            #endregion

            #region Specification Master

            var allSpecifications =
                await _specificationService
                    .GetAllAsync();

            /*
             * Normally only Active Specifications are
             * available for selection.
             *
             * During Edit, an existing Item may reference
             * a Specification that was later made inactive.
             * Such selected records are also included so
             * existing Item data remains visible.
             */
            var selectedSpecificationIds =
                model.ItemSpecifications
                    .Where(x =>
                        x.SpecificationId > 0)
                    .Select(x =>
                        x.SpecificationId)
                    .ToHashSet();

            model.SpecificationOptions =
                allSpecifications
                    .Where(x =>
                        x.IsActive ||
                        selectedSpecificationIds
                            .Contains(
                                x.SpecificationId))
                    .OrderBy(x =>
                        x.SpecificationName)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.SpecificationId
                                    .ToString(),

                            Text =
                                $"{x.SpecificationCode} - {x.SpecificationName}"
                        })
                    .ToList();

            #endregion

            #region Specification UOM

            var selectedSpecificationUomIds =
                model.ItemSpecifications
                    .Where(x =>
                        x.UomId.HasValue &&
                        x.UomId.Value > 0)
                    .Select(x =>
                        x.UomId!.Value)
                    .ToHashSet();

            model.SpecificationUoms =
                allUoms
                    .Where(x =>
                        x.IsActive ||
                        selectedSpecificationUomIds
                            .Contains(
                                x.UomId))
                    .OrderBy(x =>
                        x.UomName)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.UomId.ToString(),

                            Text =
                                $"{x.UomCode} - {x.UomName}"
                        })
                    .ToList();

            #endregion
        }

        #endregion

        #region Entity Mapping

        /// <summary>
        /// Maps ItemViewModel including dynamic child rows
        /// into the Item aggregate.
        /// </summary>
        private static Item MapToEntity(
            ItemViewModel model)
        {
            var item =
                new Item
                {
                    ItemId =
                        model.ItemId,

                    ItemName =
                        model.ItemName,

                    Description =
                        model.Description,

                    ItemCategoryId =
                        model.ItemCategoryId,

                    BrandId =
                        model.BrandId,

                    UomId =
                        model.UomId,

                    ShapeId =
                        NormalizeShapeId(
                            model.ShapeId),

                    IsActive =
                        model.IsActive
                };

            item.ItemSpecifications =
                model.ItemSpecifications
                    .Select((row, index) =>
                        new ItemSpecification
                        {
                            ItemSpecificationId =
                                row.ItemSpecificationId,

                            SpecificationId =
                                row.SpecificationId,

                            SpecificationValue =
                                row.SpecificationValue,

                            UomId =
                                NormalizeUomId(
                                    row.UomId),

                            SortOrder =
                                index + 1
                        })
                    .ToList();

            return item;
        }

        #endregion

        #region Similar Name Methods

        private async Task<List<ItemSuggestion>>
            FindSimilarItemsAsync(
                string itemName,
                int? excludedItemId = null)
        {
            if (string.IsNullOrWhiteSpace(
                    itemName) ||
                itemName.Trim().Length < 3)
            {
                return new List<ItemSuggestion>();
            }

            var items =
                await _itemService.GetAllAsync();

            var availableItems =
                items
                    .Where(x =>
                        !excludedItemId.HasValue ||
                        x.ItemId !=
                        excludedItemId.Value)
                    .ToList();

            var matches =
                NameSimilarityHelper.FindMatches(
                    availableItems,
                    itemName,
                    x => x.ItemName,
                    5);

            return matches
                .Select(x =>
                    new ItemSuggestion
                    {
                        ItemId =
                            x.ItemId,

                        DisplayText =
                            $"{x.ItemCode} - {x.ItemName}",

                        IsExactMatch =
                            NameSimilarityHelper
                                .IsExactMatch(
                                    itemName,
                                    x.ItemName)
                    })
                .ToList();
        }

        #endregion

        #region Model Normalization

        private static void NormalizeModel(
            ItemViewModel model)
        {
            model.ItemName =
                model.ItemName?.Trim()
                ?? string.Empty;

            model.Description =
                string.IsNullOrWhiteSpace(
                    model.Description)
                    ? null
                    : model.Description.Trim();

            model.ShapeId =
                NormalizeShapeId(
                    model.ShapeId);

            if (model.ItemSpecifications == null)
            {
                model.ItemSpecifications =
                    new List<
                        ItemSpecificationRowViewModel>();

                return;
            }

            /*
             * SortOrder is determined from current UI row
             * sequence instead of trusting posted values.
             */
            for (var index = 0;
                 index <
                 model.ItemSpecifications.Count;
                 index++)
            {
                var row =
                    model.ItemSpecifications[index];

                row.SpecificationValue =
                    row.SpecificationValue?.Trim()
                    ?? string.Empty;

                row.UomId =
                    NormalizeUomId(
                        row.UomId);

                row.SortOrder =
                    index + 1;
            }
        }

        private static int? NormalizeShapeId(
            int? shapeId)
        {
            return shapeId.HasValue &&
                   shapeId.Value > 0
                ? shapeId.Value
                : null;
        }

        private static int? NormalizeUomId(
            int? uomId)
        {
            return uomId.HasValue &&
                   uomId.Value > 0
                ? uomId.Value
                : null;
        }

        #endregion

        #region Private Classes

        private sealed class ItemSuggestion
        {
            public int ItemId { get; set; }

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