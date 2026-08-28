/*
============================================================
File: InvoiceStatus.cs

Module:
Invoice

Purpose:
Defines Invoice workflow status.

Responsibilities:
- Identify editable Draft Invoice.
- Identify locked Finalized Invoice.

Important:
- Draft Invoice can be edited and deleted.
- Finalized Invoice is treated as an accounting document
  and must not be edited or deleted.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum InvoiceStatus
    {
        #region Status Values

        Draft = 1,

        Finalized = 2

        #endregion
    }
}