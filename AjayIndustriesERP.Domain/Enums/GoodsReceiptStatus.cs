using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================
// File: GoodsReceiptStatus.cs
// Purpose:
// Defines the receipt status of each Purchase Order item
// while creating a Goods Receipt Note (GRN).
//
// Used For:
// - NotReceived      : Item has not been received in this GRN.
// - PartialReceived  : Only part of the remaining PO quantity
//                      has been received.
// - FullReceived     : Complete remaining PO quantity has been
//                      received.
//
// This status controls GRN item UI behavior such as showing or
// hiding Received Now and Pending Quantity fields.
// ============================================================

namespace AjayIndustriesERP.Domain.Enums
{
    public enum GoodsReceiptStatus
    {
        NotReceived = 1,
        PartialReceived = 2,
        FullReceived = 3
    }
}