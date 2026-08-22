/*
============================================================
File: ProductionJobController.cs

Purpose:
Handles Production Job HTTP requests.

Responsibilities:
- Display Production Job Index.
- Display Production Job Details.
- Create Production Job from Customer PO Item.
- Load Customer PO production source information.
- Mark Draft Production Job as Ready.
- Soft-delete Draft Production Job.
- Map Domain entities to Web ViewModels.

Important:
- Controller does not directly access DbContext or Repository.
- Business logic belongs in ProductionJobService.
- Routing selection is automatic based on Item.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.ProductionJob;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class ProductionJobController : Controller
    {
        #region Fields

        private readonly IProductionJobService
            _productionJobService;

        #endregion


        #region Constructor

        public ProductionJobController(
            IProductionJobService productionJobService)
        {
            _productionJobService =
                productionJobService;
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
                await _productionJobService
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
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(id);


            if (productionJob == null)
            {
                return NotFound();
            }


            return View(
                MapToDetailsViewModel(
                    productionJob));
        }

        #endregion

        #region Production Pipeline

        [HttpGet]
        public async Task<IActionResult> Pipeline(
            int id)
        {
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(id);


            if (productionJob == null)
            {
                return NotFound();
            }


            var viewModel =
                MapToDetailsViewModel(
                    productionJob);


            var machines =
                await _productionJobService
                    .GetMachinesForExecutionAsync();


            ViewBag.ExecutionMachines =
                machines;


            return View(
                viewModel);
        }

        #endregion

        #region Cancel Production Job

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            ProductionJobCancelViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId
                    });
            }


            try
            {
                await _productionJobService
                    .CancelAsync(
                        viewModel.ProductionJobId,
                        viewModel.Reason);


                TempData["SuccessMessage"] =
                    "Production Job cancelled successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }


            return RedirectToAction(
                nameof(Pipeline),
                new
                {
                    id =
                        viewModel.ProductionJobId
                });
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel =
                new ProductionJobFormViewModel();


            await LoadProductionSourcesAsync(
                viewModel);


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProductionJobFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadProductionSourcesAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var productionJob =
                    new ProductionJob
                    {
                        CustomerPurchaseOrderItemId =
                            viewModel.CustomerPurchaseOrderItemId,

                        JobQuantity =
                            viewModel.JobQuantity,

                        PlannedStartOn =
                            viewModel.PlannedStartOn,

                        PlannedCompletionOn =
                            viewModel.PlannedCompletionOn,

                        Remarks =
                            viewModel.Remarks
                    };


                var created =
                    await _productionJobService
                        .CreateAsync(
                            productionJob);


                TempData["SuccessMessage"] =
                    $"Production Job {created.Code} created successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = created.Id
                    });
            }
            catch (BusinessException ex)
            {
                await LoadProductionSourcesAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Mark Ready

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReady(
            int id)
        {
            try
            {
                await _productionJobService
                    .MarkReadyAsync(id);


                TempData["SuccessMessage"] =
                    "Production Job is now Ready for production.";
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


        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _productionJobService
                    .DeleteAsync(id);


                TempData["SuccessMessage"] =
                    "Draft Production Job deleted successfully.";
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


        #region Production Sources

        private async Task LoadProductionSourcesAsync(
            ProductionJobFormViewModel viewModel)
        {
            var poItems =
                await _productionJobService
                    .GetCustomerPurchaseOrderItemsForProductionAsync();


            viewModel.SourceItems.Clear();


            foreach (var poItem in poItems)
            {
                var remainingQuantity =
                    await _productionJobService
                        .GetRemainingQuantityAsync(
                            poItem.Id);


                var allocatedQuantity =
                    poItem.OrderedQuantity -
                    remainingQuantity;


                var routing =
                    await _productionJobService
                        .GetReleasedRoutingForItemAsync(
                            poItem.ItemId);

                if (remainingQuantity <= 0)
                {
                    continue;
                }

                /*
 * Production Job can only be created for an Item
 * having a Released Routing.
 *
 * Items without Released Routing are intentionally
 * not shown in the Production Source dropdown.
 */
                if (routing == null)
                {
                    continue;
                }

                viewModel.SourceItems.Add(
                    new ProductionJobSourceOptionViewModel
                    {
                        CustomerPurchaseOrderItemId =
                            poItem.Id,

                        CustomerPurchaseOrderCode =
                            poItem.CustomerPurchaseOrder?.Code
                            ?? string.Empty,

                        CustomerPurchaseOrderNumber =
                            poItem.CustomerPurchaseOrder
                                ?.CustomerPurchaseOrderNumber
                            ?? string.Empty,

                        CustomerName =
                            poItem.CustomerPurchaseOrder
                                ?.CustomerName
                            ?? string.Empty,

                        ItemId =
                            poItem.ItemId,

                        ItemCode =
                            poItem.ItemCode,

                        ItemName =
                            poItem.ItemName,

                        UnitName =
                            poItem.UnitName,

                        OrderedQuantity =
                            poItem.OrderedQuantity,

                        AllocatedQuantity =
                            allocatedQuantity,

                        RemainingQuantity =
                            remainingQuantity,

                        HasReleasedRouting =
    true,

                        RoutingCode =
    routing.Code,

                        RoutingRevisionNumber =
    routing.RevisionNumber
                    });
            }
        }

        #endregion


        #region Details Mapping

        private static ProductionJobDetailsViewModel
            MapToDetailsViewModel(
                ProductionJob productionJob)
        {


            #region Current Item Drawing

            var currentDrawing =
                productionJob.Item
                    ?.Drawings
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderByDescending(x =>
                        x.DrawingId)
                    .FirstOrDefault();

            #endregion


            var viewModel =
                new ProductionJobDetailsViewModel
                {
                    Id =
                        productionJob.Id,

                    Code =
                        productionJob.Code,

                    Status =
                        productionJob.Status,

                    CustomerPurchaseOrderItemId =
                        productionJob.CustomerPurchaseOrderItemId,

                    CustomerPurchaseOrderCode =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.Code
                        ?? string.Empty,

                    CustomerPurchaseOrderNumber =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber
                        ?? string.Empty,

                    CustomerName =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerName
                        ?? string.Empty,

                    ItemId =
                        productionJob.ItemId,

                    ItemCode =
                        productionJob.ItemCode,

                    ItemName =
                        productionJob.ItemName,

                    UnitName =
                        productionJob.UnitName,

                    JobQuantity =
                        productionJob.JobQuantity,

                    ItemProcessRoutingId =
                        productionJob.ItemProcessRoutingId,

                    RoutingCode =
                        productionJob.RoutingCode,

                    RoutingRevisionNumber =
                        productionJob.RoutingRevisionNumber,

                    PlannedStartOn =
                        productionJob.PlannedStartOn,

                    PlannedCompletionOn =
                        productionJob.PlannedCompletionOn,

                    StartedOn =
                        productionJob.StartedOn,

                    CompletedOn =
                        productionJob.CompletedOn,

                    CancelledOn =
                          productionJob.CancelledOn,

                    Remarks =
    productionJob.Remarks,

                    CancellationReason =
    productionJob.CancellationReason,

                    #region Current Item Drawing

                    DrawingId =
    currentDrawing?.DrawingId,

                    DrawingNumber =
    currentDrawing?.DrawingNumber,

                    DrawingName =
    currentDrawing?.DrawingName,

                    DrawingType =
    currentDrawing?.DrawingType,

                    DrawingRevisionNumber =
    currentDrawing?.RevisionNumber,

                    DrawingFileName =
    currentDrawing?.FileName,

                    DrawingFilePath =
    currentDrawing?.FilePath,

                    DrawingDescription =
    currentDrawing?.Description,

                    #endregion
                };


            foreach (var step in
                productionJob.Steps
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                var stepViewModel =
                    new ProductionJobStepDetailsViewModel
                    {
                        Id =
                            step.Id,

                        SequenceNumber =
                            step.SequenceNumber,

                        ProductionOperationId =
                            step.ProductionOperationId,

                        OperationCode =
                            step.OperationCode,

                        OperationName =
                            step.OperationName,

                        OperationType =
                            step.OperationType,

                        DefaultMachineId =
                            step.DefaultMachineId,

                        DefaultMachineCode =
                            step.DefaultMachine?.Code,

                        DefaultMachineName =
                            step.DefaultMachine?.MachineName,

                        AssignedMachineId =
                            step.AssignedMachineId,

                        AssignedMachineCode =
                            step.AssignedMachine?.Code,

                        AssignedMachineName =
                            step.AssignedMachine?.MachineName,

                        SetupTimeMinutes =
                            step.SetupTimeMinutes,

                        CycleTimeMinutes =
                            step.CycleTimeMinutes,

                        StartedOn =
                            step.StartedOn,

                        CompletedOn =
                            step.CompletedOn,

                        Status =
                            step.Status,

                        GoodQuantity =
                            step.GoodQuantity,

                        RejectedQuantity =
                            step.RejectedQuantity,

                        OperationInstruction =
                            step.OperationInstruction,

                        RoutingRemarks =
                            step.RoutingRemarks,

                        ExecutionRemarks =
                            step.ExecutionRemarks
                    };


                foreach (var history in
                    step.History
                        .OrderBy(x =>
                            x.ChangedOn))
                {
                    stepViewModel.History.Add(
                        new ProductionJobStepHistoryViewModel
                        {
                            PreviousStatus =
                                history.PreviousStatus,

                            NewStatus =
                                history.NewStatus,

                            MachineCode =
                                history.MachineCode,

                            MachineName =
                                history.MachineName,

                            GoodQuantity =
                                history.GoodQuantity,

                            RejectedQuantity =
                                history.RejectedQuantity,

                            Remarks =
                                history.Remarks,

                            ChangedOn =
                                history.ChangedOn,

                            ChangedBy =
                                history.ChangedBy
                        });
                }


                viewModel.Steps.Add(
                    stepViewModel);
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


            return errors.Any()
                ? string.Join(
                    " • ",
                    errors)
                : "Please correct the validation errors.";
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(id);


            if (productionJob == null)
            {
                return NotFound();
            }


            if (productionJob.Status !=
                ProductionJobStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only Draft Production Job can be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            var viewModel =
                new ProductionJobFormViewModel
                {
                    Id =
                        productionJob.Id,

                    Code =
                        productionJob.Code,

                    Status =
                        productionJob.Status,

                    CustomerPurchaseOrderItemId =
                        productionJob.CustomerPurchaseOrderItemId,

                    CustomerPurchaseOrderCode =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.Code,

                    CustomerPurchaseOrderNumber =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber,

                    CustomerName =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerName,

                    ItemCode =
                        productionJob.ItemCode,

                    ItemName =
                        productionJob.ItemName,

                    UnitName =
                        productionJob.UnitName,

                    RoutingCode =
                        productionJob.RoutingCode,

                    RoutingRevisionNumber =
                        productionJob.RoutingRevisionNumber,

                    JobQuantity =
                        productionJob.JobQuantity,

                    PlannedStartOn =
                        productionJob.PlannedStartOn,

                    PlannedCompletionOn =
                        productionJob.PlannedCompletionOn,

                    Remarks =
                        productionJob.Remarks
                };


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductionJobFormViewModel viewModel)
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
                var productionJob =
                    new ProductionJob
                    {
                        Id =
                            viewModel.Id,

                        JobQuantity =
                            viewModel.JobQuantity,

                        PlannedStartOn =
                            viewModel.PlannedStartOn,

                        PlannedCompletionOn =
                            viewModel.PlannedCompletionOn,

                        Remarks =
                            viewModel.Remarks
                    };


                await _productionJobService
                    .UpdateAsync(
                        productionJob);


                TempData["SuccessMessage"] =
                    "Production Job updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                /*
                 * Reload immutable display information because
                 * those values are not trusted from POST.
                 */

                var existing =
                    await _productionJobService
                        .GetByIdAsync(id);


                if (existing != null)
                {
                    viewModel.Code =
                        existing.Code;

                    viewModel.Status =
                        existing.Status;

                    viewModel.CustomerPurchaseOrderItemId =
                        existing.CustomerPurchaseOrderItemId;

                    viewModel.CustomerPurchaseOrderCode =
                        existing
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.Code;

                    viewModel.CustomerPurchaseOrderNumber =
                        existing
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber;

                    viewModel.CustomerName =
                        existing
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerName;

                    viewModel.ItemCode =
                        existing.ItemCode;

                    viewModel.ItemName =
                        existing.ItemName;

                    viewModel.UnitName =
                        existing.UnitName;

                    viewModel.RoutingCode =
                        existing.RoutingCode;

                    viewModel.RoutingRevisionNumber =
                        existing.RoutingRevisionNumber;
                }


                return View(
                    viewModel);
            }
        }

        #endregion

        #region Edit Draft Pipeline

        [HttpGet]
        public async Task<IActionResult> EditPipeline(
            int id)
        {
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(
                        id);


            if (productionJob == null)
            {
                return NotFound();
            }


            var canEditPipeline =
    (
        productionJob.Status ==
            ProductionJobStatus.Draft
        ||
        productionJob.Status ==
            ProductionJobStatus.Ready
    )
    &&
    !productionJob.StartedOn.HasValue;


            if (!canEditPipeline)
            {
                TempData["ErrorMessage"] =
                    "Production Pipeline can be edited only before Production starts.";


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id
                    });
            }


            var operations =
                await _productionJobService
                    .GetProductionOperationsForPipelineAsync();


            var viewModel =
                new ProductionJobPipelineEditViewModel
                {
                    ProductionJobId =
                        productionJob.Id,

                    JobCode =
                        productionJob.Code,

                    Status =
                        productionJob.Status,

                    CustomerPurchaseOrderNumber =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber
                        ?? string.Empty,

                    CustomerName =
                        productionJob
                            .CustomerPurchaseOrderItem
                            ?.CustomerPurchaseOrder
                            ?.CustomerName
                        ?? string.Empty,

                    ItemCode =
                        productionJob.ItemCode,

                    ItemName =
                        productionJob.ItemName,

                    PipelineModificationReason =
                        productionJob
                            .PipelineModificationReason,

                    AvailableOperations =
                        operations
                            .Select(x =>
                                new Microsoft.AspNetCore.Mvc.Rendering
                                    .SelectListItem
                                {
                                    Value =
                                        x.Id.ToString(),

                                    Text =
                                        $"{x.Code} - {x.OperationName}"
                                })
                            .ToList()
                };


            foreach (var step in
                productionJob.Steps
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                viewModel.Steps.Add(
                    new ProductionJobPipelineStepEditViewModel
                    {
                        Id =
                            step.Id,

                        SequenceNumber =
                            step.SequenceNumber,

                        ProductionOperationId =
                            step.ProductionOperationId,

                        OperationCode =
                            step.OperationCode,

                        OperationName =
                            step.OperationName
                    });
            }


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPipeline(
            ProductionJobPipelineEditViewModel viewModel)
        {
            #region Basic Validation

            if (viewModel.ProductionJobId <= 0)
            {
                return BadRequest();
            }

            #endregion


            try
            {
                #region Validate Model

                if (!ModelState.IsValid)
                {
                    throw new BusinessException(
                        GetModelStateErrorMessage());
                }

                #endregion


                #region Map Submitted Pipeline

                var steps =
                    viewModel.Steps
                        .Select(x =>
                            new ProductionJobStep
                            {
                                Id =
                                    x.Id,

                                ProductionOperationId =
                                    x.ProductionOperationId,

                                SequenceNumber =
                                    x.SequenceNumber
                            })
                        .ToList();

                #endregion


                #region Update Draft Pipeline

                await _productionJobService
                    .UpdateDraftPipelineAsync(
                        viewModel.ProductionJobId,
                        steps,
                        viewModel.PipelineModificationReason);

                #endregion


                TempData["SuccessMessage"] =
                    "Production Pipeline updated successfully.";


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId
                    });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                #region Reload Production Job

                var productionJob =
                    await _productionJobService
                        .GetByIdAsync(
                            viewModel.ProductionJobId);


                if (productionJob == null)
                {
                    return NotFound();
                }


                var canEditPipeline =
    (
        productionJob.Status ==
            ProductionJobStatus.Draft
        ||
        productionJob.Status ==
            ProductionJobStatus.Ready
    )
    &&
    !productionJob.StartedOn.HasValue;


                if (!canEditPipeline)
                {
                    return RedirectToAction(
                        nameof(Pipeline),
                        new
                        {
                            id =
                                productionJob.Id
                        });
                }

                #endregion


                #region Reload Header Information

                viewModel.JobCode =
                    productionJob.Code;

                viewModel.Status =
                    productionJob.Status;

                viewModel.CustomerPurchaseOrderNumber =
                    productionJob
                        .CustomerPurchaseOrderItem
                        ?.CustomerPurchaseOrder
                        ?.CustomerPurchaseOrderNumber
                    ?? string.Empty;

                viewModel.CustomerName =
                    productionJob
                        .CustomerPurchaseOrderItem
                        ?.CustomerPurchaseOrder
                        ?.CustomerName
                    ?? string.Empty;

                viewModel.ItemCode =
                    productionJob.ItemCode;

                viewModel.ItemName =
                    productionJob.ItemName;

                #endregion


                #region Reload Operations

                var operations =
                    await _productionJobService
                        .GetProductionOperationsForPipelineAsync();


                viewModel.AvailableOperations =
                    operations
                        .Select(x =>
                            new Microsoft.AspNetCore.Mvc.Rendering
                                .SelectListItem
                            {
                                Value =
                                    x.Id.ToString(),

                                Text =
                                    $"{x.Code} - {x.OperationName}"
                            })
                        .ToList();

                #endregion


                return View(
                    viewModel);
            }
        }

        #endregion

        #region Deleted Jobs

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var jobs =
                await _productionJobService
                    .GetDeletedAsync();


            return View(
                jobs);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _productionJobService
                    .RestoreAsync(id);


                TempData["SuccessMessage"] =
                    "Production Job restored successfully.";
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

        #region Start Production Step

        [HttpGet]
        public async Task<IActionResult> StartStep(
            int jobId,
            int stepId)
        {
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(
                        jobId);


            if (productionJob == null)
            {
                return NotFound();
            }


            var step =
                productionJob.Steps
                    .FirstOrDefault(x =>
                        x.Id == stepId &&
                        !x.IsDeleted);


            if (step == null)
            {
                return NotFound();
            }


            var machines =
                await _productionJobService
                    .GetMachinesForExecutionAsync();


            var viewModel =
                new ProductionJobStartStepViewModel
                {
                    ProductionJobId =
                        productionJob.Id,

                    ProductionJobStepId =
                        step.Id,

                    JobCode =
                        productionJob.Code,

                    ItemCode =
                        productionJob.ItemCode,

                    ItemName =
                        productionJob.ItemName,

                    JobQuantity =
                        productionJob.JobQuantity,

                    UnitName =
                        productionJob.UnitName,

                    SequenceNumber =
                        step.SequenceNumber,

                    OperationCode =
                        step.OperationCode,

                    OperationName =
                        step.OperationName,

                    DefaultMachineId =
                        step.DefaultMachineId,

                    DefaultMachineCode =
                        step.DefaultMachine?.Code,

                    DefaultMachineName =
                        step.DefaultMachine?.MachineName,

                    AssignedMachineId =
                        step.DefaultMachineId,

                    Machines =
                        machines
                            .Select(x =>
                                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                {
                                    Value =
                                        x.Id.ToString(),

                                    Text =
                                        $"{x.Code} - {x.MachineName}"
                                })
                            .ToList()
                };


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartStep(
            ProductionJobStartStepViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId
                    });
            }


            try
            {
                await _productionJobService
                    .StartStepAsync(
                        viewModel.ProductionJobId,
                        viewModel.ProductionJobStepId,
                        viewModel.AssignedMachineId,
                        viewModel.Remarks);


                TempData["SuccessMessage"] =
                    $"{viewModel.OperationName} started successfully.";


                return RedirectToAction(
                     nameof(Pipeline),
                      new
                             {
                               id =
                                 viewModel.ProductionJobId
              });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId
                    });
            }
        }

        #endregion


        #region Complete Production Step

        [HttpGet]
        public async Task<IActionResult> CompleteStep(
            int jobId,
            int stepId)
        {
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(
                        jobId);


            if (productionJob == null)
            {
                return NotFound();
            }


            var step =
                productionJob.Steps
                    .FirstOrDefault(x =>
                        x.Id == stepId &&
                        !x.IsDeleted);


            if (step == null)
            {
                return NotFound();
            }


            var viewModel =
                new ProductionJobCompleteStepViewModel
                {
                    ProductionJobId =
                        productionJob.Id,

                    ProductionJobStepId =
                        step.Id,

                    JobCode =
                        productionJob.Code,

                    ItemCode =
                        productionJob.ItemCode,

                    ItemName =
                        productionJob.ItemName,

                    JobQuantity =
                        productionJob.JobQuantity,

                    UnitName =
                        productionJob.UnitName,

                    SequenceNumber =
                        step.SequenceNumber,

                    OperationCode =
                        step.OperationCode,

                    OperationName =
                        step.OperationName,

                    AssignedMachineCode =
                        step.AssignedMachine?.Code,

                    AssignedMachineName =
                        step.AssignedMachine?.MachineName,

                    GoodQuantity =
                        productionJob.JobQuantity,

                    RejectedQuantity =
                        0
                };


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteStep(
            ProductionJobCompleteStepViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId
                    });
            }


            try
            {
                await _productionJobService
                    .CompleteStepAsync(
                        viewModel.ProductionJobId,
                        viewModel.ProductionJobStepId,
                        viewModel.GoodQuantity,
                        viewModel.RejectedQuantity,
                        viewModel.Remarks);


                TempData["SuccessMessage"] =
                    $"{viewModel.OperationName} completed successfully.";


                return RedirectToAction(
    nameof(Pipeline),
    new
    {
        id =
            viewModel.ProductionJobId
    });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId
                    });
            }
        }

        #endregion
    }
}