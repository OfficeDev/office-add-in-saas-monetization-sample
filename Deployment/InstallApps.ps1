# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [String]$appsConfig = ".\AadApps.config.json",
    [String]$armConfig = ".\ARMParameters.json",
    [String]$teamsTabAppSettings = "..\MonetizationCodeSample\TeamsTabApp\appsettings.json",
    [String]$TeamsBotCSAppSettings = "..\MonetizationCodeSample\TeamsBotinCSharp\appsettings.json",
    [String]$TeamsBotCSAppManifest = "..\MonetizationCodeSample\TeamsBotinCSharp\AppManifest\manifest.json"
)

$appsJson = Get-Content -Path $appsConfig |Out-String|ConvertFrom-Json -AsHashtable
$armConfigJson= Get-Content -Path $armConfig |Out-String|ConvertFrom-Json -AsHashtable
$teamsTabAppSettingsJson = Get-Content -Path $teamsTabAppSettings |Out-String|ConvertFrom-Json -AsHashtable
$TeamsBotCSAppSettingsJson = Get-Content -Path $TeamsBotCSAppSettings |Out-String|ConvertFrom-Json -AsHashtable
$TeamsBotCSAppManifestJson = Get-Content -Path $TeamsBotCSAppManifest |Out-String|ConvertFrom-Json -AsHashtable

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
$preAuthorizedWebApi2=@{AppId=$result.outlookAddIn.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi3=@{AppId=$result.wordAddIn.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi4=@{AppId=$result.excelAddIn.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi5=@{AppId=$result.powerpointAddIn.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi6=@{AppId=$result.providerHostedAddIn.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi7=@{AppId=$result.teamsBotCsharp.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi8=@{AppId=$result.teamsBotJavascript.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApi9=@{AppId=$result.teamsTabApp.AppId;DelegatedPermissionIds=@($webApiPermissionId)};
$preAuthorizedWebApps = @($preAuthorizedWebApi1,$preAuthorizedWebApi2,$preAuthorizedWebApi3,$preAuthorizedWebApi4,$preAuthorizedWebApi5,$preAuthorizedWebApi6,$preAuthorizedWebApi7,$preAuthorizedWebApi8,$preAuthorizedWebApi9);

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

#Expose teams web api
$teamsTabAppConfig = $appsJson.apps |Where-Object {$_.appName -eq "teamsTabApp"}
$teamsTabAppApiPermissionId = $teamsTabAppConfig.bodyParameter.Api.Oauth2PermissionScopes.Id
$teamsTabApp_preAuthorizedWebApi1=@{AppId="1fec8e78-bce4-4aaf-ab1b-5451cc387264";DelegatedPermissionIds=@($teamsTabAppApiPermissionId)};
$teamsTabApp_preAuthorizedWebApi2=@{AppId="5e3ce6c0-2b1f-4285-8d4b-75ee78787346";DelegatedPermissionIds=@($teamsTabAppApiPermissionId)};
$teamsTabApp_preAuthorizedWebApps = @($teamsTabApp_preAuthorizedWebApi1,$teamsTabApp_preAuthorizedWebApi2)
$teamsTabApp_api = @{ 
    "PreAuthorizedApplications"=$teamsTabApp_preAuthorizedWebApps
}
Update-MgApplication -ApplicationId $result.teamsTabApp.id -Api $teamsTabApp_api -IdentifierUris "api://$($result.teamsTabApp.appId)"

#Add Application permissions that are exposed from webApi and update RedirectUris
$webAppRequired = @()
$webAppRequired += $exposedResource
$webAppRequired += $result.webApp.requiredResourceAccess
$webAppSiteName = $armConfigJson.parameters.webAppSiteName.value
$web = @{
    "RedirectUris" = @("https://$webAppSiteName.azurewebsites.net/signin-oidc","https://$webAppSiteName.azurewebsites.net/","https://$webAppSiteName.azurewebsites.net/home/SPHostedAddinEmbedContent")
}

Update-MgApplication -ApplicationId $result.webApp.id -RequiredResourceAccess $webAppRequired -Web $web

$outlookAddInRequired = @()
$outlookAddInRequired += $exposedResource
$outlookAddInRequired += $result.outlookAddIn.requiredResourceAccess
$outlookAddInWebSiteName = $armConfigJson.parameters.outlookAddInWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$outlookAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize")
}
Update-MgApplication -ApplicationId $result.outlookAddIn.id -RequiredResourceAccess $outlookAddInRequired -Web $web

$wordAddInRequired = @()
$wordAddInRequired += $exposedResource
$wordAddInRequired += $result.wordAddIn.requiredResourceAccess
$wordAddInWebSiteName = $armConfigJson.parameters.wordAddInWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$wordAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize")
}
Update-MgApplication -ApplicationId $result.wordAddIn.id -RequiredResourceAccess $wordAddInRequired -Web $web

$excelAddInRequired = @()
$excelAddInRequired += $exposedResource
$excelAddInRequired += $result.excelAddIn.requiredResourceAccess
$excelAddInWebSiteName = $armConfigJson.parameters.excelAddInWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$excelAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize")
}
Update-MgApplication -ApplicationId $result.excelAddIn.id -RequiredResourceAccess $excelAddInRequired -Web $web

$powerpointAddInRequired = @()
$powerpointAddInRequired += $exposedResource
$powerpointAddInRequired += $result.powerpointAddIn.requiredResourceAccess
$powerpointAddInWebSiteName = $armConfigJson.parameters.powerpointAddInWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$powerpointAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize")
}
Update-MgApplication -ApplicationId $result.powerpointAddIn.id -RequiredResourceAccess $powerpointAddInRequired -Web $web

$providerHostedAddInRequired = @()
$providerHostedAddInRequired += $exposedResource
$providerHostedAddInRequired += $result.providerHostedAddIn.requiredResourceAccess
$providerHostedAddInWebSiteName = $armConfigJson.parameters.providerHostedAddInWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$providerHostedAddInWebSiteName.azurewebsites.net/AzureADAuth/Authorize")
}
Update-MgApplication -ApplicationId $result.providerHostedAddIn.id -RequiredResourceAccess $providerHostedAddInRequired -Web $web

$resourceMockWebSiteName = $armConfigJson.parameters.resourceMockWebSiteName.value
$web = @{
    "RedirectUris" = @("https://$resourceMockWebSiteName.azurewebsites.net/signin-oidc","https://$resourceMockWebSiteName.azurewebsites.net/")
}
Update-MgApplication -ApplicationId $result.appResource.id -Web $web


$teamsBotCsharpRequired = @()
$teamsBotCsharpRequired += $exposedResource
$teamsBotCsharpRequired += $result.teamsBotCsharp.requiredResourceAccess
Update-MgApplication -ApplicationId $result.teamsBotCsharp.id -RequiredResourceAccess $teamsBotCsharpRequired

$teamsBotJavascriptRequired = @()
$teamsBotJavascriptRequired += $exposedResource
$teamsBotJavascriptRequired += $result.teamsBotJavascript.requiredResourceAccess
Update-MgApplication -ApplicationId $result.teamsBotJavascript.id -RequiredResourceAccess $teamsBotJavascriptRequired

$teamsTabAppRequired = @()
$teamsTabAppRequired += $exposedResource
$teamsTabAppRequired += $result.teamsTabApp.requiredResourceAccess
Update-MgApplication -ApplicationId $result.teamsTabApp.id -RequiredResourceAccess $teamsTabAppRequired

# grant consent
$graphServicePrincipal = Get-MgServicePrincipal -Filter "AppId eq '00000003-0000-0000-c000-000000000000'"
$null = New-MgOauth2PermissionGrant -ClientId $result.appResource.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "email openid profile User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.webApp.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "Directory.AccessAsUser.All User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.webApp.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.webApi.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "email openid profile offline_access User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.teamsBotCsharp.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read email openid profile offline_access"
$null = New-MgOauth2PermissionGrant -ClientId $result.teamsBotCsharp.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.teamsBotJavascript.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read email openid profile offline_access"
$null = New-MgOauth2PermissionGrant -ClientId $result.teamsBotJavascript.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.teamsTabApp.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read email openid profile offline_access"
$null = New-MgOauth2PermissionGrant -ClientId $result.teamsTabApp.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.wordAddIn.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.wordAddIn.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.outlookAddIn.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.outlookAddIn.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.excelAddIn.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.excelAddIn.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.powerpointAddIn.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.powerpointAddIn.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"
$null = New-MgOauth2PermissionGrant -ClientId $result.providerHostedAddIn.spId -ConsentType "AllPrincipals" -ResourceId $graphServicePrincipal.Id -Scope "User.Read"
$null = New-MgOauth2PermissionGrant -ClientId $result.providerHostedAddIn.spId -ConsentType "AllPrincipals" -ResourceId $result.webApi.spId -Scope "user_impersonation"

# update ARMParameters.json 
$armConfigJson.parameters.webAppClientId.value = $result.webApp.appId
$armConfigJson.parameters.webAppClientSecret.value = $result.webApp.appSecret

$armConfigJson.parameters.webApiClientId.value = $result.webApi.appId
$armConfigJson.parameters.webApiClientSecret.value = $result.webApi.appSecret
$armConfigJson.parameters.webApiIdentifierUri.value = $result.webApi.identifierUris[0]

$armConfigJson.parameters.outlookAddInClientId.value = $result.outlookAddIn.appId
$armConfigJson.parameters.outlookAddInClientSecret.value = $result.outlookAddIn.appSecret

$armConfigJson.parameters.wordAddInClientId.value = $result.wordAddIn.appId
$armConfigJson.parameters.wordAddInClientSecret.value = $result.wordAddIn.appSecret

$armConfigJson.parameters.excelAddInClientId.value = $result.excelAddIn.appId
$armConfigJson.parameters.excelAddInClientSecret.value = $result.excelAddIn.appSecret

$armConfigJson.parameters.powerpointAddInClientId.value = $result.powerpointAddIn.appId
$armConfigJson.parameters.powerpointAddInClientSecret.value = $result.powerpointAddIn.appSecret

$armConfigJson.parameters.providerHostedAddInClientId.value = $result.providerHostedAddIn.appId
$armConfigJson.parameters.providerHostedAddInClientSecret.value = $result.providerHostedAddIn.appSecret

$armConfigJson.parameters.sourceMockClientId.value = $result.appResource.appId

$armConfigJson.parameters.teamsTabAppClientId.value = $result.teamsTabApp.appId
$armConfigJson.parameters.teamsTabAppClientSecret.value = $result.teamsTabApp.appSecret

$armConfigJson.parameters.teamsBotCSharpClientId.value = $result.teamsBotCsharp.appId
$armConfigJson.parameters.teamsBotCSharpClientSecret.value = $result.teamsBotCsharp.appSecret
 
$armConfigJson.parameters.teamsBotJavaScriptClientId.value = $result.teamsBotJavascript.appId
$armConfigJson.parameters.teamsBotJavaScriptClientSecretd.value = $result.teamsBotJavascript.appSecret
$armConfigJson | ConvertTo-Json -depth 32| set-content $armConfig

$teamsTabAppSettingsJson.AzureAd.ClientId = $result.teamsTabApp.appId
$teamsTabAppSettingsJson.AzureAd.ClientSecret = $result.teamsTabApp.appSecret
$teamsTabAppSettingsJson.AzureAd.SaaSAPI = "https://$($armConfigJson.parameters.webApiSiteName.value).azurewebsites.net/api/Subscriptions/CheckOrActivateLicense"
$teamsTabAppSettingsJson.AzureAd.SaaSScopes = "$($result.webApi.identifierUris)/user_impersonation"
$teamsTabAppSettingsJson | ConvertTo-Json -depth 32| set-content $teamsTabAppSettings

$TeamsBotCSAppSettingsJson.MicrosoftAppId = $result.teamsBotCsharp.appId
$TeamsBotCSAppSettingsJson.MicrosoftAppPassword = $result.teamsBotCsharp.appSecret
$TeamsBotCSAppSettingsJson.SaaSAPI = "https://$($armConfigJson.parameters.webApiSiteName.value).azurewebsites.net/api/Subscriptions/CheckOrActivateLicense"
$TeamsBotCSAppSettingsJson | ConvertTo-Json -depth 32| set-content $TeamsBotCSAppSettings

$TeamsBotCSAppManifestJson.bots[0].botId = $result.teamsBotCsharp.appId
$TeamsBotCSAppManifestJson | ConvertTo-Json -depth 32| set-content $TeamsBotCSAppManifest

Write-Host "WebApp ClientId: $($result.webApp.appId)" -ForegroundColor Green
Write-Host "WebApp ClientSecret: $($result.webApp.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "WebApi Id: $($result.webApi.appId)" -ForegroundColor Green
Write-Host "WebApi ClientSecret: $($result.webApi.appSecret)" -ForegroundColor Green
Write-Host "WebApi IdentifierUri: $($result.webApi.identifierUris)" -ForegroundColor Green
Write-Host ""
Write-Host "outlookAddIn Id: $($result.outlookAddIn.appId)" -ForegroundColor Green
Write-Host "outlookAddIn ClientSecret: $($result.outlookAddIn.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "wordAddIn Id: $($result.wordAddIn.appId)" -ForegroundColor Green
Write-Host "wordAddIn ClientSecret: $($result.wordAddIn.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "excelAddIn Id: $($result.excelAddIn.appId)" -ForegroundColor Green
Write-Host "excelAddIn ClientSecret: $($result.excelAddIn.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "powerpointAddIn Id: $($result.powerpointAddIn.appId)" -ForegroundColor Green
Write-Host "powerpointAddIn ClientSecret: $($result.powerpointAddIn.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "providerHostedAddIn Id: $($result.providerHostedAddIn.appId)" -ForegroundColor Green
Write-Host "providerHostedAddIn ClientSecret: $($result.providerHostedAddIn.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "appSource Id: $($result.appResource.appId)" -ForegroundColor Green
Write-Host ""
Write-Host "teamsBotCsharp Id: $($result.teamsBotCsharp.appId)" -ForegroundColor Green
Write-Host "teamsBotCsharp ClientSecret: $($result.teamsBotCsharp.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "teamsBotJavascript Id: $($result.teamsBotJavascript.appId)" -ForegroundColor Green
Write-Host "teamsBotJavascript ClientSecret: $($result.teamsBotJavascript.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "teamsTabApp Id: $($result.teamsTabApp.appId)" -ForegroundColor Green
Write-Host "teamsTabApp ClientSecret: $($result.teamsTabApp.appSecret)" -ForegroundColor Green
Write-Host ""
Write-Host "Completed!" -ForegroundColor White


