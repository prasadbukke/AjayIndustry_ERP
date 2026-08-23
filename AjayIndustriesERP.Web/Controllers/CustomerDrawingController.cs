/*
==============================================================

File : CustomerDrawingController.cs

Purpose :
Handles Customer Drawing Master CRUD and Customer Drawing
revision history workflow.

Final Rules :
- Customer is mandatory.
- One Customer + One Item = One Drawing Number.
- Customer cannot change after creation.
- Item cannot change after creation.
- Drawing Number cannot change after creation.
- Revision Number is system generated.
- First Revision is RV-01.
- Existing revisions are never overwritten.
- New revisions create new CustomerDrawing rows.
- Old Drawing files are preserved.
- Previous revisions may be Activated / Deleted.
- Complete Customer Drawing may be Soft Deleted / Restored.
- Drawing Number and Name support live suggestions.
- Drawing Number similarity is scoped to selected Customer.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.CustomerDrawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Web.Controllers
{
    public class CustomerDrawingController :
        Controller
    {
        private const long MaxDrawingFileSize =
            25 * 1024 * 1024;


        private static readonly HashSet<string>
            AllowedDrawingExtensions =
                new(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".dwg",
                    ".dxf"
                };


        private readonly ICustomerDrawingService
            _customerDrawingService;

        private readonly ICustomerService
            _customerService;

        private readonly IItemService
            _itemService;

        private readonly IWebHostEnvironment
            _webHostEnvironment;


        public CustomerDrawingController(
            ICustomerDrawingService customerDrawingService,
            ICustomerService customerService,
            IItemService itemService,
            IWebHostEnvironment webHostEnvironment)
        {
            _customerDrawingService =
                customerDrawingService;

            _customerService =
                customerService;

            _itemService =
                itemService;

            _webHostEnvironment =
                webHostEnvironment;
        }


        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                var drawings =
                    await _customerDrawingService
                        .SearchAsync(
                            searchText);


                ViewBag.SearchText =
                    searchText;

                ViewBag.PageNumber =
                    1;

                ViewBag.PageSize =
                    pageSize;

                ViewBag.TotalRecords =
                    drawings.Count;

                ViewBag.TotalPages =
                    1;

                ViewBag.HasPrevious =
                    false;

                ViewBag.HasNext =
                    false;


                return View(
                    drawings);
            }


            var result =
                await _customerDrawingService
                    .GetPagedAsync(
                        pageNumber,
                        pageSize);


            ViewBag.SearchText =
                searchText;

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


            return View(
                result.Items);
        }

        #endregion


        #region Create

        [HttpGet]
        public async Task<IActionResult> Create(
            int? customerId = null,
            int? itemId = null)
        {
            var model =
                new CustomerDrawingFormViewModel();


            if (customerId.HasValue &&
                customerId.Value > 0)
            {
                model.CustomerId =
                    customerId.Value;
            }


            if (itemId.HasValue &&
                itemId.Value > 0)
            {
                model.ItemId =
                    itemId.Value;
            }


            await LoadFormLookupsAsync(
                model);


            return View(
                model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerDrawingFormViewModel model)
        {
            NormalizeModel(
                model);


            await LoadSimilarityWarningsAsync(
                model);


            /*
             * Exact Drawing Number is checked
             * within selected Customer.
             */
            var exactDrawingNumber =
                await HasExactDrawingNumberAsync(
                    model.CustomerId,
                    model.DrawingNumber);


            if (exactDrawingNumber)
            {
                ModelState.AddModelError(
                    nameof(model.DrawingNumber),

                    "Drawing Number already exists for " +
                    "the selected Customer. Open the " +
                    "existing Customer Drawing and add " +
                    "a new Revision.");
            }


            /*
             * One Customer + Item can have only
             * one Customer Drawing.
             */
            if (model.CustomerId > 0 &&
                model.ItemId > 0)
            {
                var existingCustomerItem =
                    await _customerDrawingService
                        .GetByCustomerAndItemAsync(
                            model.CustomerId,
                            model.ItemId);


                if (existingCustomerItem != null)
                {
                    ModelState.AddModelError(
                        nameof(model.ItemId),

                        "The selected Customer and Item " +
                        "already have a Customer Drawing. " +
                        "Open the existing Drawing and add " +
                        "a new Revision.");
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadFormLookupsAsync(
                    model);


                return View(
                    model);
            }


            string? newlySavedFilePath =
                null;


            try
            {
                if (model.DrawingFile != null)
                {
                    var savedFile =
                        await SaveCustomerDrawingFileAsync(
                            model.DrawingFile);


                    model.FileName =
                        savedFile.FileName;

                    model.FilePath =
                        savedFile.FilePath;

                    newlySavedFilePath =
                        savedFile.FilePath;
                }


                var customerDrawing =
                    new CustomerDrawing
                    {
                        CustomerId =
                            model.CustomerId,

                        ItemId =
                            model.ItemId,

                        DrawingNumber =
                            model.DrawingNumber,

                        DrawingName =
                            model.DrawingName,

                        DrawingType =
                            model.DrawingType,

                        /*
                         * Service ignores posted Revision
                         * and creates RV-01.
                         */
                        RevisionNumber =
                            model.RevisionNumber,

                        FileName =
                            model.FileName,

                        FilePath =
                            model.FilePath,

                        Description =
                            model.Description,

                        IsActive =
                            true
                    };


                await _customerDrawingService
                    .CreateAsync(
                        customerDrawing);


                TempData["Success"] =
                    "Customer Drawing created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                DeletePhysicalFile(
                    newlySavedFilePath);


                TempData["Error"] =
                    ex.Message;


                await LoadSimilarityWarningsAsync(
                    model);


                await LoadFormLookupsAsync(
                    model);


                return View(
                    model);
            }
            catch (Exception)
            {
                DeletePhysicalFile(
                    newlySavedFilePath);


                TempData["Error"] =
                    "Something went wrong. Please try again.";


                await LoadFormLookupsAsync(
                    model);


                return View(
                    model);
            }
        }

        #endregion


        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var selectedRevision =
                await _customerDrawingService
                    .GetByIdAsync(
                        id);


            if (selectedRevision == null)
            {
                TempData["Error"] =
                    "Customer Drawing not found.";


                return RedirectToAction(
                    nameof(Index));
            }


            var history =
                await _customerDrawingService
                    .GetRevisionHistoryAsync(
                        id);


            var currentDrawing =
                history.FirstOrDefault(x =>
                    x.IsActive)
                ??
                selectedRevision;


            ViewBag.RevisionHistory =
                history;


            return View(
                currentDrawing);
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var selectedRevision =
                await _customerDrawingService
                    .GetByIdAsync(
                        id);


            if (selectedRevision == null)
            {
                TempData["Error"] =
                    "Customer Drawing not found.";


                return RedirectToAction(
                    nameof(Index));
            }


            var history =
                await _customerDrawingService
                    .GetRevisionHistoryAsync(
                        id);


            var currentDrawing =
                history.FirstOrDefault(x =>
                    x.IsActive)
                ??
                selectedRevision;


            var model =
                new CustomerDrawingFormViewModel
                {
                    CustomerDrawingId =
                        currentDrawing.CustomerDrawingId,

                    CustomerId =
                        currentDrawing.CustomerId,

                    ItemId =
                        currentDrawing.ItemId,

                    DrawingNumber =
                        currentDrawing.DrawingNumber,

                    DrawingName =
                        currentDrawing.DrawingName,

                    DrawingType =
                        currentDrawing.DrawingType,

                    RevisionNumber =
                        currentDrawing.RevisionNumber,

                    FileName =
                        currentDrawing.FileName,

                    FilePath =
                        currentDrawing.FilePath,

                    Description =
                        currentDrawing.Description,

                    IsActive =
                        true,

                    RevisionHistory =
                        MapRevisionHistory(
                            history)
                };


            await LoadFormLookupsAsync(
                model);


            await LoadSimilarityWarningsAsync(
                model,
                currentDrawing.DrawingNumber);


            return View(
                model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CustomerDrawingFormViewModel model)
        {
            NormalizeModel(
                model);


            var existingDrawing =
                await _customerDrawingService
                    .GetByIdAsync(
                        model.CustomerDrawingId);


            if (existingDrawing == null)
            {
                TempData["Error"] =
                    "Customer Drawing not found.";


                return RedirectToAction(
                    nameof(Index));
            }


            var history =
                await _customerDrawingService
                    .GetRevisionHistoryAsync(
                        model.CustomerDrawingId);


            var currentDrawing =
                history.FirstOrDefault(x =>
                    x.IsActive)
                ??
                existingDrawing;


            /*
             * Customer is permanent.
             */
            model.CustomerId =
                currentDrawing.CustomerId;


            /*
             * Item is permanent.
             */
            model.ItemId =
                currentDrawing.ItemId;


            /*
             * Drawing Number is permanent.
             */
            model.DrawingNumber =
                currentDrawing.DrawingNumber;


            /*
             * Existing Current Revision is read-only.
             */
            model.RevisionNumber =
                currentDrawing.RevisionNumber;


            model.FileName =
                currentDrawing.FileName;

            model.FilePath =
                currentDrawing.FilePath;


            /*
             * Existing current revision file must
             * never be overwritten.
             *
             * New files are added only through
             * New Revision rows.
             */
            model.DrawingFile =
                null;


            if (!ModelState.IsValid)
            {
                model.RevisionHistory =
                    MapRevisionHistory(
                        history);


                await LoadFormLookupsAsync(
                    model);


                await LoadSimilarityWarningsAsync(
                    model,
                    currentDrawing.DrawingNumber);


                return View(
                    model);
            }


            var savedNewFiles =
                new List<string>();


            try
            {
                var newRevisionEntities =
                    new List<CustomerDrawing>();


                foreach (var revisionModel
                    in model.NewRevisions)
                {
                    NormalizeRevisionModel(
                        revisionModel);


                    string? fileName =
                        null;

                    string? filePath =
                        null;


                    if (revisionModel.DrawingFile != null)
                    {
                        var savedFile =
                            await SaveCustomerDrawingFileAsync(
                                revisionModel.DrawingFile);


                        fileName =
                            savedFile.FileName;

                        filePath =
                            savedFile.FilePath;


                        savedNewFiles.Add(
                            filePath);
                    }


                    newRevisionEntities.Add(
                        new CustomerDrawing
                        {
                            /*
                             * Revision Number is generated
                             * by Application Service.
                             */
                            RevisionNumber =
                                revisionModel
                                    .RevisionNumber,

                            FileName =
                                fileName,

                            FilePath =
                                filePath,

                            Description =
                                revisionModel
                                    .Description
                        });
                }


                var customerDrawing =
                    new CustomerDrawing
                    {
                        CustomerDrawingId =
                            currentDrawing
                                .CustomerDrawingId,

                        CustomerId =
                            currentDrawing
                                .CustomerId,

                        ItemId =
                            currentDrawing
                                .ItemId,

                        DrawingNumber =
                            currentDrawing
                                .DrawingNumber,

                        DrawingName =
                            model.DrawingName,

                        DrawingType =
                            model.DrawingType,

                        IsActive =
                            true
                    };


                await _customerDrawingService
                    .UpdateAsync(
                        customerDrawing,
                        newRevisionEntities);


                TempData["Success"] =
                    newRevisionEntities.Count > 0
                        ? "Customer Drawing updated and new revision added successfully."
                        : "Customer Drawing updated successfully.";


                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id =
                            model.CustomerDrawingId
                    });
            }
            catch (BusinessException ex)
            {
                DeletePhysicalFiles(
                    savedNewFiles);


                TempData["Error"] =
                    ex.Message;


                var refreshedHistory =
                    await _customerDrawingService
                        .GetRevisionHistoryAsync(
                            model.CustomerDrawingId);


                model.RevisionHistory =
                    MapRevisionHistory(
                        refreshedHistory);


                await LoadFormLookupsAsync(
                    model);


                await LoadSimilarityWarningsAsync(
                    model,
                    currentDrawing.DrawingNumber);


                return View(
                    model);
            }
            catch (Exception)
            {
                DeletePhysicalFiles(
                    savedNewFiles);


                TempData["Error"] =
                    "Something went wrong. Please try again.";


                model.RevisionHistory =
                    MapRevisionHistory(
                        history);


                await LoadFormLookupsAsync(
                    model);


                return View(
                    model);
            }
        }

        #endregion


        #region Delete Complete Drawing

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                /*
                 * Complete revision history is
                 * soft deleted.
                 *
                 * Physical historical files remain.
                 */
                await _customerDrawingService
                    .DeleteAsync(
                        id);


                TempData["Success"] =
                    "Customer Drawing deleted successfully.";
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


        #region Live Customer + Item Check

        [HttpGet]
        public async Task<IActionResult>
            CheckCustomerItem(
                int customerId,
                int itemId,
                int? customerDrawingId = null)
        {
            if (customerId <= 0 ||
                itemId <= 0)
            {
                return Json(
                    new
                    {
                        success =
                            true,

                        exists =
                            false
                    });
            }


            var existing =
                await _customerDrawingService
                    .GetByCustomerAndItemAsync(
                        customerId,
                        itemId);


            var exists =
                existing != null
                &&
                (
                    !customerDrawingId.HasValue
                    ||
                    existing.CustomerDrawingId !=
                        customerDrawingId.Value
                );


            return Json(
                new
                {
                    success =
                        true,

                    exists,

                    customerDrawingId =
                        exists
                            ? existing!
                                .CustomerDrawingId
                            : 0,

                    drawingNumber =
                        exists
                            ? existing!
                                .DrawingNumber
                            : string.Empty,

                    message =
                        exists
                            ? "A Customer Drawing already exists for the selected Customer and Item."
                            : string.Empty
                });
        }

        #endregion


        #region Live Drawing Number Check

        [HttpGet]
        public async Task<IActionResult>
            CheckSimilarNumber(
                int customerId,
                string drawingNumber,
                int? customerDrawingId = null)
        {
            if (customerId <= 0 ||
                string.IsNullOrWhiteSpace(
                    drawingNumber))
            {
                return Json(
                    new
                    {
                        success =
                            true,

                        hasMatches =
                            false,

                        hasExactMatch =
                            false,

                        records =
                            Array.Empty<object>()
                    });
            }


            var excludedDrawingNumber =
                await GetExcludedDrawingNumberAsync(
                    customerDrawingId);


            var matches =
                await FindDrawingNumberMatchesAsync(
                    customerId,
                    drawingNumber,
                    excludedDrawingNumber);


            return Json(
                new
                {
                    success =
                        true,

                    hasMatches =
                        matches.Count > 0,

                    hasExactMatch =
                        matches.Any(x =>
                            x.IsExactMatch),

                    records =
                        matches.Select(x =>
                            new
                            {
                                id =
                                    x.CustomerDrawingId,

                                displayText =
                                    x.DisplayText,

                                isExactMatch =
                                    x.IsExactMatch
                            })
                });
        }

        #endregion


        #region Live Drawing Name Check

        [HttpGet]
        public async Task<IActionResult>
            CheckSimilarName(
                int customerId,
                string drawingName,
                int? customerDrawingId = null)
        {
            if (customerId <= 0 ||
                string.IsNullOrWhiteSpace(
                    drawingName))
            {
                return Json(
                    new
                    {
                        success =
                            true,

                        hasMatches =
                            false,

                        hasExactMatch =
                            false,

                        records =
                            Array.Empty<object>()
                    });
            }


            var excludedDrawingNumber =
                await GetExcludedDrawingNumberAsync(
                    customerDrawingId);


            var matches =
                await FindDrawingNameMatchesAsync(
                    customerId,
                    drawingName,
                    excludedDrawingNumber);


            return Json(
                new
                {
                    success =
                        true,

                    hasMatches =
                        matches.Count > 0,

                    hasExactMatch =
                        matches.Any(x =>
                            x.IsExactMatch),

                    records =
                        matches.Select(x =>
                            new
                            {
                                id =
                                    x.CustomerDrawingId,

                                displayText =
                                    x.DisplayText,

                                isExactMatch =
                                    x.IsExactMatch
                            })
                });
        }

        #endregion


        #region Similarity Helpers

        private async Task
            LoadSimilarityWarningsAsync(
                CustomerDrawingFormViewModel model,
                string? excludedDrawingNumber = null)
        {
            model.SimilarDrawingNumbers
                .Clear();

            model.SimilarDrawingNames
                .Clear();


            if (model.CustomerId <= 0)
            {
                return;
            }


            if (!string.IsNullOrWhiteSpace(
                model.DrawingNumber))
            {
                model.SimilarDrawingNumbers =
                    (
                        await FindDrawingNumberMatchesAsync(
                            model.CustomerId,
                            model.DrawingNumber,
                            excludedDrawingNumber)
                    )
                    .Select(x =>
                        x.DisplayText)
                    .ToList();
            }


            if (!string.IsNullOrWhiteSpace(
                model.DrawingName))
            {
                model.SimilarDrawingNames =
                    (
                        await FindDrawingNameMatchesAsync(
                            model.CustomerId,
                            model.DrawingName,
                            excludedDrawingNumber)
                    )
                    .Select(x =>
                        x.DisplayText)
                    .ToList();
            }
        }


        private async Task<List<CustomerDrawingSuggestion>>
            FindDrawingNumberMatchesAsync(
                int customerId,
                string drawingNumber,
                string? excludedDrawingNumber = null)
        {
            var drawings =
                await _customerDrawingService
                    .GetAllAsync();


            var normalizedSearch =
                NameSimilarityHelper.Normalize(
                    drawingNumber);


            return drawings

                .Where(x =>
                    x.CustomerId ==
                        customerId)

                .Where(x =>
                    string.IsNullOrWhiteSpace(
                        excludedDrawingNumber)
                    ||
                    !NameSimilarityHelper
                        .IsExactMatch(
                            x.DrawingNumber,
                            excludedDrawingNumber))

                .Where(x =>
                {
                    var normalizedExisting =
                        NameSimilarityHelper.Normalize(
                            x.DrawingNumber);


                    return
                        NameSimilarityHelper
                            .IsExactMatch(
                                x.DrawingNumber,
                                drawingNumber)

                        ||

                        NameSimilarityHelper
                            .IsSimilarMatch(
                                x.DrawingNumber,
                                drawingNumber)

                        ||

                        normalizedExisting
                            .Contains(
                                normalizedSearch)

                        ||

                        normalizedSearch
                            .Contains(
                                normalizedExisting);
                })

                .OrderByDescending(x =>
                    NameSimilarityHelper
                        .IsExactMatch(
                            x.DrawingNumber,
                            drawingNumber))

                .ThenBy(x =>
                    x.DrawingNumber)

                .Take(5)

                .Select(x =>
                    new CustomerDrawingSuggestion
                    {
                        CustomerDrawingId =
                            x.CustomerDrawingId,

                        DisplayText =
                            $"{x.DrawingNumber} - " +
                            $"{x.DrawingName ?? x.Item.ItemName}",

                        IsExactMatch =
                            NameSimilarityHelper
                                .IsExactMatch(
                                    x.DrawingNumber,
                                    drawingNumber)
                    })

                .ToList();
        }


        private async Task<List<CustomerDrawingSuggestion>>
            FindDrawingNameMatchesAsync(
                int customerId,
                string drawingName,
                string? excludedDrawingNumber = null)
        {
            var drawings =
                await _customerDrawingService
                    .GetAllAsync();


            return drawings

                .Where(x =>
                    x.CustomerId ==
                        customerId)

                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.DrawingName))

                .Where(x =>
                    string.IsNullOrWhiteSpace(
                        excludedDrawingNumber)
                    ||
                    !NameSimilarityHelper
                        .IsExactMatch(
                            x.DrawingNumber,
                            excludedDrawingNumber))

                .Where(x =>
                    NameSimilarityHelper
                        .IsExactMatch(
                            x.DrawingName,
                            drawingName)

                    ||

                    NameSimilarityHelper
                        .IsSimilarMatch(
                            x.DrawingName,
                            drawingName))

                .OrderByDescending(x =>
                    NameSimilarityHelper
                        .IsExactMatch(
                            x.DrawingName,
                            drawingName))

                .ThenBy(x =>
                    x.DrawingName)

                .Take(5)

                .Select(x =>
                    new CustomerDrawingSuggestion
                    {
                        CustomerDrawingId =
                            x.CustomerDrawingId,

                        DisplayText =
                            $"{x.DrawingNumber} - " +
                            $"{x.DrawingName}",

                        IsExactMatch =
                            NameSimilarityHelper
                                .IsExactMatch(
                                    x.DrawingName,
                                    drawingName)
                    })

                .ToList();
        }


        private async Task<bool>
            HasExactDrawingNumberAsync(
                int customerId,
                string drawingNumber)
        {
            if (customerId <= 0 ||
                string.IsNullOrWhiteSpace(
                    drawingNumber))
            {
                return false;
            }


            var drawings =
                await _customerDrawingService
                    .GetAllAsync();


            return drawings.Any(x =>
                x.CustomerId ==
                    customerId
                &&
                NameSimilarityHelper
                    .IsExactMatch(
                        x.DrawingNumber,
                        drawingNumber));
        }


        private async Task<string?>
            GetExcludedDrawingNumberAsync(
                int? customerDrawingId)
        {
            if (!customerDrawingId.HasValue ||
                customerDrawingId.Value <= 0)
            {
                return null;
            }


            var drawing =
                await _customerDrawingService
                    .GetByIdAsync(
                        customerDrawingId.Value);


            return drawing?
                .DrawingNumber;
        }

        #endregion


        #region Form Dropdowns

        private async Task LoadFormLookupsAsync(
            CustomerDrawingFormViewModel model)
        {
            await LoadCustomersAsync(
                model);

            await LoadItemsAsync(
                model);
        }


        private async Task LoadCustomersAsync(
            CustomerDrawingFormViewModel model)
        {
            var customers =
                await _customerService
                    .GetAllAsync();


            var availableCustomers =
                customers

                    .Where(x =>
                        x.IsActive ||
                        x.Id ==
                            model.CustomerId)

                    .OrderBy(x =>
                        x.CustomerName)

                    .ToList();


            model.Customers =
                availableCustomers

                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id
                                    .ToString(),

                            Text =
                                x.CustomerName,

                            Selected =
                                x.Id ==
                                model.CustomerId
                        })

                    .ToList();
        }


        private async Task LoadItemsAsync(
            CustomerDrawingFormViewModel model)
        {
            var items =
                await _itemService
                    .GetAllAsync();


            var availableItems =
                items

                    .Where(x =>
                        x.IsActive ||
                        x.ItemId ==
                            model.ItemId)

                    .OrderBy(x =>
                        x.ItemName)

                    .ThenBy(x =>
                        x.ItemCode)

                    .ToList();


            model.Items =
                availableItems

                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.ItemId
                                    .ToString(),

                            Text =
                                BuildItemDisplayText(
                                    x),

                            Selected =
                                x.ItemId ==
                                model.ItemId
                        })

                    .ToList();
        }


        private static string BuildItemDisplayText(
            Item item)
        {
            var parts =
                new List<string>
                {
                    $"{item.ItemCode} - {item.ItemName}"
                };


            if (!string.IsNullOrWhiteSpace(
                item.PartNumber))
            {
                parts.Add(
                    $"Part: {item.PartNumber}");
            }


            if (item.Shape != null)
            {
                parts.Add(
                    $"Shape: {item.Shape.ShapeName}");
            }


            var specifications =
                item.ItemSpecifications

                    .Where(x =>
                        !x.IsDeleted)

                    .OrderBy(x =>
                        x.SortOrder)

                    .Take(3)

                    .Select(x =>
                    {
                        var specificationName =
                            x.Specification?
                                .SpecificationName
                            ??
                            "Spec";


                        var uom =
                            x.Uom != null
                                ? $" {x.Uom.UomCode}"
                                : string.Empty;


                        return
                            $"{specificationName}: " +
                            $"{x.SpecificationValue}" +
                            $"{uom}";
                    })

                    .ToList();


            parts.AddRange(
                specifications);


            return string.Join(
                " | ",
                parts);
        }

        #endregion


        #region Revision History Mapping

        private static List<
            CustomerDrawingRevisionHistoryViewModel>
            MapRevisionHistory(
                IEnumerable<CustomerDrawing> history)
        {
            return history

                .OrderByDescending(x =>
                    x.IsActive)

                .ThenByDescending(x =>
                    x.CustomerDrawingId)

                .Select(x =>
                    new CustomerDrawingRevisionHistoryViewModel
                    {
                        CustomerDrawingId =
                            x.CustomerDrawingId,

                        RevisionNumber =
                            x.RevisionNumber
                            ??
                            string.Empty,

                        FileName =
                            x.FileName,

                        FilePath =
                            x.FilePath,

                        Description =
                            x.Description,

                        IsCurrent =
                            x.IsActive,

                        CreatedOn =
                            x.CreatedOn,

                        CreatedBy =
                            x.CreatedBy
                    })

                .ToList();
        }

        #endregion


        #region Normalization

        private static void NormalizeModel(
            CustomerDrawingFormViewModel model)
        {
            model.DrawingNumber =
                NormalizeUpperText(
                    model.DrawingNumber)
                ??
                string.Empty;


            model.DrawingName =
                NormalizeText(
                    model.DrawingName);


            model.DrawingType =
                NormalizeText(
                    model.DrawingType);


            model.RevisionNumber =
                NormalizeUpperText(
                    model.RevisionNumber);


            model.Description =
                NormalizeText(
                    model.Description);


            foreach (var revision
                in model.NewRevisions)
            {
                NormalizeRevisionModel(
                    revision);
            }
        }


        private static void NormalizeRevisionModel(
            CustomerDrawingRevisionInputViewModel model)
        {
            model.RevisionNumber =
                NormalizeUpperText(
                    model.RevisionNumber)
                ??
                string.Empty;


            model.Description =
                NormalizeText(
                    model.Description);
        }


        private static string?
            NormalizeText(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return null;
            }


            return Regex.Replace(
                value.Trim(),
                @"\s+",
                " ");
        }


        private static string?
            NormalizeUpperText(
                string? value)
        {
            return NormalizeText(
                value)?
                .ToUpperInvariant();
        }

        #endregion


        #region File Storage

        private async Task<
            (string FileName, string FilePath)>
            SaveCustomerDrawingFileAsync(
                IFormFile drawingFile)
        {
            if (drawingFile.Length <= 0)
            {
                throw new BusinessException(
                    "Selected Customer Drawing file is empty.");
            }


            if (drawingFile.Length >
                MaxDrawingFileSize)
            {
                throw new BusinessException(
                    "Customer Drawing file cannot exceed 25 MB.");
            }


            var originalFileName =
                Path.GetFileName(
                    drawingFile.FileName);


            var extension =
                Path.GetExtension(
                    originalFileName);


            if (string.IsNullOrWhiteSpace(
                    extension)
                ||
                !AllowedDrawingExtensions
                    .Contains(
                        extension))
            {
                throw new BusinessException(
                    "Allowed Customer Drawing file types are " +
                    "PDF, JPG, JPEG, PNG, DWG and DXF.");
            }


            var webRootPath =
                _webHostEnvironment
                    .WebRootPath;


            if (string.IsNullOrWhiteSpace(
                webRootPath))
            {
                webRootPath =
                    Path.Combine(
                        _webHostEnvironment
                            .ContentRootPath,
                        "wwwroot");
            }


            var uploadDirectory =
                Path.Combine(
                    webRootPath,
                    "uploads",
                    "customer-drawings");


            Directory.CreateDirectory(
                uploadDirectory);


            var storedFileName =
                $"{Guid.NewGuid():N}" +
                extension.ToLowerInvariant();


            var physicalPath =
                Path.Combine(
                    uploadDirectory,
                    storedFileName);


            await using (
                var stream =
                    new FileStream(
                        physicalPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
            {
                await drawingFile
                    .CopyToAsync(
                        stream);
            }


            return (
                originalFileName,

                $"/uploads/customer-drawings/" +
                storedFileName);
        }


        private void DeletePhysicalFiles(
            IEnumerable<string> paths)
        {
            foreach (var path
                in paths)
            {
                DeletePhysicalFile(
                    path);
            }
        }


        private void DeletePhysicalFile(
            string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                relativeFilePath))
            {
                return;
            }


            if (!relativeFilePath.StartsWith(
                "/uploads/customer-drawings/",
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }


            try
            {
                var webRootPath =
                    _webHostEnvironment
                        .WebRootPath;


                if (string.IsNullOrWhiteSpace(
                    webRootPath))
                {
                    webRootPath =
                        Path.Combine(
                            _webHostEnvironment
                                .ContentRootPath,
                            "wwwroot");
                }


                var normalizedRelativePath =
                    relativeFilePath

                        .TrimStart(
                            '/',
                            '\\')

                        .Replace(
                            '/',
                            Path.DirectorySeparatorChar);


                var physicalPath =
                    Path.Combine(
                        webRootPath,
                        normalizedRelativePath);


                if (System.IO.File.Exists(
                    physicalPath))
                {
                    System.IO.File.Delete(
                        physicalPath);
                }
            }
            catch
            {
                /*
                 * File cleanup failure must not
                 * crash business transaction.
                 */
            }
        }

        #endregion


        #region Activate Revision

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ActivateRevision(
                int id,
                string returnAction = "Details")
        {
            try
            {
                await _customerDrawingService
                    .ActivateRevisionAsync(
                        id);


                TempData["Success"] =
                    "Customer Drawing revision activated successfully.";
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


            if (string.Equals(
                returnAction,
                "Edit",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id
                    });
            }


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        #endregion


        #region Delete Revision

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteRevision(
                int id,
                int customerDrawingId,
                string returnAction = "Details")
        {
            try
            {
                await _customerDrawingService
                    .DeleteRevisionAsync(
                        id);


                TempData["Success"] =
                    "Customer Drawing revision deleted successfully.";
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


            if (string.Equals(
                returnAction,
                "Edit",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id =
                            customerDrawingId
                    });
            }


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id =
                        customerDrawingId
                });
        }

        #endregion


        #region Deleted Drawings

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var drawings =
                await _customerDrawingService
                    .GetDeletedDrawingsAsync();


            return View(
                drawings);
        }

        #endregion


        #region Restore Drawing

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _customerDrawingService
                    .RestoreAsync(
                        id);


                TempData["Success"] =
                    "Customer Drawing restored successfully.";
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
                nameof(Deleted));
        }

        #endregion


        #region Private Model

        private sealed class CustomerDrawingSuggestion
        {
            public int CustomerDrawingId
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