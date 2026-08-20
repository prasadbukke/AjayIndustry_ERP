/*
============================================================
File: MachineFormViewModel.cs

Purpose:
Provides Machine Master data to Create and Edit forms.

Responsibilities:
- Accept Machine identity information.
- Accept manufacturer / model / serial information.
- Accept capacity and shop-floor location.
- Accept manually maintained Machine Status.
- Provide Web-level validation.
- Support both Create and Edit using reusable _Form.cshtml.

Important:
- Machine Code is system generated and read-only.
- Business validation remains in MachineService.
- Machine Status is manually updated by ERP users.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Machine
{
    public class MachineFormViewModel
    {
        #region Identification

        public int Id { get; set; }


        [Display(Name = "Machine Code")]
        public string? Code { get; set; }

        #endregion


        #region Machine Information

        [Required(
            ErrorMessage = "Machine Name is required.")]
        [StringLength(
            200,
            ErrorMessage = "Machine Name cannot exceed 200 characters.")]
        [Display(Name = "Machine Name")]
        public string MachineName { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Machine Type is required.")]
        [StringLength(
            100,
            ErrorMessage = "Machine Type cannot exceed 100 characters.")]
        [Display(Name = "Machine Type")]
        public string MachineType { get; set; } =
            string.Empty;

        #endregion


        #region Manufacturer Information

        [StringLength(
            150,
            ErrorMessage = "Manufacturer cannot exceed 150 characters.")]
        public string? Manufacturer { get; set; }


        [StringLength(
            150,
            ErrorMessage = "Model cannot exceed 150 characters.")]
        public string? Model { get; set; }


        [StringLength(
            100,
            ErrorMessage = "Serial Number cannot exceed 100 characters.")]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        #endregion


        #region Capacity And Location

        [StringLength(
            250,
            ErrorMessage = "Capacity cannot exceed 250 characters.")]
        public string? Capacity { get; set; }


        [StringLength(
            150,
            ErrorMessage = "Location cannot exceed 150 characters.")]
        public string? Location { get; set; }

        #endregion


        #region Operational Status

        [Required]
        [Display(Name = "Machine Status")]
        public MachineStatus Status { get; set; } =
            MachineStatus.Available;

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion
    }
}