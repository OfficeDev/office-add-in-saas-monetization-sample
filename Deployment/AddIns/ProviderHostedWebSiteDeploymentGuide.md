# Monetization Provider-Hosted Web Site

## Installation

1. Register your Provider-Hosted SharePoint add-in in Azure ACS by using the AppRegNew.aspx page, and retrieve the registration **Add-in ID** and **Add-in Secret** information. See [Register SharePoint Add-ins](https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/register-sharepoint-add-ins) for more information.

   ![](Images/35.png)

2. Open the Monetization Resource Group in the Azure portal.

3. Copy the Provider-Hosted Website URL.

   ![](Images/1.png)

4. Open the **MonetizationCodeSample** solution with Visual Studio.

5. In the **ProviderHostedAddInWeb** project, open the **Web.config** file.

6. Replace the **ClientId/ClientSecret** values with the **Add-in ID** and **Add-in Secret** values that you retrieved in step 1.

   ![](Images/33.png)

7. Select the **ProviderHostedAddInWeb** project, right click it, and select **Publish**.

   **Note:**
   If you encounter this error:

   "NuGet Package restore failed for project ProviderHostedAddInWeb: Unable to find version '4.0.0' of package 'AppForSharePointOnlineWebToolkit'.  C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\: Package 'AppForSharePointOnlineWebToolkit.4.0.0' is not found on source 'C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\'.  https://api.nuget.org/v3/index.json: Package 'AppForSharePointOnlineWebToolkit.4.0.0' is not found on source 'https://api.nuget.org/v3/index.json'.. Please see Error List window for detailed warnings and errors."

   Then, update the NuGet package Sources and add this package location: C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\Microsoft\SharePoint\NuGet Packages\ 

> ![](Images/46.png)

8. Select **Existing Service**, then click **Create Profile**.

   ![](Images/31.png)

9. Select the Provider Hosted add-in web site, and click **OK**.

   ![](Images/32.png)

10. Click **Publish**.  The Provider Hosted SharePoint add-in web site will be published successfully.