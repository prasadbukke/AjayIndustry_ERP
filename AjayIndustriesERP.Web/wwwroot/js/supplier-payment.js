// =============================================================
// File: supplier-payment.js
// Module: Supplier Payment
// Layer: Web - Client Script
//
// Purpose:
// Handles client-side behaviour for the redesigned
// Supplier Payment module.
//
// Supported Screens:
//
// 1. Create Supplier Payment
//    - Purchase Invoice selection
//    - Invoice details auto-load
//    - Invoice Total as maximum payment
//    - Full Amount button
//
// 2. Add Payment Transaction
//    - Current Outstanding as maximum payment
//    - Full Amount button
//
// Important Business Rules:
// - Company is NOT manually selected.
// - Supplier is NOT manually selected.
// - Supplier and Company are derived server-side from
//   the selected Purchase Invoice.
// - Payment amount must be greater than zero.
// - Payment amount cannot exceed current Outstanding.
// - Full Amount never creates a new Payment No.
// =============================================================


document.addEventListener(
    "DOMContentLoaded",
    function () {

        // =====================================================
        // CREATE PAYMENT SCREEN
        // =====================================================

        // #region Create Payment

        initializeCreatePayment();

        // #endregion


        // =====================================================
        // ADD TRANSACTION SCREEN
        // =====================================================

        // #region Add Transaction

        initializeAddTransaction();

        // #endregion


        // =====================================================
        // COMMON PAYMENT MODE BEHAVIOUR
        // =====================================================

        // #region Payment Mode

        initializePaymentMode();

        // #endregion
    });


// =============================================================
// CREATE PAYMENT
// =============================================================

// #region Create Payment

