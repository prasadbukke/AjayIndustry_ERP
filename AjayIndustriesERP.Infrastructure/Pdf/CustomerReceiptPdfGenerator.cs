/*
============================================================
File: CustomerReceiptPdfGenerator.cs

Module:
Customer Receipt

Purpose:
Generates Finalized Customer Receipt PDF.

Responsibilities:
- Display Company information.
- Display Receipt number and date.
- Display Customer information.
- Display Payment Mode and transaction details.
- Display Invoice allocation details.
- Display Total Received Amount.
- Display Amount In Words.
- Display Remarks.
- Display Authorized Signature section.

Important:
- Company / Customer information is read from
  saved historical snapshots.
- Receipt PDF must not depend on current master values.
- Invoice allocation values are saved historical snapshots.
============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace AjayIndustriesERP.Infrastructure.Pdf
{
    public class CustomerReceiptPdfGenerator
        : ICustomerReceiptPdfGenerator
    {
        #region Constructor

        public CustomerReceiptPdfGenerator()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;
        }

        #endregion


        #region Generate

        public byte[] Generate(
            CustomerReceipt customerReceipt)
        {
            ArgumentNullException.ThrowIfNull(
                customerReceipt);


            var companySnapshot =
                ParseSnapshot(
                    customerReceipt.CompanySnapshotJson);


            var customerSnapshot =
                ParseSnapshot(
                    customerReceipt.CustomerSnapshotJson);


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
                                        content =>
                                            ComposeHeader(
                                                content,
                                                customerReceipt,
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


                                            #region Receipt Meta

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeReceiptMeta(
                                                            content,
                                                            customerReceipt));

                                            #endregion


                                            #region Customer

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeCustomerDetails(
                                                            content,
                                                            customerReceipt,
                                                            customerSnapshot));

                                            #endregion


                                            #region Payment Details

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposePaymentDetails(
                                                            content,
                                                            customerReceipt));

                                            #endregion


                                            #region Invoice Allocations

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeAllocationTable(
                                                            content,
                                                            customerReceipt));

                                            #endregion


                                            #region Total Received

                                            column.Item()
                                                .AlignRight()
                                                .Width(
                                                    300)
                                                .Element(
                                                    content =>
                                                        ComposeTotalReceived(
                                                            content,
                                                            customerReceipt));

                                            #endregion


                                            #region Amount In Words

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeAmountInWords(
                                                            content,
                                                            customerReceipt.TotalReceivedAmount));

                                            #endregion


                                            #region Remarks

                                            if (!string.IsNullOrWhiteSpace(
                                                customerReceipt.Remarks))
                                            {
                                                column.Item()
                                                    .Element(
                                                        content =>
                                                            ComposeRemarks(
                                                                content,
                                                                customerReceipt.Remarks));
                                            }

                                            #endregion


                                            #region Signature

                                            column.Item()
                                                .Element(
                                                    content =>
                                                        ComposeSignature(
                                                            content,
                                                            customerReceipt));

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
                                                "This is a system generated Customer Receipt  |  Page ");


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
            CustomerReceipt customerReceipt,
            Dictionary<string, JsonElement> companySnapshot)
        {
            var companyName =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "CompanyName")
                ?? customerReceipt.CompanyName
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
                    "PostalCode",
                    "Pincode");


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


            var phone =
                GetFirstSnapshotValue(
                    companySnapshot,
                    "PhoneNumber",
                    "Phone");


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


                        #region Address

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


                        #region GST

                        if (!string.IsNullOrWhiteSpace(
                            gstNumber))
                        {
                            column.Item()
                                .AlignCenter()
                                .Text(
                                    $"GSTIN: {gstNumber}");
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


        #region Receipt Meta

        private static void ComposeReceiptMeta(
            IContainer container,
            CustomerReceipt customerReceipt)
        {
            container
                .Column(
                    column =>
                    {
                        column.Item()
                            .Background(
                                Colors.Grey.Lighten3)
                            .Border(
                                1)
                            .Padding(
                                4)
                            .AlignCenter()
                            .Text(
                                "CUSTOMER PAYMENT RECEIPT")
                            .Bold()
                            .FontSize(
                                12);


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
                                                        "Receipt No.: ")
                                                    .SemiBold();


                                                text.Span(
                                                    DisplayValue(
                                                        customerReceipt.Code));
                                            });


                                    table.Cell()
                                        .Element(
                                            MetaCell)
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                        "Receipt Date: ")
                                                    .SemiBold();


                                                text.Span(
                                                    customerReceipt
                                                        .ReceiptDate
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
                                                        "Status: ")
                                                    .SemiBold();


                                                text.Span(
                                                    customerReceipt.Status
                                                        .ToString());
                                            });
                                });
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


        #region Customer Details

        private static void ComposeCustomerDetails(
            IContainer container,
            CustomerReceipt customerReceipt,
            Dictionary<string, JsonElement> customerSnapshot)
        {
            var gstNumber =
                GetFirstSnapshotValue(
                    customerSnapshot,
                    "GSTIN",
                    "Gstin",
                    "GstNumber");


            var panNumber =
                GetFirstSnapshotValue(
                    customerSnapshot,
                    "PAN",
                    "PanNumber");


            var phone =
                GetFirstSnapshotValue(
                    customerSnapshot,
                    "PhoneNumber",
                    "MobileNumber",
                    "Phone");


            var email =
                GetFirstSnapshotValue(
                    customerSnapshot,
                    "Email");


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
                                "RECEIVED FROM")
                            .Bold();


                        column.Item()
                            .Padding(
                                6)
                            .Table(
                                table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn();

                                            columns.RelativeColumn();
                                        });


                                    AddDetailCell(
                                        table,
                                        "Customer Name",
                                        customerReceipt.CustomerName);


                                    AddDetailCell(
                                        table,
                                        "Customer Code",
                                        customerReceipt.CustomerCode);


                                    AddDetailCell(
                                        table,
                                        "GSTIN",
                                        gstNumber);


                                    AddDetailCell(
                                        table,
                                        "PAN",
                                        panNumber);


                                    AddDetailCell(
                                        table,
                                        "Phone",
                                        phone);


                                    AddDetailCell(
                                        table,
                                        "Email",
                                        email);
                                });
                    });
        }

        #endregion


        #region Payment Details

        private static void ComposePaymentDetails(
    IContainer container,
    CustomerReceipt customerReceipt)
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
                                "PAYMENT DETAILS")
                            .Bold();

                        #endregion


                        #region Details

                        column.Item()
                            .Padding(
                                6)
                            .Table(
                                table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn();

                                            columns.RelativeColumn();
                                        });


                                    #region Payment Mode

                                    AddDetailCell(
                                        table,
                                        "Payment Mode",
                                        customerReceipt.PaymentMode
                                            .ToString());

                                    #endregion


                                    #region Cash

                                    if (customerReceipt.PaymentMode ==
                                        PaymentMode.Cash)
                                    {
                                        /*
                                         * Cash payment has no additional
                                         * transaction-specific fields.
                                         */
                                    }

                                    #endregion


                                    #region Cheque

                                    else if (customerReceipt.PaymentMode ==
                                             PaymentMode.Cheque)
                                    {
                                        AddDetailCell(
                                            table,
                                            "Cheque No.",
                                            customerReceipt.ChequeNumber);


                                        AddDetailCell(
                                            table,
                                            "Cheque Date",
                                            customerReceipt.ChequeDate.HasValue
                                                ? customerReceipt.ChequeDate.Value
                                                    .ToString(
                                                        "dd-MM-yyyy")
                                                : null);


                                        if (!string.IsNullOrWhiteSpace(
                                            customerReceipt.BankName))
                                        {
                                            AddDetailCell(
                                                table,
                                                "Bank Name",
                                                customerReceipt.BankName);
                                        }
                                    }

                                    #endregion


                                    #region Electronic Payment

                                    else if (
                                        customerReceipt.PaymentMode ==
                                            PaymentMode.NEFT

                                        ||

                                        customerReceipt.PaymentMode ==
                                            PaymentMode.RTGS

                                        ||

                                        customerReceipt.PaymentMode ==
                                            PaymentMode.IMPS

                                        ||

                                        customerReceipt.PaymentMode ==
                                            PaymentMode.UPI

                                        ||

                                        customerReceipt.PaymentMode ==
                                            PaymentMode.BankTransfer
                                    )
                                    {
                                        AddDetailCell(
                                            table,
                                            "Reference / Transaction No.",
                                            customerReceipt.ReferenceNumber);


                                        if (!string.IsNullOrWhiteSpace(
                                            customerReceipt.BankName))
                                        {
                                            AddDetailCell(
                                                table,
                                                "Bank Name",
                                                customerReceipt.BankName);
                                        }
                                    }

                                    #endregion


                                    #region Other

                                    else if (customerReceipt.PaymentMode ==
                                             PaymentMode.Other)
                                    {
                                        if (!string.IsNullOrWhiteSpace(
                                            customerReceipt.ReferenceNumber))
                                        {
                                            AddDetailCell(
                                                table,
                                                "Reference / Transaction No.",
                                                customerReceipt.ReferenceNumber);
                                        }


                                        if (!string.IsNullOrWhiteSpace(
                                            customerReceipt.BankName))
                                        {
                                            AddDetailCell(
                                                table,
                                                "Bank Name",
                                                customerReceipt.BankName);
                                        }
                                    }

                                    #endregion


                                    #region Finalized On

                                    if (customerReceipt.FinalizedOn.HasValue)
                                    {
                                        AddDetailCell(
                                            table,
                                            "Finalized On",
                                            customerReceipt.FinalizedOn.Value
                                                .ToString(
                                                    "dd-MM-yyyy HH:mm"));
                                    }

                                    #endregion
                                });

                        #endregion
                    });
        }

        #endregion


        #region Allocation Table

        private static void ComposeAllocationTable(
            IContainer container,
            CustomerReceipt customerReceipt)
        {
            var allocations =
                customerReceipt.Allocations
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            container.Table(
                table =>
                {
                    #region Columns

                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.ConstantColumn(
                                25);             // Sr.


                            columns.RelativeColumn(
                                1.6f);           // Invoice


                            columns.RelativeColumn(
                                1.1f);           // Date


                            columns.RelativeColumn(
                                1.3f);           // Invoice Amount


                            columns.RelativeColumn(
                                1.3f);           // Already Received


                            columns.RelativeColumn(
                                1.3f);           // Outstanding


                            columns.RelativeColumn(
                                1.3f);           // Allocated


                            columns.RelativeColumn(
                                1.3f);           // Balance
                        });

                    #endregion


                    #region Header

                    table.Header(
                        header =>
                        {
                            AddHeaderCell(
                                header,
                                "Sr.");


                            AddHeaderCell(
                                header,
                                "Invoice No.");


                            AddHeaderCell(
                                header,
                                "Invoice Date");


                            AddHeaderCell(
                                header,
                                "Invoice Amount");


                            AddHeaderCell(
                                header,
                                "Already Received");


                            AddHeaderCell(
                                header,
                                "Outstanding");


                            AddHeaderCell(
                                header,
                                "Received");


                            AddHeaderCell(
                                header,
                                "Balance");
                        });

                    #endregion


                    #region Rows

                    var serialNumber =
                        1;


                    foreach (var allocation
                        in allocations)
                    {
                        var outstandingBeforeReceipt =
                            allocation.InvoiceGrandTotal -
                            allocation.AlreadyReceivedAmount;


                        if (outstandingBeforeReceipt < 0)
                        {
                            outstandingBeforeReceipt =
                                0;
                        }


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                serialNumber.ToString());


                        table.Cell()
                            .Element(
                                BodyCell)
                            .Text(
                                DisplayValue(
                                    allocation.InvoiceCode));


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignCenter()
                            .Text(
                                allocation.InvoiceDate
                                    .ToString(
                                        "dd-MM-yyyy"));


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                allocation.InvoiceGrandTotal
                                    .ToString(
                                        "0.00"));


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                allocation.AlreadyReceivedAmount
                                    .ToString(
                                        "0.00"));


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                outstandingBeforeReceipt
                                    .ToString(
                                        "0.00"));


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                allocation.AllocatedAmount
                                    .ToString(
                                        "0.00"));


                        table.Cell()
                            .Element(
                                BodyCell)
                            .AlignRight()
                            .Text(
                                allocation.BalanceAfterReceipt
                                    .ToString(
                                        "0.00"));


                        serialNumber++;
                    }

                    #endregion
                });
        }


        private static void AddHeaderCell(
    TableCellDescriptor header,
    string text)
        {
            header.Cell()
                .Element(
                    HeaderCell)
                .AlignCenter()
                .Text(
                    text)
                .SemiBold()
                .FontSize(
                    8);
        }


        private static IContainer HeaderCell(
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


        #region Total Received

        private static void ComposeTotalReceived(
            IContainer container,
            CustomerReceipt customerReceipt)
        {
            container
                .Border(
                    1)
                .Background(
                    Colors.Grey.Lighten3)
                .Padding(
                    6)
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .Text(
                                "TOTAL RECEIVED")
                            .Bold();


                        row.ConstantItem(
                                130)
                            .AlignRight()
                            .Text(
                                customerReceipt
                                    .TotalReceivedAmount
                                    .ToString(
                                        "0.00"))
                            .Bold();
                    });
        }

        #endregion


        #region Amount In Words

        private static void ComposeAmountInWords(
            IContainer container,
            decimal amount)
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
                                    amount))
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


            if (number == 0)
            {
                return "Zero";
            }


            if (number < 0)
            {
                return
                    "Minus " +
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
                words.Add(
                    units[
                        (int)(
                            number /
                            100)]
                    +
                    " Hundred");


                number %=
                    100;
            }

            #endregion


            #region Tens / Units

            if (number >= 20)
            {
                words.Add(
                    tens[
                        (int)(
                            number /
                            10)]);


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
            CustomerReceipt customerReceipt)
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
                                            $"For {DisplayValue(customerReceipt.CompanyName)}")
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


        #region Detail Cell Helper

        private static void AddDetailCell(
            TableDescriptor table,
            string label,
            string? value)
        {
            table.Cell()
                .Padding(
                    4)
                .Text(
                    text =>
                    {
                        text.Span(
                                $"{label}: ")
                            .SemiBold();


                        text.Span(
                            DisplayValue(
                                value));
                    });
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

        #endregion
    }
}