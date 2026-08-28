/*
============================================================
File: CustomerReceiptDetailsViewModel.cs

Module:
Customer Receipt

Purpose:
Display ViewModel for Customer Receipt Details page.

Responsibilities:
- Display Receipt identification and workflow status.
- Display Customer information.
- Display saved Company information.
- Display payment / transaction information.
- Display Invoice allocation history.
- Display total received amount.
- Display Finalization information.

Important:
- Details page represents the saved historical Receipt.
- Allocation financial values are saved snapshots.
- Finalized Receipt cannot be edited or deleted.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.CustomerReceipt
{
    public class CustomerReceiptDetailsViewModel
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

        #endregion


        #region Company

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

        #endregion


        #region Payment Information

        public PaymentMode PaymentMode
        {
            get;
            set;
        }


        public string? ReferenceNumber
        {
            get;
            set;
        }


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
        }


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


        #region Invoice Allocations

        public List<CustomerReceiptAllocationDetailsViewModel>
            Allocations
        {
            get;
            set;
        } = new();

        #endregion
    }


    public class CustomerReceiptAllocationDetailsViewModel
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

        #endregion


        #region Financial Snapshot

        public decimal InvoiceGrandTotal
        {
            get;
            set;
        }


        public decimal AlreadyReceivedAmount
        {
            get;
            set;
        }


        /*
         * Invoice outstanding immediately before
         * applying this Receipt allocation.
         */
        public decimal OutstandingBeforeReceipt
        {
            get;
            set;
        }


        public decimal AllocatedAmount
        {
            get;
            set;
        }


        public decimal BalanceAfterReceipt
        {
            get;
            set;
        }

        #endregion
    }
}