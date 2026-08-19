using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================
// File: GoodsReceiptMaterialStatus.cs
// Purpose:
// Defines the material condition/status selected for an item
// that has physically been received through a GRN.
//
// Used For:
// - Approved
// - Rejected
// - Failure
// - Return
//
// Phase 1:
// This value is only stored with the GRN item.
//
// Stock, rejection, failure and supplier-return business effects
// will be implemented separately after the basic GRN workflow
// is completed and finalized.
// ============================================================

namespace AjayIndustriesERP.Domain.Enums
{
    public enum GoodsReceiptMaterialStatus
    {
        Approved = 1,
        Rejected = 2,
        Failure = 3,
        Return = 4
    }
}