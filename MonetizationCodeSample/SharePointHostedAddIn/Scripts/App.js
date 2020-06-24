'use strict';

ExecuteOrDelayUntilScriptLoaded(initializePage, "sp.js");

const endpointConfig = {
    //Monetization Code Sample Web App
    SaaSWeb: "https://<SaaSWebAppUrl>",

    //Monetization Code Sample Web API
    SaaSAPI: "https://<SaaSWebAPIUrl>/api/Subscriptions/CheckOrActivateLicense",
    OfferID: "contoso_o365_addin"
};

window.addEventListener('message', function (event) {
    var accessToken = event.data;
    sendRequest(`${endpointConfig.SaaSAPI}/${endpointConfig.OfferID}`, "POST", accessToken, updateActivate);
});

function sendRequest(endpoint, method, token, callback) {
    const headers = new Headers();
    const bearer = `Bearer ${token}`;

    headers.append("Authorization", bearer);

    const options = {
        method: method,
        headers: headers
    };

    fetch(endpoint, options)
        .then(response => {
            return response.json();
        })
        .then(response => {
            callback(response, endpoint)
        })
        .catch(error => {
            console.log(error)
        })
}

function updateActivate(message) {
    $("#licenseResult").text(message.status === "Success" ? "You do have a paid license " : "You don't have a paid license ");
    $("#footer").hide();
    console.log(`DEBUG: You have ${message.availableLicenseQuantity === null ? "0" : message.availableLicenseQuantity} licenses available in your tenant`);
    console.log(`DEBUG: ${message.reason}`);
    console.log(`DEBUG: Overrun is ${message.allowOverAssignment === false ? "disabled" : "enabled"}`);
    console.log(`DEBUG: Auto-license-assignment is ${message.firstComeFirstServedAssignment === true ? "enabled" : "disabled"}`);
}

function initializePage() {
    var context = SP.ClientContext.get_current();
    var user = context.get_web().get_currentUser();

    // This code runs when the DOM is ready and creates a context object which is needed to use the SharePoint object model
    $(document).ready(function () {
        getLoginHint();
    });

    // This function prepares, loads, and then executes a SharePoint query to get the current users information
    function getLoginHint() {
        context.load(user);
        context.executeQueryAsync(onGetLoginHintSuccess, onLoginHintFail);
    }

    // This function is executed if the above call is successful
    // It replaces the contents of the 'message' element with the user name
    function onGetLoginHintSuccess() {
        $("#welcome").html("Welcome " + user.get_title());
        $('#authIframe').attr("src", `${endpointConfig.SaaSWeb}/home/SPHostedAddinEmbedContent?loginHint=${user.get_email()}`);
    }

    // This function is executed if the above call fails
    function onLoginHintFail(sender, args) {
        alert('Failed to get user name. Error:' + args.get_message());
    }
}
