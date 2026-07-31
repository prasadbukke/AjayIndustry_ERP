/*
==============================================================

File : BusinessException.cs

Purpose :
Represents business validation exception.

==============================================================
*/

namespace AjayIndustriesERP.Application.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message)
            : base(message)
        {
        }
    }
}