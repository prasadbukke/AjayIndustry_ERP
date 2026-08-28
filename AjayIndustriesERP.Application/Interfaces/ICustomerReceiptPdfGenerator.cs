/*
============================================================
File: ICustomerReceiptPdfGenerator.cs

Module:
Customer Receipt

Purpose:
Defines PDF generation contract for
Finalized Customer Receipt.

Responsibilities:
- Generate Customer Receipt PDF.
- Use saved Customer / Company snapshots.
- Display payment information.
- Display Invoice allocation details.
- Display received amount and amount in words.

Important:
- PDF generator receives a trusted CustomerReceipt
  entity prepared by the service layer.
- Historical snapshot values must be preferred over
  live master data wherever applicable.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerReceiptPdfGenerator
    {
        byte[] Generate(
            CustomerReceipt customerReceipt);
    }
}