/*
==============================================================

File : Uom.cs

Purpose :
Represents Unit Of Measure.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Uom : BaseEntity
    {
        public int UomId { get; set; }

        public string UomCode { get; set; } = string.Empty;

        public string UomName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}