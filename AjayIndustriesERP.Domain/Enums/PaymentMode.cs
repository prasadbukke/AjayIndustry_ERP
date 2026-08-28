/*
============================================================
File: PaymentMode.cs

Module:
Customer Receipt

Purpose:
Defines supported customer payment modes.

Important:
- Payment-mode-specific details such as Cheque No.,
  UTR / Transaction Reference and Bank Name are stored
  separately in CustomerReceipt.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum PaymentMode
    {
        Cash = 1,

        Cheque = 2,

        NEFT = 3,

        RTGS = 4,

        IMPS = 5,

        UPI = 6,

        BankTransfer = 7,

        Other = 8
    }
}