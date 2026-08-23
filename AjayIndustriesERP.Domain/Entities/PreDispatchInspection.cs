/*
============================================================
File: PreDispatchInspection.cs

Purpose:
Represents a Pre-Dispatch / Final Inspection Report.

Responsibilities:
- Link inspection with Production Job.
- Preserve Customer PO and Item snapshot.
- Preserve Drawing revisions used during inspection.
- Store inspection quantities and final result.
- Store inspection / approval information.
- Store generated PDF information.
- Maintain Inspection Parameter Lines.

Important:
- Production Job is the primary source transaction.
- Customer / PO / Item / Drawing information is snapshotted
  for audit purposes.
- Draft reports can be edited.
- Finalized reports must remain locked.
- One Production Job may have multiple PDI Reports.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PreDispatchInspection
        : BaseEntity
    {
        #region Identification

        public int Id { get; set; }

        public string Code { get; set; } =
            string.Empty;

        public DateTime InspectionDate { get; set; }

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

        public ProductionJob ProductionJob
        {
            get;
            set;
        } = null!;

        public string ProductionJobCode { get; set; } =
            string.Empty;

        #endregion


        #region Customer Snapshot

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } =
            string.Empty;

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

        public string? CustomerItemCode { get; set; }

        #endregion


        #region Item Snapshot

        public int ItemId { get; set; }

        public string ItemCode { get; set; } =
            string.Empty;

        public string ItemName { get; set; } =
            string.Empty;

        /*
         * Printed as Part No. on the Final Inspection Report.
         *
         * Normally Customer Item Code is preferred.
         * Item Code may be used as fallback.
         */

        public string? PartNumber { get; set; }

        public string? UnitName { get; set; }

        #endregion


        #region Workshop Drawing Snapshot

        public int? WorkshopDrawingId { get; set; }

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

        public int? CustomerDrawingId { get; set; }

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

        /*
         * Invoice module will be integrated later.
         *
         * These fields remain optional so PDI development
         * does not depend on the Invoice module.
         */

        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public decimal? InvoiceQuantity { get; set; }

        #endregion


        #region Inspection Quantity

        public decimal InspectionQuantity { get; set; }

        public decimal AcceptedQuantity { get; set; }

        public decimal ReworkQuantity { get; set; }

        public decimal RejectedQuantity { get; set; }

        #endregion


        #region Remarks

        public string? SupplierRemarks { get; set; }

        public string? InspectionRemarks { get; set; }

        #endregion


        #region Inspection And Approval

        public string? InspectedBy { get; set; }

        public string? ReviewedBy { get; set; }

        #endregion


        #region Finalization

        public DateTime? FinalizedOn { get; set; }

        public string? FinalizedBy { get; set; }

        #endregion


        #region PDF

        public string? PdfFileName { get; set; }

        public string? PdfFilePath { get; set; }

        #endregion


        #region Inspection Lines

        public ICollection<PreDispatchInspectionLine>
            Lines
        { get; set; } =
            new List<PreDispatchInspectionLine>();

        #endregion
    }
}