/*
============================================================
File: InvoiceFormViewModel.cs

Module:
Invoice

Purpose:
Represents Invoice Create / Edit form data.

Responsibilities:
- Display Invoice header information.
- Select Customer Purchase Order.
- Display Completed Production Jobs for selected Customer PO.
- Display Customer Master snapshot information.
- Allow Billing Address editing.
- Display Company / Workshop snapshot information.
- Display ISO and Bank Details.
- Display Payment Terms / Credit Days.
- Accept Invoice Quantity, Rate, Discount and GST inputs.
- Display calculated financial preview values.
- Handle PDI / Delivery Challan warning confirmation.
- Accept Invoice Terms and Remarks.

Important:
- New Invoice source flow:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory.
- PDI is NOT mandatory.
- Missing PDI / Delivery Challan is warning-only.
- Customer / Company display fields are read-only.
- Billing Address is editable while Invoice is Draft.
- Production / PO snapshot fields posted by browser
  are not trusted by server.
- Rate, Discount %, GST % and Invoice Quantity are
  commercial user inputs.
- Financial totals are finally calculated by InvoiceService.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.Invoice
{
    public class InvoiceFormViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        [Display(Name = "Invoice No.")]
        public string? Code
        {
            get;
            set;
        }


        [Display(Name = "Invoice Date")]
        [DataType(DataType.Date)]
        [Required(
            ErrorMessage = "Invoice Date is required.")]
        public DateTime InvoiceDate
        {
            get;
            set;
        } = DateTime.Today;


        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate
        {
            get;
            set;
        }


        public InvoiceStatus Status
        {
            get;
            set;
        } = InvoiceStatus.Draft;

        #endregion


        #region Customer Purchase Order Selection

        /*
         * UI selection only.
         *
         * InvoiceService does not trust this value as
         * authoritative source.
         *
         * Selected Production Jobs are revalidated and
         * their Customer PO ownership is verified by
         * InvoiceService.
         */
        [Display(Name = "Customer Purchase Order")]
        public int? CustomerPurchaseOrderId
        {
            get;
            set;
        }


        public List<SelectListItem>
            AvailableCustomerPurchaseOrders
        {
            get;
            set;
        } = new();

        #endregion


        #region Source Warning Confirmation

        /*
         * Used when one or more selected Production Jobs
         * do not have:
         *
         * - Finalized PDI, or
         * - Delivery Challan.
         *
         * PDI / Challan are NOT mandatory.
         * Explicit confirmation allows Invoice submission.
         */
        [Display(
            Name =
                "Continue even if PDI / Delivery Challan is not completed")]
        public bool ConfirmSourceWarning
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


        [Display(Name = "Customer")]
        public string? CustomerName
        {
            get;
            set;
        }

        #endregion


        #region Customer Master Information

        [Display(Name = "Customer Code")]
        public string? CustomerCode
        {
            get;
            set;
        }


        [Display(Name = "GSTIN")]
        public string? CustomerGstin
        {
            get;
            set;
        }


        [Display(Name = "PAN")]
        public string? CustomerPan
        {
            get;
            set;
        }

        #endregion


        #region Billing Address

        [Display(Name = "Address Line 1")]
        [StringLength(
            500,
            ErrorMessage =
                "Address Line 1 cannot exceed 500 characters.")]
        public string? BillingAddressLine1
        {
            get;
            set;
        }


        [Display(Name = "Address Line 2")]
        [StringLength(
            500,
            ErrorMessage =
                "Address Line 2 cannot exceed 500 characters.")]
        public string? BillingAddressLine2
        {
            get;
            set;
        }


        [Display(Name = "City")]
        [StringLength(
            150,
            ErrorMessage =
                "City cannot exceed 150 characters.")]
        public string? BillingCity
        {
            get;
            set;
        }


        [Display(Name = "District")]
        [StringLength(
            150,
            ErrorMessage =
                "District cannot exceed 150 characters.")]
        public string? BillingDistrict
        {
            get;
            set;
        }


        [Display(Name = "State")]
        [StringLength(
            150,
            ErrorMessage =
                "State cannot exceed 150 characters.")]
        public string? BillingState
        {
            get;
            set;
        }


        [Display(Name = "Pincode")]
        [StringLength(
            20,
            ErrorMessage =
                "Pincode cannot exceed 20 characters.")]
        public string? BillingPincode
        {
            get;
            set;
        }


        [Display(Name = "Country")]
        [StringLength(
            100,
            ErrorMessage =
                "Country cannot exceed 100 characters.")]
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


        [Display(Name = "Company / Workshop")]
        public string? CompanyName
        {
            get;
            set;
        }


        [Display(Name = "Company Code")]
        public string? CompanyCode
        {
            get;
            set;
        }


        [Display(Name = "GSTIN")]
        public string? CompanyGstNumber
        {
            get;
            set;
        }


        [Display(Name = "PAN")]
        public string? CompanyPanNumber
        {
            get;
            set;
        }


        [Display(Name = "ISO Certification No.")]
        public string? CompanyIsoCertificationNumber
        {
            get;
            set;
        }


        [Display(Name = "Address")]
        public string? CompanyAddress
        {
            get;
            set;
        }


        [Display(Name = "City")]
        public string? CompanyCity
        {
            get;
            set;
        }


        [Display(Name = "State")]
        public string? CompanyState
        {
            get;
            set;
        }


        [Display(Name = "Postal Code")]
        public string? CompanyPostalCode
        {
            get;
            set;
        }


        [Display(Name = "Country")]
        public string? CompanyCountry
        {
            get;
            set;
        }


        [Display(Name = "Phone")]
        public string? CompanyPhoneNumber
        {
            get;
            set;
        }


        [Display(Name = "Email")]
        public string? CompanyEmail
        {
            get;
            set;
        }

        #endregion


        #region Company Bank Details

        [Display(Name = "Bank Name")]
        public string? BankName
        {
            get;
            set;
        }


        [Display(Name = "Account Holder Name")]
        public string? BankAccountHolderName
        {
            get;
            set;
        }


        [Display(Name = "Account Number")]
        public string? BankAccountNumber
        {
            get;
            set;
        }


        [Display(Name = "IFSC Code")]
        public string? BankIfscCode
        {
            get;
            set;
        }


        [Display(Name = "Branch Name")]
        public string? BankBranchName
        {
            get;
            set;
        }


        [Display(Name = "Account Type")]
        public string? BankAccountType
        {
            get;
            set;
        }

        #endregion


        #region Payment Information

        [Display(Name = "Payment Terms")]
        public string? PaymentTerms
        {
            get;
            set;
        }


        [Display(Name = "Credit Days")]
        public int? CreditDays
        {
            get;
            set;
        }

        #endregion


        #region GST Information

        [Display(Name = "Place of Supply")]
        public string? PlaceOfSupply
        {
            get;
            set;
        }


        [Display(Name = "Inter-State Transaction")]
        public bool IsInterState
        {
            get;
            set;
        }

        #endregion


        #region Other Charges

        [Display(Name = "Other Charges")]
        [Range(
            typeof(decimal),
            "0",
            "999999999999999.99",
            ErrorMessage =
                "Other Charges cannot be negative.")]
        public decimal OtherCharges
        {
            get;
            set;
        }

        #endregion


        #region Financial Totals

        [Display(Name = "Gross Amount")]
        public decimal GrossAmount
        {
            get;
            set;
        }


        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount
        {
            get;
            set;
        }


        [Display(Name = "Taxable Amount")]
        public decimal TaxableAmount
        {
            get;
            set;
        }


        [Display(Name = "CGST")]
        public decimal CgstAmount
        {
            get;
            set;
        }


        [Display(Name = "SGST")]
        public decimal SgstAmount
        {
            get;
            set;
        }


        [Display(Name = "IGST")]
        public decimal IgstAmount
        {
            get;
            set;
        }


        [Display(Name = "Round Off")]
        public decimal RoundOffAmount
        {
            get;
            set;
        }


        [Display(Name = "Grand Total")]
        public decimal GrandTotal
        {
            get;
            set;
        }

        #endregion


        #region Terms And Conditions

        [Display(Name = "Invoice Terms & Conditions")]
        [StringLength(
            4000,
            ErrorMessage =
                "Invoice Terms & Conditions cannot exceed 4000 characters.")]
        public string? InvoiceTermsAndConditions
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        [Display(Name = "Remarks")]
        [StringLength(
            2000,
            ErrorMessage =
                "Remarks cannot exceed 2000 characters.")]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Invoice Items

        public List<InvoiceItemFormViewModel> Items
        {
            get;
            set;
        } = new();

        #endregion


        #region Temporary Controller Compatibility

        /*
         * Temporary compatibility alias.
         *
         * Current InvoiceController was changed before this
         * ViewModel and still assigns Customer PO options to:
         *
         *     AvailableDeliveryChallans
         *
         * Both properties point to the SAME list.
         *
         * Once Controller + Create/Edit View are moved fully
         * to AvailableCustomerPurchaseOrders this alias can
         * be removed.
         *
         * This is NOT Delivery Challan business logic.
         */
        public List<SelectListItem> AvailableDeliveryChallans
        {
            get
            {
                return AvailableCustomerPurchaseOrders;
            }

            set
            {
                AvailableCustomerPurchaseOrders =
                    value ?? new List<SelectListItem>();
            }
        }

        #endregion
    }


    /*
    ============================================================
    InvoiceItemFormViewModel

    Purpose:
    Represents one Invoice line on Create / Edit screen.

    Important:
    - ProductionJobId is the authoritative source reference.
    - Customer PO / Product snapshot fields are read-only.
    - Delivery Challan fields are optional historical values.
    - InvoiceQuantity, Rate, DiscountPercent and GstRate
      are editable.
    - Calculated fields are display-only previews.
    - InvoiceService recalculates all amounts.
    ============================================================
    */

    public class InvoiceItemFormViewModel
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

        [Required(
            ErrorMessage =
                "Production Job is required.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Invalid Production Job.")]
        public int? ProductionJobId
        {
            get;
            set;
        }


        [Display(Name = "Production Job")]
        public string? ProductionJobCode
        {
            get;
            set;
        }


        /*
         * Total trusted Completed Production quantity
         * for this Production Job.
         */
        [Display(Name = "Production Qty")]
        public decimal ProductionQuantity
        {
            get;
            set;
        }


        [Display(Name = "Already Invoiced")]
        public decimal AlreadyInvoicedQuantity
        {
            get;
            set;
        }


        [Display(Name = "Available Qty")]
        public decimal AvailableQuantity
        {
            get;
            set;
        }

        #endregion


        #region Source Warning Display

        /*
         * True when either Finalized PDI or
         * Delivery Challan is missing.
         *
         * Display helper only.
         * Final validation is performed by InvoiceService.
         */
        public bool RequiresSourceWarning
        {
            get;
            set;
        }

        #endregion


        #region Product / Item Snapshot

        [Display(Name = "Product ID")]
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


        [Display(Name = "Item Code")]
        public string? ItemCode
        {
            get;
            set;
        }


        [Display(Name = "Item / Product")]
        public string? ItemName
        {
            get;
            set;
        }


        [Display(Name = "Part No.")]
        public string? PartNumber
        {
            get;
            set;
        }


        [Display(Name = "Customer Item Code")]
        public string? CustomerItemCode
        {
            get;
            set;
        }


        [Display(Name = "UOM")]
        public string? UnitName
        {
            get;
            set;
        }


        [Display(Name = "HSN No.")]
        public string? HsnNumber
        {
            get;
            set;
        }

        #endregion


        #region Customer PO Snapshot

        public int? CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        [Display(Name = "Customer PO")]
        public string? CustomerPurchaseOrderCode
        {
            get;
            set;
        }


        [Display(Name = "Customer PO No.")]
        public string? CustomerPurchaseOrderNumber
        {
            get;
            set;
        }

        #endregion


        #region Optional Delivery Challan History

        /*
         * These fields remain only for historical Invoice
         * compatibility.
         *
         * New Invoice creation does NOT require or trust
         * these fields.
         */

        public int? DeliveryChallanId
        {
            get;
            set;
        }


        [Display(Name = "Delivery Challan")]
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


        [Display(Name = "DC Quantity")]
        public decimal? DeliveryChallanQuantity
        {
            get;
            set;
        }

        #endregion


        #region Invoice Quantity

        [Display(Name = "Invoice Qty")]
        [Required(
            ErrorMessage =
                "Invoice Quantity is required.")]
        [Range(
            typeof(decimal),
            "0.001",
            "999999999999999.999",
            ErrorMessage =
                "Invoice Quantity must be greater than zero.")]
        public decimal InvoiceQuantity
        {
            get;
            set;
        }

        #endregion


        #region Rate

        [Display(Name = "Rate")]
        [Required(
            ErrorMessage =
                "Rate is required.")]
        [Range(
            typeof(decimal),
            "0.0001",
            "99999999999999.9999",
            ErrorMessage =
                "Rate must be greater than zero.")]
        public decimal Rate
        {
            get;
            set;
        }


        [Display(Name = "Gross Amount")]
        public decimal GrossAmount
        {
            get;
            set;
        }

        #endregion


        #region Discount

        [Display(Name = "Discount %")]
        [Range(
            typeof(decimal),
            "0",
            "100",
            ErrorMessage =
                "Discount must be between 0 and 100%.")]
        public decimal DiscountPercent
        {
            get;
            set;
        }


        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount
        {
            get;
            set;
        }


        [Display(Name = "Taxable Amount")]
        public decimal TaxableAmount
        {
            get;
            set;
        }

        #endregion


        #region GST

        [Display(Name = "GST %")]
        [Range(
            typeof(decimal),
            "0",
            "100",
            ErrorMessage =
                "GST must be between 0 and 100%.")]
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

        [Display(Name = "Line Total")]
        public decimal LineTotal
        {
            get;
            set;
        }

        #endregion
    }
}