/*
============================================================
File: ProductionJobPipelineEditViewModel.cs

Purpose:
Represents the editable Production Pipeline of one
Production Job Item.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Display Production Job reference information.
- Identify the selected Production Job Item.
- Display Item information.
- Display existing executable Item Pipeline Steps.
- Allow Operation Add / Remove / Reorder.
- Capture optional Pipeline Modification Reason.
- Provide active Production Operations for selection.

Important:
- This ViewModel is used only in the Web layer.
- Pipeline belongs to ProductionJobItem.
- Pipeline can be edited only before Production starts
  for the selected Item.
- Parent Job may already be InProgress because another Item
  may have started Production.
- Saving this form modifies only the selected
  ProductionJobItem Steps.
- Item Process Routing Master is never modified.
- Sequence is normalized as 1, 2, 3, 4...
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobPipelineEditViewModel
    {
        #region Production Job

        public int ProductionJobId
        {
            get;
            set;
        }


        public string JobCode
        {
            get;
            set;
        } = string.Empty;


        public ProductionJobStatus Status
        {
            get;
            set;
        }

        #endregion


        #region Production Job Item

        /// <summary>
        /// Identifies the ProductionJobItem whose
        /// Pipeline is being edited.
        /// </summary>
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Production Job Item is required.")]
        public int ProductionJobItemId
        {
            get;
            set;
        }

        #endregion


        #region Customer PO

        public string CustomerPurchaseOrderNumber
        {
            get;
            set;
        } = string.Empty;


        public string CustomerName
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Item

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

        #endregion


        #region Pipeline Modification

        [StringLength(
            1000,
            ErrorMessage =
                "Pipeline Modification Reason cannot exceed 1000 characters.")]
        [Display(
            Name =
                "Pipeline Modification Reason")]
        public string? PipelineModificationReason
        {
            get;
            set;
        }

        #endregion


        #region Pipeline Steps

        public List<ProductionJobPipelineStepEditViewModel>
            Steps
        {
            get;
            set;
        } = new();

        #endregion


        #region Lookups

        public List<SelectListItem>
            AvailableOperations
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Production Job Pipeline Step
    ============================================================
    */

    public class ProductionJobPipelineStepEditViewModel
    {
        #region Identification

        /// <summary>
        /// Existing ProductionJobStep Id.
        ///
        /// Zero means a new Pipeline Step.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        #endregion


        #region Sequence

        public int SequenceNumber
        {
            get;
            set;
        }

        #endregion


        #region Operation

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Please select a Production Operation.")]
        public int ProductionOperationId
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

        #endregion
    }
}