// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

window.addEventListener('DOMContentLoaded', async () => {
    const token = await SaaSSampleWebApp.GetAccessToken();

    const getSubscriptions = await fetch(`${apiRootEndpoint}/subscriptions`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': token
        }
    });

    const subscriptions = await getSubscriptions.json();

    subscriptions.forEach(subscription => {
        var tr = document.createElement('tr');
        tr.attributes["lid"] = subscription.id;
        tr.innerHTML = `
            <td>${subscription.tenantName}</td>
            <td>${subscription.tenantId}</td>
            <td>${subscription.licenceType}</td>
            <td>${subscription.purchaseSeatsCount ? subscription.purchaseSeatsCount : ""}</td>`;
        document.getElementById('orgs').appendChild(tr);
    });

    document.querySelectorAll("#orgs tr").forEach(elm => {
        elm.addEventListener("click", async function () {
            const subscription = subscriptions.find(l => {
                return l.id === this.attributes["lid"];
            });

            document.getElementById("orgSection").style.display = "none";
            document.getElementById("orgDetailSection").style.display = "block";

            document.querySelector("#orgDetailSection span.org").textContent = subscription.tenantName;
            document.querySelector("#orgDetailSection h3.org").textContent = subscription.tenantName;
            document.querySelector('#orgDetailSection p:nth-child(3) span').textContent = subscription.firstComeFirstServedAssignment;
            document.querySelector('#orgDetailSection p:nth-child(4) span').textContent = subscription.allowOverAssignment;
            document.querySelector("#orgDetailSection p:nth-child(5) span").textContent = subscription.purchaser;

            document.getElementById('assignedusers').innerHTML = "";

            const getLicenses = await fetch(`${apiRootEndpoint}/subscriptions/${subscription.id}/licenses`, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': token
                }
            });

            if (getLicenses.status === 200) {
                const licenses = await getLicenses.json();
                licenses.forEach(user => {
                    var tr = document.createElement('tr');
                    tr.innerHTML = `<td>${user.userEmail}</td>
                                    <td>${user.activationDateTime && moment(user.activationDateTime).isSame(new Date(), 'month') ? 'Yes' : 'No'}</td>`;
                    document.getElementById('assignedusers').appendChild(tr);
                });
            }
        });
    });

    document.getElementById("manageAccountButton").addEventListener("click", () => {
        document.getElementById("landingSection").style.display = "none";
        document.getElementById("orgSection").style.display = "block";
    });

    document.querySelector("nav .ms-Link").addEventListener("click", () => {
        document.getElementById("orgDetailSection").style.display = "none";
        document.getElementById("orgSection").style.display = "block";
    });
});