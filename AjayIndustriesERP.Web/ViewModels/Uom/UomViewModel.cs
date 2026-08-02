/*
==============================================================

File : UomViewModel.cs

Purpose :
Represents UOM View Model.

==============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Uom
{
    public class UomViewModel
    {
        public int UomId { get; set; }

        [Required(ErrorMessage = "UOM Code is required.")]
        [Display(Name = "UOM Code")]
        [StringLength(20)]
        public string UomCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "UOM Name is required.")]
        [Display(Name = "UOM Name")]
        [StringLength(100)]
        public string UomName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(250)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}