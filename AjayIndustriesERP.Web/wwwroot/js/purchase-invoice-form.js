/*
============================================================
File: purchase-invoice-form.js

Module:
Purchase Invoice / Supplier Bill

Purpose:
Handles Purchase Invoice Create / Edit form behaviour.

Responsibilities:
- Reload Create screen when Purchase Order changes.
- Enable / disable GRN invoice quantity fields.
- Select All / Clear All GRN rows.
- Prevent quantity above available GRN quantity.
- Calculate live line taxable / total values.
- Calculate live Invoice GST totals.
- Calculate Grand Total.
- Calculate Due Date from Supplier Invoice Date
  and Supplier Credit Days.
- Prevent submission when no GRN line is selected.

Important:
- JavaScript calculations are preview only.
- PurchaseInvoiceService recalculates all trusted
  financial values before saving.
============================================================
*/

document.addEventListener(
    "DOMContentLoaded",
    function () {

        // =====================================================
        // ELEMENTS
        // =====================================================

        const purchaseOrderSelector =
            document.getElementById(
                "purchaseOrderSelector");


        const itemsTable =
            document.getElementById(
                "purchaseInvoiceItemsTable");


        const selectAllButton =
            document.getElementById(
                "selectAllPurchaseInvoiceItems");


        const clearAllButton =
            document.getElementById(
                "clearAllPurchaseInvoiceItems");


        const transportChargesInput =
            document.getElementById(
                "transportCharges");


        const otherChargesInput =
            document.getElementById(
                "otherCharges");


        const roundOffInput =
            document.getElementById(
                "roundOffAmount");


        const supplierInvoiceDateInput =
            document.getElementById(
                "supplierInvoiceDate");


        const dueDateDisplay =
            document.getElementById(
                "dueDateDisplay");


        const form =
            itemsTable
                ? itemsTable.closest("form")
                : purchaseOrderSelector
                    ? purchaseOrderSelector.closest("form")
                    : null;


        // =====================================================
        // PURCHASE ORDER CHANGE
        // =====================================================

        if (purchaseOrderSelector) {

            purchaseOrderSelector.addEventListener(
                "change",
                function () {

                    const purchaseOrderId =
                        purchaseOrderSelector.value;


                    const createUrl =
                        purchaseOrderSelector.dataset
                            .createUrl;


                    if (!createUrl) {
                        return;
                    }


                    if (!purchaseOrderId) {

                        window.location.href =
                            createUrl;

                        return;
                    }


                    const separator =
                        createUrl.includes("?")
                            ? "&"
                            : "?";


                    window.location.href =
                        createUrl +
                        separator +
                        "purchaseOrderId=" +
                        encodeURIComponent(
                            purchaseOrderId);
                });
        }


        // =====================================================
        // HELPERS
        // =====================================================

        function parseNumber(value) {

            if (
                value === null ||
                value === undefined ||
                value === ""
            ) {
                return 0;
            }


            const normalized =
                value
                    .toString()
                    .replace(/,/g, "")
                    .trim();


            const number =
                Number(normalized);


            return Number.isFinite(number)
                ? number
                : 0;
        }


        function roundMoney(value) {

            const number =
                parseNumber(value);


            /*
             * Financial values on this screen are normally
             * positive. EPSILON avoids common floating-point
             * issues such as 1.005 becoming 1.00.
             */
            if (number >= 0) {

                return Math.round(
                    (
                        number +
                        Number.EPSILON
                    ) *
                    100
                ) / 100;
            }


            return -Math.round(
                (
                    Math.abs(number) +
                    Number.EPSILON
                ) *
                100
            ) / 100;
        }


        function formatMoney(value) {

            return roundMoney(value)
                .toLocaleString(
                    "en-IN",
                    {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
        }


        function getRows() {

            if (!itemsTable) {
                return [];
            }


            return Array.from(
                itemsTable.querySelectorAll(
                    ".purchase-invoice-item-row"));
        }


        function getRowElements(row) {

            return {

                selector:
                    row.querySelector(
                        ".purchase-item-selector"),

                quantity:
                    row.querySelector(
                        ".purchase-invoice-quantity"),

                rate:
                    row.querySelector(
                        ".purchase-invoice-rate"),

                taxable:
                    row.querySelector(
                        ".purchase-row-taxable"),

                total:
                    row.querySelector(
                        ".purchase-row-total")
            };
        }


        function getAvailableQuantity(row) {

            return parseNumber(
                row.dataset
                    .availableQuantity);
        }


        function getRate(row) {

            const elements =
                getRowElements(row);


            if (!elements.rate) {
                return 0;
            }


            return parseNumber(
                elements.rate.value);
        }


        function getGstRate(row) {

            return parseNumber(
                row.dataset.gstRate);
        }


        // =====================================================
        // ROW STATE
        // =====================================================

        function updateRowState(row) {

            const elements =
                getRowElements(row);


            if (
                !elements.selector ||
                !elements.quantity
            ) {
                return;
            }


            const isSelected =
                elements.selector.checked;


            const availableQuantity =
                getAvailableQuantity(row);


            elements.quantity.disabled =
                !isSelected;


            if (!isSelected) {

                updateRowCalculation(
                    row);

                return;
            }


            let quantity =
                parseNumber(
                    elements.quantity.value);


            /*
             * If row is selected with blank / zero qty,
             * default it to complete available quantity.
             */
            if (
                quantity <= 0 &&
                availableQuantity > 0
            ) {

                quantity =
                    availableQuantity;


                elements.quantity.value =
                    quantity.toFixed(3);
            }


            if (quantity >
                availableQuantity) {

                quantity =
                    availableQuantity;


                elements.quantity.value =
                    quantity.toFixed(3);
            }


            updateRowCalculation(
                row);
        }


        // =====================================================
        // ROW CALCULATION
        // =====================================================

        function updateRowCalculation(row) {

            const elements =
                getRowElements(row);


            if (
                !elements.selector ||
                !elements.quantity
            ) {
                return;
            }


            const isSelected =
                elements.selector.checked;


            if (!isSelected) {

                if (elements.taxable) {
                    elements.taxable.textContent =
                        formatMoney(0);
                }


                if (elements.total) {
                    elements.total.textContent =
                        formatMoney(0);
                }


                calculateInvoiceTotals();

                return;
            }


            const availableQuantity =
                getAvailableQuantity(row);


            let quantity =
                parseNumber(
                    elements.quantity.value);


            if (quantity < 0) {

                quantity =
                    0;

                elements.quantity.value =
                    "0.000";
            }


            if (quantity >
                availableQuantity) {

                quantity =
                    availableQuantity;

                elements.quantity.value =
                    quantity.toFixed(3);
            }


            const rate =
                getRate(row);


            const gstRate =
                getGstRate(row);


            /*
             * Purchase Discount is disabled.
             *
             * Gross = Taxable.
             */
            const taxableAmount =
                roundMoney(
                    quantity *
                    rate);


            const gstAmount =
                roundMoney(
                    taxableAmount *
                    gstRate /
                    100);


            const lineTotal =
                roundMoney(
                    taxableAmount +
                    gstAmount);


            if (elements.taxable) {

                elements.taxable.textContent =
                    formatMoney(
                        taxableAmount);
            }


            if (elements.total) {

                elements.total.textContent =
                    formatMoney(
                        lineTotal);
            }


            calculateInvoiceTotals();
        }


        // =====================================================
        // INVOICE TOTALS
        // =====================================================

        function calculateInvoiceTotals() {

            let grossAmount =
                0;


            let taxableAmount =
                0;


            let totalGstAmount =
                0;


            getRows().forEach(
                function (row) {

                    const elements =
                        getRowElements(row);


                    if (
                        !elements.selector ||
                        !elements.quantity ||
                        !elements.selector.checked
                    ) {
                        return;
                    }


                    const quantity =
                        parseNumber(
                            elements.quantity.value);


                    const rate =
                        getRate(row);


                    const gstRate =
                        getGstRate(row);


                    const rowGross =
                        roundMoney(
                            quantity *
                            rate);


                    const rowTaxable =
                        rowGross;


                    const rowGst =
                        roundMoney(
                            rowTaxable *
                            gstRate /
                            100);


                    grossAmount +=
                        rowGross;


                    taxableAmount +=
                        rowTaxable;


                    totalGstAmount +=
                        rowGst;
                });


            grossAmount =
                roundMoney(
                    grossAmount);


            taxableAmount =
                roundMoney(
                    taxableAmount);


            totalGstAmount =
                roundMoney(
                    totalGstAmount);


            /*
             * The Razor form renders either:
             *
             * #purchaseIgstAmount
             *
             * OR
             *
             * #purchaseCgstAmount
             * #purchaseSgstAmount
             *
             * Therefore this also tells us which GST
             * calculation applies.
             */
            const igstElement =
                document.getElementById(
                    "purchaseIgstAmount");


            const cgstElement =
                document.getElementById(
                    "purchaseCgstAmount");


            const sgstElement =
                document.getElementById(
                    "purchaseSgstAmount");


            let cgstAmount =
                0;


            let sgstAmount =
                0;


            let igstAmount =
                0;


            if (igstElement) {

                igstAmount =
                    totalGstAmount;


                igstElement.textContent =
                    formatMoney(
                        igstAmount);

            } else {

                /*
                 * Calculate GST per row split instead of
                 * simply splitting final GST total.
                 *
                 * This better matches server-side line-level
                 * CGST / SGST rounding.
                 */
                cgstAmount =
                    0;


                sgstAmount =
                    0;


                getRows().forEach(
                    function (row) {

                        const elements =
                            getRowElements(row);


                        if (
                            !elements.selector ||
                            !elements.quantity ||
                            !elements.selector.checked
                        ) {
                            return;
                        }


                        const quantity =
                            parseNumber(
                                elements.quantity.value);


                        const rate =
                            getRate(row);


                        const gstRate =
                            getGstRate(row);


                        const taxable =
                            roundMoney(
                                quantity *
                                rate);


                        const halfGstRate =
                            gstRate /
                            2;


                        cgstAmount +=
                            roundMoney(
                                taxable *
                                halfGstRate /
                                100);


                        sgstAmount +=
                            roundMoney(
                                taxable *
                                halfGstRate /
                                100);
                    });


                cgstAmount =
                    roundMoney(
                        cgstAmount);


                sgstAmount =
                    roundMoney(
                        sgstAmount);


                if (cgstElement) {

                    cgstElement.textContent =
                        formatMoney(
                            cgstAmount);
                }


                if (sgstElement) {

                    sgstElement.textContent =
                        formatMoney(
                            sgstAmount);
                }
            }


            const transportCharges =
                parseNumber(
                    transportChargesInput
                        ? transportChargesInput.value
                        : 0);


            const otherCharges =
                parseNumber(
                    otherChargesInput
                        ? otherChargesInput.value
                        : 0);


            const roundOffAmount =
                parseNumber(
                    roundOffInput
                        ? roundOffInput.value
                        : 0);


            const grandTotal =
                roundMoney(
                    taxableAmount +
                    cgstAmount +
                    sgstAmount +
                    igstAmount +
                    transportCharges +
                    otherCharges +
                    roundOffAmount);


            setText(
                "purchaseGrossAmount",
                formatMoney(
                    grossAmount));


            setText(
                "purchaseTaxableAmount",
                formatMoney(
                    taxableAmount));


            setText(
                "purchaseTransportSummary",
                formatMoney(
                    transportCharges));


            setText(
                "purchaseOtherSummary",
                formatMoney(
                    otherCharges));


            setText(
                "purchaseRoundOffSummary",
                formatMoney(
                    roundOffAmount));


            setText(
                "purchaseGrandTotal",
                formatMoney(
                    grandTotal));
        }


        function setText(
            elementId,
            value) {

            const element =
                document.getElementById(
                    elementId);


            if (element) {

                element.textContent =
                    value;
            }
        }


        // =====================================================
        // DUE DATE
        // =====================================================

        function calculateDueDate() {

            if (
                !supplierInvoiceDateInput ||
                !dueDateDisplay
            ) {
                return;
            }


            const invoiceDateValue =
                supplierInvoiceDateInput.value;


            const creditDaysValue =
                supplierInvoiceDateInput.dataset
                    .creditDays;


            if (!invoiceDateValue) {

                dueDateDisplay.value =
                    "";

                return;
            }


            if (
                creditDaysValue === null ||
                creditDaysValue === undefined ||
                creditDaysValue === ""
            ) {

                dueDateDisplay.value =
                    "";

                return;
            }


            const creditDays =
                Number(
                    creditDaysValue);


            if (
                !Number.isFinite(
                    creditDays) ||
                creditDays < 0
            ) {

                dueDateDisplay.value =
                    "";

                return;
            }


            /*
             * Parse yyyy-MM-dd manually.
             *
             * Avoid UTC timezone date shift.
             */
            const parts =
                invoiceDateValue
                    .split("-")
                    .map(Number);


            if (parts.length !== 3) {

                dueDateDisplay.value =
                    "";

                return;
            }


            const dueDate =
                new Date(
                    parts[0],
                    parts[1] - 1,
                    parts[2]);


            dueDate.setDate(
                dueDate.getDate() +
                creditDays);


            const year =
                dueDate.getFullYear();


            const month =
                String(
                    dueDate.getMonth() + 1)
                    .padStart(
                        2,
                        "0");


            const day =
                String(
                    dueDate.getDate())
                    .padStart(
                        2,
                        "0");


            dueDateDisplay.value =
                `${year}-${month}-${day}`;
        }


        // =====================================================
        // SELECT ALL
        // =====================================================

        if (selectAllButton) {

            selectAllButton.addEventListener(
                "click",
                function () {

                    getRows().forEach(
                        function (row) {

                            const elements =
                                getRowElements(row);


                            if (!elements.selector) {
                                return;
                            }


                            elements.selector.checked =
                                true;


                            updateRowState(
                                row);
                        });


                    calculateInvoiceTotals();
                });
        }


        // =====================================================
        // CLEAR ALL
        // =====================================================

        if (clearAllButton) {

            clearAllButton.addEventListener(
                "click",
                function () {

                    getRows().forEach(
                        function (row) {

                            const elements =
                                getRowElements(row);


                            if (!elements.selector) {
                                return;
                            }


                            elements.selector.checked =
                                false;


                            updateRowState(
                                row);
                        });


                    calculateInvoiceTotals();
                });
        }


        // =====================================================
        // ROW EVENTS
        // =====================================================

        getRows().forEach(
            function (row) {

                const elements =
                    getRowElements(row);


                if (elements.selector) {

                    elements.selector.addEventListener(
                        "change",
                        function () {

                            updateRowState(
                                row);
                        });
                }


                if (elements.quantity) {

                    elements.quantity.addEventListener(
                        "input",
                        function () {

                            updateRowCalculation(
                                row);
                        });


                    elements.quantity.addEventListener(
                        "blur",
                        function () {

                            const availableQuantity =
                                getAvailableQuantity(
                                    row);


                            let quantity =
                                parseNumber(
                                    elements.quantity.value);


                            if (quantity < 0) {
                                quantity = 0;
                            }


                            if (quantity >
                                availableQuantity) {

                                quantity =
                                    availableQuantity;
                            }


                            elements.quantity.value =
                                quantity.toFixed(
                                    3);


                            updateRowCalculation(
                                row);
                        });
                }


                if (elements.rate) {

                    elements.rate.addEventListener(
                        "input",
                        function () {

                            let rate =
                                parseNumber(
                                    elements.rate.value);


                            if (rate < 0) {

                                rate =
                                    0;

                                elements.rate.value =
                                    "0.00";
                            }


                            updateRowCalculation(
                                row);
                        });


                    elements.rate.addEventListener(
                        "blur",
                        function () {

                            let rate =
                                parseNumber(
                                    elements.rate.value);


                            if (rate < 0) {
                                rate = 0;
                            }


                            elements.rate.value =
                                rate.toFixed(2);


                            updateRowCalculation(
                                row);
                        });
                }

                /*
                 * Initialize row.
                 */
                updateRowState(
                    row);
            });


        // =====================================================
        // CHARGE EVENTS
        // =====================================================

        [
            transportChargesInput,
            otherChargesInput,
            roundOffInput
        ]
            .filter(Boolean)
            .forEach(
                function (input) {

                    input.addEventListener(
                        "input",
                        calculateInvoiceTotals);
                });


        // =====================================================
        // SUPPLIER INVOICE DATE EVENT
        // =====================================================

        if (supplierInvoiceDateInput) {

            supplierInvoiceDateInput
                .addEventListener(
                    "change",
                    calculateDueDate);
        }


        // =====================================================
        // FORM SUBMIT VALIDATION
        // =====================================================

        if (form && itemsTable) {

            form.addEventListener(
                "submit",
                function (event) {

                    const selectedRows =
                        getRows()
                            .filter(
                                function (row) {

                                    const selector =
                                        row.querySelector(
                                            ".purchase-item-selector");


                                    return selector &&
                                        selector.checked;
                                });


                    if (selectedRows.length === 0) {

                        event.preventDefault();


                        showFormError(
                            "Please select at least one received GRN line for Purchase Invoice.");


                        return;
                    }


                    const invalidQuantityRow =
                        selectedRows.find(
                            function (row) {

                                const elements =
                                    getRowElements(row);


                                if (!elements.quantity) {
                                    return true;
                                }


                                const quantity =
                                    parseNumber(
                                        elements.quantity.value);


                                const available =
                                    getAvailableQuantity(
                                        row);


                                return (
                                    quantity <= 0 ||
                                    quantity >
                                    available
                                );
                            });


                    if (invalidQuantityRow) {

                        event.preventDefault();


                        showFormError(
                            "Purchase Invoice Quantity must be greater than zero and cannot exceed Available Quantity.");


                        const quantityInput =
                            invalidQuantityRow
                                .querySelector(
                                    ".purchase-invoice-quantity");


                        if (quantityInput) {

                            quantityInput.focus();
                        }
                    }
                });
        }


        // =====================================================
        // FORM ERROR
        // =====================================================

        function showFormError(
            message) {

            if (!form) {
                return;
            }


            let alertElement =
                form.querySelector(
                    ".purchase-invoice-client-error");


            if (!alertElement) {

                alertElement =
                    document.createElement(
                        "div");


                alertElement.className =
                    "alert alert-danger purchase-invoice-client-error mb-4";


                const validationSummary =
                    form.querySelector(
                        "[data-valmsg-summary='true']");


                if (
                    validationSummary &&
                    validationSummary.parentNode
                ) {

                    validationSummary.parentNode
                        .insertBefore(
                            alertElement,
                            validationSummary.nextSibling);

                } else {

                    form.prepend(
                        alertElement);
                }
            }


            alertElement.textContent =
                message;


            alertElement.scrollIntoView(
                {
                    behavior:
                        "smooth",

                    block:
                        "center"
                });
        }


        // =====================================================
        // INITIAL CALCULATION
        // =====================================================

        calculateDueDate();

        calculateInvoiceTotals();
    });