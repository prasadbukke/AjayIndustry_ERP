/*
=============================================================
File: ItemCustomerPOTrackingExcelExporter.cs
Module: Item Customer PO Tracking
Layer: Infrastructure - Export

Purpose:
Creates formatted Excel XLSX file for
Item Customer PO Tracking.

Architecture:

Controller
    ↓
Tracking Service
    ↓
Filtered Tracking Rows
    ↓
IItemCustomerPOTrackingExcelExporter
    ↓
ItemCustomerPOTrackingExcelExporter
    ↓
ClosedXML
    ↓
XLSX

Export Columns:
- Customer PO No.
- Customer
- PO Date
- Item Code
- Item Name
- Drawing No.
- Ordered Qty
- Due Date
- Completion Date
- Priority
- PO Status
- Production Jobs
- Production Status

Important:
- ClosedXML dependency stays in Infrastructure.
- Ordered Quantity is displayed without decimal points.
- Completion Date remains blank if Production is incomplete.
- Export is read-only.
- No Entity.
- No Migration.
=============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using ClosedXML.Excel;

namespace AjayIndustriesERP.Infrastructure.Exports
{
    public class ItemCustomerPOTrackingExcelExporter
        : IItemCustomerPOTrackingExcelExporter
    {
        #region Constants

        private const int TitleRow = 1;

        private const int InfoRow = 2;

        private const int HeaderRow = 4;

        private const int DataStartRow = 5;

        private const int TotalColumns = 13;

        #endregion


        #region Export

        public byte[] Export(
            IReadOnlyCollection<
                ItemCustomerPOTrackingResultRow> rows)
        {
            using var workbook =
                new XLWorkbook();


            var worksheet =
                workbook.Worksheets.Add(
                    "Customer PO Tracking");


            #region Worksheet Settings

            worksheet.SheetView.FreezeRows(
                HeaderRow);


            worksheet.Style.Font.FontName =
                "Calibri";


            worksheet.Style.Font.FontSize =
                11;

            #endregion


            #region Title

            var titleRange =
                worksheet.Range(
                    TitleRow,
                    1,
                    TitleRow,
                    TotalColumns);


            titleRange.Merge();


            titleRange.Value =
    "AJAY INDUSTRIES";


            titleRange.Style.Font.Bold =
                true;


            titleRange.Style.Font.FontSize =
                16;


            titleRange.Style.Alignment.Horizontal =
     XLAlignmentHorizontalValues.Center;


            titleRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            titleRange.Style.Fill.BackgroundColor =
                XLColor.FromHtml(
                    "#EAF2FF");


            titleRange.Style.Font.FontColor =
                XLColor.FromHtml(
                    "#1F4E78");


            worksheet.Row(
                TitleRow)
                .Height =
                    28;

            #endregion


            #region Export Information

            var infoRange =
                worksheet.Range(
                    InfoRow,
                    1,
                    InfoRow,
                    TotalColumns);


            infoRange.Merge();


            infoRange.Value =
    $"Item Customer PO Tracking | " +
    $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm} | " +
    $"Total Records: {rows.Count}";


            infoRange.Style.Font.FontSize =
                9;


            infoRange.Style.Font.FontColor =
                XLColor.Gray;


            infoRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Left;

            #endregion


            #region Headers

            var headers =
                new[]
                {
                    "Customer PO No.",
                    "Customer",
                    "PO Date",
                    "Item Code",
                    "Item Name",
                    "Drawing No.",
                    "Ordered Qty",
                    "Due Date",
                    "Completion Date",
                    "Priority",
                    "PO Status",
                    "Production Jobs",
                    "Production Status"
                };


            for (var column = 1;
                 column <= headers.Length;
                 column++)
            {
                var cell =
                    worksheet.Cell(
                        HeaderRow,
                        column);


                cell.Value =
                    headers[
                        column - 1];


                cell.Style.Font.Bold =
                    true;


                cell.Style.Font.FontColor =
                    XLColor.White;


                cell.Style.Fill.BackgroundColor =
                    XLColor.FromHtml(
                        "#2F5597");


                cell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


                cell.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;


                cell.Style.Border.TopBorder =
                    XLBorderStyleValues.Thin;


                cell.Style.Border.BottomBorder =
                    XLBorderStyleValues.Thin;


                cell.Style.Border.LeftBorder =
                    XLBorderStyleValues.Thin;


                cell.Style.Border.RightBorder =
                    XLBorderStyleValues.Thin;
            }


            worksheet.Row(
                HeaderRow)
                .Height =
                    24;

            #endregion


            #region Data Rows

            var currentRow =
                DataStartRow;


            foreach (var row in rows)
            {
                #region Customer PO Number

                worksheet
                    .Cell(
                        currentRow,
                        1)
                    .Value =
                        row.PurchaseOrderNumber;

                #endregion


                #region Customer

                worksheet
                    .Cell(
                        currentRow,
                        2)
                    .Value =
                        row.CustomerName;

                #endregion


                #region PO Date

                var poDateCell =
                    worksheet.Cell(
                        currentRow,
                        3);


                poDateCell.Value =
                    row.PurchaseOrderDate;


                poDateCell.Style.DateFormat.Format =
                    "dd-MM-yyyy";

                #endregion


                #region Item Code

                worksheet
                    .Cell(
                        currentRow,
                        4)
                    .Value =
                        row.ItemCode;

                #endregion


                #region Item Name

                worksheet
                    .Cell(
                        currentRow,
                        5)
                    .Value =
                        row.ItemName;

                #endregion


                #region Drawing Number

                worksheet
                    .Cell(
                        currentRow,
                        6)
                    .Value =
                        row.DrawingNumber
                        ?? string.Empty;

                #endregion


                #region Ordered Quantity

                var quantityCell =
                    worksheet.Cell(
                        currentRow,
                        7);


                quantityCell.Value =
                    row.OrderedQuantity;


                /*
                 * Tracking quantities are whole numbers
                 * for display.
                 */
                quantityCell.Style.NumberFormat.Format =
                    "#,##0";


                quantityCell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Right;

                #endregion


                #region Due Date

                var dueDateCell =
                    worksheet.Cell(
                        currentRow,
                        8);


                dueDateCell.Value =
                    row.DeliveryDate;


                dueDateCell.Style.DateFormat.Format =
                    "dd-MM-yyyy";

                #endregion


                #region Completion Date

                var completionDateCell =
                    worksheet.Cell(
                        currentRow,
                        9);


                if (row.CompletionDate.HasValue)
                {
                    completionDateCell.Value =
                        row.CompletionDate.Value;


                    completionDateCell.Style.DateFormat.Format =
                        "dd-MM-yyyy";
                }
                else
                {
                    completionDateCell.Value =
                        string.Empty;
                }

                #endregion


                #region Priority

                worksheet
                    .Cell(
                        currentRow,
                        10)
                    .Value =
                        row.Priority;

                #endregion


                #region PO Status

                worksheet
                    .Cell(
                        currentRow,
                        11)
                    .Value =
                        row.PurchaseOrderStatus;

                #endregion


                #region Production Jobs

                var productionJobsText =
                    row.TotalProductionJobs > 0
                        ? $"{row.CompletedProductionJobs} / " +
                          $"{row.TotalProductionJobs} Completed"
                        : "No Jobs";


                worksheet
                    .Cell(
                        currentRow,
                        12)
                    .Value =
                        productionJobsText;

                #endregion


                #region Production Status

                worksheet
                    .Cell(
                        currentRow,
                        13)
                    .Value =
                        row.ProductionPOStatus;

                #endregion


                #region Row Styling

                var dataRange =
                    worksheet.Range(
                        currentRow,
                        1,
                        currentRow,
                        TotalColumns);


                dataRange.Style.Border.BottomBorder =
                    XLBorderStyleValues.Hair;


                dataRange.Style.Border.BottomBorderColor =
                    XLColor.FromHtml(
                        "#D9E1F2");


                dataRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;


                if (currentRow % 2 == 0)
                {
                    dataRange.Style.Fill.BackgroundColor =
                        XLColor.FromHtml(
                            "#F8FAFC");
                }

                #endregion


                currentRow++;
            }

            #endregion


            #region Auto Filter

            var lastDataRow =
                rows.Count > 0
                    ? currentRow - 1
                    : HeaderRow;


            var filterRange =
                worksheet.Range(
                    HeaderRow,
                    1,
                    lastDataRow,
                    TotalColumns);


            filterRange.SetAutoFilter();

            #endregion


            #region Alignment

            worksheet
                .Column(3)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet
                .Column(8)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet
                .Column(9)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet
                .Column(10)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet
                .Column(11)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet
                .Column(12)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet
                .Column(13)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

            #endregion


            #region Column Widths

            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 28;
            worksheet.Column(3).Width = 14;
            worksheet.Column(4).Width = 16;
            worksheet.Column(5).Width = 30;
            worksheet.Column(6).Width = 18;
            worksheet.Column(7).Width = 14;
            worksheet.Column(8).Width = 14;
            worksheet.Column(9).Width = 17;
            worksheet.Column(10).Width = 13;
            worksheet.Column(11).Width = 15;
            worksheet.Column(12).Width = 22;
            worksheet.Column(13).Width = 20;

            #endregion


            #region Wrap Text

            worksheet
                .Column(2)
                .Style.Alignment.WrapText =
                    true;


            worksheet
                .Column(5)
                .Style.Alignment.WrapText =
                    true;


            worksheet
                .Column(12)
                .Style.Alignment.WrapText =
                    true;

            #endregion


            #region Save Workbook

            using var stream =
                new MemoryStream();


            workbook.SaveAs(
                stream);


            return stream.ToArray();

            #endregion
        }

        #endregion
    }
}