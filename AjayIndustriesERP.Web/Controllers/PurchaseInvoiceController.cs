/*
============================================================
File: PurchaseInvoiceController.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Handles Purchase Invoice Web operations.

Source Flow:
Purchase Order
    → GRN
    → Purchase Invoice
    → Supplier Payment
    → Supplier Outstanding

Responsibilities:
- Purchase Invoice Index / Search / Pagination.
- Prepare Create form from Purchase Order.
- Create trusted Purchase Invoice.
- Edit Draft Purchase Invoice.
- Show Purchase Invoice Details.
- Finalize Purchase Invoice.
- Soft Delete Draft Purchase Invoice.
- Show Deleted Purchase Invoices.
- Restore deleted Draft Purchase Invoice.

Important:
- Controller trusts only transaction inputs.
- Source snapshots / Rate / GST / calculated amounts
  are rebuilt by PurchaseInvoiceService.
- Only selected GRN rows are submitted to Service.
============================================================
*/

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
    public class PurchaseInvoiceController
        : Controller
    {
        #region Fields

        private readonly IPurchaseInvoiceService
            _service;

        #endregion


        #region Constructor

        public PurchaseInvoiceController(
            IPurchaseInvoiceService service)
        {
            _service =
                service;
        }

        #endregion


        // =====================================================
        // INDEX
        // =====================================================

        #region Index

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int pageNumber = 1,
            int pageSize = 10)
        {
            ViewBag.SearchText =
                searchText;


            ViewBag.PageSize =
                pageSize;


            var result =
                string.IsNullOrWhiteSpace(
                    searchText)

                    ? await _service
                        .GetPagedAsync(
                            pageNumber,
                            pageSize)

                    : await _service
                        .SearchPagedAsync(
                            searchText,
                            pageNumber,
                            pageSize);


            return View(
                result);
        }

        #endregion


        // =====================================================
        // CREATE GET
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
        // CREATE POST
        // =====================================================

        #region Create POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PurchaseInvoiceFormViewModel viewModel)
        {
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


                var purchaseInvoice =
                    BuildSubmittedPurchaseInvoice(
                        viewModel);


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
        }

        #endregion


        // =====================================================
        // EDIT GET
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
        // EDIT POST
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


            try
            {
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


                var updated =
                    await _service
                        .UpdateAsync(
                            purchaseInvoice);


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
        // FINALIZE
        // =====================================================

        #region Finalize

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
                    $"Purchase Invoice {finalized.Code} finalized successfully.";
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
        // DELETED
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
        // CREATE FORM MAPPING
        // =====================================================

        #region Map Prepared Invoice

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


                if (alreadyBilled < 0)
                {
                    alreadyBilled =
                        0;
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
        // EDIT FORM MAPPING
        // =====================================================

        #region Map Existing Invoice

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


            /*
             * Current Purchase Invoice is excluded from
             * allocation calculation.
             *
             * Therefore existing quantities remain available
             * to the Draft being edited.
             */
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


                if (alreadyBilledQuantity < 0)
                {
                    alreadyBilledQuantity =
                        0;
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


            #region Header Display

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

            #endregion


            #region Preserve Posted Item Inputs

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

            #endregion


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


                if (alreadyBilled < 0)
                {
                    alreadyBilled =
                        0;
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
             * During Edit the selected PO may have no
             * additional unbilled quantity except the current
             * Purchase Invoice itself.
             *
             * Ensure selected PO still appears.
             */
            if (
                viewModel.PurchaseOrderId > 0 &&
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
        // BUILD SUBMITTED ENTITY
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
        GoodsReceiptNoteItemId =
            item.GoodsReceiptNoteItemId,

        PurchaseInvoiceQuantity =
            item.PurchaseInvoiceQuantity,

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
        // DISPLAY ITEM FROM GRN
        // =====================================================

        #region Build Display Item

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


                Rate =
                    poItem?.UnitPrice
                    ?? 0m,

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