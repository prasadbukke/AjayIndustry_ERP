/*
=============================================================
File: HomeController.cs
Module: Home Dashboard
Layer: Web - Controller

Purpose:
Handles Home Dashboard.

Supplier Payment Due Popup:
- Loads automatically when Home Dashboard opens.
- Data comes from Supplier Outstanding Service.
- Includes:
    - Overdue supplier invoices
    - Due today
    - Due within next 5 days
- Only invoices having Outstanding > 0 are included.
- Fully paid invoices are automatically excluded.

Important:
- No direct DbContext access.
- Uses existing Supplier Outstanding service.
=============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Fields

        private readonly ILogger<HomeController> _logger;

        private readonly ISupplierOutstandingService
            _supplierOutstandingService;

        #endregion


        #region Constructor

        public HomeController(
            ILogger<HomeController> logger,
            ISupplierOutstandingService supplierOutstandingService)
        {
            _logger = logger;

            _supplierOutstandingService =
                supplierOutstandingService;
        }

        #endregion


        #region Index

        public async Task<IActionResult> Index()
        {
            #region Load Supplier Payment Due Alerts

            var dueAlerts =
                await _supplierOutstandingService
                    .GetDueAlertsAsync();

            #endregion


            #region Build Popup ViewModel

            var viewModel =
                new SupplierPaymentDueAlertViewModel
                {
                    Alerts =
                        dueAlerts
                            .Select(alert =>
                                new SupplierPaymentDueAlertRowViewModel
                                {
                                    #region Purchase Invoice

                                    PurchaseInvoiceId =
                                        alert.PurchaseInvoiceId,

                                    PurchaseInvoiceCode =
                                        alert.PurchaseInvoiceCode,

                                    SupplierInvoiceNumber =
                                        alert.SupplierInvoiceNumber,

                                    PurchaseInvoiceDate =
                                        alert.PurchaseInvoiceDate,

                                    DueDate =
                                        alert.DueDate,

                                    #endregion


                                    #region Supplier

                                    SupplierId =
                                        alert.SupplierId,

                                    SupplierName =
                                        alert.SupplierName,

                                    #endregion


                                    #region Payment Position

                                    InvoiceTotal =
                                        alert.InvoiceTotal,

                                    PaidAmount =
                                        alert.PaidAmount,

                                    OutstandingAmount =
                                        alert.OutstandingAmount,

                                    PaymentStatus =
                                        alert.PaymentStatus,

                                    #endregion


                                    #region Due Position

                                    DueStatus =
                                        alert.DueStatus,

                                    OverdueDays =
                                        alert.OverdueDays,

                                    DaysUntilDue =
                                        alert.DaysUntilDue

                                    #endregion
                                })
                            .ToList()
                };

            #endregion


            return View(viewModel);
        }

        #endregion


        #region Privacy

        public IActionResult Privacy()
        {
            return View();
        }

        #endregion
    }
}