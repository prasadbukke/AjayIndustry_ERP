/*
============================================================
File: InvoiceDetailsViewModel.cs

Module:
Invoice

Purpose:
Represents read-only Invoice details screen data.

Responsibilities:
- Display Invoice header and workflow status.
- Display Customer snapshot information.
- Display Billing Address snapshot.
- Display Company / Workshop snapshot information.
- Display ISO and Bank Details.
- Display Payment Terms and Place of Supply.
- Display Invoice line source traceability.
- Display Invoice line financial values.
- Display GST split and header totals.
- Display Invoice Terms and Remarks.
- Display Finalization information.

Important:
- New Invoice source flow:
  Customer PO → Completed Production Job → Invoice.
- Production Job is the primary Invoice source reference.
- Customer PO information is preserved for traceability.
- Delivery Challan information is optional historical data.
- PDI / Delivery Challan are not mandatory Invoice sources.
- Values shown here come from saved Invoice snapshots.
- Current Customer / Company Master values must not
  replace historical Invoice data.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.Invoice
{
    public class InvoiceDetailsViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public string Code
        {
            get;
            set;
        } = string.Empty;


        public DateTime InvoiceDate
        {
            get;
            set;
        }


        public DateTime? DueDate
        {
            get;
            set;
        }


        public InvoiceStatus Status
        {
            get;
            set;
        }

        #endregion


        #region Customer Information

        public int CustomerId
        {
            get;
            set;
        }


        public string CustomerName
        {
            get;
            set;
        } = string.Empty;


        public string? CustomerCode
        {
            get;
            set;
        }


        public string? CustomerGstin
        {
            get;
            set;
        }


        public string? CustomerPan
        {
            get;
            set;
        }

        #endregion


        #region Billing Address

        public string? BillingAddressLine1
        {
            get;
            set;
        }


        public string? BillingAddressLine2
        {
            get;
            set;
        }


        public string? BillingCity
        {
            get;
            set;
        }


        public string? BillingDistrict
        {
            get;
            set;
        }


        public string? BillingState
        {
            get;
            set;
        }


        public string? BillingPincode
        {
            get;
            set;
        }


        public string? BillingCountry
        {
            get;
            set;
        }

        #endregion


        #region Company / Workshop Information

        public int? CompanyId
        {
            get;
            set;
        }


        public string? CompanyName
        {
            get;
            set;
        }


        public string? CompanyCode
        {
            get;
            set;
        }


        public string? CompanyGstNumber
        {
            get;
            set;
        }


        public string? CompanyPanNumber
        {
            get;
            set;
        }


        public string? CompanyIsoCertificationNumber
        {
            get;
            set;
        }


        public string? CompanyAddress
        {
            get;
            set;
        }


        public string? CompanyCity
        {
            get;
            set;
        }


        public string? CompanyState
        {
            get;
            set;
        }


        public string? CompanyPostalCode
        {
            get;
            set;
        }


        public string? CompanyCountry
        {
            get;
            set;
        }


        public string? CompanyPhoneNumber
        {
            get;
            set;
        }


        public string? CompanyEmail
        {
            get;
            set;
        }

        #endregion


        #region Bank Details

        public string? BankName
        {
            get;
            set;
        }


        public string? BankAccountHolderName
        {
            get;
            set;
        }


        public string? BankAccountNumber
        {
            get;
            set;
        }


        public string? BankIfscCode
        {
            get;
            set;
        }


        public string? BankBranchName
        {
            get;
            set;
        }


        public string? BankAccountType
        {
            get;
            set;
        }

        #endregion


        #region Payment Information

        public string? PaymentTerms
        {
            get;
            set;
        }


        public int? CreditDays
        {
            get;
            set;
        }

        #endregion


        #region GST Information

        public string? PlaceOfSupply
        {
            get;
            set;
        }


        public bool IsInterState
        {
            get;
            set;
        }

        #endregion


        #region Financial Totals

        public decimal GrossAmount
        {
            get;
            set;
        }


        public decimal DiscountAmount
        {
            get;
            set;
        }


        public decimal TaxableAmount
        {
            get;
            set;
        }


        public decimal CgstAmount
        {
            get;
            set;
        }


        public decimal SgstAmount
        {
            get;
            set;
        }


        public decimal IgstAmount
        {
            get;
            set;
        }


        public decimal OtherCharges
        {
            get;
            set;
        }


        public decimal RoundOffAmount
        {
            get;
            set;
        }


        public decimal GrandTotal
        {
            get;
            set;
        }

        #endregion


        #region Terms And Conditions

        public string? InvoiceTermsAndConditions
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Finalization

        public DateTime? FinalizedOn
        {
            get;
            set;
        }


        public string? FinalizedBy
        {
            get;
            set;
        }

        #endregion


        #region Items

        public List<InvoiceItemDetailsViewModel> Items
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    InvoiceItemDetailsViewModel

    Purpose:
    Represents one read-only Invoice line.

    Important:
    - ProductionJobId is the primary source reference.
    - Customer PO information provides commercial traceability.
    - Delivery Challan fields are optional historical data.
    - Displays saved Invoice financial snapshot.
    ============================================================
    */

    public class InvoiceItemDetailsViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public int SequenceNumber
        {
            get;
            set;
        }

        #endregion


        #region Production Job Source

        public int? ProductionJobId
        {
            get;
            set;
        }


        public string? ProductionJobCode
        {
            get;
            set;
        }

        #endregion


        #region Customer Purchase Order Traceability

        public int? CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderCode
        {
            get;
            set;
        }


        public string? CustomerPurchaseOrderNumber
        {
            get;
            set;
        }

        #endregion


        #region Product / Item Snapshot

        public string? ProductReference
        {
            get;
            set;
        }


        public int ItemId
        {
            get;
            set;
        }


        public string ItemCode
        {
            get;
            set;
        } = string.Empty;


        public string ItemName
        {
            get;
            set;
        } = string.Empty;


        public string? PartNumber
        {
            get;
            set;
        }


        public string? CustomerItemCode
        {
            get;
            set;
        }


        public string? UnitName
        {
            get;
            set;
        }


        public string? HsnNumber
        {
            get;
            set;
        }

        #endregion


        #region Optional Delivery Challan History

        /*
         * These fields are kept only for old / historical
         * Invoice records that originated from Delivery
         * Challan flow.
         *
         * New Invoice records do not require these values.
         */

        public int? DeliveryChallanId
        {
            get;
            set;
        }


        public string? DeliveryChallanCode
        {
            get;
            set;
        }


        public int? DeliveryChallanItemId
        {
            get;
            set;
        }


        public decimal? DeliveryChallanQuantity
        {
            get;
            set;
        }

        #endregion


        #region Quantity And Rate

        public decimal InvoiceQuantity
        {
            get;
            set;
        }


        public decimal Rate
        {
            get;
            set;
        }


        public decimal GrossAmount
        {
            get;
            set;
        }

        #endregion


        #region Discount

        public decimal DiscountPercent
        {
            get;
            set;
        }


        public decimal DiscountAmount
        {
            get;
            set;
        }


        public decimal TaxableAmount
        {
            get;
            set;
        }

        #endregion


        #region GST

        public decimal GstRate
        {
            get;
            set;
        }


        public decimal CgstRate
        {
            get;
            set;
        }


        public decimal SgstRate
        {
            get;
            set;
        }


        public decimal IgstRate
        {
            get;
            set;
        }


        public decimal CgstAmount
        {
            get;
            set;
        }


        public decimal SgstAmount
        {
            get;
            set;
        }


        public decimal IgstAmount
        {
            get;
            set;
        }


        public decimal TotalTaxAmount
        {
            get;
            set;
        }

        #endregion


        #region Line Total

        public decimal LineTotal
        {
            get;
            set;
        }

        #endregion
    }
}