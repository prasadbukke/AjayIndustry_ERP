// ============================================================
// File: StockReferenceType.cs
// Purpose:
// Defines the source/reference document responsible for an
// Inventory stock transaction.
//
// Examples:
// - Opening Stock entry
// - Goods Receipt Note
// - Production Job
// - Delivery Challan
// - Direct Invoice
// - Manual Stock Adjustment
// - Warehouse Transfer
// - Sales Return
//
// Important:
// StockTransactionType tells WHAT stock movement happened.
// StockReferenceType tells FROM WHICH business process/document
// that stock movement originated.
// ============================================================

namespace AjayIndustriesERP.Domain.Enums
{
    public enum StockReferenceType
    {
        OpeningStock = 1,

        GoodsReceiptNote = 2,

        ProductionJob = 3,

        DeliveryChallan = 4,

        Invoice = 5,

        ManualAdjustment = 6,

        WarehouseTransfer = 7,

        SalesReturn = 8
    }
}