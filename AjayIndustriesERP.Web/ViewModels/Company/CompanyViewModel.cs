using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Company
{
    public class CompanyViewModel
    {
        public int CompanyId { get; set; }

        [Display(Name = "Company Code")]
        public string CompanyCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "GST Number")]
        public string GstNumber { get; set; } = string.Empty;

        [Display(Name = "PAN Number")]
        public string PanNumber { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Website")]
        public string Website { get; set; } = string.Empty;

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = "India";

        public bool IsActive { get; set; } = true;
    }
}