/*
==============================================================

File : UpdateCompanyRequest.cs

Purpose :
Used to update Company information.

==============================================================
*/

namespace AjayIndustriesERP.Application.Contracts.Company
{
    public class UpdateCompanyRequest : CreateCompanyRequest
    {
        public int CompanyId { get; set; }
    }
}