#                    Monetization OutLook AddIn

1.  Open Monetization group on Azure Portal.

2. Copy Outlook AddIn Web Site URL.

   ![image-AddInWebSiteUrl](Images/1.png)

3. Download the latest Assembler [Add-in Manifest xml files](OutlookWebAddInManifest.xml).

4. Open Add-in Manifest xml file, and replace https://contosomonetizationoutlookaddin.azurewebsites.net using URL that you copied on step 2 above, then save xml file.

5. Open mailbox https://outlook.office.com/mail/inbox and login your account

6. Click **New Message** button

   ![image-NewMessage](Images/2.png)

7. Click **…** button

   ![image-button](Images/3.png)

8. Select **Get Add-ins** button.

   ![image-addinsbutton](Images/4.png)

9. Select **My add-ins** Tab.

   ![image-addinsbutton](Images/5.png)

10. Select **Add a custom add-in->Add from file**

    ![image-addfromfile](Images/6.png)

11. Select **Install** button

    ![image-install](Images/7.png)

12. We can find the Add-In after install successfully.

    ![image-install1](Images/8.png)

13. Return new message edit view and click **…** button.

    ![image-newemail](Images/9.png)

14. Click **Sass Sample Outlook Addin**

    ![image-openAddIn](Images/10.png)

15. Click **Connect to Office 365**

    ![image-openAddIn1](Images/11.png)

16. Click **Allow** button and login your account.

    ![image-openAddIn2](Images/12.png)

17. You can find you don’t have a paid license.

    ![image-openAddIn3](Images/13.png)

18. Open browser console, you can find the following DEBUG logs.

    ![image-openAddIn4](Images/14.png)