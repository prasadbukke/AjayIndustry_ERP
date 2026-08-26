/*
============================================================
File: DeliveryChallanPdfGenerator.cs

Purpose:
Generates the customer-facing Delivery Challan PDF.

Responsibilities:
- Generate A4 Portrait Delivery Challan.
- Display saved Company / Workshop snapshot.
- Display saved Customer information.
- Display saved editable Customer delivery address.
- Display Challan No., Date and L.P.G. No.
- Display Product ID, Item / Part and HSN No.
- Display Customer PO reference.
- Display Dispatch Quantity and UOM.
- Display Remarks and signature section.

Important:
- PDF is generated only from saved Delivery Challan data.
- Customer delivery address uses the saved editable
  Delivery Challan address snapshot.
- Company information is read from CompanySnapshotJson.
- Customer tax information is read from CustomerSnapshotJson.
- Customer Contact / Mobile / Email are intentionally
  NOT displayed on the Delivery Challan PDF.
- Transport Details are intentionally NOT displayed.
- Customer Drawing No. / Revision are intentionally
  NOT displayed.
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
    public class DeliveryChallanPdfGenerator
        : IDeliveryChallanPdfGenerator
    {
        #region Theme

        private const string BorderColor =
            "#808080";

        private const string HeaderBackground =
            "#E9ECEF";

        private const string LightBackground =
            "#F8F9FA";

        private const string TextColor =
            "#202020";

        #endregion


        #region Generate

        public byte[] Generate(
            DeliveryChallan deliveryChallan)
        {
            ArgumentNullException.ThrowIfNull(
                deliveryChallan);


            #region Active Items

            var items =
                deliveryChallan.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();

            #endregion


            #region Parse Master Snapshots

            var customerSnapshot =
                ParseSnapshot(
                    deliveryChallan.CustomerSnapshotJson);

            var companySnapshot =
                ParseSnapshot(
                    deliveryChallan.CompanySnapshotJson);

            #endregion


            #region Document

            var document =
                Document.Create(
                    documentContainer =>
                    {
                        documentContainer.Page(
                            page =>
                            {
                                #region Page Setup

                                page.Size(
                                    PageSizes.A4);

                                page.Margin(
                                    14);

                                page.DefaultTextStyle(
                                    style =>
                                        style
                                            .FontSize(8.5f)
                                            .FontColor(
                                                TextColor));

                                #endregion


                                #region Header

                                page.Header()
                                    .Element(
                                        container =>
                                            ComposeHeader(
                                                container,
                                                deliveryChallan,
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
                                                6);


                                            column.Item()
                                                .Element(
                                                    container =>
                                                        ComposeCustomerDetails(
                                                            container,
                                                            deliveryChallan,
                                                            customerSnapshot));


                                            column.Item()
                                                .Element(
                                                    container =>
                                                        ComposeItemTable(
                                                            container,
                                                            items));


                                            column.Item()
                                                .Element(
                                                    container =>
                                                        ComposeQuantitySummary(
                                                            container,
                                                            items));


                                            column.Item()
                                                .Element(
                                                    container =>
                                                        ComposeRemarks(
                                                            container,
                                                            deliveryChallan));


                                            column.Item()
                                                .Element(
                                                    ComposeSignatureSection);
                                        });

                                #endregion


                                #region Footer

                                page.Footer()
                                    .Element(
                                        container =>
                                            ComposeFooter(
                                                container,
                                                deliveryChallan));

                                #endregion
                            });
                    });

            #endregion


            return document.GeneratePdf();
        }

        #endregion


        #region Header

        private static void ComposeHeader(
            IContainer container,
            DeliveryChallan deliveryChallan,
            Dictionary<string, JsonElement> companySnapshot)
        {
            #region Company Values

            var companyName =
                FirstNonEmpty(
                    deliveryChallan.CompanyName,
                    GetSnapshotString(
                        companySnapshot,
                        "CompanyName"),
                    "AJAY INDUSTRIES");


            var companyAddress =
                FormatCompanyAddress(
                    companySnapshot);


            var gstNumber =
                GetSnapshotString(
                    companySnapshot,
                    "GstNumber");


            var phoneNumber =
                GetSnapshotString(
                    companySnapshot,
                    "PhoneNumber");


            var email =
                GetSnapshotString(
                    companySnapshot,
                    "Email");


            var website =
                GetSnapshotString(
                    companySnapshot,
                    "Website");


            var contactLine =
                JoinNonEmpty(
                    "  |  ",
                    FormatLabelValue(
                        "Phone",
                        phoneNumber),
                    FormatLabelValue(
                        "Email",
                        email),
                    FormatLabelValue(
                        "Website",
                        website));

            #endregion


            container
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .Column(
                    column =>
                    {
                        #region Company Name

                        column.Item()
                            .PaddingTop(
                                6)
                            .PaddingHorizontal(
                                5)
                            .AlignCenter()
                            .Text(
                                companyName)
                            .FontSize(
                                14)
                            .Bold();

                        #endregion


                        #region Company Address

                        if (!string.IsNullOrWhiteSpace(
                            companyAddress))
                        {
                            column.Item()
                                .PaddingTop(
                                    2)
                                .PaddingHorizontal(
                                    8)
                                .AlignCenter()
                                .Text(
                                    companyAddress)
                                .FontSize(
                                    8);
                        }

                        #endregion


                        #region Company GST

                        if (!string.IsNullOrWhiteSpace(
                            gstNumber))
                        {
                            column.Item()
                                .PaddingTop(
                                    2)
                                .AlignCenter()
                                .DefaultTextStyle(
                                    style =>
                                        style.FontSize(
                                            8))
                                .Text(
                                    text =>
                                    {
                                        text.Span(
                                                "GSTIN: ")
                                            .Bold();

                                        text.Span(
                                            gstNumber);
                                    });
                        }

                        #endregion


                        #region Company Contact

                        if (!string.IsNullOrWhiteSpace(
                            contactLine))
                        {
                            column.Item()
                                .PaddingTop(
                                    2)
                                .PaddingBottom(
                                    5)
                                .PaddingHorizontal(
                                    8)
                                .AlignCenter()
                                .Text(
                                    contactLine)
                                .FontSize(
                                    7.5f);
                        }

                        #endregion


                        #region Document Title

                        column.Item()
                            .BorderTop(
                                1)
                            .BorderBottom(
                                1)
                            .BorderColor(
                                BorderColor)
                            .Background(
                                HeaderBackground)
                            .PaddingVertical(
                                5)
                            .AlignCenter()
                            .Text(
                                "DELIVERY CHALLAN")
                            .FontSize(
                                12)
                            .Bold();

                        #endregion


                        #region Challan Information

                        column.Item()
                            .Row(
                                row =>
                                {
                                    row.RelativeItem()
                                        .Padding(
                                            5)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                        "Challan No.: ")
                                                    .Bold();

                                                text.Span(
                                                    Display(
                                                        deliveryChallan.Code));
                                            });


                                    row.RelativeItem()
                                        .BorderLeft(
                                            1)
                                        .BorderColor(
                                            BorderColor)
                                        .Padding(
                                            5)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                        "Date: ")
                                                    .Bold();

                                                text.Span(
                                                    deliveryChallan
                                                        .ChallanDate
                                                        .ToString(
                                                            "dd-MM-yyyy"));
                                            });


                                    row.RelativeItem()
                                        .BorderLeft(
                                            1)
                                        .BorderColor(
                                            BorderColor)
                                        .Padding(
                                            5)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                        "L.P.G. No.: ")
                                                    .Bold();

                                                text.Span(
                                                    Display(
                                                        deliveryChallan
                                                            .LpgNumber));
                                            });
                                });

                        #endregion
                    });
        }

        #endregion


        #region Customer Details

        private static void ComposeCustomerDetails(
            IContainer container,
            DeliveryChallan deliveryChallan,
            Dictionary<string, JsonElement> customerSnapshot)
        {
            #region Customer Values

            var gstin =
                GetSnapshotString(
                    customerSnapshot,
                    "GSTIN");


            var addressLines =
                BuildCustomerAddressLines(
                    deliveryChallan);

            #endregion


            container
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .Column(
                    column =>
                    {
                        #region Section Heading

                        column.Item()
                            .Background(
                                HeaderBackground)
                            .BorderBottom(
                                1)
                            .BorderColor(
                                BorderColor)
                            .PaddingVertical(
                                4)
                            .PaddingHorizontal(
                                6)
                            .Text(
                                "CUSTOMER / DELIVERY DETAILS")
                            .Bold();

                        #endregion


                        #region Customer Identity Row

                        column.Item()
                            .Background(
                                LightBackground)
                            .BorderBottom(
                                1)
                            .BorderColor(
                                BorderColor)
                            .Row(
                                row =>
                                {
                                    #region Customer Name

                                    row.RelativeItem(
                                            2)
                                        .PaddingVertical(
                                            5)
                                        .PaddingHorizontal(
                                            7)
                                        .Column(
                                            customerColumn =>
                                            {
                                                customerColumn.Item()
                                                    .Text(
                                                        "Customer Name")
                                                    .FontSize(
                                                        7)
                                                    .Bold();


                                                customerColumn.Item()
                                                    .PaddingTop(
                                                        2)
                                                    .Text(
                                                        Display(
                                                            deliveryChallan
                                                                .CustomerName))
                                                    .FontSize(
                                                        9)
                                                    .Bold();
                                            });

                                    #endregion


                                    #region GSTIN

                                    row.RelativeItem()
                                        .BorderLeft(
                                            1)
                                        .BorderColor(
                                            BorderColor)
                                        .PaddingVertical(
                                            5)
                                        .PaddingHorizontal(
                                            7)
                                        .Column(
                                            gstColumn =>
                                            {
                                                gstColumn.Item()
                                                    .Text(
                                                        "GSTIN")
                                                    .FontSize(
                                                        7)
                                                    .Bold();


                                                gstColumn.Item()
                                                    .PaddingTop(
                                                        2)
                                                    .Text(
                                                        Display(
                                                            gstin))
                                                    .FontSize(
                                                        9)
                                                    .Bold();
                                            });

                                    #endregion
                                });

                        #endregion


                        #region Delivery Address

                        column.Item()
                            .PaddingVertical(
                                6)
                            .PaddingHorizontal(
                                7)
                            .Column(
                                addressColumn =>
                                {
                                    #region Address Heading

                                    addressColumn.Item()
                                        .Text(
                                            "Delivery Address")
                                        .FontSize(
                                            7.5f)
                                        .Bold();

                                    #endregion


                                    #region Address Values

                                    if (addressLines.Count == 0)
                                    {
                                        addressColumn.Item()
                                            .PaddingTop(
                                                3)
                                            .Text(
                                                "-")
                                            .FontSize(
                                                8.5f);
                                    }
                                    else
                                    {
                                        foreach (var line
                                            in addressLines)
                                        {
                                            addressColumn.Item()
                                                .PaddingTop(
                                                    2)
                                                .Text(
                                                    line)
                                                .FontSize(
                                                    8.5f);
                                        }
                                    }

                                    #endregion
                                });

                        #endregion
                    });
        }

        #endregion


        #region Item Table

        private static void ComposeItemTable(
            IContainer container,
            List<DeliveryChallanItem> items)
        {
            container
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .Table(
                    table =>
                    {
                        #region Columns

                        table.ColumnsDefinition(
                            columns =>
                            {
                                columns.ConstantColumn(
                                    26);

                                columns.ConstantColumn(
                                    62);

                                columns.RelativeColumn(
                                    2.1f);

                                columns.ConstantColumn(
                                    62);

                                columns.ConstantColumn(
                                    85);

                                columns.ConstantColumn(
                                    55);

                                columns.ConstantColumn(
                                    43);
                            });

                        #endregion


                        #region Header

                        table.Header(
                            header =>
                            {
                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "Sr.")
                                    .Bold();


                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "Product ID")
                                    .Bold();


                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "Item / Part Description")
                                    .Bold();


                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "HSN No.")
                                    .Bold();


                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "Customer PO")
                                    .Bold();


                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "Qty")
                                    .Bold();


                                header.Cell()
                                    .Element(
                                        TableHeaderCell)
                                    .Text(
                                        "UOM")
                                    .Bold();
                            });

                        #endregion


                        #region Rows

                        if (items.Count == 0)
                        {
                            table.Cell()
                                .ColumnSpan(
                                    7)
                                .Padding(
                                    8)
                                .AlignCenter()
                                .Text(
                                    "No dispatch items.");
                        }
                        else
                        {
                            var serialNumber =
                                1;


                            foreach (var item
                                in items)
                            {
                                #region Serial

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .AlignCenter()
                                    .Text(
                                        serialNumber.ToString());

                                #endregion


                                #region Product ID

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .Text(
                                        Display(
                                            item.ProductReference));

                                #endregion


                                #region Description

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .Column(
                                        column =>
                                        {
                                            column.Item()
                                                .Text(
                                                    Display(
                                                        item.ItemName))
                                                .Bold();


                                            if (!string.IsNullOrWhiteSpace(
                                                item.PartNumber))
                                            {
                                                column.Item()
                                                    .PaddingTop(
                                                        2)
                                                    .Text(
                                                        $"Part No.: {item.PartNumber}")
                                                    .FontSize(
                                                        7.2f);
                                            }


                                            if (!string.IsNullOrWhiteSpace(
                                                item.CustomerItemCode))
                                            {
                                                column.Item()
                                                    .PaddingTop(
                                                        1)
                                                    .Text(
                                                        $"Customer Item: {item.CustomerItemCode}")
                                                    .FontSize(
                                                        7.2f);
                                            }
                                        });

                                #endregion


                                #region HSN

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .AlignCenter()
                                    .Text(
                                        Display(
                                            item.HsnNumber));

                                #endregion


                                #region Customer PO

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .Text(
                                        Display(
                                            item.CustomerPurchaseOrderNumber));

                                #endregion


                                #region Quantity

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .AlignRight()
                                    .Text(
                                        item.DispatchQuantity
                                            .ToString(
                                                "0.###"));

                                #endregion


                                #region UOM

                                table.Cell()
                                    .Element(
                                        TableBodyCell)
                                    .AlignCenter()
                                    .Text(
                                        Display(
                                            item.UnitName));

                                #endregion


                                serialNumber++;
                            }
                        }

                        #endregion
                    });
        }

        #endregion


        #region Quantity Summary

        private static void ComposeQuantitySummary(
            IContainer container,
            List<DeliveryChallanItem> items)
        {
            if (items.Count == 0)
            {
                return;
            }


            #region Quantity Values

            var units =
                items
                    .Select(x =>
                        x.UnitName?.Trim())
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();


            var totalQuantity =
                items.Sum(x =>
                    x.DispatchQuantity);


            var quantityText =
                units.Count == 1
                    ? $"{totalQuantity:0.###} {units[0]}"
                    : units.Count == 0
                        ? totalQuantity.ToString(
                            "0.###")
                        : $"{totalQuantity:0.###} (Mixed UOM)";

            #endregion


            container
                .AlignRight()
                .Width(
                    220)
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .Background(
                                LightBackground)
                            .Padding(
                                5)
                            .Text(
                                "Total Dispatch Quantity")
                            .Bold();


                        row.ConstantItem(
                                95)
                            .BorderLeft(
                                1)
                            .BorderColor(
                                BorderColor)
                            .Padding(
                                5)
                            .AlignRight()
                            .Text(
                                quantityText)
                            .Bold();
                    });
        }

        #endregion


        #region Remarks

        private static void ComposeRemarks(
            IContainer container,
            DeliveryChallan deliveryChallan)
        {
            container
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .Column(
                    column =>
                    {
                        column.Item()
                            .Background(
                                HeaderBackground)
                            .BorderBottom(
                                1)
                            .BorderColor(
                                BorderColor)
                            .Padding(
                                4)
                            .Text(
                                "REMARKS")
                            .Bold();


                        column.Item()
                            .MinHeight(
                                34)
                            .Padding(
                                6)
                            .Text(
                                string.IsNullOrWhiteSpace(
                                    deliveryChallan.Remarks)
                                    ? string.Empty
                                    : deliveryChallan
                                        .Remarks
                                        .Trim());
                    });
        }

        #endregion


        #region Signature Section

        private static void ComposeSignatureSection(
            IContainer container)
        {
            container
                .Border(
                    1)
                .BorderColor(
                    BorderColor)
                .Row(
                    row =>
                    {
                        #region Receiver

                        row.RelativeItem()
                            .MinHeight(
                                72)
                            .Padding(
                                6)
                            .Column(
                                column =>
                                {
                                    column.Item()
                                        .Text(
                                            "Received By")
                                        .Bold();


                                    column.Item()
                                        .PaddingTop(
                                            34)
                                        .Text(
                                            "Name / Signature / Stamp")
                                        .FontSize(
                                            7.5f);
                                });

                        #endregion


                        #region Authorized Signatory

                        row.RelativeItem()
                            .BorderLeft(
                                1)
                            .BorderColor(
                                BorderColor)
                            .MinHeight(
                                72)
                            .Padding(
                                6)
                            .Column(
                                column =>
                                {
                                    column.Item()
                                        .AlignRight()
                                        .Text(
                                            "For AJAY INDUSTRIES")
                                        .Bold();


                                    column.Item()
                                        .PaddingTop(
                                            34)
                                        .AlignRight()
                                        .Text(
                                            "Authorized Signatory")
                                        .FontSize(
                                            7.5f);
                                });

                        #endregion
                    });
        }

        #endregion


        #region Footer

        private static void ComposeFooter(
            IContainer container,
            DeliveryChallan deliveryChallan)
        {
            container
                .PaddingTop(
                    4)
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .Text(
                                $"Challan: {deliveryChallan.Code}")
                            .FontSize(
                                7);


                        row.RelativeItem()
                            .AlignCenter()
                            .Text(
                                "Computer Generated Delivery Challan")
                            .FontSize(
                                7);


                        row.RelativeItem()
                            .AlignRight()
                            .DefaultTextStyle(
                                style =>
                                    style.FontSize(
                                        7))
                            .Text(
                                text =>
                                {
                                    text.Span(
                                        "Page ");

                                    text.CurrentPageNumber();

                                    text.Span(
                                        " of ");

                                    text.TotalPages();
                                });
                    });
        }

        #endregion


        #region Table Styles

        private static IContainer TableHeaderCell(
            IContainer container)
        {
            return container
                .Background(
                    HeaderBackground)
                .BorderRight(
                    1)
                .BorderBottom(
                    1)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    5)
                .PaddingHorizontal(
                    3)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style.FontSize(
                            7.3f));
        }


        private static IContainer TableBodyCell(
            IContainer container)
        {
            return container
                .BorderRight(
                    1)
                .BorderBottom(
                    1)
                .BorderColor(
                    BorderColor)
                .PaddingVertical(
                    5)
                .PaddingHorizontal(
                    3)
                .AlignMiddle()
                .DefaultTextStyle(
                    style =>
                        style.FontSize(
                            7.5f));
        }

        #endregion


        #region Customer Address Formatting

        private static List<string> BuildCustomerAddressLines(
            DeliveryChallan deliveryChallan)
        {
            var lines =
                new List<string>();


            #region Address Line 1

            if (!string.IsNullOrWhiteSpace(
                deliveryChallan.CustomerAddressLine1))
            {
                lines.Add(
                    deliveryChallan
                        .CustomerAddressLine1
                        .Trim());
            }

            #endregion


            #region Address Line 2

            if (!string.IsNullOrWhiteSpace(
                deliveryChallan.CustomerAddressLine2))
            {
                lines.Add(
                    deliveryChallan
                        .CustomerAddressLine2
                        .Trim());
            }

            #endregion


            #region City / District

            var cityDistrict =
                JoinNonEmpty(
                    ", ",
                    deliveryChallan.CustomerCity,
                    deliveryChallan.CustomerDistrict);


            if (!string.IsNullOrWhiteSpace(
                cityDistrict))
            {
                lines.Add(
                    cityDistrict);
            }

            #endregion


            #region State / Pincode

            var statePincode =
                JoinNonEmpty(
                    " - ",
                    deliveryChallan.CustomerState,
                    deliveryChallan.CustomerPincode);


            if (!string.IsNullOrWhiteSpace(
                statePincode))
            {
                lines.Add(
                    statePincode);
            }

            #endregion


            #region Country

            if (!string.IsNullOrWhiteSpace(
                deliveryChallan.CustomerCountry))
            {
                lines.Add(
                    deliveryChallan
                        .CustomerCountry
                        .Trim());
            }

            #endregion


            return lines;
        }

        #endregion


        #region Company Address Formatting

        private static string FormatCompanyAddress(
            Dictionary<string, JsonElement> companySnapshot)
        {
            var address =
                GetSnapshotString(
                    companySnapshot,
                    "Address");


            var city =
                GetSnapshotString(
                    companySnapshot,
                    "City");


            var state =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var postalCode =
                GetSnapshotString(
                    companySnapshot,
                    "PostalCode");


            var country =
                GetSnapshotString(
                    companySnapshot,
                    "Country");


            var cityState =
                JoinNonEmpty(
                    ", ",
                    city,
                    state);


            var cityStatePostal =
                string.IsNullOrWhiteSpace(
                    postalCode)
                    ? cityState
                    : string.IsNullOrWhiteSpace(
                        cityState)
                        ? postalCode
                        : $"{cityState} - {postalCode}";


            return JoinNonEmpty(
                ", ",
                address,
                cityStatePostal,
                country);
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
                return new Dictionary<string, JsonElement>();
            }


            try
            {
                var result =
                    JsonSerializer.Deserialize<
                        Dictionary<string, JsonElement>>(
                            snapshotJson);


                return result
                    ?? new Dictionary<string, JsonElement>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, JsonElement>();
            }
        }


        private static string? GetSnapshotString(
            Dictionary<string, JsonElement> snapshot,
            string propertyName)
        {
            if (
                !snapshot.TryGetValue(
                    propertyName,
                    out var value)
            )
            {
                return null;
            }


            if (
                value.ValueKind ==
                JsonValueKind.Null
            )
            {
                return null;
            }


            if (
                value.ValueKind ==
                JsonValueKind.String
            )
            {
                return value.GetString();
            }


            return value.ToString();
        }

        #endregion


        #region Text Helpers

        private static string Display(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? "-"
                : value.Trim();
        }


        private static string FirstNonEmpty(
            params string?[] values)
        {
            return values
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(
                        x))
                ?.Trim()
                ?? string.Empty;
        }


        private static string JoinNonEmpty(
            string separator,
            params string?[] values)
        {
            return string.Join(
                separator,
                values
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x))
                    .Select(x =>
                        x!.Trim()));
        }


        private static string? FormatLabelValue(
            string label,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return null;
            }


            return
                $"{label}: {value.Trim()}";
        }

        #endregion
    }
}