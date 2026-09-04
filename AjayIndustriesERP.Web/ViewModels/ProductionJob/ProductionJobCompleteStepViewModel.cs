/*
============================================================
File: ProductionJobCompleteStepViewModel.cs

Purpose:
Provides data required to complete an In Progress
Production Job Step.

Responsibilities:
- Display Production Job and Item information.
- Display Production Operation information.
- Display Assigned Machine.
- Accept Good Quantity for the current completion entry.
- Accept Rejected Quantity for the current completion entry.
- Accept execution remarks.

Important:
- ProductionJobStepId identifies the exact Item Pipeline Step.
- JobQuantity currently represents the selected Item's
  ProductionQuantity for compatibility with existing views.
- Good / Rejected Quantity entered here are execution values.
- Final quantity validation is performed by
  ProductionJobService according to the current Item Pipeline
  and Production plan.
- Worker cannot modify ProductionQuantity from this screen.
- Completion timestamp is generated automatically.
============================================================
*/

using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobCompleteStepViewModel
    {
        #region Identification

        public int ProductionJobId
        {
            get;
            set;
        }

        public int ProductionJobStepId
        {
            get;
            set;
        }

        #endregion


        #region Job / Item Information

        public string JobCode
        {
            get;
            set;
        } = string.Empty;


        public string ItemCode
        {
            get;
            set;
        } = string.Empty;


        public string ItemName
        {
            get;
            set;
        } = string.Empty;


        /*
         * Compatibility property.
         *
         * In the new Item-wise Production flow this value
         * represents ProductionJobItem.ProductionQuantity.
         */
        public decimal JobQuantity
        {
            get;
            set;
        }


        public string? UnitName
        {
            get;
            set;
        }

        #endregion


        #region Step Information

        public int SequenceNumber
        {
            get;
            set;
        }


        public string OperationCode
        {
            get;
            set;
        } = string.Empty;


        public string OperationName
        {
            get;
            set;
        } = string.Empty;


        public string? AssignedMachineCode
        {
            get;
            set;
        }


        public string? AssignedMachineName
        {
            get;
            set;
        }

        #endregion


        #region Execution Quantity

        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage =
                "Good Quantity cannot be negative.")]
        [Display(Name = "Good Quantity")]
        public decimal GoodQuantity
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage =
                "Rejected Quantity cannot be negative.")]
        [Display(Name = "Rejected Quantity")]
        public decimal RejectedQuantity
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage =
                "Execution Remarks cannot exceed 1000 characters.")]
        [Display(Name = "Execution Remarks")]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion
    }
}