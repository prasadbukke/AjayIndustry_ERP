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
- Render Drawing Information.
- Render Inspection Parameters and readings.
- Render Inspection Notes.
- Render Inspection Result.
- Render Remarks.
- Render Approval / Release.
- Return PDF as byte[].

Theme:
- Medium Blue section headings.
- White uppercase heading text.
- Light Blue / Grey table headings.
- Thin Blue-Grey borders.
- White body cells.
- Compact professional inspection-report layout.

Important:
- Business validation belongs in Application Service.
- PDF uses saved finalized PDI snapshot values.
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

        private const float MainTitleFontSize =
            13f;

        private const float SectionTitleFontSize =
            8f;

        private const float SectionContentGap =
            2f;

        private const int DefaultObservationCount =
            7;

        private const int DefaultIntervalCount =
            3;


        /*
         * Frozen PDF Theme
         */

        private const string ThemeDarkBlue =
            "#4477A6";

        private const string ThemeLightBlue =
            "#EEF3F8";

        private const string ThemeBorder =
            "#B7C4D2";

        private const string ThemeWhite =
            "#FFFFFF";

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
                            10);

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
                                /*
                                 * Gap BETWEEN report sections.
                                 *
                                 * Keep compact because the report
                                 * should normally fit comfortably
                                 * on A4 Landscape.
                                 */

                                column.Spacing(
                                    4);


                                column.Item()
                                    .Element(
                                        container =>
                                            ComposeMainHeader(
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
                                        5.5f)
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
                                                5.5f);


                                        text.CurrentPageNumber();


                                        text
                                            .Span(
                                                " of ")
                                            .FontSize(
                                                5.5f);


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
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Main Title Bar

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
                                    10)
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
                                "Document");

                            AddHeaderValueCell(
                                table,
                                "Final Inspection Report");


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
                    /*
                     * Very small gap between heading
                     * and table.
                     */

                    column.Spacing(
                        SectionContentGap);


                    #region Section Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "REPORT INFORMATION"));

                    #endregion


                    #region Information Table

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.ConstantColumn(
                                        76);

                                    columns.RelativeColumn();

                                    columns.ConstantColumn(
                                        62);

                                    columns.RelativeColumn();

                                    columns.ConstantColumn(
                                        72);

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


                            AddLabelCell(
                                table,
                                "Inspection Date");

                            AddValueCell(
                                table,
                                report.InspectionDate
                                    .ToString(
                                        "dd-MM-yyyy"));

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

                    #endregion
                });
        }

        #endregion


        #region Drawing Information

        private static void ComposeDrawingInformation(
            IContainer container,
            PreDispatchInspection report)
        {
            #region Customer Facing Drawing

            var reportDrawingNumber =
                !string.IsNullOrWhiteSpace(
                    report.CustomerDrawingNumber)
                    ? report.CustomerDrawingNumber
                    : report.WorkshopDrawingNumber;


            var reportDrawingRevision =
                !string.IsNullOrWhiteSpace(
                    report.CustomerDrawingRevision)
                    ? report.CustomerDrawingRevision
                    : report.WorkshopDrawingRevision;

            #endregion


            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Section Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "DRAWING INFORMATION"));

                    #endregion


                    #region Table

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
                                "Drawing No.");

                            AddCenteredLabelCell(
                                table,
                                "Revision");

                            AddCenteredLabelCell(
                                table,
                                "Workshop Drawing");

                            AddCenteredLabelCell(
                                table,
                                "Customer Drawing");

                            #endregion


                            #region Values

                            AddCenteredValueCell(
                                table,
                                reportDrawingNumber);


                            AddCenteredValueCell(
                                table,
                                reportDrawingRevision);


                            AddCenteredValueCell(
                                table,
                                BuildDrawingText(
                                    report.WorkshopDrawingNumber,
                                    report.WorkshopDrawingRevision));


                            AddCenteredValueCell(
                                table,
                                BuildDrawingText(
                                    report.CustomerDrawingNumber,
                                    report.CustomerDrawingRevision));

                            #endregion
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


            #region Dynamic Columns

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
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Section Heading

                    column.Item()
                        .Element(
                            c =>
                                SectionHeading(
                                    c,
                                    "INSPECTION PARAMETERS & OBSERVATIONS"));

                    #endregion


                    #region Inspection Table

                    column.Item()
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


                            #region Header

                            table.Header(header =>
                            {
                                ComposeInspectionTableHeader(
                                    header,
                                    observationCount,
                                    intervalCount);
                            });

                            #endregion


                            #region Empty

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

                            #endregion


                            #region Rows

                            foreach (var line in lines)
                            {
                                var observations =
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


                                var intervals =
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


                                #region Observations

                                for (
                                    var sequence = 1;
                                    sequence <= observationCount;
                                    sequence++)
                                {
                                    observations
                                        .TryGetValue(
                                            sequence,
                                            out var value);


                                    AddBodyCell(
                                        table,
                                        value,
                                        true);
                                }

                                #endregion


                                #region Interval Readings

                                for (
                                    var sequence = 1;
                                    sequence <= intervalCount;
                                    sequence++)
                                {
                                    intervals
                                        .TryGetValue(
                                            sequence,
                                            out var value);


                                    AddBodyCell(
                                        table,
                                        value,
                                        true);
                                }

                                #endregion


                                AddBodyCell(
                                    table,
                                    GetLineResultText(
                                        line.Result),
                                    true);


                                AddBodyCell(
                                    table,
                                    line.Remarks);
                            }

                            #endregion
                        });

                    #endregion
                });
        }


        private static void ComposeInspectionTableHeader(
            TableCellDescriptor header,
            int observationCount,
            int intervalCount)
        {
            #region Main Header Row

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
                    (uint)observationCount)
                .Element(
                    GroupHeaderCell)
                .Text(
                    "OBSERVATION");


            header.Cell()
                .ColumnSpan(
                    (uint)intervalCount)
                .Element(
                    GroupHeaderCell)
                .Text(
                    "READING AT INTERVAL");


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


            #region Observation Numbers

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


            #region Interval Numbers

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
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Section Heading

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
                            4)
                        .PaddingHorizontal(
                            5)
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


        #region Result Section

        private static void ComposeResultSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Section Heading

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


        #region Remarks Section

        private static void ComposeRemarksSection(
            IContainer container,
            PreDispatchInspection report)
        {
            container
                .Column(column =>
                {
                    column.Spacing(
                        SectionContentGap);


                    #region Section Heading

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
                                        95);

                                    columns.RelativeColumn();
                                });


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


                            AddLabelCell(
                                table,
                                "Inspection Remarks");


                            table.Cell()
                                .Element(
                                    ValueCell)
                                .MinHeight(
                                    26)
                                .Text(
                                    Display(
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


                    #region Section Heading

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
                                        1);


                                    body.Item()
                                        .Text(
                                            "Quality Inspector")
                                        .FontSize(
                                            SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Name: {Display(report.InspectedBy)}")
                                        .FontSize(
                                            SmallFontSize);


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
                                        1);


                                    body.Item()
                                        .Text(
                                            "Quality / Authorized Person")
                                        .FontSize(
                                            SmallFontSize);


                                    body.Item()
                                        .Text(
                                            $"Name: {Display(report.ReviewedBy)}")
                                        .FontSize(
                                            SmallFontSize);


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
                                        1);


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
                    3)
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
                    3)
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
                    3)
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
                    22)
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


        private static string BuildDrawingText(
            string? drawingNumber,
            string? revision)
        {
            if (string.IsNullOrWhiteSpace(
                drawingNumber))
            {
                return "-";
            }


            if (string.IsNullOrWhiteSpace(
                revision))
            {
                return drawingNumber.Trim();
            }


            return
                $"{drawingNumber.Trim()} / Rev {revision.Trim()}";
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