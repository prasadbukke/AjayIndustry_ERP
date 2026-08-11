/*
==============================================================

File : SupplierController.cs

Purpose :
Handles Supplier Master UI requests.

Features :
- CRUD
- Search
- Pagination
- Live Supplier Name similarity check
- Exact duplicate blocking
- Similar-name confirmation
- Soft Delete

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Supplier;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Handles Supplier Master CRUD operations.
    /// </summary>
    public class SupplierController : Controller
    {
        private readonly ISupplierService
            _supplierService;

        public SupplierController(
            ISupplierService supplierService)
        {
            _supplierService =
                supplierService;
        }

        #region Supplier List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                var suppliers =
                    await _supplierService
                        .SearchAsync(
                            searchText);

                ViewBag.SearchText =
                    searchText;

                ViewBag.PageNumber =
                    1;

                ViewBag.PageSize =
                    pageSize;

                ViewBag.TotalRecords =
                    suppliers.Count;

                ViewBag.TotalPages =
                    1;

                ViewBag.HasPrevious =
                    false;

                ViewBag.HasNext =
                    false;

                return View(suppliers);
            }

            var result =
                await _supplierService
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

        #region Create Supplier

        [HttpGet]
        public IActionResult Create()
        {
            return View(
                new SupplierViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SupplierViewModel model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var similarSuppliers =
                    await FindSimilarSuppliersAsync(
                        model.SupplierName);

                var exactMatch =
                    similarSuppliers
                        .FirstOrDefault(
                            x =>
                                x.IsExactMatch);

                if (exactMatch != null)
                {
                    model.SimilarSupplierNames =
                        similarSuppliers
                            .Select(x =>
                                x.DisplayText)
                            .ToList();

                    ModelState.AddModelError(
                        nameof(
                            model.SupplierName),
                        "Supplier Name already exists.");

                    return View(model);
                }

                if (similarSuppliers.Count > 0 &&
                    !model
                        .ConfirmSimilarSupplierName)
                {
                    model.SimilarSupplierNames =
                        similarSuppliers
                            .Select(x =>
                                x.DisplayText)
                            .ToList();

                    return View(model);
                }

                var supplier =
                    MapToEntity(model);

                await _supplierService
                    .CreateAsync(supplier);

                TempData["Success"] =
                    "Supplier created successfully.";

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

        #region Supplier Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var supplier =
                await _supplierService
                    .GetByIdAsync(id);

            if (supplier == null)
            {
                TempData["Error"] =
                    "Supplier not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(supplier);
        }

        #endregion

        #region Edit Supplier

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var supplier =
                await _supplierService
                    .GetByIdAsync(id);

            if (supplier == null)
            {
                TempData["Error"] =
                    "Supplier not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var model =
                new SupplierViewModel
                {
                    SupplierId =
                        supplier.SupplierId,

                    SupplierCode =
                        supplier.SupplierCode,

                    SupplierName =
                        supplier.SupplierName,

                    ContactPerson =
                        supplier.ContactPerson,

                    MobileNumber =
                        supplier.MobileNumber,

                    AlternateMobileNumber =
                        supplier.AlternateMobileNumber,

                    Email =
                        supplier.Email,

                    Gstin =
                        supplier.Gstin,

                    Pan =
                        supplier.Pan,

                    AddressLine1 =
                        supplier.AddressLine1,

                    AddressLine2 =
                        supplier.AddressLine2,

                    City =
                        supplier.City,

                    State =
                        supplier.State,

                    Pincode =
                        supplier.Pincode,

                    PaymentTermsDays =
                        supplier.PaymentTermsDays,

                    Description =
                        supplier.Description,

                    IsActive =
                        supplier.IsActive
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            SupplierViewModel model)
        {
            ModelState.Remove(
                nameof(
                    SupplierViewModel
                        .SupplierCode));

            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var similarSuppliers =
                    await FindSimilarSuppliersAsync(
                        model.SupplierName,
                        model.SupplierId);

                var exactMatch =
                    similarSuppliers
                        .FirstOrDefault(
                            x =>
                                x.IsExactMatch);

                if (exactMatch != null)
                {
                    model.SimilarSupplierNames =
                        similarSuppliers
                            .Select(x =>
                                x.DisplayText)
                            .ToList();

                    ModelState.AddModelError(
                        nameof(
                            model.SupplierName),
                        "Supplier Name already exists.");

                    return View(model);
                }

                if (similarSuppliers.Count > 0 &&
                    !model
                        .ConfirmSimilarSupplierName)
                {
                    model.SimilarSupplierNames =
                        similarSuppliers
                            .Select(x =>
                                x.DisplayText)
                            .ToList();

                    return View(model);
                }

                var supplier =
                    MapToEntity(model);

                await _supplierService
                    .UpdateAsync(supplier);

                TempData["Success"] =
                    "Supplier updated successfully.";

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

        #region Delete Supplier

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _supplierService
                    .DeleteAsync(id);

                TempData["Success"] =
                    "Supplier deleted successfully.";
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

        #region Live Similar Supplier Name

        /// <summary>
        /// Returns exact and similar Supplier Names
        /// while the user is typing.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckSimilarName(
            string supplierName,
            int? supplierId = null)
        {
            var matches =
                await FindSimilarSuppliersAsync(
                    supplierName,
                    supplierId);

            return Json(new
            {
                success = true,

                hasMatches =
                    matches.Count > 0,

                hasExactMatch =
                    matches.Any(x =>
                        x.IsExactMatch),

                records =
                    matches.Select(x => new
                    {
                        id =
                            x.SupplierId,

                        displayText =
                            x.DisplayText,

                        isExactMatch =
                            x.IsExactMatch
                    })
            });
        }

        #endregion

        #region Similar Supplier Methods

        private async Task<List<SupplierSuggestion>>
            FindSimilarSuppliersAsync(
                string supplierName,
                int? excludedSupplierId = null)
        {
            if (string.IsNullOrWhiteSpace(
                    supplierName) ||
                supplierName
                    .Trim()
                    .Length < 3)
            {
                return new List<
                    SupplierSuggestion>();
            }

            var suppliers =
                await _supplierService
                    .GetAllAsync();

            var availableSuppliers =
                suppliers
                    .Where(x =>
                        !excludedSupplierId.HasValue ||
                        x.SupplierId !=
                            excludedSupplierId.Value)
                    .ToList();

            var matches =
                NameSimilarityHelper
                    .FindMatches(
                        availableSuppliers,
                        supplierName,
                        x => x.SupplierName,
                        5);

            return matches
                .Select(x =>
                    new SupplierSuggestion
                    {
                        SupplierId =
                            x.SupplierId,

                        DisplayText =
                            $"{x.SupplierCode} - {x.SupplierName}",

                        IsExactMatch =
                            NameSimilarityHelper
                                .IsExactMatch(
                                    supplierName,
                                    x.SupplierName)
                    })
                .ToList();
        }

        #endregion

        #region Entity Mapping

        private static Supplier MapToEntity(
            SupplierViewModel model)
        {
            return new Supplier
            {
                SupplierId =
                    model.SupplierId,

                SupplierName =
                    model.SupplierName,

                ContactPerson =
                    model.ContactPerson,

                MobileNumber =
                    model.MobileNumber,

                AlternateMobileNumber =
                    model.AlternateMobileNumber,

                Email =
                    model.Email,

                Gstin =
                    model.Gstin,

                Pan =
                    model.Pan,

                AddressLine1 =
                    model.AddressLine1,

                AddressLine2 =
                    model.AddressLine2,

                City =
                    model.City,

                State =
                    model.State,

                Pincode =
                    model.Pincode,

                PaymentTermsDays =
                    model.PaymentTermsDays,

                Description =
                    model.Description,

                IsActive =
                    model.IsActive
            };
        }

        #endregion

        #region Model Normalization

        private static void NormalizeModel(
            SupplierViewModel model)
        {
            model.SupplierName =
                NormalizeText(
                    model.SupplierName)
                ?? string.Empty;

            model.ContactPerson =
                NormalizeText(
                    model.ContactPerson);

            model.MobileNumber =
                NormalizeText(
                    model.MobileNumber);

            model.AlternateMobileNumber =
                NormalizeText(
                    model.AlternateMobileNumber);

            model.Email =
                NormalizeText(
                    model.Email)?
                    .ToLowerInvariant();

            model.Gstin =
                NormalizeText(
                    model.Gstin)?
                    .ToUpperInvariant();

            model.Pan =
                NormalizeText(
                    model.Pan)?
                    .ToUpperInvariant();

            model.AddressLine1 =
                NormalizeText(
                    model.AddressLine1);

            model.AddressLine2 =
                NormalizeText(
                    model.AddressLine2);

            model.City =
                NormalizeText(
                    model.City);

            model.State =
                NormalizeText(
                    model.State);

            model.Pincode =
                NormalizeText(
                    model.Pincode);

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

            return System.Text.RegularExpressions
                .Regex.Replace(
                    value.Trim(),
                    @"\s+",
                    " ");
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Internal Supplier Name suggestion record.
        /// </summary>
        private sealed class SupplierSuggestion
        {
            public int SupplierId
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