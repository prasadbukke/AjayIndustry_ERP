/*
============================================================
File: Invoice.cs

Module:
Invoice

Purpose:
Represents customer Invoice header.

Responsibilities:
- Store Invoice identification and workflow status.
- Store Customer historical snapshot.
- Store editable Billing Address snapshot.
- Store Company / Workshop historical snapshot.
- Store payment terms and due date.
- Store GST / invoice financial totals.
- Store Invoice Terms & Conditions snapshot.
- Maintain Invoice line items.
- Maintain finalization information.

Important:
- Invoice is created from one or more Finalized
  Delivery Challans of the same Customer.
- CustomerSnapshotJson freezes Customer Master values.
- CompanySnapshotJson freezes Company Master values,
  including ISO and Bank Details.
- Billing Address is stored separately because it may be
  edited for a specific Invoice.
- InvoiceTermsAndConditions is copied from Company Master
  at Invoice creation and can be edited while Draft.
- Finalized Invoice must never refresh Master snapshots.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Invoice
        : BaseEntity
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        /// <summary>
        /// Example:
        /// AI/INV/26-27/00001
        /// </summary>
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


        #region Customer Reference

        public int CustomerId
        {
            get;
            set;
        }


        /// <summary>
        /// Customer Name snapshot for convenient display.
        /// </summary>
        public string CustomerName
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// Generic scalar snapshot of Customer Master.
        ///
        /// Includes:
        /// Code, GSTIN, PAN, Contact, Payment Terms,
        /// Credit Days and other scalar fields.
        /// </summary>
        public string? CustomerSnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Billing Address Snapshot

        /*
         * Auto-loaded from Customer Master during Create.
         *
         * These fields remain editable while Invoice is Draft.
         */

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


        #region Company / Workshop Snapshot

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


        /// <summary>
        /// Generic scalar snapshot of Company Master.
        ///
        /// Includes:
        /// Company address
        /// GST / PAN
        /// ISO Certification
        /// Bank Details
        /// Invoice Terms source
        /// Contact details
        /// and future scalar Company fields.
        /// </summary>
        public string? CompanySnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Payment Information

        /// <summary>
        /// Customer Payment Terms snapshot.
        /// Example:
        /// 30 Days
        /// Advance
        /// Against Delivery
        /// </summary>
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

        /// <summary>
        /// Place of Supply displayed on Invoice.
        /// Usually based on Customer Billing State.
        /// </summary>
        public string? PlaceOfSupply
        {
            get;
            set;
        }


        /// <summary>
        /// True when Company State and Customer Billing State
        /// are different and IGST applies.
        ///
        /// False means intra-state transaction where
        /// CGST + SGST normally apply.
        /// </summary>
        public bool IsInterState
        {
            get;
            set;
        }

        #endregion


        #region Financial Totals

        /// <summary>
        /// Sum of line amounts before discount and GST.
        /// </summary>
        public decimal GrossAmount
        {
            get;
            set;
        }


        /// <summary>
        /// Total discount across all Invoice lines.
        /// </summary>
        public decimal DiscountAmount
        {
            get;
            set;
        }


        /// <summary>
        /// Total taxable value after discount.
        /// </summary>
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


        /// <summary>
        /// Optional packing, freight or other invoice charges.
        /// GST treatment will be handled by Invoice Service.
        /// </summary>
        public decimal OtherCharges
        {
            get;
            set;
        }


        /// <summary>
        /// Positive or negative final round-off adjustment.
        /// </summary>
        public decimal RoundOffAmount
        {
            get;
            set;
        }


        /// <summary>
        /// Final payable Invoice amount.
        /// </summary>
        public decimal GrandTotal
        {
            get;
            set;
        }

        #endregion


        #region Terms And Conditions

        /// <summary>
        /// Invoice Terms copied from Company Master when
        /// Invoice is created.
        ///
        /// Editable while Draft.
        /// Frozen after Finalization.
        /// </summary>
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


        #region Navigation

        public ICollection<InvoiceItem> Items
        {
            get;
            set;
        } = new List<InvoiceItem>();

        #endregion
    }
}