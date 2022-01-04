# Monetization Word Add-In

## Installation

1. Open the Monetization Resource Group in the Azure portal.

1. Copy the Word add-in website URL.

1. Download the latest manifest [Add-in Manifest xml files](WordAddInManifest.xml).

1. Open the add-in manifest XML file, replace **"https://WordAddinSiteUrl"** with the URL you copied in step 2, and then save the file.

1. In a web browser, open Word Online.

   ![image-NewMessage](Images/16.png)

1. Click **Insert -> Add-ins**.

1. Click **Manage My Add-ins -> Upload My Add-in**.

   ![image-NewMessage](Images/17.png)

1. Select the manifest file, and click **Upload**.

1. Click **Home -> ... **

   ![image-button](Images/18.png)

1. Click **SaaS Sample Word Addin**.

1. Click **Connect to Office 365**.

1. Click **Allow** and sign in to your account.

    ![image-openAddIn2](Images/20.png)

1. Use the admin account to sign in and consent.

    ![image-openAddIn3](Images/15.png)

1. View your license status.

    ![image-openAddIn3](Images/21.png)

1. Open the browser Developer console (F12) to view the DEBUG logs.

    ![image-openAddIn4](Images/14.png)
