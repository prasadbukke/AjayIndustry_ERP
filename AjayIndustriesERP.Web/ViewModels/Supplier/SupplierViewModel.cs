/*
==============================================================

File : SupplierViewModel.cs

Purpose :
Represents Supplier Master Create/Edit form data.

Features :
- Supplier information
- Contact details
- GSTIN / PAN
- Address
- Payment terms
- Live similar Supplier Name warning
- Exact duplicate support

==============================================================
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Supplier
{
    /// <summary>
    /// Represents Supplier Master form data.
    /// </summary>
    public class SupplierViewModel
    {
        #region Supplier Information

        public int SupplierId { get; set; }

        [Display(Name = "Supplier Code")]
        public string? SupplierCode { get; set; }

        [Required(
            ErrorMessage = "Supplier Name is required.")]
        [StringLength(
            150,
            ErrorMessage =
                "Supplier Name cannot exceed 150 characters.")]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } =
            string.Empty;

        [StringLength(
            100,
            ErrorMessage =
                "Contact Person cannot exceed 100 characters.")]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        #endregion

        #region Contact Information

        [StringLength(
            20,
            ErrorMessage =
                "Mobile Number cannot exceed 20 characters.")]
        [RegularExpression(
            @"^[0-9+\-\s()]*$",
            ErrorMessage =
                "Please enter a valid Mobile Number.")]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        [StringLength(
            20,
            ErrorMessage =
                "Alternate Mobile Number cannot exceed 20 characters.")]
        [RegularExpression(
            @"^[0-9+\-\s()]*$",
            ErrorMessage =
                "Please enter a valid Alternate Mobile Number.")]
        [Display(Name = "Alternate Mobile Number")]
        public string? AlternateMobileNumber { get; set; }

        [StringLength(
            150,
            ErrorMessage =
                "Email cannot exceed 150 characters.")]
        [EmailAddress(
            ErrorMessage =
                "Please enter a valid Email address.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        #endregion

        #region Tax Information

        [StringLength(
            15,
            MinimumLength = 15,
            ErrorMessage =
                "GSTIN must be 15 characters.")]
        [RegularExpression(
            @"^[0-9]{2}[A-Za-z]{5}[0-9]{4}[A-Za-z][1-9A-Za-z]Z[0-9A-Za-z]$",
            ErrorMessage =
                "Please enter a valid GSTIN.")]
        [Display(Name = "GSTIN")]
        public string? Gstin { get; set; }

        [StringLength(
            10,
            MinimumLength = 10,
            ErrorMessage =
                "PAN must be 10 characters.")]
        [RegularExpression(
            @"^[A-Za-z]{5}[0-9]{4}[A-Za-z]$",
            ErrorMessage =
                "Please enter a valid PAN.")]
        [Display(Name = "PAN")]
        public string? Pan { get; set; }

        #endregion

        #region Address

        [StringLength(
            200,
            ErrorMessage =
                "Address Line 1 cannot exceed 200 characters.")]
        [Display(Name = "Address Line 1")]
        public string? AddressLine1 { get; set; }

        [StringLength(
            200,
            ErrorMessage =
                "Address Line 2 cannot exceed 200 characters.")]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [StringLength(
            100,
            ErrorMessage =
                "City cannot exceed 100 characters.")]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(
            100,
            ErrorMessage =
                "State cannot exceed 100 characters.")]
        [Display(Name = "State")]
        public string? State { get; set; }

        [StringLength(
            10,
            ErrorMessage =
                "Pincode cannot exceed 10 characters.")]
        [Display(Name = "Pincode")]
        public string? Pincode { get; set; }

        #endregion

        #region Commercial Information

        [Range(
            0,
            3650,
            ErrorMessage =
                "Payment Terms Days must be between 0 and 3650.")]
        [Display(Name = "Payment Terms (Days)")]
        public int? PaymentTermsDays { get; set; }

        #endregion

        #region Additional Information

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

        #region Similar Supplier Validation

        /// <summary>
        /// Indicates that the user reviewed similar Supplier
        /// Names and still wants to continue.
        /// </summary>
        [Display(
            Name = "Create despite similar Supplier names")]
        public bool ConfirmSimilarSupplierName
        {
            get;
            set;
        }

        /// <summary>
        /// Similar Supplier records displayed to the user.
        /// </summary>
        [ValidateNever]
        public List<string> SimilarSupplierNames
        {
            get;
            set;
        } = new();

        #endregion
    }
}