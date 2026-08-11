/*
==============================================================

File : Supplier.cs

Purpose :
Represents Supplier Master information.

Future Usage :
- Purchase Orders
- GRN
- Purchase Invoices
- Supplier Payments
- Supplier History
- Supplier-wise Pricing

Notes :
- Financial balances are NOT stored directly here.
- Purchase totals and pending payments will be derived
  from future transaction/accounting modules.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    /// <summary>
    /// Represents a Supplier registered in the ERP system.
    /// </summary>
    public class Supplier : BaseEntity
    {
        #region Primary Key

        public int SupplierId { get; set; }

        #endregion

        #region Basic Information

        public string SupplierCode { get; set; } =
            string.Empty;

        public string SupplierName { get; set; } =
            string.Empty;

        public string? ContactPerson { get; set; }

        #endregion

        #region Contact Information

        public string? MobileNumber { get; set; }

        public string? AlternateMobileNumber { get; set; }

        public string? Email { get; set; }

        #endregion

        #region Tax Information

        /// <summary>
        /// GST Registration Number.
        /// Optional for unregistered suppliers.
        /// </summary>
        public string? Gstin { get; set; }

        /// <summary>
        /// Permanent Account Number.
        /// </summary>
        public string? Pan { get; set; }

        #endregion

        #region Address

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }

        #endregion

        #region Commercial Information

        /// <summary>
        /// Standard credit/payment period in days.
        ///
        /// Example:
        /// 0  = Immediate
        /// 15 = 15 Days
        /// 30 = 30 Days
        /// </summary>
        public int? PaymentTermsDays { get; set; }

        #endregion

        #region Additional Information

        public string? Description { get; set; }

        #endregion
    }
}