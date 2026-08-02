using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ItemCategory
{
    public class ItemCategoryViewModel
    {
        public int ItemCategoryId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Category Code")]
        public string? CategoryCode { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}