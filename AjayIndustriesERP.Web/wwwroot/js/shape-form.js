/*
==============================================================

File : shape-form.js

Purpose :
Provides live exact and similar Shape Name detection.

Features :
- Starts checking after three characters
- Uses request debounce
- Cancels previous pending request
- Blocks exact duplicate save
- Requires confirmation for similar names
- Server-side validation remains authoritative

==============================================================
*/

(function () {
    "use strict";

    document.addEventListener(
        "DOMContentLoaded",
        initializeShapeForm
    );

    function initializeShapeForm() {

        const shapeNameInput =
            document.getElementById("ShapeName");

        const shapeIdInput =
            document.getElementById("ShapeId");

        const warningContainer =
            document.getElementById(
                "shapeSimilarWarning"
            );

        const similarList =
            document.getElementById(
                "shapeSimilarList"
            );

        const exactDuplicateMessage =
            document.getElementById(
                "shapeExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "shapeSimilarConfirmationContainer"
            );

        const confirmationCheckbox =
            document.getElementById(
                "ConfirmSimilarShapeName"
            );

        const saveButton =
            document.getElementById(
                "shapeSaveButton"
            );

        if (!shapeNameInput ||
            !warningContainer ||
            !similarList ||
            !exactDuplicateMessage ||
            !confirmationContainer ||
            !confirmationCheckbox ||
            !saveButton) {

            return;
        }

        const similarCheckUrl =
            shapeNameInput.getAttribute(
                "data-similar-url"
            );

        let requestTimer = null;
        let activeRequest = null;

        shapeNameInput.addEventListener(
            "input",
            function () {

                clearTimeout(requestTimer);

                clearNameValidationMessage();

                confirmationCheckbox.checked =
                    false;

                const shapeName =
                    shapeNameInput.value.trim();

                if (shapeName.length < 3) {

                    clearSuggestions();

                    return;
                }

                requestTimer =
                    setTimeout(
                        function () {

                            loadSuggestions(
                                shapeName
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

        /*
         * Apply correct button state when the server returned
         * similar records after form submission.
         */
        refreshSaveButton();

        async function loadSuggestions(
            shapeName) {

            if (!similarCheckUrl) {
                return;
            }

            if (activeRequest) {
                activeRequest.abort();
            }

            activeRequest =
                new AbortController();

            const shapeId =
                shapeIdInput?.value || "";

            const requestUrl =
                similarCheckUrl +
                "?shapeName=" +
                encodeURIComponent(shapeName) +
                "&shapeId=" +
                encodeURIComponent(shapeId);

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

                if (error.name !==
                    "AbortError") {

                    clearSuggestions();
                }
            }
        }

        function renderSuggestions(
            records) {

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

            exactDuplicateMessage.classList.toggle(
                "d-none",
                !hasExactMatch
            );

            confirmationContainer.classList.toggle(
                "d-none",
                hasExactMatch
            );

            if (hasExactMatch) {
                confirmationCheckbox.checked = false;
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

            exactDuplicateMessage.classList.add(
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
                !exactDuplicateMessage
                    .classList
                    .contains("d-none");

            const confirmationRequired =
                !confirmationContainer
                    .classList
                    .contains("d-none");

            const isConfirmed =
                confirmationCheckbox.checked;

            saveButton.disabled =
                exactDuplicateExists ||
                (
                    confirmationRequired &&
                    !isConfirmed
                );
        }

        function clearNameValidationMessage() {

            const validationMessage =
                document.querySelector(
                    "[data-valmsg-for='ShapeName']"
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