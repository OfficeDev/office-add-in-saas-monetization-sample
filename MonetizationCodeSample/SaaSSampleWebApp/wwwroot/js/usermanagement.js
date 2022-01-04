// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

window.addEventListener('DOMContentLoaded', async () => {
    const token = await SaaSSampleWebApp.GetAccessToken();

    const retrieveAssignedUsers = async () => {
        const getLicenseManagers = await fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}/licensemanagers`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': token
            }
        });
        document.getElementById('assignedusers').innerHTML = "";
        if (getLicenseManagers.status === 200) {
            const licenseManagers = await getLicenseManagers.json();

            licenseManagers.forEach(user => {
                var tr = document.createElement('tr');
                tr.innerHTML = `<td>${user.userEmail}</td>
                                <td>${user.isAdmin ? "Admin" : "User"}</td>
                                <td><span class="ms-Link remove-button" data-id="${ user.userId}" data-email="${user.userEmail }">Remove</span></td>`;

                document.getElementById('assignedusers').appendChild(tr);
            });
        }
    };

    await retrieveAssignedUsers();

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
        if (target.classList.contains('remove-button')) {
            DialogComponents.forEach(d => {
                if (d._dialog.attributes['id'].value === 'confirmationDialog') {
                    var confirmElement = document.querySelector('#confirmationDialog span');
                    confirmElement.textContent = target.attributes["data-email"].value;
                    confirmElement.attributes["data-id"] = target.attributes["data-id"].value;
                    d.open();
                }
            });
        }
    });

    document.getElementById('YesButton').addEventListener('click', async (ev) => {
        const deleteUser = await fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}/licensemanagers/${document.querySelector('#confirmationDialog span').attributes["data-id"]}`, {
            method: 'DELETE',
            headers: {
                'Authorization': token,
                'Content-Type': 'application/json'
            }
        });

        if (deleteUser.status === 204) {
            await retrieveAssignedUsers();
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
                    "subscriptionId": subscriptionId,
                    "isAdmin": document.querySelector('#adminSelect .ms-Toggle-field').classList.contains('is-selected')
                };

                const postNewUser = await fetch(`${apiRootEndpoint}/subscriptions/${subscriptionId}/licensemanagers`, {
                    method: 'POST',
                    headers: {
                        'Authorization': token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(newUser)
                });

                if (postNewUser.status === 201) {
                    await retrieveAssignedUsers();
                }
            }
        }
    });

});