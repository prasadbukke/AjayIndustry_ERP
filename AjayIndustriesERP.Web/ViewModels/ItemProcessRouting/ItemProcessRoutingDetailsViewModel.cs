/*
============================================================
File: ItemProcessRoutingDetailsViewModel.cs

Purpose:
Provides read-only Item Process Routing information.

Responsibilities:
- Display Routing Header information.
- Display Item information.
- Display Routing Revision and Status.
- Display ordered Routing Steps.
- Display Operation and Default Machine information.
- Support future Production Job references.

Important:
- This represents the Routing Template only.
- Actual Machine, Job Status and execution time belong to
  future Production Job Steps.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.ItemProcessRouting
{
    public class ItemProcessRoutingDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }

        public string Code { get; set; } =
            string.Empty;

        #endregion


        #region Item

        public int ItemId { get; set; }

        public string ItemCode { get; set; } =
            string.Empty;

        public string ItemName { get; set; } =
            string.Empty;

        #endregion


        #region Revision

        public int RevisionNumber { get; set; }

        public ItemProcessRoutingStatus Status { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion


        #region Steps

        public List<ItemProcessRoutingStepDetailsViewModel>
            Steps
        { get; set; } = new();

        #endregion
    }


    public class ItemProcessRoutingStepDetailsViewModel
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

        #endregion


        #region Estimated Time

        public decimal? SetupTimeMinutes { get; set; }

        public decimal? CycleTimeMinutes { get; set; }

        #endregion


        #region Instructions

        public string? OperationInstruction { get; set; }

        public string? Remarks { get; set; }

        #endregion
    }
}