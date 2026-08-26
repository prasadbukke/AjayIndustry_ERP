/*
============================================================
File: DispatchBillingController.cs

Purpose:
Provides a single UI entry point for Dispatch & Billing.

Responsibilities:
- Display combined Dispatch & Billing landing screen.
- Provide navigation to Delivery Challan module.
- Provide navigation to Sales Invoice module.

Important:
- Delivery Challan and Sales Invoice remain separate
  backend modules and database tables.
- This controller is only the common UI entry point.
============================================================
*/

using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class DispatchBillingController : Controller
    {
        #region Index

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #endregion
    }
}