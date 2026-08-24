/*
============================================================
File: PreDispatchInspectionPdfGenerator.cs

Purpose:
Generates the Final Inspection Report PDF for a
Finalized Pre-Dispatch Inspection.

Responsibilities:
- Generate A4 Landscape PDF.
- Apply consistent Ajay Industries report theme.
- Render Report Information.
- Render Customer Drawing information.
- Render Inspection Parameters.
- Render all readings under one OBSERVATION block.
- Render Inspection Notes.
- Render Inspection Result summary.
- Render Remarks.
- Render Approval / Release.
- Return PDF as byte[].

PDF Inspection Table:
Sr
Parameters
Specification
Inspection Method
Observation 1 ... 10

Important:
- Result and Remarks are not shown as Inspection Table columns.
- Line Result / Remarks remain stored in database.
- ERP Item Code and Customer Item Code are not shown in PDF.
- Only Customer Drawing Number and Revision are shown.
- Existing Observation + Interval Reading data is combined
  visually into one OBSERVATION block.
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
            9f;

        private const float SmallFontSize =
            7.5f;

        private const float MainTitleFontSize =
            14f;

        private const float SectionTitleFontSize =
            9f;

        private const float SectionContentGap =
            2f;


        /*
         * Default database structure:
         *
         * Normal Observations = 7
         * Interval Readings    = 3
         *
         * PDF combines both visually:
         *
         * Observation 1 ... 10
         */

        private const int DefaultObservationCount =
            7;

        private const int DefaultIntervalCount =
            3;


        #region Theme

        private const string ThemeDarkBlue =
            "#4477A6";

        private const string ThemeLightBlue =
            "#EEF3F8";

        private const string ThemeBorder =
            "#B7C4D2";

        private const string ThemeWhite =
            "#FFFFFF";

        #endregion

        #endregion


        #region Generate

        public byte[] Generate(
            PreDispatchInspection preDispatchInspection, string documentNumber)
        {
            #region Validation

            if (preDispatchInspection == null)
            {
                throw new ArgumentNullException(
                    nameof(preDispatchInspection));
            }

            #endregion


            #region Document

            return Document
                .Create(document =>
                {
                    document.Page(page =>
                    {
                        #region Page Setup

                        page.Size(
                            PageSizes.A4.Landscape());

                        page.Margin(
                            6);

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
                                    4);


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeMainHeader(
                                                container,
                                                preDispatchInspection,
                                                documentNumber));


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeReportInformation(
                                                container,
                                                preDispatchInspection));


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeDrawingInformation(
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
                            .BorderTop(
                                BorderWidth)
                            .BorderColor(
                                ThemeBorder)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text(
                                        "AJAY INDUSTRIES - FINAL INSPECTION REPORT")
                                    .FontSize(
                                        6f)
                                    .FontColor(
                                        ThemeDarkBlue)
                                    .Bold();


                                row.RelativeItem()
                                    .AlignRight()
                                    .Text(text =>
                                    {
                                        text
                                            .Span(
                                                "Page ")
                                            .FontSize(
                                                6f);


                                        text.CurrentPageNumber();


                                        text
                                            .Span(
                                                " of ")
                                            .FontSize(
                                                6f);


                                        text.TotalPages();
                                    });
                            });

                        #endregion
                    });
                })
                .GeneratePdf();

            #endregion
        }

        #endregion


        #region Main Header

        private static void ComposeMainHeader(
            IContainer container,
            PreDispatchInspection report, string documentNumber)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Title Bar

                    column.Item()
                        .Background(
                            ThemeDarkBlue)
                        .PaddingVertical(
                            6)
                        .PaddingHorizontal(
                            8)
                        .Row(row =>
                        {
                            row.RelativeItem(
                                    2)
                                .AlignMiddle()
                                .Text(
                                    "AJAY INDUSTRIES")
                                .FontColor(
                                    Colors.White)
                                .FontSize(
                                    11f)
                                .Bold();


                            row.RelativeItem(
                                    4)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(
                                    "FINAL INSPECTION REPORT")
                                .FontColor(
                                    Colors.White)
                                .FontSize(
                                    MainTitleFontSize)
                                .Bold();


                            row.RelativeItem(
                                    2)
                                .AlignRight()
                                .AlignMiddle()
                                .Text(
                                    $"REPORT NO.: {Display(report.Code)}")
                                .FontColor(
                                    Colors.White)
                                .FontSize(
                                    SmallFontSize)
                                .Bold();
                        });

                    #endregion


                    #region Document Information

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                            AddHeaderLabelCell(
                                table,
                                "Document No.");


                            AddHeaderValueCell(
                                table,
                                documentNumber);


                            AddHeaderLabelCell(
                                table,
                                "Status");


                            AddHeaderValueCell(
                                table,
                                report.Status ==
                                PreDispatchInspectionStatus.Finalized
                                    ? "FINALIZED"
                                    : "DRAFT");
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
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "REPORT INFORMATION"));

                    #endregion


                    #region Information Table

                    /*
                     * 2 information groups per row.
                     *
                     * Removing ERP Item Code and
                     * Customer Item Code gives us wider
                     * readable value columns.
                     */

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.ConstantColumn(
                                        90);

                                    columns.RelativeColumn();

                                    columns.ConstantColumn(
                                        90);

                                    columns.RelativeColumn();
                                });


                            #region Row 1

                            AddLabelCell(
                                table,
                                "Part / Product");

                            AddValueCell(
                                table,
                                report.ItemName);


                            AddLabelCell(
                                table,
                                "Part No.");

                            AddValueCell(
                                table,
                                report.PartNumber);

                            #endregion


                            #region Row 2

                            AddLabelCell(
                                table,
                                "Customer");

                            AddValueCell(
                                table,
                                report.CustomerName);


                            AddLabelCell(
                                table,
                                "Customer PO");

                            AddValueCell(
                                table,
                                report.CustomerPurchaseOrderNumber);

                            #endregion


                            #region Row 3

                            AddLabelCell(
                                table,
                                "Production Job");

                            AddValueCell(
                                table,
                                report.ProductionJobCode);


                            AddLabelCell(
                                table,
                                "Inspection Date");

                            AddValueCell(
                                table,
                                report.InspectionDate
                                    .ToString(
                                        "dd-MM-yyyy"));

                            #endregion


                            #region Row 4

                            AddLabelCell(
                                table,
                                "Inspection Qty");

                            AddValueCell(
                                table,
                                FormatQuantity(
                                    report.InspectionQuantity,
                                    report.UnitName));


                            AddLabelCell(
                                table,
                                "Invoice No.");

                            AddValueCell(
                                table,
                                report.InvoiceNumber);

                            #endregion


                            #region Row 5

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

                    #endregion
                });
        }

        #endregion


        #region Drawing Information

        private static void ComposeDrawingInformation(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "DRAWING INFORMATION"));

                    #endregion


                    #region Customer Drawing Table

                    /*
                     * Final Inspection Report is
                     * customer-facing.
                     *
                     * Therefore only Customer Drawing
                     * Number + Customer Drawing Revision
                     * are displayed.
                     */

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.ConstantColumn(
                                        130);

                                    columns.RelativeColumn();

                                    columns.ConstantColumn(
                                        100);

                                    columns.RelativeColumn();
                                });


                            AddLabelCell(
                                table,
                                "Customer Drawing No.");

                            AddValueCell(
                                table,
                                report.CustomerDrawingNumber);


                            AddLabelCell(
                                table,
                                "Revision No.");

                            AddValueCell(
                                table,
                                report.CustomerDrawingRevision);
                        });

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


            #region Observation Counts

            /*
             * Both database reading types are visually
             * merged into one Observation block.
             */

            var normalObservationCount =
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


            var intervalObservationCount =
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


            var totalObservationCount =
                normalObservationCount +
                intervalObservationCount;

            #endregion


            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "INSPECTION PARAMETERS & OBSERVATIONS"));

                    #endregion


                    #region Table

                    column.Item()
                        .Table(table =>
                        {
                            #region Columns

                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.ConstantColumn(
                                        24);

                                    columns.RelativeColumn(
                                        2.2f);

                                    columns.RelativeColumn(
                                        2.5f);

                                    columns.RelativeColumn(
                                        2.0f);


                                    for (
                                        var i = 0;
                                        i < totalObservationCount;
                                        i++)
                                    {
                                        columns.RelativeColumn(
                                            0.72f);
                                    }
                                });

                            #endregion


                            #region Header

                            table.Header(header =>
                            {
                                ComposeInspectionTableHeader(
                                    header,
                                    totalObservationCount);
                            });

                            #endregion


                            #region Empty

                            if (lines.Count == 0)
                            {
                                var totalColumns =
                                    4 +
                                    totalObservationCount;


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

                            #endregion


                            #region Rows

                            foreach (var line in lines)
                            {
                                #region Readings

                                var normalObservations =
                                    line.Observations
                                        .Where(x =>
                                            !x.IsDeleted &&
                                            x.IsActive &&
                                            !x.IsIntervalReading)
                                        .GroupBy(x =>
                                            x.SequenceNumber)
                                        .ToDictionary(
                                            x =>
                                                x.Key,
                                            x =>
                                                x.First().Value);


                                var intervalObservations =
                                    line.Observations
                                        .Where(x =>
                                            !x.IsDeleted &&
                                            x.IsActive &&
                                            x.IsIntervalReading)
                                        .GroupBy(x =>
                                            x.SequenceNumber)
                                        .ToDictionary(
                                            x =>
                                                x.Key,
                                            x =>
                                                x.First().Value);

                                #endregion


                                #region Base Columns

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


                                #region Normal Observations

                                for (
                                    var sequence = 1;
                                    sequence <= normalObservationCount;
                                    sequence++)
                                {
                                    normalObservations
                                        .TryGetValue(
                                            sequence,
                                            out var value);


                                    var displayValue =
                                        string.IsNullOrWhiteSpace(value) ||
                                        value.Trim() == "-"
                                            ? string.Empty
                                            : value.Trim();


                                    table.Cell()
                                        .Element(
                                            BodyCell)
                                        .AlignCenter()
                                        .Text(
                                            displayValue);
                                }

                                #endregion


                                #region Interval Observations

                                for (
                                    var sequence = 1;
                                    sequence <= intervalObservationCount;
                                    sequence++)
                                {
                                    intervalObservations
                                        .TryGetValue(
                                            sequence,
                                            out var value);


                                    var displayValue =
                                        string.IsNullOrWhiteSpace(value) ||
                                        value.Trim() == "-"
                                            ? string.Empty
                                            : value.Trim();


                                    table.Cell()
                                        .Element(
                                            BodyCell)
                                        .AlignCenter()
                                        .Text(
                                            displayValue);
                                }

                                #endregion
                            }

                            #endregion
                        });

                    #endregion
                });
        }


        private static void ComposeInspectionTableHeader(
            TableCellDescriptor header,
            int totalObservationCount)
        {
            #region First Row

            header.Cell()
                .RowSpan(
                    2)
                .Element(
                    TableHeaderCell)
                .Text(
                    "Sr");


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
                    (uint)totalObservationCount)
                .Element(
                    GroupHeaderCell)
                .Text(
                    "OBSERVATION");

            #endregion


            #region Observation Numbers

            for (
                var sequence = 1;
                sequence <= totalObservationCount;
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
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "INSPECTION NOTES"));

                    #endregion


                    #region Notes

                    column.Item()
                        .Border(
                            BorderWidth)
                        .BorderColor(
                            ThemeBorder)
                        .Background(
                            ThemeWhite)
                        .PaddingVertical(
                            5)
                        .PaddingHorizontal(
                            6)
                        .Column(notes =>
                        {
                            notes.Spacing(
                                2);


                            notes.Item()
                                .Text(
                                    "• ALL DIMENSIONS ARE IN MM")
                                .FontSize(
                                    SmallFontSize)
                                .Bold();


                            notes.Item()
                                .Text(
                                    "• ALL SAMPLES ARE CHECKED RANDOMLY")
                                .FontSize(
                                    SmallFontSize)
                                .Bold();
                        });

                    #endregion
                });
        }

        #endregion


        #region Inspection Result

        private static void ComposeResultSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "INSPECTION RESULT"));

                    #endregion


                    #region Result Table

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                            #region Headers

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

                    #endregion
                });
        }

        #endregion


        #region Remarks

        private static void ComposeRemarksSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "REMARKS"));

                    #endregion


                    #region Remarks Table

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.ConstantColumn(
                                        110);

                                    columns.RelativeColumn();
                                });


                            AddLabelCell(
                                table,
                                "Supplier Remarks");


                            table.Cell()
                                .Element(
                                    ValueCell)
                                .MinHeight(
                                    25)
                                .Text(
                                    DisplayBlank(
                                        report.SupplierRemarks));


                            AddLabelCell(
                                table,
                                "Inspection Remarks");


                            table.Cell()
                                .Element(
                                    ValueCell)
                                .MinHeight(
                                    28)
                                .Text(
                                    DisplayBlank(
                                        report.InspectionRemarks));
                        });

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
                    column.Spacing(
                        SectionContentGap);


                    #region Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "APPROVAL / RELEASE"));

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


                            #region Headers

                            table.Cell()
                                .Element(
                                    ApprovalHeaderCell)
                                .Text(
                                    "Inspected By");


                            table.Cell()
                                .Element(
                                    ApprovalHeaderCell)
                                .Text(
                                    "Reviewed / Approved By");


                            table.Cell()
                                .Element(
                                    ApprovalHeaderCell)
                                .Text(
                                    "Dispatch Release");

                            #endregion


                            #region Inspected By

                            table.Cell()
                                .Element(
                                    ApprovalBodyCell)
                                .Column(body =>
                                {
                                    body.Spacing(
                                        2);


                                    body.Item()
                                        .Text(
                                            "Quality Inspector")
                                        .FontSize(
                                            SmallFontSize);


                                    body.Item()
    .Text(text =>
    {
        text
            .Span("Name: ")
            .FontSize(
                SmallFontSize);

        text
            .Span(
                Display(report.InspectedBy))
            .FontSize(
                SmallFontSize)
            .Bold();
    });


                                    body.Item()
                                        .Text(
                                            $"Date: {report.InspectionDate:dd-MM-yyyy}")
                                        .FontSize(
                                            SmallFontSize);
                                });

                            #endregion


                            #region Reviewed By

                            table.Cell()
                                .Element(
                                    ApprovalBodyCell)
                                .Column(body =>
                                {
                                    body.Spacing(
                                        2);


                                    body.Item()
                                        .Text(
                                            "Quality / Authorized Person")
                                        .FontSize(
                                            SmallFontSize);


                                    body.Item()
    .Text(text =>
    {
        text
            .Span("Name: ")
            .FontSize(
                SmallFontSize);

        text
            .Span(
                Display(report.ReviewedBy))
            .FontSize(
                SmallFontSize)
            .Bold();
    });


                                    body.Item()
                                        .Text(
                                            $"Date: {(report.FinalizedOn.HasValue
                                                ? report.FinalizedOn.Value.ToString("dd-MM-yyyy")
                                                : "-")}")
                                        .FontSize(
                                            SmallFontSize);
                                });

                            #endregion


                            #region Dispatch Release

                            table.Cell()
                                .Element(
                                    ApprovalBodyCell)
                                .Column(body =>
                                {
                                    body.Spacing(
                                        2);


                                    body.Item()
                                        .Text(
                                            GetDispatchReleaseText(
                                                report))
                                        .Bold()
                                        .FontSize(
                                            DefaultFontSize);


                                    body.Item()
                                        .Text(
                                            $"Accepted Qty: {FormatQuantity(
                                                report.AcceptedQuantity,
                                                report.UnitName)}")
                                        .FontSize(
                                            SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Reference: {Display(report.Code)}")
                                        .FontSize(
                                            SmallFontSize);
                                });

                            #endregion
                        });

                    #endregion
                });
        }

        #endregion


        #region Theme Helpers

        private static IContainer SectionHeading(
            IContainer container,
            string title)
        {
            var styledContainer =
                container
                    .Background(
                        ThemeDarkBlue)
                    .PaddingVertical(
                        4)
                    .PaddingHorizontal(
                        8);


            styledContainer
                .Text(
                    title)
                .FontColor(
                    Colors.White)
                .FontSize(
                    SectionTitleFontSize)
                .Bold();


            return styledContainer;
        }


        private static IContainer LabelCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeLightBlue)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    5)
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
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeWhite)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    5)
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
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeLightBlue)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
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


        private static IContainer GroupHeaderCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeDarkBlue)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    2)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(
                                SmallFontSize)
                            .FontColor(
                                Colors.White)
                            .Bold());
        }


        private static IContainer BodyCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeWhite)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    3)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style.FontSize(
                            SmallFontSize));
        }


        private static IContainer ApprovalHeaderCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeLightBlue)
                .PaddingVertical(
                    5)
                .PaddingHorizontal(
                    6)
                .MinHeight(
                    23)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(
                                SmallFontSize)
                            .Bold());
        }


        private static IContainer ApprovalBodyCell(
            IContainer container)
        {
            return container
                .Border(
                    BorderWidth)
                .BorderColor(
                    ThemeBorder)
                .Background(
                    ThemeWhite)
                .Padding(
                    6)
                .MinHeight(
                    55);
        }

        #endregion


        #region Header Cell Helpers

        private static void AddHeaderLabelCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Element(
                    LabelCell)
                .Text(
                    text);
        }


        private static void AddHeaderValueCell(
            TableDescriptor table,
            string? text)
        {
            table.Cell()
                .Element(
                    ValueCell)
                .Text(
                    Display(text));
        }

        #endregion


        #region Standard Cell Helpers

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

        #region Observation Cell Helper

        private static void AddObservationCell(
            TableDescriptor table,
            string? text)
        {
            var displayValue =
                string.IsNullOrWhiteSpace(text) ||
                text.Trim() == "-"
                    ? string.Empty
                    : text.Trim();


            table.Cell()
                .Element(
                    BodyCell)
                .AlignCenter()
                .Text(
                    displayValue);
        }

        #endregion

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
        private static string DisplayBlank(
    string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? string.Empty
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