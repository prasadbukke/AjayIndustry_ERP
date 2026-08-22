/*
============================================================
File: ProductionJobStartStepViewModel.cs

Purpose:
Provides data required to start a Production Job Step.

Responsibilities:
- Display Production Job and Operation information.
- Display Default Machine.
- Select Actual Assigned Machine.
- Accept optional Start remarks.

Important:
- Actual Machine may differ from Default Machine.
- Machine is optional because some Operations such as
  Inspection may not require a Machine.
============================================================
*/

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobStartStepViewModel
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

        #endregion


        #region Machine

        public int? DefaultMachineId { get; set; }

        public string? DefaultMachineCode { get; set; }

        public string? DefaultMachineName { get; set; }


        [Display(Name = "Actual Machine")]
        public int? AssignedMachineId { get; set; }


        public List<SelectListItem> Machines
        {
            get;
            set;
        } = new();

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion
    }
}