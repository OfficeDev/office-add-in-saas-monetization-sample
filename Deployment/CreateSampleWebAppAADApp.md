# Create sample web app Azure AD application

1. Sign in to your Azure account through the Azure portal.
1. Click **Azure Active Directory**.
1. Click **App registrations**.
1. Click **New registration**.
1. Name the application **Contoso Monetization Code Sample Web App Dev**. 
1. In the Supported account types section, select **Accounts in any organizational directory (Any Azure AD directory - Multitenant)**.
1. Click **Register**.
1. Click **Add a Redirect URI**.
1. Enter the following web redirect URIs:

	`https://localhost:44381/signin-oidc`
	`https://localhost:44381/`

1. Click **Save**.
1. In the **Implicit grant** section, check the **ID tokens** and **Access tokens** checkboxes.
1. Click **Save**.
1. Click **API permissions**.
1. Click **Microsoft Graph**.
1. In the search box, enter **Directory.AccessAsUser.All**.
1. Check the **checkbox** next to the search result.
1. Click **Update permissions**.
1. Click **Add a permission**.
1. Click **APIs my organization uses**.
1. In the list, select **Contoso Monetization Code Sample Web API Dev**. 
1. In the **What type of permissions does your application require?** section, select **Delegated permissions**.
1. Check the **checkbox** next to **user_impersonation**.
1. Click **Add permissions**. 