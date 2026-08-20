/*
============================================================
File: ItemProcessRoutingFormViewModel.cs

Purpose:
Provides Item Process Routing data to Create and Edit forms.

Responsibilities:
- Select Item during first Routing creation.
- Display Routing Code and Revision.
- Display Routing lifecycle Status.
- Accept optional Effective Date.
- Manage multiple ordered Routing Steps.
- Provide Item / Operation / Machine dropdown data.

Important:
- Item is selected only during first Routing creation.
- Item cannot be changed after Routing creation.
- Only Draft Routing is editable.
- Released / Superseded Routing is read-only.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ItemProcessRouting
{
    public class ItemProcessRoutingFormViewModel
    {
        #region Identification

        public int Id { get; set; }


        [Display(Name = "Routing Code")]
        public string? Code { get; set; }

        #endregion


        #region Item

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Item is required.")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }


        public string? ItemCode { get; set; }


        public string? ItemName { get; set; }

        #endregion


        #region Revision

        [Display(Name = "Revision")]
        public int RevisionNumber { get; set; }


        [Display(Name = "Status")]
        public ItemProcessRoutingStatus Status { get; set; } =
            ItemProcessRoutingStatus.Draft;


        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveFrom { get; set; }

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage = "Routing Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion


        #region Steps

        public List<ItemProcessRoutingStepViewModel> Steps
        {
            get;
            set;
        } = new();

        #endregion


        #region Dropdown Data

        public List<SelectListItem> Items
        {
            get;
            set;
        } = new();


        public List<SelectListItem> Operations
        {
            get;
            set;
        } = new();


        public List<SelectListItem> Machines
        {
            get;
            set;
        } = new();

        #endregion
    }
}