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
- Confirm Draft Customer Purchase Orders.
- Soft-delete Draft Customer Purchase Orders.
- Load Customer and Item Master dropdowns.
- Provide Item Master information through AJAX.
- Map Web ViewModels to Domain entities.
- Display business and validation errors through shared Toast.

Important:
- Business logic belongs in CustomerPurchaseOrderService.
- Database access must never occur directly in Controller.
- Existing Customer Master and Item Master are reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
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

        #endregion


        #region Constructor

        public CustomerPurchaseOrderController(
            ICustomerPurchaseOrderService
                customerPurchaseOrderService)
        {
            _customerPurchaseOrderService =
                customerPurchaseOrderService;
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
        public async Task<IActionResult> Details(
            int id)
        {
            var customerPurchaseOrder =
                await _customerPurchaseOrderService
                    .GetByIdAsync(id);


            if (customerPurchaseOrder == null)
            {
                return NotFound();
            }


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
                    MapToDomain(model);


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
                        .GetByIdAsync(id);


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
                    MapToDomain(model);


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
                    .ConfirmAsync(id);


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
                    .DeleteAsync(id);


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

       

        #region Item AJAX

        [HttpGet]
        public async Task<IActionResult> GetItemData(
            int itemId)
        {
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


            var specification =
                BuildSpecificationDisplay(
                    item);


            return Json(
                new
                {
                    success = true,

                    itemId =
                        item.ItemId,

                    itemCode =
                        item.ItemCode,

                    itemName =
                        item.ItemName,

                    unitName =
                        item.Uom?.UomName
                        ?? "",

                    specification
                });
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
                                $"{x.Code} - {x.CustomerName}",

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
                                $"{x.ItemCode} - {x.ItemName}"
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

                        CustomerDrawingNumber =
                            item.CustomerDrawingNumber,

                        Revision =
                            item.Revision,

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

                                CustomerDrawingNumber =
                                    x.CustomerDrawingNumber,

                                Revision =
                                    x.Revision,

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
                            x.Uom?.UomName;


                        var valueWithUom =
                            string.IsNullOrWhiteSpace(
                                uom)
                                ? value
                                : $"{value} {uom}";


                        return string.IsNullOrWhiteSpace(
                            name)
                                ? valueWithUom
                                : $"{name}: {valueWithUom}";
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
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
                        !string.IsNullOrWhiteSpace(x))
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
                await _customerPurchaseOrderService.RestoreAsync(id);


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