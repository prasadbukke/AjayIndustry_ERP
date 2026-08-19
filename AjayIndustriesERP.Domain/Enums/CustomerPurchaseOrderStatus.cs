/*
============================================================
File: CustomerPurchaseOrderStatus.cs

Purpose:
Defines the transaction lifecycle status of a Customer
Purchase Order.

Responsibilities:
- Represent Customer PO transaction status.
- Keep Customer PO lifecycle separate from future
  Production Pipeline status.

Workflow:
Draft
  ↓
Confirmed
  ↓
Closed

Cancelled:
Used when the Customer Purchase Order is cancelled.

Important:
This enum represents Customer PO transaction status only.

Production statuses such as:
Pending, Running, Completed, Failed and Rejected
will belong to the future Production Pipeline.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum CustomerPurchaseOrderStatus
    {
        #region Status Values

        Draft = 1,

        Confirmed = 2,

        Closed = 3,

        Cancelled = 4

        #endregion
    }
}