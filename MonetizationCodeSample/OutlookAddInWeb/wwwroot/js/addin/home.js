// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license in the root of the repo.

/* 
    This file provides functions to get ask the Office host to get an access token to the add-in
	and to pass that token to the server to get Microsoft Graph data. 
*/

// To support IE (which uses ES5), we have to create a Promise object for the global context.
if (!window.Promise) {
    window.Promise = Office.Promise;
}

Office.initialize = function (reason) {
    $(document).ready(function () {
        $("#loginO365PopupButton").click(dialogFallback);
    });
};
// Dialog API
var loginDialog;

function dialogFallback() {
    var url = "/AzureADAuth/Login";
    showLoginPopup(url);
}

// This handler responds to the success or failure message that the pop-up dialog receives from the identity provider
// and access token provider.
function processMessage(arg) {
    loginDialog.close();
    console.log("Message received in processMessage: " + JSON.stringify(arg));
    let message = JSON.parse(arg.message);
    if (message.status === "success") {
        $("#welcome").html("Welcome " + message.accountName);
        $("#licenseResult").text(message.activation.Status === 1 ? "You don't have a paid license " : "You do have a paid license ");
        $("#footer").hide();
        $("#errorResult").text("");

        console.log(`DEBUG: You have ${message.activation.AvailableLicenseQuantity === null ? "0" : message.activation.AvailableLicenseQuantity} licenses available in your tenant`);
        console.log(`DEBUG: ${message.activation.Reason}`);
        console.log(`DEBUG: Overrun is ${message.activation.AllowOverAssignment === false ? "disabled" : "enabled"}`);
        console.log(`DEBUG: Auto-license-assignment is ${message.activation.FirstComeFirstServedAssignment === true ? "enabled" : "disabled"}`);

    } else {
        $("#errorResult").text("Unable to successfully authenticate user or authorize application. Error is: " + message.error);
    }
}

// Use the Office dialog API to open a pop-up and display the sign-in page for the identity provider.
function showLoginPopup(url) {
    var fullUrl = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + url;

    // height and width are percentages of the size of the parent Office application, e.g., PowerPoint, Excel, Word, etc.
    Office.context.ui.displayDialogAsync(fullUrl,
        { height: 60, width: 30 }, function (result) {
            loginDialog = result.value;
            loginDialog.addEventHandler(Microsoft.Office.WebExtension.EventType.DialogMessageReceived, processMessage);
    });
}