/*
============================================================
File: CustomerController.cs

Purpose:
Handles Customer Master HTTP requests.

Responsibilities:
- Display Customer Index with Search and Pagination.
- Display Customer Details.
- Create Customers.
- Edit Customers.
- Soft-delete Customers.
- Map Web ViewModels to Domain entities.
- Map Domain entities to Web ViewModels.
- Convert BusinessException messages to shared TempData Toasts.

Important:
- Business logic belongs in CustomerService.
- Database access must never be performed directly here.
- BusinessException must use shared TempData Toast messages.
- Search and Pagination work together.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Customer;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class CustomerController : Controller
    {
        #region Fields

        private readonly ICustomerService _customerService;

        #endregion


        #region Constructor

        public CustomerController(
            ICustomerService customerService)
        {
            _customerService = customerService;
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
                await _customerService
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
            var customer =
                await _customerService
                    .GetByIdAsync(id);


            if (customer == null)
            {
                return NotFound();
            }


            var model =
                MapToDetailsViewModel(
                    customer);


            return View(model);
        }

        #endregion


        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            var model =
                new CustomerFormViewModel
                {
                    Country = "India"
                };


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();

                return View(model);
            }


            try
            {
                var customer =
                    MapToDomain(model);


                await _customerService
                    .CreateAsync(customer);


                TempData["SuccessMessage"] =
                    "Customer created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(model);
            }
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var customer =
                await _customerService
                    .GetByIdAsync(id);


            if (customer == null)
            {
                return NotFound();
            }


            var model =
                MapToFormViewModel(
                    customer);


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CustomerFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();

                return View(model);
            }


            try
            {
                var customer =
                    MapToDomain(model);


                await _customerService
                    .UpdateAsync(customer);


                TempData["SuccessMessage"] =
                    "Customer updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(model);
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
                await _customerService
                    .DeleteAsync(id);


                TempData["SuccessMessage"] =
                    "Customer deleted successfully.";
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


        #region Domain Mapping

        private static Customer MapToDomain(
            CustomerFormViewModel model)
        {
            return new Customer
            {
                Id =
                    model.Id,

                Code =
                    model.Code
                    ?? string.Empty,


                CustomerName =
                    model.CustomerName,

                LegalName =
                    model.LegalName,


                GSTIN =
                    model.GSTIN,

                PAN =
                    model.PAN,


                ContactPerson =
                    model.ContactPerson,

                MobileNumber =
                    model.MobileNumber,

                AlternateMobileNumber =
                    model.AlternateMobileNumber,

                Email =
                    model.Email,


                AddressLine1 =
                    model.AddressLine1,

                AddressLine2 =
                    model.AddressLine2,

                City =
                    model.City,

                District =
                    model.District,

                State =
                    model.State,

                Pincode =
                    model.Pincode,

                Country =
                    model.Country,


                PaymentTerms =
                    model.PaymentTerms,

                CreditDays =
                    model.CreditDays,


                Website =
                    model.Website,

                Remarks =
                    model.Remarks
            };
        }

        #endregion


        #region Form ViewModel Mapping

        private static CustomerFormViewModel
            MapToFormViewModel(
                Customer customer)
        {
            return new CustomerFormViewModel
            {
                Id =
                    customer.Id,

                Code =
                    customer.Code,


                CustomerName =
                    customer.CustomerName,

                LegalName =
                    customer.LegalName,


                GSTIN =
                    customer.GSTIN,

                PAN =
                    customer.PAN,


                ContactPerson =
                    customer.ContactPerson,

                MobileNumber =
                    customer.MobileNumber,

                AlternateMobileNumber =
                    customer.AlternateMobileNumber,

                Email =
                    customer.Email,


                AddressLine1 =
                    customer.AddressLine1,

                AddressLine2 =
                    customer.AddressLine2,

                City =
                    customer.City,

                District =
                    customer.District,

                State =
                    customer.State,

                Pincode =
                    customer.Pincode,

                Country =
                    customer.Country,


                PaymentTerms =
                    customer.PaymentTerms,

                CreditDays =
                    customer.CreditDays,


                Website =
                    customer.Website,

                Remarks =
                    customer.Remarks
            };
        }

        #endregion


        #region Details ViewModel Mapping

        private static CustomerDetailsViewModel
            MapToDetailsViewModel(
                Customer customer)
        {
            return new CustomerDetailsViewModel
            {
                Id =
                    customer.Id,

                Code =
                    customer.Code,


                CustomerName =
                    customer.CustomerName,

                LegalName =
                    customer.LegalName,


                GSTIN =
                    customer.GSTIN,

                PAN =
                    customer.PAN,


                ContactPerson =
                    customer.ContactPerson,

                MobileNumber =
                    customer.MobileNumber,

                AlternateMobileNumber =
                    customer.AlternateMobileNumber,

                Email =
                    customer.Email,


                AddressLine1 =
                    customer.AddressLine1,

                AddressLine2 =
                    customer.AddressLine2,

                City =
                    customer.City,

                District =
                    customer.District,

                State =
                    customer.State,

                Pincode =
                    customer.Pincode,

                Country =
                    customer.Country,


                PaymentTerms =
                    customer.PaymentTerms,

                CreditDays =
                    customer.CreditDays,


                Website =
                    customer.Website,

                Remarks =
                    customer.Remarks
            };
        }

        #endregion

        #region Validation Message Helpers

        private string GetModelStateErrorMessage()
        {
            var errors =
                ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();


            if (!errors.Any())
            {
                return "Please correct the validation errors.";
            }


            return string.Join(
                " • ",
                errors);
        }

        #endregion
    }
}