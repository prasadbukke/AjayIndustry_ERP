/*
==============================================================

File : drawing-form.js

Purpose :
Drawing Master client-side behaviour.

Features :
- Select2 Item
- Drawing Number live similarity check
- Exact Drawing Number blocks Create
- Drawing Name live similarity warning
- Dynamic Add Revision rows
- Remove new Revision rows
- Revision uppercase
- File-name preview

==============================================================
*/

(function () {
    "use strict";

    document.addEventListener(
        "DOMContentLoaded",
        initializeDrawingForm
    );

    function initializeDrawingForm() {

        initializeItemSelect();

        initializeUppercaseField(
            document.getElementById(
                "DrawingNumber"
            )
        );

        initializeUppercaseField(
            document.getElementById(
                "RevisionNumber"
            )
        );

        initializeFirstRevisionFile();

        initializeDrawingNumberCheck();

        initializeDrawingNameCheck();

        initializeRevisionGrid();
    }

    // =========================================================
    // ITEM SELECT2
    // =========================================================

    function initializeItemSelect() {

        if (!window.jQuery ||
            !jQuery.fn.select2) {

            return;
        }

        const $select =
            jQuery(
                ".js-drawing-item-select"
            );

        if ($select.length === 0) {
            return;
        }

        $select.select2({
            width: "100%",
            placeholder: "-- Select Item --",
            allowClear: true
        });
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
                "drawingNumberWarning"
            );

        const list =
            document.getElementById(
                "drawingNumberSuggestionList"
            );

        const exactMessage =
            document.getElementById(
                "drawingNumberExactMessage"
            );

        const saveButton =
            document.getElementById(
                "drawingSaveButton"
            );

        if (!input ||
            !warning ||
            !list ||
            !exactMessage ||
            !saveButton) {

            return;
        }

        const url =
            input.getAttribute(
                "data-number-similar-url"
            );

        let timer = null;
        let activeRequest = null;

        input.addEventListener(
            "input",
            function () {

                initializeUppercaseValue(
                    input
                );

                clearTimeout(timer);

                const value =
                    input.value.trim();

                if (value.length < 2) {

                    clearNumberSuggestions();

                    return;
                }

                timer =
                    setTimeout(
                        function () {

                            loadNumberSuggestions(
                                value
                            );

                        },
                        450
                    );
            }
        );

        refreshSaveButton();

        async function loadNumberSuggestions(
            drawingNumber) {

            if (!url) {
                return;
            }

            if (activeRequest) {
                activeRequest.abort();
            }

            activeRequest =
                new AbortController();

            try {

                const response =
                    await fetch(
                        url +
                        "?drawingNumber=" +
                        encodeURIComponent(
                            drawingNumber
                        ),
                        {
                            method: "GET",
                            cache: "no-store",
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

                renderNumberSuggestions(
                    result.records || []
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

            list.innerHTML = "";

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
                        hasExact = true;
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
                        "d-flex justify-content-between " +
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
                            ? "Already Exists"
                            : "Similar";

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
                !hasExact
            );

            refreshSaveButton();
        }

        function clearNumberSuggestions() {

            list.innerHTML = "";

            warning.classList.add(
                "d-none"
            );

            exactMessage.classList.add(
                "d-none"
            );

            refreshSaveButton();
        }

        function refreshSaveButton() {

            const hasExact =
                !exactMessage.classList
                    .contains("d-none");

            saveButton.disabled =
                hasExact;
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
                "drawingNameWarning"
            );

        const list =
            document.getElementById(
                "drawingNameSuggestionList"
            );

        if (!input ||
            !warning ||
            !list) {

            return;
        }

        const url =
            input.getAttribute(
                "data-name-similar-url"
            );

        const drawingId =
            document.getElementById(
                "DrawingId"
            );

        let timer = null;
        let activeRequest = null;

        input.addEventListener(
            "input",
            function () {

                clearTimeout(timer);

                const value =
                    input.value.trim();

                if (value.length < 3) {

                    clearNameSuggestions();

                    return;
                }

                timer =
                    setTimeout(
                        function () {

                            loadNameSuggestions(
                                value
                            );

                        },
                        450
                    );
            }
        );

        async function loadNameSuggestions(
            drawingName) {

            if (!url) {
                return;
            }

            if (activeRequest) {
                activeRequest.abort();
            }

            activeRequest =
                new AbortController();

            let requestUrl =
                url +
                "?drawingName=" +
                encodeURIComponent(
                    drawingName
                );

            if (drawingId &&
                drawingId.value) {

                requestUrl +=
                    "&drawingId=" +
                    encodeURIComponent(
                        drawingId.value
                    );
            }

            try {

                const response =
                    await fetch(
                        requestUrl,
                        {
                            method: "GET",
                            cache: "no-store",
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

                renderNameSuggestions(
                    result.records || []
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

            list.innerHTML = "";

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
                        "d-flex justify-content-between " +
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
                            ? "badge bg-warning text-dark"
                            : "badge bg-secondary";

                    badge.textContent =
                        record.isExactMatch
                            ? "Same Name"
                            : "Similar";

                    row.appendChild(text);
                    row.appendChild(badge);

                    item.appendChild(row);

                    list.appendChild(item);
                }
            );

            warning.classList.remove(
                "d-none"
            );
        }

        function clearNameSuggestions() {

            list.innerHTML = "";

            warning.classList.add(
                "d-none"
            );
        }
    }

    // =========================================================
    // REVISION GRID
    // =========================================================

    function initializeRevisionGrid() {

        const addButton =
            document.getElementById(
                "addDrawingRevisionButton"
            );

        const section =
            document.getElementById(
                "newDrawingRevisionsSection"
            );

        const container =
            document.getElementById(
                "newDrawingRevisionsContainer"
            );

        const template =
            document.getElementById(
                "drawingRevisionRowTemplate"
            );

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
                    "new_" + counter;

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
                    wrapper.firstElementChild;

                container.appendChild(
                    row
                );

                section.classList.remove(
                    "d-none"
                );

                initializeRevisionRow(
                    row
                );

                const revisionInput =
                    row.querySelector(
                        ".js-revision-number"
                    );

                if (revisionInput) {
                    revisionInput.focus();
                }
            }
        );

        initializeExistingRevisionRows();

        function initializeExistingRevisionRows() {

            document
                .querySelectorAll(
                    "[data-revision-row]"
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

            const revisionInput =
                row.querySelector(
                    ".js-revision-number"
                );

            const fileInput =
                row.querySelector(
                    ".js-revision-file"
                );

            const fileName =
                row.querySelector(
                    ".js-revision-file-name"
                );

            const removeButton =
                row.querySelector(
                    ".js-remove-revision"
                );

            initializeUppercaseField(
                revisionInput
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
                            container.querySelectorAll(
                                "[data-revision-row]"
                            ).length === 0 &&
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
    // CREATE FILE
    // =========================================================

    function initializeFirstRevisionFile() {

        const fileInput =
            document.getElementById(
                "DrawingFile"
            );

        const fileName =
            document.getElementById(
                "drawingSelectedFileName"
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

                    fileName.textContent = "";

                    return;
                }

                fileName.textContent =
                    "Selected: " +
                    fileInput.files[0].name;
            }
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
            input.value.toUpperCase();

        if (start !== null &&
            end !== null) {

            input.setSelectionRange(
                start,
                end
            );
        }
    }

})();