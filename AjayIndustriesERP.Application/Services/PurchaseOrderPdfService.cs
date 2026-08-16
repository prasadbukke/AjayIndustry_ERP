/*
==============================================================

File : PurchaseOrderPdfService.cs

Purpose :
Generates professional Purchase Order PDF.

Approved Layout :
- Company Logo + Company Details
- Purchase Order Header
- Supplier & Delivery Details
- Purchase Order Items
- Remarks + Amount Summary
- Terms & Conditions
- Authorized Signatory
- Footer

==============================================================
*/

using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using IContainer =
    QuestPDF.Infrastructure.IContainer;

using QuestDocument =
    QuestPDF.Fluent.Document;


namespace AjayIndustriesERP.Application.Services
{
    public class PurchaseOrderPdfService :
        IPurchaseOrderPdfService
    {
        private readonly string
            _webRootPath;


        // =====================================================
        // PDF COLORS
        // =====================================================

        private const string PrimaryColor =
            "#203B68";

        private const string TableHeaderColor =
            "#35649B";

        private const string GrandTotalColor =
            "#DCEAF5";

        private const string BorderColor =
            "#C9CED6";

        private const string LabelBackground =
            "#F4F5F7";

        private const string MutedColor =
            "#667085";


        public PurchaseOrderPdfService(
            string webRootPath)
        {
            _webRootPath =
                webRootPath;
        }


        // =====================================================
        // REGION 1 — QUEST PDF CONFIGURATION
        // =====================================================

        public static void ConfigureQuestPdf()
        {
            QuestPDF.Settings.License =
                LicenseType.Evaluation;
        }


        // =====================================================
        // REGION 2 — GENERATE PDF
        // =====================================================

        public byte[] GeneratePdf(
            PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(
                    nameof(purchaseOrder));
            }


            /*
             * Logo is mandatory for approved
             * Purchase Order format.
             */
            var logoBytes =
                LoadLogo();


            var document =
                QuestDocument.Create(
                    container =>
                    {
                        container.Page(
                            page =>
                            {
                                page.Size(
                                    PageSizes.A4);

                                page.Margin(
                                    14);

                                page.PageColor(
                                    Colors.White);


                                page.DefaultTextStyle(
                                    style =>
                                        style
                                            .FontSize(
                                                7.5f)
                                            .FontColor(
                                                "#202124"));


                                // =================================
                                // Header
                                // =================================

                                page.Header()
                                    .Element(
                                        c =>
                                            ComposeHeader(
                                                c,
                                                purchaseOrder,
                                                logoBytes));


                                // =================================
                                // Main Content
                                // =================================

                                page.Content()
                                    .PaddingTop(
                                        8)
                                    .Column(
                                        column =>
                                        {
                                            column.Spacing(
                                                8);


                                            /*
                                             * Supplier + Delivery
                                             *
                                             * No separate
                                             * "Purchase Order Information"
                                             * card.
                                             */
                                            column.Item()
                                                .Element(
                                                    c =>
                                                        ComposeSupplierAndDelivery(
                                                            c,
                                                            purchaseOrder));


                                            column.Item()
                                                .Element(
                                                    c =>
                                                        ComposeItemsSection(
                                                            c,
                                                            purchaseOrder));


                                            column.Item()
                                                .Element(
                                                    c =>
                                                        ComposeRemarksAndTotals(
                                                            c,
                                                            purchaseOrder));


                                            if (!string.IsNullOrWhiteSpace(
                                                purchaseOrder
                                                    .TermsAndConditions))
                                            {
                                                column.Item()
                                                    .Element(
                                                        c =>
                                                            ComposeTermsAndConditions(
                                                                c,
                                                                purchaseOrder));
                                            }


                                            column.Item()
                                                .Element(
                                                    c =>
                                                        ComposeSignature(
                                                            c,
                                                            purchaseOrder));
                                        });


                                // =================================
                                // Footer
                                // =================================

                                page.Footer()
                                    .Element(
                                        c =>
                                            ComposeFooter(
                                                c));
                            });
                    });


            return document
                .GeneratePdf();
        }


        // =====================================================
        // REGION 3 — LOAD LOGO
        // =====================================================

