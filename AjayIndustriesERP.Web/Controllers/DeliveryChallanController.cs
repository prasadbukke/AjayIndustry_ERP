/*
============================================================
File: DeliveryChallanController.cs

Purpose:
Handles Web requests for Delivery Challan module.

Responsibilities:
- List and search Delivery Challans.
- Create Draft Delivery Challan.
- Load Finalized PDI information through AJAX.
- Auto-load Customer Master information.
- Auto-load editable Customer delivery address.
- Auto-load Company / Workshop information.
- Edit Draft Delivery Challan.
- Preserve historical Customer / Company snapshots.
- Display Delivery Challan Details.
- Finalize Delivery Challan.
- Download Finalized Challan PDF.
- Soft-delete and restore Draft Challans.
- Map Web ViewModels to Domain entities.

Important:
- Business logic remains in DeliveryChallanService.
- Controller does not directly access database.
- PDI snapshot values posted from browser are not trusted.
- Customer address is editable and posted back.
- Customer Master display fields are projected from
  CustomerSnapshotJson.
- Company / Workshop display fields are projected from
  CompanySnapshotJson.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.DeliveryChallan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace AjayIndustriesERP.Web.Controllers
{
    public class DeliveryChallanController
        : Controller
    {
        #region Fields

        private readonly IDeliveryChallanService
            _deliveryChallanService;

        #endregion


        #region Constructor

        public DeliveryChallanController(
            IDeliveryChallanService deliveryChallanService)
        {
            _deliveryChallanService =
                deliveryChallanService;
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
                await _deliveryChallanService
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


        #region Create GET

        [HttpGet]
        public async Task<IActionResult> Create(
            int? pdiId = null)
        {
            #region Prepare ViewModel

            DeliveryChallanFormViewModel
                viewModel;

            if (
                pdiId.HasValue &&
                pdiId.Value > 0
            )
            {
                try
                {
                    var draft =
                        await _deliveryChallanService
                            .PrepareDraftAsync(
                                pdiId.Value);

                    if (draft == null)
                    {
                        TempData["ErrorMessage"] =
                            "Selected Finalized PDI Report is not available for dispatch.";

                        return RedirectToAction(
                            nameof(Index));
                    }

                    viewModel =
                        await MapToFormViewModelAsync(
                            draft);
                }
                catch (BusinessException ex)
                {
                    TempData["ErrorMessage"] =
                        ex.Message;

                    return RedirectToAction(
                        nameof(Index));
                }
            }
            else
            {
                viewModel =
                    new DeliveryChallanFormViewModel
                    {
                        ChallanDate =
                            DateTime.Today,

                        Status =
                            DeliveryChallanStatus.Draft,

                        Items =
                            new List<DeliveryChallanItemFormViewModel>
                            {
                                new()
                                {
                                    SequenceNumber = 1
                                }
                            }
                    };
            }

            #endregion


            #region PDI Dropdown

            await PopulateAvailablePdisAsync(
                viewModel);

            #endregion

            return View(
                viewModel);
        }

        #endregion


        #region Create POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DeliveryChallanFormViewModel viewModel)
        {
            #region Model Validation

            if (!ModelState.IsValid)
            {
                await RefreshFormSnapshotsAsync(
                    viewModel);

                await PopulateAvailablePdisAsync(
                    viewModel);

                return View(
                    viewModel);
            }

            #endregion

            try
            {
                var deliveryChallan =
                    MapToDomain(
                        viewModel);

                var created =
                    await _deliveryChallanService
                        .CreateAsync(
                            deliveryChallan);

                TempData["SuccessMessage"] =
                    $"Delivery Challan {created.Code} created successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = created.Id
                    });
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                await RefreshFormSnapshotsAsync(
                    viewModel);

                await PopulateAvailablePdisAsync(
                    viewModel);

                return View(
                    viewModel);
            }
        }

        #endregion


        #region PDI AJAX Data

        [HttpGet]
        public async Task<IActionResult> GetPdiData(
            int id,
            int? deliveryChallanId = null)
        {
            try
            {
                #region Load PDI

                var pdi =
                    await _deliveryChallanService
                        .GetFinalizedPdiForDispatchAsync(
                            id);

                if (pdi == null)
                {
                    return Json(
                        new
                        {
                            success = false,

                            message =
                                "Selected Finalized PDI Report is not available."
                        });
                }

                #endregion


                #region Quantity Calculation

                var availableQuantity =
                    await _deliveryChallanService
                        .GetRemainingDispatchQuantityAsync(
                            id,
                            deliveryChallanId);

                var alreadyDispatchedQuantity =
                    pdi.AcceptedQuantity -
                    availableQuantity;

                if (alreadyDispatchedQuantity < 0)
                {
                    alreadyDispatchedQuantity =
                        0;
                }

                #endregion


                #region Master Snapshot Data

                /*
                 * CREATE:
                 * Load fresh Customer + Company Master defaults.
                 *
                 * EDIT:
                 * Current Master data must not overwrite the
                 * historical Challan snapshot.
                 */

                var shouldRefreshMasterData =
                    !deliveryChallanId.HasValue ||
                    deliveryChallanId.Value <= 0;

                DeliveryChallan?
                    masterSnapshotSource =
                        null;

                Dictionary<string, JsonElement>
                    customerSnapshot =
                        new();

                Dictionary<string, JsonElement>
                    companySnapshot =
                        new();

                if (shouldRefreshMasterData)
                {
                    masterSnapshotSource =
                        await _deliveryChallanService
                            .PrepareDraftAsync(
                                id);

                    if (masterSnapshotSource != null)
                    {
                        customerSnapshot =
                            ParseSnapshot(
                                masterSnapshotSource
                                    .CustomerSnapshotJson);

                        companySnapshot =
                            ParseSnapshot(
                                masterSnapshotSource
                                    .CompanySnapshotJson);
                    }
                }

                #endregion


                #region Response

                return Json(
                    new
                    {
                        success = true,


                        // =====================================
                        // Customer / PDI
                        // =====================================

                        customerId =
                            pdi.CustomerId,

                        customerName =
                            pdi.CustomerName,

                        preDispatchInspectionId =
                            pdi.Id,

                        preDispatchInspectionCode =
                            pdi.Code,


                        // =====================================
                        // Customer Master Information
                        // =====================================

                        customerCode =
                            GetSnapshotString(
                                customerSnapshot,
                                "Code"),

                        customerLegalName =
                            GetSnapshotString(
                                customerSnapshot,
                                "LegalName"),

                        customerGstin =
                            GetSnapshotString(
                                customerSnapshot,
                                "GSTIN"),

                        customerPan =
                            GetSnapshotString(
                                customerSnapshot,
                                "PAN"),

                        customerContactPerson =
                            GetSnapshotString(
                                customerSnapshot,
                                "ContactPerson"),

                        customerMobileNumber =
                            GetSnapshotString(
                                customerSnapshot,
                                "MobileNumber"),

                        customerAlternateMobileNumber =
                            GetSnapshotString(
                                customerSnapshot,
                                "AlternateMobileNumber"),

                        customerEmail =
                            GetSnapshotString(
                                customerSnapshot,
                                "Email"),

                        customerPaymentTerms =
                            GetSnapshotString(
                                customerSnapshot,
                                "PaymentTerms"),

                        customerCreditDays =
                            GetSnapshotNullableInt(
                                customerSnapshot,
                                "CreditDays"),

                        customerWebsite =
                            GetSnapshotString(
                                customerSnapshot,
                                "Website"),

                        customerMasterRemarks =
                            GetSnapshotString(
                                customerSnapshot,
                                "Remarks"),


                        // =====================================
                        // Editable Customer Address
                        // =====================================

                        customerAddressLine1 =
                            masterSnapshotSource
                                ?.CustomerAddressLine1,

                        customerAddressLine2 =
                            masterSnapshotSource
                                ?.CustomerAddressLine2,

                        customerCity =
                            masterSnapshotSource
                                ?.CustomerCity,

                        customerDistrict =
                            masterSnapshotSource
                                ?.CustomerDistrict,

                        customerState =
                            masterSnapshotSource
                                ?.CustomerState,

                        customerPincode =
                            masterSnapshotSource
                                ?.CustomerPincode,

                        customerCountry =
                            masterSnapshotSource
                                ?.CustomerCountry,


                        // =====================================
                        // Production Job
                        // =====================================

                        productionJobId =
                            pdi.ProductionJobId,

                        productionJobCode =
                            pdi.ProductionJobCode,


                        // =====================================
                        // Customer PO
                        // =====================================

                        customerPurchaseOrderItemId =
                            pdi.CustomerPurchaseOrderItemId,

                        customerPurchaseOrderCode =
                            pdi.CustomerPurchaseOrderCode,

                        customerPurchaseOrderNumber =
                            pdi.CustomerPurchaseOrderNumber,

                        customerItemCode =
                            pdi.CustomerItemCode,


                        // =====================================
                        // Item
                        // =====================================

                        itemId =
                            pdi.ItemId,

                        itemCode =
                            pdi.ItemCode,

                        itemName =
                            pdi.ItemName,

                        partNumber =
                            pdi.PartNumber,

                        unitName =
                            pdi.UnitName,


                        // =====================================
                        // Drawing - ERP Traceability
                        // =====================================

                        customerDrawingId =
                            pdi.CustomerDrawingId,

                        customerDrawingNumber =
                            pdi.CustomerDrawingNumber,

                        customerDrawingRevision =
                            pdi.CustomerDrawingRevision,


                        // =====================================
                        // Quantities
                        // =====================================

                        pdiAcceptedQuantity =
                            pdi.AcceptedQuantity,

                        alreadyDispatchedQuantity,

                        availableQuantity,


                        // =====================================
                        // Master Refresh Rule
                        // =====================================

                        shouldRefreshMasterData,


                        // =====================================
                        // Company / Workshop
                        // =====================================

                        companyId =
                            masterSnapshotSource
                                ?.CompanyId,

                        companyName =
                            masterSnapshotSource
                                ?.CompanyName,

                        companyCode =
                            GetSnapshotString(
                                companySnapshot,
                                "CompanyCode"),

                        companyGstNumber =
                            GetSnapshotString(
                                companySnapshot,
                                "GstNumber"),

                        companyPanNumber =
                            GetSnapshotString(
                                companySnapshot,
                                "PanNumber"),

                        companyPhoneNumber =
                            GetSnapshotString(
                                companySnapshot,
                                "PhoneNumber"),

                        companyEmail =
                            GetSnapshotString(
                                companySnapshot,
                                "Email"),

                        companyWebsite =
                            GetSnapshotString(
                                companySnapshot,
                                "Website"),

                        companyContactPerson =
                            GetSnapshotString(
                                companySnapshot,
                                "ContactPerson"),

                        companyAddress =
                            GetSnapshotString(
                                companySnapshot,
                                "Address"),

                        companyCity =
                            GetSnapshotString(
                                companySnapshot,
                                "City"),

                        companyState =
                            GetSnapshotString(
                                companySnapshot,
                                "State"),

                        companyCountry =
                            GetSnapshotString(
                                companySnapshot,
                                "Country"),

                        companyPostalCode =
                            GetSnapshotString(
                                companySnapshot,
                                "PostalCode"),

                        companyPurchaseOrderTermsAndConditions =
                            GetSnapshotString(
                                companySnapshot,
                                "PurchaseOrderTermsAndConditions")
                    });

                #endregion
            }
            catch (BusinessException ex)
            {
                return Json(
                    new
                    {
                        success = false,

                        message =
                            ex.Message
                    });
            }
        }

        #endregion


        #region Edit GET

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var deliveryChallan =
                await _deliveryChallanService
                    .GetByIdAsync(
                        id);

            if (deliveryChallan == null)
            {
                return NotFound();
            }

            if (
                deliveryChallan.Status !=
                DeliveryChallanStatus.Draft
            )
            {
                TempData["ErrorMessage"] =
                    "Finalized Delivery Challan cannot be edited.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            var viewModel =
                await MapToFormViewModelAsync(
                    deliveryChallan,
                    deliveryChallan.Id);

            await PopulateAvailablePdisAsync(
                viewModel,
                deliveryChallan.Id);

            return View(
                viewModel);
        }

        #endregion


        #region Edit POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            DeliveryChallanFormViewModel viewModel)
        {
            if (viewModel.Id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await RefreshFormSnapshotsAsync(
                    viewModel,
                    viewModel.Id);

                await PopulateAvailablePdisAsync(
                    viewModel,
                    viewModel.Id);

                return View(
                    viewModel);
            }

            try
            {
                var deliveryChallan =
                    MapToDomain(
                        viewModel);

                var updated =
                    await _deliveryChallanService
                        .UpdateAsync(
                            deliveryChallan);

                TempData["SuccessMessage"] =
                    $"Delivery Challan {updated.Code} updated successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = updated.Id
                    });
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                await RefreshFormSnapshotsAsync(
                    viewModel,
                    viewModel.Id);

                await PopulateAvailablePdisAsync(
                    viewModel,
                    viewModel.Id);

                return View(
                    viewModel);
            }
        }

        #endregion


        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var deliveryChallan =
                await _deliveryChallanService
                    .GetByIdAsync(
                        id);

            if (deliveryChallan == null)
            {
                return NotFound();
            }

            var viewModel =
                MapToDetailsViewModel(
                    deliveryChallan);

            return View(
                viewModel);
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
                var deliveryChallan =
                    await _deliveryChallanService
                        .FinalizeAsync(
                            id);

                TempData["SuccessMessage"] =
                    $"Delivery Challan {deliveryChallan.Code} finalized successfully.";
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
            int id)
        {
            try
            {
                var pdfBytes =
                    await _deliveryChallanService
                        .GeneratePdfAsync(
                            id);

                var deliveryChallan =
                    await _deliveryChallanService
                        .GetByIdAsync(
                            id);

                if (deliveryChallan == null)
                {
                    return NotFound();
                }

                var safeCode =
                    deliveryChallan.Code
                        .Replace(
                            "/",
                            "-")
                        .Replace(
                            "\\",
                            "-");

                var fileName =
                    $"Delivery-Challan-{safeCode}.pdf";

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName);
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
                await _deliveryChallanService
                    .DeleteAsync(
                        id);

                TempData["SuccessMessage"] =
                    "Delivery Challan deleted successfully.";
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


        #region Deleted

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var challans =
                await _deliveryChallanService
                    .GetDeletedAsync();

            return View(
                challans);
        }

        #endregion


        #region Restore

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _deliveryChallanService
                    .RestoreAsync(
                        id);

                TempData["SuccessMessage"] =
                    "Delivery Challan restored successfully.";
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


        #region Form Snapshot Refresh

        private async Task RefreshFormSnapshotsAsync(
            DeliveryChallanFormViewModel viewModel,
            int? excludeDeliveryChallanId = null)
        {
            if (
                viewModel.Items == null ||
                viewModel.Items.Count == 0
            )
            {
                return;
            }

            var sequenceNumber =
                1;

            foreach (var item
                in viewModel.Items)
            {
                item.SequenceNumber =
                    sequenceNumber++;

                if (item.PreDispatchInspectionId <= 0)
                {
                    continue;
                }

                var pdi =
                    await _deliveryChallanService
                        .GetFinalizedPdiForDispatchAsync(
                            item.PreDispatchInspectionId);

                if (pdi == null)
                {
                    continue;
                }

                var availableQuantity =
                    await _deliveryChallanService
                        .GetRemainingDispatchQuantityAsync(
                            pdi.Id,
                            excludeDeliveryChallanId);

                var alreadyDispatchedQuantity =
                    pdi.AcceptedQuantity -
                    availableQuantity;

                if (alreadyDispatchedQuantity < 0)
                {
                    alreadyDispatchedQuantity =
                        0;
                }

                ApplyPdiToFormItem(
                    item,
                    pdi,
                    alreadyDispatchedQuantity,
                    availableQuantity);

                viewModel.CustomerId =
                    pdi.CustomerId;

                viewModel.CustomerName =
                    pdi.CustomerName;
            }
        }

        #endregion


        #region Populate PDI Dropdown

        private async Task PopulateAvailablePdisAsync(
            DeliveryChallanFormViewModel viewModel,
            int? excludeDeliveryChallanId = null)
        {
            var pdiReports =
                await _deliveryChallanService
                    .GetFinalizedPdisForDispatchAsync();

            var selectedPdiIds =
                viewModel.Items
                    ?.Where(x =>
                        x.PreDispatchInspectionId > 0)
                    .Select(x =>
                        x.PreDispatchInspectionId)
                    .Distinct()
                    .ToList()
                ?? new List<int>();

            foreach (var selectedPdiId
                in selectedPdiIds)
            {
                if (
                    pdiReports.Any(x =>
                        x.Id == selectedPdiId)
                )
                {
                    continue;
                }

                var selectedPdi =
                    await _deliveryChallanService
                        .GetFinalizedPdiForDispatchAsync(
                            selectedPdiId);

                if (selectedPdi != null)
                {
                    pdiReports.Add(
                        selectedPdi);
                }
            }

            var selectList =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = "",
                        Text = "-- Select Finalized PDI --"
                    }
                };

            foreach (var pdi
                in pdiReports
                    .OrderByDescending(x =>
                        x.InspectionDate)
                    .ThenByDescending(x =>
                        x.Id))
            {
                var availableQuantity =
                    await _deliveryChallanService
                        .GetRemainingDispatchQuantityAsync(
                            pdi.Id,
                            excludeDeliveryChallanId);

                if (
                    availableQuantity <= 0 &&
                    !selectedPdiIds.Contains(
                        pdi.Id)
                )
                {
                    continue;
                }

                var unit =
                    string.IsNullOrWhiteSpace(
                        pdi.UnitName)
                        ? string.Empty
                        : $" {pdi.UnitName}";

                selectList.Add(
                    new SelectListItem
                    {
                        Value =
                            pdi.Id.ToString(),

                        Text =
                            $"{pdi.Code} | " +
                            $"{pdi.CustomerName} | " +
                            $"{pdi.ItemName} | " +
                            $"Available: {availableQuantity:0.###}{unit}"
                    });
            }

            viewModel.AvailablePdis =
                selectList;
        }

        #endregion


        #region Domain Mapping

        private static DeliveryChallan
            MapToDomain(
                DeliveryChallanFormViewModel viewModel)
        {
            var deliveryChallan =
                new DeliveryChallan
                {
                    Id =
                        viewModel.Id,

                    ChallanDate =
                        viewModel.ChallanDate,

                    LpgNumber =
                        viewModel.LpgNumber,

                    CustomerAddressLine1 =
                        viewModel.CustomerAddressLine1,

                    CustomerAddressLine2 =
                        viewModel.CustomerAddressLine2,

                    CustomerCity =
                        viewModel.CustomerCity,

                    CustomerDistrict =
                        viewModel.CustomerDistrict,

                    CustomerState =
                        viewModel.CustomerState,

                    CustomerPincode =
                        viewModel.CustomerPincode,

                    CustomerCountry =
                        viewModel.CustomerCountry,

                    TransporterName =
                        viewModel.TransporterName,

                    VehicleNumber =
                        viewModel.VehicleNumber,

                    TransportReference =
                        viewModel.TransportReference,

                    DispatchFrom =
                        viewModel.DispatchFrom,

                    Destination =
                        viewModel.Destination,

                    Remarks =
                        viewModel.Remarks
                };

            foreach (var item
                in viewModel.Items)
            {
                deliveryChallan.Items.Add(
                    new DeliveryChallanItem
                    {
                        Id =
                            item.Id,

                        SequenceNumber =
                            item.SequenceNumber,

                        PreDispatchInspectionId =
                            item.PreDispatchInspectionId,

                        ProductReference =
                            item.ProductReference,

                        HsnNumber =
                            item.HsnNumber,

                        DispatchQuantity =
                            item.DispatchQuantity,

                        IsActive =
                            true,

                        IsDeleted =
                            false
                    });
            }

            return deliveryChallan;
        }

        #endregion


        #region Form Mapping

        private async Task<DeliveryChallanFormViewModel>
            MapToFormViewModelAsync(
                DeliveryChallan deliveryChallan,
                int? excludeDeliveryChallanId = null)
        {
            #region Header

            var viewModel =
                new DeliveryChallanFormViewModel
                {
                    Id =
                        deliveryChallan.Id,

                    Code =
                        deliveryChallan.Code,

                    ChallanDate =
                        deliveryChallan.ChallanDate,

                    Status =
                        deliveryChallan.Status,

                    LpgNumber =
                        deliveryChallan.LpgNumber,

                    CustomerId =
                        deliveryChallan.CustomerId,

                    CustomerName =
                        deliveryChallan.CustomerName,

                    CustomerAddressLine1 =
                        deliveryChallan.CustomerAddressLine1,

                    CustomerAddressLine2 =
                        deliveryChallan.CustomerAddressLine2,

                    CustomerCity =
                        deliveryChallan.CustomerCity,

                    CustomerDistrict =
                        deliveryChallan.CustomerDistrict,

                    CustomerState =
                        deliveryChallan.CustomerState,

                    CustomerPincode =
                        deliveryChallan.CustomerPincode,

                    CustomerCountry =
                        deliveryChallan.CustomerCountry,

                    CompanyId =
                        deliveryChallan.CompanyId,

                    CompanyName =
                        deliveryChallan.CompanyName,

                    TransporterName =
                        deliveryChallan.TransporterName,

                    VehicleNumber =
                        deliveryChallan.VehicleNumber,

                    TransportReference =
                        deliveryChallan.TransportReference,

                    DispatchFrom =
                        deliveryChallan.DispatchFrom,

                    Destination =
                        deliveryChallan.Destination,

                    Remarks =
                        deliveryChallan.Remarks
                };

            #endregion


            #region Customer Snapshot Projection

            ApplyCustomerSnapshotToForm(
                viewModel,
                deliveryChallan.CustomerSnapshotJson);

            #endregion


            #region Company Snapshot Projection

            ApplyCompanySnapshotToForm(
                viewModel,
                deliveryChallan.CompanySnapshotJson);

            #endregion


            #region Items

            foreach (var item
                in deliveryChallan.Items
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                var availableQuantity =
                    await _deliveryChallanService
                        .GetRemainingDispatchQuantityAsync(
                            item.PreDispatchInspectionId,
                            excludeDeliveryChallanId);

                var alreadyDispatchedQuantity =
                    item.PdiAcceptedQuantity -
                    availableQuantity;

                if (alreadyDispatchedQuantity < 0)
                {
                    alreadyDispatchedQuantity =
                        0;
                }

                viewModel.Items.Add(
                    new DeliveryChallanItemFormViewModel
                    {
                        Id =
                            item.Id,

                        SequenceNumber =
                            item.SequenceNumber,

                        PreDispatchInspectionId =
                            item.PreDispatchInspectionId,

                        PreDispatchInspectionCode =
                            item.PreDispatchInspectionCode,

                        ProductionJobId =
                            item.ProductionJobId,

                        ProductionJobCode =
                            item.ProductionJobCode,

                        CustomerPurchaseOrderItemId =
                            item.CustomerPurchaseOrderItemId,

                        CustomerPurchaseOrderCode =
                            item.CustomerPurchaseOrderCode,

                        CustomerPurchaseOrderNumber =
                            item.CustomerPurchaseOrderNumber,

                        CustomerItemCode =
                            item.CustomerItemCode,

                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        PartNumber =
                            item.PartNumber,

                        UnitName =
                            item.UnitName,

                        ProductReference =
                            item.ProductReference,

                        HsnNumber =
                            item.HsnNumber,

                        CustomerDrawingId =
                            item.CustomerDrawingId,

                        CustomerDrawingNumber =
                            item.CustomerDrawingNumber,

                        CustomerDrawingRevision =
                            item.CustomerDrawingRevision,

                        PdiAcceptedQuantity =
                            item.PdiAcceptedQuantity,

                        AlreadyDispatchedQuantity =
                            alreadyDispatchedQuantity,

                        AvailableQuantity =
                            availableQuantity,

                        DispatchQuantity =
                            item.DispatchQuantity
                    });
            }

            #endregion

            return viewModel;
        }

        #endregion


        #region Customer Snapshot Projection

        private static void ApplyCustomerSnapshotToForm(
            DeliveryChallanFormViewModel viewModel,
            string? customerSnapshotJson)
        {
            var snapshot =
                ParseSnapshot(
                    customerSnapshotJson);

            if (snapshot.Count == 0)
            {
                return;
            }

            viewModel.CustomerCode =
                GetSnapshotString(
                    snapshot,
                    "Code");

            viewModel.CustomerLegalName =
                GetSnapshotString(
                    snapshot,
                    "LegalName");

            viewModel.CustomerGstin =
                GetSnapshotString(
                    snapshot,
                    "GSTIN");

            viewModel.CustomerPan =
                GetSnapshotString(
                    snapshot,
                    "PAN");

            viewModel.CustomerContactPerson =
                GetSnapshotString(
                    snapshot,
                    "ContactPerson");

            viewModel.CustomerMobileNumber =
                GetSnapshotString(
                    snapshot,
                    "MobileNumber");

            viewModel.CustomerAlternateMobileNumber =
                GetSnapshotString(
                    snapshot,
                    "AlternateMobileNumber");

            viewModel.CustomerEmail =
                GetSnapshotString(
                    snapshot,
                    "Email");

            viewModel.CustomerPaymentTerms =
                GetSnapshotString(
                    snapshot,
                    "PaymentTerms");

            viewModel.CustomerCreditDays =
                GetSnapshotNullableInt(
                    snapshot,
                    "CreditDays");

            viewModel.CustomerWebsite =
                GetSnapshotString(
                    snapshot,
                    "Website");

            viewModel.CustomerMasterRemarks =
                GetSnapshotString(
                    snapshot,
                    "Remarks");
        }

        #endregion


        #region Company Snapshot Projection

        private static void ApplyCompanySnapshotToForm(
            DeliveryChallanFormViewModel viewModel,
            string? companySnapshotJson)
        {
            var snapshot =
                ParseSnapshot(
                    companySnapshotJson);

            if (snapshot.Count == 0)
            {
                return;
            }

            viewModel.CompanyCode =
                GetSnapshotString(
                    snapshot,
                    "CompanyCode");

            if (string.IsNullOrWhiteSpace(
                viewModel.CompanyName))
            {
                viewModel.CompanyName =
                    GetSnapshotString(
                        snapshot,
                        "CompanyName");
            }

            viewModel.CompanyGstNumber =
                GetSnapshotString(
                    snapshot,
                    "GstNumber");

            viewModel.CompanyPanNumber =
                GetSnapshotString(
                    snapshot,
                    "PanNumber");

            viewModel.CompanyPhoneNumber =
                GetSnapshotString(
                    snapshot,
                    "PhoneNumber");

            viewModel.CompanyEmail =
                GetSnapshotString(
                    snapshot,
                    "Email");

            viewModel.CompanyWebsite =
                GetSnapshotString(
                    snapshot,
                    "Website");

            viewModel.CompanyContactPerson =
                GetSnapshotString(
                    snapshot,
                    "ContactPerson");

            viewModel.CompanyAddress =
                GetSnapshotString(
                    snapshot,
                    "Address");

            viewModel.CompanyCity =
                GetSnapshotString(
                    snapshot,
                    "City");

            viewModel.CompanyState =
                GetSnapshotString(
                    snapshot,
                    "State");

            viewModel.CompanyCountry =
                GetSnapshotString(
                    snapshot,
                    "Country");

            viewModel.CompanyPostalCode =
                GetSnapshotString(
                    snapshot,
                    "PostalCode");

            viewModel.CompanyPurchaseOrderTermsAndConditions =
                GetSnapshotString(
                    snapshot,
                    "PurchaseOrderTermsAndConditions");
        }

        #endregion


        #region JSON Snapshot Helpers

        private static Dictionary<string, JsonElement>
            ParseSnapshot(
                string? snapshotJson)
        {
            if (string.IsNullOrWhiteSpace(
                snapshotJson))
            {
                return new Dictionary<string, JsonElement>();
            }

            try
            {
                var snapshot =
                    JsonSerializer.Deserialize<
                        Dictionary<string, JsonElement>>(
                            snapshotJson);

                return snapshot
                    ?? new Dictionary<string, JsonElement>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, JsonElement>();
            }
        }


        private static string?
            GetSnapshotString(
                Dictionary<string, JsonElement> snapshot,
                string propertyName)
        {
            if (
                !snapshot.TryGetValue(
                    propertyName,
                    out var value)
            )
            {
                return null;
            }

            if (
                value.ValueKind ==
                JsonValueKind.Null
            )
            {
                return null;
            }

            if (
                value.ValueKind ==
                JsonValueKind.String
            )
            {
                return value.GetString();
            }

            return value.ToString();
        }


        private static int?
            GetSnapshotNullableInt(
                Dictionary<string, JsonElement> snapshot,
                string propertyName)
        {
            if (
                !snapshot.TryGetValue(
                    propertyName,
                    out var value)
            )
            {
                return null;
            }

            if (
                value.ValueKind ==
                JsonValueKind.Null
            )
            {
                return null;
            }

            if (
                value.ValueKind ==
                JsonValueKind.Number &&
                value.TryGetInt32(
                    out var number)
            )
            {
                return number;
            }

            if (
                value.ValueKind ==
                JsonValueKind.String &&
                int.TryParse(
                    value.GetString(),
                    out number)
            )
            {
                return number;
            }

            return null;
        }

        #endregion


        #region PDI Form Mapping

        private static void ApplyPdiToFormItem(
            DeliveryChallanItemFormViewModel item,
            PreDispatchInspection pdi,
            decimal alreadyDispatchedQuantity,
            decimal availableQuantity)
        {
            item.PreDispatchInspectionId =
                pdi.Id;

            item.PreDispatchInspectionCode =
                pdi.Code;

            item.ProductionJobId =
                pdi.ProductionJobId;

            item.ProductionJobCode =
                pdi.ProductionJobCode;

            item.CustomerPurchaseOrderItemId =
                pdi.CustomerPurchaseOrderItemId;

            item.CustomerPurchaseOrderCode =
                pdi.CustomerPurchaseOrderCode;

            item.CustomerPurchaseOrderNumber =
                pdi.CustomerPurchaseOrderNumber;

            item.CustomerItemCode =
                pdi.CustomerItemCode;

            item.ItemId =
                pdi.ItemId;

            item.ItemCode =
                pdi.ItemCode;

            item.ItemName =
                pdi.ItemName;

            item.PartNumber =
                pdi.PartNumber;

            item.UnitName =
                pdi.UnitName;

            item.CustomerDrawingId =
                pdi.CustomerDrawingId;

            item.CustomerDrawingNumber =
                pdi.CustomerDrawingNumber;

            item.CustomerDrawingRevision =
                pdi.CustomerDrawingRevision;

            item.PdiAcceptedQuantity =
                pdi.AcceptedQuantity;

            item.AlreadyDispatchedQuantity =
                alreadyDispatchedQuantity;

            item.AvailableQuantity =
                availableQuantity;
        }

        #endregion


        #region Details Mapping

        private static DeliveryChallanDetailsViewModel
            MapToDetailsViewModel(
                DeliveryChallan deliveryChallan)
        {
            var customerSnapshot =
                ParseSnapshot(
                    deliveryChallan.CustomerSnapshotJson);

            var companySnapshot =
                ParseSnapshot(
                    deliveryChallan.CompanySnapshotJson);

            var viewModel =
                new DeliveryChallanDetailsViewModel
                {
                    Id =
                        deliveryChallan.Id,

                    Code =
                        deliveryChallan.Code,

                    ChallanDate =
                        deliveryChallan.ChallanDate,

                    Status =
                        deliveryChallan.Status,

                    LpgNumber =
                        deliveryChallan.LpgNumber,

                    CustomerId =
                        deliveryChallan.CustomerId,

                    CustomerName =
                        deliveryChallan.CustomerName,

                    CustomerAddressLine1 =
                        deliveryChallan.CustomerAddressLine1,

                    CustomerAddressLine2 =
                        deliveryChallan.CustomerAddressLine2,

                    CustomerCity =
                        deliveryChallan.CustomerCity,

                    CustomerDistrict =
                        deliveryChallan.CustomerDistrict,

                    CustomerState =
                        deliveryChallan.CustomerState,

                    CustomerPincode =
                        deliveryChallan.CustomerPincode,

                    CustomerCountry =
                        deliveryChallan.CustomerCountry,

                    CustomerCode =
                        GetSnapshotString(
                            customerSnapshot,
                            "Code"),

                    CustomerLegalName =
                        GetSnapshotString(
                            customerSnapshot,
                            "LegalName"),

                    CustomerGstin =
                        GetSnapshotString(
                            customerSnapshot,
                            "GSTIN"),

                    CustomerPan =
                        GetSnapshotString(
                            customerSnapshot,
                            "PAN"),

                    CustomerContactPerson =
                        GetSnapshotString(
                            customerSnapshot,
                            "ContactPerson"),

                    CustomerMobileNumber =
                        GetSnapshotString(
                            customerSnapshot,
                            "MobileNumber"),

                    CustomerAlternateMobileNumber =
                        GetSnapshotString(
                            customerSnapshot,
                            "AlternateMobileNumber"),

                    CustomerEmail =
                        GetSnapshotString(
                            customerSnapshot,
                            "Email"),

                    CustomerWebsite =
                        GetSnapshotString(
                            customerSnapshot,
                            "Website"),

                    CompanyId =
                        deliveryChallan.CompanyId,

                    CompanyName =
                        !string.IsNullOrWhiteSpace(
                            deliveryChallan.CompanyName)
                            ? deliveryChallan.CompanyName
                            : GetSnapshotString(
                                companySnapshot,
                                "CompanyName"),

                    CompanyCode =
                        GetSnapshotString(
                            companySnapshot,
                            "CompanyCode"),

                    CompanyGstNumber =
                        GetSnapshotString(
                            companySnapshot,
                            "GstNumber"),

                    CompanyPanNumber =
                        GetSnapshotString(
                            companySnapshot,
                            "PanNumber"),

                    CompanyPhoneNumber =
                        GetSnapshotString(
                            companySnapshot,
                            "PhoneNumber"),

                    CompanyEmail =
                        GetSnapshotString(
                            companySnapshot,
                            "Email"),

                    CompanyWebsite =
                        GetSnapshotString(
                            companySnapshot,
                            "Website"),

                    CompanyContactPerson =
                        GetSnapshotString(
                            companySnapshot,
                            "ContactPerson"),

                    CompanyAddress =
                        GetSnapshotString(
                            companySnapshot,
                            "Address"),

                    CompanyCity =
                        GetSnapshotString(
                            companySnapshot,
                            "City"),

                    CompanyState =
                        GetSnapshotString(
                            companySnapshot,
                            "State"),

                    CompanyCountry =
                        GetSnapshotString(
                            companySnapshot,
                            "Country"),

                    CompanyPostalCode =
                        GetSnapshotString(
                            companySnapshot,
                            "PostalCode"),

                    TransporterName =
                        deliveryChallan.TransporterName,

                    VehicleNumber =
                        deliveryChallan.VehicleNumber,

                    TransportReference =
                        deliveryChallan.TransportReference,

                    DispatchFrom =
                        deliveryChallan.DispatchFrom,

                    Destination =
                        deliveryChallan.Destination,

                    Remarks =
                        deliveryChallan.Remarks,

                    FinalizedOn =
                        deliveryChallan.FinalizedOn,

                    FinalizedBy =
                        deliveryChallan.FinalizedBy
                };


            foreach (var item
                in deliveryChallan.Items
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                viewModel.Items.Add(
                    new DeliveryChallanItemDetailsViewModel
                    {
                        Id =
                            item.Id,

                        SequenceNumber =
                            item.SequenceNumber,

                        PreDispatchInspectionId =
                            item.PreDispatchInspectionId,

                        PreDispatchInspectionCode =
                            item.PreDispatchInspectionCode,

                        PdiAcceptedQuantity =
                            item.PdiAcceptedQuantity,

                        ProductionJobId =
                            item.ProductionJobId,

                        ProductionJobCode =
                            item.ProductionJobCode,

                        CustomerPurchaseOrderItemId =
                            item.CustomerPurchaseOrderItemId,

                        CustomerPurchaseOrderCode =
                            item.CustomerPurchaseOrderCode,

                        CustomerPurchaseOrderNumber =
                            item.CustomerPurchaseOrderNumber,

                        CustomerItemCode =
                            item.CustomerItemCode,

                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        PartNumber =
                            item.PartNumber,

                        UnitName =
                            item.UnitName,

                        ProductReference =
                            item.ProductReference,

                        HsnNumber =
                            item.HsnNumber,

                        CustomerDrawingId =
                            item.CustomerDrawingId,

                        CustomerDrawingNumber =
                            item.CustomerDrawingNumber,

                        CustomerDrawingRevision =
                            item.CustomerDrawingRevision,

                        DispatchQuantity =
                            item.DispatchQuantity
                    });
            }

            return viewModel;
        }

        #endregion
    }
}