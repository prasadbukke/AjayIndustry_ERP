/*
============================================================
File: CustomerReceiptController.cs

Module:
Customer Receipt

Purpose:
Handles Web requests for Customer Receipt module.

Responsibilities:
- List and search Customer Receipts.
- Create Customer Receipt.
- Load Customer outstanding Invoices through AJAX.
- Edit Draft Customer Receipt.
- Display Customer Receipt Details.
- Finalize Customer Receipt.
- Download Finalized Receipt PDF.
- Soft-delete Draft Receipt.
- Display deleted Receipts.
- Restore Draft Receipt.

Important:
- Business logic remains in CustomerReceiptService.
- Controller does not access database directly.
- Browser-posted Invoice totals / outstanding amounts
  are NOT trusted.
- Only InvoiceId and AllocatedAmount are commercial
  allocation inputs.
- Service layer performs authoritative validation.
============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.CustomerReceipt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class CustomerReceiptController
        : Controller
    {
        #region Fields

        private readonly ICustomerReceiptService
            _customerReceiptService;

        #endregion


        #region Constructor

        public CustomerReceiptController(
            ICustomerReceiptService customerReceiptService)
        {
            _customerReceiptService =
                customerReceiptService;
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
                await _customerReceiptService
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
        public async Task<IActionResult> Create()
        {
            var viewModel =
                new CustomerReceiptFormViewModel
                {
                    ReceiptDate =
                        DateTime.Today,

                    PaymentMode =
                        PaymentMode.BankTransfer
                };


            await PopulateCustomersAsync(
                viewModel);


            return View(
                viewModel);
        }

        #endregion


        #region Create POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerReceiptFormViewModel viewModel)
        {
            #region Model Validation

            if (!ModelState.IsValid)
            {
                await RefreshAllocationSnapshotsAsync(
                    viewModel);


                await PopulateCustomersAsync(
                    viewModel);


                return View(
                    viewModel);
            }

            #endregion


            try
            {
                var customerReceipt =
                    MapToDomain(
                        viewModel);


                var createdReceipt =
                    await _customerReceiptService
                        .CreateAsync(
                            customerReceipt);


                TempData["SuccessMessage"] =
                    $"Customer Receipt {createdReceipt.Code} created successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            createdReceipt.Id
                    });
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);


                await RefreshAllocationSnapshotsAsync(
                    viewModel);


                await PopulateCustomersAsync(
                    viewModel);


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Customer Invoice AJAX

        [HttpGet]
        public async Task<IActionResult>
            GetCustomerInvoices(
                int customerId,
                int? customerReceiptId = null)
        {
            try
            {
                if (customerId <= 0)
                {
                    return Json(
                        new
                        {
                            success = false,

                            message =
                                "Please select a Customer."
                        });
                }


                #region Outstanding Invoices

                var invoices =
                    await _customerReceiptService
                        .GetOutstandingInvoicesForCustomerAsync(
                            customerId,
                            customerReceiptId);

                #endregion


                #region Invoice Results

                var invoiceResults =
                    new List<object>();


                foreach (var invoice
                    in invoices
                        .OrderBy(x =>
                            x.InvoiceDate)
                        .ThenBy(x =>
                            x.Id))
                {
                    var outstandingAmount =
                        await _customerReceiptService
                            .GetInvoiceOutstandingAsync(
                                invoice.Id,
                                customerReceiptId);


                    if (outstandingAmount <= 0)
                    {
                        continue;
                    }


                    var alreadyReceivedAmount =
                        invoice.GrandTotal -
                        outstandingAmount;


                    if (alreadyReceivedAmount < 0)
                    {
                        alreadyReceivedAmount =
                            0;
                    }


                    invoiceResults.Add(
                        new
                        {
                            invoiceId =
                                invoice.Id,

                            invoiceCode =
                                invoice.Code,

                            invoiceDate =
                                invoice.InvoiceDate
                                    .ToString(
                                        "yyyy-MM-dd"),

                            invoiceGrandTotal =
                                invoice.GrandTotal,

                            alreadyReceivedAmount,

                            outstandingAmount
                        });
                }

                #endregion


                return Json(
                    new
                    {
                        success = true,

                        customerId,

                        invoices =
                            invoiceResults
                    });
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
            var customerReceipt =
                await _customerReceiptService
                    .GetByIdAsync(
                        id);


            if (customerReceipt == null)
            {
                return NotFound();
            }


            if (customerReceipt.Status !=
                CustomerReceiptStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Finalized Customer Receipt cannot be edited.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            var viewModel =
                await MapToFormViewModelAsync(
                    customerReceipt);


            await PopulateCustomersAsync(
                viewModel);


            return View(
                viewModel);
        }

        #endregion


        #region Edit POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CustomerReceiptFormViewModel viewModel)
        {
            if (viewModel.Id <= 0)
            {
                return BadRequest();
            }


            #region Model Validation

            if (!ModelState.IsValid)
            {
                await RefreshAllocationSnapshotsAsync(
                    viewModel,
                    viewModel.Id);


                await PopulateCustomersAsync(
                    viewModel);


                return View(
                    viewModel);
            }

            #endregion


            try
            {
                var customerReceipt =
                    MapToDomain(
                        viewModel);


                var updatedReceipt =
                    await _customerReceiptService
                        .UpdateAsync(
                            customerReceipt);


                TempData["SuccessMessage"] =
                    $"Customer Receipt {updatedReceipt.Code} updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            updatedReceipt.Id
                    });
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);


                await RefreshAllocationSnapshotsAsync(
                    viewModel,
                    viewModel.Id);


                await PopulateCustomersAsync(
                    viewModel);


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
            var customerReceipt =
                await _customerReceiptService
                    .GetByIdAsync(
                        id);


            if (customerReceipt == null)
            {
                return NotFound();
            }


            var viewModel =
                MapToDetailsViewModel(
                    customerReceipt);


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
                var customerReceipt =
                    await _customerReceiptService
                        .FinalizeAsync(
                            id);


                TempData["SuccessMessage"] =
                    $"Customer Receipt {customerReceipt.Code} finalized successfully.";
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
                    await _customerReceiptService
                        .GeneratePdfAsync(
                            id);


                var customerReceipt =
                    await _customerReceiptService
                        .GetByIdAsync(
                            id);


                if (customerReceipt == null)
                {
                    return NotFound();
                }


                var safeCode =
                    customerReceipt.Code
                        .Replace(
                            "/",
                            "-")
                        .Replace(
                            "\\",
                            "-");


                return File(
                    pdfBytes,
                    "application/pdf",
                    $"Customer-Receipt-{safeCode}.pdf");
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
                await _customerReceiptService
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Customer Receipt deleted successfully.";
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
        public async Task<IActionResult> Deleted(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result =
                await _customerReceiptService
                    .SearchDeletedPagedAsync(
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


        #region Restore

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _customerReceiptService
                    .RestoreAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Customer Receipt restored successfully.";
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


        #region Customer Dropdown

        private async Task PopulateCustomersAsync(
            CustomerReceiptFormViewModel viewModel)
        {
            var customers =
                await _customerReceiptService
                    .GetCustomersForReceiptAsync();


            var options =
                new List<SelectListItem>
                {
                    new()
                    {
                        Value = "",

                        Text =
                            "-- Select Customer --"
                    }
                };


            foreach (var customer
                in customers
                    .OrderBy(x =>
                        x.CustomerName)
                    .ThenBy(x =>
                        x.Code))
            {
                options.Add(
                    new SelectListItem
                    {
                        Value =
                            customer.Id
                                .ToString(),

                        Text =
                            string.IsNullOrWhiteSpace(
                                customer.Code)

                                ? customer.CustomerName

                                : $"{customer.Code} | " +
                                  $"{customer.CustomerName}",

                        Selected =
                            customer.Id ==
                            viewModel.CustomerId
                    });
            }


            viewModel.AvailableCustomers =
                options;
        }

        #endregion


        #region Refresh Allocation Snapshots

        private async Task
            RefreshAllocationSnapshotsAsync(
                CustomerReceiptFormViewModel viewModel,
                int? excludeCustomerReceiptId = null)
        {
            if (viewModel.CustomerId <= 0 ||
                viewModel.Allocations.Count == 0)
            {
                return;
            }


            try
            {
                #region Current Outstanding Invoices

                var invoices =
                    await _customerReceiptService
                        .GetOutstandingInvoicesForCustomerAsync(
                            viewModel.CustomerId,
                            excludeCustomerReceiptId);


                var invoiceMap =
                    invoices
                        .ToDictionary(
                            x =>
                                x.Id);

                #endregion


                var sequenceNumber =
                    1;


                foreach (var allocation
                    in viewModel.Allocations)
                {
                    allocation.SequenceNumber =
                        sequenceNumber++;


                    if (allocation.InvoiceId <= 0)
                    {
                        continue;
                    }


                    #region Trusted Outstanding

                    decimal outstandingAmount;


                    try
                    {
                        outstandingAmount =
                            await _customerReceiptService
                                .GetInvoiceOutstandingAsync(
                                    allocation.InvoiceId,
                                    excludeCustomerReceiptId);
                    }
                    catch (BusinessException)
                    {
                        outstandingAmount =
                            0;
                    }

                    #endregion


                    if (invoiceMap.TryGetValue(
                        allocation.InvoiceId,
                        out var invoice))
                    {
                        allocation.InvoiceCode =
                            invoice.Code;

                        allocation.InvoiceDate =
                            invoice.InvoiceDate;

                        allocation.InvoiceGrandTotal =
                            invoice.GrandTotal;


                        var alreadyReceived =
                            invoice.GrandTotal -
                            outstandingAmount;


                        allocation.AlreadyReceivedAmount =
                            alreadyReceived < 0
                                ? 0
                                : alreadyReceived;
                    }


                    allocation.OutstandingAmount =
                        outstandingAmount;


                    allocation.BalanceAfterReceipt =
                        outstandingAmount -
                        allocation.AllocatedAmount;
                }
            }
            catch (BusinessException)
            {
                /*
                 * Preserve posted form values.
                 *
                 * The business error itself is already
                 * displayed through ModelState.
                 */
            }
        }

        #endregion


        #region Domain Mapping

        private static CustomerReceipt MapToDomain(
            CustomerReceiptFormViewModel viewModel)
        {
            var customerReceipt =
                new CustomerReceipt
                {
                    Id =
                        viewModel.Id,


                    ReceiptDate =
                        viewModel.ReceiptDate,


                    CustomerId =
                        viewModel.CustomerId,


                    PaymentMode =
                        viewModel.PaymentMode,

                    ReferenceNumber =
                        viewModel.ReferenceNumber,

                    ChequeNumber =
                        viewModel.ChequeNumber,

                    ChequeDate =
                        viewModel.ChequeDate,

                    BankName =
                        viewModel.BankName,


                    TotalReceivedAmount =
                        viewModel.TotalReceivedAmount,


                    Remarks =
                        viewModel.Remarks
                };


            var sequenceNumber =
                1;


            foreach (var allocation
                in viewModel.Allocations)
            {
                customerReceipt.Allocations.Add(
                    new CustomerReceiptAllocation
                    {
                        Id =
                            allocation.Id,

                        SequenceNumber =
                            sequenceNumber++,

                        InvoiceId =
                            allocation.InvoiceId,

                        AllocatedAmount =
                            allocation.AllocatedAmount,


                        IsActive =
                            true,

                        IsDeleted =
                            false
                    });
            }


            return customerReceipt;
        }

        #endregion


        #region Form Mapping

        private async Task<CustomerReceiptFormViewModel>
            MapToFormViewModelAsync(
                CustomerReceipt customerReceipt)
        {
            var viewModel =
                new CustomerReceiptFormViewModel
                {
                    #region Header

                    Id =
                        customerReceipt.Id,

                    Code =
                        customerReceipt.Code,

                    ReceiptDate =
                        customerReceipt.ReceiptDate,

                    #endregion


                    #region Customer

                    CustomerId =
                        customerReceipt.CustomerId,

                    CustomerCode =
                        customerReceipt.CustomerCode,

                    CustomerName =
                        customerReceipt.CustomerName,

                    #endregion


                    #region Payment

                    PaymentMode =
                        customerReceipt.PaymentMode,

                    ReferenceNumber =
                        customerReceipt.ReferenceNumber,

                    ChequeNumber =
                        customerReceipt.ChequeNumber,

                    ChequeDate =
                        customerReceipt.ChequeDate,

                    BankName =
                        customerReceipt.BankName,

                    #endregion


                    #region Amount

                    TotalReceivedAmount =
                        customerReceipt.TotalReceivedAmount,

                    #endregion


                    #region Other

                    Remarks =
                        customerReceipt.Remarks

                    #endregion
                };


            #region Allocations

            foreach (var allocation
                in customerReceipt.Allocations
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                decimal outstandingAmount;


                try
                {
                    outstandingAmount =
                        await _customerReceiptService
                            .GetInvoiceOutstandingAsync(
                                allocation.InvoiceId,
                                customerReceipt.Id);
                }
                catch (BusinessException)
                {
                    outstandingAmount =
                        0;
                }


                var alreadyReceivedAmount =
                    allocation.InvoiceGrandTotal -
                    outstandingAmount;


                if (alreadyReceivedAmount < 0)
                {
                    alreadyReceivedAmount =
                        0;
                }


                viewModel.Allocations.Add(
                    new CustomerReceiptAllocationFormViewModel
                    {
                        Id =
                            allocation.Id,

                        SequenceNumber =
                            allocation.SequenceNumber,


                        InvoiceId =
                            allocation.InvoiceId,

                        InvoiceCode =
                            allocation.InvoiceCode,

                        InvoiceDate =
                            allocation.InvoiceDate,


                        InvoiceGrandTotal =
                            allocation.InvoiceGrandTotal,

                        AlreadyReceivedAmount =
                            alreadyReceivedAmount,

                        OutstandingAmount =
                            outstandingAmount,


                        AllocatedAmount =
                            allocation.AllocatedAmount,

                        BalanceAfterReceipt =
                            outstandingAmount -
                            allocation.AllocatedAmount
                    });
            }

            #endregion


            return viewModel;
        }

        #endregion


        #region Details Mapping

        private static CustomerReceiptDetailsViewModel
            MapToDetailsViewModel(
                CustomerReceipt customerReceipt)
        {
            var viewModel =
                new CustomerReceiptDetailsViewModel
                {
                    #region Identification

                    Id =
                        customerReceipt.Id,

                    Code =
                        customerReceipt.Code,

                    ReceiptDate =
                        customerReceipt.ReceiptDate,

                    #endregion


                    #region Customer

                    CustomerId =
                        customerReceipt.CustomerId,

                    CustomerCode =
                        customerReceipt.CustomerCode,

                    CustomerName =
                        customerReceipt.CustomerName,

                    #endregion


                    #region Company

                    CompanyId =
                        customerReceipt.CompanyId,

                    CompanyName =
                        customerReceipt.CompanyName,

                    #endregion


                    #region Payment

                    PaymentMode =
                        customerReceipt.PaymentMode,

                    ReferenceNumber =
                        customerReceipt.ReferenceNumber,

                    ChequeNumber =
                        customerReceipt.ChequeNumber,

                    ChequeDate =
                        customerReceipt.ChequeDate,

                    BankName =
                        customerReceipt.BankName,

                    #endregion


                    #region Amount

                    TotalReceivedAmount =
                        customerReceipt.TotalReceivedAmount,

                    #endregion


                    #region Other

                    Remarks =
                        customerReceipt.Remarks,

                    #endregion


                    #region Workflow

                    Status =
                        customerReceipt.Status,

                    FinalizedOn =
                        customerReceipt.FinalizedOn,

                    FinalizedBy =
                        customerReceipt.FinalizedBy

                    #endregion
                };


            #region Allocations

            foreach (var allocation
                in customerReceipt.Allocations
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber))
            {
                var outstandingBeforeReceipt =
                    allocation.InvoiceGrandTotal -
                    allocation.AlreadyReceivedAmount;


                if (outstandingBeforeReceipt < 0)
                {
                    outstandingBeforeReceipt =
                        0;
                }


                viewModel.Allocations.Add(
                    new CustomerReceiptAllocationDetailsViewModel
                    {
                        Id =
                            allocation.Id,

                        SequenceNumber =
                            allocation.SequenceNumber,


                        InvoiceId =
                            allocation.InvoiceId,

                        InvoiceCode =
                            allocation.InvoiceCode,

                        InvoiceDate =
                            allocation.InvoiceDate,


                        InvoiceGrandTotal =
                            allocation.InvoiceGrandTotal,

                        AlreadyReceivedAmount =
                            allocation.AlreadyReceivedAmount,

                        OutstandingBeforeReceipt =
                            outstandingBeforeReceipt,

                        AllocatedAmount =
                            allocation.AllocatedAmount,

                        BalanceAfterReceipt =
                            allocation.BalanceAfterReceipt
                    });
            }

            #endregion


            return viewModel;
        }

        #endregion
    }
}