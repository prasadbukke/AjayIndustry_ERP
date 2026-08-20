/*
============================================================
File: ItemProcessRoutingStatus.cs

Purpose:
Defines the lifecycle status of an Item Process Routing.

Responsibilities:
- Identify whether a Routing is still being prepared.
- Identify whether a Routing is approved for Production use.
- Preserve older Routing revisions when replaced by a newer
  released revision.

Workflow:
Draft -> Released -> Superseded

Status Meaning:
Draft
- Routing is being prepared.
- Routing is editable.
- Cannot be used for new Production Jobs.

Released
- Current approved Routing revision.
- Can be used to create new Production Jobs.

Superseded
- Older approved Routing revision.
- Replaced by a newer Released revision.
- Cannot be used for new Production Jobs.
- Preserved for historical Production Job traceability.

Important:
- Only Released Routing can be used for new Production Jobs.
- Superseded does NOT mean Deleted.
- Deleted routing is controlled separately through IsDeleted.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum ItemProcessRoutingStatus
    {
        #region Routing Status

        Draft = 1,

        Released = 2,

        Superseded = 3

        #endregion
    }
}