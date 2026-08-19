/*
============================================================
File: CustomerDetailsViewModel.cs

Purpose:
Provides read-only Customer Master information to Details page.

Responsibilities:
- Supply complete Customer information.
- Keep the Details View independent from direct Domain binding.
- Provide an extension point for future Customer PO,
  Production and Sales summaries.

Important:
Future Customer PO information can be added here without
changing the Customer Domain entity.
============================================================
*/

namespace AjayIndustriesERP.Web.ViewModels.Customer
{
    public class CustomerDetailsViewModel
    {
        #region Identification

        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        #endregion


        #region Customer Information

        public string CustomerName { get; set; } = string.Empty;

        public string? LegalName { get; set; }

        #endregion


        #region Tax Information

        public string? GSTIN { get; set; }

        public string? PAN { get; set; }

        #endregion


        #region Primary Contact

        public string? ContactPerson { get; set; }

        public string? MobileNumber { get; set; }

        public string? AlternateMobileNumber { get; set; }

        public string? Email { get; set; }

        #endregion


        #region Primary Address

        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        #endregion


        #region Commercial Information

        public string? PaymentTerms { get; set; }

        public int? CreditDays { get; set; }

        #endregion


        #region Other Information

        public string? Website { get; set; }

        public string? Remarks { get; set; }

        #endregion
    }
}