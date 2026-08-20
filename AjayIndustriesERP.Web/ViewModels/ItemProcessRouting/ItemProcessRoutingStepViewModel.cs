/*
============================================================
File: ItemProcessRoutingStepViewModel.cs

Purpose:
Represents one editable Step inside an Item Process Routing form.

Responsibilities:
- Accept Routing Step sequence.
- Accept Production Operation.
- Accept optional Default Machine.
- Accept Setup Time and Cycle Time estimates.
- Accept Operation Instructions and Remarks.
- Provide Web-level validation.

Important:
- Same Operation may appear multiple times.
- Sequence Number must be unique within the Routing.
- Default Machine is optional.
- Actual Production execution data does NOT belong here.
============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ItemProcessRouting
{
    public class ItemProcessRoutingStepViewModel
    {
        #region Identification

        public int Id { get; set; }

        #endregion


        #region Sequence

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Sequence Number must be greater than zero.")]
        [Display(Name = "Sequence")]
        public int SequenceNumber { get; set; }

        #endregion


        #region Operation

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Operation is required.")]
        [Display(Name = "Operation")]
        public int ProductionOperationId { get; set; }

        #endregion


        #region Default Machine

        [Display(Name = "Default Machine")]
        public int? DefaultMachineId { get; set; }

        #endregion


        #region Estimated Time

        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage = "Setup Time cannot be negative.")]
        [Display(Name = "Setup Time (Min)")]
        public decimal? SetupTimeMinutes { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage = "Cycle Time cannot be negative.")]
        [Display(Name = "Cycle Time / Piece (Min)")]
        public decimal? CycleTimeMinutes { get; set; }

        #endregion


        #region Instructions

        [StringLength(
            1000,
            ErrorMessage = "Operation Instruction cannot exceed 1000 characters.")]
        [Display(Name = "Operation Instruction")]
        public string? OperationInstruction { get; set; }


        [StringLength(
            1000,
            ErrorMessage = "Step Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion
    }
}