/*
==============================================================

File : PurchaseOrderController.cs

Purpose :
Handles Purchase Order UI requests.

Features :
- Purchase Order List
- Search and Pagination
- Create / Edit / Details / Delete
- Company / Supplier / Item dropdowns
- Dynamic Purchase Order Items
- Item information loading
- Current Drawing loading
- BusinessException handling

==============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    /// <summary>
    /// Handles Purchase Order UI operations.
    /// </summary>
    public class PurchaseOrderController :
        Controller
    {
        private readonly IPurchaseOrderService
            _purchaseOrderService;

        private readonly ICompanyService
            _companyService;

        private readonly ISupplierService
            _supplierService;

        private readonly IItemService
            _itemService;

        private readonly IDrawingService
            _drawingService;

        private readonly IPurchaseOrderPdfService
    _purchaseOrderPdfService;


        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ICompanyService companyService,
            ISupplierService supplierService,
            IItemService itemService,
            IDrawingService drawingService,
            IPurchaseOrderPdfService purchaseOrderPdfService)
        {
            _purchaseOrderService =
                purchaseOrderService;

            _companyService =
                companyService;

            _supplierService =
                supplierService;

            _itemService =
                itemService;

            _drawingService =
                drawingService;

            _purchaseOrderPdfService =
    purchaseOrderPdfService;
        }


        #region Purchase Order List

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                var purchaseOrders =
                    await _purchaseOrderService
                        .SearchAsync(
                            searchText);

                ViewBag.SearchText =
                    searchText;

                ViewBag.PageNumber =
                    1;

                ViewBag.PageSize =
                    pageSize;

                ViewBag.TotalRecords =
                    purchaseOrders.Count;

                ViewBag.TotalPages =
                    1;

                ViewBag.HasPrevious =
                    false;

                ViewBag.HasNext =
                    false;

                return View(
                    purchaseOrders);
            }


            var result =
                await _purchaseOrderService
                    .GetPagedAsync(
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


        #region Create Purchase Order

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new PurchaseOrderViewModel
                {
                    PODate =
                        DateTime.Today,

                    IsActive =
                        true
                };


            /*
             * Start Create form with one blank
             * Purchase Order Item row.
             */
            model.Items.Add(
                new PurchaseOrderItemViewModel
                {
                    Quantity = 1,
                    GSTPercent = 18
                });


            await LoadDropdownsAsync(
                model);

            await LoadDrawingOptionsAsync(
                model);


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PurchaseOrderViewModel model)
        {
            NormalizeModel(
                model);


            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    model);

                await LoadDrawingOptionsAsync(
                    model);

                return View(model);
            }


            try
            {
                var purchaseOrder =
                    MapToEntity(
                        model);


                await _purchaseOrderService
                    .CreateAsync(
                        purchaseOrder);


                TempData["Success"] =
                    "Purchase Order created successfully.";


                /*
                 * Service assigns generated PO Code
                 * and EF assigns Id after SaveChanges.
                 */
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = purchaseOrder.Id
                    });
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;

                await LoadDropdownsAsync(
                    model);

                await LoadDrawingOptionsAsync(
                    model);

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";

                await LoadDropdownsAsync(
                    model);

                await LoadDrawingOptionsAsync(
                    model);

                return View(model);
            }
        }

        #endregion


        #region Purchase Order Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var purchaseOrder =
                await _purchaseOrderService
                    .GetByIdAsync(
                        id);


            if (purchaseOrder == null)
            {
                TempData["Error"] =
                    "Purchase Order not found.";

                return RedirectToAction(
                    nameof(Index));
            }


            return View(
                purchaseOrder);
        }

        #endregion


        #region Edit Purchase Order

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var purchaseOrder =
                await _purchaseOrderService
                    .GetByIdAsync(
                        id);


            if (purchaseOrder == null)
            {
                TempData["Error"] =
                    "Purchase Order not found.";

                return RedirectToAction(
                    nameof(Index));
            }


            var model =
                MapToViewModel(
                    purchaseOrder);


            await LoadDropdownsAsync(
                model);

            await LoadDrawingOptionsAsync(
                model);


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PurchaseOrderViewModel model)
        {
            NormalizeModel(
                model);


            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    model);

                await LoadDrawingOptionsAsync(
                    model);

                return View(model);
            }


            try
            {
                var purchaseOrder =
                    MapToEntity(
                        model);


                await _purchaseOrderService
                    .UpdateAsync(
                        purchaseOrder);


                TempData["Success"] =
                    "Purchase Order updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = model.Id
                    });
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;

                await LoadDropdownsAsync(
                    model);

                await LoadDrawingOptionsAsync(
                    model);

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";

                await LoadDropdownsAsync(
                    model);

                await LoadDrawingOptionsAsync(
                    model);

                return View(model);
            }
        }

        #endregion


        #region Purchase Order Workflow

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            int id)
        {
            try
            {
                await _purchaseOrderService
                    .ConfirmAsync(
                        id);

                TempData["Success"] =
                    "Purchase Order confirmed successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSent(
            int id)
        {
            try
            {
                await _purchaseOrderService
                    .MarkAsSentAsync(
                        id);

                TempData["Success"] =
                    "Purchase Order marked as Sent.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        #endregion

        #region Delete Purchase Order

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _purchaseOrderService
                    .DeleteAsync(
                        id);


                TempData["Success"] =
                    "Purchase Order deleted successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Something went wrong. Please try again.";
            }


            return RedirectToAction(
                nameof(Index));
        }

        #endregion

        #region Tax Type AJAX

        /// <summary>
        /// Determines whether Purchase GST is
        /// Intra-State or Inter-State.
        ///
        /// Same State      = CGST + SGST
        /// Different State = IGST
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTaxType(
            int companyId,
            int supplierId)
        {
            try
            {
                if (companyId <= 0 ||
                    supplierId <= 0)
                {
                    return Json(
                        new
                        {
                            success = false
                        });
                }


                var isIntraState =
                    await _purchaseOrderService
                        .IsIntraStateAsync(
                            companyId,
                            supplierId);


                return Json(
                    new
                    {
                        success = true,
                        isIntraState
                    });
            }
            catch (BusinessException ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        #endregion


        #region Item Information AJAX

        /// <summary>
        /// Returns Item display information and
        /// Current Drawing for dynamic PO row.
        ///
        /// This information is for UI convenience only.
        /// Authoritative snapshots are generated again
        /// by PurchaseOrderService during Save.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetItemInfo(
            int itemId)
        {
            if (itemId <= 0)
            {
                return Json(
                    new
                    {
                        success = false
                    });
            }


            var item =
                await _itemService
                    .GetByIdAsync(
                        itemId);


            if (item == null ||
                item.IsDeleted ||
                !item.IsActive)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Selected Item is not available."
                    });
            }


            var itemSpecifications =
                await _itemService
                    .GetSpecificationsAsync(
                        itemId);


            var specificationText =
                string.Join(
                    " | ",
                    itemSpecifications
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)
                        .OrderBy(x =>
                            x.SortOrder)
                        .Select(x =>
                        {
                            var name =
                                x.Specification?
                                    .SpecificationName;

                            var value =
                                x.SpecificationValue;

                            var uom =
                                x.Uom?
                                    .UomName;


                            var valueText =
                                string.IsNullOrWhiteSpace(
                                    uom)
                                    ? value
                                    : $"{value} {uom}";


                            return string.IsNullOrWhiteSpace(
                                name)
                                ? valueText
                                : $"{name}: {valueText}";
                        })
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x)));


            var drawings =
                await _drawingService
                    .GetByItemIdAsync(
                        itemId);


            var currentDrawing =
                drawings
                    .FirstOrDefault(x =>
                        x.IsActive &&
                        !x.IsDeleted);


            return Json(
                new
                {
                    success = true,

                    itemCode =
                        item.ItemCode,

                    itemName =
                        item.ItemName,

                    description =
                        item.Description,

                    unitName =
                        item.Uom?.UomName,

                    specification =
                        specificationText,

                    drawing =
                        currentDrawing == null
                            ? null
                            : new
                            {
                                drawingId =
                                    currentDrawing.DrawingId,

                                drawingNumber =
                                    currentDrawing.DrawingNumber,

                                revisionNumber =
                                    currentDrawing.RevisionNumber
                            }
                });
        }

        

        #endregion


        #region Dropdown Loading

        private async Task LoadDropdownsAsync(
            PurchaseOrderViewModel model)
        {
            #region Company

            var companies =
                await _companyService
                    .GetAllAsync();


            model.Companies =
                companies
                    .Where(x =>
                        x.IsActive ||
                        x.CompanyId ==
                        model.CompanyId)
                    .OrderBy(x =>
                        x.CompanyName)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.CompanyId
                                    .ToString(),

                            Text =
                                $"{x.CompanyCode} - " +
                                $"{x.CompanyName}",

                            Selected =
                                x.CompanyId ==
                                model.CompanyId
                        })
                    .ToList();


            /*
             * If ERP currently has only one active
             * Company, select it automatically on Create.
             */
            if (model.CompanyId <= 0)
            {
                var activeCompanies =
                    companies
                        .Where(x =>
                            x.IsActive)
                        .ToList();

                if (activeCompanies.Count == 1)
                {
                    model.CompanyId =
                        activeCompanies[0]
                            .CompanyId;

                    foreach (var option
                        in model.Companies)
                    {
                        option.Selected =
                            option.Value ==
                            model.CompanyId
                                .ToString();
                    }
                }
            }

            #endregion


            #region Supplier

            model.Suppliers =
                (await _supplierService
                    .GetAllAsync())
                .Where(x =>
                    x.IsActive ||
                    x.SupplierId ==
                    model.SupplierId)
                .OrderBy(x =>
                    x.SupplierName)
                .Select(x =>
                    new SelectListItem
                    {
                        Value =
                            x.SupplierId
                                .ToString(),

                        Text =
                            $"{x.SupplierCode} - " +
                            $"{x.SupplierName}",

                        Selected =
                            x.SupplierId ==
                            model.SupplierId
                    })
                .ToList();

            #endregion


            #region Item

            var selectedItemIds =
                model.Items
                    .Where(x =>
                        x.ItemId > 0)
                    .Select(x =>
                        x.ItemId)
                    .ToHashSet();


            model.ItemOptions =
                (await _itemService
                    .GetAllAsync())
                .Where(x =>
                    x.IsActive ||
                    selectedItemIds.Contains(
                        x.ItemId))
                .OrderBy(x =>
                    x.ItemName)
                .Select(x =>
                    new SelectListItem
                    {
                        Value =
                            x.ItemId
                                .ToString(),

                        Text =
                            $"{x.ItemCode} - " +
                            $"{x.ItemName}"
                    })
                .ToList();

            #endregion
        }

        #endregion


        #region Drawing Loading

        private async Task LoadDrawingOptionsAsync(
            PurchaseOrderViewModel model)
        {
            if (model.Items == null ||
                model.Items.Count == 0)
            {
                return;
            }


            foreach (var row
                in model.Items)
            {
                row.DrawingOptions =
                    new List<SelectListItem>();


                if (row.ItemId <= 0)
                {
                    continue;
                }


                var drawings =
                    await _drawingService
                        .GetByItemIdAsync(
                            row.ItemId);


                row.DrawingOptions =
                    drawings
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)
                        .OrderBy(x =>
                            x.DrawingNumber)
                        .Select(x =>
                            new SelectListItem
                            {
                                Value =
                                    x.DrawingId
                                        .ToString(),

                                Text =
                                    $"{x.DrawingNumber} - " +
                                    $"{x.RevisionNumber}",

                                Selected =
                                    row.DrawingId ==
                                    x.DrawingId
                            })
                        .ToList();
            }
        }

        #endregion


        #region Entity Mapping

        private static PurchaseOrder MapToEntity(
            PurchaseOrderViewModel model)
        {
            var purchaseOrder =
                new PurchaseOrder
                {
                    Id =
                        model.Id,

                    Code =
                        model.Code
                        ?? string.Empty,

                    PODate =
                        model.PODate,

                    ExpectedDeliveryDate =
                        model.ExpectedDeliveryDate,

                    CompanyId =
                        model.CompanyId,

                    SupplierId =
                        model.SupplierId,

                    DeliveryAddress =
                        model.DeliveryAddress,

                    PaymentTerms =
                        model.PaymentTerms,

                    DeliveryTerms =
                        model.DeliveryTerms,

                    Remarks =
                        model.Remarks,

                    TransportCharges =
                        model.TransportCharges,

                    OtherCharges =
                        model.OtherCharges,

                    RoundOffAmount =
                        model.RoundOffAmount,

                    IsActive =
                        model.IsActive
                };


            purchaseOrder.Items =
                model.Items
                    .Select(x =>
                        new PurchaseOrderItem
                        {
                            Id =
                                x.Id,

                            ItemId =
                                x.ItemId,

                            DrawingId =
                                NormalizeDrawingId(
                                    x.DrawingId),

                            HSNCode =
                                x.HSNCode,

                            Quantity =
                                x.Quantity,

                            UnitPrice =
                                x.UnitPrice,

                            DiscountPercent =
                                x.DiscountPercent,

                            GSTPercent =
                                x.GSTPercent,

                            RequiredDate =
                                x.RequiredDate,

                            Remarks =
                                x.Remarks
                        })
                    .ToList();


            return purchaseOrder;
        }


        private static PurchaseOrderViewModel
            MapToViewModel(
                PurchaseOrder purchaseOrder)
        {
            return new PurchaseOrderViewModel
            {
                Id =
                    purchaseOrder.Id,

                Code =
                    purchaseOrder.Code,

                PODate =
                    purchaseOrder.PODate,

                ExpectedDeliveryDate =
                    purchaseOrder.ExpectedDeliveryDate,

                Status =
                    purchaseOrder.Status,

                CompanyId =
                    purchaseOrder.CompanyId,

                SupplierId =
                    purchaseOrder.SupplierId,

                DeliveryAddress =
                    purchaseOrder.DeliveryAddress,

                PaymentTerms =
                    purchaseOrder.PaymentTerms,

                DeliveryTerms =
                    purchaseOrder.DeliveryTerms,

                Remarks =
                    purchaseOrder.Remarks,

                TransportCharges =
                    purchaseOrder.TransportCharges,

                OtherCharges =
                    purchaseOrder.OtherCharges,

                RoundOffAmount =
                    purchaseOrder.RoundOffAmount,

                SubTotal =
                    purchaseOrder.SubTotal,

                DiscountAmount =
                    purchaseOrder.DiscountAmount,

                TaxableAmount =
                    purchaseOrder.TaxableAmount,

                CGSTAmount =
                    purchaseOrder.CGSTAmount,

                SGSTAmount =
                    purchaseOrder.SGSTAmount,

                IGSTAmount =
                    purchaseOrder.IGSTAmount,

                GrandTotal =
                    purchaseOrder.GrandTotal,

                IsActive =
                    purchaseOrder.IsActive,

                Items =
                    purchaseOrder.Items
                        .Where(x =>
                            !x.IsDeleted)
                        .OrderBy(x =>
                            x.Id)
                        .Select(x =>
                            new PurchaseOrderItemViewModel
                            {
                                Id =
                                    x.Id,

                                ItemId =
                                    x.ItemId,

                                ItemCode =
                                    x.ItemCode,

                                ItemName =
                                    x.ItemName,

                                Description =
                                    x.Description,

                                Specification =
                                    x.Specification,

                                UnitName =
                                    x.UnitName,

                                HSNCode =
                                    x.HSNCode,

                                DrawingId =
                                    x.DrawingId,

                                DrawingNumber =
                                    x.DrawingNumber,

                                DrawingRevision =
                                    x.DrawingRevision,

                                Quantity =
                                    x.Quantity,

                                UnitPrice =
                                    x.UnitPrice,

                                DiscountPercent =
                                    x.DiscountPercent,

                                DiscountAmount =
                                    x.DiscountAmount,

                                TaxableAmount =
                                    x.TaxableAmount,

                                GSTPercent =
                                    x.GSTPercent,

                                CGSTAmount =
                                    x.CGSTAmount,

                                SGSTAmount =
                                    x.SGSTAmount,

                                IGSTAmount =
                                    x.IGSTAmount,

                                LineTotal =
                                    x.LineTotal,

                                RequiredDate =
                                    x.RequiredDate,

                                Remarks =
                                    x.Remarks
                            })
                        .ToList()
            };
        }

        #endregion


        #region Model Normalization

        private static void NormalizeModel(
            PurchaseOrderViewModel model)
        {
            model.DeliveryAddress =
                NormalizeText(
                    model.DeliveryAddress);

            model.PaymentTerms =
                NormalizeText(
                    model.PaymentTerms);

            model.DeliveryTerms =
                NormalizeText(
                    model.DeliveryTerms);

            model.Remarks =
                NormalizeText(
                    model.Remarks);


            model.Items ??=
                new List<
                    PurchaseOrderItemViewModel>();


            foreach (var row
                in model.Items)
            {
                row.HSNCode =
                    NormalizeUpperText(
                        row.HSNCode);

                row.Remarks =
                    NormalizeText(
                        row.Remarks);

                row.DrawingId =
                    NormalizeDrawingId(
                        row.DrawingId);
            }
        }


        private static string? NormalizeText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }


        private static string? NormalizeUpperText(
            string? value)
        {
            return NormalizeText(
                value)?
                .ToUpperInvariant();
        }


        private static int? NormalizeDrawingId(
            int? drawingId)
        {
            return drawingId.HasValue &&
                   drawingId.Value > 0
                ? drawingId.Value
                : null;
        }

        #endregion

        #region Purchase Order PDF

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(
            int id)
        {
            try
            {
                var purchaseOrder =
                    await _purchaseOrderService
                        .GetByIdAsync(
                            id);


                if (purchaseOrder == null)
                {
                    return NotFound();
                }


                var pdfBytes =
                    _purchaseOrderPdfService
                        .GeneratePdf(
                            purchaseOrder);


                var fileName =
                    string.IsNullOrWhiteSpace(
                        purchaseOrder.Code)
                        ? $"Purchase-Order-{purchaseOrder.Id}.pdf"
                        : purchaseOrder.Code
                            .Replace(
                                "/",
                                "-") +
                          ".pdf";


                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName);
            }
            catch (BusinessException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to generate Purchase Order PDF.";
            }


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        #endregion
    }
}