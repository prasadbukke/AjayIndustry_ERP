/*
============================================================
File: ProductionJobDetailsViewModel.cs

Purpose:
Provides complete read-only Production Job information.

Responsibilities:
- Display Production Job Header.
- Display Customer PO source information.
- Display Item and Job Quantity.
- Display current Workshop Drawing.
- Display current Customer Drawing.
- Display Routing snapshot.
- Display Production Job lifecycle.
- Display executable Production Pipeline Steps.

Important:
- Job Steps are snapshots copied from the Released Routing.
- Routing changes later must not modify this Job.
- Workshop Drawing represents the current Item Drawing.
- Customer Drawing represents the current Drawing for
  Customer + Item.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobDetailsViewModel
    {
        #region Job Identification

        public int Id { get; set; }

        public string Code { get; set; } =
            string.Empty;

        public ProductionJobStatus Status { get; set; }

        #endregion


        #region Customer PO

        public int CustomerPurchaseOrderItemId { get; set; }

        public string CustomerPurchaseOrderCode { get; set; } =
            string.Empty;

        public string CustomerPurchaseOrderNumber { get; set; } =
            string.Empty;

        public string CustomerName { get; set; } =
            string.Empty;

        #endregion


        #region Item

        public int ItemId { get; set; }

        public string ItemCode { get; set; } =
            string.Empty;

        public string ItemName { get; set; } =
            string.Empty;

        public string? UnitName { get; set; }

        public decimal JobQuantity { get; set; }

        #endregion


        #region Current Workshop Drawing

        public int? DrawingId { get; set; }

        public string? DrawingNumber { get; set; }

        public string? DrawingName { get; set; }

        public string? DrawingType { get; set; }

        public string? DrawingRevisionNumber { get; set; }

        public string? DrawingFileName { get; set; }

        public string? DrawingFilePath { get; set; }

        public string? DrawingDescription { get; set; }

        #endregion


        #region Current Customer Drawing

        public int? CustomerDrawingId { get; set; }

        public string? CustomerDrawingNumber { get; set; }

        public string? CustomerDrawingName { get; set; }

        public string? CustomerDrawingType { get; set; }

        public string? CustomerDrawingRevisionNumber
        {
            get;
            set;
        }

        public string? CustomerDrawingFileName { get; set; }

        public string? CustomerDrawingFilePath { get; set; }

        public string? CustomerDrawingDescription
        {
            get;
            set;
        }

        #endregion


        #region Routing

        public int ItemProcessRoutingId { get; set; }

        public string RoutingCode { get; set; } =
            string.Empty;

        public int RoutingRevisionNumber { get; set; }

        #endregion


        #region Planning

        public DateTime? PlannedStartOn { get; set; }

        public DateTime? PlannedCompletionOn { get; set; }

        public DateTime? StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public DateTime? CancelledOn { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        public string? CancellationReason { get; set; }

        #endregion


        #region Steps

        public List<ProductionJobStepDetailsViewModel>
            Steps
        { get; set; } = new();

        #endregion
    }


    public class ProductionJobStepDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }

        public int SequenceNumber { get; set; }

        #endregion


        #region Operation

        public int ProductionOperationId { get; set; }

        public string OperationCode { get; set; } =
            string.Empty;

        public string OperationName { get; set; } =
            string.Empty;

        public ProductionOperationType OperationType
        {
            get;
            set;
        }

        #endregion


        #region Machine

        public int? DefaultMachineId { get; set; }

        public string? DefaultMachineCode { get; set; }

        public string? DefaultMachineName { get; set; }


        public int? AssignedMachineId { get; set; }

        public string? AssignedMachineCode { get; set; }

        public string? AssignedMachineName { get; set; }

        #endregion


        #region Time

        public decimal? SetupTimeMinutes { get; set; }

        public decimal? CycleTimeMinutes { get; set; }

        public DateTime? StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        #endregion


        #region Execution

        public ProductionJobStepStatus Status { get; set; }

        public decimal? GoodQuantity { get; set; }

        public decimal? RejectedQuantity { get; set; }

        #endregion


        #region Instructions

        public string? OperationInstruction { get; set; }

        public string? RoutingRemarks { get; set; }

        public string? ExecutionRemarks { get; set; }

        #endregion


        #region History

        public List<ProductionJobStepHistoryViewModel>
            History
        { get; set; } = new();

        #endregion
    }


    public class ProductionJobStepHistoryViewModel
    {
        #region Status

        public ProductionJobStepStatus? PreviousStatus
        {
            get;
            set;
        }

        public ProductionJobStepStatus NewStatus
        {
            get;
            set;
        }

        #endregion


        #region Machine

        public string? MachineCode { get; set; }

        public string? MachineName { get; set; }

        #endregion


        #region Quantity

        public decimal? GoodQuantity { get; set; }

        public decimal? RejectedQuantity { get; set; }

        #endregion


        #region Audit

        public string? Remarks { get; set; }

        public DateTime ChangedOn { get; set; }

        public string ChangedBy { get; set; } =
            string.Empty;

        #endregion
    }
}