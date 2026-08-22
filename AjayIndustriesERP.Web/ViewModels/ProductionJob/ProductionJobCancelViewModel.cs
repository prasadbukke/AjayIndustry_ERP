/*
============================================================
File: ProductionJobCancelViewModel.cs

Purpose:
Accepts Production Job cancellation information.

Responsibilities:
- Identify the Production Job being cancelled.
- Accept mandatory Cancellation Reason.
- Provide Web-level cancellation validation.

Important:
- Cancellation applies to the entire Production Job.
- Cancellation Reason is mandatory.
- A cancelled Job cannot continue to the next Production Step.
============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobCancelViewModel
    {
        #region Identification

        public int ProductionJobId { get; set; }

        #endregion


        #region Cancellation

        [Required(
            ErrorMessage = "Cancellation Reason is required.")]
        [StringLength(
            1000,
            ErrorMessage = "Cancellation Reason cannot exceed 1000 characters.")]
        [Display(Name = "Cancellation Reason")]
        public string Reason { get; set; } =
            string.Empty;

        #endregion
    }
}