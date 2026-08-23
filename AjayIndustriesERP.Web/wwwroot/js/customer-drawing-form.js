/*
==============================================================

File : customer-drawing-form.js

Purpose :
Customer Drawing Master client-side behaviour.

Features :
- Select2 Customer
- Select2 Item
- Customer + Item duplicate live check
- Drawing Number live similarity check
- Drawing Number similarity scoped to Customer
- Exact Drawing Number blocks Create
- Drawing Name live similarity warning
- Drawing Name similarity scoped to Customer
- Dynamic Add Revision rows
- Remove new Revision rows
- Drawing Number uppercase
- File-name preview

Final Rules :
- Customer Drawing follows Drawing Master workflow.
- First Revision is system generated as RV-01.
- New Revision Numbers are system generated.
- Existing revisions are never overwritten.
- Customer + Item duplicate blocks Create.
- Exact Drawing Number within same Customer blocks Create.

==============================================================
*/

(function () {
    "use strict";


    const validationState = {
        hasCustomerItemDuplicate: false,
        hasExactDrawingNumber: false
    };


    let duplicateRequest = null;
    let numberRequest = null;
    let nameRequest = null;

    let numberTimer = null;
    let nameTimer = null;


    document.addEventListener(
        "DOMContentLoaded",
        initializeCustomerDrawingForm
    );


    function initializeCustomerDrawingForm() {

        initializeSearchableSelects();

        initializeUppercaseField(
            document.getElementById(
                "DrawingNumber"
            )
        );

        initializeFirstRevisionFile();

        initializeCustomerItemCheck();

        initializeDrawingNumberCheck();

        initializeDrawingNameCheck();

        initializeRevisionGrid();

        initializeInitialSaveState();
    }


    // =========================================================
    // SEARCHABLE DROPDOWNS
    // =========================================================

    function initializeSearchableSelects() {

        if (!window.jQuery ||
            !jQuery.fn.select2) {

            return;
        }


        initializeSelect2(
            ".js-customer-drawing-customer-select",
            "-- Select Customer --"
        );


        initializeSelect2(
            ".js-customer-drawing-item-select",
            "-- Select Item --"
        );
    }


    function initializeSelect2(
        selector,
        placeholder) {

        const $select =
            jQuery(
                selector
            );


        if ($select.length === 0) {
            return;
        }


        $select.each(
            function () {

                const $current =
                    jQuery(
                        this
                    );


                if ($current.hasClass(
                    "select2-hidden-accessible"
                )) {

                    return;
                }


                $current.select2({

                    width:
                        "100%",

                    placeholder:
                        placeholder,

                    allowClear:
                        true,

                    minimumResultsForSearch:
                        0
                });
            }
        );
    }


    // =========================================================
    // CUSTOMER + ITEM DUPLICATE CHECK
    // =========================================================

    function initializeCustomerItemCheck() {

        const customerSelect =
            document.querySelector(
                ".js-customer-drawing-customer-select"
            );

        const itemSelect =
            document.querySelector(
                ".js-customer-drawing-item-select"
            );

        const warning =
            document.getElementById(
                "customerDrawingDuplicateWarning"
            );

        const drawingNumber =
            document.getElementById(
                "existingCustomerDrawingNumber"
            );

        const drawingLink =
            document.getElementById(
                "existingCustomerDrawingLink"
            );


        /*
         * Duplicate check is required only during Create.
         *
         * During Edit Customer and Item are permanent
         * hidden fields, so these Selects do not exist.
         */
        if (!customerSelect ||
            !itemSelect ||
            !warning) {

            return;
        }


        bindChange(
            customerSelect,
            "customerItem",
            function () {

                checkCustomerItemDuplicate();

                /*
                 * Similarity belongs to selected Customer.
                 *
                 * If Customer changes, refresh existing
                 * Drawing Number / Name suggestions.
                 */
                triggerDrawingNumberCheck();

                triggerDrawingNameCheck();
            }
        );


        bindChange(
            itemSelect,
            "customerItem",
            function () {

                checkCustomerItemDuplicate();
            }
        );


        if (customerSelect.value &&
            itemSelect.value) {

            checkCustomerItemDuplicate();
        }


        async function checkCustomerItemDuplicate() {

            const customerId =
                String(
                    customerSelect.value ||
                    ""
                ).trim();

            const itemId =
                String(
                    itemSelect.value ||
                    ""
                ).trim();


            if (!customerId ||
                !itemId) {

                clearDuplicateWarning();

                return;
            }


            const url =
                customerSelect.getAttribute(
                    "data-duplicate-url"
                );


            if (!url) {

                clearDuplicateWarning();

                return;
            }


            if (duplicateRequest) {

                duplicateRequest.abort();
            }


            duplicateRequest =
                new AbortController();


            const parameters =
                new URLSearchParams();


            parameters.set(
                "customerId",
                customerId
            );


            parameters.set(
                "itemId",
                itemId
            );


            try {

                const response =
                    await fetch(
                        url +
                        "?" +
                        parameters.toString(),
                        {
                            method:
                                "GET",

                            cache:
                                "no-store",

                            signal:
                                duplicateRequest.signal,

                            headers: {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        }
                    );


                if (!response.ok) {

                    clearDuplicateWarning();

                    return;
                }


                const result =
                    await response.json();


                if (result.success &&
                    result.exists) {

                    showDuplicateWarning(
                        result
                    );

                    return;
                }


                clearDuplicateWarning();

            } catch (error) {

                if (error.name !==
                    "AbortError") {

                    clearDuplicateWarning();
                }
            }
        }


        function showDuplicateWarning(
            result) {

            validationState
                .hasCustomerItemDuplicate =
                true;


            warning.classList.remove(
                "d-none"
            );


            if (drawingNumber) {

                drawingNumber.textContent =
                    result.drawingNumber ||
                    "-";
            }


            if (drawingLink &&
                result.customerDrawingId) {

                drawingLink.href =
                    "/CustomerDrawing/Details/" +
                    encodeURIComponent(
                        result.customerDrawingId
                    );
            }


            refreshSaveButton();
        }


        function clearDuplicateWarning() {

            validationState
                .hasCustomerItemDuplicate =
                false;


            warning.classList.add(
                "d-none"
            );


            if (drawingNumber) {

                drawingNumber.textContent =
                    "";
            }


            if (drawingLink) {

                drawingLink.href =
                    "#";
            }


            refreshSaveButton();
        }
    }


    // =========================================================
    // DRAWING NUMBER LIVE CHECK
    // =========================================================

    function initializeDrawingNumberCheck() {

        const input =
            document.getElementById(
                "DrawingNumber"
            );

        const warning =
            document.getElementById(
                "customerDrawingNumberWarning"
            );

        const list =
            document.getElementById(
                "customerDrawingNumberSuggestionList"
            );

        const exactMessage =
            document.getElementById(
                "customerDrawingNumberExactMessage"
            );


        /*
         * Number warning exists only during Create.
         */
        if (!input ||
            !warning ||
            !list ||
            !exactMessage) {

            return;
        }


        validationState.hasExactDrawingNumber =
            !exactMessage.classList
                .contains(
                    "d-none"
                );


        input.addEventListener(
            "input",
            function () {

                initializeUppercaseValue(
                    input
                );

                scheduleNumberCheck();
            }
        );


        /*
         * Create page may open with pre-populated values.
         */
        if (input.value.trim().length >= 2 &&
            getCustomerId()) {

            scheduleNumberCheck(
                0
            );
        }


        window.customerDrawingRunNumberCheck =
            scheduleNumberCheck;


        function scheduleNumberCheck(
            delay = 450) {

            clearTimeout(
                numberTimer
            );


            const value =
                input.value.trim();


            if (!getCustomerId() ||
                value.length < 2) {

                clearNumberSuggestions();

                return;
            }


            numberTimer =
                setTimeout(
                    function () {

                        loadNumberSuggestions(
                            value
                        );

                    },
                    delay
                );
        }


        async function loadNumberSuggestions(
            drawingNumber) {

            const url =
                input.getAttribute(
                    "data-number-similar-url"
                );

            const customerId =
                getCustomerId();


            if (!url ||
                !customerId) {

                clearNumberSuggestions();

                return;
            }


            if (numberRequest) {

                numberRequest.abort();
            }


            numberRequest =
                new AbortController();


            const parameters =
                new URLSearchParams();


            parameters.set(
                "customerId",
                customerId
            );


            parameters.set(
                "drawingNumber",
                drawingNumber
            );


            const customerDrawingId =
                getCustomerDrawingId();


            if (customerDrawingId) {

                parameters.set(
                    "customerDrawingId",
                    customerDrawingId
                );
            }


            try {

                const response =
                    await fetch(
                        url +
                        "?" +
                        parameters.toString(),
                        {
                            method:
                                "GET",

                            cache:
                                "no-store",

                            signal:
                                numberRequest.signal,

                            headers: {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        }
                    );


                if (!response.ok) {

                    clearNumberSuggestions();

                    return;
                }


                const result =
                    await response.json();


                renderNumberSuggestions(
                    result.records ||
                    []
                );

            } catch (error) {

                if (error.name !==
                    "AbortError") {

                    clearNumberSuggestions();
                }
            }
        }


        function renderNumberSuggestions(
            records) {

            list.innerHTML =
                "";


            if (!records ||
                records.length === 0) {

                clearNumberSuggestions();

                return;
            }


            let hasExact =
                false;


            records.forEach(
                function (record) {

                    if (record.isExactMatch) {

                        hasExact =
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
                        "align-items-center " +
                        "gap-2";


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
                            ? "Already Exists"
                            : "Similar";


                    row.appendChild(
                        text
                    );

                    row.appendChild(
                        badge
                    );

                    item.appendChild(
                        row
                    );

                    list.appendChild(
                        item
                    );
                }
            );


            warning.classList.remove(
                "d-none"
            );


            exactMessage.classList.toggle(
                "d-none",
                !hasExact
            );


            validationState
                .hasExactDrawingNumber =
                hasExact;


            refreshSaveButton();
        }


        function clearNumberSuggestions() {

            list.innerHTML =
                "";


            warning.classList.add(
                "d-none"
            );


            exactMessage.classList.add(
                "d-none"
            );


            validationState
                .hasExactDrawingNumber =
                false;


            refreshSaveButton();
        }
    }


    // =========================================================
    // DRAWING NAME LIVE CHECK
    // =========================================================

    function initializeDrawingNameCheck() {

        const input =
            document.getElementById(
                "DrawingName"
            );

        const warning =
            document.getElementById(
                "customerDrawingNameWarning"
            );

        const list =
            document.getElementById(
                "customerDrawingNameSuggestionList"
            );


        if (!input ||
            !warning ||
            !list) {

            return;
        }


        input.addEventListener(
            "input",
            function () {

                scheduleNameCheck();
            }
        );


        if (input.value.trim().length >= 3 &&
            getCustomerId()) {

            scheduleNameCheck(
                0
            );
        }


        window.customerDrawingRunNameCheck =
            scheduleNameCheck;


        function scheduleNameCheck(
            delay = 450) {

            clearTimeout(
                nameTimer
            );


            const value =
                input.value.trim();


            if (!getCustomerId() ||
                value.length < 3) {

                clearNameSuggestions();

                return;
            }


            nameTimer =
                setTimeout(
                    function () {

                        loadNameSuggestions(
                            value
                        );

                    },
                    delay
                );
        }


        async function loadNameSuggestions(
            drawingName) {

            const url =
                input.getAttribute(
                    "data-name-similar-url"
                );

            const customerId =
                getCustomerId();


            if (!url ||
                !customerId) {

                clearNameSuggestions();

                return;
            }


            if (nameRequest) {

                nameRequest.abort();
            }


            nameRequest =
                new AbortController();


            const parameters =
                new URLSearchParams();


            parameters.set(
                "customerId",
                customerId
            );


            parameters.set(
                "drawingName",
                drawingName
            );


            const customerDrawingId =
                getCustomerDrawingId();


            if (customerDrawingId) {

                parameters.set(
                    "customerDrawingId",
                    customerDrawingId
                );
            }


            try {

                const response =
                    await fetch(
                        url +
                        "?" +
                        parameters.toString(),
                        {
                            method:
                                "GET",

                            cache:
                                "no-store",

                            signal:
                                nameRequest.signal,

                            headers: {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        }
                    );


                if (!response.ok) {

                    clearNameSuggestions();

                    return;
                }


                const result =
                    await response.json();


                renderNameSuggestions(
                    result.records ||
                    []
                );

            } catch (error) {

                if (error.name !==
                    "AbortError") {

                    clearNameSuggestions();
                }
            }
        }


        function renderNameSuggestions(
            records) {

            list.innerHTML =
                "";


            if (!records ||
                records.length === 0) {

                clearNameSuggestions();

                return;
            }


            records.forEach(
                function (record) {

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
                        "align-items-center " +
                        "gap-2";


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
                            ? "badge bg-warning text-dark"
                            : "badge bg-secondary";


                    badge.textContent =
                        record.isExactMatch
                            ? "Same Name"
                            : "Similar";


                    row.appendChild(
                        text
                    );

                    row.appendChild(
                        badge
                    );

                    item.appendChild(
                        row
                    );

                    list.appendChild(
                        item
                    );
                }
            );


            warning.classList.remove(
                "d-none"
            );
        }


        function clearNameSuggestions() {

            list.innerHTML =
                "";


            warning.classList.add(
                "d-none"
            );
        }
    }


    // =========================================================
    // CUSTOMER CHANGE -> REFRESH SIMILARITY
    // =========================================================

    function triggerDrawingNumberCheck() {

        if (typeof window
            .customerDrawingRunNumberCheck ===
            "function") {

            window
                .customerDrawingRunNumberCheck(
                    0
                );
        }
    }


    function triggerDrawingNameCheck() {

        if (typeof window
            .customerDrawingRunNameCheck ===
            "function") {

            window
                .customerDrawingRunNameCheck(
                    0
                );
        }
    }


    // =========================================================
    // REVISION GRID
    // =========================================================

    function initializeRevisionGrid() {

        const addButton =
            document.getElementById(
                "addCustomerDrawingRevisionButton"
            );

        const section =
            document.getElementById(
                "newCustomerDrawingRevisionsSection"
            );

        const container =
            document.getElementById(
                "newCustomerDrawingRevisionsContainer"
            );

        const template =
            document.getElementById(
                "customerDrawingRevisionRowTemplate"
            );


        /*
         * Create page does not contain Revision Grid.
         */
        if (!addButton ||
            !section ||
            !container ||
            !template) {

            initializeExistingRevisionRows();

            return;
        }


        let counter =
            Date.now();


        addButton.addEventListener(
            "click",
            function () {

                counter++;


                const key =
                    "new_" +
                    counter;


                const html =
                    template.innerHTML
                        .replaceAll(
                            "__key__",
                            key
                        );


                const wrapper =
                    document.createElement(
                        "div"
                    );


                wrapper.innerHTML =
                    html.trim();


                const row =
                    wrapper
                        .firstElementChild;


                if (!row) {
                    return;
                }


                container.appendChild(
                    row
                );


                section.classList.remove(
                    "d-none"
                );


                initializeRevisionRow(
                    row
                );


                const fileInput =
                    row.querySelector(
                        ".js-customer-drawing-revision-file"
                    );


                if (fileInput) {

                    fileInput.focus();
                }
            }
        );


        initializeExistingRevisionRows();


        function initializeExistingRevisionRows() {

            document
                .querySelectorAll(
                    "[data-customer-drawing-revision-row]"
                )
                .forEach(
                    function (row) {

                        initializeRevisionRow(
                            row
                        );
                    }
                );
        }


        function initializeRevisionRow(
            row) {

            if (!row ||
                row.dataset.initialized ===
                "true") {

                return;
            }


            row.dataset.initialized =
                "true";


            const fileInput =
                row.querySelector(
                    ".js-customer-drawing-revision-file"
                );

            const fileName =
                row.querySelector(
                    ".js-customer-drawing-revision-file-name"
                );

            const removeButton =
                row.querySelector(
                    ".js-remove-customer-drawing-revision"
                );


            if (fileInput) {

                fileInput.addEventListener(
                    "change",
                    function () {

                        if (!fileName) {
                            return;
                        }


                        if (!fileInput.files ||
                            fileInput.files.length === 0) {

                            fileName.textContent =
                                "";

                            return;
                        }


                        fileName.textContent =
                            "Selected: " +
                            fileInput
                                .files[0]
                                .name;
                    }
                );
            }


            if (removeButton) {

                removeButton.addEventListener(
                    "click",
                    function () {

                        row.remove();


                        if (container &&
                            container
                                .querySelectorAll(
                                    "[data-customer-drawing-revision-row]"
                                )
                                .length === 0 &&
                            section) {

                            section.classList.add(
                                "d-none"
                            );
                        }
                    }
                );
            }
        }
    }


    // =========================================================
    // CREATE - FIRST REVISION FILE
    // =========================================================

    function initializeFirstRevisionFile() {

        const fileInput =
            document.getElementById(
                "DrawingFile"
            );

        const fileName =
            document.getElementById(
                "customerDrawingSelectedFileName"
            );


        if (!fileInput ||
            !fileName) {

            return;
        }


        fileInput.addEventListener(
            "change",
            function () {

                if (!fileInput.files ||
                    fileInput.files.length === 0) {

                    fileName.textContent =
                        "";

                    return;
                }


                fileName.textContent =
                    "Selected: " +
                    fileInput
                        .files[0]
                        .name;
            }
        );
    }


    // =========================================================
    // SAVE BUTTON STATE
    // =========================================================

    function initializeInitialSaveState() {

        const duplicateWarning =
            document.getElementById(
                "customerDrawingDuplicateWarning"
            );

        const exactMessage =
            document.getElementById(
                "customerDrawingNumberExactMessage"
            );


        if (duplicateWarning) {

            validationState
                .hasCustomerItemDuplicate =
                !duplicateWarning
                    .classList
                    .contains(
                        "d-none"
                    );
        }


        if (exactMessage) {

            validationState
                .hasExactDrawingNumber =
                !exactMessage
                    .classList
                    .contains(
                        "d-none"
                    );
        }


        refreshSaveButton();
    }


    function refreshSaveButton() {

        const saveButton =
            document.getElementById(
                "customerDrawingSaveButton"
            );


        if (!saveButton) {
            return;
        }


        saveButton.disabled =
            validationState
                .hasCustomerItemDuplicate
            ||
            validationState
                .hasExactDrawingNumber;
    }


    // =========================================================
    // ID HELPERS
    // =========================================================

    function getCustomerId() {

        const customer =
            document.getElementById(
                "CustomerId"
            );


        if (!customer) {
            return "";
        }


        return String(
            customer.value ||
            ""
        ).trim();
    }


    function getCustomerDrawingId() {

        const drawing =
            document.getElementById(
                "CustomerDrawingId"
            );


        if (!drawing) {
            return "";
        }


        const value =
            String(
                drawing.value ||
                ""
            ).trim();


        if (!value ||
            Number(value) <= 0) {

            return "";
        }


        return value;
    }


    // =========================================================
    // CHANGE EVENT HELPER
    // =========================================================

    function bindChange(
        element,
        namespace,
        handler) {

        if (!element ||
            typeof handler !==
            "function") {

            return;
        }


        if (window.jQuery) {

            jQuery(
                element
            )
                .off(
                    "change." +
                    namespace
                )
                .on(
                    "change." +
                    namespace,
                    handler
                );


            return;
        }


        element.addEventListener(
            "change",
            handler
        );
    }


    // =========================================================
    // UPPERCASE HELPERS
    // =========================================================

    function initializeUppercaseField(
        input) {

        if (!input) {
            return;
        }


        input.addEventListener(
            "input",
            function () {

                initializeUppercaseValue(
                    input
                );
            }
        );
    }


    function initializeUppercaseValue(
        input) {

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

})();