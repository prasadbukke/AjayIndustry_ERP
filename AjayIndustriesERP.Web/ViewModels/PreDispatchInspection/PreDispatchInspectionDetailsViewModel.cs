/*
============================================================
File: PreDispatchInspectionDetailsViewModel.cs

Purpose:
Provides complete read-only Pre-Dispatch / Final
Inspection Report information.

Responsibilities:
- Display PDI Report identification.
- Display Production Job source.
- Display Customer / Customer PO snapshot.
- Display Item / Part information.
- Display Drawing snapshots.
- Display Invoice information.
- Display Inspection quantities.
- Display Inspection Parameters.
- Display Observations and Interval Readings.
- Display Inspection result and remarks.
- Display Inspection / Approval information.
- Display Finalization and PDF information.

Important:
- This ViewModel represents saved PDI snapshot data.
- Finalized PDI Reports are read-only audit documents.
- Drawing information must represent the snapshot stored
  against the PDI Report.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.PreDispatchInspection
{
    public class PreDispatchInspectionDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }


        public string Code { get; set; } =
            string.Empty;


        public DateTime InspectionDate
        {
            get;
            set;
        }


        public PreDispatchInspectionStatus Status
        {
            get;
            set;
        }


        public PreDispatchInspectionResult Result
        {
            get;
            set;
        }

        #endregion


        #region Production Job

        public int ProductionJobId { get; set; }


        public string ProductionJobCode
        {
            get;
            set;
        } = string.Empty;


        public decimal JobQuantity { get; set; }

        #endregion


        #region Customer Snapshot

        public int CustomerId { get; set; }


        public string CustomerName
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Customer PO Snapshot

        public int CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public string CustomerPurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;


        public string CustomerPurchaseOrderNumber
        {
            get;
            set;
        } = string.Empty;


        public string? CustomerItemCode
        {
            get;
            set;
        }

        #endregion


        #region Item Snapshot

        public int ItemId { get; set; }


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


        public string? PartNumber
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


        #region Workshop Drawing Snapshot

        public int? WorkshopDrawingId
        {
            get;
            set;
        }


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


        #region Customer Drawing Snapshot

        public int? CustomerDrawingId
        {
            get;
            set;
        }


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


        #region Invoice Information

        public string? InvoiceNumber
        {
            get;
            set;
        }


        public DateTime? InvoiceDate
        {
            get;
            set;
        }


        public decimal? InvoiceQuantity
        {
            get;
            set;
        }

        #endregion


        #region Inspection Quantity

        public decimal InspectionQuantity
        {
            get;
            set;
        }


        public decimal AcceptedQuantity
        {
            get;
            set;
        }


        public decimal ReworkQuantity
        {
            get;
            set;
        }


        public decimal RejectedQuantity
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        public string? SupplierRemarks
        {
            get;
            set;
        }


        public string? InspectionRemarks
        {
            get;
            set;
        }

        #endregion


        #region Inspection And Approval

        public string? InspectedBy
        {
            get;
            set;
        }


        public string? ReviewedBy
        {
            get;
            set;
        }

        #endregion


        #region Finalization

        public DateTime? FinalizedOn
        {
            get;
            set;
        }


        public string? FinalizedBy
        {
            get;
            set;
        }

        #endregion


        #region PDF

        public string? PdfFileName
        {
            get;
            set;
        }


        public string? PdfFilePath
        {
            get;
            set;
        }

        #endregion


        #region Inspection Lines

        public List<PreDispatchInspectionLineDetailsViewModel>
            Lines
        { get; set; } = new();

        #endregion
    }


    public class PreDispatchInspectionLineDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }


        public int SequenceNumber
        {
            get;
            set;
        }

        #endregion


        #region Inspection Parameter

        public string Parameter
        {
            get;
            set;
        } = string.Empty;


        public string Specification
        {
            get;
            set;
        } = string.Empty;


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
        }


        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Observations

        public List<PreDispatchInspectionObservationDetailsViewModel>
            Observations
        { get; set; } = new();

        #endregion


        #region Display Helpers

        /*
         * Normal observations used by the frozen
         * Final Inspection Report:
         *
         * Observation 1 ... Observation 7
         */

        public List<PreDispatchInspectionObservationDetailsViewModel>
            NormalObservations
            => Observations
                .Where(x =>
                    !x.IsIntervalReading)
                .OrderBy(x =>
                    x.SequenceNumber)
                .ToList();


        /*
         * Interval readings used by the frozen
         * Final Inspection Report:
         *
         * Interval 1 ... Interval 3
         */

        public List<PreDispatchInspectionObservationDetailsViewModel>
            IntervalReadings
            => Observations
                .Where(x =>
                    x.IsIntervalReading)
                .OrderBy(x =>
                    x.SequenceNumber)
                .ToList();

        #endregion
    }


    public class PreDispatchInspectionObservationDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }


        public int SequenceNumber
        {
            get;
            set;
        }


        public bool IsIntervalReading
        {
            get;
            set;
        }

        #endregion


        #region Reading

        public string? Value
        {
            get;
            set;
        }

        #endregion
    }
}