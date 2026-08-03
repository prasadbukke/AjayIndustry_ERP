/*
==============================================================

File : Brand.cs

Purpose :
Represents Brand.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public int BrandId { get; set; }

        public string BrandCode { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}