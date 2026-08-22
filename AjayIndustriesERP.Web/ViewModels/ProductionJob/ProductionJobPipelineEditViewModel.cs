/*
============================================================
File: ProductionJobPipelineEditViewModel.cs

Purpose:
Represents the editable Production Pipeline of a Draft
Production Job.

Responsibilities:
- Display Production Job reference information.
- Display existing executable Production Job Steps.
- Allow Operation Add / Remove / Reorder.
- Capture optional Pipeline Modification Reason.
- Provide active Production Operations for selection.

Important:
- This ViewModel is used only in the Web layer.
- Pipeline editing is allowed only while Job Status is Draft.
- Saving this form modifies ProductionJobSteps only.
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

        public int ProductionJobId { get; set; }

        public string JobCode { get; set; } =
            string.Empty;

        public ProductionJobStatus Status { get; set; }

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

        public string ItemCode { get; set; } =
            string.Empty;

        public string ItemName { get; set; } =
            string.Empty;

        #endregion


        #region Pipeline Modification

        [StringLength(
            1000,
            ErrorMessage =
                "Pipeline Modification Reason cannot exceed 1000 characters.")]
        [Display(
            Name = "Pipeline Modification Reason")]
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


    public class ProductionJobPipelineStepEditViewModel
    {
        #region Identification

        public int Id { get; set; }

        #endregion


        #region Sequence

        public int SequenceNumber { get; set; }

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