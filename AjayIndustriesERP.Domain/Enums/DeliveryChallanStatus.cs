/*
============================================================
File: DeliveryChallanStatus.cs

Purpose:
Defines lifecycle status of a Delivery Challan.

Important:
- Draft Challan can be edited or deleted.
- Finalized Challan is locked as an audit document.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum DeliveryChallanStatus
    {
        #region Status

        Draft = 1,

        Finalized = 2

        #endregion
    }
}