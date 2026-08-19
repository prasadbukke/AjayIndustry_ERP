/*
============================================================
File: CustomerFormViewModel.cs

Purpose:
Provides validated Customer Master data to Create and Edit forms.

Responsibilities:
- Accept Customer information from Web UI.
- Validate required fields.
- Validate GSTIN and PAN formats.
- Validate Indian mobile numbers.
- Validate Email format.
- Validate Indian Pincode.
- Validate Website URL.
- Validate Credit Days.
- Support Create and Edit using reusable _Form.

Important:
- Business validation is repeated in CustomerService so that
  validation remains enforced outside MVC as well.
- Duplicate GSTIN / Email / Mobile validation is handled by
  CustomerService.
============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Customer
{
    public class CustomerFormViewModel
    {
        #region Identification

        public int Id { get; set; }


        [StringLength(50)]
        [Display(Name = "Customer Code")]
        public string? Code { get; set; }

        #endregion


        #region Customer Information

        [Required(
            ErrorMessage = "Customer Name is required.")]
        [StringLength(
            200,
            ErrorMessage = "Customer Name cannot exceed 200 characters.")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } =
            string.Empty;


        [StringLength(
            250,
            ErrorMessage = "Legal Name cannot exceed 250 characters.")]
        [Display(Name = "Legal Name")]
        public string? LegalName { get; set; }

        #endregion


        #region Tax Information

        [StringLength(15)]
        [RegularExpression(
            @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$",
            ErrorMessage = "Enter a valid 15-character GSTIN.")]
        [Display(Name = "GSTIN")]
        public string? GSTIN { get; set; }


        [StringLength(10)]
        [RegularExpression(
            @"^[A-Z]{5}[0-9]{4}[A-Z]$",
            ErrorMessage = "Enter a valid PAN number.")]
        [Display(Name = "PAN")]
        public string? PAN { get; set; }

        #endregion


        #region Primary Contact

        [StringLength(150)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }


        [RegularExpression(
            @"^[6-9][0-9]{9}$",
            ErrorMessage = "Enter a valid 10-digit Mobile Number.")]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }


        [RegularExpression(
            @"^[6-9][0-9]{9}$",
            ErrorMessage = "Enter a valid 10-digit Alternate Mobile Number.")]
        [Display(Name = "Alternate Mobile")]
        public string? AlternateMobileNumber { get; set; }


        [EmailAddress(
            ErrorMessage = "Enter a valid Email Address.")]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        #endregion


        #region Primary Address

        [Required(
            ErrorMessage = "Address Line 1 is required.")]
        [StringLength(250)]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } =
            string.Empty;


        [StringLength(250)]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }


        [Required(
            ErrorMessage = "City is required.")]
        [StringLength(100)]
        public string City { get; set; } =
            string.Empty;


        [StringLength(100)]
        public string? District { get; set; }


        [Required(
            ErrorMessage = "State is required.")]
        [StringLength(100)]
        public string State { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Pincode is required.")]
        [RegularExpression(
            @"^[1-9][0-9]{5}$",
            ErrorMessage = "Enter a valid 6-digit Pincode.")]
        public string Pincode { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Country is required.")]
        [StringLength(100)]
        public string Country { get; set; } =
            "India";

        #endregion


        #region Commercial Information

        [StringLength(250)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; }


        [Range(
            0,
            3650,
            ErrorMessage = "Credit Days cannot be negative.")]
        [Display(Name = "Credit Days")]
        public int? CreditDays { get; set; }

        #endregion


        #region Other Information

        [StringLength(250)]
        [Url(
            ErrorMessage =
                "Enter a valid Website URL including http:// or https://.")]
        public string? Website { get; set; }


        [StringLength(
            1000,
            ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion
    }
}