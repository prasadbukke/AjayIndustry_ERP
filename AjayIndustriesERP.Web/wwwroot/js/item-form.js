/*
==============================================================

File : item-form.js

Purpose :
Handles Item searchable dropdowns, Quick Create modal,
live master suggestions and similar Item Name warnings.

==============================================================
*/

(function ($) {
    "use strict";

    let activeMasterSelect = null;
    let quickMasterModal = null;
    let suggestionTimer = null;
    let suggestionRequest = null;

    $(document).ready(function () {

        initializeMasterDropdowns();
        initializeQuickMasterEvents();
        initializeSimilarItemCheck();

    });

    /*
    ==========================================================
    Searchable Dropdowns
    ==========================================================
    */

    function initializeMasterDropdowns() {

        $(".js-master-select").each(function () {

            const $select = $(this);

            if ($select.hasClass("select2-hidden-accessible")) {
                $select.select2("destroy");
            }

            const selectId =
                $select.attr("id") || "";

            const placeholder =
                $select.attr("data-placeholder") ||
                "-- Select --";

            const masterType =
                $select.attr("data-master-type") || "";

            const addLabel =
                $select.attr("data-add-label") ||
                "Add New";

            $select.select2({
                width: "100%",
                placeholder: placeholder,
                allowClear: true,
                minimumResultsForSearch: 0,

                language: {
                    noResults: function () {

                        return `
                            <div class="select2-add-master-wrapper">

                                <div class="text-muted small
                                            px-2 pt-2 pb-1">

                                    No records found.

                                </div>

                                <button type="button"
                                        class="btn btn-link
                                               text-decoration-none
                                               text-start
                                               w-100
                                               px-2 py-2
                                               js-open-quick-master"
                                        data-select-id="${escapeHtml(selectId)}"
                                        data-master-type="${escapeHtml(masterType)}"
                                        data-add-label="${escapeHtml(addLabel)}">

                                    <i class="fa-solid fa-plus me-1"></i>

                                    ${escapeHtml(addLabel)}

                                </button>

                            </div>
                        `;
                    }
                },

                escapeMarkup: function (markup) {
                    return markup;
                }
            });
        });
    }

    /*
    ==========================================================
    Event Registration
    ==========================================================
    */

    function initializeQuickMasterEvents() {

        document.addEventListener(
            "pointerdown",
            handleQuickMasterOpen,
            true
        );

        document.addEventListener(
            "input",
            handleQuickMasterNameInput
        );

        document.addEventListener(
            "change",
            handleQuickConfirmationChange
        );

        document.addEventListener(
            "click",
            handleExistingMasterSelection
        );

        document.addEventListener(
            "submit",
            handleQuickMasterSubmit
        );

        const modalElement =
            document.getElementById(
                "quickAddMasterModal"
            );

        if (modalElement) {

            modalElement.addEventListener(
                "hidden.bs.modal",
                resetQuickMasterModal
            );
        }
    }

    /*
    ==========================================================
    Open Modal
    ==========================================================
    */

    function handleQuickMasterOpen(event) {

        const openButton =
            event.target.closest(
                ".js-open-quick-master"
            );

        if (!openButton) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        const selectId =
            openButton.getAttribute(
                "data-select-id"
            );

        const masterType =
            openButton.getAttribute(
                "data-master-type"
            );

        const modalTitle =
            openButton.getAttribute(
                "data-add-label"
            ) || "Add Master";

        const selectElement =
            document.getElementById(selectId);

        if (!selectElement || !masterType) {
            return;
        }

        activeMasterSelect =
            $(selectElement);

        const searchedName =
            $(".select2-container--open " +
                ".select2-search__field")
                .val()
                ?.toString()
                .trim() || "";

        activeMasterSelect.select2("close");

        configureQuickMasterModal(
            masterType,
            modalTitle,
            searchedName
        );
    }

    function configureQuickMasterModal(
        masterType,
        modalTitle,
        searchedName) {

        const modalElement =
            document.getElementById(
                "quickAddMasterModal"
            );

        const form =
            document.getElementById(
                "quickCreateMasterForm"
            );

        if (!modalElement || !form) {
            return;
        }

        form.reset();

        clearQuickSuggestions();
        clearQuickMasterErrors();

        const typeInput =
            document.getElementById(
                "QuickMasterType"
            );

        const requiresCodeInput =
            document.getElementById(
                "QuickRequiresCode"
            );

        const codeContainer =
            document.getElementById(
                "QuickCodeContainer"
            );

        const codeInput =
            document.getElementById(
                "QuickMasterCode"
            );

        const codeLabel =
            document.getElementById(
                "QuickCodeLabel"
            );

        const nameInput =
            document.getElementById(
                "QuickMasterName"
            );

        const nameLabel =
            document.getElementById(
                "QuickNameLabel"
            );

        const titleElement =
            document.getElementById(
                "quickAddMasterModalLabel"
            );

        const isUom =
            masterType.toLowerCase() === "uom";

        typeInput.value =
            masterType;

        requiresCodeInput.value =
            isUom ? "true" : "false";

        codeContainer.classList.toggle(
            "d-none",
            !isUom
        );

        codeInput.required =
            isUom;

        if (!isUom) {
            codeInput.value = "";
        }

        titleElement.textContent =
            modalTitle;

        switch (masterType.toLowerCase()) {

            case "category":

                nameLabel.innerHTML =
                    'Category Name <span class="text-danger">*</span>';

                nameInput.placeholder =
                    "Enter Category Name";

                codeLabel.innerHTML =
                    'Category Code <span class="text-danger">*</span>';

                break;

            case "brand":

                nameLabel.innerHTML =
                    'Brand Name <span class="text-danger">*</span>';

                nameInput.placeholder =
                    "Enter Brand Name";

                codeLabel.innerHTML =
                    'Brand Code <span class="text-danger">*</span>';

                break;

            case "uom":

                nameLabel.innerHTML =
                    'UOM Name <span class="text-danger">*</span>';

                nameInput.placeholder =
                    "Enter UOM Name";

                codeLabel.innerHTML =
                    'UOM Code <span class="text-danger">*</span>';

                codeInput.placeholder =
                    "Example: KG, NOS, MTR";

                break;
        }

        nameInput.value =
            searchedName;

        quickMasterModal =
            bootstrap.Modal.getOrCreateInstance(
                modalElement
            );

        quickMasterModal.show();

        setTimeout(function () {

            nameInput.focus();

            if (searchedName.length >= 3) {

                nameInput.dispatchEvent(
                    new Event(
                        "input",
                        {
                            bubbles: true
                        }
                    )
                );
            }

        }, 200);

        refreshQuickSaveButton();
    }

    /*
    ==========================================================
    Live Suggestions
    ==========================================================
    */

    function handleQuickMasterNameInput(event) {

        if (event.target.id !==
            "QuickMasterName") {

            return;
        }

        clearTimeout(suggestionTimer);

        resetQuickSimilarConfirmation();

        const enteredName =
            event.target.value.trim();

        if (enteredName.length < 3) {

            clearQuickSuggestions();

            return;
        }

        suggestionTimer =
            setTimeout(function () {

                loadQuickSuggestions(
                    enteredName
                );

            }, 450);
    }

    async function loadQuickSuggestions(
        enteredName) {

        const modalElement =
            document.getElementById(
                "quickAddMasterModal"
            );

        const masterType =
            document.getElementById(
                "QuickMasterType"
            )?.value || "";

        const suggestionsUrl =
            modalElement?.getAttribute(
                "data-suggestions-url"
            );

        if (!suggestionsUrl ||
            !masterType) {

            return;
        }

        if (suggestionRequest) {
            suggestionRequest.abort();
        }

        suggestionRequest =
            new AbortController();

        const requestUrl =
            suggestionsUrl +
            "?masterType=" +
            encodeURIComponent(masterType) +
            "&name=" +
            encodeURIComponent(enteredName);

        try {

            const response =
                await fetch(requestUrl, {
                    method: "GET",
                    cache: "no-store",
                    signal:
                        suggestionRequest.signal,
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

            renderQuickSuggestions(
                result.records || []
            );

        } catch (error) {

            if (error.name !== "AbortError") {
                clearQuickSuggestions();
            }
        }
    }

    function renderQuickSuggestions(records) {

        const container =
            document.getElementById(
                "quickSimilarRecordsContainer"
            );

        const list =
            document.getElementById(
                "quickSimilarRecordsList"
            );

        const exactMessage =
            document.getElementById(
                "quickExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "quickSimilarConfirmationContainer"
            );

        if (!container ||
            !list ||
            !exactMessage ||
            !confirmationContainer) {

            return;
        }

        list.innerHTML = "";

        if (!records || records.length === 0) {

            clearQuickSuggestions();

            return;
        }

        const hasExactMatch =
            records.some(
                x => x.isExactMatch
            );

        records.forEach(function (record) {

            list.appendChild(
                createSuggestionElement(record)
            );
        });

        container.classList.remove(
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

        refreshQuickSaveButton();
    }

    function createSuggestionElement(record) {

        const wrapper =
            document.createElement("div");

        wrapper.className =
            "list-group-item";

        const row =
            document.createElement("div");

        row.className =
            "d-flex justify-content-between " +
            "align-items-center gap-2";

        const information =
            document.createElement("div");

        const title =
            document.createElement("div");

        title.className =
            "fw-semibold";

        title.textContent =
            record.displayText;

        const badge =
            document.createElement("span");

        if (record.isExactMatch) {

            badge.className =
                "badge bg-danger mt-1";

            badge.textContent =
                "Exact duplicate";

        } else {

            badge.className =
                "badge bg-warning text-dark mt-1";

            badge.textContent =
                "Similar name";
        }

        information.appendChild(title);
        information.appendChild(badge);

        const selectButton =
            document.createElement("button");

        selectButton.type =
            "button";

        selectButton.className =
            "btn btn-sm btn-outline-primary " +
            "js-select-existing-master";

        selectButton.setAttribute(
            "data-existing-id",
            record.id
        );

        selectButton.setAttribute(
            "data-existing-text",
            record.displayText
        );

        selectButton.textContent =
            "Select Existing";

        row.appendChild(information);
        row.appendChild(selectButton);

        wrapper.appendChild(row);

        return wrapper;
    }

    /*
    ==========================================================
    Confirmation
    ==========================================================
    */

    function handleQuickConfirmationChange(event) {

        if (event.target.id !==
            "QuickSimilarConfirmationCheckbox") {

            return;
        }

        const hiddenInput =
            document.getElementById(
                "QuickConfirmSimilarName"
            );

        hiddenInput.value =
            event.target.checked
                ? "true"
                : "false";

        refreshQuickSaveButton();
    }

    function resetQuickSimilarConfirmation() {

        const checkbox =
            document.getElementById(
                "QuickSimilarConfirmationCheckbox"
            );

        const hiddenInput =
            document.getElementById(
                "QuickConfirmSimilarName"
            );

        if (checkbox) {
            checkbox.checked = false;
        }

        if (hiddenInput) {
            hiddenInput.value = "false";
        }
    }

    function refreshQuickSaveButton() {

        const saveButton =
            document.getElementById(
                "quickCreateMasterSaveButton"
            );

        if (!saveButton ||
            saveButton.dataset.saving === "true") {

            return;
        }

        const exactMessage =
            document.getElementById(
                "quickExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "quickSimilarConfirmationContainer"
            );

        const confirmationCheckbox =
            document.getElementById(
                "QuickSimilarConfirmationCheckbox"
            );

        const hasExactDuplicate =
            exactMessage &&
            !exactMessage.classList.contains(
                "d-none"
            );

        const confirmationRequired =
            confirmationContainer &&
            !confirmationContainer.classList.contains(
                "d-none"
            );

        const confirmed =
            confirmationCheckbox?.checked ||
            false;

        saveButton.disabled =
            hasExactDuplicate ||
            (
                confirmationRequired &&
                !confirmed
            );
    }

    /*
    ==========================================================
    Existing Master Selection
    ==========================================================
    */

    function handleExistingMasterSelection(event) {

        const button =
            event.target.closest(
                ".js-select-existing-master"
            );

        if (!button) {
            return;
        }

        event.preventDefault();

        selectMasterValue(
            button.getAttribute(
                "data-existing-id"
            ),
            button.getAttribute(
                "data-existing-text"
            )
        );

        quickMasterModal?.hide();
    }

    /*
    ==========================================================
    AJAX Save
    ==========================================================
    */

    async function handleQuickMasterSubmit(event) {

        if (event.target.id !==
            "quickCreateMasterForm") {

            return;
        }

        event.preventDefault();

        const form =
            event.target;

        clearQuickMasterErrors();

        if (!form.checkValidity()) {

            form.reportValidity();

            return;
        }

        const saveButton =
            document.getElementById(
                "quickCreateMasterSaveButton"
            );

        if (saveButton.disabled) {
            return;
        }

        const originalHtml =
            saveButton.innerHTML;

        saveButton.dataset.saving =
            "true";

        saveButton.disabled =
            true;

        saveButton.innerHTML = `
            <span class="spinner-border
                         spinner-border-sm
                         me-1">
            </span>

            Saving...
        `;

        try {

            const response =
                await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form),
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest"
                    }
                });

            const result =
                await response.json();

            if (result.records) {

                renderQuickSuggestions(
                    result.records
                );
            }

            if (response.ok &&
                result.success) {

                selectMasterValue(
                    result.id,
                    result.text
                );

                quickMasterModal?.hide();

                if (typeof toastr !==
                    "undefined") {

                    toastr.success(
                        result.message ||
                        "Record created successfully."
                    );
                }

                return;
            }

            showQuickMasterErrors(
                result.errors,
                result.message
            );

        } catch {

            showQuickMasterErrors(
                null,
                "Something went wrong. Please try again."
            );

        } finally {

            saveButton.dataset.saving =
                "false";

            saveButton.innerHTML =
                originalHtml;

            refreshQuickSaveButton();
        }
    }

    /*
    ==========================================================
    Dropdown Value Selection
    ==========================================================
    */

    function selectMasterValue(
        value,
        displayText) {

        if (!activeMasterSelect ||
            !value) {

            return;
        }

        const stringValue =
            value.toString();

        let option =
            activeMasterSelect.find(
                "option"
            ).filter(function () {

                return this.value ===
                    stringValue;
            });

        if (option.length === 0) {

            option =
                new Option(
                    displayText,
                    stringValue,
                    true,
                    true
                );

            activeMasterSelect.append(
                option
            );
        }

        activeMasterSelect
            .val(stringValue)
            .trigger("change");
    }

    /*
    ==========================================================
    Modal Reset
    ==========================================================
    */

    function resetQuickMasterModal() {

        clearTimeout(suggestionTimer);

        if (suggestionRequest) {

            suggestionRequest.abort();

            suggestionRequest = null;
        }

        const form =
            document.getElementById(
                "quickCreateMasterForm"
            );

        if (form) {
            form.reset();
        }

        const codeContainer =
            document.getElementById(
                "QuickCodeContainer"
            );

        const codeInput =
            document.getElementById(
                "QuickMasterCode"
            );

        codeContainer?.classList.add(
            "d-none"
        );

        if (codeInput) {
            codeInput.required = false;
        }

        clearQuickSuggestions();
        clearQuickMasterErrors();

        activeMasterSelect = null;
    }

    /*
    ==========================================================
    Error Helpers
    ==========================================================
    */

    function showQuickMasterErrors(
        errors,
        message) {

        const summary =
            document.getElementById(
                "quickCreateValidationSummary"
            );

        if (!summary) {
            return;
        }

        const messages =
            Array.isArray(errors) &&
                errors.length > 0
                ? errors
                : [
                    message ||
                    "Unable to save the record."
                ];

        const list =
            document.createElement("ul");

        list.className =
            "mb-0";

        messages.forEach(function (text) {

            const item =
                document.createElement("li");

            item.textContent =
                text;

            list.appendChild(item);
        });

        summary.innerHTML = "";
        summary.appendChild(list);
        summary.classList.remove("d-none");
    }

    function clearQuickMasterErrors() {

        const summary =
            document.getElementById(
                "quickCreateValidationSummary"
            );

        if (!summary) {
            return;
        }

        summary.innerHTML = "";
        summary.classList.add("d-none");
    }

    function clearQuickSuggestions() {

        const container =
            document.getElementById(
                "quickSimilarRecordsContainer"
            );

        const list =
            document.getElementById(
                "quickSimilarRecordsList"
            );

        const exactMessage =
            document.getElementById(
                "quickExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "quickSimilarConfirmationContainer"
            );

        if (list) {
            list.innerHTML = "";
        }

        container?.classList.add(
            "d-none"
        );

        exactMessage?.classList.add(
            "d-none"
        );

        confirmationContainer?.classList.add(
            "d-none"
        );

        resetQuickSimilarConfirmation();
        refreshQuickSaveButton();
    }

    /*
    ==========================================================
    Item Similar-Name Check
    ==========================================================
    */

    function initializeSimilarItemCheck() {

        const itemNameInput =
            document.getElementById(
                "ItemName"
            );

        const warningContainer =
            document.getElementById(
                "similarItemWarning"
            );

        const similarItemList =
            document.getElementById(
                "similarItemList"
            );

        const confirmationCheckbox =
            document.getElementById(
                "ConfirmSimilarItemName"
            );

        if (!itemNameInput ||
            !warningContainer ||
            !similarItemList ||
            !confirmationCheckbox) {

            return;
        }

        const similarNameUrl =
            itemNameInput.dataset.similarUrl;

        let timer;

        itemNameInput.addEventListener(
            "input",
            function () {

                clearTimeout(timer);

                confirmationCheckbox.checked =
                    false;

                const itemName =
                    itemNameInput.value.trim();

                if (itemName.length < 3) {

                    similarItemList.innerHTML = "";

                    warningContainer.classList.add(
                        "d-none"
                    );

                    return;
                }

                timer = setTimeout(
                    async function () {

                        const itemId =
                            document.getElementById(
                                "ItemId"
                            )?.value || "";

                        const requestUrl =
                            similarNameUrl +
                            "?itemName=" +
                            encodeURIComponent(itemName) +
                            "&itemId=" +
                            encodeURIComponent(itemId);

                        try {

                            const response =
                                await fetch(requestUrl);

                            if (!response.ok) {
                                return;
                            }

                            const result =
                                await response.json();

                            similarItemList.innerHTML =
                                "";

                            if (!result.hasSimilarItems) {

                                warningContainer.classList.add(
                                    "d-none"
                                );

                                return;
                            }

                            result.items.forEach(
                                function (item) {

                                    const listItem =
                                        document.createElement(
                                            "li"
                                        );

                                    listItem.textContent =
                                        item;

                                    similarItemList.appendChild(
                                        listItem
                                    );
                                }
                            );

                            warningContainer.classList.remove(
                                "d-none"
                            );

                        } catch {
                            // Server validation remains active.
                        }

                    },
                    500
                );
            }
        );
    }

    /*
    ==========================================================
    Security Helper
    ==========================================================
    */

    function escapeHtml(value) {

        return $("<div>")
            .text(value || "")
            .html();
    }

})(jQuery);