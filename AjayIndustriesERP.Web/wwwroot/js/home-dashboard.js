/*
=============================================================
File: home-dashboard.js
Module: Home Dashboard
Layer: Web - JavaScript

Purpose:
Handles Home Dashboard client-side behavior.

Current Responsibility:
- Automatically opens Supplier Payment Due modal
  when Home Dashboard loads.

Important:
- No user click is required.
- Modal opens only when the modal element exists.
- Modal element exists only when due alerts are available.
=============================================================
*/

document.addEventListener("DOMContentLoaded", function () {

    initializeSupplierPaymentDueModal();

});


/* =========================================================
   SUPPLIER PAYMENT DUE MODAL
   ========================================================= */

function initializeSupplierPaymentDueModal() {

    const modalElement =
        document.getElementById("supplierPaymentDueModal");


    if (!modalElement) {
        return;
    }


    const shouldAutoOpen =
        modalElement.getAttribute("data-auto-open") === "true";


    if (!shouldAutoOpen) {
        return;
    }


    if (typeof bootstrap === "undefined" ||
        !bootstrap.Modal) {

        console.error(
            "Bootstrap Modal is not available."
        );

        return;
    }


    const modal =
        bootstrap.Modal.getOrCreateInstance(
            modalElement
        );


    modal.show();
}