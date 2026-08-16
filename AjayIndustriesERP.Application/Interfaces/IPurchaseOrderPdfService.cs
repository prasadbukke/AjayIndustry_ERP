using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPurchaseOrderPdfService
    {
        byte[] GeneratePdf(
            PurchaseOrder purchaseOrder);
    }
}