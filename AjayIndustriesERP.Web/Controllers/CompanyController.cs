using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Company;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        #region Company List

        /// <summary>
        /// Displays Company List.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string searchText = "", int pageNumber = 1, int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var companies = await _companyService.SearchAsync(searchText);

                ViewBag.SearchText = searchText;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;

                ViewBag.TotalRecords = companies.Count;
                ViewBag.TotalPages = 1;
                ViewBag.HasPrevious = false;
                ViewBag.HasNext = false;

                return View(companies);
            }

            var result = await _companyService.GetPagedAsync(pageNumber, pageSize);

            ViewBag.SearchText = searchText;
            ViewBag.PageNumber = result.PageNumber;
            ViewBag.PageSize = result.PageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalRecords = result.TotalRecords;
            ViewBag.HasPrevious = result.HasPrevious;
            ViewBag.HasNext = result.HasNext;

            return View(result.Items);
        }

        #endregion

        #region Create

        /// <summary>
        /// Displays Create Company page.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CompanyViewModel();

            // Temporary Auto Generated Company Code
            //model.CompanyCode = $"CMP{DateTime.Now:yyyyMMddHHmmss}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel model)
        {
            ModelState.Remove(nameof(CompanyViewModel.CompanyCode));

            if (!ModelState.IsValid)
                return View(model);

            var company = new Company
            {
                CompanyCode = model.CompanyCode,
                CompanyName = model.CompanyName,

                GstNumber = model.GstNumber,

                PanNumber = model.PanNumber,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Website = model.Website,
                ContactPerson = model.ContactPerson,

                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                PostalCode = model.PostalCode,

                #region ISO Certification

                IsoCertificationNumber =
    model.IsoCertificationNumber,

                #endregion


                #region Bank Details

                BankName =
    model.BankName,

                BankAccountHolderName =
    model.BankAccountHolderName,

                BankAccountNumber =
    model.BankAccountNumber,

                BankIfscCode =
    model.BankIfscCode,

                BankBranchName =
    model.BankBranchName,

                BankAccountType =
    model.BankAccountType,

                #endregion


                #region Terms And Conditions

                PurchaseOrderTermsAndConditions =
    model.PurchaseOrderTermsAndConditions,

                InvoiceTermsAndConditions =
    model.InvoiceTermsAndConditions,

                #endregion

                IsActive = model.IsActive
            };

            try
            {
                await _companyService.CreateAsync(company);

                TempData["Success"] = "Company created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";

                return View(model);
            }
        }

        #endregion

        #region Edit Company

        /// <summary>
        /// Displays Edit Company page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
                return NotFound();

            var model = new CompanyViewModel
            {
                CompanyId = company.CompanyId,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,

                GstNumber = company.GstNumber,

                PanNumber = company.PanNumber,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                Website = company.Website,
                ContactPerson = company.ContactPerson,

                Address = company.Address,
                City = company.City,
                State = company.State,
                Country = company.Country,
                PostalCode = company.PostalCode,

                #region ISO Certification

                IsoCertificationNumber =
    company.IsoCertificationNumber,

                #endregion


                #region Bank Details

                BankName =
    company.BankName,

                BankAccountHolderName =
    company.BankAccountHolderName,

                BankAccountNumber =
    company.BankAccountNumber,

                BankIfscCode =
    company.BankIfscCode,

                BankBranchName =
    company.BankBranchName,

                BankAccountType =
    company.BankAccountType,

                #endregion


                #region Terms And Conditions

                PurchaseOrderTermsAndConditions =
    company.PurchaseOrderTermsAndConditions,

                InvoiceTermsAndConditions =
    company.InvoiceTermsAndConditions,

                #endregion

                IsActive = company.IsActive
            };

            return View(model);
        }

        /// <summary>
        /// Updates Company.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyViewModel model)
        {
            ModelState.Remove(nameof(CompanyViewModel.CompanyCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var company = new Company
                {
                    CompanyId = model.CompanyId,
                    CompanyCode =
        model.CompanyCode ??
        string.Empty,

                    CompanyName =
        model.CompanyName,

                    GstNumber =
        model.GstNumber,

                    PanNumber =
        model.PanNumber,

                    PhoneNumber =
        model.PhoneNumber,

                    Email =
        model.Email,

                    Website =
        model.Website,

                    ContactPerson =
        model.ContactPerson,

                    Address =
        model.Address,

                    City =
        model.City,

                    State =
        model.State,

                    Country =
        model.Country,

                    PostalCode =
        model.PostalCode,

                    #region ISO Certification

                    IsoCertificationNumber =
    model.IsoCertificationNumber,

                    #endregion


                    #region Bank Details

                    BankName =
    model.BankName,

                    BankAccountHolderName =
    model.BankAccountHolderName,

                    BankAccountNumber =
    model.BankAccountNumber,

                    BankIfscCode =
    model.BankIfscCode,

                    BankBranchName =
    model.BankBranchName,

                    BankAccountType =
    model.BankAccountType,

                    #endregion


                    #region Terms And Conditions

                    PurchaseOrderTermsAndConditions =
    model.PurchaseOrderTermsAndConditions,

                    InvoiceTermsAndConditions =
    model.InvoiceTermsAndConditions,

                    #endregion

                    IsActive =
        model.IsActive
                };

                await _companyService.UpdateAsync(company);

                TempData["Success"] = "Company updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";

                return View(model);
            }
        }
        #endregion


        #region Company Details

        /// <summary>
        /// Displays Company Details.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
                return NotFound();

            return View(company);
        }

        #endregion

        #region Delete Company

        /// <summary>
        /// Soft Deletes Company.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _companyService.DeleteAsync(id);

                TempData["Success"] = "Company deleted successfully.";
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}