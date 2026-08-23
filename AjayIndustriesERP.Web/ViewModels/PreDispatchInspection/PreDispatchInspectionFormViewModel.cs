/*
============================================================
File: PreDispatchInspectionFormViewModel.cs

Purpose:
Provides Create / Edit form data for the
Pre-Dispatch / Final Inspection Report.

Responsibilities:
- Select Production Job source.
- Display trusted Customer / PO / Item information.
- Display Drawing snapshots.
- Capture Inspection quantities.
- Capture Invoice information.
- Capture Inspection Parameters.
- Capture normal Observations.
- Capture Interval Readings.
- Capture Inspection Result and remarks.
- Capture Inspection / Approval information.

Important:
- Customer / PO / Item / Drawing values are display-only.
- Trusted snapshot values are prepared again in
  Application Service before save.
- Inspection Lines and Observations are editable while
  the PDI Report is Draft.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.PreDispatchInspection
{
    public class PreDispatchInspectionFormViewModel
    {
        #region Identification

        public int Id { get; set; }


        public string? Code { get; set; }


        public PreDispatchInspectionStatus Status
        {
            get;
            set;
        } = PreDispatchInspectionStatus.Draft;


        public PreDispatchInspectionResult Result
        {
            get;
            set;
        } = PreDispatchInspectionResult.Pending;


        [Required(
            ErrorMessage =
                "Inspection Date is required.")]
        [DataType(
            DataType.Date)]
        public DateTime InspectionDate
        {
            get;
            set;
        } = DateTime.Today;

        #endregion


        #region Production Job

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Production Job is required.")]
        public int ProductionJobId { get; set; }


        public string? ProductionJobCode
        {
            get;
            set;
        }


        public List<SelectListItem>
            ProductionJobs
        { get; set; } = new();

        #endregion


        #region Customer PO Source

        public string? CustomerName { get; set; }


        public string? CustomerPurchaseOrderCode
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderNumber
        {
            get;
            set;
        }


        public string? CustomerItemCode
        {
            get;
            set;
        }

        #endregion


        #region Item

        public int ItemId { get; set; }


        public string? ItemCode { get; set; }


        public string? ItemName { get; set; }


        public string? PartNumber { get; set; }


        public string? UnitName { get; set; }

        #endregion


        #region Current Workshop Drawing

        public string? WorkshopDrawingNumber
        {
            get;
            set;
        }


        public string? WorkshopDrawingRevision
        {
            get;
            set;
        }

        #endregion


        #region Current Customer Drawing

        public string? CustomerDrawingNumber
        {
            get;
            set;
        }


        public string? CustomerDrawingRevision
        {
            get;
            set;
        }

        #endregion


        #region Quantity

        public decimal JobQuantity { get; set; }


        public decimal RemainingInspectionQuantity
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0.001",
            "999999999999999.999",
            ErrorMessage =
                "Inspection Quantity must be greater than zero.")]
        public decimal InspectionQuantity
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage =
                "Accepted Quantity cannot be negative.")]
        public decimal AcceptedQuantity
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage =
                "Rework Quantity cannot be negative.")]
        public decimal ReworkQuantity
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
        public decimal RejectedQuantity
        {
            get;
            set;
        }

        #endregion


        #region Invoice Information

        /*
         * Invoice module will be integrated later.
         *
         * These fields remain optional.
         */

        [StringLength(
            100,
            ErrorMessage =
                "Invoice Number cannot exceed 100 characters.")]
        public string? InvoiceNumber { get; set; }


        [DataType(
            DataType.Date)]
        public DateTime? InvoiceDate { get; set; }


        [Range(
            typeof(decimal),
            "0",
            "999999999999999.999",
            ErrorMessage =
                "Invoice Quantity cannot be negative.")]
        public decimal? InvoiceQuantity { get; set; }

        #endregion


        #region Remarks

        [StringLength(
            1000,
            ErrorMessage =
                "Supplier Remarks cannot exceed 1000 characters.")]
        public string? SupplierRemarks { get; set; }


        [StringLength(
            2000,
            ErrorMessage =
                "Inspection Remarks cannot exceed 2000 characters.")]
        public string? InspectionRemarks { get; set; }

        #endregion


        #region Inspection And Approval

        [StringLength(
            150,
            ErrorMessage =
                "Inspected By cannot exceed 150 characters.")]
        public string? InspectedBy { get; set; }


        [StringLength(
            150,
            ErrorMessage =
                "Reviewed By cannot exceed 150 characters.")]
        public string? ReviewedBy { get; set; }

        #endregion


        #region Inspection Lines

        public List<PreDispatchInspectionLineViewModel>
            Lines
        { get; set; } = new();

        #endregion
    }


    public class PreDispatchInspectionLineViewModel
    {
        #region Identification

        public int Id { get; set; }


        public int SequenceNumber { get; set; }

        #endregion


        #region Inspection Parameter

        [Required(
            ErrorMessage =
                "Inspection Parameter is required.")]
        [StringLength(
            250,
            ErrorMessage =
                "Inspection Parameter cannot exceed 250 characters.")]
        public string Parameter { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage =
                "Specification is required.")]
        [StringLength(
            500,
            ErrorMessage =
                "Specification cannot exceed 500 characters.")]
        public string Specification { get; set; } =
            string.Empty;

        #endregion


        #region Inspection Method

        [StringLength(
            250,
            ErrorMessage =
                "Inspection Method cannot exceed 250 characters.")]
        public string? InspectionMethod
        {
            get;
            set;
        }

        #endregion


        #region Result

        public PreDispatchInspectionLineResult Result
        {
            get;
            set;
        } = PreDispatchInspectionLineResult.Pending;


        [StringLength(
            1000,
            ErrorMessage =
                "Line Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }

        #endregion


        #region Observations

        public List<PreDispatchInspectionObservationViewModel>
            Observations
        { get; set; } = new();

        #endregion
    }


    public class PreDispatchInspectionObservationViewModel
    {
        #region Identification

        public int Id { get; set; }


        public int SequenceNumber { get; set; }


        public bool IsIntervalReading
        {
            get;
            set;
        }

        #endregion


        #region Reading

        /*
         * String is intentional.
         *
         * Supported examples:
         *
         * 14.05
         * 31.10
         * OK
         * FOUND OK
         * NOT OK
         */

        [StringLength(
            250,
            ErrorMessage =
                "Observation cannot exceed 250 characters.")]
        public string? Value { get; set; }

        #endregion
    }
}