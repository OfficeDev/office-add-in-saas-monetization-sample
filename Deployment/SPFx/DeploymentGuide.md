# Monetization SPFx

## Installation

1. In the Azure Portal, copy the Contoso Monetization Code Sample Web API Application **Display Name** and **Application ID** and paste it into a text file. You will need them in subsequent steps.

   ![image-Deploy](Images/13.png)

1. Copy the Contoso Monetization Code Sample Web API **URL**  and paste it into a text file. You will need it in subsequent steps.

   ![image-ConfigureWebpart3](Images/9.png)

1. Download the [SPFx source code](../../MonetizationCodeSample/SPFX), and open it using Visual Studio Code.

   ![image-Deploy](Images/14.png)

1. Open the **package-solution.json** file in the config folder, then paste the Contoso Monetization Code Sample Web API Display Name that you copied in step 1 into the resource value on line 12. **Save** the file.

   ![image-Deploy](Images/15.png)

1. Open the **SaaSSampleWebPartWebPart.manifest.json** file.

   ![image-Deploy](Images/16.png)

1. Paste the Contoso Monetization Code Sample Web API **Application ID** that you copied in step 1 into the **clientId** value on line 25.

   ![image-Deploy](Images/17.png)

1. Paste Contoso Monetization Code Sample Web API **URL** that you copied in step 2 into the **sassWebApi** value on line 25.

   >**Note:** You are only replacing a portion of the URL.  Refer to the picture below to see which portion of the URL to replace.**

   **Save** the file.

   ![image-Deploy](Images/18.png)

1. Execute the following commands in the terminal to build, bundle, and package the SPFx Add-in.

   ``` command
   npm install
   
   gulp build
   
   gulp bundle --ship
   
   gulp package-solution --ship
   ```

1. Open the folder the packaged file was generated in and located the **saas-sample-web-part.sppkg** file.  Note the file path to the package file, you will need it in the next step.

   ![image-Deploy](Images/19.png)

1. Open your SharePoint App Catalog and upload the SPFx package file.

   ![image-Catalog](Images/1.png)

1. Select the **checkbox** and click **Deploy**.

    ![image-Deploy](Images/2.png)

1. Select the **saas-sample-web-part-client-side-solution** Add-in, then in the Ribbon, click **Sync to Teams**.

    ![image-Deploy](Images/12.png)

1. Go to the SharePoint Admin Center.

1. Open **API management** and approve the following two permissions.

   ![image-Permission](Images/3.png)

1. Approve successfully.

   ![image-Approve](Images/4.png)

1. Go to a SharePoint site and edit a page. Add the SaaSSampleWebPart to the page.

   ![image-UploadWebpart](Images/5.png)

1. Save the page and observe the license status in the web part and developer console.

   ![image-ConfigureWebpart5](Images/11.png)
