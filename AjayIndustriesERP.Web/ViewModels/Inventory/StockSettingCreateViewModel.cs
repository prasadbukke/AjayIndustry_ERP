// ============================================================
// File: StockSettingCreateViewModel.cs
// Module: Inventory / Stock Management
//
// Purpose:
// Represents Create / Edit Stock Setting form.
//
// Important:
// - Item + Warehouse identify one Stock Setting.
// - Item / Warehouse cannot be changed after creation.
// - Final business validation remains in InventoryService.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Inventory
{
    public class StockSettingCreateViewModel
        : IValidatableObject
    {
        public int Id { get; set; }


        // =====================================================
        // ITEM / WAREHOUSE
        // =====================================================

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select Item.")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select Warehouse.")]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }


        // =====================================================
        // STOCK LEVELS
        // =====================================================

        [Display(Name = "Minimum Stock")]
        public decimal MinimumStock { get; set; }


        [Display(Name = "Reorder Level")]
        public decimal ReorderLevel { get; set; }


        [Display(Name = "Maximum Stock")]
        public decimal MaximumStock { get; set; }


        [Display(Name = "Reorder Quantity")]
        public decimal ReorderQuantity { get; set; }


        // =====================================================
        // WEB VALIDATION
        // =====================================================

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (MinimumStock < 0)
            {
                yield return new ValidationResult(
                    "Minimum Stock cannot be negative.",
                    new[]
                    {
                        nameof(MinimumStock)
                    });
            }


            if (ReorderLevel < MinimumStock)
            {
                yield return new ValidationResult(
                    "Reorder Level must be greater than or equal to Minimum Stock.",
                    new[]
                    {
                        nameof(ReorderLevel)
                    });
            }


            if (MaximumStock < ReorderLevel)
            {
                yield return new ValidationResult(
                    "Maximum Stock must be greater than or equal to Reorder Level.",
                    new[]
                    {
                        nameof(MaximumStock)
                    });
            }


            if (ReorderQuantity <= 0)
            {
                yield return new ValidationResult(
                    "Reorder Quantity must be greater than zero.",
                    new[]
                    {
                        nameof(ReorderQuantity)
                    });
            }
        }
    }
}