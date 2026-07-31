// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/*
==============================================================

Delete Confirmation Modal

Purpose :
Reusable delete confirmation.

==============================================================
*/

function confirmDelete(actionUrl) {

    const deleteForm = document.getElementById("deleteForm");

    deleteForm.action = actionUrl;

    const modal = new bootstrap.Modal(
        document.getElementById("deleteModal"));

    modal.show();
}