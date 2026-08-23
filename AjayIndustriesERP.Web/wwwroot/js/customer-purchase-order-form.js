/*
============================================================

File: customer-purchase-order-form.js

Purpose:
Handles Customer Purchase Order Create/Edit client-side
functionality.

Responsibilities:
- Initialize searchable Item dropdowns using Select2.
- Manage dynamic Customer PO Item rows.
- Load Item Master information through AJAX.
- Display current Item Drawing information.
- Prevent duplicate Items in the same Customer PO.
- Collapse and expand Customer PO Item rows.
- Reindex dynamic Item rows.
- Reparse unobtrusive validation.
- Check similar Customer PO Numbers while typing.
- Block exact duplicate Customer PO Number.
- Require confirmation before continuing with similar PO
  Numbers.

Important:
- Raw Material filtering is enforced by Repository.
- Same Item cannot appear more than once in one Customer PO.
- Customer PO Number exact duplicate is always blocked.
- Similar Customer PO Number is warning-only after user
  confirmation.
- Save-time business validation remains in Application Service.

============================================================
*/

(function () {
    "use strict";

    let customerPoNumberTimer = null;
    let customerPoNumberRequest = null;

    document.addEventListener(
        "DOMContentLoaded",
        initializeCustomerPurchaseOrderForm
    );


    // =========================================================
    // INITIALIZATION
    // =========================================================

    function initializeCustomerPurchaseOrderForm() {

        const form =
            document.getElementById(
                "customerPurchaseOrderForm"
            );

        const container =
            document.getElementById(
                "customerPoItemsContainer"
            );

        const template =
            document.getElementById(
                "customerPoItemTemplate"
            );

        const addButton =
            document.getElementById(
                "btnAddItem"
            );

        const emptyMessage =
            document.getElementById(
                "emptyItemMessage"
            );


        if (!form ||
            !container ||
            !template ||
            !addButton ||
            !emptyMessage) {

            return;
        }


        initializeItemRows(
            form,
            container,
            template,
            addButton,
            emptyMessage
        );


        initializeCustomerPoNumberCheck(
            form
        );
    }


    // =========================================================
    // CUSTOMER PO ITEM ROWS
    // =========================================================

    function initializeItemRows(
        form,
        container,
        template,
        addButton,
        emptyMessage) {

        // =====================================================
        // EMPTY STATE
        // =====================================================

        function updateEmptyState() {

            const rows =
                container.querySelectorAll(
                    ".customer-po-item-row"
                );


            emptyMessage.classList.toggle(
                "d-none",
                rows.length > 0
            );
        }


        // =====================================================
        // COLLAPSE / EXPAND
        // =====================================================

        function collapseRow(
            row) {

            if (!row) {
                return;
            }


            row.classList.add(
                "is-collapsed"
            );


            const body =
                row.querySelector(
                    ".item-row-body"
                );


            if (body) {

                body.classList.add(
                    "d-none"
                );
            }
        }


        function expandRow(
            row) {

            if (!row) {
                return;
            }


            row.classList.remove(
                "is-collapsed"
            );


            const body =
                row.querySelector(
                    ".item-row-body"
                );


            if (body) {

                body.classList.remove(
                    "d-none"
                );
            }
        }


        function collapseAllExcept(
            activeRow) {

            const rows =
                container.querySelectorAll(
                    ".customer-po-item-row"
                );


            rows.forEach(
                function (row) {

                    if (row === activeRow) {

                        expandRow(
                            row
                        );
                    }
                    else {

                        collapseRow(
                            row
                        );
                    }
                }
            );
        }


        // =====================================================
        // ITEM ROW SUMMARY
        // =====================================================

        function updateRowSummary(
            row,
            itemCode,
            itemName) {

            const summary =
                row.querySelector(
                    ".item-row-summary"
                );


            if (!summary) {
                return;
            }


            if (!itemCode &&
                !itemName) {

                summary.textContent =
                    "";

                return;
            }


            if (itemCode &&
                itemName) {

                summary.textContent =
                    itemCode +
                    " - " +
                    itemName;

                return;
            }


            summary.textContent =
                itemCode ||
                itemName ||
                "";
        }


        // =====================================================
        // SELECT2
        // =====================================================

        function hasSelect2() {

            return (
                window.jQuery &&
                jQuery.fn &&
                typeof jQuery.fn.select2 ===
                "function"
            );
        }


        function destroyItemSelects(
            scope) {

            if (!hasSelect2()) {
                return;
            }


            jQuery(scope)
                .find(
                    ".js-customer-po-item-select." +
                    "select2-hidden-accessible"
                )
                .each(
                    function () {

                        jQuery(this)
                            .off(
                                ".customerPoItem"
                            )
                            .select2(
                                "destroy"
                            );
                    }
                );
        }


        function initializeItemSelects(
            scope) {

            if (!hasSelect2()) {
                return;
            }


            jQuery(scope)
                .find(
                    ".js-customer-po-item-select"
                )
                .each(
                    function () {

                        const $select =
                            jQuery(this);


                        if ($select.hasClass(
                            "select2-hidden-accessible")) {

                            return;
                        }


                        $select.select2({

                            width:
                                "100%",

                            placeholder:
                                "-- Select Item --",

                            allowClear:
                                true,

                            minimumResultsForSearch:
                                0,

                            language: {

                                noResults:
                                    function () {

                                        return "No Items found";
                                    }
                            }
                        });


                        /*
                         * Direct Select2 events are handled here
                         * so Item details always reload correctly.
                         */
                        $select
                            .off(
                                ".customerPoItem"
                            )
                            .on(
                                "select2:select.customerPoItem",
                                function () {

                                    handleItemSelection(
                                        this
                                    );
                                }
                            )
                            .on(
                                "select2:clear.customerPoItem",
                                function () {

                                    handleItemSelection(
                                        this
                                    );
                                }
                            );
                    }
                );
        }


        // =====================================================
        // DUPLICATE ITEM PROTECTION
        // =====================================================

        function isDuplicateItemSelection(
            currentSelect) {

            const selectedValue =
                String(
                    currentSelect.value ||
                    ""
                ).trim();


            if (!selectedValue) {
                return false;
            }


            return Array
                .from(
                    container.querySelectorAll(
                        ".item-select"
                    )
                )
                .some(
                    function (select) {

                        return (
                            select !==
                            currentSelect
                            &&
                            String(
                                select.value ||
                                ""
                            ).trim() ===
                            selectedValue
                        );
                    }
                );
        }


        function refreshDuplicateItemOptions() {

            const itemSelects =
                Array.from(
                    container.querySelectorAll(
                        ".item-select"
                    )
                );


            const selectedValues =
                itemSelects
                    .map(
                        function (select) {

                            return String(
                                select.value ||
                                ""
                            ).trim();
                        }
                    )
                    .filter(
                        function (value) {

                            return Boolean(
                                value
                            );
                        }
                    );


            itemSelects.forEach(
                function (select) {

                    Array
                        .from(
                            select.options
                        )
                        .forEach(
                            function (option) {

                                if (!option.value) {

                                    option.disabled =
                                        false;

                                    return;
                                }


                                option.disabled =
                                    selectedValues.includes(
                                        String(
                                            option.value
                                        )
                                    )
                                    &&
                                    String(
                                        select.value ||
                                        ""
                                    ) !==
                                    String(
                                        option.value
                                    );
                            }
                        );
                }
            );
        }


        // =====================================================
        // REINDEX ITEM ROWS
        // =====================================================

        function reindexRows() {

            destroyItemSelects(
                container
            );


            const rows =
                container.querySelectorAll(
                    ".customer-po-item-row"
                );


            rows.forEach(
                function (
                    row,
                    index) {

                    row.dataset.index =
                        index;


                    // =========================================
                    // TITLE
                    // =========================================

                    const title =
                        row.querySelector(
                            ".item-row-title"
                        );


                    if (title) {

                        title.textContent =
                            "Item " +
                            (index + 1);
                    }


                    // =========================================
                    // NAME ATTRIBUTES
                    // =========================================

                    row.querySelectorAll(
                        "[name]"
                    ).forEach(
                        function (element) {

                            element.name =
                                element.name.replace(
                                    /Items\[\d+\]/g,
                                    "Items[" +
                                    index +
                                    "]"
                                );
                        }
                    );


                    // =========================================
                    // ID ATTRIBUTES
                    // =========================================

                    row.querySelectorAll(
                        "[id]"
                    ).forEach(
                        function (element) {

                            element.id =
                                element.id.replace(
                                    /Items_\d+__/g,
                                    "Items_" +
                                    index +
                                    "__"
                                );
                        }
                    );


                    // =========================================
                    // LABEL FOR
                    // =========================================

                    row.querySelectorAll(
                        "label[for]"
                    ).forEach(
                        function (label) {

                            label.htmlFor =
                                label.htmlFor.replace(
                                    /Items_\d+__/g,
                                    "Items_" +
                                    index +
                                    "__"
                                );
                        }
                    );


                    // =========================================
                    // VALIDATION TARGETS
                    // =========================================

                    row.querySelectorAll(
                        "[data-valmsg-for]"
                    ).forEach(
                        function (element) {

                            const current =
                                element.getAttribute(
                                    "data-valmsg-for"
                                );


                            if (!current) {
                                return;
                            }


                            element.setAttribute(
                                "data-valmsg-for",
                                current.replace(
                                    /Items\[\d+\]/g,
                                    "Items[" +
                                    index +
                                    "]"
                                )
                            );
                        }
                    );
                }
            );


            initializeItemSelects(
                container
            );


            refreshDuplicateItemOptions();

            updateEmptyState();
        }


        // =====================================================
        // VALIDATION REPARSE
        // =====================================================

        function reparseValidation() {

            if (
                !window.jQuery ||
                !jQuery.validator ||
                !jQuery.validator.unobtrusive
            ) {

                return;
            }


            jQuery(form)
                .removeData(
                    "validator"
                )
                .removeData(
                    "unobtrusiveValidation"
                );


            jQuery.validator
                .unobtrusive
                .parse(
                    form
                );
        }


        // =====================================================
        // LOAD ITEM MASTER DATA
        // =====================================================

        async function loadItemData(
            row,
            itemId) {

            const itemDataUrl =
                container.getAttribute(
                    "data-item-data-url"
                );


            const codeDisplay =
                row.querySelector(
                    ".item-code-display"
                );

            const unitDisplay =
                row.querySelector(
                    ".item-unit-display"
                );

            const specificationDisplay =
                row.querySelector(
                    ".item-specification-display"
                );


            const codeHidden =
                row.querySelector(
                    ".item-code-hidden"
                );

            const nameHidden =
                row.querySelector(
                    ".item-name-hidden"
                );

            const specificationHidden =
                row.querySelector(
                    ".item-specification-hidden"
                );

            const unitHidden =
                row.querySelector(
                    ".item-unit-hidden"
                );


            const drawingStatus =
                row.querySelector(
                    ".item-drawing-status"
                );

            const drawingNumber =
                row.querySelector(
                    ".item-drawing-number"
                );

            const drawingRevision =
                row.querySelector(
                    ".item-drawing-revision"
                );

            const drawingName =
                row.querySelector(
                    ".item-drawing-name"
                );

            const drawingType =
                row.querySelector(
                    ".item-drawing-type"
                );


            function clearItemInformation() {

                if (codeDisplay) {
                    codeDisplay.value = "";
                }

                if (unitDisplay) {
                    unitDisplay.value = "";
                }

                if (specificationDisplay) {
                    specificationDisplay.value = "";
                }


                if (codeHidden) {
                    codeHidden.value = "";
                }

                if (nameHidden) {
                    nameHidden.value = "";
                }

                if (specificationHidden) {
                    specificationHidden.value = "";
                }

                if (unitHidden) {
                    unitHidden.value = "";
                }


                if (drawingStatus) {
                    drawingStatus.textContent =
                        "No Drawing";
                }

                if (drawingNumber) {
                    drawingNumber.textContent =
                        "-";
                }

                if (drawingRevision) {
                    drawingRevision.textContent =
                        "-";
                }

                if (drawingName) {
                    drawingName.textContent =
                        "-";
                }

                if (drawingType) {
                    drawingType.textContent =
                        "-";
                }


                updateRowSummary(
                    row,
                    "",
                    ""
                );
            }


            if (!itemId) {

                clearItemInformation();

                return;
            }


            if (!itemDataUrl) {

                showError(
                    "Unable to load Item information."
                );

                return;
            }


            try {

                const parameters =
                    new URLSearchParams();


                parameters.set(
                    "itemId",
                    itemId
                );


                const response =
                    await fetch(
                        itemDataUrl +
                        "?" +
                        parameters.toString(),
                        {
                            cache:
                                "no-store"
                        }
                    );


                if (!response.ok) {

                    throw new Error(
                        "Unable to load Item."
                    );
                }


                const data =
                    await response.json();


                if (!data.success) {

                    clearItemInformation();


                    showError(
                        data.message ||
                        "Unable to load Item."
                    );


                    return;
                }


                // =============================================
                // DISPLAY VALUES
                // =============================================

                if (codeDisplay) {

                    codeDisplay.value =
                        data.itemCode ||
                        "";
                }


                if (unitDisplay) {

                    unitDisplay.value =
                        data.unitName ||
                        "";
                }


                if (specificationDisplay) {

                    specificationDisplay.value =
                        data.specification ||
                        "";
                }


                // =============================================
                // HIDDEN POST VALUES
                // =============================================

                if (codeHidden) {

                    codeHidden.value =
                        data.itemCode ||
                        "";
                }


                if (nameHidden) {

                    nameHidden.value =
                        data.itemName ||
                        "";
                }


                if (specificationHidden) {

                    specificationHidden.value =
                        data.specification ||
                        "";
                }


                if (unitHidden) {

                    unitHidden.value =
                        data.unitName ||
                        "";
                }


                // =============================================
                // CURRENT DRAWING
                // =============================================

                const hasDrawing =
                    Boolean(
                        data.drawingId &&
                        data.drawingNumber
                    );


                if (drawingStatus) {

                    drawingStatus.textContent =
                        hasDrawing
                            ? "Current"
                            : "No Drawing";
                }


                if (drawingNumber) {

                    drawingNumber.textContent =
                        data.drawingNumber ||
                        "-";
                }


                if (drawingRevision) {

                    drawingRevision.textContent =
                        data.drawingRevision ||
                        "-";
                }


                if (drawingName) {

                    drawingName.textContent =
                        data.drawingName ||
                        "-";
                }


                if (drawingType) {

                    drawingType.textContent =
                        data.drawingType ||
                        "-";
                }


                // =============================================
                // SUMMARY
                // =============================================

                updateRowSummary(
                    row,
                    data.itemCode ||
                    "",
                    data.itemName ||
                    ""
                );

            }
            catch (error) {

                clearItemInformation();


                showError(
                    "Unable to load Item information."
                );
            }
        }


        // =====================================================
        // HANDLE ITEM SELECTION
        // =====================================================

        function handleItemSelection(
            itemSelect) {

            if (!itemSelect) {
                return;
            }


            const row =
                itemSelect.closest(
                    ".customer-po-item-row"
                );


            if (!row) {
                return;
            }


            if (
                isDuplicateItemSelection(
                    itemSelect
                )
            ) {

                const selectedOption =
                    itemSelect.options[
                    itemSelect.selectedIndex
                    ];


                const selectedText =
                    selectedOption
                        ?.textContent
                        ?.trim()
                    ||
                    "Selected Item";


                if (hasSelect2()) {

                    jQuery(itemSelect)
                        .val(null)
                        .trigger(
                            "change.select2"
                        );
                }
                else {

                    itemSelect.value =
                        "";
                }


                loadItemData(
                    row,
                    ""
                );


                refreshDuplicateItemOptions();


                showError(
                    selectedText +
                    " is already selected in this Customer PO."
                );


                return;
            }


            refreshDuplicateItemOptions();


            loadItemData(
                row,
                itemSelect.value
            );
        }


        // =====================================================
        // ADD ITEM ROW
        // =====================================================

        function addItemRow() {

            const rows =
                container.querySelectorAll(
                    ".customer-po-item-row"
                );


            const index =
                rows.length;


            const html =
                template.innerHTML
                    .replaceAll(
                        "__index__",
                        index
                    )
                    .replaceAll(
                        "__number__",
                        index + 1
                    );


            container.insertAdjacentHTML(
                "beforeend",
                html
            );


            reindexRows();

            reparseValidation();


            const updatedRows =
                container.querySelectorAll(
                    ".customer-po-item-row"
                );


            const newRow =
                updatedRows[
                updatedRows.length - 1
                ];


            collapseAllExcept(
                newRow
            );


            if (newRow) {

                /*
                 * Do not automatically open the Item dropdown.
                 *
                 * User will open the searchable dropdown manually.
                 * This also prevents Select2 from opening upward
                 * automatically during initial Create page load.
                 */

                newRow.scrollIntoView({

                    behavior:
                        "smooth",

                    block:
                        "nearest"
                });
            }
        }


        // =====================================================
        // ADD BUTTON
        // =====================================================

        addButton.addEventListener(
            "click",
            addItemRow
        );


        // =====================================================
        // ROW CLICK EVENTS
        // =====================================================

        container.addEventListener(
            "click",
            function (event) {

                const toggleButton =
                    event.target.closest(
                        ".item-toggle-button"
                    );


                if (toggleButton) {

                    const row =
                        toggleButton.closest(
                            ".customer-po-item-row"
                        );


                    if (!row) {
                        return;
                    }


                    const body =
                        row.querySelector(
                            ".item-row-body"
                        );


                    if (
                        body &&
                        body.classList.contains(
                            "d-none"
                        )
                    ) {

                        collapseAllExcept(
                            row
                        );
                    }
                    else {

                        collapseRow(
                            row
                        );
                    }


                    return;
                }


                const removeButton =
                    event.target.closest(
                        ".btn-remove-item"
                    );


                if (!removeButton) {
                    return;
                }


                const row =
                    removeButton.closest(
                        ".customer-po-item-row"
                    );


                if (!row) {
                    return;
                }


                destroyItemSelects(
                    row
                );


                row.remove();


                reindexRows();

                reparseValidation();


                const remainingRows =
                    container.querySelectorAll(
                        ".customer-po-item-row"
                    );


                if (remainingRows.length > 0) {

                    collapseAllExcept(
                        remainingRows[
                        remainingRows.length - 1
                        ]
                    );
                }
            }
        );


        // =====================================================
        // NATIVE ITEM CHANGE
        // =====================================================

        container.addEventListener(
            "change",
            function (event) {

                if (
                    !event.target.classList
                        .contains(
                            "item-select"
                        )
                ) {

                    return;
                }


                /*
                 * Native dropdown fallback.
                 *
                 * When Select2 is active its own select2:select /
                 * select2:clear events already handle the change.
                 */
                if (
                    hasSelect2() &&
                    event.target.classList
                        .contains(
                            "select2-hidden-accessible"
                        )
                ) {

                    return;
                }


                handleItemSelection(
                    event.target
                );
            }
        );


        // =====================================================
        // INITIALIZE EXISTING ROWS
        // =====================================================

        let initialRows =
            container.querySelectorAll(
                ".customer-po-item-row"
            );


        if (initialRows.length === 0) {

            addItemRow();

            return;
        }


        reindexRows();


        initialRows =
            container.querySelectorAll(
                ".customer-po-item-row"
            );


        collapseAllExcept(
            initialRows[
            initialRows.length - 1
            ]
        );


        updateEmptyState();
    }


    // =========================================================
    // CUSTOMER PO NUMBER SIMILARITY
    // =========================================================

    function initializeCustomerPoNumberCheck(
        form) {

        const customerSelect =
            document.getElementById(
                "CustomerId"
            );

        const poNumberInput =
            document.getElementById(
                "CustomerPurchaseOrderNumber"
            );

        const warning =
            document.getElementById(
                "similarCustomerPoWarning"
            );

        const list =
            document.getElementById(
                "similarCustomerPoList"
            );

        const exactMessage =
            document.getElementById(
                "customerPoExactDuplicateMessage"
            );

        const confirmationContainer =
            document.getElementById(
                "customerPoSimilarConfirmationContainer"
            );

        const confirmation =
            document.getElementById(
                "ConfirmSimilarCustomerPoNumber"
            );


        if (!customerSelect ||
            !poNumberInput ||
            !warning ||
            !list) {

            return;
        }


        function clearSuggestions() {

            list.innerHTML =
                "";


            warning.classList.add(
                "d-none"
            );


            if (exactMessage) {

                exactMessage.classList.add(
                    "d-none"
                );
            }


            if (confirmationContainer) {

                confirmationContainer
                    .classList
                    .add(
                        "d-none"
                    );
            }


            if (confirmation) {

                confirmation.checked =
                    false;
            }


            poNumberInput.setCustomValidity(
                ""
            );


            refreshSaveButtons();
        }


        function refreshSaveButtons() {

            const warningVisible =
                !warning.classList
                    .contains(
                        "d-none"
                    );


            const exactExists =
                exactMessage &&
                !exactMessage.classList
                    .contains(
                        "d-none"
                    );


            const confirmationRequired =
                warningVisible &&
                !exactExists &&
                confirmationContainer &&
                !confirmationContainer
                    .classList
                    .contains(
                        "d-none"
                    );


            const blockSave =
                Boolean(
                    exactExists
                )
                ||
                Boolean(
                    confirmationRequired &&
                    confirmation &&
                    !confirmation.checked
                );


            form
                .querySelectorAll(
                    "button[type='submit'], " +
                    "input[type='submit']"
                )
                .forEach(
                    function (button) {

                        button.disabled =
                            blockSave;
                    }
                );
        }


        function renderSuggestions(
            result) {

            const orders =
                result.orders ||
                [];


            if (
                !result.hasSimilarOrders ||
                orders.length === 0
            ) {

                clearSuggestions();

                return;
            }


            list.innerHTML =
                "";


            orders.forEach(
                function (order) {

                    const li =
                        document.createElement(
                            "li"
                        );


                    li.textContent =
                        order;


                    list.appendChild(
                        li
                    );
                }
            );


            warning.classList.remove(
                "d-none"
            );


            const hasExactMatch =
                Boolean(
                    result.hasExactMatch
                );


            if (exactMessage) {

                exactMessage
                    .classList
                    .toggle(
                        "d-none",
                        !hasExactMatch
                    );
            }


            /*
             * Exact duplicate:
             * - blocked
             * - no confirmation option
             *
             * Similar:
             * - user reviews list
             * - confirmation required
             */

            if (confirmationContainer) {

                confirmationContainer
                    .classList
                    .toggle(
                        "d-none",
                        hasExactMatch
                    );
            }


            if (confirmation) {

                confirmation.checked =
                    false;
            }


            poNumberInput
                .setCustomValidity(
                    hasExactMatch
                        ? "Customer PO Number already exists for the selected Customer."
                        : ""
                );


            refreshSaveButtons();
        }


        async function loadSuggestions() {

            const customerId =
                String(
                    customerSelect.value ||
                    ""
                ).trim();


            const poNumber =
                poNumberInput
                    .value
                    .trim();


            const url =
                poNumberInput.getAttribute(
                    "data-similar-url"
                );


            if (
                !customerId ||
                poNumber.length < 3 ||
                !url
            ) {

                clearSuggestions();

                return;
            }


            if (customerPoNumberRequest) {

                customerPoNumberRequest
                    .abort();
            }


            customerPoNumberRequest =
                new AbortController();


            const parameters =
                new URLSearchParams();


            parameters.set(
                "customerId",
                customerId
            );


            parameters.set(
                "customerPurchaseOrderNumber",
                poNumber
            );


            const excludeId =
                poNumberInput.getAttribute(
                    "data-exclude-id"
                );


            if (
                excludeId &&
                Number(excludeId) > 0
            ) {

                parameters.set(
                    "excludeId",
                    excludeId
                );
            }


            try {

                const response =
                    await fetch(
                        url +
                        "?" +
                        parameters.toString(),
                        {
                            cache:
                                "no-store",

                            signal:
                                customerPoNumberRequest
                                    .signal
                        }
                    );


                if (!response.ok) {

                    clearSuggestions();

                    return;
                }


                const result =
                    await response.json();


                renderSuggestions(
                    result
                );

            }
            catch (error) {

                if (
                    error.name !==
                    "AbortError"
                ) {

                    clearSuggestions();
                }
            }
        }


        function queueCheck() {

            clearTimeout(
                customerPoNumberTimer
            );


            const poNumber =
                poNumberInput
                    .value
                    .trim();


            if (
                !customerSelect.value ||
                poNumber.length < 3
            ) {

                clearSuggestions();

                return;
            }


            if (confirmation) {

                confirmation.checked =
                    false;
            }


            refreshSaveButtons();


            customerPoNumberTimer =
                setTimeout(
                    loadSuggestions,
                    450
                );
        }


        poNumberInput.addEventListener(
            "input",
            queueCheck
        );


        customerSelect.addEventListener(
            "change",
            function () {

                clearSuggestions();

                queueCheck();
            }
        );


        if (confirmation) {

            confirmation.addEventListener(
                "change",
                refreshSaveButtons
            );
        }


        /*
         * Edit / validation-return page.
         */
        if (
            customerSelect.value &&
            poNumberInput.value.trim()
                .length >= 3
        ) {

            queueCheck();
        }
        else {

            refreshSaveButtons();
        }
    }


    // =========================================================
    // NOTIFICATION HELPERS
    // =========================================================

    function showError(
        message) {

        if (window.showAppToast) {

            window.showAppToast(
                "error",
                message
            );

            return;
        }


        if (window.toastr) {

            toastr.error(
                message
            );

            return;
        }


        alert(
            message
        );
    }

})();