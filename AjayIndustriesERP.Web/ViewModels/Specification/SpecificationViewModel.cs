/*
==============================================================

File : SpecificationViewModel.cs

Purpose :
Represents Specification Master Create/Edit form data.

Features :
- Form validation
- Live similar-name warning
- Exact duplicate detection support

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Specification
{
    /// <summary>
    /// Represents Specification Master form data.
    /// </summary>
    public class SpecificationViewModel
    {
        #region Specification Information

        public int SpecificationId { get; set; }

        [Display(Name = "Specification Code")]
        public string? SpecificationCode { get; set; }

        [Required(
            ErrorMessage = "Specification Name is required.")]
        [StringLength(
            100,
            ErrorMessage =
                "Specification Name cannot exceed 100 characters.")]
        [Display(Name = "Specification Name")]
        public string SpecificationName { get; set; } =
            string.Empty;

        [StringLength(
            500,
            ErrorMessage =
                "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        #endregion

        #region Status

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Similar Name Validation

        /// <summary>
        /// Indicates that the user reviewed similar
        /// Specification Names and still wants to continue.
        /// </summary>
        [Display(
            Name = "Create despite similar names")]
        public bool ConfirmSimilarSpecificationName
        {
            get;
            set;
        }

        /// <summary>
        /// Contains similar existing Specification records.
        /// </summary>
        [ValidateNever]
        public List<string> SimilarSpecificationNames
        {
            get;
            set;
        } = new();

        #endregion
    }
}