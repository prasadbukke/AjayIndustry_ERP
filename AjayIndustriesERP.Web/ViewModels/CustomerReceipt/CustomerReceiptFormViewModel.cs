/*
============================================================
File: CustomerReceiptFormViewModel.cs

Module:
Customer Receipt

Purpose:
Create / Edit ViewModel for Customer Receipt.

Responsibilities:
- Capture Receipt header information.
- Select Customer.
- Capture Payment Mode information.
- Display trusted Invoice financial snapshots.
- Capture Invoice allocation amounts.
- Support both Create and Edit screens.

Important:
- InvoiceGrandTotal, AlreadyReceivedAmount,
  OutstandingAmount and BalanceAfterReceipt are
  display values only.
- Service layer recalculates all trusted financial values.
- AllocatedAmount is the user-entered allocation amount.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.CustomerReceipt
{
    public class CustomerReceiptFormViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        [Display(
            Name = "Receipt No.")]
        public string? Code
        {
            get;
            set;
        }

        #endregion


        #region Receipt Date

        [Required(
            ErrorMessage =
                "Receipt Date is required.")]
        [Display(
            Name = "Receipt Date")]
        [DataType(
            DataType.Date)]
        public DateTime ReceiptDate
        {
            get;
            set;
        } = DateTime.Today;

        #endregion


        #region Customer

        [Required(
            ErrorMessage =
                "Customer is required.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Customer is required.")]
        [Display(
            Name = "Customer")]
        public int CustomerId
        {
            get;
            set;
        }


        [Display(
            Name = "Customer Code")]
        public string? CustomerCode
        {
            get;
            set;
        }


        [Display(
            Name = "Customer Name")]
        public string? CustomerName
        {
            get;
            set;
        }


        public List<SelectListItem>
            AvailableCustomers
        {
            get;
            set;
        } = new();

        #endregion


        #region Payment Information

        [Required(
            ErrorMessage =
                "Payment Mode is required.")]
        [Display(
            Name = "Payment Mode")]
        public PaymentMode PaymentMode
        {
            get;
            set;
        } = PaymentMode.BankTransfer;


        [Display(
            Name = "Transaction / Reference No.")]
        [StringLength(
            100,
            ErrorMessage =
                "Transaction / Reference Number cannot exceed 100 characters.")]
        public string? ReferenceNumber
        {
            get;
            set;
        }


        [Display(
            Name = "Cheque No.")]
        [StringLength(
            50,
            ErrorMessage =
                "Cheque Number cannot exceed 50 characters.")]
        public string? ChequeNumber
        {
            get;
            set;
        }


        [Display(
            Name = "Cheque Date")]
        [DataType(
            DataType.Date)]
        public DateTime? ChequeDate
        {
            get;
            set;
        }


        [Display(
            Name = "Bank Name")]
        [StringLength(
            200,
            ErrorMessage =
                "Bank Name cannot exceed 200 characters.")]
        public string? BankName
        {
            get;
            set;
        }

        #endregion


        #region Receipt Amount

        [Required(
            ErrorMessage =
                "Total Received Amount is required.")]
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage =
                "Total Received Amount must be greater than zero.")]
        [Display(
            Name = "Total Received Amount")]
        public decimal TotalReceivedAmount
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        [Display(
            Name = "Remarks")]
        [StringLength(
            1000,
            ErrorMessage =
                "Remarks cannot exceed 1000 characters.")]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Invoice Allocations

        public List<CustomerReceiptAllocationFormViewModel>
            Allocations
        {
            get;
            set;
        } = new();

        #endregion
    }


    public class CustomerReceiptAllocationFormViewModel
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

        [Required(
            ErrorMessage =
                "Invoice is required.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Invoice is required.")]
        public int InvoiceId
        {
            get;
            set;
        }


        [Display(
            Name = "Invoice No.")]
        public string? InvoiceCode
        {
            get;
            set;
        }


        [Display(
            Name = "Invoice Date")]
        public DateTime InvoiceDate
        {
            get;
            set;
        }

        #endregion


        #region Invoice Financial Information

        /*
         * Trusted display snapshot.
         * Service recalculates before saving.
         */
        [Display(
            Name = "Invoice Amount")]
        public decimal InvoiceGrandTotal
        {
            get;
            set;
        }


        /*
         * Total amount already received through
         * Finalized Customer Receipts before
         * the current allocation.
         */
        [Display(
            Name = "Already Received")]
        public decimal AlreadyReceivedAmount
        {
            get;
            set;
        }


        /*
         * Current Invoice balance available
         * for allocation before this Receipt.
         */
        [Display(
            Name = "Outstanding")]
        public decimal OutstandingAmount
        {
            get;
            set;
        }

        #endregion


        #region Allocation

        [Required(
            ErrorMessage =
                "Allocated Amount is required.")]
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage =
                "Allocated Amount must be greater than zero.")]
        [Display(
            Name = "Allocated Amount")]
        public decimal AllocatedAmount
        {
            get;
            set;
        }


        [Display(
            Name = "Balance After Receipt")]
        public decimal BalanceAfterReceipt
        {
            get;
            set;
        }

        #endregion
    }
}