/*
==============================================================

File : ShapeViewModel.cs

Purpose :
Represents Shape Master Create and Edit form data.

Features :
- Form validation
- Similar-name warning
- Exact duplicate detection support

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Shape
{
    /// <summary>
    /// Represents Shape Master form data.
    /// </summary>
    public class ShapeViewModel
    {
        #region Shape Information

        public int ShapeId { get; set; }

        [Display(Name = "Shape Code")]
        public string? ShapeCode { get; set; }

        [Required(ErrorMessage = "Shape Name is required.")]
        [StringLength(
            100,
            ErrorMessage = "Shape Name cannot exceed 100 characters.")]
        [Display(Name = "Shape Name")]
        public string ShapeName { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        #endregion

        #region Status

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Similar Name Validation

        /// <summary>
        /// Indicates that the user reviewed similar Shape Names
        /// and still wants to continue.
        /// </summary>
        [Display(Name = "Create despite similar names")]
        public bool ConfirmSimilarShapeName { get; set; }

        /// <summary>
        /// Contains similar existing Shape records.
        /// </summary>
        [ValidateNever]
        public List<string> SimilarShapeNames { get; set; } = new();

        #endregion
    }
}