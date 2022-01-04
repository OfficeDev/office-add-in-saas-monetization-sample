// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

window.addEventListener('DOMContentLoaded', async () => {
    const token = await SaaSSampleWebApp.GetAccessToken();
    const getSubscription = await fetch(`${apiRootEndpoint}/Subscriptions/${subscriptionId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
            'mode': 'cors'
        }
    });
    const subscription = await getSubscription.json();

    if (subscription.licenceType === "SeatBased") {
        const licenseQuantity = subscription.purchaseSeatsCount;
        document.getElementById("seatBasedSection").style.display = "block";
        document.querySelector('#licenseQuantity').textContent = licenseQuantity;

        const retrieveLicenses = async () => {
            const getLicenses = await fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}/Licenses`, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': token,
                    'mode': 'cors'
                }
            });

            document.getElementById('assignedusers').innerHTML = "";

            if (getLicenses.status === 200) {
                const assignedUsers = await getLicenses.json();
                const availableLicenseQuantity = licenseQuantity - assignedUsers.length >= 0 ? licenseQuantity - assignedUsers.length : 0;
                const assignedLicenseQuantity = assignedUsers.length;
                assignedUsers.forEach(user => {
                    var tr = document.createElement('tr');
                    tr.innerHTML = `<td>${user.userEmail}</td>
                                    <td>${user.activationDateTime && moment(user.activationDateTime).isSame(new Date(), 'month') ? 'Yes' : 'No'}</td>
                                    <td><span class="ms-Link remove-button" data-id="${user.id}" data-email="${user.userEmail}">Remove</span></td>`;

                    document.getElementById('assignedusers').appendChild(tr);
                });


                document.querySelector('#availableLicenseQuantity').textContent = availableLicenseQuantity;
                document.querySelector('#assignedLicenseQuantity').textContent = assignedLicenseQuantity;
            }
        };

        await retrieveLicenses();

        if (!isLicenseAdmin) return;

        if (subscription.firstComeFirstServedAssignment) {
            document.querySelector('#fcfsSelect .ms-Toggle-field').classList.add("is-selected");
        }

        if (subscription.allowOverAssignment) {
            document.querySelector('#aoaSelect .ms-Toggle-field').classList.add("is-selected");
        }

        document.getElementById('addNewButton').addEventListener("click", () => {
            DialogComponents.forEach(d => {
                if (d._dialog.attributes['id'].value === 'assignDialog') {
                    document.querySelector('#email ').value = "";
                    d.open();
                }
            });
        });

        document.getElementById('assignedusers').addEventListener('click', (ev) => {
            var target = ev.toElement || ev.target;
            var confirmElement = document.querySelector('#confirmationDialog span');
            if (target.classList.contains('remove-button')) {
                DialogComponents.forEach(d => {
                    if (d._dialog.attributes['id'].value === 'confirmationDialog') {
                        confirmElement.textContent = target.attributes["data-email"].value;
                        confirmElement.attributes["data-id"] = target.attributes["data-id"].value;
                        d.open();
                    }
                });
            }
        });

        document.getElementById('YesButton').addEventListener('click', async (ev) => {
            if (email) {
                const deleteUser = await fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}/licenses/${document.querySelector('#confirmationDialog span').attributes["data-id"]}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': token,
                        'Content-Type': 'application/json'
                    }
                });

                if (deleteUser.status === 204) {
                    await retrieveLicenses();
                }
            }
        });

        document.getElementById('assignButton').addEventListener("click", async () => {
            let email = document.getElementById('email').value.trim();
            let re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            if (re.test(email)) {
                const validateEmail = await fetch(`/common/GetUserByEmail?email=${email}`);
                if (validateEmail.status === 200) {
                    let user = await validateEmail.json();
                    let newUser = {
                        "id": "00000000-0000-0000-0000-000000000000",
                        "userId": user.id,
                        "userEmail": email,
                        "subscriptionId": subscriptionId
                    };

                    const postNewUser = await fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}/Licenses`, {
                        method: 'POST',
                        headers: {
                            'Authorization': token,
                            'Content-Type': 'application/json',
                            'mode': 'cors'
                        },
                        body: JSON.stringify(newUser)
                    });

                    await retrieveLicenses();
                }
            }
        });

        document.querySelector('#fcfsSelect .ms-Toggle-field').addEventListener("click", () => {
            const selValue = document.querySelector('#fcfsSelect .ms-Toggle-field').classList.contains('is-selected') ? true : false;
            subscription.firstComeFirstServedAssignment = selValue;
            fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': token,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(subscription)
            });
        });

        document.querySelector('#aoaSelect .ms-Toggle-field').addEventListener("click", () => {
            const selValue = document.querySelector('#aoaSelect .ms-Toggle-field').classList.contains('is-selected') ? true : false;
            subscription.allowOverAssignment = selValue;
            fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': token,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(subscription)
            });
        });
    }
    else {
        document.getElementById("siteBasedSection").style.display = "block";
    }
});