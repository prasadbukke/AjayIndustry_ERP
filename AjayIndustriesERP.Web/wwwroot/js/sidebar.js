document.addEventListener("DOMContentLoaded", function () {

    const menuHeaders = document.querySelectorAll(".erp-menu-header");

    menuHeaders.forEach(header => {

        header.addEventListener("click", function () {

            const submenu = this.nextElementSibling;

            submenu.classList.toggle("show");

            const arrow = this.querySelector(".menu-arrow");

            arrow.classList.toggle("rotate");

        });

    });

});