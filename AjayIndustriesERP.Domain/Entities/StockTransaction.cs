// ============================================================
// File: StockTransaction.cs
// Purpose:
// Represents the header of an Inventory stock movement.
//
// Examples:
// - Opening Stock
// - Purchase Receipt through GRN
// - Production Material Issue
// - Production Material Return
// - Finished Goods Receipt
// - Warehouse Transfer
// - Manual Stock Adjustment
// - Dispatch
// - Sales Return
//
// Important:
// Actual item-wise IN / OUT quantities are stored in
// StockTransactionLine.
//
// ReferenceType + ReferenceId identify the business document
// responsible for creating this stock transaction.
// ============================================================

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class StockTransaction : BaseEntity
    {
        // =============================================
        // PRIMARY KEY
        // =============================================

        public int Id { get; set; }


        // =============================================
        // TRANSACTION INFORMATION
        // =============================================

        public string Code { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; }

        public StockTransactionType TransactionType { get; set; }


        // =============================================
        // SOURCE / REFERENCE
        // =============================================

        public StockReferenceType ReferenceType { get; set; }

        /// <summary>
        /// Id of the source business document.
        ///
        /// Examples:
        /// GoodsReceiptNote.Id
        /// ProductionJob.Id
        /// DeliveryChallan.Id
        /// Invoice.Id
        ///
        /// Null for transactions such as Opening Stock or
        /// manual transactions that do not have a source document.
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Human-readable source document number.
        ///
        /// Examples:
        /// GRN-0001
        /// PJ-0025
        /// DC-0010
        /// INV-0015
        ///
        /// Stored as a snapshot for easy ledger/report display.
        /// </summary>
        public string? ReferenceNumber { get; set; }


        // =============================================
        // REMARKS
        // =============================================

        public string? Remarks { get; set; }


        // =============================================
        // ITEMS
        // =============================================

        public ICollection<StockTransactionLine> Items { get; set; }
            = new List<StockTransactionLine>();
    }
}