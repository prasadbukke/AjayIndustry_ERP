/*
============================================================
File: CustomerReceiptAllocation.cs

Module:
Customer Receipt

Purpose:
Represents allocation of a Customer Receipt amount
against one Finalized Invoice.

Responsibilities:
- Link Customer Receipt with Invoice.
- Store Invoice historical snapshot information.
- Store amount already received before this Receipt.
- Store amount allocated through this Receipt.
- Store remaining Invoice balance after allocation.
- Maintain line sequence.

Important:
- One Customer Receipt may contain multiple allocations.
- One Invoice may receive multiple Receipt allocations
  over time.
- Only Finalized Customer Receipts affect Invoice
  outstanding balance.
- AlreadyReceivedAmount and BalanceAfterReceipt are
  historical snapshots.
- Service layer must recalculate live outstanding
  before Create / Update / Finalize.
- Common audit / soft-delete fields come from BaseEntity.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class CustomerReceiptAllocation
        : BaseEntity
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


        #region Customer Receipt

        public int CustomerReceiptId
        {
            get;
            set;
        }


        public CustomerReceipt? CustomerReceipt
        {
            get;
            set;
        }

        #endregion


        #region Invoice

        public int InvoiceId
        {
            get;
            set;
        }


        public string InvoiceCode
        {
            get;
            set;
        } = string.Empty;


        public DateTime InvoiceDate
        {
            get;
            set;
        }


        public decimal InvoiceGrandTotal
        {
            get;
            set;
        }


        public Invoice? Invoice
        {
            get;
            set;
        }

        #endregion


        #region Allocation Amounts

        /*
         * Amount already received against this Invoice
         * through previous Finalized Customer Receipts.
         *
         * Historical snapshot only.
         */
        public decimal AlreadyReceivedAmount
        {
            get;
            set;
        }


        /*
         * Amount from the current Customer Receipt
         * allocated against this Invoice.
         */
        public decimal AllocatedAmount
        {
            get;
            set;
        }


        /*
         * Remaining Invoice balance immediately
         * after applying this Receipt allocation.
         *
         * Historical snapshot only.
         */
        public decimal BalanceAfterReceipt
        {
            get;
            set;
        }

        #endregion
    }
}