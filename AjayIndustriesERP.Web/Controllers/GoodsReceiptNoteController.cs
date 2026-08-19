// ============================================================
// File: GoodsReceiptNoteController.cs
// Purpose:
// Handles HTTP requests/responses for the GRN module.
//
// Responsibilities:
// - Index
// - Details
// - Create GET/POST
// - Edit GET/POST
// - PO AJAX loading
// - Edit AJAX loading
// - Web ViewModel ↔ Domain mapping
// - Toast feedback through TempData
//
// Important:
// No GRN business logic exists in Controller.
// Business rules remain in GoodsReceiptNoteService.
// ============================================================

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.GoodsReceiptNote;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class GoodsReceiptNoteController : Controller
    {
        private readonly IGoodsReceiptNoteService
            _goodsReceiptNoteService;

        public GoodsReceiptNoteController(
            IGoodsReceiptNoteService goodsReceiptNoteService)
        {
            _goodsReceiptNoteService =
                goodsReceiptNoteService;
        }


        // =====================================================
        // INDEX
        // =====================================================

        // =====================================================
        // INDEX
        // =====================================================
        //
        // GRN Index is grouped by Purchase Order.
        //
        // Search:
        // - GRN Number
        // - PO Number
        // - Supplier
        // - Challan Number
        //
        // Pagination:
        // One Purchase Order = One paged record.
        //
        // Example:
        //
        // PO-00001
        //   GRN-001
        //   GRN-002
        //
        // counts as ONE pagination record.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                var goodsReceiptNotes =
                    await _goodsReceiptNoteService
                        .SearchAsync(
                            searchText);


                var groups =
                    BuildIndexGroups(
                        goodsReceiptNotes);


                ViewBag.SearchText =
                    searchText;

                ViewBag.PageNumber =
                    1;

                ViewBag.PageSize =
                    pageSize;

                ViewBag.TotalRecords =
                    groups.Count;

                ViewBag.TotalPages =
                    1;

                ViewBag.HasPrevious =
                    false;

                ViewBag.HasNext =
                    false;


                return View(
                    groups);
            }


            var result =
                await _goodsReceiptNoteService
                    .GetPagedAsync(
                        pageNumber,
                        pageSize);


            var groupedResult =
                BuildIndexGroups(
                    result.Items);


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
                groupedResult);
        }


        // =====================================================
        // DETAILS
        // =====================================================

        // =====================================================
        // DETAILS
        // =====================================================
        //
        // Displays current GRN together with cumulative receipt
        // history of every Purchase Order item.
        //
        // This allows the latest GRN to act as a complete receipt
        // summary for the Purchase Order.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            try
            {
                var grn =
                    await _goodsReceiptNoteService
                        .GetByIdAsync(id);


                if (grn == null)
                {
                    return NotFound();
                }


                var receiptHistory =
                    await _goodsReceiptNoteService
                        .GetReceiptHistoryAsync(
                            grn.PurchaseOrderId,
                            grn.Id);


                var model =
                    new GoodsReceiptNoteDetailsViewModel
                    {
                        GoodsReceiptNote =
                            grn,

                        ReceiptHistory =
                            receiptHistory
                                .GroupBy(x =>
                                    x.PurchaseOrderItemId)
                                .ToDictionary(
                                    x => x.Key,
                                    x => x
                                        .OrderBy(y =>
                                            y.GoodsReceiptNote.GRNDate)
                                        .ThenBy(y =>
                                            y.GoodsReceiptNoteId)
                                        .ToList())
                    };


                return View(model);
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Index));
            }
        }


        // =====================================================
        // CREATE GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadPurchaseOrdersAsync();


            return View(
                new GoodsReceiptNoteCreateViewModel
                {
                    GRNDate =
                        DateTime.Today
                });
        }


        // =====================================================
        // PO DATA AJAX
        // =====================================================

        [HttpGet]
        public async Task<IActionResult>
            GetPurchaseOrderData(
                int purchaseOrderId)
        {
            try
            {
                var grn =
                    await _goodsReceiptNoteService
                        .PrepareForPurchaseOrderAsync(
                            purchaseOrderId);


                return Json(
                    BuildAjaxResult(grn));
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


        // =====================================================
        // CREATE POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GoodsReceiptNoteCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadPurchaseOrdersAsync(
                    model.PurchaseOrderId);

                return View(model);
            }


            try
            {
                var created =
                    await _goodsReceiptNoteService
                        .CreateAsync(
                            MapToDomain(model));


                TempData["SuccessMessage"] =
                    $"GRN {created.Code} created successfully.";


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
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadPurchaseOrdersAsync(
                    model.PurchaseOrderId);


                return View(model);
            }
        }


        // =====================================================
        // EDIT GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            try
            {
                var grn =
                    await _goodsReceiptNoteService
                        .PrepareForEditAsync(id);


                await LoadPurchaseOrdersAsync(
                    grn.PurchaseOrderId);


                return View(
                    MapToViewModel(grn));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Index));
            }
        }


        // =====================================================
        // EDIT DATA AJAX
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetEditData(
            int id)
        {
            try
            {
                var grn =
                    await _goodsReceiptNoteService
                        .PrepareForEditAsync(id);


                return Json(
                    BuildAjaxResult(grn));
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


        // =====================================================
        // EDIT POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            GoodsReceiptNoteCreateViewModel model)
        {
            if (id <= 0 ||
                id != model.Id)
            {
                TempData["ErrorMessage"] =
                    "Invalid GRN.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (!ModelState.IsValid)
            {
                await LoadPurchaseOrdersAsync(
                    model.PurchaseOrderId);

                return View(model);
            }


            try
            {
                var updated =
                    await _goodsReceiptNoteService
                        .UpdateAsync(
                            MapToDomain(model));


                TempData["SuccessMessage"] =
                    $"GRN {updated.Code} updated successfully.";


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
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadPurchaseOrdersAsync(
                    model.PurchaseOrderId);


                return View(model);
            }
        }


        // =====================================================
        // PURCHASE ORDER DROPDOWN
        // =====================================================

        private async Task LoadPurchaseOrdersAsync(
            int? selectedId = null)
        {
            var purchaseOrders =
                await _goodsReceiptNoteService
                    .GetPurchaseOrdersForReceiptAsync();


            ViewBag.PurchaseOrders =
                new SelectList(
                    purchaseOrders,
                    "Id",
                    "Code",
                    selectedId);
        }


        // =====================================================
        // WEB → DOMAIN
        // =====================================================

        private static GoodsReceiptNote MapToDomain(
            GoodsReceiptNoteCreateViewModel model)
        {
            var grn =
                new GoodsReceiptNote
                {
                    Id =
                        model.Id,

                    GRNDate =
                        model.GRNDate,

                    PurchaseOrderId =
                        model.PurchaseOrderId,

                    SupplierChallanNumber =
                        model.SupplierChallanNumber,

                    SupplierChallanDate =
                        model.SupplierChallanDate,

                    Remarks =
                        model.Remarks
                };


            foreach (var item in model.Items)
            {
                grn.Items.Add(
                    new GoodsReceiptNoteItem
                    {
                        PurchaseOrderItemId =
                            item.PurchaseOrderItemId,

                        ItemId =
                            item.ItemId,

                        ReceiptStatus =
                            item.ReceiptStatus,

                        ReceivedQuantity =
                            item.ReceivedQuantity,

                        MaterialStatus =
                            item.MaterialStatus,

                        Remarks =
                            item.Remarks
                    });
            }


            return grn;
        }


        // =====================================================
        // DOMAIN → WEB
        // =====================================================

        private static GoodsReceiptNoteCreateViewModel
            MapToViewModel(
                GoodsReceiptNote grn)
        {
            return new GoodsReceiptNoteCreateViewModel
            {
                Id =
                    grn.Id,

                GRNDate =
                    grn.GRNDate,

                PurchaseOrderId =
                    grn.PurchaseOrderId,

                SupplierId =
                    grn.SupplierId,

                SupplierName =
                    grn.SupplierName,

                SupplierChallanNumber =
                    grn.SupplierChallanNumber,

                SupplierChallanDate =
                    grn.SupplierChallanDate,

                Remarks =
                    grn.Remarks,

                Items =
                    grn.Items
                        .Select(x =>
                            new GoodsReceiptNoteItemViewModel
                            {
                                PurchaseOrderItemId =
                                    x.PurchaseOrderItemId,

                                ItemId =
                                    x.ItemId,

                                ItemCode =
                                    x.ItemCode,

                                ItemName =
                                    x.ItemName,

                                Specification =
                                    x.Specification,

                                UnitName =
                                    x.UnitName,

                                OrderedQuantity =
                                    x.OrderedQuantity,

                                PreviouslyReceivedQuantity =
                                    x.PreviouslyReceivedQuantity,

                                BalanceQuantity =
                                    x.BalanceQuantity,

                                ReceiptStatus =
                                    x.ReceiptStatus,

                                ReceivedQuantity =
                                    x.ReceivedQuantity,

                                PendingQuantity =
                                    x.PendingQuantity,

                                MaterialStatus =
                                    x.MaterialStatus,

                                Remarks =
                                    x.Remarks
                            })
                        .ToList()
            };
        }


        // =====================================================
        // AJAX RESULT
        // =====================================================

        private static object BuildAjaxResult(
            GoodsReceiptNote grn)
        {
            return new
            {
                grnId =
                    grn.Id,

                purchaseOrderId =
                    grn.PurchaseOrderId,

                supplierId =
                    grn.SupplierId,

                supplierName =
                    grn.SupplierName,

                items =
                    grn.Items
                        .Select(x => new
                        {
                            purchaseOrderItemId =
                                x.PurchaseOrderItemId,

                            itemId =
                                x.ItemId,

                            itemCode =
                                x.ItemCode,

                            itemName =
                                x.ItemName,

                            specification =
                                x.Specification,

                            unitName =
                                x.UnitName,

                            orderedQuantity =
                                x.OrderedQuantity,

                            previouslyReceivedQuantity =
                                x.PreviouslyReceivedQuantity,

                            balanceQuantity =
                                x.BalanceQuantity,

                            receiptStatus =
                                (int)x.ReceiptStatus,

                            receivedQuantity =
                                x.ReceivedQuantity,

                            pendingQuantity =
                                x.PendingQuantity,

                            materialStatus =
                                x.MaterialStatus.HasValue
                                    ? (int?)x.MaterialStatus.Value
                                    : null,

                            remarks =
                                x.Remarks
                        })
                        .ToList()
            };
        }

        // =====================================================
        // BUILD GRN INDEX PURCHASE ORDER GROUPS
        // =====================================================
        //
        // Purpose:
        // Converts flat GRN records returned by Application layer
        // into the grouped presentation required by Index.cshtml.
        //
        // This is presentation mapping only.
        // No receipt/business calculations are performed here.
        // =====================================================

        private static List<GoodsReceiptNoteIndexViewModel>
            BuildIndexGroups(
                IEnumerable<GoodsReceiptNote>
                    goodsReceiptNotes)
        {
            return goodsReceiptNotes
                .GroupBy(x =>
                    x.PurchaseOrderId)
                .Select(group =>
                {
                    // Latest means latest created GRN.
                    // This matches the Service rule where only the
                    // latest GRN against the PO can be edited.

                    var history =
                        group
                            .OrderByDescending(x =>
                                x.Id)
                            .ToList();


                    var latest =
                        history.First();

                    // =================================================
                    // CALCULATE CURRENT PO RECEIPT POSITION
                    // =================================================
                    //
                    // Latest GRN stores the receipt position of every
                    // Purchase Order item after that GRN.
                    //
                    // If every item has PendingQuantity = 0,
                    // the PO receipt is Complete.
                    //
                    // Otherwise material is still Pending.
                    // =================================================

                    var isReceiptComplete =
                        latest.Items != null &&
                        latest.Items.Any() &&
                        latest.Items.All(x =>
                            x.PendingQuantity <= 0);


                    return new GoodsReceiptNoteIndexViewModel
                    {
                        PurchaseOrderId =
                            latest.PurchaseOrderId,

                        PurchaseOrderCode =
                            latest.PurchaseOrder?.Code
                            ?? "-",

                        SupplierName =
                            latest.SupplierName,

                        LatestGoodsReceiptNoteId =
                            latest.Id,

                        LatestGoodsReceiptNoteCode =
                            latest.Code,

                        LatestGoodsReceiptNoteDate =
                            latest.GRNDate,

                        IsReceiptComplete =
                            isReceiptComplete,

                        History =
                            history
                                .Select(x =>
                                    new GoodsReceiptNoteHistoryViewModel
                                    {
                                        Id =
                                            x.Id,

                                        Code =
                                            x.Code,

                                        GRNDate =
                                            x.GRNDate,

                                        SupplierChallanNumber =
                                            x.SupplierChallanNumber,

                                        SupplierChallanDate =
                                            x.SupplierChallanDate,

                                        IsLatest =
                                            x.Id ==
                                            latest.Id
                                    })
                                .ToList()
                    };
                })
                .OrderByDescending(x =>
                    x.LatestGoodsReceiptNoteId)
                .ToList();
        }
    }
}