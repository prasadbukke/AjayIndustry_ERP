/*
============================================================
File: ItemProcessRoutingController.cs

Purpose:
Handles Item Process Routing HTTP requests.

Responsibilities:
- Display Routing Index with Search and Pagination.
- Display Routing Details.
- Create first Draft Routing for an Item.
- Edit Draft Routing.
- Release Draft Routing.
- Create a new Draft Revision from Released Routing.
- Soft-delete Draft Routing.
- Display deleted Draft Routings.
- Restore deleted Draft Routings.
- Load Item / Operation / Machine dropdowns.
- Map Web ViewModels and Domain entities.

Important:
- Business logic belongs in ItemProcessRoutingService.
- Database access does not occur directly in Controller.
- Only Draft Routing is editable.
- Released Routing is used as the future Production template.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.ItemProcessRouting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class ItemProcessRoutingController : Controller
    {
        #region Fields

        private readonly IItemProcessRoutingService
            _itemProcessRoutingService;

        #endregion


        #region Constructor

        public ItemProcessRoutingController(
            IItemProcessRoutingService itemProcessRoutingService)
        {
            _itemProcessRoutingService =
                itemProcessRoutingService;
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
                await _itemProcessRoutingService
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
            var routing =
                await _itemProcessRoutingService
                    .GetByIdAsync(id);


            if (routing == null)
            {
                return NotFound();
            }


            var viewModel =
                MapToDetailsViewModel(
                    routing);


            return View(
                viewModel);
        }

        #endregion


        #region Create

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel =
                new ItemProcessRoutingFormViewModel
                {
                    Status =
                        ItemProcessRoutingStatus.Draft,

                    RevisionNumber =
                        1,

                    Steps =
                    {
                        new ItemProcessRoutingStepViewModel
                        {
                            SequenceNumber = 10
                        }
                    }
                };


            await LoadDropdownDataAsync(
                viewModel);


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ItemProcessRoutingFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var routing =
                    MapToDomain(
                        viewModel);


                var created =
                    await _itemProcessRoutingService
                        .CreateAsync(
                            routing);


                TempData["SuccessMessage"] =
                    "Item Process Routing created successfully.";


                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id = created.Id
                    });
            }
            catch (BusinessException ex)
            {
                await LoadDropdownDataAsync(
                    viewModel);


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
            var routing =
                await _itemProcessRoutingService
                    .GetByIdAsync(id);


            if (routing == null)
            {
                return NotFound();
            }


            if (routing.Status !=
                ItemProcessRoutingStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only Draft Routing can be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            var viewModel =
                MapToFormViewModel(
                    routing);


            await LoadDropdownDataAsync(
                viewModel);


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ItemProcessRoutingFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var routing =
                    MapToDomain(
                        viewModel);


                await _itemProcessRoutingService
                    .UpdateAsync(
                        routing);


                TempData["SuccessMessage"] =
                    "Item Process Routing updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch (BusinessException ex)
            {
                await LoadDropdownDataAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Release

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(
            int id)
        {
            try
            {
                await _itemProcessRoutingService
                    .ReleaseAsync(id);


                TempData["SuccessMessage"] =
                    "Routing released successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        #endregion


        #region Create Revision

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRevision(
            int id)
        {
            try
            {
                var newRevision =
                    await _itemProcessRoutingService
                        .CreateRevisionAsync(id);


                TempData["SuccessMessage"] =
                    $"Revision {newRevision.RevisionNumber} created successfully.";


                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        id = newRevision.Id
                    });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
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
                await _itemProcessRoutingService
                    .DeleteAsync(id);


                TempData["SuccessMessage"] =
                    "Draft Routing deleted successfully.";
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


        #region Deleted Routings

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var routings =
                await _itemProcessRoutingService
                    .GetDeletedAsync();


            return View(
                routings);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _itemProcessRoutingService
                    .RestoreAsync(id);


                TempData["SuccessMessage"] =
                    "Draft Routing restored successfully.";
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


        #region Dropdown Data

        private async Task LoadDropdownDataAsync(
            ItemProcessRoutingFormViewModel viewModel)
        {
            var items =
                await _itemProcessRoutingService
                    .GetItemsForRoutingAsync();


            var operations =
                await _itemProcessRoutingService
                    .GetOperationsForRoutingAsync();


            var machines =
                await _itemProcessRoutingService
                    .GetMachinesForRoutingAsync();


            viewModel.Items =
                items
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.ItemId.ToString(),

                            Text =
                                $"{x.ItemCode} - {x.ItemName}"
                        })
                    .ToList();


            viewModel.Operations =
                operations
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                $"{x.Code} - {x.OperationName}"
                        })
                    .ToList();


            viewModel.Machines =
                machines
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                $"{x.Code} - {x.MachineName}"
                        })
                    .ToList();
        }

        #endregion


        #region Domain Mapping

        private static ItemProcessRouting
            MapToDomain(
                ItemProcessRoutingFormViewModel viewModel)
        {
            var routing =
                new ItemProcessRouting
                {
                    Id =
                        viewModel.Id,

                    Code =
                        viewModel.Code
                        ?? string.Empty,

                    ItemId =
                        viewModel.ItemId,

                    RevisionNumber =
                        viewModel.RevisionNumber,

                    Status =
                        viewModel.Status,

                    EffectiveFrom =
                        viewModel.EffectiveFrom,

                    Remarks =
                        viewModel.Remarks
                };


            foreach (var stepViewModel
                in viewModel.Steps)
            {
                routing.Steps.Add(
                    new ItemProcessRoutingStep
                    {
                        Id =
                            stepViewModel.Id,

                        SequenceNumber =
                            stepViewModel.SequenceNumber,

                        ProductionOperationId =
                            stepViewModel
                                .ProductionOperationId,

                        DefaultMachineId =
                            stepViewModel
                                .DefaultMachineId,

                        SetupTimeMinutes =
                            stepViewModel
                                .SetupTimeMinutes,

                        CycleTimeMinutes =
                            stepViewModel
                                .CycleTimeMinutes,

                        OperationInstruction =
                            stepViewModel
                                .OperationInstruction,

                        Remarks =
                            stepViewModel.Remarks
                    });
            }


            return routing;
        }

        #endregion


        #region Form ViewModel Mapping

        private static ItemProcessRoutingFormViewModel
            MapToFormViewModel(
                ItemProcessRouting routing)
        {
            var viewModel =
                new ItemProcessRoutingFormViewModel
                {
                    Id =
                        routing.Id,

                    Code =
                        routing.Code,

                    ItemId =
                        routing.ItemId,

                    ItemCode =
                        routing.Item?.ItemCode,

                    ItemName =
                        routing.Item?.ItemName,

                    RevisionNumber =
                        routing.RevisionNumber,

                    Status =
                        routing.Status,

                    EffectiveFrom =
                        routing.EffectiveFrom,

                    Remarks =
                        routing.Remarks
                };


            foreach (var step in routing.Steps
                .Where(x =>
                    !x.IsDeleted)
                .OrderBy(x =>
                    x.SequenceNumber))
            {
                viewModel.Steps.Add(
                    new ItemProcessRoutingStepViewModel
                    {
                        Id =
                            step.Id,

                        SequenceNumber =
                            step.SequenceNumber,

                        ProductionOperationId =
                            step.ProductionOperationId,

                        DefaultMachineId =
                            step.DefaultMachineId,

                        SetupTimeMinutes =
                            step.SetupTimeMinutes,

                        CycleTimeMinutes =
                            step.CycleTimeMinutes,

                        OperationInstruction =
                            step.OperationInstruction,

                        Remarks =
                            step.Remarks
                    });
            }


            return viewModel;
        }

        #endregion


        #region Details ViewModel Mapping

        private static ItemProcessRoutingDetailsViewModel
            MapToDetailsViewModel(
                ItemProcessRouting routing)
        {
            var viewModel =
                new ItemProcessRoutingDetailsViewModel
                {
                    Id =
                        routing.Id,

                    Code =
                        routing.Code,

                    ItemId =
                        routing.ItemId,

                    ItemCode =
                        routing.Item?.ItemCode
                        ?? string.Empty,

                    ItemName =
                        routing.Item?.ItemName
                        ?? string.Empty,

                    RevisionNumber =
                        routing.RevisionNumber,

                    Status =
                        routing.Status,

                    EffectiveFrom =
                        routing.EffectiveFrom,

                    Remarks =
                        routing.Remarks
                };


            foreach (var step in routing.Steps
                .Where(x =>
                    !x.IsDeleted)
                .OrderBy(x =>
                    x.SequenceNumber))
            {
                viewModel.Steps.Add(
                    new ItemProcessRoutingStepDetailsViewModel
                    {
                        Id =
                            step.Id,

                        SequenceNumber =
                            step.SequenceNumber,

                        ProductionOperationId =
                            step.ProductionOperationId,

                        OperationCode =
                            step.ProductionOperation?.Code
                            ?? string.Empty,

                        OperationName =
                            step.ProductionOperation?.OperationName
                            ?? string.Empty,

                        OperationType =
                            step.ProductionOperation?.OperationType
                            ?? ProductionOperationType.Production,

                        DefaultMachineId =
                            step.DefaultMachineId,

                        DefaultMachineCode =
                            step.DefaultMachine?.Code,

                        DefaultMachineName =
                            step.DefaultMachine?.MachineName,

                        SetupTimeMinutes =
                            step.SetupTimeMinutes,

                        CycleTimeMinutes =
                            step.CycleTimeMinutes,

                        OperationInstruction =
                            step.OperationInstruction,

                        Remarks =
                            step.Remarks
                    });
            }


            return viewModel;
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