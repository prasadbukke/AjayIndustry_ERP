using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Warehouse
{
    public class WarehouseViewModel
    {
        public int WarehouseId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Warehouse Code")]
        public string? WarehouseCode { get; set; }

        [Required]
        [Display(Name = "Warehouse Name")]
        [StringLength(100)]
        public string WarehouseName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Warehouse Type")]
        public string WarehouseType { get; set; } = string.Empty;

        [Display(Name = "Default Warehouse")]
        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;
    }
}