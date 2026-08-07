/*
==============================================================

File : item-form.js

Purpose :
Handles Item Master client-side functionality.

Features :
- Select2 Master dropdowns
- Category Quick Add
- Brand Quick Add
- UOM Quick Add
- Shape Quick Add
- Specification Quick Add
- Quick Master similar-name detection
- Existing Master selection
- Dynamic Item Specification rows
- Duplicate Specification prevention
- Item similar-name detection

==============================================================
*/

(function () {
    "use strict";

    let activeMasterSelect = null;
    let quickSuggestionTimer = null;
    let quickSuggestionRequest = null;
    let itemNameTimer = null;
    let itemNameRequest = null;
    let dynamicRowCounter = 0;

    document.addEventListener(
        "DOMContentLoaded",
        initializeItemForm
    );

    function initializeItemForm() {

        initializeMasterSelects(document);

        initializeDynamicSpecifications();

        initializeQuickMasterModal();

        initializeItemNameCheck();

        updateSpecificationSortOrders();

        updateSpecificationEmptyState();
    }

    // =========================================================
    // SELECT2 MASTER DROPDOWNS
    // =========================================================

    function initializeMasterSelects(container) {

        if (!window.jQuery ||
            !jQuery.fn.select2) {

            return;
        }

        const $container =
            jQuery(container);

        $container
            .find(".js-master-select")
            .each(function () {

                initializeMasterSelect(
                    jQuery(this)
                );
            });
    }

    function initializeMasterSelect($select) {

        if ($select.hasClass(
            "select2-hidden-accessible")) {

            return;
        }

        const placeholder =
            $select.data("placeholder") ||
            "-- Select --";

        const masterType =
            $select.data("master-type") ||
            "";

        const addLabel =
            $select.data("add-label") ||
            "Add New";

        ensureSelectId($select);

        $select.select2({

            width: "100%",

            placeholder:
                placeholder,

            allowClear: true,

            language: {

                noResults: function () {

                    if (!masterType) {

                        return "No results found";
                    }

                    const selectId =
                        $select.attr("id");

                    const searchedText =
                        String(
                            $select.data(
                                "last-search"
                            ) || ""
                        ).trim();

                    return `
                        <button type="button"
                                class="btn btn-link
                                       text-decoration-none
                                       p-1
                                       js-open-quick-master"
                                data-select-id="${escapeHtml(selectId)}"
                                data-master-type="${escapeHtml(masterType)}"
                                data-search-text="${escapeHtml(searchedText)}">
                            <i class="fa-solid fa-plus me-1"></i>
                            ${escapeHtml(addLabel)}
                        </button>
                    `;
                }
            },

            escapeMarkup:
                function (markup) {
                    return markup;
                }
        });

        $select.on(
            "select2:open",
            function () {

                activeMasterSelect =
                    $select;

                setTimeout(
                    function () {

                        const searchInput =
                            document.querySelector(
                                ".select2-container--open " +
                                ".select2-search__field"
                            );

                        if (!searchInput) {
                            return;
                        }

                        $select.data(
                            "last-search",
                            searchInput.value || ""
                        );

                        searchInput.addEventListener(
                            "input",
                            function () {

                                $select.data(
                                    "last-search",
                                    searchInput.value || ""
                                );
                            }
                        );

                    },
                    0
                );
            }
        );
    }

    function ensureSelectId($select) {

        if ($select.attr("id")) {
            return;
        }

        dynamicRowCounter++;

        $select.attr(
            "id",
            "masterSelect_" +
            Date.now() +
            "_" +
            dynamicRowCounter
        );
    }

    // =========================================================
    // QUICK MASTER MODAL
    // =========================================================

    function initializeQuickMasterModal() {

        const modal =
            document.getElementById(
                "quickAddMasterModal"
            );

        const form =
            document.getElementById(
                "quickCreateMasterForm"
            );

        if (!modal || !form) {
            return;
        }

        document.addEventListener(
            "click",
            function (event) {

                const button =
                    event.target.closest(
                        ".js-open-quick-master"
                    );

                if (!button) {
                    return;
                }

                event.preventDefault();

                const selectId =
                    button.getAttribute(
                        "data-select-id"
                    );

                if (selectId &&
                    window.jQuery) {

                    const selectedControl =
                        document.getElementById(
                            selectId
                        );

                    if (selectedControl) {

                        activeMasterSelect =
                            jQuery(
                                selectedControl
                            );
                    }
                }

                if (!activeMasterSelect ||
                    activeMasterSelect.length === 0) {

                    return;
                }

                const masterType =
                    button.getAttribute(
                        "data-master-type"
                    ) ||
                    activeMasterSelect.data(
                        "master-type"
                    );

                const searchText =
                    button.getAttribute(
                        "data-search-text"
                    ) ||
                    activeMasterSelect.data(
                        "last-search"
                    ) ||
                    "";

                openQuickMasterModal(
                    masterType,
                    searchText
                );
            }
        );

        const nameInput =
            document.getElementById(
                "QuickMasterName"
            );

        if (nameInput) {

            nameInput.addEventListener(
                "input",
                function () {

                    clearTimeout(
                        quickSuggestionTimer
                    );

                    resetQuickConfirmation();

                    const name =
                        nameInput.value.trim();

                    if (name.length < 3) {

                        clearQuickSuggestions();

                        return;
                    }

                    quickSuggestionTimer =
                        setTimeout(
                            loadQuickSuggestions,
                            450
                        );
                }
            );
        }

        const confirmCheckbox =
            document.getElementById(
                "QuickConfirmSimilarName"
            );

        if (confirmCheckbox) {

            confirmCheckbox.addEventListener(
                "change",
                refreshQuickSaveButton
            );
        }

        document.addEventListener(
            "click",
            function (event) {

                const button =
                    event.target.closest(
                        ".js-use-existing-master"
                    );

                if (!button) {
                    return;
                }

                event.preventDefault();

                const id =
                    button.getAttribute(
                        "data-id"
                    );

                const text =
                    button.getAttribute(
                        "data-text"
                    );

                selectMasterValue(
                    id,
                    text
                );

                hideQuickMasterModal();
            }
        );

        form.addEventListener(
            "submit",
            submitQuickMasterForm
        );
    }

    function openQuickMasterModal(
        masterType,
        searchText) {

        const modal =
            document.getElementById(
                "quickAddMasterModal"
            );

        const form =
            document.getElementById(
                "quickCreateMasterForm"
            );

        if (!modal || !form) {
            return;
        }

        form.reset();

        clearQuickSuggestions();

        const normalizedType =
            normalizeMasterType(
                masterType
            );

        const masterTypeInput =
            document.getElementById(
                "QuickMasterType"
            );

        const requiresCodeInput =
            document.getElementById(
                "QuickRequiresCode"
            );

        const nameInput =
            document.getElementById(
                "QuickMasterName"
            );

        const codeInput =
            document.getElementById(
                "QuickMasterCode"
            );

        const codeContainer =
            document.getElementById(
                "QuickCodeContainer"
            );

        const modalTitle =
            modal.querySelector(
                ".modal-title"
            );

        const nameLabel =
            modal.querySelector(
                "label[for='QuickMasterName']"
            );

        const codeLabel =
            modal.querySelector(
                "label[for='QuickMasterCode']"
            );

        const config =
            getMasterConfiguration(
                normalizedType
            );

        if (!config) {
            return;
        }

        if (masterTypeInput) {

            masterTypeInput.value =
                config.masterType;
        }

        if (requiresCodeInput) {

            requiresCodeInput.value =
                config.requiresCode
                    ? "true"
                    : "false";
        }

        if (modalTitle) {

            modalTitle.textContent =
                config.title;
        }

        if (nameLabel) {

            nameLabel.innerHTML =
                `${escapeHtml(config.nameLabel)}
                 <span class="text-danger">*</span>`;
        }

        if (nameInput) {

            nameInput.value =
                String(searchText || "").trim();

            nameInput.placeholder =
                config.namePlaceholder;
        }

        if (codeLabel) {

            codeLabel.innerHTML =
                config.requiresCode
                    ? `${escapeHtml(config.codeLabel)}
                       <span class="text-danger">*</span>`
                    : escapeHtml(
                        config.codeLabel
                    );
        }

        if (codeInput) {

            codeInput.value = "";

            codeInput.required =
                config.requiresCode;

            codeInput.placeholder =
                config.codePlaceholder ||
                "";
        }

        if (codeContainer) {

            codeContainer.classList.toggle(
                "d-none",
                !config.requiresCode
            );
        }

        resetQuickConfirmation();

        const bootstrapModal =
            bootstrap.Modal
                .getOrCreateInstance(
                    modal
                );

        bootstrapModal.show();

        setTimeout(
            function () {

                if (nameInput) {

                    nameInput.focus();

                    if (nameInput.value) {

                        nameInput.dispatchEvent(
                            new Event("input")
                        );
                    }
                }

            },
            250
        );
    }

    function getMasterConfiguration(
        masterType) {

        switch (
        normalizeMasterType(masterType)
        ) {

            case "Category":

                return {
                    masterType:
                        "Category",

                    title:
                        "Add Category",

                    nameLabel:
                        "Category Name",

                    namePlaceholder:
                        "Enter Category Name",

                    codeLabel:
                        "Category Code",

                    codePlaceholder:
                        "",

                    requiresCode:
                        false
                };

            case "Brand":

                return {
                    masterType:
                        "Brand",

                    title:
                        "Add Brand",

                    nameLabel:
                        "Brand Name",

                    namePlaceholder:
                        "Enter Brand Name",

                    codeLabel:
                        "Brand Code",

                    codePlaceholder:
                        "",

                    requiresCode:
                        false
                };

            case "Uom":

                return {
                    masterType:
                        "Uom",

                    title:
                        "Add UOM",

                    nameLabel:
                        "UOM Name",

                    namePlaceholder:
                        "Enter UOM Name",

                    codeLabel:
                        "UOM Code",

                    codePlaceholder:
                        "Example: MM, KG, NOS",

                    requiresCode:
                        true
                };

            case "Shape":

                return {
                    masterType:
                        "Shape",

                    title:
                        "Add Shape",

                    nameLabel:
                        "Shape Name",

                    namePlaceholder:
                        "Enter Shape Name",

                    codeLabel:
                        "Shape Code",

                    codePlaceholder:
                        "",

                    requiresCode:
                        false
                };

            case "Specification":

                return {
                    masterType:
                        "Specification",

                    title:
                        "Add Specification",

                    nameLabel:
                        "Specification Name",

                    namePlaceholder:
                        "Example: Diameter",

                    codeLabel:
                        "Specification Code",

                    codePlaceholder:
                        "",

                    requiresCode:
                        false
                };

            default:

                return null;
        }
    }

    function normalizeMasterType(
        masterType) {

        const value =
            String(masterType || "")
                .trim()
                .toLowerCase();

        switch (value) {

            case "category":
            case "itemcategory":
                return "Category";

            case "brand":
                return "Brand";

            case "uom":
            case "unit":
                return "Uom";

            case "shape":
                return "Shape";

            case "specification":
            case "specifications":
                return "Specification";

            default:
                return "";
        }
    }

    async function loadQuickSuggestions() {

        const modal =
            document.getElementById(
                "quickAddMasterModal"
            );

        const typeInput =
            document.getElementById(
                "QuickMasterType"
            );

        const nameInput =
            document.getElementById(
                "QuickMasterName"
            );

        if (!modal ||
            !typeInput ||
            !nameInput) {

            return;
        }

        const name =
            nameInput.value.trim();

        if (name.length < 3) {

            clearQuickSuggestions();

            return;
        }

        const url =
            modal.getAttribute(
                "data-suggestions-url"
            );

        if (!url) {
            return;
        }

        if (quickSuggestionRequest) {

            quickSuggestionRequest.abort();
        }

        quickSuggestionRequest =
            new AbortController();

        const requestUrl =
            url +
            "?masterType=" +
            encodeURIComponent(
                typeInput.value
            ) +
            "&name=" +
            encodeURIComponent(name);

        try {

            const response =
                await fetch(
                    requestUrl,
                    {
                        cache:
                            "no-store",

                        signal:
                            quickSuggestionRequest.signal
                    }
                );

            if (!response.ok) {

                clearQuickSuggestions();

                return;
            }

            const result =
                await response.json();

            renderQuickSuggestions(
                result.records || []
            );

        } catch (error) {

            if (error.name !==
                "AbortError") {

                clearQuickSuggestions();
            }
        }
    }

    function renderQuickSuggestions(
        records) {

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

        const confirmCheckbox =
            document.getElementById(
                "QuickConfirmSimilarName"
            );

        if (!container || !list) {
            return;
        }

        list.innerHTML = "";

        if (!records ||
            records.length === 0) {

            clearQuickSuggestions();

            return;
        }

        let hasExactMatch = false;

        records.forEach(
            function (record) {

                if (record.isExactMatch) {

                    hasExactMatch = true;
                }

                const button =
                    document.createElement(
                        "button"
                    );

                button.type =
                    "button";

                button.className =
                    "list-group-item " +
                    "list-group-item-action " +
                    "js-use-existing-master";

                button.setAttribute(
                    "data-id",
                    record.id
                );

                button.setAttribute(
                    "data-text",
                    record.displayText
                );

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

                badge.className =
                    record.isExactMatch
                        ? "badge bg-danger"
                        : "badge bg-warning text-dark";

                badge.textContent =
                    record.isExactMatch
                        ? "Exact"
                        : "Similar";

                row.appendChild(text);
                row.appendChild(badge);

                button.appendChild(row);

                list.appendChild(button);
            }
        );

        container.classList.remove(
            "d-none"
        );

        if (exactMessage) {

            exactMessage.classList.toggle(
                "d-none",
                !hasExactMatch
            );
        }

        if (confirmationContainer) {

            confirmationContainer
                .classList
                .toggle(
                    "d-none",
                    hasExactMatch
                );
        }

        if (hasExactMatch &&
            confirmCheckbox) {

            confirmCheckbox.checked =
                false;
        }

        refreshQuickSaveButton();
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

        if (container) {

            container.classList.add(
                "d-none"
            );
        }

        if (exactMessage) {

            exactMessage.classList.add(
                "d-none"
            );
        }

        if (confirmationContainer) {

            confirmationContainer
                .classList
                .add("d-none");
        }

        refreshQuickSaveButton();
    }

    function resetQuickConfirmation() {

        const checkbox =
            document.getElementById(
                "QuickConfirmSimilarName"
            );

        if (checkbox) {

            checkbox.checked =
                false;
        }

        clearQuickSuggestions();
    }

    function refreshQuickSaveButton() {

        const button =
            document.getElementById(
                "quickCreateMasterSaveButton"
            );

        const exactMessage =
            document.getElementById(
                "quickExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "quickSimilarConfirmationContainer"
            );

        const confirmation =
            document.getElementById(
                "QuickConfirmSimilarName"
            );

        if (!button) {
            return;
        }

        const exactExists =
            exactMessage &&
            !exactMessage.classList
                .contains("d-none");

        const confirmationRequired =
            confirmationContainer &&
            !confirmationContainer
                .classList
                .contains("d-none");

        button.disabled =
            Boolean(exactExists) ||
            Boolean(
                confirmationRequired &&
                confirmation &&
                !confirmation.checked
            );
    }

    async function submitQuickMasterForm(
        event) {

        event.preventDefault();

        const form =
            event.currentTarget;

        const saveButton =
            document.getElementById(
                "quickCreateMasterSaveButton"
            );

        if (saveButton) {

            saveButton.disabled =
                true;
        }

        try {

            const response =
                await fetch(
                    form.action,
                    {
                        method:
                            "POST",

                        body:
                            new FormData(form),

                        headers: {
                            "X-Requested-With":
                                "XMLHttpRequest"
                        }
                    }
                );

            const result =
                await response.json();

            if (response.ok &&
                result.success) {

                selectMasterValue(
                    result.id,
                    result.text
                );

                hideQuickMasterModal();

                showSuccess(
                    result.message ||
                    "Record created successfully."
                );

                return;
            }

            if (result.records) {

                renderQuickSuggestions(
                    result.records
                );
            }

            showError(
                result.message ||
                "Unable to save record."
            );

        } catch (error) {

            showError(
                "Something went wrong. Please try again."
            );

        } finally {

            refreshQuickSaveButton();
        }
    }

    function selectMasterValue(
        id,
        text) {

        if (!activeMasterSelect ||
            activeMasterSelect.length === 0) {

            return;
        }

        const stringId =
            String(id);

        let option = null;

        activeMasterSelect
            .find("option")
            .each(function () {

                if (this.value ===
                    stringId) {

                    option = this;
                }
            });

        if (!option) {

            option =
                new Option(
                    text,
                    stringId,
                    true,
                    true
                );

            activeMasterSelect.append(
                option
            );
        }

        activeMasterSelect
            .val(stringId)
            .trigger("change");

        /*
         * A newly created Specification must become
         * available in all Specification rows.
         */
        const masterType =
            normalizeMasterType(
                activeMasterSelect.data(
                    "master-type"
                )
            );

        if (masterType ===
            "Specification") {

            synchronizeNewOption(
                ".js-specification-select",
                stringId,
                text
            );
        }

        if (masterType === "Uom") {

            synchronizeNewOption(
                ".js-specification-uom-select",
                stringId,
                text
            );
        }

        validateDuplicateSpecifications();
    }

    function synchronizeNewOption(
        selector,
        value,
        text) {

        document
            .querySelectorAll(selector)
            .forEach(
                function (select) {

                    const exists =
                        Array.from(
                            select.options
                        )
                            .some(
                                option =>
                                    option.value ===
                                    value
                            );

                    if (!exists) {

                        select.add(
                            new Option(
                                text,
                                value,
                                false,
                                false
                            )
                        );
                    }
                }
            );
    }

    function hideQuickMasterModal() {

        const modal =
            document.getElementById(
                "quickAddMasterModal"
            );

        if (!modal) {
            return;
        }

        const instance =
            bootstrap.Modal
                .getInstance(modal);

        if (instance) {
            instance.hide();
        }
    }

    // =========================================================
    // DYNAMIC ITEM SPECIFICATIONS
    // =========================================================

    function initializeDynamicSpecifications() {

        const addButton =
            document.getElementById(
                "addItemSpecificationButton"
            );

        const rowsContainer =
            document.getElementById(
                "itemSpecificationRows"
            );

        const template =
            document.getElementById(
                "itemSpecificationRowTemplate"
            );

        if (!addButton ||
            !rowsContainer ||
            !template) {

            return;
        }

        addButton.addEventListener(
            "click",
            function () {

                dynamicRowCounter++;

                const key =
                    "new_" +
                    Date.now() +
                    "_" +
                    dynamicRowCounter;

                const html =
                    template.innerHTML
                        .replaceAll(
                            "__KEY__",
                            key
                        );

                const wrapper =
                    document.createElement(
                        "tbody"
                    );

                wrapper.innerHTML =
                    html.trim();

                const newRow =
                    wrapper.firstElementChild;

                if (!newRow) {
                    return;
                }

                rowsContainer.appendChild(
                    newRow
                );

                initializeMasterSelects(
                    newRow
                );

                updateSpecificationSortOrders();

                updateSpecificationEmptyState();

                const firstSelect =
                    newRow.querySelector(
                        ".js-specification-select"
                    );

                if (firstSelect &&
                    window.jQuery &&
                    jQuery.fn.select2) {

                    jQuery(firstSelect)
                        .select2("open");
                }
            }
        );

        rowsContainer.addEventListener(
            "click",
            function (event) {

                const removeButton =
                    event.target.closest(
                        ".js-remove-specification"
                    );

                if (!removeButton) {
                    return;
                }

                const row =
                    removeButton.closest(
                        ".item-specification-row"
                    );

                if (!row) {
                    return;
                }

                destroySelect2InRow(row);

                row.remove();

                updateSpecificationSortOrders();

                updateSpecificationEmptyState();

                validateDuplicateSpecifications();
            }
        );

        rowsContainer.addEventListener(
            "change",
            function (event) {

                if (!event.target.classList
                    .contains(
                        "js-specification-select"
                    )) {

                    return;
                }

                validateDuplicateSpecifications(
                    event.target
                );
            }
        );

        if (window.jQuery) {

            jQuery(document).on(
                "select2:select",
                ".js-specification-select",
                function () {

                    validateDuplicateSpecifications(
                        this
                    );
                }
            );
        }
    }

    function destroySelect2InRow(row) {

        if (!window.jQuery ||
            !jQuery.fn.select2) {

            return;
        }

        jQuery(row)
            .find(
                ".select2-hidden-accessible"
            )
            .each(function () {

                jQuery(this)
                    .select2("destroy");
            });
    }

    function updateSpecificationSortOrders() {

        const rows =
            document.querySelectorAll(
                ".item-specification-row"
            );

        rows.forEach(
            function (row, index) {

                const input =
                    row.querySelector(
                        ".js-spec-sort-order"
                    );

                if (input) {

                    input.value =
                        index + 1;
                }
            }
        );
    }

    function updateSpecificationEmptyState() {

        const emptyRow =
            document.getElementById(
                "itemSpecificationEmptyRow"
            );

        if (!emptyRow) {
            return;
        }

        const rowCount =
            document.querySelectorAll(
                ".item-specification-row"
            ).length;

        emptyRow.classList.toggle(
            "d-none",
            rowCount > 0
        );
    }

    function validateDuplicateSpecifications(
        changedSelect) {

        const selectedValues =
            new Map();

        let duplicateFound =
            false;

        document
            .querySelectorAll(
                ".js-specification-select"
            )
            .forEach(
                function (select) {

                    const value =
                        String(
                            select.value || ""
                        ).trim();

                    if (!value) {
                        return;
                    }

                    if (selectedValues
                        .has(value)) {

                        duplicateFound =
                            true;

                        if (changedSelect ===
                            select) {

                            if (window.jQuery &&
                                jQuery.fn.select2) {

                                jQuery(select)
                                    .val(null)
                                    .trigger(
                                        "change.select2"
                                    );

                            } else {

                                select.value = "";
                            }

                            showError(
                                "The same Specification cannot be added more than once."
                            );
                        }

                        return;
                    }

                    selectedValues.set(
                        value,
                        select
                    );
                }
            );

        return !duplicateFound;
    }

    // =========================================================
    // ITEM NAME SIMILARITY
    // =========================================================

    function initializeItemNameCheck() {

        const input =
            document.getElementById(
                "ItemName"
            );

        if (!input) {
            return;
        }

        input.addEventListener(
            "input",
            function () {

                clearTimeout(
                    itemNameTimer
                );

                const value =
                    input.value.trim();

                const confirmation =
                    document.getElementById(
                        "ConfirmSimilarItemName"
                    );

                if (confirmation) {

                    confirmation.checked =
                        false;
                }

                if (value.length < 3) {

                    clearItemSuggestions();

                    return;
                }

                itemNameTimer =
                    setTimeout(
                        loadItemSuggestions,
                        450
                    );
            }
        );

        const confirmation =
            document.getElementById(
                "ConfirmSimilarItemName"
            );

        if (confirmation) {

            confirmation.addEventListener(
                "change",
                refreshItemSaveButton
            );
        }

        refreshItemSaveButton();
    }

    async function loadItemSuggestions() {

        const input =
            document.getElementById(
                "ItemName"
            );

        const itemId =
            document.getElementById(
                "ItemId"
            );

        if (!input) {
            return;
        }

        const url =
            input.getAttribute(
                "data-similar-url"
            );

        const value =
            input.value.trim();

        if (!url ||
            value.length < 3) {

            return;
        }

        if (itemNameRequest) {

            itemNameRequest.abort();
        }

        itemNameRequest =
            new AbortController();

        const requestUrl =
            url +
            "?itemName=" +
            encodeURIComponent(value) +
            "&itemId=" +
            encodeURIComponent(
                itemId?.value || ""
            );

        try {

            const response =
                await fetch(
                    requestUrl,
                    {
                        cache:
                            "no-store",

                        signal:
                            itemNameRequest.signal
                    }
                );

            if (!response.ok) {
                return;
            }

            const result =
                await response.json();

            renderItemSuggestions(
                result
            );

        } catch (error) {

            if (error.name !==
                "AbortError") {

                clearItemSuggestions();
            }
        }
    }

    function renderItemSuggestions(
        result) {

        const warning =
            document.getElementById(
                "similarItemWarning"
            );

        const list =
            document.getElementById(
                "similarItemList"
            );

        const sameNameMessage =
            document.getElementById(
                "itemExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "itemSimilarConfirmationContainer"
            );

        const confirmation =
            document.getElementById(
                "ConfirmSimilarItemName"
            );

        if (!warning || !list) {
            return;
        }

        const items =
            result.items || [];

        if (!result.hasSimilarItems ||
            items.length === 0) {

            clearItemSuggestions();

            return;
        }

        list.innerHTML = "";

        items.forEach(
            function (item) {

                const li =
                    document.createElement(
                        "li"
                    );

                li.textContent =
                    item;

                list.appendChild(li);
            }
        );

        warning.classList.remove(
            "d-none"
        );

        /*
         * Same Item Name is only informational.
         *
         * Final duplicate validation uses:
         * Name + Shape + Specifications.
         */
        const hasSameName =
            Boolean(
                result.hasSameName
            );

        if (sameNameMessage) {

            sameNameMessage.classList.toggle(
                "d-none",
                !hasSameName
            );
        }

        /*
         * Similar/same names still require the user
         * to review existing Items.
         */
        if (confirmationContainer) {

            confirmationContainer
                .classList
                .remove("d-none");
        }

        if (confirmation) {

            confirmation.checked =
                false;
        }

        refreshItemSaveButton();
    }

    function clearItemSuggestions() {

        const warning =
            document.getElementById(
                "similarItemWarning"
            );

        const list =
            document.getElementById(
                "similarItemList"
            );

        const exactMessage =
            document.getElementById(
                "itemExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "itemSimilarConfirmationContainer"
            );

        const confirmation =
            document.getElementById(
                "ConfirmSimilarItemName"
            );

        if (list) {
            list.innerHTML = "";
        }

        if (warning) {

            warning.classList.add(
                "d-none"
            );
        }

        if (exactMessage) {

            exactMessage.classList.add(
                "d-none"
            );
        }

        if (confirmationContainer) {

            confirmationContainer
                .classList
                .remove("d-none");
        }

        if (confirmation) {

            confirmation.checked =
                false;
        }

        refreshItemSaveButton();
    }

    function refreshItemSaveButton() {

        const saveButton =
            document.getElementById(
                "itemSaveButton"
            );

        const warning =
            document.getElementById(
                "similarItemWarning"
            );

        const confirmationContainer =
            document.getElementById(
                "itemSimilarConfirmationContainer"
            );

        const confirmation =
            document.getElementById(
                "ConfirmSimilarItemName"
            );

        if (!saveButton) {
            return;
        }

        const warningVisible =
            warning &&
            !warning.classList
                .contains("d-none");

        const confirmationRequired =
            warningVisible &&
            confirmationContainer &&
            !confirmationContainer
                .classList
                .contains("d-none");

        /*
         * Same Item Name does NOT disable Save.
         *
         * User only confirms that existing similar/same
         * Items have been reviewed.
         *
         * Exact configuration duplicate is enforced
         * securely by ItemService during Save.
         */
        saveButton.disabled =
            Boolean(
                confirmationRequired &&
                confirmation &&
                !confirmation.checked
            );
    }

    // =========================================================
    // NOTIFICATION HELPERS
    // =========================================================

    function showSuccess(message) {

        if (window.toastr) {

            toastr.success(message);

            return;
        }

        console.log(message);
    }

    function showError(message) {

        if (window.toastr) {

            toastr.error(message);

            return;
        }

        alert(message);
    }

    // =========================================================
    // HTML HELPERS
    // =========================================================

    function escapeHtml(value) {

        return String(value || "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

})();