/*
=============================================================
File: item-customer-po-tracking.js
Module: Item Customer PO Tracking
Layer: Web - JavaScript

Purpose:
Handles autocomplete behaviour for:

1. Item Name / Item Code
2. Customer PO Number

Features:
- Starts searching after 2 characters.
- Maximum results are controlled by backend.
- Displays suggestions below input.
- Selecting Item stores exact ItemId.
- Selecting PO stores exact CustomerPurchaseOrderId.
- Manual typing is also supported.
- Changing selected text clears previously selected hidden Id.
- Suggestions close when clicking outside.
- Escape key closes suggestions.
- No inline JavaScript required.
=============================================================
*/

document.addEventListener("DOMContentLoaded", function () {

    initializeItemAutocomplete();

    initializePurchaseOrderAutocomplete();

});


/* =========================================================
   ITEM AUTOCOMPLETE
   ========================================================= */

function initializeItemAutocomplete() {

    const input =
        document.getElementById(
            "itemCustomerPOTrackingItemSearch"
        );

    const hiddenId =
        document.getElementById(
            "itemCustomerPOTrackingItemId"
        );

    const suggestionBox =
        document.getElementById(
            "itemCustomerPOTrackingItemSuggestions"
        );


    if (!input ||
        !hiddenId ||
        !suggestionBox) {

        return;
    }


    let debounceTimer = null;

    let abortController = null;

    let selectedText =
        input.value.trim();


    /* =====================================================
       INPUT EVENT
       ===================================================== */

    input.addEventListener(
        "input",
        function () {

            const searchText =
                input.value.trim();


            /*
             * User changed the text after selecting
             * an autocomplete value.
             *
             * Clear exact ItemId so manual text
             * search will work correctly.
             */

            if (searchText !== selectedText) {

                hiddenId.value = "";

            }


            clearTimeout(
                debounceTimer
            );


            if (searchText.length < 2) {

                hideSuggestionBox(
                    suggestionBox
                );

                return;
            }


            debounceTimer =
                setTimeout(
                    function () {

                        loadItemSuggestions(
                            searchText,
                            input,
                            hiddenId,
                            suggestionBox,
                            function (text) {

                                selectedText =
                                    text;

                            },
                            abortController,
                            function (controller) {

                                abortController =
                                    controller;

                            }
                        );

                    },
                    250
                );

        }
    );


    /* =====================================================
       FOCUS EVENT
       ===================================================== */

    input.addEventListener(
        "focus",
        function () {

            const searchText =
                input.value.trim();


            if (searchText.length < 2) {

                return;
            }


            loadItemSuggestions(
                searchText,
                input,
                hiddenId,
                suggestionBox,
                function (text) {

                    selectedText =
                        text;

                },
                abortController,
                function (controller) {

                    abortController =
                        controller;

                }
            );

        }
    );


    /* =====================================================
       KEYBOARD
       ===================================================== */

    input.addEventListener(
        "keydown",
        function (event) {

            if (event.key === "Escape") {

                hideSuggestionBox(
                    suggestionBox
                );

            }

        }
    );


    /* =====================================================
       CLICK OUTSIDE
       ===================================================== */

    document.addEventListener(
        "click",
        function (event) {

            if (!input.contains(event.target) &&
                !suggestionBox.contains(event.target)) {

                hideSuggestionBox(
                    suggestionBox
                );

            }

        }
    );
}


/* =========================================================
   LOAD ITEM SUGGESTIONS
   ========================================================= */

