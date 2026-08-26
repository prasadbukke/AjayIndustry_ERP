/*
============================================================
File: DeliveryChallanDetailsViewModel.cs

Purpose:
ViewModel used by Delivery Challan Details screen.

Responsibilities:
- Display Challan header information.
- Display manually entered L.P.G. No.
- Display Customer snapshot information.
- Display saved Customer delivery address.
- Display Company / Workshop snapshot information.
- Display transport / dispatch information.
- Display saved Challan Item snapshots.
- Display Product ID and HSN No.
- Display Draft / Finalized status.
- Display Finalization information.

Important:
- This ViewModel belongs only to Web layer.
- Customer address displayed here is the saved Challan
  address snapshot, not current Customer Master address.
- Company / Workshop information is projected from the
  saved CompanySnapshotJson.
- Finalized Challan remains historical and read-only.
============================================================
*/

using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Web.ViewModels.DeliveryChallan
{
    public class DeliveryChallanDetailsViewModel
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


        public DateTime ChallanDate
        {
            get;
            set;
        }


        public DeliveryChallanStatus Status
        {
            get;
            set;
        }

        #endregion


        #region LPG Information

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


        public string CustomerName
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Customer Delivery Address

        public string? CustomerAddressLine1
        {
            get;
            set;
        }


        public string? CustomerAddressLine2
        {
            get;
            set;
        }


        public string? CustomerCity
        {
            get;
            set;
        }


        public string? CustomerDistrict
        {
            get;
            set;
        }


        public string? CustomerState
        {
            get;
            set;
        }


        public string? CustomerPincode
        {
            get;
            set;
        }


        public string? CustomerCountry
        {
            get;
            set;
        }

        #endregion


        #region Customer Master Snapshot Information

        /*
         * Current commonly-used Customer Master values.
         *
         * These values will be projected from the saved
         * CustomerSnapshotJson.
         */

        public string? CustomerCode
        {
            get;
            set;
        }


        public string? CustomerLegalName
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


        public string? CustomerContactPerson
        {
            get;
            set;
        }


        public string? CustomerMobileNumber
        {
            get;
            set;
        }


        public string? CustomerAlternateMobileNumber
        {
            get;
            set;
        }


        public string? CustomerEmail
        {
            get;
            set;
        }


        public string? CustomerWebsite
        {
            get;
            set;
        }

        #endregion


        #region Company Workshop Information

        public int? CompanyId
        {
            get;
            set;
        }


        public string? CompanyCode
        {
            get;
            set;
        }


        public string? CompanyName
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


        public string? CompanyWebsite
        {
            get;
            set;
        }


        public string? CompanyContactPerson
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


        public string? CompanyCountry
        {
            get;
            set;
        }


        public string? CompanyPostalCode
        {
            get;
            set;
        }

        #endregion


        #region Dispatch Information

        public string? TransporterName
        {
            get;
            set;
        }


        public string? VehicleNumber
        {
            get;
            set;
        }


        public string? TransportReference
        {
            get;
            set;
        }


        public string? DispatchFrom
        {
            get;
            set;
        }


        public string? Destination
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


        #region Challan Items

        public List<DeliveryChallanItemDetailsViewModel>
            Items
        {
            get;
            set;
        } = new();

        #endregion
    }


    public class DeliveryChallanItemDetailsViewModel
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


        #region PDI Information

        public int PreDispatchInspectionId
        {
            get;
            set;
        }


        public string PreDispatchInspectionCode
        {
            get;
            set;
        } = string.Empty;


        public decimal PdiAcceptedQuantity
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


        public string ProductionJobCode
        {
            get;
            set;
        } = string.Empty;

        #endregion


        #region Customer PO

        public int CustomerPurchaseOrderItemId
        {
            get;
            set;
        }


        public string CustomerPurchaseOrderCode
        {
            get;
            set;
        } = string.Empty;


        public string CustomerPurchaseOrderNumber
        {
            get;
            set;
        } = string.Empty;


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


        public string? UnitName
        {
            get;
            set;
        }

        #endregion


        #region Product Reference

        public string? ProductReference
        {
            get;
            set;
        }

        #endregion


        #region HSN Information

        public string? HsnNumber
        {
            get;
            set;
        }

        #endregion


        #region Customer Drawing

        /*
         * Retained for internal ERP traceability.
         *
         * Current Delivery Challan PDF will not display
         * Customer Drawing Number / Revision.
         */

        public int? CustomerDrawingId
        {
            get;
            set;
        }


        public string? CustomerDrawingNumber
        {
            get;
            set;
        }


        public string? CustomerDrawingRevision
        {
            get;
            set;
        }

        #endregion


        #region Dispatch Quantity

        public decimal DispatchQuantity
        {
            get;
            set;
        }

        #endregion
    }
}