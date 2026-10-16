// ============================================================
// File: inventory-form.js
// Module: Inventory / Stock Management
//
// Purpose:
// Handles client-side UI behaviour for Opening Stock.
//
// Responsibilities:
// - Add Opening Stock item row.
// - Remove item row.
// - Re-index rows for ASP.NET Core model binding.
// - Update row numbers.
// - Calculate display Amount = Quantity x Rate.
// - Detect duplicate Item + Warehouse combinations.
// - Maintain item count / empty state.
//
// Important:
// - This JavaScript is only for UI convenience.
// - Final validation remains in InventoryService.
// - Amount calculated here is display-only.
// - Authoritative Amount is recalculated in InventoryService.
// ============================================================

document.addEventListener(
    "DOMContentLoaded",
    function () {

        // =====================================================
        // DOM REFERENCES
        // =====================================================

        const form =
            document.getElementById(
                "inventoryForm");


        const addButton =
            document.getElementById(
                "addOpeningStockRow");


        const itemsBody =
            document.getElementById(
                "openingStockItemsBody");


        const rowTemplate =
            document.getElementById(
                "openingStockRowTemplate");


        const emptyState =
            document.getElementById(
                "openingStockEmptyState");


        const tableContainer =
            document.getElementById(
                "openingStockTableContainer");


        const itemCount =
            document.getElementById(
                "openingStockItemCount");


        if (!form ||
            !addButton ||
            !itemsBody ||
            !rowTemplate ||
            !emptyState ||
            !tableContainer ||
            !itemCount) {

            return;
        }


        // =====================================================
        // ADD ITEM ROW
        // =====================================================

        addButton.addEventListener(
            "click",
            function () {

                addRow();
            });


        // =====================================================
        // TABLE EVENTS
        // =====================================================
        //
        // Event delegation is used because rows are added
        // dynamically after page load.
        // =====================================================

        itemsBody.addEventListener(
            "click",
            function (event) {

                const removeButton =
                    event.target.closest(
                        ".remove-opening-stock-row");


                if (!removeButton) {
                    return;
                }


                const row =
                    removeButton.closest(
                        ".opening-stock-row");


                if (!row) {
                    return;
                }


                row.remove();


                reindexRows();

                updateItemState();

                validateDuplicateCombinations();
            });


        itemsBody.addEventListener(
            "input",
            function (event) {

                if (!event.target.matches(
                    ".quantity-input, .rate-input")) {

                    return;
                }


                const row =
                    event.target.closest(
                        ".opening-stock-row");


                if (!row) {
                    return;
                }


                calculateRowAmount(
                    row);
            });


        itemsBody.addEventListener(
            "change",
            function (event) {

                if (!event.target.matches(
                    ".item-select, .warehouse-select")) {

                    return;
                }


                validateDuplicateCombinations();
            });


        // =====================================================
        // FORM SUBMIT
        // =====================================================

        form.addEventListener(
            "submit",
            function (event) {

                /*
                 * Always normalize collection indexes before
                 * ASP.NET Core model binding.
                 */

                reindexRows();


                /*
                 * Client-side duplicate validation is only a
                 * convenience.
                 *
                 * InventoryService performs the authoritative
                 * duplicate validation again.
                 */

                const hasDuplicates =
                    validateDuplicateCombinations();


                if (hasDuplicates) {

                    event.preventDefault();


                    const firstInvalid =
                        form.querySelector(
                            ".item-select:invalid, " +
                            ".warehouse-select:invalid");


                    if (firstInvalid) {

                        firstInvalid.reportValidity();

                        firstInvalid.focus();
                    }
                }
            });


        // =====================================================
        // ADD ROW
        // =====================================================

        function addRow() {

            /*
             * Existing rows may have changed after removal,
             * therefore normalize first.
             */

            reindexRows();


            const rows =
                getRows();


            const index =
                rows.length;


            const row =
                rowTemplate
                    .content
                    .firstElementChild
                    .cloneNode(true);


            row.dataset.index =
                index.toString();


            // =================================================
            // MODEL BINDING INDEX
            // =================================================

            row
                .querySelectorAll(
                    "[name]")
                .forEach(function (element) {

                    element.name =
                        element.name.replaceAll(
                            "__index__",
                            index.toString());
                });


            // =================================================
            // ROW NUMBER
            // =================================================

            const rowNumber =
                row.querySelector(
                    ".row-number");


            if (rowNumber) {

                rowNumber.textContent =
                    (index + 1).toString();
            }


            itemsBody.appendChild(
                row);


            updateItemState();


            const itemSelect =
                row.querySelector(
                    ".item-select");


            if (itemSelect) {

                itemSelect.focus();
            }
        }


        // =====================================================
        // RE-INDEX ROWS
        // =====================================================
        //
        // Example:
        //
        // Before Remove:
        // Items[0]
        // Items[1]
        // Items[2]
        //
        // After removing Items[1]:
        //
        // Items[0]
        // Items[1]
        //
        // This keeps ASP.NET Core collection model binding
        // predictable.
        // =====================================================

        function reindexRows() {

            const rows =
                getRows();


            rows.forEach(
                function (
                    row,
                    index) {

                    row.dataset.index =
                        index.toString();


                    const rowNumber =
                        row.querySelector(
                            ".row-number");


                    if (rowNumber) {

                        rowNumber.textContent =
                            (index + 1).toString();
                    }


                    row
                        .querySelectorAll(
                            "[name]")
                        .forEach(
                            function (element) {

                                element.name =
                                    element.name.replace(
                                        /Items\[\d+\]/,
                                        `Items[${index}]`);
                            });
                });
        }


        // =====================================================
        // CALCULATE ROW AMOUNT
        // =====================================================

        function calculateRowAmount(
            row) {

            const quantityInput =
                row.querySelector(
                    ".quantity-input");


            const rateInput =
                row.querySelector(
                    ".rate-input");


            const amountDisplay =
                row.querySelector(
                    ".amount-display");


            if (!quantityInput ||
                !rateInput ||
                !amountDisplay) {

                return;
            }


            /*
             * Rate is optional.
             *
             * When Rate is empty, Amount display must also
             * remain empty.
             */

            if (!rateInput.value.trim()) {

                amountDisplay.value =
                    "";

                return;
            }


            const quantity =
                toNumber(
                    quantityInput.value);


            const rate =
                toNumber(
                    rateInput.value);


            if (!Number.isFinite(quantity) ||
                !Number.isFinite(rate)) {

                amountDisplay.value =
                    "";

                return;
            }


            const amount =
                quantity * rate;


            amountDisplay.value =
                amount.toFixed(2);
        }


        // =====================================================
        // DUPLICATE ITEM + WAREHOUSE VALIDATION
        // =====================================================

        function validateDuplicateCombinations() {

            const rows =
                getRows();


            /*
             * Clear previous duplicate validation first.
             */

            rows.forEach(
                function (row) {

                    const itemSelect =
                        row.querySelector(
                            ".item-select");


                    const warehouseSelect =
                        row.querySelector(
                            ".warehouse-select");


                    if (itemSelect) {

                        itemSelect.setCustomValidity(
                            "");
                    }


                    if (warehouseSelect) {

                        warehouseSelect.setCustomValidity(
                            "");
                    }
                });


            const combinations =
                new Map();


            let hasDuplicates =
                false;


            rows.forEach(
                function (row) {

                    const itemSelect =
                        row.querySelector(
                            ".item-select");


                    const warehouseSelect =
                        row.querySelector(
                            ".warehouse-select");


                    if (!itemSelect ||
                        !warehouseSelect) {

                        return;
                    }


                    const itemId =
                        itemSelect.value.trim();


                    const warehouseId =
                        warehouseSelect.value.trim();


                    /*
                     * Do not perform duplicate validation until
                     * both Item and Warehouse are selected.
                     */

                    if (!itemId ||
                        !warehouseId) {

                        return;
                    }


                    const key =
                        `${itemId}|${warehouseId}`;


                    if (!combinations.has(
                        key)) {

                        combinations.set(
                            key,
                            {
                                itemSelect:
                                    itemSelect,

                                warehouseSelect:
                                    warehouseSelect
                            });


                        return;
                    }


                    hasDuplicates =
                        true;


                    const firstRow =
                        combinations.get(
                            key);


                    const message =
                        "The same Item and Warehouse combination " +
                        "cannot be added more than once.";


                    /*
                     * Mark both duplicate rows invalid so the
                     * user can clearly identify the conflict.
                     */

                    firstRow.itemSelect
                        .setCustomValidity(
                            message);


                    firstRow.warehouseSelect
                        .setCustomValidity(
                            message);


                    itemSelect.setCustomValidity(
                        message);


                    warehouseSelect.setCustomValidity(
                        message);
                });


            return hasDuplicates;
        }


        // =====================================================
        // EMPTY STATE / ITEM COUNT
        // =====================================================

        function updateItemState() {

            const rows =
                getRows();


            const count =
                rows.length;


            itemCount.textContent =
                `${count} ` +
                `${count === 1
                    ? "Item"
                    : "Items"}`;


            if (count === 0) {

                emptyState.classList.remove(
                    "d-none");


                tableContainer.classList.add(
                    "d-none");


                return;
            }


            emptyState.classList.add(
                "d-none");


            tableContainer.classList.remove(
                "d-none");
        }


        // =====================================================
        // GET CURRENT ROWS
        // =====================================================

        function getRows() {

            return Array.from(
                itemsBody.querySelectorAll(
                    ".opening-stock-row"));
        }


        // =====================================================
        // NUMBER HELPER
        // =====================================================

        function toNumber(
            value) {

            const number =
                parseFloat(
                    value);


            return Number.isFinite(
                number)
                ? number
                : 0;
        }


        // =====================================================
        // INITIAL PAGE LOAD
        // =====================================================
        //
        // Important for:
        // - ModelState return after server validation failure.
        // - Existing rows rendered again by Razor.
        // =====================================================

        const initialRows =
            getRows();


        initialRows.forEach(
            function (row) {

                calculateRowAmount(
                    row);
            });


        reindexRows();

        updateItemState();

        validateDuplicateCombinations();
    });