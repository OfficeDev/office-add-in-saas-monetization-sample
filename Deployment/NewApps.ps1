# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [String]$config = (Read-Host -Prompt "Enter the config file")
)


$json = Get-Content -Path $config |Out-String|ConvertFrom-Json -AsHashtable
$result = @{}

foreach ($app in $json.apps)
{
    #$newApp = New-Application -BodyParameter $app.bodyParameter;
    $newApp = New-MgApplication -DisplayName $app.bodyParameter.displayName `
                             -RequiredResourceAccess $app.bodyParameter.requiredResourceAccess `
                             -SignInAudience $app.bodyParameter.signInAudience `
                             -ApiOauth2PermissionScopes $app.bodyParameter.apiOauth2PermissionScopes `
                             -ImplicitGrantSettingEnableIdTokenIssuance:$app.bodyParameter.implicitGrantSettingEnableIdTokenIssuance

    if ($app.passwordCredential){
        $pwd = Add-MgApplicationPassword -ApplicationId $newApp.Id -BodyParameter $app.passwordCredential
    }
    $result.Add($app.appName, @{"id"=$newApp.Id; "appId"=$newApp.AppId; "appSecret"=$pwd.SecretText; "requiredResourceAccess"=$app.bodyParameter.requiredResourceAccess})
}

#Expose webApi to apps
$webApiConfig = $json.apps |Where-Object {$_.appName -eq "webApi"}
$webApiPermissionId = $webApiConfig.bodyParameter.apiOauth2PermissionScopes.id
$preAuthorizedWebApi1=@{AppId=$result.webApp.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApi2=@{AppId=$result.outlookAddIn.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApi3=@{AppId=$result.wordAddIn.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApi4=@{AppId=$result.excelAddIn.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApi5=@{AppId=$result.powerpointAddIn.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApi6=@{AppId=$result.providerHostedAddIn.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApi7=@{AppId=$result.appResource.AppId;PermissionIds=$webApiPermissionId};
$preAuthorizedWebApis = @($preAuthorizedWebApi1,$preAuthorizedWebApi2,$preAuthorizedWebApi3,$preAuthorizedWebApi4,$preAuthorizedWebApi5,$preAuthorizedWebApi6,$preAuthorizedWebApi7);

#Update-Application method has bug, the -ApplicationId parameter is id instead of appid.
Update-MgApplication -ApplicationId $result.webApi.id -ApiPreAuthorizedApplications $preAuthorizedWebApis -IdentifierUris "api://$($result.webApi.appId)"
$newWebApi = Get-MgApplication -ApplicationId $result.webApi.id
$result.webApi.identifierUris = $newWebApi.IdentifierUris

$exposedResource = @{
    "resourceAppId"=$result.webApi.appId;
    "resourceAccess"= @(@{"Id"= $webApiConfig.bodyParameter.apiOauth2PermissionScopes.id ; "Type"="Scope"})
}

#Add Application permissions that are exposed from webApi
$webAppRequired = @()
$webAppRequired += $exposedResource
$webAppRequired += $result.webApp.requiredResourceAccess
Update-MgApplication -ApplicationId $result.webApp.id -RequiredResourceAccess $webAppRequired

$outlookAddInRequired = @()
$outlookAddInRequired += $exposedResource
$outlookAddInRequired += $result.outlookAddIn.requiredResourceAccess
Update-MgApplication -ApplicationId $result.outlookAddIn.id -RequiredResourceAccess $outlookAddInRequired

$wordAddInRequired = @()
$wordAddInRequired += $exposedResource
$wordAddInRequired += $result.wordAddIn.requiredResourceAccess
Update-MgApplication -ApplicationId $result.wordAddIn.id -RequiredResourceAccess $wordAddInRequired

$excelAddInRequired = @()
$excelAddInRequired += $exposedResource
$excelAddInRequired += $result.excelAddIn.requiredResourceAccess
Update-MgApplication -ApplicationId $result.excelAddIn.id -RequiredResourceAccess $excelAddInRequired

$powerpointAddInRequired = @()
$powerpointAddInRequired += $exposedResource
$powerpointAddInRequired += $result.powerpointAddIn.requiredResourceAccess
Update-MgApplication -ApplicationId $result.powerpointAddIn.id -RequiredResourceAccess $powerpointAddInRequired

$providerHostedAddInRequired = @()
$providerHostedAddInRequired += $exposedResource
$providerHostedAddInRequired += $result.providerHostedAddIn.requiredResourceAccess
Update-MgApplication -ApplicationId $result.providerHostedAddIn.id -RequiredResourceAccess $providerHostedAddInRequired

$appResourceRequired = @()
$appResourceRequired += $exposedResource
$appResourceRequired += $result.appResource.requiredResourceAccess
Update-MgApplication -ApplicationId $result.appResource.id -RequiredResourceAccess $appResourceRequired

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
Write-Host "appResource Id: $($result.appResource.appId)" -ForegroundColor Green
Write-Host "Completed!" -ForegroundColor White
#Consent manually