/*
==============================================================

File : supplier-form.js

Purpose :
Handles Supplier Master client-side behavior.

Features :
- Live Supplier Name similarity checking
- Exact duplicate blocks Save
- Similar Supplier requires confirmation
- GSTIN automatic uppercase
- PAN automatic uppercase

==============================================================
*/

(function () {
    "use strict";

    document.addEventListener(
        "DOMContentLoaded",
        initializeSupplierForm
    );

    function initializeSupplierForm() {

        initializeSupplierNameCheck();

        initializeUppercaseField(
            "Gstin"
        );

        initializeUppercaseField(
            "Pan"
        );
    }

    // =========================================================
    // SUPPLIER NAME SIMILARITY
    // =========================================================

    function initializeSupplierNameCheck() {

        const nameInput =
            document.getElementById(
                "SupplierName"
            );

        const supplierId =
            document.getElementById(
                "SupplierId"
            );

        const warning =
            document.getElementById(
                "supplierSimilarWarning"
            );

        const list =
            document.getElementById(
                "supplierSimilarList"
            );

        const exactMessage =
            document.getElementById(
                "supplierExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "supplierSimilarConfirmationContainer"
            );

        const confirmation =
            document.getElementById(
                "ConfirmSimilarSupplierName"
            );

        const saveButton =
            document.getElementById(
                "supplierSaveButton"
            );

        if (!nameInput ||
            !warning ||
            !list ||
            !exactMessage ||
            !confirmationContainer ||
            !confirmation ||
            !saveButton) {

            return;
        }

        const similarUrl =
            nameInput.getAttribute(
                "data-similar-url"
            );

        let timer = null;
        let activeRequest = null;

        nameInput.addEventListener(
            "input",
            function () {

                clearTimeout(timer);

                confirmation.checked =
                    false;

                clearSupplierNameValidation();

                const supplierName =
                    nameInput.value.trim();

                if (supplierName.length < 3) {

                    clearSuggestions();

                    return;
                }

                timer =
                    setTimeout(
                        function () {

                            loadSuggestions(
                                supplierName
                            );

                        },
                        450
                    );
            }
        );

        confirmation.addEventListener(
            "change",
            refreshSaveButton
        );

        refreshSaveButton();

        async function loadSuggestions(
            supplierName) {

            if (!similarUrl) {
                return;
            }

            if (activeRequest) {

                activeRequest.abort();
            }

            activeRequest =
                new AbortController();

            const requestUrl =
                similarUrl +
                "?supplierName=" +
                encodeURIComponent(
                    supplierName
                ) +
                "&supplierId=" +
                encodeURIComponent(
                    supplierId?.value || ""
                );

            try {

                const response =
                    await fetch(
                        requestUrl,
                        {
                            method:
                                "GET",

                            cache:
                                "no-store",

                            signal:
                                activeRequest.signal,

                            headers: {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        }
                    );

                if (!response.ok) {
                    return;
                }

                const result =
                    await response.json();

                renderSuggestions(
                    result.records || []
                );

            } catch (error) {

                if (error.name !==
                    "AbortError") {

                    clearSuggestions();
                }
            }
        }

        function renderSuggestions(
            records) {

            list.innerHTML = "";

            if (!records ||
                records.length === 0) {

                clearSuggestions();

                return;
            }

            let hasExactMatch =
                false;

            records.forEach(
                function (record) {

                    if (record.isExactMatch) {

                        hasExactMatch =
                            true;
                    }

                    const item =
                        document.createElement(
                            "div"
                        );

                    item.className =
                        "list-group-item";

                    const row =
                        document.createElement(
                            "div"
                        );

                    row.className =
                        "d-flex " +
                        "justify-content-between " +
                        "align-items-center gap-2";

                    const text =
                        document.createElement(
                            "span"
                        );

                    text.textContent =
                        record.displayText;

                    const badge =
                        document.createElement(
                            "span"
                        );

                    if (record.isExactMatch) {

                        badge.className =
                            "badge bg-danger";

                        badge.textContent =
                            "Exact duplicate";

                    } else {

                        badge.className =
                            "badge bg-warning text-dark";

                        badge.textContent =
                            "Similar name";
                    }

                    row.appendChild(text);
                    row.appendChild(badge);

                    item.appendChild(row);

                    list.appendChild(item);
                }
            );

            warning.classList.remove(
                "d-none"
            );

            exactMessage.classList.toggle(
                "d-none",
                !hasExactMatch
            );

            confirmationContainer.classList.toggle(
                "d-none",
                hasExactMatch
            );

            if (hasExactMatch) {

                confirmation.checked =
                    false;
            }

            refreshSaveButton();
        }

        function clearSuggestions() {

            list.innerHTML = "";

            warning.classList.add(
                "d-none"
            );

            exactMessage.classList.add(
                "d-none"
            );

            confirmationContainer.classList.add(
                "d-none"
            );

            confirmation.checked =
                false;

            refreshSaveButton();
        }

        function refreshSaveButton() {

            const exactDuplicateExists =
                !exactMessage.classList
                    .contains("d-none");

            const confirmationRequired =
                !confirmationContainer
                    .classList
                    .contains("d-none");

            saveButton.disabled =
                exactDuplicateExists ||
                (
                    confirmationRequired &&
                    !confirmation.checked
                );
        }

        function clearSupplierNameValidation() {

            const message =
                document.querySelector(
                    "[data-valmsg-for='SupplierName']"
                );

            if (!message) {
                return;
            }

            message.textContent = "";

            message.classList.remove(
                "field-validation-error"
            );

            message.classList.add(
                "field-validation-valid"
            );
        }
    }

    // =========================================================
    // GSTIN / PAN UPPERCASE
    // =========================================================

    function initializeUppercaseField(
        elementId) {

        const input =
            document.getElementById(
                elementId
            );

        if (!input) {
            return;
        }

        input.addEventListener(
            "input",
            function () {

                const start =
                    input.selectionStart;

                const end =
                    input.selectionEnd;

                input.value =
                    input.value
                        .toUpperCase();

                if (start !== null &&
                    end !== null) {

                    input.setSelectionRange(
                        start,
                        end
                    );
                }
            }
        );
    }

})();