        private byte[] LoadLogo()
        {
            var companyFolder =
                Path.Combine(
                    _webRootPath,
                    "images",
                    "company");


            /*
             * Preferred exact filename.
             */
            var preferredLogo =
                Path.Combine(
                    companyFolder,
                    "ajay-industries-logo.png");


            if (File.Exists(
                preferredLogo))
            {
                return File.ReadAllBytes(
                    preferredLogo);
            }


            /*
             * Defensive fallback:
             * If logo exists with jpg/jpeg or
             * slightly different filename.
             */
            if (Directory.Exists(
                companyFolder))
            {
                var fallbackLogo =
                    Directory
                        .EnumerateFiles(
                            companyFolder)
                        .FirstOrDefault(
                            file =>
                            {
                                var extension =
                                    Path
                                        .GetExtension(
                                            file)
                                        .ToLowerInvariant();


                                var fileName =
                                    Path
                                        .GetFileNameWithoutExtension(
                                            file);


                                var supported =
                                    extension == ".png" ||
                                    extension == ".jpg" ||
                                    extension == ".jpeg";


                                return supported &&
                                    fileName.Contains(
                                        "logo",
                                        StringComparison
                                            .OrdinalIgnoreCase);
                            });


                if (!string.IsNullOrWhiteSpace(
                    fallbackLogo))
                {
                    return File.ReadAllBytes(
                        fallbackLogo);
                }
            }


            throw new BusinessException(
                "Company logo not found. " +
                "Please save the logo at: " +
                preferredLogo);
        }


        // =====================================================
        // REGION 4 — HEADER
        // =====================================================

        private static void ComposeHeader(
    IContainer container,
    PurchaseOrder purchaseOrder,
    byte[] logoBytes)
        {
            container
                .PaddingBottom(
                    6)
                .BorderBottom(
                    2)
                .BorderColor(
                    PrimaryColor)
                .Row(
                    row =>
                    {
                        // =============================================
                        // LOGO
                        // =============================================

                        row.ConstantItem(
                                68)
                            .Height(
                                50)
                            .PaddingRight(
                                8)
                            .AlignMiddle()
                            .Image(
                                logoBytes)
                            .FitArea();


                        // =============================================
                        // COMPANY INFORMATION
                        // =============================================

                        row.RelativeItem()
                            .Column(
                                column =>
                                {
                                    column.Spacing(
                                        1);


                                    // Company Name

                                    column.Item()
                                        .Text(
                                            purchaseOrder
                                                .CompanyName ??
                                            "AJAY INDUSTRIES")
                                        .FontSize(
                                            13)
                                        .Bold()
                                        .FontColor(
                                            PrimaryColor);


                                    // Address

                                    if (!string.IsNullOrWhiteSpace(
                                        purchaseOrder.CompanyAddress))
                                    {
                                        column.Item()
                                            .Text(
                                                purchaseOrder
                                                    .CompanyAddress)
                                            .FontSize(
                                                7);
                                    }


                                    // Phone + Email

                                    var contactParts =
                                        new List<string>();


                                    if (!string.IsNullOrWhiteSpace(
                                        purchaseOrder.CompanyPhone))
                                    {
                                        contactParts.Add(
                                            $"Phone: " +
                                            $"{purchaseOrder.CompanyPhone}");
                                    }


                                    if (!string.IsNullOrWhiteSpace(
                                        purchaseOrder.CompanyEmail))
                                    {
                                        contactParts.Add(
                                            $"Email: " +
                                            $"{purchaseOrder.CompanyEmail}");
                                    }


                                    if (contactParts.Count > 0)
                                    {
                                        column.Item()
                                            .Text(
                                                string.Join(
                                                    "    ",
                                                    contactParts))
                                            .FontSize(
                                                7);
                                    }


                                    // Website

                                    if (!string.IsNullOrWhiteSpace(
                                        purchaseOrder.CompanyWebsite))
                                    {
                                        column.Item()
                                            .Text(
                                                $"Website: " +
                                                $"{purchaseOrder.CompanyWebsite}")
                                            .FontSize(
                                                7);
                                    }


                                    // GSTIN Optional

                                    if (!string.IsNullOrWhiteSpace(
                                        purchaseOrder.CompanyGSTIN))
                                    {
                                        column.Item()
                                            .Text(
                                                $"GSTIN: " +
                                                $"{purchaseOrder.CompanyGSTIN}")
                                            .FontSize(
                                                7);
                                    }
                                });


                        // =============================================
                        // PURCHASE ORDER INFORMATION
                        // =============================================

                        row.ConstantItem(
                                205)
                            .PaddingLeft(
                                15)
                            .Column(
                                column =>
                                {
                                    column.Spacing(
                                        2);


                                    // ---------------------------------
                                    // Title
                                    // ---------------------------------

                                    column.Item()
                                        .AlignLeft()
                                        .Text(
                                            "PURCHASE ORDER")
                                        .FontSize(
                                            16)
                                        .Bold()
                                        .FontColor(
                                            PrimaryColor);


                                    // ---------------------------------
                                    // PO No / Date
                                    //
                                    // Label | : | Value
                                    // ---------------------------------

                                    column.Item()
                                        .PaddingTop(
                                            2)
                                        .Table(
                                            table =>
                                            {
                                                table.ColumnsDefinition(
                                                    columns =>
                                                    {
                                                        // Label
                                                        columns.ConstantColumn(
                                                            48);

                                                        // Colon
                                                        columns.ConstantColumn(
                                                            10);

                                                        // Value
                                                        columns.RelativeColumn();
                                                    });


                                                // =====================
                                                // PO NUMBER
                                                // =====================

                                                table.Cell()
                                                    .Text(
                                                        "PO No")
                                                    .Bold()
                                                    .FontSize(
                                                        8);


                                                table.Cell()
                                                    .AlignCenter()
                                                    .Text(
                                                        ":")
                                                    .Bold()
                                                    .FontSize(
                                                        8);


                                                table.Cell()
                                                    .Text(
                                                        purchaseOrder.Code ??
                                                        "-")
                                                    .FontSize(
                                                        8);


                                                // =====================
                                                // PO DATE
                                                // =====================

                                                table.Cell()
                                                    .Text(
                                                        "PO Date")
                                                    .Bold()
                                                    .FontSize(
                                                        8);


                                                table.Cell()
                                                    .AlignCenter()
                                                    .Text(
                                                        ":")
                                                    .Bold()
                                                    .FontSize(
                                                        8);


                                                table.Cell()
                                                    .Text(
                                                        purchaseOrder
                                                            .PODate
                                                            .ToString(
                                                                "dd-MM-yyyy"))
                                                    .FontSize(
                                                        8);
                                            });
                                });
                    });
        }


