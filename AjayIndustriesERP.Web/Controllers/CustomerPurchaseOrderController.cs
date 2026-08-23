/*
============================================================
File: CustomerPurchaseOrderController.cs

Purpose:
Handles Customer Purchase Order HTTP requests.

Responsibilities:
- Display Customer PO Index with Search + Pagination.
- Display Customer PO Details.
- Create Draft Customer Purchase Orders.
- Edit Draft Customer Purchase Orders.
- Confirm Customer Purchase Orders.
- Soft-delete Customer Purchase Orders.
- Restore deleted Customer Purchase Orders.
- Load Customer and Item Master dropdowns.
- Provide Item Master information through AJAX.
- Provide current Workshop Drawing through AJAX.
- Provide current Customer Drawing through AJAX.
- Map Web ViewModels to Domain entities.
- Display business and validation errors through shared Toast.

Important:
- Business logic belongs in CustomerPurchaseOrderService.
- Database access must never occur directly in Controller.
- Existing Customer Master and Item Master are reused.
- Workshop Drawing is resolved by Item.
- Customer Drawing is resolved by Customer + Item.
- Customer Drawing Number / Revision posted from browser
  are NOT trusted.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.CustomerPurchaseOrder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class CustomerPurchaseOrderController
        : Controller
    {
        #region Fields

        private readonly
            ICustomerPurchaseOrderService
            _customerPurchaseOrderService;

        private readonly
            ICustomerDrawingService
            _customerDrawingService;

        #endregion


        #region Constructor

        public CustomerPurchaseOrderController(
            ICustomerPurchaseOrderService
                customerPurchaseOrderService,
            ICustomerDrawingService
                customerDrawingService)
        {
            _customerPurchaseOrderService =
                customerPurchaseOrderService;

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
                await _customerPurchaseOrderService
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
        [HttpGet]
        public async Task<IActionResult> Details(
    int id)
        {
            var customerPurchaseOrder =
                await _customerPurchaseOrderService
                    .GetByIdAsync(
                        id);


            if (customerPurchaseOrder == null)
            {
                return NotFound();
            }


            // =========================================================
            // CURRENT CUSTOMER DRAWINGS
            // =========================================================

            /*
             * Customer PO Details should display the CURRENT
             * Customer Drawing exactly like the Workshop Drawing.
             *
             * Customer Drawing is resolved using:
             *
             * Customer + Item
             */

            var currentCustomerDrawings =
                new Dictionary<int, CustomerDrawing>();


            foreach (var item
                in customerPurchaseOrder.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive))
            {
                var customerDrawing =
                    await _customerDrawingService
                        .GetByCustomerAndItemAsync(
                            customerPurchaseOrder.CustomerId,
                            item.ItemId);


                if (customerDrawing == null)
                {
                    continue;
                }


                currentCustomerDrawings[
                    item.Id
                ] =
                    customerDrawing;
            }


            ViewBag.CurrentCustomerDrawings =
                currentCustomerDrawings;


            return View(
                customerPurchaseOrder);
        }

        #endregion


        #region Create

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new CustomerPurchaseOrderFormViewModel
                {
                    CustomerPurchaseOrderDate =
                        DateTime.Today,

                    ReceivedDate =
                        DateTime.Today,

                    RequiredDeliveryDate =
                        DateTime.Today,

                    Priority =
                        CustomerPurchaseOrderPriority.Normal,

                    Status =
                        CustomerPurchaseOrderStatus.Draft
                };


            await LoadDropdownDataAsync(
                model);


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerPurchaseOrderFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                await LoadDropdownDataAsync(
                    model);


                return View(model);
            }


            try
            {
                var customerPurchaseOrder =
                    MapToDomain(
                        model);


                await _customerPurchaseOrderService
                    .CreateAsync(
                        customerPurchaseOrder);


                TempData["SuccessMessage"] =
                    "Customer Purchase Order created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadDropdownDataAsync(
                    model);


                return View(model);
            }
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            try
            {
                var customerPurchaseOrder =
                    await _customerPurchaseOrderService
                        .GetByIdAsync(
                            id);


                if (customerPurchaseOrder == null)
                {
                    return NotFound();
                }


                if (customerPurchaseOrder.Status !=
                    CustomerPurchaseOrderStatus.Draft)
                {
                    TempData["ErrorMessage"] =
                        "Only Draft Customer Purchase Orders can be edited.";


                    return RedirectToAction(
                        nameof(Details),
                        new
                        {
                            id
                        });
                }


                var model =
                    MapToFormViewModel(
                        customerPurchaseOrder);


                await LoadDropdownDataAsync(
                    model);


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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CustomerPurchaseOrderFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                await LoadDropdownDataAsync(
                    model);


                return View(model);
            }


            try
            {
                var customerPurchaseOrder =
                    MapToDomain(
                        model);


                await _customerPurchaseOrderService
                    .UpdateAsync(
                        customerPurchaseOrder);


                TempData["SuccessMessage"] =
                    "Customer Purchase Order updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                await LoadDropdownDataAsync(
                    model);


                return View(model);
            }
        }

        #endregion


        #region Confirm

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            int id)
        {
            try
            {
                await _customerPurchaseOrderService
                    .ConfirmAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Customer Purchase Order confirmed successfully.";
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


        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _customerPurchaseOrderService
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Customer Purchase Order deleted successfully.";
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


        #region Customer PO Number Similarity Check

        [HttpGet]
        public async Task<IActionResult>
            CheckCustomerPurchaseOrderNumber(
                int customerId,
                string customerPurchaseOrderNumber,
                int? excludeId = null)
        {
            if (
                customerId <= 0 ||
                string.IsNullOrWhiteSpace(
                    customerPurchaseOrderNumber) ||
                customerPurchaseOrderNumber
                    .Trim()
                    .Length < 3
            )
            {
                return Json(
                    new
                    {
                        hasSimilarOrders = false,
                        hasExactMatch = false,
                        orders =
                            Array.Empty<string>()
                    });
            }


            var searchText =
                customerPurchaseOrderNumber
                    .Trim();


            var allOrders =
                await _customerPurchaseOrderService
                    .GetAllAsync();


            var matchingOrders =
                allOrders
                    .Where(x =>
                        x.CustomerId ==
                            customerId
                        &&
                        (
                            !excludeId.HasValue ||
                            x.Id !=
                                excludeId.Value
                        )
                        &&
                        IsSimilarCustomerPoNumber(
                            searchText,
                            x.CustomerPurchaseOrderNumber
                        )
                    )
                    .OrderByDescending(x =>
                        string.Equals(
                            x.CustomerPurchaseOrderNumber
                                ?.Trim(),
                            searchText,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .ThenByDescending(x =>
                        x.CustomerPurchaseOrderDate
                    )
                    .Take(10)
                    .ToList();


            /*
             * Exact means genuinely the same entered PO Number.
             *
             * Formatting variants such as:
             *
             * ABC-PO-03
             * ABC - PO - 03
             *
             * are treated as Similar, not Exact.
             */

            var hasExactMatch =
                matchingOrders.Any(x =>
                    string.Equals(
                        x.CustomerPurchaseOrderNumber
                            ?.Trim(),
                        searchText,
                        StringComparison.OrdinalIgnoreCase
                    )
                );


            var orders =
                matchingOrders
                    .Select(x =>
                        $"{x.CustomerPurchaseOrderNumber}" +
                        $" | " +
                        $"{x.CustomerPurchaseOrderDate:dd-MM-yyyy}"
                    )
                    .ToList();


            return Json(
                new
                {
                    hasSimilarOrders =
                        orders.Count > 0,

                    hasExactMatch,

                    orders
                });
        }

        #endregion


        #region Customer PO Number Similarity Helpers

        private static bool IsSimilarCustomerPoNumber(
            string searchText,
            string? existingPoNumber)
        {
            if (string.IsNullOrWhiteSpace(
                existingPoNumber))
            {
                return false;
            }


            var search =
                ParseCustomerPoNumber(
                    searchText);


            var existing =
                ParseCustomerPoNumber(
                    existingPoNumber);


            if (
                string.IsNullOrWhiteSpace(
                    search.CompactValue) ||
                string.IsNullOrWhiteSpace(
                    existing.CompactValue)
            )
            {
                return false;
            }


            // =================================================
            // RULE 1
            // Same value after removing spaces / - / _ / symbols.
            // =================================================

            if (
                string.Equals(
                    search.NormalizedValue,
                    existing.NormalizedValue,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }


            // =================================================
            // RULE 2
            // Typed normalized value exists in existing PO.
            // =================================================

            if (
                search.CompactValue.Length >= 3 &&
                existing.CompactValue.Contains(
                    search.CompactValue,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }


            // =================================================
            // RULE 3
            // Same numeric suffix + related alphabetic prefix.
            // =================================================

            if (
                search.NumberPart.HasValue &&
                existing.NumberPart.HasValue &&
                search.NumberPart.Value ==
                    existing.NumberPart.Value &&
                !string.IsNullOrWhiteSpace(
                    search.LetterPart) &&
                !string.IsNullOrWhiteSpace(
                    existing.LetterPart)
            )
            {
                var relatedPrefix =
                    search.LetterPart.StartsWith(
                        existing.LetterPart,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    existing.LetterPart.StartsWith(
                        search.LetterPart,
                        StringComparison.OrdinalIgnoreCase
                    );


                if (relatedPrefix)
                {
                    return true;
                }
            }


            return false;
        }


        private static CustomerPoNumberParts
            ParseCustomerPoNumber(
                string value)
        {
            var compact =
                new string(
                    value
                        .Where(
                            char.IsLetterOrDigit)
                        .Select(
                            char.ToUpperInvariant)
                        .ToArray()
                );


            if (string.IsNullOrWhiteSpace(
                compact))
            {
                return new CustomerPoNumberParts();
            }


            var numberStart =
                compact.Length;


            while (
                numberStart > 0 &&
                char.IsDigit(
                    compact[
                        numberStart - 1])
            )
            {
                numberStart--;
            }


            var letterPart =
                compact.Substring(
                    0,
                    numberStart);


            var numberText =
                compact.Substring(
                    numberStart);


            int? numberPart =
                null;


            if (
                !string.IsNullOrWhiteSpace(
                    numberText) &&
                int.TryParse(
                    numberText,
                    out var parsedNumber)
            )
            {
                numberPart =
                    parsedNumber;
            }


            var normalizedValue =
                numberPart.HasValue
                    ? letterPart +
                      numberPart.Value
                    : compact;


            return new CustomerPoNumberParts
            {
                CompactValue =
                    compact,

                NormalizedValue =
                    normalizedValue,

                LetterPart =
                    letterPart,

                NumberPart =
                    numberPart
            };
        }


        private sealed class CustomerPoNumberParts
        {
            public string CompactValue { get; set; } =
                string.Empty;

            public string NormalizedValue { get; set; } =
                string.Empty;

            public string LetterPart { get; set; } =
                string.Empty;

            public int? NumberPart { get; set; }
        }

        #endregion


        #region Item AJAX

        [HttpGet]
        public async Task<IActionResult> GetItemData(
            int itemId,
            int customerId = 0)
        {
            #region Validation

            if (itemId <= 0)
            {
                return Json(
                    new
                    {
                        success = false,

                        message =
                            "Invalid Item."
                    });
            }

            #endregion


            #region Load Item

            var item =
                await _customerPurchaseOrderService
                    .GetItemForOrderAsync(
                        itemId);


            if (item == null)
            {
                return Json(
                    new
                    {
                        success = false,

                        message =
                            "Item not found."
                    });
            }

            #endregion


            #region Specification

            var specification =
                BuildSpecificationDisplay(
                    item);

            #endregion


            #region Current Workshop Drawing

            /*
             * Workshop Drawing belongs to Item.
             *
             * Existing Drawing Master behaviour
             * remains unchanged.
             */

            var currentDrawing =
                item.Drawings
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderByDescending(x =>
                        x.DrawingId)
                    .FirstOrDefault();

            #endregion


            #region Current Customer Drawing

            /*
             * Customer Drawing belongs to:
             *
             * Customer + Item
             *
             * Customer may still be empty while
             * user is preparing the form.
             */

            CustomerDrawing?
                currentCustomerDrawing =
                    null;


            if (customerId > 0)
            {
                currentCustomerDrawing =
                    await _customerDrawingService
                        .GetByCustomerAndItemAsync(
                            customerId,
                            itemId);
            }

            #endregion


            #region Response

            return Json(
                new
                {
                    success = true,


                    // =========================================
                    // ITEM INFORMATION
                    // =========================================

                    itemId =
                        item.ItemId,

                    itemCode =
                        item.ItemCode,

                    itemName =
                        item.ItemName,

                    unitName =
                        item.Uom?.UomName
                        ?? "",

                    specification,


                    // =========================================
                    // WORKSHOP DRAWING
                    // Existing names intentionally preserved.
                    // =========================================

                    drawingId =
                        currentDrawing?
                            .DrawingId,

                    drawingNumber =
                        currentDrawing?
                            .DrawingNumber
                        ?? "",

                    drawingName =
                        currentDrawing?
                            .DrawingName
                        ?? "",

                    drawingType =
                        currentDrawing?
                            .DrawingType
                        ?? "",

                    drawingRevision =
                        currentDrawing?
                            .RevisionNumber
                        ?? "",

                    drawingFileName =
                        currentDrawing?
                            .FileName
                        ?? "",

                    drawingFilePath =
                        currentDrawing?
                            .FilePath
                        ?? "",

                    drawingDescription =
                        currentDrawing?
                            .Description
                        ?? "",


                    // =========================================
                    // CUSTOMER DRAWING
                    // Current revision for Customer + Item.
                    // =========================================

                    customerDrawingId =
                        currentCustomerDrawing?
                            .CustomerDrawingId,

                    customerDrawingNumber =
                        currentCustomerDrawing?
                            .DrawingNumber
                        ?? "",

                    customerDrawingName =
                        currentCustomerDrawing?
                            .DrawingName
                        ?? "",

                    customerDrawingType =
                        currentCustomerDrawing?
                            .DrawingType
                        ?? "",

                    customerDrawingRevision =
                        currentCustomerDrawing?
                            .RevisionNumber
                        ?? "",

                    customerDrawingFileName =
                        currentCustomerDrawing?
                            .FileName
                        ?? "",

                    customerDrawingFilePath =
                        currentCustomerDrawing?
                            .FilePath
                        ?? "",

                    customerDrawingDescription =
                        currentCustomerDrawing?
                            .Description
                        ?? ""
                });

            #endregion
        }

        #endregion


        #region Dropdown Loading

        private async Task LoadDropdownDataAsync(
            CustomerPurchaseOrderFormViewModel model)
        {
            var customers =
                await _customerPurchaseOrderService
                    .GetCustomersForOrderAsync();


            var items =
                await _customerPurchaseOrderService
                    .GetItemsForOrderAsync();


            model.Customers =
                customers
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                $"{x.Code} - " +
                                $"{x.CustomerName}",

                            Selected =
                                x.Id ==
                                model.CustomerId
                        })
                    .ToList();


            model.AvailableItems =
                items
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.ItemId.ToString(),

                            Text =
                                $"{x.ItemCode} - " +
                                $"{x.ItemName}"
                        })
                    .ToList();
        }

        #endregion


        #region Domain Mapping

        private static CustomerPurchaseOrder
            MapToDomain(
                CustomerPurchaseOrderFormViewModel model)
        {
            var customerPurchaseOrder =
                new CustomerPurchaseOrder
                {
                    Id =
                        model.Id,

                    Code =
                        model.Code
                        ?? string.Empty,

                    CustomerId =
                        model.CustomerId,

                    CustomerName =
                        model.CustomerName
                        ?? string.Empty,

                    CustomerPurchaseOrderNumber =
                        model.CustomerPurchaseOrderNumber,

                    CustomerPurchaseOrderDate =
                        model.CustomerPurchaseOrderDate,

                    ReceivedDate =
                        model.ReceivedDate,

                    RequiredDeliveryDate =
                        model.RequiredDeliveryDate,

                    Priority =
                        model.Priority,

                    Status =
                        model.Status,

                    CustomerReference =
                        model.CustomerReference,

                    Remarks =
                        model.Remarks
                };


            foreach (var item
                in model.Items)
            {
                customerPurchaseOrder.Items.Add(
                    new CustomerPurchaseOrderItem
                    {
                        Id =
                            item.Id,

                        ItemId =
                            item.ItemId,

                        /*
                         * ItemCode / ItemName /
                         * Specification / UnitName are
                         * mapped only for request transport.
                         *
                         * Application Service reloads
                         * trusted Item Master data.
                         */

                        ItemCode =
                            item.ItemCode,

                        ItemName =
                            item.ItemName,

                        Specification =
                            item.Specification,

                        UnitName =
                            item.UnitName,

                        CustomerItemCode =
                            item.CustomerItemCode,

                        /*
                         * CustomerDrawingNumber and Revision
                         * are intentionally NOT mapped.
                         *
                         * Application Service resolves them
                         * from current Customer Drawing using:
                         *
                         * Customer + Item
                         */

                        OrderedQuantity =
                            item.OrderedQuantity,

                        RequiredDeliveryDate =
                            item.RequiredDeliveryDate,

                        Priority =
                            item.Priority,

                        Remarks =
                            item.Remarks
                    });
            }


            return customerPurchaseOrder;
        }

        #endregion


        #region Form ViewModel Mapping

        private static CustomerPurchaseOrderFormViewModel
            MapToFormViewModel(
                CustomerPurchaseOrder
                    customerPurchaseOrder)
        {
            return new CustomerPurchaseOrderFormViewModel
            {
                Id =
                    customerPurchaseOrder.Id,

                Code =
                    customerPurchaseOrder.Code,

                CustomerId =
                    customerPurchaseOrder.CustomerId,

                CustomerName =
                    customerPurchaseOrder.CustomerName,

                CustomerPurchaseOrderNumber =
                    customerPurchaseOrder
                        .CustomerPurchaseOrderNumber,

                CustomerPurchaseOrderDate =
                    customerPurchaseOrder
                        .CustomerPurchaseOrderDate,

                ReceivedDate =
                    customerPurchaseOrder
                        .ReceivedDate,

                RequiredDeliveryDate =
                    customerPurchaseOrder
                        .RequiredDeliveryDate,

                Priority =
                    customerPurchaseOrder.Priority,

                Status =
                    customerPurchaseOrder.Status,

                CustomerReference =
                    customerPurchaseOrder
                        .CustomerReference,

                Remarks =
                    customerPurchaseOrder.Remarks,

                Items =
                    customerPurchaseOrder.Items
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive)
                        .OrderBy(x =>
                            x.Id)
                        .Select(x =>
                            new CustomerPurchaseOrderItemViewModel
                            {
                                Id =
                                    x.Id,

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

                                CustomerItemCode =
                                    x.CustomerItemCode,

                                /*
                                 * Customer Drawing snapshot
                                 * is not placed back into editable
                                 * form fields.
                                 *
                                 * Edit page reloads the current
                                 * Customer Drawing through AJAX.
                                 */

                                OrderedQuantity =
                                    x.OrderedQuantity,

                                RequiredDeliveryDate =
                                    x.RequiredDeliveryDate,

                                Priority =
                                    x.Priority,

                                Remarks =
                                    x.Remarks
                            })
                        .ToList()
            };
        }

        #endregion


        #region Item Display Helper

        private static string BuildSpecificationDisplay(
            Item item)
        {
            if (item.ItemSpecifications == null ||
                !item.ItemSpecifications.Any())
            {
                return string.Empty;
            }


            var rows =
                item.ItemSpecifications
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


                        var valueWithUom =
                            string.IsNullOrWhiteSpace(
                                uom)
                                ? value
                                : $"{value} {uom}";


                        return string.IsNullOrWhiteSpace(
                            name)
                                ? valueWithUom
                                : $"{name}: " +
                                  $"{valueWithUom}";
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x))
                    .ToList();


            return string.Join(
                " | ",
                rows);
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
                        !string.IsNullOrWhiteSpace(
                            x))
                    .Distinct()
                    .ToList();


            if (!errors.Any())
            {
                return
                    "Please correct the validation errors.";
            }


            return string.Join(
                " • ",
                errors);
        }

        #endregion


        #region Deleted Customer Purchase Orders

        [HttpGet]
        public async Task<IActionResult>
            Deleted()
        {
            var orders =
                await _customerPurchaseOrderService
                    .GetDeletedAsync();


            return View(
                orders);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            Restore(
                int id)
        {
            try
            {
                await _customerPurchaseOrderService
                    .RestoreAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Customer Purchase Order restored successfully.";
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
    }
}