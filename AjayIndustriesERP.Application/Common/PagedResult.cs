/*
==============================================================

File : PagedResult.cs

Purpose :
Represents paginated result.

==============================================================
*/

namespace AjayIndustriesERP.Application.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalRecords / PageSize);

        public bool HasPrevious => PageNumber > 1;

        public bool HasNext => PageNumber < TotalPages;
    }
}