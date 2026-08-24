/*
============================================================
File: PreDispatchInspectionController.cs

Purpose:
Handles Pre-Dispatch / Final Inspection HTTP requests.

Responsibilities:
- Display PDI Report Index.
- Create PDI Report from Completed Production Job.
- Auto-load Production Job source information.
- Edit Draft PDI Report.
- Display complete PDI Report Details.
- Finalize Draft PDI Report.
- Soft-delete Draft PDI Report.
- Display deleted PDI Reports.
- Restore deleted Draft PDI Report.
- Map Domain entities to Web ViewModels.

Important:
- Controller does not access DbContext or Repository.
- Business logic belongs in PreDispatchInspectionService.
- Production Job is the primary PDI source.
- Customer / PO / Item / Drawing source data is trusted
  from the Application Service.
- Finalized PDI Reports are read-only.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.PreDispatchInspection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class PreDispatchInspectionController
        : Controller
    {
        #region Fields

        private readonly
            IPreDispatchInspectionService
            _preDispatchInspectionService;

        #endregion


        #region Constructor

        public PreDispatchInspectionController(
            IPreDispatchInspectionService
                preDispatchInspectionService)
        {
            _preDispatchInspectionService =
                preDispatchInspectionService;
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
                await _preDispatchInspectionService
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
            var preDispatchInspection =
                await _preDispatchInspectionService
                    .GetByIdAsync(
                        id);


            if (preDispatchInspection == null)
            {
                return NotFound();
            }


            var viewModel =
                MapToDetailsViewModel(
                    preDispatchInspection);


            return View(
                viewModel);
        }

        #endregion


        #region Create

        [HttpGet]
        public async Task<IActionResult> Create(
            int? productionJobId = null)
        {
            #region Empty Form

            if (
                !productionJobId.HasValue ||
                productionJobId.Value <= 0
            )
            {
                var emptyViewModel =
                    new PreDispatchInspectionFormViewModel
                    {
                        InspectionDate =
                            DateTime.Today,

                        Status =
                            PreDispatchInspectionStatus.Draft,

                        Result =
                            PreDispatchInspectionResult.Pending
                    };


                await LoadProductionJobsAsync(
                    emptyViewModel);


                return View(
                    emptyViewModel);
            }

            #endregion


            #region Prepare Selected Production Job

            try
            {
                var prepared =
                    await _preDispatchInspectionService
                        .PrepareDraftAsync(
                            productionJobId.Value);


                if (prepared == null)
                {
                    TempData["ErrorMessage"] =
                        "Selected Production Job is not available for Inspection.";


                    return RedirectToAction(
                        nameof(Create));
                }


                var productionJob =
                    await _preDispatchInspectionService
                        .GetProductionJobForInspectionAsync(
                            productionJobId.Value);


                var remainingQuantity =
                    await _preDispatchInspectionService
                        .GetRemainingInspectionQuantityAsync(
                            productionJobId.Value);


                var viewModel =
                    MapToFormViewModel(
                        prepared);


                viewModel.JobQuantity =
                    productionJob?.JobQuantity
                    ?? 0;


                viewModel.RemainingInspectionQuantity =
                    remainingQuantity;


                await LoadProductionJobsAsync(
                    viewModel);


                return View(
                    viewModel);
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Create));
            }

            #endregion
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PreDispatchInspectionFormViewModel viewModel)
        {
            #region Model Validation

            if (!ModelState.IsValid)
            {
                await ReloadCreateFormAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }

            #endregion


            try
            {
                #region Map Form

                var preDispatchInspection =
                    MapToDomain(
                        viewModel);

                #endregion


                #region Create PDI

                var created =
                    await _preDispatchInspectionService
                        .CreateAsync(
                            preDispatchInspection);

                #endregion


                TempData["SuccessMessage"] =
                    $"PDI Report {created.Code} created successfully.";


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


        #region Production Job Auto Load

        /*
         * Used by Create UI when Production Job changes.
         *
         * Returns trusted source information:
         *
         * - Customer
         * - Customer PO
         * - Item
         * - Part Number
         * - UOM
         * - Job Quantity
         * - Remaining Inspection Quantity
         * - Workshop Drawing
         * - Customer Drawing
         * - Inspection Parameter Lines
         * - Default 7 Observations
         * - Default 3 Interval Readings
         */

        [HttpGet]
        public async Task<IActionResult>
            GetProductionJobData(
                int productionJobId)
        {
            if (productionJobId <= 0)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Invalid Production Job."
                    });
            }


            try
            {
                #region Prepare PDI Source

                var productionJob =
                    await _preDispatchInspectionService
                        .GetProductionJobForInspectionAsync(
                            productionJobId);


                if (productionJob == null)
                {
                    return NotFound(
                        new
                        {
                            message =
                                "Production Job is not available for Inspection."
                        });
                }


                var prepared =
                    await _preDispatchInspectionService
                        .PrepareDraftAsync(
                            productionJobId);


                if (prepared == null)
                {
                    return NotFound(
                        new
                        {
                            message =
                                "Unable to prepare PDI source information."
                        });
                }


                var remainingQuantity =
                    await _preDispatchInspectionService
                        .GetRemainingInspectionQuantityAsync(
                            productionJobId);

                #endregion


                #region Build JSON

                return Json(
                    new
                    {
                        productionJobId =
                            prepared.ProductionJobId,

                        productionJobCode =
                            prepared.ProductionJobCode,

                        customerName =
                            prepared.CustomerName,

                        customerPurchaseOrderCode =
                            prepared.CustomerPurchaseOrderCode,

                        customerPurchaseOrderNumber =
                            prepared.CustomerPurchaseOrderNumber,

                        customerItemCode =
                            prepared.CustomerItemCode,

                        itemId =
                            prepared.ItemId,

                        itemCode =
                            prepared.ItemCode,

                        itemName =
                            prepared.ItemName,

                        partNumber =
                            prepared.PartNumber,

                        unitName =
                            prepared.UnitName,

                        jobQuantity =
                            productionJob.JobQuantity,

                        remainingInspectionQuantity =
                            remainingQuantity,

                        inspectionQuantity =
                            prepared.InspectionQuantity,

                        workshopDrawingNumber =
                            prepared.WorkshopDrawingNumber,

                        workshopDrawingRevision =
                            prepared.WorkshopDrawingRevision,

                        customerDrawingNumber =
                            prepared.CustomerDrawingNumber,

                        customerDrawingRevision =
                            prepared.CustomerDrawingRevision,

                        lines =
                            prepared.Lines
                                .Where(x =>
                                    !x.IsDeleted &&
                                    x.IsActive)
                                .OrderBy(x =>
                                    x.SequenceNumber)
                                .Select(line =>
                                    new
                                    {
                                        id =
                                            line.Id,

                                        sequenceNumber =
                                            line.SequenceNumber,

                                        parameter =
                                            line.Parameter,

                                        specification =
                                            line.Specification,

                                        inspectionMethod =
                                            line.InspectionMethod,

                                        result =
                                            (int)line.Result,

                                        remarks =
                                            line.Remarks,

                                        observations =
                                            line.Observations
                                                .Where(x =>
                                                    !x.IsDeleted &&
                                                    x.IsActive)
                                                .OrderBy(x =>
                                                    x.IsIntervalReading)
                                                .ThenBy(x =>
                                                    x.SequenceNumber)
                                                .Select(observation =>
                                                    new
                                                    {
                                                        id =
                                                            observation.Id,

                                                        sequenceNumber =
                                                            observation.SequenceNumber,

                                                        isIntervalReading =
                                                            observation.IsIntervalReading,

                                                        value =
                                                            observation.Value
                                                    })
                                                .ToList()
                                    })
                                .ToList()
                    });

                #endregion
            }
            catch (BusinessException ex)
            {
                return BadRequest(
                    new
                    {
                        message =
                            ex.Message
                    });
            }
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            #region Load PDI

            var preDispatchInspection =
                await _preDispatchInspectionService
                    .GetByIdAsync(
                        id);


            if (preDispatchInspection == null)
            {
                return NotFound();
            }

            #endregion


            #region Validate Draft

            if (preDispatchInspection.Status !=
                PreDispatchInspectionStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only Draft PDI Report can be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            #endregion


            #region Remaining Quantity

            var remainingQuantity =
                await _preDispatchInspectionService
                    .GetRemainingInspectionQuantityAsync(
                        preDispatchInspection
                            .ProductionJobId,
                        preDispatchInspection.Id);

            #endregion


            #region Map Form

            var viewModel =
                MapToFormViewModel(
                    preDispatchInspection);


            viewModel.JobQuantity =
                preDispatchInspection
                    .ProductionJob?
                    .JobQuantity
                ?? 0;


            viewModel.RemainingInspectionQuantity =
                remainingQuantity;


            /*
             * Production Job is permanent after PDI Create.
             * This list is only populated for safe model
             * binding / display if the shared form needs it.
             */

            viewModel.ProductionJobs.Add(
                new SelectListItem
                {
                    Value =
                        preDispatchInspection
                            .ProductionJobId
                            .ToString(),

                    Text =
                        preDispatchInspection
                            .ProductionJobCode,

                    Selected =
                        true
                });

            #endregion


            return View(
                viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PreDispatchInspectionFormViewModel viewModel)
        {
            #region Basic Validation

            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            #endregion


            #region Model Validation

            if (!ModelState.IsValid)
            {
                await ReloadEditFormAsync(
                    viewModel);


                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }

            #endregion


            try
            {
                #region Map Form

                var preDispatchInspection =
                    MapToDomain(
                        viewModel);

                #endregion


                #region Update

                await _preDispatchInspectionService
                    .UpdateAsync(
                        preDispatchInspection);

                #endregion


                TempData["SuccessMessage"] =
                    "PDI Report updated successfully.";


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
                    viewModel);


                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Finalize

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(
            int id)
        {
            try
            {
                var finalized =
                    await _preDispatchInspectionService
                        .FinalizeAsync(
                            id);


                TempData["SuccessMessage"] =
                    $"PDI Report {finalized.Code} finalized successfully.";
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

        #region Download PDF

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(
            int id,
            string documentNumber = "AI / QA / 04")
        {
            try
            {
                #region Load Report

                var preDispatchInspection =
                    await _preDispatchInspectionService
                        .GetByIdAsync(
                            id);


                if (preDispatchInspection == null)
                {
                    return NotFound();
                }

                #endregion


                #region Generate PDF

                var pdfBytes =
                    await _preDispatchInspectionService
                        .GeneratePdfAsync(
                            id,
                            documentNumber);

                #endregion


                #region File Name

                var safeCode =
                    preDispatchInspection
                        .Code
                        .Replace(
                            "/",
                            "-")
                        .Replace(
                            "\\",
                            "-");


                var fileName =
                    $"{safeCode}-Final-Inspection-Report.pdf";

                #endregion


                #region Return PDF

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName);

                #endregion
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
                await _preDispatchInspectionService
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Draft PDI Report deleted successfully.";
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


        #region Deleted Reports

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var reports =
                await _preDispatchInspectionService
                    .GetDeletedAsync();


            return View(
                reports);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _preDispatchInspectionService
                    .RestoreAsync(
                        id);


                TempData["SuccessMessage"] =
                    "PDI Report restored successfully.";
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


        #region Form Reload Helpers

        private async Task ReloadCreateFormAsync(
            PreDispatchInspectionFormViewModel viewModel)
        {
            #region Production Job Dropdown

            await LoadProductionJobsAsync(
                viewModel);

            #endregion


            #region Source Information

            if (viewModel.ProductionJobId <= 0)
            {
                return;
            }


            try
            {
                var productionJob =
                    await _preDispatchInspectionService
                        .GetProductionJobForInspectionAsync(
                            viewModel.ProductionJobId);


                var prepared =
                    await _preDispatchInspectionService
                        .PrepareDraftAsync(
                            viewModel.ProductionJobId);


                if (
                    productionJob == null ||
                    prepared == null
                )
                {
                    return;
                }


                var remainingQuantity =
                    await _preDispatchInspectionService
                        .GetRemainingInspectionQuantityAsync(
                            viewModel.ProductionJobId);


                ApplyTrustedDisplayValues(
                    viewModel,
                    prepared);


                viewModel.JobQuantity =
                    productionJob.JobQuantity;


                viewModel.RemainingInspectionQuantity =
                    remainingQuantity;
            }
            catch (BusinessException)
            {
                /*
                 * Original BusinessException is handled
                 * by the calling action.
                 */
            }

            #endregion
        }


        private async Task ReloadEditFormAsync(
            PreDispatchInspectionFormViewModel viewModel)
        {
            #region Load Existing PDI

            if (viewModel.Id <= 0)
            {
                return;
            }


            var existing =
                await _preDispatchInspectionService
                    .GetByIdAsync(
                        viewModel.Id);


            if (existing == null)
            {
                return;
            }

            #endregion


            #region Trusted Display Values

            viewModel.Code =
                existing.Code;


            viewModel.Status =
                existing.Status;


            viewModel.Result =
                existing.Result;


            viewModel.ProductionJobId =
                existing.ProductionJobId;


            viewModel.ProductionJobCode =
                existing.ProductionJobCode;


            viewModel.CustomerName =
                existing.CustomerName;


            viewModel.CustomerPurchaseOrderCode =
                existing.CustomerPurchaseOrderCode;


            viewModel.CustomerPurchaseOrderNumber =
                existing.CustomerPurchaseOrderNumber;


            viewModel.CustomerItemCode =
                existing.CustomerItemCode;


            viewModel.ItemId =
                existing.ItemId;


            viewModel.ItemCode =
                existing.ItemCode;


            viewModel.ItemName =
                existing.ItemName;


            viewModel.PartNumber =
                existing.PartNumber;


            viewModel.UnitName =
                existing.UnitName;


            viewModel.WorkshopDrawingNumber =
                existing.WorkshopDrawingNumber;


            viewModel.WorkshopDrawingRevision =
                existing.WorkshopDrawingRevision;


            viewModel.CustomerDrawingNumber =
                existing.CustomerDrawingNumber;


            viewModel.CustomerDrawingRevision =
                existing.CustomerDrawingRevision;


            viewModel.JobQuantity =
                existing.ProductionJob?
                    .JobQuantity
                ?? 0;

            #endregion


            #region Remaining Quantity

            try
            {
                viewModel.RemainingInspectionQuantity =
                    await _preDispatchInspectionService
                        .GetRemainingInspectionQuantityAsync(
                            existing.ProductionJobId,
                            existing.Id);
            }
            catch (BusinessException)
            {
                viewModel.RemainingInspectionQuantity =
                    existing.InspectionQuantity;
            }

            #endregion


            #region Production Job Display Option

            viewModel.ProductionJobs.Clear();


            viewModel.ProductionJobs.Add(
                new SelectListItem
                {
                    Value =
                        existing
                            .ProductionJobId
                            .ToString(),

                    Text =
                        existing
                            .ProductionJobCode,

                    Selected =
                        true
                });

            #endregion
        }

        #endregion


        #region Production Job Dropdown

        private async Task LoadProductionJobsAsync(
            PreDispatchInspectionFormViewModel viewModel)
        {
            var productionJobs =
                await _preDispatchInspectionService
                    .GetProductionJobsForInspectionAsync();


            viewModel.ProductionJobs =
                productionJobs
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                BuildProductionJobOptionText(
                                    x),

                            Selected =
                                x.Id ==
                                viewModel
                                    .ProductionJobId
                        })
                    .ToList();
        }


        private static string
            BuildProductionJobOptionText(
                ProductionJob productionJob)
        {
            var customerName =
                productionJob
                    .CustomerPurchaseOrderItem
                    ?.CustomerPurchaseOrder
                    ?.CustomerName
                ?? string.Empty;


            return
                $"{productionJob.Code} | " +
                $"{customerName} | " +
                $"{productionJob.ItemCode} - " +
                $"{productionJob.ItemName}";
        }

        #endregion


        #region Map Domain To Form

        private static
            PreDispatchInspectionFormViewModel
            MapToFormViewModel(
                PreDispatchInspection
                    preDispatchInspection)
        {
            var viewModel =
                new PreDispatchInspectionFormViewModel
                {
                    Id =
                        preDispatchInspection.Id,

                    Code =
                        preDispatchInspection.Code,

                    Status =
                        preDispatchInspection.Status,

                    Result =
                        preDispatchInspection.Result,

                    InspectionDate =
                        preDispatchInspection
                            .InspectionDate,

                    ProductionJobId =
                        preDispatchInspection
                            .ProductionJobId,

                    ProductionJobCode =
                        preDispatchInspection
                            .ProductionJobCode,

                    CustomerName =
                        preDispatchInspection
                            .CustomerName,

                    CustomerPurchaseOrderCode =
                        preDispatchInspection
                            .CustomerPurchaseOrderCode,

                    CustomerPurchaseOrderNumber =
                        preDispatchInspection
                            .CustomerPurchaseOrderNumber,

                    CustomerItemCode =
                        preDispatchInspection
                            .CustomerItemCode,

                    ItemId =
                        preDispatchInspection.ItemId,

                    ItemCode =
                        preDispatchInspection.ItemCode,

                    ItemName =
                        preDispatchInspection.ItemName,

                    PartNumber =
                        preDispatchInspection.PartNumber,

                    UnitName =
                        preDispatchInspection.UnitName,

                    WorkshopDrawingNumber =
                        preDispatchInspection
                            .WorkshopDrawingNumber,

                    WorkshopDrawingRevision =
                        preDispatchInspection
                            .WorkshopDrawingRevision,

                    CustomerDrawingNumber =
                        preDispatchInspection
                            .CustomerDrawingNumber,

                    CustomerDrawingRevision =
                        preDispatchInspection
                            .CustomerDrawingRevision,

                    JobQuantity =
                        preDispatchInspection
                            .ProductionJob?
                            .JobQuantity
                        ?? 0,

                    InspectionQuantity =
                        preDispatchInspection
                            .InspectionQuantity,

                    AcceptedQuantity =
                        preDispatchInspection
                            .AcceptedQuantity,

                    ReworkQuantity =
                        preDispatchInspection
                            .ReworkQuantity,

                    RejectedQuantity =
                        preDispatchInspection
                            .RejectedQuantity,

                    InvoiceNumber =
                        preDispatchInspection
                            .InvoiceNumber,

                    InvoiceDate =
                        preDispatchInspection
                            .InvoiceDate,

                    InvoiceQuantity =
                        preDispatchInspection
                            .InvoiceQuantity,

                    SupplierRemarks =
                        preDispatchInspection
                            .SupplierRemarks,

                    InspectionRemarks =
                        preDispatchInspection
                            .InspectionRemarks,

                    InspectedBy =
                        preDispatchInspection
                            .InspectedBy,

                    ReviewedBy =
                        preDispatchInspection
                            .ReviewedBy
                };


            #region Lines

            foreach (var line
                in preDispatchInspection
                    .Lines
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                var lineViewModel =
                    new PreDispatchInspectionLineViewModel
                    {
                        Id =
                            line.Id,

                        SequenceNumber =
                            line.SequenceNumber,

                        Parameter =
                            line.Parameter,

                        Specification =
                            line.Specification,

                        InspectionMethod =
                            line.InspectionMethod,

                        Result =
                            line.Result,

                        Remarks =
                            line.Remarks
                    };


                foreach (var observation
                    in line.Observations
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)
                        .OrderBy(x =>
                            x.IsIntervalReading)
                        .ThenBy(x =>
                            x.SequenceNumber))
                {
                    lineViewModel
                        .Observations
                        .Add(
                            new PreDispatchInspectionObservationViewModel
                            {
                                Id =
                                    observation.Id,

                                SequenceNumber =
                                    observation
                                        .SequenceNumber,

                                IsIntervalReading =
                                    observation
                                        .IsIntervalReading,

                                Value =
                                    observation.Value
                            });
                }


                viewModel.Lines.Add(
                    lineViewModel);
            }

            #endregion


            return viewModel;
        }

        #endregion


        #region Map Form To Domain

        private static PreDispatchInspection
            MapToDomain(
                PreDispatchInspectionFormViewModel
                    viewModel)
        {
            var preDispatchInspection =
                new PreDispatchInspection
                {
                    Id =
                        viewModel.Id,

                    ProductionJobId =
                        viewModel.ProductionJobId,

                    InspectionDate =
                        viewModel.InspectionDate,

                    InspectionQuantity =
                        viewModel.InspectionQuantity,

                    AcceptedQuantity =
                        viewModel.AcceptedQuantity,

                    ReworkQuantity =
                        viewModel.ReworkQuantity,

                    RejectedQuantity =
                        viewModel.RejectedQuantity,

                    InvoiceNumber =
                        viewModel.InvoiceNumber,

                    InvoiceDate =
                        viewModel.InvoiceDate,

                    InvoiceQuantity =
                        viewModel.InvoiceQuantity,

                    SupplierRemarks =
                        viewModel.SupplierRemarks,

                    InspectionRemarks =
                        viewModel.InspectionRemarks,

                    InspectedBy =
                        viewModel.InspectedBy,

                    ReviewedBy =
                        viewModel.ReviewedBy
                };


            #region Lines

            var sequenceNumber =
                1;


            foreach (var lineViewModel
                in viewModel.Lines)
            {
                var line =
                    new PreDispatchInspectionLine
                    {
                        Id =
                            lineViewModel.Id,

                        SequenceNumber =
                            sequenceNumber,

                        Parameter =
                            lineViewModel.Parameter,

                        Specification =
                            lineViewModel.Specification,

                        InspectionMethod =
                            lineViewModel
                                .InspectionMethod,

                        Result =
                            lineViewModel.Result,

                        Remarks =
                            lineViewModel.Remarks
                    };


                #region Observations

                foreach (var observationViewModel
                    in lineViewModel
                        .Observations)
                {
                    line.Observations.Add(
                        new PreDispatchInspectionObservation
                        {
                            Id =
                                observationViewModel.Id,

                            SequenceNumber =
                                observationViewModel
                                    .SequenceNumber,

                            IsIntervalReading =
                                observationViewModel
                                    .IsIntervalReading,

                            Value =
                                observationViewModel.Value
                        });
                }

                #endregion


                preDispatchInspection
                    .Lines
                    .Add(
                        line);


                sequenceNumber++;
            }

            #endregion


            return preDispatchInspection;
        }

        #endregion


        #region Map Domain To Details

        private static
            PreDispatchInspectionDetailsViewModel
            MapToDetailsViewModel(
                PreDispatchInspection
                    preDispatchInspection)
        {
            var viewModel =
                new PreDispatchInspectionDetailsViewModel
                {
                    Id =
                        preDispatchInspection.Id,

                    Code =
                        preDispatchInspection.Code,

                    InspectionDate =
                        preDispatchInspection
                            .InspectionDate,

                    Status =
                        preDispatchInspection.Status,

                    Result =
                        preDispatchInspection.Result,

                    ProductionJobId =
                        preDispatchInspection
                            .ProductionJobId,

                    ProductionJobCode =
                        preDispatchInspection
                            .ProductionJobCode,

                    JobQuantity =
                        preDispatchInspection
                            .ProductionJob?
                            .JobQuantity
                        ?? preDispatchInspection
                            .InspectionQuantity,

                    CustomerId =
                        preDispatchInspection
                            .CustomerId,

                    CustomerName =
                        preDispatchInspection
                            .CustomerName,

                    CustomerPurchaseOrderItemId =
                        preDispatchInspection
                            .CustomerPurchaseOrderItemId,

                    CustomerPurchaseOrderCode =
                        preDispatchInspection
                            .CustomerPurchaseOrderCode,

                    CustomerPurchaseOrderNumber =
                        preDispatchInspection
                            .CustomerPurchaseOrderNumber,

                    CustomerItemCode =
                        preDispatchInspection
                            .CustomerItemCode,

                    ItemId =
                        preDispatchInspection.ItemId,

                    ItemCode =
                        preDispatchInspection.ItemCode,

                    ItemName =
                        preDispatchInspection.ItemName,

                    PartNumber =
                        preDispatchInspection.PartNumber,

                    UnitName =
                        preDispatchInspection.UnitName,

                    WorkshopDrawingId =
                        preDispatchInspection
                            .WorkshopDrawingId,

                    WorkshopDrawingNumber =
                        preDispatchInspection
                            .WorkshopDrawingNumber,

                    WorkshopDrawingRevision =
                        preDispatchInspection
                            .WorkshopDrawingRevision,

                    CustomerDrawingId =
                        preDispatchInspection
                            .CustomerDrawingId,

                    CustomerDrawingNumber =
                        preDispatchInspection
                            .CustomerDrawingNumber,

                    CustomerDrawingRevision =
                        preDispatchInspection
                            .CustomerDrawingRevision,

                    InvoiceNumber =
                        preDispatchInspection
                            .InvoiceNumber,

                    InvoiceDate =
                        preDispatchInspection
                            .InvoiceDate,

                    InvoiceQuantity =
                        preDispatchInspection
                            .InvoiceQuantity,

                    InspectionQuantity =
                        preDispatchInspection
                            .InspectionQuantity,

                    AcceptedQuantity =
                        preDispatchInspection
                            .AcceptedQuantity,

                    ReworkQuantity =
                        preDispatchInspection
                            .ReworkQuantity,

                    RejectedQuantity =
                        preDispatchInspection
                            .RejectedQuantity,

                    SupplierRemarks =
                        preDispatchInspection
                            .SupplierRemarks,

                    InspectionRemarks =
                        preDispatchInspection
                            .InspectionRemarks,

                    InspectedBy =
                        preDispatchInspection
                            .InspectedBy,

                    ReviewedBy =
                        preDispatchInspection
                            .ReviewedBy,

                    FinalizedOn =
                        preDispatchInspection
                            .FinalizedOn,

                    FinalizedBy =
                        preDispatchInspection
                            .FinalizedBy,

                    PdfFileName =
                        preDispatchInspection
                            .PdfFileName,

                    PdfFilePath =
                        preDispatchInspection
                            .PdfFilePath
                };


            #region Lines

            foreach (var line
                in preDispatchInspection
                    .Lines
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                var lineViewModel =
                    new PreDispatchInspectionLineDetailsViewModel
                    {
                        Id =
                            line.Id,

                        SequenceNumber =
                            line.SequenceNumber,

                        Parameter =
                            line.Parameter,

                        Specification =
                            line.Specification,

                        InspectionMethod =
                            line.InspectionMethod,

                        Result =
                            line.Result,

                        Remarks =
                            line.Remarks
                    };


                foreach (var observation
                    in line.Observations
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)
                        .OrderBy(x =>
                            x.IsIntervalReading)
                        .ThenBy(x =>
                            x.SequenceNumber))
                {
                    lineViewModel
                        .Observations
                        .Add(
                            new PreDispatchInspectionObservationDetailsViewModel
                            {
                                Id =
                                    observation.Id,

                                SequenceNumber =
                                    observation
                                        .SequenceNumber,

                                IsIntervalReading =
                                    observation
                                        .IsIntervalReading,

                                Value =
                                    observation.Value
                            });
                }


                viewModel.Lines.Add(
                    lineViewModel);
            }

            #endregion


            return viewModel;
        }

        #endregion


        #region Trusted Display Helper

        private static void
            ApplyTrustedDisplayValues(
                PreDispatchInspectionFormViewModel viewModel,
                PreDispatchInspection prepared)
        {
            viewModel.ProductionJobCode =
                prepared.ProductionJobCode;


            viewModel.CustomerName =
                prepared.CustomerName;


            viewModel.CustomerPurchaseOrderCode =
                prepared.CustomerPurchaseOrderCode;


            viewModel.CustomerPurchaseOrderNumber =
                prepared.CustomerPurchaseOrderNumber;


            viewModel.CustomerItemCode =
                prepared.CustomerItemCode;


            viewModel.ItemId =
                prepared.ItemId;


            viewModel.ItemCode =
                prepared.ItemCode;


            viewModel.ItemName =
                prepared.ItemName;


            viewModel.PartNumber =
                prepared.PartNumber;


            viewModel.UnitName =
                prepared.UnitName;


            viewModel.WorkshopDrawingNumber =
                prepared.WorkshopDrawingNumber;


            viewModel.WorkshopDrawingRevision =
                prepared.WorkshopDrawingRevision;


            viewModel.CustomerDrawingNumber =
                prepared.CustomerDrawingNumber;


            viewModel.CustomerDrawingRevision =
                prepared.CustomerDrawingRevision;
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
    }
}