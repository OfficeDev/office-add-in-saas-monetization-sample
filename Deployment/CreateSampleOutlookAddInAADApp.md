# Create Sample Outlook Add-in AAD Application

1. Sign in to your Azure Account through the Azure portal.
1. Click **Azure Active Directory**.
1. Click **App registrations**.
1. Click **New registration**.
1. Name the application **Contoso Monetization Code Sample Outlook Addin Dev**. 
1. In the Supported account types section, select **Accounts in any organizational directory (Any Azure AD directory - Multitenant)**.
1. Click **Register**.
1. Click **API permissions**.
1. Click **Microsoft Graph**.
1. In the search box, enter the **name** of each permission in the list below, then check the **checkbox** next to it, and finally click **Update permissions**.

	**Note:** Here is an example of doing this with the **email** permission.

	![AAD Consent Permissions](./images/outlook-add-in-aad-app-01.png)
1. Repeat the process until all of the permissions in this list are added.
	- Files.Read.All
	- offline_access
	- openid
	- profile
	- User.Read
1. Click **Add a permission**.
1. Click **APIs my organization uses**.
1. In the list, select **Contoso Monetization Code Sample Web API Dev**. 
1. In the **What type of permissions does your application require?** section, select **Delegated permissions**.
1. Check the **checkbox** next to **user_impersonation**.
1. Click **Add permissions**. 

	![Permissions Added](./images/outlook-add-in-aad-app-02.png)