/*
=============================================================
File: IItemCustomerPOTrackingExcelExporter.cs
Module: Item Customer PO Tracking
Layer: Application - Interface

Purpose:
Defines Excel export contract for Item Customer PO Tracking.

Architecture:

Controller
    ↓
Tracking Service
    ↓
Filtered Tracking Rows
    ↓
IItemCustomerPOTrackingExcelExporter
    ↓
Infrastructure Excel Exporter
    ↓
ClosedXML
    ↓
XLSX byte[]

Important:
- Application layer does NOT depend on ClosedXML.
- ClosedXML implementation remains in Infrastructure.
- Export receives already-filtered tracking rows.
- No DbContext access.
- No Entity changes.
- No Migration.
=============================================================
*/

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IItemCustomerPOTrackingExcelExporter
    {
        /// <summary>
        /// Creates an XLSX file containing all supplied
        /// Item Customer PO Tracking rows.
        ///
        /// Returned byte array represents the complete
        /// Excel workbook.
        /// </summary>
        byte[] Export(
            IReadOnlyCollection<
                ItemCustomerPOTrackingResultRow> rows);
    }
}