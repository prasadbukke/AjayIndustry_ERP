/*
============================================================
File: PurchaseInvoiceFormViewModel.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
ViewModel used by Purchase Invoice Create / Edit screens.

Important:
- Browser-posted financial/source snapshot values are NOT
  trusted by PurchaseInvoiceService.
- GoodsReceiptNoteItemId + PurchaseInvoiceQuantity are the
  important transaction inputs.
- Rate / GST / Item / GRN / PO information is displayed
  to the user but rebuilt from trusted database records.
- User can select only the GRN lines included in the
  Supplier Invoice.
============================================================
*/

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.PurchaseInvoice
{
    public class PurchaseInvoiceFormViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        public string? Code
        {
            get;
            set;
        }

        #endregion


        #region Purchase Order

        [Display(
            Name = "Purchase Order")]
        public int PurchaseOrderId
        {
            get;
            set;
        }


        public string? PurchaseOrderCode
        {
            get;
            set;
        }


        public List<SelectListItem>
            AvailablePurchaseOrders
        {
            get;
            set;
        } = new();

        #endregion


        #region Purchase Invoice Dates

        [Required]
        [DataType(
            DataType.Date)]
        [Display(
            Name = "Purchase Invoice Date")]
        public DateTime PurchaseInvoiceDate
        {
            get;
            set;
        } = DateTime.Today;


        [Required]
        [DataType(
            DataType.Date)]
        [Display(
            Name = "Supplier Invoice Date")]
        public DateTime SupplierInvoiceDate
        {
            get;
            set;
        } = DateTime.Today;

        #endregion


        #region Supplier Invoice

        [Required]
        [StringLength(
            100)]
        [Display(
            Name = "Supplier Invoice No.")]
        public string SupplierInvoiceNumber
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Supplier Display

        public int SupplierId
        {
            get;
            set;
        }


        [Display(
            Name = "Supplier")]
        public string? SupplierName
        {
            get;
            set;
        }


        public string? SupplierCode
        {
            get;
            set;
        }


        public string? SupplierGstin
        {
            get;
            set;
        }


        public string? SupplierState
        {
            get;
            set;
        }


        public string? SupplierAddress
        {
            get;
            set;
        }

        #endregion


        #region Company Display

        public int CompanyId
        {
            get;
            set;
        }


        public string? CompanyName
        {
            get;
            set;
        }


        public string? CompanyGstin
        {
            get;
            set;
        }


        public string? CompanyState
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


        [DataType(
            DataType.Date)]
        public DateTime? DueDate
        {
            get;
            set;
        }

        #endregion


        #region GST

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


        #region Charges

        [Range(
            typeof(decimal),
            "0",
            "999999999999.99")]
        [Display(
            Name = "Transport Charges")]
        public decimal TransportCharges
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999.99")]
        [Display(
            Name = "Other Charges")]
        public decimal OtherCharges
        {
            get;
            set;
        }


        /*
         * Supplier Invoice Round Off can be
         * positive or negative.
         */
        [Display(
            Name = "Round Off")]
        public decimal RoundOffAmount
        {
            get;
            set;
        }

        #endregion


        #region Totals

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


        public decimal GrandTotal
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        [StringLength(
            2000)]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Status

        public string? Status
        {
            get;
            set;
        }

        #endregion


        #region Items

        public List<PurchaseInvoiceFormItemViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Purchase Invoice Form Item
    ============================================================
    */

    public class PurchaseInvoiceFormItemViewModel
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


        #region Selection

        /*
         * Supplier Invoice may contain only some of the
         * received GRN lines.
         *
         * Therefore user can include / exclude individual
         * received lines from the Purchase Invoice.
         */
        public bool IsSelected
        {
            get;
            set;
        } = true;

        #endregion


        #region Purchase Order Source

        public int PurchaseOrderItemId
        {
            get;
            set;
        }


        public string? PurchaseOrderCode
        {
            get;
            set;
        }


        public decimal PurchaseOrderQuantity
        {
            get;
            set;
        }

        #endregion


        #region GRN Source

        public int GoodsReceiptNoteId
        {
            get;
            set;
        }


        public string? GoodsReceiptNoteCode
        {
            get;
            set;
        }


        public DateTime? GoodsReceiptNoteDate
        {
            get;
            set;
        }


        /*
         * Primary trusted source Id.
         */
        public int GoodsReceiptNoteItemId
        {
            get;
            set;
        }


        public decimal GoodsReceiptQuantity
        {
            get;
            set;
        }


        public string? SupplierChallanNumber
        {
            get;
            set;
        }


        public DateTime? SupplierChallanDate
        {
            get;
            set;
        }

        #endregion


        #region Quantity Availability

        /*
         * Quantity already consumed by OTHER active
         * Purchase Invoices.
         *
         * During Edit, current Purchase Invoice is excluded.
         */
        public decimal AlreadyBilledQuantity
        {
            get;
            set;
        }


        /*
         * Current quantity available to this form.
         */
        public decimal AvailableQuantity
        {
            get;
            set;
        }


        [Range(
            typeof(decimal),
            "0",
            "999999999999.999")]
        [Display(
            Name = "Invoice Qty")]
        public decimal PurchaseInvoiceQuantity
        {
            get;
            set;
        }

        #endregion


        #region Item Snapshot

        public int ItemId
        {
            get;
            set;
        }


        public string? ItemCode
        {
            get;
            set;
        }


        public string? ItemName
        {
            get;
            set;
        }


        public string? Description
        {
            get;
            set;
        }


        public string? Specification
        {
            get;
            set;
        }


        public string? UnitName
        {
            get;
            set;
        }


        public string? HsnCode
        {
            get;
            set;
        }

        #endregion


        #region Drawing

        public int? DrawingId
        {
            get;
            set;
        }


        public string? DrawingNumber
        {
            get;
            set;
        }


        public string? DrawingRevision
        {
            get;
            set;
        }

        #endregion


        #region Commercial

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


        #region Total

        public decimal LineTotal
        {
            get;
            set;
        }

        #endregion


        #region Material Status Display

        /*
         * Informational only in current Phase.
         *
         * Approved / Rejected / Failure / Return does not yet
         * change Purchase Invoice eligibility because GRN
         * material-effect workflow is not implemented.
         */
        public string? MaterialStatus
        {
            get;
            set;
        }

        #endregion
    }
}