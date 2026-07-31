/*
==============================================================

File : EmployeeController.cs

Purpose :
Handles Employee UI requests.

Flow

View
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
Database

==============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Web.ViewModels.Employee;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        #region Employee List

        /// <summary>
        /// Displays Employee List.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            List<Employee> employees;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                employees = await _employeeService.GetPagedAsync(pageNumber, pageSize);
            }
            else
            {
                employees = await _employeeService.SearchAsync(searchText);
            }

            ViewBag.SearchText = searchText;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(employees);
        }

        #endregion

        #region Create Employee

        /// <summary>
        /// Displays Create Employee page.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new EmployeeViewModel());
        }

        /// <summary>
        /// Creates Employee.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employee = new Employee
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                MobileNumber = model.MobileNumber,
                Email = model.Email,
                Address = model.Address,
                JoiningDate = model.JoiningDate,
                Salary = model.Salary,
                IsActive = model.IsActive
            };

            try
            {
                await _employeeService.CreateAsync(employee);

                TempData["Success"] = "Employee created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(model);
            }
        }

        #endregion

        #region Employee Details

        /// <summary>
        /// Displays Employee Details.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        #endregion

        #region Edit Employee

        /// <summary>
        /// Displays Edit Employee page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";

                return RedirectToAction(nameof(Index));
            }

            var model = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                MobileNumber = employee.MobileNumber,
                Email = employee.Email,
                Address = employee.Address,
                JoiningDate = employee.JoiningDate,
                Salary = employee.Salary,
                IsActive = employee.IsActive
            };

            return View(model);
        }

        /// <summary>
        /// Updates Employee.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeViewModel model)
        {
            ModelState.Remove(nameof(EmployeeViewModel.EmployeeCode));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var employee = new Employee
                {
                    EmployeeId = model.EmployeeId,
                    EmployeeCode = model.EmployeeCode ?? string.Empty,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    MobileNumber = model.MobileNumber,
                    Email = model.Email,
                    Address = model.Address,
                    JoiningDate = model.JoiningDate,
                    Salary = model.Salary,
                    IsActive = model.IsActive
                };

                await _employeeService.UpdateAsync(employee);

                TempData["Success"] = "Employee updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(model);
            }
        }

        #endregion

        #region Delete Employee

        /// <summary>
        /// Deletes Employee.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _employeeService.DeleteAsync(id);

                TempData["Success"] = "Employee deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}