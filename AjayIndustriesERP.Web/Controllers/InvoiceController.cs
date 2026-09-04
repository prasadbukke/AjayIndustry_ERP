/*
============================================================
File: InvoiceController.cs

Module:
Invoice

Purpose:
Handles Web requests for Invoice module.

Current Invoice Source:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Invoice Item

Invoice Source Identity:

ProductionJobId
        +
CustomerPurchaseOrderItemId

Responsibilities:
- List and search Invoices.
- Create Invoice from Customer Purchase Order.
- Load completed Production Items for selected Customer PO.
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
- One Production Job may contain multiple Production Items.
- Invoice quantity availability is Item-wise.
- ProductionJobId alone is NOT an Invoice line identity.
- CompletedQuantity is the trusted invoiceable quantity.
- Current Production plan must be complete.
- Delivery Challan is NOT mandatory.
- PDI is NOT mandatory.
- Missing PDI / Delivery Challan is warning-only.
- Browser-posted source snapshot data is NOT trusted.
- Financial calculations posted by browser are NOT trusted.
- InvoiceService recalculates authoritative values.

Temporary Compatibility:
- InvoiceFormViewModel.AvailableDeliveryChallans is currently
  reused to carry Customer PO SelectListItems.
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
                customerPurchaseOrderId.HasValue
                &&
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


            await PopulateAvailableCustomerPurchaseOrdersAsync(
                viewModel);


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
            if (!ModelState.IsValid)
            {
                await RefreshFormSnapshotsAsync(
                    viewModel);


                await PopulateAvailableCustomerPurchaseOrdersAsync(
                    viewModel);


                return View(
                    viewModel);
            }


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


        #region Customer PO AJAX

        [HttpGet]
        public async Task<IActionResult>
            GetCustomerPurchaseOrderData(
                int id,
                int? invoiceId = null)
        {
            try
            {
                #region Customer PO

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


                #region Snapshot Source

                var shouldRefreshMasterData =
                    !invoiceId.HasValue
                    ||
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


                #region Production Jobs

                var productionJobs =
                    await _invoiceService
                        .GetCompletedProductionJobsForInvoiceAsync(
                            id);


                /*
                 * During Edit, current Draft already reserves
                 * quantity.
                 *
                 * Therefore its Production Jobs may not appear
                 * in normal availability result.
                 *
                 * Add them back and use excludeInvoiceId when
                 * calculating Item-wise quantity.
                 */
                if (
                    invoiceId.HasValue
                    &&
                    invoiceId.Value > 0
                )
                {
                    var currentInvoice =
                        await _invoiceService
                            .GetByIdAsync(
                                invoiceId.Value);


                    if (currentInvoice != null)
                    {
                        var existingJobIds =
                            currentInvoice
                                .Items
                                .Where(x =>
                                    !x.IsDeleted
                                    &&
                                    x.IsActive
                                    &&
                                    x.ProductionJobId.HasValue
                                    &&
                                    x.ProductionJobId.Value > 0)
                                .Select(x =>
                                    x.ProductionJobId!.Value)
                                .Distinct()
                                .ToList();


                        foreach (var productionJobId
                            in existingJobIds)
                        {
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


                            if (productionJob.CustomerPurchaseOrderId !=
                                id)
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


                #region Production Items

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
                    var completedProductionItems =
                        GetCompletedProductionItems(
                            productionJob);


                    foreach (var productionJobItem
                        in completedProductionItems
                            .OrderBy(x =>
                                x.Id))
                    {
                        var customerPurchaseOrderItemId =
                            productionJobItem
                                .CustomerPurchaseOrderItemId;


                        var availableQuantity =
                            await _invoiceService
                                .GetRemainingInvoiceQuantityAsync(
                                    productionJob.Id,
                                    customerPurchaseOrderItemId,
                                    invoiceId);


                        if (availableQuantity <= 0m)
                        {
                            continue;
                        }


                        /*
                         * CompletedQuantity is the actual
                         * invoiceable Production quantity.
                         */
                        var productionQuantity =
                            productionJobItem
                                .CompletedQuantity;


                        var alreadyInvoicedQuantity =
                            productionQuantity
                            -
                            availableQuantity;


                        if (alreadyInvoicedQuantity < 0m)
                        {
                            alreadyInvoicedQuantity =
                                0m;
                        }


                        var poItem =
                            GetPropertyValue(
                                productionJobItem,
                                "CustomerPurchaseOrderItem");


                        var productionMasterItem =
                            GetPropertyValue(
                                productionJobItem,
                                "Item");


                        var poMasterItem =
                            GetPropertyValue(
                                poItem,
                                "Item");


                        var itemId =
                            productionJobItem.ItemId;


                        var itemCode =
                            FirstNonEmpty(
                                productionJobItem.ItemCode,

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
                                productionJobItem.ItemName,

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
                                    productionJobItem,
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
                                    poItem,
                                    "CustomerItemCode"),

                                GetStringProperty(
                                    productionJobItem,
                                    "CustomerItemCode"));


                        var unitName =
                            FirstNonEmpty(
                                productionJobItem.UnitName,

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
                                    productionJobItem,
                                    "HsnNumber",
                                    "HSNNumber",
                                    "HsnCode",
                                    "HSNCode"),

                                GetStringProperty(
                                    poItem,
                                    "HsnNumber",
                                    "HSNNumber",
                                    "HsnCode",
                                    "HSNCode"),

                                GetStringProperty(
                                    productionMasterItem,
                                    "HsnNumber",
                                    "HSNNumber",
                                    "HsnCode",
                                    "HSNCode"),

                                GetStringProperty(
                                    poMasterItem,
                                    "HsnNumber",
                                    "HSNNumber",
                                    "HsnCode",
                                    "HSNCode"));


                        var productReference =
                            FirstNonEmpty(
                                GetStringProperty(
                                    productionJobItem,
                                    "ProductReference",
                                    "ProductRef"),

                                GetStringProperty(
                                    poItem,
                                    "ProductReference",
                                    "ProductRef"),

                                GetStringProperty(
                                    productionMasterItem,
                                    "ProductReference",
                                    "ProductRef"),

                                GetStringProperty(
                                    poMasterItem,
                                    "ProductReference",
                                    "ProductRef"));


                        itemResults.Add(
                            new
                            {
                                // =============================
                                // Production Source
                                // =============================

                                productionJobId =
                                    productionJob.Id,

                                productionJobCode =
                                    GetProductionJobCode(
                                        productionJob),

                                productionJobItemId =
                                    productionJobItem.Id,

                                productionQuantity,

                                alreadyInvoicedQuantity,

                                availableQuantity,

                                requiresWarning =
                                    warningSet.Contains(
                                        productionJob.Id),


                                // =============================
                                // Item
                                // =============================

                                productReference,

                                itemId,

                                itemCode,

                                itemName,

                                partNumber,

                                customerItemCode,

                                unitName,

                                hsnNumber,


                                // =============================
                                // Customer PO Item
                                // =============================

                                customerPurchaseOrderItemId,

                                customerPurchaseOrderCode =
                                    GetCustomerPurchaseOrderCode(
                                        customerPurchaseOrder),

                                customerPurchaseOrderNumber =
                                    GetCustomerPurchaseOrderNumber(
                                        customerPurchaseOrder)
                            });
                    }
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


                #region Customer

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


                #region JSON Response

                return Json(
                    new
                    {
                        success = true,


                        // =====================================
                        // Customer PO
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
                        // Bank
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
                        // Invoice Lines
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


            var requiresSourceWarning =
                false;


            if (invoice.Status ==
                InvoiceStatus.Draft)
            {
                var productionJobIds =
                    invoice.Items
                        .Where(x =>
                            !x.IsDeleted
                            &&
                            x.IsActive
                            &&
                            x.ProductionJobId.HasValue
                            &&
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
                        warningProductionJobIds
                            .Any();
                }
            }


            ViewBag.RequiresSourceWarning =
                requiresSourceWarning;


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
            var customerPurchaseOrders =
                await _invoiceService
                    .GetCustomerPurchaseOrdersForInvoiceAsync();


            /*
             * Determine Customer PO currently selected
             * through existing Invoice lines.
             */
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
                    !productionJobId.HasValue
                    ||
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


                if (productionJob.CustomerPurchaseOrderId > 0)
                {
                    selectedCustomerPurchaseOrderIds.Add(
                        productionJob
                            .CustomerPurchaseOrderId);
                }
            }


            /*
             * During Edit, selected PO might not be present
             * in normal available list because this Draft
             * itself reserves its quantity.
             */
            foreach (var selectedId
                in selectedCustomerPurchaseOrderIds)
            {
                if (customerPurchaseOrders.Any(x =>
                    x.Id ==
                        selectedId))
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


            var options =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            "",

                        Text =
                            "-- Select Customer Purchase Order --"
                    }
                };


            foreach (var customerPurchaseOrder
                in customerPurchaseOrders
                    .GroupBy(x =>
                        x.Id)
                    .Select(x =>
                        x.First())
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
                var productionJobs =
                    await _invoiceService
                        .GetCompletedProductionJobsForInvoiceAsync(
                            customerPurchaseOrder.Id);


                /*
                 * During Edit add current Invoice Production
                 * Jobs back if required.
                 */
                if (selectedCustomerPurchaseOrderIds.Contains(
                    customerPurchaseOrder.Id))
                {
                    foreach (var formItem
                        in viewModel.Items)
                    {
                        var productionJobId =
                            GetIntProperty(
                                formItem,
                                "ProductionJobId");


                        if (
                            !productionJobId.HasValue
                            ||
                            productionJobId.Value <= 0
                        )
                        {
                            continue;
                        }


                        if (productionJobs.Any(x =>
                            x.Id ==
                                productionJobId.Value))
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


                        if (productionJob.CustomerPurchaseOrderId !=
                            customerPurchaseOrder.Id)
                        {
                            continue;
                        }


                        productionJobs.Add(
                            productionJob);
                    }
                }


                decimal totalAvailable =
                    0m;


                foreach (var productionJob
                    in productionJobs
                        .GroupBy(x =>
                            x.Id)
                        .Select(x =>
                            x.First()))
                {
                    var productionItems =
                        GetCompletedProductionItems(
                            productionJob);


                    foreach (var productionJobItem
                        in productionItems)
                    {
                        try
                        {
                            totalAvailable +=
                                await _invoiceService
                                    .GetRemainingInvoiceQuantityAsync(
                                        productionJob.Id,
                                        productionJobItem
                                            .CustomerPurchaseOrderItemId,
                                        excludeInvoiceId);
                        }
                        catch (BusinessException)
                        {
                            /*
                             * Skip invalid/unavailable source.
                             */
                        }
                    }
                }


                if (
                    totalAvailable <= 0m
                    &&
                    !selectedCustomerPurchaseOrderIds.Contains(
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
                            customerPurchaseOrder
                                .Id
                                .ToString(),

                        Text =
                            $"{poDisplay}"
                            +
                            (
                                string.IsNullOrWhiteSpace(
                                    customerName)
                                    ? string.Empty
                                    : $" | {customerName}"
                            )
                            +
                            $" | Available: {totalAvailable:0.###}",

                        Selected =
                            selectedCustomerPurchaseOrderIds.Contains(
                                customerPurchaseOrder.Id)
                    });
            }


            /*
             * Temporary old property name.
             * Currently stores Customer PO options.
             */
            viewModel.AvailableDeliveryChallans =
                options;
        }

        #endregion


        #region Refresh Form Snapshots

        private async Task RefreshFormSnapshotsAsync(
            InvoiceFormViewModel viewModel,
            int? excludeInvoiceId = null)
        {
            #region Header Snapshots

            if (
                viewModel.Id > 0
                &&
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
                            x.HasValue
                            &&
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
                                productionJob
                                    .CustomerPurchaseOrderId;


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
                        /*
                         * Keep submitted form values.
                         */
                    }
                }
            }

            #endregion


            #region Refresh Item Quantities

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


                var customerPurchaseOrderItemId =
                    GetIntProperty(
                        item,
                        "CustomerPurchaseOrderItemId");


                if (
                    !productionJobId.HasValue
                    ||
                    productionJobId.Value <= 0
                    ||
                    !customerPurchaseOrderItemId.HasValue
                    ||
                    customerPurchaseOrderItemId.Value <= 0
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


                    var productionJobItem =
                        FindProductionJobItem(
                            productionJob,
                            customerPurchaseOrderItemId.Value);


                    if (productionJobItem == null)
                    {
                        continue;
                    }


                    var availableQuantity =
                        await _invoiceService
                            .GetRemainingInvoiceQuantityAsync(
                                productionJobId.Value,
                                customerPurchaseOrderItemId.Value,
                                excludeInvoiceId);


                    var productionQuantity =
                        productionJobItem
                            .CompletedQuantity;


                    var alreadyInvoicedQuantity =
                        productionQuantity
                        -
                        availableQuantity;


                    if (alreadyInvoicedQuantity < 0m)
                    {
                        alreadyInvoicedQuantity =
                            0m;
                    }


                    item.ProductionQuantity =
                        productionQuantity;

                    item.AvailableQuantity =
                        availableQuantity;

                    item.AlreadyInvoicedQuantity =
                        alreadyInvoicedQuantity;
                }
                catch (BusinessException)
                {
                    /*
                     * Keep submitted line visible.
                     */
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
                        viewModel
                            .InvoiceTermsAndConditions,

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
                         * CRITICAL:
                         *
                         * Both values form the trusted
                         * Production Item source identity.
                         */
                        ProductionJobId =
                            item.ProductionJobId,

                        CustomerPurchaseOrderItemId =
                            item.CustomerPurchaseOrderItemId,


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


            ApplyCustomerSnapshotToForm(
                viewModel,
                invoice.CustomerSnapshotJson);


            ApplyCompanySnapshotToForm(
                viewModel,
                invoice.CompanySnapshotJson);


            var activeItems =
                invoice.Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            #region Warning Status

            var productionJobIds =
                activeItems
                    .Where(x =>
                        x.ProductionJobId.HasValue
                        &&
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


                var customerPurchaseOrderItemId =
                    item.CustomerPurchaseOrderItemId
                        .GetValueOrDefault();


                var productionQuantity =
                    item.InvoiceQuantity;


                var availableQuantity =
                    item.InvoiceQuantity;


                if (
                    productionJobId > 0
                    &&
                    customerPurchaseOrderItemId > 0
                )
                {
                    try
                    {
                        availableQuantity =
                            await _invoiceService
                                .GetRemainingInvoiceQuantityAsync(
                                    productionJobId,
                                    customerPurchaseOrderItemId,
                                    excludeInvoiceId);


                        var productionJob =
                            await _invoiceService
                                .GetCompletedProductionJobForInvoiceAsync(
                                    productionJobId);


                        if (productionJob != null)
                        {
                            var productionJobItem =
                                FindProductionJobItem(
                                    productionJob,
                                    customerPurchaseOrderItemId);


                            if (productionJobItem != null)
                            {
                                productionQuantity =
                                    productionJobItem
                                        .CompletedQuantity;
                            }


                            if (
                                !viewModel
                                    .CustomerPurchaseOrderId
                                    .HasValue
                                ||
                                viewModel
                                    .CustomerPurchaseOrderId
                                    .Value <= 0
                            )
                            {
                                if (
                                    productionJob
                                        .CustomerPurchaseOrderId
                                    > 0
                                )
                                {
                                    viewModel
                                        .CustomerPurchaseOrderId =
                                            productionJob
                                                .CustomerPurchaseOrderId;
                                }
                            }
                        }
                    }
                    catch (BusinessException)
                    {
                        /*
                         * Historical Invoice line must remain
                         * visible even when Production source
                         * later becomes unavailable.
                         */
                    }
                }


                var alreadyInvoicedQuantity =
                    productionQuantity
                    -
                    availableQuantity;


                if (alreadyInvoicedQuantity < 0m)
                {
                    alreadyInvoicedQuantity =
                        0m;
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
                            productionJobId > 0
                            &&
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
                        // Customer PO Item
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
                        // Commercial
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


                    #region Payment GST

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


            foreach (var item
                in invoice.Items
                    .Where(x =>
                        !x.IsDeleted
                        &&
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


            return viewModel;
        }

        #endregion


        #region Production Item Helpers

        private static List<ProductionJobItem>
            GetCompletedProductionItems(
                ProductionJob productionJob)
        {
            if (productionJob.Items == null)
            {
                return new List<ProductionJobItem>();
            }


            return productionJob
                .Items
                .Where(x =>
                    !x.IsDeleted
                    &&
                    x.IsActive
                    &&
                    x.ProductionQuantity > 0m
                    &&
                    x.CompletedQuantity > 0m
                    &&
                    x.CompletedQuantity >=
                        x.ProductionQuantity)
                .ToList();
        }


        private static ProductionJobItem?
            FindProductionJobItem(
                ProductionJob productionJob,
                int customerPurchaseOrderItemId)
        {
            if (productionJob.Items == null)
            {
                return null;
            }


            return productionJob
                .Items
                .FirstOrDefault(x =>
                    !x.IsDeleted
                    &&
                    x.IsActive
                    &&
                    x.CustomerPurchaseOrderItemId ==
                        customerPurchaseOrderItemId);
        }


        private static string GetProductionJobCode(
            ProductionJob productionJob)
        {
            return string.IsNullOrWhiteSpace(
                productionJob.Code)
                ? productionJob.Id.ToString()
                : productionJob.Code;
        }

        #endregion


        #region Customer PO Helpers

        private static string?
            GetCustomerPurchaseOrderCode(
                CustomerPurchaseOrder
                    customerPurchaseOrder)
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
                CustomerPurchaseOrder
                    customerPurchaseOrder)
        {
            return GetStringProperty(
                customerPurchaseOrder,
                "CustomerPurchaseOrderNumber",
                "PurchaseOrderNumber",
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
                    property == null
                    ||
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