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

        /// <summary>
        /// Displays Company List.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            List<Company> companies;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                companies = await _companyService.SearchAsync(searchText);
            }
            else
            {
                companies = await _companyService.GetPagedAsync(pageNumber, pageSize);
            }

            ViewBag.SearchText = searchText;
            ViewBag.PageSize = pageSize;
            ViewBag.PageNumber = pageNumber;

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
            if (!ModelState.IsValid)
                return View(model);

            var company = new Company
            {
                CompanyId = model.CompanyId,
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

            await _companyService.UpdateAsync(company);

            TempData["Success"] = "Company updated successfully.";

            return RedirectToAction(nameof(Index));
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
            await _companyService.DeleteAsync(id);

            TempData["Success"] = "Company deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}