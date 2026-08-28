/*
============================================================
File: InvoiceController.cs

Module:
Invoice

Purpose:
Handles Web requests for Invoice module.

Responsibilities:
- List and search Invoices.
- Create Invoice from Customer Purchase Order.
- Load Completed Production Jobs for selected Customer PO.
- Display Customer / Company historical snapshots.
- Allow editable Billing Address.
- Accept Invoice Qty / Rate / Discount / GST.
- Handle PDI / Delivery Challan warning confirmation.
- Edit Draft Invoice.
- Display Invoice Details.
- Finalize Invoice.
- Download Finalized Invoice PDF.
- Soft-delete and restore Draft Invoice.

Important:
- Business logic remains in InvoiceService.
- Controller does not access database directly.
- New Invoice source:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory.
- PDI is NOT mandatory.
- Missing PDI / Delivery Challan is warning-only.
- Browser-posted source snapshot data is NOT trusted.
- Financial calculations posted by browser are NOT trusted.
- InvoiceService recalculates all authoritative amounts.

Temporary Compatibility:
- InvoiceFormViewModel.AvailableDeliveryChallans is currently
  reused to carry Customer PO SelectListItems.
- This will be renamed when InvoiceFormViewModel is updated.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.Invoice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Text.Json;

namespace AjayIndustriesERP.Web.Controllers
{
    public class InvoiceController
        : Controller
    {
        #region Fields

        private readonly IInvoiceService
            _invoiceService;

        #endregion


        #region Constructor

        public InvoiceController(
            IInvoiceService invoiceService)
        {
            _invoiceService =
                invoiceService;
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
                await _invoiceService
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
            int? customerPurchaseOrderId = null)
        {
            InvoiceFormViewModel
                viewModel;


            #region Preselected Customer PO

            if (
                customerPurchaseOrderId.HasValue &&
                customerPurchaseOrderId.Value > 0
            )
            {
                try
                {
                    var preparedInvoice =
                        await _invoiceService
                            .PrepareDraftAsync(
                                customerPurchaseOrderId.Value);


                    if (preparedInvoice == null)
                    {
                        TempData["ErrorMessage"] =
                            "Selected Customer Purchase Order is not available for Invoice.";

                        return RedirectToAction(
                            nameof(Index));
                    }


                    viewModel =
                        await MapToFormViewModelAsync(
                            preparedInvoice);
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
                    new InvoiceFormViewModel
                    {
                        InvoiceDate =
                            DateTime.Today,

                        Status =
                            InvoiceStatus.Draft
                    };
            }

            #endregion


            #region Customer PO Dropdown

            await PopulateAvailableCustomerPurchaseOrdersAsync(
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
            InvoiceFormViewModel viewModel,
            bool confirmSourceWarning = false)
        {
            #region Model Validation

            if (!ModelState.IsValid)
            {
                await RefreshFormSnapshotsAsync(
                    viewModel);

                await PopulateAvailableCustomerPurchaseOrdersAsync(
                    viewModel);

                return View(
                    viewModel);
            }

            #endregion


            try
            {
                var invoice =
                    MapToDomain(
                        viewModel);


                var createdInvoice =
                    await _invoiceService
                        .CreateAsync(
                            invoice,
                            confirmSourceWarning);


                TempData["SuccessMessage"] =
                    $"Invoice {createdInvoice.Code} created successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            createdInvoice.Id
                    });
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);


                await RefreshFormSnapshotsAsync(
                    viewModel);


                await PopulateAvailableCustomerPurchaseOrdersAsync(
                    viewModel);


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Customer PO / Production Job AJAX

        [HttpGet]
        public async Task<IActionResult>
            GetCustomerPurchaseOrderData(
                int id,
                int? invoiceId = null)
        {
            try
            {
                #region Load Customer PO

                var customerPurchaseOrder =
                    await _invoiceService
                        .GetCustomerPurchaseOrderForInvoiceAsync(
                            id);


                if (customerPurchaseOrder == null)
                {
                    return Json(
                        new
                        {
                            success = false,

                            message =
                                "Selected Customer Purchase Order is not available."
                        });
                }

                #endregion


                #region Master Snapshot Source

                var shouldRefreshMasterData =
                    !invoiceId.HasValue ||
                    invoiceId.Value <= 0;


                Invoice?
                    preparedDraft =
                        null;


                var customerSnapshot =
                    new Dictionary<
                        string,
                        JsonElement>();


                var companySnapshot =
                    new Dictionary<
                        string,
                        JsonElement>();


                if (shouldRefreshMasterData)
                {
                    preparedDraft =
                        await _invoiceService
                            .PrepareDraftAsync(
                                id);


                    if (preparedDraft != null)
                    {
                        customerSnapshot =
                            ParseSnapshot(
                                preparedDraft
                                    .CustomerSnapshotJson);


                        companySnapshot =
                            ParseSnapshot(
                                preparedDraft
                                    .CompanySnapshotJson);
                    }
                }

                #endregion


                #region Completed Production Jobs

                var productionJobs =
                    await _invoiceService
                        .GetCompletedProductionJobsForInvoiceAsync(
                            id);


                /*
                 * During Edit, currently selected Production
                 * Jobs may not appear in normal availability
                 * list because current Draft reserves them.
                 *
                 * Add those trusted Jobs back so Edit AJAX
                 * can calculate quantity using exclusion.
                 */
                if (
                    invoiceId.HasValue &&
                    invoiceId.Value > 0
                )
                {
                    var currentInvoice =
                        await _invoiceService
                            .GetByIdAsync(
                                invoiceId.Value);


                    if (currentInvoice != null)
                    {
                        foreach (var currentItem
                            in currentInvoice.Items
                                .Where(x =>
                                    !x.IsDeleted &&
                                    x.IsActive &&
                                    x.ProductionJobId.HasValue &&
                                    x.ProductionJobId.Value > 0))
                        {
                            var productionJobId =
                                currentItem
                                    .ProductionJobId
                                    .Value;


                            if (productionJobs.Any(x =>
                                x.Id ==
                                    productionJobId))
                            {
                                continue;
                            }


                            var productionJob =
                                await _invoiceService
                                    .GetCompletedProductionJobForInvoiceAsync(
                                        productionJobId);


                            if (productionJob == null)
                            {
                                continue;
                            }


                            var purchaseOrderId =
                                GetCustomerPurchaseOrderId(
                                    productionJob);


                            if (purchaseOrderId != id)
                            {
                                continue;
                            }


                            productionJobs.Add(
                                productionJob);
                        }
                    }
                }

                #endregion


                #region Warning Status

                var productionJobIds =
                    productionJobs
                        .Select(x =>
                            x.Id)
                        .Distinct()
                        .ToList();


                var warningProductionJobIds =
                    await _invoiceService
                        .GetProductionJobIdsRequiringWarningAsync(
                            productionJobIds);


                var warningSet =
                    new HashSet<int>(
                        warningProductionJobIds);

                #endregion


                #region Production Job Items

                var itemResults =
                    new List<object>();


                foreach (var productionJob
                    in productionJobs
                        .GroupBy(x =>
                            x.Id)
                        .Select(x =>
                            x.First())
                        .OrderBy(x =>
                            x.Id))
                {
                    var availableQuantity =
                        await _invoiceService
                            .GetRemainingInvoiceQuantityAsync(
                                productionJob.Id,
                                invoiceId);


                    if (availableQuantity <= 0)
                    {
                        continue;
                    }


                    var productionQuantity =
                        GetProductionQuantity(
                            productionJob);


                    var alreadyInvoicedQuantity =
                        productionQuantity -
                        availableQuantity;


                    if (alreadyInvoicedQuantity < 0)
                    {
                        alreadyInvoicedQuantity =
                            0;
                    }


                    var poItem =
                        GetPropertyValue(
                            productionJob,
                            "CustomerPurchaseOrderItem");


                    var productionMasterItem =
                        GetPropertyValue(
                            productionJob,
                            "Item");


                    var poMasterItem =
                        GetPropertyValue(
                            poItem,
                            "Item");


                    var customerPurchaseOrderItemId =
                        GetIntProperty(
                            productionJob,
                            "CustomerPurchaseOrderItemId")

                        ??

                        GetIntProperty(
                            poItem,
                            "Id")

                        ??

                        0;


                    var itemId =
                        GetIntProperty(
                            productionJob,
                            "ItemId")

                        ??

                        GetIntProperty(
                            poItem,
                            "ItemId")

                        ??

                        GetIntProperty(
                            productionMasterItem,
                            "Id")

                        ??

                        GetIntProperty(
                            poMasterItem,
                            "Id")

                        ??

                        0;


                    var itemCode =
                        FirstNonEmpty(
                            GetStringProperty(
                                productionJob,
                                "ItemCode"),

                            GetStringProperty(
                                poItem,
                                "ItemCode"),

                            GetStringProperty(
                                productionMasterItem,
                                "Code",
                                "ItemCode"),

                            GetStringProperty(
                                poMasterItem,
                                "Code",
                                "ItemCode"));


                    var itemName =
                        FirstNonEmpty(
                            GetStringProperty(
                                productionJob,
                                "ItemName"),

                            GetStringProperty(
                                poItem,
                                "ItemName"),

                            GetStringProperty(
                                productionMasterItem,
                                "ItemName",
                                "Name"),

                            GetStringProperty(
                                poMasterItem,
                                "ItemName",
                                "Name"));


                    var partNumber =
                        FirstNonEmpty(
                            GetStringProperty(
                                productionJob,
                                "PartNumber"),

                            GetStringProperty(
                                poItem,
                                "PartNumber"),

                            GetStringProperty(
                                productionMasterItem,
                                "PartNumber"),

                            GetStringProperty(
                                poMasterItem,
                                "PartNumber"));


                    var customerItemCode =
                        FirstNonEmpty(
                            GetStringProperty(
                                productionJob,
                                "CustomerItemCode"),

                            GetStringProperty(
                                poItem,
                                "CustomerItemCode"));


                    var unitName =
                        FirstNonEmpty(
                            GetStringProperty(
                                productionJob,
                                "UnitName",
                                "UomName",
                                "UOMName"),

                            GetStringProperty(
                                poItem,
                                "UnitName",
                                "UomName",
                                "UOMName"),

                            GetStringProperty(
                                productionMasterItem,
                                "UnitName",
                                "UomName",
                                "UOMName"),

                            GetStringProperty(
                                poMasterItem,
                                "UnitName",
                                "UomName",
                                "UOMName"));


                    var hsnNumber =
                        FirstNonEmpty(
                            GetStringProperty(
                                productionJob,
                                "HsnNumber",
                                "HSNNumber",
                                "HsnCode"),

                            GetStringProperty(
                                poItem,
                                "HsnNumber",
                                "HSNNumber",
                                "HsnCode"),

                            GetStringProperty(
                                productionMasterItem,
                                "HsnNumber",
                                "HSNNumber",
                                "HsnCode"),

                            GetStringProperty(
                                poMasterItem,
                                "HsnNumber",
                                "HSNNumber",
                                "HsnCode"));


                    itemResults.Add(
                        new
                        {
                            productionJobId =
                                productionJob.Id,

                            productionJobCode =
                                GetProductionJobCode(
                                    productionJob),

                            productionQuantity,

                            alreadyInvoicedQuantity,

                            availableQuantity,

                            requiresWarning =
                                warningSet.Contains(
                                    productionJob.Id),


                            productReference =
                                FirstNonEmpty(
                                    GetStringProperty(
                                        productionJob,
                                        "ProductReference",
                                        "ProductRef"),

                                    GetStringProperty(
                                        poItem,
                                        "ProductReference",
                                        "ProductRef")),


                            itemId,

                            itemCode,

                            itemName,

                            partNumber,

                            customerItemCode,

                            unitName,

                            hsnNumber,


                            customerPurchaseOrderItemId,

                            customerPurchaseOrderCode =
                                GetCustomerPurchaseOrderCode(
                                    customerPurchaseOrder),

                            customerPurchaseOrderNumber =
                                GetCustomerPurchaseOrderNumber(
                                    customerPurchaseOrder)
                        });
                }

                #endregion


                if (itemResults.Count == 0)
                {
                    return Json(
                        new
                        {
                            success = false,

                            message =
                                "Selected Customer Purchase Order has no Completed Production quantity available for Invoice."
                        });
                }


                #region Customer Source

                var customer =
                    GetPropertyValue(
                        customerPurchaseOrder,
                        "Customer");


                var customerId =
                    GetIntProperty(
                        customerPurchaseOrder,
                        "CustomerId")

                    ??

                    GetIntProperty(
                        customer,
                        "Id")

                    ??

                    0;


                var customerName =
                    FirstNonEmpty(
                        GetStringProperty(
                            customerPurchaseOrder,
                            "CustomerName"),

                        GetStringProperty(
                            customer,
                            "CustomerName",
                            "Name"));

                #endregion


                #region Response

                return Json(
                    new
                    {
                        success = true,


                        // =====================================
                        // Customer Purchase Order
                        // =====================================

                        customerPurchaseOrderId =
                            customerPurchaseOrder.Id,

                        customerPurchaseOrderCode =
                            GetCustomerPurchaseOrderCode(
                                customerPurchaseOrder),

                        customerPurchaseOrderNumber =
                            GetCustomerPurchaseOrderNumber(
                                customerPurchaseOrder),

                        customerPurchaseOrderDate =
                            GetDateProperty(
                                customerPurchaseOrder,
                                "ReceivedDate",
                                "PurchaseOrderDate",
                                "PODate",
                                "OrderDate")
                                ?.ToString(
                                    "yyyy-MM-dd"),


                        // =====================================
                        // Customer
                        // =====================================

                        customerId,

                        customerName,


                        customerCode =
                            GetSnapshotString(
                                customerSnapshot,
                                "Code"),

                        customerGstin =
                            GetSnapshotString(
                                customerSnapshot,
                                "GSTIN"),

                        customerPan =
                            GetSnapshotString(
                                customerSnapshot,
                                "PAN"),


                        // =====================================
                        // Billing Address
                        // =====================================

                        billingAddressLine1 =
                            preparedDraft
                                ?.BillingAddressLine1,

                        billingAddressLine2 =
                            preparedDraft
                                ?.BillingAddressLine2,

                        billingCity =
                            preparedDraft
                                ?.BillingCity,

                        billingDistrict =
                            preparedDraft
                                ?.BillingDistrict,

                        billingState =
                            preparedDraft
                                ?.BillingState,

                        billingPincode =
                            preparedDraft
                                ?.BillingPincode,

                        billingCountry =
                            preparedDraft
                                ?.BillingCountry,


                        // =====================================
                        // Payment
                        // =====================================

                        paymentTerms =
                            preparedDraft
                                ?.PaymentTerms,

                        creditDays =
                            preparedDraft
                                ?.CreditDays,

                        dueDate =
                            preparedDraft
                                ?.DueDate
                                ?.ToString(
                                    "yyyy-MM-dd"),


                        // =====================================
                        // GST
                        // =====================================

                        placeOfSupply =
                            preparedDraft
                                ?.PlaceOfSupply,

                        isInterState =
                            preparedDraft
                                ?.IsInterState
                            ?? false,


                        // =====================================
                        // Company
                        // =====================================

                        companyId =
                            preparedDraft
                                ?.CompanyId,

                        companyName =
                            preparedDraft
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

                        companyIsoCertificationNumber =
                            GetSnapshotString(
                                companySnapshot,
                                "IsoCertificationNumber"),

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

                        companyPostalCode =
                            GetSnapshotString(
                                companySnapshot,
                                "PostalCode"),

                        companyCountry =
                            GetSnapshotString(
                                companySnapshot,
                                "Country"),

                        companyPhoneNumber =
                            GetSnapshotString(
                                companySnapshot,
                                "PhoneNumber"),

                        companyEmail =
                            GetSnapshotString(
                                companySnapshot,
                                "Email"),


                        // =====================================
                        // Bank Details
                        // =====================================

                        bankName =
                            GetSnapshotString(
                                companySnapshot,
                                "BankName"),

                        bankAccountHolderName =
                            GetSnapshotString(
                                companySnapshot,
                                "BankAccountHolderName"),

                        bankAccountNumber =
                            GetSnapshotString(
                                companySnapshot,
                                "BankAccountNumber"),

                        bankIfscCode =
                            GetSnapshotString(
                                companySnapshot,
                                "BankIfscCode"),

                        bankBranchName =
                            GetSnapshotString(
                                companySnapshot,
                                "BankBranchName"),

                        bankAccountType =
                            GetSnapshotString(
                                companySnapshot,
                                "BankAccountType"),


                        invoiceTermsAndConditions =
                            preparedDraft
                                ?.InvoiceTermsAndConditions,


                        shouldRefreshMasterData,


                        // =====================================
                        // Warning
                        // =====================================

                        requiresSourceWarning =
                            warningProductionJobIds
                                .Count > 0,

                        warningProductionJobIds,


                        // =====================================
                        // Lines
                        // =====================================

                        items =
                            itemResults
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
            var invoice =
                await _invoiceService
                    .GetByIdAsync(
                        id);


            if (invoice == null)
            {
                return NotFound();
            }


            if (invoice.Status !=
                InvoiceStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Finalized Invoice cannot be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            var viewModel =
                await MapToFormViewModelAsync(
                    invoice,
                    invoice.Id);


            await PopulateAvailableCustomerPurchaseOrdersAsync(
                viewModel,
                invoice.Id);


            return View(
                viewModel);
        }

        #endregion


        #region Edit POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            InvoiceFormViewModel viewModel,
            bool confirmSourceWarning = false)
        {
            if (viewModel.Id <= 0)
            {
                return BadRequest();
            }


            #region Model Validation

            if (!ModelState.IsValid)
            {
                await RefreshFormSnapshotsAsync(
                    viewModel,
                    viewModel.Id);


                await PopulateAvailableCustomerPurchaseOrdersAsync(
                    viewModel,
                    viewModel.Id);


                return View(
                    viewModel);
            }

            #endregion


            try
            {
                var invoice =
                    MapToDomain(
                        viewModel);


                var updatedInvoice =
                    await _invoiceService
                        .UpdateAsync(
                            invoice,
                            confirmSourceWarning);


                TempData["SuccessMessage"] =
                    $"Invoice {updatedInvoice.Code} updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            updatedInvoice.Id
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


                await PopulateAvailableCustomerPurchaseOrdersAsync(
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
            var invoice =
                await _invoiceService
                    .GetByIdAsync(
                        id);


            if (invoice == null)
            {
                return NotFound();
            }


            #region Source Warning

            /*
             * Warning is relevant only while Invoice
             * is still Draft and can be finalized.
             */
            var requiresSourceWarning =
                false;


            if (invoice.Status ==
                InvoiceStatus.Draft)
            {
                var productionJobIds =
                    invoice.Items
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive &&
                            x.ProductionJobId.HasValue &&
                            x.ProductionJobId.Value > 0)
                        .Select(x =>
                            x.ProductionJobId!.Value)
                        .Distinct()
                        .ToList();


                if (productionJobIds.Count > 0)
                {
                    var warningProductionJobIds =
                        await _invoiceService
                            .GetProductionJobIdsRequiringWarningAsync(
                                productionJobIds);


                    requiresSourceWarning =
                        warningProductionJobIds.Any();
                }
            }


            ViewBag.RequiresSourceWarning =
                requiresSourceWarning;

            #endregion


            var viewModel =
                MapToDetailsViewModel(
                    invoice);


            return View(
                viewModel);
        }

        #endregion


        #region Finalize

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(
            int id,
            bool confirmSourceWarning = false)
        {
            try
            {
                var invoice =
                    await _invoiceService
                        .FinalizeAsync(
                            id,
                            confirmSourceWarning);


                TempData["SuccessMessage"] =
                    $"Invoice {invoice.Code} finalized successfully.";
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
                    await _invoiceService
                        .GeneratePdfAsync(
                            id);


                var invoice =
                    await _invoiceService
                        .GetByIdAsync(
                            id);


                if (invoice == null)
                {
                    return NotFound();
                }


                var safeCode =
                    invoice.Code
                        .Replace(
                            "/",
                            "-")
                        .Replace(
                            "\\",
                            "-");


                return File(
                    pdfBytes,
                    "application/pdf",
                    $"Invoice-{safeCode}.pdf");
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
                await _invoiceService
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Invoice deleted successfully.";
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
            var invoices =
                await _invoiceService
                    .GetDeletedAsync();


            return View(
                invoices);
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
                await _invoiceService
                    .RestoreAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Invoice restored successfully.";
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


        #region Populate Customer PO Dropdown

        private async Task
            PopulateAvailableCustomerPurchaseOrdersAsync(
                InvoiceFormViewModel viewModel,
                int? excludeInvoiceId = null)
        {
            #region Available Sources

            var customerPurchaseOrders =
                await _invoiceService
                    .GetCustomerPurchaseOrdersForInvoiceAsync();

            #endregion


            #region Selected Customer PO Sources

            var selectedCustomerPurchaseOrderIds =
                new HashSet<int>();


            foreach (var item
                in viewModel.Items)
            {
                var productionJobId =
                    GetIntProperty(
                        item,
                        "ProductionJobId");


                if (
                    !productionJobId.HasValue ||
                    productionJobId.Value <= 0
                )
                {
                    continue;
                }


                var productionJob =
                    await _invoiceService
                        .GetCompletedProductionJobForInvoiceAsync(
                            productionJobId.Value);


                if (productionJob == null)
                {
                    continue;
                }


                var customerPurchaseOrderId =
                    GetCustomerPurchaseOrderId(
                        productionJob);


                if (customerPurchaseOrderId > 0)
                {
                    selectedCustomerPurchaseOrderIds.Add(
                        customerPurchaseOrderId);
                }
            }

            #endregion


            #region Ensure Selected PO Exists In Dropdown

            foreach (var selectedId
                in selectedCustomerPurchaseOrderIds)
            {
                if (customerPurchaseOrders.Any(x =>
                    x.Id == selectedId))
                {
                    continue;
                }


                var selectedPurchaseOrder =
                    await _invoiceService
                        .GetCustomerPurchaseOrderForInvoiceAsync(
                            selectedId);


                if (selectedPurchaseOrder != null)
                {
                    customerPurchaseOrders.Add(
                        selectedPurchaseOrder);
                }
            }

            #endregion


            #region Select List

            var options =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = "",

                        Text =
                            "-- Select Customer Purchase Order --"
                    }
                };


            foreach (var customerPurchaseOrder
                in customerPurchaseOrders
                    .OrderByDescending(x =>
                        GetDateProperty(
                            x,
                            "ReceivedDate",
                            "PurchaseOrderDate",
                            "PODate",
                            "OrderDate"))
                    .ThenByDescending(x =>
                        x.Id))
            {
                var productionJobIds =
                    new HashSet<int>();


                var availableJobs =
                    await _invoiceService
                        .GetCompletedProductionJobsForInvoiceAsync(
                            customerPurchaseOrder.Id);


                foreach (var job
                    in availableJobs)
                {
                    productionJobIds.Add(
                        job.Id);
                }


                /*
                 * Add existing Edit lines so current Invoice
                 * quantity becomes available after exclusion.
                 */
                if (selectedCustomerPurchaseOrderIds.Contains(
                    customerPurchaseOrder.Id))
                {
                    foreach (var item
                        in viewModel.Items)
                    {
                        var productionJobId =
                            GetIntProperty(
                                item,
                                "ProductionJobId");


                        if (
                            !productionJobId.HasValue ||
                            productionJobId.Value <= 0
                        )
                        {
                            continue;
                        }


                        var productionJob =
                            await _invoiceService
                                .GetCompletedProductionJobForInvoiceAsync(
                                    productionJobId.Value);


                        if (productionJob == null)
                        {
                            continue;
                        }


                        if (GetCustomerPurchaseOrderId(
                                productionJob) !=
                            customerPurchaseOrder.Id)
                        {
                            continue;
                        }


                        productionJobIds.Add(
                            productionJobId.Value);
                    }
                }


                decimal totalAvailable =
                    0;


                foreach (var productionJobId
                    in productionJobIds)
                {
                    totalAvailable +=
                        await _invoiceService
                            .GetRemainingInvoiceQuantityAsync(
                                productionJobId,
                                excludeInvoiceId);
                }


                if (
                    totalAvailable <= 0 &&
                    !selectedCustomerPurchaseOrderIds
                        .Contains(
                            customerPurchaseOrder.Id)
                )
                {
                    continue;
                }


                var customer =
                    GetPropertyValue(
                        customerPurchaseOrder,
                        "Customer");


                var customerName =
                    FirstNonEmpty(
                        GetStringProperty(
                            customerPurchaseOrder,
                            "CustomerName"),

                        GetStringProperty(
                            customer,
                            "CustomerName",
                            "Name"));


                var poCode =
                    GetCustomerPurchaseOrderCode(
                        customerPurchaseOrder);


                var poNumber =
                    GetCustomerPurchaseOrderNumber(
                        customerPurchaseOrder);


                var poDisplay =
                    FirstNonEmpty(
                        poNumber,
                        poCode,
                        $"PO #{customerPurchaseOrder.Id}");


                options.Add(
                    new SelectListItem
                    {
                        Value =
                            customerPurchaseOrder.Id
                                .ToString(),

                        Text =
                            $"{poDisplay}" +
                            (
                                string.IsNullOrWhiteSpace(
                                    customerName)
                                    ? string.Empty
                                    : $" | {customerName}"
                            ) +
                            $" | Available: {totalAvailable:0.###}",

                        Selected =
                            selectedCustomerPurchaseOrderIds
                                .Contains(
                                    customerPurchaseOrder.Id)
                    });
            }


            /*
             * Temporary property name from old DC flow.
             * It now contains Customer PO options.
             */
            viewModel.AvailableDeliveryChallans =
                options;

            #endregion
        }

        #endregion


        #region Form Snapshot Refresh

        private async Task RefreshFormSnapshotsAsync(
            InvoiceFormViewModel viewModel,
            int? excludeInvoiceId = null)
        {
            #region Restore Header Snapshots

            if (
                viewModel.Id > 0 &&
                excludeInvoiceId.HasValue
            )
            {
                var savedInvoice =
                    await _invoiceService
                        .GetByIdAsync(
                            viewModel.Id);


                if (savedInvoice != null)
                {
                    viewModel.CustomerId =
                        savedInvoice.CustomerId;

                    viewModel.CustomerName =
                        savedInvoice.CustomerName;


                    ApplyCustomerSnapshotToForm(
                        viewModel,
                        savedInvoice
                            .CustomerSnapshotJson);


                    ApplyCompanySnapshotToForm(
                        viewModel,
                        savedInvoice
                            .CompanySnapshotJson);


                    viewModel.CompanyId =
                        savedInvoice.CompanyId;

                    viewModel.CompanyName =
                        savedInvoice.CompanyName;


                    viewModel.PaymentTerms =
                        savedInvoice.PaymentTerms;

                    viewModel.CreditDays =
                        savedInvoice.CreditDays;
                }
            }
            else
            {
                var firstProductionJobId =
                    viewModel.Items
                        .Select(x =>
                            GetIntProperty(
                                x,
                                "ProductionJobId"))
                        .FirstOrDefault(x =>
                            x.HasValue &&
                            x.Value > 0);


                if (firstProductionJobId.HasValue)
                {
                    try
                    {
                        var productionJob =
                            await _invoiceService
                                .GetCompletedProductionJobForInvoiceAsync(
                                    firstProductionJobId.Value);


                        if (productionJob != null)
                        {
                            var customerPurchaseOrderId =
                                GetCustomerPurchaseOrderId(
                                    productionJob);


                            var preparedDraft =
                                await _invoiceService
                                    .PrepareDraftAsync(
                                        customerPurchaseOrderId);


                            if (preparedDraft != null)
                            {
                                viewModel.CustomerId =
                                    preparedDraft.CustomerId;

                                viewModel.CustomerName =
                                    preparedDraft.CustomerName;


                                ApplyCustomerSnapshotToForm(
                                    viewModel,
                                    preparedDraft
                                        .CustomerSnapshotJson);


                                ApplyCompanySnapshotToForm(
                                    viewModel,
                                    preparedDraft
                                        .CompanySnapshotJson);


                                viewModel.CompanyId =
                                    preparedDraft.CompanyId;

                                viewModel.CompanyName =
                                    preparedDraft.CompanyName;


                                viewModel.PaymentTerms =
                                    preparedDraft.PaymentTerms;

                                viewModel.CreditDays =
                                    preparedDraft.CreditDays;
                            }
                        }
                    }
                    catch (BusinessException)
                    {
                        // Keep submitted form data.
                    }
                }
            }

            #endregion


            #region Refresh Production Quantities

            var sequenceNumber =
                1;


            foreach (var item
                in viewModel.Items)
            {
                item.SequenceNumber =
                    sequenceNumber++;


                var productionJobId =
                    GetIntProperty(
                        item,
                        "ProductionJobId");


                if (
                    !productionJobId.HasValue ||
                    productionJobId.Value <= 0
                )
                {
                    continue;
                }


                try
                {
                    var productionJob =
                        await _invoiceService
                            .GetCompletedProductionJobForInvoiceAsync(
                                productionJobId.Value);


                    if (productionJob == null)
                    {
                        continue;
                    }


                    var availableQuantity =
                        await _invoiceService
                            .GetRemainingInvoiceQuantityAsync(
                                productionJobId.Value,
                                excludeInvoiceId);


                    var productionQuantity =
                        GetProductionQuantity(
                            productionJob);


                    var alreadyInvoicedQuantity =
                        productionQuantity -
                        availableQuantity;


                    if (alreadyInvoicedQuantity < 0)
                    {
                        alreadyInvoicedQuantity =
                            0;
                    }


                    item.AvailableQuantity =
                        availableQuantity;

                    item.AlreadyInvoicedQuantity =
                        alreadyInvoicedQuantity;


                    /*
                     * Browser-posted line snapshot display
                     * values are left intact here.
                     *
                     * InvoiceService rebuilds authoritative
                     * source snapshots before persistence.
                     */
                }
                catch (BusinessException)
                {
                    // Keep submitted form line.
                }
            }

            #endregion
        }

        #endregion


        #region Domain Mapping

        private static Invoice MapToDomain(
            InvoiceFormViewModel viewModel)
        {
            var invoice =
                new Invoice
                {
                    Id =
                        viewModel.Id,

                    InvoiceDate =
                        viewModel.InvoiceDate,


                    BillingAddressLine1 =
                        viewModel.BillingAddressLine1,

                    BillingAddressLine2 =
                        viewModel.BillingAddressLine2,

                    BillingCity =
                        viewModel.BillingCity,

                    BillingDistrict =
                        viewModel.BillingDistrict,

                    BillingState =
                        viewModel.BillingState,

                    BillingPincode =
                        viewModel.BillingPincode,

                    BillingCountry =
                        viewModel.BillingCountry,


                    OtherCharges =
                        viewModel.OtherCharges,

                    InvoiceTermsAndConditions =
                        viewModel.InvoiceTermsAndConditions,

                    Remarks =
                        viewModel.Remarks
                };


            foreach (var item
                in viewModel.Items)
            {
                invoice.Items.Add(
                    new InvoiceItem
                    {
                        Id =
                            item.Id,

                        SequenceNumber =
                            item.SequenceNumber,


                        /*
                         * Production Job is now the
                         * authoritative Invoice source.
                         */
                        ProductionJobId =
                            GetIntProperty(
                                item,
                                "ProductionJobId"),


                        InvoiceQuantity =
                            item.InvoiceQuantity,

                        Rate =
                            item.Rate,

                        DiscountPercent =
                            item.DiscountPercent,

                        GstRate =
                            item.GstRate,


                        IsActive =
                            true,

                        IsDeleted =
                            false
                    });
            }


            return invoice;
        }

        #endregion


        #region Form Mapping

        private async Task<InvoiceFormViewModel>
            MapToFormViewModelAsync(
                Invoice invoice,
                int? excludeInvoiceId = null)
        {
            #region Header

            var viewModel =
                new InvoiceFormViewModel
                {
                    Id =
                        invoice.Id,

                    Code =
                        invoice.Code,

                    InvoiceDate =
                        invoice.InvoiceDate,

                    DueDate =
                        invoice.DueDate,

                    Status =
                        invoice.Status,


                    CustomerId =
                        invoice.CustomerId,

                    CustomerName =
                        invoice.CustomerName,


                    BillingAddressLine1 =
                        invoice.BillingAddressLine1,

                    BillingAddressLine2 =
                        invoice.BillingAddressLine2,

                    BillingCity =
                        invoice.BillingCity,

                    BillingDistrict =
                        invoice.BillingDistrict,

                    BillingState =
                        invoice.BillingState,

                    BillingPincode =
                        invoice.BillingPincode,

                    BillingCountry =
                        invoice.BillingCountry,


                    CompanyId =
                        invoice.CompanyId,

                    CompanyName =
                        invoice.CompanyName,


                    PaymentTerms =
                        invoice.PaymentTerms,

                    CreditDays =
                        invoice.CreditDays,


                    PlaceOfSupply =
                        invoice.PlaceOfSupply,

                    IsInterState =
                        invoice.IsInterState,


                    OtherCharges =
                        invoice.OtherCharges,


                    GrossAmount =
                        invoice.GrossAmount,

                    DiscountAmount =
                        invoice.DiscountAmount,

                    TaxableAmount =
                        invoice.TaxableAmount,

                    CgstAmount =
                        invoice.CgstAmount,

                    SgstAmount =
                        invoice.SgstAmount,

                    IgstAmount =
                        invoice.IgstAmount,

                    RoundOffAmount =
                        invoice.RoundOffAmount,

                    GrandTotal =
                        invoice.GrandTotal,


                    InvoiceTermsAndConditions =
                        invoice
                            .InvoiceTermsAndConditions,

                    Remarks =
                        invoice.Remarks
                };

            #endregion


            #region Customer Snapshot

            ApplyCustomerSnapshotToForm(
                viewModel,
                invoice.CustomerSnapshotJson);

            #endregion


            #region Company Snapshot

            ApplyCompanySnapshotToForm(
                viewModel,
                invoice.CompanySnapshotJson);

            #endregion


            #region Active Items

            var activeItems =
                invoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();

            #endregion


            #region Source Warning Status

            var productionJobIds =
                activeItems
                    .Where(x =>
                        x.ProductionJobId.HasValue &&
                        x.ProductionJobId.Value > 0)
                    .Select(x =>
                        x.ProductionJobId!.Value)
                    .Distinct()
                    .ToList();


            var warningProductionJobIds =
                await _invoiceService
                    .GetProductionJobIdsRequiringWarningAsync(
                        productionJobIds);


            var warningSet =
                new HashSet<int>(
                    warningProductionJobIds);

            #endregion


            #region Items

            foreach (var item
                in activeItems)
            {
                var productionJobId =
                    item.ProductionJobId
                        .GetValueOrDefault();


                decimal productionQuantity =
                    item.InvoiceQuantity;


                decimal availableQuantity =
                    item.InvoiceQuantity;


                /*
                 * Production Job is now authoritative source.
                 */
                if (productionJobId > 0)
                {
                    try
                    {
                        availableQuantity =
                            await _invoiceService
                                .GetRemainingInvoiceQuantityAsync(
                                    productionJobId,
                                    excludeInvoiceId);


                        var productionJob =
                            await _invoiceService
                                .GetCompletedProductionJobForInvoiceAsync(
                                    productionJobId);


                        if (productionJob != null)
                        {
                            productionQuantity =
                                GetProductionQuantity(
                                    productionJob);


                            /*
                             * Important:
                             *
                             * Set selected Customer PO so Create/Edit
                             * dropdown displays correct PO.
                             */
                            if (
                                !viewModel.CustomerPurchaseOrderId.HasValue ||
                                viewModel.CustomerPurchaseOrderId.Value <= 0
                            )
                            {
                                var customerPurchaseOrderId =
                                    GetCustomerPurchaseOrderId(
                                        productionJob);


                                if (customerPurchaseOrderId > 0)
                                {
                                    viewModel.CustomerPurchaseOrderId =
                                        customerPurchaseOrderId;
                                }
                            }
                        }
                    }
                    catch (BusinessException)
                    {
                        /*
                         * Keep historical Invoice line visible
                         * even if Production source later becomes
                         * unavailable.
                         */
                    }
                }


                var alreadyInvoicedQuantity =
                    productionQuantity -
                    availableQuantity;


                if (alreadyInvoicedQuantity < 0)
                {
                    alreadyInvoicedQuantity =
                        0;
                }


                viewModel.Items.Add(
                    new InvoiceItemFormViewModel
                    {
                        Id =
                            item.Id,

                        SequenceNumber =
                            item.SequenceNumber,


                        // =====================================
                        // Production Source
                        // =====================================

                        ProductionJobId =
                            item.ProductionJobId,

                        ProductionJobCode =
                            item.ProductionJobCode,

                        ProductionQuantity =
                            productionQuantity,

                        AlreadyInvoicedQuantity =
                            alreadyInvoicedQuantity,

                        AvailableQuantity =
                            availableQuantity,

                        RequiresSourceWarning =
                            productionJobId > 0 &&
                            warningSet.Contains(
                                productionJobId),


                        // =====================================
                        // Product Snapshot
                        // =====================================

                        ProductReference =
                            item.ProductReference,

                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        PartNumber =
                            item.PartNumber,

                        CustomerItemCode =
                            item.CustomerItemCode,

                        UnitName =
                            item.UnitName,

                        HsnNumber =
                            item.HsnNumber,


                        // =====================================
                        // Customer PO Snapshot
                        // =====================================

                        CustomerPurchaseOrderItemId =
                            item.CustomerPurchaseOrderItemId,

                        CustomerPurchaseOrderCode =
                            item.CustomerPurchaseOrderCode,

                        CustomerPurchaseOrderNumber =
                            item.CustomerPurchaseOrderNumber,


                        // =====================================
                        // Optional Historical DC
                        // =====================================

                        DeliveryChallanId =
                            item.DeliveryChallanId,

                        DeliveryChallanCode =
                            item.DeliveryChallanCode,

                        DeliveryChallanItemId =
                            item.DeliveryChallanItemId,

                        DeliveryChallanQuantity =
                            item.DeliveryChallanQuantity,


                        // =====================================
                        // Commercial Values
                        // =====================================

                        InvoiceQuantity =
                            item.InvoiceQuantity,

                        Rate =
                            item.Rate,

                        GrossAmount =
                            item.GrossAmount,


                        DiscountPercent =
                            item.DiscountPercent,

                        DiscountAmount =
                            item.DiscountAmount,

                        TaxableAmount =
                            item.TaxableAmount,


                        GstRate =
                            item.GstRate,

                        CgstRate =
                            item.CgstRate,

                        SgstRate =
                            item.SgstRate,

                        IgstRate =
                            item.IgstRate,

                        CgstAmount =
                            item.CgstAmount,

                        SgstAmount =
                            item.SgstAmount,

                        IgstAmount =
                            item.IgstAmount,

                        TotalTaxAmount =
                            item.TotalTaxAmount,


                        LineTotal =
                            item.LineTotal
                    });
            }

            #endregion


            return viewModel;
        }

        #endregion


        #region Customer Snapshot Projection

        private static void ApplyCustomerSnapshotToForm(
            InvoiceFormViewModel viewModel,
            string? snapshotJson)
        {
            var snapshot =
                ParseSnapshot(
                    snapshotJson);


            viewModel.CustomerCode =
                GetSnapshotString(
                    snapshot,
                    "Code");


            viewModel.CustomerGstin =
                GetSnapshotString(
                    snapshot,
                    "GSTIN");


            viewModel.CustomerPan =
                GetSnapshotString(
                    snapshot,
                    "PAN");
        }

        #endregion


        #region Company Snapshot Projection

        private static void ApplyCompanySnapshotToForm(
            InvoiceFormViewModel viewModel,
            string? snapshotJson)
        {
            var snapshot =
                ParseSnapshot(
                    snapshotJson);


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


            viewModel.CompanyIsoCertificationNumber =
                GetSnapshotString(
                    snapshot,
                    "IsoCertificationNumber");


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


            viewModel.CompanyPostalCode =
                GetSnapshotString(
                    snapshot,
                    "PostalCode");


            viewModel.CompanyCountry =
                GetSnapshotString(
                    snapshot,
                    "Country");


            viewModel.CompanyPhoneNumber =
                GetSnapshotString(
                    snapshot,
                    "PhoneNumber");


            viewModel.CompanyEmail =
                GetSnapshotString(
                    snapshot,
                    "Email");


            viewModel.BankName =
                GetSnapshotString(
                    snapshot,
                    "BankName");


            viewModel.BankAccountHolderName =
                GetSnapshotString(
                    snapshot,
                    "BankAccountHolderName");


            viewModel.BankAccountNumber =
                GetSnapshotString(
                    snapshot,
                    "BankAccountNumber");


            viewModel.BankIfscCode =
                GetSnapshotString(
                    snapshot,
                    "BankIfscCode");


            viewModel.BankBranchName =
                GetSnapshotString(
                    snapshot,
                    "BankBranchName");


            viewModel.BankAccountType =
                GetSnapshotString(
                    snapshot,
                    "BankAccountType");
        }

        #endregion


        #region Details Mapping

        private static InvoiceDetailsViewModel
            MapToDetailsViewModel(
                Invoice invoice)
        {
            var customerSnapshot =
                ParseSnapshot(
                    invoice.CustomerSnapshotJson);


            var companySnapshot =
                ParseSnapshot(
                    invoice.CompanySnapshotJson);


            var viewModel =
                new InvoiceDetailsViewModel
                {
                    #region Identification

                    Id =
                        invoice.Id,

                    Code =
                        invoice.Code,

                    InvoiceDate =
                        invoice.InvoiceDate,

                    DueDate =
                        invoice.DueDate,

                    Status =
                        invoice.Status,

                    #endregion


                    #region Customer

                    CustomerId =
                        invoice.CustomerId,

                    CustomerName =
                        invoice.CustomerName,

                    CustomerCode =
                        GetSnapshotString(
                            customerSnapshot,
                            "Code"),

                    CustomerGstin =
                        GetSnapshotString(
                            customerSnapshot,
                            "GSTIN"),

                    CustomerPan =
                        GetSnapshotString(
                            customerSnapshot,
                            "PAN"),

                    #endregion


                    #region Billing Address

                    BillingAddressLine1 =
                        invoice.BillingAddressLine1,

                    BillingAddressLine2 =
                        invoice.BillingAddressLine2,

                    BillingCity =
                        invoice.BillingCity,

                    BillingDistrict =
                        invoice.BillingDistrict,

                    BillingState =
                        invoice.BillingState,

                    BillingPincode =
                        invoice.BillingPincode,

                    BillingCountry =
                        invoice.BillingCountry,

                    #endregion


                    #region Company

                    CompanyId =
                        invoice.CompanyId,

                    CompanyName =
                        invoice.CompanyName,

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

                    CompanyIsoCertificationNumber =
                        GetSnapshotString(
                            companySnapshot,
                            "IsoCertificationNumber"),

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

                    CompanyPostalCode =
                        GetSnapshotString(
                            companySnapshot,
                            "PostalCode"),

                    CompanyCountry =
                        GetSnapshotString(
                            companySnapshot,
                            "Country"),

                    CompanyPhoneNumber =
                        GetSnapshotString(
                            companySnapshot,
                            "PhoneNumber"),

                    CompanyEmail =
                        GetSnapshotString(
                            companySnapshot,
                            "Email"),

                    #endregion


                    #region Bank

                    BankName =
                        GetSnapshotString(
                            companySnapshot,
                            "BankName"),

                    BankAccountHolderName =
                        GetSnapshotString(
                            companySnapshot,
                            "BankAccountHolderName"),

                    BankAccountNumber =
                        GetSnapshotString(
                            companySnapshot,
                            "BankAccountNumber"),

                    BankIfscCode =
                        GetSnapshotString(
                            companySnapshot,
                            "BankIfscCode"),

                    BankBranchName =
                        GetSnapshotString(
                            companySnapshot,
                            "BankBranchName"),

                    BankAccountType =
                        GetSnapshotString(
                            companySnapshot,
                            "BankAccountType"),

                    #endregion


                    #region Payment / GST

                    PaymentTerms =
                        invoice.PaymentTerms,

                    CreditDays =
                        invoice.CreditDays,

                    PlaceOfSupply =
                        invoice.PlaceOfSupply,

                    IsInterState =
                        invoice.IsInterState,

                    #endregion


                    #region Totals

                    GrossAmount =
                        invoice.GrossAmount,

                    DiscountAmount =
                        invoice.DiscountAmount,

                    TaxableAmount =
                        invoice.TaxableAmount,

                    CgstAmount =
                        invoice.CgstAmount,

                    SgstAmount =
                        invoice.SgstAmount,

                    IgstAmount =
                        invoice.IgstAmount,

                    OtherCharges =
                        invoice.OtherCharges,

                    RoundOffAmount =
                        invoice.RoundOffAmount,

                    GrandTotal =
                        invoice.GrandTotal,

                    #endregion


                    #region Other

                    InvoiceTermsAndConditions =
                        invoice
                            .InvoiceTermsAndConditions,

                    Remarks =
                        invoice.Remarks,

                    FinalizedOn =
                        invoice.FinalizedOn,

                    FinalizedBy =
                        invoice.FinalizedBy

                    #endregion
                };


            #region Items

            foreach (var item
                in invoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                viewModel.Items.Add(
                    new InvoiceItemDetailsViewModel
                    {
                        Id =
                            item.Id,

                        SequenceNumber =
                            item.SequenceNumber,


                        /*
                         * Optional historical DC values.
                         */
                        DeliveryChallanId =
                            item.DeliveryChallanId
                                .GetValueOrDefault(),

                        DeliveryChallanCode =
                            item.DeliveryChallanCode,

                        DeliveryChallanItemId =
                            item.DeliveryChallanItemId
                                .GetValueOrDefault(),

                        DeliveryChallanQuantity =
                            item.DeliveryChallanQuantity
                                .GetValueOrDefault(),


                        ProductReference =
                            item.ProductReference,


                        ItemId =
                            item.ItemId,

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        PartNumber =
                            item.PartNumber,

                        CustomerItemCode =
                            item.CustomerItemCode,

                        UnitName =
                            item.UnitName,

                        HsnNumber =
                            item.HsnNumber,


                        CustomerPurchaseOrderItemId =
                            item.CustomerPurchaseOrderItemId,

                        CustomerPurchaseOrderCode =
                            item.CustomerPurchaseOrderCode,

                        CustomerPurchaseOrderNumber =
                            item.CustomerPurchaseOrderNumber,


                        ProductionJobId =
                            item.ProductionJobId
                                .GetValueOrDefault(),

                        ProductionJobCode =
                            item.ProductionJobCode,


                        InvoiceQuantity =
                            item.InvoiceQuantity,

                        Rate =
                            item.Rate,

                        GrossAmount =
                            item.GrossAmount,


                        DiscountPercent =
                            item.DiscountPercent,

                        DiscountAmount =
                            item.DiscountAmount,

                        TaxableAmount =
                            item.TaxableAmount,


                        GstRate =
                            item.GstRate,

                        CgstRate =
                            item.CgstRate,

                        SgstRate =
                            item.SgstRate,

                        IgstRate =
                            item.IgstRate,

                        CgstAmount =
                            item.CgstAmount,

                        SgstAmount =
                            item.SgstAmount,

                        IgstAmount =
                            item.IgstAmount,

                        TotalTaxAmount =
                            item.TotalTaxAmount,


                        LineTotal =
                            item.LineTotal
                    });
            }

            #endregion


            return viewModel;
        }

        #endregion


        #region Production Source Helpers

        private static int GetCustomerPurchaseOrderId(
            ProductionJob productionJob)
        {
            var directId =
                GetIntProperty(
                    productionJob,
                    "CustomerPurchaseOrderId",
                    "CustomerPOId",
                    "PurchaseOrderId");


            if (
                directId.HasValue &&
                directId.Value > 0
            )
            {
                return directId.Value;
            }


            var poItem =
                GetPropertyValue(
                    productionJob,
                    "CustomerPurchaseOrderItem");


            var poId =
                GetIntProperty(
                    poItem,
                    "CustomerPurchaseOrderId",
                    "CustomerPOId",
                    "PurchaseOrderId");


            if (
                poId.HasValue &&
                poId.Value > 0
            )
            {
                return poId.Value;
            }


            var customerPurchaseOrder =
                GetPropertyValue(
                    poItem,
                    "CustomerPurchaseOrder",
                    "PurchaseOrder");


            return GetIntProperty(
                customerPurchaseOrder,
                "Id")

                ?? 0;
        }


        private static decimal GetProductionQuantity(
            ProductionJob productionJob)
        {
            return GetDecimalProperty(
                productionJob,
                "JobQuantity",
                "CompletedQuantity",
                "ProducedQuantity",
                "ProductionQuantity",
                "Quantity")

                ?? 0m;
        }


        private static string GetProductionJobCode(
            ProductionJob productionJob)
        {
            return FirstNonEmpty(
                GetStringProperty(
                    productionJob,
                    "Code",
                    "ProductionJobCode",
                    "JobCode"),

                productionJob.Id
                    .ToString())

                ?? productionJob.Id
                    .ToString();
        }


        private static string?
            GetCustomerPurchaseOrderCode(
                CustomerPurchaseOrder customerPurchaseOrder)
        {
            return GetStringProperty(
                customerPurchaseOrder,
                "Code",
                "CustomerPurchaseOrderCode",
                "PurchaseOrderCode",
                "POCode");
        }


        private static string?
            GetCustomerPurchaseOrderNumber(
                CustomerPurchaseOrder customerPurchaseOrder)
        {
            return GetStringProperty(
                customerPurchaseOrder,
                "PurchaseOrderNumber",
                "CustomerPurchaseOrderNumber",
                "PONumber",
                "PoNumber",
                "OrderNumber");
        }

        #endregion


        #region Reflection Helpers

        private static object?
            GetPropertyValue(
                object? source,
                params string[] propertyNames)
        {
            if (source == null)
            {
                return null;
            }


            var type =
                source.GetType();


            foreach (var propertyName
                in propertyNames)
            {
                var property =
                    type.GetProperty(
                        propertyName,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.IgnoreCase);


                if (
                    property == null ||
                    !property.CanRead
                )
                {
                    continue;
                }


                return property.GetValue(
                    source);
            }


            return null;
        }


        private static string?
            GetStringProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            var text =
                value.ToString();


            return string.IsNullOrWhiteSpace(
                text)
                ? null
                : text.Trim();
        }


        private static int?
            GetIntProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            try
            {
                return Convert.ToInt32(
                    value);
            }
            catch
            {
                return null;
            }
        }


        private static decimal?
            GetDecimalProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            try
            {
                return Convert.ToDecimal(
                    value);
            }
            catch
            {
                return null;
            }
        }


        private static DateTime?
            GetDateProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            try
            {
                return Convert.ToDateTime(
                    value);
            }
            catch
            {
                return null;
            }
        }


        private static string?
            FirstNonEmpty(
                params string?[] values)
        {
            return values
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(
                        x));
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
                return new Dictionary<
                    string,
                    JsonElement>();
            }


            try
            {
                return JsonSerializer
                    .Deserialize<
                        Dictionary<
                            string,
                            JsonElement>>(
                                snapshotJson)

                    ?? new Dictionary<
                        string,
                        JsonElement>();
            }
            catch (JsonException)
            {
                return new Dictionary<
                    string,
                    JsonElement>();
            }
        }


        private static string?
            GetSnapshotString(
                Dictionary<string, JsonElement> snapshot,
                string propertyName)
        {
            if (!snapshot.TryGetValue(
                propertyName,
                out var value))
            {
                return null;
            }


            if (value.ValueKind ==
                JsonValueKind.Null)
            {
                return null;
            }


            if (value.ValueKind ==
                JsonValueKind.String)
            {
                return value.GetString();
            }


            return value.ToString();
        }

        #endregion
    }
}