async function loadItemSuggestions(
    searchText,
    input,
    hiddenId,
    suggestionBox,
    setSelectedText,
    currentAbortController,
    setAbortController) {

    try {

        /* =================================================
           CANCEL PREVIOUS REQUEST
           ================================================= */

        if (currentAbortController) {

            currentAbortController.abort();

        }


        const controller =
            new AbortController();


        setAbortController(
            controller
        );


        /* =================================================
           REQUEST
           ================================================= */

        const url =
            "/ItemCustomerPOTracking/SearchItems"
            + "?term="
            + encodeURIComponent(
                searchText
            );


        const response =
            await fetch(
                url,
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    signal:
                        controller.signal
                }
            );


        if (!response.ok) {

            hideSuggestionBox(
                suggestionBox
            );

            return;
        }


        const items =
            await response.json();


        /* =================================================
           STALE RESPONSE SAFETY
           ================================================= */

        if (input.value.trim() !==
            searchText) {

            return;
        }


        /* =================================================
           CLEAR OLD RESULTS
           ================================================= */

        suggestionBox.innerHTML = "";


        if (!Array.isArray(items) ||
            items.length === 0) {

            renderNoResults(
                suggestionBox,
                "No matching items found."
            );

            return;
        }


        /* =================================================
           BUILD ITEM RESULTS
           ================================================= */

        items.forEach(
            function (item) {

                const button =
                    document.createElement(
                        "button"
                    );


                button.type =
                    "button";


                button.className =
                    "list-group-item "
                    + "list-group-item-action "
                    + "text-start";


                /* =========================================
                   ITEM NAME
                   ========================================= */

                const nameElement =
                    document.createElement(
                        "div"
                    );


                nameElement.className =
                    "fw-semibold";


                nameElement.textContent =
                    item.itemName
                    || item.text
                    || "";


                button.appendChild(
                    nameElement
                );


                /* =========================================
                   ITEM CODE
                   ========================================= */

                if (item.itemCode) {

                    const codeElement =
                        document.createElement(
                            "div"
                        );


                    codeElement.className =
                        "small text-muted";


                    codeElement.textContent =
                        "Item Code: "
                        + item.itemCode;


                    button.appendChild(
                        codeElement
                    );

                }


                /* =========================================
                   SELECT ITEM
                   ========================================= */

                button.addEventListener(
                    "click",
                    function () {

                        /*
                         * Keep Item Name in textbox.
                         *
                         * Exact ItemId is stored separately.
                         */

                        const selectedValue =
                            item.itemName
                            || item.text
                            || "";


                        input.value =
                            selectedValue;


                        hiddenId.value =
                            item.id;


                        setSelectedText(
                            selectedValue
                        );


                        hideSuggestionBox(
                            suggestionBox
                        );


                        input.focus();

                    }
                );


                suggestionBox.appendChild(
                    button
                );

            }
        );


        showSuggestionBox(
            suggestionBox
        );

    }
    catch (error) {

        if (error.name ===
            "AbortError") {

            return;
        }


        console.error(
            "Item autocomplete failed:",
            error
        );


        hideSuggestionBox(
            suggestionBox
        );

    }
}


/* =========================================================
   CUSTOMER PO AUTOCOMPLETE
   ========================================================= */

function initializePurchaseOrderAutocomplete() {

    const input =
        document.getElementById(
            "itemCustomerPOTrackingPOSearch"
        );

    const hiddenId =
        document.getElementById(
            "itemCustomerPOTrackingPOId"
        );

    const suggestionBox =
        document.getElementById(
            "itemCustomerPOTrackingPOSuggestions"
        );


    if (!input ||
        !hiddenId ||
        !suggestionBox) {

        return;
    }


    let debounceTimer = null;

    let abortController = null;

    let selectedText =
        input.value.trim();


    /* =====================================================
       INPUT EVENT
       ===================================================== */

    input.addEventListener(
        "input",
        function () {

            const searchText =
                input.value.trim();


            /*
             * If user modifies previously selected
             * PO number, remove exact PO Id.
             */

            if (searchText !== selectedText) {

                hiddenId.value = "";

            }


            clearTimeout(
                debounceTimer
            );


            if (searchText.length < 2) {

                hideSuggestionBox(
                    suggestionBox
                );

                return;
            }


            debounceTimer =
                setTimeout(
                    function () {

                        loadPurchaseOrderSuggestions(
                            searchText,
                            input,
                            hiddenId,
                            suggestionBox,
                            function (text) {

                                selectedText =
                                    text;

                            },
                            abortController,
                            function (controller) {

                                abortController =
                                    controller;

                            }
                        );

                    },
                    250
                );

        }
    );


    /* =====================================================
       FOCUS EVENT
       ===================================================== */

    input.addEventListener(
        "focus",
        function () {

            const searchText =
                input.value.trim();


            if (searchText.length < 2) {

                return;
            }


            loadPurchaseOrderSuggestions(
                searchText,
                input,
                hiddenId,
                suggestionBox,
                function (text) {

                    selectedText =
                        text;

                },
                abortController,
                function (controller) {

                    abortController =
                        controller;

                }
            );

        }
    );


    /* =====================================================
       KEYBOARD
       ===================================================== */

    input.addEventListener(
        "keydown",
        function (event) {

            if (event.key === "Escape") {

                hideSuggestionBox(
                    suggestionBox
                );

            }

        }
    );


    /* =====================================================
       CLICK OUTSIDE
       ===================================================== */

    document.addEventListener(
        "click",
        function (event) {

            if (!input.contains(event.target) &&
                !suggestionBox.contains(event.target)) {

                hideSuggestionBox(
                    suggestionBox
                );

            }

        }
    );
}


