/*
============================================================
File: InvoicePdfGenerator.cs

Module:
Invoice

Purpose:
Generates Finalized Customer Invoice PDF.

Responsibilities:
- Display Company header.
- Display Invoice number, date and due date.
- Display Customer / Billing information.
- Display Customer PO references.
- Display Invoice Items.
- Display GST and financial summary.
- Display Amount In Words.
- Display Company Bank Details.
- Display Invoice Terms & Conditions.
- Display Authorized Signature section.

Important:
- Company PAN is intentionally NOT displayed.
- Delivery Challan No. is NOT displayed
  in Item Description.
- Customer PO No. is NOT displayed
  in Item Description.
- Customer PO No. is displayed in BILL TO section.
- Company / Bank information comes from saved snapshot.
============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace AjayIndustriesERP.Infrastructure.Pdf
{
    public class InvoicePdfGenerator
        : IInvoicePdfGenerator
    {
        #region Constructor

        public InvoicePdfGenerator()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;
        }

        #endregion


        #region Generate

        public byte[] Generate(
            Invoice invoice)
        {
            ArgumentNullException.ThrowIfNull(
                invoice);


            var companySnapshot =
                ParseSnapshot(
                    invoice.CompanySnapshotJson);


            var customerSnapshot =
                ParseSnapshot(
                    invoice.CustomerSnapshotJson);


            var document =
                Document.Create(
                    container =>
                    {
                        container.Page(
                            page =>
                            {
                                #region Page Setup

                                page.Size(
                                    PageSizes.A4);

                                page.Margin(
                                    18);

                                page.DefaultTextStyle(
                                    style =>
                                        style.FontSize(
                                            9));

                                #endregion


                                #region Header

                                page.Header()
                                    .Element(
                                        header =>
                                            ComposeHeader(
                                                header,
                                                invoice,
                                                companySnapshot));

                                #endregion


                                #region Content

                                page.Content()
                                    .PaddingTop(
                                        6)
                                    .Column(
                                        column =>
                                        {
                                            column.Spacing(
                                                7);


                                            #region Invoice Information

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeInvoiceMeta(
                                                            content,
                                                            invoice));

                                            #endregion


                                            #region Bill To

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeBillTo(
                                                            content,
                                                            invoice,
                                                            customerSnapshot));

                                            #endregion


                                            #region Items

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeItemsTable(
                                                            content,
                                                            invoice));

                                            #endregion


                                            #region Financial Summary

                                            column.Item()
                                                .AlignRight()
                                                .Width(
                                                    340)
                                                .Element(
                                                    content =>
                                                        ComposeAmountSummary(
                                                            content,
                                                            invoice));

                                            #endregion


                                            #region Amount In Words

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeAmountInWords(
                                                            content,
                                                            invoice.GrandTotal));

                                            #endregion


                                            #region Bank Details

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeBankDetails(
                                                            content,
                                                            companySnapshot));

                                            #endregion


                                            #region Terms

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeTerms(
                                                            content,
                                                            invoice));

                                            #endregion


                                            #region Remarks

                                            if (!string.IsNullOrWhiteSpace(
                                                invoice.Remarks))
                                            {
                                                column.Item()
                                                    .Element(
                                                        content =>
                                                            ComposeRemarks(
                                                                content,
                                                                invoice.Remarks));
                                            }

                                            #endregion


                                            #region Signature

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeSignature(
                                                            content,
                                                            invoice));

                                            #endregion
                                        });

                                #endregion


                                #region Footer

                                page.Footer()
                                    .AlignCenter()
                                    .DefaultTextStyle(
                                        style =>
                                            style.FontSize(
                                                8))
                                    .Text(
                                        text =>
                                        {
                                            text.Span(
                                                "This is a system generated Invoice  |  Page ");

                                            text.CurrentPageNumber();

                                            text.Span(
                                                " of ");

                                            text.TotalPages();
                                        });

                                #endregion
                            });
                    });


            return document.GeneratePdf();
        }

        #endregion


        #region Company Header

        private static void ComposeHeader(
            IContainer container,
            Invoice invoice,
            Dictionary<string, JsonElement> companySnapshot)
        {
            var companyName =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "CompanyName")
                ?? invoice.CompanyName
                ?? string.Empty;


            var address =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "Address");


            var city =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "City");


            var state =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "State");


            var postalCode =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "PostalCode");


            var country =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "Country");


            var gstNumber =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "GstNumber",
                    "GSTNumber",
                    "GSTIN");


            var isoNumber =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "IsoCertificationNumber");


            var phone =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "PhoneNumber");


            var email =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "Email");


            var website =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "Website");


            container
                .Border(
                    1)
                .Padding(
                    6)
                .Column(
                    column =>
                    {
                        column.Spacing(
                            2);


                        #region Company Name

                        column.Item()
                            .AlignCenter()
                            .Text(
                                DisplayValue(
                                    companyName))
                            .Bold()
                            .FontSize(
                                14);

                        #endregion


                        #region Company Address

                        var addressParts =
                            new List<string>();


                        AddIfNotEmpty(
                            addressParts,
                            address);


                        AddIfNotEmpty(
                            addressParts,
                            city);


                        AddIfNotEmpty(
                            addressParts,
                            state);


                        AddIfNotEmpty(
                            addressParts,
                            postalCode);


                        AddIfNotEmpty(
                            addressParts,
                            country);


                        if (addressParts.Count > 0)
                        {
                            column.Item()
                                .AlignCenter()
                                .Text(
                                    string.Join(
                                        ", ",
                                        addressParts));
                        }

                        #endregion


                        #region GST / ISO

                        if (
                            !string.IsNullOrWhiteSpace(
                                gstNumber)
                            ||
                            !string.IsNullOrWhiteSpace(
                                isoNumber)
                        )
                        {
                            column.Item()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        var hasPrevious =
                                            false;


                                        if (!string.IsNullOrWhiteSpace(
                                            gstNumber))
                                        {
                                            text.Span(
                                                $"GSTIN: {gstNumber}");

                                            hasPrevious =
                                                true;
                                        }


                                        if (!string.IsNullOrWhiteSpace(
                                            isoNumber))
                                        {
                                            if (hasPrevious)
                                            {
                                                text.Span(
                                                    "  |  ");
                                            }


                                            text.Span(
                                                $"ISO: {isoNumber}");
                                        }
                                    });
                        }

                        #endregion


                        #region Contact

                        if (
                            !string.IsNullOrWhiteSpace(
                                phone)
                            ||
                            !string.IsNullOrWhiteSpace(
                                email)
                            ||
                            !string.IsNullOrWhiteSpace(
                                website)
                        )
                        {
                            column.Item()
                                .AlignCenter()
                                .Text(
                                    text =>
                                    {
                                        var hasPrevious =
                                            false;


                                        if (!string.IsNullOrWhiteSpace(
                                            phone))
                                        {
                                            text.Span(
                                                $"Phone: {phone}");

                                            hasPrevious =
                                                true;
                                        }


                                        if (!string.IsNullOrWhiteSpace(
                                            email))
                                        {
                                            if (hasPrevious)
                                            {
                                                text.Span(
                                                    "  |  ");
                                            }


                                            text.Span(
                                                $"Email: {email}");

                                            hasPrevious =
                                                true;
                                        }


                                        if (!string.IsNullOrWhiteSpace(
                                            website))
                                        {
                                            if (hasPrevious)
                                            {
                                                text.Span(
                                                    "  |  ");
                                            }


                                            text.Span(
                                                $"Website: {website}");
                                        }
                                    });
                        }

                        #endregion
                    });
        }

        #endregion


        #region Invoice Information

        private static void ComposeInvoiceMeta(
            IContainer container,
            Invoice invoice)
        {
            container
                .Column(
                    column =>
                    {
                        #region Title

                        column.Item()
                            .Background(
                                Colors.Grey.Lighten3)
                            .Border(
                                1)
                            .Padding(
                                4)
                            .AlignCenter()
                            .Text(
                                "TAX INVOICE")
                            .Bold()
                            .FontSize(
                                12);

                        #endregion


                        #region Meta Table

                        column.Item()
                            .Table(
                                table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });


                                    table.Cell()
                                        .Element(
                                            MetaCell)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                    "Invoice No.: ")
                                                    .SemiBold();

                                                text.Span(
                                                    invoice.Code);
                                            });


                                    table.Cell()
                                        .Element(
                                            MetaCell)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                    "Invoice Date: ")
                                                    .SemiBold();

                                                text.Span(
                                                    invoice.InvoiceDate
                                                        .ToString(
                                                            "dd-MM-yyyy"));
                                            });


                                    table.Cell()
                                        .Element(
                                            MetaCell)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                    "Due Date: ")
                                                    .SemiBold();

                                                text.Span(
                                                    invoice.DueDate.HasValue
                                                        ? invoice.DueDate.Value
                                                            .ToString(
                                                                "dd-MM-yyyy")
                                                        : "-");
                                            });
                                });

                        #endregion
                    });
        }


        private static IContainer MetaCell(
            IContainer container)
        {
            return container
                .BorderLeft(
                    1)
                .BorderRight(
                    1)
                .BorderBottom(
                    1)
                .Padding(
                    4);
        }

        #endregion


        #region Bill To

        private static void ComposeBillTo(
            IContainer container,
            Invoice invoice,
            Dictionary<string, JsonElement> customerSnapshot)
        {
            var customerGstin =
                GetFirstSnapshotValue(
                    customerSnapshot,
                    "GSTIN",
                    "Gstin",
                    "GstNumber");


            var customerPan =
                GetFirstSnapshotValue(
                    customerSnapshot,
                    "PAN",
                    "PanNumber");


            var customerPONumbers =
                invoice.Items
                    .Where(
                        item =>
                            !item.IsDeleted &&
                            item.IsActive &&
                            !string.IsNullOrWhiteSpace(
                                item.CustomerPurchaseOrderNumber))
                    .Select(
                        item =>
                            item.CustomerPurchaseOrderNumber!)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();


            var addressLines =
                BuildBillingAddressLines(
                    invoice);


            container
                .Border(
                    1)
                .Column(
                    column =>
                    {
                        #region Heading

                        column.Item()
                            .Background(
                                Colors.Grey.Lighten3)
                            .BorderBottom(
                                1)
                            .Padding(
                                4)
                            .Text(
                                "BILL TO")
                            .Bold();

                        #endregion


                        #region Main Information

                        column.Item()
                            .Table(
                                table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn(
                                                1.6f);

                                            columns.RelativeColumn(
                                                1.4f);

                                            columns.RelativeColumn(
                                                1.4f);
                                        });


                                    #region Customer Name

                                    table.Cell()
                                        .BorderRight(
                                            1)
                                        .BorderBottom(
                                            1)
                                        .Padding(
                                            6)
                                        .Column(
                                            cell =>
                                            {
                                                cell.Item()
                                                    .Text(
                                                        "Customer Name")
                                                    .SemiBold()
                                                    .FontSize(
                                                        8);


                                                cell.Item()
                                                    .PaddingTop(
                                                        2)
                                                    .Text(
                                                        DisplayValue(
                                                            invoice.CustomerName))
                                                    .Bold();
                                            });

                                    #endregion


                                    #region Customer PO

                                    table.Cell()
                                        .BorderRight(
                                            1)
                                        .BorderBottom(
                                            1)
                                        .Padding(
                                            6)
                                        .Column(
                                            cell =>
                                            {
                                                cell.Item()
                                                    .Text(
                                                        "Customer PO No.")
                                                    .SemiBold()
                                                    .FontSize(
                                                        8);


                                                cell.Item()
                                                    .PaddingTop(
                                                        2)
                                                    .Text(
                                                        customerPONumbers.Count > 0
                                                            ? string.Join(
                                                                ", ",
                                                                customerPONumbers)
                                                            : "-")
                                                    .Bold();
                                            });

                                    #endregion


                                    #region GSTIN

                                    table.Cell()
                                        .BorderBottom(
                                            1)
                                        .Padding(
                                            6)
                                        .Column(
                                            cell =>
                                            {
                                                cell.Item()
                                                    .Text(
                                                        "GSTIN")
                                                    .SemiBold()
                                                    .FontSize(
                                                        8);


                                                cell.Item()
                                                    .PaddingTop(
                                                        2)
                                                    .Text(
                                                        DisplayValue(
                                                            customerGstin))
                                                    .Bold();
                                            });

                                    #endregion


                                    #region Billing Address

                                    table.Cell()
                                        .ColumnSpan(
                                            2)
                                        .BorderRight(
                                            1)
                                        .Padding(
                                            6)
                                        .Column(
                                            cell =>
                                            {
                                                cell.Item()
                                                    .Text(
                                                        "Billing Address")
                                                    .SemiBold()
                                                    .FontSize(
                                                        8);


                                                if (addressLines.Count == 0)
                                                {
                                                    cell.Item()
                                                        .PaddingTop(
                                                            3)
                                                        .Text(
                                                            "-");
                                                }
                                                else
                                                {
                                                    foreach (var line
                                                        in addressLines)
                                                    {
                                                        cell.Item()
                                                            .PaddingTop(
                                                                2)
                                                            .Text(
                                                                line);
                                                    }
                                                }
                                            });

                                    #endregion


                                    #region Billing Information

                                    table.Cell()
                                        .Padding(
                                            6)
                                        .Column(
                                            cell =>
                                            {
                                                cell.Spacing(
                                                    5);


                                                cell.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "PAN: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    customerPan));
                                                        });


                                                cell.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Place of Supply: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    invoice.PlaceOfSupply));
                                                        });


                                                cell.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Payment Terms: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    invoice.PaymentTerms));
                                                        });


                                                cell.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Credit Days: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                invoice.CreditDays.HasValue
                                                                    ? invoice.CreditDays.Value
                                                                        .ToString()
                                                                    : "-");
                                                        });
                                            });

                                    #endregion
                                });

                        #endregion
                    });
        }

        #endregion


        #region Items Table

        private static void ComposeItemsTable(
            IContainer container,
            Invoice invoice)
        {
            var items =
                invoice.Items
                    .Where(
                        item =>
                            !item.IsDeleted &&
                            item.IsActive)
                    .OrderBy(
                        item =>
                            item.SequenceNumber)
                    .ToList();


            container.Table(
                table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.ConstantColumn(
                                22);                 // Sr.

                            columns.RelativeColumn(
                                2.7f);               // Description

                            columns.RelativeColumn(
                                1.15f);              // Product ID

                            columns.RelativeColumn(
                                1.35f);              // Customer PO

                            columns.RelativeColumn(
                                1.0f);               // HSN No.

                            columns.RelativeColumn(
                                0.9f);               // Qty

                            columns.RelativeColumn(
                                1.0f);               // Rate

                            columns.RelativeColumn(
                                0.8f);               // Disc. %

                            columns.RelativeColumn(
                                0.8f);               // GST %

                            columns.RelativeColumn(
                                1.35f);              // Amount
                        });

                    #endregion


                    #region Header

                    table.Header(
                        header =>
                        {
                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Sr.")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Description")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Product ID")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Customer PO")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "HSN No.")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Qty")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Rate")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Disc.%")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "GST %")
                                .SemiBold()
                                .FontSize(
                                    8);


                            header.Cell()
                                .Element(
                                    HeaderCellStyle)
                                .AlignCenter()
                                .Text(
                                    "Amount")
                                .SemiBold()
                                .FontSize(
                                    8);
                        });

                    #endregion


                    #region Rows

                    var serialNumber =
                        1;


                    foreach (var item
                        in items)
                    {
                        #region Serial Number

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                serialNumber.ToString());

                        #endregion


                        #region Description

                        table.Cell()
                            .Element(
                                BodyCell)
                            .Column(
                                description =>
                                {
                                    description.Item()
                                        .Text(
                                            DisplayValue(
                                                item.ItemName))
                                        .SemiBold();


                                    if (!string.IsNullOrWhiteSpace(
                                        item.PartNumber))
                                    {
                                        description.Item()
                                            .PaddingTop(
                                                1)
                                            .Text(
                                                $"Part No.: {item.PartNumber}")
                                            .FontSize(
                                                8);
                                    }


                                    if (!string.IsNullOrWhiteSpace(
                                        item.CustomerItemCode))
                                    {
                                        description.Item()
                                            .PaddingTop(
                                                1)
                                            .Text(
                                                $"Customer Item Code: {item.CustomerItemCode}")
                                            .FontSize(
                                                8);
                                    }
                                });

                        #endregion


                        #region Product ID

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                DisplayValue(
                                    item.ProductReference));

                        #endregion


                        #region Customer PO

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                !string.IsNullOrWhiteSpace(
                                    item.CustomerPurchaseOrderNumber)
                                    ? item.CustomerPurchaseOrderNumber
                                    : DisplayValue(
                                        item.CustomerPurchaseOrderCode));

                        #endregion


                        #region HSN Number

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                DisplayValue(
                                    item.HsnNumber));

                        #endregion


                        #region Quantity

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Column(
                                quantity =>
                                {
                                    quantity.Item()
                                        .AlignCenter()
                                        .Text(
                                            item.InvoiceQuantity
                                                .ToString(
                                                    "0.###"));


                                    if (!string.IsNullOrWhiteSpace(
                                        item.UnitName))
                                    {
                                        quantity.Item()
                                            .AlignCenter()
                                            .Text(
                                                item.UnitName)
                                            .FontSize(
                                                8);
                                    }
                                });

                        #endregion


                        #region Rate

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                item.Rate
                                    .ToString(
                                        "0.00"));

                        #endregion


                        #region Discount

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                item.DiscountPercent
                                    .ToString(
                                        "0.##"));

                        #endregion


                        #region GST

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                item.GstRate
                                    .ToString(
                                        "0.##"));

                        #endregion


                        #region Amount

                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                item.LineTotal
                                    .ToString(
                                        "0.00"));

                        #endregion


                        serialNumber++;
                    }

                    #endregion
                });
        }


        private static IContainer HeaderCellStyle(
            IContainer container)
        {
            return container
                .Background(
                    Colors.Grey.Lighten3)
                .Border(
                    1)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    2);
        }


        private static IContainer BodyCell(
            IContainer container)
        {
            return container
                .Border(
                    1)
                .PaddingVertical(
                    4)
                .PaddingHorizontal(
                    3);
        }

        #endregion


        #region Amount Summary

        private static void ComposeAmountSummary(
            IContainer container,
            Invoice invoice)
        {
            var activeItems =
                invoice.Items
                    .Where(
                        item =>
                            !item.IsDeleted &&
                            item.IsActive)
                    .ToList();


            var distinctGstRates =
                activeItems
                    .Select(
                        item =>
                            item.GstRate)
                    .Distinct()
                    .ToList();


            string cgstLabel;
            string sgstLabel;
            string igstLabel;


            #region Tax Labels

            if (distinctGstRates.Count == 1)
            {
                var gstRate =
                    distinctGstRates[0];


                cgstLabel =
                    $"CGST ({FormatRate(gstRate / 2)}%)";


                sgstLabel =
                    $"SGST ({FormatRate(gstRate / 2)}%)";


                igstLabel =
                    $"IGST ({FormatRate(gstRate)}%)";
            }
            else if (distinctGstRates.Count > 1)
            {
                cgstLabel =
                    "CGST (Mixed)";


                sgstLabel =
                    "SGST (Mixed)";


                igstLabel =
                    "IGST (Mixed)";
            }
            else
            {
                cgstLabel =
                    "CGST (0%)";


                sgstLabel =
                    "SGST (0%)";


                igstLabel =
                    "IGST (0%)";
            }

            #endregion


            container
                .Border(
                    1)
                .Column(
                    column =>
                    {
                        #region Gross Amount

                        AddSummaryRow(
                            column,
                            "Gross Amount",
                            invoice.GrossAmount);

                        #endregion


                        #region Discount

                        /*
                         * Always displayed.
                         * Even when Discount = 0.
                         */
                        AddSummaryRow(
                            column,
                            "Discount",
                            invoice.DiscountAmount);

                        #endregion


                        #region Taxable Amount

                        AddSummaryRow(
                            column,
                            "Taxable Amount",
                            invoice.TaxableAmount);

                        #endregion


                        #region GST

                        if (invoice.IsInterState)
                        {
                            AddSummaryRow(
                                column,
                                igstLabel,
                                invoice.IgstAmount);
                        }
                        else
                        {
                            AddSummaryRow(
                                column,
                                cgstLabel,
                                invoice.CgstAmount);


                            AddSummaryRow(
                                column,
                                sgstLabel,
                                invoice.SgstAmount);
                        }

                        #endregion


                        #region Other Charges

                        AddSummaryRow(
                            column,
                            "Other Charges",
                            invoice.OtherCharges);

                        #endregion


                        #region Round Off

                        AddSummaryRow(
                            column,
                            "Round Off",
                            invoice.RoundOffAmount);

                        #endregion


                        #region Grand Total

                        column.Item()
                            .BorderTop(
                                1)
                            .Background(
                                Colors.Grey.Lighten3)
                            .Padding(
                                5)
                            .Row(
                                row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            "GRAND TOTAL")
                                        .Bold();


                                    row.ConstantItem(
                                            125)
                                        .AlignRight()
                                        .Text(
                                            invoice.GrandTotal
                                                .ToString(
                                                    "0.00"))
                                        .Bold();
                                });

                        #endregion
                    });
        }


        private static void AddSummaryRow(
            ColumnDescriptor column,
            string label,
            decimal amount)
        {
            column.Item()
                .BorderBottom(
                    1)
                .Padding(
                    4)
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .Text(
                                label);


                        row.ConstantItem(
                            125)
                            .AlignRight()
                            .Text(
                                amount.ToString(
                                    "0.00"));
                    });
        }

        #endregion


        #region Amount In Words

        private static void ComposeAmountInWords(
            IContainer container,
            decimal grandTotal)
        {
            container
                .Border(
                    1)
                .Padding(
                    6)
                .Row(
                    row =>
                    {
                        row.ConstantItem(
                                110)
                            .Text(
                                "Amount In Words:")
                            .SemiBold();


                        row.RelativeItem()
                            .Text(
                                ConvertAmountToWords(
                                    grandTotal))
                            .SemiBold();
                    });
        }


        private static string ConvertAmountToWords(
            decimal amount)
        {
            var absoluteAmount =
                Math.Abs(
                    amount);


            var rupees =
                (long)Math.Floor(
                    absoluteAmount);


            var paise =
                (int)Math.Round(
                    (
                        absoluteAmount -
                        rupees
                    ) *
                    100,
                    MidpointRounding.AwayFromZero);


            if (paise == 100)
            {
                rupees++;

                paise =
                    0;
            }


            var result =
                $"{NumberToWordsIndian(rupees)} Rupees";


            if (paise > 0)
            {
                result +=
                    $" and {NumberToWordsIndian(paise)} Paise";
            }


            return result +
                   " Only";
        }


        private static string NumberToWordsIndian(
            long number)
        {
            #region Word Maps

            string[] units =
            {
                "Zero",
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six",
                "Seven",
                "Eight",
                "Nine",
                "Ten",
                "Eleven",
                "Twelve",
                "Thirteen",
                "Fourteen",
                "Fifteen",
                "Sixteen",
                "Seventeen",
                "Eighteen",
                "Nineteen"
            };


            string[] tens =
            {
                "Zero",
                "Ten",
                "Twenty",
                "Thirty",
                "Forty",
                "Fifty",
                "Sixty",
                "Seventy",
                "Eighty",
                "Ninety"
            };

            #endregion


            if (number == 0)
            {
                return "Zero";
            }


            if (number < 0)
            {
                return "Minus " +
                       NumberToWordsIndian(
                           Math.Abs(
                               number));
            }


            var words =
                new List<string>();


            #region Crore

            if (number >= 10000000)
            {
                words.Add(
                    NumberToWordsIndian(
                        number /
                        10000000)
                    +
                    " Crore");


                number %=
                    10000000;
            }

            #endregion


            #region Lakh

            if (number >= 100000)
            {
                words.Add(
                    NumberToWordsIndian(
                        number /
                        100000)
                    +
                    " Lakh");


                number %=
                    100000;
            }

            #endregion


            #region Thousand

            if (number >= 1000)
            {
                words.Add(
                    NumberToWordsIndian(
                        number /
                        1000)
                    +
                    " Thousand");


                number %=
                    1000;
            }

            #endregion


            #region Hundred

            if (number >= 100)
            {
                var hundredIndex =
                    (int)(
                        number /
                        100);


                words.Add(
                    units[
                        hundredIndex]
                    +
                    " Hundred");


                number %=
                    100;
            }

            #endregion


            #region Tens / Units

            if (number >= 20)
            {
                var tensIndex =
                    (int)(
                        number /
                        10);


                words.Add(
                    tens[
                        tensIndex]);


                if (number % 10 > 0)
                {
                    words.Add(
                        units[
                            (int)(
                                number %
                                10)]);
                }
            }
            else if (number > 0)
            {
                words.Add(
                    units[
                        (int)number]);
            }

            #endregion


            return string.Join(
                " ",
                words);
        }

        #endregion


        #region Bank Details

        private static void ComposeBankDetails(
            IContainer container,
            Dictionary<string, JsonElement> companySnapshot)
        {
            var bankName =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "BankName");


            var accountHolder =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "BankAccountHolderName");


            var accountNumber =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "BankAccountNumber");


            var ifsc =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "BankIfscCode");


            var branch =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "BankBranchName");


            var accountType =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "BankAccountType");


            container
                .Border(
                    1)
                .Column(
                    column =>
                    {
                        #region Heading

                        column.Item()
                            .Background(
                                Colors.Grey.Lighten3)
                            .BorderBottom(
                                1)
                            .Padding(
                                4)
                            .Text(
                                "BANK DETAILS")
                            .Bold();

                        #endregion


                        #region Bank Name

                        column.Item()
                            .BorderBottom(
                                1)
                            .Padding(
                                5)
                            .Text(
                                text =>
                                {
                                    text.Span(
                                        "Bank Name: ")
                                        .SemiBold();

                                    text.Span(
                                        DisplayValue(
                                            bankName));
                                });

                        #endregion


                        #region Two Column Bank Details

                        column.Item()
                            .Padding(
                                6)
                            .Row(
                                row =>
                                {
                                    #region Left Column

                                    row.RelativeItem()
                                        .PaddingRight(
                                            8)
                                        .Column(
                                            left =>
                                            {
                                                left.Spacing(
                                                    4);


                                                left.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Account Holder: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    accountHolder));
                                                        });


                                                left.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Account No.: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    accountNumber));
                                                        });


                                                left.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Account Type: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    accountType));
                                                        });
                                            });

                                    #endregion


                                    #region Right Column

                                    row.RelativeItem()
                                        .PaddingLeft(
                                            8)
                                        .Column(
                                            right =>
                                            {
                                                right.Spacing(
                                                    4);


                                                right.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "IFSC: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    ifsc));
                                                        });


                                                right.Item()
                                                    .Text(
                                                        text =>
                                                        {
                                                            text.Span(
                                                                "Branch: ")
                                                                .SemiBold();

                                                            text.Span(
                                                                DisplayValue(
                                                                    branch));
                                                        });
                                            });

                                    #endregion
                                });

                        #endregion
                    });
        }

        #endregion


        #region Terms And Conditions

        private static void ComposeTerms(
            IContainer container,
            Invoice invoice)
        {
            container
                .Border(
                    1)
                .Column(
                    column =>
                    {
                        #region Heading

                        column.Item()
                            .Background(
                                Colors.Grey.Lighten3)
                            .BorderBottom(
                                1)
                            .Padding(
                                4)
                            .Text(
                                "TERMS & CONDITIONS")
                            .Bold();

                        #endregion


                        #region Terms

                        column.Item()
                            .Padding(
                                6)
                            .Text(
                                DisplayValue(
                                    invoice.InvoiceTermsAndConditions));

                        #endregion
                    });
        }

        #endregion


        #region Remarks

        private static void ComposeRemarks(
            IContainer container,
            string remarks)
        {
            container
                .Border(
                    1)
                .Column(
                    column =>
                    {
                        column.Item()
                            .Background(
                                Colors.Grey.Lighten3)
                            .BorderBottom(
                                1)
                            .Padding(
                                4)
                            .Text(
                                "REMARKS")
                            .Bold();


                        column.Item()
                            .Padding(
                                6)
                            .Text(
                                remarks);
                    });
        }

        #endregion


        #region Signature

        private static void ComposeSignature(
            IContainer container,
            Invoice invoice)
        {
            container
                .PaddingTop(
                    8)
                .Row(
                    row =>
                    {
                        row.RelativeItem();


                        row.ConstantItem(
                                220)
                            .Column(
                                column =>
                                {
                                    column.Item()
                                        .AlignCenter()
                                        .Text(
                                            $"For {DisplayValue(invoice.CompanyName)}")
                                        .SemiBold();


                                    column.Item()
                                        .Height(
                                            35);


                                    column.Item()
                                        .AlignCenter()
                                        .Text(
                                            "Authorized Signatory")
                                        .SemiBold();
                                });
                    });
        }

        #endregion


        #region Billing Address Helpers

        private static List<string>
            BuildBillingAddressLines(
                Invoice invoice)
        {
            var lines =
                new List<string>();


            AddIfNotEmpty(
                lines,
                invoice.BillingAddressLine1);


            AddIfNotEmpty(
                lines,
                invoice.BillingAddressLine2);


            var locationParts =
                new List<string>();


            AddIfNotEmpty(
                locationParts,
                invoice.BillingCity);


            AddIfNotEmpty(
                locationParts,
                invoice.BillingDistrict);


            AddIfNotEmpty(
                locationParts,
                invoice.BillingState);


            var location =
                string.Join(
                    ", ",
                    locationParts);


            if (!string.IsNullOrWhiteSpace(
                invoice.BillingPincode))
            {
                location =
                    string.IsNullOrWhiteSpace(
                        location)
                        ? invoice.BillingPincode
                        : $"{location} - {invoice.BillingPincode}";
            }


            AddIfNotEmpty(
                lines,
                location);


            AddIfNotEmpty(
                lines,
                invoice.BillingCountry);


            return lines;
        }

        #endregion


        #region Snapshot Helpers

        private static Dictionary<string, JsonElement>
            ParseSnapshot(
                string? snapshotJson)
        {
            if (string.IsNullOrWhiteSpace(
                snapshotJson))
            {
                return new Dictionary<
                    string,
                    JsonElement>();
            }


            try
            {
                return JsonSerializer
                           .Deserialize<
                               Dictionary<
                                   string,
                                   JsonElement>>(
                                       snapshotJson)

                       ?? new Dictionary<
                           string,
                           JsonElement>();
            }
            catch (JsonException)
            {
                return new Dictionary<
                    string,
                    JsonElement>();
            }
        }


        private static string?
            GetFirstSnapshotValue(
                Dictionary<string, JsonElement> snapshot,
                params string[] propertyNames)
        {
            foreach (var propertyName
                in propertyNames)
            {
                if (!snapshot.TryGetValue(
                    propertyName,
                    out var value))
                {
                    continue;
                }


                if (value.ValueKind ==
                    JsonValueKind.Null)
                {
                    continue;
                }


                if (value.ValueKind ==
                    JsonValueKind.String)
                {
                    var text =
                        value.GetString();


                    if (!string.IsNullOrWhiteSpace(
                        text))
                    {
                        return text;
                    }


                    continue;
                }


                var result =
                    value.ToString();


                if (!string.IsNullOrWhiteSpace(
                    result))
                {
                    return result;
                }
            }


            return null;
        }

        #endregion


        #region Common Helpers

        private static string DisplayValue(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? "-"
                : value;
        }


        private static void AddIfNotEmpty(
            List<string> values,
            string? value)
        {
            if (!string.IsNullOrWhiteSpace(
                value))
            {
                values.Add(
                    value.Trim());
            }
        }


        private static string FormatRate(
            decimal value)
        {
            return value
                .ToString(
                    "0.##");
        }

        #endregion
    }
}