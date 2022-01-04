# Monetization PowerPoint Add-In

## Installation

1. Open the Monetization Resource Group in the Azure portal.

1. Copy the PowerPoint add-in website URL.

1. Download the latest manifest [Add-in Manifest xml files](PowerPointAddInManifest.xml).

1. Open the add-in manifest XML file, replace **"https://PowerPointAddinSiteUrl"** with the URL you copied in step 2, and then save the file.

1. In a web browser, open PowerPoint Online.

   ![image-NewMessage](Images/28.png)

1. Click **Insert -> Add-ins**.

1. Click **Manage My Add-ins -> Upload My Add-in**.

   ![image-NewMessage](Images/17.png)

1. Select the manifest file, and click **Upload**.

1. Click **SaaS Sample PowerPoint Addin**.

   ![image-button](Images/29.png)

1. Click **Connect to Office 365**.

1. Click **Allow** and sign in to your account.

    ![image-openAddIn2](Images/30.png)

1. Use the admin account to sign in and consent.

    ![image-openAddIn3](Images/15.png)

1. View your license status.

    ![image-openAddIn3](Images/24.png)

1. Open the browser Developer console (F12) to view the DEBUG logs.

    ![image-openAddIn4](Images/27.png)
