/*
============================================================
File: ProductionJobController.cs

Purpose:
Handles Production Job HTTP requests.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step
        ↓
Production Job Step History

Responsibilities:
- Display Production Job Index.
- Display PO-level Production Job Details.
- Create one Production Job from one Customer PO.
- Auto-load all Customer PO Items.
- Accept Admin Production Quantity Item-wise.
- Edit Production Quantity Item-wise.
- Display Item-wise Production Pipeline.
- Edit Item Pipeline before Production starts.
- Execute Production Job Steps.
- Preserve existing Machine / Start / Complete flow.
- Display Item-wise Workshop and Customer Drawings.
- Soft delete and restore Production Jobs.

Important:
- One Customer PO has one Production Job.
- Ordered Quantity comes from Customer PO.
- Production Quantity is planned by Admin.
- Worker does not change Production Quantity.
- Each ProductionJobItem has its own Pipeline.
- Different Items under the same PJOB can execute
  independently.
- Parent Job completes only after all Items complete
  their full Ordered Quantity.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.ProductionJob;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class ProductionJobController : Controller
    {
        #region Fields

        private readonly IProductionJobService
            _productionJobService;


        private readonly ICustomerDrawingService
            _customerDrawingService;

        #endregion


        #region Constructor

        public ProductionJobController(
            IProductionJobService productionJobService,
            ICustomerDrawingService customerDrawingService)
        {
            _productionJobService =
                productionJobService;


            _customerDrawingService =
                customerDrawingService;
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
                    .GetByIdAsync(
                        id);


            if (productionJob == null)
            {
                return NotFound();
            }


            var viewModel =
                await MapToDetailsViewModelAsync(
                    productionJob);


            return View(
                viewModel);
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
                await ReloadCreateFormAsync(
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
                        CustomerPurchaseOrderId =
                            viewModel.CustomerPurchaseOrderId,

                        PlannedStartOn =
                            viewModel.PlannedStartOn,

                        PlannedCompletionOn =
                            viewModel.PlannedCompletionOn,

                        Remarks =
                            viewModel.Remarks
                    };


                foreach (var item
                    in viewModel.Items)
                {
                    productionJob.Items.Add(
                        new ProductionJobItem
                        {
                            Id =
                                item.Id,

                            CustomerPurchaseOrderItemId =
                                item.CustomerPurchaseOrderItemId,

                            ProductionQuantity =
                                item.ProductionQuantity
                        });
                }


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
                        id =
                            created.Id
                    });
            }
            catch (BusinessException ex)
            {
                await ReloadCreateFormAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Customer PO AJAX Source

        [HttpGet]
        public async Task<IActionResult>
            GetCustomerPurchaseOrderSource(
                int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }


            var customerPurchaseOrder =
                await _productionJobService
                    .GetCustomerPurchaseOrderForProductionAsync(
                        id);


            if (customerPurchaseOrder == null)
            {
                return NotFound();
            }


            var items =
                new List<object>();


            foreach (var poItem
                in customerPurchaseOrder
                    .Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.Id))
            {
                var routing =
                    await _productionJobService
                        .GetReleasedRoutingForItemAsync(
                            poItem.ItemId);


                items.Add(
                    new
                    {
                        customerPurchaseOrderItemId =
                            poItem.Id,

                        itemId =
                            poItem.ItemId,

                        itemCode =
                            poItem.ItemCode,

                        itemName =
                            poItem.ItemName,

                        unitName =
                            poItem.UnitName,

                        orderedQuantity =
                            poItem.OrderedQuantity,

                        productionQuantity =
                            0m,

                        completedQuantity =
                            0m,

                        requiredDeliveryDate =
                            poItem.RequiredDeliveryDate
                            ??
                            customerPurchaseOrder
                                .RequiredDeliveryDate,

                        hasReleasedRouting =
                            routing != null,

                        itemProcessRoutingId =
                            routing?.Id
                            ??
                            0,

                        routingCode =
                            routing?.Code,

                        routingRevisionNumber =
                            routing?.RevisionNumber
                    });
            }


            return Json(
                new
                {
                    id =
                        customerPurchaseOrder.Id,

                    code =
                        customerPurchaseOrder.Code,

                    customerPurchaseOrderNumber =
                        customerPurchaseOrder
                            .CustomerPurchaseOrderNumber,

                    customerName =
                        customerPurchaseOrder
                            .CustomerName,

                    customerPurchaseOrderDate =
                        customerPurchaseOrder
                            .CustomerPurchaseOrderDate,

                    receivedDate =
                        customerPurchaseOrder
                            .ReceivedDate,

                    requiredDeliveryDate =
                        customerPurchaseOrder
                            .RequiredDeliveryDate,

                    items
                });
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
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


            if (
                productionJob.Status ==
                    ProductionJobStatus.Completed
                ||
                productionJob.Status ==
                    ProductionJobStatus.Cancelled
            )
            {
                TempData["ErrorMessage"] =
                    "Completed or Cancelled Production Job cannot be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            var viewModel =
                MapToFormViewModel(
                    productionJob);


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
                await ReloadEditFormAsync(
                    id,
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
                        Id =
                            viewModel.Id,

                        CustomerPurchaseOrderId =
                            viewModel
                                .CustomerPurchaseOrderId,

                        PlannedStartOn =
                            viewModel.PlannedStartOn,

                        PlannedCompletionOn =
                            viewModel
                                .PlannedCompletionOn,

                        Remarks =
                            viewModel.Remarks
                    };


                foreach (var item
                    in viewModel.Items)
                {
                    productionJob.Items.Add(
                        new ProductionJobItem
                        {
                            Id =
                                item.Id,

                            CustomerPurchaseOrderItemId =
                                item
                                    .CustomerPurchaseOrderItemId,

                            ProductionQuantity =
                                item.ProductionQuantity
                        });
                }


                await _productionJobService
                    .UpdateAsync(
                        productionJob);


                TempData["SuccessMessage"] =
                    "Production planning updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch (BusinessException ex)
            {
                await ReloadEditFormAsync(
                    id,
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
                    .MarkReadyAsync(
                        id);


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


        #region Production Pipeline

        [HttpGet]
        public async Task<IActionResult> Pipeline(
            int id,
            int? itemId = null)
        {
            var productionJob =
                await _productionJobService
                    .GetByIdAsync(
                        id);


            if (productionJob == null)
            {
                return NotFound();
            }


            var activeItems =
                productionJob
                    .Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.Id)
                    .ToList();


            if (!activeItems.Any())
            {
                return NotFound();
            }


            ProductionJobItem? selectedItem;


            if (itemId.HasValue)
            {
                selectedItem =
                    activeItems
                        .FirstOrDefault(x =>
                            x.Id ==
                            itemId.Value);


                if (selectedItem == null)
                {
                    return NotFound();
                }
            }
            else
            {
                /*
                 * Default:
                 *
                 * First Item having pending current
                 * Production Quantity.
                 *
                 * Otherwise first active Item.
                 */
                selectedItem =
                    activeItems
                        .FirstOrDefault(x =>
                            x.ProductionQuantity >
                            x.CompletedQuantity)
                    ??
                    activeItems.First();
            }


            var viewModel =
                await MapToDetailsViewModelAsync(
                    productionJob);


            ViewBag.SelectedProductionJobItemId =
                selectedItem.Id;


            var machines =
                await _productionJobService
                    .GetMachinesForExecutionAsync();


            ViewBag.ExecutionMachines =
                machines;


            return View(
                viewModel);
        }

        #endregion


        #region Edit Item Pipeline

        [HttpGet]
        public async Task<IActionResult> EditPipeline(
            int id,
            int itemId)
        {
            #region Load Production Job

            var productionJob =
                await _productionJobService
                    .GetByIdAsync(
                        id);


            if (productionJob == null)
            {
                return NotFound();
            }

            #endregion


            #region Find Production Job Item

            var productionJobItem =
                productionJob
                    .Items
                    .FirstOrDefault(x =>
                        x.Id == itemId
                        &&
                        !x.IsDeleted
                        &&
                        x.IsActive);


            if (productionJobItem == null)
            {
                return NotFound();
            }

            #endregion


            #region Check Production Started

            var productionStarted =
                productionJobItem
                    .Steps
                    .Where(step =>
                        !step.IsDeleted
                        &&
                        step.IsActive)
                    .Any(step =>
                        step.StartedOn.HasValue
                        ||
                        step.History.Any(history =>
                            history.NewStatus ==
                                ProductionJobStepStatus.InProgress
                            ||
                            history.NewStatus ==
                                ProductionJobStepStatus.Completed));

            #endregion


            #region Validate Pipeline Editing

            /*
             * Parent Job may already be InProgress because
             * another Item has started Production.
             *
             * This Item Pipeline remains editable until
             * Production starts for THIS Item.
             */
            var canEditPipeline =
                (
                    productionJob.Status ==
                        ProductionJobStatus.Draft
                    ||
                    productionJob.Status ==
                        ProductionJobStatus.Ready
                    ||
                    productionJob.Status ==
                        ProductionJobStatus.InProgress
                )
                &&
                !productionStarted;


            if (!canEditPipeline)
            {
                TempData["ErrorMessage"] =
                    "Production Pipeline can be edited only before Production starts for this Item.";


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            productionJob.Id,

                        itemId =
                            productionJobItem.Id
                    });
            }

            #endregion


            #region Load Operations

            var operations =
                await _productionJobService
                    .GetProductionOperationsForPipelineAsync();

            #endregion


            #region Build ViewModel

            var viewModel =
                new ProductionJobPipelineEditViewModel
                {
                    ProductionJobId =
                        productionJob.Id,

                    ProductionJobItemId =
                        productionJobItem.Id,

                    JobCode =
                        productionJob.Code,

                    Status =
                        productionJob.Status,

                    CustomerPurchaseOrderNumber =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber
                        ??
                        string.Empty,

                    CustomerName =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerName
                        ??
                        string.Empty,

                    ItemCode =
                        productionJobItem.ItemCode,

                    ItemName =
                        productionJobItem.ItemName,

                    PipelineModificationReason =
                        productionJobItem
                            .PipelineModificationReason,

                    AvailableOperations =
                        operations
                            .Select(x =>
                                new SelectListItem
                                {
                                    Value =
                                        x.Id.ToString(),

                                    Text =
                                        $"{x.Code} - {x.OperationName}"
                                })
                            .ToList()
                };


            foreach (var step
                in productionJobItem
                    .Steps
                    .Where(x =>
                        !x.IsDeleted
                        &&
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

            #endregion


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPipeline(
            ProductionJobPipelineEditViewModel viewModel)
        {
            if (
                viewModel.ProductionJobId <= 0
                ||
                viewModel.ProductionJobItemId <= 0
            )
            {
                return BadRequest();
            }


            try
            {
                #region Model Validation

                if (!ModelState.IsValid)
                {
                    throw new BusinessException(
                        GetModelStateErrorMessage());
                }

                #endregion


                #region Map Submitted Steps

                var steps =
                    viewModel
                        .Steps
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


                #region Save Pipeline

                await _productionJobService
                    .UpdateDraftPipelineAsync(
                        viewModel.ProductionJobId,
                        viewModel.ProductionJobItemId,
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
                            viewModel.ProductionJobId,

                        itemId =
                            viewModel.ProductionJobItemId
                    });
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                #region Reload Trusted Data

                var productionJob =
                    await _productionJobService
                        .GetByIdAsync(
                            viewModel.ProductionJobId);


                if (productionJob == null)
                {
                    return NotFound();
                }


                var productionJobItem =
                    productionJob
                        .Items
                        .FirstOrDefault(x =>
                            x.Id ==
                                viewModel.ProductionJobItemId
                            &&
                            !x.IsDeleted
                            &&
                            x.IsActive);


                if (productionJobItem == null)
                {
                    return NotFound();
                }

                #endregion


                #region Restore Display Information

                viewModel.JobCode =
                    productionJob.Code;


                viewModel.Status =
                    productionJob.Status;


                viewModel.CustomerPurchaseOrderNumber =
                    productionJob
                        .CustomerPurchaseOrder
                        ?.CustomerPurchaseOrderNumber
                    ??
                    string.Empty;


                viewModel.CustomerName =
                    productionJob
                        .CustomerPurchaseOrder
                        ?.CustomerName
                    ??
                    string.Empty;


                viewModel.ItemCode =
                    productionJobItem.ItemCode;


                viewModel.ItemName =
                    productionJobItem.ItemName;

                #endregion


                #region Reload Operation Lookup

                var operations =
                    await _productionJobService
                        .GetProductionOperationsForPipelineAsync();


                viewModel.AvailableOperations =
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

                #endregion


                return View(
                    viewModel);
            }
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


            var productionJobItem =
                FindProductionJobItemByStep(
                    productionJob,
                    stepId);


            if (productionJobItem == null)
            {
                return NotFound();
            }


            var step =
                productionJobItem
                    .Steps
                    .FirstOrDefault(x =>
                        x.Id == stepId
                        &&
                        !x.IsDeleted
                        &&
                        x.IsActive);


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
                        productionJobItem.ItemCode,

                    ItemName =
                        productionJobItem.ItemName,

                    /*
                     * Compatibility property.
                     *
                     * JobQuantity now represents the
                     * Admin-planned ProductionQuantity
                     * of this Item.
                     */
                    JobQuantity =
                        productionJobItem
                            .ProductionQuantity,

                    UnitName =
                        productionJobItem.UnitName,

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
                        step.DefaultMachine
                            ?.MachineName,

                    AssignedMachineId =
                        step.DefaultMachineId,

                    Machines =
                        machines
                            .Select(x =>
                                new SelectListItem
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
            var productionJobItemId =
                await ResolveProductionJobItemIdByStepAsync(
                    viewModel.ProductionJobId,
                    viewModel.ProductionJobStepId);


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId,

                        itemId =
                            productionJobItemId
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
                        viewModel.ProductionJobId,

                    itemId =
                        productionJobItemId
                });
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


            var productionJobItem =
                FindProductionJobItemByStep(
                    productionJob,
                    stepId);


            if (productionJobItem == null)
            {
                return NotFound();
            }


            var activeSteps =
                productionJobItem
                    .Steps
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            var step =
                activeSteps
                    .FirstOrDefault(x =>
                        x.Id ==
                        stepId);


            if (step == null)
            {
                return NotFound();
            }


            /*
             * Step quantities are cumulative.
             *
             * Popup defaults to quantity still
             * required at this Step.
             */
            var downstreamRejected =
                activeSteps
                    .Where(x =>
                        x.SequenceNumber >
                        step.SequenceNumber)
                    .Sum(x =>
                        x.RejectedQuantity
                        ??
                        0m);


            var requiredGoodQuantity =
                productionJobItem
                    .ProductionQuantity
                +
                downstreamRejected;


            var currentGoodQuantity =
                step.GoodQuantity
                ??
                0m;


            var pendingGoodQuantity =
                requiredGoodQuantity
                -
                currentGoodQuantity;


            if (pendingGoodQuantity < 0m)
            {
                pendingGoodQuantity =
                    0m;
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
                        productionJobItem.ItemCode,

                    ItemName =
                        productionJobItem.ItemName,

                    /*
                     * Compatibility property.
                     */
                    JobQuantity =
                        productionJobItem
                            .ProductionQuantity,

                    UnitName =
                        productionJobItem.UnitName,

                    SequenceNumber =
                        step.SequenceNumber,

                    OperationCode =
                        step.OperationCode,

                    OperationName =
                        step.OperationName,

                    AssignedMachineCode =
                        step.AssignedMachine?.Code,

                    AssignedMachineName =
                        step.AssignedMachine
                            ?.MachineName,

                    GoodQuantity =
                        pendingGoodQuantity,

                    RejectedQuantity =
                        0m
                };


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteStep(
            ProductionJobCompleteStepViewModel viewModel)
        {
            var productionJobItemId =
                await ResolveProductionJobItemIdByStepAsync(
                    viewModel.ProductionJobId,
                    viewModel.ProductionJobStepId);


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return RedirectToAction(
                    nameof(Pipeline),
                    new
                    {
                        id =
                            viewModel.ProductionJobId,

                        itemId =
                            productionJobItemId
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
                        viewModel.ProductionJobId,

                    itemId =
                        productionJobItemId
                });
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


        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _productionJobService
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Production Job deleted successfully.";
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
                    .RestoreAsync(
                        id);


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


        #region Production Sources

        private async Task LoadProductionSourcesAsync(
            ProductionJobFormViewModel viewModel)
        {
            var customerPurchaseOrders =
                await _productionJobService
                    .GetCustomerPurchaseOrdersForProductionAsync();


            viewModel
                .CustomerPurchaseOrders
                .Clear();


            foreach (var purchaseOrder
                in customerPurchaseOrders)
            {
                viewModel
                    .CustomerPurchaseOrders
                    .Add(
                        new SelectListItem
                        {
                            Value =
                                purchaseOrder
                                    .Id
                                    .ToString(),

                            Text =
                                $"{purchaseOrder.CustomerPurchaseOrderNumber} | {purchaseOrder.CustomerName}",

                            Selected =
                                purchaseOrder.Id ==
                                viewModel
                                    .CustomerPurchaseOrderId
                        });
            }
        }


        private async Task ReloadCreateFormAsync(
            ProductionJobFormViewModel viewModel)
        {
            var submittedQuantityLookup =
                viewModel
                    .Items
                    .Where(x =>
                        x.CustomerPurchaseOrderItemId > 0)
                    .GroupBy(x =>
                        x.CustomerPurchaseOrderItemId)
                    .ToDictionary(
                        x =>
                            x.Key,
                        x =>
                            x.Last()
                                .ProductionQuantity);


            await LoadProductionSourcesAsync(
                viewModel);


            if (
                viewModel.CustomerPurchaseOrderId <= 0
            )
            {
                return;
            }


            var purchaseOrder =
                await _productionJobService
                    .GetCustomerPurchaseOrderForProductionAsync(
                        viewModel.CustomerPurchaseOrderId);


            if (purchaseOrder == null)
            {
                return;
            }


            viewModel.CustomerPurchaseOrderCode =
                purchaseOrder.Code;

            viewModel.CustomerPurchaseOrderNumber =
                purchaseOrder
                    .CustomerPurchaseOrderNumber;

            viewModel.CustomerName =
                purchaseOrder.CustomerName;

            viewModel.ReceivedDate =
                purchaseOrder.ReceivedDate;

            viewModel.RequiredDeliveryDate =
                purchaseOrder.RequiredDeliveryDate;


            viewModel.Items.Clear();


            foreach (var poItem
                in purchaseOrder
                    .Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.Id))
            {
                var routing =
                    await _productionJobService
                        .GetReleasedRoutingForItemAsync(
                            poItem.ItemId);


                var productionQuantity =
                    submittedQuantityLookup
                        .TryGetValue(
                            poItem.Id,
                            out var submittedQuantity)
                        ? submittedQuantity
                        : 0m;


                viewModel.Items.Add(
                    new ProductionJobFormItemViewModel
                    {
                        CustomerPurchaseOrderItemId =
                            poItem.Id,

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

                        ProductionQuantity =
                            productionQuantity,

                        CompletedQuantity =
                            0m,

                        ItemProcessRoutingId =
                            routing?.Id
                            ??
                            0,

                        RoutingCode =
                            routing?.Code,

                        RoutingRevisionNumber =
                            routing?.RevisionNumber,

                        HasReleasedRouting =
                            routing != null,

                        RequiredDeliveryDate =
                            poItem.RequiredDeliveryDate
                            ??
                            purchaseOrder
                                .RequiredDeliveryDate
                    });
            }
        }

        #endregion


        #region Edit Form Mapping

        private static ProductionJobFormViewModel
            MapToFormViewModel(
                ProductionJob productionJob)
        {
            var viewModel =
                new ProductionJobFormViewModel
                {
                    Id =
                        productionJob.Id,

                    Code =
                        productionJob.Code,

                    Status =
                        productionJob.Status,

                    CustomerPurchaseOrderId =
                        productionJob
                            .CustomerPurchaseOrderId,

                    CustomerPurchaseOrderCode =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.Code,

                    CustomerPurchaseOrderNumber =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber,

                    CustomerName =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerName,

                    ReceivedDate =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.ReceivedDate,

                    RequiredDeliveryDate =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.RequiredDeliveryDate,

                    PlannedStartOn =
                        productionJob.PlannedStartOn,

                    PlannedCompletionOn =
                        productionJob
                            .PlannedCompletionOn,

                    Remarks =
                        productionJob.Remarks
                };


            foreach (var item
                in productionJob
                    .Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.Id))
            {
                viewModel.Items.Add(
                    new ProductionJobFormItemViewModel
                    {
                        Id =
                            item.Id,

                        CustomerPurchaseOrderItemId =
                            item
                                .CustomerPurchaseOrderItemId,

                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        UnitName =
                            item.UnitName,

                        OrderedQuantity =
                            item.OrderedQuantity,

                        ProductionQuantity =
                            item.ProductionQuantity,

                        CompletedQuantity =
                            item.CompletedQuantity,

                        ItemProcessRoutingId =
                            item.ItemProcessRoutingId,

                        RoutingCode =
                            item.RoutingCode,

                        RoutingRevisionNumber =
                            item.RoutingRevisionNumber,

                        HasReleasedRouting =
                            true,

                        RequiredDeliveryDate =
                            item
                                .CustomerPurchaseOrderItem
                                ?.RequiredDeliveryDate
                            ??
                            productionJob
                                .CustomerPurchaseOrder
                                ?.RequiredDeliveryDate
                    });
            }


            return viewModel;
        }


        private async Task ReloadEditFormAsync(
            int id,
            ProductionJobFormViewModel viewModel)
        {
            /*
             * Preserve Admin submitted ProductionQuantity
             * values after validation / business error.
             */
            var submittedQuantityLookup =
                viewModel
                    .Items
                    .Where(x =>
                        x.Id > 0)
                    .GroupBy(x =>
                        x.Id)
                    .ToDictionary(
                        x =>
                            x.Key,
                        x =>
                            x.Last()
                                .ProductionQuantity);


            var existing =
                await _productionJobService
                    .GetByIdAsync(
                        id);


            if (existing == null)
            {
                return;
            }


            var trustedViewModel =
                MapToFormViewModel(
                    existing);


            foreach (var item
                in trustedViewModel.Items)
            {
                if (
                    submittedQuantityLookup
                        .TryGetValue(
                            item.Id,
                            out var submittedQuantity)
                )
                {
                    item.ProductionQuantity =
                        submittedQuantity;
                }
            }


            /*
             * Restore immutable / trusted display data.
             *
             * Planned dates and Remarks intentionally remain
             * from submitted ViewModel.
             */
            viewModel.Code =
                trustedViewModel.Code;

            viewModel.Status =
                trustedViewModel.Status;

            viewModel.CustomerPurchaseOrderId =
                trustedViewModel
                    .CustomerPurchaseOrderId;

            viewModel.CustomerPurchaseOrderCode =
                trustedViewModel
                    .CustomerPurchaseOrderCode;

            viewModel.CustomerPurchaseOrderNumber =
                trustedViewModel
                    .CustomerPurchaseOrderNumber;

            viewModel.CustomerName =
                trustedViewModel.CustomerName;

            viewModel.ReceivedDate =
                trustedViewModel.ReceivedDate;

            viewModel.RequiredDeliveryDate =
                trustedViewModel
                    .RequiredDeliveryDate;

            viewModel.Items =
                trustedViewModel.Items;
        }

        #endregion


        #region Details Mapping

        private async Task<ProductionJobDetailsViewModel>
            MapToDetailsViewModelAsync(
                ProductionJob productionJob)
        {
            var viewModel =
                new ProductionJobDetailsViewModel
                {
                    Id =
                        productionJob.Id,

                    Code =
                        productionJob.Code,

                    Status =
                        productionJob.Status,

                    CustomerPurchaseOrderId =
                        productionJob
                            .CustomerPurchaseOrderId,

                    CustomerPurchaseOrderCode =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.Code
                        ??
                        string.Empty,

                    CustomerPurchaseOrderNumber =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderNumber
                        ??
                        string.Empty,

                    CustomerName =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerName
                        ??
                        string.Empty,

                    CustomerPurchaseOrderDate =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.CustomerPurchaseOrderDate,

                    ReceivedDate =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.ReceivedDate,

                    RequiredDeliveryDate =
                        productionJob
                            .CustomerPurchaseOrder
                            ?.RequiredDeliveryDate,

                    PlannedStartOn =
                        productionJob.PlannedStartOn,

                    PlannedCompletionOn =
                        productionJob
                            .PlannedCompletionOn,

                    StartedOn =
                        productionJob.StartedOn,

                    CompletedOn =
                        productionJob.CompletedOn,

                    CancelledOn =
                        productionJob.CancelledOn,

                    Remarks =
                        productionJob.Remarks,

                    CancellationReason =
                        productionJob
                            .CancellationReason
                };


            var customerId =
                productionJob
                    .CustomerPurchaseOrder
                    ?.CustomerId
                ??
                0;


            foreach (var item
                in productionJob
                    .Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.Id))
            {
                #region Workshop Drawing

                var currentDrawing =
                    item.Item
                        ?.Drawings
                        .Where(x =>
                            !x.IsDeleted
                            &&
                            x.IsActive)
                        .OrderByDescending(x =>
                            x.DrawingId)
                        .FirstOrDefault();

                #endregion


                #region Customer Drawing

                CustomerDrawing?
                    currentCustomerDrawing =
                        null;


                if (
                    customerId > 0
                    &&
                    item.ItemId > 0
                )
                {
                    currentCustomerDrawing =
                        await _customerDrawingService
                            .GetByCustomerAndItemAsync(
                                customerId,
                                item.ItemId);
                }

                #endregion


                var itemViewModel =
                    new ProductionJobDetailsItemViewModel
                    {
                        Id =
                            item.Id,

                        CustomerPurchaseOrderItemId =
                            item
                                .CustomerPurchaseOrderItemId,

                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        UnitName =
                            item.UnitName,

                        OrderedQuantity =
                            item.OrderedQuantity,

                        ProductionQuantity =
                            item.ProductionQuantity,

                        CompletedQuantity =
                            item.CompletedQuantity,

                        ItemProcessRoutingId =
                            item.ItemProcessRoutingId,

                        RoutingCode =
                            item.RoutingCode,

                        RoutingRevisionNumber =
                            item.RoutingRevisionNumber,

                        PipelineModificationReason =
                            item
                                .PipelineModificationReason,

                        #region Workshop Drawing

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


                        #region Customer Drawing

                        CustomerDrawingId =
                            currentCustomerDrawing?
                                .CustomerDrawingId,

                        CustomerDrawingNumber =
                            currentCustomerDrawing?
                                .DrawingNumber,

                        CustomerDrawingName =
                            currentCustomerDrawing?
                                .DrawingName,

                        CustomerDrawingType =
                            currentCustomerDrawing?
                                .DrawingType,

                        CustomerDrawingRevisionNumber =
                            currentCustomerDrawing?
                                .RevisionNumber,

                        CustomerDrawingFileName =
                            currentCustomerDrawing?
                                .FileName,

                        CustomerDrawingFilePath =
                            currentCustomerDrawing?
                                .FilePath,

                        CustomerDrawingDescription =
                            currentCustomerDrawing?
                                .Description

                        #endregion
                    };


                foreach (var step
                    in item
                        .Steps
                        .Where(x =>
                            !x.IsDeleted
                            &&
                            x.IsActive)
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
                                step
                                    .ProductionOperationId,

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
                                step.DefaultMachine
                                    ?.MachineName,

                            AssignedMachineId =
                                step.AssignedMachineId,

                            AssignedMachineCode =
                                step.AssignedMachine?.Code,

                            AssignedMachineName =
                                step.AssignedMachine
                                    ?.MachineName,

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


                    foreach (var history
                        in step
                            .History
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


                    itemViewModel
                        .Steps
                        .Add(
                            stepViewModel);
                }


                viewModel
                    .Items
                    .Add(
                        itemViewModel);
            }


            return viewModel;
        }

        #endregion


        #region Production Item Helpers

        private static ProductionJobItem?
            FindProductionJobItemByStep(
                ProductionJob productionJob,
                int stepId)
        {
            return productionJob
                .Items
                .Where(x =>
                    !x.IsDeleted
                    &&
                    x.IsActive)
                .FirstOrDefault(item =>
                    item.Steps.Any(step =>
                        step.Id ==
                            stepId
                        &&
                        !step.IsDeleted
                        &&
                        step.IsActive));
        }


        private async Task<int?>
            ResolveProductionJobItemIdByStepAsync(
                int productionJobId,
                int productionJobStepId)
        {
            if (
                productionJobId <= 0
                ||
                productionJobStepId <= 0
            )
            {
                return null;
            }


            var productionJob =
                await _productionJobService
                    .GetByIdAsync(
                        productionJobId);


            if (productionJob == null)
            {
                return null;
            }


            return FindProductionJobItemByStep(
                productionJob,
                productionJobStepId)
                ?.Id;
        }

        #endregion


        #region Validation Helper

        private string GetModelStateErrorMessage()
        {
            var errors =
                ModelState
                    .Values
                    .SelectMany(x =>
                        x.Errors)
                    .Select(x =>
                        x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x))
                    .Distinct()
                    .ToList();


            return errors.Any()
                ? string.Join(
                    " • ",
                    errors)
                : "Please correct the validation errors.";
        }

        #endregion
    }
}