// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

var provisionButtonElem = document.getElementById('provisionButton');

window.addEventListener('DOMContentLoaded', async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const authCode = urlParams.get('token');
    if (authCode) {
        const token = await SaaSSampleWebApp.GetAccessToken();

        var formData = new FormData();
        formData.append('AuthCode', authCode);

        const resolveAuthCode = fetch(`${apiRootEndpoint}/subscriptions/resolve`, {
            method: 'POST',
            headers: {
                'Authorization': token
            },
            body: formData
        });

        const response = await resolveAuthCode;
        if (response.status === 200) {
            var resolvedResult = await response.json();

            provisionButtonElem.disabled = false;

            provisionButtonElem.addEventListener('click', async () => {
                provisionButtonElem.disabled = true;
                const tenantInfo = await (await fetch('/Common/GetTenantInfo')).json();
                const token = await SaaSSampleWebApp.GetAccessToken();

                resolvedResult.location = document.getElementById('location').selectedOptions[0].innerText;
                resolvedResult.tenantName = tenantInfo.tenantName;

                const postSubscription = await fetch(`${apiRootEndpoint}/subscriptions`, {
                    method: 'POST',
                    headers: {
                        'Authorization': token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(resolvedResult)
                });

                const subscription = await postSubscription.json();

                if (postSubscription.status === 201) {
                    const activteSubscription = await fetch(`${apiRootEndpoint}/subscriptions/${subscription.id}/activate`, {
                        method: 'POST',
                        headers: {
                            'Authorization': token
                        }
                    });
                    if (activteSubscription.status === 200) {
                        location.href = "/";
                    }
                }
            });
        }
    }
});