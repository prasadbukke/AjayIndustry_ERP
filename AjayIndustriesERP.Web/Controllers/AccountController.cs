/*
=============================================================
File: AccountController.cs
Module: Account / Authentication
Layer: Web - Controller

Purpose:
Handles Login and Password related pages.

Important Login Flow:

Login Success
      ↓
Redirect to HomeController.Index
      ↓
Supplier Due Alerts loaded
      ↓
Home Dashboard rendered
      ↓
Supplier Payment Due popup auto-opens

Important:
Do NOT directly return Home/Index.cshtml from Login POST,
because that bypasses HomeController.Index().
=============================================================
*/

using AjayIndustriesERP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class AccountController : Controller
    {
        #region Login - GET

        [HttpGet]
        public IActionResult Login()
        {
            return View(
                new LoginViewModel());
        }

        #endregion


        #region Login - POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            LoginViewModel model)
        {
            #region Validation

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            #endregion


            #region Login Success

            /*
             * Important:
             *
             * Redirect through HomeController.
             *
             * Do NOT use:
             *
             * return View("~/Views/Home/Index.cshtml");
             *
             * because it bypasses HomeController.Index()
             * and Supplier Payment Due alerts will not load.
             */

            return RedirectToAction(
                "Index",
                "Home");

            #endregion
        }

        #endregion


        #region Forgot Password

        public IActionResult ForgotPassword()
        {
            return View();
        }

        #endregion


        #region Reset Password

        public IActionResult ResetPassword()
        {
            return View();
        }

        #endregion


        #region Password Reset Success

        public IActionResult PasswordResetSuccess()
        {
            return View();
        }

        #endregion
    }
}