using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Brand
{
    public class BrandViewModel
    {
        public int BrandId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Brand Code")]
        public string? BrandCode { get; set; }

        [Required]
        [Display(Name = "Brand Name")]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}