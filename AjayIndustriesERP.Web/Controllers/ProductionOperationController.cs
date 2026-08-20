/*
============================================================
File: ProductionOperationController.cs

Purpose:
Handles Production Operation Master HTTP requests.

Responsibilities:
- Display Operation Index with Search and Pagination.
- Display Operation Details.
- Create Operations.
- Edit Operations.
- Soft-delete Operations.
- Display deleted Operations separately.
- Restore deleted Operations.
- Map Web ViewModels to Domain entities.
- Map Domain entities to Web ViewModels.
- Display BusinessException and validation messages using
  shared Toast notifications.

Important:
- Business logic belongs in ProductionOperationService.
- Database access must never occur directly in Controller.
- Complex ViewModel POST parameters are named "viewModel"
  to avoid MVC model-binding prefix collisions.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.ProductionOperation;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class ProductionOperationController : Controller
    {
        #region Fields

        private readonly IProductionOperationService
            _productionOperationService;

        #endregion


        #region Constructor

        public ProductionOperationController(
            IProductionOperationService productionOperationService)
        {
            _productionOperationService =
                productionOperationService;
        }

        #endregion


        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result =
                await _productionOperationService
                    .SearchPagedAsync(
                        searchText,
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


        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var operation =
                await _productionOperationService
                    .GetByIdAsync(id);


            if (operation == null)
            {
                return NotFound();
            }


            var viewModel =
                MapToDetailsViewModel(
                    operation);


            return View(
                viewModel);
        }

        #endregion


        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel =
                new ProductionOperationFormViewModel
                {
                    OperationType =
                        ProductionOperationType.Production
                };


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProductionOperationFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var operation =
                    MapToDomain(
                        viewModel);


                await _productionOperationService
                    .CreateAsync(
                        operation);


                TempData["SuccessMessage"] =
                    "Production Operation created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var operation =
                await _productionOperationService
                    .GetByIdAsync(id);


            if (operation == null)
            {
                return NotFound();
            }


            var viewModel =
                MapToFormViewModel(
                    operation);


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductionOperationFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var operation =
                    MapToDomain(
                        viewModel);


                await _productionOperationService
                    .UpdateAsync(
                        operation);


                TempData["SuccessMessage"] =
                    "Production Operation updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
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
                await _productionOperationService
                    .DeleteAsync(id);


                TempData["SuccessMessage"] =
                    "Production Operation deleted successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }


            return RedirectToAction(
                nameof(Index));
        }

        #endregion


        #region Deleted Operations

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var operations =
                await _productionOperationService
                    .GetDeletedAsync();


            return View(
                operations);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _productionOperationService
                    .RestoreAsync(id);


                TempData["SuccessMessage"] =
                    "Production Operation restored successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }


            return RedirectToAction(
                nameof(Deleted));
        }

        #endregion


        #region Domain Mapping

        private static ProductionOperation MapToDomain(
            ProductionOperationFormViewModel viewModel)
        {
            return new ProductionOperation
            {
                Id =
                    viewModel.Id,

                Code =
                    viewModel.Code
                    ?? string.Empty,


                OperationName =
                    viewModel.OperationName,

                OperationType =
                    viewModel.OperationType,

                Description =
                    viewModel.Description,


                Remarks =
                    viewModel.Remarks
            };
        }

        #endregion


        #region Form ViewModel Mapping

        private static ProductionOperationFormViewModel
            MapToFormViewModel(
                ProductionOperation operation)
        {
            return new ProductionOperationFormViewModel
            {
                Id =
                    operation.Id,

                Code =
                    operation.Code,


                OperationName =
                    operation.OperationName,

                OperationType =
                    operation.OperationType,

                Description =
                    operation.Description,


                Remarks =
                    operation.Remarks
            };
        }

        #endregion


        #region Details ViewModel Mapping

        private static ProductionOperationDetailsViewModel
            MapToDetailsViewModel(
                ProductionOperation operation)
        {
            return new ProductionOperationDetailsViewModel
            {
                Id =
                    operation.Id,

                Code =
                    operation.Code,


                OperationName =
                    operation.OperationName,

                OperationType =
                    operation.OperationType,

                Description =
                    operation.Description,


                Remarks =
                    operation.Remarks
            };
        }

        #endregion


        #region Validation Message Helper

        private string GetModelStateErrorMessage()
        {
            var errors =
                ModelState.Values
                    .SelectMany(x =>
                        x.Errors)
                    .Select(x =>
                        x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();


            if (!errors.Any())
            {
                return
                    "Please correct the validation errors.";
            }


            return string.Join(
                " • ",
                errors);
        }

        #endregion
    }
}