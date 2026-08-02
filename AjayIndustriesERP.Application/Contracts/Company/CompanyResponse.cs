/*
==============================================================

File : CompanyResponse.cs

Purpose :
Returns Company data from Service layer.

==============================================================
*/

namespace AjayIndustriesERP.Application.Contracts.Company
{
    public class CompanyResponse
    {
        public int CompanyId { get; set; }

        public string CompanyCode { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string GstNumber { get; set; } = string.Empty;

        public string? PanNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Website { get; set; }

        public string? ContactPerson { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string Country { get; set; } = string.Empty;

        public string? PostalCode { get; set; }

        public bool IsActive { get; set; }
    }
}