        // =====================================================
        // REGION 5 — SUPPLIER & DELIVERY DETAILS
        // =====================================================

        private static void ComposeSupplierAndDelivery(
            IContainer container,
            PurchaseOrder purchaseOrder)
        {
            container
                .Column(
                    column =>
                    {
                        // =====================================
                        // Section Header
                        // =====================================

                        column.Item()
                            .Element(
                                c =>
                                    SectionHeader(
                                        c,
                                        "SUPPLIER & DELIVERY DETAILS"));


                        // =====================================
                        // Details Table
                        // =====================================

                        column.Item()
                            .Table(
                                table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn(
                                                1.35f);

                                            columns.RelativeColumn(
                                                2.55f);

                                            columns.RelativeColumn(
                                                1.35f);

                                            columns.RelativeColumn(
                                                2.55f);
                                        });


                                    // -------------------------
                                    // Row 1
                                    // -------------------------

                                    DetailLabelCell(
                                        table,
                                        "Supplier");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .SupplierName);

                                    DetailLabelCell(
                                        table,
                                        "Delivery To");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .CompanyName);


                                    // -------------------------
                                    // Row 2
                                    // -------------------------

                                    DetailLabelCell(
                                        table,
                                        "Address");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .SupplierAddress);

                                    DetailLabelCell(
                                        table,
                                        "Delivery Address");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .DeliveryAddress);


                                    // -------------------------
                                    // Row 3
                                    // -------------------------

                                    DetailLabelCell(
                                        table,
                                        "GSTIN");

                                    DetailValueCell(
                                        table,
                                        string.IsNullOrWhiteSpace(
                                            purchaseOrder
                                                .SupplierGSTIN)
                                            ? "-"
                                            : purchaseOrder
                                                .SupplierGSTIN);

                                    DetailLabelCell(
                                        table,
                                        "Expected Delivery");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .ExpectedDeliveryDate
                                            ?.ToString(
                                                "dd-MM-yyyy")
                                        ?? "-");


                                    // -------------------------
                                    // Row 4
                                    // -------------------------

                                    DetailLabelCell(
                                        table,
                                        "Contact");

                                    DetailValueCell(
                                        table,
                                        BuildSupplierContact(
                                            purchaseOrder));

                                    DetailLabelCell(
                                        table,
                                        "Payment Terms");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .PaymentTerms);


                                    // -------------------------
                                    // Row 5
                                    // -------------------------

                                    DetailLabelCell(
                                        table,
                                        "Email");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .SupplierEmail);

                                    DetailLabelCell(
                                        table,
                                        "Delivery Terms");

                                    DetailValueCell(
                                        table,
                                        purchaseOrder
                                            .DeliveryTerms);
                                });
                    });
        }


        // =====================================================
        // REGION 6 — PURCHASE ORDER ITEMS
        // =====================================================

        private static void ComposeItemsSection(
            IContainer container,
            PurchaseOrder purchaseOrder)
        {
            container
                .Column(
                    column =>
                    {
                        column.Item()
                            .Element(
                                c =>
                                    SectionHeader(
                                        c,
                                        "PURCHASE ORDER ITEMS"));


                        column.Item()
                            .Element(
                                c =>
                                    ComposeItemsTable(
                                        c,
                                        purchaseOrder));
                    });
        }


        private static void ComposeItemsTable(
            IContainer container,
            PurchaseOrder purchaseOrder)
        {
            var items =
                purchaseOrder
                    .Items
                    .Where(
                        x =>
                            !x.IsDeleted)
                    .OrderBy(
                        x =>
                            x.Id)
                    .ToList();


            container
                .Table(
                    table =>
                    {
                        /*
                         * Fixed widths are selected
                         * for A4 portrait to avoid
                         * HSN / UOM splitting badly.
                         */
                        table.ColumnsDefinition(
                            columns =>
                            {
                                columns.ConstantColumn(
                                    20);

                                columns.ConstantColumn(
                                    150);

                                columns.ConstantColumn(
                                    64);

                                columns.ConstantColumn(
                                    39);

                                columns.ConstantColumn(
                                    50);

                                columns.ConstantColumn(
                                    55);

                                columns.ConstantColumn(
                                    42);

                                columns.ConstantColumn(
                                    70);

                                columns.RelativeColumn();
                            });


                        // =====================================
                        // Header
                        // =====================================

                        table.Header(
                            header =>
                            {
                                ItemHeaderCell(
                                    header,
                                    "#");

                                ItemHeaderCell(
                                    header,
                                    "Item / Specification / Drawing");

                                ItemHeaderCell(
                                    header,
                                    "HSN");

                                ItemHeaderCell(
                                    header,
                                    "Qty",
                                    true);

                                ItemHeaderCell(
                                    header,
                                    "UOM");

                                ItemHeaderCell(
                                    header,
                                    "Rate",
                                    true);

                                ItemHeaderCell(
                                    header,
                                    "GST %",
                                    true);

                                ItemHeaderCell(
                                    header,
                                    "Taxable",
                                    true);

                                ItemHeaderCell(
                                    header,
                                    "Line Total",
                                    true);
                            });


                        // =====================================
                        // Body
                        // =====================================

                        var serial =
                            1;


                        foreach (var item
                            in items)
                        {
                            // -----------------------------
                            // Serial
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .Text(
                                    serial);


                            // -----------------------------
                            // Item / Spec / Drawing
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .Column(
                                    itemColumn =>
                                    {
                                        itemColumn.Spacing(
                                            0);


                                        itemColumn.Item()
                                            .Text(
                                                item.ItemName ??
                                                "-")
                                            .Bold()
                                            .FontSize(
                                                7);


                                        if (!string.IsNullOrWhiteSpace(
                                            item.ItemCode))
                                        {
                                            itemColumn.Item()
                                                .Text(
                                                    $"Code: " +
                                                    $"{item.ItemCode}")
                                                .FontSize(
                                                    6);
                                        }


                                        if (!string.IsNullOrWhiteSpace(
                                            item.Specification))
                                        {
                                            itemColumn.Item()
                                                .Text(
                                                    $"Spec: " +
                                                    $"{item.Specification}")
                                                .FontSize(
                                                    6);
                                        }


                                        var drawingText =
                                            "-";


                                        if (!string.IsNullOrWhiteSpace(
                                            item.DrawingNumber))
                                        {
                                            drawingText =
                                                item.DrawingNumber;


                                            if (!string.IsNullOrWhiteSpace(
                                                item.DrawingRevision))
                                            {
                                                drawingText +=
                                                    $" / Rev: " +
                                                    $"{item.DrawingRevision}";
                                            }
                                        }


                                        itemColumn.Item()
                                            .Text(
                                                $"Drawing: " +
                                                $"{drawingText}")
                                            .FontSize(
                                                6);
                                    });


                            // -----------------------------
                            // HSN
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .Text(
                                    string.IsNullOrWhiteSpace(
                                        item.HSNCode)
                                        ? "-"
                                        : item.HSNCode);


                            // -----------------------------
                            // Qty
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .AlignRight()
                                .Text(
                                    item.Quantity
                                        .ToString(
                                            "0.000"));


                            // -----------------------------
                            // UOM
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .Text(
                                    item.UnitName ??
                                    "-");


                            // -----------------------------
                            // Rate
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .AlignRight()
                                .Text(
                                    item.UnitPrice
                                        .ToString(
                                            "N2"));


                            // -----------------------------
                            // GST
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .AlignRight()
                                .Text(
                                    item.GSTPercent
                                        .ToString(
                                            "0.##"));


                            // -----------------------------
                            // Taxable
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .AlignRight()
                                .Text(
                                    item.TaxableAmount
                                        .ToString(
                                            "N2"));


                            // -----------------------------
                            // Line Total
                            // -----------------------------

                            table.Cell()
                                .Element(
                                    ItemBodyCell)
                                .AlignRight()
                                .Text(
                                    item.LineTotal
                                        .ToString(
                                            "N2"));


                            serial++;
                        }
                    });
        }


        // =====================================================
        // REGION 7 — REMARKS + TOTALS
        // =====================================================

        private static void ComposeRemarksAndTotals(
            IContainer container,
            PurchaseOrder purchaseOrder)
        {
            var gstLabels =
                GetGSTLabels(
                    purchaseOrder);


            container
                .Row(
                    row =>
                    {
                        // =====================================
                        // Remarks
                        // =====================================

                        row.RelativeItem(
                                1.25f)
                            .PaddingRight(
                                4)
                            .Column(
                                column =>
                                {
                                    column.Item()
                                        .Element(
                                            c =>
                                                SectionHeader(
                                                    c,
                                                    "REMARKS"));


                                    column.Item()
                                        .MinHeight(
                                            55)
                                        .Border(
                                            1)
                                        .BorderColor(
                                            BorderColor)
                                        .Padding(
                                            6)
                                        .Text(
                                            string.IsNullOrWhiteSpace(
                                                purchaseOrder
                                                    .Remarks)
                                                ? "-"
                                                : purchaseOrder
                                                    .Remarks);
                                });


                        // =====================================
                        // Totals
                        // =====================================

                        row.RelativeItem()
                            .PaddingLeft(
                                4)
                            .Table(
                                table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn();

                                            columns.ConstantColumn(
                                                120);
                                        });


                                    TotalRow(
                                        table,
                                        "Sub Total",
                                        purchaseOrder
                                            .SubTotal);


                                    TotalRow(
                                        table,
                                        "Taxable Amount",
                                        purchaseOrder
                                            .TaxableAmount);


                                    TotalRow(
                                        table,
                                        gstLabels.CGST,
                                        purchaseOrder
                                            .CGSTAmount);


                                    TotalRow(
                                        table,
                                        gstLabels.SGST,
                                        purchaseOrder
                                            .SGSTAmount);


                                    TotalRow(
                                        table,
                                        gstLabels.IGST,
                                        purchaseOrder
                                            .IGSTAmount);


                                    TotalRow(
                                        table,
                                        "Transport Charges",
                                        purchaseOrder
                                            .TransportCharges);


                                    TotalRow(
                                        table,
                                        "Other Charges",
                                        purchaseOrder
                                            .OtherCharges);


                                    GrandTotalRow(
                                        table,
                                        purchaseOrder
                                            .GrandTotal);
                                });
                    });
        }


        // =====================================================
        // REGION 8 — TERMS & CONDITIONS
        // =====================================================

        private static void ComposeTermsAndConditions(
            IContainer container,
            PurchaseOrder purchaseOrder)
        {
            var terms =
                SplitLines(
                    purchaseOrder
                        .TermsAndConditions)
                    .ToList();


            container
                .Column(
                    column =>
                    {
                        column.Item()
                            .Element(
                                c =>
                                    SectionHeader(
                                        c,
                                        "TERMS & CONDITIONS"));


                        if (terms.Count ==
                            0)
                        {
                            column.Item()
                                .Border(
                                    1)
                                .BorderColor(
                                    BorderColor)
                                .Padding(
                                    5)
                                .Text(
                                    "-");

                            return;
                        }


                        foreach (var term
                            in terms)
                        {
                            column.Item()
                                .BorderLeft(
                                    1)
                                .BorderRight(
                                    1)
                                .BorderBottom(
                                    1)
                                .BorderColor(
                                    BorderColor)
                                .PaddingVertical(
                                    4)
                                .PaddingHorizontal(
                                    6)
                                .Text(
                                    term)
                                .FontSize(
                                    6.8f);
                        }
                    });
        }


        // =====================================================
        // REGION 9 — SIGNATURE
        // =====================================================

        private static void ComposeSignature(
    IContainer container,
    PurchaseOrder purchaseOrder)
        {
            container
                .PaddingTop(
                    5)
                .Height(
                    70)
                .Row(
                    row =>
                    {
                        // =============================================
                        // PREPARED / CHECKED BY
                        // LEFT SIDE
                        // =============================================

                        row.RelativeItem()
                            .AlignLeft()
                            .Column(
                                column =>
                                {
                                    column.Item()
                                        .Text(
                                            "Prepared / Checked By")
                                        .Bold()
                                        .FontSize(
                                            7)
                                        .FontColor(
                                            MutedColor);
                                });


                        // =============================================
                        // AUTHORIZED SIGNATORY
                        // RIGHT SIDE
                        // =============================================

                        row.ConstantItem(
                                210)
                            .AlignRight()
                            .Column(
                                column =>
                                {
                                    column.Item()
                                        .AlignRight()
                                        .Text(
                                            $"For " +
                                            $"{purchaseOrder.CompanyName}")
                                        .Bold()
                                        .FontSize(
                                            7.5f);


                                    /*
                                     * Space for physical signature /
                                     * company stamp.
                                     */
                                    column.Item()
                                        .Height(
                                            38);


                                    column.Item()
                                        .AlignRight()
                                        .Text(
                                            "Authorized Signatory")
                                        .Bold()
                                        .FontSize(
                                            7);
                                });
                    });
        }


        // =====================================================
        // REGION 10 — FOOTER
        // =====================================================

        private static void ComposeFooter(
            IContainer container)
        {
            container
                .BorderTop(
                    1)
                .BorderColor(
                    BorderColor)
                .PaddingTop(
                    4)
                .DefaultTextStyle(
                    style =>
                        style
                            .FontSize(
                                6.5f)
                            .FontColor(
                                MutedColor))
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .Text(
                                "System generated Purchase Order - " +
                                "Ajay Industries ERP");


                        row.RelativeItem()
                            .AlignRight()
                            .Text(
                                text =>
                                {
                                    text.Span(
                                        "Page ");

                                    text.CurrentPageNumber();
                                });
                    });
        }


        // =====================================================
        // REGION 11 — SECTION HEADER
        // =====================================================

        private static void SectionHeader(
            IContainer container,
            string title)
        {
            container
                .Background(
                    PrimaryColor)
                .PaddingVertical(
                    5)
                .PaddingHorizontal(
                    7)
                .Text(
                    title)
                .Bold()
                .FontSize(
                    8)
                .FontColor(
                    Colors.White);
        }


        // =====================================================
        // REGION 12 — DETAILS TABLE HELPERS
        // =====================================================

        private static void DetailLabelCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Background(
                    LabelBackground)
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    5)
                .Text(
                    text)
                .Bold()
                .FontSize(
                    6.8f)
                .FontColor(
                    MutedColor);
        }


        private static void DetailValueCell(
            TableDescriptor table,
            string? text)
        {
            table.Cell()
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    5)
                .Text(
                    string.IsNullOrWhiteSpace(
                        text)
                        ? "-"
                        : text)
                .Bold()
                .FontSize(
                    7);
        }


        // =====================================================
        // REGION 13 — ITEM TABLE HELPERS
        // =====================================================

        private static void ItemHeaderCell(
            TableCellDescriptor header,
            string text,
            bool alignRight = false)
        {
            var cell =
                header.Cell()
                    .Background(
                        TableHeaderColor)
                    .Border(
                        0.5f)
                    .BorderColor(
                        Colors.White)
                    .PaddingVertical(
                        4)
                    .PaddingHorizontal(
                        3);


            if (alignRight)
            {
                cell
                    .AlignRight()
                    .Text(
                        text)
                    .Bold()
                    .FontSize(
                        6.2f)
                    .FontColor(
                        Colors.White);

                return;
            }


            cell
                .Text(
                    text)
                .Bold()
                .FontSize(
                    6.2f)
                .FontColor(
                    Colors.White);
        }


        private static IContainer ItemBodyCell(
            IContainer container)
        {
            return container
                .Border(
                    0.5f)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    3)
                .DefaultTextStyle(
                    style =>
                        style.FontSize(
                            6.2f));
        }


        // =====================================================
        // REGION 14 — TOTAL HELPERS
        // =====================================================

        private static void TotalRow(
            TableDescriptor table,
            string label,
            decimal amount)
        {
            table.Cell()
                .Border(
                    0.5f)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    5)
                .Text(
                    label)
                .FontSize(
                    7);


            table.Cell()
                .Border(
                    0.5f)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    5)
                .AlignRight()
                .Text(
                    FormatMoney(
                        amount))
                .FontSize(
                    7);
        }


        private static void GrandTotalRow(
            TableDescriptor table,
            decimal amount)
        {
            table.Cell()
                .Background(
                    GrandTotalColor)
                .Border(
                    0.5f)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    6)
                .PaddingHorizontal(
                    5)
                .Text(
                    "GRAND TOTAL")
                .Bold()
                .FontSize(
                    8)
                .FontColor(
                    PrimaryColor);


            table.Cell()
                .Background(
                    GrandTotalColor)
                .Border(
                    0.5f)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    6)
                .PaddingHorizontal(
                    5)
                .AlignRight()
                .Text(
                    FormatMoney(
                        amount))
                .Bold()
                .FontSize(
                    9)
                .FontColor(
                    PrimaryColor);
        }


        // =====================================================
        // REGION 15 — GST LABELS
        // =====================================================

        private static (
            string CGST,
            string SGST,
            string IGST)
            GetGSTLabels(
                PurchaseOrder purchaseOrder)
        {
            var rates =
                purchaseOrder
                    .Items
                    .Where(
                        x =>
                            !x.IsDeleted &&
                            x.GSTPercent >
                            0)
                    .Select(
                        x =>
                            x.GSTPercent)
                    .Distinct()
                    .ToList();


            if (rates.Count ==
                0)
            {
                return (
                    "CGST (0%)",
                    "SGST (0%)",
                    "IGST (0%)");
            }


            if (rates.Count ==
                1)
            {
                var gst =
                    rates[0];


                var half =
                    gst /
                    2m;


                return (
                    $"CGST ({half:0.##}%)",
                    $"SGST ({half:0.##}%)",
                    $"IGST ({gst:0.##}%)");
            }


            return (
                "CGST (Mixed)",
                "SGST (Mixed)",
                "IGST (Mixed)");
        }


        // =====================================================
        // REGION 16 — FORMATTING HELPERS
        // =====================================================

        private static string
            BuildSupplierContact(
                PurchaseOrder purchaseOrder)
        {
            var parts =
                new List<string>();


            if (!string.IsNullOrWhiteSpace(
                purchaseOrder
                    .SupplierContactPerson))
            {
                parts.Add(
                    purchaseOrder
                        .SupplierContactPerson);
            }


            if (!string.IsNullOrWhiteSpace(
                purchaseOrder
                    .SupplierPhone))
            {
                parts.Add(
                    purchaseOrder
                        .SupplierPhone);
            }


            return parts.Count ==
                0
                ? "-"
                : string.Join(
                    " | ",
                    parts);
        }


        private static string FormatMoney(
            decimal amount)
        {
            return
                $"INR {amount:N2}";
        }


        private static IEnumerable<string>
            SplitLines(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return Array.Empty<string>();
            }


            return value
                .Replace(
                    "\r\n",
                    "\n")
                .Replace(
                    "\r",
                    "\n")
                .Split(
                    '\n',
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .Select(
                    x =>
                        x.Trim())
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x));
        }
    }
}