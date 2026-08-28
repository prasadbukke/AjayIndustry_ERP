/*
============================================================
File: CustomerOutstandingReportViewModel.cs

Module:
Customer Outstanding / Receivables Report

Purpose:
ViewModel for Customer Outstanding Report screen.

Responsibilities:
- Hold report filters.
- Hold Customer filter options.
- Hold Payment Status filter options.
- Hold report summary totals.
- Hold Invoice-wise receivable rows.
- Hold pagination information.

Important:
- This is a read-only report.
- No new database entity or table is required.
- Invoice Outstanding is derived from:
      Finalized Invoice GrandTotal
      -
      Finalized Customer Receipt Allocations.
- Summary totals represent the complete filtered result,
  not only the current page.
============================================================
*/

using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.ViewModels.CustomerOutstandingReport
{
    public class CustomerOutstandingReportViewModel
    {
        #region Filters

        public int? CustomerId
        {
            get;
            set;
        }


        public DateTime? FromDate
        {
            get;
            set;
        }


        public DateTime? ToDate
        {
            get;
            set;
        }


        /*
         * Supported values:
         *
         * Outstanding
         * All
         * Unpaid
         * PartiallyPaid
         * Paid
         *
         * Default:
         * Outstanding
         */
        public string PaymentStatus
        {
            get;
            set;
        } = "Outstanding";


        public string? SearchText
        {
            get;
            set;
        }

        #endregion


        #region Filter Options

        public List<SelectListItem>
            AvailableCustomers
        {
            get;
            set;
        } = new();


        public List<SelectListItem>
            AvailablePaymentStatuses
        {
            get;
            set;
        } = new();

        #endregion


        #region Summary

        /*
         * Total value of all Finalized Invoices
         * matching the selected filters.
         */
        public decimal TotalInvoiceAmount
        {
            get;
            set;
        }


        /*
         * Total amount received through Finalized
         * Customer Receipts against filtered Invoices.
         */
        public decimal TotalReceivedAmount
        {
            get;
            set;
        }


        /*
         * Total amount still receivable.
         */
        public decimal TotalOutstandingAmount
        {
            get;
            set;
        }


        /*
         * Number of filtered Invoices having
         * OutstandingAmount > 0.
         */
        public int OutstandingInvoiceCount
        {
            get;
            set;
        }

        #endregion


        #region Report Rows

        public List<CustomerOutstandingReportRowViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion


        #region Pagination

        public int PageNumber
        {
            get;
            set;
        } = 1;


        public int PageSize
        {
            get;
            set;
        } = 25;


        public int TotalRecords
        {
            get;
            set;
        }


        public int TotalPages
        {
            get;
            set;
        }


        public bool HasPrevious
        {
            get;
            set;
        }


        public bool HasNext
        {
            get;
            set;
        }

        #endregion
    }


    public class CustomerOutstandingReportRowViewModel
    {
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


        public DateTime? DueDate
        {
            get;
            set;
        }

        #endregion


        #region Financial Information

        public decimal InvoiceAmount
        {
            get;
            set;
        }


        public decimal ReceivedAmount
        {
            get;
            set;
        }


        public decimal OutstandingAmount
        {
            get;
            set;
        }

        #endregion


        #region Payment Status

        /*
         * Runtime derived values:
         *
         * Unpaid
         * Partially Paid
         * Paid
         */
        public string PaymentStatus
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Ageing

        /*
         * Number of days from Invoice Date
         * up to today.
         */
        public int AgeDays
        {
            get;
            set;
        }


        /*
         * Number of days past Due Date.
         *
         * 0 means:
         * - not yet due, or
         * - Due Date is not available.
         */
        public int OverdueDays
        {
            get;
            set;
        }


        public bool IsOverdue
        {
            get;
            set;
        }

        #endregion
    }
}