/*
==============================================================

File : production-job.js

Purpose :
Handles Production Job client-side functionality.

Responsibilities :
- Initialize searchable Customer PO Item dropdown.
- Reuse the existing Select2 UI pattern used by ERP Masters.
- Keep Production Job dropdown logic separate from Item Master.
- Preserve normal select change events used by Production
  Source Information.

Important :
- This file does not provide Quick Master creation.
- Customer PO Item is a transaction source, not Master data.
- Only records supplied by the Production Job ViewModel are
  available for selection.

==============================================================
*/

(function () {
    "use strict";

    // =========================================================
    // INITIALIZATION
    // =========================================================

    document.addEventListener(
        "DOMContentLoaded",
        initializeProductionJob
    );


    function initializeProductionJob() {

        initializeProductionSourceSelect();

    }


    // =========================================================
    // CUSTOMER PO ITEM SEARCHABLE SELECT
    // =========================================================

    function initializeProductionSourceSelect() {

        if (!window.jQuery ||
            !jQuery.fn.select2) {

            return;
        }


        const $select =
            jQuery(
                ".js-production-source-select"
            );


        if ($select.length === 0) {
            return;
        }


        $select.each(
            function () {

                const $currentSelect =
                    jQuery(this);


                if ($currentSelect.hasClass(
                    "select2-hidden-accessible"
                )) {

                    return;
                }


                const placeholder =
                    $currentSelect.data(
                        "placeholder"
                    ) ||
                    "-- Select Customer PO Item --";


                $currentSelect.select2({

                    width: "100%",

                    placeholder:
                        placeholder,

                    allowClear:
                        true,

                    minimumResultsForSearch:
                        0,

                    language: {

                        noResults:
                            function () {

                                return "No matching Customer PO Item found.";

                            }

                    }

                });


                /*
                 * Keep the existing Production Source
                 * Information logic working when Select2
                 * selection changes.
                 */
                $currentSelect.on(
                    "select2:select select2:clear",
                    function () {

                        const element =
                            this;


                        element.dispatchEvent(
                            new Event(
                                "change",
                                {
                                    bubbles: true
                                }
                            )
                        );

                    }
                );

            }
        );

    }

})();