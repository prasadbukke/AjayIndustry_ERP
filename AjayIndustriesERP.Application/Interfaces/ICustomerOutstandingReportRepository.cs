/*
============================================================
File: ICustomerOutstandingReportRepository.cs

Module:
Customer Outstanding / Receivables Report

Purpose:
Defines read-only persistence operations required by
Customer Outstanding Report.

Responsibilities:
- Load Customers for report filter.
- Load Finalized Invoice receivable data.
- Aggregate Finalized Customer Receipt allocations.
- Support report filters.
- Support pagination.
- Return report summary totals.

Important:
- This is a read-only reporting repository.
- No new database entity or table is required.
- Outstanding is derived from Finalized Invoices and
  Finalized Customer Receipt Allocations.
- Only active, non-deleted Finalized Receipts affect
  received / outstanding calculations.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerOutstandingReportRepository
    {
        #region Customers

        Task<List<Customer>>
            GetCustomersForFilterAsync();

        #endregion


        #region Report

        Task<CustomerOutstandingReportData>
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


    /*
     * Internal application-level report DTO.
     *
     * This is NOT a database entity.
     */
    public class CustomerOutstandingReportData
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


        #region Pagination

        public int TotalRecords
        {
            get;
            set;
        }


        public List<CustomerOutstandingReportItem>
            Items
        {
            get;
            set;
        } = new();

        #endregion
    }


    public class CustomerOutstandingReportItem
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
    }
}