# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [String]$armConfig = (Read-Host -Prompt "Enter the ARMParameters.json file"),
    [Parameter(Mandatory)]
    [String]$newAppsConfig = (Read-Host -Prompt "Enter the NewApps.json file")
)

$armConfigJson = Get-Content -Path $armConfig |Out-String|ConvertFrom-Json
$newAppsConfigJson = Get-Content -Path $newAppsConfig |Out-String|ConvertFrom-Json

$webApp = $newAppsConfigJson.apps|Where-Object {$_.appName -eq "webApp"}
$webAppDisplayName = $webApp.bodyParameter.displayName
Write-Host "Updating $webAppDisplayName redirectUris..." -ForegroundColor Green

$webApp = Get-MgApplication -top 400|Where-Object {$_.DisplayName -eq $webAppDisplayName}
$webAppSiteName = $armConfigJson.parameters.webAppSiteName.value
$redirectUris = "https://$webAppSiteName.azurewebsites.net/signin-oidc","https://$webAppSiteName.azurewebsites.net/" 
Update-MgApplication -ApplicationId $webApp.Id -WebRedirectUris $redirectUris

$appResource = $newAppsConfigJson.apps|Where-Object {$_.appName -eq "appResource"}
$appResourceNameDisplayName = $appResource.bodyParameter.displayName
Write-Host "Updating $appResourceNameDisplayName redirectUris..." -ForegroundColor Green
$appResource = Get-MgApplication -top 400|Where-Object {$_.DisplayName -eq $appResourceNameDisplayName}
$resourceMockWebSiteName = $armConfigJson.parameters.resourceMockWebSiteName.value
$redirectUris = "https://$resourceMockWebSiteName.azurewebsites.net/signin-oidc","https://$resourceMockWebSiteName.azurewebsites.net/" 
Update-MgApplication -ApplicationId $appResource.Id -WebRedirectUris $redirectUris

$outlookAddin = $newAppsConfigJson.apps|Where-Object {$_.appName -eq "outlookAddin"}
$outlookAddinDisplayName = $outlookAddin.bodyParameter.displayName
Write-Host "Updating $outlookAddinDisplayName redirectUris..." -ForegroundColor Green
$outlookAddin = Get-MgApplication -top 400|Where-Object {$_.DisplayName -eq $outlookAddinDisplayName}
$outlookAddInWebSiteName = $armConfigJson.parameters.outlookAddInWebSiteName.value
$redirectUris = "https://$outlookAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize"
Update-MgApplication -ApplicationId $outlookAddin.Id -WebRedirectUris $redirectUris
Write-Host "Completed!" -ForegroundColor Green

