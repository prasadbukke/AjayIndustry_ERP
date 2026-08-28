/*
============================================================
File: CustomerReceiptStatus.cs

Module:
Customer Receipt

Purpose:
Defines Customer Receipt workflow status.

Workflow:
Draft
    ↓
Finalized

Important:
- Draft Receipt can be edited or soft-deleted.
- Finalized Receipt affects Invoice outstanding balance.
- Finalized Receipt cannot be edited.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum CustomerReceiptStatus
    {
        Draft = 1,

        Finalized = 2
    }
}