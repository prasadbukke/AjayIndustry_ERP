/*
============================================================
File: MachineController.cs

Purpose:
Handles Machine Master HTTP requests.

Responsibilities:
- Display Machine Index with Search and Pagination.
- Display Machine Details.
- Create Machines.
- Edit Machines.
- Soft-delete Machines.
- Display deleted Machines separately.
- Restore deleted Machines.
- Map Web ViewModels to Domain entities.
- Map Domain entities to Web ViewModels.
- Display BusinessException and validation messages using
  shared Toast notifications.

Important:
- Business logic belongs in MachineService.
- Database access must never occur directly in Controller.
- Machine Status is manually maintained by ERP users.
- BusinessException messages use TempData shared Toast.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Web.ViewModels.Machine;
using Microsoft.AspNetCore.Mvc;

namespace AjayIndustriesERP.Web.Controllers
{
    public class MachineController : Controller
    {
        #region Fields

        private readonly IMachineService _machineService;

        #endregion


        #region Constructor

        public MachineController(
            IMachineService machineService)
        {
            _machineService = machineService;
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
                await _machineService
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
            var machine =
                await _machineService
                    .GetByIdAsync(id);


            if (machine == null)
            {
                return NotFound();
            }


            var model =
                MapToDetailsViewModel(
                    machine);


            return View(model);
        }

        #endregion


        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            var model =
                new MachineFormViewModel
                {
                    Status =
                        MachineStatus.Available
                };


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MachineFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var machine =
                    MapToDomain(
                        viewModel);


                await _machineService
                    .CreateAsync(
                        machine);


                TempData["SuccessMessage"] =
                    "Machine created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
            }
        }

        #endregion


        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var machine =
                await _machineService
                    .GetByIdAsync(id);


            if (machine == null)
            {
                return NotFound();
            }


            var model =
                MapToFormViewModel(
                    machine);


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            MachineFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    GetModelStateErrorMessage();


                return View(
                    viewModel);
            }


            try
            {
                var machine =
                    MapToDomain(
                        viewModel);


                await _machineService
                    .UpdateAsync(
                        machine);


                TempData["SuccessMessage"] =
                    "Machine updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return View(
                    viewModel);
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
                await _machineService
                    .DeleteAsync(id);


                TempData["SuccessMessage"] =
                    "Machine deleted successfully.";
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


        #region Deleted Machines

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var machines =
                await _machineService
                    .GetDeletedAsync();


            return View(machines);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(
            int id)
        {
            try
            {
                await _machineService
                    .RestoreAsync(id);


                TempData["SuccessMessage"] =
                    "Machine restored successfully.";
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


        #region Domain Mapping

        private static Machine MapToDomain(
            MachineFormViewModel model)
        {
            return new Machine
            {
                Id =
                    model.Id,

                Code =
                    model.Code
                    ?? string.Empty,


                MachineName =
                    model.MachineName,

                MachineType =
                    model.MachineType,


                Manufacturer =
                    model.Manufacturer,

                Model =
                    model.Model,

                SerialNumber =
                    model.SerialNumber,


                Capacity =
                    model.Capacity,

                Location =
                    model.Location,


                Status =
                    model.Status,


                Remarks =
                    model.Remarks
            };
        }

        #endregion


        #region Form ViewModel Mapping

        private static MachineFormViewModel
            MapToFormViewModel(
                Machine machine)
        {
            return new MachineFormViewModel
            {
                Id =
                    machine.Id,

                Code =
                    machine.Code,


                MachineName =
                    machine.MachineName,

                MachineType =
                    machine.MachineType,


                Manufacturer =
                    machine.Manufacturer,

                Model =
                    machine.Model,

                SerialNumber =
                    machine.SerialNumber,


                Capacity =
                    machine.Capacity,

                Location =
                    machine.Location,


                Status =
                    machine.Status,


                Remarks =
                    machine.Remarks
            };
        }

        #endregion


        #region Details ViewModel Mapping

        private static MachineDetailsViewModel
            MapToDetailsViewModel(
                Machine machine)
        {
            return new MachineDetailsViewModel
            {
                Id =
                    machine.Id,

                Code =
                    machine.Code,


                MachineName =
                    machine.MachineName,

                MachineType =
                    machine.MachineType,


                Manufacturer =
                    machine.Manufacturer,

                Model =
                    machine.Model,

                SerialNumber =
                    machine.SerialNumber,


                Capacity =
                    machine.Capacity,

                Location =
                    machine.Location,


                Status =
                    machine.Status,


                Remarks =
                    machine.Remarks
            };
        }

        #endregion


        #region Validation Message Helper

        private string GetModelStateErrorMessage()
        {
            var errors =
                ModelState.Values
                    .SelectMany(x =>
                        x.Errors)
                    .Select(x =>
                        x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();


            if (!errors.Any())
            {
                return
                    "Please correct the validation errors.";
            }


            return string.Join(
                " • ",
                errors);
        }

        #endregion
    }
}