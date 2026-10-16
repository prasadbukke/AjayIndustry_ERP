// ============================================================
// File: StockTransactionType.cs
// Purpose:
// Defines the type of stock movement recorded in Inventory.
//
// Used For:
// - OpeningStock       : Initial stock entered into the ERP.
// - PurchaseReceipt    : Material received through GRN.
// - ProductionIssue    : Raw material issued to Production.
// - ProductionReturn   : Unused material returned from Production.
// - ProductionReceipt  : Finished Goods received after Production.
// - Transfer            : Stock moved between Warehouses.
// - AdjustmentIn        : Manual positive stock adjustment.
// - AdjustmentOut       : Manual negative stock adjustment.
// - Dispatch            : Finished Goods issued through dispatch/challan.
// - SalesReturn         : Material returned by Customer.
//
// Important:
// This enum only identifies the business type of stock movement.
// Actual stock quantity movement is stored in StockTransactionLine.
// ============================================================

namespace AjayIndustriesERP.Domain.Enums
{
    public enum StockTransactionType
    {
        OpeningStock = 1,
        PurchaseReceipt = 2,
        ProductionIssue = 3,
        ProductionReturn = 4,
        ProductionReceipt = 5,
        Transfer = 6,
        AdjustmentIn = 7,
        AdjustmentOut = 8,
        Dispatch = 9,
        SalesReturn = 10
    }
}