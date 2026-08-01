/*
==============================================================

File : Warehouse.cs

Purpose :
Represents Warehouse.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class Warehouse : BaseEntity
    {
        public int WarehouseId { get; set; }

        public string WarehouseCode { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string WarehouseType { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}