/*
============================================================
File: delivery-challan-form.js

Purpose:
Handles Delivery Challan Create/Edit form behaviour.

Responsibilities:
- Load Finalized PDI information using AJAX.
- Auto-fill trusted PDI snapshot fields.
- Auto-load Customer Master information on Create.
- Auto-load editable Customer delivery address on Create.
- Auto-load Company / Workshop information on Create.
- Preserve manually edited Customer address.
- Preserve historical master snapshot information on Edit.
- Manage multiple dispatch lines.
- Prevent duplicate PDI selection.
- Enforce same Customer across all lines.
- Validate Dispatch Quantity against Available Qty.
- Re-index dynamic ASP.NET Core collection fields.
- Maintain Customer header from selected PDI.
- Maintain UOM beside Dispatch Quantity.

Important:
- Business validation is still enforced server-side.
- Product ID is manually entered.
- HSN No. is manually entered for now.
- Product ID and HSN No. are cleared when PDI changes.
- Customer Master display data is read-only.
- Customer address is auto-loaded but editable.
- Same-customer additional PDI selection does not overwrite
  manually edited Customer address.
- Edit mode never replaces saved historical master snapshot
  data with current Customer / Company Master values.
============================================================
*/

document.addEventListener(
    "DOMContentLoaded",
    function () {

        "use strict";


        // ==================================================
        // Main Elements
        // ==================================================

        const container =
            document.getElementById(
                "deliveryChallanItemsContainer");

        const template =
            document.getElementById(
                "deliveryChallanItemTemplate");

        const addButton =
            document.getElementById(
                "btnAddDeliveryChallanItem");

        const noItemsMessage =
            document.getElementById(
                "deliveryChallanNoItemsMessage");

        const customerIdInput =
            document.getElementById(
                "CustomerId");

        const customerNameInput =
            document.getElementById(
                "CustomerName");


        if (
            !container ||
            !template ||
            !addButton
        ) {
            return;
        }


        const getPdiUrl =
            container.dataset.getPdiUrl;

        const challanId =
            parseInt(
                container.dataset.challanId || "0",
                10);


        // ==================================================
        // Master Snapshot State
        // ==================================================

        let masterCustomerId =
            parseInt(
                customerIdInput?.value || "0",
                10);


        // ==================================================
        // Initialization
        // ==================================================

        initializeExistingItems();

        bindEvents();

        reindexItems();

        updateRemoveButtons();

        updateNoItemsMessage();

        refreshCustomerFromItems();


        // ==================================================
        // Event Binding
        // ==================================================

        function bindEvents() {

            addButton.addEventListener(
                "click",
                function () {

                    addNewItem();
                });


            container.addEventListener(
                "click",
                function (event) {

                    const removeButton =
                        event.target.closest(
                            ".btn-remove-delivery-item");


                    if (!removeButton) {
                        return;
                    }


                    const itemCard =
                        removeButton.closest(
                            ".delivery-challan-item");


                    if (!itemCard) {
                        return;
                    }


                    removeItem(
                        itemCard);
                });


            container.addEventListener(
                "change",
                function (event) {

                    if (
                        event.target.classList.contains(
                            "pdi-select")
                    ) {
                        handlePdiSelection(
                            event.target);
                    }
                });


            container.addEventListener(
                "input",
                function (event) {

                    if (
                        event.target.classList.contains(
                            "dispatch-quantity")
                    ) {
                        validateDispatchQuantity(
                            event.target);
                    }
                });


            const form =
                container.closest(
                    "form");


            if (form) {

                form.addEventListener(
                    "submit",
                    function (event) {

                        const isValid =
                            validateAllDispatchQuantities();


                        if (!isValid) {

                            event.preventDefault();

                            event.stopPropagation();
                        }
                    });
            }
        }


        // ==================================================
        // Initialize Existing Items
        // ==================================================

        function initializeExistingItems() {

            const items =
                getItemCards();


            const existingCustomerId =
                customerIdInput
                    ? customerIdInput.value
                    : "";


            const existingCustomerName =
                customerNameInput
                    ? customerNameInput.value
                    : "";


            items.forEach(
                function (itemCard) {

                    const pdiSelect =
                        itemCard.querySelector(
                            ".pdi-select");


                    if (
                        pdiSelect &&
                        pdiSelect.value
                    ) {
                        itemCard.dataset.customerId =
                            existingCustomerId;

                        itemCard.dataset.customerName =
                            existingCustomerName;
                    }


                    syncExistingUnitName(
                        itemCard);


                    const quantityInput =
                        itemCard.querySelector(
                            ".dispatch-quantity");


                    if (quantityInput) {

                        validateDispatchQuantity(
                            quantityInput);
                    }
                });
        }


        // ==================================================
        // Existing UOM
        // ==================================================

        function syncExistingUnitName(
            itemCard) {

            const hiddenUnit =
                itemCard.querySelector(
                    ".unit-name-value");

            const displayUnit =
                itemCard.querySelector(
                    ".unit-name");


            if (
                !hiddenUnit ||
                !displayUnit
            ) {
                return;
            }


            displayUnit.textContent =
                hiddenUnit.value || "";
        }


        // ==================================================
        // Add Item
        // ==================================================

        function addNewItem() {

            const index =
                getItemCards().length;

            const lineNumber =
                index + 1;


            let html =
                template.innerHTML;


            html =
                html.replaceAll(
                    "__index__",
                    index.toString());


            html =
                html.replaceAll(
                    "__lineNumber__",
                    lineNumber.toString());


            container.insertAdjacentHTML(
                "beforeend",
                html);


            reindexItems();

            updateRemoveButtons();

            updateNoItemsMessage();

            refreshUnobtrusiveValidation();
        }


        // ==================================================
        // Remove Item
        // ==================================================

        function removeItem(
            itemCard) {

            const items =
                getItemCards();


            if (items.length <= 1) {
                return;
            }


            itemCard.remove();


            reindexItems();

            updateRemoveButtons();

            updateNoItemsMessage();

            refreshCustomerFromItems();

            refreshMasterCustomerState();

            refreshUnobtrusiveValidation();
        }


        // ==================================================
        // PDI Selection
        // ==================================================

        async function handlePdiSelection(
            pdiSelect) {

            const itemCard =
                pdiSelect.closest(
                    ".delivery-challan-item");


            if (!itemCard) {
                return;
            }


            const pdiId =
                parseInt(
                    pdiSelect.value || "0",
                    10);


            // ----------------------------------------------
            // Clear Manual Line References
            // ----------------------------------------------

            clearManualItemReferences(
                itemCard);


            // ----------------------------------------------
            // Clear Old PDI Snapshot
            // ----------------------------------------------

            clearPdiSnapshot(
                itemCard);


            itemCard.dataset.customerId =
                "";

            itemCard.dataset.customerName =
                "";


            if (pdiId <= 0) {

                refreshCustomerFromItems();

                refreshMasterCustomerState();

                return;
            }


            // ----------------------------------------------
            // Duplicate PDI Validation
            // ----------------------------------------------

            if (
                isDuplicatePdi(
                    pdiId,
                    itemCard)
            ) {

                window.alert(
                    "The same PDI Report cannot be added more than once in one Delivery Challan.");


                pdiSelect.value =
                    "";


                refreshCustomerFromItems();

                refreshMasterCustomerState();

                return;
            }


            // ----------------------------------------------
            // Load PDI Data
            // ----------------------------------------------

            pdiSelect.disabled =
                true;


            try {

                const url =
                    new URL(
                        getPdiUrl,
                        window.location.origin);


                url.searchParams.set(
                    "id",
                    pdiId.toString());


                if (challanId > 0) {

                    url.searchParams.set(
                        "deliveryChallanId",
                        challanId.toString());
                }


                const response =
                    await fetch(
                        url.toString(),
                        {
                            method: "GET",

                            headers: {
                                "X-Requested-With":
                                    "XMLHttpRequest"
                            }
                        });


                if (!response.ok) {

                    throw new Error(
                        "Unable to load PDI information.");
                }


                const data =
                    await response.json();


                if (!data.success) {

                    window.alert(
                        data.message ||
                        "Unable to load PDI information.");


                    pdiSelect.value =
                        "";


                    refreshCustomerFromItems();

                    refreshMasterCustomerState();

                    return;
                }


                // ------------------------------------------
                // Same Customer Validation
                // ------------------------------------------

                if (
                    hasDifferentCustomer(
                        data.customerId,
                        itemCard)
                ) {

                    window.alert(
                        "All items in one Delivery Challan must belong to the same Customer.");


                    pdiSelect.value =
                        "";


                    clearPdiSnapshot(
                        itemCard);


                    refreshCustomerFromItems();

                    refreshMasterCustomerState();

                    return;
                }


                // ------------------------------------------
                // Apply Trusted PDI Data
                // ------------------------------------------

                applyPdiData(
                    itemCard,
                    data);


                itemCard.dataset.customerId =
                    data.customerId
                        ? data.customerId.toString()
                        : "";


                itemCard.dataset.customerName =
                    data.customerName || "";


                // ------------------------------------------
                // Apply Customer / Company Master Data
                // ------------------------------------------

                applyMasterDataIfRequired(
                    data);


                // ------------------------------------------
                // Default Dispatch Quantity
                // ------------------------------------------

                const dispatchQuantityInput =
                    itemCard.querySelector(
                        ".dispatch-quantity");


                if (dispatchQuantityInput) {

                    dispatchQuantityInput.value =
                        formatQuantity(
                            data.availableQuantity);


                    validateDispatchQuantity(
                        dispatchQuantityInput);
                }


                refreshCustomerFromItems();
            }
            catch (error) {

                console.error(
                    error);


                window.alert(
                    "Unable to load PDI information. Please try again.");


                pdiSelect.value =
                    "";


                clearPdiSnapshot(
                    itemCard);


                refreshCustomerFromItems();

                refreshMasterCustomerState();
            }
            finally {

                pdiSelect.disabled =
                    false;
            }
        }


        // ==================================================
        // Master Data Application
        // ==================================================

        function applyMasterDataIfRequired(
            data) {

            /*
             * Edit mode:
             * Controller intentionally returns
             * shouldRefreshMasterData = false.
             */
            if (
                data.shouldRefreshMasterData !==
                true
            ) {
                return;
            }


            const newCustomerId =
                parseInt(
                    data.customerId || "0",
                    10);


            if (newCustomerId <= 0) {
                return;
            }


            /*
             * Same Customer's master snapshot is already on
             * the form.
             *
             * Do not reload it because Customer Address may
             * have been manually edited by the user.
             */
            if (
                masterCustomerId > 0 &&
                masterCustomerId ===
                newCustomerId
            ) {
                return;
            }


            applyCustomerInformation(
                data);


            applyCustomerAddress(
                data);


            applyCompanyInformation(
                data);


            masterCustomerId =
                newCustomerId;
        }


        // ==================================================
        // Customer Master Information
        // ==================================================

        function applyCustomerInformation(
            data) {

            setPageValue(
                ".customer-code",
                data.customerCode);


            setPageValue(
                ".customer-legal-name",
                data.customerLegalName);


            setPageValue(
                ".customer-gstin",
                data.customerGstin);


            setPageValue(
                ".customer-pan",
                data.customerPan);


            setPageValue(
                ".customer-contact-person",
                data.customerContactPerson);


            setPageValue(
                ".customer-mobile-number",
                data.customerMobileNumber);


            setPageValue(
                ".customer-alternate-mobile-number",
                data.customerAlternateMobileNumber);


            setPageValue(
                ".customer-email",
                data.customerEmail);


            setPageValue(
                ".customer-payment-terms",
                data.customerPaymentTerms);


            setPageValue(
                ".customer-credit-days",
                data.customerCreditDays);


            setPageValue(
                ".customer-website",
                data.customerWebsite);


            setPageValue(
                ".customer-master-remarks",
                data.customerMasterRemarks);
        }


        // ==================================================
        // Customer Address
        // ==================================================

        function applyCustomerAddress(
            data) {

            setPageValue(
                ".customer-address-line-1",
                data.customerAddressLine1);


            setPageValue(
                ".customer-address-line-2",
                data.customerAddressLine2);


            setPageValue(
                ".customer-city",
                data.customerCity);


            setPageValue(
                ".customer-district",
                data.customerDistrict);


            setPageValue(
                ".customer-state",
                data.customerState);


            setPageValue(
                ".customer-pincode",
                data.customerPincode);


            setPageValue(
                ".customer-country",
                data.customerCountry);
        }


        // ==================================================
        // Company / Workshop Information
        // ==================================================

        function applyCompanyInformation(
            data) {

            setPageValue(
                ".company-id",
                data.companyId);


            setPageValue(
                ".company-code",
                data.companyCode);


            setPageValue(
                ".company-name",
                data.companyName);


            setPageValue(
                ".company-gst-number",
                data.companyGstNumber);


            setPageValue(
                ".company-pan-number",
                data.companyPanNumber);


            setPageValue(
                ".company-phone-number",
                data.companyPhoneNumber);


            setPageValue(
                ".company-email",
                data.companyEmail);


            setPageValue(
                ".company-website",
                data.companyWebsite);


            setPageValue(
                ".company-contact-person",
                data.companyContactPerson);


            setPageValue(
                ".company-address",
                data.companyAddress);


            setPageValue(
                ".company-city",
                data.companyCity);


            setPageValue(
                ".company-state",
                data.companyState);


            setPageValue(
                ".company-country",
                data.companyCountry);


            setPageValue(
                ".company-postal-code",
                data.companyPostalCode);


            setPageValue(
                ".company-po-terms",
                data.companyPurchaseOrderTermsAndConditions);
        }


        // ==================================================
        // Master Customer State
        // ==================================================

        function refreshMasterCustomerState() {

            /*
             * Edit mode:
             * Never clear saved historical snapshot data.
             */
            if (challanId > 0) {
                return;
            }


            const selectedCustomerId =
                getFirstSelectedCustomerId();


            if (selectedCustomerId > 0) {

                masterCustomerId =
                    selectedCustomerId;

                return;
            }


            masterCustomerId =
                0;


            clearCustomerInformation();

            clearCustomerAddress();

            clearCompanyInformation();
        }


        function getFirstSelectedCustomerId() {

            const items =
                getItemCards();


            for (
                const itemCard
                of items
            ) {

                const pdiSelect =
                    itemCard.querySelector(
                        ".pdi-select");


                if (
                    !pdiSelect ||
                    !pdiSelect.value
                ) {
                    continue;
                }


                const customerId =
                    parseInt(
                        itemCard.dataset.customerId || "0",
                        10);


                if (customerId > 0) {

                    return customerId;
                }
            }


            return 0;
        }


        // ==================================================
        // Clear Customer Master Information
        // ==================================================

        function clearCustomerInformation() {

            const selectors =
                [
                    ".customer-code",
                    ".customer-legal-name",
                    ".customer-gstin",
                    ".customer-pan",
                    ".customer-contact-person",
                    ".customer-mobile-number",
                    ".customer-alternate-mobile-number",
                    ".customer-email",
                    ".customer-payment-terms",
                    ".customer-credit-days",
                    ".customer-website",
                    ".customer-master-remarks"
                ];


            selectors.forEach(
                function (selector) {

                    setPageValue(
                        selector,
                        "");
                });
        }


        // ==================================================
        // Clear Customer Address
        // ==================================================

        function clearCustomerAddress() {

            const selectors =
                [
                    ".customer-address-line-1",
                    ".customer-address-line-2",
                    ".customer-city",
                    ".customer-district",
                    ".customer-state",
                    ".customer-pincode",
                    ".customer-country"
                ];


            selectors.forEach(
                function (selector) {

                    setPageValue(
                        selector,
                        "");
                });
        }


        // ==================================================
        // Clear Company Information
        // ==================================================

        function clearCompanyInformation() {

            const selectors =
                [
                    ".company-id",
                    ".company-code",
                    ".company-name",
                    ".company-gst-number",
                    ".company-pan-number",
                    ".company-phone-number",
                    ".company-email",
                    ".company-website",
                    ".company-contact-person",
                    ".company-address",
                    ".company-city",
                    ".company-state",
                    ".company-country",
                    ".company-postal-code",
                    ".company-po-terms"
                ];


            selectors.forEach(
                function (selector) {

                    setPageValue(
                        selector,
                        "");
                });
        }


        // ==================================================
        // Clear Manual Item References
        // ==================================================

        function clearManualItemReferences(
            itemCard) {

            const productReference =
                itemCard.querySelector(
                    ".product-reference");


            const hsnNumber =
                itemCard.querySelector(
                    ".hsn-number");


            if (productReference) {

                productReference.value =
                    "";
            }


            if (hsnNumber) {

                hsnNumber.value =
                    "";
            }
        }


        // ==================================================
        // Apply PDI Data
        // ==================================================

        // ==================================================
        // Apply PDI Data
        // ==================================================

        function applyPdiData(
            itemCard,
            data) {

            // ----------------------------------------------
            // Hidden IDs
            // ----------------------------------------------

            setValue(
                itemCard,
                ".production-job-id",
                data.productionJobId);


            setValue(
                itemCard,
                ".customer-po-item-id",
                data.customerPurchaseOrderItemId);


            setValue(
                itemCard,
                ".item-master-id",
                data.itemId);


            setValue(
                itemCard,
                ".customer-drawing-id",
                data.customerDrawingId);


            // ----------------------------------------------
            // PDI / Production
            // ----------------------------------------------

            setValue(
                itemCard,
                ".pdi-code",
                data.preDispatchInspectionCode);


            setValue(
                itemCard,
                ".production-job-code",
                data.productionJobCode);


            // ----------------------------------------------
            // Customer PO
            // ----------------------------------------------

            setValue(
                itemCard,
                ".customer-po-number",
                data.customerPurchaseOrderNumber);


            // ----------------------------------------------
            // Item
            // ----------------------------------------------

            setValue(
                itemCard,
                ".item-code",
                data.itemCode);


            setValue(
                itemCard,
                ".customer-item-code",
                data.customerItemCode);


            setValue(
                itemCard,
                ".item-name",
                data.itemName);


            setValue(
                itemCard,
                ".part-number",
                data.partNumber);


            // ----------------------------------------------
            // Customer Drawing
            // ----------------------------------------------

            setValue(
                itemCard,
                ".customer-drawing-number",
                data.customerDrawingNumber);


            setValue(
                itemCard,
                ".customer-drawing-revision",
                data.customerDrawingRevision);


            // ----------------------------------------------
            // UOM
            // ----------------------------------------------

            setValue(
                itemCard,
                ".unit-name-value",
                data.unitName);


            setText(
                itemCard,
                ".unit-name",
                data.unitName);


            // ----------------------------------------------
            // Quantities
            // ----------------------------------------------

            setValue(
                itemCard,
                ".pdi-accepted-quantity",
                formatQuantity(
                    data.pdiAcceptedQuantity));


            setValue(
                itemCard,
                ".already-dispatched-quantity",
                formatQuantity(
                    data.alreadyDispatchedQuantity));


            setValue(
                itemCard,
                ".available-quantity",
                formatQuantity(
                    data.availableQuantity));
        }


        // ==================================================
        // Clear PDI Snapshot
        // ==================================================

        function clearPdiSnapshot(
            itemCard) {

            const selectors =
                [
                    ".production-job-id",
                    ".customer-po-item-id",
                    ".item-master-id",
                    ".customer-drawing-id",

                    ".pdi-code",
                    ".production-job-code",
                    ".customer-po-number",

                    ".item-code",
                    ".customer-item-code",
                    ".item-name",
                    ".part-number",

                    ".customer-drawing-number",
                    ".customer-drawing-revision",

                    ".unit-name-value",

                    ".pdi-accepted-quantity",
                    ".already-dispatched-quantity",
                    ".available-quantity",
                    ".dispatch-quantity"
                ];


            selectors.forEach(
                function (selector) {

                    const element =
                        itemCard.querySelector(
                            selector);


                    if (!element) {
                        return;
                    }


                    if (
                        element.classList.contains(
                            "production-job-id") ||
                        element.classList.contains(
                            "customer-po-item-id") ||
                        element.classList.contains(
                            "item-master-id")
                    ) {
                        element.value =
                            "0";
                    }
                    else {
                        element.value =
                            "";
                    }


                    if (
                        element.classList.contains(
                            "dispatch-quantity")
                    ) {
                        element.setCustomValidity(
                            "");
                    }
                });


            const unitDisplay =
                itemCard.querySelector(
                    ".unit-name");


            if (unitDisplay) {

                unitDisplay.textContent =
                    "";
            }
        }


        // ==================================================
        // Duplicate PDI Validation
        // ==================================================

        function isDuplicatePdi(
            pdiId,
            currentItemCard) {

            return getItemCards().some(
                function (itemCard) {

                    if (
                        itemCard ===
                        currentItemCard
                    ) {
                        return false;
                    }


                    const select =
                        itemCard.querySelector(
                            ".pdi-select");


                    if (!select) {
                        return false;
                    }


                    return (
                        parseInt(
                            select.value || "0",
                            10
                        ) === pdiId
                    );
                });
        }


        // ==================================================
        // Same Customer Validation
        // ==================================================

        function hasDifferentCustomer(
            customerId,
            currentItemCard) {

            const selectedCustomerId =
                parseInt(
                    customerId || "0",
                    10);


            if (selectedCustomerId <= 0) {
                return false;
            }


            return getItemCards().some(
                function (itemCard) {

                    if (
                        itemCard ===
                        currentItemCard
                    ) {
                        return false;
                    }


                    const pdiSelect =
                        itemCard.querySelector(
                            ".pdi-select");


                    if (
                        !pdiSelect ||
                        !pdiSelect.value
                    ) {
                        return false;
                    }


                    const otherCustomerId =
                        parseInt(
                            itemCard.dataset.customerId || "0",
                            10);


                    if (otherCustomerId <= 0) {
                        return false;
                    }


                    return (
                        otherCustomerId !==
                        selectedCustomerId
                    );
                });
        }


        // ==================================================
        // Customer Header
        // ==================================================

        function refreshCustomerFromItems() {

            const items =
                getItemCards();


            let customerId =
                "";

            let customerName =
                "";


            for (
                const itemCard
                of items
            ) {

                const pdiSelect =
                    itemCard.querySelector(
                        ".pdi-select");


                if (
                    !pdiSelect ||
                    !pdiSelect.value
                ) {
                    continue;
                }


                if (
                    !itemCard.dataset.customerId
                ) {
                    continue;
                }


                customerId =
                    itemCard.dataset.customerId;


                customerName =
                    itemCard.dataset.customerName || "";


                break;
            }


            if (customerIdInput) {

                customerIdInput.value =
                    customerId || "0";
            }


            if (customerNameInput) {

                customerNameInput.value =
                    customerName;
            }
        }


        // ==================================================
        // Dispatch Quantity Validation
        // ==================================================

        function validateDispatchQuantity(
            quantityInput) {

            if (!quantityInput) {
                return true;
            }


            const itemCard =
                quantityInput.closest(
                    ".delivery-challan-item");


            if (!itemCard) {
                return true;
            }


            const pdiSelect =
                itemCard.querySelector(
                    ".pdi-select");


            if (
                !pdiSelect ||
                !pdiSelect.value
            ) {

                quantityInput.setCustomValidity(
                    "");

                return true;
            }


            const availableInput =
                itemCard.querySelector(
                    ".available-quantity");


            const dispatchQuantity =
                parseDecimal(
                    quantityInput.value);


            const availableQuantity =
                parseDecimal(
                    availableInput
                        ? availableInput.value
                        : "0");


            if (dispatchQuantity <= 0) {

                quantityInput.setCustomValidity(
                    "Dispatch Quantity must be greater than zero.");

                return false;
            }


            if (
                dispatchQuantity >
                availableQuantity
            ) {

                quantityInput.setCustomValidity(
                    "Dispatch Quantity cannot exceed Available Quantity.");

                return false;
            }


            quantityInput.setCustomValidity(
                "");


            return true;
        }


        function validateAllDispatchQuantities() {

            const quantityInputs =
                container.querySelectorAll(
                    ".dispatch-quantity");


            let isValid =
                true;


            quantityInputs.forEach(
                function (input) {

                    if (
                        !validateDispatchQuantity(
                            input)
                    ) {
                        isValid =
                            false;
                    }
                });


            if (!isValid) {

                const firstInvalid =
                    container.querySelector(
                        ".dispatch-quantity:invalid");


                if (firstInvalid) {

                    firstInvalid.reportValidity();

                    firstInvalid.focus();
                }
            }


            return isValid;
        }


        // ==================================================
        // Re-index ASP.NET Collection
        // ==================================================

        function reindexItems() {

            const items =
                getItemCards();


            items.forEach(
                function (
                    itemCard,
                    index
                ) {

                    const lineNumber =
                        index + 1;


                    itemCard.dataset.itemIndex =
                        index.toString();


                    const lineNumberElement =
                        itemCard.querySelector(
                            ".delivery-line-number");


                    if (lineNumberElement) {

                        lineNumberElement.textContent =
                            lineNumber.toString();
                    }


                    const sequenceInput =
                        itemCard.querySelector(
                            ".sequence-number");


                    if (sequenceInput) {

                        sequenceInput.value =
                            lineNumber.toString();
                    }


                    updateCollectionAttributes(
                        itemCard,
                        index);
                });
        }


        function updateCollectionAttributes(
            itemCard,
            newIndex) {

            const elements =
                itemCard.querySelectorAll(
                    "[name], [id], [for], [data-valmsg-for], [aria-describedby]");


            elements.forEach(
                function (element) {

                    updateAttributeIndex(
                        element,
                        "name",
                        newIndex);


                    updateAttributeIndex(
                        element,
                        "id",
                        newIndex);


                    updateAttributeIndex(
                        element,
                        "for",
                        newIndex);


                    updateAttributeIndex(
                        element,
                        "data-valmsg-for",
                        newIndex);


                    updateAttributeIndex(
                        element,
                        "aria-describedby",
                        newIndex);
                });
        }


        function updateAttributeIndex(
            element,
            attributeName,
            newIndex) {

            if (
                !element.hasAttribute(
                    attributeName)
            ) {
                return;
            }


            const value =
                element.getAttribute(
                    attributeName);


            if (!value) {
                return;
            }


            let updatedValue =
                value;


            updatedValue =
                updatedValue.replace(
                    /Items\[\d+\]/g,
                    `Items[${newIndex}]`);


            updatedValue =
                updatedValue.replace(
                    /Items_\d+__/g,
                    `Items_${newIndex}__`);


            element.setAttribute(
                attributeName,
                updatedValue);
        }


        // ==================================================
        // Remove Button State
        // ==================================================

        function updateRemoveButtons() {

            const items =
                getItemCards();


            const disableRemove =
                items.length <= 1;


            items.forEach(
                function (itemCard) {

                    const button =
                        itemCard.querySelector(
                            ".btn-remove-delivery-item");


                    if (!button) {
                        return;
                    }


                    button.disabled =
                        disableRemove;
                });
        }


        // ==================================================
        // No Items Message
        // ==================================================

        function updateNoItemsMessage() {

            if (!noItemsMessage) {
                return;
            }


            const hasItems =
                getItemCards().length > 0;


            noItemsMessage.classList.toggle(
                "d-none",
                hasItems);
        }


        // ==================================================
        // Unobtrusive Validation Refresh
        // ==================================================

        function refreshUnobtrusiveValidation() {

            if (
                typeof window.jQuery ===
                "undefined"
            ) {
                return;
            }


            const $ =
                window.jQuery;


            if (
                !$.validator ||
                !$.validator.unobtrusive
            ) {
                return;
            }


            const form =
                container.closest(
                    "form");


            if (!form) {
                return;
            }


            const $form =
                $(form);


            $form.removeData(
                "validator");


            $form.removeData(
                "unobtrusiveValidation");


            $.validator.unobtrusive.parse(
                $form);
        }


        // ==================================================
        // Helpers
        // ==================================================

        function getItemCards() {

            return Array.from(
                container.querySelectorAll(
                    ".delivery-challan-item"));
        }


        function setValue(
            itemCard,
            selector,
            value) {

            const element =
                itemCard.querySelector(
                    selector);


            if (!element) {
                return;
            }


            element.value =
                value === null ||
                    value === undefined
                    ? ""
                    : value;
        }


        function setText(
            itemCard,
            selector,
            value) {

            const element =
                itemCard.querySelector(
                    selector);


            if (!element) {
                return;
            }


            element.textContent =
                value === null ||
                    value === undefined
                    ? ""
                    : value;
        }


        function setPageValue(
            selector,
            value) {

            const element =
                document.querySelector(
                    selector);


            if (!element) {
                return;
            }


            element.value =
                value === null ||
                    value === undefined
                    ? ""
                    : value;
        }


        function parseDecimal(
            value) {

            const number =
                parseFloat(
                    value);


            return Number.isFinite(
                number)
                ? number
                : 0;
        }


        function formatQuantity(
            value) {

            const number =
                parseDecimal(
                    value);


            return Number(
                number.toFixed(
                    3)
            ).toString();
        }


        // ==================================================
        // End
        // ==================================================
    });