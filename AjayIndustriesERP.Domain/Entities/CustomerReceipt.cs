/*
============================================================
File: CustomerReceipt.cs

Module:
Customer Receipt

Purpose:
Represents one Customer Payment Receipt.

Responsibilities:
- Store Receipt identification and date.
- Store Customer reference and historical snapshot.
- Store Company historical snapshot for Receipt PDF.
- Store payment mode and transaction details.
- Store total received amount.
- Maintain Invoice allocations.
- Maintain Draft / Finalized workflow.

Important:
- One Customer Receipt may be allocated against
  one or multiple Finalized Invoices.
- Customer and Company snapshots preserve historical data.
- TotalReceivedAmount must match the total active
  Invoice allocations when the Receipt is Finalized.
- Finalized Receipt allocations affect Invoice outstanding.
- Invoice outstanding is derived from Finalized Receipts;
  it is not stored directly on Invoice.
- Common audit / soft-delete fields come from BaseEntity.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class CustomerReceipt
        : BaseEntity
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public string Code
        {
            get;
            set;
        } = string.Empty;


        public DateTime ReceiptDate
        {
            get;
            set;
        }

        #endregion


        #region Customer

        public int CustomerId
        {
            get;
            set;
        }


        public string? CustomerCode
        {
            get;
            set;
        }


        public string CustomerName
        {
            get;
            set;
        } = string.Empty;


        public string? CustomerSnapshotJson
        {
            get;
            set;
        }


        public Customer? Customer
        {
            get;
            set;
        }

        #endregion


        #region Company Snapshot

        public int? CompanyId
        {
            get;
            set;
        }


        public string? CompanyName
        {
            get;
            set;
        }


        public string? CompanySnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Payment Information

        public PaymentMode PaymentMode
        {
            get;
            set;
        }


        /*
         * Common transaction reference.
         *
         * Examples:
         * - UTR
         * - UPI Transaction ID
         * - Bank Transaction Reference
         * - IMPS Reference
         */
        public string? ReferenceNumber
        {
            get;
            set;
        }


        /*
         * Used only for Cheque payment.
         */
        public string? ChequeNumber
        {
            get;
            set;
        }


        public DateTime? ChequeDate
        {
            get;
            set;
        }


        /*
         * Customer / remitting bank name
         * where applicable.
         */
        public string? BankName
        {
            get;
            set;
        }

        #endregion


        #region Amount

        public decimal TotalReceivedAmount
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

        #endregion


        #region Workflow

        public CustomerReceiptStatus Status
        {
            get;
            set;
        } = CustomerReceiptStatus.Draft;


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


        #region Allocations

        public ICollection<CustomerReceiptAllocation>
            Allocations
        {
            get;
            set;
        } = new List<CustomerReceiptAllocation>();

        #endregion
    }
}