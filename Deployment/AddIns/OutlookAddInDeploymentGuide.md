# Monetization Outlook Add-In

## Installation

1. Open the Monetization Resource Group in the Azure portal.

2. Copy the Outlook add-in website URL.

   ![image-AddInWebSiteUrl](Images/1.png)

3. Download the latest manifest [Add-in Manifest xml files](OutlookAddInManifest.xml).

4. Open the add-in manifest XML file, replace **https://OutlookAddinSiteUrl** with the URL you copied in step 2, and then save the file.

5. In a web browser, go to https://outlook.office.com/mail/inbox and sign in to your account.

6. Click **New Message**.

   ![image-NewMessage](Images/2.png)

7. Click **…**.

   ![image-button](Images/3.png)

8. Click **Get Add-ins**.

   ![image-addinsbutton](Images/4.png)

9. Click the **My add-ins** tab.

   ![image-addinsbutton](Images/5.png)

10. Click **Add a custom add-in->Add from file**.

    ![image-addfromfile](Images/6.png)

11. Click **Install**.

    ![image-install](Images/7.png)

12. The add-in after a successful install.

    ![image-install1](Images/8.png)

13. Return to the new message edit view and click **…**.

    ![image-newemail](Images/9.png)

14. Click **Sass Sample Outlook Addin**.

    ![image-openAddIn](Images/10.png)

15. Click **Connect to Office 365**.

16. Click **Allow** and sign in to your account.

    ![image-openAddIn2](Images/12.png)

17. Use the admin account to sign in and consent.

    ![image-openAddIn3](Images/15.png)

18. View your license status.

    ![image-openAddIn3](Images/13.png)

19. Open the browser Developer console (F12) to view the DEBUG logs.

    ![image-openAddIn4](Images/27.png)
