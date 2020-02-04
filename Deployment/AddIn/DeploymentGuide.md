# Monetization Outlook Add-In

## Installation

1. Open the Monetization Resource Group in the Azure Portal.

2. Copy the Outlook Add-In Web Site URL.

   ![image-AddInWebSiteUrl](Images/1.png)

3. Download the latest Manifest [Add-in Manifest xml files](OutlookWebAddInManifest.xml).

4. Open the Add-in Manifest xml file, and replace https://contosomonetizationoutlookaddin.azurewebsites.net with the URL you copied on step 2 above, then save the file.

5. In a web browswer, go to https://outlook.office.com/mail/inbox and login to your account.

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

12. The Add-In after a successfull install.

    ![image-install1](Images/8.png)

13. Return to the new message edit view and click **…**.

    ![image-newemail](Images/9.png)

14. Click **Sass Sample Outlook Addin**.

    ![image-openAddIn](Images/10.png)

15. Click **Connect to Office 365**.

    ![image-openAddIn1](Images/11.png)

16. Click **Allow** and login to your account.

    ![image-openAddIn2](Images/12.png)

17. Observe your license status.

    ![image-openAddIn3](Images/13.png)

18. Open the browser Developer console (F12), to view the DEBUG logs.

    ![image-openAddIn4](Images/14.png)
