/*
============================================================
File: DeliveryChallanFormViewModel.cs

Purpose:
ViewModel used by Delivery Challan Create and Edit screens.

Responsibilities:
- Hold Delivery Challan header information.
- Hold manually entered L.P.G. No.
- Hold Customer reference.
- Hold editable Customer delivery address.
- Display Company / Workshop Master information.
- Hold transport / dispatch information.
- Provide Finalized PDI selection.
- Display trusted PDI snapshot information.
- Accept manually entered Product ID.
- Accept manually entered HSN No.
- Accept Dispatch Quantity.
- Support multiple dispatch lines.

Important:
- Customer delivery address is auto-loaded from Customer
  Master but remains editable.
- Company / Workshop information is read-only display data.
- Historical Company / Customer master data is persisted
  through JSON snapshots in DeliveryChallan entity.
- PDI snapshot values posted from browser are not trusted.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AjayIndustriesERP.Web.ViewModels.DeliveryChallan
{
    public class DeliveryChallanFormViewModel
    {
        #region Identification

        public int Id
        {
            get;
            set;
        }


        [Display(Name = "Challan No.")]
        public string? Code
        {
            get;
            set;
        }


        [Required(
            ErrorMessage = "Challan Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Challan Date")]
        public DateTime ChallanDate
        {
            get;
            set;
        } = DateTime.Today;


        public DeliveryChallanStatus Status
        {
            get;
            set;
        } = DeliveryChallanStatus.Draft;

        #endregion


        #region LPG Information

        [StringLength(
            100,
            ErrorMessage =
                "L.P.G. No. cannot exceed 100 characters.")]
        [Display(Name = "L.P.G. No.")]
        public string? LpgNumber
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


        [Display(Name = "Customer")]
        public string? CustomerName
        {
            get;
            set;
        }

        #endregion

        #region Customer Master Information

        /*
         * Read-only projection of CustomerSnapshotJson.
         *
         * These values are auto-loaded from Customer Master.
         * They are NOT separately stored as Delivery Challan columns.
         *
         * Historical source remains CustomerSnapshotJson.
         */

        [Display(Name = "Customer Code")]
        public string? CustomerCode
        {
            get;
            set;
        }


        [Display(Name = "Legal Name")]
        public string? CustomerLegalName
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


        [Display(Name = "Contact Person")]
        public string? CustomerContactPerson
        {
            get;
            set;
        }


        [Display(Name = "Mobile No.")]
        public string? CustomerMobileNumber
        {
            get;
            set;
        }


        [Display(Name = "Alternate Mobile No.")]
        public string? CustomerAlternateMobileNumber
        {
            get;
            set;
        }


        [Display(Name = "Email")]
        public string? CustomerEmail
        {
            get;
            set;
        }


        [Display(Name = "Payment Terms")]
        public string? CustomerPaymentTerms
        {
            get;
            set;
        }


        [Display(Name = "Credit Days")]
        public int? CustomerCreditDays
        {
            get;
            set;
        }


        [Display(Name = "Website")]
        public string? CustomerWebsite
        {
            get;
            set;
        }


        [Display(Name = "Customer Remarks")]
        public string? CustomerMasterRemarks
        {
            get;
            set;
        }

        #endregion


        #region Customer Editable Address

        [StringLength(
            500,
            ErrorMessage =
                "Address Line 1 cannot exceed 500 characters.")]
        [Display(Name = "Address Line 1")]
        public string? CustomerAddressLine1
        {
            get;
            set;
        }


        [StringLength(
            500,
            ErrorMessage =
                "Address Line 2 cannot exceed 500 characters.")]
        [Display(Name = "Address Line 2")]
        public string? CustomerAddressLine2
        {
            get;
            set;
        }


        [StringLength(
            150,
            ErrorMessage =
                "City cannot exceed 150 characters.")]
        [Display(Name = "City")]
        public string? CustomerCity
        {
            get;
            set;
        }


        [StringLength(
            150,
            ErrorMessage =
                "District cannot exceed 150 characters.")]
        [Display(Name = "District")]
        public string? CustomerDistrict
        {
            get;
            set;
        }


        [StringLength(
            150,
            ErrorMessage =
                "State cannot exceed 150 characters.")]
        [Display(Name = "State")]
        public string? CustomerState
        {
            get;
            set;
        }


        [StringLength(
            20,
            ErrorMessage =
                "Pincode cannot exceed 20 characters.")]
        [Display(Name = "Pincode")]
        public string? CustomerPincode
        {
            get;
            set;
        }


        [StringLength(
            100,
            ErrorMessage =
                "Country cannot exceed 100 characters.")]
        [Display(Name = "Country")]
        public string? CustomerCountry
        {
            get;
            set;
        }

        #endregion


        #region Company Workshop Information

        /*
         * Read-only UI projection of CompanySnapshotJson.
         *
         * These are not separate Delivery Challan DB columns.
         */

        public int? CompanyId
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


        [Display(Name = "Company / Workshop")]
        public string? CompanyName
        {
            get;
            set;
        }


        [Display(Name = "GST No.")]
        public string? CompanyGstNumber
        {
            get;
            set;
        }


        [Display(Name = "PAN No.")]
        public string? CompanyPanNumber
        {
            get;
            set;
        }


        [Display(Name = "Phone No.")]
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


        [Display(Name = "Website")]
        public string? CompanyWebsite
        {
            get;
            set;
        }


        [Display(Name = "Contact Person")]
        public string? CompanyContactPerson
        {
            get;
            set;
        }


        [Display(Name = "Workshop Address")]
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


        [Display(Name = "Country")]
        public string? CompanyCountry
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


        [Display(Name = "Purchase Order Terms & Conditions")]
        public string? CompanyPurchaseOrderTermsAndConditions
        {
            get;
            set;
        }

        #endregion


        #region Dispatch Information

        [StringLength(
            250,
            ErrorMessage =
                "Transporter Name cannot exceed 250 characters.")]
        [Display(Name = "Transporter Name")]
        public string? TransporterName
        {
            get;
            set;
        }


        [StringLength(
            100,
            ErrorMessage =
                "Vehicle Number cannot exceed 100 characters.")]
        [Display(Name = "Vehicle No.")]
        public string? VehicleNumber
        {
            get;
            set;
        }


        [StringLength(
            150,
            ErrorMessage =
                "Transport Reference cannot exceed 150 characters.")]
        [Display(Name = "LR / Transport Reference")]
        public string? TransportReference
        {
            get;
            set;
        }


        [StringLength(
            250,
            ErrorMessage =
                "Dispatch From cannot exceed 250 characters.")]
        [Display(Name = "Dispatch From")]
        public string? DispatchFrom
        {
            get;
            set;
        }


        [StringLength(
            250,
            ErrorMessage =
                "Destination cannot exceed 250 characters.")]
        [Display(Name = "Destination")]
        public string? Destination
        {
            get;
            set;
        }

        #endregion


        #region Remarks

        [StringLength(
            2000,
            ErrorMessage =
                "Remarks cannot exceed 2000 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks
        {
            get;
            set;
        }

        #endregion


        #region Challan Items

        public List<DeliveryChallanItemFormViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion


        #region PDI Selection

        public List<SelectListItem>
            AvailablePdis
        {
            get;
            set;
        } = new();

        #endregion
    }


    public class DeliveryChallanItemFormViewModel
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


        #region PDI Source

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Finalized PDI Report is required.")]
        [Display(Name = "PDI Report")]
        public int PreDispatchInspectionId
        {
            get;
            set;
        }


        [Display(Name = "PDI No.")]
        public string? PreDispatchInspectionCode
        {
            get;
            set;
        }

        #endregion


        #region Production Job

        public int ProductionJobId
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

        #endregion


        #region Customer PO

        public int CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        [Display(Name = "Customer PO Code")]
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


        [Display(Name = "Customer Item Code")]
        public string? CustomerItemCode
        {
            get;
            set;
        }

        #endregion


        #region Item Information

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


        [Display(Name = "UOM")]
        public string? UnitName
        {
            get;
            set;
        }

        #endregion


        #region Product Reference

        [StringLength(
            100,
            ErrorMessage =
                "Product ID cannot exceed 100 characters.")]
        [Display(Name = "Product ID")]
        public string? ProductReference
        {
            get;
            set;
        }

        #endregion


        #region HSN Information

        [StringLength(
            50,
            ErrorMessage =
                "HSN No. cannot exceed 50 characters.")]
        [Display(Name = "HSN No.")]
        public string? HsnNumber
        {
            get;
            set;
        }

        #endregion


        #region Customer Drawing

        public int? CustomerDrawingId
        {
            get;
            set;
        }


        [Display(Name = "Customer Drawing No.")]
        public string? CustomerDrawingNumber
        {
            get;
            set;
        }


        [Display(Name = "Revision")]
        public string? CustomerDrawingRevision
        {
            get;
            set;
        }

        #endregion


        #region Quantity Information

        [Display(Name = "PDI Accepted Qty")]
        public decimal PdiAcceptedQuantity
        {
            get;
            set;
        }


        [Display(Name = "Already Dispatched Qty")]
        public decimal AlreadyDispatchedQuantity
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


        [Required(
            ErrorMessage =
                "Dispatch Quantity is required.")]
        [Range(
            typeof(decimal),
            "0.001",
            "999999999999999.999",
            ErrorMessage =
                "Dispatch Quantity must be greater than zero.")]
        [Display(Name = "Dispatch Qty")]
        public decimal DispatchQuantity
        {
            get;
            set;
        }

        #endregion
    }
}