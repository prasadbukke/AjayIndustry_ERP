/*
=============================================================
File: SupplierPaymentDueAlertViewModel.cs
Module: Home Dashboard
Layer: Web - ViewModel

Purpose:
Provides data required by the Home Dashboard
Supplier Payment Due popup.

Important:
- No Entity.
- No Table.
- No Migration.
- Read-only UI model.
- Data comes from Supplier Outstanding service.

Popup Rule:
- Finalized Purchase Invoice
- Outstanding > 0
- Due Date <= Today + 5 days
- Includes Overdue + Due Soon
- Fully paid invoices do not appear.
=============================================================
*/

namespace AjayIndustriesERP.Web.ViewModels.Home
{
    #region Popup ViewModel

    public class SupplierPaymentDueAlertViewModel
    {
        /// <summary>
        /// Determines whether popup should be shown.
        /// </summary>
        public bool HasAlerts =>
            Alerts.Count > 0;


        /// <summary>
        /// Total number of invoices requiring attention.
        /// </summary>
        public int TotalAlerts =>
            Alerts.Count;


        /// <summary>
        /// Total outstanding amount of all popup invoices.
        /// </summary>
        public decimal TotalOutstanding =>
            Alerts.Sum(x =>
                x.OutstandingAmount);


        /// <summary>
        /// Supplier invoice alerts.
        /// </summary>
        public List<SupplierPaymentDueAlertRowViewModel>
            Alerts
        { get; set; }
                = new();
    }

    #endregion


    #region Popup Row ViewModel

    public class SupplierPaymentDueAlertRowViewModel
    {
        #region Purchase Invoice

        public int PurchaseInvoiceId { get; set; }

        public string PurchaseInvoiceCode { get; set; }
            = string.Empty;

        public string? SupplierInvoiceNumber { get; set; }

        public DateTime PurchaseInvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        #endregion


        #region Supplier

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;

        #endregion


        #region Payment Position

        public decimal InvoiceTotal { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal OutstandingAmount { get; set; }

        public string PaymentStatus { get; set; }
            = string.Empty;

        #endregion


        #region Due Position

        /// <summary>
        /// Overdue / Due Soon.
        /// </summary>
        public string DueStatus { get; set; }
            = string.Empty;


        /// <summary>
        /// Number of overdue days.
        /// Zero when invoice is not overdue.
        /// </summary>
        public int OverdueDays { get; set; }


        /// <summary>
        /// Number of days remaining until payment due date.
        ///
        /// 0 = Due Today
        /// 1 = Due Tomorrow
        /// 2-5 = Due within next days
        /// </summary>
        public int DaysUntilDue { get; set; }

        #endregion
    }

    #endregion
}