/*
============================================================
File: invoice-form.js

Module:
Invoice

Purpose:
Handles Invoice Create / Edit form behaviour.

Responsibilities:
- Load selected Customer Purchase Order through AJAX.
- Auto-load Completed Production Jobs.
- Preserve existing Edit commercial values on initial load.
- Display Production / Already Invoiced / Available Qty.
- Display PDI / Delivery Challan warning.
- Require explicit warning confirmation when applicable.
- Default GST to 18% for newly loaded Production Jobs.
- Validate Invoice Quantity against Available Quantity.
- Calculate Gross / Discount / Taxable / GST / Line Total.
- Automatically determine CGST + SGST or IGST by State.
- Calculate Other Charges, Round Off and Grand Total.
- Re-index dynamic ASP.NET Core collection rows.

Important:
- New Invoice source:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory.
- PDI is NOT mandatory.
- Missing PDI / Delivery Challan is warning-only.
- JavaScript calculations are UI preview only.
- InvoiceService performs authoritative calculations.
============================================================
*/

(function initializeInvoiceForm() {

    "use strict";


    // =========================================================
    // REGION 1 — DOM REFERENCES
    // =========================================================

    const itemsBody =
        document.getElementById(
            "invoiceItemsBody");


    const itemTemplate =
        document.getElementById(
            "invoiceItemTemplate");


    const customerPOSelector =
        document.getElementById(
            "invoiceCustomerPOSelector");


    const noItemsMessage =
        document.getElementById(
            "invoiceNoItemsMessage");


    const sourceLoadMessage =
        document.getElementById(
            "sourceLoadMessage");


    const sourceWarningBox =
        document.getElementById(
            "sourceWarningBox");


    const confirmSourceWarningInput =
        document.getElementById(
            "confirmSourceWarning");


    const customerIdInput =
        document.getElementById(
            "CustomerId");


    const customerNameInput =
        document.getElementById(
            "CustomerName");


    const customerGstinInput =
        document.getElementById(
            "CustomerGstin");


    const companyIdInput =
        document.getElementById(
            "CompanyId");


    const companyStateInput =
        document.getElementById(
            "CompanyState");


    const isInterStateInput =
        document.getElementById(
            "IsInterState");


    const billingAddressLine1Input =
        document.getElementById(
            "BillingAddressLine1");


    const billingAddressLine2Input =
        document.getElementById(
            "BillingAddressLine2");


    const billingDistrictInput =
        document.getElementById(
            "BillingDistrict");


    const billingCityInput =
        document.getElementById(
            "BillingCity");


    const billingStateInput =
        document.getElementById(
            "BillingState");


    const billingPincodeInput =
        document.getElementById(
            "BillingPincode");


    const billingCountryInput =
        document.getElementById(
            "BillingCountry");


    const paymentTermsInput =
        document.getElementById(
            "PaymentTerms");


    const creditDaysInput =
        document.getElementById(
            "CreditDays");


    const placeOfSupplyInput =
        document.getElementById(
            "PlaceOfSupply");


    const gstTransactionTypeInput =
        document.getElementById(
            "gstTransactionType");


    const invoiceDateInput =
        document.getElementById(
            "InvoiceDate");


    const dueDateInput =
        document.getElementById(
            "DueDate");


    const invoiceTermsInput =
        document.getElementById(
            "InvoiceTermsAndConditions");


    const otherChargesInput =
        document.getElementById(
            "OtherCharges");


    const customerPONumbersElement =
        document.getElementById(
            "customerPONumbers");


    /*
     * Defensive guard.
     */
    if (
        !itemsBody ||
        !itemTemplate
    ) {

        console.error(
            "Invoice Item table could not be initialized.");

        return;
    }


    const invoiceId =
        getNumber(
            document.getElementById(
                "Id")
                ?.value);


    const getCustomerPOUrl =
        "/Invoice/GetCustomerPurchaseOrderData";


    // =========================================================
    // REGION 2 — NUMBER HELPERS
    // =========================================================

    function getNumber(value) {

        const normalized =
            (
                value ??
                ""
            )
                .toString()
                .replace(
                    /,/g,
                    "")
                .trim();


        const number =
            parseFloat(
                normalized);


        return Number.isFinite(
            number)
            ? number
            : 0;
    }


    function formatAmount(value) {

        return new Intl.NumberFormat(
            "en-IN",
            {
                minimumFractionDigits:
                    2,

                maximumFractionDigits:
                    2
            })
            .format(
                getNumber(
                    value));
    }


    function formatQuantity(value) {

        const number =
            getNumber(
                value);


        return Number(
            number.toFixed(
                3))
            .toString();
    }


    function formatRate(value) {

        const number =
            getNumber(
                value);


        return Number.isInteger(
            number)
            ? number.toString()
            : number
                .toFixed(
                    2)
                .replace(
                    /0+$/,
                    "")
                .replace(
                    /\.$/,
                    "");
    }


    function roundAmount(value) {

        return Math.round(
            (
                getNumber(
                    value) +
                Number.EPSILON
            ) *
            100
        ) / 100;
    }


    // =========================================================
    // REGION 3 — TEXT HELPERS
    // =========================================================

    function normalizeText(value) {

        return (
            value ??
            ""
        )
            .toString()
            .trim();
    }


    function normalizeState(value) {

        return normalizeText(
            value)
            .replace(
                /\s+/g,
                " ")
            .toUpperCase();
    }


    // =========================================================
    // REGION 4 — CURRENT ROWS
    // =========================================================

    function getRows() {

        return Array.from(
            itemsBody
                .querySelectorAll(
                    ".invoice-item-row"));
    }


    // =========================================================
    // REGION 5 — SOURCE MESSAGE
    // =========================================================

    function showSourceMessage(
        message) {

        if (!sourceLoadMessage) {

            return;
        }


        sourceLoadMessage.textContent =
            message ||
            "Unable to load Customer Purchase Order.";


        sourceLoadMessage
            .classList
            .remove(
                "d-none");
    }


    function clearSourceMessage() {

        if (!sourceLoadMessage) {

            return;
        }


        sourceLoadMessage.textContent =
            "";


        sourceLoadMessage
            .classList
            .add(
                "d-none");
    }


    // =========================================================
    // REGION 6 — GST TYPE
    // =========================================================

    function refreshGSTType() {

        const companyState =
            normalizeState(
                companyStateInput
                    ?.value);


        const billingState =
            normalizeState(
                billingStateInput
                    ?.value);


        /*
         * Incomplete State keeps intra-state preview.
         *
         * InvoiceService performs final validation.
         */
        let isInterState =
            false;


        if (
            companyState &&
            billingState
        ) {

            isInterState =
                companyState !==
                billingState;
        }


        if (isInterStateInput) {

            isInterStateInput.value =
                isInterState
                    ? "true"
                    : "false";
        }


        if (gstTransactionTypeInput) {

            gstTransactionTypeInput.value =
                isInterState
                    ? "IGST"
                    : "CGST + SGST";
        }


        if (placeOfSupplyInput) {

            placeOfSupplyInput.value =
                billingStateInput
                    ?.value ??
                "";
        }


        return isInterState;
    }


    function isInterStateTransaction() {

        return (
            isInterStateInput
                ?.value
                ?.toLowerCase() ===
            "true"
        );
    }


    // =========================================================
    // REGION 7 — GST SUMMARY LABELS
    // =========================================================

    function updateGSTRateLabels() {

        const gstRates =
            getRows()
                .map(
                    function (row) {

                        return getNumber(
                            row
                                .querySelector(
                                    ".gst-input")
                                ?.value);
                    });


        const uniqueRates =
            [
                ...new Set(
                    gstRates.map(
                        function (rate) {

                            return Number(
                                rate.toFixed(
                                    4));
                        })
                )
            ];


        const cgstLabel =
            document.getElementById(
                "summaryCGSTLabel");


        const sgstLabel =
            document.getElementById(
                "summarySGSTLabel");


        const igstLabel =
            document.getElementById(
                "summaryIGSTLabel");


        if (uniqueRates.length === 0) {

            if (cgstLabel) {

                cgstLabel.textContent =
                    "CGST";
            }


            if (sgstLabel) {

                sgstLabel.textContent =
                    "SGST";
            }


            if (igstLabel) {

                igstLabel.textContent =
                    "IGST";
            }


            return;
        }


        if (uniqueRates.length === 1) {

            const gstRate =
                uniqueRates[0];


            const halfRate =
                gstRate /
                2;


            if (cgstLabel) {

                cgstLabel.textContent =
                    "CGST (" +
                    formatRate(
                        halfRate) +
                    "%)";
            }


            if (sgstLabel) {

                sgstLabel.textContent =
                    "SGST (" +
                    formatRate(
                        halfRate) +
                    "%)";
            }


            if (igstLabel) {

                igstLabel.textContent =
                    "IGST (" +
                    formatRate(
                        gstRate) +
                    "%)";
            }


            return;
        }


        if (cgstLabel) {

            cgstLabel.textContent =
                "CGST (Mixed)";
        }


        if (sgstLabel) {

            sgstLabel.textContent =
                "SGST (Mixed)";
        }


        if (igstLabel) {

            igstLabel.textContent =
                "IGST (Mixed)";
        }
    }


    // =========================================================
    // REGION 8 — ROW VALIDATION
    // =========================================================

    function validateRow(row) {

        let isValid =
            true;


        const quantityInput =
            row.querySelector(
                ".quantity-input");


        const rateInput =
            row.querySelector(
                ".rate-input");


        const discountInput =
            row.querySelector(
                ".discount-input");


        const gstInput =
            row.querySelector(
                ".gst-input");


        const availableQuantity =
            getNumber(
                row.dataset
                    .availableQuantity ||
                row
                    .querySelector(
                        ".available-qty")
                    ?.textContent);


        // -----------------------------------------------------
        // Invoice Quantity
        // -----------------------------------------------------

        if (quantityInput) {

            const quantity =
                getNumber(
                    quantityInput.value);


            if (quantity <= 0) {

                quantityInput
                    .setCustomValidity(
                        "Invoice Quantity must be greater than zero.");


                isValid =
                    false;
            }
            else if (
                quantity >
                availableQuantity
            ) {

                quantityInput
                    .setCustomValidity(
                        "Invoice Quantity cannot exceed Available Quantity.");


                isValid =
                    false;
            }
            else {

                quantityInput
                    .setCustomValidity(
                        "");
            }
        }


        // -----------------------------------------------------
        // Rate
        // -----------------------------------------------------

        if (rateInput) {

            const rate =
                getNumber(
                    rateInput.value);


            if (rate <= 0) {

                rateInput
                    .setCustomValidity(
                        "Rate must be greater than zero.");


                isValid =
                    false;
            }
            else {

                rateInput
                    .setCustomValidity(
                        "");
            }
        }


        // -----------------------------------------------------
        // Discount
        // -----------------------------------------------------

        if (discountInput) {

            const discount =
                getNumber(
                    discountInput.value);


            if (
                discount < 0 ||
                discount > 100
            ) {

                discountInput
                    .setCustomValidity(
                        "Discount must be between 0 and 100%.");


                isValid =
                    false;
            }
            else {

                discountInput
                    .setCustomValidity(
                        "");
            }
        }


        // -----------------------------------------------------
        // GST
        // -----------------------------------------------------

        if (gstInput) {

            const gstRate =
                getNumber(
                    gstInput.value);


            if (
                gstRate < 0 ||
                gstRate > 100
            ) {

                gstInput
                    .setCustomValidity(
                        "GST must be between 0 and 100%.");


                isValid =
                    false;
            }
            else {

                gstInput
                    .setCustomValidity(
                        "");
            }
        }


        return isValid;
    }


    // =========================================================
    // REGION 9 — ROW CALCULATION
    // =========================================================

    function calculateRow(row) {

        const quantity =
            getNumber(
                row
                    .querySelector(
                        ".quantity-input")
                    ?.value);


        const rate =
            getNumber(
                row
                    .querySelector(
                        ".rate-input")
                    ?.value);


        const discountPercent =
            getNumber(
                row
                    .querySelector(
                        ".discount-input")
                    ?.value);


        const gstRate =
            getNumber(
                row
                    .querySelector(
                        ".gst-input")
                    ?.value);


        const grossAmount =
            roundAmount(
                quantity *
                rate);


        const discountAmount =
            roundAmount(
                grossAmount *
                discountPercent /
                100);


        const taxableAmount =
            roundAmount(
                grossAmount -
                discountAmount);


        let cgstAmount =
            0;


        let sgstAmount =
            0;


        let igstAmount =
            0;


        if (gstRate > 0) {

            if (isInterStateTransaction()) {

                igstAmount =
                    roundAmount(
                        taxableAmount *
                        gstRate /
                        100);
            }
            else {

                const halfGST =
                    gstRate /
                    2;


                cgstAmount =
                    roundAmount(
                        taxableAmount *
                        halfGST /
                        100);


                sgstAmount =
                    roundAmount(
                        taxableAmount *
                        halfGST /
                        100);
            }
        }


        const totalTax =
            roundAmount(
                cgstAmount +
                sgstAmount +
                igstAmount);


        const lineTotal =
            roundAmount(
                taxableAmount +
                totalTax);


        /*
         * UI-preview values only.
         */
        row.dataset.grossAmount =
            grossAmount;


        row.dataset.discountAmount =
            discountAmount;


        row.dataset.taxableAmount =
            taxableAmount;


        row.dataset.cgstAmount =
            cgstAmount;


        row.dataset.sgstAmount =
            sgstAmount;


        row.dataset.igstAmount =
            igstAmount;


        row.dataset.totalTax =
            totalTax;


        row.dataset.lineTotal =
            lineTotal;


        const taxableElement =
            row.querySelector(
                ".taxable-amount");


        const lineTotalElement =
            row.querySelector(
                ".line-total");


        if (taxableElement) {

            taxableElement.textContent =
                formatAmount(
                    taxableAmount);
        }


        if (lineTotalElement) {

            lineTotalElement.textContent =
                formatAmount(
                    lineTotal);
        }
    }


    // =========================================================
    // REGION 10 — AMOUNT SUMMARY
    // =========================================================

    function calculateSummary() {

        refreshGSTType();

        updateGSTRateLabels();


        let grossAmount =
            0;


        let discountAmount =
            0;


        let taxableAmount =
            0;


        let cgstAmount =
            0;


        let sgstAmount =
            0;


        let igstAmount =
            0;


        getRows()
            .forEach(
                function (row) {

                    calculateRow(
                        row);


                    grossAmount +=
                        getNumber(
                            row.dataset
                                .grossAmount);


                    discountAmount +=
                        getNumber(
                            row.dataset
                                .discountAmount);


                    taxableAmount +=
                        getNumber(
                            row.dataset
                                .taxableAmount);


                    cgstAmount +=
                        getNumber(
                            row.dataset
                                .cgstAmount);


                    sgstAmount +=
                        getNumber(
                            row.dataset
                                .sgstAmount);


                    igstAmount +=
                        getNumber(
                            row.dataset
                                .igstAmount);
                });


        grossAmount =
            roundAmount(
                grossAmount);


        discountAmount =
            roundAmount(
                discountAmount);


        taxableAmount =
            roundAmount(
                taxableAmount);


        cgstAmount =
            roundAmount(
                cgstAmount);


        sgstAmount =
            roundAmount(
                sgstAmount);


        igstAmount =
            roundAmount(
                igstAmount);


        const otherCharges =
            Math.max(
                0,
                getNumber(
                    otherChargesInput
                        ?.value));


        const amountBeforeRoundOff =
            roundAmount(
                taxableAmount +
                cgstAmount +
                sgstAmount +
                igstAmount +
                otherCharges);


        const roundedGrandTotal =
            Math.round(
                amountBeforeRoundOff);


        const roundOffAmount =
            roundAmount(
                roundedGrandTotal -
                amountBeforeRoundOff);


        const grandTotal =
            roundAmount(
                amountBeforeRoundOff +
                roundOffAmount);


        setSummaryAmount(
            "summaryGross",
            grossAmount);


        setSummaryAmount(
            "summaryDiscount",
            discountAmount);


        setSummaryAmount(
            "summaryTaxable",
            taxableAmount);


        setSummaryAmount(
            "summaryCGST",
            cgstAmount);


        setSummaryAmount(
            "summarySGST",
            sgstAmount);


        setSummaryAmount(
            "summaryIGST",
            igstAmount);


        setSummaryAmount(
            "summaryRoundOff",
            roundOffAmount);


        setSummaryAmount(
            "summaryGrandTotal",
            grandTotal);
    }


    function setSummaryAmount(
        elementId,
        value) {

        const element =
            document.getElementById(
                elementId);


        if (!element) {

            return;
        }


        element.textContent =
            "₹ " +
            formatAmount(
                value);
    }


    // =========================================================
    // REGION 11 — DUE DATE
    // =========================================================

    function refreshDueDate() {

        if (
            !invoiceDateInput ||
            !dueDateInput
        ) {

            return;
        }


        const invoiceDateValue =
            invoiceDateInput.value;


        if (!invoiceDateValue) {

            dueDateInput.value =
                "";

            return;
        }


        const creditDays =
            Math.max(
                0,
                parseInt(
                    creditDaysInput
                        ?.value ||
                    "0",
                    10));


        const dateParts =
            invoiceDateValue
                .split("-")
                .map(
                    Number);


        if (dateParts.length !== 3) {

            return;
        }


        const date =
            new Date(
                dateParts[0],
                dateParts[1] - 1,
                dateParts[2]);


        date.setDate(
            date.getDate() +
            creditDays);


        const year =
            date.getFullYear();


        const month =
            String(
                date.getMonth() +
                1)
                .padStart(
                    2,
                    "0");


        const day =
            String(
                date.getDate())
                .padStart(
                    2,
                    "0");


        dueDateInput.value =
            `${year}-${month}-${day}`;
    }


    // =========================================================
    // REGION 12 — APPLY CUSTOMER / BILLING DATA
    // =========================================================

    function applyMasterData(data) {

        /*
         * Edit mode:
         *
         * Saved Customer / Company snapshots must remain.
         *
         * Controller returns:
         * shouldRefreshMasterData = false.
         */
        if (
            data.shouldRefreshMasterData !==
            true
        ) {

            return;
        }


        if (customerIdInput) {

            customerIdInput.value =
                data.customerId ??
                "";
        }


        if (customerNameInput) {

            customerNameInput.value =
                data.customerName ??
                "";
        }


        if (customerGstinInput) {

            customerGstinInput.value =
                data.customerGstin ??
                "";
        }


        if (companyIdInput) {

            companyIdInput.value =
                data.companyId ??
                "";
        }


        if (companyStateInput) {

            companyStateInput.value =
                data.companyState ??
                "";
        }


        if (billingAddressLine1Input) {

            billingAddressLine1Input.value =
                data.billingAddressLine1 ??
                "";
        }


        if (billingAddressLine2Input) {

            billingAddressLine2Input.value =
                data.billingAddressLine2 ??
                "";
        }


        if (billingDistrictInput) {

            billingDistrictInput.value =
                data.billingDistrict ??
                "";
        }


        if (billingCityInput) {

            billingCityInput.value =
                data.billingCity ??
                "";
        }


        if (billingStateInput) {

            billingStateInput.value =
                data.billingState ??
                "";
        }


        if (billingPincodeInput) {

            billingPincodeInput.value =
                data.billingPincode ??
                "";
        }


        if (billingCountryInput) {

            billingCountryInput.value =
                data.billingCountry ??
                "";
        }


        if (paymentTermsInput) {

            paymentTermsInput.value =
                data.paymentTerms ??
                "";
        }


        if (creditDaysInput) {

            creditDaysInput.value =
                data.creditDays ??
                "";
        }


        if (placeOfSupplyInput) {

            placeOfSupplyInput.value =
                data.placeOfSupply ??
                data.billingState ??
                "";
        }


        if (
            invoiceTermsInput &&
            !normalizeText(
                invoiceTermsInput.value)
        ) {

            invoiceTermsInput.value =
                data.invoiceTermsAndConditions ??
                "";
        }


        refreshDueDate();

        refreshGSTType();
    }


    // =========================================================
    // REGION 13 — CLEAR CREATE MASTER DATA
    // =========================================================

    function clearCreateMasterData() {

        /*
         * Historical Edit snapshot must not
         * be cleared.
         */
        if (invoiceId > 0) {

            return;
        }


        if (customerIdInput) {

            customerIdInput.value =
                "";
        }


        if (customerNameInput) {

            customerNameInput.value =
                "";
        }


        if (customerGstinInput) {

            customerGstinInput.value =
                "";
        }


        if (companyIdInput) {

            companyIdInput.value =
                "";
        }


        if (companyStateInput) {

            companyStateInput.value =
                "";
        }


        if (billingAddressLine1Input) {

            billingAddressLine1Input.value =
                "";
        }


        if (billingAddressLine2Input) {

            billingAddressLine2Input.value =
                "";
        }


        if (billingDistrictInput) {

            billingDistrictInput.value =
                "";
        }


        if (billingCityInput) {

            billingCityInput.value =
                "";
        }


        if (billingStateInput) {

            billingStateInput.value =
                "";
        }


        if (billingPincodeInput) {

            billingPincodeInput.value =
                "";
        }


        if (billingCountryInput) {

            billingCountryInput.value =
                "";
        }


        if (paymentTermsInput) {

            paymentTermsInput.value =
                "";
        }


        if (creditDaysInput) {

            creditDaysInput.value =
                "";
        }


        if (placeOfSupplyInput) {

            placeOfSupplyInput.value =
                "";
        }


        if (dueDateInput) {

            dueDateInput.value =
                "";
        }


        if (invoiceTermsInput) {

            invoiceTermsInput.value =
                "";
        }


        if (isInterStateInput) {

            isInterStateInput.value =
                "false";
        }


        if (gstTransactionTypeInput) {

            gstTransactionTypeInput.value =
                "CGST + SGST";
        }
    }


    // =========================================================
    // REGION 14 — ROW DISPLAY HELPERS
    // =========================================================

    function setRowText(
        row,
        selector,
        value) {

        const element =
            row.querySelector(
                selector);


        if (!element) {

            return;
        }


        element.textContent =
            value ??
            "";
    }


    function setRowInputValue(
        row,
        selector,
        value) {

        const element =
            row.querySelector(
                selector);


        if (!element) {

            return;
        }


        element.value =
            value ??
            "";
    }

    function ensureHiddenSourceInput(
        row,
        fieldName,
        cssClass,
        value) {

        let input =
            row.querySelector(
                cssClass);


        if (!input) {

            input =
                row.querySelector(
                    `input[name$=".${fieldName}"]`);
        }


        if (!input) {

            const rowIndex =
                getRows()
                    .indexOf(
                        row);


            input =
                document.createElement(
                    "input");


            input.type =
                "hidden";


            input.name =
                `Items[${rowIndex}].${fieldName}`;


            input.className =
                cssClass.replace(
                    ".",
                    "");


            const firstCell =
                row.querySelector(
                    "td");


            if (firstCell) {

                firstCell.appendChild(
                    input);
            }
            else {

                row.appendChild(
                    input);
            }
        }


        input.value =
            value ??
            "";


        return input;
    }


    function getProductionSourceKey(
        productionJobId,
        customerPurchaseOrderItemId) {

        return (
            getNumber(
                productionJobId)
            +
            ":"
            +
            getNumber(
                customerPurchaseOrderItemId)
        );
    }

    function setOptionalRowText(
        row,
        rowSelector,
        valueSelector,
        value) {

        const wrapper =
            row.querySelector(
                rowSelector);


        const valueElement =
            row.querySelector(
                valueSelector);


        const text =
            normalizeText(
                value);


        if (valueElement) {

            valueElement.textContent =
                text;
        }


        wrapper
            ?.classList
            .toggle(
                "d-none",
                !text);
    }


    // =========================================================
    // REGION 15 — APPEND PRODUCTION JOB
    // =========================================================

    function appendProductionJob(
        itemData) {

        const index =
            getRows()
                .length;


        const lineNumber =
            index +
            1;


        let templateHtml =
            itemTemplate
                .innerHTML;


        templateHtml =
            templateHtml
                .replaceAll(
                    "__index__",
                    index.toString());


        templateHtml =
            templateHtml
                .replaceAll(
                    "__lineNumber__",
                    lineNumber.toString());


        itemsBody
            .insertAdjacentHTML(
                "beforeend",
                templateHtml);


        const rows =
            getRows();


        const row =
            rows[
            rows.length - 1
            ];


        if (!row) {

            return;
        }


        // =====================================================
        // SOURCE IDENTITY
        // =====================================================

        const productionJobId =
            getNumber(
                itemData.productionJobId);


        const customerPurchaseOrderItemId =
            getNumber(
                itemData.customerPurchaseOrderItemId);


        row.dataset.productionJobId =
            productionJobId;


        row.dataset.customerPurchaseOrderItemId =
            customerPurchaseOrderItemId;


        row.dataset.customerPoNumber =
            itemData.customerPurchaseOrderNumber
            ??
            "";


        row.dataset.requiresWarning =
            itemData.requiresWarning
                ? "true"
                : "false";


        /*
         * IMPORTANT:
         *
         * Invoice line source is:
         *
         * ProductionJobId
         * +
         * CustomerPurchaseOrderItemId
         *
         * If template does not contain these hidden inputs,
         * JavaScript creates them automatically.
         */

        ensureHiddenSourceInput(
            row,
            "ProductionJobId",
            ".production-job-id",
            productionJobId);


        ensureHiddenSourceInput(
            row,
            "CustomerPurchaseOrderItemId",
            ".customer-po-item-id",
            customerPurchaseOrderItemId);


        // =====================================================
        // PRODUCTION JOB DISPLAY
        // =====================================================

        const productionJobCode =
            normalizeText(
                itemData.productionJobCode);


        setRowText(
            row,
            ".production-job-code",
            productionJobCode
            ||
            (
                productionJobId > 0
                    ? productionJobId.toString()
                    : "-"
            ));


        // =====================================================
        // WARNING
        // =====================================================

        const warningLabel =
            row.querySelector(
                ".source-warning-label");


        warningLabel
            ?.classList
            .toggle(
                "d-none",
                itemData.requiresWarning !==
                true);


        // =====================================================
        // ITEM / PRODUCT
        // =====================================================

        const itemCode =
            normalizeText(
                itemData.itemCode);


        const itemName =
            normalizeText(
                itemData.itemName);


        /*
         * Prefer Item Name.
         *
         * If Item Name is unavailable,
         * Item Code is still shown instead of blank.
         */
        const itemDisplay =
            itemName
            ||
            itemCode
            ||
            "-";


        setRowText(
            row,
            ".item-name",
            itemDisplay);


        setOptionalRowText(
            row,
            ".item-code-row",
            ".item-code",
            itemCode);


        setOptionalRowText(
            row,
            ".product-reference-row",
            ".product-reference",
            itemData.productReference);


        // =====================================================
        // HSN
        // =====================================================

        setRowText(
            row,
            ".hsn-number",
            normalizeText(
                itemData.hsnNumber)
            ||
            "-");


        // =====================================================
        // PRODUCTION QUANTITY
        // =====================================================

        const productionQuantity =
            getNumber(
                itemData.productionQuantity);


        setRowText(
            row,
            ".production-qty",
            formatQuantity(
                productionQuantity));


        row.dataset.productionQuantity =
            productionQuantity;


        // =====================================================
        // ALREADY INVOICED
        // =====================================================

        const alreadyInvoicedQuantity =
            getNumber(
                itemData.alreadyInvoicedQuantity);


        setRowText(
            row,
            ".already-invoiced-qty",
            formatQuantity(
                alreadyInvoicedQuantity));


        row.dataset.alreadyInvoicedQuantity =
            alreadyInvoicedQuantity;


        // =====================================================
        // AVAILABLE QUANTITY
        // =====================================================

        const availableQuantity =
            getNumber(
                itemData.availableQuantity);


        setRowText(
            row,
            ".available-qty",
            formatQuantity(
                availableQuantity));


        row.dataset.availableQuantity =
            availableQuantity;


        const quantityInput =
            row.querySelector(
                ".quantity-input");


        if (quantityInput) {

            quantityInput.value =
                formatQuantity(
                    availableQuantity);


            quantityInput.max =
                formatQuantity(
                    availableQuantity);
        }


        // =====================================================
        // UOM
        // =====================================================

        setRowText(
            row,
            ".item-uom",
            normalizeText(
                itemData.unitName)
            ||
            "-");


        // =====================================================
        // COMMERCIAL DEFAULTS
        // =====================================================

        const rateInput =
            row.querySelector(
                ".rate-input");


        if (rateInput) {

            rateInput.value =
                "";
        }


        const discountInput =
            row.querySelector(
                ".discount-input");


        if (discountInput) {

            discountInput.value =
                "0";
        }


        const gstInput =
            row.querySelector(
                ".gst-input");


        if (gstInput) {

            gstInput.value =
                "18";
        }


        validateRow(
            row);


        calculateRow(
            row);
    }


    // =========================================================
    // REGION 16 — REPLACE PRODUCTION JOB ROWS
    // =========================================================

    function replaceProductionJobRows(
        sourceItems) {

        itemsBody.innerHTML =
            "";


        const items =
            Array.isArray(
                sourceItems)
                ? sourceItems
                : [];


        items.forEach(
            function (item) {

                appendProductionJob(
                    item);
            });


        reindexRows();

        refreshCustomerPONumbers();

        refreshNoItemsMessage();

        refreshSourceWarning();

        refreshUnobtrusiveValidation();

        calculateSummary();
    }


    // =========================================================
    // REGION 17 — REFRESH EXISTING EDIT ROWS
    // =========================================================

    function refreshExistingRows(
        sourceItems) {

        const items =
            Array.isArray(
                sourceItems)
                ? sourceItems
                : [];


        const sourceMap =
            new Map();


        items.forEach(
            function (item) {

                const productionJobId =
                    getNumber(
                        item.productionJobId);


                const customerPurchaseOrderItemId =
                    getNumber(
                        item.customerPurchaseOrderItemId);


                if (
                    productionJobId > 0
                    &&
                    customerPurchaseOrderItemId > 0
                ) {

                    const sourceKey =
                        getProductionSourceKey(
                            productionJobId,
                            customerPurchaseOrderItemId);


                    sourceMap.set(
                        sourceKey,
                        item);
                }
            });


        getRows()
            .forEach(
                function (row) {

                    const productionJobId =
                        getNumber(
                            row.dataset.productionJobId
                            ||
                            row
                                .querySelector(
                                    ".production-job-id")
                                ?.value);


                    const customerPurchaseOrderItemId =
                        getNumber(
                            row.dataset.customerPurchaseOrderItemId
                            ||
                            row
                                .querySelector(
                                    ".customer-po-item-id")
                                ?.value
                            ||
                            row
                                .querySelector(
                                    'input[name$=".CustomerPurchaseOrderItemId"]')
                                ?.value);


                    if (
                        productionJobId <= 0
                        ||
                        customerPurchaseOrderItemId <= 0
                    ) {

                        return;
                    }


                    const sourceKey =
                        getProductionSourceKey(
                            productionJobId,
                            customerPurchaseOrderItemId);


                    const sourceItem =
                        sourceMap.get(
                            sourceKey);


                    if (!sourceItem) {

                        return;
                    }


                    row.dataset.productionJobId =
                        productionJobId;


                    row.dataset.customerPurchaseOrderItemId =
                        customerPurchaseOrderItemId;


                    // =========================================
                    // Source IDs
                    // =========================================

                    ensureHiddenSourceInput(
                        row,
                        "ProductionJobId",
                        ".production-job-id",
                        productionJobId);


                    ensureHiddenSourceInput(
                        row,
                        "CustomerPurchaseOrderItemId",
                        ".customer-po-item-id",
                        customerPurchaseOrderItemId);


                    // =========================================
                    // Job
                    // =========================================

                    setRowText(
                        row,
                        ".production-job-code",
                        normalizeText(
                            sourceItem.productionJobCode)
                        ||
                        productionJobId.toString());


                    // =========================================
                    // Item
                    // =========================================

                    const itemCode =
                        normalizeText(
                            sourceItem.itemCode);


                    const itemName =
                        normalizeText(
                            sourceItem.itemName);


                    setRowText(
                        row,
                        ".item-name",
                        itemName
                        ||
                        itemCode
                        ||
                        "-");


                    setOptionalRowText(
                        row,
                        ".item-code-row",
                        ".item-code",
                        itemCode);


                    setOptionalRowText(
                        row,
                        ".product-reference-row",
                        ".product-reference",
                        sourceItem.productReference);


                    setRowText(
                        row,
                        ".hsn-number",
                        normalizeText(
                            sourceItem.hsnNumber)
                        ||
                        "-");


                    setRowText(
                        row,
                        ".item-uom",
                        normalizeText(
                            sourceItem.unitName)
                        ||
                        "-");


                    // =========================================
                    // Warning
                    // =========================================

                    row.dataset.requiresWarning =
                        sourceItem.requiresWarning
                            ? "true"
                            : "false";


                    const warningLabel =
                        row.querySelector(
                            ".source-warning-label");


                    warningLabel
                        ?.classList
                        .toggle(
                            "d-none",
                            sourceItem.requiresWarning !==
                            true);


                    // =========================================
                    // Production Qty
                    // =========================================

                    const productionQuantity =
                        getNumber(
                            sourceItem.productionQuantity);


                    row.dataset.productionQuantity =
                        productionQuantity;


                    setRowText(
                        row,
                        ".production-qty",
                        formatQuantity(
                            productionQuantity));


                    // =========================================
                    // Already Invoiced
                    // =========================================

                    const alreadyInvoicedQuantity =
                        getNumber(
                            sourceItem.alreadyInvoicedQuantity);


                    row.dataset.alreadyInvoicedQuantity =
                        alreadyInvoicedQuantity;


                    setRowText(
                        row,
                        ".already-invoiced-qty",
                        formatQuantity(
                            alreadyInvoicedQuantity));


                    // =========================================
                    // Available Qty
                    // =========================================

                    const availableQuantity =
                        getNumber(
                            sourceItem.availableQuantity);


                    row.dataset.availableQuantity =
                        availableQuantity;


                    setRowText(
                        row,
                        ".available-qty",
                        formatQuantity(
                            availableQuantity));


                    const quantityInput =
                        row.querySelector(
                            ".quantity-input");


                    if (quantityInput) {

                        quantityInput.max =
                            formatQuantity(
                                availableQuantity);
                    }


                    validateRow(
                        row);
                });


        refreshCustomerPONumbers();

        refreshSourceWarning();

        calculateSummary();
    }


    // =========================================================
    // REGION 18 — CUSTOMER PO NUMBERS
    // =========================================================

    function refreshCustomerPONumbers() {

        if (!customerPONumbersElement) {

            return;
        }


        const poNumbers =
            getRows()
                .map(
                    function (row) {

                        return normalizeText(
                            row.dataset
                                .customerPoNumber);
                    })
                .filter(
                    function (value) {

                        return !!value;
                    });


        const uniquePONumbers =
            [
                ...new Set(
                    poNumbers)
            ];


        if (uniquePONumbers.length === 0) {

            customerPONumbersElement
                .textContent =
                "-";

            return;
        }


        customerPONumbersElement
            .textContent =
            uniquePONumbers.join(
                ", ");
    }


    function applyCustomerPONumberFromSource(
        result) {

        if (!customerPONumbersElement) {

            return;
        }


        const poNumber =
            normalizeText(
                result.customerPurchaseOrderNumber)
            ||
            normalizeText(
                result.customerPurchaseOrderCode)
            ||
            "-";


        customerPONumbersElement
            .textContent =
            poNumber;
    }


    // =========================================================
    // REGION 19 — SOURCE WARNING
    // =========================================================

    function hasSourceWarning() {

        return getRows()
            .some(
                function (row) {

                    return (
                        row.dataset
                            .requiresWarning ===
                        "true"
                    );
                });
    }


    function refreshSourceWarning() {

        const requiresWarning =
            hasSourceWarning();


        sourceWarningBox
            ?.classList
            .toggle(
                "d-none",
                !requiresWarning);


        if (confirmSourceWarningInput) {

            if (!requiresWarning) {

                confirmSourceWarningInput.checked =
                    false;


                confirmSourceWarningInput
                    .setCustomValidity(
                        "");
            }
            else {

                /*
                 * Do not automatically uncheck here.
                 *
                 * This preserves confirmation after
                 * server-side validation redisplays form.
                 */
                confirmSourceWarningInput
                    .setCustomValidity(
                        "");
            }
        }
    }


    // =========================================================
    // REGION 20 — LOAD CUSTOMER PURCHASE ORDER
    // =========================================================

    async function loadCustomerPurchaseOrder(
        replaceExistingRows) {

        clearSourceMessage();


        const customerPurchaseOrderId =
            getNumber(
                customerPOSelector
                    ?.value);


        if (customerPurchaseOrderId <= 0) {

            if (replaceExistingRows) {

                itemsBody.innerHTML =
                    "";


                refreshNoItemsMessage();

                refreshCustomerPONumbers();

                refreshSourceWarning();

                calculateSummary();


                if (invoiceId <= 0) {

                    clearCreateMasterData();
                }
            }


            return;
        }


        if (customerPOSelector) {

            customerPOSelector.disabled =
                true;
        }


        try {

            let url =
                getCustomerPOUrl +
                "?id=" +
                encodeURIComponent(
                    customerPurchaseOrderId);


            if (invoiceId > 0) {

                url +=
                    "&invoiceId=" +
                    encodeURIComponent(
                        invoiceId);
            }


            /*
             * Quantity availability must not use
             * stale browser cache.
             */
            url +=
                "&_=" +
                Date.now();


            const response =
                await fetch(
                    url,
                    {
                        method:
                            "GET",

                        cache:
                            "no-store",

                        headers:
                        {
                            "X-Requested-With":
                                "XMLHttpRequest",

                            "Accept":
                                "application/json"
                        }
                    });


            if (!response.ok) {

                throw new Error(
                    "Unable to load Customer Purchase Order.");
            }


            const result =
                await response.json();


            if (!result.success) {

                showSourceMessage(
                    result.message ||
                    "Selected Customer Purchase Order is not available.");


                if (replaceExistingRows) {

                    itemsBody.innerHTML =
                        "";


                    refreshNoItemsMessage();

                    refreshCustomerPONumbers();

                    refreshSourceWarning();

                    calculateSummary();
                }


                return;
            }


            /*
             * Create:
             * Load current Customer / Billing Master snapshot.
             *
             * Edit:
             * Controller says shouldRefreshMasterData=false,
             * so saved historical Invoice snapshot remains.
             */
            applyMasterData(
                result);


            applyCustomerPONumberFromSource(
                result);


            if (replaceExistingRows) {

                replaceProductionJobRows(
                    result.items);
            }
            else {

                /*
                 * Initial Edit / validation redisplay.
                 *
                 * Keep:
                 * - Invoice Qty
                 * - Rate
                 * - Discount
                 * - GST
                 *
                 * Refresh only trusted source availability
                 * and warning status.
                 */
                refreshExistingRows(
                    result.items);
            }
        }
        catch (error) {

            console.error(
                "Unable to load Customer Purchase Order.",
                error);


            showSourceMessage(
                "Unable to load Customer Purchase Order. Please try again.");
        }
        finally {

            if (customerPOSelector) {

                customerPOSelector.disabled =
                    false;
            }
        }
    }


    // =========================================================
    // REGION 21 — REMOVE ROW
    // =========================================================

    function removeRow(row) {

        row.remove();


        reindexRows();

        refreshCustomerPONumbers();

        refreshNoItemsMessage();

        refreshSourceWarning();

        refreshUnobtrusiveValidation();

        calculateSummary();
    }


    // =========================================================
    // REGION 22 — REINDEX ROWS
    // =========================================================

    function reindexRows() {

        const rows =
            getRows();


        rows.forEach(
            function (
                row,
                index) {

                const lineNumber =
                    index +
                    1;


                const lineNumberElement =
                    row.querySelector(
                        ".line-number");


                if (lineNumberElement) {

                    lineNumberElement
                        .textContent =
                        lineNumber.toString();
                }


                const sequenceInput =
                    row.querySelector(
                        ".sequence-input");


                if (sequenceInput) {

                    sequenceInput.value =
                        lineNumber.toString();
                }


                // ---------------------------------------------
                // Name
                // ---------------------------------------------

                row
                    .querySelectorAll(
                        "[name]")
                    .forEach(
                        function (element) {

                            element.name =
                                element.name
                                    .replace(
                                        /Items\[\d+\]/g,
                                        "Items[" +
                                        index +
                                        "]");
                        });


                // ---------------------------------------------
                // Id
                // ---------------------------------------------

                row
                    .querySelectorAll(
                        "[id]")
                    .forEach(
                        function (element) {

                            element.id =
                                element.id
                                    .replace(
                                        /Items_\d+__/g,
                                        "Items_" +
                                        index +
                                        "__");
                        });


                // ---------------------------------------------
                // Validation
                // ---------------------------------------------

                row
                    .querySelectorAll(
                        "[data-valmsg-for]")
                    .forEach(
                        function (element) {

                            const value =
                                element.getAttribute(
                                    "data-valmsg-for");


                            if (!value) {

                                return;
                            }


                            element
                                .setAttribute(
                                    "data-valmsg-for",
                                    value.replace(
                                        /Items\[\d+\]/g,
                                        "Items[" +
                                        index +
                                        "]"));
                        });
            });
    }


    // =========================================================
    // REGION 23 — NO ITEMS MESSAGE
    // =========================================================

    function refreshNoItemsMessage() {

        if (!noItemsMessage) {

            return;
        }


        noItemsMessage
            .classList
            .toggle(
                "d-none",
                getRows().length >
                0);
    }


    // =========================================================
    // REGION 24 — UNOBTRUSIVE VALIDATION
    // =========================================================

    function refreshUnobtrusiveValidation() {

        if (
            typeof window.jQuery ===
            "undefined"
        ) {

            return;
        }


        const $ =
            window.jQuery;


        if (
            !$.validator ||
            !$.validator.unobtrusive
        ) {

            return;
        }


        const form =
            itemsBody.closest(
                "form");


        if (!form) {

            return;
        }


        const $form =
            $(form);


        $form.removeData(
            "validator");


        $form.removeData(
            "unobtrusiveValidation");


        $.validator
            .unobtrusive
            .parse(
                $form);
    }


    // =========================================================
    // REGION 25 — EXISTING ROW INITIALIZATION
    // =========================================================

    function initializeExistingRows() {

        getRows()
            .forEach(
                function (row) {

                    const productionJobId =
                        getNumber(
                            row
                                .querySelector(
                                    ".production-job-id")
                                ?.value);


                    if (productionJobId > 0) {

                        row.dataset.productionJobId =
                            productionJobId;
                    }


                    const availableQuantity =
                        getNumber(
                            row
                                .querySelector(
                                    ".available-qty")
                                ?.textContent);


                    row.dataset.availableQuantity =
                        availableQuantity;


                    const productionQuantity =
                        getNumber(
                            row
                                .querySelector(
                                    ".production-qty")
                                ?.textContent);


                    row.dataset.productionQuantity =
                        productionQuantity;


                    const alreadyInvoicedQuantity =
                        getNumber(
                            row
                                .querySelector(
                                    ".already-invoiced-qty")
                                ?.textContent);


                    row.dataset.alreadyInvoicedQuantity =
                        alreadyInvoicedQuantity;


                    const quantityInput =
                        row.querySelector(
                            ".quantity-input");


                    if (
                        quantityInput &&
                        availableQuantity > 0
                    ) {

                        quantityInput.max =
                            formatQuantity(
                                availableQuantity);
                    }


                    /*
                     * Create prepared row:
                     * default GST = 18.
                     *
                     * Existing Edit row retains saved GST.
                     */
                    const gstInput =
                        row.querySelector(
                            ".gst-input");


                    if (
                        invoiceId <= 0 &&
                        gstInput &&
                        getNumber(
                            gstInput.value) ===
                        0
                    ) {

                        gstInput.value =
                            "18";
                    }


                    validateRow(
                        row);


                    calculateRow(
                        row);
                });
    }


    // =========================================================
    // REGION 26 — WARNING CONFIRMATION VALIDATION
    // =========================================================

    function validateSourceWarning() {

        if (!hasSourceWarning()) {

            if (confirmSourceWarningInput) {

                confirmSourceWarningInput
                    .setCustomValidity(
                        "");
            }


            return true;
        }


        if (
            confirmSourceWarningInput &&
            confirmSourceWarningInput.checked
        ) {

            confirmSourceWarningInput
                .setCustomValidity(
                    "");


            return true;
        }


        if (confirmSourceWarningInput) {

            confirmSourceWarningInput
                .setCustomValidity(
                    "Please confirm the PDI / Delivery Challan warning to continue.");
        }


        return false;
    }


    // =========================================================
    // REGION 27 — FORM SUBMISSION VALIDATION
    // =========================================================

    function validateBeforeSubmit(
        event) {

        const rows =
            getRows();


        if (rows.length === 0) {

            event.preventDefault();


            window.alert(
                "Please select a Customer Purchase Order with Completed Production quantity.");


            customerPOSelector
                ?.focus();


            return;
        }


        let isValid =
            true;


        let firstInvalidInput =
            null;


        rows.forEach(
            function (row) {

                if (!validateRow(
                    row)) {

                    isValid =
                        false;


                    if (!firstInvalidInput) {

                        firstInvalidInput =
                            row.querySelector(
                                ":invalid");
                    }
                }
            });


        if (otherChargesInput) {

            const otherCharges =
                getNumber(
                    otherChargesInput.value);


            if (otherCharges < 0) {

                otherChargesInput
                    .setCustomValidity(
                        "Other Charges cannot be negative.");


                isValid =
                    false;


                firstInvalidInput =
                    firstInvalidInput ||
                    otherChargesInput;
            }
            else {

                otherChargesInput
                    .setCustomValidity(
                        "");
            }
        }


        if (!validateSourceWarning()) {

            isValid =
                false;


            firstInvalidInput =
                firstInvalidInput ||
                confirmSourceWarningInput;
        }


        if (!isValid) {

            event.preventDefault();


            firstInvalidInput
                ?.reportValidity();


            firstInvalidInput
                ?.focus();


            return;
        }


        calculateSummary();
    }


    // =========================================================
    // REGION 28 — CUSTOMER PO CHANGE EVENT
    // =========================================================

    customerPOSelector
        ?.addEventListener(
            "change",
            function () {

                /*
                 * Changing source means previous warning
                 * confirmation no longer applies.
                 */
                if (confirmSourceWarningInput) {

                    confirmSourceWarningInput.checked =
                        false;


                    confirmSourceWarningInput
                        .setCustomValidity(
                            "");
                }


                /*
                 * User intentionally selected another PO.
                 *
                 * Old Production Jobs must not remain.
                 */
                itemsBody.innerHTML =
                    "";


                refreshNoItemsMessage();

                refreshCustomerPONumbers();

                refreshSourceWarning();

                calculateSummary();


                loadCustomerPurchaseOrder(
                    true);
            });


    // =========================================================
    // REGION 29 — LIVE ROW CALCULATION EVENT
    // =========================================================

    itemsBody
        .addEventListener(
            "input",
            function (event) {

                const target =
                    event.target;


                if (!(target instanceof
                    HTMLElement)) {

                    return;
                }


                if (!target
                    .classList
                    .contains(
                        "calc-field")) {

                    return;
                }


                const row =
                    target.closest(
                        ".invoice-item-row");


                if (!row) {

                    return;
                }


                validateRow(
                    row);


                calculateRow(
                    row);


                calculateSummary();
            });


    // =========================================================
    // REGION 30 — REMOVE ITEM EVENT
    // =========================================================

    itemsBody
        .addEventListener(
            "click",
            function (event) {

                const target =
                    event.target;


                if (!(target instanceof
                    HTMLElement)) {

                    return;
                }


                const removeButton =
                    target.closest(
                        ".remove-item-row");


                if (!removeButton) {

                    return;
                }


                const row =
                    removeButton.closest(
                        ".invoice-item-row");


                if (!row) {

                    return;
                }


                removeRow(
                    row);
            });


    // =========================================================
    // REGION 31 — WARNING CHECKBOX EVENT
    // =========================================================

    confirmSourceWarningInput
        ?.addEventListener(
            "change",
            function () {

                if (
                    confirmSourceWarningInput.checked
                ) {

                    confirmSourceWarningInput
                        .setCustomValidity(
                            "");
                }
            });


    // =========================================================
    // REGION 32 — BILLING STATE EVENT
    // =========================================================

    billingStateInput
        ?.addEventListener(
            "input",
            function () {

                refreshGSTType();

                calculateSummary();
            });


    // =========================================================
    // REGION 33 — INVOICE DATE EVENT
    // =========================================================

    invoiceDateInput
        ?.addEventListener(
            "change",
            refreshDueDate);


    // =========================================================
    // REGION 34 — OTHER CHARGES EVENT
    // =========================================================

    otherChargesInput
        ?.addEventListener(
            "input",
            function () {

                const rawValue =
                    normalizeText(
                        otherChargesInput.value);


                const value =
                    getNumber(
                        rawValue);


                /*
                 * getNumber converts invalid / blank to zero,
                 * but negative values remain negative.
                 */
                if (
                    rawValue &&
                    value < 0
                ) {

                    otherChargesInput
                        .setCustomValidity(
                            "Other Charges cannot be negative.");
                }
                else {

                    otherChargesInput
                        .setCustomValidity(
                            "");
                }


                calculateSummary();
            });


    // =========================================================
    // REGION 35 — FORM SUBMIT EVENT
    // =========================================================

    const invoiceForm =
        itemsBody.closest(
            "form");


    invoiceForm
        ?.addEventListener(
            "submit",
            validateBeforeSubmit);


    // =========================================================
    // REGION 36 — INITIALIZATION
    // =========================================================

    initializeExistingRows();

    reindexRows();

    refreshCustomerPONumbers();

    refreshNoItemsMessage();

    refreshSourceWarning();

    refreshGSTType();

    calculateSummary();


    /*
     * Selected PO + existing rows:
     *
     * Edit / server-side redisplay.
     *
     * DO NOT replace existing commercial values.
     * Refresh source quantity and warning only.
     */
    if (
        customerPOSelector &&
        getNumber(
            customerPOSelector.value) > 0 &&
        getRows().length > 0
    ) {

        loadCustomerPurchaseOrder(
            false);
    }


    /*
     * Selected PO but no rows:
     *
     * Load all Completed Production Jobs.
     */
    else if (
        customerPOSelector &&
        getNumber(
            customerPOSelector.value) > 0
    ) {

        loadCustomerPurchaseOrder(
            true);
    }

})();