/*
============================================================
File: CustomerPurchaseOrderPriority.cs

Purpose:
Defines business priority levels for Customer Purchase Orders
and Customer Purchase Order Items.

Responsibilities:
- Represent Customer PO execution priority.
- Support priority-based display and future production planning.
- Allow an individual PO Item to override the PO header priority.

Priority Meaning:
Normal   = Standard production priority.
High     = Higher than normal priority.
Urgent   = Requires immediate production attention.
Critical = Highest operational priority.

Important:
Priority does not automatically schedule machines in Phase 1.
It will later be used by Production Planning and Job Scheduling.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum CustomerPurchaseOrderPriority
    {
        #region Priority Values

        Normal = 1,

        High = 2,

        Urgent = 3,

        Critical = 4

        #endregion
    }
}