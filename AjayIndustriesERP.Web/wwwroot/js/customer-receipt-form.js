/*
============================================================
File: customer-receipt-form.js

Module:
Customer Receipt

Purpose:
Handles Create / Edit Customer Receipt UI behavior.

Responsibilities:
- Load Customer outstanding Finalized Invoices.
- Add Invoice allocation rows.
- Prevent duplicate Invoice allocation.
- Support partial / full payment.
- Calculate Balance After Receipt.
- Calculate Total Received Amount.
- Refresh trusted Invoice snapshots during Edit.
- Reindex ASP.NET Core collection fields.
- Show / hide Payment Mode fields.
- Perform browser-side allocation validation.

Important:
- Browser values are display / convenience values only.
- Service layer remains authoritative for:
  Invoice Total,
  Already Received,
  Outstanding,
  Allocation validation.
============================================================
*/

document.addEventListener(
    "DOMContentLoaded",
    function () {

        "use strict";


        // =====================================================
        // REGION 1 — ELEMENTS
        // =====================================================

        const customerSelector =
            document.getElementById(
                "customerReceiptCustomerSelector");


        const availableInvoiceSelector =
            document.getElementById(
                "availableInvoiceSelector");


        const addInvoiceButton =
            document.getElementById(
                "addInvoiceButton");


        const allocationBody =
            document.getElementById(
                "customerReceiptAllocationBody");


        const allocationTemplate =
            document.getElementById(
                "customerReceiptAllocationTemplate");


        const noAllocationMessage =
            document.getElementById(
                "noAllocationMessage");


        const invoiceSourceMessage =
            document.getElementById(
                "invoiceSourceMessage");


        const totalReceivedAmount =
            document.getElementById(
                "totalReceivedAmount");


        const paymentModeSelector =
            document.getElementById(
                "paymentModeSelector");


        const referenceNumberSection =
            document.getElementById(
                "referenceNumberSection");


        const bankNameSection =
            document.getElementById(
                "bankNameSection");


        const chequeNumberSection =
            document.getElementById(
                "chequeNumberSection");


        const chequeDateSection =
            document.getElementById(
                "chequeDateSection");


        const referenceNumber =
            document.getElementById(
                "referenceNumber");


        const bankName =
            document.getElementById(
                "bankName");


        const chequeNumber =
            document.getElementById(
                "chequeNumber");


        const chequeDate =
            document.getElementById(
                "chequeDate");


        const form =
            customerSelector
                ?.closest("form");


        // =====================================================
        // REGION 2 — STATE
        // =====================================================

        let availableInvoices =
            [];


        let isLoadingInvoices =
            false;


        // =====================================================
        // REGION 3 — INITIALIZATION
        // =====================================================

        initialize();


        function initialize() {

            updatePaymentModeFields();

            attachExistingRowEvents();

            recalculateAllRows();

            reindexRows();

            updateNoAllocationMessage();


            if (
                customerSelector &&
                getPositiveInteger(
                    customerSelector.value) > 0
            ) {

                /*
                 * Edit:
                 * Existing rows must NOT be removed.
                 *
                 * Create / validation redisplay:
                 * Posted rows must also be preserved.
                 */
                loadCustomerInvoices(
                    false);
            }
            else {

                resetInvoiceSelector();

            }

        }


        // =====================================================
        // REGION 4 — CUSTOMER CHANGE
        // =====================================================

        customerSelector?.addEventListener(
            "change",
            function () {

                /*
                 * Customer changed intentionally.
                 *
                 * Old Invoice allocations belong to the
                 * previous Customer and must be cleared.
                 */
                clearAllAllocations();

                availableInvoices =
                    [];

                resetInvoiceSelector();

                clearSourceMessage();


                const customerId =
                    getPositiveInteger(
                        customerSelector.value);


                if (customerId <= 0) {

                    showSourceMessage(
                        "Please select a Customer.",
                        "info");

                    return;

                }


                loadCustomerInvoices(
                    true);

            });


        // =====================================================
        // REGION 5 — LOAD CUSTOMER INVOICES
        // =====================================================

        async function loadCustomerInvoices(
            customerChanged) {

            if (
                !customerSelector ||
                !availableInvoiceSelector
            ) {
                return;
            }


            const customerId =
                getPositiveInteger(
                    customerSelector.value);


            if (customerId <= 0) {

                resetInvoiceSelector();

                return;

            }


            if (isLoadingInvoices) {
                return;
            }


            isLoadingInvoices =
                true;


            setInvoiceLoadingState(
                true);


            clearSourceMessage();


            try {

                const customerReceiptId =
                    getCurrentReceiptId();


                let url =
                    `/CustomerReceipt/GetCustomerInvoices?customerId=${encodeURIComponent(customerId)}`;


                if (customerReceiptId > 0) {

                    url +=
                        `&customerReceiptId=${encodeURIComponent(customerReceiptId)}`;

                }


                const response =
                    await fetch(
                        url,
                        {
                            method:
                                "GET",

                            headers:
                            {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        });


                if (!response.ok) {

                    throw new Error(
                        "Unable to load Customer Invoices.");

                }


                const result =
                    await response.json();


                if (!result.success) {

                    availableInvoices =
                        [];

                    populateInvoiceSelector();


                    showSourceMessage(
                        result.message ||
                        "Unable to load Customer Invoices.",
                        "danger");

                    return;

                }


                availableInvoices =
                    Array.isArray(
                        result.invoices)

                        ? result.invoices

                        : [];


                /*
                 * On initial Edit / validation redisplay,
                 * refresh live Invoice snapshots without
                 * replacing user-entered Allocated Amount.
                 */
                if (!customerChanged) {

                    refreshExistingRows();

                }


                populateInvoiceSelector();


                if (availableInvoices.length === 0) {

                    showSourceMessage(
                        "No outstanding Finalized Invoices are available for this Customer.",
                        "info");

                }

            }
            catch (error) {

                console.error(
                    error);


                availableInvoices =
                    [];


                populateInvoiceSelector();


                showSourceMessage(
                    "Unable to load outstanding Invoices. Please try again.",
                    "danger");

            }
            finally {

                isLoadingInvoices =
                    false;


                setInvoiceLoadingState(
                    false);

            }

        }


        // =====================================================
        // REGION 6 — POPULATE INVOICE SELECTOR
        // =====================================================

        function populateInvoiceSelector() {

            if (!availableInvoiceSelector) {
                return;
            }


            const selectedInvoiceIds =
                getSelectedInvoiceIds();


            availableInvoiceSelector.innerHTML =
                "";


            const placeholder =
                document.createElement(
                    "option");


            placeholder.value =
                "";


            placeholder.textContent =
                "-- Select Outstanding Invoice --";


            availableInvoiceSelector.appendChild(
                placeholder);


            const selectableInvoices =
                availableInvoices
                    .filter(
                        invoice =>
                            !selectedInvoiceIds.has(
                                Number(
                                    invoice.invoiceId)));


            selectableInvoices.forEach(
                invoice => {

                    const option =
                        document.createElement(
                            "option");


                    option.value =
                        String(
                            invoice.invoiceId);


                    option.textContent =
                        buildInvoiceOptionText(
                            invoice);


                    availableInvoiceSelector
                        .appendChild(
                            option);

                });


            const customerSelected =
                getPositiveInteger(
                    customerSelector?.value) > 0;


            availableInvoiceSelector.disabled =
                !customerSelected ||
                selectableInvoices.length === 0;


            if (addInvoiceButton) {

                addInvoiceButton.disabled =
                    true;

            }

        }


        function buildInvoiceOptionText(
            invoice) {

            const invoiceCode =
                invoice.invoiceCode ||
                "-";


            const invoiceDate =
                formatDateForDisplay(
                    invoice.invoiceDate);


            const outstanding =
                formatMoney(
                    invoice.outstandingAmount);


            return `${invoiceCode} | ${invoiceDate} | Outstanding ₹ ${outstanding}`;

        }


        // =====================================================
        // REGION 7 — INVOICE SELECTOR CHANGE
        // =====================================================

        availableInvoiceSelector
            ?.addEventListener(
                "change",
                function () {

                    if (!addInvoiceButton) {
                        return;
                    }


                    addInvoiceButton.disabled =
                        getPositiveInteger(
                            availableInvoiceSelector.value) <= 0;

                });


        // =====================================================
        // REGION 8 — ADD INVOICE
        // =====================================================

        addInvoiceButton?.addEventListener(
            "click",
            function () {

                const invoiceId =
                    getPositiveInteger(
                        availableInvoiceSelector?.value);


                if (invoiceId <= 0) {

                    showSourceMessage(
                        "Please select an Invoice.",
                        "warning");

                    return;

                }


                if (
                    getSelectedInvoiceIds()
                        .has(
                            invoiceId)
                ) {

                    showSourceMessage(
                        "This Invoice is already added.",
                        "warning");

                    populateInvoiceSelector();

                    return;

                }


                const invoice =
                    availableInvoices
                        .find(
                            x =>
                                Number(
                                    x.invoiceId) ===
                                invoiceId);


                if (!invoice) {

                    showSourceMessage(
                        "Selected Invoice information is not available.",
                        "danger");

                    return;

                }


                appendInvoiceRow(
                    invoice);


                clearSourceMessage();

                populateInvoiceSelector();

            });


        // =====================================================
        // REGION 9 — APPEND INVOICE ROW
        // =====================================================

        function appendInvoiceRow(
            invoice) {

            if (
                !allocationBody ||
                !allocationTemplate
            ) {
                return;
            }


            const index =
                getAllocationRows()
                    .length;


            const sequence =
                index + 1;


            const invoiceId =
                getPositiveInteger(
                    invoice.invoiceId);


            const invoiceCode =
                invoice.invoiceCode ||
                "";


            const invoiceDate =
                invoice.invoiceDate ||
                "";


            const invoiceGrandTotal =
                roundMoney(
                    invoice.invoiceGrandTotal);


            const alreadyReceivedAmount =
                roundMoney(
                    invoice.alreadyReceivedAmount);


            const outstandingAmount =
                roundMoney(
                    invoice.outstandingAmount);


            let html =
                allocationTemplate.innerHTML;


            const replacements =
            {
                "__INDEX__":
                    String(
                        index),

                "__SEQUENCE__":
                    String(
                        sequence),

                "__INVOICE_ID__":
                    String(
                        invoiceId),

                "__INVOICE_CODE__":
                    escapeHtmlAttribute(
                        invoiceCode),

                "__INVOICE_DATE__":
                    escapeHtmlAttribute(
                        invoiceDate),

                "__INVOICE_DATE_DISPLAY__":
                    escapeHtml(
                        formatDateForDisplay(
                            invoiceDate)),

                "__INVOICE_TOTAL__":
                    invoiceGrandTotal
                        .toFixed(
                            2),

                "__INVOICE_TOTAL_DISPLAY__":
                    formatMoney(
                        invoiceGrandTotal),

                "__ALREADY_RECEIVED__":
                    alreadyReceivedAmount
                        .toFixed(
                            2),

                "__ALREADY_RECEIVED_DISPLAY__":
                    formatMoney(
                        alreadyReceivedAmount),

                "__OUTSTANDING__":
                    outstandingAmount
                        .toFixed(
                            2),

                "__OUTSTANDING_DISPLAY__":
                    formatMoney(
                        outstandingAmount)
            };


            Object.entries(
                replacements)
                .forEach(
                    ([key, value]) => {

                        html =
                            html
                                .split(
                                    key)
                                .join(
                                    value);

                    });


            allocationBody
                .insertAdjacentHTML(
                    "beforeend",
                    html);


            const rows =
                getAllocationRows();


            const newRow =
                rows[
                rows.length - 1];


            if (!newRow) {
                return;
            }


            attachRowEvents(
                newRow);


            recalculateRow(
                newRow);


            reindexRows();

            recalculateTotalReceived();

            updateNoAllocationMessage();

        }


        // =====================================================
        // REGION 10 — EXISTING ROW EVENTS
        // =====================================================

        function attachExistingRowEvents() {

            getAllocationRows()
                .forEach(
                    row =>
                        attachRowEvents(
                            row));

        }


        function attachRowEvents(
            row) {

            if (!row) {
                return;
            }


            const allocatedInput =
                row.querySelector(
                    ".allocated-amount");


            const removeButton =
                row.querySelector(
                    ".remove-allocation-button");


            allocatedInput
                ?.addEventListener(
                    "input",
                    function () {

                        recalculateRow(
                            row);

                        recalculateTotalReceived();

                    });


            allocatedInput
                ?.addEventListener(
                    "blur",
                    function () {

                        normalizeAllocatedInput(
                            allocatedInput);

                        recalculateRow(
                            row);

                        recalculateTotalReceived();

                    });


            removeButton
                ?.addEventListener(
                    "click",
                    function () {

                        row.remove();


                        reindexRows();

                        recalculateTotalReceived();

                        updateNoAllocationMessage();

                        populateInvoiceSelector();

                    });

        }


        // =====================================================
        // REGION 11 — REFRESH EXISTING ROWS
        // =====================================================

        function refreshExistingRows() {

            getAllocationRows()
                .forEach(
                    row => {

                        const invoiceId =
                            getPositiveInteger(
                                row.querySelector(
                                    ".invoice-id")
                                    ?.value);


                        if (invoiceId <= 0) {
                            return;
                        }


                        const invoice =
                            availableInvoices
                                .find(
                                    x =>
                                        Number(
                                            x.invoiceId) ===
                                        invoiceId);


                        if (!invoice) {

                            /*
                             * Preserve row.
                             * Server-side service will perform
                             * authoritative validation.
                             */
                            return;

                        }


                        const grandTotal =
                            roundMoney(
                                invoice.invoiceGrandTotal);


                        const alreadyReceived =
                            roundMoney(
                                invoice.alreadyReceivedAmount);


                        const outstanding =
                            roundMoney(
                                invoice.outstandingAmount);


                        setInputValue(
                            row,
                            ".invoice-grand-total",
                            grandTotal);


                        setInputValue(
                            row,
                            ".already-received-amount",
                            alreadyReceived);


                        setInputValue(
                            row,
                            ".outstanding-amount",
                            outstanding);


                        const outstandingDisplay =
                            row.querySelector(
                                ".outstanding-display");


                        if (outstandingDisplay) {

                            outstandingDisplay.textContent =
                                `₹ ${formatMoney(outstanding)}`;

                        }


                        recalculateRow(
                            row);

                    });


            recalculateTotalReceived();

        }


        // =====================================================
        // REGION 12 — ROW CALCULATION
        // =====================================================

        function recalculateAllRows() {

            getAllocationRows()
                .forEach(
                    row =>
                        recalculateRow(
                            row));


            recalculateTotalReceived();

        }


        function recalculateRow(
            row) {

            const outstanding =
                getNumberFromRow(
                    row,
                    ".outstanding-amount");


            const allocated =
                getNumberFromRow(
                    row,
                    ".allocated-amount");


            const balance =
                roundMoney(
                    outstanding -
                    allocated);


            const balanceInput =
                row.querySelector(
                    ".balance-after-receipt");


            if (balanceInput) {

                balanceInput.value =
                    balance
                        .toFixed(
                            2);

            }


            const balanceDisplay =
                row.querySelector(
                    ".balance-display");


            if (balanceDisplay) {

                balanceDisplay.textContent =
                    `₹ ${formatMoney(balance)}`;

            }


            validateAllocationRow(
                row);

        }


        // =====================================================
        // REGION 13 — ALLOCATION VALIDATION
        // =====================================================

        function validateAllocationRow(
            row) {

            const outstanding =
                getNumberFromRow(
                    row,
                    ".outstanding-amount");


            const allocated =
                getNumberFromRow(
                    row,
                    ".allocated-amount");


            const errorElement =
                row.querySelector(
                    ".allocation-error");


            let message =
                "";


            if (allocated <= 0) {

                message =
                    "Allocated Amount must be greater than zero.";

            }
            else if (allocated > outstanding) {

                message =
                    `Allocated Amount cannot exceed Outstanding ₹ ${formatMoney(outstanding)}.`;

            }


            if (errorElement) {

                errorElement.textContent =
                    message;


                errorElement.classList.toggle(
                    "d-none",
                    !message);

            }


            return !message;

        }


        function validateAllAllocations() {

            const rows =
                getAllocationRows();


            if (rows.length === 0) {

                showSourceMessage(
                    "At least one Invoice allocation is required.",
                    "danger");

                return false;

            }


            let valid =
                true;


            rows.forEach(
                row => {

                    if (!validateAllocationRow(
                        row)) {
                        valid =
                            false;
                    }

                });


            if (!valid) {

                const firstInvalid =
                    rows.find(
                        row =>
                            !validateAllocationRow(
                                row));


                firstInvalid
                    ?.querySelector(
                        ".allocated-amount")
                    ?.focus();

            }


            return valid;

        }


        // =====================================================
        // REGION 14 — TOTAL RECEIVED
        // =====================================================

        function recalculateTotalReceived() {

            if (!totalReceivedAmount) {
                return;
            }


            const total =
                getAllocationRows()
                    .reduce(
                        (sum, row) => {

                            return sum +
                                getNumberFromRow(
                                    row,
                                    ".allocated-amount");

                        },
                        0);


            totalReceivedAmount.value =
                roundMoney(
                    total)
                    .toFixed(
                        2);

        }


        // =====================================================
        // REGION 15 — PAYMENT MODE
        // =====================================================

        paymentModeSelector
            ?.addEventListener(
                "change",
                updatePaymentModeFields);


        function updatePaymentModeFields() {

            const paymentMode =
                getPositiveInteger(
                    paymentModeSelector?.value);


            /*
             * PaymentMode:
             *
             * 1 = Cash
             * 2 = Cheque
             * 3 = NEFT
             * 4 = RTGS
             * 5 = IMPS
             * 6 = UPI
             * 7 = BankTransfer
             * 8 = Other
             */

            const isCash =
                paymentMode === 1;


            const isCheque =
                paymentMode === 2;


            const isElectronic =
                [
                    3,
                    4,
                    5,
                    6,
                    7
                ]
                    .includes(
                        paymentMode);


            const isOther =
                paymentMode === 8;


            setSectionVisible(
                referenceNumberSection,
                isElectronic ||
                isOther);


            setSectionVisible(
                bankNameSection,
                isCheque ||
                isElectronic ||
                isOther);


            setSectionVisible(
                chequeNumberSection,
                isCheque);


            setSectionVisible(
                chequeDateSection,
                isCheque);


            /*
             * Clear fields that cannot apply
             * to the selected mode.
             */

            if (isCash) {

                clearInput(
                    referenceNumber);

                clearInput(
                    bankName);

                clearInput(
                    chequeNumber);

                clearInput(
                    chequeDate);

            }
            else if (isCheque) {

                clearInput(
                    referenceNumber);

            }
            else {

                clearInput(
                    chequeNumber);

                clearInput(
                    chequeDate);

            }

        }


        // =====================================================
        // REGION 16 — FORM SUBMIT VALIDATION
        // =====================================================

        form?.addEventListener(
            "submit",
            function (event) {

                clearSourceMessage();


                if (!validatePaymentModeFields()) {

                    event.preventDefault();

                    return;

                }


                if (!validateAllAllocations()) {

                    event.preventDefault();

                    return;

                }


                recalculateTotalReceived();

                reindexRows();

            });


        function validatePaymentModeFields() {

            const paymentMode =
                getPositiveInteger(
                    paymentModeSelector?.value);


            if (paymentMode <= 0) {

                showSourceMessage(
                    "Please select a Payment Mode.",
                    "danger");

                paymentModeSelector
                    ?.focus();

                return false;

            }


            if (paymentMode === 2) {

                if (
                    !chequeNumber ||
                    !chequeNumber.value.trim()
                ) {

                    showSourceMessage(
                        "Cheque Number is required for Cheque payment.",
                        "danger");

                    chequeNumber
                        ?.focus();

                    return false;

                }


                if (
                    !chequeDate ||
                    !chequeDate.value
                ) {

                    showSourceMessage(
                        "Cheque Date is required for Cheque payment.",
                        "danger");

                    chequeDate
                        ?.focus();

                    return false;

                }


                if (
                    !bankName ||
                    !bankName.value.trim()
                ) {

                    showSourceMessage(
                        "Bank Name is required for Cheque payment.",
                        "danger");

                    bankName
                        ?.focus();

                    return false;

                }

            }


            if (
                [
                    3,
                    4,
                    5,
                    6,
                    7
                ]
                    .includes(
                        paymentMode)
            ) {

                if (
                    !referenceNumber ||
                    !referenceNumber.value.trim()
                ) {

                    showSourceMessage(
                        "Transaction / Reference Number is required for the selected Payment Mode.",
                        "danger");

                    referenceNumber
                        ?.focus();

                    return false;

                }

            }


            return true;

        }


        // =====================================================
        // REGION 17 — REINDEX COLLECTION
        // =====================================================

        function reindexRows() {

            const rows =
                getAllocationRows();


            rows.forEach(
                (row, index) => {

                    const sequence =
                        index + 1;


                    const sequenceDisplay =
                        row.querySelector(
                            ".allocation-sequence");


                    if (sequenceDisplay) {

                        sequenceDisplay.textContent =
                            String(
                                sequence);

                    }


                    const sequenceInput =
                        row.querySelector(
                            ".allocation-sequence-input");


                    if (sequenceInput) {

                        sequenceInput.value =
                            String(
                                sequence);

                    }


                    row.querySelectorAll(
                        "input, select, textarea, span[data-valmsg-for]")
                        .forEach(
                            element => {

                                reindexElement(
                                    element,
                                    index);

                            });

                });

        }


        function reindexElement(
            element,
            index) {

            if (!element) {
                return;
            }


            if (element.name) {

                element.name =
                    element.name.replace(
                        /Allocations\[\d+\]/g,
                        `Allocations[${index}]`);

            }


            if (element.id) {

                element.id =
                    element.id.replace(
                        /Allocations_\d+__/g,
                        `Allocations_${index}__`);

            }


            const validationFor =
                element.getAttribute(
                    "data-valmsg-for");


            if (validationFor) {

                element.setAttribute(
                    "data-valmsg-for",
                    validationFor.replace(
                        /Allocations\[\d+\]/g,
                        `Allocations[${index}]`));

            }

        }


        // =====================================================
        // REGION 18 — CLEAR ALLOCATIONS
        // =====================================================

        function clearAllAllocations() {

            if (!allocationBody) {
                return;
            }


            allocationBody.innerHTML =
                "";


            recalculateTotalReceived();

            updateNoAllocationMessage();

        }


        // =====================================================
        // REGION 19 — NO ITEMS MESSAGE
        // =====================================================

        function updateNoAllocationMessage() {

            if (!noAllocationMessage) {
                return;
            }


            noAllocationMessage
                .classList
                .toggle(
                    "d-none",
                    getAllocationRows()
                        .length > 0);

        }


        // =====================================================
        // REGION 20 — SOURCE MESSAGE
        // =====================================================

        function showSourceMessage(
            message,
            type) {

            if (!invoiceSourceMessage) {
                return;
            }


            invoiceSourceMessage.className =
                `alert alert-${type || "info"}`;


            invoiceSourceMessage.textContent =
                message;


            invoiceSourceMessage
                .classList
                .remove(
                    "d-none");

        }


        function clearSourceMessage() {

            if (!invoiceSourceMessage) {
                return;
            }


            invoiceSourceMessage.textContent =
                "";


            invoiceSourceMessage
                .classList
                .add(
                    "d-none");

        }


        // =====================================================
        // REGION 21 — LOADING STATE
        // =====================================================

        function setInvoiceLoadingState(
            isLoading) {

            if (!availableInvoiceSelector) {
                return;
            }


            if (isLoading) {

                availableInvoiceSelector.disabled =
                    true;


                availableInvoiceSelector.innerHTML =
                    '<option value="">Loading outstanding Invoices...</option>';


                if (addInvoiceButton) {

                    addInvoiceButton.disabled =
                        true;

                }


                return;
            }


            /*
             * Loading completed.
             *
             * Rebuild selector from loaded invoices
             * and enable it when selectable invoices exist.
             */
            populateInvoiceSelector();

        }


        function resetInvoiceSelector() {

            if (!availableInvoiceSelector) {
                return;
            }


            availableInvoiceSelector.innerHTML =
                '<option value="">-- Select Customer First --</option>';


            availableInvoiceSelector.disabled =
                true;


            if (addInvoiceButton) {

                addInvoiceButton.disabled =
                    true;

            }

        }


        // =====================================================
        // REGION 22 — SELECTED INVOICE IDS
        // =====================================================

        function getSelectedInvoiceIds() {

            return new Set(
                getAllocationRows()
                    .map(
                        row =>
                            getPositiveInteger(
                                row.querySelector(
                                    ".invoice-id")
                                    ?.value))
                    .filter(
                        id =>
                            id > 0));

        }


        // =====================================================
        // REGION 23 — CURRENT RECEIPT ID
        // =====================================================

        function getCurrentReceiptId() {

            const idInput =
                form
                    ?.querySelector(
                        'input[name="Id"]');


            return getPositiveInteger(
                idInput?.value);

        }


        // =====================================================
        // REGION 24 — ROW HELPERS
        // =====================================================

        function getAllocationRows() {

            if (!allocationBody) {
                return [];
            }


            return Array.from(
                allocationBody
                    .querySelectorAll(
                        ".allocation-row"));

        }


        function getNumberFromRow(
            row,
            selector) {

            const element =
                row.querySelector(
                    selector);


            return getNumber(
                element?.value);

        }


        function setInputValue(
            row,
            selector,
            value) {

            const input =
                row.querySelector(
                    selector);


            if (input) {

                input.value =
                    roundMoney(
                        value)
                        .toFixed(
                            2);

            }

        }


        // =====================================================
        // REGION 25 — INPUT HELPERS
        // =====================================================

        function normalizeAllocatedInput(
            input) {

            if (!input) {
                return;
            }


            const value =
                getNumber(
                    input.value);


            if (value > 0) {

                input.value =
                    roundMoney(
                        value)
                        .toFixed(
                            2);

            }

        }


        function clearInput(
            input) {

            if (input) {

                input.value =
                    "";

            }

        }


        function setSectionVisible(
            section,
            visible) {

            section
                ?.classList
                .toggle(
                    "d-none",
                    !visible);

        }


        // =====================================================
        // REGION 26 — NUMBER HELPERS
        // =====================================================

        function getNumber(
            value) {

            const parsed =
                Number.parseFloat(
                    value);


            return Number.isFinite(
                parsed)
                ? parsed
                : 0;

        }


        function getPositiveInteger(
            value) {

            const parsed =
                Number.parseInt(
                    value,
                    10);


            return Number.isFinite(
                parsed) &&
                parsed > 0

                ? parsed

                : 0;

        }


        function roundMoney(
            value) {

            const number =
                getNumber(
                    value);


            return Math.round(
                (
                    number +
                    Number.EPSILON
                ) *
                100) /
                100;

        }


        function formatMoney(
            value) {

            return roundMoney(
                value)
                .toLocaleString(
                    "en-IN",
                    {
                        minimumFractionDigits:
                            2,

                        maximumFractionDigits:
                            2
                    });

        }


        // =====================================================
        // REGION 27 — DATE HELPERS
        // =====================================================

        function formatDateForDisplay(
            value) {

            if (!value) {
                return "-";
            }


            const parts =
                String(
                    value)
                    .split(
                        "T")[0]
                    .split(
                        "-");


            if (parts.length !== 3) {
                return value;
            }


            return `${parts[2]}-${parts[1]}-${parts[0]}`;

        }


        // =====================================================
        // REGION 28 — HTML HELPERS
        // =====================================================

        function escapeHtml(
            value) {

            const div =
                document.createElement(
                    "div");


            div.textContent =
                value == null
                    ? ""
                    : String(
                        value);


            return div.innerHTML;

        }


        function escapeHtmlAttribute(
            value) {

            return escapeHtml(
                value)
                .replace(
                    /"/g,
                    "&quot;")
                .replace(
                    /'/g,
                    "&#39;");

        }

    });