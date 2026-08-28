/*
============================================================
File: IInvoicePdfGenerator.cs

Module:
Invoice

Purpose:
Defines Invoice PDF generation contract.

Responsibilities:
- Generate customer-facing Invoice PDF.
- Accept finalized Invoice domain data.
- Return PDF as byte array.

Important:
- PDF formatting belongs to Infrastructure.
- InvoiceService decides whether Invoice is eligible
  for PDF generation.
- Generator must use saved Invoice snapshots and must not
  reload current Customer / Company Master data.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInvoicePdfGenerator
    {
        #region Generate PDF

        byte[] Generate(
            Invoice invoice);

        #endregion
    }
}