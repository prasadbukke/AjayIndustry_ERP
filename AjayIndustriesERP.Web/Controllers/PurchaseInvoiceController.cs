using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.PurchaseInvoice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace AjayIndustriesERP.Web.Controllers
{
    public class PurchaseInvoiceController : Controller
    {
        // =====================================================
        // CONSTANTS
        // =====================================================

        #region Constants

        /*
         * Supplier Invoice PDF:
         * Maximum allowed size = 10 MB.
         */
        private const long MaxSupplierInvoicePdfSize =
            10 * 1024 * 1024;


        private const string PurchaseInvoiceUploadFolder =
            "uploads/purchase-invoices";

        #endregion


        // =====================================================
        // FIELDS
        // =====================================================

        #region Fields

        private readonly IPurchaseInvoiceService
            _service;


        private readonly IWebHostEnvironment
            _webHostEnvironment;

        #endregion


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        #region Constructor

        public PurchaseInvoiceController(
            IPurchaseInvoiceService service,
            IWebHostEnvironment webHostEnvironment)
        {
            _service =
                service;


            _webHostEnvironment =
                webHostEnvironment;
        }

        #endregion


        // =====================================================
        // INDEX / SEARCH / FILTER
        // =====================================================

        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            DateTime? purchaseInvoiceDate,
            DateTime? supplierInvoiceDate,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // ---------------------------------------------
            // Preserve filters for Index View.
            // ---------------------------------------------

            ViewBag.SearchText =
                searchText;


            ViewBag.PurchaseInvoiceDate =
                purchaseInvoiceDate?
                    .ToString(
                        "yyyy-MM-dd");


            ViewBag.SupplierInvoiceDate =
                supplierInvoiceDate?
                    .ToString(
                        "yyyy-MM-dd");


            ViewBag.PageSize =
                pageSize;


            // ---------------------------------------------
            // Determine whether any filter is active.
            // ---------------------------------------------

            var hasFilters =
                !string.IsNullOrWhiteSpace(
                    searchText)
                ||
                purchaseInvoiceDate.HasValue
                ||
                supplierInvoiceDate.HasValue;


            var result =
                hasFilters

                    ? await _service
                        .SearchPagedAsync(
                            searchText,
                            purchaseInvoiceDate,
                            supplierInvoiceDate,
                            pageNumber,
                            pageSize)

                    : await _service
                        .GetPagedAsync(
                            pageNumber,
                            pageSize);


            return View(
                result);
        }

        #endregion


        // =====================================================
        // CREATE - GET
        // =====================================================

        #region Create GET

        [HttpGet]
        public async Task<IActionResult> Create(
            int? purchaseOrderId)
        {
            try
            {
                PurchaseInvoiceFormViewModel
                    viewModel;


                if (
                    purchaseOrderId.HasValue &&
                    purchaseOrderId.Value > 0
                )
                {
                    var prepared =
                        await _service
                            .PrepareDraftAsync(
                                purchaseOrderId.Value);


                    viewModel =
                        await MapPreparedInvoiceToFormAsync(
                            prepared);
                }
                else
                {
                    viewModel =
                        new PurchaseInvoiceFormViewModel
                        {
                            PurchaseInvoiceDate =
                                DateTime.Today,

                            SupplierInvoiceDate =
                                DateTime.Today
                        };
                }


                await PopulatePurchaseOrdersAsync(
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
        }

        #endregion


        // =====================================================
        // CREATE - POST
        // =====================================================

        #region Create POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PurchaseInvoiceFormViewModel viewModel)
        {
            string? newlyUploadedPdfPath =
                null;


            try
            {
                if (!ModelState.IsValid)
                {
                    await RehydrateFormAsync(
                        viewModel,
                        excludePurchaseInvoiceId:
                            null);


                    return View(
                        viewModel);
                }


                // ---------------------------------------------
                // Build transaction input.
                //
                // Service reloads trusted PO / GRN data.
                // ---------------------------------------------

                var purchaseInvoice =
                    BuildSubmittedPurchaseInvoice(
                        viewModel);


                // ---------------------------------------------
                // Optional Supplier Invoice PDF.
                // ---------------------------------------------

                if (viewModel.SupplierInvoicePdf != null)
                {
                    var savedPdf =
                        await SaveSupplierInvoicePdfAsync(
                            viewModel.SupplierInvoicePdf);


                    newlyUploadedPdfPath =
                        savedPdf.RelativePath;


                    ApplyPdfInformation(
                        purchaseInvoice,
                        savedPdf);
                }


                // ---------------------------------------------
                // Create Draft Purchase Invoice.
                // ---------------------------------------------

                var created =
                    await _service
                        .CreateAsync(
                            purchaseInvoice);


                TempData["SuccessMessage"] =
                    $"Purchase Invoice {created.Code} created successfully.";


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
                /*
                 * Remove only newly uploaded file if
                 * Purchase Invoice creation failed.
                 */
                DeleteFileIfExists(
                    newlyUploadedPdfPath);


                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);


                await RehydrateFormAsync(
                    viewModel,
                    excludePurchaseInvoiceId:
                        null);


                return View(
                    viewModel);
            }
            catch (IOException)
            {
                DeleteFileIfExists(
                    newlyUploadedPdfPath);


                ModelState.AddModelError(
                    string.Empty,
                    "Supplier Invoice PDF could not be saved. Please try again.");


                await RehydrateFormAsync(
                    viewModel,
                    excludePurchaseInvoiceId:
                        null);


                return View(
                    viewModel);
            }
        }

        #endregion


        // =====================================================
        // EDIT - GET
        // =====================================================

        #region Edit GET

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var purchaseInvoice =
                await _service
                    .GetByIdAsync(
                        id);


            if (purchaseInvoice == null)
            {
                return NotFound();
            }


            /*
             * Financial Edit remains Draft-only.
             *
             * Finalized Purchase Invoice:
             * - Details allowed
             * - Delete allowed
             * - Edit disabled
             */
            if (purchaseInvoice.Status !=
                PurchaseInvoiceStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only Draft Purchase Invoice can be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            var viewModel =
                await MapExistingInvoiceToFormAsync(
                    purchaseInvoice);


            await PopulatePurchaseOrdersAsync(
                viewModel);


            return View(
                viewModel);
        }

        #endregion


        // =====================================================
        // EDIT - POST
        // =====================================================

        #region Edit POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PurchaseInvoiceFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            string? newlyUploadedPdfPath =
                null;


            string? oldPdfPath =
                null;


            try
            {
                // ---------------------------------------------
                // Reload existing Purchase Invoice.
                //
                // Browser-posted existing PDF metadata
                // is intentionally not trusted.
                // ---------------------------------------------

                var existing =
                    await _service
                        .GetByIdAsync(
                            id);


                if (existing == null)
                {
                    return NotFound();
                }


                oldPdfPath =
                    existing.SupplierInvoicePdfPath;


                if (!ModelState.IsValid)
                {
                    await RehydrateFormAsync(
                        viewModel,
                        excludePurchaseInvoiceId:
                            id);


                    return View(
                        viewModel);
                }


                var purchaseInvoice =
                    BuildSubmittedPurchaseInvoice(
                        viewModel);


                purchaseInvoice.Id =
                    id;


                // ---------------------------------------------
                // Preserve existing PDF by default.
                // ---------------------------------------------

                CopyExistingPdfInformation(
                    purchaseInvoice,
                    existing);


                // ---------------------------------------------
                // Replace PDF only when new file is selected.
                // ---------------------------------------------

                if (viewModel.SupplierInvoicePdf != null)
                {
                    var savedPdf =
                        await SaveSupplierInvoicePdfAsync(
                            viewModel.SupplierInvoicePdf);


                    newlyUploadedPdfPath =
                        savedPdf.RelativePath;


                    ApplyPdfInformation(
                        purchaseInvoice,
                        savedPdf);
                }


                var updated =
                    await _service
                        .UpdateAsync(
                            purchaseInvoice);


                // ---------------------------------------------
                // Database now points to new PDF.
                // Old physical PDF can safely be removed.
                // ---------------------------------------------

                if (
                    !string.IsNullOrWhiteSpace(
                        newlyUploadedPdfPath)
                    &&
                    !string.IsNullOrWhiteSpace(
                        oldPdfPath)
                    &&
                    !string.Equals(
                        newlyUploadedPdfPath,
                        oldPdfPath,
                        StringComparison.OrdinalIgnoreCase)
                )
                {
                    DeleteFileIfExists(
                        oldPdfPath);
                }


                TempData["SuccessMessage"] =
                    $"Purchase Invoice {updated.Code} updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            updated.Id
                    });
            }
            catch (BusinessException ex)
            {
                /*
                 * Keep old PDF.
                 * Remove only newly uploaded replacement.
                 */
                DeleteFileIfExists(
                    newlyUploadedPdfPath);


                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);


                await RehydrateFormAsync(
                    viewModel,
                    excludePurchaseInvoiceId:
                        id);


                return View(
                    viewModel);
            }
            catch (IOException)
            {
                DeleteFileIfExists(
                    newlyUploadedPdfPath);


                ModelState.AddModelError(
                    string.Empty,
                    "Supplier Invoice PDF could not be saved. Please try again.");


                await RehydrateFormAsync(
                    viewModel,
                    excludePurchaseInvoiceId:
                        id);


                return View(
                    viewModel);
            }
        }

        #endregion


        // =====================================================
        // DETAILS
        // =====================================================

        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var purchaseInvoice =
                await _service
                    .GetByIdAsync(
                        id);


            if (purchaseInvoice == null)
            {
                return NotFound();
            }


            var viewModel =
                MapDetails(
                    purchaseInvoice);


            return View(
                viewModel);
        }

        #endregion


        // =====================================================
        // VIEW SUPPLIER INVOICE PDF
        // =====================================================

        #region View Supplier Invoice PDF

        [HttpGet]
        public async Task<IActionResult>
            ViewSupplierInvoicePdf(
                int id)
        {
            var purchaseInvoice =
                await _service
                    .GetByIdAsync(
                        id);


            if (purchaseInvoice == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                purchaseInvoice.SupplierInvoicePdfPath))
            {
                return NotFound();
            }


            var physicalPath =
                GetSafePdfPhysicalPath(
                    purchaseInvoice
                        .SupplierInvoicePdfPath);


            if (
                string.IsNullOrWhiteSpace(
                    physicalPath)
                ||
                !System.IO.File.Exists(
                    physicalPath)
            )
            {
                return NotFound();
            }


            return new PhysicalFileResult(
                physicalPath,
                "application/pdf")
            {
                EnableRangeProcessing =
                    true
            };
        }

        #endregion


        // =====================================================
        // DOWNLOAD SUPPLIER INVOICE PDF
        // =====================================================

        #region Download Supplier Invoice PDF

        [HttpGet]
        public async Task<IActionResult>
            DownloadSupplierInvoicePdf(
                int id)
        {
            var purchaseInvoice =
                await _service
                    .GetByIdAsync(
                        id);


            if (purchaseInvoice == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                purchaseInvoice.SupplierInvoicePdfPath))
            {
                return NotFound();
            }


            var physicalPath =
                GetSafePdfPhysicalPath(
                    purchaseInvoice
                        .SupplierInvoicePdfPath);


            if (
                string.IsNullOrWhiteSpace(
                    physicalPath)
                ||
                !System.IO.File.Exists(
                    physicalPath)
            )
            {
                return NotFound();
            }


            var downloadName =
                !string.IsNullOrWhiteSpace(
                    purchaseInvoice
                        .SupplierInvoicePdfOriginalName)

                    ? purchaseInvoice
                        .SupplierInvoicePdfOriginalName

                    : $"{purchaseInvoice.Code.Replace("/", "-")}.pdf";


            return new PhysicalFileResult(
                physicalPath,
                "application/pdf")
            {
                FileDownloadName =
                    downloadName,

                EnableRangeProcessing =
                    true
            };
        }

        #endregion


        // =====================================================
        // APPROVE / FINALIZE
        // =====================================================

        #region Finalize / Approve

        /*
         * UI label:
         *      Approve
         *
         * Internal status:
         *      Finalized
         *
         * We intentionally do NOT add another
         * Approved enum/status.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(
            int id)
        {
            try
            {
                var finalized =
                    await _service
                        .FinalizeAsync(
                            id);


                TempData["SuccessMessage"] =
                    $"Purchase Invoice {finalized.Code} approved successfully.";
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


        // =====================================================
        // DELETE
        // =====================================================

        #region Delete

        /*
         * Delete is ALWAYS available for active:
         *
         * - Draft
         * - Finalized
         *
         * Service performs soft delete.
         *
         * Supplier Invoice PDF is NOT physically deleted.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _service
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Purchase Invoice deleted successfully.";
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


        // =====================================================
        // DELETED LIST
        // =====================================================

        #region Deleted

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var purchaseInvoices =
                await _service
                    .GetDeletedAsync();


            return View(
                purchaseInvoices);
        }

        #endregion


        // =====================================================
        // RESTORE
        // =====================================================

        #region Restore

        /*
         * Restore supports both:
         *
         * - Deleted Draft
         * - Deleted Finalized
         *
         * Service preserves original status and
         * revalidates Supplier Invoice Number + GRN Qty.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _service
                    .RestoreAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Purchase Invoice restored successfully.";
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


        // =====================================================
        // PREPARED CREATE FORM MAPPING
        // =====================================================

        #region Map Prepared Invoice To Form

        private async Task<PurchaseInvoiceFormViewModel>
            MapPreparedInvoiceToFormAsync(
                PurchaseInvoice purchaseInvoice)
        {
            var supplierSnapshot =
                ParseSnapshot(
                    purchaseInvoice
                        .SupplierSnapshotJson);


            var companySnapshot =
                ParseSnapshot(
                    purchaseInvoice
                        .CompanySnapshotJson);


            var sourceItems =
                await _service
                    .GetAvailableGoodsReceiptItemsAsync(
                        purchaseInvoice
                            .PurchaseOrderId);


            var sourceMap =
                sourceItems
                    .ToDictionary(
                        x =>
                            x.Id);


            var viewModel =
                new PurchaseInvoiceFormViewModel
                {
                    Id =
                        purchaseInvoice.Id,

                    Code =
                        purchaseInvoice.Code,


                    PurchaseOrderId =
                        purchaseInvoice
                            .PurchaseOrderId,

                    PurchaseOrderCode =
                        purchaseInvoice
                            .PurchaseOrderCode,


                    PurchaseInvoiceDate =
                        purchaseInvoice
                            .PurchaseInvoiceDate,

                    SupplierInvoiceDate =
                        purchaseInvoice
                            .SupplierInvoiceDate,

                    SupplierInvoiceNumber =
                        purchaseInvoice
                            .SupplierInvoiceNumber,


                    SupplierId =
                        purchaseInvoice.SupplierId,

                    SupplierName =
                        purchaseInvoice.SupplierName,

                    SupplierCode =
                        GetSnapshotString(
                            supplierSnapshot,
                            "SupplierCode"),

                    SupplierGstin =
                        GetSnapshotString(
                            supplierSnapshot,
                            "Gstin"),

                    SupplierState =
                        GetSnapshotString(
                            supplierSnapshot,
                            "State"),

                    SupplierAddress =
                        BuildSupplierAddress(
                            supplierSnapshot),


                    CompanyId =
                        purchaseInvoice.CompanyId,

                    CompanyName =
                        purchaseInvoice.CompanyName,

                    CompanyGstin =
                        GetSnapshotString(
                            companySnapshot,
                            "GstNumber"),

                    CompanyState =
                        GetSnapshotString(
                            companySnapshot,
                            "State"),


                    PaymentTerms =
                        purchaseInvoice.PaymentTerms,

                    CreditDays =
                        purchaseInvoice.CreditDays,

                    DueDate =
                        purchaseInvoice.DueDate,


                    PlaceOfSupply =
                        purchaseInvoice.PlaceOfSupply,

                    IsInterState =
                        purchaseInvoice.IsInterState,


                    TransportCharges =
                        purchaseInvoice
                            .TransportCharges,

                    OtherCharges =
                        purchaseInvoice
                            .OtherCharges,

                    RoundOffAmount =
                        purchaseInvoice
                            .RoundOffAmount,


                    GrossAmount =
                        purchaseInvoice.GrossAmount,

                    DiscountAmount =
                        purchaseInvoice.DiscountAmount,

                    TaxableAmount =
                        purchaseInvoice.TaxableAmount,

                    CgstAmount =
                        purchaseInvoice.CgstAmount,

                    SgstAmount =
                        purchaseInvoice.SgstAmount,

                    IgstAmount =
                        purchaseInvoice.IgstAmount,

                    GrandTotal =
                        purchaseInvoice.GrandTotal,


                    Remarks =
                        purchaseInvoice.Remarks,

                    Status =
                        purchaseInvoice.Status
                            .ToString()
                };


            foreach (var item
                in purchaseInvoice.Items
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                sourceMap.TryGetValue(
                    item.GoodsReceiptNoteItemId,
                    out var sourceItem);


                var availableQuantity =
                    item.PurchaseInvoiceQuantity;


                var alreadyBilled =
                    item.GoodsReceiptQuantity -
                    availableQuantity;


                if (alreadyBilled < 0m)
                {
                    alreadyBilled =
                        0m;
                }


                viewModel.Items.Add(
                    MapFormItem(
                        item,
                        sourceItem,
                        isSelected:
                            true,
                        alreadyBilledQuantity:
                            alreadyBilled,
                        availableQuantity:
                            availableQuantity,
                        purchaseInvoiceQuantity:
                            item.PurchaseInvoiceQuantity));
            }


            return viewModel;
        }

        #endregion


        // =====================================================
        // EXISTING EDIT FORM MAPPING
        // =====================================================

        #region Map Existing Invoice To Form

        private async Task<PurchaseInvoiceFormViewModel>
            MapExistingInvoiceToFormAsync(
                PurchaseInvoice purchaseInvoice)
        {
            var supplierSnapshot =
                ParseSnapshot(
                    purchaseInvoice
                        .SupplierSnapshotJson);


            var companySnapshot =
                ParseSnapshot(
                    purchaseInvoice
                        .CompanySnapshotJson);


            var viewModel =
                new PurchaseInvoiceFormViewModel
                {
                    Id =
                        purchaseInvoice.Id,

                    Code =
                        purchaseInvoice.Code,


                    PurchaseOrderId =
                        purchaseInvoice
                            .PurchaseOrderId,

                    PurchaseOrderCode =
                        purchaseInvoice
                            .PurchaseOrderCode,


                    PurchaseInvoiceDate =
                        purchaseInvoice
                            .PurchaseInvoiceDate,

                    SupplierInvoiceNumber =
                        purchaseInvoice
                            .SupplierInvoiceNumber,

                    SupplierInvoiceDate =
                        purchaseInvoice
                            .SupplierInvoiceDate,


                    // -----------------------------------------
                    // Existing Supplier Invoice PDF
                    // -----------------------------------------

                    ExistingSupplierInvoicePdfPath =
                        purchaseInvoice
                            .SupplierInvoicePdfPath,

                    ExistingSupplierInvoicePdfOriginalName =
                        purchaseInvoice
                            .SupplierInvoicePdfOriginalName,

                    ExistingSupplierInvoicePdfUploadedOn =
                        purchaseInvoice
                            .SupplierInvoicePdfUploadedOn,


                    SupplierId =
                        purchaseInvoice.SupplierId,

                    SupplierName =
                        purchaseInvoice.SupplierName,

                    SupplierCode =
                        GetSnapshotString(
                            supplierSnapshot,
                            "SupplierCode"),

                    SupplierGstin =
                        GetSnapshotString(
                            supplierSnapshot,
                            "Gstin"),

                    SupplierState =
                        GetSnapshotString(
                            supplierSnapshot,
                            "State"),

                    SupplierAddress =
                        BuildSupplierAddress(
                            supplierSnapshot),


                    CompanyId =
                        purchaseInvoice.CompanyId,

                    CompanyName =
                        purchaseInvoice.CompanyName,

                    CompanyGstin =
                        GetSnapshotString(
                            companySnapshot,
                            "GstNumber"),

                    CompanyState =
                        GetSnapshotString(
                            companySnapshot,
                            "State"),


                    PaymentTerms =
                        purchaseInvoice.PaymentTerms,

                    CreditDays =
                        purchaseInvoice.CreditDays,

                    DueDate =
                        purchaseInvoice.DueDate,


                    PlaceOfSupply =
                        purchaseInvoice.PlaceOfSupply,

                    IsInterState =
                        purchaseInvoice.IsInterState,


                    TransportCharges =
                        purchaseInvoice
                            .TransportCharges,

                    OtherCharges =
                        purchaseInvoice
                            .OtherCharges,

                    RoundOffAmount =
                        purchaseInvoice
                            .RoundOffAmount,


                    GrossAmount =
                        purchaseInvoice.GrossAmount,

                    DiscountAmount =
                        purchaseInvoice.DiscountAmount,

                    TaxableAmount =
                        purchaseInvoice.TaxableAmount,

                    CgstAmount =
                        purchaseInvoice.CgstAmount,

                    SgstAmount =
                        purchaseInvoice.SgstAmount,

                    IgstAmount =
                        purchaseInvoice.IgstAmount,

                    GrandTotal =
                        purchaseInvoice.GrandTotal,


                    Remarks =
                        purchaseInvoice.Remarks,

                    Status =
                        purchaseInvoice.Status
                            .ToString()
                };


            // ---------------------------------------------
            // Exclude current Draft from reserved qty.
            // ---------------------------------------------

            var sourceItems =
                await _service
                    .GetAvailableGoodsReceiptItemsAsync(
                        purchaseInvoice.PurchaseOrderId,
                        purchaseInvoice.Id);


            var existingItemMap =
                purchaseInvoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .ToDictionary(
                        x =>
                            x.GoodsReceiptNoteItemId);


            var sequenceNumber =
                1;


            foreach (var sourceItem
                in sourceItems)
            {
                var availableQuantity =
                    await _service
                        .GetRemainingPurchaseInvoiceQuantityAsync(
                            sourceItem.Id,
                            purchaseInvoice.Id);


                var alreadyBilledQuantity =
                    sourceItem.ReceivedQuantity -
                    availableQuantity;


                if (alreadyBilledQuantity < 0m)
                {
                    alreadyBilledQuantity =
                        0m;
                }


                existingItemMap.TryGetValue(
                    sourceItem.Id,
                    out var existingItem);


                PurchaseInvoiceItem
                    trustedDisplayItem;


                if (existingItem != null)
                {
                    trustedDisplayItem =
                        existingItem;
                }
                else
                {
                    trustedDisplayItem =
                        BuildDisplayItemFromSource(
                            purchaseInvoice,
                            sourceItem);
                }


                trustedDisplayItem.SequenceNumber =
                    sequenceNumber++;


                viewModel.Items.Add(
                    MapFormItem(
                        trustedDisplayItem,
                        sourceItem,
                        isSelected:
                            existingItem != null,
                        alreadyBilledQuantity:
                            alreadyBilledQuantity,
                        availableQuantity:
                            availableQuantity,
                        purchaseInvoiceQuantity:
                            existingItem?
                                .PurchaseInvoiceQuantity
                            ??
                            availableQuantity));
            }


            return viewModel;
        }

        #endregion


        // =====================================================
        // REHYDRATE INVALID FORM
        // =====================================================

        #region Rehydrate Form

        private async Task RehydrateFormAsync(
            PurchaseInvoiceFormViewModel viewModel,
            int? excludePurchaseInvoiceId)
        {
            await PopulatePurchaseOrdersAsync(
                viewModel);


            // ---------------------------------------------
            // Reload trusted existing PDF metadata.
            // ---------------------------------------------

            if (excludePurchaseInvoiceId.HasValue)
            {
                var existing =
                    await _service
                        .GetByIdAsync(
                            excludePurchaseInvoiceId.Value);


                if (existing != null)
                {
                    viewModel
                        .ExistingSupplierInvoicePdfPath =
                        existing
                            .SupplierInvoicePdfPath;


                    viewModel
                        .ExistingSupplierInvoicePdfOriginalName =
                        existing
                            .SupplierInvoicePdfOriginalName;


                    viewModel
                        .ExistingSupplierInvoicePdfUploadedOn =
                        existing
                            .SupplierInvoicePdfUploadedOn;
                }
            }


            if (viewModel.PurchaseOrderId <= 0)
            {
                return;
            }


            var purchaseOrder =
                await _service
                    .GetPurchaseOrderForInvoiceAsync(
                        viewModel.PurchaseOrderId);


            if (purchaseOrder == null)
            {
                return;
            }


            // ---------------------------------------------
            // PO / Supplier / Company display information.
            // ---------------------------------------------

            viewModel.PurchaseOrderCode =
                purchaseOrder.Code;


            if (purchaseOrder.Supplier != null)
            {
                viewModel.SupplierId =
                    purchaseOrder.Supplier.SupplierId;


                viewModel.SupplierName =
                    purchaseOrder.Supplier.SupplierName;


                viewModel.SupplierCode =
                    purchaseOrder.Supplier.SupplierCode;


                viewModel.SupplierGstin =
                    purchaseOrder.Supplier.Gstin;


                viewModel.SupplierState =
                    purchaseOrder.Supplier.State;


                viewModel.SupplierAddress =
                    BuildSupplierAddress(
                        purchaseOrder.Supplier);
            }


            if (purchaseOrder.Company != null)
            {
                viewModel.CompanyId =
                    purchaseOrder.Company.CompanyId;


                viewModel.CompanyName =
                    purchaseOrder.Company.CompanyName;


                viewModel.CompanyGstin =
                    purchaseOrder.Company.GstNumber;


                viewModel.CompanyState =
                    purchaseOrder.Company.State;
            }


            // ---------------------------------------------
            // Preserve browser-posted:
            //
            // - Checkbox
            // - Qty
            // - Manual Supplier Rate
            // ---------------------------------------------

            var postedItems =
                viewModel.Items
                    .Where(x =>
                        x.GoodsReceiptNoteItemId > 0)
                    .GroupBy(x =>
                        x.GoodsReceiptNoteItemId)
                    .ToDictionary(
                        x =>
                            x.Key,
                        x =>
                            x.First());


            var sourceItems =
                await _service
                    .GetAvailableGoodsReceiptItemsAsync(
                        viewModel.PurchaseOrderId,
                        excludePurchaseInvoiceId);


            var rebuiltItems =
                new List<
                    PurchaseInvoiceFormItemViewModel>();


            var sequenceNumber =
                1;


            foreach (var sourceItem
                in sourceItems)
            {
                var availableQuantity =
                    await _service
                        .GetRemainingPurchaseInvoiceQuantityAsync(
                            sourceItem.Id,
                            excludePurchaseInvoiceId);


                var alreadyBilled =
                    sourceItem.ReceivedQuantity -
                    availableQuantity;


                if (alreadyBilled < 0m)
                {
                    alreadyBilled =
                        0m;
                }


                postedItems.TryGetValue(
                    sourceItem.Id,
                    out var postedItem);


                var displayItem =
                    BuildDisplayItemFromSource(
                        new PurchaseInvoice
                        {
                            PurchaseOrderId =
                                purchaseOrder.Id,

                            PurchaseOrderCode =
                                purchaseOrder.Code
                        },
                        sourceItem);


                /*
                 * Preserve manually entered Supplier Invoice
                 * Rate when validation fails.
                 */
                if (postedItem != null)
                {
                    displayItem.Rate =
                        postedItem.Rate;
                }


                displayItem.SequenceNumber =
                    sequenceNumber++;


                rebuiltItems.Add(
                    MapFormItem(
                        displayItem,
                        sourceItem,
                        isSelected:
                            postedItem?.IsSelected
                            ?? false,
                        alreadyBilledQuantity:
                            alreadyBilled,
                        availableQuantity:
                            availableQuantity,
                        purchaseInvoiceQuantity:
                            postedItem?
                                .PurchaseInvoiceQuantity
                            ??
                            availableQuantity));
            }


            viewModel.Items =
                rebuiltItems;
        }

        #endregion


        // =====================================================
        // PURCHASE ORDER DROPDOWN
        // =====================================================

        #region Populate Purchase Orders

        private async Task PopulatePurchaseOrdersAsync(
            PurchaseInvoiceFormViewModel viewModel)
        {
            var purchaseOrders =
                await _service
                    .GetPurchaseOrdersForInvoiceAsync();


            var options =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value =
                            string.Empty,

                        Text =
                            "-- Select Purchase Order --"
                    }
                };


            foreach (var purchaseOrder
                in purchaseOrders)
            {
                options.Add(
                    new SelectListItem
                    {
                        Value =
                            purchaseOrder.Id
                                .ToString(),

                        Text =
                            $"{purchaseOrder.Code} | " +
                            $"{purchaseOrder.SupplierName}",

                        Selected =
                            purchaseOrder.Id ==
                            viewModel.PurchaseOrderId
                    });
            }


            /*
             * Edit case:
             * Current PO might no longer appear in normal
             * available list because its quantity is fully
             * held by this Draft.
             */
            if (
                viewModel.PurchaseOrderId > 0
                &&
                !options.Any(x =>
                    x.Value ==
                    viewModel.PurchaseOrderId
                        .ToString())
            )
            {
                options.Add(
                    new SelectListItem
                    {
                        Value =
                            viewModel.PurchaseOrderId
                                .ToString(),

                        Text =
                            viewModel.PurchaseOrderCode
                            ??
                            $"PO #{viewModel.PurchaseOrderId}",

                        Selected =
                            true
                    });
            }


            viewModel.AvailablePurchaseOrders =
                options;
        }

        #endregion


        // =====================================================
        // BUILD SUBMITTED PURCHASE INVOICE
        // =====================================================

        #region Build Submitted Purchase Invoice

        private static PurchaseInvoice
            BuildSubmittedPurchaseInvoice(
                PurchaseInvoiceFormViewModel viewModel)
        {
            var purchaseInvoice =
                new PurchaseInvoice
                {
                    Id =
                        viewModel.Id,


                    PurchaseOrderId =
                        viewModel.PurchaseOrderId,


                    PurchaseInvoiceDate =
                        viewModel.PurchaseInvoiceDate,


                    SupplierInvoiceNumber =
                        viewModel.SupplierInvoiceNumber,


                    SupplierInvoiceDate =
                        viewModel.SupplierInvoiceDate,


                    TransportCharges =
                        viewModel.TransportCharges,


                    OtherCharges =
                        viewModel.OtherCharges,


                    RoundOffAmount =
                        viewModel.RoundOffAmount,


                    Remarks =
                        viewModel.Remarks
                };


            var selectedItems =
                viewModel.Items
                    .Where(x =>
                        x.IsSelected)
                    .ToList();


            foreach (var item
                in selectedItems)
            {
                purchaseInvoice.Items.Add(
                    new PurchaseInvoiceItem
                    {
                        /*
                         * Only actual transaction inputs
                         * are trusted from browser.
                         *
                         * Service reloads all item/GRN/PO
                         * source information.
                         */
                        GoodsReceiptNoteItemId =
                            item.GoodsReceiptNoteItemId,


                        PurchaseInvoiceQuantity =
                            item.PurchaseInvoiceQuantity,


                        /*
                         * Actual Supplier Invoice Rate.
                         */
                        Rate =
                            item.Rate
                    });
            }


            return purchaseInvoice;
        }

        #endregion


        // =====================================================
        // FORM ITEM MAPPING
        // =====================================================

        #region Map Form Item

        private static PurchaseInvoiceFormItemViewModel
            MapFormItem(
                PurchaseInvoiceItem item,
                GoodsReceiptNoteItem? sourceItem,
                bool isSelected,
                decimal alreadyBilledQuantity,
                decimal availableQuantity,
                decimal purchaseInvoiceQuantity)
        {
            return new PurchaseInvoiceFormItemViewModel
            {
                Id =
                    item.Id,


                SequenceNumber =
                    item.SequenceNumber,


                IsSelected =
                    isSelected,


                PurchaseOrderItemId =
                    item.PurchaseOrderItemId,


                PurchaseOrderCode =
                    item.PurchaseOrderCode,


                PurchaseOrderQuantity =
                    item.PurchaseOrderQuantity,


                GoodsReceiptNoteId =
                    item.GoodsReceiptNoteId,


                GoodsReceiptNoteCode =
                    item.GoodsReceiptNoteCode,


                GoodsReceiptNoteDate =
                    sourceItem?
                        .GoodsReceiptNote?
                        .GRNDate

                    ??

                    item.GoodsReceiptNote?
                        .GRNDate,


                GoodsReceiptNoteItemId =
                    item.GoodsReceiptNoteItemId,


                GoodsReceiptQuantity =
                    item.GoodsReceiptQuantity,


                SupplierChallanNumber =
                    item.SupplierChallanNumber,


                SupplierChallanDate =
                    item.SupplierChallanDate,


                AlreadyBilledQuantity =
                    alreadyBilledQuantity,


                AvailableQuantity =
                    availableQuantity,


                PurchaseInvoiceQuantity =
                    purchaseInvoiceQuantity,


                ItemId =
                    item.ItemId,


                ItemCode =
                    item.ItemCode,


                ItemName =
                    item.ItemName,


                Description =
                    item.Description,


                Specification =
                    item.Specification,


                UnitName =
                    item.UnitName,


                HsnCode =
                    item.HsnCode,


                DrawingId =
                    item.DrawingId,


                DrawingNumber =
                    item.DrawingNumber,


                DrawingRevision =
                    item.DrawingRevision,


                /*
                 * Actual manually entered Supplier Rate.
                 */
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
                    item.LineTotal,


                MaterialStatus =
                    sourceItem?
                        .MaterialStatus?
                        .ToString()

                    ??

                    item.GoodsReceiptNoteItem?
                        .MaterialStatus?
                        .ToString()
            };
        }

        #endregion


        // =====================================================
        // DISPLAY ITEM FROM TRUSTED GRN SOURCE
        // =====================================================

        #region Build Display Item From Source

        private static PurchaseInvoiceItem
            BuildDisplayItemFromSource(
                PurchaseInvoice purchaseInvoice,
                GoodsReceiptNoteItem sourceItem)
        {
            var poItem =
                sourceItem.PurchaseOrderItem;


            return new PurchaseInvoiceItem
            {
                PurchaseOrderItemId =
                    sourceItem.PurchaseOrderItemId,


                PurchaseOrderCode =
                    purchaseInvoice.PurchaseOrderCode,


                PurchaseOrderQuantity =
                    poItem?.Quantity
                    ??
                    sourceItem.OrderedQuantity,


                GoodsReceiptNoteId =
                    sourceItem.GoodsReceiptNoteId,


                GoodsReceiptNoteCode =
                    sourceItem
                        .GoodsReceiptNote
                        .Code,


                GoodsReceiptNoteItemId =
                    sourceItem.Id,


                GoodsReceiptQuantity =
                    sourceItem.ReceivedQuantity,


                SupplierChallanNumber =
                    sourceItem
                        .GoodsReceiptNote
                        .SupplierChallanNumber,


                SupplierChallanDate =
                    sourceItem
                        .GoodsReceiptNote
                        .SupplierChallanDate,


                ItemId =
                    sourceItem.ItemId,


                ItemCode =
                    sourceItem.ItemCode,


                ItemName =
                    sourceItem.ItemName,


                Description =
                    poItem?.Description,


                Specification =
                    sourceItem.Specification,


                UnitName =
                    sourceItem.UnitName,


                HsnCode =
                    poItem?.HSNCode,


                DrawingId =
                    poItem?.DrawingId,


                DrawingNumber =
                    poItem?.DrawingNumber,


                DrawingRevision =
                    poItem?.DrawingRevision,


                /*
                 * IMPORTANT:
                 *
                 * Do NOT load Purchase Order UnitPrice here.
                 *
                 * Supplier Invoice Rate is manually entered.
                 */
                Rate =
                    0m,


                GstRate =
                    poItem?.GSTPercent
                    ?? 0m
            };
        }

        #endregion


        // =====================================================
        // DETAILS MAPPING
        // =====================================================

        #region Map Details

        private static PurchaseInvoiceDetailsViewModel
            MapDetails(
                PurchaseInvoice purchaseInvoice)
        {
            var supplierSnapshot =
                ParseSnapshot(
                    purchaseInvoice
                        .SupplierSnapshotJson);


            var companySnapshot =
                ParseSnapshot(
                    purchaseInvoice
                        .CompanySnapshotJson);


            var viewModel =
                new PurchaseInvoiceDetailsViewModel
                {
                    Id =
                        purchaseInvoice.Id,


                    Code =
                        purchaseInvoice.Code,


                    PurchaseInvoiceDate =
                        purchaseInvoice
                            .PurchaseInvoiceDate,


                    Status =
                        purchaseInvoice.Status
                            .ToString(),


                    SupplierInvoiceNumber =
                        purchaseInvoice
                            .SupplierInvoiceNumber,


                    SupplierInvoiceDate =
                        purchaseInvoice
                            .SupplierInvoiceDate,


                    // -----------------------------------------
                    // Supplier Invoice PDF
                    // -----------------------------------------

                    SupplierInvoicePdfPath =
                        purchaseInvoice
                            .SupplierInvoicePdfPath,


                    SupplierInvoicePdfOriginalName =
                        purchaseInvoice
                            .SupplierInvoicePdfOriginalName,


                    SupplierInvoicePdfUploadedOn =
                        purchaseInvoice
                            .SupplierInvoicePdfUploadedOn,


                    PurchaseOrderId =
                        purchaseInvoice
                            .PurchaseOrderId,


                    PurchaseOrderCode =
                        purchaseInvoice
                            .PurchaseOrderCode,


                    SupplierId =
                        purchaseInvoice.SupplierId,


                    SupplierName =
                        purchaseInvoice.SupplierName,


                    SupplierCode =
                        GetSnapshotString(
                            supplierSnapshot,
                            "SupplierCode"),


                    SupplierGstin =
                        GetSnapshotString(
                            supplierSnapshot,
                            "Gstin"),


                    SupplierPan =
                        GetSnapshotString(
                            supplierSnapshot,
                            "Pan"),


                    SupplierContactPerson =
                        GetSnapshotString(
                            supplierSnapshot,
                            "ContactPerson"),


                    SupplierMobileNumber =
                        GetSnapshotString(
                            supplierSnapshot,
                            "MobileNumber"),


                    SupplierEmail =
                        GetSnapshotString(
                            supplierSnapshot,
                            "Email"),


                    SupplierAddress =
                        BuildSupplierAddress(
                            supplierSnapshot),


                    SupplierState =
                        GetSnapshotString(
                            supplierSnapshot,
                            "State"),


                    CompanyId =
                        purchaseInvoice.CompanyId,


                    CompanyName =
                        purchaseInvoice.CompanyName,


                    CompanyGstin =
                        GetSnapshotString(
                            companySnapshot,
                            "GstNumber"),


                    CompanyPan =
                        FirstSnapshotValue(
                            companySnapshot,
                            "Pan",
                            "PAN",
                            "PanNumber"),


                    CompanyAddress =
                        BuildCompanyAddress(
                            companySnapshot),


                    CompanyState =
                        GetSnapshotString(
                            companySnapshot,
                            "State"),


                    CompanyPhone =
                        GetSnapshotString(
                            companySnapshot,
                            "PhoneNumber"),


                    CompanyEmail =
                        GetSnapshotString(
                            companySnapshot,
                            "Email"),


                    PaymentTerms =
                        purchaseInvoice.PaymentTerms,


                    CreditDays =
                        purchaseInvoice.CreditDays,


                    DueDate =
                        purchaseInvoice.DueDate,


                    PlaceOfSupply =
                        purchaseInvoice.PlaceOfSupply,


                    IsInterState =
                        purchaseInvoice.IsInterState,


                    GrossAmount =
                        purchaseInvoice.GrossAmount,


                    DiscountAmount =
                        purchaseInvoice.DiscountAmount,


                    TaxableAmount =
                        purchaseInvoice.TaxableAmount,


                    CgstAmount =
                        purchaseInvoice.CgstAmount,


                    SgstAmount =
                        purchaseInvoice.SgstAmount,


                    IgstAmount =
                        purchaseInvoice.IgstAmount,


                    TransportCharges =
                        purchaseInvoice
                            .TransportCharges,


                    OtherCharges =
                        purchaseInvoice.OtherCharges,


                    RoundOffAmount =
                        purchaseInvoice.RoundOffAmount,


                    GrandTotal =
                        purchaseInvoice.GrandTotal,


                    Remarks =
                        purchaseInvoice.Remarks,


                    FinalizedOn =
                        purchaseInvoice.FinalizedOn,


                    FinalizedBy =
                        purchaseInvoice.FinalizedBy,


                    CreatedOn =
                        purchaseInvoice.CreatedOn,


                    CreatedBy =
                        purchaseInvoice.CreatedBy,


                    ModifiedOn =
                        purchaseInvoice.ModifiedOn,


                    ModifiedBy =
                        purchaseInvoice.ModifiedBy
                };


            foreach (var item
                in purchaseInvoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                viewModel.Items.Add(
                    new PurchaseInvoiceDetailsItemViewModel
                    {
                        Id =
                            item.Id,


                        SequenceNumber =
                            item.SequenceNumber,


                        PurchaseOrderItemId =
                            item.PurchaseOrderItemId,


                        PurchaseOrderCode =
                            item.PurchaseOrderCode,


                        PurchaseOrderQuantity =
                            item.PurchaseOrderQuantity,


                        GoodsReceiptNoteId =
                            item.GoodsReceiptNoteId,


                        GoodsReceiptNoteCode =
                            item.GoodsReceiptNoteCode,


                        GoodsReceiptNoteDate =
                            item.GoodsReceiptNote?
                                .GRNDate,


                        GoodsReceiptNoteItemId =
                            item.GoodsReceiptNoteItemId,


                        GoodsReceiptQuantity =
                            item.GoodsReceiptQuantity,


                        SupplierChallanNumber =
                            item.SupplierChallanNumber,


                        SupplierChallanDate =
                            item.SupplierChallanDate,


                        MaterialStatus =
                            item.GoodsReceiptNoteItem?
                                .MaterialStatus?
                                .ToString(),


                        ItemId =
                            item.ItemId,


                        ItemCode =
                            item.ItemCode,


                        ItemName =
                            item.ItemName,


                        Description =
                            item.Description,


                        Specification =
                            item.Specification,


                        UnitName =
                            item.UnitName,


                        HsnCode =
                            item.HsnCode,


                        DrawingNumber =
                            item.DrawingNumber,


                        DrawingRevision =
                            item.DrawingRevision,


                        PurchaseInvoiceQuantity =
                            item.PurchaseInvoiceQuantity,


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


        // =====================================================
        // SUPPLIER INVOICE PDF - SAVE
        // =====================================================

        #region Supplier Invoice PDF - Save

        private async Task<SavedPdfFile>
            SaveSupplierInvoicePdfAsync(
                IFormFile file)
        {
            await ValidateSupplierInvoicePdfAsync(
                file);


            var uploadDirectory =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads",
                    "purchase-invoices");


            Directory.CreateDirectory(
                uploadDirectory);


            /*
             * Never use browser filename as physical filename.
             */
            var storedFileName =
                $"{Guid.NewGuid():N}.pdf";


            var physicalPath =
                Path.Combine(
                    uploadDirectory,
                    storedFileName);


            await using (
                var sourceStream =
                    file.OpenReadStream())
            await using (
                var targetStream =
                    new FileStream(
                        physicalPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        useAsync:
                            true))
            {
                await sourceStream
                    .CopyToAsync(
                        targetStream);
            }


            var originalFileName =
                Path.GetFileName(
                    file.FileName);


            if (originalFileName.Length > 500)
            {
                originalFileName =
                    originalFileName[..500];
            }


            var relativePath =
                $"/uploads/purchase-invoices/{storedFileName}";


            return new SavedPdfFile
            {
                RelativePath =
                    relativePath,


                OriginalFileName =
                    originalFileName,


                UploadedOn =
                    DateTime.Now
            };
        }

        #endregion


        // =====================================================
        // SUPPLIER INVOICE PDF - VALIDATION
        // =====================================================

        #region Supplier Invoice PDF - Validation

        private static async Task
            ValidateSupplierInvoicePdfAsync(
                IFormFile file)
        {
            // ---------------------------------------------
            // Empty File
            // ---------------------------------------------

            if (file.Length <= 0)
            {
                throw new BusinessException(
                    "Selected Supplier Invoice PDF is empty.");
            }


            // ---------------------------------------------
            // Maximum 10 MB
            // ---------------------------------------------

            if (file.Length >
                MaxSupplierInvoicePdfSize)
            {
                throw new BusinessException(
                    "Supplier Invoice PDF cannot be larger than 10 MB.");
            }


            // ---------------------------------------------
            // Extension Validation
            // ---------------------------------------------

            var extension =
                Path.GetExtension(
                    file.FileName);


            if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Only PDF files are allowed for Supplier Invoice.");
            }


            // ---------------------------------------------
            // MIME Validation
            // ---------------------------------------------

            if (
                !string.IsNullOrWhiteSpace(
                    file.ContentType)
                &&
                !string.Equals(
                    file.ContentType,
                    "application/pdf",
                    StringComparison.OrdinalIgnoreCase)
                &&
                !string.Equals(
                    file.ContentType,
                    "application/octet-stream",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new BusinessException(
                    "Selected file is not a valid PDF.");
            }


            // ---------------------------------------------
            // Actual PDF Header Validation
            //
            // Valid PDF starts with:
            // %PDF-
            // ---------------------------------------------

            var header =
                new byte[5];


            await using var stream =
                file.OpenReadStream();


            var bytesRead =
                await stream.ReadAsync(
                    header.AsMemory(
                        0,
                        header.Length));


            if (
                bytesRead < 5
                ||
                header[0] != (byte)'%'
                ||
                header[1] != (byte)'P'
                ||
                header[2] != (byte)'D'
                ||
                header[3] != (byte)'F'
                ||
                header[4] != (byte)'-'
            )
            {
                throw new BusinessException(
                    "Selected file is not a valid PDF document.");
            }
        }

        #endregion


        // =====================================================
        // SUPPLIER INVOICE PDF - ENTITY MAPPING
        // =====================================================

        #region Supplier Invoice PDF - Entity Mapping

        private static void ApplyPdfInformation(
            PurchaseInvoice purchaseInvoice,
            SavedPdfFile savedPdf)
        {
            purchaseInvoice.SupplierInvoicePdfPath =
                savedPdf.RelativePath;


            purchaseInvoice.SupplierInvoicePdfOriginalName =
                savedPdf.OriginalFileName;


            purchaseInvoice.SupplierInvoicePdfUploadedOn =
                savedPdf.UploadedOn;
        }


        private static void CopyExistingPdfInformation(
            PurchaseInvoice target,
            PurchaseInvoice source)
        {
            target.SupplierInvoicePdfPath =
                source.SupplierInvoicePdfPath;


            target.SupplierInvoicePdfOriginalName =
                source.SupplierInvoicePdfOriginalName;


            target.SupplierInvoicePdfUploadedOn =
                source.SupplierInvoicePdfUploadedOn;
        }

        #endregion


        // =====================================================
        // SUPPLIER INVOICE PDF - SAFE PATH
        // =====================================================

        #region Supplier Invoice PDF - Safe Path

        private string? GetSafePdfPhysicalPath(
            string relativePath)
        {
            if (string.IsNullOrWhiteSpace(
                relativePath))
            {
                return null;
            }


            var uploadRoot =
                Path.GetFullPath(
                    Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "uploads",
                        "purchase-invoices"));


            var normalizedRelativePath =
                relativePath
                    .TrimStart(
                        '/',
                        '\\')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar)
                    .Replace(
                        '\\',
                        Path.DirectorySeparatorChar);


            var physicalPath =
                Path.GetFullPath(
                    Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        normalizedRelativePath));


            var requiredPrefix =
                uploadRoot +
                Path.DirectorySeparatorChar;


            /*
             * Prevent path escaping outside our
             * Purchase Invoice PDF folder.
             */
            if (!physicalPath.StartsWith(
                requiredPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }


            return physicalPath;
        }

        #endregion


        // =====================================================
        // SUPPLIER INVOICE PDF - CLEANUP
        // =====================================================

        #region Supplier Invoice PDF - Delete Physical File

        private void DeleteFileIfExists(
            string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(
                relativePath))
            {
                return;
            }


            try
            {
                var physicalPath =
                    GetSafePdfPhysicalPath(
                        relativePath);


                if (
                    !string.IsNullOrWhiteSpace(
                        physicalPath)
                    &&
                    System.IO.File.Exists(
                        physicalPath)
                )
                {
                    System.IO.File.Delete(
                        physicalPath);
                }
            }
            catch
            {
                /*
                 * Cleanup failure must not hide original
                 * Purchase Invoice business error.
                 */
            }
        }

        #endregion


        // =====================================================
        // PDF HELPER MODEL
        // =====================================================

        #region Supplier Invoice PDF - Helper Model

        private sealed class SavedPdfFile
        {
            public string RelativePath
            {
                get;
                set;
            } = string.Empty;


            public string OriginalFileName
            {
                get;
                set;
            } = string.Empty;


            public DateTime UploadedOn
            {
                get;
                set;
            }
        }

        #endregion


        // =====================================================
        // SNAPSHOT HELPERS
        // =====================================================

        #region Snapshot Helpers

        private static Dictionary<string, JsonElement>
            ParseSnapshot(
                string? json)
        {
            if (string.IsNullOrWhiteSpace(
                json))
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
                                json)

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


        private static string?
            FirstSnapshotValue(
                Dictionary<string, JsonElement> snapshot,
                params string[] names)
        {
            foreach (var name
                in names)
            {
                var value =
                    GetSnapshotString(
                        snapshot,
                        name);


                if (!string.IsNullOrWhiteSpace(
                    value))
                {
                    return value;
                }
            }


            return null;
        }

        #endregion


        // =====================================================
        // ADDRESS HELPERS
        // =====================================================

        #region Address Helpers

        private static string?
            BuildSupplierAddress(
                Dictionary<string, JsonElement> snapshot)
        {
            return JoinAddress(
                GetSnapshotString(
                    snapshot,
                    "AddressLine1"),

                GetSnapshotString(
                    snapshot,
                    "AddressLine2"),

                GetSnapshotString(
                    snapshot,
                    "City"),

                GetSnapshotString(
                    snapshot,
                    "State"),

                GetSnapshotString(
                    snapshot,
                    "Pincode"));
        }


        private static string?
            BuildSupplierAddress(
                Supplier supplier)
        {
            return JoinAddress(
                supplier.AddressLine1,
                supplier.AddressLine2,
                supplier.City,
                supplier.State,
                supplier.Pincode);
        }


        private static string?
            BuildCompanyAddress(
                Dictionary<string, JsonElement> snapshot)
        {
            return JoinAddress(
                GetSnapshotString(
                    snapshot,
                    "Address"),

                GetSnapshotString(
                    snapshot,
                    "City"),

                GetSnapshotString(
                    snapshot,
                    "State"),

                GetSnapshotString(
                    snapshot,
                    "PostalCode"),

                GetSnapshotString(
                    snapshot,
                    "Country"));
        }


        private static string?
            JoinAddress(
                params string?[] parts)
        {
            var values =
                parts
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x))
                    .Select(x =>
                        x!.Trim())
                    .ToList();


            return values.Count == 0
                ? null
                : string.Join(
                    ", ",
                    values);
        }

        #endregion
    }
}