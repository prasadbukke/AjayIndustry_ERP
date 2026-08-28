/*
============================================================
File: PurchaseInvoiceStatus.cs

Module:
Purchase Invoice

Purpose:
Defines Purchase Invoice workflow status.

Important:
- Draft Purchase Invoice can be edited and deleted.
- Finalized Purchase Invoice is treated as an accounting
  document and cannot be edited or deleted.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum PurchaseInvoiceStatus
    {
        Draft = 1,

        Finalized = 2
    }
}