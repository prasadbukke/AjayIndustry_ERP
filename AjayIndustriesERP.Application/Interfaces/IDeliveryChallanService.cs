/*
============================================================
File: IDeliveryChallanService.cs

Purpose:
Defines business operations for Delivery Challan.

Responsibilities:
- Read and search Delivery Challans.
- Load Finalized PDI Reports available for dispatch.
- Calculate remaining dispatch quantity.
- Prepare trusted Challan Draft source.
- Create and update Draft Challans.
- Finalize Delivery Challan.
- Generate Delivery Challan PDF.
- Soft-delete Draft Challan.
- Restore deleted Draft Challan.

Important:
- Finalized PDI is the trusted dispatch source.
- One PDI may be dispatched through multiple Challans.
- One Challan may contain one or more dispatch lines.
- Draft Challans reserve Dispatch Quantity.
- Finalized Challans are locked audit documents.
- Price / GST / Invoice business logic does not belong here.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IDeliveryChallanService
    {
        #region Read Operations

        Task<DeliveryChallan?>
            GetByIdAsync(
                int id);


        Task<PagedResult<DeliveryChallan>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Finalized PDI Source

        /*
         * Returns Finalized PDI Reports that still
         * have Accepted Quantity available for dispatch.
         */

        Task<List<PreDispatchInspection>>
            GetFinalizedPdisForDispatchAsync();


        /*
         * Loads one Finalized PDI Report as
         * trusted dispatch source.
         */

        Task<PreDispatchInspection?>
            GetFinalizedPdiForDispatchAsync(
                int preDispatchInspectionId);


        /*
         * Available To Dispatch
         *
         * =
         *
         * PDI Accepted Quantity
         * -
         * Quantity already allocated to active Challans.
         *
         * excludeDeliveryChallanId is used during Edit
         * so the current Draft does not reserve its
         * quantity twice.
         */

        Task<decimal>
            GetRemainingDispatchQuantityAsync(
                int preDispatchInspectionId,
                int? excludeDeliveryChallanId = null);

        #endregion


        #region Prepare Draft Source

        /*
         * Creates an in-memory Delivery Challan Draft.
         *
         * DOES NOT save to database.
         *
         * Auto-loads:
         *
         * - Customer
         * - Finalized PDI
         * - Production Job
         * - Customer PO
         * - Item / Part
         * - Customer Drawing
         * - PDI Accepted Quantity
         * - Already Dispatched Quantity calculation
         * - Available Dispatch Quantity calculation
         * - UOM
         *
         * Dispatch Quantity initially defaults to the
         * currently available quantity.
         */

        Task<DeliveryChallan?>
            PrepareDraftAsync(
                int preDispatchInspectionId);

        #endregion


        #region Create

        /*
         * Creates Delivery Challan as Draft.
         *
         * Source snapshot values posted from Web UI
         * are NOT trusted.
         *
         * Application Service reloads each PDI and
         * rebuilds trusted snapshot before save.
         */

        Task<DeliveryChallan>
            CreateAsync(
                DeliveryChallan deliveryChallan);

        #endregion


        #region Update Draft

        /*
         * Only Draft Challans can be edited.
         *
         * Dispatch quantity is revalidated against
         * current available PDI Accepted Quantity.
         */

        Task<DeliveryChallan>
            UpdateAsync(
                DeliveryChallan deliveryChallan);

        #endregion


        #region Finalize

        /*
         * Performs final quantity validation and
         * permanently locks the Delivery Challan.
         */

        Task<DeliveryChallan>
            FinalizeAsync(
                int id);

        #endregion


        #region PDF

        /*
         * Generates Delivery Challan PDF from
         * finalized saved snapshot data.
         *
         * PDF implementation will be separated
         * through a dedicated PDF generator.
         */

        Task<byte[]>
            GeneratePdfAsync(
                int id);

        #endregion


        #region Delete

        /*
         * Only Draft Delivery Challans can
         * be soft-deleted.
         */

        Task DeleteAsync(
            int id);

        #endregion


        #region Deleted Challans

        Task<List<DeliveryChallan>>
            GetDeletedAsync();


        /*
         * Restore is allowed only when the PDI still
         * has sufficient available Dispatch Quantity.
         */

        Task RestoreAsync(
            int id);

        #endregion
    }
}