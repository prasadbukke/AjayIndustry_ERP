using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Company
{
    public class CompanyViewModel
    {
        public int CompanyId { get; set; }

        [Display(Name = "Company Code")]
        public string? CompanyCode { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        [StringLength(100)]
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

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Url]
        [Display(Name = "Website")]
        public string Website { get; set; } = string.Empty;

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Country { get; set; } = "India";

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}