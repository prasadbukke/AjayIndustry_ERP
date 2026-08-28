/*
============================================================
File: PurchaseInvoiceDetailsViewModel.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
ViewModel used by Purchase Invoice Details screen.

Responsibilities:
- Display internal Purchase Invoice information.
- Display Supplier's original Invoice information.
- Display Purchase Order reference.
- Display Supplier / Company snapshot information.
- Display Payment Terms and Due Date.
- Display GRN-wise billed material traceability.
- Display GST and financial totals.
- Display workflow / finalization information.

Important:
- This is display-only ViewModel.
- Purchase Invoice Details must show exact relationship:
      Purchase Order
      → GRN
      → Purchase Invoice.
============================================================
*/

namespace AjayIndustriesERP.Web.ViewModels.PurchaseInvoice
{
    public class PurchaseInvoiceDetailsViewModel
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


        public DateTime PurchaseInvoiceDate
        {
            get;
            set;
        }


        public string Status
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Supplier Invoice

        public string SupplierInvoiceNumber
        {
            get;
            set;
        } = string.Empty;


        public DateTime SupplierInvoiceDate
        {
            get;
            set;
        }

        #endregion


        #region Purchase Order

        public int PurchaseOrderId
        {
            get;
            set;
        }


        public string PurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Supplier

        public int SupplierId
        {
            get;
            set;
        }


        public string SupplierName
        {
            get;
            set;
        } = string.Empty;


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


        public string? SupplierPan
        {
            get;
            set;
        }


        public string? SupplierContactPerson
        {
            get;
            set;
        }


        public string? SupplierMobileNumber
        {
            get;
            set;
        }


        public string? SupplierEmail
        {
            get;
            set;
        }


        public string? SupplierAddress
        {
            get;
            set;
        }


        public string? SupplierState
        {
            get;
            set;
        }

        #endregion


        #region Company

        public int CompanyId
        {
            get;
            set;
        }


        public string CompanyName
        {
            get;
            set;
        } = string.Empty;


        public string? CompanyGstin
        {
            get;
            set;
        }


        public string? CompanyPan
        {
            get;
            set;
        }


        public string? CompanyAddress
        {
            get;
            set;
        }


        public string? CompanyState
        {
            get;
            set;
        }


        public string? CompanyPhone
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


        public string GstType
        {
            get
            {
                return IsInterState
                    ? "IGST"
                    : "CGST + SGST";
            }
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


        public decimal TransportCharges
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


        #region Audit

        public DateTime CreatedOn
        {
            get;
            set;
        }


        public string? CreatedBy
        {
            get;
            set;
        }


        public DateTime? ModifiedOn
        {
            get;
            set;
        }


        public string? ModifiedBy
        {
            get;
            set;
        }

        #endregion


        #region Items

        public List<PurchaseInvoiceDetailsItemViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion
    }


    /*
    ============================================================
    Purchase Invoice Details Item
    ============================================================
    */

    public class PurchaseInvoiceDetailsItemViewModel
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


        #region Purchase Order Source

        public int PurchaseOrderItemId
        {
            get;
            set;
        }


        public string PurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;


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


        public string GoodsReceiptNoteCode
        {
            get;
            set;
        } = string.Empty;


        public DateTime? GoodsReceiptNoteDate
        {
            get;
            set;
        }


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


        public string? MaterialStatus
        {
            get;
            set;
        }

        #endregion


        #region Item

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


        #region Quantity

        public decimal PurchaseInvoiceQuantity
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
    }
}