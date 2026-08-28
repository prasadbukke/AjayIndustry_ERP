/*
============================================================
File: ICustomerOutstandingReportService.cs

Module:
Customer Outstanding / Receivables Report

Purpose:
Defines business operations for Customer Outstanding Report.

Responsibilities:
- Load Customers for report filter.
- Load Customer Outstanding report.
- Apply report-level validation.
- Calculate runtime payment status.
- Calculate ageing and overdue information.
- Prepare pagination information.

Important:
- This is a read-only report service.
- No database entity or migration is required.
- Outstanding is derived from:
      Finalized Invoice GrandTotal
      -
      Finalized Customer Receipt Allocations.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerOutstandingReportService
    {
        #region Customers

        Task<List<Customer>>
            GetCustomersForFilterAsync();

        #endregion


        #region Report

        Task<CustomerOutstandingReportResult>
            GetReportAsync(
                int? customerId,
                DateTime? fromDate,
                DateTime? toDate,
                string? paymentStatus,
                string? searchText,
                int pageNumber,
                int pageSize);

        #endregion
    }


    public class CustomerOutstandingReportResult
    {
        #region Summary

        public decimal TotalInvoiceAmount
        {
            get;
            set;
        }


        public decimal TotalReceivedAmount
        {
            get;
            set;
        }


        public decimal TotalOutstandingAmount
        {
            get;
            set;
        }


        public int OutstandingInvoiceCount
        {
            get;
            set;
        }

        #endregion


        #region Rows

        public List<CustomerOutstandingReportResultItem>
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
        }


        public int PageSize
        {
            get;
            set;
        }


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


    public class CustomerOutstandingReportResultItem
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


        #region Amounts

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

        public string PaymentStatus
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Ageing

        public int AgeDays
        {
            get;
            set;
        }


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