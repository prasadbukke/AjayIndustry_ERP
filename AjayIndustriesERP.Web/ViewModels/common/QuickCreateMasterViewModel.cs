/*
==============================================================

File : QuickCreateMasterViewModel.cs

Purpose :
Represents reusable Quick Create modal data for name-based
master records.

Used By :
- Item Category
- Brand
- UOM
- Future name-based masters

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Common
{
    /// <summary>
    /// Represents the common Quick Create Master form.
    /// </summary>
    public class QuickCreateMasterViewModel
    {
        #region Master Configuration

        /// <summary>
        /// Identifies the master being created.
        /// Examples: Category, Brand, Uom.
        /// </summary>
        [Required]
        public string MasterType { get; set; } = string.Empty;

        /// <summary>
        /// Modal heading displayed to the user.
        /// </summary>
        [ValidateNever]
        public string MasterTitle { get; set; } = "Add Master";

        /// <summary>
        /// Label displayed for the Name field.
        /// </summary>
        [ValidateNever]
        public string NameLabel { get; set; } = "Name";

        /// <summary>
        /// Label displayed for the Code field.
        /// </summary>
        [ValidateNever]
        public string CodeLabel { get; set; } = "Code";

        /// <summary>
        /// Determines whether the Code field is required.
        /// UOM requires a manually entered code.
        /// Category and Brand codes are auto-generated.
        /// </summary>
        public bool RequiresCode { get; set; }

        #endregion

        #region Form Fields

        [StringLength(
            20,
            ErrorMessage = "Code cannot exceed 20 characters.")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(
            150,
            ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Confirms that the user reviewed similar records
        /// and still wants to create a new record.
        /// </summary>
        public bool ConfirmSimilarName { get; set; }

        #endregion

        #region Endpoint Configuration

        /// <summary>
        /// URL used to submit the Quick Create form.
        /// </summary>
        [ValidateNever]
        public string FormAction { get; set; } = string.Empty;

        /// <summary>
        /// URL used for live similar-name checking.
        /// </summary>
        [ValidateNever]
        public string SimilarCheckUrl { get; set; } = string.Empty;

        #endregion

        #region Similar Records

        /// <summary>
        /// Existing exact or similar records.
        /// </summary>
        [ValidateNever]
        public List<QuickCreateSuggestionViewModel> SimilarRecords
        {
            get;
            set;
        } = new();

        #endregion
    }

    /// <summary>
    /// Represents an existing record displayed as a live
    /// similar-name suggestion.
    /// </summary>
    public class QuickCreateSuggestionViewModel
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the entered name exactly matches
        /// this existing record.
        /// </summary>
        public bool IsExactMatch { get; set; }

        /// <summary>
        /// Text displayed inside the suggestion list.
        /// </summary>
        public string DisplayText =>
            string.IsNullOrWhiteSpace(Code)
                ? Name
                : $"{Code} - {Name}";
    }
}