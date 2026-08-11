/*
==============================================================

File : DrawingController.cs

Purpose :
Handles Drawing Master CRUD and Drawing revision
history workflow.

Important Rules :
- Drawing Number is entered only during Create.
- Drawing Number cannot change after creation.
- Existing revisions are never overwritten.
- New revisions create new Drawing rows.
- Old Drawing files are preserved.
- Drawing Number and Name support live suggestions.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Drawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Handles Drawing Master operations.
    /// </summary>
    public class DrawingController : Controller
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

        private readonly IDrawingService
            _drawingService;

        private readonly IItemService
            _itemService;

        private readonly IWebHostEnvironment
            _webHostEnvironment;

        public DrawingController(
            IDrawingService drawingService,
            IItemService itemService,
            IWebHostEnvironment webHostEnvironment)
        {
            _drawingService =
                drawingService;

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
                    await _drawingService
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

                return View(drawings);
            }

            var result =
                await _drawingService
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

            return View(result.Items);
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<IActionResult> Create(
            int? itemId = null)
        {
            var model =
                new DrawingViewModel();

            if (itemId.HasValue &&
                itemId.Value > 0)
            {
                model.ItemId =
                    itemId.Value;
            }

            await LoadItemsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DrawingViewModel model)
        {
            NormalizeModel(model);

            
            

            await LoadSimilarityWarningsAsync(
                model);

            /*
             * Exact current Drawing Number blocks
             * Create before file upload.
             */
            var exactDrawingNumber =
                await HasExactDrawingNumberAsync(
                    model.DrawingNumber);

            if (exactDrawingNumber)
            {
                ModelState.AddModelError(
                    nameof(model.DrawingNumber),
                    "Drawing Number already exists. " +
                    "Open the existing Drawing and add " +
                    "a new Revision.");
            }

            if (!ModelState.IsValid)
            {
                await LoadItemsAsync(model);

                return View(model);
            }

            string? newlySavedFilePath =
                null;

            try
            {
                if (model.DrawingFile != null)
                {
                    var savedFile =
                        await SaveDrawingFileAsync(
                            model.DrawingFile);

                    model.FileName =
                        savedFile.FileName;

                    model.FilePath =
                        savedFile.FilePath;

                    newlySavedFilePath =
                        savedFile.FilePath;
                }

                var drawing =
                    new Drawing
                    {
                        ItemId =
                            model.ItemId,

                        DrawingNumber =
                            model.DrawingNumber,

                        DrawingName =
                            model.DrawingName,

                        DrawingType =
                            model.DrawingType,

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

                await _drawingService
                    .CreateAsync(
                        drawing);

                TempData["Success"] =
                    "Drawing created successfully.";

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

                await LoadItemsAsync(model);

                return View(model);
            }
            catch (Exception)
            {
                DeletePhysicalFile(
                    newlySavedFilePath);

                TempData["Error"] =
                    "Something went wrong. Please try again.";

                await LoadItemsAsync(model);

                return View(model);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var selectedRevision =
                await _drawingService
                    .GetByIdAsync(id);

            if (selectedRevision == null)
            {
                TempData["Error"] =
                    "Drawing not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var history =
                await _drawingService
                    .GetRevisionHistoryAsync(id);

            var currentDrawing =
                history.FirstOrDefault(x =>
                    x.IsActive)
                ?? selectedRevision;

            /*
             * Existing Details view still accepts Drawing.
             *
             * New Details UI will read RevisionHistory
             * from ViewBag.
             */
            ViewBag.RevisionHistory =
                history;

            return View(currentDrawing);
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var selectedRevision =
                await _drawingService
                    .GetByIdAsync(id);

            if (selectedRevision == null)
            {
                TempData["Error"] =
                    "Drawing not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var history =
                await _drawingService
                    .GetRevisionHistoryAsync(id);

            var currentDrawing =
                history.FirstOrDefault(x =>
                    x.IsActive)
                ?? selectedRevision;

            var model =
                new DrawingViewModel
                {
                    DrawingId =
                        currentDrawing.DrawingId,

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

            await LoadItemsAsync(model);

            await LoadSimilarityWarningsAsync(
                model,
                currentDrawing.DrawingNumber);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            DrawingViewModel model)
        {
            NormalizeModel(model);

            var existingDrawing =
                await _drawingService
                    .GetByIdAsync(
                        model.DrawingId);

            if (existingDrawing == null)
            {
                TempData["Error"] =
                    "Drawing not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var history =
                await _drawingService
                    .GetRevisionHistoryAsync(
                        model.DrawingId);

            var currentDrawing =
                history.FirstOrDefault(x =>
                    x.IsActive)
                ?? existingDrawing;

            /*
             * Drawing Number is permanent.
             *
             * Ignore any posted/tampered value.
             */
            model.DrawingNumber =
                currentDrawing.DrawingNumber;

            /*
             * Existing current revision is also
             * read-only.
             */
            model.RevisionNumber =
                currentDrawing.RevisionNumber;

            model.FileName =
                currentDrawing.FileName;

            model.FilePath =
                currentDrawing.FilePath;

            /*
             * Old single-file Edit controls are ignored.
             * New revision rows are the only way to add
             * a new file/revision.
             */
            model.DrawingFile =
                null;

           

            if (!ModelState.IsValid)
            {
                model.RevisionHistory =
                    MapRevisionHistory(
                        history);

                await LoadItemsAsync(model);

                await LoadSimilarityWarningsAsync(
                    model,
                    currentDrawing.DrawingNumber);

                return View(model);
            }

            var savedNewFiles =
                new List<string>();

            try
            {
                var newRevisionEntities =
                    new List<Drawing>();

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
                            await SaveDrawingFileAsync(
                                revisionModel.DrawingFile);

                        fileName =
                            savedFile.FileName;

                        filePath =
                            savedFile.FilePath;

                        savedNewFiles.Add(
                            filePath);
                    }

                    newRevisionEntities.Add(
                        new Drawing
                        {
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

                var drawing =
                    new Drawing
                    {
                        DrawingId =
                            currentDrawing.DrawingId,

                        ItemId =
                            model.ItemId,

                        /*
                         * Service also protects this,
                         * but Controller sends the
                         * permanent value.
                         */
                        DrawingNumber =
                            currentDrawing.DrawingNumber,

                        DrawingName =
                            model.DrawingName,

                        DrawingType =
                            model.DrawingType,

                        

                        IsActive =
                            true
                    };

                await _drawingService
                    .UpdateAsync(
                        drawing,
                        newRevisionEntities);

                TempData["Success"] =
                    newRevisionEntities.Count > 0
                        ? "Drawing updated and new revision added successfully."
                        : "Drawing updated successfully.";

                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id = model.DrawingId
                    });
            }
            catch (BusinessException ex)
            {
                DeletePhysicalFiles(
                    savedNewFiles);

                TempData["Error"] =
                    ex.Message;

                var refreshedHistory =
                    await _drawingService
                        .GetRevisionHistoryAsync(
                            model.DrawingId);

                model.RevisionHistory =
                    MapRevisionHistory(
                        refreshedHistory);

                await LoadItemsAsync(model);

                await LoadSimilarityWarningsAsync(
                    model,
                    currentDrawing.DrawingNumber);

                return View(model);
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

                await LoadItemsAsync(model);

                return View(model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                /*
                 * Service soft deletes complete Drawing
                 * revision history.
                 *
                 * Physical historical files remain.
                 */
                await _drawingService
                    .DeleteAsync(id);

                TempData["Success"] =
                    "Drawing deleted successfully.";
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

        #region Live Drawing Number Check

        [HttpGet]
        public async Task<IActionResult>
            CheckSimilarNumber(
                string drawingNumber,
                int? drawingId = null)
        {
            if (string.IsNullOrWhiteSpace(
                drawingNumber))
            {
                return Json(
                    new
                    {
                        success = true,
                        hasMatches = false,
                        hasExactMatch = false,
                        records =
                            Array.Empty<object>()
                    });
            }

            var excludedDrawingNumber =
                await GetExcludedDrawingNumberAsync(
                    drawingId);

            var matches =
                await FindDrawingNumberMatchesAsync(
                    drawingNumber,
                    excludedDrawingNumber);

            return Json(
                new
                {
                    success = true,

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
                                    x.DrawingId,

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
                string drawingName,
                int? drawingId = null)
        {
            if (string.IsNullOrWhiteSpace(
                drawingName))
            {
                return Json(
                    new
                    {
                        success = true,
                        hasMatches = false,
                        hasExactMatch = false,
                        records =
                            Array.Empty<object>()
                    });
            }

            var excludedDrawingNumber =
                await GetExcludedDrawingNumberAsync(
                    drawingId);

            var matches =
                await FindDrawingNameMatchesAsync(
                    drawingName,
                    excludedDrawingNumber);

            return Json(
                new
                {
                    success = true,

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
                                    x.DrawingId,

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
                DrawingViewModel model,
                string? excludedDrawingNumber = null)
        {
            if (!string.IsNullOrWhiteSpace(
                model.DrawingNumber))
            {
                model.SimilarDrawingNumbers =
                    (
                        await FindDrawingNumberMatchesAsync(
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
                            model.DrawingName,
                            excludedDrawingNumber)
                    )
                    .Select(x =>
                        x.DisplayText)
                    .ToList();
            }
        }

        private async Task<List<DrawingSuggestion>>
            FindDrawingNumberMatchesAsync(
                string drawingNumber,
                string? excludedDrawingNumber = null)
        {
            var drawings =
                await _drawingService
                    .GetAllAsync();

            var normalizedSearch =
                NameSimilarityHelper.Normalize(
                    drawingNumber);

            return drawings
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
                    new DrawingSuggestion
                    {
                        DrawingId =
                            x.DrawingId,

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

        private async Task<List<DrawingSuggestion>>
            FindDrawingNameMatchesAsync(
                string drawingName,
                string? excludedDrawingNumber = null)
        {
            var drawings =
                await _drawingService
                    .GetAllAsync();

            return drawings
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
                    new DrawingSuggestion
                    {
                        DrawingId =
                            x.DrawingId,

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
                string drawingNumber)
        {
            if (string.IsNullOrWhiteSpace(
                drawingNumber))
            {
                return false;
            }

            var drawings =
                await _drawingService
                    .GetAllAsync();

            return drawings.Any(x =>
                NameSimilarityHelper
                    .IsExactMatch(
                        x.DrawingNumber,
                        drawingNumber));
        }

        private async Task<string?>
            GetExcludedDrawingNumberAsync(
                int? drawingId)
        {
            if (!drawingId.HasValue ||
                drawingId.Value <= 0)
            {
                return null;
            }

            var drawing =
                await _drawingService
                    .GetByIdAsync(
                        drawingId.Value);

            return drawing?.DrawingNumber;
        }

        #endregion

        #region Item Dropdown

        private async Task LoadItemsAsync(
            DrawingViewModel model)
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
                                x.ItemId.ToString(),

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
                            ?? "Spec";

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
            DrawingRevisionHistoryViewModel>
            MapRevisionHistory(
                IEnumerable<Drawing> history)
        {
            return history
                .OrderByDescending(x =>
                    x.IsActive)
                .ThenByDescending(x =>
                    x.DrawingId)
                .Select(x =>
                    new DrawingRevisionHistoryViewModel
                    {
                        DrawingId =
                            x.DrawingId,

                        RevisionNumber =
                            x.RevisionNumber
                            ?? string.Empty,

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
            DrawingViewModel model)
        {
            model.DrawingNumber =
                NormalizeUpperText(
                    model.DrawingNumber)
                ?? string.Empty;

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
            DrawingRevisionInputViewModel model)
        {
            model.RevisionNumber =
                NormalizeUpperText(
                    model.RevisionNumber)
                ?? string.Empty;

            model.Description =
                NormalizeText(
                    model.Description);
        }

        private static string? NormalizeText(
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

        private static string? NormalizeUpperText(
            string? value)
        {
            return NormalizeText(value)?
                .ToUpperInvariant();
        }

        #endregion

        #region File Storage

        private async Task<
            (string FileName, string FilePath)>
            SaveDrawingFileAsync(
                IFormFile drawingFile)
        {
            if (drawingFile.Length <= 0)
            {
                throw new BusinessException(
                    "Selected Drawing file is empty.");
            }

            if (drawingFile.Length >
                MaxDrawingFileSize)
            {
                throw new BusinessException(
                    "Drawing file cannot exceed 25 MB.");
            }

            var originalFileName =
                Path.GetFileName(
                    drawingFile.FileName);

            var extension =
                Path.GetExtension(
                    originalFileName);

            if (string.IsNullOrWhiteSpace(
                    extension) ||
                !AllowedDrawingExtensions
                    .Contains(extension))
            {
                throw new BusinessException(
                    "Allowed Drawing file types are " +
                    "PDF, JPG, JPEG, PNG, DWG and DXF.");
            }

            var webRootPath =
                _webHostEnvironment.WebRootPath;

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
                    "drawings");

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
                    .CopyToAsync(stream);
            }

            return (
                originalFileName,
                $"/uploads/drawings/{storedFileName}");
        }

        private void DeletePhysicalFiles(
            IEnumerable<string> paths)
        {
            foreach (var path in paths)
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
                "/uploads/drawings/",
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var webRootPath =
                    _webHostEnvironment.WebRootPath;

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
                        .TrimStart('/', '\\')
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
                 * File cleanup failure should not
                 * crash the business transaction.
                 */
            }
        }

        #endregion

        #region Private Model

        private sealed class DrawingSuggestion
        {
            public int DrawingId { get; set; }

            public string DisplayText { get; set; } =
                string.Empty;

            public bool IsExactMatch { get; set; }
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
                await _drawingService
                    .ActivateRevisionAsync(id);

                TempData["Success"] =
                    "Drawing revision activated successfully.";
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
                int drawingId,
                string returnAction = "Details")
        {
            try
            {
                await _drawingService
                    .DeleteRevisionAsync(id);

                TempData["Success"] =
                    "Drawing revision deleted successfully.";
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
                        id = drawingId
                    });
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = drawingId
                });
        }

        #endregion

        #region Deleted Drawings

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var drawings =
                await _drawingService
                    .GetDeletedDrawingsAsync();

            return View(drawings);
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
                await _drawingService
                    .RestoreAsync(id);

                TempData["Success"] =
                    "Drawing restored successfully.";
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
    }
}