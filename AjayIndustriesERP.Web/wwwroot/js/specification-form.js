/*
==============================================================

File : specification-form.js

Purpose :
Provides live exact and similar Specification Name detection.

Features :
- Starts checking after 3 characters
- 450ms debounce
- Cancels previous pending request
- Exact duplicate blocks Save
- Similar names require confirmation

==============================================================
*/

(function () {
    "use strict";

    document.addEventListener(
        "DOMContentLoaded",
        initializeSpecificationForm
    );

    function initializeSpecificationForm() {

        const nameInput =
            document.getElementById(
                "SpecificationName"
            );

        const idInput =
            document.getElementById(
                "SpecificationId"
            );

        const warningContainer =
            document.getElementById(
                "specificationSimilarWarning"
            );

        const similarList =
            document.getElementById(
                "specificationSimilarList"
            );

        const exactMessage =
            document.getElementById(
                "specificationExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "specificationConfirmationContainer"
            );

        const confirmationCheckbox =
            document.getElementById(
                "ConfirmSimilarSpecificationName"
            );

        const saveButton =
            document.getElementById(
                "specificationSaveButton"
            );

        if (!nameInput ||
            !warningContainer ||
            !similarList ||
            !exactMessage ||
            !confirmationContainer ||
            !confirmationCheckbox ||
            !saveButton) {

            return;
        }

        const similarUrl =
            nameInput.getAttribute(
                "data-similar-url"
            );

        let requestTimer = null;
        let activeRequest = null;

        nameInput.addEventListener(
            "input",
            function () {

                clearTimeout(requestTimer);

                confirmationCheckbox.checked =
                    false;

                clearNameValidationMessage();

                const enteredName =
                    nameInput.value.trim();

                if (enteredName.length < 3) {

                    clearSuggestions();

                    return;
                }

                requestTimer =
                    setTimeout(
                        function () {

                            loadSuggestions(
                                enteredName
                            );

                        },
                        450
                    );
            }
        );

        confirmationCheckbox.addEventListener(
            "change",
            refreshSaveButton
        );

        refreshSaveButton();

        async function loadSuggestions(
            enteredName) {

            if (!similarUrl) {
                return;
            }

            if (activeRequest) {
                activeRequest.abort();
            }

            activeRequest =
                new AbortController();

            const specificationId =
                idInput?.value || "";

            const requestUrl =
                similarUrl +
                "?specificationName=" +
                encodeURIComponent(
                    enteredName
                ) +
                "&specificationId=" +
                encodeURIComponent(
                    specificationId
                );

            try {

                const response =
                    await fetch(requestUrl, {
                        method: "GET",
                        cache: "no-store",
                        signal:
                            activeRequest.signal,

                        headers: {
                            "X-Requested-With":
                                "XMLHttpRequest"
                        }
                    });

                if (!response.ok) {
                    return;
                }

                const result =
                    await response.json();

                renderSuggestions(
                    result.records || []
                );

            } catch (error) {

                if (error.name !== "AbortError") {
                    clearSuggestions();
                }
            }
        }

        function renderSuggestions(records) {

            similarList.innerHTML = "";

            if (!records ||
                records.length === 0) {

                clearSuggestions();

                return;
            }

            records.forEach(
                function (record) {

                    similarList.appendChild(
                        createSuggestionElement(
                            record
                        )
                    );
                }
            );

            const hasExactMatch =
                records.some(
                    record =>
                        record.isExactMatch
                );

            warningContainer.classList.remove(
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
                confirmationCheckbox.checked =
                    false;
            }

            refreshSaveButton();
        }

        function createSuggestionElement(
            record) {

            const item =
                document.createElement("div");

            item.className =
                "list-group-item";

            const row =
                document.createElement("div");

            row.className =
                "d-flex justify-content-between " +
                "align-items-center gap-2";

            const name =
                document.createElement("span");

            name.className =
                "fw-semibold";

            name.textContent =
                record.displayText;

            const badge =
                document.createElement("span");

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

            row.appendChild(name);
            row.appendChild(badge);

            item.appendChild(row);

            return item;
        }

        function clearSuggestions() {

            similarList.innerHTML = "";

            warningContainer.classList.add(
                "d-none"
            );

            exactMessage.classList.add(
                "d-none"
            );

            confirmationContainer.classList.add(
                "d-none"
            );

            confirmationCheckbox.checked =
                false;

            refreshSaveButton();
        }

        function refreshSaveButton() {

            const exactDuplicateExists =
                !exactMessage.classList.contains(
                    "d-none"
                );

            const confirmationRequired =
                !confirmationContainer
                    .classList
                    .contains("d-none");

            const confirmed =
                confirmationCheckbox.checked;

            saveButton.disabled =
                exactDuplicateExists ||
                (
                    confirmationRequired &&
                    !confirmed
                );
        }

        function clearNameValidationMessage() {

            const validationMessage =
                document.querySelector(
                    "[data-valmsg-for='SpecificationName']"
                );

            if (!validationMessage) {
                return;
            }

            validationMessage.textContent = "";

            validationMessage.classList.remove(
                "field-validation-error"
            );

            validationMessage.classList.add(
                "field-validation-valid"
            );
        }
    }

})();