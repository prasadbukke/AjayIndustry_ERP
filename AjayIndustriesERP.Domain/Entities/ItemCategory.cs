/*
==============================================================

File : ItemCategory.cs

Purpose :
Represents Item Category.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ItemCategory : BaseEntity
    {
        public int ItemCategoryId { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}