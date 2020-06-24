# Monetization PowerPoint Add-In

## Installation

1. Open the Monetization Resource Group in the Azure portal.

2. Copy the PowerPoint add-in website URL.

   ![image-AddInWebSiteUrl](Images/1.png)

3. Download the latest manifest [Add-in Manifest xml files](PowerPointAddInManifest.xml).

4. Open the add-in manifest XML file, replace **https://PowerPointAddinSiteUrl** with the URL you copied in step 2, and then save the file.

5. In a web browser, open PowerPoint Online.

   ![image-NewMessage](Images/28.png)

6. Click **Insert -> Add-ins**.

7. Click **Manage My Add-ins -> Upload My Add-in**.

   ![image-NewMessage](Images/17.png)

8. Select the manifest file, and click **Upload**.

9. Click **SaaS Sample PowerPoint Addin**.

   ![image-button](Images/29.png)

11. Click **Connect to Office 365**.

12. Click **Allow** and sign in to your account.

    ![image-openAddIn2](Images/30.png)

13. Use the admin account to sign in and consent. 

    ![image-openAddIn3](Images/15.png)

14. View your license status.

    ![image-openAddIn3](Images/24.png)

15. Open the browser Developer console (F12) to view the DEBUG logs.

    ![image-openAddIn4](Images/27.png)
