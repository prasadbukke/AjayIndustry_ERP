using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Company;
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

        public async Task<IActionResult> Index()
        {
            var companies = await _companyService.GetAllAsync();

            return View(companies);
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
            model.CompanyCode = $"CMP{DateTime.Now:yyyyMMddHHmmss}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel model)
        {
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
                IsActive = model.IsActive
            };

            await _companyService.CreateAsync(company);

            TempData["Success"] = "Company created successfully.";

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}