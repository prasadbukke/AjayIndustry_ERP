/*
============================================================
File: DeliveryChallan.cs

Purpose:
Represents a Delivery Challan created against one or more
Finalized Pre-Dispatch Inspection reports.

Responsibilities:
- Store Delivery Challan identification and status.
- Store Customer reference and Customer Name.
- Store editable Customer delivery address snapshot.
- Store complete Customer Master snapshot as JSON.
- Store Company / Workshop reference and name.
- Store complete Company Master snapshot as JSON.
- Store dispatch / transport information.
- Store manually entered L.P.G. No.
- Store Finalization information.
- Maintain Delivery Challan Items.

Important:
- Customer Master and Company Master snapshots are stored
  as JSON for future extensibility.
- New fields added to Customer / Company Master can become
  part of future snapshots without adding Delivery Challan
  columns for every new field.
- Customer delivery address is stored separately because
  it is editable on the Draft Challan.
- Saved snapshots preserve historical document data.
- Draft Challans are editable.
- Finalized Challans are locked.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class DeliveryChallan : BaseEntity
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

        /*
         * Manual L.P.G. Number used on the Challan.
         */
        public string? LpgNumber
        {
            get;
            set;
        }

        #endregion


        #region Customer Reference

        /*
         * CustomerId keeps the link/reference to Customer Master.
         *
         * CustomerName is also stored as a historical
         * document snapshot for convenient display/search.
         */

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


        #region Customer Editable Address Snapshot

        /*
         * Initially auto-loaded from Customer Master.
         *
         * User may edit these fields on Draft Challan because
         * the actual delivery address may be different from
         * the primary Customer Master address.
         *
         * These values are saved with the Challan and therefore
         * remain unchanged if Customer Master is edited later.
         */

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


        #region Customer Master Snapshot

        /*
         * Complete Customer Master snapshot serialized as JSON.
         *
         * Example information currently available:
         * - Code
         * - Customer Name / Legal Name
         * - GSTIN / PAN
         * - Contact Person
         * - Mobile / Alternate Mobile
         * - Email
         * - Address
         * - Payment Terms / Credit Days
         * - Website / Remarks
         *
         * If future scalar fields are added to Customer Master,
         * they can automatically become part of new snapshots
         * without creating matching Delivery Challan columns.
         */

        public string? CustomerSnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Company Workshop Reference

        /*
         * Company represents Ajay Industries / Workshop source.
         */

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

        #endregion


        #region Company Master Snapshot

        /*
         * Complete Company Master snapshot serialized as JSON.
         *
         * Example information currently available:
         * - Company Code / Name
         * - GST Number
         * - PAN
         * - Phone
         * - Email
         * - Website
         * - Contact Person
         * - Address
         * - City / State / Country / Postal Code
         * - Purchase Order Terms
         *
         * New Company Master scalar fields can become part of
         * future snapshots without adding new Challan columns.
         */

        public string? CompanySnapshotJson
        {
            get;
            set;
        }

        #endregion


        #region Dispatch Information

        /*
         * Transport data remains available inside ERP.
         *
         * Current customer-facing Delivery Challan PDF will
         * not display the Transport Details section.
         */

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


        #region Items

        public ICollection<DeliveryChallanItem> Items
        {
            get;
            set;
        } = new List<DeliveryChallanItem>();

        #endregion
    }
}