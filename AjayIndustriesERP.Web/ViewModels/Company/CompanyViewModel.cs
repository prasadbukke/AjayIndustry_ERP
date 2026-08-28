using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Company
{
    public class CompanyViewModel
    {
        public int CompanyId { get; set; }

        [Display(Name = "Company Code")]
        public string? CompanyCode { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [RegularExpression(
    @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
    ErrorMessage = "Invalid GST Number.")]
        [Display(Name = "GST Number")]
        public string? GstNumber { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$",
            ErrorMessage = "Invalid PAN Number.")]
        [Display(Name = "PAN Number")]
        public string PanNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10 digit mobile number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid Website URL.")]
        [Display(Name = "Website")]
        public string Website { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Person is required.")]
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100)]
        public string Country { get; set; } = "India";
            
                

        [Required(ErrorMessage = "Postal Code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal Code must be 6 digits.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        #region ISO Certification

        [Display(Name = "ISO Certification Number")]
        [StringLength(
            100,
            ErrorMessage = "ISO Certification Number cannot exceed 100 characters.")]
        public string? IsoCertificationNumber
        {
            get;
            set;
        }

        #endregion


        #region Bank Details

        [Display(Name = "Bank Name")]
        [StringLength(
            200,
            ErrorMessage = "Bank Name cannot exceed 200 characters.")]
        public string? BankName
        {
            get;
            set;
        }


        [Display(Name = "Account Holder Name")]
        [StringLength(
            200,
            ErrorMessage = "Account Holder Name cannot exceed 200 characters.")]
        public string? BankAccountHolderName
        {
            get;
            set;
        }


        [Display(Name = "Account Number")]
        [StringLength(
            100,
            ErrorMessage = "Account Number cannot exceed 100 characters.")]
        public string? BankAccountNumber
        {
            get;
            set;
        }


        [Display(Name = "IFSC Code")]
        [StringLength(
            20,
            ErrorMessage = "IFSC Code cannot exceed 20 characters.")]
        public string? BankIfscCode
        {
            get;
            set;
        }


        [Display(Name = "Branch Name")]
        [StringLength(
            200,
            ErrorMessage = "Branch Name cannot exceed 200 characters.")]
        public string? BankBranchName
        {
            get;
            set;
        }


        [Display(Name = "Account Type")]
        [StringLength(
            50,
            ErrorMessage = "Account Type cannot exceed 50 characters.")]
        public string? BankAccountType
        {
            get;
            set;
        }

        #endregion


        #region Terms And Conditions

        [Display(Name = "Purchase Order Terms & Conditions")]
        [StringLength(
            4000,
            ErrorMessage = "Purchase Order Terms & Conditions cannot exceed 4000 characters.")]
        public string? PurchaseOrderTermsAndConditions
        {
            get;
            set;
        }


        [Display(Name = "Invoice Terms & Conditions")]
        [StringLength(
            4000,
            ErrorMessage = "Invoice Terms & Conditions cannot exceed 4000 characters.")]
        public string? InvoiceTermsAndConditions
        {
            get;
            set;
        }

        #endregion
        public bool IsActive { get; set; } = true;
    }
}