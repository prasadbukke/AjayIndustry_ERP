/*
============================================================
File: ProductionJobDetailsViewModel.cs

Purpose:
Provides complete read-only Production Job information.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Display Production Job Header.
- Display Customer PO information.
- Display all Production Job Items.
- Display Item-wise Production progress.
- Display Item-wise Workshop Drawing.
- Display Item-wise Customer Drawing.
- Display Item-wise Routing snapshot.
- Display Item-wise executable Production Pipeline.
- Display Production Job lifecycle.

Quantity Meaning:

OrderedQuantity
    Customer PO ordered quantity.

ProductionQuantity
    Current cumulative Production target planned by Admin.

CompletedQuantity
    Cumulative final GOOD Production output.

PendingQuantity
    OrderedQuantity - CompletedQuantity.

ProductionPendingQuantity
    ProductionQuantity - CompletedQuantity.

Important:
- One Customer PO has one Production Job.
- Each Customer PO Item has its own ProductionJobItem.
- Each ProductionJobItem has its own Pipeline.
- Routing changes later must not modify copied Job Steps.
- Worker executes Pipeline but does not change
  ProductionQuantity.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.ProductionJob
{
    public class ProductionJobDetailsViewModel
    {
        #region Production Job

        public int Id { get; set; }


        public string Code { get; set; } =
            string.Empty;


        public ProductionJobStatus Status
        {
            get;
            set;
        }

        #endregion


        #region Customer Purchase Order

        public int CustomerPurchaseOrderId
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


        public string CustomerName
        {
            get;
            set;
        } = string.Empty;


        public DateTime? CustomerPurchaseOrderDate
        {
            get;
            set;
        }


        public DateTime? ReceivedDate
        {
            get;
            set;
        }


        public DateTime? RequiredDeliveryDate
        {
            get;
            set;
        }

        #endregion


        #region Planning

        public DateTime? PlannedStartOn
        {
            get;
            set;
        }


        public DateTime? PlannedCompletionOn
        {
            get;
            set;
        }


        public DateTime? StartedOn
        {
            get;
            set;
        }


        public DateTime? CompletedOn
        {
            get;
            set;
        }


        public DateTime? CancelledOn
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        public string? Remarks
        {
            get;
            set;
        }


        public string? CancellationReason
        {
            get;
            set;
        }

        #endregion


        #region Production Items

        public List<ProductionJobDetailsItemViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Production Job Item
    ============================================================
    */

    public class ProductionJobDetailsItemViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public int CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public int ItemId
        {
            get;
            set;
        }

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


        public string? UnitName
        {
            get;
            set;
        }

        #endregion


        #region Quantity

        public decimal OrderedQuantity
        {
            get;
            set;
        }


        public decimal ProductionQuantity
        {
            get;
            set;
        }


        public decimal CompletedQuantity
        {
            get;
            set;
        }


        public decimal PendingQuantity =>
            Math.Max(
                0m,
                OrderedQuantity -
                CompletedQuantity);


        public decimal ProductionPendingQuantity =>
            Math.Max(
                0m,
                ProductionQuantity -
                CompletedQuantity);


        public bool IsCurrentProductionCompleted =>
            ProductionQuantity > 0m
            &&
            CompletedQuantity >=
                ProductionQuantity;


        public bool IsProductionCompleted =>
            OrderedQuantity > 0m
            &&
            CompletedQuantity >=
                OrderedQuantity;

        #endregion


        #region Routing

        public int ItemProcessRoutingId
        {
            get;
            set;
        }


        public string RoutingCode
        {
            get;
            set;
        } = string.Empty;


        public int RoutingRevisionNumber
        {
            get;
            set;
        }


        public string? PipelineModificationReason
        {
            get;
            set;
        }

        #endregion


        #region Current Workshop Drawing

        public int? DrawingId
        {
            get;
            set;
        }


        public string? DrawingNumber
        {
            get;
            set;
        }


        public string? DrawingName
        {
            get;
            set;
        }


        public string? DrawingType
        {
            get;
            set;
        }


        public string? DrawingRevisionNumber
        {
            get;
            set;
        }


        public string? DrawingFileName
        {
            get;
            set;
        }


        public string? DrawingFilePath
        {
            get;
            set;
        }


        public string? DrawingDescription
        {
            get;
            set;
        }

        #endregion


        #region Current Customer Drawing

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


        public string? CustomerDrawingName
        {
            get;
            set;
        }


        public string? CustomerDrawingType
        {
            get;
            set;
        }


        public string? CustomerDrawingRevisionNumber
        {
            get;
            set;
        }


        public string? CustomerDrawingFileName
        {
            get;
            set;
        }


        public string? CustomerDrawingFilePath
        {
            get;
            set;
        }


        public string? CustomerDrawingDescription
        {
            get;
            set;
        }

        #endregion


        #region Production Steps

        public List<ProductionJobStepDetailsViewModel>
            Steps
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Production Job Step
    ============================================================
    */

    public class ProductionJobStepDetailsViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public int SequenceNumber
        {
            get;
            set;
        }

        #endregion


        #region Operation

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


        public ProductionOperationType OperationType
        {
            get;
            set;
        }

        #endregion


        #region Machine

        public int? DefaultMachineId
        {
            get;
            set;
        }


        public string? DefaultMachineCode
        {
            get;
            set;
        }


        public string? DefaultMachineName
        {
            get;
            set;
        }


        public int? AssignedMachineId
        {
            get;
            set;
        }


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


        #region Time

        public decimal? SetupTimeMinutes
        {
            get;
            set;
        }


        public decimal? CycleTimeMinutes
        {
            get;
            set;
        }


        public DateTime? StartedOn
        {
            get;
            set;
        }


        public DateTime? CompletedOn
        {
            get;
            set;
        }

        #endregion


        #region Execution

        public ProductionJobStepStatus Status
        {
            get;
            set;
        }


        public decimal? GoodQuantity
        {
            get;
            set;
        }


        public decimal? RejectedQuantity
        {
            get;
            set;
        }

        #endregion


        #region Instructions

        public string? OperationInstruction
        {
            get;
            set;
        }


        public string? RoutingRemarks
        {
            get;
            set;
        }


        public string? ExecutionRemarks
        {
            get;
            set;
        }

        #endregion


        #region History

        public List<ProductionJobStepHistoryViewModel>
            History
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Production Job Step History
    ============================================================
    */

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

        public string? MachineCode
        {
            get;
            set;
        }


        public string? MachineName
        {
            get;
            set;
        }

        #endregion


        #region Quantity

        public decimal? GoodQuantity
        {
            get;
            set;
        }


        public decimal? RejectedQuantity
        {
            get;
            set;
        }

        #endregion


        #region Audit

        public string? Remarks
        {
            get;
            set;
        }


        public DateTime ChangedOn
        {
            get;
            set;
        }


        public string ChangedBy
        {
            get;
            set;
        } = string.Empty;

        #endregion
    }
}