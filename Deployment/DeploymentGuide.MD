# Installation Guide

## Prerequisites

- Install the following PowerShell modules
   - [PowerShell 6](https://github.com/PowerShell/PowerShell/releases/tag/v6.2.3)
   - [Microsoft Graph PowerShell SDK](https://github.com/microsoftgraph/msgraph-sdk-powershell)
   - [Azure](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-3.1.0)
   - Install [.NET Core 3.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0) and a text editor like Visual Studio or [Visual Studio Code](https://code.visualstudio.com)
- Azure Subscription and an Admin account

## Create the AAD Applications

This sample uses 4 Azure Active Directory Applications.  Refer to the four documents below to create them.

1. [Create Sample App Source AAD App](CreateSampleAppSourceAADApp.md)
1. [Create Sample Web App AAD App](CreateSampleWebAppAADApp.md)
1. [Create Sample Outlook Add-in AAD App](CreateSampleOutlookAddInAADApp.md)
1. [Create Sample Web API AAD App](CreateSampleWebAPIAADApp.md) - The other AAD Apps must be created before this one may be created and configured. 

## Grant consent for the Sample Web App AAD Application

1. Click **Azure Active Directory**.
1. Click **App registrations**.
1. Click the Sample Web App AAD Application you created. 
1. Click **Grand admin consent for YOUR TENANT NAME**.
1. Click **Yes**.

When you are finished, the screen will indicate all permissions have been granted.

![Permissions Granted](./images/web-app-aad-app-01.png)

## Grant consent for the Sample Outlook Add-in AAD Application

1. Click **Azure Active Directory**.
1. Click **App registrations**.
1. Click the Sample Outlook Add-in AAD Application you created. 
1. Click **Grand admin consent for YOUR TENANT NAME**.
1. Click **Yes**.

When you are finished, the screen will indicate all permissions have been granted.

![Permissions Granted](./images/outlook-add-in-aad-app-03.png)

## Deploy the ARM Template with PowerShell

1. Open the `ARMParameters.json` file and update the following values using the corresponding values from the AAD applications you created in steps 1-4 above. These values can be found on the Overview page for each AAD application in the Azure Portal.

   ![Code Path](Images/6.png)

2. Run the Powershell command `Connect-AzAccount`.

   ![Code Path](Images/7.png)

3. Run the script `.\DeployTemplate.ps1`. When prompted, enter the name of the resource group to create.

   ![Code Path](Images/8.png)

4. Deploy ARM Template successfully.

   ![Code Path](Images/9.png)   

## Compile and deploy the server code

1. In the command line, change to the **.\MonetizationCodeSample** directory.

1. Run the script `.\PublishWebapp.ps1`. 
   
1. When prompted, enter the same resource group name and webAppSiteName,webApiSiteName,outlookAddInWebSiteName,resourceMockWebSiteName you entered in `ARMParameters.json` in previous steps.
   
1. Deploy code successfully.
   
   ![Code Path](Images/10.png)

   

## Update AAD Application

1. Change back to the **.\Deployment** directory.

1. Run the script `.\UpdateApps.ps1 -armConfig .\ARMParameters.json -newAppsConfig .\NewApps.json ` 

   ![Code Path](Images/11.png)

1. Now we have completed all deployment.



## Clean up

If you want to remove all applications and sites, you can execute the following steps.

1. Change back to the **.\Deployment** directory.
2. Run the Powershell command `Remove-AzResourceGroup -name <resource group name>` and pay attention to replacing **&lt;resource group name&gt;** with your resource group name

3. Change to the **.\Deployment** directory.
   -Run the script `.\RemoveApps.ps1 -config .\NewApps.json`

