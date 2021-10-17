// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", ready);

function ready(event) {
    var NightState = localStorage.getItem("NightMode")
    if (NightState == "on") {
        document.getElementById("main-body").style = "filter: invert(100%); background: #000;";
    }
    else if (NightState == "off") {
        document.getElementById("main-body").style = "";
    }
}

