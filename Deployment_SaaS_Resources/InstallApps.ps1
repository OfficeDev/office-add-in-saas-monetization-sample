# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [String]$appsConfig = ".\AadApps.config.json",
    [String]$armConfig = ".\ARMParameters.json"
  

)

$appsJson = Get-Content -Path $appsConfig |Out-String|ConvertFrom-Json -AsHashtable
$armConfigJson= Get-Content -Path $armConfig |Out-String|ConvertFrom-Json -AsHashtable

$result = @{}

foreach ($app in $appsJson.apps)
{
    $newApp = New-MgApplication -DisplayName $app.bodyParameter.displayName `
                             -RequiredResourceAccess $app.bodyParameter.requiredResourceAccess `
                             -SignInAudience $app.bodyParameter.signInAudience `
                             -Web:$app.bodyParameter.Web `
                             -Api:$app.bodyParameter.Api

    
    if ($app.passwordCredential) {
        Start-Sleep -s 3
        $password = Add-MgApplicationPassword -ApplicationId $newApp.Id -BodyParameter $app.passwordCredential
    }
    Start-Sleep -s 3

    $newAppSP = New-MgServicePrincipal -AppId $newApp.AppId

    $result.Add($app.appName, @{"id" = $newApp.Id; "appId"=$newApp.AppId; "spId"=$newAppSP.Id; "appSecret"=$password.SecretText; "requiredResourceAccess"=$app.bodyParameter.requiredResourceAccess})

    Write-Host $newApp.DisplayName "application is created."
}

#Expose webApi to apps
$webApiConfig = $appsJson.apps |Where-Object {$_.appName -eq "webApi"}
$webApiPermissionId = $webApiConfig.bodyParameter.Api.Oauth2PermissionScopes.Id
$preAuthorizedWebApi1=@{AppId=$result.webApp.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApps = @($preAuthorizedWebApi1);

$api = @{ 
    "PreAuthorizedApplications"=$preAuthorizedWebApps;
    "KnownClientApplications"=@($result.webApp.appId)
}

Update-MgApplication -ApplicationId $result.webApi.id -Api $api -IdentifierUris "api://$($result.webApi.appId)"
$newWebApi = Get-MgApplication -ApplicationId $result.webApi.id
$result.webApi.identifierUris = $newWebApi.IdentifierUris

$exposedResource = @{
    "resourceAppId"=$result.webApi.appId;
    "resourceAccess"= @(@{"Id"= $webApiConfig.bodyParameter.Api.Oauth2PermissionScopes.Id ; "Type"="Scope"})
}

#Add Application permissions that are exposed from webApi and update RedirectUris
$webAppRequired = @()
$webAppRequired += $exposedResource
$webAppRequired += $result.webApp.requiredResourceAccess
$webAppSiteName = $armConfigJson.parameters.webAppSiteName.value
$web = @{
    "RedirectUris" = @("https://$webAppSiteName.azurewebsites.net/signin-oidc","https://$webAppSiteName.azurewebsites.net/","https://$webAppSiteName.azurewebsites.net/home/SPHostedAddinEmbedContent")
}

Update-MgApplication -ApplicationId $result.webApp.id -RequiredResourceAccess $webAppRequired -Web $web

$resourceMockWebSiteName = $armConfigJson.parameters.resourceMockWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$resourceMockWebSiteName.azurewebsites.net/signin-oidc","https://$resourceMockWebSiteName.azurewebsites.net/")
}
Update-MgApplication -ApplicationId $result.appResource.id -Web $web


# grant consent
$graphServicePrincipal = Get-MgServicePrincipal -Filter "AppId eq '00000003-0000-0000-c000-000000000000'"
$null = New-MgOauth2PermissionGrant -ClientId $result.appResource.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "email openid profile User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.webApp.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "Directory.AccessAsUser.All User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.webApp.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.webApi.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "email openid profile offline_access User.Read"

# update ARMParameters.json 
$armConfigJson.parameters.webAppClientId.value = $result.webApp.appId
$armConfigJson.parameters.webAppClientSecret.value = $result.webApp.appSecret

$armConfigJson.parameters.webApiClientId.value = $result.webApi.appId
$armConfigJson.parameters.webApiClientSecret.value = $result.webApi.appSecret
$armConfigJson.parameters.webApiIdentifierUri.value = $result.webApi.identifierUris[0]


$armConfigJson.parameters.sourceMockClientId.value = $result.appResource.appId


$armConfigJson | ConvertTo-Json -depth 32| set-content $armConfig


Write-Host "WebApp ClientId: $($result.webApp.appId)" -ForegroundColor Green
Write-Host "WebApp ClientSecret: $($result.webApp.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "WebApi Id: $($result.webApi.appId)" -ForegroundColor Green
Write-Host "WebApi ClientSecret: $($result.webApi.appSecret)" -ForegroundColor Green
Write-Host "WebApi IdentifierUri: $($result.webApi.identifierUris)" -ForegroundColor Green
Write-Host ""
Write-Host ""
Write-Host "appSource Id: $($result.appResource.appId)" -ForegroundColor Green
Write-Host ""

Write-Host "Completed!" -ForegroundColor White


