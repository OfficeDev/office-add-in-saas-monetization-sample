# Monetization Provider-Hosted SharePoint Add-In

## Installation

1. Open the **MonetizationCodeSample** solution with Visual Studio.

2. Select the **ProviderHostedAddIn** project, right click it, and select **Publish**.

3. Click **Package the add-in**.

   ![image-openAddIn4](Images/36.png)

4. Enter the Provider Hosted add-in web site **URL** and **Add-in ID**, then click **Finish** button.  This will create package file named **ProviderHostedAddIn.app**.

   ![image-openAddIn4](Images/37.png)

5. Go to the SharePoint App Catalog site, and click **Apps for SharePoint**.

6. Upload the **ProviderHostedAddIn.app** file.

7. Go to the Sharepoint site collection that you registered your SharePoint Add-in.

8. Open **Site contents**, click **+ New** -> **App**

9. Select **SaaSSampleProviderHostedAddin** and add it.

   ![image-openAddIn4](Images/38.png)

10. Click the app, and it will go to the Provier Hosted web site.

    ![image-openAddIn4](Images/39.png)

11. Click **Connect to Office 365**.

12. Use the admin account to sign in and consent. 

    ![image-openAddIn3](Images/15.png)

13. View your license status.

    ![image-openAddIn3](Images/40.png)

14. Open the browser Developer console (F12) to view the DEBUG logs.

![image-openAddIn4](Images/27.png)