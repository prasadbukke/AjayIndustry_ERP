namespace AjayIndustriesERP.Domain.Enums
{
    /// <summary>
    /// Represents the workflow status of a Supplier Payment.
    ///
    /// Draft:
    /// Payment can still be edited and does not reduce
    /// Supplier Outstanding.
    ///
    /// Finalized:
    /// Payment is confirmed and its invoice allocations
    /// reduce Supplier Outstanding.
    /// </summary>
    public enum SupplierPaymentStatus
    {
        Draft = 1,

        Finalized = 2
    }
}