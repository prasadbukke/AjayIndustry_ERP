/*
============================================================
File: Company.cs

Purpose:
Represents Company / Workshop Master information.

Responsibilities:
- Store company identity and statutory information.
- Store company contact and address information.
- Store ISO certification information.
- Store primary bank account information.
- Provide company data for ERP documents such as
  Delivery Challan and Invoice.

Important:
- Bank details currently represent one primary company
  bank account.
- If multiple bank accounts are required in future,
  a separate CompanyBankAccount entity can be introduced.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Company
        : BaseEntity
    {
        #region Identification

        public int CompanyId
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

        #endregion


        #region Statutory Information

        public string? GstNumber
        {
            get;
            set;
        }


        public string? PanNumber
        {
            get;
            set;
        }

        #endregion


        #region ISO Certification

        public string? IsoCertificationNumber
        {
            get;
            set;
        }

        #endregion


        #region Contact Information

        public string? PhoneNumber
        {
            get;
            set;
        }


        public string? Email
        {
            get;
            set;
        }


        public string? Website
        {
            get;
            set;
        }


        public string? ContactPerson
        {
            get;
            set;
        }

        #endregion


        #region Address

        public string? Address
        {
            get;
            set;
        }


        public string? City
        {
            get;
            set;
        }


        public string? State
        {
            get;
            set;
        }


        public string? Country
        {
            get;
            set;
        }


        public string? PostalCode
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


        #region Terms And Conditions

        /// <summary>
        /// Default Terms and Conditions used on Purchase Orders.
        /// </summary>
        public string? PurchaseOrderTermsAndConditions
        {
            get;
            set;
        }


        /// <summary>
        /// Default Terms and Conditions used on customer Invoices.
        /// </summary>
        public string? InvoiceTermsAndConditions
        {
            get;
            set;
        }

        #endregion
    }
}