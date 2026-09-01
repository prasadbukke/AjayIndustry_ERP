// =============================================================
// File: SupplierPaymentController.cs
// Module: Supplier Payment
// Layer: Web - Controller
//
// Purpose:
// Handles complete Supplier Payment workflow.
//
// Final Flow:
//
// Finalized Purchase Invoice
//          ↓
// Create Supplier Payment + First Transaction
//          ↓
// Same Supplier Payment No.
//          ↓
// Add Multiple Transactions
//          ↓
// Edit Existing Transaction if required
//          ↓
// Paid / Outstanding / Status calculated live
//
// Important Business Rules:
// - One Purchase Invoice = One Supplier Payment No.
// - Multiple part payments use SAME Payment No.
// - Company and Supplier derive from Purchase Invoice.
// - Payment No. / Invoice / Supplier / Company cannot be
//   changed while editing a transaction.
// - Completed invoice cannot accept another transaction.
// - Transaction edit recalculates Paid / Outstanding / Status.
// - Delete is Soft Delete.
// - Deleted Payment must be restored.
// =============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.SupplierPayment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Controllers
{
    public class SupplierPaymentController : Controller
    {
        private readonly ISupplierPaymentService
            _supplierPaymentService;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        #region Constructor

        public SupplierPaymentController(
            ISupplierPaymentService supplierPaymentService)
        {
            _supplierPaymentService =
                supplierPaymentService;
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
            PagedResult<SupplierPayment> result;


            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                result =
                    await _supplierPaymentService
                        .GetPagedAsync(
                            pageNumber,
                            pageSize);
            }
            else
            {
                result =
                    await _supplierPaymentService
                        .SearchPagedAsync(
                            searchText,
                            pageNumber,
                            pageSize);
            }


            var items =
                result.Items
                    .Select(
                        MapIndexViewModel)
                    .ToList();


            var model =
                new PagedResult<SupplierPaymentIndexViewModel>
                {
                    Items =
                        items,

                    PageNumber =
                        result.PageNumber,

                    PageSize =
                        result.PageSize,

                    TotalRecords =
                        result.TotalRecords
                };


            ViewBag.SearchText =
                searchText;


            ViewBag.PageSize =
                pageSize;


            return View(
                model);
        }

        #endregion


        // =====================================================
        // CREATE - FIRST PAYMENT
        // =====================================================

        #region Create Payment

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new SupplierPaymentCreateViewModel
                {
                    Transaction =
                        new SupplierPaymentTransactionInputViewModel
                        {
                            PaymentDate =
                                DateTime.Today
                        }
                };


            await PopulateCreateFormAsync(
                model);


            return View(
                model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SupplierPaymentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateFormAsync(
                    model);


                return View(
                    model);
            }


            try
            {
                var transaction =
                    MapTransactionEntity(
                        model.Transaction);


                var payment =
                    await _supplierPaymentService
                        .CreateAsync(
                            model.PurchaseInvoiceId,
                            transaction);


                TempData["SuccessMessage"] =
                    $"Supplier Payment {payment.Code} created successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = payment.Id
                    });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create Supplier Payment.");
            }


            await PopulateCreateFormAsync(
                model);


            return View(
                model);
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE DETAILS - AJAX
        // =====================================================

        #region Purchase Invoice AJAX

        [HttpGet]
        public async Task<IActionResult>
            GetPurchaseInvoiceDetails(
                int purchaseInvoiceId)
        {
            if (purchaseInvoiceId <= 0)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Invalid Purchase Invoice."
                    });
            }


            var invoices =
                await _supplierPaymentService
                    .GetAvailablePurchaseInvoicesAsync();


            var invoice =
                invoices.FirstOrDefault(x =>
                    x.Id ==
                    purchaseInvoiceId);


            if (invoice == null)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Purchase Invoice is not available for payment."
                    });
            }


            return Json(
                new
                {
                    success = true,

                    invoice =
                        new
                        {
                            purchaseInvoiceId =
                                invoice.Id,

                            purchaseInvoiceCode =
                                invoice.Code,

                            supplierInvoiceNumber =
                                invoice.SupplierInvoiceNumber
                                ?? string.Empty,

                            supplierName =
                                GetSupplierName(
                                    invoice),

                            purchaseInvoiceDate =
                                invoice.PurchaseInvoiceDate
                                    .ToString(
                                        "dd-MM-yyyy"),

                            supplierInvoiceDate =
                                invoice.SupplierInvoiceDate
                                    .ToString(
                                        "dd-MM-yyyy"),

                            dueDate =
                                invoice.DueDate.HasValue
                                    ? invoice.DueDate.Value
                                        .ToString(
                                            "dd-MM-yyyy")
                                    : "-",

                            invoiceTotal =
                                RoundMoney(
                                    invoice.GrandTotal)
                        }
                });
        }

        #endregion


        // =====================================================
        // ADD PAYMENT TRANSACTION
        // =====================================================

        #region Add Payment

        [HttpGet]
        public async Task<IActionResult>
            AddTransaction(
                int id)
        {
            var model =
                await BuildAddTransactionViewModelAsync(
                    id,
                    null);


            if (model == null)
            {
                return NotFound();
            }


            if (model.OutstandingAmount <= 0m)
            {
                TempData["ErrorMessage"] =
                    "This Purchase Invoice is already fully paid.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }


            return View(
                model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AddTransaction(
                int id,
                SupplierPaymentAddTransactionViewModel model)
        {
            if (id !=
                model.SupplierPaymentId)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                var invalidModel =
                    await BuildAddTransactionViewModelAsync(
                        id,
                        model.Transaction);


                if (invalidModel == null)
                {
                    return NotFound();
                }


                return View(
                    invalidModel);
            }


            try
            {
                var transaction =
                    MapTransactionEntity(
                        model.Transaction);


                await _supplierPaymentService
                    .AddTransactionAsync(
                        id,
                        transaction);


                TempData["SuccessMessage"] =
                    "Payment transaction added successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to add payment transaction.");
            }


            var rebuiltModel =
                await BuildAddTransactionViewModelAsync(
                    id,
                    model.Transaction);


            if (rebuiltModel == null)
            {
                return NotFound();
            }


            return View(
                rebuiltModel);
        }

        #endregion


        // =====================================================
        // EDIT PAYMENT TRANSACTION
        // =====================================================

        #region Edit Payment Transaction

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            int transactionId)
        {
            var result =
                await BuildEditTransactionViewModelAsync(
                    id,
                    transactionId,
                    null);


            if (result == null)
            {
                return NotFound();
            }


            ViewBag.TransactionId =
                transactionId;


            ViewBag.MaximumEditableAmount =
                result.MaximumEditableAmount;


            return View(
                result.Model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            int transactionId,
            SupplierPaymentAddTransactionViewModel model)
        {
            if (id !=
                model.SupplierPaymentId)
            {
                return BadRequest();
            }


            if (transactionId <= 0)
            {
                return BadRequest();
            }


            // =================================================
            // MODEL VALIDATION
            // =================================================

            if (!ModelState.IsValid)
            {
                var invalidResult =
                    await BuildEditTransactionViewModelAsync(
                        id,
                        transactionId,
                        model.Transaction);


                if (invalidResult == null)
                {
                    return NotFound();
                }


                ViewBag.TransactionId =
                    transactionId;


                ViewBag.MaximumEditableAmount =
                    invalidResult
                        .MaximumEditableAmount;


                return View(
                    invalidResult.Model);
            }


            // =================================================
            // UPDATE TRANSACTION
            // =================================================

            try
            {
                var transaction =
                    MapTransactionEntity(
                        model.Transaction);


                await _supplierPaymentService
                    .UpdateTransactionAsync(
                        id,
                        transactionId,
                        transaction);


                TempData["SuccessMessage"] =
                    "Payment transaction updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update payment transaction.");
            }


            // =================================================
            // REBUILD FORM AFTER SERVICE VALIDATION ERROR
            // =================================================

            var rebuiltResult =
                await BuildEditTransactionViewModelAsync(
                    id,
                    transactionId,
                    model.Transaction);


            if (rebuiltResult == null)
            {
                return NotFound();
            }


            ViewBag.TransactionId =
                transactionId;


            ViewBag.MaximumEditableAmount =
                rebuiltResult
                    .MaximumEditableAmount;


            return View(
                rebuiltResult.Model);
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
            var payment =
                await _supplierPaymentService
                    .GetByIdAsync(
                        id);


            if (payment == null)
            {
                return NotFound();
            }


            if (payment.PurchaseInvoice == null)
            {
                TempData["ErrorMessage"] =
                    "Purchase Invoice information is not available.";


                return RedirectToAction(
                    nameof(Index));
            }


            var model =
                MapDetailsViewModel(
                    payment);


            return View(
                model);
        }

        #endregion


        // =====================================================
        // DELETE PAYMENT
        // =====================================================

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            try
            {
                await _supplierPaymentService
                    .DeleteAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Supplier Payment deleted successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Unable to delete Supplier Payment.";
            }


            return RedirectToAction(
                nameof(Index));
        }

        #endregion


        // =====================================================
        // DELETED PAYMENT LIST
        // =====================================================

        #region Deleted List

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var payments =
                await _supplierPaymentService
                    .GetDeletedAsync();


            var model =
                payments
                    .Select(
                        MapDeletedViewModel)
                    .ToList();


            return View(
                model);
        }

        #endregion


        // =====================================================
        // RESTORE PAYMENT
        // =====================================================

        #region Restore

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _supplierPaymentService
                    .RestoreAsync(
                        id);


                TempData["SuccessMessage"] =
                    "Supplier Payment restored successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Unable to restore Supplier Payment.";
            }


            return RedirectToAction(
                nameof(Deleted));
        }

        #endregion


        // =====================================================
        // CREATE FORM DATA
        // =====================================================

        #region Create Form Data

        private async Task PopulateCreateFormAsync(
            SupplierPaymentCreateViewModel model)
        {
            var invoices =
                await _supplierPaymentService
                    .GetAvailablePurchaseInvoicesAsync();


            model.PurchaseInvoices =
                invoices
                    .Select(invoice =>
                        new SelectListItem
                        {
                            Value =
                                invoice.Id
                                    .ToString(),

                            Text =
                                BuildPurchaseInvoiceOptionText(
                                    invoice),

                            Selected =
                                invoice.Id ==
                                model.PurchaseInvoiceId
                        })
                    .ToList();


            model.PurchaseInvoices.Insert(
                0,
                new SelectListItem
                {
                    Value =
                        string.Empty,

                    Text =
                        "-- Select Purchase Invoice --"
                });


            model.PaymentModes =
                GetPaymentModes();


            // =================================================
            // SELECTED INVOICE PREVIEW
            // =================================================

            if (model.PurchaseInvoiceId > 0)
            {
                var selectedInvoice =
                    invoices.FirstOrDefault(x =>
                        x.Id ==
                        model.PurchaseInvoiceId);


                if (selectedInvoice != null)
                {
                    model.PurchaseInvoiceCode =
                        selectedInvoice.Code;


                    model.SupplierInvoiceNumber =
                        selectedInvoice
                            .SupplierInvoiceNumber;


                    model.SupplierName =
                        GetSupplierName(
                            selectedInvoice);


                    model.PurchaseInvoiceDate =
                        selectedInvoice
                            .PurchaseInvoiceDate;


                    model.DueDate =
                        selectedInvoice
                            .DueDate;


                    model.InvoiceTotal =
                        RoundMoney(
                            selectedInvoice
                                .GrandTotal);
                }
            }
        }

        #endregion


        // =====================================================
        // ADD TRANSACTION VIEW MODEL
        // =====================================================

        #region Add Transaction ViewModel

        private async Task
            <SupplierPaymentAddTransactionViewModel?>
            BuildAddTransactionViewModelAsync(
                int supplierPaymentId,
                SupplierPaymentTransactionInputViewModel?
                    postedTransaction)
        {
            var payment =
                await _supplierPaymentService
                    .GetByIdAsync(
                        supplierPaymentId);


            if (payment == null ||
                payment.PurchaseInvoice == null)
            {
                return null;
            }


            var invoice =
                payment.PurchaseInvoice;


            var position =
                CalculatePaymentPosition(
                    payment);


            var transaction =
                postedTransaction
                ?? new SupplierPaymentTransactionInputViewModel
                {
                    PaymentDate =
                        DateTime.Today
                };


            return new SupplierPaymentAddTransactionViewModel
            {
                SupplierPaymentId =
                    payment.Id,

                PaymentCode =
                    payment.Code,

                PurchaseInvoiceId =
                    invoice.Id,

                PurchaseInvoiceCode =
                    invoice.Code,

                SupplierInvoiceNumber =
                    invoice.SupplierInvoiceNumber
                    ?? string.Empty,

                SupplierName =
                    GetSupplierName(
                        invoice),

                PurchaseInvoiceDate =
                    invoice.PurchaseInvoiceDate,

                DueDate =
                    invoice.DueDate,

                InvoiceTotal =
                    position.InvoiceTotal,

                PaidAmount =
                    position.PaidAmount,

                OutstandingAmount =
                    position.OutstandingAmount,

                PaymentStatus =
                    position.Status,

                Transaction =
                    transaction,

                PaymentModes =
                    GetPaymentModes()
            };
        }

        #endregion


        // =====================================================
        // EDIT TRANSACTION VIEW MODEL
        // =====================================================

        #region Edit Transaction ViewModel

        private async Task<EditTransactionBuildResult?>
            BuildEditTransactionViewModelAsync(
                int supplierPaymentId,
                int transactionId,
                SupplierPaymentTransactionInputViewModel?
                    postedTransaction)
        {
            var payment =
                await _supplierPaymentService
                    .GetByIdAsync(
                        supplierPaymentId);


            if (payment == null ||
                payment.PurchaseInvoice == null)
            {
                return null;
            }


            var existingTransaction =
                payment.Transactions
                    .FirstOrDefault(x =>
                        x.Id ==
                            transactionId &&
                        x.IsActive &&
                        !x.IsDeleted);


            if (existingTransaction == null)
            {
                return null;
            }


            var invoice =
                payment.PurchaseInvoice;


            var position =
                CalculatePaymentPosition(
                    payment);


            // =================================================
            // MAXIMUM EDITABLE AMOUNT
            //
            // Invoice Total
            // -
            // Other Active Transactions
            // =================================================

            var otherTransactionsTotal =
                RoundMoney(
                    payment.Transactions

                        .Where(x =>
                            x.Id !=
                                transactionId &&
                            x.IsActive &&
                            !x.IsDeleted)

                        .Sum(x =>
                            x.Amount));


            var maximumEditableAmount =
                RoundMoney(
                    invoice.GrandTotal -
                    otherTransactionsTotal);


            if (maximumEditableAmount < 0m)
            {
                maximumEditableAmount =
                    0m;
            }


            // =================================================
            // FORM TRANSACTION
            //
            // GET:
            // Existing transaction values.
            //
            // POST validation error:
            // Preserve user's posted values.
            // =================================================

            var transaction =
                postedTransaction
                ?? new SupplierPaymentTransactionInputViewModel
                {
                    PaymentDate =
                        existingTransaction
                            .PaymentDate,

                    Amount =
                        existingTransaction
                            .Amount,

                    PaymentMode =
                        existingTransaction
                            .PaymentMode,

                    BankName =
                        existingTransaction
                            .BankName,

                    ReferenceNumber =
                        existingTransaction
                            .ReferenceNumber,

                    Remarks =
                        existingTransaction
                            .Remarks
                };


            var model =
                new SupplierPaymentAddTransactionViewModel
                {
                    SupplierPaymentId =
                        payment.Id,

                    PaymentCode =
                        payment.Code,

                    PurchaseInvoiceId =
                        invoice.Id,

                    PurchaseInvoiceCode =
                        invoice.Code,

                    SupplierInvoiceNumber =
                        invoice.SupplierInvoiceNumber
                        ?? string.Empty,

                    SupplierName =
                        GetSupplierName(
                            invoice),

                    PurchaseInvoiceDate =
                        invoice.PurchaseInvoiceDate,

                    DueDate =
                        invoice.DueDate,

                    InvoiceTotal =
                        position.InvoiceTotal,

                    PaidAmount =
                        position.PaidAmount,

                    OutstandingAmount =
                        position.OutstandingAmount,

                    PaymentStatus =
                        position.Status,

                    Transaction =
                        transaction,

                    PaymentModes =
                        GetPaymentModes()
                };


            return new EditTransactionBuildResult
            {
                Model =
                    model,

                MaximumEditableAmount =
                    maximumEditableAmount
            };
        }

        #endregion


        // =====================================================
        // INDEX MAPPING
        // =====================================================

        #region Index Mapping

        private static SupplierPaymentIndexViewModel
            MapIndexViewModel(
                SupplierPayment payment)
        {
            var invoice =
                payment.PurchaseInvoice;


            var position =
                CalculatePaymentPosition(
                    payment);


            var lastPaymentDate =
                payment.Transactions

                    .Where(x =>
                        x.IsActive &&
                        !x.IsDeleted)

                    .Select(x =>
                        (DateTime?)x.PaymentDate)

                    .OrderByDescending(x =>
                        x)

                    .FirstOrDefault();


            return new SupplierPaymentIndexViewModel
            {
                Id =
                    payment.Id,

                Code =
                    payment.Code,

                PurchaseInvoiceId =
                    payment.PurchaseInvoiceId,

                PurchaseInvoiceCode =
                    invoice?.Code
                    ?? string.Empty,

                SupplierInvoiceNumber =
                    invoice?.SupplierInvoiceNumber
                    ?? string.Empty,

                SupplierId =
                    payment.SupplierId,

                SupplierName =
                    invoice != null
                        ? GetSupplierName(
                            invoice)
                        : $"Supplier #{payment.SupplierId}",

                InvoiceTotal =
                    position.InvoiceTotal,

                PaidAmount =
                    position.PaidAmount,

                OutstandingAmount =
                    position.OutstandingAmount,

                PaymentStatus =
                    position.Status,

                LastPaymentDate =
                    lastPaymentDate
            };
        }

        #endregion


        // =====================================================
        // DETAILS MAPPING
        // =====================================================

        #region Details Mapping

        private static SupplierPaymentDetailsViewModel
            MapDetailsViewModel(
                SupplierPayment payment)
        {
            var invoice =
                payment.PurchaseInvoice!;


            var position =
                CalculatePaymentPosition(
                    payment);


            return new SupplierPaymentDetailsViewModel
            {
                Id =
                    payment.Id,

                Code =
                    payment.Code,

                PurchaseInvoiceId =
                    invoice.Id,

                PurchaseInvoiceCode =
                    invoice.Code,

                SupplierInvoiceNumber =
                    invoice.SupplierInvoiceNumber
                    ?? string.Empty,

                PurchaseInvoiceDate =
                    invoice.PurchaseInvoiceDate,

                SupplierInvoiceDate =
                    invoice.SupplierInvoiceDate,

                DueDate =
                    invoice.DueDate,

                SupplierId =
                    payment.SupplierId,

                SupplierName =
                    GetSupplierName(
                        invoice),

                InvoiceTotal =
                    position.InvoiceTotal,

                PaidAmount =
                    position.PaidAmount,

                OutstandingAmount =
                    position.OutstandingAmount,

                PaymentStatus =
                    position.Status,

                Transactions =
                    payment.Transactions

                        .Where(x =>
                            x.IsActive &&
                            !x.IsDeleted)

                        .OrderByDescending(x =>
                            x.PaymentDate)

                        .ThenByDescending(x =>
                            x.Id)

                        .Select(x =>
                            new SupplierPaymentTransactionRowViewModel
                            {
                                Id =
                                    x.Id,

                                PaymentDate =
                                    x.PaymentDate,

                                Amount =
                                    RoundMoney(
                                        x.Amount),

                                PaymentMode =
                                    x.PaymentMode,

                                BankName =
                                    x.BankName,

                                ReferenceNumber =
                                    x.ReferenceNumber,

                                Remarks =
                                    x.Remarks
                            })

                        .ToList()
            };
        }

        #endregion


        // =====================================================
        // DELETED MAPPING
        // =====================================================

        #region Deleted Mapping

        private static SupplierPaymentDeletedViewModel
            MapDeletedViewModel(
                SupplierPayment payment)
        {
            var invoice =
                payment.PurchaseInvoice;


            var position =
                CalculatePaymentPosition(
                    payment);


            return new SupplierPaymentDeletedViewModel
            {
                Id =
                    payment.Id,

                Code =
                    payment.Code,

                PurchaseInvoiceId =
                    payment.PurchaseInvoiceId,

                PurchaseInvoiceCode =
                    invoice?.Code
                    ?? string.Empty,

                SupplierInvoiceNumber =
                    invoice?.SupplierInvoiceNumber
                    ?? string.Empty,

                SupplierName =
                    invoice != null
                        ? GetSupplierName(
                            invoice)
                        : $"Supplier #{payment.SupplierId}",

                InvoiceTotal =
                    position.InvoiceTotal,

                PaidAmount =
                    position.PaidAmount,

                OutstandingAmount =
                    position.OutstandingAmount,

                PaymentStatus =
                    position.Status,

                DeletedOn =
                    payment.ModifiedOn
            };
        }

        #endregion


        // =====================================================
        // TRANSACTION MAPPING
        // =====================================================

        #region Transaction Mapping

        private static SupplierPaymentTransaction
            MapTransactionEntity(
                SupplierPaymentTransactionInputViewModel model)
        {
            return new SupplierPaymentTransaction
            {
                PaymentDate =
                    model.PaymentDate,

                Amount =
                    RoundMoney(
                        model.Amount),

                PaymentMode =
                    model.PaymentMode,

                BankName =
                    model.BankName,

                ReferenceNumber =
                    model.ReferenceNumber,

                Remarks =
                    model.Remarks
            };
        }

        #endregion


        // =====================================================
        // PAYMENT POSITION
        // =====================================================

        #region Payment Position

        private static PaymentPosition
            CalculatePaymentPosition(
                SupplierPayment payment)
        {
            var invoiceTotal =
                RoundMoney(
                    payment.PurchaseInvoice?
                        .GrandTotal
                    ?? 0m);


            var paidAmount =
                RoundMoney(
                    payment.Transactions

                        .Where(x =>
                            x.IsActive &&
                            !x.IsDeleted)

                        .Sum(x =>
                            x.Amount));


            var outstanding =
                RoundMoney(
                    invoiceTotal -
                    paidAmount);


            if (outstanding < 0m)
            {
                outstanding =
                    0m;
            }


            string status;


            if (paidAmount <= 0m)
            {
                status =
                    "Pending";
            }
            else if (paidAmount >=
                     invoiceTotal)
            {
                status =
                    "Completed";
            }
            else
            {
                status =
                    "Partially Paid";
            }


            return new PaymentPosition
            {
                InvoiceTotal =
                    invoiceTotal,

                PaidAmount =
                    paidAmount,

                OutstandingAmount =
                    outstanding,

                Status =
                    status
            };
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE HELPERS
        // =====================================================

        #region Purchase Invoice Helpers

        private static string GetSupplierName(
            PurchaseInvoice invoice)
        {
            if (!string.IsNullOrWhiteSpace(
                invoice.SupplierName))
            {
                return invoice
                    .SupplierName
                    .Trim();
            }


            return
                $"Supplier #{invoice.SupplierId}";
        }


        private static string
            BuildPurchaseInvoiceOptionText(
                PurchaseInvoice invoice)
        {
            var supplierInvoiceNo =
                string.IsNullOrWhiteSpace(
                    invoice.SupplierInvoiceNumber)
                    ? "-"
                    : invoice.SupplierInvoiceNumber;


            var supplierName =
                GetSupplierName(
                    invoice);


            var invoiceTotal =
                RoundMoney(
                    invoice.GrandTotal);


            return
                $"{invoice.Code} | " +
                $"{supplierInvoiceNo} | " +
                $"{supplierName} | " +
                $"₹ {invoiceTotal:N2}";
        }

        #endregion


        // =====================================================
        // PAYMENT MODE OPTIONS
        // =====================================================

        #region Payment Modes

        private static List<SelectListItem>
            GetPaymentModes()
        {
            return new List<SelectListItem>
            {
                new()
                {
                    Value = "",
                    Text = "-- Select Payment Mode --"
                },

                new()
                {
                    Value = "Cash",
                    Text = "Cash"
                },

                new()
                {
                    Value = "Bank Transfer",
                    Text = "Bank Transfer"
                },

                new()
                {
                    Value = "UPI",
                    Text = "UPI"
                },

                new()
                {
                    Value = "Cheque",
                    Text = "Cheque"
                },

                new()
                {
                    Value = "NEFT",
                    Text = "NEFT"
                },

                new()
                {
                    Value = "RTGS",
                    Text = "RTGS"
                }
            };
        }

        #endregion


        // =====================================================
        // MONEY
        // =====================================================

        #region Money

        private static decimal RoundMoney(
            decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }

        #endregion


        // =====================================================
        // PRIVATE MODELS
        // =====================================================

        #region Private Models

        private class PaymentPosition
        {
            public decimal InvoiceTotal { get; set; }


            public decimal PaidAmount { get; set; }


            public decimal OutstandingAmount { get; set; }


            public string Status { get; set; }
                = string.Empty;
        }


        private class EditTransactionBuildResult
        {
            public SupplierPaymentAddTransactionViewModel
                Model
            { get; set; }
                    = new();


            public decimal MaximumEditableAmount
            {
                get;
                set;
            }
        }

        #endregion
    }
}