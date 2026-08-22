/*
============================================================
File: ProductionJobCompleteStepViewModel.cs

Purpose:
Provides data required to complete an In Progress
Production Job Step.

Responsibilities:
- Display Production Job and Operation information.
- Display Assigned Machine.
- Accept Good Quantity.
- Accept Rejected Quantity.
- Accept execution remarks.

Important:
- Good + Rejected Quantity cannot exceed Job Quantity.
- Completion timestamp is generated automatically.
============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobCompleteStepViewModel
    {
        #region Identification

        public int ProductionJobId { get; set; }

        public int ProductionJobStepId { get; set; }

        #endregion


        #region Job Information

        public string JobCode { get; set; } =
            string.Empty;

        public string ItemCode { get; set; } =
            string.Empty;

        public string ItemName { get; set; } =
            string.Empty;

        public decimal JobQuantity { get; set; }

        public string? UnitName { get; set; }

        #endregion


        #region Step Information

        public int SequenceNumber { get; set; }

        public string OperationCode { get; set; } =
            string.Empty;

        public string OperationName { get; set; } =
            string.Empty;

        public string? AssignedMachineCode { get; set; }

        public string? AssignedMachineName { get; set; }

        #endregion


        #region Production Quantity

        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage = "Good Quantity cannot be negative.")]
        [Display(Name = "Good Quantity")]
        public decimal GoodQuantity { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage = "Rejected Quantity cannot be negative.")]
        [Display(Name = "Rejected Quantity")]
        public decimal RejectedQuantity { get; set; }

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage = "Execution Remarks cannot exceed 1000 characters.")]
        [Display(Name = "Execution Remarks")]
        public string? Remarks { get; set; }

        #endregion
    }
}