/*
==============================================================

File : production-job.js

Purpose :
Handles Production Job client-side functionality.

Responsibilities :
- Initialize searchable Customer PO dropdown.
- Load selected Customer PO information through AJAX.
- Auto-load all Customer PO Items.
- Build item-wise Production Quantity inputs.
- Display Ordered / Completed / Pending Quantity.
- Display Released Routing Item-wise.
- Preserve submitted rows after server validation errors.
- Keep Production Job client logic separate from Razor views.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Items
        ↓
Production Job Steps

Important :
- One Customer PO = One Production Job.
- Ordered Quantity comes from Customer PO.
- Production Quantity is entered by Admin.
- Worker does not enter Production Quantity here.
- Customer PO Item information returned by server is treated
  as the source used to build the Create form.
- Final business validation remains in ProductionJobService.

==============================================================
*/

(function () {
    "use strict";


    // =========================================================
    // INITIALIZATION
    // =========================================================

    document.addEventListener(
        "DOMContentLoaded",
        initializeProductionJob
    );


    function initializeProductionJob() {

        initializeCustomerPoSelect();

        initializeProductionQuantityValidation();

    }


    // =========================================================
    // CUSTOMER PO SEARCHABLE SELECT
    // =========================================================

    function initializeCustomerPoSelect() {

        const select =
            document.getElementById(
                "customerPurchaseOrderSelect"
            );


        /*
         * Edit page does not contain the Customer PO dropdown.
         */
        if (!select) {
            return;
        }


        initializeSelect2(
            select
        );


        select.addEventListener(
            "change",
            function () {

                handleCustomerPoChange(
                    select
                );

            }
        );


        /*
         * Important:
         *
         * If the page was returned after server-side validation
         * and Items already exist in the Razor model,
         * do NOT reload the PO.
         *
         * This preserves Admin-entered Production Quantities.
         */
        const existingRows =
            document.querySelectorAll(
                "#productionItemsBody " +
                "[data-production-item-row]"
            );


        if (
            select.value &&
            existingRows.length === 0
        ) {

            handleCustomerPoChange(
                select
            );
        }

    }


    // =========================================================
    // SELECT2
    // =========================================================

    function initializeSelect2(
        select) {

        if (
            !window.jQuery ||
            !jQuery.fn.select2
        ) {
            return;
        }


        const $select =
            jQuery(
                select
            );


        if (
            $select.hasClass(
                "select2-hidden-accessible"
            )
        ) {
            return;
        }


        const placeholder =
            $select.data(
                "placeholder"
            )
            ||
            "-- Select Customer PO --";


        $select.select2({

            width:
                "100%",

            placeholder:
                placeholder,

            allowClear:
                true,

            minimumResultsForSearch:
                0,

            language: {

                noResults:
                    function () {

                        return "No matching Customer PO found.";

                    }

            }

        });

    }


    // =========================================================
    // CUSTOMER PO CHANGE
    // =========================================================

    async function handleCustomerPoChange(
        select) {

        const customerPurchaseOrderId =
            parseInt(
                select.value || "0",
                10
            );


        if (
            !customerPurchaseOrderId ||
            customerPurchaseOrderId <= 0
        ) {

            clearCustomerPoSource();

            return;
        }


        const sourceUrl =
            select.dataset.sourceUrl;


        if (!sourceUrl) {

            showSourceError(
                "Production source URL is not configured."
            );

            return;
        }


        setSourceLoadingState(
            true
        );


        try {

            const requestUrl =
                sourceUrl +
                "?id=" +
                encodeURIComponent(
                    customerPurchaseOrderId
                );


            const response =
                await fetch(
                    requestUrl,
                    {
                        method:
                            "GET",

                        headers: {

                            "X-Requested-With":
                                "XMLHttpRequest"

                        }
                    }
                );


            if (!response.ok) {

                if (response.status === 404) {

                    throw new Error(
                        "Selected Customer PO is not available for Production."
                    );
                }


                throw new Error(
                    "Unable to load Customer PO information."
                );
            }


            const data =
                await response.json();


            renderCustomerPoSource(
                data
            );

        }
        catch (error) {

            console.error(
                "Production Job source loading failed:",
                error
            );


            clearCustomerPoSource();


            showSourceError(
                error &&
                    error.message
                    ? error.message
                    : "Unable to load Customer PO information."
            );

        }
        finally {

            setSourceLoadingState(
                false
            );
        }

    }


    // =========================================================
    // RENDER CUSTOMER PO SOURCE
    // =========================================================

    function renderCustomerPoSource(
        data) {

        updateCustomerPoSummary(
            data
        );


        renderProductionItems(
            Array.isArray(
                data.items
            )
                ? data.items
                : []
        );

    }


    // =========================================================
    // CUSTOMER PO SUMMARY
    // =========================================================

    function updateCustomerPoSummary(
        data) {

        const card =
            document.getElementById(
                "selectedPoSummaryCard"
            );


        if (!card) {
            return;
        }


        setText(
            "selectedPoCode",
            data.code || "-"
        );


        setText(
            "selectedPoNumber",
            data.customerPurchaseOrderNumber || "-"
        );


        setText(
            "selectedPoCustomer",
            data.customerName || "-"
        );


        setText(
            "selectedPoReceivedDate",
            formatDate(
                data.receivedDate
            )
        );


        setText(
            "selectedPoDeliveryDate",
            formatDate(
                data.requiredDeliveryDate
            )
        );


        const items =
            Array.isArray(
                data.items
            )
                ? data.items
                : [];


        setText(
            "selectedPoItemCount",
            items.length.toString()
        );


        card.classList.remove(
            "d-none"
        );

    }


    // =========================================================
    // PRODUCTION ITEMS
    // =========================================================

    function renderProductionItems(
        items) {

        const card =
            document.getElementById(
                "productionItemsCard"
            );


        const tbody =
            document.getElementById(
                "productionItemsBody"
            );


        const emptyState =
            document.getElementById(
                "productionItemsEmptyState"
            );


        const countBadge =
            document.getElementById(
                "productionItemsCount"
            );


        if (
            !card ||
            !tbody
        ) {
            return;
        }


        tbody.innerHTML =
            "";


        if (countBadge) {

            countBadge.textContent =
                items.length +
                " Item(s)";
        }


        if (
            !items ||
            items.length === 0
        ) {

            card.classList.remove(
                "d-none"
            );


            if (emptyState) {

                emptyState.classList.remove(
                    "d-none"
                );
            }


            return;
        }


        if (emptyState) {

            emptyState.classList.add(
                "d-none"
            );
        }


        items.forEach(
            function (
                item,
                index) {

                const rowContent =
                    createProductionItemContent(
                        item,
                        index
                    );


                tbody.insertAdjacentHTML(
                    "beforeend",
                    rowContent
                );

            }
        );


        card.classList.remove(
            "d-none"
        );


        initializeProductionQuantityValidation();

    }


    // =========================================================
    // BUILD PRODUCTION ITEM ROW
    // =========================================================

    function createProductionItemContent(
        item,
        index) {

        const itemCode =
            item.itemCode || "";


        const itemName =
            item.itemName || "";


        const unitName =
            item.unitName || "";


        const orderedQuantity =
            toDecimal(
                item.orderedQuantity
            );


        const productionQuantity =
            toDecimal(
                item.productionQuantity
            );


        const completedQuantity =
            toDecimal(
                item.completedQuantity
            );


        const pendingQuantity =
            Math.max(
                0,
                orderedQuantity -
                completedQuantity
            );


        const hasReleasedRouting =
            item.hasReleasedRouting === true;


        const routingCode =
            item.routingCode || "";


        const routingRevisionNumber =
            item.routingRevisionNumber;


        const requiredDeliveryDate =
            formatDate(
                item.requiredDeliveryDate
            );


        const safeItemCode =
            escapeHtml(
                itemCode
            );


        const safeItemName =
            escapeHtml(
                itemName
            );


        const safeUnitName =
            escapeHtml(
                unitName
            );


        const orderedValue =
            decimalValue(
                orderedQuantity
            );


        const productionValue =
            decimalValue(
                productionQuantity
            );


        const completedValue =
            decimalValue(
                completedQuantity
            );


        const routingId =
            parseInt(
                item.itemProcessRoutingId || "0",
                10
            )
            ||
            0;


        const routingRevisionValue =
            routingRevisionNumber === null ||
                routingRevisionNumber === undefined
                ? ""
                : routingRevisionNumber;


        /*
         * Hidden inputs are placed inside the same row so that
         * ASP.NET Core model binding receives:
         *
         * Items[0].Id
         * Items[0].CustomerPurchaseOrderItemId
         * Items[0].ProductionQuantity
         * ...
         */

        return `
            <tr data-production-item-row
                data-item-index="${index}">

                <td>

                    ${createHiddenInput(
            `Items[${index}].Id`,
            0
        )}

                    ${createHiddenInput(
            `Items[${index}].CustomerPurchaseOrderItemId`,
            item.customerPurchaseOrderItemId || 0
        )}

                    ${createHiddenInput(
            `Items[${index}].ItemId`,
            item.itemId || 0
        )}

                    ${createHiddenInput(
            `Items[${index}].ItemCode`,
            itemCode
        )}

                    ${createHiddenInput(
            `Items[${index}].ItemName`,
            itemName
        )}

                    ${createHiddenInput(
            `Items[${index}].UnitName`,
            unitName
        )}

                    ${createHiddenInput(
            `Items[${index}].OrderedQuantity`,
            orderedValue
        )}

                    ${createHiddenInput(
            `Items[${index}].CompletedQuantity`,
            completedValue
        )}

                    ${createHiddenInput(
            `Items[${index}].ItemProcessRoutingId`,
            routingId
        )}

                    ${createHiddenInput(
            `Items[${index}].RoutingCode`,
            routingCode
        )}

                    ${createHiddenInput(
            `Items[${index}].RoutingRevisionNumber`,
            routingRevisionValue
        )}

                    ${createHiddenInput(
            `Items[${index}].HasReleasedRouting`,
            hasReleasedRouting
                ? "true"
                : "false"
        )}

                    ${createHiddenInput(
            `Items[${index}].RequiredDeliveryDate`,
            toFormDateValue(
                item.requiredDeliveryDate
            )
        )}


                    <div class="fw-semibold">
                        ${safeItemCode || "-"}
                    </div>

                    <div class="small text-muted">
                        ${safeItemName || "-"}
                    </div>

                    ${unitName
                ? `
                                <div class="small text-muted mt-1">
                                    UOM:
                                    ${safeUnitName}
                                </div>
                              `
                : ""
            }

                </td>


                <td class="text-end">

                    <span class="fw-semibold">
                        ${formatQuantity(
                orderedQuantity
            )}
                    </span>

                    ${unitName
                ? `
                                <div class="small text-muted">
                                    ${safeUnitName}
                                </div>
                              `
                : ""
            }

                </td>


                <td class="text-end">

                    <span class="fw-semibold
                                 ${completedQuantity > 0
                ? "text-success"
                : ""
            }">

                        ${formatQuantity(
                completedQuantity
            )}

                    </span>

                </td>


                <td class="text-end">

                    ${pendingQuantity <= 0
                ? `
                                <span class="fw-semibold text-success">
                                    0
                                </span>

                                <div>
                                    <span class="badge bg-success">
                                        Complete
                                    </span>
                                </div>
                              `
                : `
                                <span class="fw-semibold">
                                    ${formatQuantity(
                    pendingQuantity
                )}
                                </span>
                              `
            }

                </td>


                <td>

                    <div class="input-group">

                        <input
                            type="number"
                            name="Items[${index}].ProductionQuantity"
                            id="Items_${index}__ProductionQuantity"
                            value="${escapeAttribute(
                productionValue
            )}"
                            min="${escapeAttribute(
                completedValue
            )}"
                            max="${escapeAttribute(
                orderedValue
            )}"
                            step="0.001"
                            class="form-control production-quantity-input"
                            data-item-code="${escapeAttribute(
                itemCode
            )}"
                            data-ordered-quantity="${escapeAttribute(
                orderedValue
            )}"
                            data-completed-quantity="${escapeAttribute(
                completedValue
            )}" />

                        ${unitName
                ? `
                                    <span class="input-group-text">
                                        ${safeUnitName}
                                    </span>
                                  `
                : ""
            }

                    </div>


                    <span
                        class="text-danger small production-quantity-error"
                        data-production-quantity-error="${index}">
                    </span>

                </td>


                <td>

                    ${hasReleasedRouting
                ? `
                                <div class="fw-semibold text-success">
                                    ${escapeHtml(
                    routingCode
                )}
                                </div>

                                <div class="small text-muted">
                                    Revision
                                    ${routingRevisionNumber ??
                "-"
                }
                                </div>
                              `
                : `
                                <span class="badge bg-danger">
                                    Routing Missing
                                </span>

                                <div class="small text-danger mt-1">
                                    Released Routing required.
                                </div>
                              `
            }

                </td>


                <td>

                    ${requiredDeliveryDate !== "-"
                ? escapeHtml(
                    requiredDeliveryDate
                )
                : `<span class="text-muted">-</span>`
            }

                </td>

            </tr>
        `;

    }


    // =========================================================
    // PRODUCTION QUANTITY VALIDATION
    // =========================================================

    function initializeProductionQuantityValidation() {

        const inputs =
            document.querySelectorAll(
                ".production-quantity-input"
            );


        inputs.forEach(
            function (
                input) {

                if (
                    input.dataset.validationBound ===
                    "true"
                ) {
                    return;
                }


                input.dataset.validationBound =
                    "true";


                input.addEventListener(
                    "input",
                    function () {

                        validateProductionQuantityInput(
                            input
                        );

                    }
                );


                input.addEventListener(
                    "blur",
                    function () {

                        validateProductionQuantityInput(
                            input
                        );

                    }
                );

            }
        );

    }


    function validateProductionQuantityInput(
        input) {

        const row =
            input.closest(
                "[data-production-item-row]"
            );


        if (!row) {
            return true;
        }


        const rowIndex =
            row.dataset.itemIndex;


        const errorElement =
            document.querySelector(
                `[data-production-quantity-error="${rowIndex}"]`
            );


        const itemCode =
            input.dataset.itemCode ||
            "Item";


        const orderedQuantity =
            toDecimal(
                input.dataset.orderedQuantity
            );


        const completedQuantity =
            toDecimal(
                input.dataset.completedQuantity
            );


        const productionQuantity =
            toDecimal(
                input.value
            );


        let errorMessage =
            "";


        if (
            productionQuantity < 0
        ) {

            errorMessage =
                "Production Quantity cannot be negative.";

        }
        else if (
            productionQuantity <
            completedQuantity
        ) {

            errorMessage =
                `${itemCode}: Production Quantity cannot be less than Completed Quantity ${formatQuantity(
                    completedQuantity
                )}.`;

        }
        else if (
            productionQuantity >
            orderedQuantity
        ) {

            errorMessage =
                `${itemCode}: Production Quantity cannot exceed Ordered Quantity ${formatQuantity(
                    orderedQuantity
                )}.`;

        }


        if (errorElement) {

            errorElement.textContent =
                errorMessage;
        }


        if (errorMessage) {

            input.classList.add(
                "is-invalid"
            );

            return false;
        }


        input.classList.remove(
            "is-invalid"
        );


        return true;

    }


    // =========================================================
    // CLEAR CUSTOMER PO SOURCE
    // =========================================================

    function clearCustomerPoSource() {

        const summaryCard =
            document.getElementById(
                "selectedPoSummaryCard"
            );


        const itemsCard =
            document.getElementById(
                "productionItemsCard"
            );


        const tbody =
            document.getElementById(
                "productionItemsBody"
            );


        const emptyState =
            document.getElementById(
                "productionItemsEmptyState"
            );


        const countBadge =
            document.getElementById(
                "productionItemsCount"
            );


        if (summaryCard) {

            summaryCard.classList.add(
                "d-none"
            );
        }


        if (tbody) {

            tbody.innerHTML =
                "";
        }


        if (countBadge) {

            countBadge.textContent =
                "0 Item(s)";
        }


        if (emptyState) {

            emptyState.classList.remove(
                "d-none"
            );
        }


        if (itemsCard) {

            itemsCard.classList.add(
                "d-none"
            );
        }

    }


    // =========================================================
    // LOADING STATE
    // =========================================================

    function setSourceLoadingState(
        isLoading) {

        const select =
            document.getElementById(
                "customerPurchaseOrderSelect"
            );


        if (!select) {
            return;
        }


        select.disabled =
            isLoading;


        if (
            window.jQuery &&
            jQuery.fn.select2 &&
            jQuery(select).hasClass(
                "select2-hidden-accessible"
            )
        ) {

            jQuery(select)
                .prop(
                    "disabled",
                    isLoading
                )
                .trigger(
                    "change.select2"
                );
        }

    }


    // =========================================================
    // SOURCE ERROR
    // =========================================================

    function showSourceError(
        message) {

        const itemsCard =
            document.getElementById(
                "productionItemsCard"
            );


        const tbody =
            document.getElementById(
                "productionItemsBody"
            );


        const emptyState =
            document.getElementById(
                "productionItemsEmptyState"
            );


        if (
            !itemsCard ||
            !tbody
        ) {

            window.alert(
                message
            );

            return;
        }


        tbody.innerHTML =
            "";


        if (emptyState) {

            emptyState.classList.remove(
                "d-none"
            );


            emptyState.innerHTML = `

                <i class="fa-solid
                          fa-triangle-exclamation
                          fa-2x
                          text-danger
                          mb-3">
                </i>

                <div class="fw-semibold text-danger">
                    Unable to load Production Items
                </div>

                <div class="small text-muted mt-1">
                    ${escapeHtml(
                message
            )}
                </div>

            `;
        }


        itemsCard.classList.remove(
            "d-none"
        );

    }


    // =========================================================
    // HIDDEN INPUT HELPER
    // =========================================================

    function createHiddenInput(
        name,
        value) {

        const safeName =
            escapeAttribute(
                name
            );


        const safeValue =
            escapeAttribute(
                value === null ||
                    value === undefined
                    ? ""
                    : value
            );


        return `
            <input
                type="hidden"
                name="${safeName}"
                value="${safeValue}" />
        `;

    }


    // =========================================================
    // TEXT HELPER
    // =========================================================

    function setText(
        elementId,
        value) {

        const element =
            document.getElementById(
                elementId
            );


        if (!element) {
            return;
        }


        element.textContent =
            value === null ||
                value === undefined ||
                value === ""
                ? "-"
                : value;

    }


    // =========================================================
    // QUANTITY HELPERS
    // =========================================================

    function toDecimal(
        value) {

        if (
            value === null ||
            value === undefined ||
            value === ""
        ) {

            return 0;
        }


        const number =
            parseFloat(
                value
            );


        return Number.isFinite(
            number
        )
            ? number
            : 0;

    }


    function decimalValue(
        value) {

        const number =
            toDecimal(
                value
            );


        return number.toString();

    }


    function formatQuantity(
        value) {

        const number =
            toDecimal(
                value
            );


        return number.toLocaleString(
            undefined,
            {
                minimumFractionDigits:
                    0,

                maximumFractionDigits:
                    3
            }
        );

    }


    // =========================================================
    // DATE HELPERS
    // =========================================================

    function formatDate(
        value) {

        if (!value) {
            return "-";
        }


        const date =
            new Date(
                value
            );


        if (
            Number.isNaN(
                date.getTime()
            )
        ) {
            return "-";
        }


        const day =
            String(
                date.getDate()
            ).padStart(
                2,
                "0"
            );


        const monthNames =
            [
                "Jan",
                "Feb",
                "Mar",
                "Apr",
                "May",
                "Jun",
                "Jul",
                "Aug",
                "Sep",
                "Oct",
                "Nov",
                "Dec"
            ];


        const month =
            monthNames[
            date.getMonth()
            ];


        const year =
            date.getFullYear();


        return `${day}-${month}-${year}`;

    }


    function toFormDateValue(
        value) {

        if (!value) {
            return "";
        }


        const date =
            new Date(
                value
            );


        if (
            Number.isNaN(
                date.getTime()
            )
        ) {
            return "";
        }


        const year =
            date.getFullYear();


        const month =
            String(
                date.getMonth() + 1
            ).padStart(
                2,
                "0"
            );


        const day =
            String(
                date.getDate()
            ).padStart(
                2,
                "0"
            );


        return `${year}-${month}-${day}`;

    }


    // =========================================================
    // HTML ENCODING
    // =========================================================

    function escapeHtml(
        value) {

        if (
            value === null ||
            value === undefined
        ) {
            return "";
        }


        return String(
            value
        )
            .replace(
                /&/g,
                "&amp;"
            )
            .replace(
                /</g,
                "&lt;"
            )
            .replace(
                />/g,
                "&gt;"
            )
            .replace(
                /"/g,
                "&quot;"
            )
            .replace(
                /'/g,
                "&#039;"
            );

    }


    function escapeAttribute(
        value) {

        return escapeHtml(
            value
        );

    }

})();