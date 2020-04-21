---
page_type: sample
products:
- office-outlook
- office-365
- office-sp
languages:
- javascript
extensions:
  contentType: samples
  technologies:
  - Add-ins
  createdDate: 2/14/2020 12:00:00 PM
description: "Learn how to create a simple license management system to manage add-ins sold in Microsoft AppSource"
---

# Introduction 
This code sample demonstrates how Microsoft ISVs can create a simple license management system to manage add-ins sold in Microsoft AppSource. This code sample package includes a Microsoft AppSource mock web app, a SaaS sample, a SharePoint Framework (SPFx) add-in, an Outlook add-in, and a licensing management tool. 

## Installation and configuration
Follow the instructions in the deployment guides to install and configure the sample.

- [Sample deployment guide](/Deployment/DeploymentGuide.MD)
- [SPFx add-in deployment guide](/Deployment/SPFx/DeploymentGuide.md)
- Office add-ins deployment guides:
   -  [Outlook add-in deployment guide](/Deployment/AddIn/OutlookAddInDeploymentGuide.md)
   -  [Word add-in deployment guide](/Deployment/AddIn/WordAddInDeploymentGuide.md)
   -  [Excel add-in deployment guide](/Deployment/AddIn/ExcelAddInDeploymentGuide.md)
   -  [PowerPoint add-in deployment guide](/Deployment/AddIn/PowerPointAddInDeploymentGuide.md)

## Testing
Follow the instructions in the test guide to test the different use cases.

1. [Test guide](/Test/TestGuide.md)

## Inventory
This section links to the various README files associated with the projects included in the sample.  Read the README files for more information about each project.

- [AppSourceMockWebApp](/MonetizationCodeSample/AppSourceMockWebApp/README.MD)
- [Microsoft.Identity.Web](/MonetizationCodeSample/Microsoft.Identity.Web/README.md)
- [MockAppData](/MonetizationCodeSample/MockAppData/README.MD)
- [OutlookAddIn](/MonetizationCodeSample/OutlookAddIn/README.MD)
- [OutlookAddInWeb](/MonetizationCodeSample/OutlookAddInWeb/README.MD)
- [WordAddIn](/MonetizationCodeSample/WordAddIn/README.MD)
- [WordAddinWeb](/MonetizationCodeSample/WordAddinWeb/README.MD)
- [ExcelAddIn](/MonetizationCodeSample/ExcelAddIn/README.MD)
- [ExcelAddinWeb](/MonetizationCodeSample/ExcelAddinWeb/README.MD)
- [PowerPointAddIn](/MonetizationCodeSample/PowerPointAddIn/README.MD)
- [PowerPointAddinWeb](/MonetizationCodeSample/PowerPointAddinWeb/README.MD)
- [SaaSSampleWebApi](/MonetizationCodeSample/SaaSSampleWebApi/README.MD)
- [SaaSSampleWebApp](/MonetizationCodeSample/SaaSSampleWebApp/README.MD)
- [SPFXAddIn](/MonetizationCodeSample/SPFXAddIn/README.md)

## Appendix

### UX / API Mapping

This matrix describes the UI actions and how they correlate to different API calls.

| UI where the API is invoked  | API call                                                                                                                                                                                                                                                                                                                                                                    | Mock API exists | Mock API works | Able to test the mock API without the offer being published to the public? | Able to test the production API without the offer being published to the public? |
|------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------|----------------|---------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| ![](images/contoso_app_purchase.png)     | Resolve a subscription in purchase process: <br/><br/>HttpPost<br/> https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version=2018-09-15                                                                                                                                                                                                              | Yes             | Yes            | Yes                                                                       | No                                                                              |
| ![](images/contoso_app_purchase.png)     | Activate a subscription in purchase process:<br/><br/>HttpPost<br/> https://marketplaceapi.microsoft.com/api/saas/subscriptions/0aa95e32-8be7-4e5e-94f9-563f6d7d9dcd/activate?api-version=2018-09-15                                                                                                                                                                        | Yes             | Yes            | Yes                                                                       | No                                                                              |
| ![](images/contoso_app_quantity.png)     | Update the quantity on the subscription in plan/quantity changed process<br/><br/>HttpPatch<br/> https://marketplaceapi.microsoft.com/api/saas/subscriptions/0aa95e32-8be7-4e5e-94f9-563f6d7d9dcd?api-version=2018-09-15 <br/><br/> Note: Although the patch request works, but actually the test data will not be updated, and the web hook will not be triggered as well. | Yes             | Yes            | No                                                                        | No                                                                              |
| ![](images/contoso_app_quantity.png)     | Update the status of an operation in plan/quantity changed process<br/><br/>HttpPatch<br/> https://marketplaceapi.microsoft.com/api/saas/subscriptions/0aa95e32-8be7-4e5e-94f9-563f6d7d9dcd/operations/7688ae05-1579-4fdd-be89-12b45f0a4ef3?api-version=2018-09-15                                                                                                          | Yes             | Yes            | No                                                                        | No                                                                              |
| ![](images/contoso_app_quantity.png)     | Query operations in plan/quantity changed process<br/><br/>HttpPatch<br/> https://marketplaceapi.microsoft.com/api/saas/subscriptions/0aa95e32-8be7-4e5e-94f9-563f6d7d9dcd/operations?api-version=2018-09-15                                                                                                                                                                | Yes             | Yes            | Yes                                                                       | No                                                                              |
| ![](images/contoso_app_quantity.png)     | Webhook on the SaaS service                                                                                                                                                                                                                                                                                                                                                 | No              | No             | No                                                                        | No                                                                              |

## Copyright

Copyright (c) 2020 Microsoft Corporation. All rights reserved.
