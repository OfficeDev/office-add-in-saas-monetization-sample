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

Write-Host "Completed!" -ForegroundColor Green