function initializeCreatePayment() {

    const form =
        document.getElementById(
            "supplierPaymentCreateForm");


    if (!form) {
        return;
    }


    const invoiceSelect =
        document.getElementById(
            "supplierPaymentPurchaseInvoice");


    const amountInput =
        document.getElementById(
            "supplierPaymentAmount");


    const fullAmountButton =
        document.getElementById(
            "supplierPaymentFullAmountButton");


    if (!invoiceSelect) {
        return;
    }


    // =========================================================
    // INITIAL PAGE STATE
    //
    // Important when server-side validation failed and
    // the page is rendered again with selected invoice.
    // =========================================================

    initializeCreateMaximumAmount(
        amountInput,
        fullAmountButton);


    // =========================================================
    // PURCHASE INVOICE CHANGE
    // =========================================================

    invoiceSelect.addEventListener(
        "change",
        async function () {

            const purchaseInvoiceId =
                parseInt(
                    invoiceSelect.value,
                    10);


            resetCreateInvoicePreview();


            clearAmountValidation(
                amountInput);


            if (amountInput) {
                amountInput.value = "";
                amountInput.removeAttribute(
                    "max");
            }


            if (fullAmountButton) {

                fullAmountButton.disabled =
                    true;


                fullAmountButton.removeAttribute(
                    "data-full-amount");
            }


            if (!purchaseInvoiceId ||
                purchaseInvoiceId <= 0) {

                showSelectInvoiceMessage();

                return;
            }


            const detailsUrl =
                invoiceSelect.dataset
                    .detailsUrl;


            if (!detailsUrl) {

                showCreateInvoiceError(
                    "Unable to load Purchase Invoice details.");

                return;
            }


            setInvoiceSelectLoading(
                invoiceSelect,
                true);


            try {

                const url =
                    buildPurchaseInvoiceDetailsUrl(
                        detailsUrl,
                        purchaseInvoiceId);


                const response =
                    await fetch(
                        url,
                        {
                            method: "GET",
                            headers: {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        });


                if (!response.ok) {

                    throw new Error(
                        "Unable to load Purchase Invoice details.");
                }


                const result =
                    await response.json();


                const success =
                    getValue(
                        result,
                        "success",
                        "Success");


                if (!success) {

                    const message =
                        getValue(
                            result,
                            "message",
                            "Message")
                        ||
                        "Purchase Invoice is not available for payment.";


                    showCreateInvoiceError(
                        message);


                    return;
                }


                const invoice =
                    getValue(
                        result,
                        "invoice",
                        "Invoice");


                if (!invoice) {

                    showCreateInvoiceError(
                        "Purchase Invoice information is not available.");

                    return;
                }


                populateCreateInvoicePreview(
                    invoice);


                const invoiceTotal =
                    toNumber(
                        getValue(
                            invoice,
                            "invoiceTotal",
                            "InvoiceTotal"));


                configureCreateAmount(
                    amountInput,
                    fullAmountButton,
                    invoiceTotal);

            }
            catch (error) {

                console.error(
                    "Supplier Payment invoice load failed:",
                    error);


                showCreateInvoiceError(
                    "Unable to load Purchase Invoice details.");

            }
            finally {

                setInvoiceSelectLoading(
                    invoiceSelect,
                    false);
            }
        });


    // =========================================================
    // FULL AMOUNT BUTTON
    // =========================================================

    if (fullAmountButton &&
        amountInput) {

        fullAmountButton.addEventListener(
            "click",
            function () {

                const maximum =
                    getElementMaximum(
                        amountInput,
                        fullAmountButton);


                if (maximum <= 0) {
                    return;
                }


                amountInput.value =
                    formatInputAmount(
                        maximum);


                clearAmountValidation(
                    amountInput);


                amountInput.focus();
            });


        attachAmountValidation(
            amountInput);
    }

}


// =============================================================
// INITIAL CREATE MAXIMUM
// =============================================================

function initializeCreateMaximumAmount(
    amountInput,
    fullAmountButton) {

    if (!amountInput ||
        !fullAmountButton) {

        return;
    }


    const maximum =
        toNumber(
            amountInput.getAttribute(
                "max"));


    if (maximum > 0) {

        fullAmountButton.disabled =
            false;


        fullAmountButton.dataset.fullAmount =
            maximum.toString();

    }
    else {

        fullAmountButton.disabled =
            true;
    }


    attachAmountValidation(
        amountInput);
}


// =============================================================
// CREATE INVOICE PREVIEW
// =============================================================

function populateCreateInvoicePreview(
    invoice) {

    const purchaseInvoiceCode =
        getValue(
            invoice,
            "purchaseInvoiceCode",
            "PurchaseInvoiceCode")
        || "-";


    const supplierInvoiceNumber =
        getValue(
            invoice,
            "supplierInvoiceNumber",
            "SupplierInvoiceNumber")
        || "-";


    const supplierName =
        getValue(
            invoice,
            "supplierName",
            "SupplierName")
        || "-";


    const purchaseInvoiceDate =
        getValue(
            invoice,
            "purchaseInvoiceDate",
            "PurchaseInvoiceDate")
        || "-";


    const dueDate =
        getValue(
            invoice,
            "dueDate",
            "DueDate")
        || "-";


    const invoiceTotal =
        toNumber(
            getValue(
                invoice,
                "invoiceTotal",
                "InvoiceTotal"));


    setText(
        "supplierPaymentPurchaseInvoiceCode",
        purchaseInvoiceCode);


    setText(
        "supplierPaymentSupplierInvoiceNumber",
        supplierInvoiceNumber);


    setText(
        "supplierPaymentSupplierName",
        supplierName);


    setText(
        "supplierPaymentPurchaseInvoiceDate",
        purchaseInvoiceDate);


    setText(
        "supplierPaymentDueDate",
        dueDate);


    setText(
        "supplierPaymentInvoiceTotalDisplay",
        formatCurrency(
            invoiceTotal));


    setText(
        "supplierPaymentMaximumAmount",
        formatCurrency(
            invoiceTotal));


    const message =
        document.getElementById(
            "supplierPaymentSelectInvoiceMessage");


    const preview =
        document.getElementById(
            "supplierPaymentInvoicePreview");


    if (message) {
        message.classList.add(
            "d-none");
    }


    if (preview) {
        preview.classList.remove(
            "d-none");
    }
}


// =============================================================
// CONFIGURE CREATE AMOUNT
// =============================================================

function configureCreateAmount(
    amountInput,
    fullAmountButton,
    invoiceTotal) {

    if (!amountInput) {
        return;
    }


    if (invoiceTotal <= 0) {

        amountInput.removeAttribute(
            "max");


        if (fullAmountButton) {

            fullAmountButton.disabled =
                true;
        }


        return;
    }


    const amountValue =
        formatInputAmount(
            invoiceTotal);


    amountInput.setAttribute(
        "max",
        amountValue);


    if (fullAmountButton) {

        fullAmountButton.dataset.fullAmount =
            amountValue;


        fullAmountButton.disabled =
            false;
    }
}


// =============================================================
// RESET CREATE PREVIEW
// =============================================================

function resetCreateInvoicePreview() {

    setText(
        "supplierPaymentPurchaseInvoiceCode",
        "-");


    setText(
        "supplierPaymentSupplierInvoiceNumber",
        "-");


    setText(
        "supplierPaymentSupplierName",
        "-");


    setText(
        "supplierPaymentPurchaseInvoiceDate",
        "-");


    setText(
        "supplierPaymentDueDate",
        "-");


    setText(
        "supplierPaymentInvoiceTotalDisplay",
        formatCurrency(
            0));


    setText(
        "supplierPaymentMaximumAmount",
        formatCurrency(
            0));


    const preview =
        document.getElementById(
            "supplierPaymentInvoicePreview");


    if (preview) {

        preview.classList.add(
            "d-none");
    }
}


// =============================================================
// SELECT INVOICE MESSAGE
// =============================================================

function showSelectInvoiceMessage() {

    const message =
        document.getElementById(
            "supplierPaymentSelectInvoiceMessage");


    if (!message) {
        return;
    }


    message.className =
        "alert alert-light border mt-3 mb-0";


    message.innerHTML =
        '<i class="fa-solid fa-circle-info text-primary me-2"></i>' +
        "Select a Purchase Invoice to view supplier and invoice details.";
}


// =============================================================
// CREATE INVOICE ERROR
// =============================================================

function showCreateInvoiceError(
    messageText) {

    resetCreateInvoicePreview();


    const message =
        document.getElementById(
            "supplierPaymentSelectInvoiceMessage");


    if (!message) {
        return;
    }


    message.className =
        "alert alert-danger mt-3 mb-0";


    message.textContent =
        messageText;
}


// =============================================================
// CREATE SELECT LOADING
// =============================================================

function setInvoiceSelectLoading(
    invoiceSelect,
    isLoading) {

    if (!invoiceSelect) {
        return;
    }


    invoiceSelect.disabled =
        isLoading;
}


// =============================================================
// DETAILS URL
// =============================================================

function buildPurchaseInvoiceDetailsUrl(
    baseUrl,
    purchaseInvoiceId) {

    const separator =
        baseUrl.includes("?")
            ? "&"
            : "?";


    return (
        baseUrl +
        separator +
        "purchaseInvoiceId=" +
        encodeURIComponent(
            purchaseInvoiceId)
    );
}

// #endregion


// =============================================================
// ADD TRANSACTION
// =============================================================

// #region Add Transaction

function initializeAddTransaction() {

    const form =
        document.getElementById(
            "supplierPaymentAddTransactionForm");


    if (!form) {
        return;
    }


    const amountInput =
        document.getElementById(
            "supplierPaymentAmount");


    const fullAmountButton =
        document.getElementById(
            "supplierPaymentFullAmountButton");


    if (!amountInput) {
        return;
    }


    const outstanding =
        toNumber(
            form.dataset.outstanding);


    if (outstanding > 0) {

        const maximum =
            formatInputAmount(
                outstanding);


        amountInput.setAttribute(
            "max",
            maximum);


        if (fullAmountButton) {

            fullAmountButton.dataset.fullAmount =
                maximum;


            fullAmountButton.disabled =
                false;
        }
    }
    else {

        if (fullAmountButton) {

            fullAmountButton.disabled =
                true;
        }
    }


    // =========================================================
    // FULL OUTSTANDING BUTTON
    // =========================================================

    if (fullAmountButton) {

        fullAmountButton.addEventListener(
            "click",
            function () {

                const maximum =
                    getElementMaximum(
                        amountInput,
                        fullAmountButton);


                if (maximum <= 0) {
                    return;
                }


                amountInput.value =
                    formatInputAmount(
                        maximum);


                clearAmountValidation(
                    amountInput);


                amountInput.focus();
            });
    }


    attachAmountValidation(
        amountInput);
}

// #endregion


// =============================================================
// AMOUNT VALIDATION
// =============================================================

// #region Amount Validation

function attachAmountValidation(
    amountInput) {

    if (!amountInput) {
        return;
    }


    /*
     * Prevent duplicate listeners when an input is initialized
     * through both page-specific and common initialization.
     */
    if (amountInput.dataset
        .supplierPaymentValidationAttached ===
        "true") {

        return;
    }


    amountInput.dataset
        .supplierPaymentValidationAttached =
        "true";


    amountInput.addEventListener(
        "input",
        function () {

            validatePaymentAmount(
                amountInput);
        });


    amountInput.addEventListener(
        "change",
        function () {

            validatePaymentAmount(
                amountInput);
        });


    amountInput.addEventListener(
        "blur",
        function () {

            validatePaymentAmount(
                amountInput);
        });
}


function validatePaymentAmount(
    amountInput) {

    if (!amountInput) {
        return true;
    }


    clearAmountValidation(
        amountInput);


    const amount =
        toNumber(
            amountInput.value);


    const maximum =
        toNumber(
            amountInput.getAttribute(
                "max"));


    /*
     * Empty input should be handled by normal
     * required/model validation.
     */
    if (!amountInput.value) {
        return true;
    }


    if (amount <= 0) {

        amountInput.setCustomValidity(
            "Payment Amount must be greater than zero.");


        return false;
    }


    if (maximum > 0 &&
        amount > maximum) {

        amountInput.setCustomValidity(
            "Payment Amount cannot exceed current Outstanding of " +
            formatCurrency(
                maximum) +
            ".");


        return false;
    }


    return true;
}


function clearAmountValidation(
    amountInput) {

    if (!amountInput) {
        return;
    }


    amountInput.setCustomValidity(
        "");
}


function getElementMaximum(
    amountInput,
    fullAmountButton) {

    if (fullAmountButton) {

        const buttonMaximum =
            toNumber(
                fullAmountButton.dataset
                    .fullAmount);


        if (buttonMaximum > 0) {
            return buttonMaximum;
        }
    }


    if (amountInput) {

        const inputMaximum =
            toNumber(
                amountInput.getAttribute(
                    "max"));


        if (inputMaximum > 0) {
            return inputMaximum;
        }
    }


    return 0;
}

// #endregion


// =============================================================
// PAYMENT MODE
// =============================================================

// #region Payment Mode

function initializePaymentMode() {

    const paymentMode =
        document.getElementById(
            "supplierPaymentPaymentMode");


    const bankName =
        document.getElementById(
            "supplierPaymentBankName");


    if (!paymentMode ||
        !bankName) {

        return;
    }


    /*
     * Bank remains optional because existing business rules
     * do not require bank information for every payment mode.
     *
     * For Cash, bank field is visually disabled because it
     * is not applicable.
     */

    function applyPaymentModeState() {

        const mode =
            (
                paymentMode.value
                || ""
            )
                .trim()
                .toLowerCase();


        if (mode === "cash") {

            bankName.value =
                "";


            bankName.disabled =
                true;


            bankName.placeholder =
                "Not applicable for Cash";
        }
        else {

            bankName.disabled =
                false;


            bankName.placeholder =
                "Bank name";
        }
    }


    paymentMode.addEventListener(
        "change",
        applyPaymentModeState);


    applyPaymentModeState();
}

// #endregion


// =============================================================
// COMMON HELPERS
// =============================================================

// #region Common Helpers

function getValue(
    object,
    camelCaseName,
    pascalCaseName) {

    if (!object) {
        return null;
    }


    if (Object.prototype
        .hasOwnProperty
        .call(
            object,
            camelCaseName)) {

        return object[
            camelCaseName];
    }


    if (Object.prototype
        .hasOwnProperty
        .call(
            object,
            pascalCaseName)) {

        return object[
            pascalCaseName];
    }


    return null;
}


function setText(
    elementId,
    value) {

    const element =
        document.getElementById(
            elementId);


    if (!element) {
        return;
    }


    element.textContent =
        value ?? "-";
}


function toNumber(
    value) {

    if (value === null ||
        value === undefined ||
        value === "") {

        return 0;
    }


    const parsed =
        Number(
            value);


    return Number.isFinite(
        parsed)
        ? parsed
        : 0;
}


function formatInputAmount(
    value) {

    return toNumber(
        value)
        .toFixed(
            2);
}


function formatCurrency(
    value) {

    const amount =
        toNumber(
            value);


    return (
        "₹ " +
        new Intl.NumberFormat(
            "en-IN",
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            })
            .format(
                amount)
    );
}

// #endregion