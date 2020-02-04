# Monetization SPFx Add-in

# Installation

1.  Download the SPFx Add-in [**saas-sample-web-part.sppkg**](saas-sample-web-part.sppkg) package file.

2.   Open your SharePoint Catalog then upload the SPFx package file.

   ![image-Catalog](Images/1.png)

3. Select the **checkbox** and click **Deploy**.

   ![image-Deploy](Images/2.png)

4. Select the **saas-sample-web-part** file, then click **Sync to Teams** in the Ribbon menu.

   ![image-Deploy](Images/12.png)

5. Go to the SharePoint Admin Center.

6. Open the **API management** and approve the following two permissions.

   ![image-Permission](Images/3.png)

7. Successfully approved.

   ![image-Approve](Images/4.png)

8. Open a SharePoint site page and edit the page.

9. Add the SaaSSampleWebPart to the page.

   ![image-UploadWebpart](Images/5.png)

10. Edit the web part, and click **Configure**.

   ![image-ConfigureWebpart](Images/6.png)

11. Copy Contoso Monetization Code Sample Web API Client Id and paste it into the web part.

    ![image-ConfigureWebpart1](Images/7.png)

    ![image-ConfigureWebpart2](Images/8.png)

12. Copy Contoso Monetization Code Sample Web API URL

    ![image-ConfigureWebpart3](Images/9.png)

13. Replace the WebAPI URL copied above, then paste it into the SaaS Web API Text Field.  The following example shows how to replace it.

    [**https://YOURContosoMonetizationCodeSampleWebAPIURL**/api/Subscriptions/CheckOrActivateLicense/contoso_o365_addin](https://YOURContosoMonetizationCodeSampleWebAPIURL/api/Subscriptions/CheckOrActivateLicense/contoso_o365_addin)

    ![image-ConfigureWebpart4](Images/10.png)

14. Save the configuration and stop editing the page.

15. Observe the license status in the web part.

    ![image-ConfigureWebpart5](Images/11.png)
