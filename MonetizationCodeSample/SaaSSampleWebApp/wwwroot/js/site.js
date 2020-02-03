// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var TableElements = document.querySelectorAll(".ms-Table");
for (var i = 0; i < TableElements.length; i++) {
    new fabric['Table'](TableElements[i]);
}

var DropdownHTMLElements = document.querySelectorAll('.ms-Dropdown');
for (var i = 0; i < DropdownHTMLElements.length; ++i) {
    var Dropdown = new fabric['Dropdown'](DropdownHTMLElements[i]);
}

var TextFieldElements = document.querySelectorAll(".ms-TextField");
for (var i = 0; i < TextFieldElements.length; i++) {
    new fabric['TextField'](TextFieldElements[i]);
}

var DialogElements = document.querySelectorAll(".ms-Dialog");
var DialogComponents = [];
for (var i = 0; i < DialogElements.length; i++) {
    (function () {
        DialogComponents[i] = new fabric['Dialog'](DialogElements[i]);
    }());
}

var CommandButtonElements = document.querySelectorAll(".ms-CommandButton");
for (var i = 0; i < CommandButtonElements.length; i++) {
    new fabric['CommandButton'](CommandButtonElements[i]);
}

var ToggleElements = document.querySelectorAll(".ms-Toggle");
for (var i = 0; i < ToggleElements.length; i++) {
    new fabric['Toggle'](ToggleElements[i]);
}

var SpinnerElements = document.querySelectorAll(".ms-Spinner");
for (var i = 0; i < SpinnerElements.length; i++) {
    new fabric['Spinner'](SpinnerElements[i]);
}

var SaaSSampleWebApp = {
    GetAccessToken: () => {
        return fetch("/Common/GetSaaSWebApiAccessToken")
            .then(_ => _.text())
            .then(_ => {
                return "Bearer " + _;
            });
    },
    ShowMessage: (message, close) => {
        DialogComponents.forEach(d => {
            if (d._dialog.attributes['id'].value === 'messageDialog') {
                document.querySelector('#messageDialog .ms-Dialog-content p').innerHTML = message;
                d.open();
            }
        });
    }
};