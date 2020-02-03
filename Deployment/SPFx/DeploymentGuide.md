#                    Monetization SPFX

1.  Download the latest SPFX [**saas-sample-web-part.sppkg**](saas-sample-web-part.sppkg) package file.

2.   Open your SharePoint Catalog  https://cand3.sharepoint.com/sites/AppCatelog/AppCatalog/Forms/AllItems.aspx, upload SPFX package file to SharePoint Catalog.

   ![image-Catalog](Images/1.png)

3. Select Checkbox and click Deploy button.

   ![image-Deploy](Images/2.png)

4. Select **saas-sample-web-part** file, then select **Sync to Teams** menu.

   ![image-Deploy](Images/12.png)

5. Go to SharePoint Admin Center.

6. Open **API management** and approve the following two permission.

   ![image-Permission](Images/3.png)

7. Approve successfully.

   ![image-Approve](Images/4.png)

8. Open SharePoint site a page and edit this page.

9. Add SaaSSampleWebPart into this page.

   ![image-UploadWebpart](Images/5.png)

10. Edit this Webpart, click Configure button.

   ![image-ConfigureWebpart](Images/6.png)

11. Copy Contoso Monetization Code Sample Web API Client Id and paste it  

    ![image-ConfigureWebpart1](Images/7.png)

    ![image-ConfigureWebpart2](Images/8.png)

12. Copy Contoso Monetization Code Sample Web API URL

    ![image-ConfigureWebpart3](Images/9.png)

13. Replace WebAPI URL using URAL was copied above, then past it SaaS Web API Text Field.

    [**https://YOURContosoMonetizationCodeSampleWebAPIURL**/api/Subscriptions/CheckOrActivateLicense/contoso_o365_addin](https://YOURContosoMonetizationCodeSampleWebAPIURL/api/Subscriptions/CheckOrActivateLicense/contoso_o365_addin)

    ![image-ConfigureWebpart4](Images/10.png)

14. Save configure and stop editing.

15. Return to SharePoint page and check SPFX.

    ![image-ConfigureWebpart5](Images/11.png)