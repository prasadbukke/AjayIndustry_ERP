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

        [Required(ErrorMessage = "GST Number is required.")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
            ErrorMessage = "Invalid GST Number.")]
        [Display(Name = "GST Number")]
        public string GstNumber { get; set; } = string.Empty;

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
        [RegularExpression(@"^\d{6}$",  ErrorMessage = "Postal Code must be 6 digits.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}