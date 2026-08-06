/*
==============================================================

File : item-form.js

Purpose :
Handles Item form searchable master dropdowns and
similar Item Name warnings.

Features :
- Search inside Category, Brand and UOM dropdowns.
- Add Master option when no result is found.
- Reliable navigation from Select2 dropdown.
- Similar Item Name warning.

==============================================================
*/

(function ($) {
    "use strict";

    $(document).ready(function () {

        initializeMasterDropdowns();
        initializeMasterNavigation();
        initializeSimilarItemCheck();

    });

    /*
    ==========================================================
    Searchable Master Dropdowns
    ==========================================================
    */

    function initializeMasterDropdowns() {

        $(".js-master-select").each(function () {

            const $select = $(this);

            const placeholder =
                $select.attr("data-placeholder") ||
                "-- Select --";

            const addLabel =
                $select.attr("data-add-label") ||
                "Add New";

            const addUrl =
                $select.attr("data-add-url");

            $select.select2({
                width: "100%",
                placeholder: placeholder,
                allowClear: true,

                /*
                 * Always display search box inside dropdown.
                 */
                minimumResultsForSearch: 0,

                language: {
                    searching: function () {
                        return "Searching...";
                    },

                    noResults: function () {

                        if (!addUrl) {
                            return "No records found.";
                        }

                        return `
                            <div class="select2-add-master-wrapper">

                                <div class="text-muted small px-2 pt-2 pb-1">
                                    No records found.
                                </div>

                                <a href="${addUrl}"
                                   class="js-add-master
                                          btn btn-link
                                          text-decoration-none
                                          text-start
                                          w-100
                                          px-2
                                          py-2">

                                    <i class="fa-solid fa-plus me-1"></i>

                                    ${addLabel}

                                </a>

                            </div>
                        `;
                    }
                },

                /*
                 * Allows the Add Master link HTML.
                 * addLabel and addUrl are provided by our Razor view.
                 */
                escapeMarkup: function (markup) {
                    return markup;
                }
            });

        });
    }

    /*
    ==========================================================
    Add Master Navigation
    ==========================================================
    */

    function initializeMasterNavigation() {

        /*
         * Select2 captures normal click events.
         * Capture-phase pointerdown executes before Select2
         * closes the dropdown.
         */
        document.addEventListener(
            "pointerdown",
            function (event) {

                const addMasterLink =
                    event.target.closest(".js-add-master");

                if (!addMasterLink) {
                    return;
                }

                event.preventDefault();
                event.stopPropagation();

                const destinationUrl =
                    addMasterLink.getAttribute("href");

                if (destinationUrl) {
                    window.location.assign(destinationUrl);
                }

            },
            true
        );

        /*
         * Keyboard navigation support.
         */
        document.addEventListener(
            "keydown",
            function (event) {

                if (event.key !== "Enter") {
                    return;
                }

                const addMasterLink =
                    event.target.closest(".js-add-master");

                if (!addMasterLink) {
                    return;
                }

                event.preventDefault();

                const destinationUrl =
                    addMasterLink.getAttribute("href");

                if (destinationUrl) {
                    window.location.assign(destinationUrl);
                }

            },
            true
        );
    }

    /*
    ==========================================================
    Similar Item Name Check
    ==========================================================
    */

    function initializeSimilarItemCheck() {

        const itemNameInput =
            document.getElementById("ItemName");

        const warningContainer =
            document.getElementById("similarItemWarning");

        const similarItemList =
            document.getElementById("similarItemList");

        const confirmationCheckbox =
            document.getElementById("ConfirmSimilarItemName");

        if (!itemNameInput ||
            !warningContainer ||
            !similarItemList ||
            !confirmationCheckbox) {

            return;
        }

        const similarNameUrl =
            itemNameInput.dataset.similarUrl;

        let requestTimer;

        itemNameInput.addEventListener("input", function () {

            clearTimeout(requestTimer);

            confirmationCheckbox.checked = false;

            const itemName =
                itemNameInput.value.trim();

            if (itemName.length < 3) {

                clearSimilarItemWarning();

                return;
            }

            requestTimer = setTimeout(function () {

                checkSimilarItemNames(
                    similarNameUrl,
                    itemName
                );

            }, 500);
        });

        async function checkSimilarItemNames(
            requestUrl,
            itemName) {

            if (!requestUrl) {
                return;
            }

            const itemId =
                document.getElementById("ItemId")?.value || "";

            const requestAddress =
                requestUrl +
                "?itemName=" +
                encodeURIComponent(itemName) +
                "&itemId=" +
                encodeURIComponent(itemId);

            try {

                const response = await fetch(requestAddress, {
                    method: "GET",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                if (!response.ok) {
                    return;
                }

                const result =
                    await response.json();

                similarItemList.innerHTML = "";

                if (!result.hasSimilarItems) {

                    clearSimilarItemWarning();

                    return;
                }

                result.items.forEach(function (item) {

                    const listItem =
                        document.createElement("li");

                    listItem.textContent = item;

                    similarItemList.appendChild(listItem);
                });

                warningContainer.classList.remove("d-none");

            } catch {

                /*
                 * Server-side similar-name validation still executes
                 * when the form is submitted.
                 */
            }
        }

        function clearSimilarItemWarning() {

            similarItemList.innerHTML = "";

            warningContainer.classList.add("d-none");
        }
    }

})(jQuery);