/*
============================================================
File: pre-dispatch-inspection-form.js

Purpose:
Handles dynamic behaviour of the PDI Create / Edit form.

Responsibilities:
- Load Production Job source information.
- Auto-fill Customer / PO / Item information.
- Auto-fill Workshop Drawing.
- Auto-fill Customer Drawing.
- Auto-load Item Specifications.
- Create 7 Observation inputs per Inspection Line.
- Create 3 Interval Reading inputs per Inspection Line.
- Add manual Inspection Parameter rows.
- Remove Inspection Parameter rows.
- Re-index MVC collection field names.
- Refresh unobtrusive validation after dynamic changes.

Important:
- Production Job source data comes from trusted backend.
- Customer / Item / Drawing values are display-only.
- Edit mode keeps Production Job locked.
============================================================
*/

document.addEventListener(
    "DOMContentLoaded",
    function () {

        // =====================================================
        // REGION — DOM REFERENCES
        // =====================================================

        const productionJobSelect =
            document.getElementById(
                "ProductionJobId");

        const inspectionLinesContainer =
            document.getElementById(
                "pdiInspectionLinesContainer");

        const inspectionLineTemplate =
            document.getElementById(
                "pdiInspectionLineTemplate");

        const btnAddInspectionLine =
            document.getElementById(
                "btnAddInspectionLine");

        const noInspectionLinesMessage =
            document.getElementById(
                "pdiNoInspectionLinesMessage");


        if (
            !inspectionLinesContainer ||
            !inspectionLineTemplate ||
            !btnAddInspectionLine
        ) {
            return;
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — INITIALIZE
        // =====================================================

        initializeExistingRows();

        updateEmptyMessage();

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — PRODUCTION JOB CHANGE
        // =====================================================

        if (productionJobSelect) {

            productionJobSelect.addEventListener(
                "change",
                async function () {

                    const productionJobId =
                        parseInt(
                            productionJobSelect.value);

                    if (
                        !productionJobId ||
                        productionJobId <= 0
                    ) {
                        clearSourceInformation();

                        clearInspectionLines();

                        resetQuantityInformation();

                        return;
                    }


                    await loadProductionJobData(
                        productionJobId);
                });
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — ADD INSPECTION LINE
        // =====================================================

        btnAddInspectionLine.addEventListener(
            "click",
            function () {

                addInspectionLine(
                    null);

                updateEmptyMessage();
            });

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — PRODUCTION JOB DATA
        // =====================================================

        async function loadProductionJobData(
            productionJobId) {

            setProductionJobLoadingState(
                true);


            try {

                const url =
                    `/PreDispatchInspection/GetProductionJobData` +
                    `?productionJobId=${encodeURIComponent(
                        productionJobId)}`;


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


                let data =
                    null;


                try {

                    data =
                        await response.json();

                }
                catch {

                    data =
                        null;
                }


                if (!response.ok) {

                    const message =
                        data?.message ||
                        "Unable to load Production Job information.";


                    throw new Error(
                        message);
                }


                applySourceInformation(
                    data);

                applyQuantityInformation(
                    data);

                loadInspectionLines(
                    data.lines || []);

            }
            catch (error) {

                clearSourceInformation();

                clearInspectionLines();

                resetQuantityInformation();


                showError(
                    error?.message ||
                    "Unable to load Production Job information.");
            }
            finally {

                setProductionJobLoadingState(
                    false);
            }
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — APPLY SOURCE INFORMATION
        // =====================================================

        function applySourceInformation(
            data) {

            setText(
                "displayItemName",
                data.itemName);

            setText(
                "displayPartNumber",
                data.partNumber);

            setText(
                "displayItemCode",
                data.itemCode);

            setText(
                "displayCustomerName",
                data.customerName);

            setText(
                "displayCustomerPoNumber",
                data.customerPurchaseOrderNumber);

            setText(
                "displayCustomerItemCode",
                data.customerItemCode);

            setText(
                "displayWorkshopDrawingNumber",
                data.workshopDrawingNumber);

            setText(
                "displayWorkshopDrawingRevision",
                data.workshopDrawingRevision);

            setText(
                "displayCustomerDrawingNumber",
                data.customerDrawingNumber);

            setText(
                "displayCustomerDrawingRevision",
                data.customerDrawingRevision);
        }


        function clearSourceInformation() {

            setText(
                "displayItemName",
                null);

            setText(
                "displayPartNumber",
                null);

            setText(
                "displayItemCode",
                null);

            setText(
                "displayCustomerName",
                null);

            setText(
                "displayCustomerPoNumber",
                null);

            setText(
                "displayCustomerItemCode",
                null);

            setText(
                "displayWorkshopDrawingNumber",
                null);

            setText(
                "displayWorkshopDrawingRevision",
                null);

            setText(
                "displayCustomerDrawingNumber",
                null);

            setText(
                "displayCustomerDrawingRevision",
                null);
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


            const normalizedValue =
                value === null ||
                    value === undefined ||
                    String(value).trim() === ""
                    ? "-"
                    : String(value);


            element.textContent =
                normalizedValue;
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — QUANTITY INFORMATION
        // =====================================================

        function applyQuantityInformation(
            data) {

            setText(
                "displayJobQuantity",
                formatQuantity(
                    data.jobQuantity));

            setText(
                "displayUnitName",
                data.unitName || "");

            setText(
                "displayRemainingInspectionQuantity",
                formatQuantity(
                    data.remainingInspectionQuantity));


            const inspectionQuantityInput =
                document.getElementById(
                    "InspectionQuantity");


            if (inspectionQuantityInput) {

                inspectionQuantityInput.value =
                    normalizeNumberForInput(
                        data.inspectionQuantity ??
                        data.remainingInspectionQuantity);
            }


            const acceptedQuantityInput =
                document.getElementById(
                    "AcceptedQuantity");

            const reworkQuantityInput =
                document.getElementById(
                    "ReworkQuantity");

            const rejectedQuantityInput =
                document.getElementById(
                    "RejectedQuantity");


            if (acceptedQuantityInput) {

                acceptedQuantityInput.value =
                    "0";
            }


            if (reworkQuantityInput) {

                reworkQuantityInput.value =
                    "0";
            }


            if (rejectedQuantityInput) {

                rejectedQuantityInput.value =
                    "0";
            }
        }


        function resetQuantityInformation() {

            setText(
                "displayJobQuantity",
                "0");

            setText(
                "displayRemainingInspectionQuantity",
                "0");

            setText(
                "displayUnitName",
                "");


            const inspectionQuantityInput =
                document.getElementById(
                    "InspectionQuantity");


            if (inspectionQuantityInput) {

                inspectionQuantityInput.value =
                    "";
            }
        }


        function formatQuantity(
            value) {

            const number =
                Number(value);


            if (!Number.isFinite(number)) {
                return "0";
            }


            return number
                .toLocaleString(
                    undefined,
                    {
                        maximumFractionDigits:
                            3
                    });
        }


        function normalizeNumberForInput(
            value) {

            const number =
                Number(value);


            if (!Number.isFinite(number)) {
                return "";
            }


            return String(number);
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — LOAD INSPECTION LINES
        // =====================================================

        function loadInspectionLines(
            lines) {

            clearInspectionLines();


            if (
                !Array.isArray(lines) ||
                lines.length === 0
            ) {
                updateEmptyMessage();

                return;
            }


            lines
                .sort(
                    function (a, b) {

                        return (
                            Number(
                                a.sequenceNumber) -
                            Number(
                                b.sequenceNumber)
                        );
                    })
                .forEach(
                    function (line) {

                        addInspectionLine(
                            line);
                    });


            reindexInspectionLines();

            refreshValidation();

            updateEmptyMessage();
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — ADD LINE
        // =====================================================

        function addInspectionLine(
            lineData) {

            const currentRows =
                getInspectionRows();


            const lineIndex =
                currentRows.length;

            const sequenceNumber =
                lineIndex + 1;


            let html =
                inspectionLineTemplate
                    .innerHTML;


            html =
                html
                    .replaceAll(
                        "__lineIndex__",
                        String(
                            lineIndex))
                    .replaceAll(
                        "__sequenceNumber__",
                        String(
                            sequenceNumber))
                    .replaceAll(
                        "__parameter__",
                        "")
                    .replaceAll(
                        "__specification__",
                        "")
                    .replaceAll(
                        "__inspectionMethod__",
                        "");


            inspectionLinesContainer
                .insertAdjacentHTML(
                    "beforeend",
                    html);


            const rows =
                getInspectionRows();


            const row =
                rows[
                rows.length - 1];


            initializeRow(
                row);


            if (lineData) {

                populateLineRow(
                    row,
                    lineData);
            }


            reindexInspectionLines();

            refreshValidation();

            updateEmptyMessage();


            return row;
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — POPULATE LINE
        // =====================================================

        function populateLineRow(
            row,
            lineData) {

            setRowValue(
                row,
                ".Parameter",
                lineData.parameter);

            setRowValue(
                row,
                ".Specification",
                lineData.specification);

            setRowValue(
                row,
                ".InspectionMethod",
                lineData.inspectionMethod);


            const resultSelect =
                findInputEndingWith(
                    row,
                    ".Result");


            if (resultSelect) {

                resultSelect.value =
                    String(
                        lineData.result || 1);
            }


            setRowValue(
                row,
                ".Remarks",
                lineData.remarks);


            const observations =
                Array.isArray(
                    lineData.observations)
                    ? lineData.observations
                    : [];


            observations.forEach(
                function (observation) {

                    const input =
                        findObservationValueInput(
                            row,
                            Number(
                                observation.sequenceNumber),
                            Boolean(
                                observation.isIntervalReading));


                    if (input) {

                        input.value =
                            observation.value ??
                            "";
                    }
                });
        }


        function setRowValue(
            row,
            nameEnding,
            value) {

            const input =
                findInputEndingWith(
                    row,
                    nameEnding);


            if (!input) {
                return;
            }


            input.value =
                value ??
                "";
        }


        function findInputEndingWith(
            row,
            nameEnding) {

            const controls =
                row.querySelectorAll(
                    "input[name], select[name], textarea[name]");


            return Array
                .from(
                    controls)
                .find(
                    function (control) {

                        return control
                            .name
                            .endsWith(
                                nameEnding);
                    })
                || null;
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — OBSERVATION LOOKUP
        // =====================================================

        function findObservationValueInput(
            row,
            sequenceNumber,
            isIntervalReading) {

            const valueInputs =
                Array.from(
                    row.querySelectorAll(
                        'input[name$=".Value"]'));


            for (const valueInput
                of valueInputs) {

                const observationPrefix =
                    valueInput.name
                        .substring(
                            0,
                            valueInput.name.length -
                            ".Value".length);


                const sequenceInput =
                    row.querySelector(
                        `[name="${cssEscape(
                            observationPrefix +
                            ".SequenceNumber")}"]`);


                const intervalInput =
                    row.querySelector(
                        `[name="${cssEscape(
                            observationPrefix +
                            ".IsIntervalReading")}"]`);


                if (
                    !sequenceInput ||
                    !intervalInput
                ) {
                    continue;
                }


                const actualSequenceNumber =
                    Number(
                        sequenceInput.value);


                const actualIsIntervalReading =
                    String(
                        intervalInput.value)
                        .toLowerCase() ===
                    "true";


                if (
                    actualSequenceNumber ===
                    sequenceNumber &&
                    actualIsIntervalReading ===
                    isIntervalReading
                ) {
                    return valueInput;
                }
            }


            return null;
        }


        function cssEscape(
            value) {

            if (
                window.CSS &&
                typeof window.CSS.escape ===
                "function"
            ) {
                return window.CSS.escape(
                    value);
            }


            return value
                .replace(
                    /\\/g,
                    "\\\\")
                .replace(
                    /"/g,
                    '\\"');
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — REMOVE LINE
        // =====================================================

        function initializeRow(
            row) {

            const removeButton =
                row.querySelector(
                    ".btn-remove-inspection-line");


            if (!removeButton) {
                return;
            }


            removeButton.addEventListener(
                "click",
                function () {

                    row.remove();

                    reindexInspectionLines();

                    refreshValidation();

                    updateEmptyMessage();
                });
        }


        function initializeExistingRows() {

            getInspectionRows()
                .forEach(
                    function (row) {

                        initializeRow(
                            row);
                    });


            reindexInspectionLines();
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — REINDEX MVC COLLECTION
        // =====================================================

        function reindexInspectionLines() {

            const rows =
                getInspectionRows();


            rows.forEach(
                function (
                    row,
                    lineIndex) {

                    const sequenceNumber =
                        lineIndex + 1;


                    const sequenceDisplay =
                        row.querySelector(
                            ".line-sequence-display");


                    if (sequenceDisplay) {

                        sequenceDisplay.textContent =
                            String(
                                sequenceNumber);
                    }


                    const sequenceInput =
                        row.querySelector(
                            ".line-sequence-number");


                    if (sequenceInput) {

                        sequenceInput.value =
                            String(
                                sequenceNumber);
                    }


                    const controls =
                        row.querySelectorAll(
                            "input[name], select[name], textarea[name]");


                    controls.forEach(
                        function (control) {

                            if (
                                !control.name ||
                                !control.name.startsWith(
                                    "Lines[")
                            ) {
                                return;
                            }


                            control.name =
                                control.name.replace(
                                    /^Lines\[\d+\]/,
                                    `Lines[${lineIndex}]`);


                            if (control.id) {

                                control.id =
                                    control.id.replace(
                                        /^Lines_\d+__/,
                                        `Lines_${lineIndex}__`);
                            }
                        });
                });
        }


        function getInspectionRows() {

            return Array.from(
                inspectionLinesContainer
                    .querySelectorAll(
                        ".pdi-inspection-line"));
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — CLEAR LINES
        // =====================================================

        function clearInspectionLines() {

            inspectionLinesContainer
                .innerHTML =
                "";


            updateEmptyMessage();
        }


        function updateEmptyMessage() {

            if (!noInspectionLinesMessage) {
                return;
            }


            const hasRows =
                getInspectionRows()
                    .length > 0;


            noInspectionLinesMessage
                .classList
                .toggle(
                    "d-none",
                    hasRows);
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — LOADING STATE
        // =====================================================

        function setProductionJobLoadingState(
            isLoading) {

            if (!productionJobSelect) {
                return;
            }


            productionJobSelect.disabled =
                isLoading;


            btnAddInspectionLine.disabled =
                isLoading;
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — VALIDATION REFRESH
        // =====================================================

        function refreshValidation() {

            if (
                typeof window.jQuery ===
                "undefined" ||
                typeof window.jQuery.validator ===
                "undefined" ||
                typeof window.jQuery.validator.unobtrusive ===
                "undefined"
            ) {
                return;
            }


            const form =
                document.getElementById(
                    "preDispatchInspectionForm");


            if (!form) {
                return;
            }


            const $form =
                window.jQuery(
                    form);


            $form
                .removeData(
                    "validator");


            $form
                .removeData(
                    "unobtrusiveValidation");


            window
                .jQuery
                .validator
                .unobtrusive
                .parse(
                    $form);
        }

        // =====================================================
        // END REGION
        // =====================================================


        // =====================================================
        // REGION — ERROR DISPLAY
        // =====================================================

        function showError(
            message) {

            /*
             * Shared TempData toast is used for normal
             * Controller navigation.
             *
             * AJAX errors need immediate feedback.
             * If a shared toast helper exists, use it.
             */

            if (
                typeof window.showToast ===
                "function"
            ) {
                window.showToast(
                    "error",
                    message);

                return;
            }


            window.alert(
                message);
        }

        // =====================================================
        // END REGION
        // =====================================================
    });