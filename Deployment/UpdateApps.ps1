# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [String]$armConfig = (Read-Host -Prompt "Enter the ARMParameters.json file")
)

$armConfigJson = Get-Content -Path $armConfig |Out-String|ConvertFrom-Json

$webApp = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.webAppClientId.value}
$webAppSiteName = $armConfigJson.parameters.webAppSiteName.value
Write-Host "Updating $webAppSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$webAppSiteName.azurewebsites.net/signin-oidc","https://$webAppSiteName.azurewebsites.net/","https://$webAppSiteName.azurewebsites.net/home/SPHostedAddinEmbedContent"
Update-MgApplication -ApplicationId $webApp.Id -WebRedirectUris $redirectUris

$appResource = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.sourceMockClientId.value}
$resourceMockWebSiteName = $armConfigJson.parameters.resourceMockWebSiteName.value
Write-Host "Updating $resourceMockWebSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$resourceMockWebSiteName.azurewebsites.net/signin-oidc","https://$resourceMockWebSiteName.azurewebsites.net/" 
Update-MgApplication -ApplicationId $appResource.Id -WebRedirectUris $redirectUris

$outlookAddIn = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.outlookAddInClientId.value}
$outlookAddInWebSiteName = $armConfigJson.parameters.outlookAddInWebSiteName.value
Write-Host "Updating $outlookAddInWebSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$outlookAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize"
Update-MgApplication -ApplicationId $outlookAddIn.Id -WebRedirectUris $redirectUris

$wordAddIn = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.wordAddInClientId.value}
$wordAddInWebSiteName = $armConfigJson.parameters.wordAddInWebSiteName.value
Write-Host "Updating $wordAddInWebSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$wordAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize"
Update-MgApplication -ApplicationId $wordAddIn.Id -WebRedirectUris $redirectUris

$excelAddIn = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.excelAddInClientId.value}
$excelAddInWebSiteName = $armConfigJson.parameters.excelAddInWebSiteName.value
Write-Host "Updating $excelAddInWebSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$excelAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize"
Update-MgApplication -ApplicationId $excelAddIn.Id -WebRedirectUris $redirectUris

$powerpointAddIn = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.powerpointAddInClientId.value}
$powerpointAddInWebSiteName = $armConfigJson.parameters.powerpointAddInWebSiteName.value
Write-Host "Updating $powerpointAddInWebSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$powerpointAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize"
Update-MgApplication -ApplicationId $powerpointAddIn.Id -WebRedirectUris $redirectUris

$providerHostedAddIn = Get-MgApplication -top 400|Where-Object {$_.AppId -eq $armConfigJson.parameters.providerHostedAddInClientId.value}
$providerHostedAddInWebSiteName = $armConfigJson.parameters.providerHostedAddInWebSiteName.value
Write-Host "Updating $providerHostedAddInWebSiteName redirectUris..." -ForegroundColor Green
$redirectUris = "https://$providerHostedAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize"
Update-MgApplication -ApplicationId $providerHostedAddIn.Id -WebRedirectUris $redirectUris

Write-Host "Completed!" -ForegroundColor Green