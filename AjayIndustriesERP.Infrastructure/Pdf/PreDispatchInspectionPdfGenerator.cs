/*
============================================================
File: PreDispatchInspectionPdfGenerator.cs

Purpose:
Generates the Final Inspection Report PDF for a
Finalized Pre-Dispatch Inspection.

Responsibilities:
- Generate A4 Landscape PDF.
- Render Ajay Industries report header.
- Render saved PDI snapshot information.
- Render Inspection Parameters.
- Render normal Observations.
- Render Interval Readings.
- Render Quantity Results.
- Render Inspection Remarks.
- Render Approval / Release section.
- Return generated PDF as byte[].

Important:
- This class contains presentation / PDF layout only.
- Business validation belongs in Application Service.
- PDF uses saved PDI snapshot values.
- Customer Drawing is preferred for customer-facing
  Drawing No. / Revision.
- Workshop Drawing is used as fallback.
============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AjayIndustriesERP.Infrastructure.Pdf
{
    public class PreDispatchInspectionPdfGenerator
        : IPreDispatchInspectionPdfGenerator
    {
        #region Constants

        private const float BorderWidth =
            0.7f;

        private const float DefaultFontSize =
            7f;

        private const float SmallFontSize =
            6f;

        private const float HeadingFontSize =
            11f;

        private const int DefaultObservationCount =
            7;

        private const int DefaultIntervalCount =
            3;

        #endregion


        #region Generate

        public byte[] Generate(
            PreDispatchInspection preDispatchInspection)
        {
            #region Validation

            if (preDispatchInspection == null)
            {
                throw new ArgumentNullException(
                    nameof(preDispatchInspection));
            }

            #endregion


            #region Generate Document

            return Document
                .Create(document =>
                {
                    document.Page(page =>
                    {
                        #region Page Setup

                        page.Size(
                            PageSizes.A4.Landscape());

                        page.Margin(
                            12);

                        page.PageColor(
                            Colors.White);

                        page.DefaultTextStyle(
                            style =>
                                style
                                    .FontSize(
                                        DefaultFontSize)
                                    .FontColor(
                                        Colors.Black));

                        #endregion


                        #region Content

                        page.Content()
                            .Column(column =>
                            {
                                column.Spacing(
                                    5);


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeReportHeader(
                                                container,
                                                preDispatchInspection));


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeReportInformation(
                                                container,
                                                preDispatchInspection));


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeInspectionTable(
                                                container,
                                                preDispatchInspection));


                                column.Item()
                                    .Element(
                                        ComposeInspectionNotes);


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeResultSection(
                                                container,
                                                preDispatchInspection));


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeRemarksSection(
                                                container,
                                                preDispatchInspection));


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeApprovalSection(
                                                container,
                                                preDispatchInspection));
                            });

                        #endregion


                        #region Footer

                        page.Footer()
                            .PaddingTop(
                                3)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text(
                                        "AJAY INDUSTRIES - FINAL INSPECTION REPORT")
                                    .FontSize(
                                        5.5f);


                                row.RelativeItem()
                                    .AlignRight()
                                    .Text(text =>
                                    {
                                        text
                                            .Span(
                                                "Page ")
                                            .FontSize(
                                                5.5f);


                                        text
                                            .CurrentPageNumber();


                                        text
                                            .Span(
                                                " of ")
                                            .FontSize(
                                                5.5f);


                                        text
                                            .TotalPages();
                                    });
                            });

                        #endregion
                    });
                })
                .GeneratePdf();

            #endregion
        }

        #endregion


        #region Report Header

        private static void ComposeReportHeader(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Table(table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.RelativeColumn(
                                2);

                            columns.RelativeColumn(
                                4);

                            columns.RelativeColumn(
                                2);
                        });

                    #endregion


                    #region Company

                    table.Cell()
                        .Element(
                            HeaderCell)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(
                            "AJAY INDUSTRIES")
                        .Bold()
                        .FontSize(
                            HeadingFontSize);

                    #endregion


                    #region Title

                    table.Cell()
                        .Element(
                            HeaderCell)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(
                            "FINAL INSPECTION REPORT")
                        .Bold()
                        .FontSize(
                            13);

                    #endregion


                    #region Document Number

                    table.Cell()
                        .Element(
                            HeaderCell)
                        .AlignMiddle()
                        .Column(column =>
                        {
                            column.Item()
                                .Text(
                                    "Doc No.")
                                .FontSize(
                                    SmallFontSize)
                                .Bold();


                            column.Item()
                                .Text(
                                    "-")
                                .FontSize(
                                    DefaultFontSize);


                            column.Item()
                                .PaddingTop(
                                    2)
                                .Text(
                                    $"Report No.: {Display(report.Code)}")
                                .Bold()
                                .FontSize(
                                    SmallFontSize);
                        });

                    #endregion
                });
        }

        #endregion


        #region Report Information

        private static void ComposeReportInformation(
            IContainer container,
            PreDispatchInspection report)
        {
            #region Drawing

            var drawingNumber =
                !string.IsNullOrWhiteSpace(
                    report.CustomerDrawingNumber)
                    ? report.CustomerDrawingNumber
                    : report.WorkshopDrawingNumber;


            var drawingRevision =
                !string.IsNullOrWhiteSpace(
                    report.CustomerDrawingRevision)
                    ? report.CustomerDrawingRevision
                    : report.WorkshopDrawingRevision;

            #endregion


            container
                .Table(table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.ConstantColumn(
                                75);

                            columns.RelativeColumn();

                            columns.ConstantColumn(
                                60);

                            columns.RelativeColumn();

                            columns.ConstantColumn(
                                70);

                            columns.RelativeColumn();
                        });

                    #endregion


                    #region Row 1

                    AddLabelCell(
                        table,
                        "Part / Product Name");

                    AddValueCell(
                        table,
                        report.ItemName);


                    AddLabelCell(
                        table,
                        "Part No.");

                    AddValueCell(
                        table,
                        report.PartNumber);


                    AddLabelCell(
                        table,
                        "Date");

                    AddValueCell(
                        table,
                        report.InspectionDate
                            .ToString(
                                "dd-MM-yyyy"));

                    #endregion


                    #region Row 2

                    AddLabelCell(
                        table,
                        "Drawing No.");

                    AddValueCell(
                        table,
                        drawingNumber);


                    AddLabelCell(
                        table,
                        "Rev No.");

                    AddValueCell(
                        table,
                        drawingRevision);


                    AddLabelCell(
                        table,
                        "Production Job");

                    AddValueCell(
                        table,
                        report.ProductionJobCode);

                    #endregion


                    #region Row 3

                    AddLabelCell(
                        table,
                        "Customer Name");

                    AddValueCell(
                        table,
                        report.CustomerName);


                    AddLabelCell(
                        table,
                        "Customer PO");

                    AddValueCell(
                        table,
                        report.CustomerPurchaseOrderNumber);


                    AddLabelCell(
                        table,
                        "Inspection Qty");

                    AddValueCell(
                        table,
                        FormatQuantity(
                            report.InspectionQuantity,
                            report.UnitName));

                    #endregion


                    #region Row 4

                    AddLabelCell(
                        table,
                        "ERP Item Code");

                    AddValueCell(
                        table,
                        report.ItemCode);


                    AddLabelCell(
                        table,
                        "Customer Item");

                    AddValueCell(
                        table,
                        report.CustomerItemCode);


                    AddLabelCell(
                        table,
                        "Report No.");

                    AddValueCell(
                        table,
                        report.Code);

                    #endregion


                    #region Row 5

                    AddLabelCell(
                        table,
                        "Invoice No.");

                    AddValueCell(
                        table,
                        report.InvoiceNumber);


                    AddLabelCell(
                        table,
                        "Invoice Date");

                    AddValueCell(
                        table,
                        report.InvoiceDate.HasValue
                            ? report.InvoiceDate.Value
                                .ToString(
                                    "dd-MM-yyyy")
                            : null);


                    AddLabelCell(
                        table,
                        "Invoice Qty");

                    AddValueCell(
                        table,
                        report.InvoiceQuantity.HasValue
                            ? FormatQuantity(
                                report.InvoiceQuantity.Value,
                                report.UnitName)
                            : null);

                    #endregion
                });
        }

        #endregion


        #region Inspection Table

        private static void ComposeInspectionTable(
            IContainer container,
            PreDispatchInspection report)
        {
            #region Lines

            var lines =
                report.Lines
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();

            #endregion


            #region Dynamic Column Count

            var observationCount =
                Math.Max(
                    DefaultObservationCount,
                    lines
                        .SelectMany(x =>
                            x.Observations)
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive &&
                            !x.IsIntervalReading)
                        .Select(x =>
                            x.SequenceNumber)
                        .DefaultIfEmpty(
                            0)
                        .Max());


            var intervalCount =
                Math.Max(
                    DefaultIntervalCount,
                    lines
                        .SelectMany(x =>
                            x.Observations)
                        .Where(x =>
                            !x.IsDeleted &&
                            x.IsActive &&
                            x.IsIntervalReading)
                        .Select(x =>
                            x.SequenceNumber)
                        .DefaultIfEmpty(
                            0)
                        .Max());

            #endregion


            container
                .Table(table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.ConstantColumn(
                                22);

                            columns.RelativeColumn(
                                2.0f);

                            columns.RelativeColumn(
                                2.2f);

                            columns.RelativeColumn(
                                1.7f);


                            for (
                                var i = 0;
                                i < observationCount;
                                i++)
                            {
                                columns.RelativeColumn(
                                    0.65f);
                            }


                            for (
                                var i = 0;
                                i < intervalCount;
                                i++)
                            {
                                columns.RelativeColumn(
                                    0.65f);
                            }


                            columns.RelativeColumn(
                                1.0f);

                            columns.RelativeColumn(
                                1.4f);
                        });

                    #endregion


                    #region Repeating Header

                    table.Header(header =>
                    {
                        ComposeInspectionTableHeader(
                            header,
                            observationCount,
                            intervalCount);
                    });

                    #endregion


                    #region Rows

                    if (lines.Count == 0)
                    {
                        var totalColumns =
                            6 +
                            observationCount +
                            intervalCount;


                        table.Cell()
                            .ColumnSpan(
                                (uint)totalColumns)
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                "No Inspection Parameters available.");

                        return;
                    }


                    foreach (var line in lines)
                    {
                        #region Prepare Readings

                        var observations =
                            line.Observations
                                .Where(x =>
                                    !x.IsDeleted &&
                                    x.IsActive &&
                                    !x.IsIntervalReading)
                                .ToDictionary(
                                    x =>
                                        x.SequenceNumber,
                                    x =>
                                        x.Value);


                        var intervals =
                            line.Observations
                                .Where(x =>
                                    !x.IsDeleted &&
                                    x.IsActive &&
                                    x.IsIntervalReading)
                                .ToDictionary(
                                    x =>
                                        x.SequenceNumber,
                                    x =>
                                        x.Value);

                        #endregion


                        #region Main Cells

                        AddBodyCell(
                            table,
                            line.SequenceNumber
                                .ToString(),
                            true);


                        AddBodyCell(
                            table,
                            line.Parameter);


                        AddBodyCell(
                            table,
                            line.Specification);


                        AddBodyCell(
                            table,
                            line.InspectionMethod);

                        #endregion


                        #region Observation Cells

                        for (
                            var sequence = 1;
                            sequence <= observationCount;
                            sequence++)
                        {
                            observations.TryGetValue(
                                sequence,
                                out var value);


                            AddBodyCell(
                                table,
                                value,
                                true);
                        }

                        #endregion


                        #region Interval Cells

                        for (
                            var sequence = 1;
                            sequence <= intervalCount;
                            sequence++)
                        {
                            intervals.TryGetValue(
                                sequence,
                                out var value);


                            AddBodyCell(
                                table,
                                value,
                                true);
                        }

                        #endregion


                        #region Result

                        AddBodyCell(
                            table,
                            GetLineResultText(
                                line.Result),
                            true);

                        #endregion


                        #region Remarks

                        AddBodyCell(
                            table,
                            line.Remarks);

                        #endregion
                    }

                    #endregion
                });
        }


        private static void ComposeInspectionTableHeader(
            TableCellDescriptor header,
            int observationCount,
            int intervalCount)
        {
            #region First Row

            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Sr No");


            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Parameters");


            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Specification");


            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Inspection Method");


            header.Cell()
                .ColumnSpan(
                    (uint)observationCount)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Observation");


            header.Cell()
                .ColumnSpan(
                    (uint)intervalCount)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Reading At Interval");


            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Result");


            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Remarks");

            #endregion


            #region Second Row - Observations

            for (
                var sequence = 1;
                sequence <= observationCount;
                sequence++)
            {
                header.Cell()
                    .Element(
                        TableHeaderCell)
                    .Text(
                        sequence.ToString());
            }

            #endregion


            #region Second Row - Interval

            for (
                var sequence = 1;
                sequence <= intervalCount;
                sequence++)
            {
                header.Cell()
                    .Element(
                        TableHeaderCell)
                    .Text(
                        sequence.ToString());
            }

            #endregion
        }

        #endregion


        #region Inspection Notes

        private static void ComposeInspectionNotes(
            IContainer container)
        {
            container
                .Border(
                    BorderWidth)
                .Padding(
                    4)
                .Column(column =>
                {
                    column.Item()
                        .Text(
                            "ALL DIMENSIONS ARE IN MM")
                        .Bold()
                        .FontSize(
                            SmallFontSize);


                    column.Item()
                        .Text(
                            "ALL SAMPLES ARE CHECKED RANDOMLY")
                        .Bold()
                        .FontSize(
                            SmallFontSize);
                });
        }

        #endregion


        #region Result Section

        private static void ComposeResultSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Table(table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                    #endregion


                    #region Labels

                    AddCenteredLabelCell(
                        table,
                        "Accepted Qty");

                    AddCenteredLabelCell(
                        table,
                        "Rework Qty");

                    AddCenteredLabelCell(
                        table,
                        "Reject Qty");

                    AddCenteredLabelCell(
                        table,
                        "Overall Result");

                    #endregion


                    #region Values

                    AddCenteredValueCell(
                        table,
                        FormatQuantity(
                            report.AcceptedQuantity,
                            report.UnitName));


                    AddCenteredValueCell(
                        table,
                        FormatQuantity(
                            report.ReworkQuantity,
                            report.UnitName));


                    AddCenteredValueCell(
                        table,
                        FormatQuantity(
                            report.RejectedQuantity,
                            report.UnitName));


                    AddCenteredValueCell(
                        table,
                        GetOverallResultText(
                            report.Result));

                    #endregion
                });
        }

        #endregion


        #region Remarks Section

        private static void ComposeRemarksSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Table(table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.ConstantColumn(
                                90);

                            columns.RelativeColumn();
                        });

                    #endregion


                    #region Supplier Remarks

                    AddLabelCell(
                        table,
                        "Supplier Remarks");

                    table.Cell()
                        .Element(
                            ValueCell)
                        .MinHeight(
                            24)
                        .Text(
                            Display(
                                report.SupplierRemarks));

                    #endregion


                    #region Inspection Remarks

                    AddLabelCell(
                        table,
                        "Inspection Remarks");

                    table.Cell()
                        .Element(
                            ValueCell)
                        .MinHeight(
                            30)
                        .Text(
                            Display(
                                report.InspectionRemarks));

                    #endregion
                });
        }

        #endregion


        #region Approval Section

        private static void ComposeApprovalSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    #region Section Heading

                    column.Item()
                        .Background("#1F4E78")
                        .PaddingVertical(4)
                        .PaddingHorizontal(8)
                        .Text("APPROVAL / RELEASE")
                        .FontColor(Colors.White)
                        .FontSize(8)
                        .Bold();

                    #endregion


                    #region Approval Table

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                            #region Column Headers

                            table.Cell()
                                .Element(ApprovalHeaderCell)
                                .Text("Inspected By");


                            table.Cell()
                                .Element(ApprovalHeaderCell)
                                .Text("Reviewed / Approved By");


                            table.Cell()
                                .Element(ApprovalHeaderCell)
                                .Text("Dispatch Release");

                            #endregion


                            #region Inspected By

                            table.Cell()
                                .Element(ApprovalBodyCell)
                                .Column(body =>
                                {
                                    body.Spacing(1);


                                    body.Item()
                                        .Text("Quality Inspector")
                                        .FontSize(SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Name: {Display(report.InspectedBy)}")
                                        .FontSize(SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Date: {report.InspectionDate:dd-MM-yyyy}")
                                        .FontSize(SmallFontSize);
                                });

                            #endregion


                            #region Reviewed / Approved By

                            table.Cell()
                                .Element(ApprovalBodyCell)
                                .Column(body =>
                                {
                                    body.Spacing(1);


                                    body.Item()
                                        .Text("Quality / Authorized Person")
                                        .FontSize(SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Name: {Display(report.ReviewedBy)}")
                                        .FontSize(SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Date: {(report.FinalizedOn.HasValue
                                                ? report.FinalizedOn.Value.ToString("dd-MM-yyyy")
                                                : "-")}")
                                        .FontSize(SmallFontSize);
                                });

                            #endregion


                            #region Dispatch Release

                            table.Cell()
                                .Element(ApprovalBodyCell)
                                .Column(body =>
                                {
                                    body.Spacing(1);


                                    body.Item()
                                        .Text(
                                            GetDispatchReleaseText(report))
                                        .Bold()
                                        .FontSize(DefaultFontSize);


                                    body.Item()
                                        .Text(
                                            $"Accepted Qty: {FormatQuantity(
                                                report.AcceptedQuantity,
                                                report.UnitName)}")
                                        .FontSize(SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Reference: {Display(report.Code)}")
                                        .FontSize(SmallFontSize);
                                });

                            #endregion
                        });

                    #endregion
                });
        }

        #endregion


        #region Table Cell Helpers

        private static IContainer HeaderCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .Padding(
                    4);
        }

        #region Approval Theme Helpers

        private static IContainer ApprovalHeaderCell(
            IContainer container)
        {
            return container
                .Border(0.7f)
                .BorderColor("#B7C4D2")
                .Background("#EEF3F8")
                .PaddingVertical(5)
                .PaddingHorizontal(6)
                .MinHeight(24)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(SmallFontSize)
                            .Bold());
        }


        private static IContainer ApprovalBodyCell(
            IContainer container)
        {
            return container
                .Border(0.7f)
                .BorderColor("#B7C4D2")
                .Background(Colors.White)
                .Padding(6)
                .MinHeight(62);
        }

        #endregion


        private static IContainer LabelCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .Background(
                    Colors.Grey.Lighten3)
                .PaddingVertical(
                    3)
                .PaddingHorizontal(
                    4)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(
                                SmallFontSize)
                            .Bold());
        }


        private static IContainer ValueCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .PaddingVertical(
                    3)
                .PaddingHorizontal(
                    4)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style.FontSize(
                            DefaultFontSize));
        }


        private static IContainer TableHeaderCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .Background(
                    Colors.Grey.Lighten3)
                .Padding(
                    2)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(
                                SmallFontSize)
                            .Bold());
        }


        private static IContainer BodyCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .Padding(
                    2)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style.FontSize(
                            SmallFontSize));
        }


        private static IContainer ApprovalValueCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .MinHeight(
                    28)
                .Padding(
                    4)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(
                                DefaultFontSize)
                            .Bold());
        }

        #endregion


        #region Table Add Helpers

        private static void AddLabelCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Element(
                    LabelCell)
                .Text(
                    text);
        }


        private static void AddValueCell(
            TableDescriptor table,
            string? text)
        {
            table.Cell()
                .Element(
                    ValueCell)
                .Text(
                    Display(text));
        }


        private static void AddCenteredLabelCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Element(
                    LabelCell)
                .AlignCenter()
                .Text(
                    text);
        }


        private static void AddCenteredValueCell(
            TableDescriptor table,
            string? text)
        {
            table.Cell()
                .Element(
                    ValueCell)
                .AlignCenter()
                .Text(
                    Display(text))
                .Bold();
        }


        private static void AddBodyCell(
            TableDescriptor table,
            string? text,
            bool center = false)
        {
            var cell =
                table.Cell()
                    .Element(
                        BodyCell);


            if (center)
            {
                cell =
                    cell.AlignCenter();
            }


            cell.Text(
                Display(text));
        }

        #endregion


        #region Display Helpers

        private static string Display(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? "-"
                : value.Trim();
        }


        private static string FormatQuantity(
            decimal quantity,
            string? unitName)
        {
            var value =
                quantity.ToString(
                    "0.###");


            return string.IsNullOrWhiteSpace(
                unitName)
                ? value
                : $"{value} {unitName.Trim()}";
        }


        private static string GetLineResultText(
            PreDispatchInspectionLineResult result)
        {
            return result switch
            {
                PreDispatchInspectionLineResult.Pass =>
                    "PASS",

                PreDispatchInspectionLineResult.Fail =>
                    "FAIL",

                PreDispatchInspectionLineResult.NotApplicable =>
                    "N/A",

                _ =>
                    "PENDING"
            };
        }


        private static string GetOverallResultText(
            PreDispatchInspectionResult result)
        {
            return result switch
            {
                PreDispatchInspectionResult.Pass =>
                    "PASS",

                PreDispatchInspectionResult.Fail =>
                    "FAIL",

                PreDispatchInspectionResult.Partial =>
                    "PARTIAL",

                _ =>
                    "PENDING"
            };
        }


        private static string GetDispatchReleaseText(
            PreDispatchInspection report)
        {
            if (
                report.Status !=
                PreDispatchInspectionStatus.Finalized)
            {
                return "PENDING";
            }


            return report.Result ==
                PreDispatchInspectionResult.Pass
                    ? "RELEASED"
                    : "NOT RELEASED";
        }

        #endregion
    }
}