/*
============================================================
File: IDeliveryChallanPdfGenerator.cs

Purpose:
Defines PDF generation contract for Delivery Challan.

Responsibilities:
- Generate Delivery Challan PDF from saved
  Delivery Challan snapshot data.
- Return generated PDF as byte array.

Important:
- PDF generation implementation belongs to Infrastructure.
- Application Service calls this interface only.
- PDF must use saved Finalized Challan snapshot data.
- No database access belongs in PDF generator.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IDeliveryChallanPdfGenerator
    {
        #region Generate

        byte[] Generate(
            DeliveryChallan deliveryChallan);

        #endregion
    }
}