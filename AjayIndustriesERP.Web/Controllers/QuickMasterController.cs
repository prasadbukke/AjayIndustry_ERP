/*
==============================================================

File : QuickMasterController.cs

Purpose :
Handles reusable AJAX Quick Create operations for
Category, Brand and UOM masters.

Features :
- Loads Quick Create forms inside Bootstrap Modal.
- Provides live exact/similar-name suggestions.
- Blocks exact duplicates.
- Requires confirmation for similar names.
- Returns newly created record for automatic dropdown selection.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Provides common Quick Create operations for masters
    /// used inside transactional and relational forms.
    /// </summary>

    public class QuickMasterController : Controller
    {
        private const string CategoryType = "Category";
        private const string BrandType = "Brand";
        private const string UomType = "Uom";



        private readonly IItemCategoryService _itemCategoryService;
        private readonly IBrandService _brandService;
        private readonly IUomService _uomService;

        public QuickMasterController(
            IItemCategoryService itemCategoryService,
            IBrandService brandService,
            IUomService uomService)
        {
            _itemCategoryService = itemCategoryService;
            _brandService = brandService;
            _uomService = uomService;
        }









        #region Live Suggestions

        /// <summary>
        /// Returns exact and similar existing records while
        /// the user types a master name.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Suggestions(
            string masterType,
            string name)
        {
            var normalizedMasterType =
                NormalizeMasterType(masterType);

            if (string.IsNullOrWhiteSpace(name) ||
                name.Trim().Length < 3)
            {
                return Json(new
                {
                    success = true,
                    records = Array.Empty<object>()
                });
            }

            var suggestions =
                await GetSuggestionsAsync(
                    normalizedMasterType,
                    name);

            if (suggestions == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid master type."
                });
            }

            return Json(new
            {
                success = true,
                records = suggestions.Select(x => new
                {
                    id = x.Id,
                    code = x.Code,
                    name = x.Name,
                    displayText = x.DisplayText,
                    isExactMatch = x.IsExactMatch
                })
            });
        }

        #endregion

        #region Save Quick Master

        /// <summary>
        /// Creates Category, Brand or UOM using the common modal.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            QuickCreateMasterViewModel model)
        {
            model.MasterType =
                NormalizeMasterType(model.MasterType);

            if (string.IsNullOrWhiteSpace(model.MasterType))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid master type."
                });
            }

            if (!ConfigureModel(model))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid master type."
                });
            }

            NormalizeFormValues(model);

            /*
             * Revalidate after trimming and normalizing the values.
             */
            ModelState.Clear();

            TryValidateModel(model);

            ValidateConditionalFields(model);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage)
                        ? "Invalid value."
                        : x.ErrorMessage)
                    .Distinct()
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors
                });
            }

            var suggestions =
                await GetSuggestionsAsync(
                    model.MasterType,
                    model.Name);

            if (suggestions == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid master type."
                });
            }

            var suggestionResponse = suggestions
                .Select(x => new
                {
                    id = x.Id,
                    code = x.Code,
                    name = x.Name,
                    displayText = x.DisplayText,
                    isExactMatch = x.IsExactMatch
                })
                .ToList();

            var exactMatches = suggestions
                .Where(x => x.IsExactMatch)
                .ToList();

            if (exactMatches.Count > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "A record with the same name already exists.",
                    isExactDuplicate = true,
                    records = suggestionResponse
                });
            }

            var similarMatches = suggestions
                .Where(x => !x.IsExactMatch)
                .ToList();

            if (similarMatches.Count > 0 &&
                !model.ConfirmSimilarName)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Please review and confirm the similar records.",
                    requiresConfirmation = true,
                    records = suggestionResponse
                });
            }

            try
            {
                return model.MasterType switch
                {
                    CategoryType =>
                        await CreateCategoryAsync(model),

                    BrandType =>
                        await CreateBrandAsync(model),

                    UomType =>
                        await CreateUomAsync(model),

                    _ => BadRequest(new
                    {
                        success = false,
                        message = "Invalid master type."
                    })
                };
            }
            catch (BusinessException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message =
                            "Something went wrong. Please try again."
                    });
            }
        }

        #endregion

        #region Create Individual Masters

        private async Task<IActionResult> CreateCategoryAsync(
            QuickCreateMasterViewModel model)
        {
            var category = new ItemCategory
            {
                CategoryName = model.Name,
                Description = model.Description,
                IsActive = true
            };

            await _itemCategoryService.CreateAsync(category);

            return Json(new
            {
                success = true,
                masterType = CategoryType,
                id = category.ItemCategoryId,
                text =
                    $"{category.CategoryCode} - {category.CategoryName}",
                message = "Category created successfully."
            });
        }

        private async Task<IActionResult> CreateBrandAsync(
            QuickCreateMasterViewModel model)
        {
            var brand = new Brand
            {
                BrandName = model.Name,
                Description = model.Description,
                IsActive = true
            };

            await _brandService.CreateAsync(brand);

            return Json(new
            {
                success = true,
                masterType = BrandType,
                id = brand.BrandId,
                text =
                    $"{brand.BrandCode} - {brand.BrandName}",
                message = "Brand created successfully."
            });
        }

        private async Task<IActionResult> CreateUomAsync(
            QuickCreateMasterViewModel model)
        {
            var uom = new Uom
            {
                UomCode =
                    model.Code?.Trim().ToUpperInvariant()
                    ?? string.Empty,

                UomName = model.Name,
                Description = model.Description,
                IsActive = true
            };

            await _uomService.CreateAsync(uom);

            return Json(new
            {
                success = true,
                masterType = UomType,
                id = uom.UomId,
                text =
                    $"{uom.UomCode} - {uom.UomName}",
                message = "UOM created successfully."
            });
        }

        #endregion

        #region Similar Record Loading

        private async Task<List<QuickCreateSuggestionViewModel>?>
            GetSuggestionsAsync(
                string masterType,
                string enteredName)
        {
            switch (masterType)
            {
                case CategoryType:
                    {
                        var categories =
                            (await _itemCategoryService.GetAllAsync())
                            .Where(x => x.IsActive)
                            .ToList();

                        var matches =
                            NameSimilarityHelper.FindMatches(
                                categories,
                                enteredName,
                                x => x.CategoryName,
                                5);

                        return matches
                            .Select(x =>
                                new QuickCreateSuggestionViewModel
                                {
                                    Id = x.ItemCategoryId,
                                    Code = x.CategoryCode,
                                    Name = x.CategoryName,
                                    IsExactMatch =
                                        NameSimilarityHelper.IsExactMatch(
                                            enteredName,
                                            x.CategoryName)
                                })
                            .ToList();
                    }

                case BrandType:
                    {
                        var brands =
                            (await _brandService.GetAllAsync())
                            .Where(x => x.IsActive)
                            .ToList();

                        var matches =
                            NameSimilarityHelper.FindMatches(
                                brands,
                                enteredName,
                                x => x.BrandName,
                                5);

                        return matches
                            .Select(x =>
                                new QuickCreateSuggestionViewModel
                                {
                                    Id = x.BrandId,
                                    Code = x.BrandCode,
                                    Name = x.BrandName,
                                    IsExactMatch =
                                        NameSimilarityHelper.IsExactMatch(
                                            enteredName,
                                            x.BrandName)
                                })
                            .ToList();
                    }

                case UomType:
                    {
                        var uoms =
                            (await _uomService.GetAllAsync())
                            .Where(x => x.IsActive)
                            .ToList();

                        var matches =
                            NameSimilarityHelper.FindMatches(
                                uoms,
                                enteredName,
                                x => x.UomName,
                                5);

                        return matches
                            .Select(x =>
                                new QuickCreateSuggestionViewModel
                                {
                                    Id = x.UomId,
                                    Code = x.UomCode,
                                    Name = x.UomName,
                                    IsExactMatch =
                                        NameSimilarityHelper.IsExactMatch(
                                            enteredName,
                                            x.UomName)
                                })
                            .ToList();
                    }

                default:
                    return null;
            }
        }

        #endregion

        #region Model Configuration

        private bool ConfigureModel(
            QuickCreateMasterViewModel model)
        {
            model.FormAction =
                Url.Action(
                    nameof(Create),
                    "QuickMaster")
                ?? string.Empty;

            model.SimilarCheckUrl =
                Url.Action(
                    nameof(Suggestions),
                    "QuickMaster")
                ?? string.Empty;

            switch (model.MasterType)
            {
                case CategoryType:
                    model.MasterTitle = "Add Category";
                    model.NameLabel = "Category Name";
                    model.CodeLabel = "Category Code";
                    model.RequiresCode = false;
                    return true;

                case BrandType:
                    model.MasterTitle = "Add Brand";
                    model.NameLabel = "Brand Name";
                    model.CodeLabel = "Brand Code";
                    model.RequiresCode = false;
                    return true;

                case UomType:
                    model.MasterTitle = "Add UOM";
                    model.NameLabel = "UOM Name";
                    model.CodeLabel = "UOM Code";
                    model.RequiresCode = true;
                    return true;

                default:
                    return false;
            }
        }

        private static string NormalizeMasterType(
            string? masterType)
        {
            if (string.IsNullOrWhiteSpace(masterType))
            {
                return string.Empty;
            }

            return masterType
                .Trim()
                .ToLowerInvariant() switch
            {
                "category" => CategoryType,
                "itemcategory" => CategoryType,
                "brand" => BrandType,
                "uom" => UomType,
                "unit" => UomType,
                _ => string.Empty
            };
        }

        private static void NormalizeFormValues(
            QuickCreateMasterViewModel model)
        {
            model.Name =
                model.Name?.Trim() ?? string.Empty;

            model.Code =
                string.IsNullOrWhiteSpace(model.Code)
                    ? null
                    : model.Code.Trim().ToUpperInvariant();

            model.Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim();
        }

        private void ValidateConditionalFields(
            QuickCreateMasterViewModel model)
        {
            if (model.RequiresCode &&
                string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError(
                    nameof(model.Code),
                    $"{model.CodeLabel} is required.");
            }
        }

        #endregion


    }
}