/* =========================================================
   LOAD CUSTOMER PO SUGGESTIONS
   ========================================================= */

async function loadPurchaseOrderSuggestions(
    searchText,
    input,
    hiddenId,
    suggestionBox,
    setSelectedText,
    currentAbortController,
    setAbortController) {

    try {

        /* =================================================
           CANCEL PREVIOUS REQUEST
           ================================================= */

        if (currentAbortController) {

            currentAbortController.abort();

        }


        const controller =
            new AbortController();


        setAbortController(
            controller
        );


        /* =================================================
           REQUEST
           ================================================= */

        const url =
            "/ItemCustomerPOTracking/SearchPurchaseOrders"
            + "?term="
            + encodeURIComponent(
                searchText
            );


        const response =
            await fetch(
                url,
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    signal:
                        controller.signal
                }
            );


        if (!response.ok) {

            hideSuggestionBox(
                suggestionBox
            );

            return;
        }


        const purchaseOrders =
            await response.json();


        /* =================================================
           STALE RESPONSE SAFETY
           ================================================= */

        if (input.value.trim() !==
            searchText) {

            return;
        }


        /* =================================================
           CLEAR OLD RESULTS
           ================================================= */

        suggestionBox.innerHTML = "";


        if (!Array.isArray(
            purchaseOrders) ||
            purchaseOrders.length === 0) {

            renderNoResults(
                suggestionBox,
                "No matching Customer POs found."
            );

            return;
        }


        /* =================================================
           BUILD PO RESULTS
           ================================================= */

        purchaseOrders.forEach(
            function (purchaseOrder) {

                const button =
                    document.createElement(
                        "button"
                    );


                button.type =
                    "button";


                button.className =
                    "list-group-item "
                    + "list-group-item-action "
                    + "text-start";


                /* =========================================
                   PO NUMBER
                   ========================================= */

                const poElement =
                    document.createElement(
                        "div"
                    );


                poElement.className =
                    "fw-semibold";


                poElement.textContent =
                    purchaseOrder
                        .purchaseOrderNumber
                    || purchaseOrder.text
                    || "";


                button.appendChild(
                    poElement
                );


                /* =========================================
                   CUSTOMER NAME
                   ========================================= */

                if (purchaseOrder
                    .customerName) {

                    const customerElement =
                        document.createElement(
                            "div"
                        );


                    customerElement.className =
                        "small text-muted";


                    customerElement.textContent =
                        purchaseOrder
                            .customerName;


                    button.appendChild(
                        customerElement
                    );

                }


                /* =========================================
                   SELECT CUSTOMER PO
                   ========================================= */

                button.addEventListener(
                    "click",
                    function () {

                        const selectedValue =
                            purchaseOrder
                                .purchaseOrderNumber
                            || purchaseOrder.text
                            || "";


                        input.value =
                            selectedValue;


                        hiddenId.value =
                            purchaseOrder.id;


                        setSelectedText(
                            selectedValue
                        );


                        hideSuggestionBox(
                            suggestionBox
                        );


                        input.focus();

                    }
                );


                suggestionBox.appendChild(
                    button
                );

            }
        );


        showSuggestionBox(
            suggestionBox
        );

    }
    catch (error) {

        if (error.name ===
            "AbortError") {

            return;
        }


        console.error(
            "Customer PO autocomplete failed:",
            error
        );


        hideSuggestionBox(
            suggestionBox
        );

    }
}


/* =========================================================
   SHOW SUGGESTION BOX
   ========================================================= */

function showSuggestionBox(
    suggestionBox) {

    suggestionBox.classList.remove(
        "d-none"
    );
}


/* =========================================================
   HIDE SUGGESTION BOX
   ========================================================= */

function hideSuggestionBox(
    suggestionBox) {

    suggestionBox.classList.add(
        "d-none"
    );
}


/* =========================================================
   NO RESULTS
   ========================================================= */

function renderNoResults(
    suggestionBox,
    message) {

    suggestionBox.innerHTML = "";


    const item =
        document.createElement(
            "div"
        );


    item.className =
        "list-group-item "
        + "text-muted "
        + "small";


    item.textContent =
        message;


    suggestionBox.appendChild(
        item
    );


    showSuggestionBox(
        suggestionBox
    );
}